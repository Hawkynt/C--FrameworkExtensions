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

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// Extension methods for <see cref="ILerp{T}"/>.
/// </summary>
public static class LerpExtensions {

  /// <summary>
  /// Averages three colors equally (1/3 weight each).
  /// </summary>
  /// <typeparam name="T">The color type to interpolate.</typeparam>
  /// <typeparam name="TLerp">The lerp implementation type.</typeparam>
  /// <param name="lerp">The lerp instance.</param>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <param name="c">The third color.</param>
  /// <returns>The average of the three colors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Lerp<T, TLerp>(this TLerp lerp, in T a, in T b, in T c)
    where T : unmanaged
    where TLerp : struct, ILerp<T>
    => lerp.Lerp(lerp.Lerp(a, b), c, 2, 1);

  /// <summary>
  /// Averages four colors equally (1/4 weight each).
  /// </summary>
  /// <typeparam name="T">The color type to interpolate.</typeparam>
  /// <typeparam name="TLerp">The lerp implementation type.</typeparam>
  /// <param name="lerp">The lerp instance.</param>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <param name="c">The third color.</param>
  /// <param name="d">The fourth color.</param>
  /// <returns>The average of the four colors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Lerp<T, TLerp>(this TLerp lerp, in T a, in T b, in T c, in T d)
    where T : unmanaged
    where TLerp : struct, ILerp<T>
    => lerp.Lerp(lerp.Lerp(a, b), lerp.Lerp(c, d));

  /// <summary>
  /// Blends three colors with integer weights.
  /// </summary>
  /// <typeparam name="T">The color type to interpolate.</typeparam>
  /// <typeparam name="TLerp">The lerp implementation type.</typeparam>
  /// <param name="lerp">The lerp instance.</param>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <param name="c">The third color.</param>
  /// <param name="w1">The weight for the first color.</param>
  /// <param name="w2">The weight for the second color.</param>
  /// <param name="w3">The weight for the third color.</param>
  /// <returns>The weighted blend of the three colors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Lerp<T, TLerp>(this TLerp lerp, in T a, in T b, in T c, int w1, int w2, int w3)
    where T : unmanaged
    where TLerp : struct, ILerp<T> {
    var w12 = w1 + w2;
    var ab = lerp.Lerp(a, b, w1, w2);
    return lerp.Lerp(ab, c, w12, w3);
  }

  /// <summary>
  /// Blends four colors with integer weights.
  /// </summary>
  /// <typeparam name="T">The color type to interpolate.</typeparam>
  /// <typeparam name="TLerp">The lerp implementation type.</typeparam>
  /// <param name="lerp">The lerp instance.</param>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <param name="c">The third color.</param>
  /// <param name="d">The fourth color.</param>
  /// <param name="w1">The weight for the first color.</param>
  /// <param name="w2">The weight for the second color.</param>
  /// <param name="w3">The weight for the third color.</param>
  /// <param name="w4">The weight for the fourth color.</param>
  /// <returns>The weighted blend of the four colors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Lerp<T, TLerp>(this TLerp lerp, in T a, in T b, in T c, in T d, int w1, int w2, int w3, int w4)
    where T : unmanaged
    where TLerp : struct, ILerp<T> {
    var w12 = w1 + w2;
    var w34 = w3 + w4;
    var ab = lerp.Lerp(a, b, w1, w2);
    var cd = lerp.Lerp(c, d, w3, w4);
    return lerp.Lerp(ab, cd, w12, w34);
  }
}
