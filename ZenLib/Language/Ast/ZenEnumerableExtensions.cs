using System;
using System.Collections.Generic;
using System.Linq;

namespace ZenLib
{
  /// <summary>
  /// Extensions of System.Linq for producing Zen expressions.
  /// Lifting of Linq methods to Zen types.
  /// </summary>
  public static class ZenEnumerableExtensions
  {
    /// <summary>
    /// Compute the or of an enumerable of values according to the given predicate.
    /// </summary>
    /// <param name="enumerable">The enumerable.</param>
    /// <param name="predicate">A predicate from the enumerable's element type to Zen booleans.</param>
    /// <typeparam name="T">The type of the enumerable's elements.</typeparam>
    /// <returns>Zen value.</returns>
    public static Zen<bool> Exists<T>(this IEnumerable<T> enumerable, Func<T, Zen<bool>> predicate) =>
      enumerable.Aggregate(Zen.False(), (b, e) => Zen.Or(b, predicate(e)));

    /// <summary>
    /// Compute the and of an enumerable of values according to the given predicate.
    /// </summary>
    /// <param name="enumerable">The enumerable.</param>
    /// <param name="predicate">A predicate from the enumerable's element type to Zen booleans.</param>
    /// <typeparam name="T">The type of the enumerable's elements.</typeparam>
    /// <returns>Zen value.</returns>
    public static Zen<bool> ForAll<T>(this IEnumerable<T> enumerable, Func<T, Zen<bool>> predicate) =>
      enumerable.Aggregate(Zen.True(), (b, e) => Zen.And(b, predicate(e)));
  }
}