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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Kriging-based bilateral upscaling resampler.
/// </summary>
/// <remarks>
/// <para>Combines kriging interpolation with bilateral filtering for edge-aware results.</para>
/// <para>Spatial weights are modulated by color similarity (range kernel).</para>
/// <para>Produces smooth gradients while preserving sharp edges.</para>
/// <para>Based on geostatistical interpolation adapted for image processing.</para>
/// </remarks>
[ScalerInfo("KrigBilateral",
  Description = "Kriging-based bilateral edge-aware upscaling", Category = ScalerCategory.Resampler)]
public readonly struct KrigBilateral : IResampler {

  private readonly float _spatialSigma;
  private readonly float _rangeSigma;

  /// <summary>
  /// Creates a KrigBilateral resampler with default parameters.
  /// </summary>
  public KrigBilateral() : this(1.5f, 0.1f) { }

  /// <summary>
  /// Creates a KrigBilateral resampler with custom parameters.
  /// </summary>
  /// <param name="spatialSigma">Spatial kernel sigma. Higher = more blur.</param>
  /// <param name="rangeSigma">Range kernel sigma. Lower = more edge preservation.</param>
  public KrigBilateral(float spatialSigma, float rangeSigma) {
    this._spatialSigma = Math.Max(spatialSigma, 0.1f);
    this._rangeSigma = Math.Max(rangeSigma, 0.01f);
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new KrigBilateralKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._spatialSigma, this._rangeSigma, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static KrigBilateral Default => new();

  /// <summary>
  /// Gets a configuration optimized for edge preservation.
  /// </summary>
  public static KrigBilateral EdgePreserving => new(1.0f, 0.05f);

  /// <summary>
  /// Gets a configuration optimized for smooth results.
  /// </summary>
  public static KrigBilateral Smooth => new(2.0f, 0.2f);
}

file readonly struct KrigBilateralKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float spatialSigma, float rangeSigma, bool useCenteredGrid = true)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  // Precomputed scale factors and offsets for zero-cost grid centering
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;
  private readonly float _spatialFactor = -0.5f / (spatialSigma * spatialSigma);
  private readonly float _rangeFactor = -0.5f / (rangeSigma * rangeSigma);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel back to source coordinates
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Sample 4x4 neighborhood
    var colors = stackalloc TWork[16];
    colors[0] = frame[x0 - 1, y0 - 1].Work;
    colors[1] = frame[x0, y0 - 1].Work;
    colors[2] = frame[x0 + 1, y0 - 1].Work;
    colors[3] = frame[x0 + 2, y0 - 1].Work;
    colors[4] = frame[x0 - 1, y0].Work;
    colors[5] = frame[x0, y0].Work;
    colors[6] = frame[x0 + 1, y0].Work;
    colors[7] = frame[x0 + 2, y0].Work;
    colors[8] = frame[x0 - 1, y0 + 1].Work;
    colors[9] = frame[x0, y0 + 1].Work;
    colors[10] = frame[x0 + 1, y0 + 1].Work;
    colors[11] = frame[x0 + 2, y0 + 1].Work;
    colors[12] = frame[x0 - 1, y0 + 2].Work;
    colors[13] = frame[x0, y0 + 2].Work;
    colors[14] = frame[x0 + 1, y0 + 2].Work;
    colors[15] = frame[x0 + 2, y0 + 2].Work;

    // Compute initial estimate using bilinear (for range kernel reference)
    var bilinearEst = BilinearInterpolate(colors[5], colors[6], colors[9], colors[10], fx, fy);
    var refLuma = ColorConverter.GetLuminance(in bilinearEst);

    // Compute bilateral weights combining spatial and range components
    var weights = stackalloc float[16];
    var totalWeight = 0f;

    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var idx = ky * 4 + kx;

      // Spatial distance
      var dx = kx - 1 - fx;
      var dy = ky - 1 - fy;
      var spatialDist2 = dx * dx + dy * dy;
      var spatialWeight = MathF.Exp(spatialDist2 * this._spatialFactor);

      // Range distance (luminance difference)
      var luma = ColorConverter.GetLuminance(in colors[idx]);
      var rangeDist2 = (luma - refLuma) * (luma - refLuma);
      var rangeWeight = MathF.Exp(rangeDist2 * this._rangeFactor);

      // Combined bilateral weight
      var weight = spatialWeight * rangeWeight;

      // Apply kriging-like correction based on local variance
      // This helps with smooth gradient areas
      weights[idx] = weight;
      totalWeight += weight;
    }

    // Normalize weights
    if (totalWeight > 0.0001f) {
      var invTotal = 1f / totalWeight;
      for (var i = 0; i < 16; ++i)
        weights[i] *= invTotal;
    } else {
      // Fallback to uniform if all weights are near zero
      for (var i = 0; i < 16; ++i)
        weights[i] = 1f / 16f;
    }

    // Apply kriging correction: compute residual variance
    var meanLuma = 0f;
    for (var i = 0; i < 16; ++i)
      meanLuma += ColorConverter.GetLuminance(in colors[i]) * weights[i];

    var variance = 0f;
    for (var i = 0; i < 16; ++i) {
      var diff = ColorConverter.GetLuminance(in colors[i]) - meanLuma;
      variance += diff * diff * weights[i];
    }

    // Adjust weights based on local variance (kriging nugget effect)
    // In low-variance areas, trust spatial interpolation more
    // In high-variance areas, trust range filtering more
    var varianceFactor = MathF.Min(variance * 10f, 1f);
    if (varianceFactor < 0.5f) {
      // Low variance: blend towards spatial-only weights
      var spatialWeights = stackalloc float[16];
      var spatialTotal = 0f;
      for (var ky = 0; ky < 4; ++ky)
      for (var kx = 0; kx < 4; ++kx) {
        var idx = ky * 4 + kx;
        var dx = kx - 1 - fx;
        var dy = ky - 1 - fy;
        var dist2 = dx * dx + dy * dy;
        spatialWeights[idx] = MathF.Exp(dist2 * this._spatialFactor);
        spatialTotal += spatialWeights[idx];
      }

      if (spatialTotal > 0.0001f) {
        var invSpatial = 1f / spatialTotal;
        var blend = (0.5f - varianceFactor) * 2f; // 0 when variance=0.5, 1 when variance=0
        for (var i = 0; i < 16; ++i) {
          var spatialW = spatialWeights[i] * invSpatial;
          weights[i] = weights[i] * (1f - blend * 0.5f) + spatialW * blend * 0.5f;
        }
      }
    }

    // Accumulate final result
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 16; ++i)
      if (weights[i] > 0.0001f)
        acc.AddMul(colors[i], weights[i]);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BilinearInterpolate(TWork c00, TWork c10, TWork c01, TWork c11, float fx, float fy) {
    var w00 = (1f - fx) * (1f - fy);
    var w10 = fx * (1f - fy);
    var w01 = (1f - fx) * fy;
    var w11 = fx * fy;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);
    return acc.Result;
  }

}
