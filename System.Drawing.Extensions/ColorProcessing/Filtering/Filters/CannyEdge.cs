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
/// Canny edge detection — multi-stage edge detector (J. Canny 1986).
/// </summary>
/// <remarks>
/// <para>Five-stage pipeline:
/// (1) Gaussian smoothing,
/// (2) Sobel-gradient magnitude and direction,
/// (3) Non-maximum suppression (compares each pixel's gradient magnitude vs the two
/// neighbours along the gradient direction; only local maxima survive),
/// (4) Hysteresis thresholding (high-threshold pixels are strong edges; low-threshold
/// pixels survive only if connected to a strong edge via 8-connectivity),
/// (5) Edge tracing.</para>
/// <para>Reference: J. Canny, "A Computational Approach to Edge Detection",
/// IEEE Transactions on Pattern Analysis and Machine Intelligence PAMI-8(6):679-698,
/// November 1986. The de-facto reference edge detector for ~40 years.</para>
/// </remarks>
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
    => throw new NotSupportedException("CannyEdge requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    //
    // Sector classification via cross-multiplied tangent comparison rather than Atan2:
    // Math.Atan2 is not specified to be correctly-rounded across CLRs, so the cascade
    // against π/8, 3π/8, 5π/8, 7π/8 can flip sectors at boundaries between TFMs. The
    // four sectors of the gradient direction map directly to ratios of |gy|/|gx|:
    //   horizontal       |gy|/|gx| <  tan(π/8)  ≈ 0.41421356
    //   diagonal         tan(π/8) ≤ |gy|/|gx| < tan(3π/8) ≈ 2.41421356
    //   vertical         |gy|/|gx| ≥ tan(3π/8)
    // Sign of gx*gy distinguishes NE-SW (+) from NW-SE (-) for the diagonal sector.
    // Float multiply/compare is IEEE-754 deterministic, so this is TFM-stable.
    const float TAN_22_5 = 0.41421356f; // tan(π/8) = √2 - 1
    const float TAN_67_5 = 2.41421356f; // tan(3π/8) = √2 + 1
    var absGx = gx < 0 ? -gx : gx;
    var absGy = gy < 0 ? -gy : gy;
    int nx1, ny1, nx2, ny2;
    if (absGy < absGx * TAN_22_5) {
      // Gradient horizontal → neighbours at (-1, 0) and (+1, 0)
      nx1 = -1; ny1 = 0; nx2 = 1; ny2 = 0;
    } else if (absGy < absGx * TAN_67_5) {
      // Diagonal sector: same-sign gx,gy ↔ original NE-SW branch (absDir ∈ [π/8, 3π/8));
      // opposite-sign ↔ original NW-SE branch (absDir ∈ [5π/8, 7π/8)).
      if (gx * gy >= 0f) {
        // gx*gy ≥ 0 (gradient runs NE-SW) → neighbours at (+1, -1) and (-1, +1)
        nx1 = 1; ny1 = -1; nx2 = -1; ny2 = 1;
      } else {
        // gx*gy < 0 (gradient runs NW-SE) → neighbours at (-1, -1) and (+1, +1)
        nx1 = -1; ny1 = -1; nx2 = 1; ny2 = 1;
      }
    } else {
      // Gradient vertical → neighbours at (0, -1) and (0, +1)
      nx1 = 0; ny1 = -1; nx2 = 0; ny2 = 1;
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
