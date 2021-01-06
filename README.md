[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://dev.azure.com/rybecket/Zen/_apis/build/status/microsoft.Zen?branchName=master)](https://dev.azure.com/rybecket/Zen/_build/latest?definitionId=2&branchName=master)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/rybecket/Zen/2)

# Introduction 
Zen is a research library and verification toolbox that allows for creating models of functionality. The Zen library has a number of built-in tools for processing these models, including a compiler, model checker, and test input generator. 

# Installation
Just add the project to your visual studio solution. A nuget package is available [here](https://www.nuget.org/packages/ZenLib).

# Getting Started
This page gives a high-level overview of the features in Zen. To see more detailed documentation, check out the [wiki](https://github.com/microsoft/Zen/wiki) page.

To import the Zen library, add the following lines to your source file:

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

Zen overloads common C# operators such as `&,|,^,<=, <, >, >=, +, -, *, true, false` to work over Zen values and supports implicit conversions between C# values and Zen values. To use Zen, we must next create a `ZenFunction` to wrap the `MultiplyAndAdd` function:

```csharp
ZenFunction<int, int, int> function = Function<int, int, int>(MultiplyAndAdd);
```

Given a `ZenFunction` we can leverage the library to perform multiple tasks.

### Executing a function

Zen can execute the function we have built on a given collection of inputs. The simplest way to do so is to call the `Evaluate` method on the `ZenFunction`:

```csharp
var output = function.Evaluate(3, 2); // output = 11
```

This will interpret abstract syntax tree represented by the Zen function at runtime. Of course doing so can be quite slow, particularly compared to a native version of the function.

When performance is important, or if you need to execute the model on many inputs, Zen can compile the model using the C# `System.Reflection.Emit` API. This generates IL instructions that execute more efficiently. Doing so is easy, just call the `Compile` method on the function first:

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

A powerful feature Zen supports is the ability to find function inputs that lead to some (un)desirable outcome. For example, we can find an `(x, y)` input pair such that `x` is less than zero and the output of the function is `11`:

```csharp
var input = function.Find((x, y, result) => And(x <= 0, result == 11)); 
// input.Value = (-1883171776, 1354548043)
```

The type of the result in this case is `Option<(int, int)>`, which will have a pair of integer inputs that make the expression true if such a pair exists. In this case the library will find `x = -1883171776` and `y = 1354548043`

To find multiple inputs, Zen supports an equivalent `FindAll` method, which returns an `IEnumerable` of inputs.

```csharp
var inputs = function.FindAll((x, y, result) => And(x <= 0, result == 11)).Take(5);
```

Each input in `inputs` will be unique so there will be no duplicates.

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

We can verify properties about this sorting algorithm by proving that there is no input that can lead to some undesirable outcome. For instance, we can use Zen to show that a sorted list has the same length as the input list:

```csharp
var f = Function<IList<byte>, IList<byte>>(l => Sort(l));
var input = f.Find((inlist, outlist) => inlist.Length() != outlist.Length());
// input = None
```

Input search uses [bounded model checking](https://en.wikipedia.org/wiki/Model_checking#:~:text=Bounded%20model%20checking%20algorithms%20unroll,as%20an%20instance%20of%20SAT.) to perform verification. For data structures like lists, it finds examples up to a given input size *k*, which is an optional parameter to the function.

### Computing with sets

While the `Find` function provides a way to find a single input to a function, Zen also provides an additional API for reasoning about sets of inputs and outputs to functions. 

It does this through a `StateSetTransformer` API. A transformer is created by calling the `Transformer()` method on a `ZenFunction`:

```csharp
ZenFunction<uint, uint> f = Function<uint, uint>(i => i + 1);

// create a set transformer from the function
StateSetTransformer<uint, uint> t = f.Transformer();
```

Transformers allow for manipulating (potentially huge) sets of objects efficient. For example, we can get the set of all input `uint` values where adding one will result in an output `y` that is no more than 10 thousand:

```csharp
// find the set of all inputs where the output is no more than 10,000
StateSet<uint> inputSet = t.InputSet((x, y) => y <= 10000);
```

This set will include all the values `0 - 9999` as well as `uint.MaxValue` due to wrapping. Transformers can also manpulate sets by propagating them forward or backwards: 

```csharp
// run the set through the transformer to get the set of all outputs
StateSet<uint> outputSet = t.TransformForward(inputSet);
```

Finally, `StateSet` objects can also be intersected, unioned, and negated. We can pull an example element out of a set as follows:

```csharp
// get an example value in the set if one exists.
Option<uint> example = inputSet.Element(); // example.Value = 0
```

Internally, transformers leverage [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) to represent, possibly very large, sets of objects efficiently.

### Generating test inputs

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
[0]
[0,0]
[0,0,0]
[64,54]
[0,64,54]
[136,102,242]
[32,64,30]
[136,103,118]
[144,111,14]
```

The test generation approach uses [symbolic execution](https://en.wikipedia.org/wiki/Symbolic_execution) to enumerate program paths and solve constraints on inputs that lead down each path.

# Supported data types

Zen currently supports a subset of the C# language, described in more detail below.

### Primitive types

Zen supports the following primitive types: `bool, byte, short, ushort, int, uint, long, ulong`. It does not support `char`, though you can typically achieve the same effect by casting to `ushort`.

### String types

Zen supports the `string` type for reasoning about unbounded strings. However, string theories are generally incomplete in SMT solvers so  there may be problems that they can not solve. 

For this reason, Zen also includes a library-defined `FiniteString` type for reasoning about strings with bounded size. The is done by treating a string as a list of characters `IList<ushort>`. The implementation of this class is [here](https://github.com/microsoft/Zen/blob/master/ZenLib/DataTypes/FiniteString.cs).

### Integer types

Aside from primitive types, Zen also supports the `BigInteger` type found in `System.Numerics` for reasoning about ubounded integers.

Zen also supports other types of integers with fixed, but non-standard bit width (for instance a 7-bit integer).

Out of the box, Zen provides the types `Int1`, `UInt1`, `Int2`, `UInt2`, `Int3`, `UInt3` ..., `Int64`, `UInt64` as well as the types `Int128`, `UInt128`, `Int256`, `UInt256`.

You can also create a custom fixed-width integer of a given length. For example, to create a 65-bit integer, just add the following code:

```csharp
    public class Int65 : IntN<Int65, Signed> 
    { 
        public override int Size { get { return 65; } } 
        public Int65(byte[] bytes) : base(bytes) { } 
        public Int65(long value) : base(value) { } 
    }
```
The library should take care of the rest. Or equivalently, for unsigned integer semantics.

```csharp
    public class UInt65 : IntN<UInt65, Unsigned> 
    { 
        public override int Size { get { return 65; } } 
        public UInt65(byte[] bytes) : base(bytes) { } 
        public UInt65(long value) : base(value) { } 
    }
```

### Lists, Dictionaries, Options, Tuples

Zen supports values with type `Tuple<T1, T2>`, `(T1, T2)`, `IList<T>` and `IDictionary<T>` so long as the inner types are also supported. 

By default all values are assumed to be non-null by Zen. For nullable values, it provides an `Option<T>` type.

### Custom classes and structs

Zen supports custom `class` and `struct` types with some limitations. It will attempt to model all public fields and properties. For these types to work, the class/struct must also have a default constructor.

# Solver backends

Zen currently supports two solvers, one based on the [Z3](https://github.com/Z3Prover/z3) SMT solver and another based on [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) (BDDs). 

The `Find` API provides an option to select one of the two backends and will default to Z3 if left unspecified. The `StateSetTransformer` uses the BDD backend. 

The BDD backend has the limitation that it can only reason about bounded size objects. This means that it can not reason about values with type `BigInteger` or `string` and will throw an exception. Similarly, these types along with `IList<T>` and `IDictionary<T>` can not be used with transformers.

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
    public static Zen<uint> GetDstIp(this Zen<Packet> packet)
    {
        return packet.GetField<Packet, uint>("DstIp");
    }

    public static Zen<uint> GetSrcIp(this Zen<Packet> packet)
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
