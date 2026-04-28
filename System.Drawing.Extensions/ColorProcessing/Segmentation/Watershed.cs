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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Guard;

namespace Hawkynt.ColorProcessing.Segmentation;

/// <summary>
/// Result of a watershed segmentation: per-pixel label map plus metadata.
/// </summary>
/// <remarks>
/// Pixels labelled <see cref="WatershedRidge"/> sit on watershed lines
/// (between basins). Other pixels carry the basin ID of the marker that
/// flooded them.
/// </remarks>
public sealed class WatershedResult {

  /// <summary>Sentinel label written to ridge pixels (between basins).</summary>
  public const int WatershedRidge = -1;

  /// <summary>Per-pixel basin label (or <see cref="WatershedRidge"/>).</summary>
  public int[,] Labels { get; }

  /// <summary>Number of distinct basins (one per non-zero marker label).</summary>
  public int BasinCount { get; }

  internal WatershedResult(int[,] labels, int basinCount) {
    this.Labels = labels;
    this.BasinCount = basinCount;
  }
}

/// <summary>
/// Marker-based watershed segmentation (Vincent-Soille 1991).
/// </summary>
/// <remarks>
/// <para>
/// Reference: Vincent, L., &amp; Soille, P. (1991). "Watersheds in digital
/// spaces: An efficient algorithm based on immersion simulations."
/// <i>IEEE Transactions on Pattern Analysis and Machine Intelligence,
/// 13</i>(6), 583–598.
/// </para>
/// <para>
/// Floods a topographic surface (the gradient image) starting from the
/// supplied markers. Each marker is interpreted as a "drain hole" labelled
/// with its non-zero ID. As the flood level rises, neighbouring pixels are
/// added to the basin of the lowest-ID neighbour; pixels reached by two
/// different basins simultaneously become watershed ridges.
/// </para>
/// <para>
/// This implementation uses a priority queue keyed by gradient intensity
/// and a FIFO secondary order — the standard "ordered-queues" variant of
/// Vincent-Soille that runs in <c>O(n log n)</c> on an n-pixel image.
/// </para>
/// </remarks>
public static class Watershed {

  // Sentinel for "not yet visited". Distinct from background marker (0) and
  // ridge label (WatershedResult.WatershedRidge = -1).
  private const int UNVISITED = -2;
  private const int IN_QUEUE = -3;

  /// <summary>Flood the gradient image from the given marker map.</summary>
  /// <param name="gradient">
  /// 8-bit gradient/intensity image. Lower values = lower terrain (basin
  /// floors). Common pre-processing: Sobel magnitude of the original image.
  /// </param>
  /// <param name="markers">
  /// Same dimensions as <paramref name="gradient"/>. Non-zero values are
  /// marker labels (the IDs of the resulting basins). <c>0</c> means
  /// "unmarked, decide by flooding". The marker label set must be a
  /// contiguous range starting at 1 if you want sequential basin IDs;
  /// otherwise the basin IDs in the result preserve the marker IDs as-is.
  /// </param>
  /// <param name="connectivity">4- or 8-connectivity (default 8).</param>
  /// <returns>The flooded label map.</returns>
  /// <exception cref="ArgumentNullException">Either input is null.</exception>
  /// <exception cref="ArgumentException">Inputs differ in size.</exception>
  public static WatershedResult Flood(Bitmap gradient, int[,] markers, Connectivity connectivity = Connectivity.Eight) {
    Against.ArgumentIsNull(gradient);
    Against.ArgumentIsNull(markers);
    var w = gradient.Width;
    var h = gradient.Height;
    if (markers.GetLength(0) != w || markers.GetLength(1) != h)
      throw new ArgumentException("Marker map must match gradient dimensions.", nameof(markers));

    // Read gradient as 8-bit luminance.
    var grad = new byte[w * h];
    using (var srcLock = gradient.Lock(ImageLockMode.ReadOnly)) {
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        var c = srcLock[x, y];
        grad[y * w + x] = (byte)((c.R * 299 + c.G * 587 + c.B * 114) / 1000);
      }
    }

