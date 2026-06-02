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
/// Represents a 4-bit floating-point value in E2M1 format (1 sign + 2 exponent + 1 mantissa bit, bias 1).
/// This is the element type shared by the MXFP4 and NVFP4 block-scaled formats.
/// </summary>
/// <remarks>
/// The code is stored in the low nibble of <see cref="RawValue"/>; the high nibble is always zero.
/// There are no infinities or NaNs: the sixteen codes encode ±{0, 0.5, 1, 1.5, 2, 3, 4, 6}.
/// On its own an E2M1 is unscaled; the MXFP4/NVFP4 containers apply per-block scales.
/// </remarks>
public readonly struct E2M1 : IComparable, IComparable<E2M1>, IEquatable<E2M1>, IFormattable, ISpanFormattable, IParsable<E2M1>, ISpanParsable<E2M1> {

  // Decoded magnitude for each of the 8 positive codes (index == code & 0x7).
  private static readonly float[] _magnitudes = [0f, 0.5f, 1f, 1.5f, 2f, 3f, 4f, 6f];

  private const byte SignMask = 0x8;
  private const byte CodeMask = 0xF;

  /// <summary>
  /// Gets the raw 4-bit representation (low nibble).
  /// </summary>
  public byte RawValue { get; }

  private E2M1(int raw) => this.RawValue = (byte)(raw & CodeMask);

  /// <summary>
  /// Creates an E2M1 from a raw nibble (only the low 4 bits are used).
  /// </summary>
  public static E2M1 FromRaw(byte raw) => new(raw);

  /// <summary>Gets positive zero.</summary>
  public static E2M1 Zero => new(0);

  /// <summary>Gets negative zero.</summary>
  public static E2M1 NegativeZero => new(SignMask);

  /// <summary>Gets the value 1.0.</summary>
  public static E2M1 One => new(2);

  /// <summary>Gets the smallest positive value (0.5).</summary>
  public static E2M1 Epsilon => new(1);

  /// <summary>Gets the maximum finite value (6.0).</summary>
  public static E2M1 MaxValue => new(7);

  /// <summary>Gets the minimum finite value (-6.0).</summary>
  public static E2M1 MinValue => new(0xF);

  // -0 and +0 compare/hash equal; every other code is a distinct value.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _Normalize(byte raw) => raw == SignMask ? (byte)0 : raw;

  /// <summary>Returns true if the value is negative (including -0).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(E2M1 value) => (value.RawValue & SignMask) != 0;

  /// <summary>
  /// Converts this E2M1 to a single-precision float.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ToSingle() {
    var magnitude = _magnitudes[this.RawValue & 0x7];
    return (this.RawValue & SignMask) != 0 ? -magnitude : magnitude;
  }

  /// <summary>
  /// Converts this E2M1 to a double-precision float.
  /// </summary>
  public double ToDouble() => this.ToSingle();

  /// <summary>
  /// Creates the nearest E2M1 to a single-precision float, rounding ties to even and saturating at ±6.
  /// NaN maps to zero (E2M1 has no NaN).
  /// </summary>
  public static E2M1 FromSingle(float value) {
    if (float.IsNaN(value))
      return Zero;

    var sign = value < 0 ? SignMask : 0;
    var a = Math.Abs(value);

    var best = 0;
    var bestDiff = Math.Abs(a - _magnitudes[0]);
    for (var c = 1; c < 8; ++c) {
      var diff = Math.Abs(a - _magnitudes[c]);
      if (diff < bestDiff) {
        best = c;
        bestDiff = diff;
      } else if (diff == bestDiff && (c & 1) == 0) // tie: prefer the even-mantissa code
        best = c;
    }

    return new(sign | best);
  }

  /// <summary>
  /// Creates the nearest E2M1 to a double-precision float.
  /// </summary>
  public static E2M1 FromDouble(double value) => FromSingle((float)value);

  // Comparison (treats -0 == +0; no NaN to consider)
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(E2M1 other) {
    float a = this.ToSingle(), b = other.ToSingle();
    return a == b ? 0 : a < b ? -1 : 1;
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not E2M1 other)
      throw new ArgumentException("Object must be of type E2M1.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(E2M1 other) => _Normalize(this.RawValue) == _Normalize(other.RawValue);

  public override bool Equals(object? obj) => obj is E2M1 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => _Normalize(this.RawValue);

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

  // Operators
  public static bool operator ==(E2M1 left, E2M1 right) => left.Equals(right);
  public static bool operator !=(E2M1 left, E2M1 right) => !left.Equals(right);
  public static bool operator <(E2M1 left, E2M1 right) => left.CompareTo(right) < 0;
  public static bool operator >(E2M1 left, E2M1 right) => left.CompareTo(right) > 0;
  public static bool operator <=(E2M1 left, E2M1 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(E2M1 left, E2M1 right) => left.CompareTo(right) >= 0;

  public static E2M1 operator +(E2M1 left, E2M1 right) => FromSingle(left.ToSingle() + right.ToSingle());
  public static E2M1 operator -(E2M1 left, E2M1 right) => FromSingle(left.ToSingle() - right.ToSingle());
  public static E2M1 operator *(E2M1 left, E2M1 right) => FromSingle(left.ToSingle() * right.ToSingle());
  public static E2M1 operator /(E2M1 left, E2M1 right) => FromSingle(left.ToSingle() / right.ToSingle());
  public static E2M1 operator -(E2M1 value) => new(value.RawValue ^ SignMask);
  public static E2M1 operator +(E2M1 value) => value;

  // Conversions
  public static explicit operator E2M1(float value) => FromSingle(value);
  public static explicit operator E2M1(double value) => FromDouble(value);
  public static implicit operator float(E2M1 value) => value.ToSingle();
  public static implicit operator double(E2M1 value) => value.ToDouble();

  // Math helpers
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E2M1 Abs(E2M1 value) => new(value.RawValue & 0x7);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E2M1 Min(E2M1 left, E2M1 right) => left.ToSingle() <= right.ToSingle() ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static E2M1 Max(E2M1 left, E2M1 right) => left.ToSingle() >= right.ToSingle() ? left : right;

  // Parsing
  public static E2M1 Parse(string s) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);
  public static E2M1 Parse(string s, IFormatProvider? provider) => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static E2M1 Parse(string s, NumberStyles style, IFormatProvider? provider) => FromSingle(float.Parse(s, style, provider));

  public static bool TryParse(string? s, out E2M1 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);
  public static bool TryParse(string? s, IFormatProvider? provider, out E2M1 result) => TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out E2M1 result) {
    if (float.TryParse(s, style, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }

    result = Zero;
    return false;
  }

  public static E2M1 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => FromSingle(float.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider));

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out E2M1 result) {
    if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromSingle(value);
      return true;
    }

    result = Zero;
    return false;
  }
}

/// <summary>
/// A 4-bit <see cref="IBitCodec{T}"/> mapping E2M1 codes to/from <see cref="float"/>, for use with
/// <see cref="System.Collections.PackedBuffer{T,TCodec,TBitOrder}"/>.
/// </summary>
public readonly struct E2M1Codec : IBitCodec<float> {
  /// <inheritdoc />
  public int BitWidth => 4;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Decode(ulong code) => E2M1.FromRaw((byte)code).ToSingle();

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ulong Encode(float value) => E2M1.FromSingle(value).RawValue;
}
