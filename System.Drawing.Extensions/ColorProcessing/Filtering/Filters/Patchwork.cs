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
/// Patchwork quilt effect — divides the image into square blocks, averages
/// their color, and adds emboss-style relief at block edges for a quilted appearance.
/// </summary>
[FilterInfo("Patchwork",
  Description = "Patchwork quilt effect with relief edges", Category = FilterCategory.Artistic)]
public readonly struct Patchwork(int squareSize = 5, float relief = 0.5f) : IPixelFilter, IFrameFilter {

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
    => callback.Invoke(new PatchworkPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new PatchworkFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      squareSize, relief, sourceWidth, sourceHeight));

  public static Patchwork Default => new(5, 0.5f);
}

file readonly struct PatchworkPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct PatchworkFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int squareSize, float relief, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, squareSize);
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var sz = Math.Max(1, squareSize);
    var blockX = destX - destX % sz;
    var blockY = destY - destY % sz;
    var endX = Math.Min(blockX + sz, sourceWidth);
    var endY = Math.Min(blockY + sz, sourceHeight);
    var count = (endX - blockX) * (endY - blockY);

    float ar = 0f, ag = 0f, ab = 0f, aa = 0f;
    for (var sy = blockY; sy < endY; ++sy)
    for (var sx = blockX; sx < endX; ++sx) {
      var px = frame[sx, sy].Work;
      var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
      ar += r;
      ag += g;
      ab += b;
      aa += a;
    }

    var inv = 1f / count;
    var avgR = ar * inv;
    var avgG = ag * inv;
    var avgB = ab * inv;
    var avgA = aa * inv;

    var edgeX = destX % sz;
    var edgeY = destY % sz;
    var isEdge = edgeX == 0 || edgeY == 0;
    var isHighlight = edgeX == sz - 1 || edgeY == sz - 1;

    float outR, outG, outB;
    if (isEdge) {
      var factor = 1f - relief * 0.5f;
      outR = avgR * factor;
      outG = avgG * factor;
      outB = avgB * factor;
    } else if (isHighlight) {
      outR = Math.Min(1f, avgR * (1f + relief * 0.3f));
      outG = Math.Min(1f, avgG * (1f + relief * 0.3f));
      outB = Math.Min(1f, avgB * (1f + relief * 0.3f));
    } else {
      outR = avgR;
      outG = avgG;
      outB = avgB;
    }

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, avgA));
  }
}
