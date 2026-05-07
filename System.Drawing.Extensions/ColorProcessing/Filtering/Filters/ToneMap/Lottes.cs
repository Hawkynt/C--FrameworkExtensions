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

namespace Hawkynt.ColorProcessing.Filtering.Filters.ToneMap;

/// <summary>
/// Lottes single-curve filmic tone-mapping (Timothy Lottes 2016 GDC).
/// Generic four-parameter rational curve <c>f(x) = x^a / (x^(a·d)·b + c)</c>
/// where (b, c) are pre-computed from (midIn, midOut) and (hdrMax, hdrOut)
/// constraints so the curve hits both anchors exactly.
/// </summary>
/// <remarks>
/// <para>
/// Reference: Timothy Lottes, "Advanced Techniques and Optimization of HDR
/// Color Pipelines", GDC 2016; AMD GPUOpen Reversible Tonemapper post.
/// Defaults match the GDC slides: contrast a=1.6, shoulder d=0.977,
/// midIn=0.18, midOut=0.267, hdrMax=8.0, hdrOut=1.0.
/// </para>
/// <para>Parameter ranges: <paramref name="exposure"/> 0.1–10 (default 1),
/// <paramref name="contrast"/> 0.5–4 (default 1.6) — higher values steepen
/// the toe.</para>
/// </remarks>
[FilterInfo("Lottes",
  Author = "Timothy Lottes", Year = 2016,
  Url = "https://gpuopen.com/learn/optimized-reversible-tonemapper-for-resolve/",
  Description = "Lottes 2016 four-parameter filmic tone mapper",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Lottes : IPixelFilter {
  // Lottes GDC-2016 defaults; held constant so the public API exposes only the
  // most useful knob (contrast). Users wanting per-anchor control should
  // construct via the all-arg overload.
  private const float ShoulderDefault = 0.977f;
  private const float MidInDefault = 0.18f;
  private const float MidOutDefault = 0.267f;
  private const float HdrMaxDefault = 8.0f;
  private const float HdrOutDefault = 1.0f;

  private readonly float _exposure;
  private readonly float _a;    // contrast
  private readonly float _ad;   // contrast * shoulder
  private readonly float _b, _c;

  public Lottes() : this(1f, 1.6f) { }

  public Lottes(float exposure = 1f, float contrast = 1.6f)
    : this(exposure, contrast, ShoulderDefault, MidInDefault, MidOutDefault, HdrMaxDefault, HdrOutDefault) { }

  public Lottes(float exposure, float contrast, float shoulder, float midIn, float midOut, float hdrMax, float hdrOut) {
    this._exposure = Math.Max(0.1f, Math.Min(10f, exposure));
    var a = Math.Max(0.5f, Math.Min(4f, contrast));
    var d = Math.Max(0.1f, Math.Min(2f, shoulder));
    this._a = a;
    this._ad = a * d;

    // Solve the system y = x^a / (x^(ad)·b + c) at two anchors:
    //   (midIn → midOut) and (hdrMax → hdrOut).
    // Algebra (Lottes GDC 2016 / GPUOpen) gives:
    //   b = (−midIn^a + (midOut/hdrOut)·hdrMax^a) / ((midOut/hdrOut)·hdrMax^(ad) − midIn^(ad))
    //   c = (hdrMax^(ad)·midIn^a − hdrMax^a·midIn^(ad)·(midOut/hdrOut)) / ((midOut/hdrOut)·hdrMax^(ad) − midIn^(ad))
    var midIn_a = (float)Math.Pow(midIn, a);
    var midIn_ad = (float)Math.Pow(midIn, this._ad);
    var hdrMax_a = (float)Math.Pow(hdrMax, a);
    var hdrMax_ad = (float)Math.Pow(hdrMax, this._ad);
    var ratio = midOut / Math.Max(1e-6f, hdrOut);
    var denom = ratio * hdrMax_ad - midIn_ad;
    if (Math.Abs(denom) < 1e-9f) denom = 1e-9f;

    this._b = (-midIn_a + ratio * hdrMax_a) / denom;
    this._c = (hdrMax_ad * midIn_a - hdrMax_a * midIn_ad * ratio) / denom;
  }

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
    => callback.Invoke(new LottesKernel<TWork, TKey, TPixel, TEncode>(this._exposure, this._a, this._ad, this._b, this._c));

  public static Lottes Default => new();
}

file readonly struct LottesKernel<TWork, TKey, TPixel, TEncode>(float exposure, float a, float ad, float b, float c)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _Curve(float x) {
    if (x <= 0f) return 0f;
    var pa = (float)Math.Pow(x, a);
    var pad = (float)Math.Pow(x, ad);
    var denom = pad * b + c;
    return denom > 1e-9f ? pa / denom : 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b_, a_) = ColorConverter.GetNormalizedRgba(in pixel);

    var or = _Curve(r * exposure);
    var og = _Curve(g * exposure);
    var ob = _Curve(b_ * exposure);

    or = or < 0f ? 0f : (or > 1f ? 1f : or);
    og = og < 0f ? 0f : (og > 1f ? 1f : og);
    ob = ob < 0f ? 0f : (ob > 1f ? 1f : ob);

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a_));
  }
}
