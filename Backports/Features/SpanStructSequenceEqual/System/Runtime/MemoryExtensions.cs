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

#if !SUPPORTS_SPAN_STRUCT_SEQUENCE_EQUAL

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public struct __NOT_EQUATABLE_MARKER;

public static partial class MemoryExtensions {

  /// <summary>Determines whether two sequences are equal by comparing the elements using an <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
  /// <param name="span">The first sequence to compare.</param>
  /// <param name="other">The second sequence to compare.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  /// <returns>
  /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool SequenceEqual<T>(
    this Span<T> span,
    ReadOnlySpan<T> other,
    __NOT_EQUATABLE_MARKER _ = default)
    => ((ReadOnlySpan<T>)span).SequenceEqual(other, _);

  /// <summary>Determines whether two sequences are equal by comparing the elements using an <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
  /// <param name="span">The first sequence to compare.</param>
  /// <param name="other">The second sequence to compare.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  /// <returns>
  /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
  public static bool SequenceEqual<T>(
    this ReadOnlySpan<T> span,
    ReadOnlySpan<T> other,
    __NOT_EQUATABLE_MARKER _ = default
  ) {
    if (span.Length != other.Length)
      return false;

    if (IsValueType()) {
      for (var i = 0; i < span.Length; ++i)
        if (!EqualityComparer<T>.Default.Equals(span[i], other[i]))
          return false;
      return true;
    }

    var comparer = EqualityComparer<T>.Default;
    for (var i = 0; i < span.Length; ++i)
      if (!comparer.Equals(span[i], other[i]))
        return false;

    return true;

    static bool IsValueType() =>
      typeof(T) == typeof(byte) 
      || typeof(T) == typeof(sbyte) 
      || typeof(T) == typeof(short) 
      || typeof(T) == typeof(ushort) 
      || typeof(T) == typeof(int) 
      || typeof(T) == typeof(uint) 
      || typeof(T) == typeof(long) 
      || typeof(T) == typeof(ulong) 
      || typeof(T) == typeof(float) 
      || typeof(T) == typeof(double) 
      || typeof(T) == typeof(char) 
      || typeof(T) == typeof(bool) 
      || typeof(T) == typeof(decimal) 
      || typeof(T).IsValueType
    ;
  }
}
#endif