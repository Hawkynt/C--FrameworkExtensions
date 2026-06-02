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
/// Represents an unsigned 8-bit exponent-only scale (UE8M0): the value is 2^(<see cref="RawValue"/> - 127).
/// This is the per-block scale format used by MXFP4.
/// </summary>
/// <remarks>
/// Has 8 exponent bits and no mantissa, so it can only represent powers of two from 2^-127 to 2^127.
/// The raw value 0xFF represents NaN (per the OCP Microscaling specification). There is no signed zero
/// and no negative values.
/// </remarks>
public readonly struct E8M0 : IComparable, IComparable<E8M0>, IEquatable<E8M0>, IFormattable, ISpanFormattable {

  private const int Bias = 127;
  private const byte NaNBits = 0xFF;

  /// <summary>
  /// Gets the raw 8-bit representation (a biased power-of-two exponent).
  /// </summary>
  public byte RawValue { get; }

  private E8M0(byte raw) => this.RawValue = raw;

  /// <summary>
  /// Creates an E8M0 from its raw representation.
  /// </summary>
  public static E8M0 FromRaw(byte raw) => new(raw);

  /// <summary>Gets the value 1.0 (2^0).</summary>
  public static E8M0 One => new(Bias);

  /// <summary>Gets the smallest representable value (2^-127).</summary>
  public static E8M0 MinValue => new(0);

  /// <summary>Gets the largest finite representable value (2^127).</summary>
  public static E8M0 MaxValue => new(NaNBits - 1);

  /// <summary>Gets a NaN value.</summary>
  public static E8M0 NaN => new(NaNBits);

  /// <summary>Returns true if the value is NaN.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(E8M0 value) => value.RawValue == NaNBits;

  /// <summary>
  /// Gets the base-two exponent this scale represents (<see cref="RawValue"/> - 127).
  /// </summary>
  public int Exponent => this.RawValue - Bias;

  /// <summary>
  /// Converts this scale to a single-precision float (2^exponent), or NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ToSingle() => this.RawValue == NaNBits ? float.NaN : (float)Math.Pow(2.0, this.RawValue - Bias);

  /// <summary>
  /// Converts this scale to a double-precision float.
  /// </summary>
  public double ToDouble() => this.RawValue == NaNBits ? double.NaN : Math.Pow(2.0, this.RawValue - Bias);

  /// <summary>
  /// Creates an E8M0 from a base-two exponent, clamping to the representable range.
  /// </summary>
  public static E8M0 FromExponent(int exponent) {
    var raw = exponent + Bias;
    if (raw < 0)
      return MinValue;
    return raw > NaNBits - 1 ? MaxValue : new((byte)raw);
  }

  /// <summary>
  /// Creates the nearest E8M0 (power of two) to a positive value, clamping to the representable range.
  /// Non-positive input maps to <see cref="MinValue"/>; NaN maps to <see cref="NaN"/>.
  /// </summary>
  public static E8M0 FromSingle(float value) {
    if (float.IsNaN(value))
      return NaN;
    if (value <= 0)
      return MinValue;

    var exponent = (int)Math.Round(Math.Log(value, 2.0), MidpointRounding.ToEven);
    return FromExponent(exponent);
  }

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(E8M0 other) {
    if (IsNaN(this) || IsNaN(other))
      return IsNaN(this) && IsNaN(other) ? 0 : IsNaN(this) ? 1 : -1;
    return this.RawValue.CompareTo(other.RawValue);
  }

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not E8M0 other)
      throw new ArgumentException("Object must be of type E8M0.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(E8M0 other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is E8M0 other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

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

  public static bool operator ==(E8M0 left, E8M0 right) => left.Equals(right);
  public static bool operator !=(E8M0 left, E8M0 right) => !left.Equals(right);
  public static bool operator <(E8M0 left, E8M0 right) => left.CompareTo(right) < 0;
  public static bool operator >(E8M0 left, E8M0 right) => left.CompareTo(right) > 0;
  public static bool operator <=(E8M0 left, E8M0 right) => left.CompareTo(right) <= 0;
  public static bool operator >=(E8M0 left, E8M0 right) => left.CompareTo(right) >= 0;

  public static implicit operator float(E8M0 value) => value.ToSingle();
  public static implicit operator double(E8M0 value) => value.ToDouble();
}
