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

// StringSplitOptions.TrimEntries was added in .NET 5.0
#if !SUPPORTS_STRINGSPLIT_TRIMENTRIES

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {

  extension(string @this) {

    /// <summary>
    /// Splits a string into substrings based on specified delimiting characters and options.
    /// </summary>
    /// <param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null"/>.</param>
    /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string[] SplitWithTrim(char[]? separator, StringSplitOptions options) {
      var baseOptions = options & ~StringSplitOptionsPolyfills.TrimEntries;
      var results = @this.Split(separator, baseOptions);
      return _ApplyTrimEntries(results, options);
    }

    /// <summary>
    /// Splits a string into a maximum number of substrings based on specified delimiting characters and options.
    /// </summary>
    /// <param name="separator">An array of characters that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null"/>.</param>
    /// <param name="count">The maximum number of substrings to return.</param>
    /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string[] SplitWithTrim(char[]? separator, int count, StringSplitOptions options) {
      var baseOptions = options & ~StringSplitOptionsPolyfills.TrimEntries;
      var results = @this.Split(separator, count, baseOptions);
      return _ApplyTrimEntries(results, options);
    }

    /// <summary>
    /// Splits a string into substrings based on specified delimiting strings and options.
    /// </summary>
    /// <param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null"/>.</param>
    /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string[] SplitWithTrim(string[]? separator, StringSplitOptions options) {
      var baseOptions = options & ~StringSplitOptionsPolyfills.TrimEntries;
      var results = @this.Split(separator, baseOptions);
      return _ApplyTrimEntries(results, options);
    }

    /// <summary>
    /// Splits a string into a maximum number of substrings based on specified delimiting strings and options.
    /// </summary>
    /// <param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or <see langword="null"/>.</param>
    /// <param name="count">The maximum number of substrings to return.</param>
    /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
    /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string[] SplitWithTrim(string[]? separator, int count, StringSplitOptions options) {
      var baseOptions = options & ~StringSplitOptionsPolyfills.TrimEntries;
      var results = @this.Split(separator, count, baseOptions);
      return _ApplyTrimEntries(results, options);
    }

  }

  private static string[] _ApplyTrimEntries(string[] results, StringSplitOptions options) {
    if ((options & StringSplitOptionsPolyfills.TrimEntries) == 0)
      return results;

    var removeEmpty = (options & StringSplitOptions.RemoveEmptyEntries) != 0;
    var trimmedCount = 0;

    for (var i = 0; i < results.Length; ++i) {
      var trimmed = results[i].Trim();
      if (removeEmpty && trimmed.Length == 0)
        continue;

      results[trimmedCount++] = trimmed;
    }

    if (trimmedCount == results.Length)
      return results;

    var finalResults = new string[trimmedCount];
    Array.Copy(results, finalResults, trimmedCount);
    return finalResults;
  }

}

#endif
