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
/// Area (source-box averaging) resampler — the OpenCV <c>INTER_AREA</c> / PIL <c>BOX</c> algorithm.
/// </summary>
/// <remarks>
/// <para>
/// For each destination pixel, integrates the average colour of the rectangular source footprint
/// that it covers — i.e. each output sample equals the area-weighted mean of the source pixels
/// whose unit squares overlap the destination pixel's back-projected unit square.
/// </para>
/// <para>
/// This is the de-facto industry-standard arbitrary-ratio downsampling filter: aliasing-free by
/// construction (no kernel can leak energy outside the footprint) and moiré-free on photographic
/// content. Noticeably sharper than Gaussian pre-filter + subsample for the same ratio.
/// </para>
/// <para>
/// For upsampling (target &gt; source) the footprint is smaller than one source pixel and Area
/// degenerates into nearest-neighbour — use Bilinear/Bicubic/Lanczos for upsampling instead.
/// </para>
/// </remarks>
[ScalerInfo("Area", Description = "Source-box area-averaging filter (INTER_AREA / PIL BOX) for arbitrary-ratio downsampling",
  Category = ScalerCategory.Resampler)]
public readonly struct Area : IResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  /// <remarks>
  /// Nominal radius — the actual footprint is determined at invocation time from the source/target
  /// ratio. This value is the smallest safe kernel half-width for 1:1 sampling; larger ratios
  /// dynamically extend the footprint inside the kernel and also adjust the safe-destination
  /// region via the per-instance <c>radius</c> field.
  /// </remarks>
  public int Radius => 1;

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
    => callback.Invoke(new AreaKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Area Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new AreaKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      AreaKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

file readonly struct AreaKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private readonly int _sourceWidth;
  private readonly int _sourceHeight;
  private readonly int _targetWidth;
  private readonly int _targetHeight;
  private readonly float _scaleX;
  private readonly float _scaleY;
  private readonly float _offsetX;
  private readonly float _offsetY;
  private readonly int _radius;

  public AreaKernel(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, bool useCenteredGrid) {
    this._sourceWidth = sourceWidth;
    this._sourceHeight = sourceHeight;
    this._targetWidth = targetWidth;
    this._targetHeight = targetHeight;
    this._scaleX = (float)sourceWidth / targetWidth;
    this._scaleY = (float)sourceHeight / targetHeight;
    this._offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
    this._offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;
    // Footprint extends at most ceil(scale) source pixels wide; radius is half-span rounded up.
    // For upscaling (scale < 1) we still need at least radius 1 to cover the two straddling pixels.
    var halfSpanX = Math.Max(1, (int)MathF.Ceiling(this._scaleX * 0.5f) + 1);
    var halfSpanY = Math.Max(1, (int)MathF.Ceiling(this._scaleY * 0.5f) + 1);
    this._radius = Math.Max(halfSpanX, halfSpanY);
  }

  public int Radius => this._radius;
  public int SourceWidth => this._sourceWidth;
  public int SourceHeight => this._sourceHeight;
  public int TargetWidth => this._targetWidth;
  public int TargetHeight => this._targetHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    this.ResampleCore(frame, destX, destY, dest, destStride, encoder, safe: false);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void ResampleUnchecked(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    this.ResampleCore(frame, destX, destY, dest, destStride, encoder, safe: true);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe void ResampleCore(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder,
    bool safe) {
    // Back-project the destination pixel's unit square to a source-space rectangle.
    // Centered grid: dest pixel (d + 0.5) maps to src (d + 0.5)*scale - 0.5 → edges at ±0.5*scale.
    var centerX = destX * this._scaleX + this._offsetX + 0.5f;
    var centerY = destY * this._scaleY + this._offsetY + 0.5f;
    var halfX = 0.5f * this._scaleX;
    var halfY = 0.5f * this._scaleY;
    // Upsampling case (scale < 1): footprint < 1 pixel. Widen to a minimum half-width of 0.5 so
    // we still straddle at least two source pixels and avoid nearest-neighbour hard-edge.
    if (halfX < 0.5f) halfX = 0.5f;
    if (halfY < 0.5f) halfY = 0.5f;

    var left = centerX - halfX;
    var right = centerX + halfX;
    var top = centerY - halfY;
    var bottom = centerY + halfY;

    var xMin = (int)MathF.Floor(left);
    var xMaxExcl = (int)MathF.Ceiling(right);
    var yMin = (int)MathF.Floor(top);
    var yMaxExcl = (int)MathF.Ceiling(bottom);

    // Accum4F normalises internally by the accumulated weight sum, so the raw weighted sum here
    // is automatically rescaled to the area-weighted mean at Result time — no explicit division.
    Accum4F<TWork> acc = default;
    for (var sy = yMin; sy < yMaxExcl; ++sy) {
      var rowTop = MathF.Max(top, sy);
      var rowBottom = MathF.Min(bottom, sy + 1);
      var wy = rowBottom - rowTop;
      if (wy <= 0f) continue;
      for (var sx = xMin; sx < xMaxExcl; ++sx) {
        var colLeft = MathF.Max(left, sx);
        var colRight = MathF.Min(right, sx + 1);
        var wx = colRight - colLeft;
        if (wx <= 0f) continue;
        var w = wx * wy;
        var pixel = safe ? frame.GetUnchecked(sx, sy) : frame[sx, sy];
        acc.AddMul(pixel.Work, w);
      }
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -this._radius + 1, kxMaxExcl: this._radius + 1, this._scaleX, this._offsetX, this._sourceWidth, this._targetWidth,
      kyMin: -this._radius + 1, kyMaxExcl: this._radius + 1, this._scaleY, this._offsetY, this._sourceHeight, this._targetHeight);
}
