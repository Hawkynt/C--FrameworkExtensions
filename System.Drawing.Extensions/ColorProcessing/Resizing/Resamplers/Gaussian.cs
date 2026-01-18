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
/// Gaussian resampler - smooth Gaussian-weighted interpolation.
/// </summary>
/// <remarks>
/// <para>Uses a Gaussian bell curve as the filter kernel.</para>
/// <para>Produces very smooth results with controllable blur via sigma parameter.</para>
/// </remarks>
[ScalerInfo("Gaussian", Author = "Carl Friedrich Gauss", Year = 1809,
  Description = "Gaussian-weighted smooth interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Gaussian : IResampler {

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
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new GaussianKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight,
      this._sigma == 0f ? 0.5f : this._sigma,
      this._radius == 0 ? 2 : this._radius));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Gaussian Default => new();
}

file readonly struct GaussianKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float sigma, int radius)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
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

  // Precomputed scale factors
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;

  // Precomputed Gaussian coefficient
  private readonly float _coeff = -1f / (2f * sigma * sigma);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel center back to source coordinates
    var srcXf = (destX + 0.5f) * this._scaleX - 0.5f;
    var srcYf = (destY + 0.5f) * this._scaleY - 0.5f;

    // Integer coordinates
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    // Accumulate weighted colors from (2*radius)x(2*radius) kernel
    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var dx = fx - kx;
      var dy = fy - ky;
      var weight = this.GaussianWeight(dx, dy);
      if (weight < 1e-6f)
        continue;

      var pixel = frame[srcXi + kx, srcYi + ky].Work;
      acc.AddMul(pixel, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Computes the 2D Gaussian weight for given distances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float GaussianWeight(float dx, float dy)
    => MathF.Exp((dx * dx + dy * dy) * this._coeff);
}
