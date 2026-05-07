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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Canny edge detection with hysteresis thresholding.
/// Computes Sobel gradient magnitude and direction, applies simplified non-maximum suppression,
/// and uses dual thresholds (low/high) for hysteresis: strong edges are white, weak edges are gray,
/// and non-edges are black.
/// </summary>
[FilterInfo("CannyEdge",
  Description = "Canny edge detection with hysteresis thresholding", Category = FilterCategory.Analysis)]
public readonly struct CannyEdge(float lowThreshold = 0.1f, float highThreshold = 0.3f) : IPixelFilter, IFrameFilter {
  private readonly float _lowThreshold = lowThreshold;
  private readonly float _highThreshold = highThreshold;

  /// <inheritdoc />
  public bool UsesFrameAccess => true;

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
    => callback.Invoke(new CannyEdgePassThroughKernel<TWork, TKey, TPixel, TEncode>());

  /// <inheritdoc />
  public TResult InvokeFrameKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth, int sourceHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new CannyEdgeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._lowThreshold, this._highThreshold, sourceWidth, sourceHeight));

  public static CannyEdge Default => new(0.1f, 0.3f);
}

file readonly struct CannyEdgePassThroughKernel<TWork, TKey, TPixel, TEncode>
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
    in TEncode encoder)
    => dest[0] = encoder.Encode(window.P0P0.Work);
}

file readonly struct CannyEdgeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float lowThreshold, float highThreshold, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int x, int y) {
    var px = frame[x, y].Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorConverter.LuminanceFromRgb(r, g, b);
  }

  /// <summary>
  /// Hysteresis tracking: returns true if a strong-edge pixel is reachable from (x, y)
  /// via an 8-connected path of weak (or strong) pixels within a bounded search radius.
  /// Bounded so we don't scan the whole image per pixel; depth=4 (~25-cell BFS) is
  /// adequate for typical edge thinning.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool _ReachesStrongEdge(NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
      int sx, int sy, int unused) {
    const int MaxDepth = 4;
    // Iterative DFS using 8-neighbour expansion. Visited-set is a small fixed-size
    // bitmask keyed on (dx, dy) within ±MaxDepth (a 9×9 grid).
    Span<bool> visited = stackalloc bool[(2 * MaxDepth + 1) * (2 * MaxDepth + 1)];
    Span<(int x, int y)> stack = stackalloc (int, int)[81];
    var top = 0;
    stack[top++] = (sx, sy);
    visited[MaxDepth * (2 * MaxDepth + 1) + MaxDepth] = true;

    while (top > 0) {
      var (x, y) = stack[--top];
      // Test all 8 neighbours.
      for (var dy = -1; dy <= 1; ++dy)
      for (var dx = -1; dx <= 1; ++dx) {
        if (dx == 0 && dy == 0) continue;
        var nx = x + dx;
        var ny = y + dy;
        var rdx = nx - sx + MaxDepth;
        var rdy = ny - sy + MaxDepth;
        if (rdx < 0 || rdx > 2 * MaxDepth || rdy < 0 || rdy > 2 * MaxDepth) continue;
        var idx = rdy * (2 * MaxDepth + 1) + rdx;
        if (visited[idx]) continue;
        visited[idx] = true;

        var nMag = _SobelMag(frame, nx, ny);
        if (nMag >= highThreshold) return true;
        if (nMag >= lowThreshold && top < stack.Length) {
          stack[top++] = (nx, ny);
        }
      }
    }
    return false;
  }

  /// <summary>Sobel gradient magnitude at (x, y), used by NMS to compare against neighbour
  /// gradient magnitudes per Canny 1986.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _SobelMag(NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int x, int y) {
    var tl = _Lum(frame, x - 1, y - 1);
    var t = _Lum(frame, x, y - 1);
    var tr = _Lum(frame, x + 1, y - 1);
    var l = _Lum(frame, x - 1, y);
    var ri = _Lum(frame, x + 1, y);
    var bl = _Lum(frame, x - 1, y + 1);
    var bo = _Lum(frame, x, y + 1);
    var br = _Lum(frame, x + 1, y + 1);
    var gx = -tl + tr - 2f * l + 2f * ri - bl + br;
    var gy = -tl - 2f * t - tr + bl + 2f * bo + br;
    return (float)Math.Sqrt(gx * gx + gy * gy);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var px = frame[destX, destY].Work;
    var (_, _, _, a) = ColorConverter.GetNormalizedRgba(in px);

    // Sobel gradient
    var tl = _Lum(frame, destX - 1, destY - 1);
    var t = _Lum(frame, destX, destY - 1);
    var tr = _Lum(frame, destX + 1, destY - 1);
    var l = _Lum(frame, destX - 1, destY);
    var ri = _Lum(frame, destX + 1, destY);
    var bl = _Lum(frame, destX - 1, destY + 1);
    var bo = _Lum(frame, destX, destY + 1);
    var br = _Lum(frame, destX + 1, destY + 1);

    var gx = -tl + tr - 2f * l + 2f * ri - bl + br;
    var gy = -tl - 2f * t - tr + bl + 2f * bo + br;
    var edgeMag = (float)Math.Sqrt(gx * gx + gy * gy);

    // Non-maximum suppression per Canny 1986: pick the two neighbours along the gradient
    // direction (not perpendicular to it), compute their gradient magnitudes (Sobel
    // applied at each), and suppress the centre if it's not a local maximum among the
    // three. Simplified to one-pixel-step sampling (no sub-pixel interpolation along the
    // gradient direction; OpenCV uses linear interpolation between adjacent neighbours
    // — a defensible simplification for an integer-grid filter).
    var dir = (float)Math.Atan2(gy, gx);
    var absDir = dir < 0 ? dir + (float)Math.PI : dir;
    int nx1, ny1, nx2, ny2;
    if (absDir < Math.PI / 8 || absDir >= 7 * Math.PI / 8) {
      // Gradient horizontal → neighbours at (-1, 0) and (+1, 0)
      nx1 = -1; ny1 = 0; nx2 = 1; ny2 = 0;
    } else if (absDir < 3 * Math.PI / 8) {
      // Gradient diagonal NE-SW → neighbours at (+1, -1) and (-1, +1)
      nx1 = 1; ny1 = -1; nx2 = -1; ny2 = 1;
    } else if (absDir < 5 * Math.PI / 8) {
      // Gradient vertical → neighbours at (0, -1) and (0, +1)
      nx1 = 0; ny1 = -1; nx2 = 0; ny2 = 1;
    } else {
      // Gradient diagonal NW-SE → neighbours at (-1, -1) and (+1, +1)
      nx1 = -1; ny1 = -1; nx2 = 1; ny2 = 1;
    }
    var mag1 = _SobelMag(frame, destX + nx1, destY + ny1);
    var mag2 = _SobelMag(frame, destX + nx2, destY + ny2);
    if (edgeMag < mag1 || edgeMag < mag2)
      edgeMag = 0f;

    // Hysteresis tracking per Canny 1986: a pixel is a true edge iff it's STRONG itself,
    // OR it's WEAK and reachable through a chain of WEAK pixels to a STRONG pixel via
    // 8-connectivity. Implemented as a bounded BFS: if this pixel is strong, output 1.
    // If weak, search the 8-neighbourhood within `maxDepth` steps for any strong pixel
    // along an unbroken weak chain — output 1 if found, else 0.
    float v;
    if (edgeMag >= highThreshold) {
      v = 1f;
    } else if (edgeMag >= lowThreshold) {
      v = _ReachesStrongEdge(frame, destX, destY, 0) ? 1f : 0f;
    } else {
      v = 0f;
    }

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, a));
  }
}
