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
/// Ordered dithering using a pre-placed Poisson-disc stipple pattern —
/// dots are added to the screen in an order that maximises the minimum
/// inter-dot distance, so the resulting threshold matrix has stipple-art
/// aesthetics rather than halftone or Bayer grid structure.
/// </summary>
/// <remarks>
/// <para>
/// Generated once at type-load by Mitchell's best-candidate algorithm
/// (K. Mitchell, 1987), a deterministic Poisson-disc approximation: for each
/// new sample the algorithm draws a fixed number of random candidates and
/// keeps the one furthest from all previously-placed dots. With a fixed seed
/// this produces a completely deterministic, reproducible 16×16 stipple
/// sequence. Rank in the placement order becomes the threshold, yielding a
/// 256-level screen whose white→black transition resembles hand-stippled ink
/// art: new dots land in the largest remaining "voids" rather than next to
/// existing dots.
/// </para>
/// <para>
/// Artefact profile: isotropic stipple with no axis-aligned or diagonal grain.
/// Distinct from <c>VoidAndClusterDitherer</c>: void-and-cluster uses iterative
/// void/cluster swaps and targets blue-noise spectrum, while stipple here
/// targets the visual "best-candidate" dot placement that stippling pen-and-
/// ink illustrations approximate. The two look similar at a distance but the
/// artistic intent differs — stipple keeps large unbroken voids in the midtone
/// regions that classical void-and-cluster aggressively fills.
/// </para>
/// <para>
/// References: K. Mitchell 1987 "Generating antialiased images at low sampling
/// densities", SIGGRAPH Computer Graphics vol. 21, pp. 65-72 (best-candidate
/// algorithm). Poisson-disc sampling in general: R. Cook 1986, "Stochastic
/// sampling in computer graphics", ACM TOG vol. 5, pp. 51-72. Hand-stipple
/// analogue: A. Secord 2002, "Weighted Voronoi stippling", NPAR.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Stipple", Description = "Pre-placed Poisson-disc stipple threshold screen (best-candidate)", Type = DitheringType.Ordered, Author = "Don P. Mitchell", Year = 1987)]
public readonly struct StippleDitherer : IDitherer {

  private const int _SIZE = 16;

  // Mitchell's best-candidate Poisson-disc matrix, materialised once at
  // type-load and shared across all calls. 256 unique threshold levels.
  private static readonly float[,] _Matrix = _BuildStippleMatrix(_SIZE, 42);

  private static readonly OrderedDitherer _Inner = new(_Matrix);

  /// <summary>Default instance.</summary>
  public static StippleDitherer Instance { get; } = new();

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

  /// <summary>
  /// Builds a <paramref name="size"/>×<paramref name="size"/> stipple matrix
  /// whose cells contain placement-rank indices 0..size²-1. A cell with rank
  /// <c>k</c> is the <c>k</c>-th dot that would be placed by Mitchell's
  /// best-candidate algorithm on a toroidal (wrap-around) <paramref name="size"/>-
  /// side grid. Runs exactly once per process (at type-load).
  /// </summary>
  private static float[,] _BuildStippleMatrix(int size, int seed) {
    var total = size * size;
    var ranks = new int[total];
    var rng = new Random(seed);

    // Swap-remove remaining list: fast O(1) "pick uniformly random unplaced".
    var remaining = new int[total];
    for (var i = 0; i < total; ++i)
      remaining[i] = i;
    var remainingCount = total;

    // Place dot 0 at a deterministic starting cell (0,0).
    ranks[0] = 0;
    _RemoveFromList(remaining, ref remainingCount, 0);

    for (var k = 1; k < total; ++k) {
      // Candidate count grows with k; clamp at 10 per Mitchell's recommendation.
      var candidateCount = Math.Min(10, remainingCount);
      var bestRemovalPos = -1;
      var bestCandidateIdx = -1;
      var bestMinDist = -1.0;

      for (var c = 0; c < candidateCount; ++c) {
        var pos = rng.Next(remainingCount);
        var candidateIdx = remaining[pos];

        var cx = candidateIdx % size;
        var cy = candidateIdx / size;

        // Minimum toroidal distance from this candidate to any placed dot.
        var minDist = double.MaxValue;
        for (var j = 0; j < k; ++j) {
          var pIdx = ranks[j];
          var px = pIdx % size;
          var py = pIdx / size;
          var dx = Math.Abs(cx - px);
          var dy = Math.Abs(cy - py);
          if (dx > size / 2) dx = size - dx;
          if (dy > size / 2) dy = size - dy;
          var d = dx * (double)dx + dy * (double)dy;
          if (d < minDist)
            minDist = d;
        }

        if (minDist > bestMinDist) {
          bestMinDist = minDist;
          bestCandidateIdx = candidateIdx;
          bestRemovalPos = pos;
        }
      }

      ranks[k] = bestCandidateIdx;
      _RemoveFromList(remaining, ref remainingCount, bestRemovalPos);
    }

    // Invert: ranks[k] = grid index placed at rank k → matrix[y,x] = rank.
    var matrix = new float[size, size];
    for (var k = 0; k < total; ++k) {
      var idx = ranks[k];
      matrix[idx / size, idx % size] = k;
    }
    return matrix;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _RemoveFromList(int[] list, ref int count, int pos) {
    --count;
    list[pos] = list[count];
  }
}
