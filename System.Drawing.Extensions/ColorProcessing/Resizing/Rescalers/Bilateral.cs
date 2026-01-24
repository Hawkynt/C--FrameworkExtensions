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
using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Bilateral filter - edge-preserving smoothing scaler.
/// </summary>
/// <remarks>
/// <para>The bilateral filter is an edge-preserving smoothing filter that combines</para>
/// <para>spatial and range (color) information to preserve edges while smoothing.</para>
/// <para>Algorithm steps:</para>
/// <list type="number">
/// <item>For each output pixel, sample 5x5 neighborhood</item>
/// <item>Compute spatial weight (Gaussian based on distance)</item>
/// <item>Compute range weight (Gaussian based on color difference)</item>
/// <item>Combined weight = spatial * range</item>
/// <item>Apply weighted average</item>
/// </list>
/// </remarks>
[ScalerInfo("Bilateral", Description = "Edge-preserving smoothing filter", Category = ScalerCategory.Resampler)]
public readonly struct Bilateral : IPixelScaler {

  private readonly int _scale;
  private readonly BilateralVariant _variant;

  /// <summary>
  /// Creates a new Bilateral instance.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  /// <param name="variant">Quality variant.</param>
  public Bilateral(int scale = 2, BilateralVariant variant = BilateralVariant.Standard) {
    if (scale is not (2 or 3 or 4))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "Bilateral supports 2x, 3x, 4x scaling");
    this._scale = scale;
    this._variant = variant;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel> {
    var (spatialSigmaSq, rangeSigmaSq) = BilateralHelpers.GetVariantParams(this._variant);
    return this._scale switch {
      0 or 2 => callback.Invoke(new Bilateral2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, spatialSigmaSq, rangeSigmaSq)),
      3 => callback.Invoke(new Bilateral3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, spatialSigmaSq, rangeSigmaSq)),
      4 => callback.Invoke(new Bilateral4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, spatialSigmaSq, rangeSigmaSq)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };
  }

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  #region Static Presets

  /// <summary>Gets a 2x Bilateral scaler with standard settings.</summary>
  public static Bilateral X2 => new(2);

  /// <summary>Gets a 3x Bilateral scaler with standard settings.</summary>
  public static Bilateral X3 => new(3);

  /// <summary>Gets a 4x Bilateral scaler with standard settings.</summary>
  public static Bilateral X4 => new(4);

  /// <summary>Gets the default Bilateral scaler (2x standard).</summary>
  public static Bilateral Default => X2;

  /// <summary>Gets a 2x soft Bilateral scaler (more smoothing).</summary>
  public static Bilateral X2Soft => new(2, BilateralVariant.Soft);

  /// <summary>Gets a 2x sharp Bilateral scaler (more edge preservation).</summary>
  public static Bilateral X2Sharp => new(2, BilateralVariant.Sharp);

  #endregion

  /// <summary>
  /// Creates a configuration with the specified variant.
  /// </summary>
  public Bilateral WithVariant(BilateralVariant variant) => new(this._scale == 0 ? 2 : this._scale, variant);
}

/// <summary>
/// Bilateral filter variants.
/// </summary>
public enum BilateralVariant {
  /// <summary>Standard settings - balanced smoothing and edge preservation.</summary>
  Standard,
  /// <summary>Soft settings - more smoothing, larger spatial sigma.</summary>
  Soft,
  /// <summary>Sharp settings - sharper edges, smaller range sigma.</summary>
  Sharp
}

#region Bilateral Helpers

