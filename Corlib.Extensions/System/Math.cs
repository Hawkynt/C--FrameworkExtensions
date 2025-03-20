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


using System.Collections.Generic;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

#if SUPPORTS_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif
#if SUPPORTS_BITOPERATIONS
using System.Numerics;
#endif

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantCast
// ReSharper disable CompareOfFloatsByEqualityOperator
namespace System;

public static partial class MathEx {

  public const decimal Pi     = 3.14159265358979323846264338327m; // taken from https://3.141592653589793238462643383279502884197169399375105820974944592.eu/
  public const decimal E      = 2.71828182845904523536028747135m; // taken from https://www.mathsisfun.com/numbers/e-eulers-number.html
  public const decimal Sqrt2  = 1.41421356237309504880168872421m; // taken from https://en.wikipedia.org/wiki/Square_root_of_2
  public const decimal Sqrt3  = 1.73205080756887729352744634151m; // taken from https://en.wikipedia.org/wiki/Square_root_of_3
  public const decimal Sqrt5  = 2.23606797749978969640917366873m; // taken from https://en.wikipedia.org/wiki/Square_root_of_5
  public const decimal Sqrt6  = 2.44948974278317809819728407471m; // taken from https://en.wikipedia.org/wiki/Square_root_of_6
  public const decimal Sqrt7  = 2.64575131106459059050161575364m; // taken from https://en.wikipedia.org/wiki/Square_root_of_7
  public const decimal Sqrt8  = 2.82842712474619009760337744842m; // taken from https://en.wikipedia.org/wiki/Square_root#Square_roots_of_positive_integers
  public const decimal Sqrt10 = 3.16227766016837933199889354443m; // taken from https://en.wikipedia.org/wiki/Square_root#Square_roots_of_positive_integers


  /// <summary>
  /// Extracts the lower nibble of the specified byte.
  /// </summary>
  /// <param name="this">The byte from which to extract the lower half.</param>
  /// <returns>A byte containing the lower 4 bits of the input byte.</returns>
  /// <example>
  /// <code>
  /// byte value = 0xAB;
  /// byte lowerHalf = value.LowerHalf(); // lowerHalf is 0x0B
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LowerHalf(this byte @this) => (byte)(@this & 0x0F);

  /// <summary>
  /// Extracts the upper nibble of the specified byte.
  /// </summary>
  /// <param name="this">The byte from which to extract the upper half.</param>
  /// <returns>A byte containing the upper 4 bits of the input byte.</returns>
  /// <example>
  /// <code>
  /// byte value = 0xAB;
  /// byte upperHalf = value.UpperHalf(); // upperHalf is 0x0A
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte UpperHalf(this byte @this) => (byte)(@this >> 4);

  /// <summary>
  /// Extracts the lower byte of the specified word.
  /// </summary>
  /// <param name="this">The ushort from which to extract the lower half.</param>
  /// <returns>A byte containing the lower 8 bits of the input ushort.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0xABCD;
  /// byte lowerHalf = value.LowerHalf(); // lowerHalf is 0xCD
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LowerHalf(this ushort @this) => (byte)@this;

  /// <summary>
  /// Extracts the upper byte of the specified word.
  /// </summary>
  /// <param name="this">The ushort from which to extract the upper half.</param>
  /// <returns>A byte containing the upper 8 bits of the input ushort.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0xABCD;
  /// byte upperHalf = value.UpperHalf(); // upperHalf is 0xAB
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte UpperHalf(this ushort @this) => (byte)(@this >> 8);

  /// <summary>
  /// Extracts the lower word of the specified dword.
  /// </summary>
  /// <param name="this">The uint from which to extract the lower half.</param>
  /// <returns>A ushort containing the lower 16 bits of the input uint.</returns>
  /// <example>
  /// <code>
  /// uint value = 0xABCDEF12;
  /// ushort lowerHalf = value.LowerHalf(); // lowerHalf is 0xEF12
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort LowerHalf(this uint @this) => (ushort)@this;

