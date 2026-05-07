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
/// Simulates cross-processed film (E6 film in C41 chemistry) with shifted color curves and boosted saturation.
/// </summary>
/// <remarks>
/// On the linear-RGB (HQ) work path the saturation-boost stage gamma-encodes to sRGB
/// before the HSL conversion and gamma-decodes after, so HSL math operates in its
/// canonical sRGB display domain (linear-light fix).
/// </remarks>
[FilterInfo("CrossProcess",
  Description = "Simulate cross-processed film look with shifted color curves", Category = FilterCategory.Artistic)]
public readonly struct CrossProcess(float intensity = 1f) : IPixelFilter {
  private readonly float _intensity = ColorConverter.Saturate(intensity);

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
    => callback.Invoke(new CrossProcessKernel<TWork, TKey, TPixel, TEncode>(this._intensity));

  public static CrossProcess Default => new(1f);
}

file readonly struct CrossProcessKernel<TWork, TKey, TPixel, TEncode>(float intensity)
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

    var nr = r + intensity * 0.1f * (float)Math.Sin(r * Math.PI);
    var ng = g - intensity * 0.05f * (float)Math.Sin(g * Math.PI);
    var nb = b - intensity * 0.1f * (float)Math.Sin(b * Math.PI);

    nr = ColorConverter.Saturate(nr);
    ng = ColorConverter.Saturate(ng);
    nb = ColorConverter.Saturate(nb);

    // HSL is defined on sRGB display values; on the linear-RGB (HQ) path the floats
    // are linear-light, so encode→sRGB before HSL, decode after.
    float hr, hg, hb;
    if (typeof(TWork) == typeof(Bgra8888)) {
      hr = nr; hg = ng; hb = nb;
    } else {
      hr = FixedPointMath.GammaCompress((int)(nr * 65536f + 0.5f)) / 255f;
      hg = FixedPointMath.GammaCompress((int)(ng * 65536f + 0.5f)) / 255f;
      hb = FixedPointMath.GammaCompress((int)(nb * 65536f + 0.5f)) / 255f;
    }

    var (h, s, l) = HslMath.RgbToHsl(hr, hg, hb);
    s = Math.Min(1f, s * (1f + 0.3f * intensity));
    var (or, og, ob) = HslMath.HslToRgb(h, s, l);

    if (typeof(TWork) != typeof(Bgra8888)) {
      or = FixedPointMath.GammaExpand((byte)(int)(or * 255f + 0.5f)) / 65536f;
      og = FixedPointMath.GammaExpand((byte)(int)(og * 255f + 0.5f)) / 65536f;
      ob = FixedPointMath.GammaExpand((byte)(int)(ob * 255f + 0.5f)) / 65536f;
    }

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      ColorConverter.Saturate(or),
      ColorConverter.Saturate(og),
      ColorConverter.Saturate(ob), a));
  }
}
