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
/// 3x3 emboss filter using [-2,-1,0;-1,1,1;0,1,2] kernel with 0.5 gray bias.
/// </summary>
[FilterInfo("Emboss",
  Description = "3x3 emboss convolution with gray bias", Category = FilterCategory.Enhancement)]
public readonly struct Emboss : IPixelFilter {

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
    => callback.Invoke(new EmbossKernel<TWork, TKey, TPixel, TEncode>());

  public static Emboss Default => new();
}

file readonly struct EmbossKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float r, float g, float b) _GetRgb(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return (r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Kernel: [-2,-1,0; -1,1,1; 0,1,2]
    var (tlr, tlg, tlb) = _GetRgb(window.M1M1);
    var (tr, tg, tb) = _GetRgb(window.M1P0);
    var (lr, lg, lb) = _GetRgb(window.P0M1);
    var (cr, cg, cb) = _GetRgb(window.P0P0);
    var (rr, rg, rb) = _GetRgb(window.P0P1);
    var (br, bg, bb) = _GetRgb(window.P1P0);
    var (brr, brg, brb) = _GetRgb(window.P1P1);

    var or = Math.Max(0f, Math.Min(1f, -2f * tlr - tr - lr + cr + rr + br + 2f * brr + 0.5f));
    var og = Math.Max(0f, Math.Min(1f, -2f * tlg - tg - lg + cg + rg + bg + 2f * brg + 0.5f));
    var ob = Math.Max(0f, Math.Min(1f, -2f * tlb - tb - lb + cb + rb + bb + 2f * brb + 0.5f));

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
