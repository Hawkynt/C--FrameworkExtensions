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

/// <summary>
/// Provides extension methods for memory-related types.
/// This class name is required by the compiler for certain wellknown members.
/// </summary>
public static partial class MemoryExtensions {

  extension(string text)
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan() => text == null ? default : new(text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(int start) => text == null ? default : new(text, start, text.Length - start);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(int start, int length) => text == null ? default : new(text, start, length);
  }

  extension<T>(T[] array)
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => array == null ? default : new(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start) => array == null ? default : new(array, start, array.Length - start);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start, int length) => array == null ? default : new(array, start, length);
  }

  /// <summary>
  /// Sorts the elements in the entire <see cref="Span{T}"/> using the <see cref="IComparable{T}"/> implementation of each element.
  /// </summary>
  /// <typeparam name="T">The type of elements in the span.</typeparam>
  /// <param name="span">The span to sort.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Sort<T>(Span<T> span) {
    if (span.Length <= 1)
      return;

    var array = span.ToArray();
    Array.Sort(array);
    array.AsSpan().CopyTo(span);
  }

  /// <summary>
  /// Sorts the elements in the entire <see cref="Span{T}"/> using the specified <see cref="Comparison{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of elements in the span.</typeparam>
  /// <param name="span">The span to sort.</param>
  /// <param name="comparison">The comparison to use when comparing elements.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Sort<T>(Span<T> span, Comparison<T> comparison) {
    if (span.Length <= 1)
      return;

    var array = span.ToArray();
    Array.Sort(array, comparison);
    array.AsSpan().CopyTo(span);
  }

  /// <summary>
  /// Sorts a pair of spans based on the keys in the first span.
  /// </summary>
  /// <typeparam name="TKey">The type of elements in the keys span.</typeparam>
  /// <typeparam name="TValue">The type of elements in the items span.</typeparam>
  /// <param name="keys">The span containing the keys to sort by.</param>
  /// <param name="items">The span containing the items to sort based on the keys.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Sort<TKey, TValue>(Span<TKey> keys, Span<TValue> items) {
    if (keys.Length <= 1)
      return;

    var keyArray = keys.ToArray();
    var itemArray = items.ToArray();
    Array.Sort(keyArray, itemArray);
    keyArray.AsSpan().CopyTo(keys);
    itemArray.AsSpan().CopyTo(items);
  }

  /// <summary>
  /// Reverses the sequence of the elements in the entire span.
  /// </summary>
  /// <typeparam name="T">The type of elements in the span.</typeparam>
  /// <param name="span">The span to reverse.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Reverse<T>(Span<T> span) {
    if (span.Length <= 1)
      return;

    var left = 0;
    var right = span.Length - 1;
    while (left < right) {
      (span[left], span[right]) = (span[right], span[left]);
      ++left;
      --right;
    }
  }

}

public static partial class MemoryPolyfills {

  extension<T>(T[] @this)
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(Index startIndex) => @this.AsSpan(startIndex.GetOffset(@this.Length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(Range range) {
      var offsetAndLength = range.GetOffsetAndLength(@this.Length);
      return new(@this, offsetAndLength.Offset, offsetAndLength.Length);
    }
  }

  extension(string @this)
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(Index startIndex) => @this.AsSpan(startIndex.GetOffset(@this.Length));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(Range range) {
      var offsetAndLength = range.GetOffsetAndLength(@this.Length);
      return new(@this.ToCharArray(), offsetAndLength.Offset, offsetAndLength.Length);
    }
  }

