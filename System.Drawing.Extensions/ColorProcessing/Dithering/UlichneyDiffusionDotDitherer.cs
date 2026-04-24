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
/// Ulichney diffusion-dot hybrid ditherer — per-pixel adaptive algorithm that
/// switches between a clustered-dot halftone screen (in flat regions) and
/// Floyd-Steinberg error diffusion (in textured regions), as described in
/// R. Ulichney's <i>Digital Halftoning</i> chapter 6 "Combined methods".
/// </summary>
/// <remarks>
/// <para>
/// Flat regions are best rendered with <em>clustered-dot</em> halftones:
/// classical newspaper-print screens, because they concentrate ink in tight
/// clusters that survive ink-on-paper gain distortions. Textured regions are
/// best rendered with <em>dispersed</em> error diffusion: the propagation
/// hides edges in the background noise. Neither choice is universally right
/// for a whole image. Ulichney 1987 <i>Digital Halftoning</i> §6.4 describes
/// a hybrid that picks between them per-pixel based on a local activity
/// measure (high-pass filter of a 3×3 neighbourhood).
/// </para>
/// <para>
/// Algorithm:
/// </para>
/// <list type="number">
/// <item><description>Estimate local activity with a 3×3 Laplacian-like
/// sum.</description></item>
/// <item><description>If activity is below a threshold, use a <em>Cluster
/// Dot 6×6</em> halftone threshold; else use Floyd-Steinberg error
/// propagation to the next row.</description></item>
/// </list>
/// <para>
/// Artefact profile: flat backgrounds look halftoned (clear newspaper-ish
/// cluster-dot grain), textured regions look FS-propagated (smooth, no
/// visible screen). The transition is per-pixel so there are no hard
/// boundaries between the two modes; activity-driven threshold blending
/// hides the hand-off.
/// </para>
/// <para>
/// References: R. Ulichney 1987, <i>Digital Halftoning</i>, MIT Press, §6.4
/// "Diffusion-dot methods". Earlier versions: R. Ulichney 1988, "Dithering
/// with blue noise", <i>Proc. IEEE</i> 76(1). Activity-based adaptive
/// switching was later formalised in M. Eschbach &amp; R. Knox 1991,
/// "Error-diffusion with adaptive coefficients", <i>Proc. SPIE</i> 1450.
/// </para>
/// <para>Sequential (error-diffusion component). Deterministic.</para>
/// </remarks>
[Ditherer("Ulichney Diffusion-Dot", Description = "Per-pixel adaptive hybrid of cluster-dot halftone and Floyd-Steinberg diffusion", Type = DitheringType.ErrorDiffusion, Author = "Robert Ulichney", Year = 1987)]
public readonly struct UlichneyDiffusionDotDitherer : IDitherer {

  private const int _SCREEN = 6;

  // Cluster-dot 6×6 threshold screen, normalised to [-0.5, 0.5]. Central
  // dots carry the lowest thresholds so ink grows outward from the centre.
  private static readonly float[] _Cluster6 = _BuildCluster6();

  /// <summary>Default instance.</summary>
  public static UlichneyDiffusionDotDitherer Instance { get; } = new();

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
    var endY = startY + height;

    var errR = new float[width, height];
    var errG = new float[width, height];
    var errB = new float[width, height];

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x) {
        var color = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, alpha) = color.ToNormalized();

        var activity = _EstimateActivity<TPixel, TWork, TDecode>(source, decoder, x, y, sourceStride, width, endY);
        // Weight ∈ [0, 1]: 0 = use pure halftone (flat), 1 = use pure FS
        // (textured). Smooth ramp centred on 0.04.
        var edFrac = Math.Max(0f, Math.Min(1f, (activity - 0.02f) / 0.04f));
        var halftoneFrac = 1f - edFrac;

        // Halftone contribution: ordered threshold.
        var halftoneBias = _Cluster6[(y % _SCREEN) * _SCREEN + (x % _SCREEN)] * halftoneFrac;

        var pr = c1.ToFloat() + errR[x, localY] + halftoneBias;
        var pg = c2.ToFloat() + errG[x, localY] + halftoneBias;
        var pb = c3.ToFloat() + errB[x, localY] + halftoneBias;
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
        // Only the error-diffusion fraction of the residual is propagated;
        // halftone-region error is discarded (clustered dots don't diffuse).
        var er = (adjR - n1.ToFloat()) * edFrac;
        var eg = (adjG - n2.ToFloat()) * edFrac;
        var eb = (adjB - n3.ToFloat()) * edFrac;

        _Deposit(errR, errG, errB, x + 1, localY, er, eg, eb, 7f / 16f, width, height);
        if (localY + 1 < height) {
          _Deposit(errR, errG, errB, x - 1, localY + 1, er, eg, eb, 3f / 16f, width, height);
          _Deposit(errR, errG, errB, x, localY + 1, er, eg, eb, 5f / 16f, width, height);
          _Deposit(errR, errG, errB, x + 1, localY + 1, er, eg, eb, 1f / 16f, width, height);
        }
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float _EstimateActivity<TPixel, TWork, TDecode>(
    TPixel* source, in TDecode decoder, int x, int y, int sourceStride, int width, int endY)
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDecode : struct, IDecode<TPixel, TWork> {
    // Compute |4·centre - sum(4-neighbour)| (luminance Laplacian-like).
    var centreLum = _Luma<TPixel, TWork, TDecode>(source, decoder, x, y, sourceStride);
    var sum = 0f;
    var n = 0;
    if (x + 1 < width) { sum += _Luma<TPixel, TWork, TDecode>(source, decoder, x + 1, y, sourceStride); ++n; }
    if (x - 1 >= 0) { sum += _Luma<TPixel, TWork, TDecode>(source, decoder, x - 1, y, sourceStride); ++n; }
    if (y + 1 < endY) { sum += _Luma<TPixel, TWork, TDecode>(source, decoder, x, y + 1, sourceStride); ++n; }
    if (y - 1 >= 0) { sum += _Luma<TPixel, TWork, TDecode>(source, decoder, x, y - 1, sourceStride); ++n; }
    if (n == 0)
      return 0f;
    return Math.Abs(centreLum - sum / n);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float _Luma<TPixel, TWork, TDecode>(TPixel* source, in TDecode decoder, int x, int y, int sourceStride)
    where TPixel : unmanaged, IStorageSpace
    where TWork : unmanaged, IColorSpace4<TWork>
    where TDecode : struct, IDecode<TPixel, TWork> {
    var decoded = decoder.Decode(source[y * sourceStride + x]);
    var (q1, q2, q3, _) = decoded.ToNormalized();
    return 0.299f * q1.ToFloat() + 0.587f * q2.ToFloat() + 0.114f * q3.ToFloat();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _Deposit(float[,] errR, float[,] errG, float[,] errB, int x, int y, float er, float eg, float eb, float w, int width, int height) {
    if (x < 0 || x >= width || y < 0 || y >= height) return;
    errR[x, y] += er * w;
    errG[x, y] += eg * w;
    errB[x, y] += eb * w;
  }

  private static float[] _BuildCluster6() {
    // 6×6 clustered-dot screen — central cells have the lowest indices so
    // ink grows out from the centre of each cell.
    int[,] raw = {
      { 34, 29, 17, 21, 30, 35 },
      { 28, 14,  9, 16, 20, 31 },
      { 13,  8,  4,  5, 15, 19 },
      { 25,  3,  1,  2, 11, 12 },
      { 27,  7,  6, 10, 23, 24 },
      { 33, 26, 22, 18, 32, 36 }
    };
    var result = new float[_SCREEN * _SCREEN];
    const float max = _SCREEN * _SCREEN; // 36
    for (var y = 0; y < _SCREEN; ++y)
    for (var x = 0; x < _SCREEN; ++x)
      result[y * _SCREEN + x] = (raw[y, x] - 0.5f) / max - 0.5f;
    return result;
  }
}
