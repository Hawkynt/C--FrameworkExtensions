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
public readonly struct Smoothstep : IResampler {

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
    => this._mode == SmoothstepMode.Standard
      ? callback.Invoke(new SmoothstepKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
        sourceWidth, sourceHeight, targetWidth, targetHeight))
      : callback.Invoke(new SmoothestKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
        sourceWidth, sourceHeight, targetWidth, targetHeight));

  /// <summary>
  /// Gets the default configuration (standard smoothstep).
  /// </summary>
  public static Smoothstep Default => new();

  /// <summary>
  /// Gets the smoothest configuration (7th-degree polynomial).
  /// </summary>
  public static Smoothstep Smoothest => new(SmoothstepMode.Smoothest);
}

file readonly struct SmoothstepKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
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

  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel center to source coordinates
    var srcXf = (destX + 0.5f) * this._scaleX - 0.5f;
    var srcYf = (destY + 0.5f) * this._scaleY - 0.5f;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Apply smoothstep: t² * (3 - 2t) = t² * (-2t + 3)
    var sx = fx * fx * (-2f * fx + 3f);
    var sy = fy * fy * (-2f * fy + 3f);

    // Sample 2x2 neighborhood
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;

    // Bilinear interpolation with smoothstep weights
    var w00 = (1f - sx) * (1f - sy);
    var w10 = sx * (1f - sy);
    var w01 = (1f - sx) * sy;
    var w11 = sx * sy;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }
}

file readonly struct SmoothestKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
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

  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel center to source coordinates
    var srcXf = (destX + 0.5f) * this._scaleX - 0.5f;
    var srcYf = (destY + 0.5f) * this._scaleY - 0.5f;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Apply smootheststep: t⁴ * (t * (t * (-20t + 70) - 84) + 35)
    var sx = SmoothestStep(fx);
    var sy = SmoothestStep(fy);

    // Sample 2x2 neighborhood
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;

    // Bilinear interpolation with smootheststep weights
    var w00 = (1f - sx) * (1f - sy);
    var w10 = sx * (1f - sy);
    var w01 = (1f - sx) * sy;
    var w11 = sx * sy;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

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
