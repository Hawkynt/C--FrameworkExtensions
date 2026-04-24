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
/// Trilinear mipmap resampler — GPU-style two-level pyramid blend.
/// </summary>
/// <remarks>
/// <para>
/// Emulates a hardware trilinear filter: for each destination pixel, sample bilinearly at two
/// virtual mipmap levels bracketing the target ratio and lerp between them by the fractional
/// mip level. The coarser level is integrated on-the-fly as a 2×2 box of bilinear samples,
/// giving the exact semantics of a downsampled mipmap chain without storing pyramid textures.
/// </para>
/// <para>
/// Trilinear is the de-facto standard for real-time 3-D rendering: smoother than bilinear at
/// high downscale ratios (no moiré bands), much faster than Lanczos, predictable aliasing
/// profile. For 1:1 and upsampling it is bit-identical to plain bilinear.
/// </para>
/// <para>
/// Non-separable: the effective kernel shape depends on both scale and target position, so this
/// resampler does not implement <see cref="IKernelResampler"/> — the kernel chart is hidden in
/// consumer UIs that test for it.
/// </para>
/// </remarks>
[ScalerInfo("Trilinear", Description = "Two-level mipmap pyramid blend — GPU-style trilinear filter",
  Category = ScalerCategory.Resampler)]
public readonly struct Trilinear : IResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  /// <remarks>
  /// Nominal radius — the effective footprint extends up to the coarser virtual-mipmap level,
  /// so the real access radius is 2 (bilinear sample of a 2×2 box of bilinear samples = 3×3
  /// source-space span). The kernel reports its concrete per-instance radius via the kernel
  /// struct for safe-path region computation.
  /// </remarks>
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
    => callback.Invoke(new TrilinearKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Trilinear Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new TrilinearKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      TrilinearKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

file readonly struct TrilinearKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
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
  // Trilinear mip fractional: 0 = pure bilinear (upscale / 1:1), approaches 1 as downscale ratio
  // crosses each power-of-two boundary. We use log2(max(scaleX, scaleY)) clamped to [0, 1] to
  // keep the blend between just two virtual mip levels (base and 2× box-averaged).
  private readonly float _mipFraction;
  private readonly int _radius;

  public TrilinearKernel(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, bool useCenteredGrid) {
    this._sourceWidth = sourceWidth;
    this._sourceHeight = sourceHeight;
    this._targetWidth = targetWidth;
    this._targetHeight = targetHeight;
    this._scaleX = (float)sourceWidth / targetWidth;
    this._scaleY = (float)sourceHeight / targetHeight;
    this._offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
    this._offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;

    var maxScale = MathF.Max(this._scaleX, this._scaleY);
    // MathF.Log2 is .NET Core 2.0+ — derive portably via natural log so net35/40/45/48 compile.
    const float InvLn2 = 1.4426950408889634f;
    this._mipFraction = maxScale <= 1f ? 0f : MathF.Min(1f, MathF.Log(maxScale) * InvLn2);
    // Footprint: bilinear sample (radius 1) on top of a 2× box mip (radius 1 more) = radius 2.
    // For larger downscales we widen proportionally to keep aliasing bounded.
    this._radius = Math.Max(2, (int)MathF.Ceiling(maxScale * 0.5f) + 1);
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
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

    // Level 0: plain bilinear over the 2×2 source-pixel neighbourhood at (srcXf, srcYf).
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    Accum4F<TWork> level0 = default;
    level0.AddMul(Sample(frame, x0, y0, safe).Work, (1f - fx) * (1f - fy));
    level0.AddMul(Sample(frame, x0 + 1, y0, safe).Work, fx * (1f - fy));
    level0.AddMul(Sample(frame, x0, y0 + 1, safe).Work, (1f - fx) * fy);
    level0.AddMul(Sample(frame, x0 + 1, y0 + 1, safe).Work, fx * fy);

    // Trivial fast-path: upscale / 1:1 — mip fraction is zero, no need to compute level 1.
    if (this._mipFraction <= 0f) {
      dest[destY * destStride + destX] = encoder.Encode(level0.Result);
      return;
    }

    // Level 1: bilinear sample of the virtual 2× box-downsampled image. Each "level-1 texel"
    // is the average of a 2×2 source block; we compute them on-the-fly for the 2×2 footprint
    // of a second bilinear lookup centered on the same destination-back-projected position.
    // Equivalent to a 3×3 or 4×4 weighted sum over the source, directly expressed.
    var srcXf1 = (srcXf - 0.5f) * 0.5f;
    var srcYf1 = (srcYf - 0.5f) * 0.5f;
    var x1 = (int)MathF.Floor(srcXf1);
    var y1 = (int)MathF.Floor(srcYf1);
    var fx1 = srcXf1 - x1;
    var fy1 = srcYf1 - y1;

    Accum4F<TWork> level1 = default;
    // Expand each level-1 texel to its 2×2 source contributors, weighted by the bilinear lookup.
    AddLevel1Texel(ref level1, frame, 2 * x1, 2 * y1, (1f - fx1) * (1f - fy1), safe);
    AddLevel1Texel(ref level1, frame, 2 * x1 + 2, 2 * y1, fx1 * (1f - fy1), safe);
    AddLevel1Texel(ref level1, frame, 2 * x1, 2 * y1 + 2, (1f - fx1) * fy1, safe);
    AddLevel1Texel(ref level1, frame, 2 * x1 + 2, 2 * y1 + 2, fx1 * fy1, safe);

    // Blend the two levels by the fractional mip depth.
    var l0 = level0.Result;
    var l1 = level1.Result;
    var t = this._mipFraction;
    var oneMinusT = 1f - t;
    Accum4F<TWork> blend = default;
    blend.AddMul(l0, oneMinusT);
    blend.AddMul(l1, t);

    dest[destY * destStride + destX] = encoder.Encode(blend.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static NeighborPixel<TWork, TKey> Sample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int x, int y, bool safe)
    => safe ? frame.GetUnchecked(x, y) : frame[x, y];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void AddLevel1Texel(
    ref Accum4F<TWork> acc,
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int baseX, int baseY, float weight, bool safe) {
    // A level-1 texel is the average of a 2×2 source block. Distribute the outer weight evenly
    // (×0.25) to each contributor so the accumulator sees the identical weighted sum regardless
    // of whether we materialise level-1 explicitly.
    var w = weight * 0.25f;
    acc.AddMul(Sample(frame, baseX, baseY, safe).Work, w);
    acc.AddMul(Sample(frame, baseX + 1, baseY, safe).Work, w);
    acc.AddMul(Sample(frame, baseX, baseY + 1, safe).Work, w);
    acc.AddMul(Sample(frame, baseX + 1, baseY + 1, safe).Work, w);
  }

  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -this._radius + 1, kxMaxExcl: this._radius + 1, this._scaleX, this._offsetX, this._sourceWidth, this._targetWidth,
      kyMin: -this._radius + 1, kyMaxExcl: this._radius + 1, this._scaleY, this._offsetY, this._sourceHeight, this._targetHeight);
}
