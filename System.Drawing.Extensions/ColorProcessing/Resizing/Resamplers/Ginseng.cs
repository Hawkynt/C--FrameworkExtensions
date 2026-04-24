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
/// Ginseng resampler — Jinc windowed by Jinc (EWA, radially symmetric).
/// </summary>
/// <remarks>
/// <para>
/// Standard mpv / madVR shader: <c>jinc(r) * jinc(r / (radius * cutoff))</c> with the Jinc
/// function <c>J₁(πr)/(πr)</c>. Unlike <c>EWA Lanczos</c> (which windows Jinc with a Lanczos
/// sinc), Ginseng windows Jinc with another Jinc, giving a perfectly radially-symmetric
/// envelope. This yields slightly softer passband but visibly less ringing on high-contrast
/// edges, particularly at moderate downscale ratios.
/// </para>
/// <para>
/// Well-known in the HTPC / video-upscaling community; ships with mpv as <c>scale=ewa_ginseng</c>
/// and is widely used for film / anime upscaling where Lanczos ringing is objectionable.
/// </para>
/// </remarks>
[ScalerInfo("Ginseng", Year = 2015,
  Description = "EWA Jinc-windowed Jinc — lower-ringing relative to EWA Lanczos",
  Category = ScalerCategory.Resampler)]
public readonly struct Ginseng : IKernelResampler, IResamplerWithSafePath {

  private readonly int _radius;

  /// <summary>
  /// Creates a Ginseng resampler with radius 3 (default).
  /// </summary>
  public Ginseng() : this(3) { }

  /// <summary>
  /// Creates a Ginseng resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Ginseng(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  /// <remarks>
  /// Ginseng is radially symmetric; this returns the radial profile
  /// <c>jinc(|x|) * jinc(|x|/radius)</c> for charting.
  /// </remarks>
  public float EvaluateWeight(float distance) {
    var r = MathF.Abs(distance);
    var radius = this.Radius;
    if (r >= radius) return 0f;
    return GinsengWeight(r, radius);
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
    => callback.Invoke(new GinsengKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Ginseng Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new GinsengKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, this.Radius, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      GinsengKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float GinsengWeight(float r, int radius) => JincMath.Jinc(r) * JincMath.Jinc(r / radius);
}

file readonly struct GinsengKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, int radius, bool useCenteredGrid)
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

    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var dx = fx - kx;
      var dy = fy - ky;
      var r = MathF.Sqrt(dx * dx + dy * dy);
      if (r >= radius) continue;
      var weight = Ginseng.GinsengWeight(r, radius);
      if (weight == 0f) continue;
      acc.AddMul(frame[srcXi + kx, srcYi + ky].Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

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
      var dx = fx - kx;
      var dy = fy - ky;
      var r = MathF.Sqrt(dx * dx + dy * dy);
      if (r >= radius) continue;
      var weight = Ginseng.GinsengWeight(r, radius);
      if (weight == 0f) continue;
      acc.AddMul(frame.GetUnchecked(srcXi + kx, srcYi + ky).Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -radius + 1, kxMaxExcl: radius + 1, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: -radius + 1, kyMaxExcl: radius + 1, this._scaleY, this._offsetY, sourceHeight, targetHeight);
}
