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
/// Local auto-levels contrast stretching — Photoshop-style per-channel histogram stretch.
/// </summary>
/// <remarks>
/// <para>For each output pixel, finds per-channel min/max in the local neighbourhood and
/// stretches each channel independently to fill <c>[0, 1]</c>:
/// <c>out_C = (C − minC) / (maxC − minC)</c>. The per-channel formulation matches Adobe's
/// AutoLevels and is idempotent in the limit: applying AutoLevels twice converges to the
/// fixed point of "all channel histograms locally fill [0, 1]".</para>
/// <para>Note: per-channel stretching can shift colours (e.g. a sky-blue cast in shadows
/// gets pushed toward neutral). For luminance-preserving local contrast that preserves
/// hue, use Equalize or CLAHE instead.</para>
/// <para>Always uses frame-level random access.</para>
/// </remarks>
[FilterInfo("AutoLevels",
  Description = "Local auto-levels per-channel histogram stretch", Category = FilterCategory.ColorCorrection)]
public readonly struct AutoLevels(int radius = 10) : IPixelFilter, IFrameFilter {
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
    => throw new NotSupportedException("AutoLevels requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new AutoLevelsFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, sourceWidth, sourceHeight));

  public static AutoLevels Default => new(10);
}

file readonly struct AutoLevelsFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
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
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    // Per-channel histogram stretch over the local window. Matches Photoshop AutoLevels
    // and is idempotent in the limit (fixed point: all channel histograms locally fill
    // [0, 1]).
    var minR = float.MaxValue; var maxR = float.MinValue;
    var minG = float.MaxValue; var maxG = float.MinValue;
    var minB = float.MaxValue; var maxB = float.MinValue;

    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      if (r < minR) minR = r;
      if (r > maxR) maxR = r;
      if (g < minG) minG = g;
      if (g > maxG) maxG = g;
      if (b < minB) minB = b;
      if (b > maxB) maxB = b;
    }

    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    var rangeR = maxR - minR;
    var rangeG = maxG - minG;
    var rangeB = maxB - minB;

    var outR = rangeR > 0.001f ? (cr - minR) / rangeR : cr;
    var outG = rangeG > 0.001f ? (cg - minG) / rangeG : cg;
    var outB = rangeB > 0.001f ? (cb - minB) / rangeB : cb;

    if (outR < 0f) outR = 0f; else if (outR > 1f) outR = 1f;
    if (outG < 0f) outG = 0f; else if (outG > 1f) outG = 1f;
    if (outB < 0f) outB = 0f; else if (outB > 1f) outB = 1f;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, ca));
  }
}
