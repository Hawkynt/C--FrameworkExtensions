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
/// L0 gradient-minimisation smoothing — Xu, Lu, Xu &amp; Jia 2011, canonical FFT-based
/// half-quadratic-splitting solver.
/// </summary>
/// <remarks>
/// <para>Reference: Xu, L., Lu, C., Xu, Y., &amp; Jia, J. (2011). "Image Smoothing via L0
/// Gradient Minimization", ACM Transactions on Graphics 30(6), Article 174 (SIGGRAPH
/// Asia).</para>
/// <para><b>Algorithm.</b> Minimises the energy
/// <c>E(S) = ‖S − I‖² + λ·‖∇S‖₀</c> via half-quadratic splitting with auxiliary
/// fields (h, v) and a β-doubling schedule. Per outer iteration:</para>
/// <list type="bullet">
///   <item>Auxiliary update — analytic hard-shrink on (h, v) using the colour-L0 norm
///   (paper eq. 12): retain ∂xS / ∂yS only where the SUM of squared gradients across
///   all three RGB channels exceeds λ/β.</item>
///   <item>S update — global least-squares solve in the frequency domain (paper eq.
///   8): <c>F(S) = (F(I) + β·(conj(F(∂x))·F(h) + conj(F(∂y))·F(v))) /
///   (1 + β·(|F(∂x)|² + |F(∂y)|²))</c>. Diagonal in Fourier; trivially solvable.</item>
///   <item>β doubles each iteration (κ = 2) until β ≥ β_max (= 1e5).</item>
/// </list>
/// <para><b>Dispatch.</b> The bitmap-extension path
/// (<see cref="Hawkynt.Drawing.BitmapFilterExtensions"/>) routes L0Smoothing through a
/// dedicated whole-image FFT solver (<see cref="L0SmoothingFftHqs"/>) for canonical
/// behaviour. The kernel-driven path retained on this filter falls back to a per-pixel
/// hard-shrink approximation, used only when the filter is invoked through APIs that
/// don't reach the bitmap extension.</para>
/// </remarks>
[FilterInfo("L0Smoothing",
  Description = "L0 gradient-minimisation edge-preserving smoothing (iterative hard-shrink approximation of Xu 2011)",
  Category = FilterCategory.Enhancement,
  Author = "Xu, Lu, Xu & Jia", Year = 2011)]
public readonly struct L0Smoothing : IPixelFilter, IMultiPassFilter {

  private readonly int _iterations;
  private readonly float _lambda;

  /// <summary>Default: 4 iterations, λ = 0.02 (paper's standard cartoon setting).</summary>
  public L0Smoothing() : this(4, 0.02f) { }

  /// <summary>Creates an L0Smoothing filter with custom parameters.</summary>
  /// <param name="iterations">Number of half-quadratic-splitting passes (≥ 1, ≤ 16).</param>
  /// <param name="lambda">Smoothing strength λ ∈ (0, 0.1]. Higher = stronger flattening.</param>
  public L0Smoothing(int iterations, float lambda) {
    this._iterations = Math.Max(1, Math.Min(16, iterations));
    this._lambda = Math.Max(1e-4f, Math.Min(0.1f, lambda));
  }

  /// <inheritdoc />
  public int PassCount => this._iterations;

  /// <summary>Gets the L0 smoothing strength λ (used by the FFT-HQS solver).</summary>
  public float Lambda => this._lambda;

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
    => callback.Invoke(new L0Kernel<TWork, TKey, TPixel, TEncode>(this._lambda));

  /// <summary>Default configuration.</summary>
  public static L0Smoothing Default => new();
}

file readonly struct L0Kernel<TWork, TKey, TPixel, TEncode>(float lambda)
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
    var c = window.P0P0.Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in c);

    var nP = window.M1P0.Work;
    var sP = window.P1P0.Work;
    var wP = window.P0M1.Work;
    var eP = window.P0P1.Work;
    var (nr, ng, nb, _) = ColorConverter.GetNormalizedRgba(in nP);
    var (sr, sg, sb, _) = ColorConverter.GetNormalizedRgba(in sP);
    var (wr, wg, wb, _) = ColorConverter.GetNormalizedRgba(in wP);
    var (er, eg, eb, _) = ColorConverter.GetNormalizedRgba(in eP);

    // Per-channel forward differences ∇h, ∇v on R, G, B. Xu et al. 2011 §3 defines the
    // L0 norm as the count of pixels where ANY channel has a non-zero gradient — so the
    // threshold uses the SUM of squared gradients across all colour channels and both
    // axes (eq. 3, "color L0 norm"). Using only luminance would miss iso-luminant
    // chromatic edges.
    var hR = er - cr; var hG = eg - cg; var hB = eb - cb;
    var vR = sr - cr; var vG = sg - cg; var vB = sb - cb;
    var gradMag2 = hR * hR + hG * hG + hB * hB + vR * vR + vG * vG + vB * vB;

    // Hard threshold τ on the gradient magnitude squared. Below threshold the gradient is
    // zeroed and we pull the centre toward the average of its 4-neighbours; above
    // threshold the gradient is preserved. This is the L0 hard-shrink limit of the
    // half-quadratic-splitting auxiliary update from Xu 2011 eq. (5)-(6) — the global
    // FFT solve over auxiliary variables (h, v) is approximated here by iterating this
    // pixel-local rule across multiple passes (PassCount), which converges to a similar
    // piecewise-constant fixed point for fixed λ. Output differs from paper's exact
    // optimum but produces the same characteristic cartoon-flat regions.
    var tau2 = lambda;
    if (gradMag2 < tau2) {
      var avgR = (cr + nr + sr + wr + er) / 5f;
      var avgG = (cg + ng + sg + wg + eg) / 5f;
      var avgB = (cb + nb + sb + wb + eb) / 5f;
      dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(avgR, avgG, avgB, ca));
      return;
    }

    // Above threshold — preserve the centre value (an edge runs through this pixel).
    dest[0] = encoder.Encode(c);
  }
}
