[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://dev.azure.com/rybecket/Zen/_apis/build/status/microsoft.Zen?branchName=master)](https://dev.azure.com/rybecket/Zen/_build/latest?definitionId=2&branchName=master)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/rybecket/Zen/2)

# Introduction 
Zen is a research library and verification toolbox that allows for creating models of functionality. The Zen library has a number of built-in tools for processing its models, including a compiler, model checker, and test input generator. 

# Installation
Just add the project to your visual studio solution. A nuget package is available [here](https://www.nuget.org/packages/ZenLib).

# Getting Started
To import the library, add the following lines to your source file:

```csharp
using ZenLib;
using static ZenLib.Language;
```

The main abstraction Zen provides is through the type `Zen<T>` which represents a value of type `T` that the library knows how to manipulate. As a simple example, consider the following code that computes a new integer from two integer inputs `x` and `y`:

```csharp
Zen<int> MultiplyAndAdd(Zen<int> x, Zen<int> y)
{
    return 3 * x + y;
}
```

Zen overloads common C# operators such as `&,|,^,<=, <, >, >=, +, -, *, true, false` to work over Zen values and supports implicit conversions between integer/string literals and Zen values. To use Zen, we must next create a `ZenFunction` to wrap the `MultiplyAndAdd` function:

```csharp
ZenFunction<int, int, int> function = Function<int, int, int>(MultiplyAndAdd);
```

Given a `ZenFunction` we can leverage the library to perform multiple tasks.

### Executing a function

Zen can simply execute the function we have built on a given collection of inputs. To do so, one simply calls the `Evaluate` method on the `ZenFunction`:

```csharp
var output = function.Evaluate(3, 2); // output = 11
```

This will interpret abstract syntax tree represented by the Zen function at runtime. To improve performance, for example when one needs to execute the function against many inputs, Zen can also compile the function to generate IL that executes more efficiently:

```csharp
function.Compile();
output = function.Evaluate(3, 2); // output = 11
```

We can see the difference by comparing the performance between the two:

```csharp
var watch = System.Diagnostics.Stopwatch.StartNew();

for (int i = 0; i < 1000000; i++)
    function.Evaluate(3, 2);

Console.WriteLine($"interpreted function time: {watch.ElapsedMilliseconds}ms");
watch.Restart();

function.Compile();

Console.WriteLine($"compilation time: {watch.ElapsedMilliseconds}ms");
watch.Restart();

for (int i = 0; i < 1000000; i++)
    function.Evaluate(3, 2);

Console.WriteLine($"compiled function time: {watch.ElapsedMilliseconds}ms");
```

```
interpreted function time: 4601ms
compilation time: 4ms
compiled function time: 2ms
```

### Searching for inputs

A powerful feature Zen supports is the ability to find function inputs that lead to some (un)desirable outcome: For example, we can find an `(x, y)` input pair such that `x` is less than zero and the output of the function is `11`:

```csharp
var input = function.Find((x, y, result) => And(x <= 0, result == 11)); 
// input.Value = (-1883171776, 1354548043)
```

The type of the result in this case is `Option<(int, int)>`, which will have a pair of integer inputs that make the expression true if such a pair exists. In this case the library will find `x = -1883171776` and `y = 1354548043`

Zen also supports richer data types such as lists. For example, we can write an implementation for the insertion sort algorithm using recursion:

```csharp
Zen<IList<T>> Sort<T>(Zen<IList<T>> expr)
{
    return expr.Case(empty: EmptyList<T>(), cons: (hd, tl) => Insert(hd, Sort(tl)));
}

Zen<IList<T>> Insert<T>(Zen<T> elt, Zen<IList<T>> list)
{
    return list.Case(
        empty: Singleton(elt),
        cons: (hd, tl) => If(elt <= hd, list.AddFront(elt), Insert(elt, tl).AddFront(hd)));
}
```

We can also verify properties about this sorting algorithm by proving that there is no input that can lead to some undesirable outcome. For instance, we can use Zen to show that a sorted list has the same length as the input list:

```csharp
var f = Function<IList<byte>, IList<byte>>(l => Sort(l));
var input = f.Find((inlist, outlist) => inlist.Length() != outlist.Length());
// input = None
```

Input search uses [bounded model checking](https://en.wikipedia.org/wiki/Model_checking#:~:text=Bounded%20model%20checking%20algorithms%20unroll,as%20an%20instance%20of%20SAT.) to perform verification. For data structures like lists, it finds examples up to a given input size $k$, which is an optional parameter to the function.

### Computing with sets

While the `Find` function provides a way to find a single input to a function, Zen also provides an additional API for reasoning about sets of inputs and outputs to functions. It does this through a `StateSetTransformer` API whose use is shown below:

```csharp
ZenFunction<uint, uint> f = Function<uint, uint>(i => i + 1);

// create a set transformer from the function
StateSetTransformer<uint, uint> t = f.Transformer();

// find the set of all inputs where the output is no more than 10,000
StateSet<uint> inputSet = t.InputSet((x, y) => y <= 10000);

// run the set through the transformer to get the set of all outputs
StateSet<uint> outputSet = t.TransformForward(inputSet);

// get an example value in the set if one exists.
Option<uint> example = inputSet.Element(); // example.Value = 0
```

`StateSet` objects can also be intersected, unioned, and negated. Moreover, they leverage [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) to represent, possibly very large, sets of objects efficiently.

### Test input generation

As a final use case, Zen can automatically generate interesting use cases for a given model by finding inputs that will lead to different execution paths. For instance, consider again the insertion sort implementation. We can ask Zen to generate test inputs for the function that can then be used, for instance to test other sorting algorithms:

```csharp
var f = Function<IList<byte>, IList<byte>>(l => Sort(l));

foreach (var list in f.GenerateInputs(listSize: 3))
{
    Console.WriteLine($"[{string.Join(",", list)}]");
}
```

In this case, we get the following output, which includes all permutations of relative orderings that may affect a sorting algorithm.

```text
[]
[-1560264679,1754872525,-1628602412]
[1179126401,271786067,-706749952]
[-1061158268,1090519060,1019166290]
[67391745,-5104748,1132410975]
[1091043349,-1464322172,1063741011]
[268451904,1115293484,1460084738]
[537004033,400151020]
[-1064824700,393686098]
[0]
```

Or as another example testing if a list contains a given value:

```csharp
var f = Function<IList<byte>, bool>(l => l.Contains(5));

foreach (var list in f.GenerateInputs(listSize: 3))
{
    Console.WriteLine($"[{string.Join(",", list)}]");
}
```

We get the following inputs, which exercise all possibilities:

```text
[]
[0,0,0]
[0,0,5]
[0,5,0]
[5,0,0]
[0,0]
[0,5]
[5,0]
[0]
[5]
```

The test generation approach uses [symbolic execution](https://en.wikipedia.org/wiki/Symbolic_execution) to enumerate program paths and solve constraints on inputs that lead down each path.

# Other information

### Supported data types

Zen currently supports the following primitive types: `bool, byte, short, ushort, int, uint, long, ulong, string`. There is a library-defined type `FiniteString` for reasoning about strings with bounded size. It also supports values with type `Tuple<T1, T2>`, `(T1, T2)`, `Option<T>`, `IList<T>` and `IDictionary<T>` so long as the inner types are also supported. Zen has some limited support for `class` and `struct` types; it will attempt to model all public fields and properties. The class/struct must also have a default constructor.

### Supported solver backends

Zen currently supports two solvers, one based on the [Z3](https://github.com/Z3Prover/z3) SMT solver and another based on [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) (BDDs). The `Find` API provides an option to select one of the two backends and will default to Z3 if left unspecified. The Z3 solver is generally more scalable but the BDD solver may perform better in some cases.

# Example: Network access control lists

As a more complete example, the following shows how to use Zen to encode and then verify a simplified network access control list that allows or blocks packets. ACLs generally consist of an ordered collection of match-action rules that apply in sequence with the first applicable rule determining the fate of the packet. We can model an ACL with Zen:

```csharp
// define a class to model Packets using public properties
public class Packet
{
    // packet destination ip
    public uint DstIp { get; set; } 
    // packet source ip
    public uint SrcIp { get; set; }
}

// define helper extension methods for manipulating packets.
public static class PacketExtensions
{
    public static Zen<uint> GetDstIp(Zen<Packet> packet)
    {
        return packet.GetField<Packet, uint>("DstIp");
    }

    public static Zen<uint> GetSrcIp(Zen<Packet> packet)
    {
        return packet.GetField<Packet, uint>("SrcIp");
    }
}

// class representing an ACL with a list of prioritized rules.
public class Acl
{
    public string Name { get; set; }
    public AclLine[] Lines { get; set; }

    public Zen<bool> Allowed(Zen<Packet> packet)
    {
        return Allowed(packet, 0);
    }

    // compute whether a packet is allowed by the ACL recursively
    private Zen<bool> Allowed(Zen<Packet> packet, int lineNumber)
    {
        if (lineNumber >= this.Lines.Length) 
        {
            return false; // Zen implicitly converts false to Zen<bool>
        }

        var line = this.Lines[lineNumber];

        // if the current line matches, then return the action, otherwise continue to the next line
        return If(line.Matches(packet), line.Action, this.Allowed(packet, lineNumber + 1));
    }
}

// An ACL line that matches a packet.
public class AclLine
{
    public bool Action { get; set; }
    public uint DstIpLow { get; set; }
    public uint DstIpHigh { get; set; }
    public uint SrcIpLow { get; set; }
    public uint SrcIpHigh { get; set; }

    // a packet matches a line if it falls within the specified ranges.
    public Zen<bool> Matches(Zen<Packet> packet)
    {
        return And(packet.GetDstIp() >= this.DstIpLow,
                   packet.GetDstIp() <= this.DstIpHigh,
                   packet.GetSrcIp() >= this.SrcIpLow,
                   packet.GetSrcIp() <= this.SrcIpHigh);
    }
}
```

# Implementation
Zen builds an abstract syntax tree (AST) for a given user function and then leverages C#'s reflection capabilities to interpret, compile, and symbolically evaluate the AST.

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
