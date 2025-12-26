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
  /// Linearly interpolates between two colors at the midpoint (t=0.5).
  /// </summary>
  /// <typeparam name="T">The color type to interpolate.</typeparam>
  /// <typeparam name="TLerp">The lerp implementation type.</typeparam>
  /// <param name="lerp">The lerp instance.</param>
  /// <param name="a">The start color.</param>
  /// <param name="b">The end color.</param>
  /// <returns>The color at the midpoint between a and b.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Lerp<T, TLerp>(this TLerp lerp, in T a, in T b)
    where T : unmanaged
    where TLerp : struct, ILerp<T>
    => lerp.Lerp(a, b, 0.5f);
}
