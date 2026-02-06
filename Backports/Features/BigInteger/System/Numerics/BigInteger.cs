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

#if !SUPPORTS_BIG_INTEGER

using System.Globalization;
using System.Text;

namespace System.Numerics;

/// <summary>
/// Represents an arbitrarily large signed integer.
/// </summary>
[Serializable]
public readonly struct BigInteger : IComparable, IComparable<BigInteger>, IEquatable<BigInteger>, IFormattable {

  // Internal representation: sign and magnitude
  // _sign: 0 = zero, 1 = positive, -1 = negative
  // _bits: magnitude stored as uint[] in little-endian order (least significant first)
  private readonly int _sign;
  private readonly uint[]? _bits;

  #region Static Properties

  /// <summary>Gets a value that represents the number 0 (zero).</summary>
  public static BigInteger Zero { get; } = new(0);

  /// <summary>Gets a value that represents the number one (1).</summary>
  public static BigInteger One { get; } = new(1);

  /// <summary>Gets a value that represents the number negative one (-1).</summary>
  public static BigInteger MinusOne { get; } = new(-1);

  #endregion

  #region Instance Properties

  /// <summary>Gets a value indicating whether the current BigInteger is zero.</summary>
  public bool IsZero => this._sign == 0;

  /// <summary>Gets a value indicating whether the current BigInteger is one.</summary>
  public bool IsOne => this._sign == 1 && this._bits == null;

  /// <summary>Gets a value indicating whether the current BigInteger is even.</summary>
  public bool IsEven => this._bits == null ? (this._sign & 1) == 0 : (this._bits[0] & 1) == 0;

  /// <summary>Gets a value indicating whether the current BigInteger is a power of two.</summary>
  public bool IsPowerOfTwo {
    get {
      if (this._sign <= 0)
        return false;

      if (this._bits == null)
        return (this._sign & (this._sign - 1)) == 0;

      // Check if only one bit is set across all uint values
      var foundOne = false;
      foreach (var b in this._bits) {
        if (b == 0)
          continue;

        if (foundOne || (b & (b - 1)) != 0)
          return false;

        foundOne = true;
      }
      return foundOne;
    }
  }

  /// <summary>Gets a number indicating the sign of the BigInteger.</summary>
  public int Sign => this._sign;

  #endregion

  #region Constructors

  /// <summary>Initializes a new instance of BigInteger using an Int32 value.</summary>
  public BigInteger(int value) {
    if (value == 0) {
      this._sign = 0;
      this._bits = null;
    } else if (value > 0) {
      this._sign = 1;
      this._bits = value == 1 ? null : [(uint)value];
    } else if (value == int.MinValue) {
      this._sign = -1;
      this._bits = [0x80000000];
    } else {
      this._sign = -1;
      this._bits = [(uint)(-value)];
    }
  }

  /// <summary>Initializes a new instance of BigInteger using an Int64 value.</summary>
  public BigInteger(long value) {
    if (value == 0) {
      this._sign = 0;
      this._bits = null;
    } else if (value > 0) {
      this._sign = 1;
      if (value <= uint.MaxValue)
        this._bits = value == 1 ? null : [(uint)value];
      else
        this._bits = [(uint)value, (uint)(value >> 32)];
    } else if (value == long.MinValue) {
      this._sign = -1;
      this._bits = [0, 0x80000000];
    } else {
      this._sign = -1;
      var absValue = (ulong)(-value);
      if (absValue <= uint.MaxValue)
        this._bits = [(uint)absValue];
      else
        this._bits = [(uint)absValue, (uint)(absValue >> 32)];
    }
  }

  /// <summary>Initializes a new instance of BigInteger using a UInt32 value.</summary>
  [CLSCompliant(false)]
  public BigInteger(uint value) {
    if (value == 0) {
      this._sign = 0;
      this._bits = null;
    } else {
      this._sign = 1;
      this._bits = value == 1 ? null : [value];
    }
  }

  /// <summary>Initializes a new instance of BigInteger using a UInt64 value.</summary>
  [CLSCompliant(false)]
  public BigInteger(ulong value) {
    if (value == 0) {
      this._sign = 0;
      this._bits = null;
    } else {
      this._sign = 1;
      if (value <= uint.MaxValue)
        this._bits = value == 1 ? null : [(uint)value];
      else
        this._bits = [(uint)value, (uint)(value >> 32)];
    }
  }

  /// <summary>Initializes a new instance of BigInteger using a byte array.</summary>
  public BigInteger(byte[] value) {
    if (value == null)
      throw new ArgumentNullException(nameof(value));

    if (value.Length == 0) {
      this._sign = 0;
      this._bits = null;
      return;
    }

    // Determine sign from most significant byte
    var isNegative = (value[^1] & 0x80) != 0;

    // Convert from little-endian two's complement
    if (isNegative) {
      // Convert from two's complement
      var magnitude = new byte[value.Length];
      var carry = true;
      for (var i = 0; i < value.Length; ++i) {
        var b = (byte)~value[i];
        if (carry) {
          if (b == 255) {
            magnitude[i] = 0;
          } else {
            magnitude[i] = (byte)(b + 1);
            carry = false;
          }
        } else {
          magnitude[i] = b;
        }
      }

      this._bits = _BytesToUints(magnitude);
      // _bits == null means magnitude is 1 (special representation), sign should be -1
      this._sign = -1;
    } else {
      this._bits = _BytesToUints(value);
      // _bits == null can mean either 0 or 1; need to check if input was non-zero
      var isZero = true;
      foreach (var b in value) {
        if (b != 0) {
          isZero = false;
          break;
        }
      }
      this._sign = isZero ? 0 : 1;
    }
  }

  /// <summary>Initializes a new instance of BigInteger using a Double value.</summary>
  public BigInteger(double value) {
    if (double.IsInfinity(value) || double.IsNaN(value))
      throw new OverflowException("Value is not a finite number.");

    if (value == 0) {
      this._sign = 0;
      this._bits = null;
      return;
    }

    var isNegative = value < 0;
    value = Math.Abs(value);
    value = Math.Floor(value);

    if (value <= uint.MaxValue) {
      this._sign = isNegative ? -1 : 1;
      var uval = (uint)value;
      this._bits = uval == 1 && !isNegative ? null : [uval];
      return;
    }

    // Extract bits from double
    var bits = BitConverter.DoubleToInt64Bits(value);
    var exponent = (int)((bits >> 52) & 0x7FF) - 1023;
    var mantissa = (bits & 0xFFFFFFFFFFFFF) | 0x10000000000000;

    // Calculate how many uint values we need
    var shift = exponent - 52;
    if (shift >= 0) {
      var result = new BigInteger(mantissa);
      for (var i = 0; i < shift; ++i)
        result *= 2;

      this._sign = isNegative ? -result._sign : result._sign;
      this._bits = result._bits;
    } else {
      var result = mantissa >> (-shift);
      this._sign = isNegative ? -1 : 1;
      this._bits = result <= 1 && !isNegative ? null : [(uint)result, (uint)(result >> 32)];
      this._bits = _TrimLeadingZeros(this._bits);
    }
  }

  /// <summary>Initializes a new instance of BigInteger using a Decimal value.</summary>
  public BigInteger(decimal value) {
    var bits = decimal.GetBits(value);
    var isNegative = (bits[3] & 0x80000000) != 0;
    var scale = (bits[3] >> 16) & 0x7F;

    // Build the unscaled value
    var result = new BigInteger((uint)bits[0]) +
                 new BigInteger((uint)bits[1]) * new BigInteger(0x100000000L) +
                 new BigInteger((uint)bits[2]) * new BigInteger(0x100000000L) * new BigInteger(0x100000000L);

    // Apply scale (divide by 10^scale)
    for (var i = 0; i < scale; ++i)
      result /= 10;

    this._sign = isNegative ? -result._sign : result._sign;
    this._bits = result._bits;
  }

  /// <summary>Initializes a new instance of BigInteger using a Single value.</summary>
  public BigInteger(float value) : this((double)value) { }

  // Private constructor for internal use
  private BigInteger(int sign, uint[]? bits) {
    this._sign = sign;
    this._bits = bits;
  }

  #endregion

  #region Comparison Methods

  /// <inheritdoc />
  public int CompareTo(object? obj) {
    if (obj == null)
      return 1;
    if (obj is not BigInteger other)
      throw new ArgumentException("Object must be of type BigInteger.");
    return this.CompareTo(other);
  }

  /// <inheritdoc />
  public int CompareTo(BigInteger other) {
    if (this._sign != other._sign)
      return this._sign.CompareTo(other._sign);

    if (this._sign == 0)
      return 0;

    var comparison = _CompareMagnitude(this._bits, other._bits);
    return this._sign > 0 ? comparison : -comparison;
  }

  /// <summary>Compares this instance to an Int64 value.</summary>
  public int CompareTo(long other) => this.CompareTo(new BigInteger(other));

  /// <summary>Compares this instance to a UInt64 value.</summary>
  [CLSCompliant(false)]
  public int CompareTo(ulong other) => this.CompareTo(new BigInteger(other));

  /// <summary>Compares two BigInteger values.</summary>
  public static int Compare(BigInteger left, BigInteger right) => left.CompareTo(right);

  #endregion

  #region Equality Methods

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is BigInteger other && this.Equals(other);

  /// <inheritdoc />
  public bool Equals(BigInteger other) {
    if (this._sign != other._sign)
      return false;
    if (this._sign == 0)
      return true;

    return _CompareMagnitude(this._bits, other._bits) == 0;
  }

  /// <summary>Compares this instance to an Int64 value for equality.</summary>
  public bool Equals(long other) => this.Equals(new BigInteger(other));

  /// <summary>Compares this instance to a UInt64 value for equality.</summary>
  [CLSCompliant(false)]
  public bool Equals(ulong other) => this.Equals(new BigInteger(other));

  /// <inheritdoc />
  public override int GetHashCode() {
    if (this._sign == 0)
      return 0;

    var hash = this._sign;
    if (this._bits != null)
      foreach (var b in this._bits)
        hash = (hash * 397) ^ (int)b;
    else
      hash = (hash * 397) ^ 1;

    return hash;
  }

  #endregion

  #region Arithmetic Methods

  /// <summary>Returns the absolute value of a BigInteger.</summary>
  public static BigInteger Abs(BigInteger value)
    => value._sign >= 0 ? value : new(-value._sign, value._bits);

  /// <summary>Negates a BigInteger value.</summary>
  public static BigInteger Negate(BigInteger value)
    => new(-value._sign, value._bits);

  /// <summary>Adds two BigInteger values.</summary>
  public static BigInteger Add(BigInteger left, BigInteger right) {
    if (left._sign == 0)
      return right;
    if (right._sign == 0)
      return left;

    if (left._sign == right._sign) {
      var result = _AddMagnitudes(left._bits, right._bits);
      return new(left._sign, _TrimLeadingZeros(result));
    }

    var comparison = _CompareMagnitude(left._bits, right._bits);
    if (comparison == 0)
      return Zero;

    if (comparison > 0) {
      var result = _SubtractMagnitudes(left._bits, right._bits);
      return new(left._sign, result);
    } else {
      var result = _SubtractMagnitudes(right._bits, left._bits);
      return new(right._sign, result);
    }
  }

  /// <summary>Subtracts one BigInteger from another.</summary>
  public static BigInteger Subtract(BigInteger left, BigInteger right)
    => Add(left, Negate(right));

  /// <summary>Multiplies two BigInteger values.</summary>
  public static BigInteger Multiply(BigInteger left, BigInteger right) {
    if (left._sign == 0 || right._sign == 0)
      return Zero;

    var resultBits = _MultiplyMagnitudes(left._bits, right._bits);
    var resultSign = left._sign * right._sign;
    return new(resultSign, _TrimLeadingZeros(resultBits));
  }

  /// <summary>Divides one BigInteger by another.</summary>
  public static BigInteger Divide(BigInteger dividend, BigInteger divisor) {
    DivRem(dividend, divisor, out _);
    return DivRem(dividend, divisor).Quotient;
  }

  /// <summary>Returns the remainder of dividing one BigInteger by another.</summary>
  public static BigInteger Remainder(BigInteger dividend, BigInteger divisor) {
    DivRem(dividend, divisor, out var remainder);
    return remainder;
  }

  /// <summary>Divides one BigInteger by another and returns the quotient and remainder.</summary>
  public static (BigInteger Quotient, BigInteger Remainder) DivRem(BigInteger dividend, BigInteger divisor) {
    DivRem(dividend, divisor, out var remainder);
    var quotient = _DivRemCore(dividend, divisor, out _);
    return (quotient, remainder);
  }

  /// <summary>Divides one BigInteger by another and returns the remainder.</summary>
  public static BigInteger DivRem(BigInteger dividend, BigInteger divisor, out BigInteger remainder) {
    if (divisor._sign == 0)
      throw new DivideByZeroException();

    if (dividend._sign == 0) {
      remainder = Zero;
      return Zero;
    }

    return _DivRemCore(dividend, divisor, out remainder);
  }

  private static BigInteger _DivRemCore(BigInteger dividend, BigInteger divisor, out BigInteger remainder) {
    var comparison = _CompareMagnitude(dividend._bits, divisor._bits);

    if (comparison < 0) {
      remainder = dividend;
      return Zero;
    }

    if (comparison == 0) {
      remainder = Zero;
      return dividend._sign == divisor._sign ? One : MinusOne;
    }

    // Simple division for single uint divisor
    if (divisor._bits == null || divisor._bits.Length == 1) {
      var div = divisor._bits == null ? 1u : divisor._bits[0];
      var quotientBits = _DivideSingleDivisor(dividend._bits, div, out var rem);
      remainder = rem == 0 ? Zero : new(dividend._sign, [rem]);
      return new(dividend._sign * divisor._sign, quotientBits);
    }

    // General case - use long division
    var (q, r) = _LongDivide(dividend._bits ?? [1], divisor._bits);
    remainder = new(r == null || r.Length == 0 || r is [0] ? 0 : dividend._sign, _TrimLeadingZeros(r));
    return new(q == null || q.Length == 0 || q is [0] ? 0 : dividend._sign * divisor._sign, _TrimLeadingZeros(q));
  }

  /// <summary>Raises a BigInteger to a power.</summary>
  public static BigInteger Pow(BigInteger value, int exponent) {
    if (exponent < 0)
      throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent must be non-negative.");

    if (exponent == 0)
      return One;
    if (exponent == 1)
      return value;
    if (value._sign == 0)
      return Zero;

    var result = One;
    var @base = value;
    while (exponent > 0) {
      if ((exponent & 1) == 1)
        result *= @base;

      @base *= @base;
      exponent >>= 1;
    }
    return result;
  }

  /// <summary>Performs modular exponentiation.</summary>
  public static BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger modulus) {
    if (exponent._sign < 0)
      throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent must be non-negative.");
    if (modulus._sign == 0)
      throw new DivideByZeroException();

    if (exponent._sign == 0)
      return One;

    var result = One;
    var @base = Remainder(value, modulus);
    while (exponent._sign != 0) {
      if (!exponent.IsEven)
        result = Remainder(result * @base, modulus);

      @base = Remainder(@base * @base, modulus);
      exponent >>= 1;
    }
    return result;
  }

  /// <summary>Finds the greatest common divisor of two BigInteger values.</summary>
  public static BigInteger GreatestCommonDivisor(BigInteger left, BigInteger right) {
    left = Abs(left);
    right = Abs(right);

    while (right._sign != 0) {
      var temp = right;
      right = Remainder(left, right);
      left = temp;
    }
    return left;
  }

  /// <summary>Returns the larger of two BigInteger values.</summary>
  public static BigInteger Max(BigInteger left, BigInteger right)
    => left.CompareTo(right) >= 0 ? left : right;

  /// <summary>Returns the smaller of two BigInteger values.</summary>
  public static BigInteger Min(BigInteger left, BigInteger right)
    => left.CompareTo(right) <= 0 ? left : right;

  /// <summary>Returns the natural logarithm of a BigInteger.</summary>
  public static double Log(BigInteger value)
    => Log(value, Math.E);

  /// <summary>Returns the logarithm of a BigInteger in a specified base.</summary>
  public static double Log(BigInteger value, double baseValue) {
    if (value._sign <= 0)
      return double.NaN;

    // Approximate using double conversion
    return Math.Log((double)value) / Math.Log(baseValue);
  }

  /// <summary>Returns the base 10 logarithm of a BigInteger.</summary>
  public static double Log10(BigInteger value)
    => Log(value, 10);

  #endregion

  #region Bitwise Operations

  /// <summary>Performs a bitwise left shift.</summary>
  public static BigInteger operator <<(BigInteger value, int shift) {
    if (shift == 0 || value._sign == 0)
      return value;

    if (shift < 0)
      return value >> (-shift);

    var wordShift = shift / 32;
    var bitShift = shift % 32;

    var bits = value._bits ?? [1];
    var newBits = new uint[bits.Length + wordShift + 1];

    uint carry = 0;
    for (var i = 0; i < bits.Length; ++i) {
      var newVal = (ulong)bits[i] << bitShift | carry;
      newBits[i + wordShift] = (uint)newVal;
      carry = (uint)(newVal >> 32);
    }
    if (carry != 0)
      newBits[bits.Length + wordShift] = carry;

    return new(value._sign, _TrimLeadingZeros(newBits));
  }

  /// <summary>Performs a bitwise right shift.</summary>
  public static BigInteger operator >>(BigInteger value, int shift) {
    if (shift == 0 || value._sign == 0)
      return value;

    if (shift < 0)
      return value << (-shift);

    var wordShift = shift / 32;
    var bitShift = shift % 32;

    var bits = value._bits ?? [1];
    if (wordShift >= bits.Length)
      return value._sign >= 0 ? Zero : MinusOne;

    var newLength = bits.Length - wordShift;
    var newBits = new uint[newLength];

    for (var i = 0; i < newLength; ++i) {
      newBits[i] = bits[i + wordShift] >> bitShift;
      if (bitShift != 0 && i + wordShift + 1 < bits.Length)
        newBits[i] |= bits[i + wordShift + 1] << (32 - bitShift);
    }

    var result = _TrimLeadingZeros(newBits);
    if (result == null && value._sign < 0)
      return MinusOne;

    if (result == null) {
      // _TrimLeadingZeros returns null for both all-zeros and magnitude-1 ([1] normalized to null).
      // Check original newBits to distinguish.
      for (var i = 0; i < newBits.Length; ++i)
        if (newBits[i] != 0)
          return One;
      return Zero;
    }

    return new(value._sign, result);
  }

  /// <summary>Performs a bitwise AND operation.</summary>
  public static BigInteger operator &(BigInteger left, BigInteger right) {
    if (left._sign == 0 || right._sign == 0)
      return Zero;

    // Convert to two's complement representation for the operation
    var leftBytes = left.ToByteArray();
    var rightBytes = right.ToByteArray();
    var maxLen = Math.Max(leftBytes.Length, rightBytes.Length);

    // Sign extend if needed
    var leftExtend = left._sign < 0 ? (byte)0xFF : (byte)0;
    var rightExtend = right._sign < 0 ? (byte)0xFF : (byte)0;

    // Determine if result should be positive (both non-negative, or result of AND won't have sign bit)
    var resultPositive = left._sign > 0 && right._sign > 0;

    // Allocate extra byte for sign if both operands are positive
    var result = new byte[maxLen + (resultPositive ? 1 : 0)];
    for (var i = 0; i < maxLen; ++i) {
      var l = i < leftBytes.Length ? leftBytes[i] : leftExtend;
      var r = i < rightBytes.Length ? rightBytes[i] : rightExtend;
      result[i] = (byte)(l & r);
    }
    // The extra byte (if added) is already 0x00, ensuring positive interpretation

    return new BigInteger(result);
  }

  /// <summary>Performs a bitwise AND operation.</summary>
  public static BigInteger operator &(BigInteger left, long right) => left & new BigInteger(right);

  /// <summary>Performs a bitwise AND operation.</summary>
  public static BigInteger operator &(BigInteger left, ulong right) => left & new BigInteger(right);

  /// <summary>Performs a bitwise OR operation.</summary>
  public static BigInteger operator |(BigInteger left, BigInteger right) {
    if (left._sign == 0)
      return right;
    if (right._sign == 0)
      return left;

    var leftBytes = left.ToByteArray();
    var rightBytes = right.ToByteArray();
    var maxLen = Math.Max(leftBytes.Length, rightBytes.Length);

    var leftExtend = left._sign < 0 ? (byte)0xFF : (byte)0;
    var rightExtend = right._sign < 0 ? (byte)0xFF : (byte)0;

    // If both operands are positive, result should be positive
    var resultPositive = left._sign > 0 && right._sign > 0;

    var result = new byte[maxLen + (resultPositive ? 1 : 0)];
    for (var i = 0; i < maxLen; ++i) {
      var l = i < leftBytes.Length ? leftBytes[i] : leftExtend;
      var r = i < rightBytes.Length ? rightBytes[i] : rightExtend;
      result[i] = (byte)(l | r);
    }
    // The extra byte (if added) is already 0x00, ensuring positive interpretation

    return new BigInteger(result);
  }

  /// <summary>Performs a bitwise OR operation.</summary>
  public static BigInteger operator |(BigInteger left, int right) => left | new BigInteger(right);

  /// <summary>Performs a bitwise OR operation.</summary>
  public static BigInteger operator |(BigInteger left, long right) => left | new BigInteger(right);

  /// <summary>Performs a bitwise XOR operation.</summary>
  public static BigInteger operator ^(BigInteger left, BigInteger right) {
    if (left._sign == 0)
      return right;
    if (right._sign == 0)
      return left;

    var leftBytes = left.ToByteArray();
    var rightBytes = right.ToByteArray();
    var maxLen = Math.Max(leftBytes.Length, rightBytes.Length);

    var leftExtend = left._sign < 0 ? (byte)0xFF : (byte)0;
    var rightExtend = right._sign < 0 ? (byte)0xFF : (byte)0;

    // If both operands have same sign, result should be positive (XOR of same signs = positive)
    var resultPositive = (left._sign > 0) == (right._sign > 0);

    var result = new byte[maxLen + (resultPositive ? 1 : 0)];
    for (var i = 0; i < maxLen; ++i) {
      var l = i < leftBytes.Length ? leftBytes[i] : leftExtend;
      var r = i < rightBytes.Length ? rightBytes[i] : rightExtend;
      result[i] = (byte)(l ^ r);
    }
    // The extra byte (if added) is already 0x00, ensuring positive interpretation

    return new BigInteger(result);
  }

  /// <summary>Performs a bitwise complement (NOT) operation.</summary>
  public static BigInteger operator ~(BigInteger value) {
    var bytes = value.ToByteArray();
    var result = new byte[bytes.Length];
    for (var i = 0; i < bytes.Length; ++i)
      result[i] = (byte)~bytes[i];

    return new BigInteger(result);
  }

  #endregion

  #region Parsing

  /// <summary>Parses a string into a BigInteger.</summary>
  public static BigInteger Parse(string value)
    => Parse(value, NumberStyles.Integer, null);

  /// <summary>Parses a string into a BigInteger using the specified style.</summary>
  public static BigInteger Parse(string value, NumberStyles style)
    => Parse(value, style, null);

  /// <summary>Parses a string into a BigInteger using the specified format provider.</summary>
  public static BigInteger Parse(string value, IFormatProvider? provider)
    => Parse(value, NumberStyles.Integer, provider);

  /// <summary>Parses a string into a BigInteger using the specified style and format provider.</summary>
  public static BigInteger Parse(string value, NumberStyles style, IFormatProvider? provider) {
    if (!TryParse(value, style, provider, out var result))
      throw new FormatException("The value is not in a valid format.");
    return result;
  }

  /// <summary>Tries to parse a string into a BigInteger.</summary>
  public static bool TryParse(string? value, out BigInteger result)
    => TryParse(value, NumberStyles.Integer, null, out result);

  /// <summary>Tries to parse a string into a BigInteger.</summary>
  public static bool TryParse(string? value, NumberStyles style, IFormatProvider? provider, out BigInteger result) {
    result = Zero;

    if (string.IsNullOrWhiteSpace(value))
      return false;

    value = value!.Trim();
    var isNegative = false;
    var startIndex = 0;

    if (value[0] == '-') {
      isNegative = true;
      startIndex = 1;
    } else if (value[0] == '+') {
      startIndex = 1;
    }

    // Handle hex
    if ((style & NumberStyles.AllowHexSpecifier) != 0)
      return _TryParseHex(value, startIndex, isNegative, out result);

    // Parse decimal
    var current = Zero;
    var ten = new BigInteger(10);
    for (var i = startIndex; i < value.Length; ++i) {
      var c = value[i];
      if (c is < '0' or > '9')
        return false;

      current = current * ten + new BigInteger(c - '0');
    }

    result = isNegative ? Negate(current) : current;
    return true;
  }

  private static bool _TryParseHex(string value, int startIndex, bool isNegative, out BigInteger result) {
    result = Zero;
    var current = Zero;
    var sixteen = new BigInteger(16);

    for (var i = startIndex; i < value.Length; ++i) {
      var c = value[i];
      int digit;
      if (c is >= '0' and <= '9')
        digit = c - '0';
      else if (c is >= 'a' and <= 'f')
        digit = c - 'a' + 10;
      else if (c is >= 'A' and <= 'F')
        digit = c - 'A' + 10;
      else
        return false;

      current = current * sixteen + new BigInteger(digit);
    }

    result = isNegative ? Negate(current) : current;
    return true;
  }

  #endregion

  #region Conversion Methods

  /// <inheritdoc />
  public override string ToString()
    => this.ToString(null, null);

  /// <summary>Converts this BigInteger to a string using the specified format.</summary>
  public string ToString(string? format)
    => this.ToString(format, null);

  /// <summary>Converts this BigInteger to a string using the specified format provider.</summary>
  public string ToString(IFormatProvider? provider)
    => this.ToString(null, provider);

  /// <inheritdoc />
  public string ToString(string? format, IFormatProvider? provider) {
    if (this._sign == 0)
      return "0";

    format = format?.ToUpperInvariant() ?? "D";

    if (format.StartsWith("X"))
      return _ToHexString(format.Length > 1 ? int.Parse(format[1..]) : 0);

    // Default decimal format
    var sb = new StringBuilder();
    var current = Abs(this);
    var ten = new BigInteger(10);

    while (current._sign != 0) {
      current = DivRem(current, ten, out var remainder);
      sb.Insert(0, (char)('0' + (int)remainder));
    }

    if (this._sign < 0)
      sb.Insert(0, '-');

    return sb.ToString();
  }

  private string _ToHexString(int minDigits) {
    if (this._sign == 0)
      return minDigits > 0 ? new string('0', minDigits) : "0";

    var sb = new StringBuilder();
    var bits = this._bits ?? [1];

    for (var i = bits.Length - 1; i >= 0; --i)
      sb.Append(bits[i].ToString(i == bits.Length - 1 ? "X" : "X8"));

    while (sb.Length < minDigits)
      sb.Insert(0, '0');

    if (this._sign < 0)
      sb.Insert(0, '-');

    return sb.ToString();
  }

  /// <summary>Converts this BigInteger to a byte array.</summary>
  public byte[] ToByteArray() {
    if (this._sign == 0)
      return [0];

    var bits = this._bits ?? [1];
    var bytes = new byte[bits.Length * 4 + 1];
    var index = 0;

    foreach (var b in bits) {
      bytes[index++] = (byte)b;
      bytes[index++] = (byte)(b >> 8);
      bytes[index++] = (byte)(b >> 16);
      bytes[index++] = (byte)(b >> 24);
    }

    // Trim trailing zeros for positive, or handle two's complement for negative
    if (this._sign > 0) {
      while (index > 1 && bytes[index - 1] == 0 && (bytes[index - 2] & 0x80) == 0)
        --index;

      var result = new byte[index];
      Array.Copy(bytes, result, index);
      return result;
    } else {
      // Convert to two's complement
      var carry = true;
      for (var i = 0; i < index; ++i) {
        bytes[i] = (byte)~bytes[i];
        if (carry) {
          if (bytes[i] == 255) {
            bytes[i] = 0;
          } else {
            bytes[i]++;
            carry = false;
          }
        }
      }

      // Ensure the high bit is set for negative numbers
      while (index > 1 && bytes[index - 1] == 0xFF && (bytes[index - 2] & 0x80) != 0)
        --index;

      if ((bytes[index - 1] & 0x80) == 0)
        bytes[index++] = 0xFF;

      var result = new byte[index];
      Array.Copy(bytes, result, index);
      return result;
    }
  }

  #endregion

  #region Operators

  public static BigInteger operator +(BigInteger left, BigInteger right) => Add(left, right);
  public static BigInteger operator -(BigInteger left, BigInteger right) => Subtract(left, right);
  public static BigInteger operator *(BigInteger left, BigInteger right) => Multiply(left, right);
  public static BigInteger operator /(BigInteger dividend, BigInteger divisor) => Divide(dividend, divisor);
  public static BigInteger operator %(BigInteger dividend, BigInteger divisor) => Remainder(dividend, divisor);
  public static BigInteger operator -(BigInteger value) => Negate(value);
  public static BigInteger operator +(BigInteger value) => value;
  public static BigInteger operator ++(BigInteger value) => value + One;
  public static BigInteger operator --(BigInteger value) => value - One;

  public static bool operator ==(BigInteger left, BigInteger right) => left.Equals(right);
  public static bool operator !=(BigInteger left, BigInteger right) => !left.Equals(right);
  public static bool operator <(BigInteger left, BigInteger right) => left.CompareTo(right) < 0;
  public static bool operator <=(BigInteger left, BigInteger right) => left.CompareTo(right) <= 0;
  public static bool operator >(BigInteger left, BigInteger right) => left.CompareTo(right) > 0;
  public static bool operator >=(BigInteger left, BigInteger right) => left.CompareTo(right) >= 0;

  public static bool operator ==(BigInteger left, long right) => left.Equals(right);
  public static bool operator !=(BigInteger left, long right) => !left.Equals(right);
  public static bool operator <(BigInteger left, long right) => left.CompareTo(right) < 0;
  public static bool operator <=(BigInteger left, long right) => left.CompareTo(right) <= 0;
  public static bool operator >(BigInteger left, long right) => left.CompareTo(right) > 0;
  public static bool operator >=(BigInteger left, long right) => left.CompareTo(right) >= 0;

  public static bool operator ==(long left, BigInteger right) => right.Equals(left);
  public static bool operator !=(long left, BigInteger right) => !right.Equals(left);
  public static bool operator <(long left, BigInteger right) => right.CompareTo(left) > 0;
  public static bool operator <=(long left, BigInteger right) => right.CompareTo(left) >= 0;
  public static bool operator >(long left, BigInteger right) => right.CompareTo(left) < 0;
  public static bool operator >=(long left, BigInteger right) => right.CompareTo(left) <= 0;

  [CLSCompliant(false)]
  public static bool operator ==(BigInteger left, ulong right) => left.Equals(right);
  [CLSCompliant(false)]
  public static bool operator !=(BigInteger left, ulong right) => !left.Equals(right);
  [CLSCompliant(false)]
  public static bool operator <(BigInteger left, ulong right) => left.CompareTo(right) < 0;
  [CLSCompliant(false)]
  public static bool operator <=(BigInteger left, ulong right) => left.CompareTo(right) <= 0;
  [CLSCompliant(false)]
  public static bool operator >(BigInteger left, ulong right) => left.CompareTo(right) > 0;
  [CLSCompliant(false)]
  public static bool operator >=(BigInteger left, ulong right) => left.CompareTo(right) >= 0;

  [CLSCompliant(false)]
  public static bool operator ==(ulong left, BigInteger right) => right.Equals(left);
  [CLSCompliant(false)]
  public static bool operator !=(ulong left, BigInteger right) => !right.Equals(left);
  [CLSCompliant(false)]
  public static bool operator <(ulong left, BigInteger right) => right.CompareTo(left) > 0;
  [CLSCompliant(false)]
  public static bool operator <=(ulong left, BigInteger right) => right.CompareTo(left) >= 0;
  [CLSCompliant(false)]
  public static bool operator >(ulong left, BigInteger right) => right.CompareTo(left) < 0;
  [CLSCompliant(false)]
  public static bool operator >=(ulong left, BigInteger right) => right.CompareTo(left) <= 0;

  #endregion

  #region Implicit/Explicit Conversions

  public static implicit operator BigInteger(byte value) => new(value);
  public static implicit operator BigInteger(sbyte value) => new(value);
  public static implicit operator BigInteger(short value) => new(value);
  public static implicit operator BigInteger(ushort value) => new(value);
  public static implicit operator BigInteger(int value) => new(value);
  public static implicit operator BigInteger(uint value) => new(value);
  public static implicit operator BigInteger(long value) => new(value);
  public static implicit operator BigInteger(ulong value) => new(value);

  public static explicit operator BigInteger(float value) => new(value);
  public static explicit operator BigInteger(double value) => new(value);
  public static explicit operator BigInteger(decimal value) => new(value);

  public static explicit operator byte(BigInteger value) {
    if (value._sign == 0)
      return 0;
    if (value._sign < 0)
      throw new OverflowException();

    var bits = value._bits;

    // Check for overflow: any non-zero bits beyond the first uint
    if (bits != null)
      for (var i = 1; i < bits.Length; ++i)
        if (bits[i] != 0)
          throw new OverflowException();

    var val = bits?[0] ?? 1u;
    if (val > byte.MaxValue)
      throw new OverflowException();
    return (byte)val;
  }

  public static explicit operator sbyte(BigInteger value) {
    var val = (int)value;
    if (val is < sbyte.MinValue or > sbyte.MaxValue)
      throw new OverflowException();
    return (sbyte)val;
  }

  public static explicit operator short(BigInteger value) {
    var val = (int)value;
    if (val is < short.MinValue or > short.MaxValue)
      throw new OverflowException();
    return (short)val;
  }

  public static explicit operator ushort(BigInteger value) {
    if (value._sign == 0)
      return 0;
    if (value._sign < 0)
      throw new OverflowException();

    var bits = value._bits;

    // Check for overflow: any non-zero bits beyond the first uint
    if (bits != null)
      for (var i = 1; i < bits.Length; ++i)
        if (bits[i] != 0)
          throw new OverflowException();

    var val = bits?[0] ?? 1u;
    if (val > ushort.MaxValue)
      throw new OverflowException();
    return (ushort)val;
  }

  public static explicit operator int(BigInteger value) {
    if (value._sign == 0)
      return 0;

    var bits = value._bits;

    // Check for overflow: any non-zero bits beyond the first uint
    if (bits != null)
      for (var i = 1; i < bits.Length; ++i)
        if (bits[i] != 0)
          throw new OverflowException();

    var val = bits?[0] ?? 1u;
    if (value._sign > 0) {
      if (val > int.MaxValue)
        throw new OverflowException();
      return (int)val;
    } else {
      if (val > (uint)int.MaxValue + 1)
        throw new OverflowException();
      return -(int)val;
    }
  }

  public static explicit operator uint(BigInteger value) {
    if (value._sign == 0)
      return 0;
    if (value._sign < 0)
      throw new OverflowException();

    // Check if value fits in uint (all bits above 31 must be zero)
    var bits = value._bits;
    if (bits == null)
      return 1u;

    // Check for overflow: any non-zero bits beyond the first uint
    for (var i = 1; i < bits.Length; ++i)
      if (bits[i] != 0)
        throw new OverflowException();

    return bits[0];
  }

  public static explicit operator long(BigInteger value) {
    if (value._sign == 0)
      return 0;

    var bits = value._bits;

    // Check for overflow: any non-zero bits beyond the first two uints
    if (bits != null)
      for (var i = 2; i < bits.Length; ++i)
        if (bits[i] != 0)
          throw new OverflowException();

    ulong magnitude;
    if (bits == null)
      magnitude = 1;
    else if (bits.Length == 1)
      magnitude = bits[0];
    else
      magnitude = bits[0] | ((ulong)bits[1] << 32);

    if (value._sign > 0) {
      if (magnitude > long.MaxValue)
        throw new OverflowException();
      return (long)magnitude;
    } else {
      if (magnitude > (ulong)long.MaxValue + 1)
        throw new OverflowException();
      return -(long)magnitude;
    }
  }

  public static explicit operator ulong(BigInteger value) {
    if (value._sign == 0)
      return 0;
    if (value._sign < 0)
      throw new OverflowException();

    var bits = value._bits;
    if (bits == null)
      return 1;

    // Check for overflow: any non-zero bits beyond the first two uints
    for (var i = 2; i < bits.Length; ++i)
      if (bits[i] != 0)
        throw new OverflowException();

    var result = (ulong)bits[0];
    if (bits.Length > 1)
      result |= (ulong)bits[1] << 32;
    return result;
  }

  public static explicit operator float(BigInteger value) => (float)(double)value;

  public static explicit operator double(BigInteger value) {
    if (value._sign == 0)
      return 0.0;

    var bits = value._bits ?? [1];
    double result = 0;
    for (var i = bits.Length - 1; i >= 0; --i)
      result = result * 4294967296.0 + bits[i];

    return value._sign < 0 ? -result : result;
  }

  public static explicit operator decimal(BigInteger value) {
    if (value._sign == 0)
      return 0m;

    var bits = value._bits ?? [1];
    if (bits.Length > 3)
      throw new OverflowException();

    var lo = bits.Length > 0 ? (int)bits[0] : 0;
    var mid = bits.Length > 1 ? (int)bits[1] : 0;
    var hi = bits.Length > 2 ? (int)bits[2] : 0;

    return new decimal(lo, mid, hi, value._sign < 0, 0);
  }

  #endregion

  #region Private Helper Methods

  private static uint[]? _BytesToUints(byte[] bytes) {
    var length = (bytes.Length + 3) / 4;
    var result = new uint[length];

    for (var i = 0; i < bytes.Length; ++i)
      result[i / 4] |= (uint)bytes[i] << (8 * (i % 4));

    return _TrimLeadingZeros(result);
  }

  private static uint[]? _TrimLeadingZeros(uint[]? bits) {
    if (bits == null)
      return null;

    var length = bits.Length;
    while (length > 0 && bits[length - 1] == 0)
      --length;

    if (length == 0)
      return null;
    if (length == 1 && bits[0] == 1)
      return null;
    if (length == bits.Length)
      return bits;

    var result = new uint[length];
    Array.Copy(bits, result, length);
    return result;
  }

  private static int _CompareMagnitude(uint[]? left, uint[]? right) {
    var leftLen = left?.Length ?? (left == null ? 0 : 1);
    var rightLen = right?.Length ?? (right == null ? 0 : 1);

    // Handle null cases (representing value 1)
    if (left == null && right == null)
      return 0;
    if (left == null)
      return rightLen == 1 && right![0] == 1 ? 0 : -1;
    if (right == null)
      return leftLen == 1 && left[0] == 1 ? 0 : 1;

    if (leftLen != rightLen)
      return leftLen.CompareTo(rightLen);

    for (var i = leftLen - 1; i >= 0; --i) {
      if (left[i] != right[i])
        return left[i].CompareTo(right[i]);
    }
    return 0;
  }

  private static uint[] _AddMagnitudes(uint[]? left, uint[]? right) {
    var leftBits = left ?? [1];
    var rightBits = right ?? [1];

    if (leftBits.Length < rightBits.Length)
      (leftBits, rightBits) = (rightBits, leftBits);

    var result = new uint[leftBits.Length + 1];
    ulong carry = 0;

    int i;
    for (i = 0; i < rightBits.Length; ++i) {
      var sum = (ulong)leftBits[i] + rightBits[i] + carry;
      result[i] = (uint)sum;
      carry = sum >> 32;
    }

    for (; i < leftBits.Length; ++i) {
      var sum = (ulong)leftBits[i] + carry;
      result[i] = (uint)sum;
      carry = sum >> 32;
    }

    if (carry != 0)
      result[i] = (uint)carry;

    return result;
  }

  private static uint[]? _SubtractMagnitudes(uint[]? left, uint[]? right) {
    // Assumes left >= right in magnitude
    var leftBits = left ?? [1];
    var rightBits = right ?? [1];

    var result = new uint[leftBits.Length];
    long borrow = 0;

    int i;
    for (i = 0; i < rightBits.Length; ++i) {
      var diff = (long)leftBits[i] - rightBits[i] - borrow;
      if (diff < 0) {
        diff += 0x100000000L;
        borrow = 1;
      } else {
        borrow = 0;
      }
      result[i] = (uint)diff;
    }

    for (; i < leftBits.Length; ++i) {
      var diff = (long)leftBits[i] - borrow;
      if (diff < 0) {
        diff += 0x100000000L;
        borrow = 1;
      } else {
        borrow = 0;
      }
      result[i] = (uint)diff;
    }

    return _TrimLeadingZeros(result);
  }

  private static uint[] _MultiplyMagnitudes(uint[]? left, uint[]? right) {
    var leftBits = left ?? [1];
    var rightBits = right ?? [1];

    var result = new uint[leftBits.Length + rightBits.Length];

    for (var i = 0; i < leftBits.Length; ++i) {
      ulong carry = 0;
      for (var j = 0; j < rightBits.Length; ++j) {
        var product = (ulong)leftBits[i] * rightBits[j] + result[i + j] + carry;
        result[i + j] = (uint)product;
        carry = product >> 32;
      }
      if (carry != 0)
        result[i + rightBits.Length] = (uint)carry;
    }

    return result;
  }

  private static uint[]? _DivideSingleDivisor(uint[]? dividend, uint divisor, out uint remainder) {
    var bits = dividend ?? [1];
    var result = new uint[bits.Length];
    ulong rem = 0;

    for (var i = bits.Length - 1; i >= 0; --i) {
      var current = (rem << 32) | bits[i];
      result[i] = (uint)(current / divisor);
      rem = current % divisor;
    }

    remainder = (uint)rem;
    return _TrimLeadingZeros(result);
  }

  private static (uint[] quotient, uint[] remainder) _LongDivide(uint[] dividend, uint[] divisor) {
    // Simple long division implementation
    var quotient = new uint[dividend.Length];
    var remainder = (uint[])dividend.Clone();

    var n = dividend.Length;
    var m = divisor.Length;

    for (var i = n - m; i >= 0; --i) {
      // Estimate quotient digit
      var rHigh = i + m < remainder.Length ? remainder[i + m] : 0;
      var rMid = i + m - 1 < remainder.Length ? remainder[i + m - 1] : 0;
      var dHigh = divisor[m - 1];

      var estimate = ((ulong)rHigh << 32 | rMid) / dHigh;
      if (estimate > uint.MaxValue)
        estimate = uint.MaxValue;

      // Multiply and subtract
      while (estimate > 0) {
        var product = new uint[m + 1];
        ulong carry = 0;
        for (var j = 0; j < m; ++j) {
          var p = (ulong)divisor[j] * estimate + carry;
          product[j] = (uint)p;
          carry = p >> 32;
        }
        product[m] = (uint)carry;

        // Compare and subtract
        var canSubtract = true;
        for (var j = m; j >= 0; --j) {
          var ri = i + j < remainder.Length ? remainder[i + j] : 0;
          if (ri < product[j]) {
            canSubtract = false;
            break;
          }
          if (ri > product[j])
            break;
        }

        if (canSubtract) {
          long borrow = 0;
          for (var j = 0; j <= m && i + j < remainder.Length; ++j) {
            var diff = (long)remainder[i + j] - product[j] - borrow;
            if (diff < 0) {
              diff += 0x100000000L;
              borrow = 1;
            } else {
              borrow = 0;
            }
            remainder[i + j] = (uint)diff;
          }

          quotient[i] = (uint)estimate;
          break;
        }

        estimate--;
      }
    }

    return (quotient, remainder);
  }

  #endregion

}

#endif
