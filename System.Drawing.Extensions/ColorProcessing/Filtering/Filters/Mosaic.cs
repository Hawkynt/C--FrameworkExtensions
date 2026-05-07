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
/// Mosaic tile effect — like Pixelate but with dark grout lines between tiles.
/// Divides the image into uniform rectangular tiles separated by visible grout.
/// </summary>
[FilterInfo("Mosaic",
  Description = "Mosaic tile effect with dark grout lines between tiles", Category = FilterCategory.Artistic)]
public readonly struct Mosaic(int tileSize, int groutWidth = 2) : IPixelFilter, IFrameFilter {
  private readonly int _tileSize = Math.Max(2, tileSize);
  // Clamp grout to (tileSize-1)/2 — the grout test in MosaicFrameKernel.Resample
  // marks a pixel as grout when localX < groutWidth OR localX >= tileSize-groutWidth
  // (and same for Y). If 2*groutWidth >= tileSize the two grout bands meet/overlap
  // and every pixel is grout, producing an all-grey image (e.g. Mosaic(2,2) → fully
  // dark). Clamping to (tileSize-1)/2 guarantees at least one row/column of tile
  // pixels exists between grout bands for any tileSize >= 2.
  private readonly int _groutWidth = Math.Min(Math.Max(0, groutWidth), (Math.Max(2, tileSize) - 1) / 2);

  public Mosaic() : this(15, 2) { }

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
    => throw new NotSupportedException("Mosaic requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new MosaicFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._tileSize, this._groutWidth, sourceWidth, sourceHeight));

  public static Mosaic Default => new();
}

file readonly struct MosaicFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int tileSize, int groutWidth, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => tileSize;
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
    var localX = destX % tileSize;
    var localY = destY % tileSize;

    if (localX < groutWidth || localY < groutWidth ||
        localX >= tileSize - groutWidth || localY >= tileSize - groutWidth) {
      var center = frame[destX, destY].Work;
      var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
      dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(0.15f, 0.15f, 0.15f, ca));
      return;
    }

    var blockX = destX - localX;
    var blockY = destY - localY;
    var endX = Math.Min(blockX + tileSize, sourceWidth);
    var endY = Math.Min(blockY + tileSize, sourceHeight);
    var count = (endX - blockX) * (endY - blockY);

    float ar = 0, ag = 0, ab = 0, aa = 0;
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
    ar *= inv;
    ag *= inv;
    ab *= inv;
    aa *= inv;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(ar, ag, ab, aa));
  }
}
