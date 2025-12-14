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

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryPolyfills {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> AsSpan<T>(this T[] @this) => new(@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> AsSpan<T>(this T[] @this, int start) => new(@this, start, @this.Length - start);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> AsSpan<T>(this T[] @this, int start, int length) => new(@this, start, length);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> AsSpan<T>(this T[] @this, Index startIndex) => AsSpan(@this, startIndex.GetOffset(@this.Length));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> AsSpan<T>(this T[] @this, Range range) {
    var offsetAndLength = range.GetOffsetAndLength(@this.Length);
    return new(@this, offsetAndLength.Offset, offsetAndLength.Length);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> AsSpan(this string @this) => new(@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> AsSpan(this string @this, int start) => new(@this, start, @this.Length - start);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> AsSpan(this string @this, int start, int length) => new(@this, start, length);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> AsSpan(this string @this, Index startIndex) => AsSpan(@this, startIndex.GetOffset(@this.Length));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> AsSpan(this string @this, Range range) {
    var offsetAndLength = range.GetOffsetAndLength(@this.Length);
    return new(@this.ToCharArray(), offsetAndLength.Offset, offsetAndLength.Length);
  }

  /// <summary>Determines whether two read-only sequences are equal by comparing the elements using IEquatable{T}.Equals(T).</summary>
  /// <param name="span">The first sequence to compare.</param>
  /// <param name="other">The second sequence to compare.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  /// <returns>
  /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool SequenceEqual<T>(this Span<T> span, ReadOnlySpan<T> other) where T : IEquatable<T>
    => SequenceEqual((ReadOnlySpan<T>)span, other);

  /// <summary>Determines whether two read-only sequences are equal by comparing the elements using IEquatable{T}.Equals(T).</summary>
  /// <param name="span">The first sequence to compare.</param>
  /// <param name="other">The second sequence to compare.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  /// <returns>
  /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
  public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other) where T : IEquatable<T> {
    var spanLength = span.Length;
    var otherLength = other.Length;
    if (spanLength != otherLength)
      return false;

    if (spanLength <= 0)
      return true;

    if (SpanHelper.IsValueType<T>() && span.memoryHandler is SpanHelper.UnmanagedPointerMemoryHandler<T> spanHandler && other.memoryHandler is SpanHelper.UnmanagedPointerMemoryHandler<T> otherHandler)
      return spanHandler.CompareAsBytesTo(otherHandler, spanLength * Unsafe.SizeOf<T>());

    if (SpanHelper.IsChar<T>())
      return span.ToString() == other.ToString();

    for (var i = 0; i < spanLength; ++i)
      if (!EqualityComparer<T>.Default.Equals(span[i], other[i]))
        return false;

    return true;
  }

  /// <summary>Determines whether two sequences are equal by comparing the elements using an <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
  /// <param name="span">The first sequence to compare.</param>
  /// <param name="other">The second sequence to compare.</param>
  /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing elements, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the type of an element.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  /// <returns>
  /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool SequenceEqual<T>(
    this Span<T> span,
    ReadOnlySpan<T> other,
    IEqualityComparer<T> comparer)
    => ((ReadOnlySpan<T>)span).SequenceEqual(other, comparer);

  /// <summary>
  /// Determines whether two sequences are equal by comparing the elements using an <see cref="IEqualityComparer{T}"/>.
  /// </summary>
  /// <param name="span">The first sequence to compare.</param>
  /// <param name="other">The second sequence to compare.</param>
  /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
  /// <returns>true if the two sequences are equal; otherwise, false.</returns>
  public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other, IEqualityComparer<T> comparer) {
    if (span.Length != other.Length)
      return false;

    if (SpanHelper.IsValueType<T>())
      if (comparer is null || Equals(comparer, EqualityComparer<T>.Default)) {
        for (var i = 0; i < span.Length; ++i)
          if (!EqualityComparer<T>.Default.Equals(span[i], other[i]))
            return false;

        return true;
      }

    comparer ??= EqualityComparer<T>.Default;
    for (var i = 0; i < span.Length; ++i)
      if (!comparer.Equals(span[i], other[i]))
        return false;

    return true;
  }

  /// <summary>Searches for the specified value and returns the index of its first occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
  /// <param name="span">The span to search.</param>
  /// <param name="value">The value to search for.</param>
  /// <typeparam name="T">The type of the span and value.</typeparam>
  /// <returns>The index of the occurrence of the value in the span. If not found, returns -1.</returns>
  public static int IndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T> {
    for (var i = 0; i < span.Length; ++i)
      if (EqualityComparer<T>.Default.Equals(span[i], value))
        return i;

    return -1;
  }

  /// <summary>Searches for the specified sequence and returns the index of its first occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
  /// <param name="span">The span to search.</param>
  /// <param name="value">The sequence to search for.</param>
  /// <typeparam name="T">The type of the span and value.</typeparam>
  /// <returns>The index of the occurrence of the value in the span. If not found, returns -1.</returns>
  public static int IndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T> {
    var needleLength = value.Length;
    if (needleLength <= 0)
      return 0;

    if (needleLength > span.Length)
      return -1;

    if (SpanHelper.IsChar<T>())
      return span.ToString().IndexOf(value.ToString(), StringComparison.Ordinal);

    for (var i = 0; i <= span.Length - needleLength; ++i)
      if (SequenceEqual(span.Slice(i, needleLength), value))
        return i;

    return -1;
  }

  /// <summary>
  /// Reports the zero-based index of the first occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
  /// </summary>
  /// <param name="span">The source span.</param>
  /// <param name="value">The value to seek within the source span.</param>
  /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType) => span.ToString().IndexOf(value.ToString(), comparisonType);

  /// <summary>Searches for the specified value and returns the index of its last occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
  /// <param name="span">The span to search.</param>
  /// <param name="value">The value to search for.</param>
  /// <typeparam name="T">The type of the span and value.</typeparam>
  /// <returns>The index of the last occurrence of the value in the span. If not found, returns -1.</returns>
  public static int LastIndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T> {
    for (var i = span.Length - 1; i >=0 ; --i)
      if (EqualityComparer<T>.Default.Equals(span[i], value))
        return i;

    return -1;
  }

  /// <summary>Searches for the specified sequence and returns the index of its last occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
  /// <param name="span">The span to search.</param>
  /// <param name="value">The sequence to search for.</param>
  /// <typeparam name="T">The type of the span and value.</typeparam>
  /// <returns>The index of the last occurrence of the value in the span. If not found, returns -1.</returns>
  public static int LastIndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T> {
    var needleLength = value.Length;
    if (needleLength <= 0)
      return 0;

    if (needleLength > span.Length)
      return -1;

    if (SpanHelper.IsChar<T>())
      return span.ToString().LastIndexOf(value.ToString(), StringComparison.Ordinal);

    for (var i = span.Length - needleLength; i >= 0 ; --i)
      if (SequenceEqual(span.Slice(i, needleLength), value))
        return i;

    return -1;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T> => value.Length <= span.Length && span[..value.Length].SequenceEqual(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T> => StartsWith((ReadOnlySpan<T>)span, value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType) {
    if (value.Length > span.Length)
      return false;

    var left = span[..value.Length];

    return comparisonType switch {
      StringComparison.CurrentCulture => string.Compare(left.ToString(), value.ToString(), CultureInfo.CurrentCulture, CompareOptions.None) == 0,
      StringComparison.CurrentCultureIgnoreCase => string.Compare(left.ToString(), value.ToString(), CultureInfo.CurrentCulture, CompareOptions.IgnoreCase) == 0,
      StringComparison.InvariantCulture => string.Compare(left.ToString(), value.ToString(), CultureInfo.InvariantCulture, CompareOptions.None) == 0,
      StringComparison.InvariantCultureIgnoreCase => string.Compare(left.ToString(), value.ToString(), CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0,
      StringComparison.Ordinal => left.SequenceEqual(value),
      StringComparison.OrdinalIgnoreCase => left.ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase),
      _ => AlwaysThrow.UnknownEnumValue<StringComparison, bool>(nameof(comparisonType), comparisonType)
    };
  }

}
#endif