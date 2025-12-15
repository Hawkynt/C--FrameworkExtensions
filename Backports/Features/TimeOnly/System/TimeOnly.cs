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

#if !SUPPORTS_TIMEONLY

using System.Globalization;

namespace System;

/// <summary>
/// Represents a time of day, as would be read from a clock, within the range 00:00:00 to 23:59:59.9999999.
/// </summary>
public readonly struct TimeOnly : IComparable, IComparable<TimeOnly>, IEquatable<TimeOnly>, IFormattable {

  private readonly long _ticks;

  private const long MinTicks = 0;
  private const long MaxTicks = TimeSpan.TicksPerDay - 1;

  /// <summary>
  /// Gets the earliest possible time that can be created.
  /// </summary>
  public static TimeOnly MinValue => new(MinTicks);

  /// <summary>
  /// Gets the latest possible time that can be created.
  /// </summary>
  public static TimeOnly MaxValue => new(MaxTicks);

  /// <summary>
  /// Initializes a new instance of the TimeOnly structure using the specified number of ticks.
  /// </summary>
  public TimeOnly(long ticks) {
    ArgumentOutOfRangeException.ThrowIfNegative(ticks);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ticks, MaxTicks);
    _ticks = ticks;
  }

  /// <summary>
  /// Initializes a new instance of the TimeOnly structure to the specified hour and minute.
  /// </summary>
  public TimeOnly(int hour, int minute) : this(hour, minute, 0, 0) { }

  /// <summary>
  /// Initializes a new instance of the TimeOnly structure to the specified hour, minute, and second.
  /// </summary>
  public TimeOnly(int hour, int minute, int second) : this(hour, minute, second, 0) { }

  /// <summary>
  /// Initializes a new instance of the TimeOnly structure to the specified hour, minute, second, and millisecond.
  /// </summary>
  public TimeOnly(int hour, int minute, int second, int millisecond) {
    ArgumentOutOfRangeException.ThrowIfNegative(hour);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(hour, 23);
    ArgumentOutOfRangeException.ThrowIfNegative(minute);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(minute, 59);
    ArgumentOutOfRangeException.ThrowIfNegative(second);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(second, 59);
    ArgumentOutOfRangeException.ThrowIfNegative(millisecond);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(millisecond, 999);

    _ticks = hour * TimeSpan.TicksPerHour +
             minute * TimeSpan.TicksPerMinute +
             second * TimeSpan.TicksPerSecond +
             millisecond * TimeSpan.TicksPerMillisecond;
  }

  /// <summary>
  /// Gets the hour component of the time represented by this instance.
  /// </summary>
  public int Hour => (int)(_ticks / TimeSpan.TicksPerHour);

  /// <summary>
  /// Gets the minute component of the time represented by this instance.
  /// </summary>
  public int Minute => (int)(_ticks / TimeSpan.TicksPerMinute % 60);

  /// <summary>
  /// Gets the second component of the time represented by this instance.
  /// </summary>
  public int Second => (int)(_ticks / TimeSpan.TicksPerSecond % 60);

  /// <summary>
  /// Gets the millisecond component of the time represented by this instance.
  /// </summary>
  public int Millisecond => (int)(_ticks / TimeSpan.TicksPerMillisecond % 1000);

  /// <summary>
  /// Gets the number of ticks that represent the time of this instance.
  /// </summary>
  public long Ticks => _ticks;

  /// <summary>
  /// Returns a new TimeOnly that adds the specified TimeSpan to the value of this instance.
  /// </summary>
  public TimeOnly Add(TimeSpan value) => this.Add(value, out _);

  /// <summary>
  /// Returns a new TimeOnly that adds the specified TimeSpan to the value of this instance.
  /// </summary>
  public TimeOnly Add(TimeSpan value, out int wrappedDays) {
    var totalTicks = _ticks + value.Ticks;
    wrappedDays = (int)(totalTicks / TimeSpan.TicksPerDay);
    totalTicks %= TimeSpan.TicksPerDay;
    if (totalTicks < 0) {
      totalTicks += TimeSpan.TicksPerDay;
      --wrappedDays;
    }
    return new(totalTicks);
  }

  /// <summary>
  /// Returns a new TimeOnly that adds the specified number of hours to the value of this instance.
  /// </summary>
  public TimeOnly AddHours(double value) => this.Add(TimeSpan.FromHours(value));

  /// <summary>
  /// Returns a new TimeOnly that adds the specified number of hours to the value of this instance.
  /// </summary>
  public TimeOnly AddHours(double value, out int wrappedDays) => this.Add(TimeSpan.FromHours(value), out wrappedDays);

  /// <summary>
  /// Returns a new TimeOnly that adds the specified number of minutes to the value of this instance.
  /// </summary>
  public TimeOnly AddMinutes(double value) => this.Add(TimeSpan.FromMinutes(value));

  /// <summary>
  /// Returns a new TimeOnly that adds the specified number of minutes to the value of this instance.
  /// </summary>
  public TimeOnly AddMinutes(double value, out int wrappedDays) => this.Add(TimeSpan.FromMinutes(value), out wrappedDays);

  /// <summary>
  /// Determines whether a specified time falls within the range provided.
  /// </summary>
  public bool IsBetween(TimeOnly start, TimeOnly end) {
    if (start._ticks <= end._ticks)
      return _ticks >= start._ticks && _ticks < end._ticks;

    // Range wraps around midnight
    return _ticks >= start._ticks || _ticks < end._ticks;
  }

  /// <summary>
  /// Returns a TimeOnly instance that is set to the time part of the specified DateTime.
  /// </summary>
  public static TimeOnly FromDateTime(DateTime dateTime) => new(dateTime.TimeOfDay.Ticks);

  /// <summary>
  /// Constructs a TimeOnly object from a TimeSpan.
  /// </summary>
  public static TimeOnly FromTimeSpan(TimeSpan timeSpan) => new(timeSpan.Ticks);

  /// <summary>
  /// Converts this TimeOnly instance to a TimeSpan.
  /// </summary>
  public TimeSpan ToTimeSpan() => new(_ticks);

  /// <summary>
  /// Returns the long time string representation of the current TimeOnly object.
  /// </summary>
  public string ToLongTimeString() => new DateTime(_ticks).ToString("T");

  /// <summary>
  /// Returns the short time string representation of the current TimeOnly object.
  /// </summary>
  public string ToShortTimeString() => new DateTime(_ticks).ToString("t");

  public override string ToString() => new DateTime(_ticks).ToString("t");
  public string ToString(string? format) => new DateTime(_ticks).ToString(format ?? "t");
  public string ToString(IFormatProvider? provider) => new DateTime(_ticks).ToString("t", provider);
  public string ToString(string? format, IFormatProvider? provider) => new DateTime(_ticks).ToString(format ?? "t", provider);

  public override int GetHashCode() => _ticks.GetHashCode();
  public override bool Equals(object? obj) => obj is TimeOnly other && this.Equals(other);
  public bool Equals(TimeOnly other) => _ticks == other._ticks;

  public int CompareTo(TimeOnly other) => _ticks.CompareTo(other._ticks);
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not TimeOnly other)
      throw new ArgumentException("Object must be of type TimeOnly.");
    return this.CompareTo(other);
  }

  public static bool operator ==(TimeOnly left, TimeOnly right) => left._ticks == right._ticks;
  public static bool operator !=(TimeOnly left, TimeOnly right) => left._ticks != right._ticks;
  public static bool operator <(TimeOnly left, TimeOnly right) => left._ticks < right._ticks;
  public static bool operator >(TimeOnly left, TimeOnly right) => left._ticks > right._ticks;
  public static bool operator <=(TimeOnly left, TimeOnly right) => left._ticks <= right._ticks;
  public static bool operator >=(TimeOnly left, TimeOnly right) => left._ticks >= right._ticks;
  public static TimeSpan operator -(TimeOnly t1, TimeOnly t2) => new(t1._ticks - t2._ticks);

  /// <summary>
  /// Converts a string to a TimeOnly.
  /// </summary>
  public static TimeOnly Parse(string s) => FromDateTime(DateTime.Parse(s));

  /// <summary>
  /// Converts a string to a TimeOnly using the specified format provider.
  /// </summary>
  public static TimeOnly Parse(string s, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.Parse(s, provider, style));

  /// <summary>
  /// Converts a span of characters to a TimeOnly.
  /// </summary>
  public static TimeOnly Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.Parse(s, provider, style));

  /// <summary>
  /// Converts a string to a TimeOnly using the specified format.
  /// </summary>
  public static TimeOnly ParseExact(string s, string format)
    => FromDateTime(DateTime.ParseExact(s, format, null));

  /// <summary>
  /// Converts a string to a TimeOnly using the specified format and format provider.
  /// </summary>
  public static TimeOnly ParseExact(string s, string format, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.ParseExact(s, format, provider, style));

  /// <summary>
  /// Converts a string to a TimeOnly using the specified formats.
  /// </summary>
  public static TimeOnly ParseExact(string s, string[] formats)
    => FromDateTime(DateTime.ParseExact(s, formats, null, DateTimeStyles.None));

  /// <summary>
  /// Converts a string to a TimeOnly using the specified formats and format provider.
  /// </summary>
  public static TimeOnly ParseExact(string s, string[] formats, IFormatProvider? provider, DateTimeStyles style = DateTimeStyles.None)
    => FromDateTime(DateTime.ParseExact(s, formats, provider, style));

  /// <summary>
  /// Tries to convert a string to a TimeOnly.
  /// </summary>
  public static bool TryParse(string? s, out TimeOnly result) {
    if (DateTime.TryParse(s, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a TimeOnly using the specified format provider.
  /// </summary>
  public static bool TryParse(string? s, IFormatProvider? provider, DateTimeStyles style, out TimeOnly result) {
    if (DateTime.TryParse(s, provider, style, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a span of characters to a TimeOnly.
  /// </summary>
  public static bool TryParse(ReadOnlySpan<char> s, out TimeOnly result) {
    if (DateTime.TryParse(s, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a span of characters to a TimeOnly using the specified format provider.
  /// </summary>
  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, DateTimeStyles style, out TimeOnly result) {
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
    => new DateTime(_ticks).TryFormat(destination, out charsWritten, format.Length == 0 ? "t".AsSpan() : format, provider);

  /// <summary>
  /// Tries to convert a string to a TimeOnly using the specified format.
  /// </summary>
  public static bool TryParseExact(string? s, string? format, out TimeOnly result) {
    if (DateTime.TryParseExact(s, format, null, DateTimeStyles.None, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a TimeOnly using the specified format and format provider.
  /// </summary>
  public static bool TryParseExact(string? s, string? format, IFormatProvider? provider, DateTimeStyles style, out TimeOnly result) {
    if (DateTime.TryParseExact(s, format, provider, style, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a TimeOnly using the specified formats.
  /// </summary>
  public static bool TryParseExact(string? s, string?[]? formats, out TimeOnly result) {
    if (DateTime.TryParseExact(s, formats, null, DateTimeStyles.None, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a TimeOnly using the specified formats and format provider.
  /// </summary>
  public static bool TryParseExact(string? s, string?[]? formats, IFormatProvider? provider, DateTimeStyles style, out TimeOnly result) {
    if (DateTime.TryParseExact(s, formats, provider, style, out var dt)) {
      result = FromDateTime(dt);
      return true;
    }
    result = default;
    return false;
  }

}

#endif
