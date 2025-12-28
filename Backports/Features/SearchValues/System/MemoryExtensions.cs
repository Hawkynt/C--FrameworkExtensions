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

using System.Buffers;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryExtensionsPolyfills {

  extension<T>(Span<T> @this) where T : IEquatable<T> {

    /// <summary>
    /// Searches for the first index of any of the specified values.
    /// </summary>
    /// <param name="values">The set of values to search for.</param>
    /// <returns>The index in the span of the first occurrence of any of the specified values, or -1 if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOfAny(SearchValues<T> values) => ((ReadOnlySpan<T>)@this).IndexOfAny(values);

    /// <summary>
    /// Determines whether the span contains any of the specified values.
    /// </summary>
    /// <param name="values">The set of values to search for.</param>
    /// <returns><see langword="true"/> if the span contains any of the specified values; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsAny(SearchValues<T> values) => ((ReadOnlySpan<T>)@this).IndexOfAny(values) >= 0;

    /// <summary>
    /// Searches for the last index of any of the specified values.
    /// </summary>
    /// <param name="values">The set of values to search for.</param>
    /// <returns>The index in the span of the last occurrence of any of the specified values, or -1 if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int LastIndexOfAny(SearchValues<T> values) => ((ReadOnlySpan<T>)@this).LastIndexOfAny(values);

    /// <summary>
    /// Searches for the first index of any value other than the specified values.
    /// </summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The index in the span of the first occurrence of any value not in <paramref name="values"/>, or -1 if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOfAnyExcept(SearchValues<T> values) => ((ReadOnlySpan<T>)@this).IndexOfAnyExcept(values);

    /// <summary>
    /// Searches for the last index of any value other than the specified values.
    /// </summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The index in the span of the last occurrence of any value not in <paramref name="values"/>, or -1 if not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int LastIndexOfAnyExcept(SearchValues<T> values) => ((ReadOnlySpan<T>)@this).LastIndexOfAnyExcept(values);

    /// <summary>
    /// Determines whether the span contains any value other than the specified values.
    /// </summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns><see langword="true"/> if the span contains any value not in <paramref name="values"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsAnyExcept(SearchValues<T> values) => ((ReadOnlySpan<T>)@this).IndexOfAnyExcept(values) >= 0;

  }

  extension<T>(ReadOnlySpan<T> @this) where T : IEquatable<T> {

    /// <summary>
    /// Searches for the first index of any of the specified values.
    /// </summary>
    /// <param name="values">The set of values to search for.</param>
    /// <returns>The index in the span of the first occurrence of any of the specified values, or -1 if not found.</returns>
    public int IndexOfAny(SearchValues<T> values) {
      for (var i = 0; i < @this.Length; ++i)
        if (values.Contains(@this[i]))
          return i;

      return -1;
    }

    /// <summary>
    /// Determines whether the span contains any of the specified values.
    /// </summary>
    /// <param name="values">The set of values to search for.</param>
    /// <returns><see langword="true"/> if the span contains any of the specified values; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsAny(SearchValues<T> values) => @this.IndexOfAny(values) >= 0;

    /// <summary>
    /// Searches for the last index of any of the specified values.
    /// </summary>
    /// <param name="values">The set of values to search for.</param>
    /// <returns>The index in the span of the last occurrence of any of the specified values, or -1 if not found.</returns>
    public int LastIndexOfAny(SearchValues<T> values) {
      for (var i = @this.Length - 1; i >= 0; --i)
        if (values.Contains(@this[i]))
          return i;

      return -1;
    }

    /// <summary>
    /// Searches for the first index of any value other than the specified values.
    /// </summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The index in the span of the first occurrence of any value not in <paramref name="values"/>, or -1 if not found.</returns>
    public int IndexOfAnyExcept(SearchValues<T> values) {
      for (var i = 0; i < @this.Length; ++i)
        if (!values.Contains(@this[i]))
          return i;

      return -1;
    }

    /// <summary>
    /// Searches for the last index of any value other than the specified values.
    /// </summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns>The index in the span of the last occurrence of any value not in <paramref name="values"/>, or -1 if not found.</returns>
    public int LastIndexOfAnyExcept(SearchValues<T> values) {
      for (var i = @this.Length - 1; i >= 0; --i)
        if (!values.Contains(@this[i]))
          return i;

      return -1;
    }

    /// <summary>
    /// Determines whether the span contains any value other than the specified values.
    /// </summary>
    /// <param name="values">The set of values to exclude.</param>
    /// <returns><see langword="true"/> if the span contains any value not in <paramref name="values"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsAnyExcept(SearchValues<T> values) => @this.IndexOfAnyExcept(values) >= 0;

  }

}

#endif
