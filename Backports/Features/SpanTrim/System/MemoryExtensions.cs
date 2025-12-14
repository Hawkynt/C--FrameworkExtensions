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

// Provides Trim, TrimStart, TrimEnd methods for ReadOnlySpan<char> and Span<char>
// Only needed when using our own Span implementation (not official System.Memory package)
// System.Memory package already provides these methods

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryPolyfills {
  /// <param name="span">The source span from which the characters are removed.</param>
  extension(ReadOnlySpan<char> span)
  {
    /// <summary>
    /// Removes all leading and trailing white-space characters from the span.
    /// </summary>
    /// <returns>The trimmed span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> Trim() => span.TrimStart().TrimEnd();

    /// <summary>
    /// Removes all leading white-space characters from the span.
    /// </summary>
    /// <returns>The trimmed span.</returns>
    public ReadOnlySpan<char> TrimStart() {
      var start = 0;
      while (start < span.Length && char.IsWhiteSpace(span[start]))
        ++start;
      return span.Slice(start, span.Length - start);
    }

    /// <summary>
    /// Removes all trailing white-space characters from the span.
    /// </summary>
    /// <returns>The trimmed span.</returns>
    public ReadOnlySpan<char> TrimEnd() {
      var end = span.Length - 1;
      while (end >= 0 && char.IsWhiteSpace(span[end]))
        --end;
      return span.Slice(0, end + 1);
    }

    /// <summary>
    /// Removes all leading and trailing occurrences of a specified character from the span.
    /// </summary>
    /// <param name="trimChar">The character to remove.</param>
    /// <returns>The trimmed span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> Trim(char trimChar) => span.TrimStart(trimChar).TrimEnd(trimChar);

    /// <summary>
    /// Removes all leading occurrences of a specified character from the span.
    /// </summary>
    /// <param name="trimChar">The character to remove.</param>
    /// <returns>The trimmed span.</returns>
    public ReadOnlySpan<char> TrimStart(char trimChar) {
      var start = 0;
      while (start < span.Length && span[start] == trimChar)
        ++start;
      return span.Slice(start, span.Length - start);
    }

    /// <summary>
    /// Removes all trailing occurrences of a specified character from the span.
    /// </summary>
    /// <param name="trimChar">The character to remove.</param>
    /// <returns>The trimmed span.</returns>
    public ReadOnlySpan<char> TrimEnd(char trimChar) {
      var end = span.Length - 1;
      while (end >= 0 && span[end] == trimChar)
        --end;
      return span.Slice(0, end + 1);
    }
  }

  /// <param name="span">The source span from which the characters are removed.</param>
  extension(Span<char> span)
  {
    /// <summary>
    /// Removes all leading and trailing white-space characters from the span.
    /// </summary>
    /// <returns>The trimmed span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> Trim() => ((ReadOnlySpan<char>)span).Trim().ToArray();

    /// <summary>
    /// Removes all leading white-space characters from the span.
    /// </summary>
    /// <returns>The trimmed span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> TrimStart() => ((ReadOnlySpan<char>)span).TrimStart().ToArray();

    /// <summary>
    /// Removes all trailing white-space characters from the span.
    /// </summary>
    /// <returns>The trimmed span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> TrimEnd() => ((ReadOnlySpan<char>)span).TrimEnd().ToArray();
  }
}

#endif
