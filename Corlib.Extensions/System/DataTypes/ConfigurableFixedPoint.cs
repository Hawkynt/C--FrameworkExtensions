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
using System.Numerics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents an instance-configurable fixed-point number with customizable integer and fractional parts.
/// The storage type determines signedness: signed types (sbyte, short, int, long) can represent negative values,
/// unsigned types (byte, ushort, uint, ulong) use saturating arithmetic for subtraction.
/// Each instance carries its own bit layout configuration.
/// Cross-config arithmetic uses the left operand's config for the result.
/// </summary>
/// <typeparam name="TStorage">The underlying storage type (byte, sbyte, ushort, short, uint, int, ulong, long, UInt96, Int96, UInt128, Int128).</typeparam>
public readonly struct ConfigurableFixedPoint<TStorage> : IComparable, IComparable<ConfigurableFixedPoint<TStorage>>, IEquatable<ConfigurableFixedPoint<TStorage>>, IFormattable, ISpanFormattable, IParsable<ConfigurableFixedPoint<TStorage>>, ISpanParsable<ConfigurableFixedPoint<TStorage>>
  where TStorage : struct, IComparable, IEquatable<TStorage> {

  #region Static Configuration (type-level, depends only on TStorage)

  /// <summary>
  /// Gets the total number of bits in the storage type.
  /// </summary>
  public static readonly int TotalBits = _GetStorageTypeBits(typeof(TStorage));

  /// <summary>
  /// Gets whether this type has a sign bit (determined by storage type signedness).
  /// </summary>
  public static readonly bool HasSign = _IsSignedType(typeof(TStorage));

  private static int _GetStorageTypeBits(Type type) {
    if (type == typeof(byte) || type == typeof(sbyte)) return 8;
    if (type == typeof(ushort) || type == typeof(short)) return 16;
    if (type == typeof(uint) || type == typeof(int)) return 32;
    if (type == typeof(ulong) || type == typeof(long)) return 64;
    if (type == typeof(UInt96) || type == typeof(Int96)) return 96;
    if (type == typeof(UInt128) || type == typeof(Int128)) return 128;
    throw new NotSupportedException($"Storage type {type.Name} is not supported.");
  }

  private static bool _IsSignedType(Type type) =>
    type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(Int96) || type == typeof(Int128);

  private static int _GetDefaultFractionalBits(int totalBits) => totalBits / 2;

  private static readonly byte _defaultFractionalBits = (byte)_GetDefaultFractionalBits(TotalBits);

  #endregion

  #region Instance Data

  private readonly TStorage _rawValue;
  private readonly byte _fractionalBits;

  #endregion

  #region Computed Instance Properties (derived from _fractionalBits)

  private byte _EffectiveFractionalBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._fractionalBits != 0 ? this._fractionalBits : _defaultFractionalBits;
  }

  /// <summary>
  /// Gets the number of fractional bits.
  /// </summary>
  public int FractionalBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._EffectiveFractionalBits;
  }

  /// <summary>
  /// Gets the number of integer bits (excluding sign for signed types).
  /// </summary>
  public int IntegerBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => TotalBits - this._EffectiveFractionalBits - (HasSign ? 1 : 0);
  }

  /// <summary>
  /// Gets the scale factor (2^FractionalBits).
  /// </summary>
  public BigInteger Scale {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => BigInteger.One << this._EffectiveFractionalBits;
  }

  private BigInteger _MaxRawValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => HasSign ? (BigInteger.One << (TotalBits - 1)) - 1 : (BigInteger.One << TotalBits) - 1;
  }

  private BigInteger _MinRawValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => HasSign ? -(BigInteger.One << (TotalBits - 1)) : BigInteger.Zero;
  }

  #endregion

  #region Constructors

  /// <summary>
  /// Creates a zero-valued instance with the specified fractional bit count.
  /// </summary>
  /// <param name="fractionalBits">Number of bits for the fractional part.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if fractional bits are invalid for the storage type.</exception>
  public ConfigurableFixedPoint(int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    this._rawValue = default;
    this._fractionalBits = (byte)fractionalBits;
  }

  private ConfigurableFixedPoint(TStorage raw, byte fractionalBits) {
    this._rawValue = raw;
    this._fractionalBits = fractionalBits;
  }

  private ConfigurableFixedPoint(BigInteger raw, byte fractionalBits) {
    this._fractionalBits = fractionalBits;
    var maxRaw = HasSign ? (BigInteger.One << (TotalBits - 1)) - 1 : (BigInteger.One << TotalBits) - 1;
    var minRaw = HasSign ? -(BigInteger.One << (TotalBits - 1)) : BigInteger.Zero;
    if (HasSign) {
      if (raw > maxRaw)
        raw = maxRaw;
      else if (raw < minRaw)
        raw = minRaw;
    } else {
      if (raw < 0)
        raw = BigInteger.Zero;
      else if (raw > maxRaw)
        raw = maxRaw;
    }
    this._rawValue = _ConvertFromBigInteger(raw);
  }

  #endregion

  #region Properties

  /// <summary>
  /// Gets the raw fixed-point representation as the underlying storage type.
  /// </summary>
  public TStorage RawValue => this._rawValue;

  /// <summary>
  /// Gets the raw fixed-point representation as a BigInteger for extended precision operations.
  /// </summary>
  public BigInteger RawBits => _ConvertToBigInteger(this._rawValue);

  #endregion

  #region Decompose / Recompose (BigInteger exact representation)

  /// <summary>
  /// Returns exact (signedMantissa, powerOfTwo) such that value = signedMantissa * 2^powerOfTwo.
  /// For fixed-point: value = rawBits * 2^(-fractionalBits).
  /// </summary>
  internal (BigInteger signedMantissa, int powerOfTwo) DecomposeExact() =>
    (this.RawBits, -(int)this._EffectiveFractionalBits);

  /// <summary>
  /// Recomposes a signedMantissa * 2^powerOfTwo into fixed-point format with the specified fractional bits.
  /// Rounds to nearest (half-to-even) and clamps to storage range.
  /// </summary>
  internal static ConfigurableFixedPoint<TStorage> RecomposeFixed(BigInteger signedMantissa, int powerOfTwo, byte targetFractionalBits) {
    if (signedMantissa == 0)
      return new(BigInteger.Zero, targetFractionalBits);

    var targetShift = powerOfTwo + targetFractionalBits;
    BigInteger raw;
    if (targetShift >= 0)
      raw = signedMantissa << targetShift;
    else {
      var absShift = -targetShift;
      var absMant = BigInteger.Abs(signedMantissa);
      var rounded = _RoundRightShift(absMant, absShift);
      raw = signedMantissa < 0 ? -rounded : rounded;
    }

    if (!HasSign && raw < 0)
      raw = BigInteger.Zero;

    return new(raw, targetFractionalBits);
  }

  /// <summary>
  /// Rescales raw bits from one fractional precision to another with rounding.
  /// </summary>
  private static BigInteger _RescaleRawBits(BigInteger rawBits, int from, int to) {
    var shift = to - from;
    if (shift == 0)
      return rawBits;
    if (shift > 0)
      return rawBits << shift;

    var abs = BigInteger.Abs(rawBits);
    var rounded = _RoundRightShift(abs, -shift);
    return rawBits >= 0 ? rounded : -rounded;
  }

  /// <summary>
  /// Round-to-nearest-even right shift by the given number of bits.
  /// </summary>
  private static BigInteger _RoundRightShift(BigInteger value, int shift) {
    if (shift <= 0)
      return value << (-shift);
    var half = BigInteger.One << (shift - 1);
    var truncated = value >> shift;
    var remainder = value & ((BigInteger.One << shift) - 1);
    if (remainder > half || (remainder == half && !truncated.IsEven))
      ++truncated;
    return truncated;
  }

  #endregion

  #region Special Value Factories

  /// <summary>Creates a zero value with the specified fractional bit count.</summary>
  public static ConfigurableFixedPoint<TStorage> Zero(int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    return new(BigInteger.Zero, (byte)fractionalBits);
  }

  /// <summary>Creates a one value with the specified fractional bit count.</summary>
  public static ConfigurableFixedPoint<TStorage> One(int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    return new(BigInteger.One << fractionalBits, (byte)fractionalBits);
  }

  /// <summary>Creates the smallest positive value (1/Scale) with the specified fractional bit count.</summary>
  public static ConfigurableFixedPoint<TStorage> Epsilon(int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    return new(BigInteger.One, (byte)fractionalBits);
  }

  /// <summary>Creates the maximum value with the specified fractional bit count.</summary>
  public static ConfigurableFixedPoint<TStorage> MaxValue(int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    var maxRaw = HasSign ? (BigInteger.One << (TotalBits - 1)) - 1 : (BigInteger.One << TotalBits) - 1;
    return new(maxRaw, (byte)fractionalBits);
  }

  /// <summary>Creates the minimum value with the specified fractional bit count.</summary>
  public static ConfigurableFixedPoint<TStorage> MinValue(int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    var minRaw = HasSign ? -(BigInteger.One << (TotalBits - 1)) : BigInteger.Zero;
    return new(minRaw, (byte)fractionalBits);
  }

  #endregion

  #region Instance Convenience Properties for Special Values

  /// <summary>Gets a zero value with the same configuration.</summary>
  public ConfigurableFixedPoint<TStorage> AsZero => new(BigInteger.Zero, this._EffectiveFractionalBits);

  /// <summary>Gets a one value with the same configuration.</summary>
  public ConfigurableFixedPoint<TStorage> AsOne => new(this.Scale, this._EffectiveFractionalBits);

  /// <summary>Gets an epsilon value with the same configuration.</summary>
  public ConfigurableFixedPoint<TStorage> AsEpsilon => new(BigInteger.One, this._EffectiveFractionalBits);

  /// <summary>Gets the maximum value with the same configuration.</summary>
  public ConfigurableFixedPoint<TStorage> AsMaxValue => new(this._MaxRawValue, this._EffectiveFractionalBits);

  /// <summary>Gets the minimum value with the same configuration.</summary>
  public ConfigurableFixedPoint<TStorage> AsMinValue => new(this._MinRawValue, this._EffectiveFractionalBits);

  #endregion

  #region Factory Methods

  /// <summary>Creates a value from the raw fixed-point representation with the specified configuration.</summary>
  public static ConfigurableFixedPoint<TStorage> FromRaw(TStorage raw, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    return new(_ConvertToBigInteger(raw), (byte)fractionalBits);
  }

  /// <summary>Creates a value from a double-precision floating-point number with the specified configuration.</summary>
  public static ConfigurableFixedPoint<TStorage> FromDouble(double value, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    if (double.IsNaN(value) || double.IsInfinity(value))
      throw new ArgumentException("Value cannot be NaN or Infinity.", nameof(value));

    if (!HasSign && value < 0)
      return new(BigInteger.Zero, (byte)fractionalBits);

    var scale = BigInteger.One << fractionalBits;
    var scaledValue = value * (double)scale;
    var raw = (BigInteger)Math.Round(scaledValue);
    return new(raw, (byte)fractionalBits);
  }

  /// <summary>Creates a value from a double using this instance's configuration.</summary>
  public ConfigurableFixedPoint<TStorage> CreateFromDouble(double value) =>
    FromDouble(value, this.FractionalBits);

  /// <summary>Creates a value from a single-precision floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromSingle(float value, int fractionalBits) => FromDouble(value, fractionalBits);

  /// <summary>Creates a value from a decimal number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromDecimal(decimal value, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    if (!HasSign && value < 0)
      return new(BigInteger.Zero, (byte)fractionalBits);

    var scale = BigInteger.One << fractionalBits;
    var scaledValue = value * (decimal)scale;
    var raw = (BigInteger)Math.Round(scaledValue);
    return new(raw, (byte)fractionalBits);
  }

  /// <summary>Creates a value from a Half-precision floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromHalf(Half value, int fractionalBits) => FromDouble((double)value, fractionalBits);

  /// <summary>Creates a value from a Quarter-precision floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromQuarter(Quarter value, int fractionalBits) => FromSingle(value.ToSingle(), fractionalBits);

  /// <summary>Creates a value from an E4M3 floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromE4M3(E4M3 value, int fractionalBits) => FromSingle(value.ToSingle(), fractionalBits);

  /// <summary>Creates a value from a BFloat8 floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromBFloat8(BFloat8 value, int fractionalBits) => FromSingle(value.ToSingle(), fractionalBits);

  /// <summary>Creates a value from a BFloat16 floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromBFloat16(BFloat16 value, int fractionalBits) => FromSingle(value.ToSingle(), fractionalBits);

  /// <summary>Creates a value from a BFloat32 floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromBFloat32(BFloat32 value, int fractionalBits) => FromDouble(value.ToDouble(), fractionalBits);

  /// <summary>Creates a value from a BFloat64 floating-point number.</summary>
  public static ConfigurableFixedPoint<TStorage> FromBFloat64(BFloat64 value, int fractionalBits) => FromDouble(value.ToDouble(), fractionalBits);

  /// <summary>Creates a value from an integer.</summary>
  public static ConfigurableFixedPoint<TStorage> FromInt32(int value, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    if (!HasSign && value < 0)
      return new(BigInteger.Zero, (byte)fractionalBits);
    return new((BigInteger)value << fractionalBits, (byte)fractionalBits);
  }

  /// <summary>Creates a value from a long integer.</summary>
  public static ConfigurableFixedPoint<TStorage> FromInt64(long value, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    if (!HasSign && value < 0)
      return new(BigInteger.Zero, (byte)fractionalBits);
    return new((BigInteger)value << fractionalBits, (byte)fractionalBits);
  }

  /// <summary>Creates a value from an unsigned integer.</summary>
  public static ConfigurableFixedPoint<TStorage> FromUInt32(uint value, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    return new((BigInteger)value << fractionalBits, (byte)fractionalBits);
  }

  /// <summary>Creates a value from an unsigned long integer.</summary>
  public static ConfigurableFixedPoint<TStorage> FromUInt64(ulong value, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    return new((BigInteger)value << fractionalBits, (byte)fractionalBits);
  }

  /// <summary>Creates a value from a BigInteger.</summary>
  public static ConfigurableFixedPoint<TStorage> FromBigInteger(BigInteger value, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    if (!HasSign && value < 0)
      return new(BigInteger.Zero, (byte)fractionalBits);
    return new(value << fractionalBits, (byte)fractionalBits);
  }

  #endregion

  #region Conversion To Other Types

  /// <summary>Converts this value to a double-precision floating-point number.</summary>
  public double ToDouble() => (double)this.RawBits / (double)this.Scale;

  /// <summary>Converts this value to a single-precision floating-point number.</summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>Converts this value to a decimal number.</summary>
  public decimal ToDecimal() => (decimal)this.RawBits / (decimal)this.Scale;

  /// <summary>Converts this value to a Half-precision floating-point number.</summary>
  public Half ToHalf() => (Half)this.ToDouble();

  /// <summary>Converts this value to a Quarter-precision floating-point number.</summary>
  public Quarter ToQuarter() => Quarter.FromSingle(this.ToSingle());

  /// <summary>Converts this value to an E4M3 floating-point number.</summary>
  public E4M3 ToE4M3() => E4M3.FromSingle(this.ToSingle());

  /// <summary>Converts this value to a BFloat8 floating-point number.</summary>
  public BFloat8 ToBFloat8() => BFloat8.FromSingle(this.ToSingle());

  /// <summary>Converts this value to a BFloat16 floating-point number.</summary>
  public BFloat16 ToBFloat16() => BFloat16.FromSingle(this.ToSingle());

  /// <summary>Converts this value to a BFloat32 floating-point number.</summary>
  public BFloat32 ToBFloat32() => BFloat32.FromDouble(this.ToDouble());

  /// <summary>Converts this value to a BFloat64 floating-point number.</summary>
  public BFloat64 ToBFloat64() => BFloat64.FromDouble(this.ToDouble());

  /// <summary>Gets the integer part of this value.</summary>
  public BigInteger ToIntegerPart() => this.RawBits >> this.FractionalBits;

  /// <summary>Converts the integer part to an int.</summary>
  public int ToInt32() => (int)(this.RawBits >> this.FractionalBits);

  /// <summary>Converts the integer part to a long.</summary>
  public long ToInt64() => (long)(this.RawBits >> this.FractionalBits);

  /// <summary>Converts the integer part to a uint.</summary>
  public uint ToUInt32() => (uint)(this.RawBits >> this.FractionalBits);

  /// <summary>Converts the integer part to a ulong.</summary>
  public ulong ToUInt64() => (ulong)(this.RawBits >> this.FractionalBits);

  /// <summary>
  /// Converts this fixed-point value to a floating-point representation with the specified mantissa bits.
  /// </summary>
  public ConfigurableFloatingPoint<TStorage> ToFloatingPoint(int mantissaBits) {
    if (this.RawBits == 0)
      return ConfigurableFloatingPoint<TStorage>.Zero(mantissaBits);

    var (sm, p2) = this.DecomposeExact();
    return ConfigurableFloatingPoint<TStorage>.FromDouble(this.ToDouble(), mantissaBits);
  }

  /// <summary>
  /// Converts this value to another fixed-point configuration.
  /// </summary>
  public ConfigurableFixedPoint<TStorage> ConvertTo(int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    if (this._EffectiveFractionalBits == fractionalBits)
      return this;

    var rescaled = _RescaleRawBits(this.RawBits, this._EffectiveFractionalBits, fractionalBits);
    return new(rescaled, (byte)fractionalBits);
  }

  #endregion

  #region FromMemory / ToMemory

  /// <summary>Creates a value from a byte span with the specified configuration.</summary>
  public static ConfigurableFixedPoint<TStorage> FromMemory(ReadOnlySpan<byte> data, int fractionalBits) {
    _ValidateFractionalBits(fractionalBits);
    var totalBytes = (TotalBits + 7) / 8;
    if (data.Length < totalBytes)
      throw new ArgumentException($"Data must be at least {totalBytes} bytes for {TotalBits}-bit storage.", nameof(data));

    var raw = BigInteger.Zero;
    for (var i = totalBytes - 1; i >= 0; --i)
      raw = (raw << 8) | data[i];

    if (HasSign) {
      var signBit = BigInteger.One << (TotalBits - 1);
      if ((raw & signBit) != 0)
        raw -= BigInteger.One << TotalBits;
    }

    return new(raw, (byte)fractionalBits);
  }

  /// <summary>Writes the raw value to a new byte array.</summary>
  public byte[] ToMemory() {
    var totalBytes = (TotalBits + 7) / 8;
    var result = new byte[totalBytes];
    var raw = this.RawBits;
    if (raw < 0)
      raw += BigInteger.One << TotalBits;
    for (var i = 0; i < totalBytes; ++i) {
      result[i] = (byte)(raw & 0xFF);
      raw >>= 8;
    }
    return result;
  }

  /// <summary>Writes the raw value to the destination span.</summary>
  /// <returns>The number of bytes written.</returns>
  public int ToMemory(Span<byte> destination) {
    var totalBytes = (TotalBits + 7) / 8;
    if (destination.Length < totalBytes)
      throw new ArgumentException($"Destination must be at least {totalBytes} bytes.", nameof(destination));

    var raw = this.RawBits;
    if (raw < 0)
      raw += BigInteger.One << TotalBits;
    for (var i = 0; i < totalBytes; ++i) {
      destination[i] = (byte)(raw & 0xFF);
      raw >>= 8;
    }
    return totalBytes;
  }

  #endregion

  #region Arithmetic Operations

  /// <summary>Adds two values. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Add(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    if (left._EffectiveFractionalBits == right._EffectiveFractionalBits)
      return new(left.RawBits + right.RawBits, config);

    var rescaledRight = _RescaleRawBits(right.RawBits, right._EffectiveFractionalBits, config);
    return new(left.RawBits + rescaledRight, config);
  }

  /// <summary>Subtracts two values. For unsigned types, uses saturating arithmetic. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Subtract(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    BigInteger result;
    if (left._EffectiveFractionalBits == right._EffectiveFractionalBits)
      result = left.RawBits - right.RawBits;
    else {
      var rescaledRight = _RescaleRawBits(right.RawBits, right._EffectiveFractionalBits, config);
      result = left.RawBits - rescaledRight;
    }
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return new(result, config);
  }

  /// <summary>Multiplies two values. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Multiply(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    BigInteger result;
    if (left._EffectiveFractionalBits == right._EffectiveFractionalBits)
      result = (left.RawBits * right.RawBits) >> config;
    else {
      var (m1, p1) = left.DecomposeExact();
      var (m2, p2) = right.DecomposeExact();
      var product = m1 * m2;
      result = RecomposeFixed(product, p1 + p2, config).RawBits;
      return new(result, config);
    }
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return new(result, config);
  }

  /// <summary>Divides two values. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Divide(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    if (right.RawBits == 0)
      throw new DivideByZeroException();

    var config = left._EffectiveFractionalBits;
    BigInteger result;
    if (left._EffectiveFractionalBits == right._EffectiveFractionalBits)
      result = (left.RawBits << config) / right.RawBits;
    else {
      var (m1, p1) = left.DecomposeExact();
      var (m2, p2) = right.DecomposeExact();
      var extraBits = config + 3;
      var absM1 = BigInteger.Abs(m1);
      var absM2 = BigInteger.Abs(m2);
      var m1Ext = absM1 << extraBits;
      var quotient = m1Ext / absM2;
      var rem = m1Ext % absM2;
      if (rem != 0)
        quotient |= 1;

      var signNeg = (m1 < 0) != (m2 < 0);
      if (signNeg)
        quotient = -quotient;
      return RecomposeFixed(quotient, p1 - p2 - extraBits, config);
    }
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return new(result, config);
  }

  /// <summary>Negates a value. For unsigned types, returns zero.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Negate(ConfigurableFixedPoint<TStorage> value) {
    if (!HasSign)
      return new(BigInteger.Zero, value._EffectiveFractionalBits);
    return new(-value.RawBits, value._EffectiveFractionalBits);
  }

  /// <summary>Returns the absolute value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Abs(ConfigurableFixedPoint<TStorage> value) {
    if (!HasSign)
      return value;
    return value.RawBits >= 0 ? value : new(-value.RawBits, value._EffectiveFractionalBits);
  }

  /// <summary>Computes the modulo (remainder) of division. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Modulo(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    if (right.RawBits == 0)
      throw new DivideByZeroException();

    var config = left._EffectiveFractionalBits;
    BigInteger result;
    if (left._EffectiveFractionalBits == right._EffectiveFractionalBits)
      result = left.RawBits % right.RawBits;
    else {
      var maxFrac = Math.Max((int)left._EffectiveFractionalBits, (int)right._EffectiveFractionalBits);
      var leftScaled = _RescaleRawBits(left.RawBits, left._EffectiveFractionalBits, maxFrac);
      var rightScaled = _RescaleRawBits(right.RawBits, right._EffectiveFractionalBits, maxFrac);
      var modResult = leftScaled % rightScaled;
      result = _RescaleRawBits(modResult, maxFrac, config);
    }
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return new(result, config);
  }

  #endregion

  #region Math Helpers

  /// <summary>Returns the smaller of two values. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Min(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    if (left.CompareTo(right) <= 0)
      return left;
    if (left._EffectiveFractionalBits == right._EffectiveFractionalBits)
      return right;
    var rescaled = _RescaleRawBits(right.RawBits, right._EffectiveFractionalBits, left._EffectiveFractionalBits);
    return new(rescaled, left._EffectiveFractionalBits);
  }

  /// <summary>Returns the larger of two values. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Max(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    if (left.CompareTo(right) >= 0)
      return left;
    if (left._EffectiveFractionalBits == right._EffectiveFractionalBits)
      return right;
    var rescaled = _RescaleRawBits(right.RawBits, right._EffectiveFractionalBits, left._EffectiveFractionalBits);
    return new(rescaled, left._EffectiveFractionalBits);
  }

  /// <summary>Clamps a value to the specified range. Result in value's config.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> Clamp(ConfigurableFixedPoint<TStorage> value, ConfigurableFixedPoint<TStorage> min, ConfigurableFixedPoint<TStorage> max) {
    if (value.CompareTo(min) < 0) {
      if (value._EffectiveFractionalBits == min._EffectiveFractionalBits)
        return min;
      return new(_RescaleRawBits(min.RawBits, min._EffectiveFractionalBits, value._EffectiveFractionalBits), value._EffectiveFractionalBits);
    }
    if (value.CompareTo(max) > 0) {
      if (value._EffectiveFractionalBits == max._EffectiveFractionalBits)
        return max;
      return new(_RescaleRawBits(max.RawBits, max._EffectiveFractionalBits, value._EffectiveFractionalBits), value._EffectiveFractionalBits);
    }
    return value;
  }

  /// <summary>Returns the floor of this value.</summary>
  public static ConfigurableFixedPoint<TStorage> Floor(ConfigurableFixedPoint<TStorage> value) {
    var scale = value.Scale;
    var integerPart = value.RawBits / scale;
    if (value.RawBits < 0 && value.RawBits % scale != 0)
      --integerPart;
    return new(integerPart * scale, value._EffectiveFractionalBits);
  }

  /// <summary>Returns the ceiling of this value.</summary>
  public static ConfigurableFixedPoint<TStorage> Ceiling(ConfigurableFixedPoint<TStorage> value) {
    var scale = value.Scale;
    var integerPart = value.RawBits / scale;
    if (value.RawBits > 0 && value.RawBits % scale != 0)
      ++integerPart;
    return new(integerPart * scale, value._EffectiveFractionalBits);
  }

  /// <summary>Returns the value rounded to the nearest integer.</summary>
  public static ConfigurableFixedPoint<TStorage> Round(ConfigurableFixedPoint<TStorage> value) {
    var scale = value.Scale;
    var half = scale / 2;
    BigInteger adjusted;
    if (value.RawBits >= 0)
      adjusted = value.RawBits + half;
    else
      adjusted = value.RawBits - half;
    var integerPart = adjusted / scale;
    return new(integerPart * scale, value._EffectiveFractionalBits);
  }

  /// <summary>Returns the truncated value (integer part towards zero).</summary>
  public static ConfigurableFixedPoint<TStorage> Truncate(ConfigurableFixedPoint<TStorage> value) {
    var scale = value.Scale;
    var integerPart = value.RawBits / scale;
    return new(integerPart * scale, value._EffectiveFractionalBits);
  }

  /// <summary>Returns the fractional part of this value.</summary>
  public static ConfigurableFixedPoint<TStorage> FractionalPart(ConfigurableFixedPoint<TStorage> value) {
    var fractional = value.RawBits % value.Scale;
    return new(fractional, value._EffectiveFractionalBits);
  }

  #endregion

  #region Comparison

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(ConfigurableFixedPoint<TStorage> other) {
    if (this._EffectiveFractionalBits == other._EffectiveFractionalBits)
      return this.RawBits.CompareTo(other.RawBits);

    var maxFrac = Math.Max((int)this._EffectiveFractionalBits, (int)other._EffectiveFractionalBits);
    var leftScaled = _RescaleRawBits(this.RawBits, this._EffectiveFractionalBits, maxFrac);
    var rightScaled = _RescaleRawBits(other.RawBits, other._EffectiveFractionalBits, maxFrac);
    return leftScaled.CompareTo(rightScaled);
  }

  /// <summary>Cross-type comparison with floating-point.</summary>
  public int CompareTo(ConfigurableFloatingPoint<TStorage> other) =>
    -other.CompareTo(this);

  public int CompareTo(object? obj) {
    if (obj is null) return 1;
    if (obj is ConfigurableFixedPoint<TStorage> other)
      return this.CompareTo(other);
    if (obj is ConfigurableFloatingPoint<TStorage> floatingOther)
      return this.CompareTo(floatingOther);
    throw new ArgumentException($"Object must be of type {nameof(ConfigurableFixedPoint<TStorage>)}.", nameof(obj));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ConfigurableFixedPoint<TStorage> other) {
    if (this._EffectiveFractionalBits == other._EffectiveFractionalBits)
      return this._rawValue.Equals(other._rawValue);

    if (this.RawBits == 0 && other.RawBits == 0)
      return true;

    var maxFrac = Math.Max((int)this._EffectiveFractionalBits, (int)other._EffectiveFractionalBits);
    var leftScaled = _RescaleRawBits(this.RawBits, this._EffectiveFractionalBits, maxFrac);
    var rightScaled = _RescaleRawBits(other.RawBits, other._EffectiveFractionalBits, maxFrac);
    return leftScaled == rightScaled;
  }

  public override bool Equals(object? obj) =>
    obj is ConfigurableFixedPoint<TStorage> other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    if (this.RawBits == 0)
      return 0;
    var raw = this.RawBits;
    var frac = (int)this._EffectiveFractionalBits;
    while (frac > 0 && raw != 0 && raw.IsEven) {
      raw >>= 1;
      --frac;
    }
    return HashCode.Combine(raw, frac);
  }

  #endregion

  #region Operators

  public static bool operator ==(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => left.Equals(right);
  public static bool operator !=(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => !left.Equals(right);
  public static bool operator <(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => left.CompareTo(right) < 0;
  public static bool operator >(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => left.CompareTo(right) > 0;
  public static bool operator <=(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => left.CompareTo(right) <= 0;
  public static bool operator >=(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => left.CompareTo(right) >= 0;

  public static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => Add(left, right);
  public static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => Subtract(left, right);
  public static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => Multiply(left, right);
  public static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => Divide(left, right);
  public static ConfigurableFixedPoint<TStorage> operator %(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) => Modulo(left, right);
  public static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> value) => Negate(value);
  public static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> value) => value;

  public static ConfigurableFixedPoint<TStorage> operator ++(ConfigurableFixedPoint<TStorage> value) => new(value.RawBits + 1, value._EffectiveFractionalBits);
  public static ConfigurableFixedPoint<TStorage> operator --(ConfigurableFixedPoint<TStorage> value) {
    if (!HasSign && value.RawBits <= 0)
      return new(BigInteger.Zero, value._EffectiveFractionalBits);
    return new(value.RawBits - 1, value._EffectiveFractionalBits);
  }

  #endregion

  #region Mixed-Type Arithmetic with Integers

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, int right) =>
    new(left.RawBits * right, left._EffectiveFractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> operator *(int left, ConfigurableFixedPoint<TStorage> right) =>
    new(left * right.RawBits, right._EffectiveFractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, int right) =>
    new(left.RawBits / right, left._EffectiveFractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, int right) =>
    new(left.RawBits + ((BigInteger)right << left.FractionalBits), left._EffectiveFractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> operator +(int left, ConfigurableFixedPoint<TStorage> right) =>
    new(((BigInteger)left << right.FractionalBits) + right.RawBits, right._EffectiveFractionalBits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, int right) {
    var result = left.RawBits - ((BigInteger)right << left.FractionalBits);
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, left._EffectiveFractionalBits);
    return new(result, left._EffectiveFractionalBits);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFixedPoint<TStorage> operator -(int left, ConfigurableFixedPoint<TStorage> right) {
    var result = ((BigInteger)left << right.FractionalBits) - right.RawBits;
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, right._EffectiveFractionalBits);
    return new(result, right._EffectiveFractionalBits);
  }

  #endregion

  #region Mixed-Type Arithmetic with Doubles

  public static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, double right) =>
    FromDouble(left.ToDouble() + right, left.FractionalBits);

  public static ConfigurableFixedPoint<TStorage> operator +(double left, ConfigurableFixedPoint<TStorage> right) =>
    FromDouble(left + right.ToDouble(), right.FractionalBits);

  public static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, double right) {
    var result = left.ToDouble() - right;
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, left._EffectiveFractionalBits);
    return FromDouble(result, left.FractionalBits);
  }

  public static ConfigurableFixedPoint<TStorage> operator -(double left, ConfigurableFixedPoint<TStorage> right) {
    var result = left - right.ToDouble();
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, right._EffectiveFractionalBits);
    return FromDouble(result, right.FractionalBits);
  }

  public static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, double right) =>
    FromDouble(left.ToDouble() * right, left.FractionalBits);

  public static ConfigurableFixedPoint<TStorage> operator *(double left, ConfigurableFixedPoint<TStorage> right) =>
    FromDouble(left * right.ToDouble(), right.FractionalBits);

  public static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, double right) =>
    FromDouble(left.ToDouble() / right, left.FractionalBits);

  public static ConfigurableFixedPoint<TStorage> operator /(double left, ConfigurableFixedPoint<TStorage> right) =>
    FromDouble(left / right.ToDouble(), right.FractionalBits);

  #endregion

  #region Cross-Type Arithmetic (with ConfigurableFloatingPoint)

  public static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    if (ConfigurableFloatingPoint<TStorage>.IsNaN(right))
      throw new ArgumentException("Cannot add NaN to fixed-point.");
    if (ConfigurableFloatingPoint<TStorage>.IsInfinity(right))
      throw new OverflowException("Cannot add Infinity to fixed-point.");
    if (ConfigurableFloatingPoint<TStorage>.IsZero(right))
      return left;
    if (left.RawBits == 0) {
      var (m2, p2) = right.DecomposeExact();
      return RecomposeFixed(m2, p2, config);
    }

    var (m1, p1) = left.DecomposeExact();
    var (rm2, rp2) = right.DecomposeExact();
    var minP = Math.Min(p1, rp2);
    var result = (m1 << (p1 - minP)) + (rm2 << (rp2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return RecomposeFixed(result, minP, config);
  }

  public static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    if (ConfigurableFloatingPoint<TStorage>.IsNaN(right))
      throw new ArgumentException("Cannot subtract NaN from fixed-point.");
    if (ConfigurableFloatingPoint<TStorage>.IsInfinity(right))
      throw new OverflowException("Cannot subtract Infinity from fixed-point.");
    if (ConfigurableFloatingPoint<TStorage>.IsZero(right))
      return left;

    var (m1, p1) = left.DecomposeExact();
    var (m2, p2) = right.DecomposeExact();
    var minP = Math.Min(p1, p2);
    var result = (m1 << (p1 - minP)) - (m2 << (p2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return RecomposeFixed(result, minP, config);
  }

  public static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    if (ConfigurableFloatingPoint<TStorage>.IsNaN(right))
      throw new ArgumentException("Cannot multiply fixed-point by NaN.");
    if (ConfigurableFloatingPoint<TStorage>.IsInfinity(right))
      throw new OverflowException("Cannot multiply fixed-point by Infinity.");
    if (left.RawBits == 0 || ConfigurableFloatingPoint<TStorage>.IsZero(right))
      return new(BigInteger.Zero, config);

    var (m1, p1) = left.DecomposeExact();
    var (m2, p2) = right.DecomposeExact();
    var result = m1 * m2;
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return RecomposeFixed(result, p1 + p2, config);
  }

  public static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    if (ConfigurableFloatingPoint<TStorage>.IsNaN(right))
      throw new ArgumentException("Cannot divide fixed-point by NaN.");
    if (ConfigurableFloatingPoint<TStorage>.IsZero(right))
      throw new DivideByZeroException();
    if (ConfigurableFloatingPoint<TStorage>.IsInfinity(right))
      return new(BigInteger.Zero, config);
    if (left.RawBits == 0)
      return new(BigInteger.Zero, config);

    var (m1, p1) = left.DecomposeExact();
    var (m2, p2) = right.DecomposeExact();
    var extraBits = config + 3;
    var absM1 = BigInteger.Abs(m1);
    var absM2 = BigInteger.Abs(m2);
    var m1Ext = absM1 << extraBits;
    var quotient = m1Ext / absM2;
    var rem = m1Ext % absM2;
    if (rem != 0)
      quotient |= 1;

    var signNeg = (m1 < 0) != (m2 < 0);
    if (signNeg)
      quotient = -quotient;
    if (!HasSign && quotient < 0)
      return new(BigInteger.Zero, config);
    return RecomposeFixed(quotient, p1 - p2 - extraBits, config);
  }

  public static ConfigurableFixedPoint<TStorage> operator %(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveFractionalBits;
    if (ConfigurableFloatingPoint<TStorage>.IsNaN(right))
      throw new ArgumentException("Cannot modulo fixed-point by NaN.");
    if (ConfigurableFloatingPoint<TStorage>.IsZero(right))
      throw new DivideByZeroException();
    if (left.RawBits == 0)
      return new(BigInteger.Zero, config);
    if (ConfigurableFloatingPoint<TStorage>.IsInfinity(right))
      return left;

    var (m1, p1) = left.DecomposeExact();
    var (m2, p2) = right.DecomposeExact();
    var minP = Math.Min(p1, p2);
    var result = (m1 << (p1 - minP)) % (m2 << (p2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return RecomposeFixed(result, minP, config);
  }

  #endregion

  #region Type Conversions

  public static explicit operator double(ConfigurableFixedPoint<TStorage> value) => value.ToDouble();
  public static explicit operator float(ConfigurableFixedPoint<TStorage> value) => value.ToSingle();
  public static explicit operator decimal(ConfigurableFixedPoint<TStorage> value) => value.ToDecimal();
  public static explicit operator Half(ConfigurableFixedPoint<TStorage> value) => value.ToHalf();
  public static explicit operator Quarter(ConfigurableFixedPoint<TStorage> value) => value.ToQuarter();
  public static explicit operator E4M3(ConfigurableFixedPoint<TStorage> value) => value.ToE4M3();
  public static explicit operator BFloat8(ConfigurableFixedPoint<TStorage> value) => value.ToBFloat8();
  public static explicit operator BFloat16(ConfigurableFixedPoint<TStorage> value) => value.ToBFloat16();
  public static explicit operator BFloat32(ConfigurableFixedPoint<TStorage> value) => value.ToBFloat32();
  public static explicit operator BFloat64(ConfigurableFixedPoint<TStorage> value) => value.ToBFloat64();

  public static explicit operator byte(ConfigurableFixedPoint<TStorage> value) => (byte)value.ToInt32();
  public static explicit operator sbyte(ConfigurableFixedPoint<TStorage> value) => (sbyte)value.ToInt32();
  public static explicit operator short(ConfigurableFixedPoint<TStorage> value) => (short)value.ToInt32();
  public static explicit operator ushort(ConfigurableFixedPoint<TStorage> value) => (ushort)value.ToInt32();
  public static explicit operator int(ConfigurableFixedPoint<TStorage> value) => value.ToInt32();
  public static explicit operator uint(ConfigurableFixedPoint<TStorage> value) => value.ToUInt32();
  public static explicit operator long(ConfigurableFixedPoint<TStorage> value) => value.ToInt64();
  public static explicit operator ulong(ConfigurableFixedPoint<TStorage> value) => value.ToUInt64();

  #endregion

  #region String Representation

  public override string ToString() => this.ToDouble().ToString(CultureInfo.InvariantCulture);

  public string ToString(IFormatProvider? provider) => this.ToDouble().ToString(provider);

  public string ToString(string? format) => this.ToDouble().ToString(format, CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? provider) => this.ToDouble().ToString(format, provider);

  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) {
    var str = format.IsEmpty
      ? this.ToDouble().ToString(provider)
      : this.ToDouble().ToString(format.ToString(), provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  #endregion

  #region Parsing (uses default config for static interface)

  public static ConfigurableFixedPoint<TStorage> Parse(string s) =>
    Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static ConfigurableFixedPoint<TStorage> Parse(string s, IFormatProvider? provider) =>
    Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static ConfigurableFixedPoint<TStorage> Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    return FromDouble(value, _defaultFractionalBits);
  }

  public static bool TryParse(string? s, out ConfigurableFixedPoint<TStorage> result) =>
    TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out ConfigurableFixedPoint<TStorage> result) =>
    TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ConfigurableFixedPoint<TStorage> result) {
    if (double.TryParse(s, style, provider, out var value)) {
      result = FromDouble(value, _defaultFractionalBits);
      return true;
    }
    result = default;
    return false;
  }

  public static ConfigurableFixedPoint<TStorage> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    return FromDouble(value, _defaultFractionalBits);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ConfigurableFixedPoint<TStorage> result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      result = FromDouble(value, _defaultFractionalBits);
      return true;
    }
    result = default;
    return false;
  }

  #endregion

  #region Validation

  private static void _ValidateFractionalBits(int fractionalBits) {
    var signBit = HasSign ? 1 : 0;
    if (fractionalBits < 0)
      throw new ArgumentOutOfRangeException(nameof(fractionalBits), "Fractional bits cannot be negative.");
    if (fractionalBits + signBit > TotalBits)
      throw new ArgumentOutOfRangeException(nameof(fractionalBits), $"Fractional bits ({fractionalBits}) exceeds available space in {TotalBits}-bit storage.");
  }

  #endregion

  #region Helper Methods

  private static BigInteger _ConvertToBigInteger(TStorage value) {
    return value switch {
      byte b => b,
      sbyte sb => sb,
      ushort us => us,
      short s => s,
      uint ui => ui,
      int i => i,
      ulong ul => ul,
      long l => l,
      UInt96 ui96 => _UInt96ToBigInteger(ui96),
      Int96 i96 => _Int96ToBigInteger(i96),
      UInt128 ui128 => _UInt128ToBigInteger(ui128),
      Int128 i128 => _Int128ToBigInteger(i128),
      _ => throw new NotSupportedException($"Storage type {typeof(TStorage).Name} is not supported.")
    };
  }

  private static BigInteger _UInt96ToBigInteger(UInt96 value) {
    var upper = value.Upper;
    var lower = value.Lower;
    return (new BigInteger(upper) << 64) | new BigInteger(lower);
  }

  private static BigInteger _Int96ToBigInteger(Int96 value) {
    var isNegative = Int96.IsNegative(value);
    if (isNegative)
      value = -value;
    var upper = value.Upper;
    var lower = value.Lower;
    var result = (new BigInteger(upper) << 64) | new BigInteger(lower);
    return isNegative ? -result : result;
  }

  private static UInt96 _BigIntegerToUInt96(BigInteger value) {
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (uint)((value >> 64) & uint.MaxValue);
    return new UInt96(upper, lower);
  }

  private static Int96 _BigIntegerToInt96(BigInteger value) {
    var isNegative = value < 0;
    if (isNegative)
      value = -value;
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (uint)((value >> 64) & uint.MaxValue);
    var result = new Int96(upper, lower);
    return isNegative ? -result : result;
  }

  private static BigInteger _UInt128ToBigInteger(UInt128 value) {
    var upper = (ulong)(value >> 64);
    var lower = (ulong)value;
    return (new BigInteger(upper) << 64) | new BigInteger(lower);
  }

  private static BigInteger _Int128ToBigInteger(Int128 value) {
    var isNegative = Int128.IsNegative(value);
    if (isNegative)
      value = -value;
    var upper = (ulong)((UInt128)value >> 64);
    var lower = (ulong)(UInt128)value;
    var result = (new BigInteger(upper) << 64) | new BigInteger(lower);
    return isNegative ? -result : result;
  }

  private static UInt128 _BigIntegerToUInt128(BigInteger value) {
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (ulong)((value >> 64) & ulong.MaxValue);
    return new UInt128(upper, lower);
  }

  private static Int128 _BigIntegerToInt128(BigInteger value) {
    var isNegative = value < 0;
    if (isNegative)
      value = -value;
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (ulong)((value >> 64) & ulong.MaxValue);
    var result = new Int128(upper, lower);
    return isNegative ? -result : result;
  }

  private static TStorage _ConvertFromBigInteger(BigInteger value) {
    if (HasSign) {
      var maxRaw = (BigInteger.One << (TotalBits - 1)) - 1;
      var minRaw = -(BigInteger.One << (TotalBits - 1));
      if (value > maxRaw)
        value = maxRaw;
      else if (value < minRaw)
        value = minRaw;
    } else {
      var maxRaw = (BigInteger.One << TotalBits) - 1;
      if (value < 0)
        value = 0;
      else if (value > maxRaw)
        value = maxRaw;
    }

    if (typeof(TStorage) == typeof(byte)) { var r = (byte)value; return Unsafe.As<byte, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(sbyte)) { var r = (sbyte)value; return Unsafe.As<sbyte, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(ushort)) { var r = (ushort)value; return Unsafe.As<ushort, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(short)) { var r = (short)value; return Unsafe.As<short, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(uint)) { var r = (uint)value; return Unsafe.As<uint, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(int)) { var r = (int)value; return Unsafe.As<int, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(ulong)) { var r = (ulong)value; return Unsafe.As<ulong, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(long)) { var r = (long)value; return Unsafe.As<long, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(UInt96)) { var r = _BigIntegerToUInt96(value); return Unsafe.As<UInt96, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(Int96)) { var r = _BigIntegerToInt96(value); return Unsafe.As<Int96, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(UInt128)) { var r = _BigIntegerToUInt128(value); return Unsafe.As<UInt128, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(Int128)) { var r = _BigIntegerToInt128(value); return Unsafe.As<Int128, TStorage>(ref r); }
    throw new NotSupportedException($"Storage type {typeof(TStorage).Name} is not supported.");
  }

  #endregion

}
