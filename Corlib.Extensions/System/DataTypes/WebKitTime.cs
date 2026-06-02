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
/// Represents a WebKit / Chrome timestamp: the number of microseconds since
/// 1601-01-01 00:00:00 UTC.
/// </summary>
public readonly struct WebKitTime : IComparable, IComparable<WebKitTime>, IEquatable<WebKitTime> {

  private static readonly DateTime Epoch = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  /// <summary>
  /// Gets the raw microseconds since 1601-01-01 UTC.
  /// </summary>
  public ulong RawValue { get; }

  private WebKitTime(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="WebKitTime"/> from the raw microsecond count.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static WebKitTime FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  // 1 tick = 100 ns = 0.1 microseconds, so microseconds * 10 = ticks.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DateTime ToDateTime() => Epoch + TimeSpan.FromTicks((long)(this.RawValue * 10));

  /// <summary>
  /// Creates a <see cref="WebKitTime"/> from a <see cref="DateTime"/>.
  /// </summary>
  public static WebKitTime FromDateTime(DateTime dt) {
    var ticks = (dt.ToUniversalTime() - Epoch).Ticks;
    if (ticks < 0)
      throw new ArgumentOutOfRangeException(nameof(dt), dt, "Value is before the WebKit epoch (1601-01-01).");
    return new((ulong)ticks / 10);
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(WebKitTime other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not WebKitTime other)
      throw new ArgumentException("Object must be of type WebKitTime.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(WebKitTime other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is WebKitTime other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(WebKitTime left, WebKitTime right) => left.Equals(right);
  public static bool operator !=(WebKitTime left, WebKitTime right) => !left.Equals(right);
  public static bool operator <(WebKitTime left, WebKitTime right) => left.CompareTo(right) < 0;
  public static bool operator >(WebKitTime left, WebKitTime right) => left.CompareTo(right) > 0;
  public static bool operator <=(WebKitTime left, WebKitTime right) => left.CompareTo(right) <= 0;
  public static bool operator >=(WebKitTime left, WebKitTime right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(WebKitTime value) => value.ToDateTime();

}
