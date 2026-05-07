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
/// Crystallize — Voronoi-tessellation stylisation (Photoshop-style "Crystallize").
/// </summary>
/// <remarks>
/// <para>Generates a grid of jittered seed points and assigns each output pixel the
/// colour of its NEAREST seed (Voronoi region). Variant of the Adobe Photoshop
/// "Filter → Pixelate → Crystallize" effect. Reference: G. Voronoi,
/// "Nouvelles applications des paramètres continus à la théorie des formes
/// quadratiques", J. reine angew. Math. 133:97-178, 1908; image-stylisation
/// usage popularised by Photoshop manuals.</para>
/// </remarks>
[FilterInfo("Crystallize",
  Description = "Grid-based Voronoi crystallization effect", Category = FilterCategory.Artistic)]
public readonly struct Crystallize(int cellSize, int seed = 0) : IPixelFilter, IFrameFilter {
  private readonly int _cellSize = Math.Max(2, cellSize);

  public Crystallize() : this(10, 0) { }

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
    => throw new NotSupportedException("Crystallize requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new CrystallizeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._cellSize, seed, sourceWidth, sourceHeight));

  public static Crystallize Default => new();
}

file readonly struct CrystallizeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int cellSize, int seed, int sourceWidth, int sourceHeight)
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
        bestDist = dist;
        bestSx = sx;
        bestSy = sy;
      }
    }

    var px = frame[bestSx, bestSy].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
