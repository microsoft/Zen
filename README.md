[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://dev.azure.com/rybecket/Zen/_apis/build/status/microsoft.Zen?branchName=master)](https://dev.azure.com/rybecket/Zen/_build/latest?definitionId=2&branchName=master)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/rybecket/Zen/2)

# Introduction 
Zen is a research library that provides high-level abstractions in .NET to make it easier to leverage constraint solvers such as Z3. Zen automates translations and optimizations to low-level constraint solvers and then automates their translation back to .NET objects. It makes it easier to construct complex encodings and manipulate complex symbolic objects. The Zen library comes equipped with a number of built-in tools for processing constraints and models, including a compiler (to .NET IL), an exhaustive model checker, and a test input generator. It supports multiple backends including one based on Z3 and another based on Binary Decision Diagrams (BDDs).

# Table of contents
1. [Installation](#installation)
2. [Overview of Zen](#overview-of-zen)
    1. [Computing with Zen Expressions](#computing-with-zen-expressions)
    2. [Executing a Function](#executing-a-function)
    3. [Searching for Inputs](#searching-for-inputs)
    4. [Computing with Sets](#computing-with-sets)
    5. [Generating Test Inputs](#generating-test-inputs)
3. [Supported Data Types](#supported-data-types)
    1. [Primitive Types](#primitive-types)
    2. [Integer Types](#integer-types)
    3. [Options and Tuples](#options-tuples)
    4. [Finite Sequences, Bags, Maps](#finite-sequences-bags-maps)
    5. [Unbounded Sets, Maps](#unbounded-sets-and-maps)
    6. [Strings, Sequences, and Regexes](#strings-and-sequences)
    7. [Custom Classes and Structs](#custom-classes-and-structs)
4. [Solver Backends](#solver-backends)
5. [Example: Network ACLs](#example-network-acls)
6. [Implementation Details](#implementation)
7. [Contributing](#contributing)

<a name="installation"></a>
# Installation
Just add the project to your visual studio solution. A nuget package is available [here](https://www.nuget.org/packages/ZenLib).

<a name="overview-of-zen"></a>
# Overview of Zen
This page gives a high-level overview of the features in Zen. To see more detailed documentation, check out the [wiki](https://github.com/microsoft/Zen/wiki) page. To import the Zen library, add the following lines to your source file:

```csharp
using ZenLib;
```

The main abstraction Zen provides is through the type `Zen<T>` which represents a value of type `T` that can take on any value. The following code shows a basic use of Zen -- it creates several symbolic variables of different types (e.g., `bool`, `int`, `string`, `FSeq`) and then encodes constraints over those variables.

```csharp
// create symbolic variables of different types
var b = Zen.Symbolic<bool>();
var i = Zen.Symbolic<int>();
var s = Zen.Symbolic<string>();
var o = Zen.Symbolic<Option<ulong>>();
var l = Zen.Symbolic<FSeq<int>>(depth: 10, exhaustiveDepth: false);

// build constraints on these variables
var c1 = Zen.Or(b, i <= 10);
var c2 = Zen.Or(Zen.Not(b), o == Option.Some(1UL));
var c3 = Zen.Or(s.Contains("hello"), o.IsNone());
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

```csharp
b: True
i: 68
s: hello
o: Some(1)
l: [69,69,69,5,69,69,5,5,5,5]
```

<a name="computing-with-zen-expressions"></a>
### Computing with Zen Expressions
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

<a name="executing-a-function"></a>
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

```text
interpreted function time: 4601ms
compilation time: 4ms
compiled function time: 2ms
```

<a name="searching-for-inputs"></a>
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

Input search uses [bounded model checking](https://en.wikipedia.org/wiki/Model_checking#:~:text=Bounded%20model%20checking%20algorithms%20unroll,as%20an%20instance%20of%20SAT.) to perform verification. For data structures like lists, it finds examples up to a given input size *k*, which is an optional parameter to the function.

<a name="computing-with-sets"></a>
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


<a name="generating-test-inputs"></a>
### Generating test inputs

As a final use case, Zen can automatically generate interesting use cases for a given model by finding inputs that will lead to different execution paths. For instance, consider again the insertion sort implementation. We can ask Zen to generate test inputs for the function that can then be used, for instance to test other sorting algorithms:

```csharp
var f = new ZenFunction<FSeq<byte>, FSeq<byte>>(l => Sort(l));

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


<a name="supported-data-types"></a>
# Supported data types

Zen currently supports a subset of .NET types and also introduces some of its own data types summarized below.

| .NET Type   | Description          | Supported by Z3 backend | Supported by BDD backend | Supported by `StateSetTransformers`
| ------ | -------------------- | ----------------------- | ------------------------ | ------------|
| `bool`   | {true, false}        | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `byte`   | 8-bit value          | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `char`   | 16-bit value         | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `short`  | 16-bit signed value  | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `ushort` | 16-bit unsigned value| :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `int`    | 32-bit signed value  | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `uint`   | 32-bit unsigned value| :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `long`   | 64-bit signed value  | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `ulong`  | 64-bit unsigned value| :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `Int1`, `Int2`, ..., `IntN` | N-bit signed value| :heavy_check_mark:      | :heavy_check_mark:  | :heavy_check_mark: |
| `UInt1`, `UInt2`, ..., `UIntN` | N-bit unsigned value| :heavy_check_mark: | :heavy_check_mark:  | :heavy_check_mark: |
| `Option<T>`    | an optional/nullable value of type `T` | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:  |
| `Pair<T1, ...>`  | pairs of different values | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:  |
| `class`, `struct` | classes and structs with public fields and/or properties | :heavy_check_mark: | :heavy_check_mark:  | :heavy_check_mark:  |
| `FSeq<T>`       | finite length sequence of elements of type `T` | :heavy_check_mark: | :heavy_check_mark: | :x:  |
| `FBag<T>`       | finite size unordered multiset of elements of type `T` | :heavy_check_mark: | :heavy_check_mark: | :x:  |
| `FMap<T1, T2>` | finite size maps of keys and values of type `T1` and `T2` | :heavy_check_mark: | :heavy_check_mark: | :x:  |
| `FString` | finite length string | :heavy_check_mark: | :heavy_check_mark:  | :x:  |
| `string`     | arbitrary length string | :heavy_check_mark:           | :x:                 | :x:  |
| `BigInteger` | arbitrary length integer| :heavy_check_mark:           | :x:                 | :x:  |
| `Map<T1, T2>` | arbitrary size maps of keys and values of type `T1` and `T2`. Note that `T1` and `T2` can not use finite sequences | :heavy_check_mark: | :x: | :x:  |
| `Set<T>` | arbitrary size sets of values of type `T`. Same restrictions as with `Map<T1, T2>` | :heavy_check_mark: | :x: | :x:  |
| `Seq<T>` | arbitrary size sequences of values of type `T`. Same restrictions as with `Set<T>`. Note that SMT solvers use heuristics to solve for sequences and are incomplete. | :heavy_check_mark: | :x: | :x:  |
| `string` | arbitrary size strings. Implemented as `Seq<char>` | :heavy_check_mark: | :x: | :x:  |


<a name="primitive-types"></a>
### Primitive types

Zen supports the following primitive types: `bool, byte, char, short, ushort, int, uint, long, ulong`. All primitive types support (in)equality and integer types support integer arithmetic.

##### Example

```csharp
var x = Symbolic<int>();
var y = Symbolic<int>();
var c1 = (~x & y) == 1;
var c2 = And(x + y > 0, x + y < 100);
var solution = And(c1, c2).Solve();
```
```csharp
x: -20
y: 105
```

<a name="integer-types"></a>
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
The library should take care of the rest. Or equivalently, for unsigned integer semantics use `Unsigned`.


##### Example

```csharp
var b = Symbolic<bool>();
var x = Symbolic<BigInteger>();
var y = Symbolic<UInt9>();
var c1 = If(b, y < new UInt9(10), x == new BigInteger(3));
var c2 = Implies(Not(b), (y & new UInt9(1)) == new UInt9(1));
var solution = And(c1, c2).Solve();
```
```csharp
b: True
x: 0
y: 4
```

<a name="options-and-tuples"></a>
### Options, Tuples

Zen offers `Pair<T1, T2, ...>`, types as a lightweight alternative to classes. By default all values are assumed to be non-null by Zen. For nullable values, it provides an `Option<T>` type.

<a name="finite-sequences-bags-maps"></a>
### Finite Sequences, Bags, Maps

Zen supports a number of high-level data types that are finite (bounded) in size (the default size is 5 but can be changed). These include:

- `FSeq<T>` for reasoning about variable length sequences of values where the order is important. For instance, the sorting example earlier.
- `FBag<T>` represents finite unordered multi-sets. When the order of elements is not important, it is usually preferred to use `FBag<T>` if possible compared to `FSeq<T>` as it will frequently scale better.
- `FMap<T1, T2>` type to emulate finite maps from keys to values.

##### Example

As an example, we can write an implementation for the insertion sort algorithm using recursion:

```csharp
Zen<FSeq<T>> Sort<T>(Zen<FSeq<T>> expr)
{
    return expr.Case(empty: FSeq.Empty<T>(), cons: (hd, tl) => Insert(hd, Sort(tl)));
}

Zen<FSeq<T>> Insert<T>(Zen<T> elt, Zen<FSeq<T>> list)
{
    return list.Case(
        empty: FSeq.Create(elt),
        cons: (hd, tl) => Zen.If(elt <= hd, list.AddFront(elt), Insert(elt, tl).AddFront(hd)));
}
```

We can verify properties about this sorting algorithm by proving that there is no input that can lead to some undesirable outcome. For instance, we can use Zen to show that a sorted list has the same length as the input list:

```csharp
var f = new ZenFunction<FSeq<byte>, FSeq<byte>>(l => Sort(l));
var input = f.Find((inseq, outseq) => inseq.Length() != outseq.Length());
// input = None
```


<a name="unbounded-sets-maps"></a>
### Unbounded Sets and Maps

Zen also supports `Set<T>` and `Map<T1, T2>` data types that do not restrict the size of the set/map ahead of time. However, this type only works with the Z3 backend and requires that `T`, `T1` and `T2` not contain sequences or other maps/sets. For instance primitive types (bool, integers, string, BigInteger) as well as classes/structs are allowed.

```csharp
var s  = Symbolic<string>();
var s1 = Symbolic<Set<string>>();
var s2 = Symbolic<Set<string>>();
var s3 = Symbolic<Set<string>>();
var s4 = Symbolic<Set<string>>();

var c1 = s1.Contains("a");
var c2 = s1.Intersect(s2).Contains("b");
var c3 = Implies(s == "c", s3.Add(s) == s2);
var c4 = s4 == s1.Union(s2);
var solution = And(c1, c2, c3, c4).Solve();
```
```csharp
s:  a
s1: {b, a}
s2: {b}
s3: {}
s4: {b, a}
```

<a name="strings-and-sequences"></a>
### Sequences, Strings, and Regular Expressions

Zen has a `Seq<T>` type to represent arbitrarily large sequences of elements of type `T`. The `string` type is implemented as a `Seq<char>` for unicode strings.

As there is no complete decision procedure for sequences, queries for sequences may not always terminate, and you may need to use a timeout. If this is not acceptable, you can always use `FSeq` or `FString` instead, which will model a finite sequence up to a given depth.

Sequences also support matching against regular expressions.

##### Example

```csharp
var r = Regex.Star(Regex.Char(1));

var s1 = Symbolic<Seq<int>>();
var s2 = Symbolic<Seq<int>>();

var c1 = s1.MatchesRegex(r);
var c2 = s1 != Seq.Empty<int>();
var c3 = Not(s2.MatchesRegex(r));
var c4 = s1.Length() == s2.Length();
var solution = And(c1, c2, c3, c4).Solve();
```
```csharp
s1: [1]
s2: [0]
```

Zen supports the `string` type for reasoning about unbounded strings. As mentioned above, these are implemented as `Seq<char>`. Strings also support matching regular expressions. The regular expression parsing supports a limited subset of constructs currently - it does not support anchors like `$` and `^` or any metacharacters like `\w,\s,\d,\D` or backreferences `\1`.

##### Example

```csharp
var r1 = Regex.Parse("[0-9a-z]+");
var r2 = Regex.Parse("(0.)*");

var s = Symbolic<string>();

var c1 = s.MatchesRegex(Regex.Intersect(r1, r2));
var c2 = s.Contains("a0b0c");
var c3 = s.Length() == new BigInteger(10);
var solution = And(c1, c2, c3).Solve();
```
```csharp
s: "020z0a0b0c"
```

<a name="custom-classes-and-structs"></a>
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

<a name="solver-backends"></a>
# Solver backends

Zen currently supports two solvers, one based on the [Z3](https://github.com/Z3Prover/z3) SMT solver and another based on [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) (BDDs). The `Find` API provides an option to select one of the two backends and will default to Z3 if left unspecified. The `StateSetTransformer` uses the BDD backend. The BDD backend has the limitation that it can only reason about bounded size objects. This means that it can not reason about values with type `BigInteger` or `string` and will throw an exception. Similarly, these types along with `FSeq<T>`, `FBag<T>`, `FMap<T1, T2>`, and `Map<T1, T2>` can not be used with transformers.

<a name="example-network-acls"></a>
# Example: Network ACLs

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

<a name="implementation"></a>
# Implementation Details
Zen builds an abstract syntax tree (AST) for a given user function and then leverages C#'s reflection capabilities to interpret, compile, and symbolically evaluate the AST.

<a name="contributing"></a>
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
