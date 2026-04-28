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
// <https://github.com/Hawkynt+C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Guard;

namespace Hawkynt.ColorProcessing.Segmentation;

/// <summary>
/// Selects pixel adjacency for <see cref="ConnectedComponentLabeller"/>.
/// </summary>
public enum Connectivity {
  /// <summary>4-connectivity: north, south, east, west.</summary>
  Four = 4,

  /// <summary>8-connectivity: 4-connectivity plus the four diagonals.</summary>
  Eight = 8,
}

/// <summary>
/// Result of connected-component labelling: the per-pixel label map plus
/// per-region statistics.
/// </summary>
/// <remarks>
/// Background pixels are labelled <c>0</c>; foreground regions receive
/// contiguous labels starting at <c>1</c>. <see cref="Count"/> excludes the
/// background; <see cref="Regions"/> has length <c>Count</c> (label <i>k</i>
/// at index <c>k − 1</c>).
/// </remarks>
public sealed class ComponentLabelResult {

  /// <summary>The label map, indexed <c>[x, y]</c>.</summary>
  public int[,] Labels { get; }

  /// <summary>Number of foreground regions (background label <c>0</c> excluded).</summary>
  public int Count { get; }

  /// <summary>Per-region descriptors. <c>Regions[0]</c> corresponds to label <c>1</c>.</summary>
  public ComponentRegion[] Regions { get; }

  internal ComponentLabelResult(int[,] labels, int count, ComponentRegion[] regions) {
    this.Labels = labels;
    this.Count = count;
    this.Regions = regions;
  }
}

/// <summary>
/// Per-region statistics produced by <see cref="ConnectedComponentLabeller"/>.
/// </summary>
public readonly struct ComponentRegion {

  /// <summary>The label value for this region (≥ 1).</summary>
  public int Label { get; }

  /// <summary>Pixel count.</summary>
  public int Area { get; }

  /// <summary>Tight axis-aligned bounding box.</summary>
  public Rectangle Bounds { get; }

  /// <summary>Centroid in pixel coordinates (mean of member pixel positions).</summary>
  public PointF Centroid { get; }

  /// <summary>Constructs a region descriptor.</summary>
  public ComponentRegion(int label, int area, Rectangle bounds, PointF centroid) {
    this.Label = label;
    this.Area = area;
    this.Bounds = bounds;
    this.Centroid = centroid;
  }
}

/// <summary>
/// Two-pass connected-component labelling with union-find equivalence
/// resolution (Hoshen-Kopelman 1976).
/// </summary>
/// <remarks>
/// <para>
/// Reference: Hoshen, J., &amp; Kopelman, R. (1976).
/// "Percolation and cluster distribution. I. Cluster multiple labeling
/// technique and critical concentration algorithm." <i>Physical Review B,
/// 14</i>(8), 3438–3445.
/// </para>
/// <para>
/// Pass 1 raster-scans the binary mask, assigning a provisional label to each
/// foreground pixel based on the labels of already-visited neighbours
/// (north + west for 4-connectivity, plus the two NW/NE diagonals for
/// 8-connectivity). Conflicts are merged in a union-find structure with path
/// compression and union-by-rank.
/// </para>
/// <para>
/// Pass 2 rewrites the provisional map with canonical (root) labels packed
/// into the contiguous range <c>[1, Count]</c>, while accumulating area,
/// bounding box, and centroid statistics.
/// </para>
/// <para>
/// The labeller is parameterised on a <see cref="Predicate{Color}"/> so callers
/// can supply any per-pixel "is foreground" rule (threshold, alpha mask, hue
/// gate, …). For the common binary-threshold case, see
/// <see cref="LabelThreshold"/>.
/// </para>
/// </remarks>
public static class ConnectedComponentLabeller {

  /// <summary>Labels the foreground pixels of <paramref name="source"/>.</summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="isForeground">Predicate returning <c>true</c> for foreground pixels.</param>
  /// <param name="connectivity">4- or 8-connectivity (default 8).</param>
  /// <returns>The label map plus per-region statistics.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="isForeground"/> is null.</exception>
  public static ComponentLabelResult Label(Bitmap source, Predicate<Color> isForeground, Connectivity connectivity = Connectivity.Eight) {
    Against.ArgumentIsNull(source);
    Against.ArgumentIsNull(isForeground);

    var w = source.Width;
    var h = source.Height;

    // Build foreground mask first to avoid keeping the bitmap locked across union-find work.
    var mask = new bool[w * h];
    using (var srcLock = source.Lock(ImageLockMode.ReadOnly)) {
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x)
        mask[y * w + x] = isForeground(srcLock[x, y]);
    }

