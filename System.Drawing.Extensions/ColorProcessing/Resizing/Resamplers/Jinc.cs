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

#region Jinc

/// <summary>
/// Jinc resampler - 2D equivalent of sinc function.
/// </summary>
/// <remarks>
/// <para>Uses the Bessel function J₁ for radially symmetric filtering.</para>
/// <para>Better suited for 2D resampling than separable sinc filters.</para>
/// <para>Formula: jinc(x) = J₁(πx)/(πx) where J₁ is the first-order Bessel function.</para>
/// </remarks>
[ScalerInfo("Jinc", Year = 1990,
  Description = "2D Bessel-based resampler using J₁(πx)/(πx)", Category = ScalerCategory.Resampler)]
public readonly struct Jinc : IKernelResampler, IResamplerWithSafePath {

  private readonly int _radius;

  /// <summary>
  /// Creates a Jinc resampler with radius 3 (default).
  /// </summary>
  public Jinc() : this(3) { }

  /// <summary>
  /// Creates a Jinc resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Jinc(int radius) {
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
  /// Jinc is radially symmetric (not separable); this returns the radial profile
  /// jinc(|x|) suitable for plotting a 1-D cross-section of the kernel.
  /// </remarks>
  public float EvaluateWeight(float distance) {
    var r = MathF.Abs(distance);
    var radius = this.Radius;
    return r < radius ? JincMath.Jinc(r) : 0f;
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
    => callback.Invoke(new JincKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, JincType.Jinc, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Jinc Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _JincSafePath.Dispatch(this.Radius, JincType.Jinc, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region EWALanczos

/// <summary>
/// EWA Lanczos resampler - Elliptical Weighted Average with Lanczos-windowed Jinc.
/// </summary>
/// <remarks>
/// <para>Combines the Jinc function with Lanczos windowing for high-quality resampling.</para>
/// <para>Better handles rotations and non-uniform scaling than separable filters.</para>
/// <para>EWA (Elliptical Weighted Average) provides proper filtering for geometric transformations.</para>
/// </remarks>
[ScalerInfo("EWA Lanczos", Year = 2000,
  Description = "Elliptical Weighted Average with Lanczos-windowed Jinc", Category = ScalerCategory.Resampler)]
public readonly struct EwaLanczos : IKernelResampler, IResamplerWithSafePath {

  private readonly int _radius;

  /// <summary>
  /// Creates an EWA Lanczos resampler with radius 3 (default).
  /// </summary>
  public EwaLanczos() : this(3) { }

  /// <summary>
  /// Creates an EWA Lanczos resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public EwaLanczos(int radius) {
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
  /// EWA Lanczos is radially symmetric; this returns the 1-D radial profile
  /// jinc(|x|) * jinc(|x|/radius) for charting.
  /// </remarks>
  public float EvaluateWeight(float distance) {
    var r = MathF.Abs(distance);
    var radius = this.Radius;
    return r < radius ? JincMath.Jinc(r) * JincMath.Jinc(r / radius) : 0f;
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
    => callback.Invoke(new JincKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, JincType.EwaLanczos, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static EwaLanczos Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode,
    Color canvasColor, bool useCenteredGrid)
    => _JincSafePath.Dispatch(this.Radius, JincType.EwaLanczos, source, targetWidth, targetHeight,
      horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

#endregion

#region Shared Kernel Infrastructure

file enum JincType {
  Jinc,
  EwaLanczos
}

file readonly struct JincKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, int radius, JincType jincType, bool useCenteredGrid)
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
      var dx = fx - kx;
      var dy = fy - ky;
      var r = MathF.Sqrt(dx * dx + dy * dy);
      var weight = jincType == JincType.EwaLanczos ? this.EwaLanczosWeight(r) : this.JincWeight(r);
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
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var dx = fx - kx;
      var dy = fy - ky;
      var r = MathF.Sqrt(dx * dx + dy * dy);
      var weight = jincType == JincType.EwaLanczos ? this.EwaLanczosWeight(r) : this.JincWeight(r);
      if (weight == 0f) continue;
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
  /// Jinc weight: J₁(πr)/(πr).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float JincWeight(float r) {
    if (r >= radius)
      return 0f;
    return JincFunction(r);
  }

  /// <summary>
  /// EWA Lanczos weight: jinc(r) * jinc(r/a) where a is the radius.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float EwaLanczosWeight(float r) {
    if (r >= radius)
      return 0f;
    return JincFunction(r) * JincFunction(r / radius);
  }

  /// <summary>
  /// Jinc function: J₁(πr)/(πr).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float JincFunction(float r) => JincMath.Jinc(r);
}

internal static class JincMath {

  /// <summary>Jinc function: J₁(πr)/(πr), with the limit 0.5 at r=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Jinc(float r) {
    if (r == 0f) return 0.5f;
    var pir = MathF.PI * r;
    return BesselJ1(pir) / pir;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BesselJ1(float x) {
    var ax = MathF.Abs(x);
    if (ax < 8f) {
      var y = x * x;
      var ans1 = x * (72362614232.0f + y * (-7895059235.0f + y * (242396853.1f
        + y * (-2972611.439f + y * (15704.4826f + y * -30.16036606f)))));
      var ans2 = 144725228442.0f + y * (2300535178.0f + y * (18583304.74f
        + y * (99447.43394f + y * (376.9991397f + y))));
      return ans1 / ans2;
    }
    var z = 8f / ax;
    var y2 = z * z;
    var xx = ax - 2.356194491f;
    var ans1b = 1f + y2 * (0.183105e-2f + y2 * (-0.3516396496e-4f
      + y2 * (0.2457520174e-5f + y2 * -0.240337019e-6f)));
    var ans2b = 0.04687499995f + y2 * (-0.2002690873e-3f
      + y2 * (0.8449199096e-5f + y2 * (-0.88228987e-6f + y2 * 0.105787412e-6f)));
    var result = MathF.Sqrt(0.636619772f / ax) *
      (MathF.Cos(xx) * ans1b - z * MathF.Sin(xx) * ans2b);
    return x < 0f ? -result : result;
  }
}

file static class _JincSafePath {
  public static Bitmap Dispatch(int radius, JincType jincType,
    Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new JincKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, radius, jincType, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      JincKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

#endregion
