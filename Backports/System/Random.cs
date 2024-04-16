#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
namespace System;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class RandomPolyfills {

#if !SUPPORTS_RANDOM_NEXTINT64

  /// <summary>Returns a non-negative random integer.</summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue"/>.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long NextInt64(this Random @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    
    return (long)(@this.NextDouble() * ulong.MaxValue);
  }

  /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to 0.</param>
  /// <returns>
  /// A 64-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily
  /// includes 0 but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long NextInt64(this Random @this, long maxValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (maxValue < 0)
      throw new ArgumentOutOfRangeException(nameof(maxValue));
    
    return (long)(@this.NextDouble() * maxValue);
  }

  /// <summary>Returns a random integer that is within a specified range.</summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
  /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
  /// <returns>
  /// A 64-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
  /// but not <paramref name="maxValue"/>. If minValue equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long NextInt64(this Random @this, long minValue, long maxValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (maxValue < minValue)
      throw new ArgumentOutOfRangeException(nameof(maxValue));
    
    return minValue + (long)(@this.NextDouble() * (maxValue - minValue));
  }

#endif

}