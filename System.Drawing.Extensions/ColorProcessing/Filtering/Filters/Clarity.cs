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
/// Local contrast enhancement targeting midtones (like Lightroom's Clarity slider).
/// Computes a local mean over a configurable radius and applies a midtone-weighted
/// unsharp mask to boost local contrast without affecting highlights and shadows.
/// </summary>
[FilterInfo("Clarity",
  Description = "Local contrast enhancement targeting midtones", Category = FilterCategory.Enhancement)]
public readonly struct Clarity(float amount = 0.5f, int radius = 3) : IPixelFilter, IFrameFilter {
  private readonly float _amount = Math.Max(0f, Math.Min(1f, amount));
  private readonly int _radius = Math.Max(1, radius);

  public Clarity() : this(0.5f, 3) { }

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
    => callback.Invoke(new ClarityPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new ClarityFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._amount, this._radius, sourceWidth, sourceHeight));

  public static Clarity Default => new();
}

file readonly struct ClarityPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct ClarityFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float amount, int radius, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, radius);
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
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    var lum = ColorMatrices.BT601_R * cr + ColorMatrices.BT601_G * cg + ColorMatrices.BT601_B * cb;
    var midWeight = 1f - 2f * Math.Abs(lum - 0.5f);

    float sr = 0, sg = 0, sb = 0;
    var count = 0;
    for (var dy = -radius; dy <= radius; ++dy)
      for (var dx = -radius; dx <= radius; ++dx) {
        var px = frame[destX + dx, destY + dy].Work;
        var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
        sr += r;
        sg += g;
        sb += b;
        ++count;
      }

    var invCount = 1f / count;
    var mr = sr * invCount;
    var mg = sg * invCount;
    var mb = sb * invCount;

    var scale = amount * midWeight;
    var or = Math.Max(0f, Math.Min(1f, cr + scale * (cr - mr)));
    var og = Math.Max(0f, Math.Min(1f, cg + scale * (cg - mg)));
    var ob = Math.Max(0f, Math.Min(1f, cb + scale * (cb - mb)));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
