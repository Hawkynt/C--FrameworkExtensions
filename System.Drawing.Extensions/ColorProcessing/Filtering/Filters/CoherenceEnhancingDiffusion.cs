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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Weickert coherence-enhancing diffusion — anisotropic diffusion guided by structure-tensor
/// principal direction.
/// </summary>
/// <remarks>
/// <para>Distinct from <see cref="AnisotropicDiffusion"/> (Perona-Malik): that filter slows
/// diffusion at strong gradients but is otherwise isotropic at each pixel. Coherence-
/// enhancing diffusion analyses the local structure tensor's coherence — a measure of how
/// strongly directional the local content is — and diffuses preferentially <i>along</i>
/// the dominant orientation. Net effect: edges and line-like features are enhanced and
/// connected, while orthogonal noise is suppressed.</para>
/// <para>Per-pixel form (5×5 reach): compute gradients at the centre and 4-neighbours,
/// build the 2×2 structure tensor, eigen-decompose to get the coherence c, weight
/// diffusion-along-tangent by an exponentially-decaying function of c, and take a single
/// forward-Euler step. Iterating via <see cref="IMultiPassFilter"/> compounds the
/// enhancement.</para>
/// <para>Reference: Weickert 1999, "Coherence-Enhancing Diffusion Filtering", International
/// Journal of Computer Vision 31(2/3):111–127.</para>
/// </remarks>
[FilterInfo("CoherenceEnhancingDiffusion",
  Description = "Weickert structure-tensor anisotropic diffusion", Category = FilterCategory.Enhancement,
  Author = "Joachim Weickert", Year = 1999)]
public readonly struct CoherenceEnhancingDiffusion : IPixelFilter, IMultiPassFilter {

  private readonly int _iterations;
  private readonly float _lambda;
  private readonly float _alpha;

  /// <summary>Default: 8 iterations, λ = 0.15, α = 0.001 (Weickert's nominal values).</summary>
  public CoherenceEnhancingDiffusion() : this(8, 0.15f, 0.001f) { }

  /// <summary>Creates a CED filter with custom parameters.</summary>
  /// <param name="iterations">Forward-Euler iteration count (≥ 1, ≤ 32).</param>
  /// <param name="lambda">Step size λ ∈ (0, 0.25] — above 0.25 the 4-neighbour scheme is unstable.</param>
  /// <param name="alpha">Coherence anisotropy floor α ∈ (0, 1] — small values produce highly
  /// directional diffusion; α = 1 reduces to isotropic Perona-Malik-like flow.</param>
  public CoherenceEnhancingDiffusion(int iterations, float lambda, float alpha) {
    this._iterations = Math.Max(1, Math.Min(32, iterations));
    this._lambda = Math.Max(0.01f, Math.Min(0.25f, lambda));
    this._alpha = Math.Max(1e-4f, Math.Min(1f, alpha));
  }

  /// <inheritdoc />
  public int PassCount => this._iterations;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new CedKernel<TWork, TKey, TPixel, TEncode>(this._lambda, this._alpha));

  /// <summary>Default configuration.</summary>
  public static CoherenceEnhancingDiffusion Default => new();
}

