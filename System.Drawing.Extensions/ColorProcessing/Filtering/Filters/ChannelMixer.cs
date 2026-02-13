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
/// Recombines R/G/B channels with arbitrary coefficients.
/// </summary>
[FilterInfo("ChannelMixer",
  Description = "Recombine RGB channels with a 3x3 coefficient matrix", Category = FilterCategory.ColorCorrection)]
public readonly struct ChannelMixer(
  float rr = 1f,
  float rg = 0f,
  float rb = 0f,
  float gr = 0f,
  float gg = 1f,
  float gb = 0f,
  float br = 0f,
  float bg = 0f,
  float bb = 1f
)
  : IPixelFilter {
  public ChannelMixer()
    : this(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f) { }

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
    => callback.Invoke(new ChannelMixerKernel<TWork, TKey, TPixel, TEncode>(
      rr, rg, rb,
      gr, gg, gb,
      br, bg, bb));

  public static ChannelMixer Default => new();
}

file readonly struct ChannelMixerKernel<TWork, TKey, TPixel, TEncode>(
  float rr, float rg, float rb,
  float gr, float gg, float gb,
  float br, float bg, float bb)
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
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);

    var or = r * rr + g * rg + b * rb;
    var og = r * gr + g * gg + b * gb;
    var ob = r * br + g * bg + b * bb;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Max(0f, Math.Min(1f, or)),
      Math.Max(0f, Math.Min(1f, og)),
      Math.Max(0f, Math.Min(1f, ob)), a));
  }
}
