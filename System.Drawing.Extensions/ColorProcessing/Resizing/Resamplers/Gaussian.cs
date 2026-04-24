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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Gaussian resampler - smooth Gaussian-weighted interpolation.
/// </summary>
/// <remarks>
/// <para>Uses a Gaussian bell curve as the filter kernel.</para>
/// <para>Produces very smooth results with controllable blur via sigma parameter.</para>
/// </remarks>
[ScalerInfo("Gaussian", Author = "Carl Friedrich Gauss", Year = 1809,
  Description = "Gaussian-weighted smooth interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Gaussian : IKernelResampler, IResamplerWithSafePath {

  private readonly float _sigma;
  private readonly int _radius;

  /// <summary>
  /// Creates a Gaussian resampler with default parameters (sigma=0.5, radius=2).
  /// </summary>
  public Gaussian() : this(0.5f, 2) { }

  /// <summary>
  /// Creates a Gaussian resampler with custom sigma and default radius.
  /// </summary>
  /// <param name="sigma">The standard deviation of the Gaussian. Larger values produce more blur.</param>
  public Gaussian(float sigma) : this(sigma, Math.Max(2, (int)MathF.Ceiling(sigma * 3f))) { }

  /// <summary>
  /// Creates a Gaussian resampler with custom sigma and radius.
  /// </summary>
  /// <param name="sigma">The standard deviation of the Gaussian. Larger values produce more blur.</param>
  /// <param name="radius">The kernel radius. Should be at least 3*sigma for accurate results.</param>
  public Gaussian(float sigma, int radius) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sigma);
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._sigma = sigma;
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 2 : this._radius;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) {
    var s = this._sigma == 0f ? 0.5f : this._sigma;
    var x = MathF.Abs(distance);
    if (x >= this.Radius) return 0f;
    var coeff = -1f / (2f * s * s);
    return MathF.Exp(x * x * coeff);
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
    => callback.Invoke(new GaussianKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight,
      this._sigma == 0f ? 0.5f : this._sigma,
      this._radius == 0 ? 2 : this._radius, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Gaussian Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new GaussianKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight,
      this._sigma == 0f ? 0.5f : this._sigma,
      this._radius == 0 ? 2 : this._radius, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      GaussianKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

file readonly struct GaussianKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float sigma, int radius, bool useCenteredGrid)
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

  // Precomputed scale factors and offsets for zero-cost grid centering
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;

  // Precomputed Gaussian coefficient
  private readonly float _coeff = -1f / (2f * sigma * sigma);

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

    // Integer coordinates
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    // Edge path: bounds-checked indexer. Pipeline routes only edge-band pixels here; the safe
    // interior runs through ResampleUnchecked (below).
    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var weight = this.GaussianWeight(fx - kx, fy - ky);
      if (weight < 1e-6f) continue;
      acc.AddMul(frame[srcXi + kx, srcYi + ky].Work, weight);
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

    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var weight = this.GaussianWeight(fx - kx, fy - ky);
      if (weight < 1e-6f) continue;
      acc.AddMul(frame.GetUnchecked(srcXi + kx, srcYi + ky).Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -radius + 1, kxMaxExcl: radius + 1, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: -radius + 1, kyMaxExcl: radius + 1, this._scaleY, this._offsetY, sourceHeight, targetHeight);

  /// <summary>
  /// Computes the 2D Gaussian weight for given distances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float GaussianWeight(float dx, float dy)
    => MathF.Exp((dx * dx + dy * dy) * this._coeff);
}