    return _Label(mask, w, h, connectivity);
  }

  /// <summary>Convenience: labels pixels whose luminance ≥ <paramref name="threshold"/>.</summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="threshold">Inclusive luminance threshold (0–255). Default 128.</param>
  /// <param name="connectivity">4- or 8-connectivity.</param>
  /// <returns>The label map plus statistics.</returns>
  public static ComponentLabelResult LabelThreshold(Bitmap source, int threshold = 128, Connectivity connectivity = Connectivity.Eight)
    => Label(source, c => (c.R * 299 + c.G * 587 + c.B * 114) / 1000 >= threshold, connectivity);

  private static ComponentLabelResult _Label(bool[] mask, int w, int h, Connectivity connectivity) {
    // Provisional labels, indexed [y * w + x]. 0 = background.
    var prov = new int[w * h];

    // Union-find. Index 0 is reserved for "no label / background".
    // Capacity grows on demand; the upper bound for a w×h binary image is w*h/2 + 1 (checkerboard).
    var parent = new int[Math.Max(64, (w * h) / 4 + 1)];
    var rank = new byte[parent.Length];
    var nextLabel = 1;

    int Allocate() {
      if (nextLabel >= parent.Length) {
        var newSize = parent.Length * 2;
        Array.Resize(ref parent, newSize);
        Array.Resize(ref rank, newSize);
      }
      parent[nextLabel] = nextLabel;
      rank[nextLabel] = 0;
      return nextLabel++;
    }

    int Find(int a) {
      // Path compression.
      var root = a;
      while (parent[root] != root)
        root = parent[root];
      while (parent[a] != root) {
        var next = parent[a];
        parent[a] = root;
        a = next;
      }
      return root;
    }

    void Union(int a, int b) {
      var ra = Find(a);
      var rb = Find(b);
      if (ra == rb) return;
      if (rank[ra] < rank[rb]) {
        parent[ra] = rb;
      } else if (rank[ra] > rank[rb]) {
        parent[rb] = ra;
      } else {
        parent[rb] = ra;
        rank[ra]++;
      }
    }

    var eight = connectivity == Connectivity.Eight;

    // Pass 1: provisional labelling + union resolution.
    for (var y = 0; y < h; ++y) {
      for (var x = 0; x < w; ++x) {
        var idx = y * w + x;
        if (!mask[idx]) continue;

        var n = (y > 0) ? prov[idx - w] : 0;
        var ww = (x > 0) ? prov[idx - 1] : 0;
        var nw = (eight && x > 0 && y > 0) ? prov[idx - w - 1] : 0;
        var ne = (eight && x < w - 1 && y > 0) ? prov[idx - w + 1] : 0;

        var minLabel = 0;
        if (n != 0 && (minLabel == 0 || n < minLabel)) minLabel = n;
        if (ww != 0 && (minLabel == 0 || ww < minLabel)) minLabel = ww;
        if (nw != 0 && (minLabel == 0 || nw < minLabel)) minLabel = nw;
        if (ne != 0 && (minLabel == 0 || ne < minLabel)) minLabel = ne;

        if (minLabel == 0) {
          prov[idx] = Allocate();
        } else {
          prov[idx] = minLabel;
          if (n != 0) Union(minLabel, n);
          if (ww != 0) Union(minLabel, ww);
          if (nw != 0) Union(minLabel, nw);
          if (ne != 0) Union(minLabel, ne);
        }
      }
    }

    // Pass 2: build canonical → packed-label map and accumulate stats.
    // canonical[k] = root label of provisional label k.
    // packed[root] = final 1..count label, lazily assigned in scan order.
    var packed = new int[nextLabel];
    var count = 0;

    var areas = new int[1];
    var minXs = new int[1];
    var minYs = new int[1];
    var maxXs = new int[1];
    var maxYs = new int[1];
    var sumXs = new long[1];
    var sumYs = new long[1];

    void Grow(int needed) {
      if (needed < areas.Length) return;
      var newSize = Math.Max(needed + 1, areas.Length * 2);
      Array.Resize(ref areas, newSize);
      Array.Resize(ref minXs, newSize);
      Array.Resize(ref minYs, newSize);
      Array.Resize(ref maxXs, newSize);
      Array.Resize(ref maxYs, newSize);
      Array.Resize(ref sumXs, newSize);
      Array.Resize(ref sumYs, newSize);
    }

    var labels = new int[w, h];
    for (var y = 0; y < h; ++y) {
      for (var x = 0; x < w; ++x) {
        var p = prov[y * w + x];
        if (p == 0) {
          labels[x, y] = 0;
          continue;
        }
        var root = Find(p);
        var pk = packed[root];
        if (pk == 0) {
          ++count;
          pk = count;
          packed[root] = pk;
          Grow(pk);
          minXs[pk] = x; maxXs[pk] = x;
          minYs[pk] = y; maxYs[pk] = y;
          areas[pk] = 1;
          sumXs[pk] = x;
          sumYs[pk] = y;
        } else {
          if (x < minXs[pk]) minXs[pk] = x;
          if (x > maxXs[pk]) maxXs[pk] = x;
          if (y < minYs[pk]) minYs[pk] = y;
          if (y > maxYs[pk]) maxYs[pk] = y;
          areas[pk]++;
          sumXs[pk] += x;
          sumYs[pk] += y;
        }
        labels[x, y] = pk;
      }
    }

    var regions = new ComponentRegion[count];
    for (var k = 1; k <= count; ++k) {
      var area = areas[k];
      var rect = new Rectangle(minXs[k], minYs[k], maxXs[k] - minXs[k] + 1, maxYs[k] - minYs[k] + 1);
      var cen = new PointF((float)((double)sumXs[k] / area), (float)((double)sumYs[k] / area));
      regions[k - 1] = new ComponentRegion(k, area, rect, cen);
    }

    return new ComponentLabelResult(labels, count, regions);
  }
}
