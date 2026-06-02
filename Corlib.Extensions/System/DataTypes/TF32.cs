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
/// Represents NVIDIA's TensorFloat-32 (TF32): 1 sign + 8 exponent + 10 mantissa bits, the same dynamic range
/// as <see cref="float"/> with the reduced precision of <see cref="Half"/>.
/// </summary>
/// <remarks>
/// Although only 19 bits are significant, TF32 values are carried in a 32-bit slot whose low 13 mantissa bits
/// are always zero (matching how tensor cores store the format). <see cref="RawValue"/> therefore equals the
/// IEEE-754 single-precision bit pattern with its low 13 bits cleared.
/// </remarks>
public readonly struct TF32 : IComparable, IComparable<TF32>, IEquatable<TF32>, IFormattable, ISpanFormattable, IParsable<TF32>, ISpanParsable<TF32> {

  private const uint MantissaDropBits = 13;
  private const uint UsedMask = 0xFFFFE000; // top 19 bits
  private const uint ExponentMask = 0x7F800000;
  private const uint QuietNaN = 0x7FC00000;

  /// <summary>
  /// Gets the raw bit pattern (a single-precision layout with the low 13 mantissa bits zeroed).
  /// </summary>
  public uint RawValue { get; }

  private TF32(uint raw) => this.RawValue = raw & UsedMask;

  /// <summary>
  /// Creates a TF32 from a raw bit pattern (low 13 bits are ignored).
  /// </summary>
  public static TF32 FromRaw(uint raw) => new(raw);

  /// <summary>Gets positive zero.</summary>
  public static TF32 Zero => new(0);

  /// <summary>Gets the value 1.0.</summary>
  public static TF32 One => new(0x3F800000);

  /// <summary>Gets positive infinity.</summary>
  public static TF32 PositiveInfinity => new(ExponentMask);

  /// <summary>Gets negative infinity.</summary>
  public static TF32 NegativeInfinity => new(0xFF800000);

  /// <summary>Gets a quiet NaN.</summary>
  public static TF32 NaN => new(QuietNaN);

  /// <summary>Gets the largest finite value.</summary>
  public static TF32 MaxValue => new(0x7F7FE000);

  /// <summary>Gets the smallest (most negative) finite value.</summary>
  public static TF32 MinValue => new(0xFF7FE000);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(TF32 value) => (value.RawValue & ExponentMask) == ExponentMask && (value.RawValue & 0x007FE000) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(TF32 value) => (value.RawValue & 0x7FFFE000) == ExponentMask;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(TF32 value) => (value.RawValue & ExponentMask) != ExponentMask;

  /// <summary>
  /// Converts this TF32 to a single-precision float (exact: TF32 is a subset of float).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ToSingle() => BitConverter.Int32BitsToSingle((int)this.RawValue);

  /// <summary>
  /// Converts this TF32 to a double-precision float.
  /// </summary>
  public double ToDouble() => this.ToSingle();

  /// <summary>
  /// Creates the nearest TF32 to a single-precision float, rounding the mantissa to 10 bits (ties to even).
  /// </summary>
  public static TF32 FromSingle(float value) {
    if (float.IsNaN(value))
      return NaN;

    var bits = unchecked((uint)BitConverter.SingleToInt32Bits(value));

    // round-to-nearest-even when dropping the low 13 mantissa bits
    var lsb = (bits >> (int)MantissaDropBits) & 1u;
    var rounded = bits + 0x0FFFu + lsb;
    return new(rounded);
  }

  /// <summary>
  /// Creates the nearest TF32 to a double-precision float.
  /// </summary>
  public static TF32 FromDouble(double value) => FromSingle((float)value);

  public int CompareTo(TF32 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.ToSingle().CompareTo(other.ToSingle());
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not TF32 other)
      throw new ArgumentException("Object must be of type TF32.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(TF32 other) => this.RawValue == other.RawValue || (IsNaN(this) && IsNaN(other));

  public override bool Equals(object? obj) => obj is TF32 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => IsNaN(this) ? QuietNaN.GetHashCode() : this.RawValue.GetHashCode();

  public override string ToString() => this.ToSingle().ToString(CultureInfo.InvariantCulture);
  public string ToString(IFormatProvider? provider) => this.ToSingle().ToString(provider);
  public string ToString(string? format) => this.ToSingle().ToString(format, CultureInfo.InvariantCulture);
  public string ToString(string? format, IFormatProvider? provider) => this.ToSingle().ToString(format, provider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.ToSingle().ToString(provider)
      : this.ToSingle().ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }

    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  public static bool operator ==(TF32 left, TF32 right) => left.Equals(right);
  public static bool operator !=(TF32 left, TF32 right) => !left.Equals(right);
  public static bool operator <(TF32 left, TF32 right) => left.CompareTo(right) < 0;
  public static bool operator >(TF32 left, TF32 right) => left.CompareTo(right) > 0;
  public static bool operator <=(TF32 left, TF32 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(TF32 left, TF32 right) => left.CompareTo(right) >= 0;

  public static TF32 operator +(TF32 left, TF32 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static TF32 operator -(TF32 left, TF32 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static TF32 operator *(TF32 left, TF32 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static TF32 operator /(TF32 left, TF32 right) => FromSingle(left.ToSingle() / right.ToSingle());
  public static TF32 operator -(TF32 value) => new(value.RawValue ^ 0x80000000);
  public static TF32 operator +(TF32 value) => value;

  public static explicit operator TF32(float value) => FromSingle(value);
  public static explicit operator TF32(double value) => FromDouble(value);
  public static implicit operator float(TF32 value) => value.ToSingle();
  public static implicit operator double(TF32 value) => value.ToDouble();

  // Parsing
  public static TF32 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);
  public static TF32 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
  public static TF32 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromSingle(float.Parse(s, style, provider));

  public static bool TryParse(string? s, out TF32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);
  public static bool TryParse(string? s, IFormatProvider? provider, out TF32 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out TF32 result) {
    if (float.TryParse(s, style, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }

    result = Zero;
    return false;
  }

  public static TF32 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromSingle(float.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out TF32 result) {
    if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }

    result = Zero;
    return false;
  }
}
