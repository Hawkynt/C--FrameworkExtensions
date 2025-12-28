#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

#if !SUPPORTS_ISET

namespace System.Collections.Generic;

/// <summary>
/// Provides the base interface for the abstraction of sets.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
/// <remarks>
/// This is a polyfill for .NET 2.0/3.5 which lack ISet&lt;T&gt;.
/// ISet&lt;T&gt; was introduced in .NET 4.0.
/// </remarks>
public interface ISet<T> : ICollection<T> {
  /// <summary>
  /// Adds an element to the current set and returns a value to indicate if the element was successfully added.
  /// </summary>
  /// <param name="item">The element to add to the set.</param>
  /// <returns><see langword="true"/> if the element is added to the set; <see langword="false"/> if the element is already in the set.</returns>
  new bool Add(T item);

  /// <summary>
  /// Removes all elements in the specified collection from the current set.
  /// </summary>
  /// <param name="other">The collection of items to remove from the set.</param>
  void ExceptWith(IEnumerable<T> other);

  /// <summary>
  /// Modifies the current set so that it contains only elements that are also in a specified collection.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  void IntersectWith(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set is a proper (strict) subset of a specified collection.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  /// <returns><see langword="true"/> if the current set is a proper subset of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
  bool IsProperSubsetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set is a proper (strict) superset of a specified collection.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  /// <returns><see langword="true"/> if the current set is a proper superset of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
  bool IsProperSupersetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether a set is a subset of a specified collection.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  /// <returns><see langword="true"/> if the current set is a subset of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
  bool IsSubsetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set is a superset of a specified collection.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  /// <returns><see langword="true"/> if the current set is a superset of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
  bool IsSupersetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set overlaps with the specified collection.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  /// <returns><see langword="true"/> if the current set and <paramref name="other"/> share at least one common element; otherwise, <see langword="false"/>.</returns>
  bool Overlaps(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set and the specified collection contain the same elements.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  /// <returns><see langword="true"/> if the current set is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
  bool SetEquals(IEnumerable<T> other);

  /// <summary>
  /// Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  void SymmetricExceptWith(IEnumerable<T> other);

  /// <summary>
  /// Modifies the current set so that it contains all elements that are present in the current set, in the specified collection, or in both.
  /// </summary>
  /// <param name="other">The collection to compare to the current set.</param>
  void UnionWith(IEnumerable<T> other);
}

#endif
