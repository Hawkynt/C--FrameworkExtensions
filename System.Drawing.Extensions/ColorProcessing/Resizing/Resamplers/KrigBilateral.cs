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
/// Ordinary-kriging upscaler (Krige 1951 / Matheron 1962) on a 4×4 neighbourhood with a
/// Gaussian covariance model. Solves a 17×17 linear system per output pixel via Gaussian
/// elimination to produce minimum-variance unbiased weights, then averages neighbour
/// colours with those weights.
/// </summary>
/// <remarks>
/// <para>For each output pixel we sample 16 source neighbours (4×4 grid around the
/// destination's source-mapping). The Gaussian process model assigns a covariance
/// <c>C(d) = exp(−d²/(2σ²))</c> to any pair separated by Euclidean distance <c>d</c>.
/// Ordinary kriging finds weights <c>w_i</c> minimising prediction variance subject to
/// the unbiasedness constraint <c>Σ w_i = 1</c>. The result is the optimal linear
/// estimator under the Gaussian-process model — smoother than bicubic in flat regions,
/// sharper across edges than Gaussian-only smoothing.</para>
/// <para>The <c>rangeSigma</c> parameter applies an additional bilateral range-kernel
/// modulation to the kriging weights, dampening contributions from neighbours whose
/// luminance differs strongly from the local mean — preserving sharp edges where the
/// Gaussian-process covariance assumption breaks down.</para>
/// <para>Reference: D.G. Krige 1951 "A Statistical Approach to Some Basic Mine Valuation
/// Problems"; Matheron 1962-1965 (formalisation as ordinary kriging). Cost is
/// O(17³) ≈ 5000 ops per output pixel.</para>
/// </remarks>
[ScalerInfo("KrigBilateral",
  Author = "Krige & Matheron", Year = 1951,
  Description = "Ordinary kriging upscaler on 4×4 neighbourhood with Gaussian covariance + bilateral range modulation",
  Category = ScalerCategory.Resampler)]
