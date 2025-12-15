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

#if !SUPPORTS_DATEONLY

using System.Globalization;
using Guard;

namespace System;

/// <summary>
/// Represents dates with values ranging from January 1, 0001 Anno Domini (Common Era)
/// through December 31, 9999 A.D. (C.E.) in the Gregorian calendar.
/// </summary>
public readonly struct DateOnly : IComparable, IComparable<DateOnly>, IEquatable<DateOnly>, IFormattable {

  private readonly int _dayNumber;

  private const int MinDayNumber = 0;
  private const int MaxDayNumber = 3652058; // December 31, 9999

  /// <summary>
  /// Gets the earliest possible date that can be created.
  /// </summary>
  public static DateOnly MinValue => new(MinDayNumber);

  /// <summary>
  /// Gets the latest possible date that can be created.
  /// </summary>
  public static DateOnly MaxValue => new(MaxDayNumber);

  /// <summary>
  /// Initializes a new instance of the DateOnly structure to the specified year, month, and day.
  /// </summary>
  public DateOnly(int year, int month, int day) {
    var dt = new DateTime(year, month, day);
    _dayNumber = (int)(dt.Ticks / TimeSpan.TicksPerDay);
  }

  /// <summary>
  /// Initializes a new instance of the DateOnly structure to the specified year, month, and day for the specified calendar.
  /// </summary>
  public DateOnly(int year, int month, int day, Calendar calendar) {
    var dt = new DateTime(year, month, day, calendar);
    _dayNumber = (int)(dt.Ticks / TimeSpan.TicksPerDay);
  }

  private DateOnly(int dayNumber) => _dayNumber = dayNumber;

  /// <summary>
  /// Gets the year component of the date represented by this instance.
  /// </summary>
  public int Year => this._GetDateTime().Year;

  /// <summary>
  /// Gets the month component of the date represented by this instance.
  /// </summary>
  public int Month => this._GetDateTime().Month;

  /// <summary>
  /// Gets the day component of the date represented by this instance.
  /// </summary>
  public int Day => this._GetDateTime().Day;

  /// <summary>
  /// Gets the day of the week represented by this instance.
  /// </summary>
  public DayOfWeek DayOfWeek => this._GetDateTime().DayOfWeek;

  /// <summary>
  /// Gets the day of the year represented by this instance.
  /// </summary>
  public int DayOfYear => this._GetDateTime().DayOfYear;

  /// <summary>
  /// Gets the number of days since January 1, 0001 in the Proleptic Gregorian calendar.
  /// </summary>
  public int DayNumber => _dayNumber;

  private DateTime _GetDateTime() => new(_dayNumber * TimeSpan.TicksPerDay);

  /// <summary>
  /// Returns a new DateOnly that adds the specified number of days to the value of this instance.
  /// </summary>
  public DateOnly AddDays(int value) => new(_dayNumber + value);

  /// <summary>
  /// Returns a new DateOnly that adds the specified number of months to the value of this instance.
  /// </summary>
  public DateOnly AddMonths(int value) => FromDateTime(this._GetDateTime().AddMonths(value));

  /// <summary>
  /// Returns a new DateOnly that adds the specified number of years to the value of this instance.
  /// </summary>
  public DateOnly AddYears(int value) => FromDateTime(this._GetDateTime().AddYears(value));

  /// <summary>
  /// Returns a DateOnly instance that is set to the date part of the specified dateTime.
  /// </summary>
  public static DateOnly FromDateTime(DateTime dateTime) => new((int)(dateTime.Ticks / TimeSpan.TicksPerDay));

  /// <summary>
  /// Creates a new instance of the DateOnly structure to the specified number of days.
  /// </summary>
  public static DateOnly FromDayNumber(int dayNumber) {
    ArgumentOutOfRangeException.ThrowIfLessThan(dayNumber,MinDayNumber);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(dayNumber,MaxDayNumber);
    
    return new(dayNumber);
  }

  /// <summary>
  /// Returns a DateTime instance with the specified input kind that is set to the date of this DateOnly instance and the time of the specified input TimeOnly instance.
  /// </summary>
  public DateTime ToDateTime(TimeOnly time) => new(_dayNumber * TimeSpan.TicksPerDay + time.Ticks);

  /// <summary>
  /// Returns a DateTime instance with the specified input kind that is set to the date of this DateOnly instance and the time of the specified input TimeOnly instance.
  /// </summary>
  public DateTime ToDateTime(TimeOnly time, DateTimeKind kind) => new(_dayNumber * TimeSpan.TicksPerDay + time.Ticks, kind);

  /// <summary>
  /// Returns the long date string representation of the current DateOnly object.
  /// </summary>
  public string ToLongDateString() => this._GetDateTime().ToLongDateString();

  /// <summary>
  /// Returns the short date string representation of the current DateOnly object.
  /// </summary>
  public string ToShortDateString() => this._GetDateTime().ToShortDateString();

  public override string ToString() => this._GetDateTime().ToString("d");
  public string ToString(string? format) => this._GetDateTime().ToString(format ?? "d");
  public string ToString(IFormatProvider? provider) => this._GetDateTime().ToString("d", provider);
  public string ToString(string? format, IFormatProvider? provider) => this._GetDateTime().ToString(format ?? "d", provider);

  public override int GetHashCode() => _dayNumber;
  public override bool Equals(object? obj) => obj is DateOnly other && this.Equals(other);
  public bool Equals(DateOnly other) => _dayNumber == other._dayNumber;

  public int CompareTo(DateOnly other) => _dayNumber.CompareTo(other._dayNumber);
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not DateOnly other)
      throw new ArgumentException("Object must be of type DateOnly.");
    return this.CompareTo(other);
  }

  public static bool operator ==(DateOnly left, DateOnly right) => left._dayNumber == right._dayNumber;
  public static bool operator !=(DateOnly left, DateOnly right) => left._dayNumber != right._dayNumber;
  public static bool operator <(DateOnly left, DateOnly right) => left._dayNumber < right._dayNumber;
  public static bool operator >(DateOnly left, DateOnly right) => left._dayNumber > right._dayNumber;
  public static bool operator <=(DateOnly left, DateOnly right) => left._dayNumber <= right._dayNumber;
  public static bool operator >=(DateOnly left, DateOnly right) => left._dayNumber >= right._dayNumber;

  /// <summary>
  /// Converts a string to a DateOnly.
  /// </summary>
  public static DateOnly Parse(string s) => FromDateTime(DateTime.Parse(s));

  /// <summary>
  /// Converts a string to a DateOnly using the specified format provider.
  /// </summary>
  public static DateOnly Parse(string s, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.Parse(s, provider, style));

  /// <summary>
  /// Converts a span of characters to a DateOnly.
  /// </summary>
  public static DateOnly Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.Parse(s, provider, style));

  /// <summary>
  /// Converts a string to a DateOnly using the specified format.
  /// </summary>
  public static DateOnly ParseExact(string s, string format)
    => FromDateTime(DateTime.ParseExact(s, format, null));

  /// <summary>
  /// Converts a string to a DateOnly using the specified format and format provider.
  /// </summary>
  public static DateOnly ParseExact(string s, string format, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.ParseExact(s, format, provider, style));

  /// <summary>
  /// Converts a string to a DateOnly using the specified formats.
  /// </summary>
  public static DateOnly ParseExact(string s, string[] formats)
    => FromDateTime(DateTime.ParseExact(s, formats, null, DateTimeStyles.None));

  /// <summary>
  /// Converts a string to a DateOnly using the specified formats and format provider.
  /// </summary>
  public static DateOnly ParseExact(string s, string[] formats, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.ParseExact(s, formats, provider, style));

  /// <summary>
  /// Tries to convert a string to a DateOnly.
  /// </summary>
  public static bool TryParse(string? s, out DateOnly result) {
    if (DateTime.TryParse(s, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a DateOnly using the specified format provider.
  /// </summary>
  public static bool TryParse(string? s, IFormatProvider? provider, DateTimeStyles style, out DateOnly result) {
    if (DateTime.TryParse(s, provider, style, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a span of characters to a DateOnly.
  /// </summary>
  public static bool TryParse(ReadOnlySpan<char> s, out DateOnly result) {
    if (DateTime.TryParse(s, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a span of characters to a DateOnly using the specified format provider.
  /// </summary>
  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, DateTimeStyles style, out DateOnly result) {
    if (DateTime.TryParse(s, provider, style, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to format the value of the current instance into the provided span of characters.
  /// </summary>
  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    => this._GetDateTime().TryFormat(destination, out charsWritten, format.Length == 0 ? "d".AsSpan() : format, provider);

  /// <summary>
  /// Tries to convert a string to a DateOnly using the specified format.
  /// </summary>
  public static bool TryParseExact(string? s, string? format, out DateOnly result) {
    if (DateTime.TryParseExact(s, format, null, DateTimeStyles.None, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a DateOnly using the specified format and format provider.
  /// </summary>
  public static bool TryParseExact(string? s, string? format, IFormatProvider? provider, DateTimeStyles style, out DateOnly result) {
    if (DateTime.TryParseExact(s, format, provider, style, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a DateOnly using the specified formats.
  /// </summary>
  public static bool TryParseExact(string? s, string?[]? formats, out DateOnly result) {
    if (DateTime.TryParseExact(s, formats, null, DateTimeStyles.None, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a DateOnly using the specified formats and format provider.
  /// </summary>
  public static bool TryParseExact(string? s, string?[]? formats, IFormatProvider? provider, DateTimeStyles style, out DateOnly result) {
    if (DateTime.TryParseExact(s, formats, provider, style, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

}

#endif
