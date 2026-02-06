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
/// Represents an instance-configurable floating-point number with customizable mantissa and exponent sizes.
/// The storage type determines signedness: signed types (sbyte, short, int, long) have a sign bit,
/// unsigned types (byte, ushort, uint, ulong) do not and use saturating arithmetic for subtraction.
/// Each instance carries its own bit layout configuration.
/// Exponent bits are computed automatically: TotalBits - sign - mantissaBits.
/// Cross-config arithmetic uses the left operand's config for the result.
/// </summary>
/// <typeparam name="TStorage">The underlying storage type (byte, sbyte, ushort, short, uint, int, ulong, long, UInt96, Int96, UInt128, Int128).</typeparam>
public readonly struct ConfigurableFloatingPoint<TStorage> : IComparable, IComparable<ConfigurableFloatingPoint<TStorage>>, IEquatable<ConfigurableFloatingPoint<TStorage>>, IFormattable, ISpanFormattable, IParsable<ConfigurableFloatingPoint<TStorage>>, ISpanParsable<ConfigurableFloatingPoint<TStorage>>
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

  private static (int mantissa, int exponent) _GetDefaultBitLayout(int totalBits, bool hasSign) =>
    totalBits switch {
      8 => hasSign ? (3, 4) : (3, 5),
      16 => hasSign ? (10, 5) : (10, 6),
      32 => hasSign ? (23, 8) : (23, 9),
      64 => hasSign ? (52, 11) : (52, 12),
      96 => hasSign ? (80, 15) : (80, 16),
      128 => hasSign ? (112, 15) : (112, 16),
      _ => ((totalBits - (hasSign ? 1 : 0)) * 2 / 3, (totalBits - (hasSign ? 1 : 0)) / 3)
    };

  private static readonly byte _defaultMantissaBits = (byte)_GetDefaultBitLayout(TotalBits, HasSign).mantissa;

  /// <summary>
  /// Gets the default mantissa bits for this storage type (IEEE 754 standard layout).
  /// NaN, PositiveInfinity, and NegativeInfinity always use this config.
  /// </summary>
  public static int DefaultMantissaBits => _defaultMantissaBits;

  private static readonly ConfigurableFloatingPoint<TStorage> _nan = _CreateSpecialNaN();
  private static readonly ConfigurableFloatingPoint<TStorage> _positiveInfinity = _CreateSpecialPositiveInfinity();
  private static readonly ConfigurableFloatingPoint<TStorage> _negativeInfinity = _CreateSpecialNegativeInfinity();

  private static ConfigurableFloatingPoint<TStorage> _CreateSpecialNaN() {
    var m = (int)_defaultMantissaBits;
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - m;
    var expMask = (BigInteger.One << exponentBits) - 1;
    return new((expMask << m) | 1, _defaultMantissaBits);
  }

  private static ConfigurableFloatingPoint<TStorage> _CreateSpecialPositiveInfinity() {
    var m = (int)_defaultMantissaBits;
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - m;
    var expMask = (BigInteger.One << exponentBits) - 1;
    return new(expMask << m, _defaultMantissaBits);
  }

  private static ConfigurableFloatingPoint<TStorage> _CreateSpecialNegativeInfinity() {
    var m = (int)_defaultMantissaBits;
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - m;
    var expMask = (BigInteger.One << exponentBits) - 1;
    var raw = expMask << m;
    if (HasSign)
      raw |= BigInteger.One << (exponentBits + m);
    return new(raw, _defaultMantissaBits);
  }

  #endregion

  #region Instance Data

  private readonly TStorage _rawValue;
  private readonly byte _mantissaBits;

  #endregion

  #region Computed Instance Properties (derived from _mantissaBits)

  private byte _EffectiveMantissaBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._mantissaBits != 0 ? this._mantissaBits : _defaultMantissaBits;
  }

  /// <summary>
  /// Gets the number of mantissa (significand) bits.
  /// </summary>
  public int MantissaBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._EffectiveMantissaBits;
  }

  /// <summary>
  /// Gets the number of exponent bits.
  /// </summary>
  public int ExponentBits {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => TotalBits - (HasSign ? 1 : 0) - this._EffectiveMantissaBits;
  }

  /// <summary>
  /// Gets the exponent bias.
  /// </summary>
  public int ExponentBias {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (1 << (this.ExponentBits - 1)) - 1;
  }

  private BigInteger _MantissaMask {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (BigInteger.One << this._EffectiveMantissaBits) - 1;
  }

  private BigInteger _ExponentMask {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (BigInteger.One << this.ExponentBits) - 1;
  }

  private BigInteger _SignMask {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => HasSign ? BigInteger.One << (this.ExponentBits + this._EffectiveMantissaBits) : BigInteger.Zero;
  }

  private BigInteger _MaxRawValue {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (BigInteger.One << TotalBits) - 1;
  }

  private BigInteger _InfinityRaw {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ExponentMask << this._EffectiveMantissaBits;
  }

  private BigInteger _NegativeInfinityRaw {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => HasSign ? this._SignMask | this._InfinityRaw : this._InfinityRaw;
  }

  private BigInteger _NanRaw {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (this._ExponentMask << this._EffectiveMantissaBits) | 1;
  }

  private BigInteger _OneRaw {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (BigInteger)this.ExponentBias << this._EffectiveMantissaBits;
  }

  private BigInteger _MaxValueRaw {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ((this._ExponentMask - 1) << this._EffectiveMantissaBits) | this._MantissaMask;
  }

  private BigInteger _MinValueRaw {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => HasSign ? this._SignMask | this._MaxValueRaw : BigInteger.Zero;
  }

  #endregion

  #region Constructors

  /// <summary>
  /// Creates a zero-valued instance with the specified mantissa bits.
  /// Exponent bits are computed as TotalBits - sign - mantissaBits.
  /// </summary>
  /// <param name="mantissaBits">Number of bits for the mantissa (significand).</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the bit layout is invalid for the storage type.</exception>
  public ConfigurableFloatingPoint(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    this._rawValue = default;
    this._mantissaBits = (byte)mantissaBits;
  }

  private ConfigurableFloatingPoint(TStorage raw, byte mantissaBits) {
    this._rawValue = raw;
    this._mantissaBits = mantissaBits;
  }

  private ConfigurableFloatingPoint(BigInteger raw, byte mantissaBits) {
    this._mantissaBits = mantissaBits;
    var maxRaw = (BigInteger.One << TotalBits) - 1;
    this._rawValue = _ConvertFromBigInteger(raw & maxRaw);
  }

  #endregion

  #region Properties

  /// <summary>
  /// Gets the raw bit representation as the underlying storage type.
  /// </summary>
  public TStorage RawValue => this._rawValue;

  /// <summary>
  /// Gets the raw bit representation as a BigInteger for extended precision operations.
  /// </summary>
  public BigInteger RawBits => _ConvertToBigInteger(this._rawValue);

  private BigInteger Sign => HasSign ? (this.RawBits >> (this.ExponentBits + this._EffectiveMantissaBits)) & 1 : BigInteger.Zero;
  private BigInteger Exponent => (this.RawBits >> this._EffectiveMantissaBits) & this._ExponentMask;
  private BigInteger Mantissa => this.RawBits & this._MantissaMask;

  #endregion

  #region Special Value Factories

  /// <summary>
  /// Creates a zero value with the specified mantissa bits.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> Zero(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    return new(BigInteger.Zero, (byte)mantissaBits);
  }

  /// <summary>
  /// Creates a one value with the specified mantissa bits.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> One(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var bias = (1 << (exponentBits - 1)) - 1;
    return new((BigInteger)bias << mantissaBits, (byte)mantissaBits);
  }

  /// <summary>
  /// Creates an epsilon (smallest positive subnormal) value with the specified mantissa bits.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> Epsilon(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    return new(BigInteger.One, (byte)mantissaBits);
  }

  /// <summary>
  /// Gets positive infinity. Always uses the default mantissa bits for this storage type.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> PositiveInfinity => _positiveInfinity;

  /// <summary>
  /// Creates positive infinity with a specific mantissa config (for interop scenarios).
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> CreatePositiveInfinity(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var expMask = (BigInteger.One << exponentBits) - 1;
    return new(expMask << mantissaBits, (byte)mantissaBits);
  }

  /// <summary>
  /// Gets negative infinity. Always uses the default mantissa bits for this storage type.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> NegativeInfinity => _negativeInfinity;

  /// <summary>
  /// Creates negative infinity with a specific mantissa config (for interop scenarios).
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> CreateNegativeInfinity(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var expMask = (BigInteger.One << exponentBits) - 1;
    var raw = expMask << mantissaBits;
    if (HasSign)
      raw |= BigInteger.One << (exponentBits + mantissaBits);
    return new(raw, (byte)mantissaBits);
  }

  /// <summary>
  /// Gets a NaN value. Always uses the default mantissa bits for this storage type.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> NaN => _nan;

  /// <summary>
  /// Creates a NaN value with a specific mantissa config (for interop scenarios).
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> CreateNaN(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var expMask = (BigInteger.One << exponentBits) - 1;
    return new((expMask << mantissaBits) | 1, (byte)mantissaBits);
  }

  /// <summary>
  /// Creates the maximum finite positive value with the specified mantissa bits.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> MaxValue(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var expMask = (BigInteger.One << exponentBits) - 1;
    var mantMask = (BigInteger.One << mantissaBits) - 1;
    return new(((expMask - 1) << mantissaBits) | mantMask, (byte)mantissaBits);
  }

  /// <summary>
  /// Creates the minimum finite value with the specified mantissa bits.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> MinValue(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var expMask = (BigInteger.One << exponentBits) - 1;
    var mantMask = (BigInteger.One << mantissaBits) - 1;
    var maxVal = ((expMask - 1) << mantissaBits) | mantMask;
    if (HasSign)
      maxVal |= BigInteger.One << (exponentBits + mantissaBits);
    else
      maxVal = BigInteger.Zero;
    return new(maxVal, (byte)mantissaBits);
  }

  /// <summary>
  /// Computes mantissa bits from a given exponent bit count.
  /// Useful when thinking in terms of exponent size.
  /// </summary>
  public static int MantissaBitsFromExponent(int exponentBits) =>
    TotalBits - (HasSign ? 1 : 0) - exponentBits;

  #endregion

  #region Instance Convenience Properties for Special Values

  /// <summary>Gets a zero value with the same configuration.</summary>
  public ConfigurableFloatingPoint<TStorage> AsZero => new(BigInteger.Zero, this._EffectiveMantissaBits);

  /// <summary>Gets a one value with the same configuration.</summary>
  public ConfigurableFloatingPoint<TStorage> AsOne => new(this._OneRaw, this._EffectiveMantissaBits);

  /// <summary>Gets an epsilon value with the same configuration.</summary>
  public ConfigurableFloatingPoint<TStorage> AsEpsilon => new(BigInteger.One, this._EffectiveMantissaBits);

  /// <summary>Gets positive infinity (always uses default config).</summary>
  public ConfigurableFloatingPoint<TStorage> AsPositiveInfinity => PositiveInfinity;

  /// <summary>Gets negative infinity (always uses default config).</summary>
  public ConfigurableFloatingPoint<TStorage> AsNegativeInfinity => NegativeInfinity;

  /// <summary>Gets a NaN value (always uses default config).</summary>
  public ConfigurableFloatingPoint<TStorage> AsNaN => NaN;

  /// <summary>Gets the maximum finite positive value with the same configuration.</summary>
  public ConfigurableFloatingPoint<TStorage> AsMaxValue => new(this._MaxValueRaw, this._EffectiveMantissaBits);

  /// <summary>Gets the minimum finite value with the same configuration.</summary>
  public ConfigurableFloatingPoint<TStorage> AsMinValue => new(this._MinValueRaw, this._EffectiveMantissaBits);

  #endregion

  #region Special Value Detection

  /// <summary>Returns true if the value is NaN.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(ConfigurableFloatingPoint<TStorage> value) =>
    value.Exponent == value._ExponentMask && value.Mantissa != BigInteger.Zero;

  /// <summary>Returns true if the value is positive or negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(ConfigurableFloatingPoint<TStorage> value) =>
    value.Exponent == value._ExponentMask && value.Mantissa == BigInteger.Zero;

  /// <summary>Returns true if the value is positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(ConfigurableFloatingPoint<TStorage> value) =>
    value.RawBits == value._InfinityRaw;

  /// <summary>Returns true if the value is negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(ConfigurableFloatingPoint<TStorage> value) =>
    HasSign && value.RawBits == value._NegativeInfinityRaw;

  /// <summary>Returns true if the value is finite (not NaN or infinity).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(ConfigurableFloatingPoint<TStorage> value) =>
    value.Exponent != value._ExponentMask;

  /// <summary>Returns true if the value is a normal number.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(ConfigurableFloatingPoint<TStorage> value) {
    var exp = value.Exponent;
    return exp != BigInteger.Zero && exp != value._ExponentMask;
  }

  /// <summary>Returns true if the value is subnormal (denormalized).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(ConfigurableFloatingPoint<TStorage> value) =>
    value.Exponent == BigInteger.Zero && value.Mantissa != BigInteger.Zero;

  /// <summary>Returns true if the value is negative.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(ConfigurableFloatingPoint<TStorage> value) =>
    HasSign && value.Sign != BigInteger.Zero;

  /// <summary>Returns true if the value is zero (positive or negative).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsZero(ConfigurableFloatingPoint<TStorage> value) =>
    value.Exponent == BigInteger.Zero && value.Mantissa == BigInteger.Zero;

  #endregion

  #region Decompose / Recompose (BigInteger exact arithmetic core)

  /// <summary>
  /// Decomposes this finite non-zero floating-point value into exact (signedMantissa, powerOfTwo)
  /// such that value = signedMantissa * 2^powerOfTwo.
  /// Callers MUST check for specials (NaN, Infinity, Zero) before calling.
  /// </summary>
  private (BigInteger signedMantissa, int powerOfTwo) _Decompose() {
    var mantissaBits = (int)this._EffectiveMantissaBits;
    var bias = this.ExponentBias;
    var exp = (int)this.Exponent;
    var mant = this.Mantissa;

    BigInteger fullMantissa;
    int p2;
    if (exp == 0) {
      fullMantissa = mant;
      p2 = 1 - bias - mantissaBits;
    } else {
      fullMantissa = (BigInteger.One << mantissaBits) | mant;
      p2 = exp - bias - mantissaBits;
    }

    if (IsNegative(this))
      fullMantissa = -fullMantissa;
    return (fullMantissa, p2);
  }

  /// <summary>
  /// Internal accessor for cross-type arithmetic.
  /// Returns exact (signedMantissa, powerOfTwo) representation.
  /// Callers MUST check for specials before calling.
  /// </summary>
  internal (BigInteger signedMantissa, int powerOfTwo) DecomposeExact() => this._Decompose();

  /// <summary>
  /// Recomposes a signedMantissa * 2^powerOfTwo into the target floating-point config
  /// with IEEE 754 round-to-nearest-even semantics.
  /// </summary>
  private static ConfigurableFloatingPoint<TStorage> _Recompose(BigInteger signedMantissa, int powerOfTwo, byte targetMantissaBits) {
    if (signedMantissa == 0)
      return new(BigInteger.Zero, targetMantissaBits);

    var isNegative = signedMantissa < 0;
    if (!HasSign && isNegative)
      return new(BigInteger.Zero, targetMantissaBits);

    var absMantissa = BigInteger.Abs(signedMantissa);
    var mantissaBits = (int)targetMantissaBits;
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var bias = (1 << (exponentBits - 1)) - 1;
    var maxExp = (1 << exponentBits) - 1;
    var expMask = (BigInteger.One << exponentBits) - 1;
    var signMask = HasSign ? BigInteger.One << (exponentBits + mantissaBits) : BigInteger.Zero;

    var bitLen = _BitLength(absMantissa);
    var biasedExp = powerOfTwo + bitLen - 1 + bias;

    if (biasedExp >= maxExp) {
      var raw = expMask << mantissaBits;
      if (HasSign && isNegative)
        raw |= signMask;
      return new(raw, targetMantissaBits);
    }

    BigInteger storedMantissa;
    if (biasedExp > 0) {
      var shift = bitLen - 1 - mantissaBits;
      if (shift > 0) {
        storedMantissa = _RoundRightShift(absMantissa, shift);
        if (_BitLength(storedMantissa) > mantissaBits + 1) {
          ++biasedExp;
          if (biasedExp >= maxExp) {
            var raw = expMask << mantissaBits;
            if (HasSign && isNegative)
              raw |= signMask;
            return new(raw, targetMantissaBits);
          }
          storedMantissa >>= 1;
        }
      } else if (shift < 0)
        storedMantissa = absMantissa << (-shift);
      else
        storedMantissa = absMantissa;

      storedMantissa &= (BigInteger.One << mantissaBits) - 1;
    } else {
      var subnormalShift = 1 - biasedExp;
      var totalShift = bitLen - 1 - mantissaBits + subnormalShift;
      if (totalShift > 0)
        storedMantissa = _RoundRightShift(absMantissa, totalShift);
      else if (totalShift < 0)
        storedMantissa = absMantissa << (-totalShift);
      else
        storedMantissa = absMantissa;

      if (_BitLength(storedMantissa) > mantissaBits) {
        biasedExp = 1;
        storedMantissa &= (BigInteger.One << mantissaBits) - 1;
      } else
        biasedExp = 0;
    }

    var result = ((BigInteger)biasedExp << mantissaBits) | storedMantissa;
    if (HasSign && isNegative)
      result |= signMask;
    return new(result, targetMantissaBits);
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

  #region Factory Methods

  /// <summary>
  /// Creates a value from the raw bit representation with the specified configuration.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromRaw(TStorage raw, int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    return new(_ConvertToBigInteger(raw), (byte)mantissaBits);
  }

  /// <summary>
  /// Creates a value from mantissa, exponent, and sign components.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromComponents(BigInteger mantissa, int exponent, bool isNegative, int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var exponentBits = TotalBits - (HasSign ? 1 : 0) - mantissaBits;
    var mantMask = (BigInteger.One << mantissaBits) - 1;
    var expMask = (BigInteger.One << exponentBits) - 1;
    var maskedMantissa = mantissa & mantMask;
    var clampedExp = BigInteger.Max(BigInteger.Zero, BigInteger.Min((BigInteger)exponent, expMask));
    var raw = (clampedExp << mantissaBits) | maskedMantissa;
    if (HasSign && isNegative)
      raw |= BigInteger.One << (exponentBits + mantissaBits);
    return new(raw, (byte)mantissaBits);
  }

  /// <summary>
  /// Creates a value from a double-precision floating-point number with the specified configuration.
  /// Uses exact BigInteger decomposition of the double, then recomposes into target format.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromDouble(double value, int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var config = (byte)mantissaBits;

    if (double.IsNaN(value))
      return NaN;

    if (double.IsPositiveInfinity(value))
      return PositiveInfinity;

    if (double.IsNegativeInfinity(value))
      return HasSign ? NegativeInfinity : new(BigInteger.Zero, config);

    if (value == 0.0)
      return new(BigInteger.Zero, config);

    var (sm, p2) = _DecomposeDouble(value);
    return _Recompose(sm, p2, config);
  }

  /// <summary>
  /// Creates a value from a double using this instance's configuration.
  /// </summary>
  public ConfigurableFloatingPoint<TStorage> CreateFromDouble(double value) =>
    FromDouble(value, this.MantissaBits);

  /// <summary>
  /// Decomposes a double into exact (signedMantissa, powerOfTwo).
  /// </summary>
  private static (BigInteger signedMantissa, int powerOfTwo) _DecomposeDouble(double value) {
    var bits = BitConverter.DoubleToInt64Bits(value);
    var isNeg = (bits >> 63) != 0;
    var dExp = (int)((bits >> 52) & 0x7FF);
    var dMant = bits & 0xFFFFFFFFFFFFFL;

    BigInteger fullMant;
    int p2;
    if (dExp == 0) {
      fullMant = (BigInteger)dMant;
      p2 = 1 - 1023 - 52;
    } else {
      fullMant = (BigInteger.One << 52) | (BigInteger)dMant;
      p2 = dExp - 1023 - 52;
    }

    return (isNeg ? -fullMant : fullMant, p2);
  }

  /// <summary>
  /// Creates a value from a single-precision floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromSingle(float value, int mantissaBits) => FromDouble(value, mantissaBits);

  /// <summary>
  /// Creates a value from a decimal number using exact BigInteger decomposition (no double intermediate).
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromDecimal(decimal value, int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    if (value == 0m)
      return new(BigInteger.Zero, (byte)mantissaBits);

    var bits = decimal.GetBits(value);
    var lo = (uint)bits[0];
    var mid = (uint)bits[1];
    var hi = (uint)bits[2];
    var intMantissa = (BigInteger)lo | ((BigInteger)mid << 32) | ((BigInteger)hi << 64);
    var scale = (bits[3] >> 16) & 0xFF;
    var isNegative = bits[3] < 0;

    if (!HasSign && isNegative)
      return new(BigInteger.Zero, (byte)mantissaBits);

    if (scale == 0) {
      var sm = isNegative ? -intMantissa : intMantissa;
      return _Recompose(sm, 0, (byte)mantissaBits);
    }

    var extraBits = mantissaBits + 3;
    var divisor = BigInteger.Pow(5, scale);
    var numerator = intMantissa << extraBits;
    var quotient = numerator / divisor;
    var remainder = numerator % divisor;
    if (remainder != 0)
      quotient |= 1;

    var signedResult = isNegative ? -quotient : quotient;
    return _Recompose(signedResult, -scale - extraBits, (byte)mantissaBits);
  }

  /// <summary>
  /// Creates a value from a Half-precision floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromHalf(Half value, int mantissaBits) => FromDouble((double)value, mantissaBits);

  /// <summary>
  /// Creates a value from a Quarter-precision floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromQuarter(Quarter value, int mantissaBits) => FromSingle(value.ToSingle(), mantissaBits);

  /// <summary>
  /// Creates a value from an E4M3 floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromE4M3(E4M3 value, int mantissaBits) => FromSingle(value.ToSingle(), mantissaBits);

  /// <summary>
  /// Creates a value from a BFloat8 floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromBFloat8(BFloat8 value, int mantissaBits) => FromSingle(value.ToSingle(), mantissaBits);

  /// <summary>
  /// Creates a value from a BFloat16 floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromBFloat16(BFloat16 value, int mantissaBits) => FromSingle(value.ToSingle(), mantissaBits);

  /// <summary>
  /// Creates a value from a BFloat32 floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromBFloat32(BFloat32 value, int mantissaBits) => FromDouble(value.ToDouble(), mantissaBits);

  /// <summary>
  /// Creates a value from a BFloat64 floating-point number.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromBFloat64(BFloat64 value, int mantissaBits) => FromDouble(value.ToDouble(), mantissaBits);

  #endregion

  #region Conversion To Other Types

  /// <summary>
  /// Converts this value to a double-precision floating-point number.
  /// Uses exact decomposition then packs into IEEE 754 double bits.
  /// </summary>
  public double ToDouble() {
    if (IsNaN(this)) return double.NaN;
    if (IsPositiveInfinity(this)) return double.PositiveInfinity;
    if (IsNegativeInfinity(this)) return double.NegativeInfinity;
    if (IsZero(this)) return IsNegative(this) ? -0.0 : 0.0;

    var (sm, p2) = this._Decompose();
    return _PackToDouble(sm, p2);
  }

  /// <summary>
  /// Packs (signedMantissa, powerOfTwo) into an IEEE 754 double.
  /// </summary>
  private static double _PackToDouble(BigInteger signedMantissa, int powerOfTwo) {
    if (signedMantissa == 0)
      return 0.0;

    var isNeg = signedMantissa < 0;
    var absMant = BigInteger.Abs(signedMantissa);
    var bitLen = _BitLength(absMant);
    var biasedExp = powerOfTwo + bitLen - 1 + 1023;

    if (biasedExp >= 2047)
      return isNeg ? double.NegativeInfinity : double.PositiveInfinity;

    long doubleMantissa;
    if (biasedExp > 0) {
      var shift = bitLen - 1 - 52;
      BigInteger rounded;
      if (shift > 0)
        rounded = _RoundRightShift(absMant, shift);
      else if (shift < 0)
        rounded = absMant << (-shift);
      else
        rounded = absMant;

      if (_BitLength(rounded) > 53) {
        ++biasedExp;
        if (biasedExp >= 2047)
          return isNeg ? double.NegativeInfinity : double.PositiveInfinity;
        rounded >>= 1;
      }

      doubleMantissa = (long)(rounded & ((BigInteger.One << 52) - 1));
    } else {
      var subnormalShift = 1 - biasedExp;
      var totalShift = bitLen - 1 - 52 + subnormalShift;
      BigInteger rounded;
      if (totalShift > 0)
        rounded = _RoundRightShift(absMant, totalShift);
      else if (totalShift < 0)
        rounded = absMant << (-totalShift);
      else
        rounded = absMant;

      if (totalShift > 52 + bitLen)
        return isNeg ? -0.0 : 0.0;

      if (_BitLength(rounded) > 52) {
        biasedExp = 1;
        rounded &= (BigInteger.One << 52) - 1;
      } else
        biasedExp = 0;

      doubleMantissa = (long)(rounded & ((BigInteger.One << 52) - 1));
    }

    var doubleBits = ((long)biasedExp << 52) | doubleMantissa;
    if (isNeg)
      doubleBits |= unchecked((long)0x8000000000000000);
    return BitConverter.Int64BitsToDouble(doubleBits);
  }

  /// <summary>Converts this value to a single-precision floating-point number.</summary>
  public float ToSingle() => (float)this.ToDouble();

  /// <summary>Converts this value to a decimal number using exact decomposition.</summary>
  public decimal ToDecimal() {
    if (IsNaN(this) || IsInfinity(this))
      throw new OverflowException("Cannot convert NaN or Infinity to decimal.");
    if (IsZero(this))
      return 0m;

    var (sm, p2) = this._Decompose();
    var isNeg = sm < 0;
    var absMant = BigInteger.Abs(sm);

    if (p2 >= 0) {
      var intValue = absMant << p2;
      if (intValue > new BigInteger(decimal.MaxValue))
        throw new OverflowException("Value is too large for decimal.");
      return isNeg ? -(decimal)intValue : (decimal)intValue;
    }

    var negP2 = -p2;
    var numerator = absMant;
    var denominator = BigInteger.One << negP2;
    const int maxScale = 28;
    var scaleFactor = BigInteger.Pow(10, maxScale);
    var scaled = (numerator * scaleFactor) / denominator;

    if (scaled > new BigInteger(decimal.MaxValue) * BigInteger.One)
      return (decimal)this.ToDouble();

    var lo = (int)(uint)(scaled & 0xFFFFFFFF);
    var mid = (int)(uint)((scaled >> 32) & 0xFFFFFFFF);
    var hi = (int)(uint)((scaled >> 64) & 0xFFFFFFFF);
    return new decimal(lo, mid, hi, isNeg, maxScale);
  }

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

  /// <summary>
  /// Converts this floating-point value to a fixed-point representation with the specified fractional bits.
  /// </summary>
  public ConfigurableFixedPoint<TStorage> ToFixedPoint(int fractionalBits) {
    if (IsNaN(this) || IsInfinity(this))
      throw new ArgumentException("Cannot convert NaN or Infinity to fixed-point.");
    if (IsZero(this))
      return ConfigurableFixedPoint<TStorage>.Zero(fractionalBits);

    var (sm, p2) = this._Decompose();
    return ConfigurableFixedPoint<TStorage>.RecomposeFixed(sm, p2, (byte)fractionalBits);
  }

  /// <summary>
  /// Converts this value to another floating-point configuration.
  /// </summary>
  public ConfigurableFloatingPoint<TStorage> ConvertTo(int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    if (this._EffectiveMantissaBits == mantissaBits)
      return this;
    if (IsNaN(this))
      return NaN;
    if (IsPositiveInfinity(this))
      return PositiveInfinity;
    if (IsNegativeInfinity(this))
      return NegativeInfinity;
    if (IsZero(this))
      return Zero(mantissaBits);

    var (sm, p2) = this._Decompose();
    return _Recompose(sm, p2, (byte)mantissaBits);
  }

  #endregion

  #region FromMemory / ToMemory

  /// <summary>
  /// Creates a value from a byte span with the specified configuration.
  /// </summary>
  public static ConfigurableFloatingPoint<TStorage> FromMemory(ReadOnlySpan<byte> data, int mantissaBits) {
    _ValidateMantissaBits(mantissaBits);
    var totalBytes = (TotalBits + 7) / 8;
    if (data.Length < totalBytes)
      throw new ArgumentException($"Data must be at least {totalBytes} bytes for {TotalBits}-bit storage.", nameof(data));

    var raw = BigInteger.Zero;
    for (var i = totalBytes - 1; i >= 0; --i)
      raw = (raw << 8) | data[i];

    var maxRaw = (BigInteger.One << TotalBits) - 1;
    raw &= maxRaw;
    return new(raw, (byte)mantissaBits);
  }

  /// <summary>
  /// Writes the raw value to a new byte array.
  /// </summary>
  public byte[] ToMemory() {
    var totalBytes = (TotalBits + 7) / 8;
    var result = new byte[totalBytes];
    var raw = this.RawBits;
    for (var i = 0; i < totalBytes; ++i) {
      result[i] = (byte)(raw & 0xFF);
      raw >>= 8;
    }
    return result;
  }

  /// <summary>
  /// Writes the raw value to the destination span.
  /// </summary>
  /// <returns>The number of bytes written.</returns>
  public int ToMemory(Span<byte> destination) {
    var totalBytes = (TotalBits + 7) / 8;
    if (destination.Length < totalBytes)
      throw new ArgumentException($"Destination must be at least {totalBytes} bytes.", nameof(destination));

    var raw = this.RawBits;
    for (var i = 0; i < totalBytes; ++i) {
      destination[i] = (byte)(raw & 0xFF);
      raw >>= 8;
    }
    return totalBytes;
  }

  #endregion

  #region Arithmetic Operations

  /// <summary>Adds two values. Cross-config: left's config determines result.</summary>
  public static ConfigurableFloatingPoint<TStorage> Add(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left) || IsNaN(right)) return NaN;
    if (IsInfinity(left) || IsInfinity(right)) {
      if (IsPositiveInfinity(left) && IsNegativeInfinity(right)) return NaN;
      if (IsNegativeInfinity(left) && IsPositiveInfinity(right)) return NaN;
      if (IsInfinity(left)) return IsPositiveInfinity(left) ? PositiveInfinity : NegativeInfinity;
      return IsPositiveInfinity(right) ? PositiveInfinity : NegativeInfinity;
    }
    if (IsZero(left) && IsZero(right)) return new(BigInteger.Zero, config);
    if (IsZero(left)) return _ConvertToConfig(right, config);
    if (IsZero(right)) return left;

    if (left._EffectiveMantissaBits == right._EffectiveMantissaBits && left._EffectiveMantissaBits <= 52) {
      var dResult = left.ToDouble() + right.ToDouble();
      if (!HasSign && dResult < 0)
        return new(BigInteger.Zero, config);
      return FromDouble(dResult, config);
    }

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right._Decompose();
    var minP = Math.Min(p1, p2);
    var result = (m1 << (p1 - minP)) + (m2 << (p2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(result, minP, config);
  }

  /// <summary>Subtracts two values. Cross-config: left's config determines result.</summary>
  public static ConfigurableFloatingPoint<TStorage> Subtract(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left) || IsNaN(right)) return NaN;
    if (IsInfinity(left) && IsInfinity(right)) {
      if (IsPositiveInfinity(left) && IsPositiveInfinity(right)) return NaN;
      if (IsNegativeInfinity(left) && IsNegativeInfinity(right)) return NaN;
    }
    if (IsInfinity(left)) return IsPositiveInfinity(left) ? PositiveInfinity : NegativeInfinity;
    if (IsInfinity(right)) return HasSign ? (IsPositiveInfinity(right) ? NegativeInfinity : PositiveInfinity) : new(BigInteger.Zero, config);
    if (IsZero(right)) return left;

    if (left._EffectiveMantissaBits == right._EffectiveMantissaBits && left._EffectiveMantissaBits <= 52) {
      var dResult = left.ToDouble() - right.ToDouble();
      if (!HasSign && dResult < 0)
        return new(BigInteger.Zero, config);
      return FromDouble(dResult, config);
    }

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right._Decompose();
    var minP = Math.Min(p1, p2);
    var result = (m1 << (p1 - minP)) - (m2 << (p2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(result, minP, config);
  }

  /// <summary>Multiplies two values. Cross-config: left's config determines result.</summary>
  public static ConfigurableFloatingPoint<TStorage> Multiply(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left) || IsNaN(right)) return NaN;
    if (IsInfinity(left) || IsInfinity(right)) {
      if (IsZero(left) || IsZero(right)) return NaN;
      var resultNegative = IsNegative(left) != IsNegative(right);
      return resultNegative && HasSign ? NegativeInfinity : PositiveInfinity;
    }
    if (IsZero(left) || IsZero(right)) return new(BigInteger.Zero, config);

    if (left._EffectiveMantissaBits == right._EffectiveMantissaBits && left._EffectiveMantissaBits <= 52) {
      var dResult = left.ToDouble() * right.ToDouble();
      if (!HasSign && dResult < 0)
        return new(BigInteger.Zero, config);
      return FromDouble(dResult, config);
    }

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right._Decompose();
    var result = m1 * m2;
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(result, p1 + p2, config);
  }

  /// <summary>Divides two values. Cross-config: left's config determines result.</summary>
  public static ConfigurableFloatingPoint<TStorage> Divide(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left) || IsNaN(right)) return NaN;
    if (IsInfinity(left) && IsInfinity(right)) return NaN;
    if (IsZero(left) && IsZero(right)) return NaN;
    if (IsInfinity(left)) {
      var resultNegative = IsNegative(left) != IsNegative(right);
      return resultNegative && HasSign ? NegativeInfinity : PositiveInfinity;
    }
    if (IsInfinity(right)) return new(BigInteger.Zero, config);
    if (IsZero(right)) {
      if (IsZero(left)) return NaN;
      var resultNegative = IsNegative(left) != IsNegative(right);
      return resultNegative && HasSign ? NegativeInfinity : PositiveInfinity;
    }

    if (left._EffectiveMantissaBits == right._EffectiveMantissaBits && left._EffectiveMantissaBits <= 52) {
      var dResult = left.ToDouble() / right.ToDouble();
      if (!HasSign && dResult < 0)
        return new(BigInteger.Zero, config);
      return FromDouble(dResult, config);
    }

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right._Decompose();
    var extraBits = (int)config + 3;
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
    return _Recompose(quotient, p1 - p2 - extraBits, config);
  }

  /// <summary>Negates a value.</summary>
  public static ConfigurableFloatingPoint<TStorage> Negate(ConfigurableFloatingPoint<TStorage> value) {
    if (!HasSign)
      return new(BigInteger.Zero, value._EffectiveMantissaBits);
    if (IsNaN(value)) return NaN;
    if (IsPositiveInfinity(value)) return NegativeInfinity;
    if (IsNegativeInfinity(value)) return PositiveInfinity;
    return new(value.RawBits ^ value._SignMask, value._EffectiveMantissaBits);
  }

  /// <summary>Returns the absolute value.</summary>
  public static ConfigurableFloatingPoint<TStorage> Abs(ConfigurableFloatingPoint<TStorage> value) {
    if (!HasSign) return value;
    if (IsNaN(value)) return NaN;
    if (IsInfinity(value)) return PositiveInfinity;
    return new(value.RawBits & ~value._SignMask, value._EffectiveMantissaBits);
  }

  /// <summary>Computes the modulo (remainder) of division. Cross-config: left's config determines result.</summary>
  public static ConfigurableFloatingPoint<TStorage> Modulo(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left) || IsNaN(right) || IsInfinity(left) || IsZero(right))
      return NaN;
    if (IsInfinity(right) || IsZero(left))
      return left;

    if (left._EffectiveMantissaBits == right._EffectiveMantissaBits && left._EffectiveMantissaBits <= 52) {
      var dResult = left.ToDouble() % right.ToDouble();
      if (!HasSign && dResult < 0)
        return new(BigInteger.Zero, config);
      return FromDouble(dResult, config);
    }

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right._Decompose();
    var minP = Math.Min(p1, p2);
    var result = (m1 << (p1 - minP)) % (m2 << (p2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(result, minP, config);
  }

  /// <summary>Converts a value to the target config. Handles specials safely.</summary>
  private static ConfigurableFloatingPoint<TStorage> _ConvertToConfig(ConfigurableFloatingPoint<TStorage> source, byte targetConfig) {
    if (source._EffectiveMantissaBits == targetConfig)
      return source;
    if (IsNaN(source)) return NaN;
    if (IsPositiveInfinity(source)) return PositiveInfinity;
    if (IsNegativeInfinity(source)) return NegativeInfinity;
    if (IsZero(source))
      return new(BigInteger.Zero, targetConfig);

    var (sm, p2) = source._Decompose();
    return _Recompose(sm, p2, targetConfig);
  }

  #endregion

  #region Math Helpers

  /// <summary>Returns the smaller of two values. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFloatingPoint<TStorage> Min(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    if (IsNaN(left) || IsNaN(right)) return NaN;
    if (left.CompareTo(right) <= 0)
      return left;
    return _ConvertToConfig(right, left._EffectiveMantissaBits);
  }

  /// <summary>Returns the larger of two values. Cross-config: left's config determines result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ConfigurableFloatingPoint<TStorage> Max(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) {
    if (IsNaN(left) || IsNaN(right)) return NaN;
    if (left.CompareTo(right) >= 0)
      return left;
    return _ConvertToConfig(right, left._EffectiveMantissaBits);
  }

  /// <summary>Clamps a value to the specified range. Result in value's config.</summary>
  public static ConfigurableFloatingPoint<TStorage> Clamp(ConfigurableFloatingPoint<TStorage> value, ConfigurableFloatingPoint<TStorage> min, ConfigurableFloatingPoint<TStorage> max) {
    if (IsNaN(value) || IsNaN(min) || IsNaN(max)) return NaN;
    if (value.CompareTo(min) < 0) return _ConvertToConfig(min, value._EffectiveMantissaBits);
    if (value.CompareTo(max) > 0) return _ConvertToConfig(max, value._EffectiveMantissaBits);
    return value;
  }

  /// <summary>Copies the sign from one value to another. Result in magnitude's config.</summary>
  public static ConfigurableFloatingPoint<TStorage> CopySign(ConfigurableFloatingPoint<TStorage> magnitude, ConfigurableFloatingPoint<TStorage> sign) {
    if (!HasSign) return magnitude;
    var magnitudeBits = magnitude.RawBits & ~magnitude._SignMask;
    if (IsNegative(sign))
      magnitudeBits |= magnitude._SignMask;
    return new(magnitudeBits, magnitude._EffectiveMantissaBits);
  }

  #endregion

  #region Comparison

  public int CompareTo(ConfigurableFloatingPoint<TStorage> other) {
    if (IsNaN(this)) return IsNaN(other) ? 0 : 1;
    if (IsNaN(other)) return -1;
    if (IsZero(this) && IsZero(other)) return 0;
    var thisNeg = IsNegative(this);
    var otherNeg = IsNegative(other);
    if (thisNeg != otherNeg) return thisNeg ? -1 : 1;
    if (IsPositiveInfinity(this)) return IsPositiveInfinity(other) ? 0 : 1;
    if (IsPositiveInfinity(other)) return -1;
    if (IsNegativeInfinity(this)) return IsNegativeInfinity(other) ? 0 : -1;
    if (IsNegativeInfinity(other)) return 1;
    if (IsZero(this)) return otherNeg ? 1 : -1;
    if (IsZero(other)) return thisNeg ? -1 : 1;

    var (m1, p1) = this._Decompose();
    var (m2, p2) = other._Decompose();
    var minP = Math.Min(p1, p2);
    return (m1 << (p1 - minP)).CompareTo(m2 << (p2 - minP));
  }

  /// <summary>Cross-type comparison with fixed-point.</summary>
  public int CompareTo(ConfigurableFixedPoint<TStorage> other) {
    if (IsNaN(this)) return 1;
    if (IsPositiveInfinity(this)) return 1;
    if (IsNegativeInfinity(this)) return -1;
    if (IsZero(this) && other.RawBits == 0) return 0;

    if (IsZero(this))
      return other.RawBits > 0 ? -1 : other.RawBits < 0 ? 1 : 0;

    var (m1, p1) = this._Decompose();
    var (m2, p2) = other.DecomposeExact();
    var minP = Math.Min(p1, p2);
    return (m1 << (p1 - minP)).CompareTo(m2 << (p2 - minP));
  }

  public int CompareTo(object? obj) {
    if (obj is null) return 1;
    if (obj is ConfigurableFloatingPoint<TStorage> other)
      return this.CompareTo(other);
    if (obj is ConfigurableFixedPoint<TStorage> fixedOther)
      return this.CompareTo(fixedOther);
    throw new ArgumentException($"Object must be of type {nameof(ConfigurableFloatingPoint<TStorage>)}.", nameof(obj));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ConfigurableFloatingPoint<TStorage> other) {
    if (IsNaN(this) && IsNaN(other)) return true;
    if (IsNaN(this) || IsNaN(other)) return false;
    if (IsZero(this) && IsZero(other)) return true;
    if (IsInfinity(this) || IsInfinity(other))
      return IsPositiveInfinity(this) == IsPositiveInfinity(other)
        && IsNegativeInfinity(this) == IsNegativeInfinity(other);
    if (this._EffectiveMantissaBits == other._EffectiveMantissaBits)
      return this._rawValue.Equals(other._rawValue);

    var (m1, p1) = this._Decompose();
    var (m2, p2) = other._Decompose();
    var minP = Math.Min(p1, p2);
    return (m1 << (p1 - minP)) == (m2 << (p2 - minP));
  }

  public override bool Equals(object? obj) =>
    obj is ConfigurableFloatingPoint<TStorage> other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    if (IsNaN(this)) return int.MinValue;
    if (IsZero(this)) return 0;
    if (IsPositiveInfinity(this)) return int.MaxValue;
    if (IsNegativeInfinity(this)) return int.MaxValue - 1;
    var (m, p) = this._Decompose();
    while (m != 0 && m.IsEven) {
      m >>= 1;
      ++p;
    }
    return HashCode.Combine(m, p);
  }

  #endregion

  #region Operators

  public static bool operator ==(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) => left.Equals(right);
  public static bool operator !=(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) => !left.Equals(right);
  public static bool operator <(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) =>
    !IsNaN(left) && !IsNaN(right) && left.CompareTo(right) < 0;
  public static bool operator >(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) =>
    !IsNaN(left) && !IsNaN(right) && left.CompareTo(right) > 0;
  public static bool operator <=(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) =>
    !IsNaN(left) && !IsNaN(right) && left.CompareTo(right) <= 0;
  public static bool operator >=(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) =>
    !IsNaN(left) && !IsNaN(right) && left.CompareTo(right) >= 0;

  public static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) => Add(left, right);
  public static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) => Subtract(left, right);
  public static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) => Multiply(left, right);
  public static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) => Divide(left, right);
  public static ConfigurableFloatingPoint<TStorage> operator %(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right) => Modulo(left, right);
  public static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> value) => Negate(value);
  public static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> value) => value;

  public static ConfigurableFloatingPoint<TStorage> operator ++(ConfigurableFloatingPoint<TStorage> value) => Add(value, value.AsOne);
  public static ConfigurableFloatingPoint<TStorage> operator --(ConfigurableFloatingPoint<TStorage> value) => Subtract(value, value.AsOne);

  #endregion

  #region Mixed-Type Arithmetic (with double)

  public static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, double right) => left.CreateFromDouble(left.ToDouble() + right);
  public static ConfigurableFloatingPoint<TStorage> operator +(double left, ConfigurableFloatingPoint<TStorage> right) => right.CreateFromDouble(left + right.ToDouble());
  public static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, double right) {
    var result = left.ToDouble() - right;
    return !HasSign && result < 0 ? new(BigInteger.Zero, left._EffectiveMantissaBits) : left.CreateFromDouble(result);
  }
  public static ConfigurableFloatingPoint<TStorage> operator -(double left, ConfigurableFloatingPoint<TStorage> right) {
    var result = left - right.ToDouble();
    return !HasSign && result < 0 ? new(BigInteger.Zero, right._EffectiveMantissaBits) : right.CreateFromDouble(result);
  }
  public static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, double right) => left.CreateFromDouble(left.ToDouble() * right);
  public static ConfigurableFloatingPoint<TStorage> operator *(double left, ConfigurableFloatingPoint<TStorage> right) => right.CreateFromDouble(left * right.ToDouble());
  public static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, double right) => left.CreateFromDouble(left.ToDouble() / right);
  public static ConfigurableFloatingPoint<TStorage> operator /(double left, ConfigurableFloatingPoint<TStorage> right) => right.CreateFromDouble(left / right.ToDouble());

  public static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, int right) => left.CreateFromDouble(left.ToDouble() + right);
  public static ConfigurableFloatingPoint<TStorage> operator +(int left, ConfigurableFloatingPoint<TStorage> right) => right.CreateFromDouble(left + right.ToDouble());
  public static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, int right) {
    var result = left.ToDouble() - right;
    return !HasSign && result < 0 ? new(BigInteger.Zero, left._EffectiveMantissaBits) : left.CreateFromDouble(result);
  }
  public static ConfigurableFloatingPoint<TStorage> operator -(int left, ConfigurableFloatingPoint<TStorage> right) {
    var result = left - right.ToDouble();
    return !HasSign && result < 0 ? new(BigInteger.Zero, right._EffectiveMantissaBits) : right.CreateFromDouble(result);
  }
  public static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, int right) => left.CreateFromDouble(left.ToDouble() * right);
  public static ConfigurableFloatingPoint<TStorage> operator *(int left, ConfigurableFloatingPoint<TStorage> right) => right.CreateFromDouble(left * right.ToDouble());
  public static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, int right) => left.CreateFromDouble(left.ToDouble() / right);
  public static ConfigurableFloatingPoint<TStorage> operator /(int left, ConfigurableFloatingPoint<TStorage> right) => right.CreateFromDouble(left / right.ToDouble());

  #endregion

  #region Cross-Type Arithmetic (with ConfigurableFixedPoint)

  public static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left)) return NaN;
    if (IsInfinity(left)) return IsPositiveInfinity(left) ? PositiveInfinity : NegativeInfinity;
    if (IsZero(left) && right.RawBits == 0) return new(BigInteger.Zero, config);
    if (right.RawBits == 0) return left;

    if (IsZero(left)) {
      var (m2, p2) = right.DecomposeExact();
      return _Recompose(m2, p2, config);
    }

    var (m1, p1) = left._Decompose();
    var (rm2, rp2) = right.DecomposeExact();
    var minP = Math.Min(p1, rp2);
    var result = (m1 << (p1 - minP)) + (rm2 << (rp2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(result, minP, config);
  }

  public static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left)) return NaN;
    if (IsInfinity(left)) return IsPositiveInfinity(left) ? PositiveInfinity : NegativeInfinity;
    if (right.RawBits == 0) return left;

    if (IsZero(left)) {
      var (m2, p2) = right.DecomposeExact();
      var result = _Recompose(-m2, p2, config);
      if (!HasSign && IsNegative(result))
        return new(BigInteger.Zero, config);
      return result;
    }

    var (m1, p1) = left._Decompose();
    var (rm2, rp2) = right.DecomposeExact();
    var minP = Math.Min(p1, rp2);
    var res = (m1 << (p1 - minP)) - (rm2 << (rp2 - minP));
    if (!HasSign && res < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(res, minP, config);
  }

  public static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left)) return NaN;
    if (IsInfinity(left)) {
      if (right.RawBits == 0) return NaN;
      var resultNeg = IsNegative(left) != (right.RawBits < 0);
      return resultNeg && HasSign ? NegativeInfinity : PositiveInfinity;
    }
    if (IsZero(left) || right.RawBits == 0) return new(BigInteger.Zero, config);

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right.DecomposeExact();
    var result = m1 * m2;
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(result, p1 + p2, config);
  }

  public static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left)) return NaN;
    if (right.RawBits == 0) {
      if (IsZero(left)) return NaN;
      var resultNeg = IsNegative(left) != (right.RawBits < 0);
      return resultNeg && HasSign ? NegativeInfinity : PositiveInfinity;
    }
    if (IsInfinity(left)) {
      var resultNeg = IsNegative(left) != (right.RawBits < 0);
      return resultNeg && HasSign ? NegativeInfinity : PositiveInfinity;
    }
    if (IsZero(left)) return new(BigInteger.Zero, config);

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right.DecomposeExact();
    var extraBits = (int)config + 3;
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
    return _Recompose(quotient, p1 - p2 - extraBits, config);
  }

  public static ConfigurableFloatingPoint<TStorage> operator %(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right) {
    var config = left._EffectiveMantissaBits;
    if (IsNaN(left) || IsInfinity(left) || right.RawBits == 0)
      return NaN;
    if (IsZero(left))
      return left;

    var (m1, p1) = left._Decompose();
    var (m2, p2) = right.DecomposeExact();
    var minP = Math.Min(p1, p2);
    var result = (m1 << (p1 - minP)) % (m2 << (p2 - minP));
    if (!HasSign && result < 0)
      return new(BigInteger.Zero, config);
    return _Recompose(result, minP, config);
  }

  #endregion

  #region Type Conversions

  public static explicit operator double(ConfigurableFloatingPoint<TStorage> value) => value.ToDouble();
  public static explicit operator float(ConfigurableFloatingPoint<TStorage> value) => value.ToSingle();
  public static explicit operator decimal(ConfigurableFloatingPoint<TStorage> value) => value.ToDecimal();
  public static explicit operator Half(ConfigurableFloatingPoint<TStorage> value) => value.ToHalf();
  public static explicit operator Quarter(ConfigurableFloatingPoint<TStorage> value) => value.ToQuarter();
  public static explicit operator E4M3(ConfigurableFloatingPoint<TStorage> value) => value.ToE4M3();
  public static explicit operator BFloat8(ConfigurableFloatingPoint<TStorage> value) => value.ToBFloat8();
  public static explicit operator BFloat16(ConfigurableFloatingPoint<TStorage> value) => value.ToBFloat16();
  public static explicit operator BFloat32(ConfigurableFloatingPoint<TStorage> value) => value.ToBFloat32();
  public static explicit operator BFloat64(ConfigurableFloatingPoint<TStorage> value) => value.ToBFloat64();

  public static explicit operator byte(ConfigurableFloatingPoint<TStorage> value) => (byte)value.ToDouble();
  public static explicit operator sbyte(ConfigurableFloatingPoint<TStorage> value) => (sbyte)value.ToDouble();
  public static explicit operator short(ConfigurableFloatingPoint<TStorage> value) => (short)value.ToDouble();
  public static explicit operator ushort(ConfigurableFloatingPoint<TStorage> value) => (ushort)value.ToDouble();
  public static explicit operator int(ConfigurableFloatingPoint<TStorage> value) => (int)value.ToDouble();
  public static explicit operator uint(ConfigurableFloatingPoint<TStorage> value) => (uint)value.ToDouble();
  public static explicit operator long(ConfigurableFloatingPoint<TStorage> value) => (long)value.ToDouble();
  public static explicit operator ulong(ConfigurableFloatingPoint<TStorage> value) => (ulong)value.ToDouble();

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

  public static ConfigurableFloatingPoint<TStorage> Parse(string s) =>
    Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, null);

  public static ConfigurableFloatingPoint<TStorage> Parse(string s, IFormatProvider? provider) =>
    Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);

  public static ConfigurableFloatingPoint<TStorage> Parse(string s, NumberStyles style, IFormatProvider? provider) {
    var value = double.Parse(s, style, provider);
    var m = _GetDefaultBitLayout(TotalBits, HasSign).mantissa;
    return FromDouble(value, m);
  }

  public static bool TryParse(string? s, out ConfigurableFloatingPoint<TStorage> result) =>
    TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);

  public static bool TryParse(string? s, IFormatProvider? provider, out ConfigurableFloatingPoint<TStorage> result) =>
    TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out ConfigurableFloatingPoint<TStorage> result) {
    if (double.TryParse(s, style, provider, out var value)) {
      var m = _GetDefaultBitLayout(TotalBits, HasSign).mantissa;
      result = FromDouble(value, m);
      return true;
    }
    result = default;
    return false;
  }

  public static ConfigurableFloatingPoint<TStorage> Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
    var value = double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
    var m = _GetDefaultBitLayout(TotalBits, HasSign).mantissa;
    return FromDouble(value, m);
  }

  public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ConfigurableFloatingPoint<TStorage> result) {
    if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var value)) {
      var m = _GetDefaultBitLayout(TotalBits, HasSign).mantissa;
      result = FromDouble(value, m);
      return true;
    }
    result = default;
    return false;
  }

  #endregion

  #region Validation

  private static void _ValidateMantissaBits(int mantissaBits) {
    var signBit = HasSign ? 1 : 0;
    if (mantissaBits < 1)
      throw new ArgumentOutOfRangeException(nameof(mantissaBits), "Mantissa must have at least 1 bit.");
    if (TotalBits - signBit - mantissaBits < 1)
      throw new ArgumentOutOfRangeException(nameof(mantissaBits), $"Mantissa bits ({mantissaBits}) leaves no room for exponent in {TotalBits}-bit storage.");
  }

  #endregion

  #region Helper Methods

  private static BigInteger _ConvertToBigInteger(TStorage value) {
    return value switch {
      byte b => b,
      sbyte sb => sb < 0 ? (BigInteger)(byte)sb : sb,
      ushort us => us,
      short s => s < 0 ? (BigInteger)(ushort)s : s,
      uint ui => ui,
      int i => i < 0 ? (BigInteger)(uint)i : i,
      ulong ul => ul,
      long l => l < 0 ? (BigInteger)(ulong)l : l,
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
    var upper = value.Upper;
    var lower = value.Lower;
    return (new BigInteger(upper) << 64) | new BigInteger(lower);
  }

  private static UInt96 _BigIntegerToUInt96(BigInteger value) {
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (uint)((value >> 64) & uint.MaxValue);
    return new UInt96(upper, lower);
  }

  private static Int96 _BigIntegerToInt96(BigInteger value) {
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (uint)((value >> 64) & uint.MaxValue);
    return new Int96(upper, lower);
  }

  private static BigInteger _UInt128ToBigInteger(UInt128 value) {
    var upper = (ulong)(value >> 64);
    var lower = (ulong)value;
    return (new BigInteger(upper) << 64) | new BigInteger(lower);
  }

  private static BigInteger _Int128ToBigInteger(Int128 value) {
    var upper = (ulong)((UInt128)value >> 64);
    var lower = (ulong)(UInt128)value;
    return (new BigInteger(upper) << 64) | new BigInteger(lower);
  }

  private static UInt128 _BigIntegerToUInt128(BigInteger value) {
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (ulong)((value >> 64) & ulong.MaxValue);
    return new UInt128(upper, lower);
  }

  private static Int128 _BigIntegerToInt128(BigInteger value) {
    var lower = (ulong)(value & ulong.MaxValue);
    var upper = (ulong)((value >> 64) & ulong.MaxValue);
    return new Int128(upper, lower);
  }

  private static TStorage _ConvertFromBigInteger(BigInteger value) {
    var maxRaw = (BigInteger.One << TotalBits) - 1;
    value &= maxRaw;
    if (typeof(TStorage) == typeof(byte)) { var r = (byte)value; return Unsafe.As<byte, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(sbyte)) { var r = (sbyte)(byte)value; return Unsafe.As<sbyte, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(ushort)) { var r = (ushort)value; return Unsafe.As<ushort, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(short)) { var r = (short)(ushort)value; return Unsafe.As<short, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(uint)) { var r = (uint)value; return Unsafe.As<uint, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(int)) { var r = (int)(uint)value; return Unsafe.As<int, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(ulong)) { var r = (ulong)value; return Unsafe.As<ulong, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(long)) { var r = (long)(ulong)value; return Unsafe.As<long, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(UInt96)) { var r = _BigIntegerToUInt96(value); return Unsafe.As<UInt96, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(Int96)) { var r = _BigIntegerToInt96(value); return Unsafe.As<Int96, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(UInt128)) { var r = _BigIntegerToUInt128(value); return Unsafe.As<UInt128, TStorage>(ref r); }
    if (typeof(TStorage) == typeof(Int128)) { var r = _BigIntegerToInt128(value); return Unsafe.As<Int128, TStorage>(ref r); }
    throw new NotSupportedException($"Storage type {typeof(TStorage).Name} is not supported.");
  }

  private static int _BitLength(BigInteger value) {
    if (value <= 0) return 0;
    var length = 0;
    while (value > 0) {
      ++length;
      value >>= 1;
    }
    return length;
  }

  #endregion

}
