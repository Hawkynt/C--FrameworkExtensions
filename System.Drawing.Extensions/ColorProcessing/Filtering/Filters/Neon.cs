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
/// Neon glow effect with per-channel Sobel edge detection and glow spread.
/// Computes edge magnitude for each RGB channel separately, multiplies by strength,
/// and averages nearby edge values to create a soft glow around edges.
/// Always uses frame-level random access due to configurable glow size.
/// </summary>
[FilterInfo("Neon",
  Description = "Neon glow effect with per-channel edge detection and spread", Category = FilterCategory.Artistic)]
public readonly struct Neon(float strength, float glowSize = 2f) : IPixelFilter, IFrameFilter {
  private readonly float _strength = Math.Max(0f, strength);
  private readonly float _glowSize = Math.Max(1f, glowSize);

  public Neon() : this(1f, 2f) { }

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
    => callback.Invoke(new NeonPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new NeonFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strength, this._glowSize, sourceWidth, sourceHeight));

  public static Neon Default => new();
}

file readonly struct NeonPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct NeonFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float strength, float glowSize, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => (int)Math.Ceiling(glowSize) + 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float r, float g, float b) _GetRgb(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return (r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float er, float eg, float eb) _SobelAt(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int x, int y) {
    var (tlr, tlg, tlb) = _GetRgb(frame[x - 1, y - 1]);
    var (tr, tg, tb) = _GetRgb(frame[x, y - 1]);
    var (trr, trg, trb) = _GetRgb(frame[x + 1, y - 1]);
    var (lr, lg, lb) = _GetRgb(frame[x - 1, y]);
    var (rr, rg, rb) = _GetRgb(frame[x + 1, y]);
    var (blr, blg, blb) = _GetRgb(frame[x - 1, y + 1]);
    var (br, bg, bb) = _GetRgb(frame[x, y + 1]);
    var (brr, brg, brb) = _GetRgb(frame[x + 1, y + 1]);

    var gxr = -tlr + trr - 2f * lr + 2f * rr - blr + brr;
    var gyr = -tlr - 2f * tr - trr + blr + 2f * br + brr;
    var er = (float)Math.Sqrt(gxr * gxr + gyr * gyr);

    var gxg = -tlg + trg - 2f * lg + 2f * rg - blg + brg;
    var gyg = -tlg - 2f * tg - trg + blg + 2f * bg + brg;
    var eg = (float)Math.Sqrt(gxg * gxg + gyg * gyg);

    var gxb = -tlb + trb - 2f * lb + 2f * rb - blb + brb;
    var gyb = -tlb - 2f * tb - trb + blb + 2f * bb + brb;
    var eb = (float)Math.Sqrt(gxb * gxb + gyb * gyb);

    return (er, eg, eb);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);

    // Accumulate edge values from nearby pixels for glow spread
    var glowRadius = (int)Math.Ceiling(glowSize);
    var sumR = 0f;
    var sumG = 0f;
    var sumB = 0f;
    var count = 0;

    for (var dy = -glowRadius; dy <= glowRadius; ++dy)
    for (var dx = -glowRadius; dx <= glowRadius; ++dx) {
      var dist = (float)Math.Sqrt(dx * dx + dy * dy);
      if (dist > glowSize)
        continue;

      var (er, eg, eb) = _SobelAt(frame, destX + dx, destY + dy);
      var weight = 1f - dist / glowSize;
      sumR += er * weight;
      sumG += eg * weight;
      sumB += eb * weight;
      ++count;
    }

    if (count > 0) {
      var inv = 1f / count;
      sumR *= inv;
      sumG *= inv;
      sumB *= inv;
    }

    var or = Math.Min(1f, sumR * strength);
    var og = Math.Min(1f, sumG * strength);
    var ob = Math.Min(1f, sumB * strength);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
