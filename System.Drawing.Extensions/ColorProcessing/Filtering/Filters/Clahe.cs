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
/// Contrast-Limited Adaptive Histogram Equalization (Karel Zuiderveld 1994).
/// Splits the image into a regular grid of <paramref name="tileSize"/>×<paramref name="tileSize"/>
/// tiles, computes a clipped per-tile histogram CDF, then bilinearly interpolates between
/// neighbouring tile CDFs to remap each pixel's luminance — preserving local contrast
/// while suppressing the over-amplification of noise that plain histogram equalization
/// produces in flat regions.
/// </summary>
/// <remarks>
/// <para>
/// Algorithm: (1) tile the image, (2) build per-tile 256-bin luminance histograms,
/// (3) clip every bin at <c>clipLimit · meanBinCount</c> redistributing the excess
/// uniformly, (4) integrate clipped histogram → 256-entry CDF per tile,
/// (5) for each pixel bilinearly blend the four nearest tile CDFs and look up its
/// luminance bin to get the new luminance, (6) reproject onto RGB by per-channel
/// scale.
/// </para>
/// <para>
/// Reference: Zuiderveld, "Contrast Limited Adaptive Histogram Equalization", in
/// <i>Graphics Gems IV</i>, 1994, pp. 474-485.
/// </para>
/// <para>
/// Use case: medical imaging, X-ray, low-contrast photo enhancement, photographic
/// tonemap fallback. Standard reference operator.
/// </para>
/// <para>Parameter ranges: <paramref name="tileSize"/> 4–64 (default 8),
/// <paramref name="clipLimit"/> 1–40 (default 4 — values &lt;1 disable clipping).</para>
/// </remarks>
[FilterInfo("Clahe",
  Author = "Karel Zuiderveld", Year = 1994,
  Url = "https://en.wikipedia.org/wiki/Adaptive_histogram_equalization#Contrast_Limited_AHE",
  Description = "Contrast-Limited Adaptive Histogram Equalization (Zuiderveld 1994)",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Clahe : IPixelFilter, IFrameFilter {
  private readonly int _tileSize;
  private readonly float _clipLimit;

  public Clahe() : this(8, 4f) { }

  public Clahe(int tileSize = 8, float clipLimit = 4f) {
    this._tileSize = Math.Max(4, Math.Min(64, tileSize));
    this._clipLimit = Math.Max(1f, Math.Min(40f, clipLimit));
  }

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
    => throw new NotSupportedException("Clahe requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new ClaheFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._tileSize, this._clipLimit, sourceWidth, sourceHeight));

  public static Clahe Default => new();
}

file readonly struct ClaheFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int tileSize, float clipLimit, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 0;
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
  /// Builds the CDF for the tile centred at (tileX,tileY) (in tile coordinates).
  /// Stores 256 entries into <paramref name="cdf"/>, normalised to [0..1].
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe void _BuildTileCdf(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int tileX, int tileY,
    int tilesX, int tilesY,
    int* histogram,
    int* cdf,
    float* outCdf) {
    // Tile pixel rectangle (clamped to image).
    var x0 = (int)((long)tileX * sourceWidth / tilesX);
    var x1 = (int)((long)(tileX + 1) * sourceWidth / tilesX);
    var y0 = (int)((long)tileY * sourceHeight / tilesY);
    var y1 = (int)((long)(tileY + 1) * sourceHeight / tilesY);
    if (x1 <= x0) x1 = x0 + 1;
    if (y1 <= y0) y1 = y0 + 1;

    for (var i = 0; i < 256; ++i)
      histogram[i] = 0;

    var totalPixels = 0;
    for (var y = y0; y < y1; ++y)
    for (var x = x0; x < x1; ++x) {
      var lum = _Lum(frame, x, y);
      var bin = (int)(lum * 255f);
      if (bin < 0) bin = 0;
      else if (bin > 255) bin = 255;
      ++histogram[bin];
      ++totalPixels;
    }
    if (totalPixels < 1) totalPixels = 1;

    // Clip & redistribute. clipLimit is the multiplier on the mean bin count.
    var meanBin = totalPixels / 256.0f;
    var clipCount = (int)(clipLimit * meanBin);
    if (clipCount < 1) clipCount = 1;

    var excess = 0;
    for (var i = 0; i < 256; ++i)
      if (histogram[i] > clipCount) {
        excess += histogram[i] - clipCount;
        histogram[i] = clipCount;
      }
    var redist = excess / 256;
    var residual = excess - redist * 256;
    for (var i = 0; i < 256; ++i)
      histogram[i] += redist;
    // Distribute residual one per bin starting from 0.
    for (var i = 0; i < residual && i < 256; ++i)
      ++histogram[i];

    // CDF.
    cdf[0] = histogram[0];
    for (var i = 1; i < 256; ++i)
      cdf[i] = cdf[i - 1] + histogram[i];

    var inv = cdf[255] > 0 ? 1f / cdf[255] : 0f;
    for (var i = 0; i < 256; ++i)
      outCdf[i] = cdf[i] * inv;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var tilesX = Math.Max(1, sourceWidth / tileSize);
    var tilesY = Math.Max(1, sourceHeight / tileSize);

    // Working buffers for histograms / CDFs (allocated outside loops; this method runs per pixel).
    var histogram = stackalloc int[256];
    var cdfInt = stackalloc int[256];
    var cdfA = stackalloc float[256];
    var cdfB = stackalloc float[256];
    var cdfC = stackalloc float[256];
    var cdfD = stackalloc float[256];

    // Compute floating tile coordinates of the pixel — places the pixel between four tile centres.
    var tx = (destX * tilesX / (float)sourceWidth) - 0.5f;
    var ty = (destY * tilesY / (float)sourceHeight) - 0.5f;
    var tx0 = (int)Math.Floor(tx);
    var ty0 = (int)Math.Floor(ty);
    var fx = tx - tx0;
    var fy = ty - ty0;
    var tx1 = tx0 + 1;
    var ty1 = ty0 + 1;

    // Clamp to valid tile range; this naturally handles edge replication.
    if (tx0 < 0) tx0 = 0;
    if (ty0 < 0) ty0 = 0;
    if (tx1 < 0) tx1 = 0;
    if (ty1 < 0) ty1 = 0;
    if (tx0 >= tilesX) tx0 = tilesX - 1;
    if (ty0 >= tilesY) ty0 = tilesY - 1;
    if (tx1 >= tilesX) tx1 = tilesX - 1;
    if (ty1 >= tilesY) ty1 = tilesY - 1;

    this._BuildTileCdf(frame, tx0, ty0, tilesX, tilesY, histogram, cdfInt, cdfA);
    this._BuildTileCdf(frame, tx1, ty0, tilesX, tilesY, histogram, cdfInt, cdfB);
    this._BuildTileCdf(frame, tx0, ty1, tilesX, tilesY, histogram, cdfInt, cdfC);
    this._BuildTileCdf(frame, tx1, ty1, tilesX, tilesY, histogram, cdfInt, cdfD);

    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var lum = ColorConverter.LuminanceFromRgb(cr, cg, cb);
    var bin = (int)(lum * 255f);
    if (bin < 0) bin = 0;
    else if (bin > 255) bin = 255;

    var a = cdfA[bin];
    var b = cdfB[bin];
    var c = cdfC[bin];
    var d = cdfD[bin];

    var top = a * (1f - fx) + b * fx;
    var bot = c * (1f - fx) + d * fx;
    var newLum = top * (1f - fy) + bot * fy;

    var scale = lum > 1e-6f ? newLum / lum : 0f;
    var or = Math.Min(1f, Math.Max(0f, cr * scale));
    var og = Math.Min(1f, Math.Max(0f, cg * scale));
    var ob = Math.Min(1f, Math.Max(0f, cb * scale));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
