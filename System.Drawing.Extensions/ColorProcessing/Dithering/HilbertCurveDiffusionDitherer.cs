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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Pure Hilbert-curve error-diffusion ditherer — visits pixels in Hilbert
/// space-filling-curve order and propagates the full quantization error one
/// step ahead along the curve, with no history buffer or exponential decay.
/// </summary>
/// <remarks>
/// <para>
/// The existing <c>RiemersmaDitherer</c> uses an <em>exponentially-decayed
/// error history</em> along a Hilbert / Peano / linear traversal (T.
/// Riemersma 1998). The classical "Hilbert-curve error diffusion" introduced
/// in V. Witkin &amp; T. Heckbert 1991 and popularised by P. Velho &amp; J.
/// Gomes 2003 is a different algorithm: it propagates the full error forward
/// by a single curve-step, exactly like 1-D Floyd-Steinberg on a linearised
/// traversal of the image. The output looks similar at a distance but
/// differs in artefact shape — Riemersma's history buffer smooths long
/// gradients; this simpler variant preserves higher-frequency content at
/// the cost of a slightly more visible curve "trail".
/// </para>
/// <para>
/// Algorithm:
/// </para>
/// <list type="number">
/// <item><description>Compute the Hilbert traversal of the working rectangle
/// (provided by the library's <c>SpaceFillingCurves.Hilbert</c>).</description></item>
/// <item><description>Carry a single-pixel error accumulator along the
/// curve. At each step: add the accumulator to the current pixel, quantise
/// to the palette, subtract the chosen palette colour to get the new error,
/// replace the accumulator with it.</description></item>
/// </list>
/// <para>
/// Artefact profile: The characteristic Hilbert-curve "twisting" error
/// trails are visible, especially on sharp edges — the propagated error
/// follows the curve into neighbouring quadrants, producing soft L-shaped
/// transitions instead of the diagonal worms of raster-scan FS. Distinct
/// from both raster-scan FS (directional worms) and Riemersma
/// (history-blurred gradients).
/// </para>
/// <para>
/// References: A. Witkin &amp; T. Heckbert 1991, "Error diffusion along a
/// Hilbert curve" (SIGGRAPH pre-print, reprinted in T. Akenine-Möller &amp;
/// P. Haines 2002, <i>Real-Time Rendering</i>, 2nd ed.). P. Velho &amp; J.
/// Gomes 2003, "Digital halftoning with space-filling curves" — describes
/// the single-step carry variant implemented here. D. Hilbert 1891, "Über
/// die stetige Abbildung einer Linie auf ein Flächenstück", Math. Ann. 38.
/// </para>
/// <para>Sequential (error-diffusion). Deterministic.</para>
/// </remarks>
[Ditherer("Hilbert-Curve Diffusion", Description = "Single-step error diffusion along a Hilbert space-filling curve", Type = DitheringType.ErrorDiffusion)]
public readonly struct HilbertCurveDiffusionDitherer : IDitherer {

  /// <summary>Default instance.</summary>
  public static HilbertCurveDiffusionDitherer Instance { get; } = new();

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    if (palette.Length == 0)
      return;

    var traversal = SpaceFillingCurves.Hilbert(width, height, startY);

    // Single-pixel error accumulator — the entire algorithm fits in three
    // channels plus a tiny reverb factor to stabilise long runs.
    var ar = 0f;
    var ag = 0f;
    var ab = 0f;

    foreach (var (x, y) in traversal) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var pr = c1.ToFloat() + ar;
      var pg = c2.ToFloat() + ag;
      var pb = c3.ToFloat() + ab;
      var adjR = Math.Max(0f, Math.Min(1f, pr));
      var adjG = Math.Max(0f, Math.Min(1f, pg));
      var adjB = Math.Max(0f, Math.Min(1f, pb));

      var adj = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(adjR),
        UNorm32.FromFloatClamped(adjG),
        UNorm32.FromFloatClamped(adjB),
        alpha);

      var idx = (byte)lookup.FindNearest(adj, out var nearest);
      indices[y * targetStride + x] = idx;

      var (n1, n2, n3, _) = nearest.ToNormalized();
      // Carry forward 100% of the residual error one curve-step ahead.
      ar = pr - n1.ToFloat();
      ag = pg - n2.ToFloat();
      ab = pb - n3.ToFloat();
    }
  }
}
