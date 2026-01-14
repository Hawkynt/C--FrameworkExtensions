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
/// Robust Adaptive Video Upscaling (RAVU) resampler.
/// </summary>
/// <remarks>
/// <para>Uses local gradient analysis for adaptive directional filtering.</para>
/// <para>Applies different interpolation based on local edge characteristics.</para>
/// <para>Features robust anti-aliasing through gradient-aware weighting.</para>
/// <para>Based on techniques from the mpv RAVU shader.</para>
/// </remarks>
[ScalerInfo("RAVU", Year = 2017,
  Description = "Robust adaptive video upscaling", Category = ScalerCategory.Resampler)]
public readonly struct Ravu : IResampler {

  private readonly float _sharpness;
  private readonly float _antiRinging;

  /// <summary>
  /// Creates a RAVU resampler with default parameters.
  /// </summary>
  public Ravu() : this(0.5f, 0.5f) { }

  /// <summary>
  /// Creates a RAVU resampler with custom parameters.
  /// </summary>
  /// <param name="sharpness">Sharpness parameter (0-1).</param>
  /// <param name="antiRinging">Anti-ringing strength (0-1).</param>
  public Ravu(float sharpness, float antiRinging) {
    this._sharpness = Math.Clamp(sharpness, 0f, 1f);
    this._antiRinging = Math.Clamp(antiRinging, 0f, 1f);
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
    => callback.Invoke(new RavuKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._sharpness, this._antiRinging));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Ravu Default => new();

  /// <summary>
  /// Gets a sharp configuration.
  /// </summary>
  public static Ravu Sharp => new(0.8f, 0.3f);

  /// <summary>
  /// Gets a soft configuration with strong anti-ringing.
  /// </summary>
  public static Ravu Soft => new(0.3f, 0.8f);
}

