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

using System;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Weighted Euclidean (L2) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Applies custom weights to each component before calculating Euclidean distance.
/// Useful when different color channels have different perceptual importance.
/// </remarks>
public readonly struct WeightedEuclidean3<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the weighted Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
#if SUPPORTS_MATHF
    => MathF.Sqrt(WeightedEuclideanSquared3<TKey>._Calculate(a, b, w1, w2, w3));
#else
    => (float)Math.Sqrt(WeightedEuclideanSquared3<TKey>._Calculate(a, b, w1, w2, w3));
#endif
}

/// <summary>
/// Weighted squared Euclidean distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Faster than WeightedEuclidean3 (no sqrt) when only relative comparison is needed.
/// </remarks>
public readonly struct WeightedEuclideanSquared3<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Internal weighted squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b, float w1, float w2, float w3) {
    var d1 = (a.C1 - b.C1) * w1;
    var d2 = (a.C2 - b.C2) * w2;
    var d3 = (a.C3 - b.C3) * w3;
    return d1 * d1 + d2 * d2 + d3 * d3;
  }

  /// <summary>
  /// Calculates the weighted squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b, w1, w2, w3);
}

/// <summary>
/// Non-generic weighted Euclidean distance for LinearRgbF colors.
/// </summary>
public readonly struct WeightedEuclideanRgb(float wR, float wG, float wB) : IColorMetric<Working.LinearRgbF> {

  /// <summary>
  /// Calculates the weighted Euclidean distance between two RGB colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in Working.LinearRgbF a, in Working.LinearRgbF b)
#if SUPPORTS_MATHF
    => MathF.Sqrt(WeightedEuclideanSquaredRgb._Calculate(a, b, wR, wG, wB));
#else
    => (float)Math.Sqrt(WeightedEuclideanSquaredRgb._Calculate(a, b, wR, wG, wB));
#endif
}

/// <summary>
/// Non-generic weighted squared Euclidean distance for LinearRgbF colors.
/// </summary>
public readonly struct WeightedEuclideanSquaredRgb(float wR, float wG, float wB) : IColorMetric<Working.LinearRgbF> {

  /// <summary>
  /// Internal weighted squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in Working.LinearRgbF a, in Working.LinearRgbF b, float wR, float wG, float wB) {
    var dr = (a.R - b.R) * wR;
    var dg = (a.G - b.G) * wG;
    var db = (a.B - b.B) * wB;
    return dr * dr + dg * dg + db * db;
  }

  /// <summary>
  /// Calculates the weighted squared Euclidean distance between two RGB colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in Working.LinearRgbF a, in Working.LinearRgbF b) => _Calculate(a, b, wR, wG, wB);
}
