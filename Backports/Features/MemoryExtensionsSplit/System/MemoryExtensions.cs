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

// MemoryExtensions.Split was added in .NET 8.0
#if !SUPPORTS_MEMORYEXTENSIONS_SPLIT

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryExtensionsPolyfills {

  extension(ReadOnlySpan<char> @this) {

    /// <summary>
    /// Splits a span using a specified separator and returns an enumerator for the resulting ranges.
    /// </summary>
    /// <param name="separator">The character to split on.</param>
    /// <returns>A <see cref="SpanSplitEnumerator{T}"/> that can be used to iterate through the ranges.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanSplitEnumerator<char> Split(char separator)
      => new(@this, separator);

    /// <summary>
    /// Splits a span using a specified separator and returns an enumerator for the resulting ranges.
    /// </summary>
    /// <param name="separator">The string to split on.</param>
    /// <returns>A <see cref="SpanSplitEnumerator{T}"/> that can be used to iterate through the ranges.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanSplitEnumerator<char> Split(ReadOnlySpan<char> separator)
      => new(@this, separator);

    /// <summary>
    /// Splits a span using any of the specified separators and returns an enumerator for the resulting ranges.
    /// </summary>
    /// <param name="separators">The characters to split on.</param>
    /// <returns>A <see cref="SpanSplitAnyEnumerator{T}"/> that can be used to iterate through the ranges.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanSplitAnyEnumerator<char> SplitAny(ReadOnlySpan<char> separators)
      => new(@this, separators);

  }

}

/// <summary>
/// Enumerates the split ranges in a span.
/// </summary>
/// <typeparam name="T">The type of elements in the span.</typeparam>
public ref struct SpanSplitEnumerator<T> where T : IEquatable<T> {

  private readonly ReadOnlySpan<T> _span;
  private readonly T _singleSeparator;
  private readonly ReadOnlySpan<T> _separator;
  private readonly bool _useSingleSeparator;
  private readonly int _separatorLength;
  private int _currentStart;
  private int _nextSearchStart;
  private bool _isInitialized;

  internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator) {
    this._span = span;
    this._singleSeparator = separator;
    this._separator = default;
    this._useSingleSeparator = true;
    this._separatorLength = 1;
    this._currentStart = 0;
    this._nextSearchStart = 0;
    this.Current = default;
    this._isInitialized = false;
  }

  internal SpanSplitEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> separator) {
    this._span = span;
    this._singleSeparator = default!;
    this._separator = separator;
    this._useSingleSeparator = false;
    this._separatorLength = separator.Length;
    this._currentStart = 0;
    this._nextSearchStart = 0;
    this.Current = default;
    this._isInitialized = false;
  }

  /// <summary>
  /// Gets the current range.
  /// </summary>
  public Range Current { get; private set; }

  /// <summary>
  /// Returns the enumerator.
  /// </summary>
  public SpanSplitEnumerator<T> GetEnumerator() => this;

  /// <summary>
  /// Advances the enumerator to the next range.
  /// </summary>
  /// <returns><see langword="true"/> if there is another range; otherwise, <see langword="false"/>.</returns>
  public bool MoveNext() {
    if (!this._isInitialized) {
      this._isInitialized = true;
      this._currentStart = 0;
      this._nextSearchStart = 0;
    }

    if (this._nextSearchStart > this._span.Length)
      return false;

    var searchStart = this._nextSearchStart;
    var remaining = this._span.Slice(searchStart);

    int separatorIndex;
    if (this._useSingleSeparator)
      separatorIndex = remaining.IndexOf(this._singleSeparator);
    else
      separatorIndex = this._separatorLength > 0 ? remaining.IndexOf(this._separator) : -1;

    if (separatorIndex >= 0) {
      this.Current = new(searchStart, searchStart + separatorIndex);
      this._nextSearchStart = searchStart + separatorIndex + this._separatorLength;
    } else {
      this.Current = new(searchStart, this._span.Length);
      this._nextSearchStart = this._span.Length + 1;
    }

    return true;
  }

}

/// <summary>
/// Enumerates the split ranges in a span when splitting by any of multiple separators.
/// </summary>
/// <typeparam name="T">The type of elements in the span.</typeparam>
public ref struct SpanSplitAnyEnumerator<T> where T : IEquatable<T> {

  private readonly ReadOnlySpan<T> _span;
  private readonly ReadOnlySpan<T> _separators;
  private int _nextSearchStart;
  private bool _isInitialized;

  internal SpanSplitAnyEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> separators) {
    this._span = span;
    this._separators = separators;
    this._nextSearchStart = 0;
    this.Current = default;
    this._isInitialized = false;
  }

  /// <summary>
  /// Gets the current range.
  /// </summary>
  public Range Current { get; private set; }

  /// <summary>
  /// Returns the enumerator.
  /// </summary>
  public SpanSplitAnyEnumerator<T> GetEnumerator() => this;

  /// <summary>
  /// Advances the enumerator to the next range.
  /// </summary>
  /// <returns><see langword="true"/> if there is another range; otherwise, <see langword="false"/>.</returns>
  public bool MoveNext() {
    if (!this._isInitialized) {
      this._isInitialized = true;
      this._nextSearchStart = 0;
    }

    if (this._nextSearchStart > this._span.Length)
      return false;

    var searchStart = this._nextSearchStart;
    var remaining = this._span.Slice(searchStart);

    var separatorIndex = IndexOfAny(remaining, this._separators);

    if (separatorIndex >= 0) {
      this.Current = new(searchStart, searchStart + separatorIndex);
      this._nextSearchStart = searchStart + separatorIndex + 1;
    } else {
      this.Current = new(searchStart, this._span.Length);
      this._nextSearchStart = this._span.Length + 1;
    }

    return true;
  }

  private static int IndexOfAny(ReadOnlySpan<T> span, ReadOnlySpan<T> values) {
    for (var i = 0; i < span.Length; ++i)
      for (var j = 0; j < values.Length; ++j)
        if (span[i].Equals(values[j]))
          return i;
    return -1;
  }

}

#endif
