[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://dev.azure.com/rybecket/Zen/_apis/build/status/microsoft.Zen?branchName=master)](https://dev.azure.com/rybecket/Zen/_build/latest?definitionId=2&branchName=master)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/rybecket/Zen/2)

# Introduction 
Zen is a research library that aims to simplify the process of building verification tools. Zen lets users write a single implementation of a function and then execute, compile, and verify that function.

# Installation
Just add the project to your visual studio solution. A nuget package is available [here](https://www.nuget.org/packages/ZenLib).

# Getting Started
To import the library, add the following line to your file:

```csharp
using ZenLib;
using static ZenLib.Language;
```

The main abstraction Zen provides is through the type `Zen<T>` which represents a value of type `T` that can be either concrete or symbolic. As a simple example, consider the following code:

```csharp
Zen<int> MultiplyAndAdd(Zen<int> x, Zen<int> y)
{
    return 3 * x + y;
}
```

This is a function that takes two Zen parameters (x and y) that represents an integer values and returns a new Zen value of type integer by multiplying x by 3 and adding y to the result. Zen overloads common C# operators such as `&,|,^,<=, <, >, >=, +, -, *, true, false` to work over Zen values and supports implicit conversions between literals and Zen values. 

The next step is to create a `ZenFunction`:

```csharp
var function = Function<int, int, int>(MultiplyAndAdd);
```

Given a `ZenFunction` we can leverage the library to perform multiple tasks. The first is to simply evaluate the function on a collection of inputs:

```csharp
var output = function.Evaluate(3, 2); // output = 11
```

To perform verification, we can ask Zen to find us the inputs that leads to something being true:

```csharp
var input = function.Find((x, y, result) => And(x >= 0, x <= 10, result == 11)); // input.Value = (0, 11)
```

The type of the result in this case will be `Option<(int, int)>`, which will have a pair of integer inputs that make the expression true if such a pair exists.

Finally, Zen can compile the function to generate IL at runtime that executes without a performance penalty.

```csharp
function.Compile();
output = function.Evaluate(3, 2); // output = 11
```

Comparing the performance between the two:

```csharp
var watch = System.Diagnostics.Stopwatch.StartNew();

for (int i = 0; i < 1000000; i++) function.Evaluate(3, 2);

Console.WriteLine($"interpreted function time: {watch.ElapsedMilliseconds}ms");
watch.Restart();

function.Compile();
Console.WriteLine($"compilation time: {watch.ElapsedMilliseconds}ms");
watch.Restart();

for (int i = 0; i < 1000000; i++) function.Evaluate(3, 2);

Console.WriteLine($"compiled function time: {watch.ElapsedMilliseconds}ms");
```

```
interpreted function time: 4601ms
compilation time: 4ms
compiled function time: 2ms
```

# Supported Types

Zen currently supports the following primitive types: `bool, byte, short, ushort, int, uint, long, ulong, string`.
It also supports values of type `Tuple<T1, T2>`, `(T1, T2)`, `Option<T>`, `IList<T>` and `IDictionary<T>` so long as the inner types are also supported. Zen has some limited support for `class` and `struct` types; it will attempt to model all public fields and properties. The class/struct must also have a default constructor.

# APIs and Backends

Zen currently supports two solvers, one based on the [Z3](https://github.com/Z3Prover/z3) SMT solver and another based on [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams). The `Find` API provides an option to select one of the two backends and will default to Z3 if left unspecified:

```csharp
function.Find((x, y, result) => result == 11, Backend.DecisionDiagrams);
```

The binary decision diagram backend has an additional limitation that it does not support the `string` type. On the other hand, it provides an expressive transformer API that allows for directly computing and transforming sets of solutions:

```csharp
ZenFunction<uint, uint> f = Function<uint, uint>(i => i + 1);
StateSetTransformer<uint, uint> t = f.Transformer();
StateSet<uint> set1 = t.InputSet((x, y) => y == 10);
StateSet<uint> set2 = t.InputSet((x, y) => y == 11);
StateSet<uint> set3 = inSet1.Union(inSet2);
StateSet<uint> set4 = t.TransformForward(set1);
StateSet<uint> set5 = t.TransformBackwards(set4);
Option<uint> value = set1.Element();
```

# Examples

### Insertion sort

As a simple example, we can use Zen to verify and extract an implementation of the [insertion sort](https://en.wikipedia.org/wiki/Insertion_sort) sorting algorithm. First we must implement the insertion sort algorithm. While Zen does not support looping constructs (for, while) it does support recursion through C# in the style of functional programming languages. Below is a simple implementation of insertion sort:

```csharp
Zen<IList<T>> Sort<T>(Zen<IList<T>> expr)
{
    return expr.Case(empty: EmptyList<T>(), cons: (hd, tl) => Insert(hd, Sort(tl)));
}

Zen<IList<T>> Insert<T>(Zen<T> element, Zen<IList<T>> expr)
{
    return expr.Case(
        empty: Language.Singleton(element),
        cons: (hd, tl) =>
            If(element <= hd, expr.AddFront(element), Insert(element, tl).AddFront(hd)));
}
```

Now to prove that the result of the sort is correct, we can write a second function: 

```csharp
private static Zen<bool> IsSorted<T>(Zen<IList<T>> expr)
{
    return expr.Case(
        empty: true,
        cons: (hd1, tl1) =>
            tl1.Case(empty: true,
                     cons: (hd2, tl2) => And(hd1 <= hd2, IsSorted(tl1))));
}
```

Finally, we can ask Zen to prove that the list will always be sorted and extract an implementation.

```csharp
var function = Function(Sort<int>);
var input = function.Find((l, b) => Not(IsSorted(b)));  // input.HasValue == false

// extract an implementation
function.Compile();
Func<IList<int>, IList<int>> implementation = function;
```

Behind the scenes Zen is using [bounded model checking]() to perform verification. For lists, it only finds examples up to a given input size, which is configurable.


### Network access control lists (ACLs)

As another example, the following shows how to use Zen to encode and then verify a simplified network access control list that allows or blocks packets using an ordered collection of rules.

```csharp
public class Packet
{
    public uint DstIp { get; set; }
    public uint SrcIp { get; set; }
}

public class Acl
{
    public string Name { get; set; }
    public AclLine[] Lines { get; set; }

    public Zen<bool> Matches(Zen<Packet> packet)
    {
        return Matches(packet, 0);
    }

    private Zen<bool> Matches(Zen<Packet> packet, int lineNumber)
    {
        if (lineNumber == this.Lines.Length) 
        {
            return false;
        }

        var line = this.Lines[lineNumber];
        return If(line.Matches(packet), line.Permitted, this.Matches(packet, lineNumber + 1));
    }
}

public class AclLine
{
    public bool Permitted { get; set; }
    public uint DstIpLow { get; set; }
    public uint DstIpHigh { get; set; }
    public uint SrcIpLow { get; set; }
    public uint SrcIpHigh { get; set; }

    public Zen<bool> Matches(Zen<Packet> packet)
    {
        var dstIp = packet.GetField<Packet, uint>("DstIp");
        var srcIp = packet.GetField<Packet, uint>("SrcIp");
        return And(dstIp >= this.DstIpLow,
                   dstIp <= this.DstIpHigh,
                   srcIp >= this.SrcIpLow,
                   srcIp <= this.SrcIpHigh);
    }
}
```

# Implementation
Zen builds an abstract syntax tree (AST) for the function it is representing and then leverages C#'s reflection capabilities to interpret, compile, and symbolically evaluate the AST.

The `Find` function uses symbolic model checking to exhaustively search for inputs that lead to a desired result.

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
