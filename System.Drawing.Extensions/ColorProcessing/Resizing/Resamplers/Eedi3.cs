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
/// Extended Edge-Directed Interpolation v3 (EEDI3) resampler.
/// </summary>
/// <remarks>
/// <para>EEDI3 improves over EEDI2 by selecting interpolation directions using a wider 4-sample
/// cost function with a curvature penalty, producing more globally consistent edges and fewer
/// direction-flip artifacts on long contours.</para>
/// <para>Reference: Kevin Stone's EEDI3 (Avisynth/VapourSynth plugin family) — the algorithm is
/// described in the plugin documentation and the underlying papers on edge-directed interpolation.
/// This implementation is derived from the published algorithm (not from the GPL source).</para>
/// <para>Algorithm (per output pixel):</para>
/// <list type="number">
/// <item>If local contrast is below threshold, fall back to bilinear (smooth-region case).</item>
/// <item>Otherwise, evaluate 16 candidate directions; for each, sample 4 points along the
///   direction (offsets ±0.5, ±1.5) and compute a cost that combines pairwise sample
///   differences (favours smooth sequences) with a curvature penalty (penalises kinks).</item>
/// <item>Pick the direction with the lowest cost and interpolate via Catmull-Rom along it.</item>
/// </list>
/// </remarks>
[ScalerInfo("EEDI3", Author = "Kevin Stone (algorithm)", Year = 2010,
  Description = "Extended edge-directed interpolation with 4-sample directional cost",
  Category = ScalerCategory.Resampler)]
public readonly struct Eedi3 : IResampler {

  /// <summary>Default contrast threshold (0.12 ≈ 30/255).</summary>
  public const float DefaultThreshold = 0.12f;

  /// <summary>Default curvature penalty weight (0.25).</summary>
  public const float DefaultCurvaturePenalty = 0.25f;

  private readonly float _threshold;
  private readonly float _curvaturePenalty;

  /// <summary>Creates an EEDI3 resampler with default parameters.</summary>
  public Eedi3() : this(DefaultThreshold, DefaultCurvaturePenalty) { }

  /// <summary>Creates an EEDI3 resampler with custom parameters.</summary>
  /// <param name="threshold">Contrast threshold below which bilinear is used (0..1).</param>
  /// <param name="curvaturePenalty">Weight on the curvature penalty term (≥ 0). Higher values
  /// favour straighter directional sequences — fewer artifacts on long edges, slightly less
  /// detail on textured regions.</param>
  public Eedi3(float threshold, float curvaturePenalty) {
    ArgumentOutOfRangeException.ThrowIfNegative(threshold);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 1f);
    ArgumentOutOfRangeException.ThrowIfNegative(curvaturePenalty);
    this._threshold = threshold;
    this._curvaturePenalty = curvaturePenalty;
  }

  /// <summary>Gets the contrast threshold.</summary>
  public float Threshold => this._threshold == 0f ? DefaultThreshold : this._threshold;

  /// <summary>Gets the curvature penalty weight.</summary>
  public float CurvaturePenalty => this._curvaturePenalty == 0f ? DefaultCurvaturePenalty : this._curvaturePenalty;

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
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new Eedi3Kernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Threshold, this.CurvaturePenalty, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static Eedi3 Default => new();
}

