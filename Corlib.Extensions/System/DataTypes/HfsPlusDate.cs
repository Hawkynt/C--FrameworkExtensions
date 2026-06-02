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
/// Represents an HFS+ / classic Mac OS date: the number of seconds since
/// 1904-01-01 00:00:00 UTC.
/// </summary>
public readonly struct HfsPlusDate : IComparable, IComparable<HfsPlusDate>, IEquatable<HfsPlusDate> {

  private static readonly DateTime Epoch = new(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  /// <summary>
  /// Gets the raw seconds since 1904-01-01 UTC.
  /// </summary>
  public uint RawValue { get; }

  private HfsPlusDate(uint raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an <see cref="HfsPlusDate"/> from the raw second count.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HfsPlusDate FromRaw(uint raw) => new(raw);

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DateTime ToDateTime() => Epoch + TimeSpan.FromSeconds(this.RawValue);

  /// <summary>
  /// Creates an <see cref="HfsPlusDate"/> from a <see cref="DateTime"/>.
  /// </summary>
  public static HfsPlusDate FromDateTime(DateTime dt) {
    var seconds = (dt.ToUniversalTime() - Epoch).TotalSeconds;
    if (seconds is < 0 or > uint.MaxValue)
      throw new ArgumentOutOfRangeException(nameof(dt), dt, "Value is outside the representable range of HfsPlusDate.");
    return new((uint)Math.Round(seconds));
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(HfsPlusDate other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not HfsPlusDate other)
      throw new ArgumentException("Object must be of type HfsPlusDate.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(HfsPlusDate other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is HfsPlusDate other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(HfsPlusDate left, HfsPlusDate right) => left.Equals(right);
  public static bool operator !=(HfsPlusDate left, HfsPlusDate right) => !left.Equals(right);
  public static bool operator <(HfsPlusDate left, HfsPlusDate right) => left.CompareTo(right) < 0;
  public static bool operator >(HfsPlusDate left, HfsPlusDate right) => left.CompareTo(right) > 0;
  public static bool operator <=(HfsPlusDate left, HfsPlusDate right) => left.CompareTo(right) <= 0;
  public static bool operator >=(HfsPlusDate left, HfsPlusDate right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(HfsPlusDate value) => value.ToDateTime();

}
