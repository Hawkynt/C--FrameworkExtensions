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

#if !SUPPORTS_CHAR_ISASCII

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class CharPolyfills {

  extension(char) {

    /// <summary>
    /// Indicates whether a character is within the ASCII character range.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII character; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAscii(char c) => c <= '\x007f';

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII digit.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII digit; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII letter.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII letter; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLetter(char c) => c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII lowercase letter.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII lowercase letter; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLetterLower(char c) => c is >= 'a' and <= 'z';

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII uppercase letter.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII uppercase letter; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLetterUpper(char c) => c is >= 'A' and <= 'Z';

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII letter or digit.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII letter or digit; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLetterOrDigit(char c) => char.IsAsciiLetter(c) || char.IsAsciiDigit(c);

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII hexadecimal digit.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII hexadecimal digit; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiHexDigit(char c) => char.IsAsciiDigit(c) || c is (>= 'A' and <= 'F') or (>= 'a' and <= 'f');

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII lowercase hexadecimal digit.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII lowercase hexadecimal digit; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiHexDigitLower(char c) => char.IsAsciiDigit(c) || c is >= 'a' and <= 'f';

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII uppercase hexadecimal digit.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII uppercase hexadecimal digit; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiHexDigitUpper(char c) => char.IsAsciiDigit(c) || c is >= 'A' and <= 'F';

    /// <summary>
    /// Indicates whether a character is categorized as an ASCII punctuation character.
    /// </summary>
    /// <param name="c">The character to evaluate.</param>
    /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII punctuation character; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(char c, char minInclusive, char maxInclusive) => (uint)(c - minInclusive) <= (uint)(maxInclusive - minInclusive);

  }

}

#endif
