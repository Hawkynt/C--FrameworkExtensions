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
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Stained glass effect — Voronoi tessellation with dark lead borders between cells.
/// Similar to Crystallize but adds dark borders where cell boundaries meet.
/// </summary>
[FilterInfo("StainedGlass",
  Description = "Stained glass Voronoi effect with dark lead borders", Category = FilterCategory.Artistic)]
public readonly struct StainedGlass(int cellSize, int borderWidth = 1, int seed = 0) : IPixelFilter, IFrameFilter {
  private readonly int _cellSize = Math.Max(2, cellSize);
  private readonly int _borderWidth = Math.Max(0, borderWidth);

  public StainedGlass() : this(10, 1, 0) { }

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
    => callback.Invoke(new StainedGlassPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new StainedGlassFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._cellSize, this._borderWidth, seed, sourceWidth, sourceHeight));

  public static StainedGlass Default => new();
}

file readonly struct StainedGlassPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct StainedGlassFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int cellSize, int borderWidth, int seed, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => cellSize;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Hash(int x, int y, int s) {
    var h = (uint)(x * 374761393 + y * 668265263 + s * 1274126177);
    h = (h ^ (h >> 13)) * 1274126177;
    h ^= h >> 16;
    return (int)(h & 0x7FFFFFFF);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var cellX = destX / cellSize;
    var cellY = destY / cellSize;

    var bestDist = float.MaxValue;
    var secondDist = float.MaxValue;
    var bestSx = destX;
    var bestSy = destY;

    for (var cy = cellY - 1; cy <= cellY + 1; ++cy)
    for (var cx = cellX - 1; cx <= cellX + 1; ++cx) {
      var baseSx = cx * cellSize + cellSize / 2;
      var baseSy = cy * cellSize + cellSize / 2;
      var jx = _Hash(cx, cy, seed) % cellSize - cellSize / 2;
      var jy = _Hash(cx, cy, seed + 1) % cellSize - cellSize / 2;
      var sx = baseSx + jx;
      var sy = baseSy + jy;
      var dx = (float)(destX - sx);
      var dy = (float)(destY - sy);
      var dist = dx * dx + dy * dy;
      if (dist < bestDist) {
        secondDist = bestDist;
        bestDist = dist;
        bestSx = sx;
        bestSy = sy;
      } else if (dist < secondDist)
        secondDist = dist;
    }

    var d1 = (float)Math.Sqrt(bestDist);
    var d2 = (float)Math.Sqrt(secondDist);

    float or, og, ob;
    if (d2 - d1 < borderWidth) {
      or = 0.1f;
      og = 0.1f;
      ob = 0.1f;
    } else {
      var px = frame[bestSx, bestSy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      or = r;
      og = g;
      ob = b;
    }

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
