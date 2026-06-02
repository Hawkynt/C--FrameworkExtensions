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
/// Represents a 64-bit Unix timestamp: the number of seconds since
/// 1970-01-01 00:00:00 UTC.
/// </summary>
public readonly struct UnixTime64 : IComparable, IComparable<UnixTime64>, IEquatable<UnixTime64> {

  private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  /// <summary>
  /// Gets the raw seconds since 1970-01-01 UTC.
  /// </summary>
  public long RawValue { get; }

  private UnixTime64(long raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="UnixTime64"/> from the raw second count.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UnixTime64 FromRaw(long raw) => new(raw);

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DateTime ToDateTime() => Epoch + TimeSpan.FromSeconds(this.RawValue);

  /// <summary>
  /// Creates a <see cref="UnixTime64"/> from a <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UnixTime64 FromDateTime(DateTime dt) => new((long)Math.Round((dt.ToUniversalTime() - Epoch).TotalSeconds));

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(UnixTime64 other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not UnixTime64 other)
      throw new ArgumentException("Object must be of type UnixTime64.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UnixTime64 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is UnixTime64 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(UnixTime64 left, UnixTime64 right) => left.Equals(right);
  public static bool operator !=(UnixTime64 left, UnixTime64 right) => !left.Equals(right);
  public static bool operator <(UnixTime64 left, UnixTime64 right) => left.CompareTo(right) < 0;
  public static bool operator >(UnixTime64 left, UnixTime64 right) => left.CompareTo(right) > 0;
  public static bool operator <=(UnixTime64 left, UnixTime64 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(UnixTime64 left, UnixTime64 right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(UnixTime64 value) => value.ToDateTime();

}
