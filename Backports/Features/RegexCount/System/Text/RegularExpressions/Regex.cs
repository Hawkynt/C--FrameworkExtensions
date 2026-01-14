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

// Regex.Count and Regex.EnumerateMatches were added in .NET 7.0
#if !SUPPORTS_REGEX_COUNT

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text.RegularExpressions;

public static partial class RegexPolyfills {

  extension(Regex @this) {

    /// <summary>
    /// Searches an input string for all occurrences of a regular expression and returns the number of matches.
    /// </summary>
    /// <param name="input">The string to search for a match.</param>
    /// <returns>The number of matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count(string input) {
      ArgumentNullException.ThrowIfNull(input);
      return @this.Matches(input).Count;
    }

    /// <summary>
    /// Searches an input span for all occurrences of a regular expression and returns the number of matches.
    /// </summary>
    /// <param name="input">The span to search for a match.</param>
    /// <returns>The number of matches.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count(ReadOnlySpan<char> input)
      => @this.Matches(input.ToString()).Count;

    /// <summary>
    /// Searches an input string for all occurrences of a regular expression and returns the number of matches.
    /// </summary>
    /// <param name="input">The string to search for a match.</param>
    /// <param name="startAt">The character position at which to start the search.</param>
    /// <returns>The number of matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count(string input, int startAt) {
      ArgumentNullException.ThrowIfNull(input);
      return @this.Matches(input, startAt).Count;
    }

    /// <summary>
    /// Searches an input span for all occurrences of a regular expression and returns an enumerable of the match locations.
    /// </summary>
    /// <param name="input">The span to search for a match.</param>
    /// <returns>An enumerable of the <see cref="ValueMatch"/> objects representing the matches.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<ValueMatch> EnumerateMatches(ReadOnlySpan<char> input)
      => EnumerateMatchesCore(@this, input.ToString());

    /// <summary>
    /// Searches an input string for all occurrences of a regular expression and returns an enumerable of the match locations.
    /// </summary>
    /// <param name="input">The string to search for a match.</param>
    /// <returns>An enumerable of the <see cref="ValueMatch"/> objects representing the matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public IEnumerable<ValueMatch> EnumerateMatches(string input) {
      ArgumentNullException.ThrowIfNull(input);
      return EnumerateMatchesCore(@this, input);
    }

    /// <summary>
    /// Searches an input string for all occurrences of a regular expression and returns an enumerable of the match locations.
    /// </summary>
    /// <param name="input">The string to search for a match.</param>
    /// <param name="startat">The character position at which to start the search.</param>
    /// <returns>An enumerable of the <see cref="ValueMatch"/> objects representing the matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public IEnumerable<ValueMatch> EnumerateMatches(string input, int startat) {
      ArgumentNullException.ThrowIfNull(input);
      return EnumerateMatchesCore(@this, input, startat);
    }

  }

  extension(Regex) {

    /// <summary>
    /// Searches an input string for all occurrences of a regular expression and returns the number of matches.
    /// </summary>
    /// <param name="input">The span to search for a match.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <returns>The number of matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count(ReadOnlySpan<char> input, string pattern) {
      ArgumentNullException.ThrowIfNull(pattern);
      return Regex.Matches(input.ToString(), pattern).Count;
    }

    /// <summary>
    /// Searches an input string for all occurrences of a regular expression and returns the number of matches.
    /// </summary>
    /// <param name="input">The span to search for a match.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <param name="options">A bitwise combination of the enumeration values that provide options for matching.</param>
    /// <returns>The number of matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count(ReadOnlySpan<char> input, string pattern, RegexOptions options) {
      ArgumentNullException.ThrowIfNull(pattern);
      return Regex.Matches(input.ToString(), pattern, options).Count;
    }

#if SUPPORTS_REGEX_MATCHTIMEOUT

    /// <summary>
    /// Searches an input string for all occurrences of a regular expression and returns the number of matches.
    /// </summary>
    /// <param name="input">The span to search for a match.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <param name="options">A bitwise combination of the enumeration values that provide options for matching.</param>
    /// <param name="matchTimeout">A time-out interval, or <see cref="Regex.InfiniteMatchTimeout"/> to indicate that the method should not time out.</param>
    /// <returns>The number of matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count(ReadOnlySpan<char> input, string pattern, RegexOptions options, TimeSpan matchTimeout) {
      ArgumentNullException.ThrowIfNull(pattern);
      return Regex.Matches(input.ToString(), pattern, options, matchTimeout).Count;
    }

#endif

  }

  private static IEnumerable<ValueMatch> EnumerateMatchesCore(Regex regex, string input) {
    var matches = regex.Matches(input);
    foreach (Match? match in matches)
      yield return new(match!.Index, match.Length);
  }

  private static IEnumerable<ValueMatch> EnumerateMatchesCore(Regex regex, string input, int startat) {
    var matches = regex.Matches(input, startat);
    foreach (Match? match in matches)
      yield return new(match!.Index, match.Length);
  }

}

/// <summary>
/// Represents the results from a single regular expression match, with only index and length information.
/// </summary>
public readonly struct ValueMatch {

  /// <summary>
  /// Gets the position in the original string where the first character of the captured substring is found.
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// Gets the length of the captured substring.
  /// </summary>
  public int Length { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ValueMatch"/> struct.
  /// </summary>
  /// <param name="index">The position where the match starts.</param>
  /// <param name="length">The length of the match.</param>
  internal ValueMatch(int index, int length) {
    this.Index = index;
    this.Length = length;
  }

}

#endif
