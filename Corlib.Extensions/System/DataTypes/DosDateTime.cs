#nullable enable

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
//

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents an MS-DOS / FAT packed date-time value.
/// High 16 bits = date: bits 9-15 = year-1980, bits 5-8 = month (1-12), bits 0-4 = day.
/// Low 16 bits = time: bits 11-15 = hour, bits 5-10 = minute, bits 0-4 = second/2.
/// </summary>
public readonly struct DosDateTime : IComparable, IComparable<DosDateTime>, IEquatable<DosDateTime> {

  /// <summary>
  /// Gets the raw 32-bit packed DOS date-time value (date in high 16 bits, time in low 16 bits).
  /// </summary>
  public uint RawValue { get; }

  private DosDateTime(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="DosDateTime"/> from the raw 32-bit packed value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static DosDateTime FromRaw(uint raw) => new(raw);

  private ushort Date => (ushort)(this.RawValue >> 16);
  private ushort Time => (ushort)(this.RawValue & 0xFFFF);

  /// <summary>Gets the year (1980-2107).</summary>
  public int Year => ((this.Date >> 9) & 0x7F) + 1980;

  /// <summary>Gets the month (1-12).</summary>
  public int Month => (this.Date >> 5) & 0x0F;

  /// <summary>Gets the day of month (1-31).</summary>
  public int Day => this.Date & 0x1F;

  /// <summary>Gets the hour (0-23).</summary>
  public int Hour => (this.Time >> 11) & 0x1F;

  /// <summary>Gets the minute (0-59).</summary>
  public int Minute => (this.Time >> 5) & 0x3F;

  /// <summary>Gets the second (0-58, even values only - DOS stores seconds/2).</summary>
  public int Second => (this.Time & 0x1F) * 2;

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  public DateTime ToDateTime() => new(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second, DateTimeKind.Utc);

  /// <summary>
  /// Creates a <see cref="DosDateTime"/> from a <see cref="DateTime"/>.
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when the year is outside 1980-2107.</exception>
  public static DosDateTime FromDateTime(DateTime dt) {
    dt = dt.ToUniversalTime();
    if (dt.Year is < 1980 or > 2107)
      throw new ArgumentOutOfRangeException(nameof(dt), dt, "Year must be between 1980 and 2107 for DosDateTime.");

    var date = (uint)(((dt.Year - 1980) << 9) | (dt.Month << 5) | dt.Day);
    var time = (uint)((dt.Hour << 11) | (dt.Minute << 5) | (dt.Second / 2));
    return new((date << 16) | time);
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(DosDateTime other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not DosDateTime other)
      throw new ArgumentException("Object must be of type DosDateTime.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(DosDateTime other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is DosDateTime other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(DosDateTime left, DosDateTime right) => left.Equals(right);
  public static bool operator !=(DosDateTime left, DosDateTime right) => !left.Equals(right);
  public static bool operator <(DosDateTime left, DosDateTime right) => left.CompareTo(right) < 0;
  public static bool operator >(DosDateTime left, DosDateTime right) => left.CompareTo(right) > 0;
  public static bool operator <=(DosDateTime left, DosDateTime right) => left.CompareTo(right) <= 0;
  public static bool operator >=(DosDateTime left, DosDateTime right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(DosDateTime value) => value.ToDateTime();

}
