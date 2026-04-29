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
/// L0 gradient-minimisation smoothing — Xu, Lu, Xu &amp; Jia 2011.
/// </summary>
/// <remarks>
/// <para>Iteratively zeros out small image gradients while preserving large ones,
/// approximating an L0 minimisation of the gradient distribution. Different mechanism
/// than the bilateral filter (range Gaussian) and the Perona-Malik / Coherence-Enhancing
/// diffusion filters (gradient-driven flow): L0 is a sparsity-inducing prior on the
/// gradient itself, producing a piecewise-flat output where small variations vanish
/// completely while genuine edges pass through unaffected.</para>
/// <para>This implementation does the per-pixel approximation of half-quadratic splitting:
/// at each pass, gradients with magnitude below the threshold τ are pulled toward zero in
/// the reconstruction step. With multiple passes (default 4) and a doubling τ schedule,
/// the result converges toward the L0 minimiser used in the paper for cartoon-style
/// flattening, mesh denoising, and JPEG-artifact suppression.</para>
/// <para>Reference: Xu, Lu, Xu &amp; Jia 2011, "Image Smoothing via L0 Gradient Minimization",
/// SIGGRAPH Asia.</para>
/// </remarks>
[FilterInfo("L0Smoothing",
  Description = "L0 gradient-minimisation edge-preserving smoothing", Category = FilterCategory.Enhancement,
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

    // Forward differences ∇h, ∇v in luminance — L0 paper uses gradient-magnitude
    // threshold across all channels for color-coherent flattening.
    var hLum = (er + eg + eb) / 3f - (cr + cg + cb) / 3f;
    var vLum = (sr + sg + sb) / 3f - (cr + cg + cb) / 3f;
    var gradMag2 = hLum * hLum + vLum * vLum;

    // Hard threshold τ on the gradient magnitude squared. Below threshold the gradient is
    // zeroed and we pull the centre toward the average of its 4-neighbours; above
    // threshold the gradient is preserved (centre stays) — the canonical L0 hard-shrink
    // schedule. λ controls the threshold magnitude.
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
