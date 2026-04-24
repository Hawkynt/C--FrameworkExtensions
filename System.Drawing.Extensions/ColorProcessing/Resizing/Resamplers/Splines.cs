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

#region Spline16

/// <summary>
/// Spline16 resampler (4-tap spline interpolation).
/// </summary>
/// <remarks>
/// <para>Standard cubic spline with radius 2, producing smooth results.</para>
/// <para>Used by VLC media player for high-quality video scaling.</para>
/// </remarks>
[ScalerInfo("Spline16", Year = 1990,
  Description = "4-tap spline interpolation filter", Category = ScalerCategory.Resampler)]
public readonly struct Spline16 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => SplineMath.Spline16Weight(distance);

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
    => callback.Invoke(new SplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, SplineType.Spline16, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Spline16 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _SplineSafePath.Dispatch(SplineType.Spline16, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region Spline36

/// <summary>
/// Spline36 resampler (6-tap spline interpolation).
/// </summary>
/// <remarks>
/// <para>Higher-order spline with radius 3, sharper than Spline16.</para>
/// <para>Used by FFmpeg for high-quality video scaling.</para>
/// </remarks>
[ScalerInfo("Spline36", Year = 1990,
  Description = "6-tap spline interpolation filter, sharper than Spline16", Category = ScalerCategory.Resampler)]
public readonly struct Spline36 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => SplineMath.Spline36Weight(distance);

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
    => callback.Invoke(new SplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, SplineType.Spline36, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Spline36 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _SplineSafePath.Dispatch(SplineType.Spline36, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region Spline64

/// <summary>
/// Spline64 resampler (8-tap spline interpolation).
/// </summary>
/// <remarks>
/// <para>Highest-order spline with radius 4, very sharp results.</para>
/// <para>Used by ImageMagick for high-quality image scaling.</para>
/// </remarks>
[ScalerInfo("Spline64", Year = 1990,
  Description = "8-tap spline interpolation filter, sharpest spline variant", Category = ScalerCategory.Resampler)]
public readonly struct Spline64 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 4;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => SplineMath.Spline64Weight(distance);

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
    => callback.Invoke(new SplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, SplineType.Spline64, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Spline64 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _SplineSafePath.Dispatch(SplineType.Spline64, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region Shared Kernel Infrastructure

file enum SplineType {
  Spline16,
  Spline36,
  Spline64
}

file readonly struct SplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, SplineType splineType, bool useCenteredGrid = true)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => splineType switch {
    SplineType.Spline16 => 2,
    SplineType.Spline36 => 3,
    SplineType.Spline64 => 4,
    _ => 2
  };

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

    // Integer coordinates
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    var r = this.Radius;

    // Edge path: bounds-checked indexer. Pipeline routes only edge-band pixels here; the safe
    // interior runs through ResampleUnchecked (below).
    Accum4F<TWork> acc = default;
    for (var ky = -r + 1; ky <= r; ++ky)
    for (var kx = -r + 1; kx <= r; ++kx) {
      var weight = this.Weight(fx - kx) * this.Weight(fy - ky);
      if (weight == 0f) continue;
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
    var r = this.Radius;

    Accum4F<TWork> acc = default;
    for (var ky = -r + 1; ky <= r; ++ky)
    for (var kx = -r + 1; kx <= r; ++kx) {
      var weight = this.Weight(fx - kx) * this.Weight(fy - ky);
      if (weight == 0f) continue;
      acc.AddMul(frame.GetUnchecked(srcXi + kx, srcYi + ky).Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public Rectangle GetSafeDestinationRegion() {
    var r = this.Radius;
    return ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -r + 1, kxMaxExcl: r + 1, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: -r + 1, kyMaxExcl: r + 1, this._scaleY, this._offsetY, sourceHeight, targetHeight);
  }

  /// <summary>
  /// Computes the spline weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Weight(float x) => splineType switch {
    SplineType.Spline16 => Spline16Weight(x),
    SplineType.Spline36 => Spline36Weight(x),
    SplineType.Spline64 => Spline64Weight(x),
    _ => 0f
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Spline16Weight(float x) => SplineMath.Spline16Weight(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Spline36Weight(float x) => SplineMath.Spline36Weight(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Spline64Weight(float x) => SplineMath.Spline64Weight(x);
}

file static class _SplineSafePath {
  public static Bitmap Dispatch(SplineType type, Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new SplineKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, type, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      SplineKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

internal static class SplineMath {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Spline16Weight(float x) {
    x = MathF.Abs(x);
    if (x < 1f)
      return ((x - 9f / 5f) * x - 1f / 5f) * x + 1f;
    if (x < 2f) {
      x -= 1f;
      return ((-1f / 3f * x + 4f / 5f) * x - 7f / 15f) * x;
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Spline36Weight(float x) {
    x = MathF.Abs(x);
    if (x < 1f)
      return ((13f / 11f * x - 453f / 209f) * x - 3f / 209f) * x + 1f;
    if (x < 2f) {
      x -= 1f;
      return ((-6f / 11f * x + 270f / 209f) * x - 156f / 209f) * x;
    }
    if (x < 3f) {
      x -= 2f;
      return ((1f / 11f * x - 45f / 209f) * x + 26f / 209f) * x;
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Spline64Weight(float x) {
    x = MathF.Abs(x);
    if (x < 1f)
      return ((49f / 41f * x - 6387f / 2911f) * x - 3f / 2911f) * x + 1f;
    if (x < 2f) {
      x -= 1f;
      return ((-24f / 41f * x + 4032f / 2911f) * x - 2328f / 2911f) * x;
    }
    if (x < 3f) {
      x -= 2f;
      return ((6f / 41f * x - 1008f / 2911f) * x + 582f / 2911f) * x;
    }
    if (x < 4f) {
      x -= 3f;
      return ((-1f / 41f * x + 168f / 2911f) * x - 97f / 2911f) * x;
    }
    return 0f;
  }
}

#endregion
