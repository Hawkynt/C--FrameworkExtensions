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
/// Anisotropic diffusion (Perona–Malik) — iterative edge-preserving smoothing.
/// </summary>
/// <remarks>
/// <para>
/// Approximates the nonlinear PDE
/// <c>∂I/∂t = div(c(|∇I|) · ∇I)</c>
/// with the forward-Euler 4-neighbour stencil introduced by Perona &amp;
/// Malik (1990). The conduction coefficient <c>c</c> uses the second of their
/// proposed functions, <c>c(g) = 1 / (1 + (g / κ)²)</c>, which favours
/// preservation of wider regions and is the usual default in practice.
/// </para>
/// <para>
/// Unlike the <see cref="BilateralFilter"/>, which is a single-pass
/// non-iterative range/spatial average, anisotropic diffusion is an
/// <em>iterative</em> flow that becomes progressively piecewise-constant as
/// iterations accumulate — producing strongly flattened regions with sharp
/// preserved discontinuities. It is the canonical choice for MRI/biomedical
/// denoising and cartoon-like flattening of photographs.
/// </para>
/// <para>
/// Typical use case: edge-preserving denoising where smooth regions should
/// become flatter than bilateral filtering leaves them; preprocessing for
/// segmentation.
/// </para>
/// <para>
/// Reference: Perona, P. &amp; Malik, J. (1990) <em>Scale-Space and Edge
/// Detection Using Anisotropic Diffusion</em>, IEEE PAMI 12(7), 629–639.
/// </para>
/// </remarks>
[FilterInfo("AnisotropicDiffusion",
  Description = "Perona-Malik iterative edge-preserving diffusion", Category = FilterCategory.Enhancement,
  Author = "Pietro Perona, Jitendra Malik", Year = 1990)]
public readonly struct AnisotropicDiffusion : IPixelFilter, IMultiPassFilter {
  private readonly int _iterations;
  private readonly float _lambda;
  private readonly float _kappa;

  public AnisotropicDiffusion() : this(10, 0.1f, 0.15f) { }

  /// <summary>
  /// Initializes a new Perona–Malik anisotropic diffusion filter.
  /// </summary>
  /// <param name="iterations">
  /// Number of explicit Euler steps. Typical values 5 .. 30; more iterations
  /// produce stronger flattening.
  /// </param>
  /// <param name="lambda">
  /// Step size <c>λ ∈ (0, 0.25]</c>. Above <c>0.25</c> the 4-neighbour
  /// explicit scheme becomes unstable. Default <c>0.1</c>.
  /// </param>
  /// <param name="kappa">
  /// Edge-stop threshold in normalised [0,1] luminance units. Gradients much
  /// larger than κ are preserved; smaller gradients are diffused away.
  /// Default <c>0.15</c>.
  /// </param>
  public AnisotropicDiffusion(int iterations, float lambda, float kappa) {
    this._iterations = Math.Max(1, iterations);
    this._lambda = Math.Max(0.01f, Math.Min(0.25f, lambda));
    this._kappa = Math.Max(1e-4f, kappa);
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
    => callback.Invoke(new AnisotropicKernel<TWork, TKey, TPixel, TEncode>(this._lambda, this._kappa));

  /// <summary>Gets the default filter (10 iterations, λ=0.1, κ=0.15).</summary>
  public static AnisotropicDiffusion Default => new();
}

file readonly struct AnisotropicKernel<TWork, TKey, TPixel, TEncode>(float lambda, float kappa)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  /// <summary>
  /// Perona–Malik conductance: <c>c(g) = 1 / (1 + (g/κ)²)</c>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _C(float gradient) {
    var x = gradient / kappa;
    return 1f / (1f + x * x);
  }

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

    // Gradient magnitude driven by luminance (single conductance per direction,
    // applied to all channels so colours don't drift relative to each other).
    var dN = _Diff(nr, ng, nb, cr, cg, cb);
    var dS = _Diff(sr, sg, sb, cr, cg, cb);
    var dW = _Diff(wr, wg, wb, cr, cg, cb);
    var dE = _Diff(er, eg, eb, cr, cg, cb);

    var cN = this._C(dN);
    var cS = this._C(dS);
    var cW = this._C(dW);
    var cE = this._C(dE);

    var outR = cr + lambda * (cN * (nr - cr) + cS * (sr - cr) + cW * (wr - cr) + cE * (er - cr));
    var outG = cg + lambda * (cN * (ng - cg) + cS * (sg - cg) + cW * (wg - cg) + cE * (eg - cg));
    var outB = cb + lambda * (cN * (nb - cb) + cS * (sb - cb) + cW * (wb - cb) + cE * (eb - cb));

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, ca));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Diff(float r1, float g1, float b1, float r2, float g2, float b2) {
    var dr = r1 - r2;
    var dg = g1 - g2;
    var db = b1 - b2;
    return (float)Math.Sqrt(dr * dr + dg * dg + db * db);
  }
}
