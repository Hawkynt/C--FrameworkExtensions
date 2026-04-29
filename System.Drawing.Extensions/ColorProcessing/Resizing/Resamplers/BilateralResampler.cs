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
using System.Drawing;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing;
using ColorConverter = Hawkynt.ColorProcessing.ColorMath.ColorConverter;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Free-factor bilateral resampler — edge-preserving smoothing for arbitrary target sizes.
/// </summary>
/// <remarks>
/// <para>Combines a spatial Gaussian (distance-based) with a range Gaussian (luminance-difference
/// based) so contributions from neighbours that differ strongly in luminance from the reference
/// pixel are damped. Result: edges are preserved while smooth regions are smoothed.</para>
/// <para>Companion to the fixed-factor <see cref="Hawkynt.ColorProcessing.Resizing.Rescalers.Bilateral"/>
/// rescaler — the math is the same; this variant exposes free target dimensions via the
/// kernel-resampler scaffold (Bicubic/Lanczos shape).</para>
/// <para>Parameters: σ<sub>s</sub> (spatial pixels) and σ<sub>r</sub> (luminance, 0..1). Larger
/// σ<sub>s</sub> blurs more; smaller σ<sub>r</sub> preserves edges more aggressively.</para>
/// <para>Reference: Tomasi &amp; Manduchi 1998, "Bilateral filtering for gray and color images",
/// ICCV.</para>
/// </remarks>
[ScalerInfo("BilateralResampler", Author = "Tomasi & Manduchi", Year = 1998,
  Description = "Edge-preserving free-factor resampler (spatial × range Gaussians)",
  Category = ScalerCategory.Resampler)]
public readonly struct BilateralResampler : IKernelResampler, IResamplerWithSafePath {

  /// <summary>Default radius (2 = 5×5 sampling neighbourhood).</summary>
  public const int DefaultRadius = 2;

  /// <summary>Default spatial σ in pixels (1.5).</summary>
  public const float DefaultSpatialSigma = 1.5f;

  /// <summary>Default range σ on normalised luminance (0.1 ≈ 25/255).</summary>
  public const float DefaultRangeSigma = 0.1f;

  private readonly int _radius;
  private readonly float _spatialSigma;
  private readonly float _rangeSigma;

  /// <summary>
  /// Creates a BilateralResampler with default radius and σ values.
  /// </summary>
  public BilateralResampler() : this(DefaultRadius, DefaultSpatialSigma, DefaultRangeSigma) { }

  /// <summary>
  /// Creates a BilateralResampler with custom radius and default σ values.
  /// </summary>
  /// <param name="radius">Sampling radius (1, 2, or 3 typical; 2 = 5×5 window).</param>
  public BilateralResampler(int radius) : this(radius, DefaultSpatialSigma, DefaultRangeSigma) { }

  /// <summary>
  /// Creates a BilateralResampler with custom radius and σ values.
  /// </summary>
  /// <param name="radius">Sampling radius (1, 2, or 3 typical; 2 = 5×5 window).</param>
  /// <param name="spatialSigma">Spatial Gaussian σ in pixels (positive).</param>
  /// <param name="rangeSigma">Range Gaussian σ on normalised luminance ∈ (0, 1] (positive).</param>
  public BilateralResampler(int radius, float spatialSigma, float rangeSigma) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(spatialSigma);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rangeSigma);
    this._radius = radius;
    this._spatialSigma = spatialSigma;
    this._rangeSigma = rangeSigma;
  }

  /// <summary>Gets the spatial Gaussian σ.</summary>
  public float SpatialSigma => this._spatialSigma == 0f ? DefaultSpatialSigma : this._spatialSigma;

  /// <summary>Gets the range Gaussian σ.</summary>
  public float RangeSigma => this._rangeSigma == 0f ? DefaultRangeSigma : this._rangeSigma;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? DefaultRadius : this._radius;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  /// <remarks>
  /// Returns the spatial-only weight; the range weight is signal-dependent and cannot be
  /// expressed by a 1-D kernel response. Charts of <c>EvaluateWeight</c> will show only the
  /// spatial Gaussian — that is the correct shape when no edges are present.
  /// </remarks>
  public float EvaluateWeight(float distance) {
    var d = MathF.Abs(distance);
    if (d >= this.Radius) return 0f;
    var sigmaSq = this.SpatialSigma * this.SpatialSigma;
    return MathF.Exp(-(d * d) / (2f * sigmaSq));
  }

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
    => callback.Invoke(new BilateralResamplerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, this.SpatialSigma, this.RangeSigma, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static BilateralResampler Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new BilateralResamplerKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, this.Radius, this.SpatialSigma, this.RangeSigma, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      BilateralResamplerKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

file readonly struct BilateralResamplerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  int radius, float spatialSigma, float rangeSigma, bool useCenteredGrid)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => radius;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;
  private readonly float _twoSpatialSigmaSq = 2f * spatialSigma * spatialSigma;
  private readonly float _twoRangeSigmaSq = 2f * rangeSigma * rangeSigma;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    // Use the nearest source pixel as the bilateral reference (deterministic, cheap).
    var center = frame[srcXi, srcYi].Work;
    var centerLum = ColorConverter.GetLuminance(center);

    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var sample = frame[srcXi + kx, srcYi + ky].Work;
      var dx = fx - kx;
      var dy = fy - ky;
      var spatial = MathF.Exp(-(dx * dx + dy * dy) / this._twoSpatialSigmaSq);
      var lumDiff = ColorConverter.GetLuminance(sample) - centerLum;
      var range = MathF.Exp(-(lumDiff * lumDiff) / this._twoRangeSigmaSq);
      var weight = spatial * range;
      if (weight == 0f) continue;
      acc.AddMul(sample, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void ResampleUnchecked(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    var center = frame.GetUnchecked(srcXi, srcYi).Work;
    var centerLum = ColorConverter.GetLuminance(center);

    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var sample = frame.GetUnchecked(srcXi + kx, srcYi + ky).Work;
      var dx = fx - kx;
      var dy = fy - ky;
      var spatial = MathF.Exp(-(dx * dx + dy * dy) / this._twoSpatialSigmaSq);
      var lumDiff = ColorConverter.GetLuminance(sample) - centerLum;
      var range = MathF.Exp(-(lumDiff * lumDiff) / this._twoRangeSigmaSq);
      var weight = spatial * range;
      if (weight == 0f) continue;
      acc.AddMul(sample, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -radius + 1, kxMaxExcl: radius + 1, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: -radius + 1, kyMaxExcl: radius + 1, this._scaleY, this._offsetY, sourceHeight, targetHeight);
}
