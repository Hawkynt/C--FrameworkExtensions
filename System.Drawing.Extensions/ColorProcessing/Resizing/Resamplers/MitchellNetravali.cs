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
/// Mitchell-Netravali resampler with B=1/3, C=1/3 (recommended neutral).
/// </summary>
/// <remarks>
/// <para>A family of cubic filters parameterized by B and C.</para>
/// <para>B=C=1/3 provides a good balance between blurring and ringing.</para>
/// </remarks>
[ScalerInfo("Mitchell-Netravali", Author = "Don P. Mitchell, Arun N. Netravali", Year = 1988,
  Description = "Cubic filter with B=1/3, C=1/3", Category = ScalerCategory.Resampler)]
public readonly struct MitchellNetravali : IKernelResampler, IResamplerWithSafePath {

  private readonly float _b, _c;

  /// <summary>
  /// Creates a Mitchell-Netravali resampler with default parameters (B=C=1/3).
  /// </summary>
  public MitchellNetravali() : this(1f / 3f, 1f / 3f) { }

  /// <summary>
  /// Creates a Mitchell-Netravali resampler with custom B and C parameters.
  /// </summary>
  /// <param name="b">The B parameter.</param>
  /// <param name="c">The C parameter.</param>
  public MitchellNetravali(float b, float c) {
    this._b = b;
    this._c = c;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => MitchellMath.Weight(distance, this._b, this._c);

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._b, this._c, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static MitchellNetravali Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _MitchellSafePath.Dispatch(this._b, this._c, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// Catmull-Rom spline resampler (B=0, C=0.5).
/// </summary>
/// <remarks>
/// <para>A cubic spline that passes through control points.</para>
/// <para>Produces sharp results with some overshoot on edges.</para>
/// </remarks>
[ScalerInfo("Catmull-Rom", Author = "Edwin Catmull, Raphael Rom", Year = 1974,
  Description = "Cubic spline with B=0, C=0.5", Category = ScalerCategory.Resampler)]
public readonly struct CatmullRom : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => MitchellMath.Weight(distance, 0f, 0.5f);

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 0f, 0.5f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static CatmullRom Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _MitchellSafePath.Dispatch(0f, 0.5f, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// B-Spline resampler (B=1, C=0) - cubic B-spline (degree 3).
/// </summary>
/// <remarks>
/// <para>Smooth cubic B-spline interpolation.</para>
/// <para>Produces smooth results with no overshoot but some blurring.</para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline3"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 3", Author = "Standard Algorithm",
  Description = "Cubic B-spline with B=1, C=0 (degree 3)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline3;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => MitchellMath.Weight(distance, 1f, 0f);

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 1f, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _MitchellSafePath.Dispatch(1f, 0f, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// Robidoux resampler - optimized Mitchell-Netravali variant.
/// </summary>
/// <remarks>
/// <para>Optimized B, C parameters (B=0.3782, C=0.3109) by Nicolas Robidoux.</para>
/// <para>Designed to minimize error when resampling photographic images.</para>
/// </remarks>
[ScalerInfo("Robidoux", Author = "Nicolas Robidoux", Year = 2011,
  Description = "Optimized Mitchell variant with B=0.3782, C=0.3109", Category = ScalerCategory.Resampler)]
public readonly struct Robidoux : IKernelResampler, IResamplerWithSafePath {

  /// <summary>B parameter value.</summary>
  public const float B = 0.3782157550102413f;

  /// <summary>C parameter value.</summary>
  public const float C = 0.3108921224948793f;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => MitchellMath.Weight(distance, B, C);

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, B, C, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Robidoux Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _MitchellSafePath.Dispatch(B, C, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// RobidouxSharp resampler - sharper Robidoux variant.
/// </summary>
/// <remarks>
/// <para>Sharper B, C parameters (B=0.2620, C=0.3690) by Nicolas Robidoux.</para>
/// <para>Produces sharper results than standard Robidoux while maintaining quality.</para>
/// </remarks>
[ScalerInfo("RobidouxSharp", Author = "Nicolas Robidoux", Year = 2011,
  Description = "Sharper Robidoux variant with B=0.2620, C=0.3690", Category = ScalerCategory.Resampler)]
public readonly struct RobidouxSharp : IKernelResampler, IResamplerWithSafePath {

  /// <summary>B parameter value.</summary>
  public const float B = 0.2620145123990142f;

  /// <summary>C parameter value.</summary>
  public const float C = 0.3689927438004929f;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => MitchellMath.Weight(distance, B, C);

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, B, C, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static RobidouxSharp Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _MitchellSafePath.Dispatch(B, C, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// RobidouxSoft resampler - smoother Robidoux variant.
/// </summary>
/// <remarks>
/// <para>Smoother B, C parameters (B=0.6796, C=0.1602) by Nicolas Robidoux.</para>
/// <para>Produces smoother results than standard Robidoux with reduced sharpening.</para>
/// </remarks>
[ScalerInfo("RobidouxSoft", Author = "Nicolas Robidoux", Year = 2011,
  Description = "Smoother Robidoux variant with B=0.6796, C=0.1602", Category = ScalerCategory.Resampler)]
public readonly struct RobidouxSoft : IKernelResampler, IResamplerWithSafePath {

  /// <summary>B parameter value.</summary>
  public const float B = 0.67962275088539597f;

  /// <summary>C parameter value.</summary>
  public const float C = 0.16018862455730199f;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => MitchellMath.Weight(distance, B, C);

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, B, C, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static RobidouxSoft Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _MitchellSafePath.Dispatch(B, C, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

file readonly struct MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float b, float c, bool useCenteredGrid)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 2;
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

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Precompute weights for x and y
    var wx = stackalloc float[4];
    var wy = stackalloc float[4];
    for (var i = 0; i < 4; ++i) {
      wx[i] = MitchellWeight(fx - (i - 1), b, c);
      wy[i] = MitchellWeight(fy - (i - 1), b, c);
    }

    // Edge path: bounds-checked indexer. Pipeline routes only edge-band pixels here.
    Accum4F<TWork> acc = default;
    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var weight = wx[kx] * wy[ky];
      if (weight == 0f) continue;
      acc.AddMul(frame[x0 + kx - 1, y0 + ky - 1].Work, weight);
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
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    var wx = stackalloc float[4];
    var wy = stackalloc float[4];
    for (var i = 0; i < 4; ++i) {
      wx[i] = MitchellWeight(fx - (i - 1), b, c);
      wy[i] = MitchellWeight(fy - (i - 1), b, c);
    }

    Accum4F<TWork> acc = default;
    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var weight = wx[kx] * wy[ky];
      if (weight == 0f) continue;
      acc.AddMul(frame.GetUnchecked(x0 + kx - 1, y0 + ky - 1).Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -1, kxMaxExcl: 3, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: -1, kyMaxExcl: 3, this._scaleY, this._offsetY, sourceHeight, targetHeight);

  /// <summary>
  /// Computes the Mitchell-Netravali weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float MitchellWeight(float x, float b, float c) => MitchellMath.Weight(x, b, c);
}

internal static class _MitchellSafePath {
  public static Bitmap Dispatch(float b, float c, Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new MitchellNetravaliKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, b, c, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      MitchellNetravaliKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

internal static class MitchellMath {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Weight(float x, float b, float c) {
    x = MathF.Abs(x);
    if (x < 1f)
      return ((12f - 9f * b - 6f * c) * x * x * x
              + (-18f + 12f * b + 6f * c) * x * x
              + (6f - 2f * b)) / 6f;
    if (x < 2f)
      return ((-b - 6f * c) * x * x * x
              + (6f * b + 30f * c) * x * x
              + (-12f * b - 48f * c) * x
              + (8f * b + 24f * c)) / 6f;
    return 0f;
  }
}
