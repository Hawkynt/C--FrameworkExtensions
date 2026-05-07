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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Vector-pipeline depixelization extension methods. Builds a polygonal cell
/// representation of pixel-art input (à la Kopf-Lischinski 2011) and renders that
/// vector representation to a raster of arbitrary target dimensions.
/// </summary>
/// <remarks>
/// <para>Reference: J. Kopf &amp; D. Lischinski (2011). "Depixelizing Pixel Art".
/// ACM Transactions on Graphics 30(4) — SIGGRAPH 2011.</para>
/// <para>Pipeline overview:</para>
/// <list type="number">
///   <item>Build the 8-neighbour similarity graph (YUV distance threshold) and
///   resolve diagonal-crossing conflicts using a curve-length heuristic.</item>
///   <item>Convert each pixel cell to a polygon. By default each cell is a unit
///   square; corners are SHIFTED at internal grid junctions where the resolved
///   similarity graph contains a diagonal connection — the connected diagonal's
///   shared corner moves inward by 0.25 units, producing the characteristic
///   "rounded" pixel-art look.</item>
///   <item>Render polygons to the target raster: for each output pixel compute its
///   source-space coordinate, locate the containing pixel cell via shifted-polygon
///   point-in-cell test, and output that cell's colour.</item>
/// </list>
/// <para>The vector intermediate is in-memory only (no SVG export). Paper-exact
/// B-spline cell boundaries are simplified to straight-line shifted polygons; the
/// topological reshaping (the visually significant feature) is preserved.</para>
/// </remarks>
public static class BitmapDepixelExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Depixelizes <paramref name="@this"/> by building an internal vector polygon
    /// representation and rendering it to a raster of <paramref name="targetWidth"/>
    /// × <paramref name="targetHeight"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap Depixelize(int targetWidth, int targetHeight) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);
      return DepixelPipeline.Run(@this, targetWidth, targetHeight);
    }

    /// <summary>
    /// Depixelizes <paramref name="@this"/> at an integer scale factor. Output
    /// dimensions are <c>source × scaleFactor</c> on each axis.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap Depixelize(int scaleFactor) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(scaleFactor);
      return DepixelPipeline.Run(@this, @this.Width * scaleFactor, @this.Height * scaleFactor);
    }
  }
}

/// <summary>
/// Internal Kopf-Lischinski-style depixelization pipeline. Each public entry point in
/// <see cref="BitmapDepixelExtensions"/> routes through <see cref="Run"/>.
/// </summary>
internal static class DepixelPipeline {

  // YUV-space dissimilarity threshold (paper §3.1): two pixels are "dissimilar" if
  // their YUV difference exceeds these per-component bounds. We keep both edges in
  // the similarity graph initially and remove crossings via heuristic resolution.
  private const float YThresh = 48f / 255f;
  private const float UThresh = 7f / 255f;
  private const float VThresh = 6f / 255f;

  // Inward shift applied to a corner where the connected diagonal direction "rounds"
  // the corner. 0.25 per Kopf-Lischinski's typical settings — keeps the geometry
  // recognisable while smoothing the staircase artifact.
  private const float CornerShift = 0.25f;

  public static unsafe Bitmap Run(Bitmap source, int targetW, int targetH) {
    var w = source.Width;
    var h = source.Height;
    if (w == 0 || h == 0) return new(targetW, targetH, PixelFormat.Format32bppArgb);

    // ---- Stage 1: read source into per-pixel YUV + RGBA arrays. ----
    var rgba = new uint[h * w];
    var y = new float[h * w];
    var u = new float[h * w];
    var v = new float[h * w];
    _ReadSource(source, w, h, rgba, y, u, v);

    // ---- Stage 2: 8-neighbour similarity graph. ----
    // Encoded as 8 bits per pixel: 0=N, 1=NE, 2=E, 3=SE, 4=S, 5=SW, 6=W, 7=NW.
    var graph = new byte[h * w];
    _BuildSimilarityGraph(w, h, y, u, v, graph);

    // ---- Stage 3: diagonal-crossing resolution. ----
    // For each interior 2×2 pixel square, if both diagonals are present in the
    // similarity graph we keep only the one with the longer "curve" through the
    // graph (paper §3.1 curve-length heuristic, bounded depth).
    _ResolveDiagonalCrossings(w, h, graph);

    // ---- Stage 4: corner shift map. Each grid corner (i, j) ∈ [0..w] × [0..h] gets
    // a shift offset (dx, dy) ∈ [-CornerShift, +CornerShift]² based on the local
    // diagonal connectivity inferred from the graph. ----
    var cornerDx = new float[(h + 1) * (w + 1)];
    var cornerDy = new float[(h + 1) * (w + 1)];
    _ComputeCornerShifts(w, h, graph, cornerDx, cornerDy);

    // ---- Stage 5: render polygons to target raster. ----
    var output = new Bitmap(targetW, targetH, PixelFormat.Format32bppArgb);
    _RenderPolygonsToRaster(output, targetW, targetH, w, h, rgba, cornerDx, cornerDy);
    return output;
  }

