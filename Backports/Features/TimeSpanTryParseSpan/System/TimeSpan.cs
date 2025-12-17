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

#if !SUPPORTS_TIMESPAN_PARSE_SPAN

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TimeSpanPolyfills {

  extension(TimeSpan) {

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the time interval to convert.</param>
    /// <returns>A time interval that corresponds to <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan Parse(ReadOnlySpan<char> s)
      => TimeSpan.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent using the specified culture-specific format information.
    /// </summary>
    /// <param name="input">A span containing the characters representing the time interval to convert.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A time interval that corresponds to <paramref name="input"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan Parse(ReadOnlySpan<char> input, IFormatProvider formatProvider)
      => TimeSpan.Parse(input.ToString(), formatProvider);

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the time interval to convert.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out TimeSpan result)
      => TimeSpan.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent using the specified culture-specific format information. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A span containing the characters representing the time interval to convert.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> input, IFormatProvider formatProvider, out TimeSpan result)
      => TimeSpan.TryParse(input.ToString(), formatProvider, out result);

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.
    /// </summary>
    /// <param name="input">A span containing the characters representing the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A time interval that corresponds to <paramref name="input"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider formatProvider)
      => TimeSpan.ParseExact(input.ToString(), format.ToString(), formatProvider);

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format, culture-specific format information, and styles. The format of the string representation must match the specified format exactly.
    /// </summary>
    /// <param name="input">A span containing the characters representing the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input"/>.</param>
    /// <returns>A time interval that corresponds to <paramref name="input"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider formatProvider, TimeSpanStyles styles)
      => TimeSpan.ParseExact(input.ToString(), format.ToString(), formatProvider, styles);

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format and culture-specific format information. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A span containing the characters representing the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider formatProvider, out TimeSpan result)
      => TimeSpan.TryParseExact(input.ToString(), format.ToString(), formatProvider, out result);

    /// <summary>
    /// Converts the span representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format, culture-specific format information, and styles. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A span containing the characters representing the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input"/>.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result)
      => TimeSpan.TryParseExact(input.ToString(), format.ToString(), formatProvider, styles, out result);

  }

}

#endif