file static class BilateralHelpers {

  // Precomputed sigma squared values
  private const float StandardSpatialSigmaSq = 1.0f * 1.0f;
  private const float StandardRangeSigmaSq = 25.0f * 25.0f;
  private const float SoftSpatialSigmaSq = 1.5f * 1.5f;
  private const float SoftRangeSigmaSq = 30.0f * 30.0f;
  private const float SharpSpatialSigmaSq = 0.8f * 0.8f;
  private const float SharpRangeSigmaSq = 15.0f * 15.0f;

  /// <summary>
  /// Gets spatial and range sigma squared for the specified variant.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (float spatialSigmaSq, float rangeSigmaSq) GetVariantParams(BilateralVariant variant) => variant switch {
    BilateralVariant.Standard => (StandardSpatialSigmaSq, StandardRangeSigmaSq),
    BilateralVariant.Soft => (SoftSpatialSigmaSq, SoftRangeSigmaSq),
    BilateralVariant.Sharp => (SharpSpatialSigmaSq, SharpRangeSigmaSq),
    _ => (StandardSpatialSigmaSq, StandardRangeSigmaSq)
  };

  /// <summary>
  /// Computes Gaussian weight for spatial distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float SpatialWeight(int dx, int dy, float sigmaSq)
    => MathF.Exp(-(dx * dx + dy * dy) / (2.0f * sigmaSq));

  /// <summary>
  /// Computes Gaussian weight for color difference.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float RangeWeight(float colorDiffSq, float sigmaSq)
    => MathF.Exp(-colorDiffSq / (2.0f * sigmaSq));

  /// <summary>
  /// Computes squared color difference using luminance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float ColorDiffSquared<TWork>(in TWork a, in TWork b) where TWork : unmanaged, IColorSpace {
    var la = ColorConverter.GetLuminance(a) * 255f;
    var lb = ColorConverter.GetLuminance(b) * 255f;
    var diff = la - lb;
    return diff * diff;
  }

  /// <summary>
  /// Applies bilateral filtering using 5x5 neighborhood.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork ApplyBilateral5x5<TWork, TKey, TLerp>(
    in NeighborWindow<TWork, TKey> window,
    float spatialSigmaSq, float rangeSigmaSq,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {

    var center = window.P0P0.Work;

    // Accumulate weighted colors using lerp
    // Start with center pixel (weight = 1.0)
    var totalWeight = 1.0f;
    var spatialW = SpatialWeight(0, 0, spatialSigmaSq);
    var result = center;

    // Sample all 24 neighbors in 5x5 window (excluding center)
    result = AccumulateSample(result, window.M2M2.Work, center, -2, -2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M2M1.Work, center, -2, -1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M2P0.Work, center, -2, 0, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M2P1.Work, center, -2, 1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M2P2.Work, center, -2, 2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);

    result = AccumulateSample(result, window.M1M2.Work, center, -1, -2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M1M1.Work, center, -1, -1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M1P0.Work, center, -1, 0, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M1P1.Work, center, -1, 1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.M1P2.Work, center, -1, 2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);

    result = AccumulateSample(result, window.P0M2.Work, center, 0, -2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P0M1.Work, center, 0, -1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    // Skip center (0,0)
    result = AccumulateSample(result, window.P0P1.Work, center, 0, 1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P0P2.Work, center, 0, 2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);

    result = AccumulateSample(result, window.P1M2.Work, center, 1, -2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P1M1.Work, center, 1, -1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P1P0.Work, center, 1, 0, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P1P1.Work, center, 1, 1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P1P2.Work, center, 1, 2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);

    result = AccumulateSample(result, window.P2M2.Work, center, 2, -2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P2M1.Work, center, 2, -1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P2P0.Work, center, 2, 0, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P2P1.Work, center, 2, 1, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);
    result = AccumulateSample(result, window.P2P2.Work, center, 2, 2, spatialSigmaSq, rangeSigmaSq, ref totalWeight, lerp);

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork AccumulateSample<TWork, TLerp>(
    TWork accumulated, in TWork sample, in TWork center,
    int dy, int dx,
    float spatialSigmaSq, float rangeSigmaSq,
    ref float totalWeight,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {

    var spatialW = SpatialWeight(dx, dy, spatialSigmaSq);
    var colorDiff = ColorDiffSquared(center, sample);
    var rangeW = RangeWeight(colorDiff, rangeSigmaSq);
    var weight = spatialW * rangeW;

    if (weight < 0.001f)
      return accumulated;

    var newTotal = totalWeight + weight;
    var blendRatio = weight / newTotal;
    var blendW2 = (int)(blendRatio * 256f);

    if (blendW2 <= 0)
      return accumulated;

    totalWeight = newTotal;
    return lerp.Lerp(accumulated, sample, 256 - blendW2, blendW2);
  }
}

#endregion

#region Bilateral 2x Kernel

file readonly struct Bilateral2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float spatialSigmaSq, float rangeSigmaSq)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var result = BilateralHelpers.ApplyBilateral5x5<TWork, TKey, TLerp>(window, spatialSigmaSq, rangeSigmaSq, lerp);
    var encoded = encoder.Encode(result);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoded;
    row0[1] = encoded;
    row1[0] = encoded;
    row1[1] = encoded;
  }
}

#endregion

#region Bilateral 3x Kernel

file readonly struct Bilateral3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float spatialSigmaSq, float rangeSigmaSq)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var result = BilateralHelpers.ApplyBilateral5x5<TWork, TKey, TLerp>(window, spatialSigmaSq, rangeSigmaSq, lerp);
    var encoded = encoder.Encode(result);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    row0[0] = encoded;
    row0[1] = encoded;
    row0[2] = encoded;
    row1[0] = encoded;
    row1[1] = encoded;
    row1[2] = encoded;
    row2[0] = encoded;
    row2[1] = encoded;
    row2[2] = encoded;
  }
}

#endregion

#region Bilateral 4x Kernel

file readonly struct Bilateral4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float spatialSigmaSq, float rangeSigmaSq)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var result = BilateralHelpers.ApplyBilateral5x5<TWork, TKey, TLerp>(window, spatialSigmaSq, rangeSigmaSq, lerp);
    var encoded = encoder.Encode(result);

    for (var dy = 0; dy < 4; ++dy) {
      var row = destTopLeft + dy * destStride;
      row[0] = encoded;
      row[1] = encoded;
      row[2] = encoded;
      row[3] = encoded;
    }
  }
}

#endregion
