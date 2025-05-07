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

#if !SUPPORTS_RANDOM_NEXTINT64

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
using Guard;

namespace System;

public static partial class RandomPolyfills {
  /// <summary>Returns a non-negative random integer.</summary>
  /// <param name="this">The <see cref="Random" /> instance used to generate the random value.</param>
  /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long NextInt64(this Random @this) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

    // Collect raw bits from Next() calls
    var high = (ulong)@this.Next();
    var mid = (ulong)@this.Next();
    var low = (ulong)@this.Next();

    // Combine the bits into a 93-bit value
    var seed = (high << 31) | mid;           // 62 bits
    var extra = low;                         // 31 more bits

    // Apply SplitMix64-like mixing function
    seed ^= seed >> 30;
    seed *= 0xBF58476D1CE4E5B9UL;
    seed ^= seed >> 27;
    seed *= 0x94D049BB133111EBUL;
    seed ^= seed >> 31;

    // Mix in the extra bits
    seed ^= extra;

    // Final mixing step (FNV-1a variant)
    seed ^= seed >> 33;
    seed *= 0xFF51AFD7ED558CCDL;
    seed ^= seed >> 33;
    seed *= 0xC4CEB9FE1A85EC53L;
    seed ^= seed >> 33;

    // Clear the sign bit to ensure positive result
    return (long)(seed & 0x7FFFFFFFFFFFFFFFUL);
  }

  /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
  /// <param name="this">The <see cref="Random" /> instance used to generate the random value.</param>
  /// <param name="maxValue">
  ///   The exclusive upper bound of the random number to be generated. <paramref name="maxValue" />
  ///   must be greater than or equal to 0.
  /// </param>
  /// <returns>
  ///   A 64-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is, the
  ///   range of return values ordinarily
  ///   includes 0 but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0,
  ///   <paramref name="maxValue" /> is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue" /> is less than 0.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long NextInt64(this Random @this, long maxValue) {
    if (@this == null)
      AlwaysThrow.NullReferenceException(nameof(@this));
    if (maxValue < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(maxValue));

    var maxValueMinusOne = maxValue - 1;
    return maxValue switch {
      0 => 0,
      <= int.MaxValue when (maxValue & maxValueMinusOne) == 0 => @this.Next() & maxValueMinusOne,
      <= int.MaxValue => SmallModuloRejectionSampling(@this,maxValue),
      _ when (maxValue & maxValueMinusOne) == 0 => NextInt64(@this) & maxValueMinusOne,
      _ => ModuloRejectionSampling(@this, maxValue)
    };

    static long SmallModuloRejectionSampling(Random random, long limit) {
      long result;
      var maxAcceptable = int.MaxValue - int.MaxValue % limit;
      do
        result = random.Next();
      while (result >= maxAcceptable);

      return result % limit;
    }

    static long ModuloRejectionSampling(Random random, long limit) {
      long result;
      var maxAcceptable = long.MaxValue - long.MaxValue % limit;
      do
        result = NextInt64(random);
      while (result >= maxAcceptable);

      return result % limit;
    }
  }

  /// <summary>Returns a random integer that is within a specified range.</summary>
  /// <param name="this">The <see cref="Random" /> instance used to generate the random value.</param>
  /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
  /// <param name="maxValue">
  ///   The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be
  ///   greater than or equal to <paramref name="minValue" />.
  /// </param>
  /// <returns>
  ///   A 64-bit signed integer greater than or equal to <paramref name="minValue" /> and less than
  ///   <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" />
  ///   but not <paramref name="maxValue" />. If minValue equals <paramref name="maxValue" />, <paramref name="minValue" />
  ///   is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  ///   <paramref name="minValue" /> is greater than <paramref name="maxValue" />
  ///   .
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long NextInt64(this Random @this, long minValue, long maxValue) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (maxValue < minValue)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(maxValue));

    return minValue + NextInt64(@this, maxValue - minValue);
  }
}

#endif
