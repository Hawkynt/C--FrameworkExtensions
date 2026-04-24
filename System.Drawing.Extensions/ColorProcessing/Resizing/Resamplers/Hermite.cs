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
/// Hermite resampler - cubic interpolation with smooth falloff.
/// </summary>
/// <remarks>
/// <para>Uses cubic Hermite polynomial for smooth interpolation.</para>
/// <para>Produces smoother results than bilinear without the ringing of higher-order filters.</para>
/// </remarks>
[ScalerInfo("Hermite", Author = "Charles Hermite", Year = 1878,
  Description = "Cubic interpolation with smooth falloff", Category = ScalerCategory.Resampler)]
public readonly struct Hermite : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 1;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) {
    var x = MathF.Abs(distance);
    return x < 1f ? (2f * x - 3f) * x * x + 1f : 0f;
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
    => callback.Invoke(new HermiteKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Hermite Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new HermiteKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      HermiteKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

file readonly struct HermiteKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
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

    // Integer coordinates of top-left source pixel
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts for interpolation weights
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Hermite weights (smooth step function)
    var hx0 = HermiteWeight(-fx);
    var hx1 = HermiteWeight(1f - fx);
    var hy0 = HermiteWeight(-fy);
    var hy1 = HermiteWeight(1f - fy);

    // Compute combined weights
    var w00 = hx0 * hy0;
    var w10 = hx1 * hy0;
    var w01 = hx0 * hy1;
    var w11 = hx1 * hy1;

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
    var hx0 = HermiteWeight(-fx);
    var hx1 = HermiteWeight(1f - fx);
    var hy0 = HermiteWeight(-fy);
    var hy1 = HermiteWeight(1f - fy);
    var w00 = hx0 * hy0;
    var w10 = hx1 * hy0;
    var w01 = hx0 * hy1;
    var w11 = hx1 * hy1;

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
  /// Computes the Hermite weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float HermiteWeight(float x) {
    x = MathF.Abs(x);
    if (x < 1f)
      return (2f * x - 3f) * x * x + 1f;
    return 0f;
  }
}