public readonly struct KrigBilateral : IResampler {

  private readonly float _spatialSigma;
  private readonly float _rangeSigma;

  /// <summary>
  /// Creates a KrigBilateral resampler with default parameters.
  /// </summary>
  public KrigBilateral() : this(1.5f, 0.1f) { }

  /// <summary>
  /// Creates a KrigBilateral resampler with custom parameters.
  /// </summary>
  /// <param name="spatialSigma">Spatial kernel sigma. Higher = more blur.</param>
  /// <param name="rangeSigma">Range kernel sigma. Lower = more edge preservation.</param>
  public KrigBilateral(float spatialSigma, float rangeSigma) {
    this._spatialSigma = Math.Max(spatialSigma, 0.1f);
    this._rangeSigma = Math.Max(rangeSigma, 0.01f);
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
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new KrigBilateralKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._spatialSigma, this._rangeSigma, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static KrigBilateral Default => new();

  /// <summary>
  /// Gets a configuration optimized for edge preservation.
  /// </summary>
  public static KrigBilateral EdgePreserving => new(1.0f, 0.05f);

  /// <summary>
  /// Gets a configuration optimized for smooth results.
  /// </summary>
  public static KrigBilateral Smooth => new(2.0f, 0.2f);
}

file readonly struct KrigBilateralKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float spatialSigma, float rangeSigma, bool useCenteredGrid = true)
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

  // Precomputed scale factors and offsets for zero-cost grid centering
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;
  private readonly float _spatialFactor = -0.5f / (spatialSigma * spatialSigma);
  private readonly float _rangeFactor = -0.5f / (rangeSigma * rangeSigma);

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

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Sample 4x4 neighborhood
    var colors = stackalloc TWork[16];
    colors[0] = frame[x0 - 1, y0 - 1].Work;
    colors[1] = frame[x0, y0 - 1].Work;
    colors[2] = frame[x0 + 1, y0 - 1].Work;
    colors[3] = frame[x0 + 2, y0 - 1].Work;
    colors[4] = frame[x0 - 1, y0].Work;
    colors[5] = frame[x0, y0].Work;
    colors[6] = frame[x0 + 1, y0].Work;
    colors[7] = frame[x0 + 2, y0].Work;
    colors[8] = frame[x0 - 1, y0 + 1].Work;
    colors[9] = frame[x0, y0 + 1].Work;
    colors[10] = frame[x0 + 1, y0 + 1].Work;
    colors[11] = frame[x0 + 2, y0 + 1].Work;
    colors[12] = frame[x0 - 1, y0 + 2].Work;
    colors[13] = frame[x0, y0 + 2].Work;
    colors[14] = frame[x0 + 1, y0 + 2].Work;
    colors[15] = frame[x0 + 2, y0 + 2].Work;

    // Bilinear estimate as the reference luminance for the bilateral range kernel.
    var bilinearEst = BilinearInterpolate(colors[5], colors[6], colors[9], colors[10], fx, fy);
    var refLuma = ColorConverter.GetLuminance(in bilinearEst);

    // -----------------------------------------------------------------
    // Step 1 — Build the ordinary-kriging system A · [w; μ] = b for the 16-point
    // neighbourhood. A is 17×17 stored row-major in `mat`; b is the last column.
    //   A[i, j] = C(||p_i − p_j||)   for i, j ∈ [0, 16)         covariance matrix
    //   A[i, 16] = 1                  for i ∈ [0, 16)            unbiasedness Lagrange
    //   A[16, j] = 1                  for j ∈ [0, 16)
    //   A[16, 16] = 0
    //   b[i]    = C(||p_i − p_target||) for i ∈ [0, 16)
    //   b[16]   = 1
    // C(d) = exp(−d²/(2σ²)) with σ = spatialSigma.
    // -----------------------------------------------------------------
    const int N = 17; // 16 + 1 Lagrange row
    var mat = stackalloc float[N * N];
    var rhs = stackalloc float[N];

    for (var i = 0; i < 16; ++i) {
      var ix = i % 4;
      var iy = i / 4;
      var pix = ix - 1 - fx;
      var piy = iy - 1 - fy;
      // RHS: covariance from neighbour i to target.
      rhs[i] = MathF.Exp((pix * pix + piy * piy) * this._spatialFactor);

      for (var j = 0; j < 16; ++j) {
        var jx = j % 4;
        var jy = j / 4;
        var dx = ix - jx;
        var dy = iy - jy;
        mat[i * N + j] = MathF.Exp((dx * dx + dy * dy) * this._spatialFactor);
      }
      mat[i * N + 16] = 1f;
      mat[16 * N + i] = 1f;
    }
    mat[16 * N + 16] = 0f;
    rhs[16] = 1f;

    // -----------------------------------------------------------------
    // Step 2 — Gaussian elimination with partial pivoting on the augmented matrix.
    // -----------------------------------------------------------------
    for (var k = 0; k < N; ++k) {
      // Partial pivot.
      var piv = k;
      var pivVal = MathF.Abs(mat[k * N + k]);
      for (var r = k + 1; r < N; ++r) {
        var v = MathF.Abs(mat[r * N + k]);
        if (v > pivVal) { pivVal = v; piv = r; }
      }
      if (pivVal < 1e-9f) {
        // Singular — fall back to bilinear and return early.
        dest[destY * destStride + destX] = encoder.Encode(bilinearEst);
        return;
      }
      if (piv != k) {
        for (var c = k; c < N; ++c) {
          var tmp = mat[k * N + c];
          mat[k * N + c] = mat[piv * N + c];
          mat[piv * N + c] = tmp;
        }
        var trh = rhs[k]; rhs[k] = rhs[piv]; rhs[piv] = trh;
      }
      // Eliminate below.
      var invPiv = 1f / mat[k * N + k];
      for (var r = k + 1; r < N; ++r) {
        var factor = mat[r * N + k] * invPiv;
        if (factor == 0f) continue;
        for (var c = k; c < N; ++c)
          mat[r * N + c] -= factor * mat[k * N + c];
        rhs[r] -= factor * rhs[k];
      }
    }

    // -----------------------------------------------------------------
    // Step 3 — Back-substitution. We only need the first 16 weights (w[i]); the
    // 17th element is the Lagrange multiplier μ which we discard.
    // -----------------------------------------------------------------
    var weights = stackalloc float[16];
    // Solve for x[16] then back-substitute. We do this for the full 17-vector but
    // copy only [0..16) into weights.
    var xSol = stackalloc float[N];
    for (var k = N - 1; k >= 0; --k) {
      var s = rhs[k];
      for (var c = k + 1; c < N; ++c)
        s -= mat[k * N + c] * xSol[c];
      xSol[k] = s / mat[k * N + k];
    }
    for (var i = 0; i < 16; ++i) weights[i] = xSol[i];

    // -----------------------------------------------------------------
    // Step 4 — Apply optional bilateral range modulation (preserve edges where the
    // Gaussian-process covariance model breaks down). Multiply each kriging weight by
    // a range-Gaussian on |luma(neighbour) − luma(bilinear estimate)|, then re-normalise
    // so the unbiasedness constraint is preserved.
    // -----------------------------------------------------------------
    var sumW = 0f;
    for (var i = 0; i < 16; ++i) {
      var luma = ColorConverter.GetLuminance(in colors[i]);
      var d = luma - refLuma;
      var rangeWeight = MathF.Exp(d * d * this._rangeFactor);
      weights[i] *= rangeWeight;
      sumW += weights[i];
    }
    if (MathF.Abs(sumW) > 1e-6f) {
      var invSum = 1f / sumW;
      for (var i = 0; i < 16; ++i) weights[i] *= invSum;
    } else {
      // Range modulation collapsed all weights — fall back to bilinear.
      dest[destY * destStride + destX] = encoder.Encode(bilinearEst);
      return;
    }

    // Accumulate weighted RGBA. Note kriging weights can be negative; AddMul handles it.
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 16; ++i)
      acc.AddMul(colors[i], weights[i]);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
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

}
