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

/// <summary>
/// Per-component threshold equality for 3-component color spaces using UNorm32 internally.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3.</typeparam>
public readonly struct ThresholdEquality3<TKey> : IColorEquality<TKey>
  where TKey : unmanaged, IColorSpace3<TKey> {

  private readonly UNorm32 _d1, _d2, _d3;

  /// <summary>Creates with per-component UNorm32 thresholds.</summary>
  public ThresholdEquality3(UNorm32 d1, UNorm32 d2, UNorm32 d3) {
    this._d1 = d1;
    this._d2 = d2;
    this._d3 = d3;
  }

  /// <summary>Creates with per-component float thresholds (0.0-1.0).</summary>
  public ThresholdEquality3(float d1, float d2, float d3)
    : this(UNorm32.FromFloat(d1), UNorm32.FromFloat(d2), UNorm32.FromFloat(d3)) { }

  /// <summary>Creates with per-component byte thresholds (0-255).</summary>
  public ThresholdEquality3(byte d1, byte d2, byte d3)
    : this(UNorm32.FromByte(d1), UNorm32.FromByte(d2), UNorm32.FromByte(d3)) { }

  /// <summary>Creates with a single threshold for all components.</summary>
  public ThresholdEquality3(UNorm32 threshold) : this(threshold, threshold, threshold) { }

  /// <summary>Creates with a single float threshold for all components.</summary>
  public ThresholdEquality3(float threshold) : this(UNorm32.FromFloat(threshold)) { }

  /// <summary>Creates with a single byte threshold for all components.</summary>
  public ThresholdEquality3(byte threshold) : this(UNorm32.FromByte(threshold)) { }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b) {
    var (a1, a2, a3) = a.ToNormalized();
    var (b1, b2, b3) = b.ToNormalized();
    return UNorm32.AbsDiff(a1, b1) < this._d1
        && UNorm32.AbsDiff(a2, b2) < this._d2
        && UNorm32.AbsDiff(a3, b3) < this._d3;
  }
}

/// <summary>
/// Per-component threshold equality for 4-component color spaces using UNorm32 internally.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4.</typeparam>
public readonly struct ThresholdEquality4<TKey> : IColorEquality<TKey>
  where TKey : unmanaged, IColorSpace4<TKey> {

  private readonly UNorm32 _d1, _d2, _d3, _dA;

  /// <summary>Creates with per-component UNorm32 thresholds.</summary>
  public ThresholdEquality4(UNorm32 d1, UNorm32 d2, UNorm32 d3, UNorm32 dA) {
    this._d1 = d1;
    this._d2 = d2;
    this._d3 = d3;
    this._dA = dA;
  }

  /// <summary>Creates with per-component float thresholds (0.0-1.0).</summary>
  public ThresholdEquality4(float d1, float d2, float d3, float dA)
    : this(UNorm32.FromFloat(d1), UNorm32.FromFloat(d2), UNorm32.FromFloat(d3), UNorm32.FromFloat(dA)) { }

  /// <summary>Creates with per-component byte thresholds (0-255).</summary>
  public ThresholdEquality4(byte d1, byte d2, byte d3, byte dA)
    : this(UNorm32.FromByte(d1), UNorm32.FromByte(d2), UNorm32.FromByte(d3), UNorm32.FromByte(dA)) { }

  /// <summary>Creates with a single threshold for all components.</summary>
  public ThresholdEquality4(UNorm32 threshold) : this(threshold, threshold, threshold, threshold) { }

  /// <summary>Creates with a single float threshold for all components.</summary>
  public ThresholdEquality4(float threshold) : this(UNorm32.FromFloat(threshold)) { }

  /// <summary>Creates with a single byte threshold for all components.</summary>
  public ThresholdEquality4(byte threshold) : this(UNorm32.FromByte(threshold)) { }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b) {
    var (a1, a2, a3, aA) = a.ToNormalized();
    var (b1, b2, b3, bA) = b.ToNormalized();
    return UNorm32.AbsDiff(a1, b1) < this._d1
        && UNorm32.AbsDiff(a2, b2) < this._d2
        && UNorm32.AbsDiff(a3, b3) < this._d3
        && UNorm32.AbsDiff(aA, bA) < this._dA;
  }
}

/// <summary>
/// Per-component threshold equality for 5-component color spaces using UNorm32 internally.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace5.</typeparam>
public readonly struct ThresholdEquality5<TKey> : IColorEquality<TKey>
  where TKey : unmanaged, IColorSpace5<TKey> {

  private readonly UNorm32 _d1, _d2, _d3, _d4, _dA;

  /// <summary>Creates with per-component UNorm32 thresholds.</summary>
  public ThresholdEquality5(UNorm32 d1, UNorm32 d2, UNorm32 d3, UNorm32 d4, UNorm32 dA) {
    this._d1 = d1;
    this._d2 = d2;
    this._d3 = d3;
    this._d4 = d4;
    this._dA = dA;
  }

  /// <summary>Creates with per-component float thresholds (0.0-1.0).</summary>
  public ThresholdEquality5(float d1, float d2, float d3, float d4, float dA)
    : this(UNorm32.FromFloat(d1), UNorm32.FromFloat(d2), UNorm32.FromFloat(d3), UNorm32.FromFloat(d4), UNorm32.FromFloat(dA)) { }

  /// <summary>Creates with per-component byte thresholds (0-255).</summary>
  public ThresholdEquality5(byte d1, byte d2, byte d3, byte d4, byte dA)
    : this(UNorm32.FromByte(d1), UNorm32.FromByte(d2), UNorm32.FromByte(d3), UNorm32.FromByte(d4), UNorm32.FromByte(dA)) { }

  /// <summary>Creates with a single threshold for all components.</summary>
  public ThresholdEquality5(UNorm32 threshold) : this(threshold, threshold, threshold, threshold, threshold) { }

  /// <summary>Creates with a single float threshold for all components.</summary>
  public ThresholdEquality5(float threshold) : this(UNorm32.FromFloat(threshold)) { }

  /// <summary>Creates with a single byte threshold for all components.</summary>
  public ThresholdEquality5(byte threshold) : this(UNorm32.FromByte(threshold)) { }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b) {
    var (a1, a2, a3, a4, aA) = a.ToNormalized();
    var (b1, b2, b3, b4, bA) = b.ToNormalized();
    return UNorm32.AbsDiff(a1, b1) < this._d1
        && UNorm32.AbsDiff(a2, b2) < this._d2
        && UNorm32.AbsDiff(a3, b3) < this._d3
        && UNorm32.AbsDiff(a4, b4) < this._d4
        && UNorm32.AbsDiff(aA, bA) < this._dA;
  }
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
