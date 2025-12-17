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

#if !SUPPORTS_TIMESPAN_PARSEEXACT

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TimeSpanPolyfills {

  extension(TimeSpan) {

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A time interval that corresponds to <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> has an invalid format.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider)
      => TryParseExact(input, format, formatProvider, TimeSpanStyles.None, out var result)
        ? result
        : throw new FormatException("Input string was not in a correct format.");

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format, culture-specific format information, and styles.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input"/>.</param>
    /// <returns>A time interval that corresponds to <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> has an invalid format.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles)
      => TryParseExact(input, format, formatProvider, styles, out var result)
        ? result
        : throw new FormatException("Input string was not in a correct format.");

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified formats and culture-specific format information.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A time interval that corresponds to <paramref name="input"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider)
      => TryParseExact(input, formats, formatProvider, TimeSpanStyles.None, out var result)
        ? result
        : throw new FormatException("Input string was not in a correct format.");

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified formats, culture-specific format information, and styles.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input"/>.</param>
    /// <returns>A time interval that corresponds to <paramref name="input"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles)
      => TryParseExact(input, formats, formatProvider, styles, out var result)
        ? result
        : throw new FormatException("Input string was not in a correct format.");

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format and culture-specific format information. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, out TimeSpan result)
      => TryParseExact(input, format, formatProvider, TimeSpanStyles.None, out result);

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified format, culture-specific format information, and styles. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="format">A standard or custom format string that defines the required format of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input"/>.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result) {
      result = TimeSpan.Zero;

      if (input == null || format == null)
        return false;

      input = input.Trim();
      if (input.Length == 0)
        return false;

      var isNegative = false;
      var inputIndex = 0;

      // Check for leading minus sign
      if (input[0] == '-') {
        isNegative = true;
        ++inputIndex;
      } else if ((styles & TimeSpanStyles.AssumeNegative) != 0)
        isNegative = true;

      // Handle standard format strings
      return format.Length == 1 
        ? _TryParseStandardFormat(input, inputIndex, format[0], isNegative, out result)
        // Handle custom format strings
        : _TryParseCustomFormat(input, inputIndex, format, isNegative, out result);
    }

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified formats and culture-specific format information. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, out TimeSpan result)
      => TryParseExact(input, formats, formatProvider, TimeSpanStyles.None, out result);

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent by using the specified formats, culture-specific format information, and styles. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input"/>.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input"/>.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseExact(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result) {
      result = TimeSpan.Zero;

      if (formats == null)
        return false;

      foreach (var format in formats)
        if (TryParseExact(input, format, formatProvider, styles, out result))
          return true;

      return false;
    }

  }

  private static bool _TryParseStandardFormat(string input, int startIndex, char format, bool isNegative, out TimeSpan result) {
    result = TimeSpan.Zero;

    // Standard formats: c, t, T (constant), g (general short), G (general long)
    return format switch {
      'c' or 't' or 'T' => _TryParseConstantFormat(input, startIndex, isNegative, out result),
      'g' => _TryParseGeneralShortFormat(input, startIndex, isNegative, out result),
      'G' => _TryParseGeneralLongFormat(input, startIndex, isNegative, out result),
      _ => false
    };
  }

  // Constant format: [-][d.]hh:mm:ss[.fffffff]
  private static bool _TryParseConstantFormat(string input, int startIndex, bool isNegative, out TimeSpan result) {
    result = TimeSpan.Zero;

    var remaining = input.Substring(startIndex);
    var days = 0;
    var hours = 0;
    var minutes = 0;
    var seconds = 0;
    var fraction = 0L;

    // Check for days part (d.)
    var dotIndex = remaining.IndexOf('.');
    var colonIndex = remaining.IndexOf(':');

    if (dotIndex >= 0 && (colonIndex < 0 || dotIndex < colonIndex)) {
      if (!int.TryParse(remaining.Substring(0, dotIndex), out days))
        return false;
      remaining = remaining.Substring(dotIndex + 1);
    }

    // Parse hh:mm:ss
    var timeParts = remaining.Split(':');
    if (timeParts.Length < 2 || timeParts.Length > 3)
      return false;

    if (!int.TryParse(timeParts[0], out hours) || hours < 0 || hours > 23)
      return false;

    if (!int.TryParse(timeParts[1], out minutes) || minutes < 0 || minutes > 59)
      return false;

    if (timeParts.Length == 3) {
      var secondsPart = timeParts[2];
      var fractionIndex = secondsPart.IndexOf('.');
      if (fractionIndex >= 0) {
        if (!int.TryParse(secondsPart.Substring(0, fractionIndex), out seconds) || seconds < 0 || seconds > 59)
          return false;
        var fractionStr = secondsPart.Substring(fractionIndex + 1).PadRight(7, '0');
        if (fractionStr.Length > 7)
          fractionStr = fractionStr.Substring(0, 7);
        if (!long.TryParse(fractionStr, out fraction))
          return false;
      } else {
        if (!int.TryParse(secondsPart, out seconds) || seconds < 0 || seconds > 59)
          return false;
      }
    }

    var ticks = (long)days * TimeSpan.TicksPerDay +
                (long)hours * TimeSpan.TicksPerHour +
                (long)minutes * TimeSpan.TicksPerMinute +
                (long)seconds * TimeSpan.TicksPerSecond +
                fraction;

    result = isNegative ? new TimeSpan(-ticks) : new TimeSpan(ticks);
    return true;
  }

  // General short format: [-][d:]h:mm:ss[.FFFFFFF]
  private static bool _TryParseGeneralShortFormat(string input, int startIndex, bool isNegative, out TimeSpan result)
    => _TryParseConstantFormat(input, startIndex, isNegative, out result);

  // General long format: [-]d:hh:mm:ss.fffffff
  private static bool _TryParseGeneralLongFormat(string input, int startIndex, bool isNegative, out TimeSpan result)
    => _TryParseConstantFormat(input, startIndex, isNegative, out result);

  private static bool _TryParseCustomFormat(string input, int startIndex, string format, bool isNegative, out TimeSpan result) {
    result = TimeSpan.Zero;

    var days = 0;
    var hours = 0;
    var minutes = 0;
    var seconds = 0;
    var fraction = 0L;

    var inputPos = startIndex;
    var formatPos = 0;

    while (formatPos < format.Length && inputPos < input.Length) {
      var formatChar = format[formatPos];

      switch (formatChar) {
        case 'd': {
          var count = _CountConsecutive(format, formatPos, 'd', 8);
          if (!_TryParseDigits(input, ref inputPos, count, out days))
            return false;
          formatPos += count;
          break;
        }
        case 'h': {
          var count = _CountConsecutive(format, formatPos, 'h', 2);
          if (!_TryParseDigits(input, ref inputPos, count, out hours) || hours > 23)
            return false;
          formatPos += count;
          break;
        }
        case 'm': {
          var count = _CountConsecutive(format, formatPos, 'm', 2);
          if (!_TryParseDigits(input, ref inputPos, count, out minutes) || minutes > 59)
            return false;
          formatPos += count;
          break;
        }
        case 's': {
          var count = _CountConsecutive(format, formatPos, 's', 2);
          if (!_TryParseDigits(input, ref inputPos, count, out seconds) || seconds > 59)
            return false;
          formatPos += count;
          break;
        }
        case 'f':
        case 'F': {
          var count = _CountConsecutive(format, formatPos, formatChar, 7);
          if (!_TryParseDigits(input, ref inputPos, count, out var fractionValue))
            if (formatChar == 'f')
              return false;
            else
              fractionValue = 0;
          fraction = fractionValue * (long)Math.Pow(10, 7 - count);
          formatPos += count;
          break;
        }
        case '\\':
          // Escape character
          ++formatPos;
          if (formatPos < format.Length) {
            if (inputPos >= input.Length || input[inputPos] != format[formatPos])
              return false;
            ++inputPos;
            ++formatPos;
          }
          break;
        case '\'':
        case '"': {
          // Literal string
          var endQuote = format.IndexOf(formatChar, formatPos + 1);
          if (endQuote < 0)
            return false;
          var literal = format.Substring(formatPos + 1, endQuote - formatPos - 1);
          if (inputPos + literal.Length > input.Length || input.Substring(inputPos, literal.Length) != literal)
            return false;
          inputPos += literal.Length;
          formatPos = endQuote + 1;
          break;
        }
        case '%':
          // Single character format specifier prefix - just skip it
          ++formatPos;
          break;
        default:
          // Literal character match
          if (input[inputPos] != formatChar)
            return false;
          ++inputPos;
          ++formatPos;
          break;
      }
    }

    // Check that we consumed all format specifiers
    if (formatPos < format.Length)
      return false;

    var ticks = days * TimeSpan.TicksPerDay +
                hours * TimeSpan.TicksPerHour +
                minutes * TimeSpan.TicksPerMinute +
                seconds * TimeSpan.TicksPerSecond +
                fraction;

    result = isNegative ? new(-ticks) : new TimeSpan(ticks);
    return true;
  }

  private static int _CountConsecutive(string format, int startIndex, char c, int max) {
    var count = 0;
    while (startIndex + count < format.Length && format[startIndex + count] == c && count < max)
      ++count;
    return count;
  }

  private static bool _TryParseDigits(string input, ref int inputPos, int minDigits, out int value) {
    value = 0;
    var startPos = inputPos;
    var digitCount = 0;

    while (inputPos < input.Length && char.IsDigit(input[inputPos])) {
      value = value * 10 + (input[inputPos] - '0');
      ++inputPos;
      ++digitCount;
    }

    return digitCount >= minDigits;
  }

}

#endif
