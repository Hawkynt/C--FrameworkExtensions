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
/// Represents a GPS time value: the number of seconds since the GPS epoch
/// 1980-01-06 00:00:00 UTC. (Leap seconds are not modeled.)
/// </summary>
public readonly struct GpsTime : IComparable, IComparable<GpsTime>, IEquatable<GpsTime> {

  private static readonly DateTime Epoch = new(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);

  /// <summary>
  /// Gets the raw seconds since 1980-01-06 UTC.
  /// </summary>
  public uint RawValue { get; }

  private GpsTime(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="GpsTime"/> from the raw second count.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static GpsTime FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DateTime ToDateTime() => Epoch + TimeSpan.FromSeconds(this.RawValue);

  /// <summary>
  /// Creates a <see cref="GpsTime"/> from a <see cref="DateTime"/>.
  /// </summary>
  public static GpsTime FromDateTime(DateTime dt) {
    var seconds = (dt.ToUniversalTime() - Epoch).TotalSeconds;
    if (seconds is < 0 or > uint.MaxValue)
      throw new ArgumentOutOfRangeException(nameof(dt), dt, "Value is outside the representable range of GpsTime.");
    return new((uint)Math.Round(seconds));
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(GpsTime other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not GpsTime other)
      throw new ArgumentException("Object must be of type GpsTime.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(GpsTime other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is GpsTime other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(GpsTime left, GpsTime right) => left.Equals(right);
  public static bool operator !=(GpsTime left, GpsTime right) => !left.Equals(right);
  public static bool operator <(GpsTime left, GpsTime right) => left.CompareTo(right) < 0;
  public static bool operator >(GpsTime left, GpsTime right) => left.CompareTo(right) > 0;
  public static bool operator <=(GpsTime left, GpsTime right) => left.CompareTo(right) <= 0;
  public static bool operator >=(GpsTime left, GpsTime right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(GpsTime value) => value.ToDateTime();

}
