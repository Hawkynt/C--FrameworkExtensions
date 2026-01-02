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

#region 3-Component Float

/// <summary>
/// Weighted Euclidean (L2) distance metric for 3-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// <para>Applies custom weights to each component before calculating Euclidean distance.
/// Useful when different color channels have different perceptual importance.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclidean3F<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the weighted Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(WeightedEuclideanSquared3F<TKey>._Calculate(a, b, w1, w2, w3));
    return UNorm32.FromFloatClamped(raw);
  }
}

/// <summary>
/// Weighted squared Euclidean distance metric for 3-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// <para>Faster than WeightedEuclidean3F (no sqrt) when only relative comparison is needed.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclideanSquared3F<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Internal weighted squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b, float w1, float w2, float w3) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return w1 * d1 * d1 + w2 * d2 * d2 + w3 * d3 * d3;
  }

  /// <summary>
  /// Calculates the weighted squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b, w1, w2, w3));
}

#endregion

#region 3-Component Byte

/// <summary>
/// Weighted Euclidean (L2) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Applies custom weights to each component before calculating Euclidean distance.
/// Useful when different color channels have different perceptual importance.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclidean3B<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3B<TKey> {

  /// <summary>
  /// Calculates the weighted Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(WeightedEuclideanSquared3B<TKey>._Calculate(a, b, w1, w2, w3));
    return UNorm32.FromFloatClamped(raw);
  }
}

/// <summary>
/// Weighted squared Euclidean distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Faster than WeightedEuclidean3B (no sqrt) when only relative comparison is needed.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclideanSquared3B<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3B<TKey> {

  /// <summary>
  /// Internal weighted squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b, float w1, float w2, float w3) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return w1 * d1 * d1 + w2 * d2 * d2 + w3 * d3 * d3;
  }

  /// <summary>
  /// Calculates the weighted squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b, w1, w2, w3));
}

#endregion

#region 4-Component Float

/// <summary>
/// Weighted Euclidean (L2) distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Applies custom weights to each component including alpha before calculating Euclidean distance.
/// Useful when different color channels have different perceptual importance.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclidean4F<TKey>(float w1, float w2, float w3, float wA) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Calculates the weighted Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(WeightedEuclideanSquared4F<TKey>._Calculate(a, b, w1, w2, w3, wA));
    return UNorm32.FromFloatClamped(raw);
  }
}

/// <summary>
/// Weighted squared Euclidean distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Faster than WeightedEuclidean4F (no sqrt) when only relative comparison is needed.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclideanSquared4F<TKey>(float w1, float w2, float w3, float wA) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Internal weighted squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b, float w1, float w2, float w3, float wA) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return w1 * d1 * d1 + w2 * d2 * d2 + w3 * d3 * d3 + wA * da * da;
  }

  /// <summary>
  /// Calculates the weighted squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b, w1, w2, w3, wA));
}

#endregion

#region 4-Component Byte

/// <summary>
/// Weighted Euclidean (L2) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Applies custom weights to each component including alpha before calculating Euclidean distance.
/// Useful when different color channels have different perceptual importance.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclidean4B<TKey>(float w1, float w2, float w3, float wA) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Calculates the weighted Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(WeightedEuclideanSquared4B<TKey>._Calculate(a, b, w1, w2, w3, wA));
    return UNorm32.FromFloatClamped(raw);
  }
}

/// <summary>
/// Weighted squared Euclidean distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Faster than WeightedEuclidean4B (no sqrt) when only relative comparison is needed.</para>
/// <para>Returns UNorm32 distance. NOT normalized - range depends on weights.</para>
/// <para>Does not implement INormalizedMetric since the range varies with weights.</para>
/// </remarks>
public readonly struct WeightedEuclideanSquared4B<TKey>(float w1, float w2, float w3, float wA) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Internal weighted squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b, float w1, float w2, float w3, float wA) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return w1 * d1 * d1 + w2 * d2 * d2 + w3 * d3 * d3 + wA * da * da;
  }

  /// <summary>
  /// Calculates the weighted squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b, w1, w2, w3, wA));
}

#endregion
