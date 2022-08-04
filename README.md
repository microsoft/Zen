[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
![Build Status](https://github.com/microsoft/Zen/actions/workflows/dotnet.yml/badge.svg)
![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/rabeckett/6623db8f2d0c01f6b2bc880e6219f97f/raw/code-coverage.json)

# Introduction
Zen is a research library that provides high-level abstractions in .NET to make it easier to leverage constraint solvers such as Z3. Zen automates translations and optimizations to low-level constraint solvers and then automates their translation back to .NET objects. It makes it easier to construct complex encodings and manipulate complex symbolic objects. The Zen library comes equipped with a number of built-in tools for processing constraints and models, including a compiler (to .NET IL), an exhaustive model checker, and a test input generator. It supports multiple backends including one based on Z3 and another based on Binary Decision Diagrams (BDDs).

# Table of contents
- [Introduction](#introduction)
- [Table of contents](#table-of-contents)
- [Installation](#installation)
- [Overview of Zen](#overview-of-zen)
  - [Zen Expressions](#zen-expressions)
  - [Executing a function](#executing-a-function)
  - [Searching for inputs](#searching-for-inputs)
  - [Computing with sets](#computing-with-sets)
  - [Generating test inputs](#generating-test-inputs)
  - [Optimization](#optimization)
- [Supported data types](#supported-data-types)
  - [Primitive types](#primitive-types)
  - [Integer types](#integer-types)
  - [Options, Tuples](#options-tuples)
  - [Real Values](#real-values)
  - [Finite Sequences, Bags, Maps](#finite-sequences-bags-maps)
  - [Unbounded Sets and Maps](#unbounded-sets-and-maps)
  - [Constant Sets and Maps](#constant-sets-and-maps)
  - [Sequences, Strings, and Regular Expressions](#sequences-strings-and-regular-expressions)
  - [Custom classes and structs](#custom-classes-and-structs)
  - [Enumerated values](#enumerated-values)
- [Zen Attributes](#zen-attributes)
- [Solver backends](#solver-backends)
- [Example: Network ACLs](#example-network-acls)
- [Implementation Details](#implementation-details)
- [Contributing](#contributing)

<a name="installation"></a>
# Installation
Just add the project to your visual studio solution. Alternatively, a nuget package is available [here](https://www.nuget.org/packages/ZenLib).

<a name="overview-of-zen"></a>
# Overview of Zen

To import the Zen library, add the following line to your source file:

```csharp
using ZenLib;
```

Most library methods are found in the `Zen.*` namespace. To avoid having to write this prefix out every time, you can alternatively add the following using statement:

```csharp
using static ZenLib.Zen;
```

The Zen library provides the type `Zen<T>`, which represents a symbolic value of type `T`. The library can then solve constraints involving symbolic values. The following code shows a basic use of Zen -- it creates several symbolic variables of different types (e.g., `bool`, `int`, `string`, `FSeq` - finite sequences) and then encodes constraints over those variables.

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
## Zen Expressions

`Zen<T>` objects are just normal .NET objects, we can pass them and return them from functions. For instance, consider the following code that computes a new integer from two integer inputs `x` and `y`:

```csharp
Zen<int> MultiplyAndAdd(Zen<int> x, Zen<int> y)
{
    return 3 * x + y;
}
```

Zen overloads common C# operators such as `&,|,^,<=, <, >, >=, +, -, *, true, false` to work over Zen values and supports implicit conversions to lift C# values to Zen values. Zen can represent a "function" like the one above to perform various symbolic tasks by creating a `ZenFunction` to wrap the `MultiplyAndAdd` function:

```csharp
var function = new ZenFunction<int, int, int>(MultiplyAndAdd);
```

<a name="executing-a-function"></a>
## Executing a function

Zen can execute the function we have built on inputs by calling the `Evaluate` method on the `ZenFunction`:

```csharp
var output = function.Evaluate(3, 2); // output = 11
```

This will interpret the expression tree represented by the Zen function at runtime and return back a C# `int` value in this case. Of course doing so can be quite slow, so if you need to execute a function many times, Zen can compile the model using the C# `System.Reflection.Emit` API. This generates IL instructions that execute more efficiently. Doing so is easy, just call the `Compile` method on the function first:

```csharp
function.Compile();
output = function.Evaluate(3, 2); // output = 11
```

Or alternatively:

```csharp
Func<int, int, int> f = Zen.Compile(MultiplyAndAdd);
var output = f(3, 2); // output = 11
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
## Searching for inputs

Zen can find function inputs that lead to some (un)desirable outcome. For example, we can find an `(x, y)` input pair such that `x` is less than zero and the output of the function is `11`:

```csharp
var input = function.Find((x, y, result) => Zen.And(x <= 0, result == 11)); 
// input.Value = (-1883171776, 1354548043)
```

The type of the result in this case is `Option<(int, int)>`, which will have a pair of integer inputs that make the output 11 if such a pair exists. In this case the library will find `x = -1883171776` and `y = 1354548043`

To find multiple inputs, Zen supports an equivalent `FindAll` method, which returns an `IEnumerable` of inputs where each input in `inputs` will be unique so there are no duplicates.

```csharp
using System.Linq;
...
var inputs = function.FindAll((x, y, result) => Zen.And(x <= 0, result == 11)).Take(5);
```


<a name="computing-with-sets"></a>
## Computing with sets

While the `Find` function provides a way to find a single input to a function, Zen also provides an additional API for reasoning about sets of inputs and outputs to functions. It does this through a `StateSetTransformer` API. A transformer is created by calling the `Transformer()` method on a `ZenFunction` (or by calling `Zen.Transformer(...)`):

```csharp
var f = new ZenFunction<uint, uint>(i => i + 1);
StateSetTransformer<uint, uint> t = f.Transformer();
```

Transformers allow for manipulating (potentially huge) sets of objects efficient. For example, we can get the set of all input `uint` values where adding one will result in an output `y` that is no more than 10 thousand:

```csharp
StateSet<uint> inputSet = t.InputSet((x, y) => y <= 10000);
```

This set will include all the values `0 - 9999` as well as `uint.MaxValue` due to wrapping. Transformers can also manpulate sets by propagating them forward or backwards: 

```csharp
StateSet<uint> outputSet = t.TransformForward(inputSet);
```

Finally, `StateSet` objects can also be intersected, unioned, and negated. We can pull an example element out of a set as follows (if one exists):

```csharp
Option<uint> example = inputSet.Element(); // example.Value = 0
```

Internally, transformers leverage [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) to represent, possibly very large, sets of objects efficiently.


<a name="generating-test-inputs"></a>
## Generating test inputs

Zen can automatically generate test inputs for a given model by finding inputs that will lead to different execution paths. For instance, consider an insertion sort implementation. We can ask Zen to generate test inputs for the function that can then be used, for instance to test other sorting algorithms:

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

The test generation approach uses [symbolic execution](https://en.wikipedia.org/wiki/Symbolic_execution) to enumerate program paths and solve constraints on inputs that lead down each path. Each `Zen.If` expression is treated as a program branch point (note: you can set the setting `Settings.PreserveBranches = true` to preserve branches if needed.).

<a name="optimization"></a>
## Optimization

Zen supports optimization of objective functions subject to constraints. The API is similar to that for `Solve`, but requires a maximization or minimization objective. The solver will find the maximal satisfying assignment to the variables.

```csharp
var a = Zen.Symbolic<Real>();
var b = Zen.Symbolic<Real>();
var constraints = Zen.And(a <= (Real)10, b <= (Real)10, a + (Real)4 <= b);
var solution = Zen.Maximize(objective: a + b, subjectTo: constraints); // a = 6, b = 10
```


<a name="supported-data-types"></a>
# Supported data types

Zen currently supports a subset of .NET types and also introduces some of its own data types summarized below.

| .NET Type   | Description          | Supported by Z3 backend | Supported by BDD backend | Supported by `StateSetTransformers`
| ------ | -------------------- | ----------------------- | ------------------------ | ------------|
| `bool`   | {true, false}        | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `byte`   | 8-bit value          | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
| `char`   | 16-bit UTF-16 character   | :heavy_check_mark:      | :heavy_check_mark:       | :heavy_check_mark: |
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
| `FSeq<T>`       | finite length sequence of elements of type `T` | :heavy_check_mark: | :heavy_check_mark: | :heavy_minus_sign:  |
| `FBag<T>`       | finite size unordered multiset of elements of type `T` | :heavy_check_mark: | :heavy_check_mark: | :heavy_minus_sign:  |
| `FMap<T1, T2>` | finite size maps of keys and values of type `T1` and `T2` | :heavy_check_mark: | :heavy_check_mark: | :heavy_minus_sign:  |
| `FString` | finite length string | :heavy_check_mark: | :heavy_check_mark:  | :heavy_minus_sign:  |
| `BigInteger` | arbitrary length integer| :heavy_check_mark:           | :heavy_minus_sign:                 | :heavy_minus_sign:  |
| `Real` | arbitrary precision rational number | :heavy_check_mark:           | :heavy_minus_sign:                 | :heavy_minus_sign:  |
| `Map<T1, T2>` | arbitrary size maps of keys and values of type `T1` and `T2`. Note that `T1` and `T2` can not use finite sequences | :heavy_check_mark: | :heavy_minus_sign: | :heavy_minus_sign:  |
| `Set<T>` | arbitrary size sets of values of type `T`. Same restrictions as with `Map<T1, T2>` | :heavy_check_mark: | :heavy_minus_sign: | :heavy_minus_sign:  |
| `ConstMap<T1, T2>` | maps of constant keys of type `T1` to values of type `T2`. | :heavy_check_mark: | :heavy_minus_sign: | :heavy_minus_sign:  |
| `ConstSet<T>` | sets of constants of type `T`. | :heavy_check_mark: | :heavy_minus_sign: | :heavy_minus_sign:  |
| `Seq<T>` | arbitrary size sequences of values of type `T`. Same restrictions as with `Set<T>`. Note that SMT solvers use heuristics to solve for sequences and are incomplete. | :heavy_check_mark: | :heavy_minus_sign: | :heavy_minus_sign:  |
| `string` | arbitrary size strings. Implemented as `Seq<char>` | :heavy_check_mark: | :heavy_minus_sign: | :heavy_minus_sign:  |


<a name="primitive-types"></a>
## Primitive types

Zen supports the primitive types `bool, byte, char, short, ushort, int, uint, long, ulong`. All primitive types support (in)equality and integer types support integer arithmetic operations. As an example:

```csharp
var x = Symbolic<int>();
var y = Symbolic<int>();
var c1 = (~x & y) == 1;
var c2 = And(x + y > 0, x + y < 100);
var solution = And(c1, c2).Solve(); // x = -20, y = 105
```

<a name="integer-types"></a>
## Integer types

Aside from primitive types, Zen also supports the `BigInteger` type found in `System.Numerics` for reasoning about ubounded integers as well as other types of integers with fixed, but non-standard bit width (for instance a 7-bit integer). Out of the box, Zen provides the types `Int1`, `UInt1`, `Int2`, `UInt2`, `Int3`, `UInt3` ..., `Int64`, `UInt64` as well as the types `Int128`, `UInt128`, `Int256`, `UInt256`. You can also create a custom fixed-width integer of a given length. For example, to create a 65-bit integer, add the following code:

```csharp
public class Int65 : IntN<Int65, Signed> 
{ 
    public override int Size { get { return 65; } } 
    public Int65(byte[] bytes) : base(bytes) { } 
    public Int65(long value) : base(value) { } 
}
```
The library should take care of the rest. Or equivalently, for unsigned integer semantics use `Unsigned`. As an example:

```csharp
var b = Symbolic<bool>();
var x = Symbolic<BigInteger>();
var y = Symbolic<UInt9>();
var c1 = If(b, y < new UInt9(10), x == new BigInteger(3));
var c2 = Implies(Not(b), (y & new UInt9(1)) == new UInt9(1));
var solution = And(c1, c2).Solve(); // b = True, x = 0, y = 4
```


<a name="options-and-tuples"></a>
## Options, Tuples

Zen offers `Pair<T1, T2, ...>`, types as a lightweight alternative to classes. By default all values are assumed to be non-null by Zen. For nullable values, it provides an `Option<T>` type.

```csharp
var b = Symbolic<Option<byte>>();
var p = Symbolic<Pair<int, int>>>();
var solution = And(b.IsNone(), p.Item1() == 3).Solve(); // b = None, p = (3, 0)
```

<a name="real-values"></a>
## Real Values

Zen supports arbitrary precision real numbers through the `Real` type.

```csharp
var c = new Real(3, 2); // the fraction 3/2 or equivalently 1.5 
var x = Symbolic<Real>();
var y = Symbolic<Real>();
var solution = (2 * x + 3 * y == c).Solve(); // x = 1/2, y = 1/6
```

<a name="finite-sequences-bags-maps"></a>
## Finite Sequences, Bags, Maps

Zen supports several high-level data types that are finite (bounded) in size (the default size is 5 but can be changed). These include:

- `FSeq<T>` for reasoning about variable length sequences of values where the order is important. For instance, the sorting example earlier.
- `FBag<T>` represents finite unordered multi-sets. When the order of elements is not important, it is often preferred to use `FBag<T>` if compared to `FSeq<T>` as it may scale better.
- `FMap<T1, T2>` type to emulate finite maps from keys to values.

One can implement complex functionality over `FSeq<T>` types by recursively processing the list in the style of functional programming. This is done via the `Zen.Case` expression, which says what to return for an empty and non-empty list. As an example, below is the implementation for the insertion sort algorithm in Zen:

```csharp
// sort a finite sequence of elements of type T.
public Zen<FSeq<T>> Sort<T>(Zen<FSeq<T>> expr)
{
    return expr.Case(
        empty: FSeq.Empty<T>(),
        cons: (hd, tl) => Insert(hd, Sort(tl)));
}

// insert the element in sorted order into the sorted list.
public Zen<FSeq<T>> Insert<T>(Zen<T> elt, Zen<FSeq<T>> list)
{
    return list.Case(
        empty: FSeq.Create(elt),
        cons: (hd, tl) => Zen.If(elt <= hd, list.AddFront(elt), Insert(elt, tl).AddFront(hd)));
}
```

We can verify properties about this sorting algorithm by proving that there is no input that can lead to some undesirable outcome. For instance, we can use Zen to show that a sorted list has the same length as the input list:

```csharp
var f = new ZenFunction<FSeq<byte>, FSeq<byte>>(l => Sort(l));
var input = f.Find((inseq, outseq) => inseq.Length() != outseq.Length()); // input = None
```


<a name="unbounded-sets-maps"></a>
## Unbounded Sets and Maps

Zen supports `Set<T>` and `Map<T1, T2>` data types that do not restrict the size of the set/map. This type only works with the Z3 backend and requires that `T`, `T1` and `T2` not contain any finitized types (`FSeq`, `FString`, or `FBag`). Primitive types (bool, integers, string, BigInteger), classes/structs are allowed.

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
var solution = And(c1, c2, c3, c4).Solve(); // s = "a", s1 = {b, a}, s2 = {b}, s3 = {}, s4 = {b, a}
```

<a name="constant-sets-maps"></a>
## Constant Sets and Maps

Arbitrary sets and maps described above are compiled to the SMT solver theory of Arrays. While this theory is quite general, it has known performance limitations, particularly for sets/maps that contain many elements. As a lightweight alternative, Zen provides the `ConstMap<T1, T2>` and `ConstSet<T>` classes that offer similar APIs but with the restriction that any maps keys or set elements must be C# constant values and are not themselves arbitrary Zen expressions. Zen will compile these sets and maps by expanding fresh variables for all possible constants used by the user for these types, which can lead to an efficient encoding.

Constant maps are useful for managing a finite number of unknown variables that should be indexed to some data (e.g., a symbolic boolean variable for every edge in a C# graph).

`ConstMap<T1, T2>` represents a total map from keys of type `T1` to values of type `T2`. When a key is not explicitly added to the map, the resulting value will be the Zen default value for the type `T2` (e.g., `0` for integers, `false` for booleans). `ConstSet<T>` is simply implemented as a `ConstMap<T, bool>` that says for each key, if the element is in the set. Any example use is shown below:


```csharp
var x = Symbolic<int>();
var m1 = Symbolic<ConstMap<string, int>>();
var m2 = Symbolic<ConstMap<string, int>>();

var c1 = m1.Get("a") == Zen.If(x < 10, x + 1, x + 2);
var c2 = m2 == m1.Set("b", x);
var solution = And(c1, c2).Solve(); // x = 0, m1 = m2 = {"a" => 1, _ => 0}
```

Constant maps and sets have several limitations:
* `T1` and `T2` are allowed to be any supported Zen types, including finitized types like `FSeq` or `FBag`.
* Equality may fail at runtime for nested maps (e.g., `ConstMap<int, ConstMap<int, int>>`). This can often be avoided by simply using the simpler type `ConstMap<(int, int), int>`. This may be handled in the future.
* Inequality may not always give the expected result, as the constant maps do not have a canonical representation.
* They can not be used as values in the `Map`, `Set`, or `Seq` types at the moment.
* You can not use them in recursive `FSeq.Case` definitions (e.g., see the sorting example from earlier).
* They do not work with the BDD backend currently.


<a name="strings-and-sequences"></a>
## Sequences, Strings, and Regular Expressions

Zen has a `Seq<T>` type to represent arbitrarily large sequences of elements of type `T`. As there is no complete decision procedure for sequences, queries for sequences may not always terminate, and you may need to use a timeout. If this is not acceptable, you can always use `FSeq` or `FString` instead, which will model a finite sequence up to a given depth. Sequences also support matching against regular expressions. As an example:

```csharp
Regex<int> r = Regex.Star(Regex.Char(1)); // zero or more 1s in a Seq<int>

var s1 = Symbolic<Seq<int>>();
var s2 = Symbolic<Seq<int>>();

var c1 = s1.MatchesRegex(r);
var c2 = s1 != Seq.Empty<int>();
var c3 = Not(s2.MatchesRegex(r));
var c4 = s1.Length() == s2.Length();
var solution = And(c1, c2, c3, c4).Solve(); // s1 = [1], s2 = [0]
```

Zen supports the `string` type for reasoning about unbounded strings (the `string` type is implemented as a `Seq<char>` for unicode strings). Strings also support matching regular expressions. Zen supports a limited subset of constructs currently - it supports anchors like `$` and `^` but not any other metacharacters like `\w,\s,\d,\D,\b` or backreferences `\1`. As an example:

```csharp
Regex<char> r1 = Regex.Parse("[0-9a-z]+");
Regex<char> r2 = Regex.Parse("(0.)*");

var s = Symbolic<string>();

var c1 = s.MatchesRegex(Regex.Intersect(r1, r2));
var c2 = s.Contains("a0b0c");
var c3 = s.Length() == new BigInteger(10);
var solution = And(c1, c2, c3).Solve(); // s = "020z0a0b0c"
```

<a name="custom-classes-and-structs"></a>
## Custom classes and structs

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

<a name="enums"></a>
## Enumerated values

Zen models `enum` values as their backing type, which is an `int` by default unless specified by the user. For example, Zen will model the following enum as a byte:

```csharp
public enum Origin : byte
{
    Egp,
    Igp,
    Incomplete,
}
```

By default, Zen does not constraint an enum value to only be one of the enumerated values - it can be any value allowed by the backing type (any value between 0 and 255 in this example instead of just the 3 listed). If you want to add a constraint to ensure the value is only one of those enumerated by the user, you write a function like the following to test if a value is one of those expected:

```csharp
public Zen<bool> IsValidOrigin(Zen<Origin> origin)
{
    var enumValues = Enum.GetValues<Origin>();
    return Zen.Or(enumValues.Select(x => r.GetOrigin() == x).ToArray());
}
```


<a name="zen-attributes"></a>
# Zen Attributes

Zen provides two attributes to simplify the creation and manipulation of symbolic objects. The first attribute `[ZenObject]` can be applied to classes or structs. It uses C# source generators to generate Get and With methods for all public fields and properties.

```csharp
[ZenObject]
public class Point 
{ 
    public int X { get; set; }
    public int Y { get; set; }

    public static Zen<Point> Add(Zen<Point> p1, Zen<Point> p2)
    {
        return p1.WithX(p1.GetX() + p2.GetX()).WithY(p1.GetY() + p2.GetY());
    }
}
```

Note that this requires C# 9.0 and .NET 6 or later to work. In addition, you must add the ZenLib.Generators nuget package to enable code generation. The other attribute supported is the `ZenSize` attribute, which controls the size of a generated field in an object. For example, to fix the size of a `FSeq` to 10:

```csharp
public class Person
{
    [ZenSize(depth: 10, enumerationType: EnumerationType.FixedSize)]
    public FSeq<string> Contacts { get; set; }
}
```


<a name="solver-backends"></a>
# Solver backends

Zen currently supports two solvers, one based on the [Z3](https://github.com/Z3Prover/z3) SMT solver and another based on [binary decision diagrams](https://github.com/microsoft/DecisionDiagrams) (BDDs). The `Find` and `Zen.Solve` APIs provide an option to select one of the two backends and will default to Z3 if left unspecified. The `StateSetTransformer` API uses the BDD backend. The BDD backend has the limitation that it can only reason about bounded-size objects. This means that it can not reason about values with type `BigInteger` or `string` and will throw an exception. Similarly, these types along with `FSeq<T>`, `FBag<T>`, `FMap<T1, T2>`, and `Map<T1, T2>` can not be used with transformers.

<a name="example-network-acls"></a>
# Example: Network ACLs

As a more complete example, the following shows how to use Zen to encode and then verify a simplified network access control list that allows or blocks packets. ACLs generally consist of an ordered collection of match-action rules that apply in sequence with the first applicable rule determining the fate of the packet. We can model an ACL with Zen:

```csharp
// define a class to model Packets using public properties
[ZenObject]
public class Packet
{
    // packet destination ip
    public uint DstIp { get; set; } 
    // packet source ip
    public uint SrcIp { get; set; }
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
        return And(
            packet.GetDstIp() >= this.DstIpLow,
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
