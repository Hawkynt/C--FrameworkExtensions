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
/// Represents an OLE Automation Date: a double-precision count of days since
/// 1899-12-30, stored as the IEEE-754 bit pattern of that double.
/// </summary>
public readonly struct OleDate : IComparable, IComparable<OleDate>, IEquatable<OleDate> {

  /// <summary>
  /// Gets the raw IEEE-754 bit pattern of the OLE Automation Date (days since 1899-12-30).
  /// </summary>
  public ulong RawValue { get; }

  private OleDate(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an <see cref="OleDate"/> from the raw IEEE-754 bit pattern.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OleDate FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Gets the OLE Automation Date as a number of days since 1899-12-30.
  /// </summary>
  public double Days => BitConverter.Int64BitsToDouble((long)this.RawValue);

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DateTime ToDateTime() => DateTime.SpecifyKind(DateTime.FromOADate(this.Days), DateTimeKind.Utc);

  /// <summary>
  /// Creates an <see cref="OleDate"/> from a <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OleDate FromDateTime(DateTime dt) => new((ulong)BitConverter.DoubleToInt64Bits(DateTime.SpecifyKind(dt.ToUniversalTime(), DateTimeKind.Unspecified).ToOADate()));

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(OleDate other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not OleDate other)
      throw new ArgumentException("Object must be of type OleDate.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(OleDate other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is OleDate other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(OleDate left, OleDate right) => left.Equals(right);
  public static bool operator !=(OleDate left, OleDate right) => !left.Equals(right);
  public static bool operator <(OleDate left, OleDate right) => left.CompareTo(right) < 0;
  public static bool operator >(OleDate left, OleDate right) => left.CompareTo(right) > 0;
  public static bool operator <=(OleDate left, OleDate right) => left.CompareTo(right) <= 0;
  public static bool operator >=(OleDate left, OleDate right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(OleDate value) => value.ToDateTime();

}
