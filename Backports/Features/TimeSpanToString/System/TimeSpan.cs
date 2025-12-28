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

#if !SUPPORTS_TIMESPAN_PROVIDER

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TimeSpanPolyfills {

  extension(TimeSpan @this) {

    /// <summary>
    /// Converts the value of the current <see cref="TimeSpan"/> object to its equivalent string representation
    /// by using the specified format.
    /// </summary>
    /// <param name="format">A standard or custom TimeSpan format string.</param>
    /// <returns>The string representation of the current TimeSpan value, as specified by format.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format) => @this.ToString(format, null);

    /// <summary>
    /// Converts the value of the current <see cref="TimeSpan"/> object to its equivalent string representation
    /// by using the specified format and culture-specific formatting information.
    /// </summary>
    /// <param name="format">A standard or custom TimeSpan format string.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the current TimeSpan value, as specified by format and formatProvider.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format, IFormatProvider? formatProvider) {
      if (string.IsNullOrEmpty(format) || format == "c" || format == "t" || format == "T")
        return @this.ToString();

      var culture = formatProvider as CultureInfo ?? CultureInfo.CurrentCulture;
      var nfi = culture.NumberFormat;
      var decimalSeparator = nfi.NumberDecimalSeparator;

      return format switch {
        "g" => FormatGeneralShort(@this, decimalSeparator),
        "G" => FormatGeneralLong(@this, decimalSeparator),
        _ => FormatCustom(@this, format, decimalSeparator)
      };
    }

  }

  private static string FormatGeneralShort(TimeSpan ts, string decimalSeparator) {
    var negative = ts < TimeSpan.Zero;
    if (negative)
      ts = ts.Negate();

    var days = ts.Days;
    var hours = ts.Hours;
    var minutes = ts.Minutes;
    var seconds = ts.Seconds;
    var fraction = ts.Ticks % TimeSpan.TicksPerSecond;

    var sb = new StringBuilder();
    if (negative)
      sb.Append('-');

    if (days > 0)
      sb.Append(days).Append(':');

    sb.Append(hours).Append(':')
      .Append(minutes.ToString("D2")).Append(':')
      .Append(seconds.ToString("D2"));

    if (fraction > 0)
      sb.Append(decimalSeparator).Append(((int)(fraction / 1000)).ToString("D4").TrimEnd('0'));

    return sb.ToString();
  }

  private static string FormatGeneralLong(TimeSpan ts, string decimalSeparator) {
    var negative = ts < TimeSpan.Zero;
    if (negative)
      ts = ts.Negate();

    var days = ts.Days;
    var hours = ts.Hours;
    var minutes = ts.Minutes;
    var seconds = ts.Seconds;
    var fraction = ts.Ticks % TimeSpan.TicksPerSecond;

    var sb = new StringBuilder();
    if (negative)
      sb.Append('-');

    sb.Append(days).Append(':')
      .Append(hours.ToString("D2")).Append(':')
      .Append(minutes.ToString("D2")).Append(':')
      .Append(seconds.ToString("D2"))
      .Append(decimalSeparator)
      .Append(fraction.ToString("D7"));

    return sb.ToString();
  }

  private static string FormatCustom(TimeSpan ts, string format, string decimalSeparator) {
    var negative = ts < TimeSpan.Zero;
    if (negative)
      ts = ts.Negate();

    var days = ts.Days;
    var hours = ts.Hours;
    var totalHours = (int)ts.TotalHours;
    var minutes = ts.Minutes;
    var seconds = ts.Seconds;
    var milliseconds = ts.Milliseconds;
    var fraction = ts.Ticks % TimeSpan.TicksPerSecond;

    var sb = new StringBuilder();
    var i = 0;
    var length = format.Length;

    while (i < length) {
      var c = format[i];

      switch (c) {
        case '\\' when i + 1 < length:
          sb.Append(format[++i]);
          ++i;
          break;

        case '\'' or '"': {
          var quote = c;
          ++i;
          while (i < length && format[i] != quote)
            sb.Append(format[i++]);
          if (i < length)
            ++i;
          break;
        }

        case 'd': {
          var count = CountConsecutive(format, i, 'd');
          sb.Append(days.ToString("D" + count));
          i += count;
          break;
        }

        case 'h': {
          var count = CountConsecutive(format, i, 'h');
          var value = count > 2 ? totalHours : hours;
          sb.Append(value.ToString("D" + Math.Min(count, 2)));
          i += count;
          break;
        }

        case 'H': {
          var count = CountConsecutive(format, i, 'H');
          sb.Append(totalHours.ToString("D" + count));
          i += count;
          break;
        }

        case 'm': {
          var count = CountConsecutive(format, i, 'm');
          sb.Append(minutes.ToString("D" + Math.Min(count, 2)));
          i += count;
          break;
        }

        case 's': {
          var count = CountConsecutive(format, i, 's');
          sb.Append(seconds.ToString("D" + Math.Min(count, 2)));
          i += count;
          break;
        }

        case 'f': {
          var count = CountConsecutive(format, i, 'f');
          var fractionStr = fraction.ToString("D7");
          sb.Append(fractionStr[..Math.Min(count, 7)]);
          i += count;
          break;
        }

        case 'F': {
          var count = CountConsecutive(format, i, 'F');
          var fractionStr = fraction.ToString("D7")[..Math.Min(count, 7)].TrimEnd('0');
          sb.Append(fractionStr);
          i += count;
          break;
        }

        case '%' when i + 1 < length:
          ++i;
          break;

        default:
          sb.Append(c);
          ++i;
          break;
      }
    }

    return (negative ? "-" : "") + sb.ToString();
  }

  private static int CountConsecutive(string format, int start, char c) {
    var count = 0;
    while (start + count < format.Length && format[start + count] == c)
      ++count;
    return count;
  }

}

#endif
