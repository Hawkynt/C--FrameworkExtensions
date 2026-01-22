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
/// <para>Used in scaling algorithms and color blending operations.</para>
/// <para>Implementations should operate in linear color space for correct results.</para>
/// <para>Uses integer weights for all interpolation to ensure pure integer operations in fast mode.</para>
/// </remarks>
public interface ILerp<T> where T : unmanaged {

  /// <summary>
  /// Linearly interpolates between two colors at the midpoint (50/50 blend).
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns>The color at the midpoint between a and b.</returns>
  T Lerp(in T a, in T b);

  /// <summary>
  /// Linearly interpolates between two colors with integer weights.
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <param name="w1">The weight for the first color.</param>
  /// <param name="w2">The weight for the second color.</param>
  /// <returns>The weighted blend: (a * w1 + b * w2) / (w1 + w2).</returns>
  T Lerp(in T a, in T b, int w1, int w2);
}
