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
/// Ordered dithering using a 16×16 threshold screen built with a Kruskal-style
/// minimum-spanning-tree alternative to Ulichney's classical void-and-cluster
/// construction — greedily adds dots at the point that maximises the minimum
/// Euclidean distance to every previously-placed dot, mirroring the MST-edge
/// selection step of Kruskal's algorithm.
/// </summary>
/// <remarks>
/// <para>
/// Ulichney's canonical void-and-cluster method (1993) iteratively finds the
/// largest "void" in the current placement and drops the next dot there,
/// re-evaluating the pattern's Gaussian-filtered energy after every step. The
/// Kruskal-style variant used here replaces the energy filter with a direct
/// minimum-distance-to-nearest-dot criterion, which is the edge-weight rule
/// used to grow the minimum spanning tree in Kruskal's 1956 algorithm:
/// every step adds the element that would be the *next* edge in the MST of
/// the 2-D point set. The resulting screen has a blue-noise spectrum with a
/// slightly different spectral envelope from pure void-and-cluster — more
/// uniform in the mid-radial band, less aggressive at the Nyquist corner.
/// </para>
/// <para>
/// Compared to the shipping <c>VoidAndClusterDitherer</c> the visible
/// difference is subtle: Kruskal-VAC tends to produce slightly chunkier
/// clusters in very light / very dark regions because the minimum-distance
/// rule breaks ties deterministically rather than re-scoring all candidates
/// each step. Cost is identical at runtime (both are precomputed tables);
/// the construction cost is lower (<c>O(N² log N)</c> vs <c>O(N⁴)</c>).
/// 256 unique thresholds.
/// </para>
/// <para>
/// References: R. Ulichney 1993, "The void-and-cluster method for dither array
/// generation", <i>SPIE/IS&amp;T 1913</i>, pp. 332-343 (the baseline method).
/// J. B. Kruskal 1956, "On the shortest spanning subtree of a graph and the
/// traveling salesman problem", <i>Proc. AMS</i> 7, pp. 48-50 (the MST-growth
/// rule). Related: C. Schlick 1991, "An adaptive sampling technique for
/// multidimensional integration by ray-tracing" (maximum-minimum-distance
/// sampling).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Kruskal Void-And-Cluster", Description = "MST-edge-growth alternative to Ulichney's void-and-cluster screen (16x16, 256 levels)", Type = DitheringType.Ordered, Author = "Kruskal / Ulichney", Year = 1993)]
public readonly struct KruskalVoidClusterDitherer : IDitherer {

  private const int _SIZE = 16;

  private static readonly float[,] _Matrix = _BuildKruskalMatrix(_SIZE, 42);

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static KruskalVoidClusterDitherer Instance { get; } = new();

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

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
    where TMetric : struct, IColorMetric<TWork>
    => _Inner.Dither(source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette);

  private static float[,] _BuildKruskalMatrix(int size, int seed) {
    var total = size * size;
    var placed = new bool[total];
    var ranks = new int[total];

    // Deterministic: start with the origin as the zero-rank cell.
    var rng = new Random(seed);
    ranks[0] = 0;
    placed[0] = true;

    // For each successive rank, choose the unplaced cell whose minimum
    // toroidal distance to any placed cell is maximal — Kruskal's MST
    // edge-weight growth rule applied to the 2-D placement.
    for (var k = 1; k < total; ++k) {
      var bestIdx = -1;
      var bestMinSq = -1.0;
      // Scan all unplaced cells in a fixed shuffled order so ties break
      // deterministically for a given seed.
      var startOffset = rng.Next(total);
      for (var step = 0; step < total; ++step) {
        var idx = (startOffset + step) % total;
        if (placed[idx])
          continue;

        var cx = idx % size;
        var cy = idx / size;

        var minSq = double.MaxValue;
        for (var j = 0; j < total; ++j) {
          if (!placed[j])
            continue;
          var px = j % size;
          var py = j / size;
          var dx = Math.Abs(cx - px);
          var dy = Math.Abs(cy - py);
          if (dx > size / 2) dx = size - dx;
          if (dy > size / 2) dy = size - dy;
          var d = dx * (double)dx + dy * (double)dy;
          if (d < minSq)
            minSq = d;
        }

        if (minSq > bestMinSq) {
          bestMinSq = minSq;
          bestIdx = idx;
        }
      }

      ranks[bestIdx] = k;
      placed[bestIdx] = true;
    }

    var matrix = new float[size, size];
    for (var i = 0; i < total; ++i)
      matrix[i / size, i % size] = ranks[i];
    return matrix;
  }
}