file readonly struct RavuKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float sharpness, float antiRinging)
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

    // Sample 4x4 neighborhood
    var c00 = frame[x0 - 1, y0 - 1].Work;
    var c10 = frame[x0, y0 - 1].Work;
    var c20 = frame[x0 + 1, y0 - 1].Work;
    var c30 = frame[x0 + 2, y0 - 1].Work;

    var c01 = frame[x0 - 1, y0].Work;
    var c11 = frame[x0, y0].Work;
    var c21 = frame[x0 + 1, y0].Work;
    var c31 = frame[x0 + 2, y0].Work;

    var c02 = frame[x0 - 1, y0 + 1].Work;
    var c12 = frame[x0, y0 + 1].Work;
    var c22 = frame[x0 + 1, y0 + 1].Work;
    var c32 = frame[x0 + 2, y0 + 1].Work;

    var c03 = frame[x0 - 1, y0 + 2].Work;
    var c13 = frame[x0, y0 + 2].Work;
    var c23 = frame[x0 + 1, y0 + 2].Work;
    var c33 = frame[x0 + 2, y0 + 2].Work;

    // Compute luminances for gradient analysis
    var lumas = stackalloc float[16];
    lumas[0] = ColorConverter.GetLuminance(in c00);
    lumas[1] = ColorConverter.GetLuminance(in c10);
    lumas[2] = ColorConverter.GetLuminance(in c20);
    lumas[3] = ColorConverter.GetLuminance(in c30);
    lumas[4] = ColorConverter.GetLuminance(in c01);
    lumas[5] = ColorConverter.GetLuminance(in c11);
    lumas[6] = ColorConverter.GetLuminance(in c21);
    lumas[7] = ColorConverter.GetLuminance(in c31);
    lumas[8] = ColorConverter.GetLuminance(in c02);
    lumas[9] = ColorConverter.GetLuminance(in c12);
    lumas[10] = ColorConverter.GetLuminance(in c22);
    lumas[11] = ColorConverter.GetLuminance(in c32);
    lumas[12] = ColorConverter.GetLuminance(in c03);
    lumas[13] = ColorConverter.GetLuminance(in c13);
    lumas[14] = ColorConverter.GetLuminance(in c23);
    lumas[15] = ColorConverter.GetLuminance(in c33);

    // Compute local gradients using Sobel-like operators
    // Horizontal gradient (gx) and vertical gradient (gy) at center
    var gx = -lumas[4] - 2f * lumas[5] - lumas[8] + lumas[6] + 2f * lumas[7] + lumas[10];
    var gy = -lumas[4] - 2f * lumas[1] - lumas[6] + lumas[8] + 2f * lumas[9] + lumas[10];

    // Edge strength and direction
    var edgeMag = MathF.Sqrt(gx * gx + gy * gy);
    var edgeAngle = MathF.Atan2(gy, gx);

    // Compute directional weights based on edge angle
    // This creates an elliptical kernel aligned with the edge
    var cosA = MathF.Cos(edgeAngle);
    var sinA = MathF.Sin(edgeAngle);

    // Adaptive kernel width based on edge strength
    var edgeStrength = MathF.Min(edgeMag * 2f, 1f);
    var kernelStretch = 1f + edgeStrength * sharpness;

    // Compute RAVU-style weights for all 16 samples
    var weights = stackalloc float[16];
    var colors = stackalloc TWork[16];
    colors[0] = c00;
    colors[1] = c10;
    colors[2] = c20;
    colors[3] = c30;
    colors[4] = c01;
    colors[5] = c11;
    colors[6] = c21;
    colors[7] = c31;
    colors[8] = c02;
    colors[9] = c12;
    colors[10] = c22;
    colors[11] = c32;
    colors[12] = c03;
    colors[13] = c13;
    colors[14] = c23;
    colors[15] = c33;

    var totalWeight = 0f;
    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var idx = ky * 4 + kx;

      // Position relative to interpolation point
      var dx = kx - 1 - fx;
      var dy = ky - 1 - fy;

      // Rotate coordinates to align with edge
      var rotX = dx * cosA + dy * sinA;
      var rotY = -dx * sinA + dy * cosA;

      // Stretch perpendicular to edge
      rotY *= kernelStretch;

      // Gaussian-like weight with directional adaptation
      var dist2 = rotX * rotX + rotY * rotY;
      var weight = MathF.Exp(-dist2 * (1f + sharpness));

      weights[idx] = weight;
      totalWeight += weight;
    }

    // Normalize weights
    var invTotal = 1f / totalWeight;
    for (var i = 0; i < 16; ++i)
      weights[i] *= invTotal;

    // Accumulate weighted result
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 16; ++i)
      if (weights[i] > 0.001f)
        acc.AddMul(colors[i], weights[i]);

    var result = acc.Result;

    // Apply anti-ringing
    if (antiRinging > 0f) {
      // Find min/max of center 4 pixels
      var minL = MathF.Min(MathF.Min(lumas[5], lumas[6]), MathF.Min(lumas[9], lumas[10]));
      var maxL = MathF.Max(MathF.Max(lumas[5], lumas[6]), MathF.Max(lumas[9], lumas[10]));

      var resultL = ColorConverter.GetLuminance(in result);

      // Clamp result luminance to local range
      if (resultL < minL || resultL > maxL) {
        // Blend back towards bilinear to reduce ringing
        var bilinear = BilinearInterpolate(c11, c21, c12, c22, fx, fy);
        var blend = antiRinging * MathF.Min(1f, MathF.Abs(resultL - (minL + maxL) * 0.5f) / ((maxL - minL) * 0.5f + 0.001f));
        result = Lerp(result, bilinear, blend);
      }
    }

    dest[destY * destStride + destX] = encoder.Encode(result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BilinearInterpolate(TWork c00, TWork c10, TWork c01, TWork c11, float fx, float fy) {
    var w00 = (1f - fx) * (1f - fy);
    var w10 = fx * (1f - fy);
    var w01 = (1f - fx) * fy;
    var w11 = fx * fy;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);
    return acc.Result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Lerp(TWork a, TWork b, float t) {
    Accum4F<TWork> acc = default;
    acc.AddMul(a, 1f - t);
    acc.AddMul(b, t);
    return acc.Result;
  }
}
