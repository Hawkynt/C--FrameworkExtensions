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
/// Provides threshold-based equality by wrapping a distance metric.
/// </summary>
/// <typeparam name="TKey">The key color type for comparison.</typeparam>
/// <typeparam name="TMetric">The metric type for distance calculation.</typeparam>
/// <remarks>
/// <para>Two colors are considered equal if their distance is below the threshold.</para>
/// <para>This enables perceptual color matching in algorithms like Scale2x/EPX.</para>
/// <para>The threshold is stored as UNorm32 to match metric output.</para>
/// </remarks>
public readonly struct ThresholdEquality<TKey, TMetric> : IColorEquality<TKey>
  where TKey : unmanaged, IColorSpace
  where TMetric : struct, IColorMetric<TKey> {

  private readonly TMetric _metric;
  private readonly UNorm32 _threshold;

  /// <summary>
  /// Creates a threshold-based equality comparer.
  /// </summary>
  /// <param name="threshold">The maximum distance for colors to be considered equal (0.0-1.0 for normalized metrics).</param>
  /// <param name="metric">The distance metric to use.</param>
  public ThresholdEquality(float threshold, TMetric metric = default) {
    this._threshold = UNorm32.FromFloat(threshold);
    this._metric = metric;
  }

  /// <summary>
  /// Creates a threshold-based equality comparer with UNorm32 threshold.
  /// </summary>
  /// <param name="threshold">The maximum distance as UNorm32.</param>
  /// <param name="metric">The distance metric to use.</param>
  public ThresholdEquality(UNorm32 threshold, TMetric metric = default) {
    this._threshold = threshold;
    this._metric = metric;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => this._metric.Distance(a, b) < this._threshold;
}

public readonly struct ThresholdEquality3F<TKey>(float d1, float d2, float d3) : IColorEquality<TKey> where TKey : unmanaged, IColorSpace3F<TKey> {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => MathF.Abs(a.C1 - b.C1) < d1
    && MathF.Abs(a.C2 - b.C2) < d2
    && MathF.Abs(a.C3 - b.C3) < d3;
}

public readonly struct ThresholdEquality4F<TKey>(float d1, float d2, float d3, float d4) : IColorEquality<TKey> where TKey : unmanaged, IColorSpace4F<TKey> {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => MathF.Abs(a.C1 - b.C1) < d1
       && MathF.Abs(a.C2 - b.C2) < d2
       && MathF.Abs(a.C3 - b.C3) < d3
       && MathF.Abs(a.A - b.A) < d4;
}

public readonly struct ThresholdEquality5F<TKey>(float d1, float d2, float d3, float d4, float d5) : IColorEquality<TKey> where TKey : unmanaged, IColorSpace5F<TKey> {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => MathF.Abs(a.C1 - b.C1) < d1
       && MathF.Abs(a.C2 - b.C2) < d2
       && MathF.Abs(a.C3 - b.C3) < d3
       && MathF.Abs(a.C4 - b.C4) < d4
       && MathF.Abs(a.A - b.A) < d5;
}

public readonly struct ThresholdEquality3B<TKey>(byte d1, byte d2, byte d3) : IColorEquality<TKey> where TKey : unmanaged, IColorSpace3B<TKey> {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => _AbsDiff(a.C1, b.C1) < d1
       && _AbsDiff(a.C2, b.C2) < d2
       && _AbsDiff(a.C3, b.C3) < d3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _AbsDiff(byte x, byte y) => x > y ? x - y : y - x;
}

public readonly struct ThresholdEquality4B<TKey>(byte d1, byte d2, byte d3, byte d4) : IColorEquality<TKey> where TKey : unmanaged, IColorSpace4B<TKey> {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => _AbsDiff(a.C1, b.C1) < d1
       && _AbsDiff(a.C2, b.C2) < d2
       && _AbsDiff(a.C3, b.C3) < d3
       && _AbsDiff(a.A, b.A) < d4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _AbsDiff(byte x, byte y) => x > y ? x - y : y - x;
}

public readonly struct ThresholdEquality5B<TKey>(byte d1, byte d2, byte d3, byte d4, byte d5) : IColorEquality<TKey> where TKey : unmanaged, IColorSpace5B<TKey> {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => _AbsDiff(a.C1, b.C1) < d1
       && _AbsDiff(a.C2, b.C2) < d2
       && _AbsDiff(a.C3, b.C3) < d3
       && _AbsDiff(a.C4, b.C4) < d4
       && _AbsDiff(a.A, b.A) < d5;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _AbsDiff(byte x, byte y) => x > y ? x - y : y - x;
}

/// <summary>
/// Provides threshold-based equality using normalized distance metrics.
/// </summary>
/// <typeparam name="TKey">The key color type for comparison.</typeparam>
/// <typeparam name="TMetric">The normalized metric type for distance calculation.</typeparam>
/// <remarks>
/// <para>Two colors are considered equal if their normalized distance is below the threshold.</para>
/// <para>Since the metric is normalized (0 to UNorm32.One), the threshold follows the same range:</para>
/// <list type="bullet">
///   <item><description>UNorm32.Zero = only identical colors are equal</description></item>
///   <item><description>~0.1 = colors within 10% of maximum difference are equal</description></item>
///   <item><description>UNorm32.One = all colors are equal</description></item>
/// </list>
/// </remarks>
public readonly struct NormalizedThresholdEquality<TKey, TMetric> : IColorEquality<TKey>
  where TKey : unmanaged, IColorSpace
  where TMetric : struct, IColorMetric<TKey>, INormalizedMetric {

  private readonly TMetric _metric;
  private readonly UNorm32 _threshold;

  /// <summary>
  /// Creates a normalized threshold-based equality comparer.
  /// </summary>
  /// <param name="threshold">The maximum normalized distance (0-1) for colors to be considered equal.</param>
  /// <param name="metric">The normalized distance metric to use.</param>
  public NormalizedThresholdEquality(float threshold, TMetric metric = default) {
    this._threshold = UNorm32.FromFloat(threshold);
    this._metric = metric;
  }

  /// <summary>
  /// Creates a normalized threshold-based equality comparer with UNorm32 threshold.
  /// </summary>
  /// <param name="threshold">The maximum normalized distance as UNorm32.</param>
  /// <param name="metric">The normalized distance metric to use.</param>
  public NormalizedThresholdEquality(UNorm32 threshold, TMetric metric = default) {
    this._threshold = threshold;
    this._metric = metric;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => this._metric.Distance(a, b) < this._threshold;
}
