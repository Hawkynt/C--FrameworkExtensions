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

#region BSpline2

/// <summary>
/// Quadratic B-spline resampler (degree 2).
/// </summary>
/// <remarks>
/// <para>
/// Uses quadratic B-spline basis function with radius 1.5.
/// Provides smooth interpolation with moderate blurring.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline2"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 2", Year = 1978,
  Description = "Quadratic B-spline interpolation (degree 2)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline2 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2; // ceil(1.5) = 2

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline2;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => BSplineMath.BSpline2Weight(distance);

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline2, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline2 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _BSplineSafePath.Dispatch(BSplineType.BSpline2, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region BSpline4

/// <summary>
/// Quartic B-spline resampler (degree 4), also known as Parzen window.
/// </summary>
/// <remarks>
/// <para>
/// Uses quartic B-spline basis function with radius 2.5.
/// Provides smooth interpolation between quadratic and quintic.
/// </para>
/// <para>
/// This is equivalent to the Parzen window function used in ImageMagick.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline4"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 4", Year = 1978,
  Description = "Quartic B-spline interpolation (degree 4) / Parzen window", Category = ScalerCategory.Resampler)]
public readonly struct BSpline4 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3; // ceil(2.5) = 3

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline4;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => BSplineMath.BSpline4Weight(distance);

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline4, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline4 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _BSplineSafePath.Dispatch(BSplineType.BSpline4, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region BSpline5

/// <summary>
/// Quintic B-spline resampler (degree 5).
/// </summary>
/// <remarks>
/// <para>
/// Uses quintic B-spline basis function with radius 3.
/// Provides very smooth interpolation.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline5"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 5", Year = 1978,
  Description = "Quintic B-spline interpolation (degree 5)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline5 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline5;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => BSplineMath.BSpline5Weight(distance);

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline5, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline5 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _BSplineSafePath.Dispatch(BSplineType.BSpline5, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region BSpline7

/// <summary>
/// Septic B-spline resampler (degree 7).
/// </summary>
/// <remarks>
/// <para>
/// Uses septic B-spline basis function with radius 4.
/// Provides extremely smooth interpolation.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline7"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 7", Year = 1978,
  Description = "Septic B-spline interpolation (degree 7)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline7 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 4;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline7;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => BSplineMath.BSpline7Weight(distance);

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline7, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline7 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _BSplineSafePath.Dispatch(BSplineType.BSpline7, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region BSpline9

/// <summary>
/// Nonic B-spline resampler (degree 9).
/// </summary>
/// <remarks>
/// <para>
/// Uses nonic B-spline basis function with radius 5.
/// Provides very high smoothness for mathematical approximation.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline9"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 9", Year = 1978,
  Description = "Nonic B-spline interpolation (degree 9)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline9 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 5;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline9;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => BSplineMath.BSpline9Weight(distance);

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline9, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline9 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _BSplineSafePath.Dispatch(BSplineType.BSpline9, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region BSpline11

/// <summary>
/// 11th-degree B-spline resampler.
/// </summary>
/// <remarks>
/// <para>
/// Uses 11th-degree B-spline basis function with radius 6.
/// Provides the highest smoothness in this family.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline11"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 11", Year = 1978,
  Description = "11th-degree B-spline interpolation", Category = ScalerCategory.Resampler)]
public readonly struct BSpline11 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 6;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline11;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => BSplineMath.BSpline11Weight(distance);

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline11, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline11 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _BSplineSafePath.Dispatch(BSplineType.BSpline11, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region Shared Kernel Infrastructure

file enum BSplineType {
  BSpline2,
  BSpline4,
  BSpline5,
  BSpline7,
  BSpline9,
  BSpline11
}

file readonly struct BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, BSplineType type, bool useCenteredGrid)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => type switch {
    BSplineType.BSpline2 => 2,
    BSplineType.BSpline4 => 3,
    BSplineType.BSpline5 => 3,
    BSplineType.BSpline7 => 4,
    BSplineType.BSpline9 => 5,
    BSplineType.BSpline11 => 6,
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

    // Edge path: bounds-checked indexer. Pipeline routes only edge-band pixels here; the safe
    // interior runs through ResampleUnchecked (below).
    Accum4F<TWork> acc = default;
    var r = this.Radius;
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

    Accum4F<TWork> acc = default;
    var r = this.Radius;
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
  /// Computes the B-spline weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Weight(float x) => type switch {
    BSplineType.BSpline2 => BSpline2Weight(x),
    BSplineType.BSpline4 => BSpline4Weight(x),
    BSplineType.BSpline5 => BSpline5Weight(x),
    BSplineType.BSpline7 => BSpline7Weight(x),
    BSplineType.BSpline9 => BSpline9Weight(x),
    BSplineType.BSpline11 => BSpline11Weight(x),
    _ => 0f
  };

  /// <summary>
  /// Quadratic B-spline (degree 2) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline2Weight(float x) => BSplineMath.BSpline2Weight(x);

  /// <summary>
  /// Quartic B-spline (degree 4) weight, equivalent to Parzen window.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline4Weight(float x) => BSplineMath.BSpline4Weight(x);

  /// <summary>
  /// Quintic B-spline (degree 5) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline5Weight(float x) => BSplineMath.BSpline5Weight(x);

  /// <summary>
  /// Septic B-spline (degree 7) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline7Weight(float x) => BSplineMath.BSpline7Weight(x);

  /// <summary>
  /// Nonic B-spline (degree 9) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline9Weight(float x) => BSplineMath.BSpline9Weight(x);

  /// <summary>
  /// 11th-degree B-spline weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline11Weight(float x) => BSplineMath.BSpline11Weight(x);
}

file static class _BSplineSafePath {
  public static Bitmap Dispatch(BSplineType type, Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new BSplineKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, type, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      BSplineKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

internal static class BSplineMath {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float BSpline2Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 0.5f)
      return 0.75f - x * x;
    if (x < 1.5f) {
      var t = 1.5f - x;
      return t * t * 0.5f;
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float BSpline4Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 0.5f) {
      var x2 = x * x;
      return 0.59895833333333f - x2 * (0.625f - x2 * 0.25f);
    }
    if (x < 1.5f) {
      var t = x - 1f;
      var t2 = t * t;
      var absT = MathF.Abs(t);
      return 0.57291666666667f - absT * (0.20833333333333f + t2 * (1.25f - absT * (0.83333333333333f - t2 * 0.16666666666667f)));
    }
    if (x < 2.5f) {
      var t = 2.5f - x;
      var t2 = t * t;
      return t2 * t2 * 0.04166666666667f;
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float BSpline5Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 1f) {
      var x2 = x * x;
      return 0.55f + x2 * (-0.5f + x2 * (0.25f - x * (1f / 12f)));
    }
    if (x < 2f) {
      var t = 2f - x;
      var t2 = t * t;
      return t2 * t2 * (t * (1f / 24f) + 1f / 24f);
    }
    if (x < 3f) {
      var t = 3f - x;
      var t2 = t * t;
      return t2 * t2 * t * (1f / 120f);
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float BSpline7Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 1f) {
      var x2 = x * x;
      var x4 = x2 * x2;
      return 151f / 315f + x2 * (-1f / 3f + x2 * (1f / 9f + x2 * (-1f / 36f + x * (1f / 144f))));
    }
    if (x < 2f) {
      var t = 2f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      return t4 * t2 * (t * (1f / 720f) + 1f / 720f + 1f / 240f);
    }
    if (x < 3f) {
      var t = 3f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      return t4 * t2 * t * (1f / 5040f) + t4 * t2 * (1f / 720f);
    }
    if (x < 4f) {
      var t = 4f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      return t4 * t2 * t * (1f / 5040f);
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float BSpline9Weight(float x) {
    x = MathF.Abs(x);
    var x2 = x * x;
    if (x <= 1f) {
      var x4 = x2 * x2;
      return 35f / 72f + x2 * (-5f / 18f + x2 * (5f / 72f + x4 * (-5f / 504f + x * (1f / 1008f))));
    }
    if (x < 2f) {
      var t = 2f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * (t * (1f / 40320f) + 1f / 4480f);
    }
    if (x < 3f) {
      var t = 3f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * t * (1f / 362880f) + t8 * (1f / 40320f);
    }
    if (x < 4f) {
      var t = 4f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * t * (1f / 362880f);
    }
    if (x < 5f) {
      var t = 5f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * t * (1f / 3628800f);
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float BSpline11Weight(float x) {
    x = MathF.Abs(x);
    var x2 = x * x;
    if (x <= 1f) {
      var x4 = x2 * x2;
      var x6 = x4 * x2;
      return 655177f / 1663200f + x2 * (-7f / 33f + x2 * (7f / 132f + x6 * (-7f / 3960f + x * (1f / 7920f))));
    }
    if (x < 2f) {
      var t = 2f - x;
      return Pow11(t) * (1f / 39916800f) + Pow10(t) * (1f / 3628800f);
    }
    if (x < 3f) {
      var t = 3f - x;
      return Pow11(t) * (1f / 39916800f);
    }
    if (x < 4f) {
      var t = 4f - x;
      return Pow11(t) * (1f / 39916800f);
    }
    if (x < 5f) {
      var t = 5f - x;
      return Pow11(t) * (1f / 479001600f);
    }
    if (x < 6f) {
      var t = 6f - x;
      return Pow11(t) * (1f / 479001600f);
    }
    return 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Pow10(float x) {
    var x2 = x * x;
    var x4 = x2 * x2;
    return x4 * x4 * x2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Pow11(float x) {
    var x2 = x * x;
    var x4 = x2 * x2;
    return x4 * x4 * x2 * x;
  }
}

#endregion
