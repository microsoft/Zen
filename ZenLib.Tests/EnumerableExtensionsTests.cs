using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZenLib.Tests;

using static ZenLib.Tests.TestHelper;
using static ZenLib.Zen;

/// <summary>
/// Tests for enumerable extension methods.
/// </summary>
[TestClass]
[ExcludeFromCodeCoverage]
public class EnumerableExtensionsTests
{
  /// <summary>
  /// Test that exists of an empty enumerable is false.
  /// </summary>
  [TestMethod]
  public void TestExistsEmpty()
  {
    CheckAgreement(() => False() == Enumerable.Empty<bool>().Exists(_ => True()));
  }

  /// <summary>
  /// Test that exists of a 1-element enumerable is the element itself.
  /// </summary>
  [TestMethod]
  public void TestExistsSingle()
  {
    CheckValid<bool>(b => b == Enumerable.Repeat(b, 1).Exists(i => i));
  }

  /// <summary>
  /// Test that exists of a 2-element enumerable is an or.
  /// </summary>
  [TestMethod]
  public void TestExistsTwo()
  {
    CheckValid<bool, bool>((x, y) => Or(x, y) == new[] { x, y }.Exists(i => i));
  }

  /// <summary>
  /// Test that exists of a 3-element enumerable is an or.
  /// </summary>
  [TestMethod]
  public void TestExistsThree()
  {
    CheckValid<bool, bool, bool>((x, y, z) => Or(x, y, z) == new[] { x, y, z }.Exists(i => i));
  }

  /// <summary>
  /// Test that forall of an empty enumerable is true.
  /// </summary>
  [TestMethod]
  public void TestForAllEmpty()
  {
    CheckAgreement(() => True() == Enumerable.Empty<bool>().ForAll(_ => False()));
  }

  /// <summary>
  /// Test that forall of a 1-element enumerable is the element itself.
  /// </summary>
  [TestMethod]
  public void TestForAllSingle()
  {
    CheckValid<bool>(b => b == Enumerable.Repeat(b, 1).ForAll(i => i));
  }

  /// <summary>
  /// Test that forall of a 2-element enumerable is an and.
  /// </summary>
  [TestMethod]
  public void TestForAllTwo()
  {
    CheckValid<bool, bool>((x, y) => And(x, y) == new[] { x, y }.ForAll(i => i));
  }

  /// <summary>
  /// Test that forall of a 3-element enumerable is an and.
  /// </summary>
  [TestMethod]
  public void TestForAllThree()
  {
    CheckValid<bool, bool, bool>((x, y, z) => And(x, y, z) == new[] { x, y, z }.ForAll(i => i));
  }
}