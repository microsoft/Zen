[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://dev.azure.com/rybecket/Zen/_apis/build/status/microsoft.Zen?branchName=master)](https://dev.azure.com/rybecket/Zen/_build/latest?definitionId=2&branchName=master)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/rybecket/Zen/2)

# Introduction 
Zen is a research library that provides high-level abstractions in .NET to make it easier to leverage constraint solvers such as Z3. Zen automates translations and optimizations to low-level constraint solvers and then automates their translation back to .NET objects. It makes it easier to construct complex encodings and and manipulate complex symbolic objects. The Zen library comes equipped with a number of built-in tools for processing constraints and models, including a compiler (to .NET IL), an exhaustive model checker, and a test input generator. It supports multiple backends including one based on Z3 and another based on Binary Decision Diagrams (BDDs).

# Installation
Just add the project to your visual studio solution. A nuget package is available [here](https://www.nuget.org/packages/ZenLib).

# Getting Started
This page gives a high-level overview of the features in Zen. To see more detailed documentation, check out the [wiki](https://github.com/microsoft/Zen/wiki) page.

To import the Zen library, add the following lines to your source file:

```csharp
using ZenLib;
```

The main abstraction Zen provides is through the type `Zen<T>` which represents a value of type `T` that can take on any value. The following code shows a basic use of Zen -- it creates several symbolic variables of different types (e.g., bool, int, string, IList) and then encodes constraints over those variables.

```csharp
// create symbolic variables of different types
var b = Zen.Symbolic<bool>();
var i = Zen.Symbolic<int>();
var s = Zen.Symbolic<string>();
var o = Zen.Symbolic<Option<ulong>>();
var l = Zen.Symbolic<Seq<int>>(depth: 10, exhaustiveDepth: false);

// build constraints on these variables
var c1 = Zen.Or(b, i <= 10);
var c2 = Zen.Or(Zen.Not(b), o == Option.Some(1UL));
var c3 = Zen.Or(s.Contains("hello"), Zen.Not(o.HasValue()));
var c4 = l.Where(x => x <= i).Length() == 5;
var c5 = l.All(x => Zen.And(x >= 0, x <= 100));
var expr = Zen.And(c1, c2, c3, c4, c5);

// solve the constraints to get a solution
var solution = expr.Solve();

System.Console.WriteLine("b: " + solution.Get(b));
System.Console.WriteLine("i: " + solution.Get(i));
System.Console.WriteLine("s: " + solution.Get(s));
System.Console.WriteLine("o: " + solution.Get(o));
System.Console.WriteLine("l: " + string.Join(",", solution.Get(l)));
```

The output of this example produces the following values:

```
b: True
i: 20
s: !0!hello!1!
o: Some(1)
l: [37,5,21,6,21,21,6,6,6,21]
```

Since `Zen<T>` objects are just normal .NET objects, we can pass them and return them from functions. For instance, consider the following code that computes a new integer from two integer inputs `x` and `y`:

```csharp
Zen<int> MultiplyAndAdd(Zen<int> x, Zen<int> y)
{
    return 3 * x + y;
}
```

Zen overloads common C# operators such as `&,|,^,<=, <, >, >=, +, -, *, true, false` to work over Zen values and supports implicit conversions between C# values and Zen values. 

Rather than manually building constraints as shown previously, we can also ask Zen to represent a "function" from some inputs to an output. To do so, we create a `ZenFunction` to wrap the `MultiplyAndAdd` function:

```csharp
var function = new ZenFunction<int, int, int>(MultiplyAndAdd);
```

Given a `ZenFunction` we can leverage the library to perform several additional tasks.

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
var input = function.Find((x, y, result) => Zen.And(x <= 0, result == 11)); 
// input.Value = (-1883171776, 1354548043)
```

The type of the result in this case is `Option<(int, int)>`, which will have a pair of integer inputs that make the expression true if such a pair exists. In this case the library will find `x = -1883171776` and `y = 1354548043`

To find multiple inputs, Zen supports an equivalent `FindAll` method, which returns an `IEnumerable` of inputs.

```csharp
using System.Linq;
...
var inputs = function.FindAll((x, y, result) => Zen.And(x <= 0, result == 11)).Take(5);
```

Each input in `inputs` will be unique so there will be no duplicates.

Zen also supports richer data types such as sequences. For example, we can write an implementation for the insertion sort algorithm using recursion:

```csharp
Zen<Seq<T>> Sort<T>(Zen<Seq<T>> expr)
{
    return expr.Case(empty: Seq.Empty<T>(), cons: (hd, tl) => Insert(hd, Sort(tl)));
}

Zen<Seq<T>> Insert<T>(Zen<T> elt, Zen<Seq<T>> list)
{
    return list.Case(
        empty: Seq.Create(elt),
        cons: (hd, tl) => Zen.If(elt <= hd, list.AddFront(elt), Insert(elt, tl).AddFront(hd)));
}
```

We can verify properties about this sorting algorithm by proving that there is no input that can lead to some undesirable outcome. For instance, we can use Zen to show that a sorted list has the same length as the input list:

```csharp
var f = new ZenFunction<Seq<byte>, IList<Seq>>(l => Sort(l));
var input = f.Find((inseq, outseq) => inseq.Length() != outseq.Length());
// input = None
```

Input search uses [bounded model checking](https://en.wikipedia.org/wiki/Model_checking#:~:text=Bounded%20model%20checking%20algorithms%20unroll,as%20an%20instance%20of%20SAT.) to perform verification. For data structures like lists, it finds examples up to a given input size *k*, which is an optional parameter to the function.

### Computing with sets

While the `Find` function provides a way to find a single input to a function, Zen also provides an additional API for reasoning about sets of inputs and outputs to functions. 

It does this through a `StateSetTransformer` API. A transformer is created by calling the `Transformer()` method on a `ZenFunction`:

```csharp
var f = new ZenFunction<uint, uint>(i => i + 1);

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
var f = new ZenFunction<Seq<byte>, Seq<byte>>(l => Sort(l));

foreach (var seq in f.GenerateInputs(depth: 3))
{
    Console.WriteLine($"[{string.Join(",", seq)}]");
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

Zen currently supports a subset of .NET types, summarized below.

| Type   | Description          | Supported by Z3 backend | Supported by BDD backend | Supported by `StateSetTransformers`
| ------ | -------------------- | ----------------------- | ------------------------ | ------------|
| `bool`   | {true, false}        | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `byte`   | 8-bit value          | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `short`  | 16-bit signed value  | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `ushort` | 16-bit unsigned value| :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `int`    | 32-bit signed value  | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `uint`   | 32-bit unsigned value| :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `long`   | 64-bit signed value  | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `ulong`  | 64-bit unsigned value| :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `Int1`, `Int2`, ..., `IntN` | N-bit signed value| :heavy_check_mark:      | :heavy_check_mark:  | :heavy_check_mark: |
| `UInt1`, `UInt2`, ..., `UIntN` | N-bit unsigned value| :heavy_check_mark: | :heavy_check_mark:  | :heavy_check_mark: |
| `string`     | arbitrary length string | :heavy_check_mark:           | :x:                 | :x:  |
| `BigInteger` | arbitrary length integer| :heavy_check_mark:           | :x:                 | :x:  |
| `FiniteString` | finite length string | :heavy_check_mark: | :heavy_check_mark:  | :x:  |
| `Option<T>`    | an optional/nullable value of type `T` | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:  |
| `Pair<T1, T2>`, `Pair<T1, T2, T3>`, ...  | pairs of different values | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:  |
| `Seq<T>`       | finite length sequence of elements of type `T` | :heavy_check_mark: | :heavy_check_mark: | :x:  |
| `Bag<T>`       | finite size unordered multiset of elements of type `T` | :heavy_check_mark: | :heavy_check_mark: | :x:  |
| `Dict<T1, T2>` | finite size dictionary of keys and values of type `T1` and `T2` | :heavy_check_mark: | :heavy_check_mark: | :x:  |
| `class`, `struct` | classes and structs with public fields and/or properties | :heavy_check_mark: | :heavy_check_mark:  | :heavy_check_mark:  |


### Primitive types

Zen supports the following primitive types: `bool, byte, short, ushort, int, uint, long, ulong`. It does not support `char`, though you can typically achieve the same effect by casting to `ushort`.

### String types

Zen supports the `string` type for reasoning about unbounded strings. However, string theories are generally incomplete in SMT solvers so there may be problems that they can not solve. 

For this reason, Zen also includes a library-defined `FiniteString` type for reasoning about strings with bounded size. The is done by treating a string as a list of characters `Seq<ushort>`. You can see the implementation of this class [here](https://github.com/microsoft/Zen/blob/master/ZenLib/DataTypes/FiniteString.cs).

### Integer types

Aside from primitive types, Zen also supports the `BigInteger` type found in `System.Numerics` for reasoning about ubounded integers. Zen also supports other types of integers with fixed, but non-standard bit width (for instance a 7-bit integer).

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

### Sequences, Bags, Dictionaries, Options, Tuples

Zen supports values with type `Seq<T>` for reasoning about variable length sequences of values. It also provides several library types based on `Seq<T>` to model other useful data structures. For example, it provides a `Dict<T1, T2>` type to emulate finite dictionaries, `Bag<T>` to represent unordered multi-sets, and pair types, e.g., `Pair<T1, T2>` as a lightweight alternative to classes. By default all values are assumed to be non-null by Zen. For nullable values, it provides an `Option<T>` type.

Note: When the order of elements is not important, it is usually preferred to use `Bag<T>` if possible compared to `Seq<T>` as it will frequently scale better.

### Custom classes and structs

Zen supports custom `class` and `struct` types with some limitations. It will attempt to model all public fields and properties. For these types to work, either (1) the class/struct must also have a default constructor and all properties must be allowed to be set, or (2) there must be a constructor with matching parameter names and types for all the public fields. For example, the following are examples that are and are not allowed:

```csharp
// this will work because the fields are public
public class Point 
{ 
    public int X;
    public int Y;
}

// this will work because the properties are public and can be set.
public class Point 
{ 
    public int X { get; set; }
    public int Y { get; set; }
}

// this will NOT work because X can not be set.
public class Point 
{ 
    public int X { get; }
    public int Y { get; set; }
}

// this will work as well since there is a constructor with the same parameter names.
// note that _z will not be modeled by Zen.
public class Point 
{ 
    public int X { get; }
    public int Y { get; set; }
    private int _z;

    public Point(int x, int y) 
    {
        this.X = x;
        this.Y = y;
    }
}

```

# Solver backends

Zen currently supports two solvers, one based on the [Z3](https://github.com/Z3Prover/z3) SMT solver and another based on [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) (BDDs). The `Find` API provides an option to select one of the two backends and will default to Z3 if left unspecified. The `StateSetTransformer` uses the BDD backend. The BDD backend has the limitation that it can only reason about bounded size objects. This means that it can not reason about values with type `BigInteger` or `string` and will throw an exception. Similarly, these types along with `Seq<T>`, `Bag<T>`, and `Dict<T1, T2>` can not be used with transformers.

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
