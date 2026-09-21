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
/// <para>
/// Each blend comes in two rounding modes. <c>Lerp</c> truncates the division, which is what
/// fixed-point blenders have always done here; <c>LerpRounded</c> rounds to nearest instead. The
/// pair exists because some algorithms are specified against one mode or the other — xBRZ, for
/// example, truncates up to release 1.8 and rounds from 1.9 on — and the caller picks the lerp
/// implementation long before the algorithm gets to say which mode it wants.
/// </para>
/// <para>
/// The rounded members are declared, not defaulted. C# 8 default interface methods would keep the
/// interface source-compatible for outside implementors, but they need runtime support this package
/// cannot assume: net35..net48 do not have it and the compiler rejects them outright (CS8701). The
/// same constraint is why <see cref="Metrics.BatchDistanceDefaults"/> exists. An implementation for
/// which rounding is meaningless — anything in floating point, or a no-op blender — should forward
/// the rounded member to the truncating one.
/// </para>
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

  /// <summary>
  /// Linearly interpolates between two colors at the midpoint (50/50 blend), rounding to nearest.
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns>The color at the midpoint between a and b, rounded rather than truncated.</returns>
  /// <remarks>
  /// Differs from <see cref="Lerp(in T, in T)"/> only where the midpoint falls between two
  /// representable values; for a fixed-point channel that is a difference of at most one step.
  /// </remarks>
  T LerpRounded(in T a, in T b);

  /// <summary>
  /// Linearly interpolates between two colors with integer weights, rounding to nearest.
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <param name="w1">The weight for the first color.</param>
  /// <param name="w2">The weight for the second color.</param>
  /// <returns>The weighted blend (a * w1 + b * w2) / (w1 + w2), rounded rather than truncated.</returns>
  /// <remarks>
  /// Differs from <see cref="Lerp(in T, in T, int, int)"/> whenever the weighted sum leaves a
  /// non-zero remainder against the total weight. Truncation biases every such blend downwards;
  /// rounding does not.
  /// </remarks>
  T LerpRounded(in T a, in T b, int w1, int w2);
}
