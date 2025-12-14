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

// Feature flags:
//   SUPPORTS_DATETIME_SPAN: Core 3.0+, Std 2.1 - Span-based Parse/TryParse overloads
//   SUPPORTS_DATETIME_UNIXEPOCH: Core 5.0+ - UnixEpoch static field
//   SUPPORTS_DATETIME_MICROSECOND: Core 7.0+ - Microsecond, Nanosecond, AddMicroseconds
//   SUPPORTS_DATETIME_DATEONLY_CTOR: Core 8.0+ - Constructor with DateOnly/TimeOnly, Deconstruct

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class DateTimePolyfills {

#if !SUPPORTS_DATETIME_UNIXEPOCH

  /// <summary>
  /// Represents the Unix epoch (January 1, 1970, 00:00:00 UTC).
  /// </summary>
  public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

#endif

#if !SUPPORTS_DATETIME_SPAN

  extension(DateTime) {

    /// <summary>
    /// Converts a span of characters to a DateTime.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null, DateTimeStyles styles = DateTimeStyles.None)
      => DateTime.Parse(s.ToString(), provider, styles);

    /// <summary>
    /// Converts a span of characters to a DateTime using the specified format.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
      => DateTime.ParseExact(s.ToString(), format.ToString(), provider, style);

    /// <summary>
    /// Converts a span of characters to a DateTime using the specified formats.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ParseExact(ReadOnlySpan<char> s, string[] formats, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
      => DateTime.ParseExact(s.ToString(), formats, provider, style);

    /// <summary>
    /// Tries to convert a span of characters to a DateTime.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> s, out DateTime result)
      => DateTime.TryParse(s.ToString(), out result);

    /// <summary>
    /// Tries to convert a span of characters to a DateTime using the specified format provider and style.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, DateTimeStyles styles, out DateTime result)
      => DateTime.TryParse(s.ToString(), provider, styles, out result);

    /// <summary>
    /// Tries to convert a span of characters to a DateTime using the specified format.
    /// </summary>
    public static bool TryParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider? provider, DateTimeStyles style, out DateTime result)
      => DateTime.TryParseExact(s.ToString(), format.ToString(), provider, style, out result);

    /// <summary>
    /// Tries to convert a span of characters to a DateTime using the specified formats.
    /// </summary>
    public static bool TryParseExact(ReadOnlySpan<char> s, string?[]? formats, IFormatProvider? provider, DateTimeStyles style, out DateTime result)
      => DateTime.TryParseExact(s.ToString(), formats, provider, style, out result);

  }

  extension(DateTime @this) {

    /// <summary>
    /// Tries to format the DateTime into the provided span of characters.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) {
      var str = format.Length == 0
        ? @this.ToString(provider)
        : @this.ToString(format.ToString(), provider);

      if (str.Length > destination.Length) {
        charsWritten = 0;
        return false;
      }
      str.AsSpan().CopyTo(destination);
      charsWritten = str.Length;
      return true;
    }

  }

#endif

#if !SUPPORTS_DATETIME_MICROSECOND

  extension(DateTime @this) {

    /// <summary>
    /// Gets the microsecond component of the date represented by this instance.
    /// </summary>
    public int Microsecond {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (int)((@this.Ticks / 10) % 1000);
    }

    /// <summary>
    /// Gets the nanosecond component of the date represented by this instance.
    /// </summary>
    public int Nanosecond {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (int)((@this.Ticks % 10) * 100);
    }

    /// <summary>
    /// Returns a new DateTime that adds the specified number of microseconds to the value of this instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTime AddMicroseconds(double value) => @this.AddTicks((long)(value * 10));

  }

#endif

#if !SUPPORTS_DATETIME_DATEONLY_CTOR

  extension(DateTime) {

    /// <summary>
    /// Creates a DateTime from a DateOnly and TimeOnly.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime FromDateAndTime(DateOnly date, TimeOnly time)
      => date.ToDateTime(time);

    /// <summary>
    /// Creates a DateTime from a DateOnly and TimeOnly with the specified DateTimeKind.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime FromDateAndTime(DateOnly date, TimeOnly time, DateTimeKind kind)
      => date.ToDateTime(time, kind);

  }

  extension(DateTime @this) {

    /// <summary>
    /// Deconstructs the DateTime into its DateOnly and TimeOnly components.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out DateOnly date, out TimeOnly time) {
      date = DateOnly.FromDateTime(@this);
      time = TimeOnly.FromDateTime(@this);
    }

    /// <summary>
    /// Deconstructs the DateTime into year, month, and day.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out int year, out int month, out int day) {
      year = @this.Year;
      month = @this.Month;
      day = @this.Day;
    }

  }

#endif

}
