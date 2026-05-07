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
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Adjusts hue, saturation, and lightness in HSL color space.
/// </summary>
/// <remarks>
/// On the linear-RGB (HQ) work path the RGB triplet is gamma-encoded to sRGB before
/// the HSL conversion and gamma-decoded after, so HSL math operates in its
/// canonical sRGB display domain (linear-light fix).
/// </remarks>
[FilterInfo("HueSaturation",
  Description = "Adjust hue, saturation, and lightness in HSL space", Category = FilterCategory.ColorCorrection)]
public readonly struct HueSaturation(float hueShift = 0f, float saturation = 0f, float lightness = 0f) : IPixelFilter {
  private readonly float _hueShift = hueShift;
  private readonly float _saturation = saturation;
  private readonly float _lightness = lightness;

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
    => callback.Invoke(new HueSaturationKernel<TWork, TKey, TPixel, TEncode>(this._hueShift, this._saturation, this._lightness));

  public static HueSaturation Default => new();
}

file readonly struct HueSaturationKernel<TWork, TKey, TPixel, TEncode>(float hueShift, float saturation, float lightness)
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

    // HSL is defined on sRGB display values; on the linear-RGB (HQ) path the floats
    // from GetNormalizedRgba are linear-light, so encode→sRGB before HSL, decode after.
    float hr, hg, hb;
    if (typeof(TWork) == typeof(Bgra8888)) {
      hr = r; hg = g; hb = b;
    } else {
      hr = FixedPointMath.GammaCompress((int)(r * 65536f + 0.5f)) / 255f;
      hg = FixedPointMath.GammaCompress((int)(g * 65536f + 0.5f)) / 255f;
      hb = FixedPointMath.GammaCompress((int)(b * 65536f + 0.5f)) / 255f;
    }

    var (h, s, l) = HslMath.RgbToHsl(hr, hg, hb);
    h += hueShift;
    if (h < 0f) ++h;
    if (h > 1f) --h;
    s = ColorConverter.Saturate(s + saturation);
    l = ColorConverter.Saturate(l + lightness);
    var (or, og, ob) = HslMath.HslToRgb(h, s, l);

    if (typeof(TWork) != typeof(Bgra8888)) {
      or = FixedPointMath.GammaExpand((byte)(int)(or * 255f + 0.5f)) / 65536f;
      og = FixedPointMath.GammaExpand((byte)(int)(og * 255f + 0.5f)) / 65536f;
      ob = FixedPointMath.GammaExpand((byte)(int)(ob * 255f + 0.5f)) / 65536f;
    }

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