    return _Flood(grad, markers, w, h, connectivity);
  }

  /// <summary>Flood directly from a pre-computed byte gradient buffer.</summary>
  /// <param name="gradient">Row-major <c>w·h</c> byte buffer.</param>
  /// <param name="width">Image width.</param>
  /// <param name="height">Image height.</param>
  /// <param name="markers">Marker map of dimensions <paramref name="width"/>×<paramref name="height"/>.</param>
  /// <param name="connectivity">4- or 8-connectivity.</param>
  /// <returns>The flooded label map.</returns>
  public static WatershedResult Flood(byte[] gradient, int width, int height, int[,] markers, Connectivity connectivity = Connectivity.Eight) {
    Against.ArgumentIsNull(gradient);
    Against.ArgumentIsNull(markers);
    Against.CountBelowOrEqualZero(width);
    Against.CountBelowOrEqualZero(height);
    if (gradient.Length < width * height) throw new ArgumentException("Gradient buffer too small.", nameof(gradient));
    if (markers.GetLength(0) != width || markers.GetLength(1) != height)
      throw new ArgumentException("Marker dimensions mismatch.", nameof(markers));
    return _Flood(gradient, markers, width, height, connectivity);
  }

  private static WatershedResult _Flood(byte[] grad, int[,] markers, int w, int h, Connectivity connectivity) {
    var labels = new int[w, h];
    // Initialise: copy markers, mark all unmarked pixels as UNVISITED.
    var basinIds = new HashSet<int>();
    for (var y = 0; y < h; ++y)
    for (var x = 0; x < w; ++x) {
      var m = markers[x, y];
      if (m > 0) {
        labels[x, y] = m;
        basinIds.Add(m);
      } else {
        labels[x, y] = UNVISITED;
      }
    }

    // 256-bucket priority queue, FIFO within each bucket. Vincent-Soille's
    // "hierarchical FIFO" variant — strictly correct because gradient is 8-bit.
    var queues = new Queue<int>[256];
    for (var i = 0; i < 256; ++i) queues[i] = new Queue<int>();

    var dx = connectivity == Connectivity.Eight ? new[] { -1, 0, 1, -1, 1, -1, 0, 1 } : new[] { 0, -1, 1, 0 };
    var dy = connectivity == Connectivity.Eight ? new[] { -1, -1, -1, 0, 0, 1, 1, 1 } : new[] { -1, 0, 0, 1 };
    var nbrCount = dx.Length;

    // Seed: every unvisited neighbour of a marker pixel goes onto the queue at its own gradient.
    for (var y = 0; y < h; ++y) {
      for (var x = 0; x < w; ++x) {
        if (labels[x, y] <= 0) continue; // skip non-markers (UNVISITED or 0 background; 0 not used here)
        for (var k = 0; k < nbrCount; ++k) {
          var nx = x + dx[k];
          var ny = y + dy[k];
          if ((uint)nx >= (uint)w || (uint)ny >= (uint)h) continue;
          if (labels[nx, ny] != UNVISITED) continue;
          labels[nx, ny] = IN_QUEUE;
          queues[grad[ny * w + nx]].Enqueue(ny * w + nx);
        }
      }
    }

    // Flood: drain queues in ascending bucket order.
    for (var level = 0; level < 256; ++level) {
      var q = queues[level];
      while (q.Count > 0) {
        var idx = q.Dequeue();
        var x = idx % w;
        var y = idx / w;

        // Inspect neighbours: choose the basin label, or watershed ridge.
        var assigned = 0;
        var conflict = false;
        for (var k = 0; k < nbrCount; ++k) {
          var nx = x + dx[k];
          var ny = y + dy[k];
          if ((uint)nx >= (uint)w || (uint)ny >= (uint)h) continue;
          var lab = labels[nx, ny];
          if (lab > 0) {
            if (assigned == 0) assigned = lab;
            else if (assigned != lab) { conflict = true; break; }
          }
        }

        labels[x, y] = conflict ? WatershedResult.WatershedRidge : assigned == 0 ? WatershedResult.WatershedRidge : assigned;

        if (!conflict && assigned > 0) {
          // Push unvisited neighbours into queue at max(level, neighbour-grad) to preserve
          // the monotonic flooding invariant.
          for (var k = 0; k < nbrCount; ++k) {
            var nx = x + dx[k];
            var ny = y + dy[k];
            if ((uint)nx >= (uint)w || (uint)ny >= (uint)h) continue;
            if (labels[nx, ny] != UNVISITED) continue;
            labels[nx, ny] = IN_QUEUE;
            var g = grad[ny * w + nx];
            if (g < level) g = (byte)level;
            queues[g].Enqueue(ny * w + nx);
          }
        }
      }
    }

    // Any remaining UNVISITED / IN_QUEUE pixels (isolated regions with no marker)
    // become ridges so the result is fully labelled.
    for (var y = 0; y < h; ++y)
    for (var x = 0; x < w; ++x)
      if (labels[x, y] == UNVISITED || labels[x, y] == IN_QUEUE)
        labels[x, y] = WatershedResult.WatershedRidge;

    return new WatershedResult(labels, basinIds.Count);
  }
}
