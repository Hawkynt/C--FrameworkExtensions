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

// TODO: This is a minimal stub for TimeZoneInfo to allow TimeProvider to compile on net20.
// A full implementation would require significant effort to replicate all TimeZoneInfo functionality.
#if !SUPPORTS_TIMEZONEINFO

namespace System;

/// <summary>
/// Represents a time zone.
/// </summary>
/// <remarks>
/// This is a minimal stub implementation for frameworks that don't have TimeZoneInfo.
/// Most operations throw <see cref="NotSupportedException"/>.
/// </remarks>
public sealed class TimeZoneInfo {

  private readonly TimeSpan _baseUtcOffset;
  private readonly string _id;
  private readonly string _displayName;

  private TimeZoneInfo(string id, TimeSpan baseUtcOffset, string displayName) {
    this._id = id;
    this._baseUtcOffset = baseUtcOffset;
    this._displayName = displayName;
  }

  /// <summary>
  /// Gets the time zone identifier.
  /// </summary>
  public string Id => this._id;

  /// <summary>
  /// Gets the display name for the time zone.
  /// </summary>
  public string DisplayName => this._displayName;

  /// <summary>
  /// Gets the general display name that represents the time zone.
  /// </summary>
  public string StandardName => this._displayName;

  /// <summary>
  /// Gets the time difference between the current time zone's standard time and Coordinated Universal Time (UTC).
  /// </summary>
  public TimeSpan BaseUtcOffset => this._baseUtcOffset;

  /// <summary>
  /// Calculates the offset or difference between the time in this time zone and Coordinated Universal Time (UTC)
  /// for a particular date and time.
  /// </summary>
  /// <param name="dateTimeOffset">The date and time to determine the offset for.</param>
  /// <returns>The time zone's UTC offset.</returns>
  public TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset) => this._baseUtcOffset;

  /// <summary>
  /// Calculates the offset or difference between the time in this time zone and Coordinated Universal Time (UTC)
  /// for a particular date and time.
  /// </summary>
  /// <param name="dateTime">The date and time to determine the offset for.</param>
  /// <returns>The time zone's UTC offset.</returns>
  public TimeSpan GetUtcOffset(DateTime dateTime) => this._baseUtcOffset;

  /// <summary>
  /// Gets a <see cref="TimeZoneInfo"/> object that represents the local time zone.
  /// </summary>
  public static TimeZoneInfo Local { get; } = new(
    "Local",
    TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now),
    TimeZone.CurrentTimeZone.StandardName
  );

  /// <summary>
  /// Gets a <see cref="TimeZoneInfo"/> object that represents the Coordinated Universal Time (UTC) zone.
  /// </summary>
  public static TimeZoneInfo Utc { get; } = new("UTC", TimeSpan.Zero, "Coordinated Universal Time");

  /// <summary>
  /// Converts a time to the time in a particular time zone.
  /// </summary>
  /// <param name="dateTimeOffset">The date and time to convert.</param>
  /// <param name="destinationTimeZone">The time zone to convert to.</param>
  /// <returns>The date and time in the destination time zone.</returns>
  public static DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo destinationTimeZone) {
    ArgumentNullException.ThrowIfNull(destinationTimeZone);
    return dateTimeOffset.ToOffset(destinationTimeZone.BaseUtcOffset);
  }

  /// <summary>
  /// Converts a time from one time zone to another.
  /// </summary>
  /// <param name="dateTime">The date and time to convert.</param>
  /// <param name="sourceTimeZone">The time zone of the source time.</param>
  /// <param name="destinationTimeZone">The time zone to convert to.</param>
  /// <returns>The date and time in the destination time zone.</returns>
  public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone) {
    ArgumentNullException.ThrowIfNull(sourceTimeZone);
    ArgumentNullException.ThrowIfNull(destinationTimeZone);

    var utcTime = dateTime - sourceTimeZone.BaseUtcOffset;
    return utcTime + destinationTimeZone.BaseUtcOffset;
  }

  /// <inheritdoc />
  public override string ToString() => this._displayName;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is TimeZoneInfo other && this._id == other._id;

  /// <inheritdoc />
  public override int GetHashCode() => this._id.GetHashCode();

}

#endif
