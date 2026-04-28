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

namespace Hawkynt.ColorProcessing.Filtering.Filters.ToneMap;

/// <summary>
/// Mantiuk perceptual tone mapping (Mantiuk, Daly &amp; Kerofsky 2008) — simplified
/// gradient-domain reproduction. The full operator solves a Poisson equation on
/// log-luminance gradients constrained to remain &lt; perceptual visibility threshold;
/// this implementation applies the contrast-equalization transfer function pixelwise
/// to log-luminance and rebuilds RGB via per-pixel saturation transfer.
/// </summary>
/// <remarks>
/// <para>
/// Curve in log-luminance: <c>L_d = (log(L) − log(L_min)) / (log(L_max) − log(L_min))</c>
/// raised to <paramref name="contrast"/> and saturation-corrected via per-channel
/// power: <c>C_d = (C/L)^saturation · L_d</c>. Approximates the look of full-blown
/// Mantiuk pipelines at a fraction of the cost.
/// </para>
/// <para>
/// Use case: HDR → LDR with perceptually-balanced midtones; produces slightly
/// flatter results than Reinhard but with better preservation of fine detail
/// across the dynamic range.
/// </para>
/// <para>Parameter ranges: <paramref name="contrast"/> 0.1–1 (default 0.5),
/// <paramref name="saturation"/> 0–2 (default 1).</para>
/// </remarks>
[FilterInfo("Mantiuk",
  Author = "Mantiuk, Daly & Kerofsky", Year = 2008,
  Url = "https://resources.mpi-inf.mpg.de/hdr/tmo/mantiuk08sap.pdf",
  Description = "Mantiuk 2008 perceptual tone mapping (simplified)",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Mantiuk : IPixelFilter, IFrameFilter {
  private readonly float _contrast;
  private readonly float _saturation;

  public Mantiuk() : this(0.5f, 1f) { }

  public Mantiuk(float contrast = 0.5f, float saturation = 1f) {
    this._contrast = Math.Max(0.1f, Math.Min(1f, contrast));
    this._saturation = Math.Max(0f, Math.Min(2f, saturation));
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
    => callback.Invoke(new MantiukPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new MantiukFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._contrast, this._saturation, sourceWidth, sourceHeight));

  public static Mantiuk Default => new();
}

file readonly struct MantiukPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct MantiukFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float contrast, float saturation, int sourceWidth, int sourceHeight)
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
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    // Subsample frame to find global log-luminance min/max bounds.
    var minLogLum = float.MaxValue;
    var maxLogLum = float.MinValue;
    var samples = Math.Min(64, sourceHeight);
    var sx = Math.Min(64, sourceWidth);
    var stepY = Math.Max(1, sourceHeight / samples);
    var stepX = Math.Max(1, sourceWidth / sx);
    for (var y = 0; y < sourceHeight; y += stepY)
    for (var x = 0; x < sourceWidth; x += stepX) {
      var l = _Lum(frame, x, y);
      var sampleLogL = (float)Math.Log(Math.Max(1e-4f, l));
      if (sampleLogL < minLogLum) minLogLum = sampleLogL;
      if (sampleLogL > maxLogLum) maxLogLum = sampleLogL;
    }

    var range = Math.Max(1e-3f, maxLogLum - minLogLum);

    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var lum = ColorMatrices.BT601_R * cr + ColorMatrices.BT601_G * cg + ColorMatrices.BT601_B * cb;

    if (lum < 1e-6f) {
      dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(0f, 0f, 0f, ca));
      return;
    }

    var logL = (float)Math.Log(lum);
    var normalized = (logL - minLogLum) / range;
    var ld = (float)Math.Pow(Math.Max(0f, Math.Min(1f, normalized)), 1f / Math.Max(0.1f, contrast));

    // Per-channel power transfer for saturation control.
    var or = (float)Math.Pow(Math.Max(0f, cr / lum), saturation) * ld;
    var og = (float)Math.Pow(Math.Max(0f, cg / lum), saturation) * ld;
    var ob = (float)Math.Pow(Math.Max(0f, cb / lum), saturation) * ld;

    or = or < 0f ? 0f : (or > 1f ? 1f : or);
    og = og < 0f ? 0f : (og > 1f ? 1f : og);
    ob = ob < 0f ? 0f : (ob > 1f ? 1f : ob);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