  /// <param name="span">The first sequence to compare.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  extension<T>(ReadOnlySpan<T> span) where T : IEquatable<T>
  {
    /// <summary>Determines whether two read-only sequences are equal by comparing the elements using IEquatable{T}.Equals(T).</summary>
    /// <param name="other">The second sequence to compare.</param>
    /// <returns>
    /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
    public bool SequenceEqual(ReadOnlySpan<T> other) {
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
  }

  /// <param name="span">The first sequence to compare.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  extension<T>(Span<T> span)
  {
    /// <summary>Determines whether two sequences are equal by comparing the elements using an <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
    /// <param name="other">The second sequence to compare.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing elements, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> for the type of an element.</param>
    /// <returns>
    /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SequenceEqual(
      ReadOnlySpan<T> other,
      IEqualityComparer<T> comparer)
      => ((ReadOnlySpan<T>)span).SequenceEqual(other, comparer);
  }

  /// <param name="span">The first sequence to compare.</param>
  extension<T>(ReadOnlySpan<T> span)
  {
    /// <summary>
    /// Determines whether two sequences are equal by comparing the elements using an <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <param name="other">The second sequence to compare.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or null to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>true if the two sequences are equal; otherwise, false.</returns>
    public bool SequenceEqual(ReadOnlySpan<T> other, IEqualityComparer<T> comparer) {
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
  }

  /// <param name="span">The span to search.</param>
  /// <typeparam name="T">The type of the span and value.</typeparam>
  extension<T>(ReadOnlySpan<T> span) where T : IEquatable<T>
  {
    /// <summary>Searches for the specified value and returns the index of its first occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
    /// <param name="value">The value to search for.</param>
    /// <returns>The index of the occurrence of the value in the span. If not found, returns -1.</returns>
    public int IndexOf(T value) {
      for (var i = 0; i < span.Length; ++i)
        if (EqualityComparer<T>.Default.Equals(span[i], value))
          return i;

      return -1;
    }

    /// <summary>Searches for the specified sequence and returns the index of its first occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
    /// <param name="value">The sequence to search for.</param>
    /// <returns>The index of the occurrence of the value in the span. If not found, returns -1.</returns>
    public int IndexOf(ReadOnlySpan<T> value) {
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
  }

  /// <param name="span">The span to search.</param>
  /// <typeparam name="T">The type of the span and value.</typeparam>
  extension<T>(ReadOnlySpan<T> span) where T : IEquatable<T>
  {
    /// <summary>Searches for the specified value and returns the index of its last occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
    /// <param name="value">The value to search for.</param>
    /// <returns>The index of the last occurrence of the value in the span. If not found, returns -1.</returns>
    public int LastIndexOf(T value) {
      for (var i = span.Length - 1; i >=0 ; --i)
        if (EqualityComparer<T>.Default.Equals(span[i], value))
          return i;

      return -1;
    }

    /// <summary>Searches for the specified sequence and returns the index of its last occurrence. Values are compared using IEquatable{T}.Equals(T).</summary>
    /// <param name="value">The sequence to search for.</param>
    /// <returns>The index of the last occurrence of the value in the span. If not found, returns -1.</returns>
    public int LastIndexOf(ReadOnlySpan<T> value) {
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
    public bool StartsWith(ReadOnlySpan<T> value) => value.Length <= span.Length && span[..value.Length].SequenceEqual(value);
  }

  /// <param name="span">The first sequence to compare.</param>
  /// <typeparam name="T">The type of elements in the sequence.</typeparam>
  extension<T>(Span<T> span) where T : IEquatable<T>
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool StartsWith(ReadOnlySpan<T> value) => StartsWith((ReadOnlySpan<T>)span, value);

    /// <summary>Determines whether two read-only sequences are equal by comparing the elements using IEquatable{T}.Equals(T).</summary>
    /// <param name="other">The second sequence to compare.</param>
    /// <returns>
    /// <see langword="true" /> if the two sequences are equal; otherwise, <see langword="false" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SequenceEqual(ReadOnlySpan<T> other) => SequenceEqual((ReadOnlySpan<T>)span, other);
  }

  /// <param name="span">The source span.</param>
  extension(ReadOnlySpan<char> span)
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool StartsWith(ReadOnlySpan<char> value, StringComparison comparisonType) {
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

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
    /// </summary>
    /// <param name="value">The value to seek within the source span.</param>
    /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(ReadOnlySpan<char> value, StringComparison comparisonType) => span.ToString().IndexOf(value.ToString(), comparisonType);
  }

}
#endif