  /// <summary>
  /// Extracts the upper word of the specified dword.
  /// </summary>
  /// <param name="this">The uint from which to extract the upper half.</param>
  /// <returns>A ushort containing the upper 16 bits of the input uint.</returns>
  /// <example>
  /// <code>
  /// uint value = 0xABCDEF12;
  /// ushort upperHalf = value.UpperHalf(); // upperHalf is 0xABCD
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort UpperHalf(this uint @this) => (ushort)(@this >> 16);

  /// <summary>
  /// Extracts the lower dword of the specified qword.
  /// </summary>
  /// <param name="this">The ulong from which to extract the lower half.</param>
  /// <returns>A uint containing the lower 32 bits of the input ulong.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0xABCDEF1234567890;
  /// uint lowerHalf = value.LowerHalf(); // lowerHalf is 0x34567890
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint LowerHalf(this ulong @this) => (uint)@this;

  /// <summary>
  /// Extracts the upper dword of the specified qword.
  /// </summary>
  /// <param name="this">The ulong from which to extract the upper half.</param>
  /// <returns>A uint containing the upper 32 bits of the input ulong.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0xABCDEF1234567890;
  /// uint upperHalf = value.UpperHalf(); // upperHalf is 0xABCDEF12
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint UpperHalf(this ulong @this) => (uint)(@this >> 32);

#if SUPPORTS_BITOPERATIONS

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte RotateLeft(this byte @this, byte count) {
    var result = @this * 0x01010101U;
    result = BitOperations.RotateLeft(result, count);
    return (byte)result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte RotateRight(this byte @this, byte count) {
    var result = @this * 0x01010101U;
    result = BitOperations.RotateRight(result, count);
    return (byte)result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort RotateLeft(this ushort @this, byte count) {
    var result = @this * 0x00010001U;
    result = BitOperations.RotateLeft(result, count);
    return (ushort)result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort RotateRight(this ushort @this, byte count) {
    var result = @this * 0x00010001U;
    result = BitOperations.RotateRight(result, count);
    return (ushort)result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint RotateLeft(this uint @this, byte count) => BitOperations.RotateLeft(@this, count);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint RotateRight(this uint @this, byte count) => BitOperations.RotateRight(@this, count);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong RotateLeft(this ulong @this, byte count) => BitOperations.RotateLeft(@this, count);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong RotateRight(this ulong @this, byte count) => BitOperations.RotateRight(@this, count);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this byte @this) => (byte)BitOperations.PopCount(@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this ushort @this) => (byte)BitOperations.PopCount(@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this uint @this) => (byte)BitOperations.PopCount(@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this ulong @this) => (byte)BitOperations.PopCount(@this);

#else

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this byte @this) => CountSetBits((uint)@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this ushort @this) => CountSetBits((uint)@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this uint @this) {
    @this -= @this >> 1 & 0x55555555;
    @this = (uint)(((int)@this & 0x33333333) + ((int)(@this >> 2) & 0x33333333));
    @this = (uint)(((int)@this + (int)(@this >> 4) & 0x0F0F0F0F) * 0x01010101 >>> 24);
    return (byte)@this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this ulong @this) {
    @this -= @this >> 1 & 0x5555555555555555;
    @this = (ulong)(((long)@this & 0x3333333333333333) + ((long)(@this >> 2) & 0x3333333333333333));
    @this = (ulong)(((long)@this + (long)(@this >> 4) & 0x0F0F0F0F0F0F0F0F) * 0x0101010101010101 >>> 56);
    return (byte)@this;
  }

#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this byte @this) => (byte)(8 - CountSetBits(@this));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this ushort @this) => (byte)(16 - CountSetBits(@this));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this uint @this) => (byte)(32 - CountSetBits(@this));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this ulong @this) => (byte)(64 - CountSetBits(@this));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ReverseBits(this byte @this) => (byte)(((@this * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort ReverseBits(this ushort @this) {
    @this = (ushort)((@this >> 1) & 0x5555 | (@this & 0x5555) << 1);
    @this = (ushort)((@this >> 2) & 0x3333 | (@this & 0x3333) << 2);
    @this = (ushort)((@this >> 4) & 0x0F0F | (@this & 0x0F0F) << 4);
    @this = (ushort)((@this >> 8) | (@this << 8));
    return @this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReverseBits(this uint @this) {
    @this = (@this >> 1) & 0x55555555 | (@this & 0x55555555) << 1;
    @this = (@this >> 2) & 0x33333333 | (@this & 0x33333333) << 2;
    @this = (@this >> 4) & 0x0F0F0F0F | (@this & 0x0F0F0F0F) << 4;
    @this = (@this >> 8) & 0x00FF00FF | (@this & 0x00FF00FF) << 8;
    @this = (@this >> 16) | (@this << 16);
    return @this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ReverseBits(this ulong @this) {
    @this = (@this >> 1) & 0x5555555555555555UL | (@this & 0x5555555555555555UL) << 1;
    @this = (@this >> 2) & 0x3333333333333333UL | (@this & 0x3333333333333333UL) << 2;
    @this = (@this >> 4) & 0x0F0F0F0F0F0F0F0FUL | (@this & 0x0F0F0F0F0F0F0F0FUL) << 4;
    @this = (@this >> 8) & 0x00FF00FF00FF00FFUL | (@this & 0x00FF00FF00FF00FFUL) << 8;
    @this = (@this >> 16) & 0x0000FFFF0000FFFFUL | (@this & 0x0000FFFF0000FFFFUL) << 16;
    @this = (@this >> 32) | (@this << 32);
    return @this;
  }

#if SUPPORTS_INTRINSICS
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static byte ParallelBitExtract(this byte @this, byte mask) {
#if SUPPORTS_INTRINSICS
    if (Bmi2.IsSupported)
      return (byte)Bmi2.ParallelBitExtract(@this, mask);
#endif

    var maskedValue = @this & mask;
    if (maskedValue == 0)
      return 0;

    var result = 0;
    var bitPos = 0;
    
    _ProcessParallelBitExtract(maskedValue, mask, 0, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 4, ref bitPos, ref result);

    return (byte)result;
  }

#if SUPPORTS_INTRINSICS
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static ushort ParallelBitExtract(this ushort @this, ushort mask) {
#if SUPPORTS_INTRINSICS
    if (Bmi2.IsSupported)
      return (ushort)Bmi2.ParallelBitExtract(@this, mask);
#endif

    var maskedValue = @this & mask;
    if (maskedValue == 0)
      return 0;

    var result = 0;
    var bitPos = 0;

    _ProcessParallelBitExtract(maskedValue, mask, 0, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 4, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 8, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 12, ref bitPos, ref result);

    return (ushort)result;
  }

#if SUPPORTS_INTRINSICS
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static uint ParallelBitExtract(this uint @this, uint mask) {
#if SUPPORTS_INTRINSICS
    if (Bmi2.IsSupported)
      return Bmi2.ParallelBitExtract(@this, mask);
#endif

    var maskedValue = (int)(@this & mask);
    if (maskedValue == 0)
      return 0;

    var result = 0;
    var bitPos = 0;

    _ProcessParallelBitExtract(maskedValue, (int)mask, 0, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 4, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 8, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 12, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 16, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 20, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 24, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 28, ref bitPos, ref result);

    return (uint)result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ProcessParallelBitExtract(int maskedValue, int mask, int startBitPos, ref int bitPos, ref int result) {
    var bitA = maskedValue >>> startBitPos;
    var maskA = mask >>> startBitPos;
    var bitB = maskedValue >>> (startBitPos + 1);
    var maskB = mask >>> (startBitPos + 1);
    var bitC = maskedValue >>> (startBitPos + 2);
    var maskC = mask >>> (startBitPos + 2);
    var bitD = maskedValue >>> (startBitPos + 3);
    var maskD = mask >>> (startBitPos + 3);

    bitA &= 1;
    maskA &= 1;
    bitB &= 1;
    maskB &= 1;
    bitC &= 1;
    maskC &= 1;
    bitD &= 1;
    maskD &= 1;

    bitA <<= bitPos;
    bitPos += maskA;

    bitB <<= bitPos;
    bitPos += maskB;

    bitC <<= bitPos;
    bitPos += maskC;

    bitD <<= bitPos;
    bitPos += maskD;

    result |= bitA;
    result |= bitB;
    result |= bitC;
    result |= bitD;
  }

#if SUPPORTS_INTRINSICS
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static ulong ParallelBitExtract(this ulong @this, ulong mask) {
#if SUPPORTS_INTRINSICS
    if (Bmi2.X64.IsSupported)
      return Bmi2.X64.ParallelBitExtract(@this, mask);
#endif

    // Quick return check
    if ((@this & mask) == 0)
      return 0;

    var result = 0UL;
    var bitPos = 0;

    for (var i = 0; i < 64; ++i) {
      var bitMask = 1UL << i;  // Precompute the bit mask for position i
      if ((mask & bitMask) == 0) // Check if the mask bit is set at this position
        continue;
      
      result |= (@this & bitMask) >> (i - bitPos);  // Directly OR the result
      ++bitPos;
    }

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) DeinterleaveBits(this byte @this) => (
    ParallelBitExtract(@this, 0b01010101),
    ParallelBitExtract(@this, 0b10101010)
  );
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) DeinterleaveBits(this ushort @this) => (
    (byte)ParallelBitExtract(@this, 0b0101010101010101),
    (byte)ParallelBitExtract(@this, 0b1010101010101010)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (ushort odd, ushort even) DeinterleaveBits(this uint @this) => (
    (ushort)ParallelBitExtract(@this, 0b01010101010101010101010101010101U),
    (ushort)ParallelBitExtract(@this, 0b10101010101010101010101010101010U)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (uint odd, uint even) DeinterleaveBits(this ulong @this) => (
    (uint)ParallelBitExtract(@this, 0b0101010101010101010101010101010101010101010101010101010101010101UL),
    (uint)ParallelBitExtract(@this, 0b1010101010101010101010101010101010101010101010101010101010101010UL)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) PairwiseDeinterleaveBits(this byte @this) => (
    ParallelBitExtract(@this, 0b00110011),
    ParallelBitExtract(@this, 0b11001100)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) PairwiseDeinterleaveBits(this ushort @this) => (
    (byte)ParallelBitExtract(@this, 0b0011001100110011),
    (byte)ParallelBitExtract(@this, 0b1100110011001100)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (ushort odd, ushort even) PairwiseDeinterleaveBits(this uint @this) => (
    (ushort)ParallelBitExtract(@this, 0b00110011001100110011001100110011U),
    (ushort)ParallelBitExtract(@this, 0b11001100110011001100110011001100U)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (uint odd, uint even) PairwiseDeinterleaveBits(this ulong @this) => (
    (uint)ParallelBitExtract(@this, 0b0011001100110011001100110011001100110011001100110011001100110011UL),
    (uint)ParallelBitExtract(@this, 0b1100110011001100110011001100110011001100110011001100110011001100UL)
  );

  /// <summary>
  ///   Calculate a more accurate square root, see
  ///   http://stackoverflow.com/questions/4124189/performing-math-operations-on-decimal-datatype-in-c
  /// </summary>
  /// <param name="this">The x.</param>
  /// <param name="epsilon">The epsilon.</param>
  /// <returns>The square root of x</returns>
  public static decimal Sqrt(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValues(@this);
    Against.NegativeValues(epsilon);

    return @this switch {
      0 => 0,
      1 => 1,
      2 => Sqrt2,
      3 => Sqrt3,
      4 => 2,
      5 => Sqrt5,
      6 => Sqrt6,
      7 => Sqrt7,
      8 => Sqrt8,
      9 => 3,
      10 => Sqrt10,
      _ => Calculate()
    };

    decimal Calculate() {
      var current = (decimal)Math.Sqrt((double)@this);
      decimal previous;
      const decimal factor = 2m;

      do {
        previous = current;
        current = (previous + @this / previous) / factor;
      } while (Math.Abs(previous - current) > epsilon);

      return current;
    }
  }

  public static decimal Tan(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValues(epsilon);

    if (@this.IsZero())
      return decimal.Zero;

    var current = (decimal)Math.Tan((double)@this);
    const int MAX_ITERATIONS = 100;

    var x2 = @this * @this;
    var term = @this;
    var correction = @this;
    var denominator = 1m;
    for (var i = 0; i < MAX_ITERATIONS && Math.Abs(term) > epsilon; ++i) {
      denominator += 2m;
      term *= x2 / denominator;
      correction += denominator % 4 == 1 ? term : -term;
    }

    current = (current + correction) / 2m;
    return current;
  }

  public static decimal Atan(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValues(epsilon);

    const decimal PI_OVER_2 = Pi/2;
    return @this switch {
      0m => 0m,
      > 1m => PI_OVER_2 - Atan(1m / @this, epsilon),
      < -1m => -PI_OVER_2 - Atan(1m / @this, epsilon),
      _ => Calculate()
    };

    decimal Calculate() {
      var current = (decimal)Math.Atan((double)@this);
      decimal previous;
      var iterations = 0;
      const int MAX_ITERATIONS = 100;
      do {
        previous = current;
        var tanY = Tan(current, epsilon);
        var sec2Y = 1m + tanY * tanY; // sec^2(y) = 1 + tan^2(y)
        current -= (tanY - @this) / sec2Y;
      }
      while (Math.Abs(current - previous) > epsilon && ++iterations < MAX_ITERATIONS);

      return current;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Floor(this float @this) => MathF.Floor(@this);
#else
  public static float Floor(this float @this) => (float)Math.Floor(@this);
#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Ceiling(this float @this) => MathF.Ceiling(@this);
#else
  public static float Ceiling(this float @this) => (float)Math.Ceiling(@this);
#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Truncate(this float @this) => MathF.Truncate(@this);
#else
  public static float Truncate(this float @this) => (float)Math.Truncate(@this);
#endif

  /// <summary>Rounds a value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <returns>
  ///   The integer nearest the <paramref name="this" /> parameter. If the fractional component of
  ///   <paramref name="this" /> is halfway between two integers, one of which is even and the other odd, the even number is
  ///   returned.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Round(this float @this) => MathF.Round(@this);
#else
  public static float Round(this float @this) => (float)Math.Round(@this);
#endif

  /// <summary>Rounds a value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <param name="digits">The number of decimal places in the return value.</param>
  /// <returns>
  ///   The integer nearest the <paramref name="this" /> parameter. If the fractional component of
  ///   <paramref name="this" /> is halfway between two integers, one of which is even and the other odd, the even number is
  ///   returned.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(this float @this, int digits) {
    Against.ValuesOutOfRange(@this, 0, 15);

#if SUPPORTS_MATHF
    return MathF.Round(@this, digits);
#else
    return (float)Math.Round(@this, digits);
#endif
  }

  /// <summary>Rounds a value to an integer using the specified rounding convention.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <param name="method">One of the enumeration values that specifies which rounding strategy to use.</param>
  /// <returns>The integer that <paramref name="this" /> is rounded to.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Round(this float @this, MidpointRounding method) => MathF.Round(@this, method);
#else
  public static float Round(this float @this, MidpointRounding method) => (float)Math.Round(@this, method);
#endif

  /// <summary>Rounds a value to an integer using the specified rounding convention.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <param name="digits">The number of decimal places in the return value.</param>
  /// <param name="method">One of the enumeration values that specifies which rounding strategy to use.</param>
  /// <returns>The integer that <paramref name="this" /> is rounded to.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(this float @this, int digits, MidpointRounding method) {
    Against.ValuesOutOfRange(@this, 0, 15);

#if SUPPORTS_MATHF
    return MathF.Round(@this, digits, method);
#else
    return (float)Math.Round(@this, digits, method);
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Log(this float @this, float @base) => MathF.Log(@this, @base);
#else
  public static float Log(this float @this, float @base) => (float)Math.Log(@this, @base);
#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Log(this double @this, double @base) => Math.Log(@this, @base);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Log2(this float @this) => MathF.Log(@this, 2);
#else
  public static float Log2(this float @this) => (float)Math.Log(@this, 2);
#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Log2(this double @this) => Math.Log(@this, 2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_MATHF
  public static float Exp(this float @this) => MathF.Exp(@this);
#else
  public static float Exp(this float @this) => (float)Math.Exp(@this);
#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Exp(this double @this) => Math.Exp(@this);

  /// <summary>
  ///   Calculates the cubic root.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Cbrt(this double @this) => Math.Pow(@this, 1d / 3);

  /// <summary>
  ///   Calculates the cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Cot(this double @this) => Math.Cos(@this) / Math.Sin(@this);

  /// <summary>
  ///   Calculates the hyperbolic cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Coth(this double @this) {
    var ex = Math.Exp(@this);
    var em = 1 / ex;
    return (ex + em) / (ex - em);
  }

  /// <summary>
  ///   Calculates the cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Csc(this double @this) => 1 / Math.Sin(@this);

  /// <summary>
  ///   Calculates the hyperbolic cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Csch(this double @this) {
    var ex = Math.Exp(@this);
    return 2 / (ex - 1 / ex);
  }

  /// <summary>
  ///   Calculates the secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Sec(this double @this) => 1 / Math.Cos(@this);

  /// <summary>
  ///   Calculates the hyperbolic secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Sech(this double @this) {
    var ex = Math.Exp(@this);
    return 2 / (ex + 1 / ex);
  }

  /// <summary>
  ///   Calculates the area hyperbolic sine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arsinh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this + 1));

  /// <summary>
  ///   Calculates the area hyperbolic cosine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arcosh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this - 1));

  /// <summary>
  ///   Calculates the area hyperbolic tangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Artanh(this double @this) => 0.5d * Math.Log((1 + @this) / (1 - @this));

  /// <summary>
  ///   Calculates the area hyperbolic cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arcoth(this double @this) => 0.5d * Math.Log((@this + 1) / (@this - 1));

  /// <summary>
  ///   Calculates the area hyperbolic secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arsech(this double @this) => Math.Log((1 + Math.Sqrt(1 - @this * @this)) / @this);

  /// <summary>
  ///   Calculates the area hyperbolic cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arcsch(this double @this) => Math.Log((1 + Math.Sqrt(1 + @this * @this)) / @this);

  /// <summary>
  ///   Calculates the arcus sine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Asin(this double @this) => Math.Asin(@this);

  /// <summary>
  ///   Calculates the arcus cosine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Acos(this double @this) => Math.Acos(@this);

  /// <summary>
  ///   Calculates the arcus tangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Atan(this double @this) => Math.Atan(@this);

  /// <summary>
  ///   Calculates the arcus cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Acot(this double @this) => Math.Atan(1 / @this);

  /// <summary>
  ///   Calculates the arcus secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Asec(this double @this) => Math.Acos(1 / @this);

  /// <summary>
  ///   Calculates the arcus cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Acsc(this double @this) => Math.Asin(1 / @this);

  /// <summary>
  ///   Enumerates all primes in the ulong value space.
  /// </summary>
  public static IEnumerable<ulong> EnumeratePrimes => _EnumeratePrimes();

  private readonly struct PrimeSieve(ulong[] values) {
    public IEnumerable<ulong> Enumerate() {
      ulong prime = 3;
      var values1 = values;
      for (var i = 0; i < values1.Length; ++i, prime += 2) {
        if (values1[i] != 0)
          continue;

#if SUPPORTS_ASYNC
        var task = this._FillSieveAsync(prime);
        yield return prime;
        task.Wait();
#else
        yield return prime;
        this._FillSieveAction(prime);
#endif
      }
    }

#if SUPPORTS_ASYNC
    private Task _FillSieveAsync(ulong prime) => Task.Factory.StartNew(this._FillSieveAction, prime);
    private void _FillSieveAction(object state) => this._FillSieveAction((ulong)state);
#endif

    private void _FillSieveAction(ulong prime) {
      var doublePrime = prime << 1;
      var values1 = values;
      var maxNumberInSieve = ((ulong)values1.Length << 1) + 3;
      for (var j = prime * prime; j < maxNumberInSieve; j += doublePrime)
        values1[(int)((j - 3) >> 1)] = j;
    }
  }

  private struct KnownPrimesStorage(ulong[] primes) {
    private int _index;

    // no checks because only used internally and guaranteed to have a primes!=null && primes.Length > 0 

    private bool _IsSpaceInBufferLeft() => this._index < primes.Length;

    // no checks because we guarantee, that all calls occur while there is still space in the array
    public void Add(ulong prime) => primes[this._index++] = prime;

    public IEnumerable<ulong> Enumerate() {
      foreach (var prime in this._GenerateAndFillBuffer())
        yield return prime;

#if COLOR_PRIME_GENERATION
      Console.ForegroundColor = ConsoleColor.Yellow;
#endif

      foreach (var prime in this._EnumerateWithFullBuffer())
        yield return prime;
    }

    private IEnumerable<ulong> _GenerateAndFillBuffer() {
      // array always valid
      var primes1 = primes;

      // array always contains at least one prime from the sieve
      var lastKnownPrime = primes1[this._index - 1];

#if SUPPORTS_ASYNC
      var task = Task.Factory.StartNew(this._FindNextPrimeWithPartiallyFilledBuffer, lastKnownPrime);
      for (;;) {
        task.Wait();
        lastKnownPrime = task.Result;
        this.Add(lastKnownPrime);
        if (this._IsSpaceInBufferLeft()) {
          task = Task.Factory.StartNew(this._FindNextPrimeWithPartiallyFilledBuffer, lastKnownPrime);
          yield return lastKnownPrime;
        } else {
          yield return lastKnownPrime;
          yield break;
        }
      }
#else
      while (this._IsSpaceInBufferLeft()) {
        lastKnownPrime = this._FindNextPrimeWithPartiallyFilledBuffer(lastKnownPrime);
        this.Add(lastKnownPrime);
        yield return lastKnownPrime;
      }
#endif
    }

#if SUPPORTS_ASYNC
    private ulong _FindNextPrimeWithPartiallyFilledBuffer(object state) => this._FindNextPrimeWithPartiallyFilledBuffer((ulong)state);
#endif

    private ulong _FindNextPrimeWithPartiallyFilledBuffer(ulong lastKnownPrime) {
      var candidate = lastKnownPrime;
      do
        candidate += 2;
      while (!this._IsPrimeWithPartiallyFilledBuffer(candidate));

      return candidate;
    }

    private bool _IsPrimeWithPartiallyFilledBuffer(ulong candidate) {
      for (var i = 0; i < this._index; ++i)
        if (candidate % primes[i] == 0)
          return false;

      return true;
    }

    private IEnumerable<ulong> _EnumerateWithFullBuffer() {
      var lastKnownPrime = primes[^1];
      var upperPrimeSquare = lastKnownPrime * lastKnownPrime;

#if SUPPORTS_ASYNC
      var task = Task.Factory.StartNew(this._FindNextPrimeWithFullBuffer, lastKnownPrime);
      for (var candidate = lastKnownPrime + 2; candidate <= upperPrimeSquare; candidate = task.Result) {
        task.Wait();
        lastKnownPrime = task.Result;

        if (lastKnownPrime <= upperPrimeSquare) {
          task = Task.Factory.StartNew(this._FindNextPrimeWithFullBuffer, lastKnownPrime);
          yield return lastKnownPrime;
        } else {
          yield return lastKnownPrime;
          yield break;
        }
      }
#else
      for (var candidate = lastKnownPrime + 2; candidate <= upperPrimeSquare; candidate += 2) {
        if (this._IsPrimeWithFullBuffer(candidate))
          yield return candidate;
      }
#endif
    }

#if SUPPORTS_ASYNC
    private ulong _FindNextPrimeWithFullBuffer(object state) => this._FindNextPrimeWithFullBuffer((ulong)state);
#endif

    private ulong _FindNextPrimeWithFullBuffer(ulong lastKnownPrime) {
      var candidate = lastKnownPrime;
      do
        candidate += 2;
      while (!this._IsPrimeWithFullBuffer(candidate));

      return candidate;
    }

    private bool _IsPrimeWithFullBuffer(ulong candidate) {
      foreach (var prime in primes)
        if (candidate % prime == 0)
          return false;

      return true;
    }
  }

  private static IEnumerable<ulong> _EnumeratePrimes() {
#if COLOR_PRIME_GENERATION
    Console.ForegroundColor = ConsoleColor.White;
#endif
    yield return 2;

    var buffer = new ulong[128];
    PrimeSieve sieve = new(buffer);
    KnownPrimesStorage knownPrimes = new(buffer);

#if COLOR_PRIME_GENERATION
    Console.ForegroundColor = ConsoleColor.Cyan;
#endif
    foreach (var prime in sieve.Enumerate()) {
      yield return prime;
      knownPrimes.Add(prime);
    }

#if COLOR_PRIME_GENERATION
    Console.ForegroundColor = ConsoleColor.Green;
#endif
    foreach (var prime in knownPrimes.Enumerate())
      yield return prime;

#if COLOR_PRIME_GENERATION
    Console.ForegroundColor = ConsoleColor.Red;
#endif
    foreach (var prime in EnumerateSlowPrimesWithKnowns())
      yield return prime;

    IEnumerable<ulong> EnumerateSlowPrimesWithKnowns() {
      var largestKnownPrime = buffer[^1];

      // Start from the square of the last known prime plus 2 (to ensure it's odd)
      var candidate = largestKnownPrime * largestKnownPrime + 2;

#if SUPPORTS_ASYNC
      var task = Task.Factory.StartNew(IsPrimeWithBufferAndBeyondT, candidate);
      for (;;) {
        task.Wait();
        var isPrime = task.Result;

        if (isPrime)
          yield return candidate;

        candidate += 2; // Ensure we only check odd numbers
        task = Task.Factory.StartNew(IsPrimeWithBufferAndBeyondT, candidate);
      }
#else
      for (;;) {
        var isPrime = IsPrimeWithBufferAndBeyond(candidate);

        if (isPrime)
          yield return candidate;

        candidate += 2; // Ensure we only check odd numbers
      }
#endif
    }

#if SUPPORTS_ASYNC
    bool IsPrimeWithBufferAndBeyondT(object state) => IsPrimeWithBufferAndBeyond((ulong)state);
#endif

    bool IsPrimeWithBufferAndBeyond(ulong candidate) {
      // 1. Check divisibility with all primes in the buffer
      foreach (var prime in buffer)
        if (candidate % prime == 0)
          return false;

      // 2. If none of the primes in the buffer divide the candidate, 
      //    check divisibility with numbers (only odd ones) up to the square root of the candidate
      var sqrtCandidate = (ulong)Math.Sqrt(candidate);
      for (var i = buffer[^1] + 2; i <= sqrtCandidate; i += 2)
        if (candidate % i == 0)
          return false;

      return true;
    }
  }
}
