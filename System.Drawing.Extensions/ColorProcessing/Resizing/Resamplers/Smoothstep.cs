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
/// Smoothstep interpolation resampler mode.
/// </summary>
public enum SmoothstepMode {
  /// <summary>
  /// Standard smoothstep (3rd-degree Hermite polynomial): t² * (3 - 2t)
  /// </summary>
  Standard,

  /// <summary>
  /// Smootheststep (7th-degree polynomial) for even smoother results.
  /// </summary>
  Smoothest
}

/// <summary>
/// Smoothstep interpolation resampler.
/// </summary>
/// <remarks>
/// <para>Uses a 2x2 kernel with Hermite polynomial weighting.</para>
/// <para>Standard mode uses 3rd-degree polynomial: t² * (3 - 2t)</para>
/// <para>Smoothest mode uses 7th-degree polynomial for C² continuity.</para>
/// </remarks>
[ScalerInfo("Smoothstep", Description = "2x2 Hermite polynomial interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Smoothstep : IKernelResampler, IResamplerWithSafePath {

  private readonly SmoothstepMode _mode;

  /// <summary>
  /// Creates a Smoothstep resampler with default mode.
  /// </summary>
  public Smoothstep() : this(SmoothstepMode.Standard) { }

  /// <summary>
  /// Creates a Smoothstep resampler with the specified mode.
  /// </summary>
  /// <param name="mode">The smoothstep variant to use.</param>
  public Smoothstep(SmoothstepMode mode) => this._mode = mode;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 1;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) {
    var d = MathF.Abs(distance);
    if (d >= 1f) return 0f;
    if (this._mode == SmoothstepMode.Standard) {
      // 1 - smoothstep(d) = 1 - d²(3 - 2d)
      return 1f - d * d * (3f - 2f * d);
    }
    // 1 - smootheststep(d); smootheststep(d) = d⁴(d(d(-20d+70)-84)+35)
    var d2 = d * d;
    var d4 = d2 * d2;
    return 1f - d4 * (d * (d * (-20f * d + 70f) - 84f) + 35f);
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
    => this._mode == SmoothstepMode.Standard
      ? callback.Invoke(new SmoothstepKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
        sourceWidth, sourceHeight, targetWidth, targetHeight, useCenteredGrid))
      : callback.Invoke(new SmoothestKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
        sourceWidth, sourceHeight, targetWidth, targetHeight, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration (standard smoothstep).
  /// </summary>
  public static Smoothstep Default => new();

  /// <summary>
  /// Gets the smoothest configuration (7th-degree polynomial).
  /// </summary>
  public static Smoothstep Smoothest => new(SmoothstepMode.Smoothest);

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    if (this._mode == SmoothstepMode.Standard) {
      var kernel = new SmoothstepKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
        source.Width, source.Height, targetWidth, targetHeight, useCenteredGrid);
      return BitmapScalerExtensions.InvokeSafePathResampler<
        SmoothstepKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
      >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
    } else {
      var kernel = new SmoothestKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
        source.Width, source.Height, targetWidth, targetHeight, useCenteredGrid);
      return BitmapScalerExtensions.InvokeSafePathResampler<
        SmoothestKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
      >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
    }
  }
}

file readonly struct SmoothstepKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, bool useCenteredGrid)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  // Precomputed scale factors and offsets for zero-cost grid centering
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
    // Map destination pixel back to source coordinates
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Apply smoothstep: t² * (3 - 2t) = t² * (-2t + 3)
    var sx = fx * fx * (-2f * fx + 3f);
    var sy = fy * fy * (-2f * fy + 3f);

    // Bilinear interpolation with smoothstep weights
    var w00 = (1f - sx) * (1f - sy);
    var w10 = sx * (1f - sy);
    var w01 = (1f - sx) * sy;
    var w11 = sx * sy;

    // Edge path: bounds-checked indexer. Pipeline routes only edge-band pixels here; the safe
    // interior runs through ResampleUnchecked (below).
    Accum4F<TWork> acc = default;
    acc.AddMul(frame[x0, y0].Work, w00);
    acc.AddMul(frame[x0 + 1, y0].Work, w10);
    acc.AddMul(frame[x0, y0 + 1].Work, w01);
    acc.AddMul(frame[x0 + 1, y0 + 1].Work, w11);

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
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;
    var sx = fx * fx * (-2f * fx + 3f);
    var sy = fy * fy * (-2f * fy + 3f);
    var w00 = (1f - sx) * (1f - sy);
    var w10 = sx * (1f - sy);
    var w01 = (1f - sx) * sy;
    var w11 = sx * sy;

    Accum4F<TWork> acc = default;
    acc.AddMul(frame.GetUnchecked(x0, y0).Work, w00);
    acc.AddMul(frame.GetUnchecked(x0 + 1, y0).Work, w10);
    acc.AddMul(frame.GetUnchecked(x0, y0 + 1).Work, w01);
    acc.AddMul(frame.GetUnchecked(x0 + 1, y0 + 1).Work, w11);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: 0, kxMaxExcl: 2, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: 0, kyMaxExcl: 2, this._scaleY, this._offsetY, sourceHeight, targetHeight);
}

file readonly struct SmoothestKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, bool useCenteredGrid)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  // Precomputed scale factors and offsets for zero-cost grid centering
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
    // Map destination pixel back to source coordinates
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Apply smootheststep: t⁴ * (t * (t * (-20t + 70) - 84) + 35)
    var sx = SmoothestStep(fx);
    var sy = SmoothestStep(fy);

    // Bilinear interpolation with smootheststep weights
    var w00 = (1f - sx) * (1f - sy);
    var w10 = sx * (1f - sy);
    var w01 = (1f - sx) * sy;
    var w11 = sx * sy;

    // Edge path: bounds-checked indexer. Pipeline routes only edge-band pixels here; the safe
    // interior runs through ResampleUnchecked (below).
    Accum4F<TWork> acc = default;
    acc.AddMul(frame[x0, y0].Work, w00);
    acc.AddMul(frame[x0 + 1, y0].Work, w10);
    acc.AddMul(frame[x0, y0 + 1].Work, w01);
    acc.AddMul(frame[x0 + 1, y0 + 1].Work, w11);

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
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;
    var sx = SmoothestStep(fx);
    var sy = SmoothestStep(fy);
    var w00 = (1f - sx) * (1f - sy);
    var w10 = sx * (1f - sy);
    var w01 = (1f - sx) * sy;
    var w11 = sx * sy;

    Accum4F<TWork> acc = default;
    acc.AddMul(frame.GetUnchecked(x0, y0).Work, w00);
    acc.AddMul(frame.GetUnchecked(x0 + 1, y0).Work, w10);
    acc.AddMul(frame.GetUnchecked(x0, y0 + 1).Work, w01);
    acc.AddMul(frame.GetUnchecked(x0 + 1, y0 + 1).Work, w11);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: 0, kxMaxExcl: 2, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: 0, kyMaxExcl: 2, this._scaleY, this._offsetY, sourceHeight, targetHeight);

  /// <summary>
  /// 7th-degree polynomial smootheststep.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float SmoothestStep(float t) {
    var t2 = t * t;
    var t4 = t2 * t2;
    return t4 * (t * (t * (-20f * t + 70f) - 84f) + 35f);
  }
}