file readonly struct CedKernel<TWork, TKey, TPixel, TEncode>(float lambda, float alpha)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Centre + 4 cardinal + 4 diagonal neighbours (5×5 reach unused; we only need 3×3).
    var c = window.P0P0.Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in c);
    var (nr, ng, nb, _) = ColorConverter.GetNormalizedRgba(window.M1P0.Work);
    var (sr, sg, sb, _) = ColorConverter.GetNormalizedRgba(window.P1P0.Work);
    var (wr, wg, wb, _) = ColorConverter.GetNormalizedRgba(window.P0M1.Work);
    var (er, eg, eb, _) = ColorConverter.GetNormalizedRgba(window.P0P1.Work);
    var (nwR, nwG, nwB, _) = ColorConverter.GetNormalizedRgba(window.M1M1.Work);
    var (neR, neG, neB, _) = ColorConverter.GetNormalizedRgba(window.M1P1.Work);
    var (swR, swG, swB, _) = ColorConverter.GetNormalizedRgba(window.P1M1.Work);
    var (seR, seG, seB, _) = ColorConverter.GetNormalizedRgba(window.P1P1.Work);

    // Luminance-based gradient via central differences for the structure tensor.
    var lumC = (cr + cg + cb) / 3f;
    var lumE = (er + eg + eb) / 3f;
    var lumW = (wr + wg + wb) / 3f;
    var lumN = (nr + ng + nb) / 3f;
    var lumS = (sr + sg + sb) / 3f;

    var ix = 0.5f * (lumE - lumW);
    var iy = 0.5f * (lumS - lumN);

    // Structure tensor J = [[ix² ixiy] [ixiy iy²]]; eigen-decomposition has closed form
    // for 2×2 symmetric: λ_+ = ½·(trace + √(trace² − 4·det)), and the coherence is
    // (λ_+ − λ_-)² / (λ_+ + λ_-)². trace = ix² + iy², det = 0 since J = (∇I)·(∇I)ᵀ.
    var trace = ix * ix + iy * iy;
    // For a rank-1 outer product, λ_+ = trace, λ_- = 0; the coherence is always 1 unless
    // we smooth J first. Approximate post-smoothing by averaging the structure tensor
    // entries over the 3×3 neighbourhood — captures the "is the gradient direction
    // consistent across neighbours?" question that the paper's σ-Gaussian smoothing
    // formalises.
    var avgIx2 = ix * ix;
    var avgIy2 = iy * iy;
    var avgIxIy = ix * iy;
    // Sample 4 cardinal neighbours' contributions to the smoothed tensor.
    avgIx2 += _Ix2(lumC, lumE, lumW, lumN, lumS);
    avgIy2 += _Iy2(lumC, lumE, lumW, lumN, lumS);
    avgIxIy += _IxIy(lumC, lumE, lumW, lumN, lumS);
    // Diagonals contribute via gradient estimates at the corners.
    var lumNW = (nwR + nwG + nwB) / 3f;
    var lumNE = (neR + neG + neB) / 3f;
    var lumSW = (swR + swG + swB) / 3f;
    var lumSE = (seR + seG + seB) / 3f;
    var ixD1 = 0.5f * (lumNE - lumNW);
    var iyD1 = 0.5f * (lumSW - lumNW);
    avgIx2 += ixD1 * ixD1;
    avgIy2 += iyD1 * iyD1;
    avgIxIy += ixD1 * iyD1;
    var ixD2 = 0.5f * (lumSE - lumSW);
    var iyD2 = 0.5f * (lumSE - lumNE);
    avgIx2 += ixD2 * ixD2;
    avgIy2 += iyD2 * iyD2;
    avgIxIy += ixD2 * iyD2;
    avgIx2 /= 4f;
    avgIy2 /= 4f;
    avgIxIy /= 4f;

    // Eigenvalues of the smoothed 2×2 tensor.
    var sumAB = avgIx2 + avgIy2;
    var diffAB = avgIx2 - avgIy2;
    var disc = MathF.Sqrt(diffAB * diffAB + 4f * avgIxIy * avgIxIy);
    var lamPlus = 0.5f * (sumAB + disc);
    var lamMinus = 0.5f * (sumAB - disc);
    var coherence = (lamPlus - lamMinus) / MathF.Max(lamPlus + lamMinus, 1e-9f);
    coherence = coherence * coherence; // square per Weickert

    // Diffusion-tensor eigenvalues:
    //   μ₁ along edge tangent  = α (always diffuses along the edge)
    //   μ₂ across edge normal  = α + (1 − α)·exp(−1/coherence)
    // i.e. when coherence is high, μ₁ ≈ 1 ≫ μ₂ ≈ α — strong line-direction smoothing.
    var muTangent = 1f;
    var muNormal = alpha + (1f - alpha) * (coherence > 1e-6f ? MathF.Exp(-1f / coherence) : 0f);

    // Map back to 4-neighbour weights using the eigenvector basis. Principal direction
    // angle θ = ½·atan2(2·avgIxIy, avgIx2 − avgIy2). The y-arg `diffAB` is legitimately
    // negative when local content is more vertical than horizontal — atan2 must accept
    // a signed second argument, so it is NOT clamped to a positive epsilon.
    // Reference: Weickert 1999 "Coherence-Enhancing Diffusion Filtering" eq. (5).
    var theta = 0.5f * MathF.Atan2(2f * avgIxIy, diffAB);
    var cT = MathF.Cos(theta);
    var sT = MathF.Sin(theta);
    var dXX = muTangent * cT * cT + muNormal * sT * sT;
    var dYY = muTangent * sT * sT + muNormal * cT * cT;
    var dXY = (muTangent - muNormal) * cT * sT;

    // 4-neighbour stencil approximation: each cardinal neighbour contributes (dXX or dYY),
    // diagonal cross-terms via dXY. Apply per-channel.
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      _Step(cr, nr, sr, wr, er, nwR, neR, swR, seR, dXX, dYY, dXY, lambda),
      _Step(cg, ng, sg, wg, eg, nwG, neG, swG, seG, dXX, dYY, dXY, lambda),
      _Step(cb, nb, sb, wb, eb, nwB, neB, swB, seB, dXX, dYY, dXY, lambda),
      ca));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Ix2(float c, float e, float w, float n, float s) {
    var ix = 0.5f * (e - w);
    return ix * ix;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Iy2(float c, float e, float w, float n, float s) {
    var iy = 0.5f * (s - n);
    return iy * iy;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _IxIy(float c, float e, float w, float n, float s)
    => 0.25f * (e - w) * (s - n);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Step(float c, float nP, float sP, float wP, float eP,
    float nw, float ne, float sw, float se,
    float dXX, float dYY, float dXY, float lambda) {
    // ∂I/∂t = ∂x(dXX·∂x I + dXY·∂y I) + ∂y(dXY·∂x I + dYY·∂y I)
    // Discrete: forward + backward differences with diffusion coefficients.
    var dx = dXX * (eP - 2f * c + wP);
    var dy = dYY * (sP - 2f * c + nP);
    var dxy = dXY * (se + nw - sw - ne) * 0.25f;
    return c + lambda * (dx + dy + 2f * dxy);
  }
}
