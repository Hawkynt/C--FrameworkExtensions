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
/// Total Variation (ROF) regularised resampler — edge-preserving smoothing fused with
/// resampling.
/// </summary>
/// <remarks>
/// <para>Implements per-pixel ROF-style regularisation: the bilinear sample is corrected by
/// a soft-thresholded gradient term, suppressing low-amplitude noise (small gradients
/// shrink toward zero) while preserving genuine edges (large gradients pass through).</para>
/// <para>Different mechanism than the bilateral filter (range × spatial Gaussians, also in
/// this namespace). Bilateral suppresses contributions from neighbours with very different
/// luminance; TV suppresses small-amplitude differences regardless of where they sit. They
/// have complementary failure modes, so shipping both is useful.</para>
/// <para>Reference: Rudin, Osher &amp; Fatemi 1992, "Nonlinear total variation based noise
/// removal algorithms", Physica D. Public domain.</para>
/// </remarks>
[ScalerInfo("TotalVariation", Author = "Rudin, Osher & Fatemi", Year = 1992,
  Description = "Total-variation-regularised edge-preserving resampler",
  Category = ScalerCategory.Resampler)]
public readonly struct TotalVariationResampler : IResampler {

  /// <summary>Default soft-threshold τ on luminance gradient (0.04 ≈ 10/255).</summary>
  public const float DefaultThreshold = 0.04f;

  /// <summary>Default regularisation strength λ (0.5).</summary>
  public const float DefaultLambda = 0.5f;

  private readonly float _threshold;
  private readonly float _lambda;

  /// <summary>Creates a TV resampler with default parameters.</summary>
  public TotalVariationResampler() : this(DefaultThreshold, DefaultLambda) { }

  /// <summary>Creates a TV resampler with custom parameters.</summary>
  /// <param name="threshold">Soft-threshold τ on the gradient magnitude (luminance, 0..1).
  /// Gradients below τ get shrunk toward zero; gradients above pass through. Larger τ →
  /// more smoothing of low-contrast detail.</param>
  /// <param name="lambda">Regularisation strength ∈ [0, 1]. 0 disables (= bilinear);
  /// 1 applies the full TV correction. Default 0.5 is a balanced setting.</param>
  public TotalVariationResampler(float threshold, float lambda) {
    ArgumentOutOfRangeException.ThrowIfNegative(threshold);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 1f);
    ArgumentOutOfRangeException.ThrowIfNegative(lambda);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(lambda, 1f);
    this._threshold = threshold;
    this._lambda = lambda;
  }

  /// <summary>Gets the soft-threshold τ.</summary>
  public float Threshold => this._threshold == 0f ? DefaultThreshold : this._threshold;

  /// <summary>Gets the regularisation strength λ.</summary>
  public float Lambda => this._lambda == 0f ? DefaultLambda : this._lambda;

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
    => callback.Invoke(new TotalVariationKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Threshold, this.Lambda, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static TotalVariationResampler Default => new();

  /// <summary>Gets a stronger configuration (more smoothing).</summary>
  public static TotalVariationResampler Strong => new(0.08f, 0.7f);

  /// <summary>Gets a softer configuration (less smoothing).</summary>
  public static TotalVariationResampler Soft => new(0.02f, 0.3f);
}

file readonly struct TotalVariationKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float threshold, float lambda, bool useCenteredGrid)
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

    // Bilinear baseline.
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;
    var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // 3×3 source neighbourhood around (x0, y0) for centred-difference gradient.
    var w = ColorConverter.GetLuminance(frame[x0 - 1, y0].Work);
    var e = ColorConverter.GetLuminance(frame[x0 + 1, y0].Work);
    var n = ColorConverter.GetLuminance(frame[x0,     y0 - 1].Work);
    var s = ColorConverter.GetLuminance(frame[x0,     y0 + 1].Work);

    var gx = (e - w) * 0.5f;
    var gy = (s - n) * 0.5f;
    var gMag = MathF.Sqrt(gx * gx + gy * gy);

    if (gMag < 1e-6f) {
      // Truly flat — no work to do.
      dest[destY * destStride + destX] = encoder.Encode(bilinear);
      return;
    }

    // Soft-threshold (shrinkage) on the gradient magnitude:
    //   shrink(g, τ) = max(0, |g| − τ) · g / |g|
    // Below τ → 0; above τ → magnitude shrunk by τ. Classical ROF / sparse-recovery operator.
    var shrunkMag = MathF.Max(0f, gMag - threshold);
    var shrinkRatio = shrunkMag / gMag; // ∈ [0, 1)

    // The TV correction is: pull the bilinear sample toward a smoother estimate by an amount
    // proportional to (1 − shrinkRatio) — i.e., the part of the gradient that fell below τ
    // and got dropped. We approximate "smoother estimate" by the average of the 4 cardinal
    // neighbours (the discrete Laplacian step), and blend with strength λ · (1 − shrinkRatio).
    var smoothing = (1f - shrinkRatio) * lambda;
    if (smoothing <= 0f) {
      dest[destY * destStride + destX] = encoder.Encode(bilinear);
      return;
    }

    var nW = frame[x0 - 1, y0    ].Work;
    var nE = frame[x0 + 1, y0    ].Work;
    var nN = frame[x0,     y0 - 1].Work;
    var nS = frame[x0,     y0 + 1].Work;

    Accum4F<TWork> acc = default;
    acc.AddMul(bilinear, 1f - smoothing);
    acc.AddMul(nW, smoothing * 0.25f);
    acc.AddMul(nE, smoothing * 0.25f);
    acc.AddMul(nN, smoothing * 0.25f);
    acc.AddMul(nS, smoothing * 0.25f);
    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
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
}
