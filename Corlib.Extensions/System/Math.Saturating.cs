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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathEx {

  #region byte

  /// <param name="this">The first operand.</param>
  extension(byte @this) {

    /// <summary>
    /// Adds two values with saturation, clamping the result to <see cref="byte.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    /// <example>
    /// <code>
    /// byte a = 250;
    /// byte result = a.SaturatingAdd(10); // result is 255 (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte SaturatingAdd(byte value) {
      var result = @this + value;
      return result > byte.MaxValue ? byte.MaxValue : (byte)result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping the result to <see cref="byte.MinValue"/> on underflow.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    /// <example>
    /// <code>
    /// byte a = 5;
    /// byte result = a.SaturatingSubtract(10); // result is 0 (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte SaturatingSubtract(byte value)
      => value > @this ? byte.MinValue : (byte)(@this - value);

    /// <summary>
    /// Multiplies two values with saturation, clamping the result to <see cref="byte.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    /// <example>
    /// <code>
    /// byte a = 100;
    /// byte result = a.SaturatingMultiply(3); // result is 255 (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte SaturatingMultiply(byte value) {
      var result = @this * value;
      return result > byte.MaxValue ? byte.MaxValue : (byte)result;
    }

    /// <summary>
    /// Divides two values. For unsigned types, division cannot overflow.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte SaturatingDivide(byte value) => (byte)(@this / value);

  }

  #endregion

  #region sbyte

  /// <param name="this">The first operand.</param>
  extension(sbyte @this) {

    /// <summary>
    /// Adds two values with saturation, clamping to <see cref="sbyte.MaxValue"/> or <see cref="sbyte.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    /// <example>
    /// <code>
    /// sbyte a = 120;
    /// sbyte result = a.SaturatingAdd(20); // result is 127 (saturated to MaxValue)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte SaturatingAdd(sbyte value) {
      var result = @this + value;
      if (result > sbyte.MaxValue)
        return sbyte.MaxValue;
      if (result < sbyte.MinValue)
        return sbyte.MinValue;
      return (sbyte)result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping to <see cref="sbyte.MaxValue"/> or <see cref="sbyte.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    /// <example>
    /// <code>
    /// sbyte a = -120;
    /// sbyte result = a.SaturatingSubtract(20); // result is -128 (saturated to MinValue)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte SaturatingSubtract(sbyte value) {
      var result = @this - value;
      if (result > sbyte.MaxValue)
        return sbyte.MaxValue;
      if (result < sbyte.MinValue)
        return sbyte.MinValue;
      return (sbyte)result;
    }

    /// <summary>
    /// Multiplies two values with saturation, clamping to <see cref="sbyte.MaxValue"/> or <see cref="sbyte.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    /// <example>
    /// <code>
    /// sbyte a = 100;
    /// sbyte result = a.SaturatingMultiply(2); // result is 127 (saturated to MaxValue)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte SaturatingMultiply(sbyte value) {
      var result = @this * value;
      if (result > sbyte.MaxValue)
        return sbyte.MaxValue;
      if (result < sbyte.MinValue)
        return sbyte.MinValue;
      return (sbyte)result;
    }

    /// <summary>
    /// Divides two values with saturation. Handles the special case of <see cref="sbyte.MinValue"/> / -1.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The saturated quotient.</returns>
    /// <example>
    /// <code>
    /// sbyte a = sbyte.MinValue;
    /// sbyte result = a.SaturatingDivide(-1); // result is 127 (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte SaturatingDivide(sbyte value)
      => @this == sbyte.MinValue && value == -1 ? sbyte.MaxValue : (sbyte)(@this / value);

    /// <summary>
    /// Negates the value with saturation. Handles the special case of <see cref="sbyte.MinValue"/>.
    /// </summary>
    /// <returns>The saturated negation.</returns>
    /// <example>
    /// <code>
    /// sbyte a = sbyte.MinValue;
    /// sbyte result = a.SaturatingNegate(); // result is 127 (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte SaturatingNegate()
      => @this == sbyte.MinValue ? sbyte.MaxValue : (sbyte)(-@this);

  }

  #endregion

  #region ushort

  /// <param name="this">The first operand.</param>
  extension(ushort @this) {

    /// <summary>
    /// Adds two values with saturation, clamping the result to <see cref="ushort.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort SaturatingAdd(ushort value) {
      var result = @this + value;
      return result > ushort.MaxValue ? ushort.MaxValue : (ushort)result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping the result to <see cref="ushort.MinValue"/> on underflow.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort SaturatingSubtract(ushort value)
      => value > @this ? ushort.MinValue : (ushort)(@this - value);

    /// <summary>
    /// Multiplies two values with saturation, clamping the result to <see cref="ushort.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort SaturatingMultiply(ushort value) {
      var result = @this * value;
      return result > ushort.MaxValue ? ushort.MaxValue : (ushort)result;
    }

    /// <summary>
    /// Divides two values. For unsigned types, division cannot overflow.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort SaturatingDivide(ushort value) => (ushort)(@this / value);

  }

  #endregion

  #region short

  /// <param name="this">The first operand.</param>
  extension(short @this) {

    /// <summary>
    /// Adds two values with saturation, clamping to <see cref="short.MaxValue"/> or <see cref="short.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short SaturatingAdd(short value) {
      var result = @this + value;
      if (result > short.MaxValue)
        return short.MaxValue;
      if (result < short.MinValue)
        return short.MinValue;
      return (short)result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping to <see cref="short.MaxValue"/> or <see cref="short.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short SaturatingSubtract(short value) {
      var result = @this - value;
      if (result > short.MaxValue)
        return short.MaxValue;
      if (result < short.MinValue)
        return short.MinValue;
      return (short)result;
    }

    /// <summary>
    /// Multiplies two values with saturation, clamping to <see cref="short.MaxValue"/> or <see cref="short.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short SaturatingMultiply(short value) {
      var result = @this * value;
      if (result > short.MaxValue)
        return short.MaxValue;
      if (result < short.MinValue)
        return short.MinValue;
      return (short)result;
    }

    /// <summary>
    /// Divides two values with saturation. Handles the special case of <see cref="short.MinValue"/> / -1.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The saturated quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short SaturatingDivide(short value)
      => @this == short.MinValue && value == -1 ? short.MaxValue : (short)(@this / value);

    /// <summary>
    /// Negates the value with saturation. Handles the special case of <see cref="short.MinValue"/>.
    /// </summary>
    /// <returns>The saturated negation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short SaturatingNegate()
      => @this == short.MinValue ? short.MaxValue : (short)(-@this);

  }

  #endregion

  #region uint

  /// <param name="this">The first operand.</param>
  extension(uint @this) {

    /// <summary>
    /// Adds two values with saturation, clamping the result to <see cref="uint.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint SaturatingAdd(uint value) {
      var result = (ulong)@this + value;
      return result > uint.MaxValue ? uint.MaxValue : (uint)result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping the result to <see cref="uint.MinValue"/> on underflow.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint SaturatingSubtract(uint value)
      => value > @this ? uint.MinValue : @this - value;

    /// <summary>
    /// Multiplies two values with saturation, clamping the result to <see cref="uint.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint SaturatingMultiply(uint value) {
      var result = (ulong)@this * value;
      return result > uint.MaxValue ? uint.MaxValue : (uint)result;
    }

    /// <summary>
    /// Divides two values. For unsigned types, division cannot overflow.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint SaturatingDivide(uint value) => @this / value;

  }

  #endregion

  #region int

  /// <param name="this">The first operand.</param>
  extension(int @this) {

    /// <summary>
    /// Adds two values with saturation, clamping to <see cref="int.MaxValue"/> or <see cref="int.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    /// <example>
    /// <code>
    /// int a = int.MaxValue - 5;
    /// int result = a.SaturatingAdd(10); // result is int.MaxValue (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SaturatingAdd(int value) {
      var result = (long)@this + value;
      if (result > int.MaxValue)
        return int.MaxValue;
      if (result < int.MinValue)
        return int.MinValue;
      return (int)result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping to <see cref="int.MaxValue"/> or <see cref="int.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    /// <example>
    /// <code>
    /// int a = int.MinValue + 5;
    /// int result = a.SaturatingSubtract(10); // result is int.MinValue (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SaturatingSubtract(int value) {
      var result = (long)@this - value;
      if (result > int.MaxValue)
        return int.MaxValue;
      if (result < int.MinValue)
        return int.MinValue;
      return (int)result;
    }

    /// <summary>
    /// Multiplies two values with saturation, clamping to <see cref="int.MaxValue"/> or <see cref="int.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SaturatingMultiply(int value) {
      var result = (long)@this * value;
      if (result > int.MaxValue)
        return int.MaxValue;
      if (result < int.MinValue)
        return int.MinValue;
      return (int)result;
    }

    /// <summary>
    /// Divides two values with saturation. Handles the special case of <see cref="int.MinValue"/> / -1.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The saturated quotient.</returns>
    /// <example>
    /// <code>
    /// int a = int.MinValue;
    /// int result = a.SaturatingDivide(-1); // result is int.MaxValue (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SaturatingDivide(int value)
      => @this == int.MinValue && value == -1 ? int.MaxValue : @this / value;

    /// <summary>
    /// Negates the value with saturation. Handles the special case of <see cref="int.MinValue"/>.
    /// </summary>
    /// <returns>The saturated negation.</returns>
    /// <example>
    /// <code>
    /// int a = int.MinValue;
    /// int result = a.SaturatingNegate(); // result is int.MaxValue (saturated)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SaturatingNegate()
      => @this == int.MinValue ? int.MaxValue : -@this;

  }

  #endregion

  #region ulong

  /// <param name="this">The first operand.</param>
  extension(ulong @this) {

    /// <summary>
    /// Adds two values with saturation, clamping the result to <see cref="ulong.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong SaturatingAdd(ulong value) {
      var result = unchecked(@this + value);
      return result < @this ? ulong.MaxValue : result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping the result to <see cref="ulong.MinValue"/> on underflow.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong SaturatingSubtract(ulong value)
      => value > @this ? ulong.MinValue : @this - value;

    /// <summary>
    /// Multiplies two values with saturation, clamping the result to <see cref="ulong.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong SaturatingMultiply(ulong value) {
      if (@this == 0 || value == 0)
        return 0;
      var result = unchecked(@this * value);
      // Check for overflow: result / value should equal @this
      return result / value != @this ? ulong.MaxValue : result;
    }

    /// <summary>
    /// Divides two values. For unsigned types, division cannot overflow.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong SaturatingDivide(ulong value) => @this / value;

  }

  #endregion

  #region long

  /// <param name="this">The first operand.</param>
  extension(long @this) {

    /// <summary>
    /// Adds two values with saturation, clamping to <see cref="long.MaxValue"/> or <see cref="long.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long SaturatingAdd(long value) {
      var result = unchecked(@this + value);
      // Overflow: both positive but result negative
      if (@this > 0 && value > 0 && result < 0)
        return long.MaxValue;
      // Underflow: both negative but result positive
      if (@this < 0 && value < 0 && result > 0)
        return long.MinValue;
      return result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping to <see cref="long.MaxValue"/> or <see cref="long.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long SaturatingSubtract(long value) {
      var result = unchecked(@this - value);
      // Overflow: positive - negative but result negative
      if (@this > 0 && value < 0 && result < 0)
        return long.MaxValue;
      // Underflow: negative - positive but result positive
      if (@this < 0 && value > 0 && result > 0)
        return long.MinValue;
      return result;
    }

    /// <summary>
    /// Multiplies two values with saturation, clamping to <see cref="long.MaxValue"/> or <see cref="long.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long SaturatingMultiply(long value) {
      if (@this == 0 || value == 0)
        return 0;

      var result = unchecked(@this * value);
      // Check for overflow by dividing back
      if (result / value != @this) {
        // Determine sign of result
        var sameSign = (@this > 0) == (value > 0);
        return sameSign ? long.MaxValue : long.MinValue;
      }
      return result;
    }

    /// <summary>
    /// Divides two values with saturation. Handles the special case of <see cref="long.MinValue"/> / -1.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The saturated quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long SaturatingDivide(long value)
      => @this == long.MinValue && value == -1 ? long.MaxValue : @this / value;

    /// <summary>
    /// Negates the value with saturation. Handles the special case of <see cref="long.MinValue"/>.
    /// </summary>
    /// <returns>The saturated negation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long SaturatingNegate()
      => @this == long.MinValue ? long.MaxValue : -@this;

  }

  #endregion

  #region UInt96

  /// <param name="this">The first operand.</param>
  extension(UInt96 @this) {

    /// <summary>
    /// Adds two values with saturation, clamping the result to <see cref="UInt96.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt96 SaturatingAdd(UInt96 value) {
      var result = @this + value;
      return result < @this ? UInt96.MaxValue : result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping the result to <see cref="UInt96.MinValue"/> on underflow.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt96 SaturatingSubtract(UInt96 value)
      => value > @this ? UInt96.MinValue : @this - value;

    /// <summary>
    /// Multiplies two values with saturation, clamping the result to <see cref="UInt96.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt96 SaturatingMultiply(UInt96 value) {
      if (@this == UInt96.Zero || value == UInt96.Zero)
        return UInt96.Zero;
      var result = @this * value;
      return result / value != @this ? UInt96.MaxValue : result;
    }

    /// <summary>
    /// Divides two values. For unsigned types, division cannot overflow.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt96 SaturatingDivide(UInt96 value) => @this / value;

  }

  #endregion

  #region Int96

  /// <param name="this">The first operand.</param>
  extension(Int96 @this) {

    /// <summary>
    /// Adds two values with saturation, clamping to <see cref="Int96.MaxValue"/> or <see cref="Int96.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int96 SaturatingAdd(Int96 value) {
      var result = @this + value;
      // Overflow: both positive but result negative
      if (@this > Int96.Zero && value > Int96.Zero && result < Int96.Zero)
        return Int96.MaxValue;
      // Underflow: both negative but result positive
      if (@this < Int96.Zero && value < Int96.Zero && result > Int96.Zero)
        return Int96.MinValue;
      return result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping to <see cref="Int96.MaxValue"/> or <see cref="Int96.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int96 SaturatingSubtract(Int96 value) {
      var result = @this - value;
      // Overflow: positive - negative but result negative
      if (@this > Int96.Zero && value < Int96.Zero && result < Int96.Zero)
        return Int96.MaxValue;
      // Underflow: negative - positive but result positive
      if (@this < Int96.Zero && value > Int96.Zero && result > Int96.Zero)
        return Int96.MinValue;
      return result;
    }

    /// <summary>
    /// Multiplies two values with saturation, clamping to <see cref="Int96.MaxValue"/> or <see cref="Int96.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int96 SaturatingMultiply(Int96 value) {
      if (@this == Int96.Zero || value == Int96.Zero)
        return Int96.Zero;

      var result = @this * value;
      if (result / value != @this) {
        var sameSign = (@this > Int96.Zero) == (value > Int96.Zero);
        return sameSign ? Int96.MaxValue : Int96.MinValue;
      }
      return result;
    }

    /// <summary>
    /// Divides two values with saturation. Handles the special case of <see cref="Int96.MinValue"/> / -1.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The saturated quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int96 SaturatingDivide(Int96 value)
      => @this == Int96.MinValue && value == Int96.NegativeOne ? Int96.MaxValue : @this / value;

    /// <summary>
    /// Negates the value with saturation. Handles the special case of <see cref="Int96.MinValue"/>.
    /// </summary>
    /// <returns>The saturated negation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int96 SaturatingNegate()
      => @this == Int96.MinValue ? Int96.MaxValue : -@this;

  }

  #endregion

  #region UInt128

  /// <param name="this">The first operand.</param>
  extension(UInt128 @this) {

    /// <summary>
    /// Adds two values with saturation, clamping the result to <see cref="UInt128.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt128 SaturatingAdd(UInt128 value) {
      var result = @this + value;
      return result < @this ? UInt128.MaxValue : result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping the result to <see cref="UInt128.MinValue"/> on underflow.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt128 SaturatingSubtract(UInt128 value)
      => value > @this ? UInt128.MinValue : @this - value;

    /// <summary>
    /// Multiplies two values with saturation, clamping the result to <see cref="UInt128.MaxValue"/> on overflow.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt128 SaturatingMultiply(UInt128 value) {
      if (@this == UInt128.Zero || value == UInt128.Zero)
        return UInt128.Zero;
      var result = @this * value;
      return result / value != @this ? UInt128.MaxValue : result;
    }

    /// <summary>
    /// Divides two values. For unsigned types, division cannot overflow.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt128 SaturatingDivide(UInt128 value) => @this / value;

  }

  #endregion

  #region Int128

  /// <param name="this">The first operand.</param>
  extension(Int128 @this) {

    /// <summary>
    /// Adds two values with saturation, clamping to <see cref="Int128.MaxValue"/> or <see cref="Int128.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The saturated sum.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int128 SaturatingAdd(Int128 value) {
      var result = @this + value;
      // Overflow: both positive but result negative
      if (@this > Int128.Zero && value > Int128.Zero && result < Int128.Zero)
        return Int128.MaxValue;
      // Underflow: both negative but result positive
      if (@this < Int128.Zero && value < Int128.Zero && result > Int128.Zero)
        return Int128.MinValue;
      return result;
    }

    /// <summary>
    /// Subtracts a value with saturation, clamping to <see cref="Int128.MaxValue"/> or <see cref="Int128.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to subtract.</param>
    /// <returns>The saturated difference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int128 SaturatingSubtract(Int128 value) {
      var result = @this - value;
      // Overflow: positive - negative but result negative
      if (@this > Int128.Zero && value < Int128.Zero && result < Int128.Zero)
        return Int128.MaxValue;
      // Underflow: negative - positive but result positive
      if (@this < Int128.Zero && value > Int128.Zero && result > Int128.Zero)
        return Int128.MinValue;
      return result;
    }

    /// <summary>
    /// Multiplies two values with saturation, clamping to <see cref="Int128.MaxValue"/> or <see cref="Int128.MinValue"/>.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>The saturated product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int128 SaturatingMultiply(Int128 value) {
      if (@this == Int128.Zero || value == Int128.Zero)
        return Int128.Zero;

      var result = @this * value;
      if (result / value != @this) {
        var sameSign = (@this > Int128.Zero) == (value > Int128.Zero);
        return sameSign ? Int128.MaxValue : Int128.MinValue;
      }
      return result;
    }

    /// <summary>
    /// Divides two values with saturation. Handles the special case of <see cref="Int128.MinValue"/> / -1.
    /// </summary>
    /// <param name="value">The divisor.</param>
    /// <returns>The saturated quotient.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int128 SaturatingDivide(Int128 value)
      => @this == Int128.MinValue && value == Int128.NegativeOne ? Int128.MaxValue : @this / value;

    /// <summary>
    /// Negates the value with saturation. Handles the special case of <see cref="Int128.MinValue"/>.
    /// </summary>
    /// <returns>The saturated negation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int128 SaturatingNegate()
      => @this == Int128.MinValue ? Int128.MaxValue : -@this;

  }

  #endregion

}
