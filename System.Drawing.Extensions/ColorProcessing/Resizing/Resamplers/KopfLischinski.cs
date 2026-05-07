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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Kopf-Lischinski 2011 pixel-art depixelization with all three voting heuristics
/// (curves, sparse-pixels, islands) for diagonal-crossing resolution. Raster-only —
/// the paper's optional Voronoi/B-spline vector output is NOT generated.
/// </summary>
/// <remarks>
/// <para>Reference: J. Kopf &amp; D. Lischinski, "Depixelizing Pixel Art", SIGGRAPH
/// 2011. The diagonal-crossing resolution implements the paper's §3.1 voting weights:
/// curves (length of curve through each diagonal, bounded traversal of the similarity
/// graph), sparse-pixels (count of each diagonal's colour in an 8×8 window — rarer
/// wins), and islands (the diagonal whose removal would create a valence-1 isolated
/// pixel is preserved).</para>
/// <para>The output is a raster image at the requested target resolution. The paper's
/// §3.2-3.4 vector pipeline (Voronoi diagram → smooth-cubic B-splines → final raster
/// rendering with stroke/fill) is not implemented; for paper-faithful vector output use
/// the original DePixel reference implementation.</para>
/// </remarks>
[ScalerInfo("Kopf-Lischinski", Author = "Kopf & Lischinski", Year = 2011,
  Url = "https://johanneskopf.de/publications/pixelart/paper/pixel.pdf",
  Description = "Kopf-Lischinski 2011 raster depixelization with all 3 voting heuristics (curves/sparse/islands)",
  Category = ScalerCategory.Resampler)]
