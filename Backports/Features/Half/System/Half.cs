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

#if !SUPPORTS_HALF

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents a half-precision floating-point number (IEEE 754 binary16).
/// </summary>
public readonly struct Half : IComparable, IComparable<Half>, IEquatable<Half>, IFormattable {

  private readonly ushort _value;

  // IEEE 754 binary16 constants
  private const ushort SignMask = 0x8000;
  private const ushort ExponentMask = 0x7C00;
  private const ushort MantissaMask = 0x03FF;
  private const int ExponentBias = 15;
  private const int ExponentShift = 10;

  // Special values
  private const ushort PositiveInfinityBits = 0x7C00;
  private const ushort NegativeInfinityBits = 0xFC00;
  private const ushort NaNBits = 0x7E00;
  private const ushort PositiveZeroBits = 0x0000;
  private const ushort NegativeZeroBits = 0x8000;
  private const ushort EpsilonBits = 0x0001;
  private const ushort MinValueBits = 0xFBFF;
  private const ushort MaxValueBits = 0x7BFF;

  private Half(ushort value) => _value = value;

  /// <summary>
  /// Gets the smallest positive Half value that is greater than zero.
  /// </summary>
  public static Half Epsilon => new(EpsilonBits);

  /// <summary>
  /// Gets the largest finite value that can be represented by Half.
  /// </summary>
  public static Half MaxValue => new(MaxValueBits);

  /// <summary>
  /// Gets the smallest finite value that can be represented by Half.
  /// </summary>
  public static Half MinValue => new(MinValueBits);

  /// <summary>
  /// Gets a value that represents NaN (Not a Number).
  /// </summary>
  public static Half NaN => new(NaNBits);

  /// <summary>
  /// Gets a value that represents negative infinity.
  /// </summary>
  public static Half NegativeInfinity => new(NegativeInfinityBits);

  /// <summary>
  /// Gets a value that represents positive infinity.
  /// </summary>
  public static Half PositiveInfinity => new(PositiveInfinityBits);

  /// <summary>
  /// Determines whether the specified value is finite (not NaN or infinity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsFinite(Half value) {
    var exp = (value._value & ExponentMask) >> ExponentShift;
    return exp != 0x1F;
  }

  /// <summary>
  /// Determines whether the specified value is positive or negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsInfinity(Half value) {
    var bits = value._value;
    return (bits & ~SignMask) == PositiveInfinityBits;
  }

  /// <summary>
  /// Determines whether the specified value is NaN.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNaN(Half value) {
    var bits = value._value;
    return ((bits & ExponentMask) == ExponentMask) && ((bits & MantissaMask) != 0);
  }

  /// <summary>
  /// Determines whether the specified value is negative.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegative(Half value) => (value._value & SignMask) != 0;

  /// <summary>
  /// Determines whether the specified value is negative infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNegativeInfinity(Half value) => value._value == NegativeInfinityBits;

  /// <summary>
  /// Determines whether the specified value is a normal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNormal(Half value) {
    var exp = (value._value & ExponentMask) >> ExponentShift;
    return exp != 0 && exp != 0x1F;
  }

  /// <summary>
  /// Determines whether the specified value is positive infinity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPositiveInfinity(Half value) => value._value == PositiveInfinityBits;

  /// <summary>
  /// Determines whether the specified value is a subnormal number.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSubnormal(Half value) {
    var bits = value._value;
    var exp = (bits & ExponentMask) >> ExponentShift;
    var mantissa = bits & MantissaMask;
    return exp == 0 && mantissa != 0;
  }

  /// <summary>
  /// Compares this instance to a specified Half.
  /// </summary>
  public int CompareTo(Half other) {
    if (IsNaN(this))
      return IsNaN(other) ? 0 : -1;
    if (IsNaN(other))
      return 1;

    var thisFloat = (float)this;
    var otherFloat = (float)other;
    return thisFloat.CompareTo(otherFloat);
  }

  /// <summary>
  /// Compares this instance to a specified object.
  /// </summary>
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not Half other)
      throw new ArgumentException("Object must be of type Half.");
    return CompareTo(other);
  }

  /// <summary>
  /// Indicates whether this instance and a specified Half are equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Half other) {
    if (IsNaN(this) && IsNaN(other))
      return true;
    return _value == other._value;
  }

  /// <summary>
  /// Indicates whether this instance and a specified object are equal.
  /// </summary>
  public override bool Equals(object? obj) => obj is Half other && Equals(other);

  /// <summary>
  /// Returns the hash code for this instance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => _value.GetHashCode();

  /// <summary>
  /// Converts this Half to a string representation.
  /// </summary>
  public override string ToString() => ((float)this).ToString();

  /// <summary>
  /// Converts this Half to a string representation using the specified format provider.
  /// </summary>
  public string ToString(IFormatProvider? provider) => ((float)this).ToString(provider);

  /// <summary>
  /// Converts this Half to a string representation using the specified format.
  /// </summary>
  public string ToString(string? format) => ((float)this).ToString(format);

  /// <summary>
  /// Converts this Half to a string representation using the specified format and format provider.
  /// </summary>
  public string ToString(string? format, IFormatProvider? provider) => ((float)this).ToString(format, provider);

  /// <summary>
  /// Tries to format the value of the current instance into the provided span of characters.
  /// </summary>
  public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) {
    var str = ToString(format.Length > 0 ? format.ToString() : null, provider);
    if (str.Length > destination.Length) {
      charsWritten = 0;
      return false;
    }
    str.AsSpan().CopyTo(destination);
    charsWritten = str.Length;
    return true;
  }

  /// <summary>
  /// Converts a string to a Half.
  /// </summary>
  public static Half Parse(string s) => (Half)float.Parse(s);

  /// <summary>
  /// Converts a string to a Half using the specified number style.
  /// </summary>
  public static Half Parse(string s, NumberStyles style) => (Half)float.Parse(s, style);

  /// <summary>
  /// Converts a string to a Half using the specified format provider.
  /// </summary>
  public static Half Parse(string s, IFormatProvider? provider) => (Half)float.Parse(s, provider);

  /// <summary>
  /// Converts a string to a Half using the specified number style and format provider.
  /// </summary>
  public static Half Parse(string s, NumberStyles style, IFormatProvider? provider) => (Half)float.Parse(s, style, provider);

  /// <summary>
  /// Converts a span of characters to a Half.
  /// </summary>
  public static Half Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands, IFormatProvider? provider = null)
    => (Half)float.Parse(s.ToString(), style, provider);

  /// <summary>
  /// Tries to convert a string to a Half.
  /// </summary>
  public static bool TryParse(string? s, out Half result) {
    if (float.TryParse(s, out var f)) {
      result = (Half)f;
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a string to a Half using the specified number style and format provider.
  /// </summary>
  public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out Half result) {
    if (float.TryParse(s, style, provider, out var f)) {
      result = (Half)f;
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a span of characters to a Half.
  /// </summary>
  public static bool TryParse(ReadOnlySpan<char> s, out Half result) {
    if (float.TryParse(s.ToString(), out var f)) {
      result = (Half)f;
      return true;
    }
    result = default;
    return false;
  }

  /// <summary>
  /// Tries to convert a span of characters to a Half using the specified number style and format provider.
  /// </summary>
  public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Half result) {
    if (float.TryParse(s.ToString(), style, provider, out var f)) {
      result = (Half)f;
      return true;
    }
    result = default;
    return false;
  }

  // Comparison operators
  public static bool operator ==(Half left, Half right) => left.Equals(right);
  public static bool operator !=(Half left, Half right) => !left.Equals(right);
  public static bool operator <(Half left, Half right) => (float)left < (float)right;
  public static bool operator >(Half left, Half right) => (float)left > (float)right;
  public static bool operator <=(Half left, Half right) => (float)left <= (float)right;
  public static bool operator >=(Half left, Half right) => (float)left >= (float)right;

  // Explicit conversion from float to Half
  public static explicit operator Half(float value) {
    if (float.IsNaN(value))
      return NaN;
    if (float.IsPositiveInfinity(value))
      return PositiveInfinity;
    if (float.IsNegativeInfinity(value))
      return NegativeInfinity;

    var floatBits = _SingleToInt32Bits(value);
    var sign = (uint)(floatBits >> 31) & 1;
    var exp = (floatBits >> 23) & 0xFF;
    var mantissa = floatBits & 0x7FFFFF;

    ushort halfBits;

    if (exp == 0) {
      // Zero or subnormal float -> zero in half
      halfBits = (ushort)(sign << 15);
    } else if (exp == 0xFF) {
      // Infinity or NaN
      halfBits = mantissa != 0
        ? (ushort)((sign << 15) | 0x7C00 | (uint)(mantissa >> 13))
        : (ushort)((sign << 15) | 0x7C00);
    } else {
      // Normalized number
      var newExp = (int)exp - 127 + ExponentBias;
      if (newExp >= 31) {
        // Overflow to infinity
        halfBits = (ushort)((sign << 15) | 0x7C00);
      } else if (newExp <= 0) {
        // Underflow to zero or subnormal
        if (newExp < -10) {
          halfBits = (ushort)(sign << 15);
        } else {
          // Subnormal
          mantissa |= 0x800000;
          var shift = 14 - newExp;
          halfBits = (ushort)((sign << 15) | (uint)(mantissa >> shift));
        }
      } else {
        // Normal half
        halfBits = (ushort)((sign << 15) | (uint)(newExp << 10) | (uint)(mantissa >> 13));
      }
    }

    return new(halfBits);
  }

  // Explicit conversion from double to Half
  public static explicit operator Half(double value) => (Half)(float)value;

  // Explicit conversion from Half to float
  public static explicit operator float(Half value) {
    var bits = value._value;
    var sign = (uint)(bits >> 15) & 1;
    var exp = (bits >> 10) & 0x1F;
    var mantissa = bits & MantissaMask;

    int floatBits;

    if (exp == 0) {
      if (mantissa == 0) {
        // Zero
        floatBits = (int)(sign << 31);
      } else {
        // Subnormal half -> normalized float
        exp = 1;
        while ((mantissa & 0x400) == 0) {
          mantissa <<= 1;
          --exp;
        }
        mantissa &= 0x3FF;
        var newExp = exp - ExponentBias + 127;
        floatBits = (int)((sign << 31) | (uint)(newExp << 23) | (uint)(mantissa << 13));
      }
    } else if (exp == 0x1F) {
      // Infinity or NaN
      floatBits = mantissa != 0
        ? (int)((sign << 31) | 0x7F800000 | (uint)(mantissa << 13))
        : (int)((sign << 31) | 0x7F800000);
    } else {
      // Normalized
      var newExp = exp - ExponentBias + 127;
      floatBits = (int)((sign << 31) | (uint)(newExp << 23) | (uint)(mantissa << 13));
    }

    return _Int32BitsToSingle(floatBits);
  }

  // Explicit conversion from Half to double
  public static explicit operator double(Half value) => (float)value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe int _SingleToInt32Bits(float value) => *(int*)&value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float _Int32BitsToSingle(int value) => *(float*)&value;

}

#endif
