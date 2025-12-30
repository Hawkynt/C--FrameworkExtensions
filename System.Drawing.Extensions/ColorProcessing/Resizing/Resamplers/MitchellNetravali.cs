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
/// Mitchell-Netravali resampler with B=1/3, C=1/3 (recommended neutral).
/// </summary>
/// <remarks>
/// <para>A family of cubic filters parameterized by B and C.</para>
/// <para>B=C=1/3 provides a good balance between blurring and ringing.</para>
/// </remarks>
[ScalerInfo("Mitchell-Netravali", Author = "Don P. Mitchell, Arun N. Netravali", Year = 1988,
  Description = "Cubic filter with B=1/3, C=1/3", Category = ScalerCategory.Resampler)]
public readonly struct MitchellNetravali : IResampler {

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._b, this._c));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static MitchellNetravali Default => new();
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
public readonly struct CatmullRom : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 0f, 0.5f));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static CatmullRom Default => new();
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
public readonly struct BSpline : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline3;

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 1f, 0f));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline Default => new();
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
public readonly struct Robidoux : IResampler {

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, B, C));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Robidoux Default => new();
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
public readonly struct RobidouxSharp : IResampler {

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, B, C));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static RobidouxSharp Default => new();
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
public readonly struct RobidouxSoft : IResampler {

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
    => callback.Invoke(new MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, B, C));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static RobidouxSoft Default => new();
}

file readonly struct MitchellNetravaliKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float b, float c)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
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

  // Precomputed scale factors
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;

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

    // Accumulate weighted colors from 4x4 kernel
    Accum4F<TWork> acc = default;
    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var weight = wx[kx] * wy[ky];
      if (weight == 0f)
        continue;

      var pixel = frame[x0 + kx - 1, y0 + ky - 1].Work;
      acc.AddMul(pixel, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Computes the Mitchell-Netravali weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float MitchellWeight(float x, float b, float c) {
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
