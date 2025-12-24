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

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// Provides linear interpolation between two colors.
/// </summary>
/// <typeparam name="T">The color type to interpolate.</typeparam>
/// <remarks>
/// Used in scaling algorithms and color blending operations.
/// Implementations should operate in linear color space for correct results.
/// </remarks>
public interface ILerp<T> where T : unmanaged {

  /// <summary>
  /// Linearly interpolates between two colors.
  /// </summary>
  /// <param name="a">The start color (t=0).</param>
  /// <param name="b">The end color (t=1).</param>
  /// <param name="t">The interpolation factor (0.0 to 1.0).</param>
  /// <returns>The interpolated color.</returns>
  T Lerp(in T a, in T b, float t);
}