public readonly struct KopfLischinski : IEdgeAwareResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TEquality, TResult>(
    IEdgeAwareResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TEquality, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    TEquality equality = default,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TEquality : struct, IColorEquality<TKey>
    => callback.Invoke(new KopfLischinskiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, equality, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static KopfLischinski Default => new();
}

#region Kopf-Lischinski Kernel

file struct KopfLischinskiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality>(
  int sourceWidth,
  int sourceHeight,
  int targetWidth,
  int targetHeight,
  TEquality equality,
  bool useCenteredGrid
)
  : IEdgeAwareResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TEquality : struct, IColorEquality<TKey> {
  private readonly TEquality _equality = equality;

  // Precomputed scale factors and offsets for zero-cost grid centering
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;

  // Heuristic weights for diagonal ambiguity resolution
  private const float ValenceWeight = 0.4f;
  private const float CurveWeight = 0.3f;

  public int Radius => 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {

    // Map destination pixel to source coordinates
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

    // Integer coordinates of center source pixel
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts for subpixel position
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Get 3x3 neighborhood around the center pixel
    var nw = frame[x0 - 1, y0 - 1];
    var n = frame[x0, y0 - 1];
    var ne = frame[x0 + 1, y0 - 1];
    var w = frame[x0 - 1, y0];
    var c = frame[x0, y0];
    var e = frame[x0 + 1, y0];
    var sw = frame[x0 - 1, y0 + 1];
    var s = frame[x0, y0 + 1];
    var se = frame[x0 + 1, y0 + 1];

    // Build local similarity graph using TEquality
    var leftEdge = this._equality.Equals(c.Key, w.Key) ? 1f : 0f;
    var rightEdge = this._equality.Equals(c.Key, e.Key) ? 1f : 0f;
    var topEdge = this._equality.Equals(c.Key, n.Key) ? 1f : 0f;
    var bottomEdge = this._equality.Equals(c.Key, s.Key) ? 1f : 0f;

    // Diagonal edges for ambiguity resolution
    var neWeight = this._equality.Equals(sw.Key, ne.Key) ? 1f : 0f;
    var nwWeight = this._equality.Equals(se.Key, nw.Key) ? 1f : 0f;

    // Resolve diagonal ambiguities using Kopf-Lischinski 2011 §3.1's three heuristics:
    //   (1) Curves     — diagonal that's part of the longer curve wins (bounded traversal)
    //   (2) Sparse-pixels — diagonal of the rarer colour in an 8×8 window wins
    //   (3) Islands    — diagonal that prevents creating a valence-1 isolated pixel wins
    if (neWeight > 0.5f && nwWeight > 0.5f) {
      var curveLenNE = this._CurveLength(frame, sw.Key, ne.Key, x0, y0);
      var curveLenNW = this._CurveLength(frame, nw.Key, se.Key, x0, y0);

      var sparseNE = this._SparsePixels(frame, sw.Key, x0, y0);
      var sparseNW = this._SparsePixels(frame, nw.Key, x0, y0);

      var islandsNE = this._IslandsPenalty(frame, sw.Key, ne.Key, x0, y0);
      var islandsNW = this._IslandsPenalty(frame, nw.Key, se.Key, x0, y0);

      // Per paper, larger curve length / smaller sparse count / lower islands penalty
      // are all in favour. Combine into a single score; ties broken by favouring curves.
      var totalNE = curveLenNE * CurveWeight + (1f / Math.Max(1f, sparseNE)) * ValenceWeight + islandsNE;
      var totalNW = curveLenNW * CurveWeight + (1f / Math.Max(1f, sparseNW)) * ValenceWeight + islandsNW;

      if (totalNE > totalNW)
        nwWeight *= 0.2f;
      else
        neWeight *= 0.2f;
    }

    // Interpolate color based on edge connectivity and subpixel position
    var leftWeight = (1f - fx) * leftEdge * 0.5f;
    var rightWeight = fx * rightEdge * 0.5f;
    var topWeight = (1f - fy) * topEdge * 0.5f;
    var bottomWeight = fy * bottomEdge * 0.5f;
    var centerWeight = 1f - leftWeight - rightWeight - topWeight - bottomWeight;

    // Ensure non-negative weights
    if (centerWeight < 0f) {
      var scale = 1f / (1f - centerWeight);
      leftWeight *= scale;
      rightWeight *= scale;
      topWeight *= scale;
      bottomWeight *= scale;
      centerWeight = 0f;
    }

    // Accumulate weighted colors
    Accum4F<TWork> acc = default;
    acc.AddMul(c.Work, centerWeight);
    acc.AddMul(w.Work, leftWeight);
    acc.AddMul(e.Work, rightWeight);
    acc.AddMul(n.Work, topWeight);
    acc.AddMul(s.Work, bottomWeight);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Curves heuristic — counts the bounded path length along same-coloured pixels in the
  /// similarity graph through the diagonal endpoints. Per Kopf-Lischinski 2011 §3.1, the
  /// diagonal that's part of a longer curve is favoured (preserves elongated features).
  /// Bounded to <c>MaxTrace</c> steps in each direction to keep cost O(1) per pixel.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _CurveLength(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    in TKey endA, in TKey endB, int cx, int cy) {
    const int MaxTrace = 8;
    var length = 1f; // the diagonal itself counts as 1 segment
    // From sw / se direction, walk along same-colour neighbours.
    length += this._WalkSameColour(frame, endA, cx - 1, cy + 1, MaxTrace);
    length += this._WalkSameColour(frame, endB, cx + 1, cy - 1, MaxTrace);
    return length;
  }

  /// <summary>Count up to <paramref name="maxSteps"/> connected same-colour neighbours
  /// starting at (sx, sy). Greedy 8-direction walk; a step is taken to whichever same-
  /// colour neighbour hasn't been visited yet (deterministic by direction priority).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _WalkSameColour(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    in TKey colour, int sx, int sy, int maxSteps) {
    var count = 0f;
    var x = sx;
    var y = sy;
    var prevX = sx + 1; // arbitrary "came from" so we don't immediately reverse
    var prevY = sy + 1;
    for (var step = 0; step < maxSteps; ++step) {
      // Find next same-colour neighbour that isn't where we came from.
      var found = false;
      for (var dy = -1; dy <= 1 && !found; ++dy)
      for (var dx = -1; dx <= 1; ++dx) {
        if (dx == 0 && dy == 0) continue;
        var nx = x + dx;
        var ny = y + dy;
        if (nx == prevX && ny == prevY) continue;
        if (this._equality.Equals(colour, frame[nx, ny].Key)) {
          prevX = x; prevY = y;
          x = nx; y = ny;
          ++count;
          found = true;
          break;
        }
      }
      if (!found) break;
    }
    return count;
  }

  /// <summary>
  /// Sparse-pixels heuristic — counts pixels matching <paramref name="colour"/> within
  /// an 8×8 window around (cx, cy). Per paper, the diagonal of the colour with smaller
  /// component size (here approximated by raw count in a finite window) wins, preserving
  /// rare features against dense backgrounds.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _SparsePixels(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    in TKey colour, int cx, int cy) {
    var count = 0f;
    for (var dy = -3; dy <= 4; ++dy)
    for (var dx = -3; dx <= 4; ++dx) {
      if (this._equality.Equals(colour, frame[cx + dx, cy + dy].Key))
        count += 1f;
    }
    return count;
  }

  /// <summary>
  /// Islands heuristic — inverted valence proxy. If either diagonal endpoint has only one
  /// same-colour neighbour (the diagonal itself), removing the diagonal would create an
  /// isolated valence-1 pixel — we want to keep that diagonal. Returns a positive bonus
  /// if connecting this diagonal AVOIDS creating an island.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _IslandsPenalty(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    in TKey endA, in TKey endB, int cx, int cy) {
    var bonus = 0f;
    // Endpoint A is at (cx-1, cy+1). Count its same-colour cardinal neighbours OTHER
    // than the diagonal partner (cx+1, cy-1).
    var valenceA = 0;
    if (this._equality.Equals(endA, frame[cx - 1, cy].Key)) ++valenceA;     // up
    if (this._equality.Equals(endA, frame[cx, cy + 1].Key)) ++valenceA;     // right (back toward c)
    if (this._equality.Equals(endA, frame[cx - 2, cy + 1].Key)) ++valenceA; // left
    if (this._equality.Equals(endA, frame[cx - 1, cy + 2].Key)) ++valenceA; // down
    if (valenceA == 0) bonus += 1f;

    var valenceB = 0;
    if (this._equality.Equals(endB, frame[cx + 1, cy].Key)) ++valenceB;     // down (back)
    if (this._equality.Equals(endB, frame[cx, cy - 1].Key)) ++valenceB;     // left (back)
    if (this._equality.Equals(endB, frame[cx + 2, cy - 1].Key)) ++valenceB; // right
    if (this._equality.Equals(endB, frame[cx + 1, cy - 2].Key)) ++valenceB; // up
    if (valenceB == 0) bonus += 1f;

    return bonus;
  }
}

#endregion
