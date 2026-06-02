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
/// Represents an NTP timestamp (RFC 5905): a 64-bit fixed-point value where the
/// high 32 bits are seconds since 1900-01-01 00:00:00 UTC and the low 32 bits
/// are the fraction of a second in units of 2^-32.
/// </summary>
public readonly struct NtpTimestamp : IComparable, IComparable<NtpTimestamp>, IEquatable<NtpTimestamp> {

  // Number of seconds between the NTP epoch (1900) and the Unix epoch (1970).
  private const double NtpToUnixSeconds = 2208988800.0;
  private const double TwoPow32 = 4294967296.0;

  private static readonly DateTime Epoch = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  /// <summary>
  /// Gets the raw 64-bit NTP timestamp (seconds in high 32 bits, fraction in low 32 bits).
  /// </summary>
  public ulong RawValue { get; }

  private NtpTimestamp(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an <see cref="NtpTimestamp"/> from the raw 64-bit value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static NtpTimestamp FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Gets the value as fractional seconds since the NTP epoch (1900-01-01 UTC).
  /// </summary>
  public double Seconds => (this.RawValue >> 32) + (this.RawValue & 0xFFFFFFFF) / TwoPow32;

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DateTime ToDateTime() => Epoch + TimeSpan.FromSeconds(this.Seconds);

  /// <summary>
  /// Creates an <see cref="NtpTimestamp"/> from a <see cref="DateTime"/>.
  /// </summary>
  public static NtpTimestamp FromDateTime(DateTime dt) {
    var ntpSeconds = (dt.ToUniversalTime() - Epoch).TotalSeconds;
    if (ntpSeconds is < 0 or >= TwoPow32)
      throw new ArgumentOutOfRangeException(nameof(dt), dt, "Value is outside the representable range of NtpTimestamp.");
    var wholeSeconds = (ulong)Math.Floor(ntpSeconds);
    var fraction = (ulong)Math.Round((ntpSeconds - wholeSeconds) * TwoPow32);
    if (fraction > 0xFFFFFFFF)
      fraction = 0xFFFFFFFF;
    return new((wholeSeconds << 32) | fraction);
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(NtpTimestamp other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not NtpTimestamp other)
      throw new ArgumentException("Object must be of type NtpTimestamp.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(NtpTimestamp other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is NtpTimestamp other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(NtpTimestamp left, NtpTimestamp right) => left.Equals(right);
  public static bool operator !=(NtpTimestamp left, NtpTimestamp right) => !left.Equals(right);
  public static bool operator <(NtpTimestamp left, NtpTimestamp right) => left.CompareTo(right) < 0;
  public static bool operator >(NtpTimestamp left, NtpTimestamp right) => left.CompareTo(right) > 0;
  public static bool operator <=(NtpTimestamp left, NtpTimestamp right) => left.CompareTo(right) <= 0;
  public static bool operator >=(NtpTimestamp left, NtpTimestamp right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(NtpTimestamp value) => value.ToDateTime();

}
