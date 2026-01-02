#region (c)2010-2042 Hawkynt

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

#endregion

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// An unsigned normalized 32-bit type representing values in the range [0.0, 1.0].
/// </summary>
/// <remarks>
/// <para>Backed by <see cref="uint"/> where 0 = 0.0 and <see cref="uint.MaxValue"/> = 1.0.</para>
/// <para>Provides 4 billion discrete values (65,536× more precision than 16.16 fixed-point).</para>
/// <para>GPU-style naming convention: UNorm = Unsigned Normalized.</para>
/// <para>Use this type to distinguish bounded [0,1] values from unbounded floats at the type level.</para>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 4)]
public readonly struct UNorm32 : IEquatable<UNorm32>, IComparable<UNorm32> {

  private readonly uint _value;

  #region Constants

  /// <summary>The value 0.0 (minimum).</summary>
  public static readonly UNorm32 Zero = new(0u);

  /// <summary>The value 1.0 (maximum).</summary>
  public static readonly UNorm32 One = new(uint.MaxValue);

  /// <summary>The value 0.5 (midpoint).</summary>
  public static readonly UNorm32 Half = new(0x80000000u);

  private const float ToFloatMultiplier = 1f / uint.MaxValue;
  private const double FromFloatMultiplier = uint.MaxValue;

  /// <summary>
  /// The maximum raw value (uint.MaxValue). Useful for integer division in consumers.
  /// </summary>
  public const uint MaxValue = uint.MaxValue;

  #endregion

  #region Constructors

  /// <summary>
  /// Creates a UNorm32 from a raw <see cref="uint"/> value.
  /// </summary>
  /// <param name="rawValue">The raw backing value where 0 = 0.0 and uint.MaxValue = 1.0.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private UNorm32(uint rawValue) => this._value = rawValue;

  #endregion

  #region Raw Access

  /// <summary>
  /// Gets the raw <see cref="uint"/> backing value.
  /// </summary>
  public uint RawValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._value;
  }

  /// <summary>
  /// Creates a UNorm32 from a raw <see cref="uint"/> value without validation.
  /// </summary>
  /// <param name="rawValue">The raw backing value where 0 = 0.0 and uint.MaxValue = 1.0.</param>
  /// <returns>A new UNorm32 instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 FromRaw(uint rawValue) => new(rawValue);

  #endregion

  #region Float Conversion

  /// <summary>
  /// Converts to <see cref="float"/>.
  /// </summary>
  /// <returns>The equivalent float in [0.0, 1.0].</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float ToFloat() => this._value * ToFloatMultiplier;

  /// <summary>
  /// Creates from <see cref="float"/> without bounds checking.
  /// </summary>
  /// <param name="value">A float assumed to be in [0.0, 1.0].</param>
  /// <returns>A new UNorm32 instance.</returns>
  /// <remarks>For internal use where the value is known to be in range.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static UNorm32 FromFloatUnchecked(float value) => new((uint)(value * FromFloatMultiplier));

  /// <summary>
  /// Creates from <see cref="float"/> with validation.
  /// </summary>
  /// <param name="value">A float value in [0.0, 1.0].</param>
  /// <returns>A new UNorm32 instance.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside [0.0, 1.0].</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 FromFloat(float value) {
    if (value < 0f || value > 1f)
      throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be in range [0.0, 1.0]");
    return new((uint)(value * FromFloatMultiplier));
  }

  /// <summary>
  /// Creates from <see cref="float"/> with bounds clamping.
  /// </summary>
  /// <param name="value">A float value (clamped to [0.0, 1.0]).</param>
  /// <returns>A new UNorm32 instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 FromFloatClamped(float value) {
    if (value <= 0f)
      return Zero;
    if (value >= 1f)
      return One;
    return new((uint)(value * FromFloatMultiplier));
  }

  #endregion

  #region Byte Conversion

  /// <summary>
  /// Creates from a <see cref="byte"/> (0-255 mapped to full uint range).
  /// </summary>
  /// <param name="value">A byte value where 0 = 0.0 and 255 = 1.0.</param>
  /// <returns>A new UNorm32 instance.</returns>
  /// <remarks>Uses 0x01010101 multiplier to spread byte evenly across uint range.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 FromByte(byte value) => new((uint)value * 0x01010101u);

  /// <summary>
  /// Converts to <see cref="byte"/> (0-255).
  /// </summary>
  /// <returns>The byte representation (upper 8 bits).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte ToByte() => (byte)(this._value >> 24);

  #endregion

  #region 16-bit Conversion

  /// <summary>
  /// Creates from a 16-bit unsigned value (0-65535 mapped to full uint range).
  /// </summary>
  /// <param name="value">A ushort value where 0 = 0.0 and 65535 = 1.0.</param>
  /// <returns>A new UNorm32 instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 FromUInt16(ushort value) => new((uint)value * 0x00010001u);

  /// <summary>
  /// Converts to 16-bit unsigned value (0-65535).
  /// </summary>
  /// <returns>The ushort representation (upper 16 bits).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ushort ToUInt16() => (ushort)(this._value >> 16);

  #endregion

  #region Fixed-Point Compatibility

  /// <summary>
  /// Creates from legacy 16.16 fixed-point format.
  /// </summary>
  /// <param name="fixed16">A 16.16 fixed-point value where 65536 = 1.0.</param>
  /// <returns>A new UNorm32 instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 FromFixed16(int fixed16) => new((uint)fixed16 << 16);

  /// <summary>
  /// Converts to legacy 16.16 fixed-point format.
  /// </summary>
  /// <returns>A 16.16 fixed-point value where 65536 = 1.0.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int ToFixed16() => (int)(this._value >> 16);

  #endregion

  #region Arithmetic

  /// <summary>
  /// Multiplies two UNorm32 values with proper rounding.
  /// </summary>
  /// <param name="a">First operand.</param>
  /// <param name="b">Second operand.</param>
  /// <returns>Product, where 1.0 × 1.0 = 1.0.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 Mul(UNorm32 a, UNorm32 b) {
    var product = (ulong)a._value * b._value;
    return new((uint)((product + 0x80000000u) >> 32));
  }

  /// <summary>
  /// Linearly interpolates between two values.
  /// </summary>
  /// <param name="a">Start value (returned when t = 0).</param>
  /// <param name="b">End value (returned when t = 1).</param>
  /// <param name="t">Interpolation factor in [0, 1].</param>
  /// <returns>Interpolated value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 Lerp(UNorm32 a, UNorm32 b, UNorm32 t) {
    var diff = (long)b._value - a._value;
    var scaled = (diff * t._value + 0x80000000L) >> 32;
    return new((uint)(a._value + scaled));
  }

  /// <summary>
  /// Computes the exact midpoint of two values without overflow.
  /// </summary>
  /// <param name="a">First value.</param>
  /// <param name="b">Second value.</param>
  /// <returns>The midpoint (a + b) / 2.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 Midpoint(UNorm32 a, UNorm32 b)
    => new((a._value >> 1) + (b._value >> 1) + ((a._value | b._value) & 1));

  /// <summary>
  /// Adds two UNorm32 values, saturating at 1.0.
  /// </summary>
  /// <param name="a">First operand.</param>
  /// <param name="b">Second operand.</param>
  /// <returns>Sum clamped to [0, 1].</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 AddSaturate(UNorm32 a, UNorm32 b) {
    var sum = (ulong)a._value + b._value;
    return sum > uint.MaxValue ? One : new((uint)sum);
  }

  /// <summary>
  /// Subtracts two UNorm32 values, saturating at 0.0.
  /// </summary>
  /// <param name="a">First operand.</param>
  /// <param name="b">Second operand.</param>
  /// <returns>Difference clamped to [0, 1].</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 SubSaturate(UNorm32 a, UNorm32 b)
    => a._value > b._value ? new(a._value - b._value) : Zero;

  /// <summary>
  /// Returns the absolute difference between two values.
  /// </summary>
  /// <param name="a">First value.</param>
  /// <param name="b">Second value.</param>
  /// <returns>|a - b|</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 AbsDiff(UNorm32 a, UNorm32 b)
    => new(a._value > b._value ? a._value - b._value : b._value - a._value);

  /// <summary>
  /// Returns the minimum of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 Min(UNorm32 a, UNorm32 b) => new(a._value < b._value ? a._value : b._value);

  /// <summary>
  /// Returns the maximum of two values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 Max(UNorm32 a, UNorm32 b) => new(a._value > b._value ? a._value : b._value);

  /// <summary>
  /// Clamps a value between minimum and maximum bounds.
  /// </summary>
  /// <param name="value">The value to clamp.</param>
  /// <param name="min">The minimum bound.</param>
  /// <param name="max">The maximum bound.</param>
  /// <returns>The clamped value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 Clamp(UNorm32 value, UNorm32 min, UNorm32 max)
    => new(value._value < min._value ? min._value : value._value > max._value ? max._value : value._value);

  /// <summary>
  /// Inverts the value (1.0 - x).
  /// </summary>
  /// <returns>The inverted value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Invert() => new(uint.MaxValue - this._value);

  /// <summary>
  /// Adds two UNorm32 values (saturating at 1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 operator +(UNorm32 a, UNorm32 b) {
    var sum = (ulong)a._value + b._value;
    return sum > uint.MaxValue ? One : new((uint)sum);
  }

  /// <summary>
  /// Multiplies a UNorm32 by an integer (saturating at 1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 operator *(UNorm32 a, int b) {
    if (b <= 0)
      return Zero;
    var product = (ulong)a._value * (uint)b;
    return product > uint.MaxValue ? One : new((uint)product);
  }

  /// <summary>
  /// Multiplies an integer by a UNorm32 (saturating at 1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 operator *(int a, UNorm32 b) => b * a;

  #endregion

  #region Comparison

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(UNorm32 other) => this._value == other._value;

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is UNorm32 other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() => (int)this._value;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(UNorm32 other) => this._value.CompareTo(other._value);

  public static bool operator ==(UNorm32 left, UNorm32 right) => left._value == right._value;
  public static bool operator !=(UNorm32 left, UNorm32 right) => left._value != right._value;
  public static bool operator <(UNorm32 left, UNorm32 right) => left._value < right._value;
  public static bool operator >(UNorm32 left, UNorm32 right) => left._value > right._value;
  public static bool operator <=(UNorm32 left, UNorm32 right) => left._value <= right._value;
  public static bool operator >=(UNorm32 left, UNorm32 right) => left._value >= right._value;

  // Comparison with float (converts float to UNorm32 for comparison, clamping out-of-range values)
  public static bool operator <(UNorm32 left, float right) => left._value < FromFloatClamped(right)._value;
  public static bool operator >(UNorm32 left, float right) => left._value > FromFloatClamped(right)._value;
  public static bool operator <=(UNorm32 left, float right) => left._value <= FromFloatClamped(right)._value;
  public static bool operator >=(UNorm32 left, float right) => left._value >= FromFloatClamped(right)._value;
  public static bool operator <(float left, UNorm32 right) => FromFloatClamped(left)._value < right._value;
  public static bool operator >(float left, UNorm32 right) => FromFloatClamped(left)._value > right._value;
  public static bool operator <=(float left, UNorm32 right) => FromFloatClamped(left)._value <= right._value;
  public static bool operator >=(float left, UNorm32 right) => FromFloatClamped(left)._value >= right._value;

  /// <summary>
  /// Explicit conversion to float for arithmetic operations.
  /// </summary>
  /// <remarks>
  /// Explicit to prevent accidental precision loss. Use .RawValue for integer math.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator float(UNorm32 u) => u._value * ToFloatMultiplier;

  #endregion

  #region String Conversion

  /// <inheritdoc />
  public override string ToString() => $"{this.ToFloat():F6}";

  /// <summary>
  /// Returns a string representation with the raw uint value.
  /// </summary>
  /// <returns>A string in the format "value (raw: rawValue)".</returns>
  public string ToDebugString() => $"{this.ToFloat():F6} (raw: 0x{this._value:X8})";

  #endregion
}
