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

#if !SUPPORTS_IREADONLYSET

namespace System.Collections.Generic;

/// <summary>
/// Provides a read-only abstraction of a set.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
/// <remarks>
/// This is a polyfill for frameworks before .NET 5.0 which lack IReadOnlySet&lt;T&gt;.
/// Note: This polyfill does not use covariance (out T) because older .NET versions
/// may not fully support generic variance in all scenarios.
/// </remarks>
public interface IReadOnlySet<T> : IReadOnlyCollection<T> {
  /// <summary>
  /// Determines whether the set contains a specific value.
  /// </summary>
  /// <param name="item">The object to locate in the set.</param>
  /// <returns><see langword="true"/> if <paramref name="item"/> is found in the set; otherwise, <see langword="false"/>.</returns>
  bool Contains(T item);

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
}

#endif
