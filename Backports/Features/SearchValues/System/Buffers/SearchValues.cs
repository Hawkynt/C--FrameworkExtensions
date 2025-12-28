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

#if !SUPPORTS_SEARCHVALUES

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Buffers;

/// <summary>
/// Provides an immutable, read-only set of values optimized for efficient searching.
/// </summary>
/// <typeparam name="T">The type of the values to search for.</typeparam>
/// <remarks>
/// This is a polyfill implementation that provides functional compatibility but not the
/// SIMD-optimized performance of the native .NET 8+ implementation.
/// Uses byte bitmap for O(1) lookup when T is byte, and HashSet for larger value sets.
/// </remarks>
public class SearchValues<T> where T : IEquatable<T> {
  private const int _HASHSET_THRESHOLD = 8;

  private readonly T[] _values;
  private readonly bool[]? _byteLookup;  // 256-entry lookup for byte type - O(1) instead of O(n)
  private readonly HashSet<T>? _hashSet; // HashSet for larger value sets

  internal SearchValues(T[] values) {
    this._values = values;

    // Specialize for byte type - O(1) lookup instead of O(n)
    if (typeof(T) == typeof(byte)) {
      this._byteLookup = new bool[256];
      for (var i = 0; i < values.Length; ++i)
        this._byteLookup[(byte)(object)values[i]!] = true;
    } else if (values.Length >= _HASHSET_THRESHOLD) {
      // Use HashSet for larger value sets of non-byte types
      this._hashSet = new(values);
    }
  }

  /// <summary>
  /// Searches for the specified value.
  /// </summary>
  /// <param name="value">The value to search for.</param>
  /// <returns><see langword="true"/> if <paramref name="value"/> was found; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Contains(T value) {
    // Fast path for byte type - O(1)
    if (this._byteLookup != null)
      return this._byteLookup[(byte)(object)value!];

    // Fast path for HashSet - O(1) average
    if (this._hashSet != null)
      return this._hashSet.Contains(value);

    // Linear search fallback for small non-byte sets
    var values = this._values;
    for (var i = 0; i < values.Length; ++i)
      if (EqualityComparer<T>.Default.Equals(values[i], value))
        return true;

    return false;
  }

  internal int Length => this._values.Length;
  internal T this[int index] => this._values[index];
}

/// <summary>
/// Provides factory methods for creating <see cref="SearchValues{T}"/> instances.
/// </summary>
public static class SearchValues {

  /// <summary>
  /// Creates a <see cref="SearchValues{T}"/> containing the specified values.
  /// </summary>
  /// <param name="values">The set of values.</param>
  /// <returns>A <see cref="SearchValues{T}"/> containing the specified values.</returns>
  public static SearchValues<byte> Create(ReadOnlySpan<byte> values)
    => new(values.ToArray());

  /// <summary>
  /// Creates a <see cref="SearchValues{T}"/> containing the specified values.
  /// </summary>
  /// <param name="values">The set of values.</param>
  /// <returns>A <see cref="SearchValues{T}"/> containing the specified values.</returns>
  public static SearchValues<char> Create(ReadOnlySpan<char> values)
    => new(values.ToArray());

}

#endif
