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
/// Local histogram equalisation — adaptive contrast enhancement.
/// </summary>
/// <remarks>
/// <para>For each pixel, builds the luminance histogram within a square neighbourhood
/// and remaps the centre pixel via the local cumulative distribution function:</para>
/// <code>
///   I'(p) = (cdf(I(p)) − cdf_min) / (N − cdf_min) · 255
/// </code>
/// <para>Stretches the local tonal range so flat-but-dark regions reveal hidden detail
/// at the cost of amplifying noise. For an artefact-limited variant see
/// <see cref="Clahe"/> (Contrast-Limited Adaptive Histogram Equalisation).</para>
/// <para>Reference: R. C. Gonzalez &amp; R. E. Woods, "Digital Image Processing"
/// (4th ed., Pearson 2018), §3.3.1 (global) / §3.3.3 (local). The cdf_min term
/// is the standard correction to ensure output range starts at 0.</para>
/// </remarks>
[FilterInfo("Equalize",
  Description = "Local histogram equalization", Category = FilterCategory.ColorCorrection)]
public readonly struct Equalize(int radius = 10) : IPixelFilter, IFrameFilter {
  private readonly int _radius = Math.Max(1, radius);

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
    => throw new NotSupportedException("Equalize requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new EqualizeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, sourceWidth, sourceHeight));

  public static Equalize Default => new(10);
}

file readonly struct EqualizeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int radius, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => radius;
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    // Build luminance histogram (256 bins)
    var histogram = stackalloc int[256];
    for (var i = 0; i < 256; ++i)
      histogram[i] = 0;

    var totalPixels = 0;

    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var lum = _Lum(frame, destX + dx, destY + dy);
      var bin = (int)(lum * 255f);
      bin = Math.Max(0, Math.Min(255, bin));
      ++histogram[bin];
      ++totalPixels;
    }

    // Build CDF; cdfMin is the smallest non-zero CDF value, needed for the textbook
    // mapping T(v) = (cdf(v) − cdf_min) / (N − cdf_min) so that the minimum input
    // luminance maps to 0 instead of cdf_min/N (Gonzalez & Woods §3.3.1).
    var cumulativeCounts = stackalloc int[256];
    cumulativeCounts[0] = histogram[0];
    for (var i = 1; i < 256; ++i)
      cumulativeCounts[i] = cumulativeCounts[i - 1] + histogram[i];
    var cdfMin = 0;
    for (var i = 0; i < 256; ++i)
      if (cumulativeCounts[i] > 0) { cdfMin = cumulativeCounts[i]; break; }

    // Map center pixel
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var centerLum = ColorConverter.LuminanceFromRgb(cr, cg, cb);
    var centerBin = Math.Max(0, Math.Min(255, (int)(centerLum * 255f)));
    var denom = totalPixels - cdfMin;
    var cdf = denom > 0
      ? Math.Max(0f, cumulativeCounts[centerBin] - cdfMin) / (float)denom
      : cumulativeCounts[centerBin] / (float)totalPixels;

    var scale = centerLum > 0.001f ? cdf / centerLum : cdf;
    var outR = Math.Min(1f, cr * scale);
    var outG = Math.Min(1f, cg * scale);
    var outB = Math.Min(1f, cb * scale);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, ca));
  }
}