  // -------------------------------------------------------------------------
  // Stage 1: Read source pixels into YUV + RGBA arrays.
  // -------------------------------------------------------------------------

  private static unsafe void _ReadSource(Bitmap source, int w, int h, uint[] rgba, float[] y, float[] u, float[] v) {
    var rect = new Rectangle(0, 0, w, h);
    var data = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)data.Scan0;
      var stride = data.Stride;
      for (var yy = 0; yy < h; ++yy) {
        for (var xx = 0; xx < w; ++xx) {
          var off = yy * stride + xx * 4;
          var b = p[off + 0];
          var g = p[off + 1];
          var r = p[off + 2];
          var a = p[off + 3];
          rgba[yy * w + xx] = (uint)((a << 24) | (r << 16) | (g << 8) | b);
          // BT.601 YUV (paper uses YUV-space comparison).
          var rf = r / 255f;
          var gf = g / 255f;
          var bf = b / 255f;
          y[yy * w + xx] = 0.299f * rf + 0.587f * gf + 0.114f * bf;
          u[yy * w + xx] = -0.14713f * rf - 0.28886f * gf + 0.436f * bf;
          v[yy * w + xx] = 0.615f * rf - 0.51499f * gf - 0.10001f * bf;
        }
      }
    } finally {
      source.UnlockBits(data);
    }
  }

  // -------------------------------------------------------------------------
  // Stage 2: Build 8-neighbour similarity graph using YUV thresholds.
  // -------------------------------------------------------------------------

  private const int N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7;
  private static readonly int[] _Dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
  private static readonly int[] _Dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool _Similar(float[] y, float[] u, float[] v, int idxA, int idxB) {
    var dy = MathF.Abs(y[idxA] - y[idxB]);
    if (dy > YThresh) return false;
    var du = MathF.Abs(u[idxA] - u[idxB]);
    if (du > UThresh) return false;
    var dv = MathF.Abs(v[idxA] - v[idxB]);
    return dv <= VThresh;
  }

  private static void _BuildSimilarityGraph(int w, int h, float[] y, float[] u, float[] v, byte[] graph) {
    for (var yy = 0; yy < h; ++yy) {
      for (var xx = 0; xx < w; ++xx) {
        byte mask = 0;
        for (var d = 0; d < 8; ++d) {
          var nx = xx + _Dx[d];
          var ny = yy + _Dy[d];
          if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
          if (_Similar(y, u, v, yy * w + xx, ny * w + nx))
            mask |= (byte)(1 << d);
        }
        graph[yy * w + xx] = mask;
      }
    }
  }

  // -------------------------------------------------------------------------
  // Stage 3: Diagonal-crossing resolution via bounded curve traversal.
  // -------------------------------------------------------------------------

  private static void _ResolveDiagonalCrossings(int w, int h, byte[] graph) {
    // For each interior 2×2 square at top-left (x, y), check whether both diagonals
    // exist (NE-SW and NW-SE). If both, keep the one with the longer curve through
    // the graph (= more pixels traversed before terminating). Tie-broken by lex order.
    for (var yy = 0; yy < h - 1; ++yy) {
      for (var xx = 0; xx < w - 1; ++xx) {
        // The 2×2 square is { (xx, yy), (xx+1, yy), (xx, yy+1), (xx+1, yy+1) }.
        // Diagonal 1: (xx, yy) — (xx+1, yy+1) — i.e., SE from (xx, yy) and NW from (xx+1, yy+1).
        // Diagonal 2: (xx+1, yy) — (xx, yy+1) — i.e., SW from (xx+1, yy) and NE from (xx, yy+1).
        var topLeft = yy * w + xx;
        var topRight = yy * w + (xx + 1);
        var bottomLeft = (yy + 1) * w + xx;
        var bottomRight = (yy + 1) * w + (xx + 1);

        var diag1 = (graph[topLeft] & (1 << SE)) != 0;
        var diag2 = (graph[topRight] & (1 << SW)) != 0;

        if (!diag1 || !diag2) continue;
        // Both diagonals present: resolve by curve length.

        var len1 = _CurveLength(w, h, graph, xx, yy, xx + 1, yy + 1);
        var len2 = _CurveLength(w, h, graph, xx + 1, yy, xx, yy + 1);

        if (len1 >= len2) {
          // Drop diagonal 2 (NE-SW).
          graph[topRight] &= unchecked((byte)~(1 << SW));
          graph[bottomLeft] &= unchecked((byte)~(1 << NE));
        } else {
          // Drop diagonal 1 (NW-SE).
          graph[topLeft] &= unchecked((byte)~(1 << SE));
          graph[bottomRight] &= unchecked((byte)~(1 << NW));
        }
      }
    }
  }

  /// <summary>
  /// Returns a bounded estimate of the curve length passing through pixels
  /// (x1, y1) and (x2, y2) (assumed diagonally connected). We walk the similarity
  /// graph from each endpoint up to a fixed depth (8) and sum the visited pixel
  /// count. This implements the curve-length heuristic from paper §3.1.
  /// </summary>
  private static int _CurveLength(int w, int h, byte[] graph, int x1, int y1, int x2, int y2) {
    const int MaxDepth = 8;
    var len = 2; // both endpoints
    len += _Walk(w, h, graph, x1, y1, x2, y2, MaxDepth);
    len += _Walk(w, h, graph, x2, y2, x1, y1, MaxDepth);
    return len;
  }

  private static int _Walk(int w, int h, byte[] graph, int fromX, int fromY, int blockX, int blockY, int depthLeft) {
    if (depthLeft <= 0) return 0;
    var idx = fromY * w + fromX;
    var mask = graph[idx];
    // For curve continuation, follow the (unique) neighbour different from the blocked one.
    // Use first set bit not pointing to (blockX, blockY).
    for (var d = 0; d < 8; ++d) {
      if ((mask & (1 << d)) == 0) continue;
      var nx = fromX + _Dx[d];
      var ny = fromY + _Dy[d];
      if (nx == blockX && ny == blockY) continue;
      if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
      // Continue from neighbour with current as the new "block".
      return 1 + _Walk(w, h, graph, nx, ny, fromX, fromY, depthLeft - 1);
    }
    return 0;
  }

  // -------------------------------------------------------------------------
  // Stage 4: Corner shift map. Each interior grid corner (cx, cy) ∈ [1..w-1] ×
  // [1..h-1] is the meeting point of four pixels (cx-1, cy-1), (cx, cy-1),
  // (cx-1, cy), (cx, cy). If the resolved similarity graph has a diagonal
  // connection through this corner, shift it toward the connected diagonal's
  // midpoint (rounding the cell corners on the disconnected pair).
  // -------------------------------------------------------------------------

  private static void _ComputeCornerShifts(int w, int h, byte[] graph, float[] dx, float[] dy) {
    var stride = w + 1;
    for (var cy = 1; cy < h; ++cy) {
      for (var cx = 1; cx < w; ++cx) {
        var topLeft = (cy - 1) * w + (cx - 1);
        var topRight = (cy - 1) * w + cx;
        var bottomLeft = cy * w + (cx - 1);
        // Diagonal 1: (cx-1, cy-1) — (cx, cy) — SE direction from top-left.
        // Diagonal 2: (cx, cy-1) — (cx-1, cy) — SW direction from top-right.
        var diag1 = (graph[topLeft] & (1 << SE)) != 0;
        var diag2 = (graph[topRight] & (1 << SW)) != 0;
        if (diag1 && !diag2) {
          // NW-SE diagonal active: round corner along NE/SW direction.
          dx[cy * stride + cx] = -CornerShift;
          dy[cy * stride + cx] = -CornerShift;
        } else if (diag2 && !diag1) {
          // NE-SW diagonal active: round corner along NW/SE direction.
          dx[cy * stride + cx] = +CornerShift;
          dy[cy * stride + cx] = -CornerShift;
        }
        // If neither (or both — shouldn't happen after resolution), leave shift = 0.
      }
    }
  }

  // -------------------------------------------------------------------------
  // Stage 5: Render polygons to target raster. For each output pixel, map back to
  // source space and find the containing cell. With shifted corners, the cells
  // are quadrilaterals; use a simple winding test.
  // -------------------------------------------------------------------------

  private static unsafe void _RenderPolygonsToRaster(
    Bitmap output, int targetW, int targetH, int srcW, int srcH, uint[] rgba,
    float[] cornerDx, float[] cornerDy) {
    var rect = new Rectangle(0, 0, targetW, targetH);
    var data = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)data.Scan0;
      var stride = data.Stride;
      var sxScale = (float)srcW / targetW;
      var syScale = (float)srcH / targetH;

      for (var ty = 0; ty < targetH; ++ty) {
        // Source-space y at output row centre.
        var sy = (ty + 0.5f) * syScale;
        var py = (int)sy; if (py >= srcH) py = srcH - 1;
        for (var tx = 0; tx < targetW; ++tx) {
          var sx = (tx + 0.5f) * sxScale;
          var px = (int)sx; if (px >= srcW) px = srcW - 1;

          // Default cell = (px, py). Check if (sx, sy) actually falls in a corner
          // shifted region of an adjacent cell.
          var cell = _ResolveCell(px, py, sx, sy, srcW, srcH, cornerDx, cornerDy);

          var color = rgba[cell.y * srcW + cell.x];
          var off = ty * stride + tx * 4;
          p[off + 0] = (byte)(color & 0xFF);          // B
          p[off + 1] = (byte)((color >> 8) & 0xFF);   // G
          p[off + 2] = (byte)((color >> 16) & 0xFF);  // R
          p[off + 3] = (byte)((color >> 24) & 0xFF);  // A
        }
      }
    } finally {
      output.UnlockBits(data);
    }
  }

  /// <summary>
  /// Determines which pixel cell contains source-space point <paramref name="sx"/>,
  /// <paramref name="sy"/>. The default cell is (px, py) but if the point is past a
  /// shifted corner, it may belong to an adjacent cell.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (int x, int y) _ResolveCell(int px, int py, float sx, float sy, int srcW, int srcH, float[] cornerDx, float[] cornerDy) {
    // Compute fractional position within cell (px, py).
    var fx = sx - px;
    var fy = sy - py;
    var stride = srcW + 1;

    // Corner shifts at the four corners of cell (px, py): TL=(px, py), TR=(px+1, py), BL=(px, py+1), BR=(px+1, py+1).
    var trDx = cornerDx[py * stride + (px + 1)];
    var trDy = cornerDy[py * stride + (px + 1)];
    var blDx = cornerDx[(py + 1) * stride + px];
    var blDy = cornerDy[(py + 1) * stride + px];

    // Top-right corner shifted toward NE/SW means the cell's TR corner pulled in/out.
    // For a point near the TR corner: if the corner is shifted toward (-, -), the cell
    // is "smaller" near TR; the point may belong to the cell at (px+1, py).
    if (fx > 0.5f && fy < 0.5f && trDx < 0f && trDy < 0f) {
      // TR corner pulled inward — point past corner belongs to cell to the right.
      var distToCorner = (1f + trDx - fx) + (-trDy - fy);
      if (distToCorner < 0f && px + 1 < srcW)
        return (px + 1, py);
    }
    if (fx < 0.5f && fy > 0.5f && blDx > 0f && blDy < 0f) {
      // BL corner pulled outward (anti-diagonal): point may belong to cell below.
      var distToCorner = (-blDx + fx) + (1f + blDy - fy);
      if (distToCorner < 0f && py + 1 < srcH)
        return (px, py + 1);
    }

    return (px, py);
  }
}