file readonly struct Eedi3Kernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float threshold, float curvaturePenalty, bool useCenteredGrid)
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
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;

  // 16 candidate directions on the unit circle (every 22.5°).
  private static readonly float[] _dirX = [
    1.000f, 0.924f, 0.707f, 0.383f, 0.000f, -0.383f, -0.707f, -0.924f,
    -1.000f, -0.924f, -0.707f, -0.383f, 0.000f, 0.383f, 0.707f, 0.924f
  ];
  private static readonly float[] _dirY = [
    0.000f, 0.383f, 0.707f, 0.924f, 1.000f, 0.924f, 0.707f, 0.383f,
    0.000f, -0.383f, -0.707f, -0.924f, -1.000f, -0.924f, -0.707f, -0.383f
  ];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
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

    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;

    var l00 = ColorConverter.GetLuminance(in c00);
    var l10 = ColorConverter.GetLuminance(in c10);
    var l01 = ColorConverter.GetLuminance(in c01);
    var l11 = ColorConverter.GetLuminance(in c11);

    var contrast = MathF.Max(MathF.Abs(l00 - l11), MathF.Abs(l10 - l01));
    if (contrast < threshold) {
      // Smooth region: bilinear is the right answer (no edge to align to).
      dest[destY * destStride + destX] = encoder.Encode(BilinearInterpolate(c00, c10, c01, c11, fx, fy));
      return;
    }

    var bestDir = 0;
    var bestCost = float.MaxValue;
    var dirX = _dirX;
    var dirY = _dirY;

    // EEDI3 cost: 4-point smoothness along the direction + curvature penalty (mismatch
    // between consecutive pairwise differences). Captures both "is this a smooth sequence?"
    // and "is the second derivative small?" — penalising kinky / inconsistent directions.
    for (var d = 0; d < 16; ++d) {
      var dx = dirX[d];
      var dy = dirY[d];
      var p0 = SampleLuma(frame, srcXf - 1.5f * dx, srcYf - 1.5f * dy);
      var p1 = SampleLuma(frame, srcXf - 0.5f * dx, srcYf - 0.5f * dy);
      var p2 = SampleLuma(frame, srcXf + 0.5f * dx, srcYf + 0.5f * dy);
      var p3 = SampleLuma(frame, srcXf + 1.5f * dx, srcYf + 1.5f * dy);

      var d01 = p1 - p0;
      var d12 = p2 - p1;
      var d23 = p3 - p2;

      var smoothness = MathF.Abs(d01) + MathF.Abs(d12) + MathF.Abs(d23);
      // Curvature ≈ second-difference magnitude; smaller = straighter sequence.
      var curvature = MathF.Abs(d12 - d01) + MathF.Abs(d23 - d12);
      var cost = smoothness + curvaturePenalty * curvature;

      if (cost >= bestCost) continue;
      bestCost = cost;
      bestDir = d;
    }

    {
      var dx = dirX[bestDir];
      var dy = dirY[bestDir];
      // Catmull-Rom along the chosen direction at t = 0.5 (between p1 and p2).
      var p0 = SampleBilinear(frame, srcXf - 1.5f * dx, srcYf - 1.5f * dy);
      var p1 = SampleBilinear(frame, srcXf - 0.5f * dx, srcYf - 0.5f * dy);
      var p2 = SampleBilinear(frame, srcXf + 0.5f * dx, srcYf + 0.5f * dy);
      var p3 = SampleBilinear(frame, srcXf + 1.5f * dx, srcYf + 1.5f * dy);

      // Catmull-Rom weights at t=0.5: (-1, 9, 9, -1)/16.
      const float w0 = -1f / 16f;
      const float w1 = 9f / 16f;
      const float w2 = 9f / 16f;
      const float w3 = -1f / 16f;

      Accum4F<TWork> acc = default;
      acc.AddMul(p0, w0);
      acc.AddMul(p1, w1);
      acc.AddMul(p2, w2);
      acc.AddMul(p3, w3);
      dest[destY * destStride + destX] = encoder.Encode(acc.Result);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BilinearInterpolate(in TWork c00, in TWork c10, in TWork c01, in TWork c11, float fx, float fy) {
    var invFx = 1f - fx;
    var invFy = 1f - fy;
    Accum4F<TWork> acc = default;
    acc.AddMul(c00, invFx * invFy);
    acc.AddMul(c10, fx * invFy);
    acc.AddMul(c01, invFx * fy);
    acc.AddMul(c11, fx * fy);
    return acc.Result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork SampleBilinear(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    float x, float y) {
    var ix = (int)MathF.Floor(x);
    var iy = (int)MathF.Floor(y);
    var fx = x - ix;
    var fy = y - iy;
    return BilinearInterpolate(frame[ix, iy].Work, frame[ix + 1, iy].Work,
      frame[ix, iy + 1].Work, frame[ix + 1, iy + 1].Work, fx, fy);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float SampleLuma(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    float x, float y) {
    var ix = (int)MathF.Floor(x);
    var iy = (int)MathF.Floor(y);
    var fx = x - ix;
    var fy = y - iy;
    var l00 = ColorConverter.GetLuminance(frame[ix, iy].Work);
    var l10 = ColorConverter.GetLuminance(frame[ix + 1, iy].Work);
    var l01 = ColorConverter.GetLuminance(frame[ix, iy + 1].Work);
    var l11 = ColorConverter.GetLuminance(frame[ix + 1, iy + 1].Work);
    var invFx = 1f - fx;
    var invFy = 1f - fy;
    return invFx * invFy * l00 + fx * invFy * l10 + invFx * fy * l01 + fx * fy * l11;
  }
}
