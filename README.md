[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://dev.azure.com/rybecket/Zen/_apis/build/status/microsoft.Zen?branchName=master)](https://dev.azure.com/rybecket/Zen/_build/latest?definitionId=2&branchName=master)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/rybecket/Zen/2)

# Introduction 
Zen is a library built at Microsoft Research that aims to simplify the process of building performant verification tools.

# Installation
Just add the project to your visual studio solution.

# Getting Started
To import the library, add the following line to your file:

```csharp
using Microsoft.Research.Zen;
```

The main abstraction Zen provides is through the type `Zen<T>` which represents a value of type `T` that can be either concrete or symbolic. As a simple example, consider the following code:

```csharp
Zen<int> PerformMath(Zen<int> x, Zen<int> y)
{
    return 3 * x + y;
}
```

This is a function that takes a Zen parameter that represents an integer value and returns a Zen value of type integer by multiplying the input by 3 and adding the input to the result. Zen overloads common C# operators such as `&,|,^,<=, <, >, >=, +, -, *, true, false` to work over Zen values. Now one can create a `ZenFunction`:

```csharp
// create a ZenFunction from MultiplyFour
var function = Function<int, int, int>(PerformMath);
```

Given a `ZenFunction` we can leverage the library to perform multiple tasks. The first is to simply evaluate the function on an input(s):

```csharp
var output = function.Evaluate(3, 2);
// output = 11
```

To perform verification, we can ask Zen to find us an input(s) that leads to something being true:

```csharp
var input = function.Find((x, y, result) => And(x >= 0, x <= 10, result == 11));
// input.Value = (0, 11)
```

Finally, Zen can compile the function to generate IL at runtime that executes with no performance penalty.

```csharp
function.Compile();
output = function.Evaluate(3, 2);
// output = 11
```

Comparing the performance between the two:

```csharp
var watch = System.Diagnostics.Stopwatch.StartNew();

for (int i = 0; i < 1000000; i++) function.Evaluate(3, 2);

Console.WriteLine($"interpreted function took: {watch.ElapsedMilliseconds}ms");
watch.Restart();

function.Compile();
Console.WriteLine($"compilation took: {watch.ElapsedMilliseconds}ms");

for (int i = 0; i < 1000000; i++) function.Evaluate(3, 2);

Console.WriteLine($"compiled function took: {watch.ElapsedMilliseconds}ms");
watch.Restart();
```

```
interpreted function took: 4601ms
compilation took: 4ms
compiled function took: 6ms
```

# Supported Types

Zen currently supports the following primitive types: `bool, byte, short, ushort, int, uint, long, ulong`.
It also supports values of type `IList` and `IDictionary`. For example:

```csharp
Zen<bool> ListOp(Zen<IList<int>> list)
{
    return list.Select(v => v + 1).Contains(4);
} 
```

Zen has some limited support for `class` and `struct` types; it will attempt to model all public fields and properties. The class/struct must also have a default constructor. An example is shown next.

# An example: Access Control Lists

As a more comprehensive example, the following shows how to use Zen to encode and then verify a simplified network access control list.

```csharp
using static Microsoft.Research.Zen.Language;

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
        if (lineNumber == this.Lines.Length) return false;
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

```csharp
var aclLine1 = new AclLine { DstIpLow = 10, DstIpHigh = 20, SrcIpLow = 7, SrcIpHigh = 39, Permitted = true };
var aclLine2 = new AclLine { DstIpLow = 0, DstIpHigh = 100, SrcIpLow = 0, SrcIpHigh = 100, Permitted = false };
var acl = Acl { Lines = new AclLine[] { aclLine1, aclLine2 } };

var f = Funcion<Packet, bool>(p => acl.Matches(p));
var packet = f.Find((p, permitted) => Not(permitted));
```

# Implementation
Zen builds an abstract syntax tree for the function it is representing and then leverages C#'s reflection capabilities to analyze the types at runtime and perform various tasks. The `Find` function uses symbolic model checking by leveraging state-of-the-art solvers such as [Z3](https://github.com/Z3Prover/z3). Compiling functions works by generating IL at runtime to avoid interpretation overhead.


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
