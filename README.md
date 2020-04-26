[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

# Introduction 
Zen is a library built at Microsoft Research that aims to simplify the process of building performant verification tools.

# Installation
Just add the project to your visual studio solution.

# Getting Started
To import the library, add the following line to your file:

```csharp
using Zen;
```

A simple use of the library is shown shown below:

```csharp
// create a manager that uses chain-reduced binary decision diagrams
var manager = new DDManager<CBDDNode>(new CBDDNodeFactory());

// allocate three variables, two booleans and one 32-bit integer
// the internal ordering will match the order allocated from the manager.
var a = manager.CreateBool();
var b = manager.CreateBool();
var c = manager.CreateInt32();

// build formulas from the variables.
DD f1 = manager.Or(a.Id(), b.Id());
DD f2 = manager.And(c.GreaterOrEqual(1), c.LessOrEqual(4));

// get a satisfying assignment for a formula
var assignment = manager.Sat(manager.And(f1, f2));

// get the values as C# objects
bool valuea = assignment.Get(a);  // valuea = false
bool valueb = assignment.Get(b);  // valueb = true
int valuec = assignment.Get(c);   // valuec = 1
```

You can find more detailed examples in the tests.

# Implementation
The library is based on the cache-optimized implementation of decision diagrams [here](https://research.ibm.com/haifa/projects/verification/SixthSense/papers/bdd_iwls_01.pdf), and implements three variants: 
- Binary decision diagrams ([link](https://en.wikipedia.org/wiki/Binary_decision_diagram))
- Zero-suppressed binary decision diagrams ([link](https://en.wikipedia.org/wiki/Zero-suppressed_decision_diagram))
- Chain-reduced binary decision diagrams ([link](https://link.springer.com/content/pdf/10.1007%2F978-3-319-89960-2_5.pdf))

### Data representation
Internally decision diagram nodes are represented using integer ids that are bit-packed with other metadata such as a garbage collection mark bit, and a complemented bit. User references to nodes (`DD` type) are maintained through a separate (smaller) table.

### Garbage collection
The `DD` reference table uses `WeakReference` wrappers to integrate with the .NET garbage collector. This means that users of the library do not need to perform any reference counting, which is common in BDD libraries. Nodes are maintained in a memory pool and the library maintains the invariant that a node allocated before another will appear earlier in this pool. This allows for various optimizations when looking up nodes in the unique table. To maintain this invariant, the library implements a mark, sweep, and shift garbage collector that compacts nodes when necessary. 

### Memory allocation
By hashconsing nodes in the unique table, the library ensures that two boolean functions are equal if and only if their pointers (indices) are equal. The unique table maintains all nodes and is periodically resized when out of memory. For performance reasons, we ensure that this table is always a power of two size. This makes allocating new space a bit inflexible (harder to use all memory) but in return makes all operations faster. To compensate for this inflexible allocation scheme, the library becomes more reluctant to resize the table as the number of nodes grows.

### Optimizations
The library makes use of "complement edges" (a single bit packed into the node id), which determines whether the formula represented by the node is negated. This ensures that all negation operations take constant time and also reduces memory consumption since a formula and its negation share the same representation. The implementation also includes a compressed node type `CBDDNode` which can offer large memory savings in many cases.

### Operations
Internally, the manager only supports a single operation: and, but then leverages free negation to support other operations efficiently. This does make some operations such as ite and iff more costly, but can improve cache behavior since there is now only a single operation cache. Because "and" is commutative, the cache can further order the arguments to avoid redundant entries. Currently, the library does not support dynamic variable reordering as well as a number of operations such as functional composition.

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