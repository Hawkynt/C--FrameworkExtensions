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
/// ACES Filmic Tone Mapping Curve — Krzysztof Narkowicz / Stephen Hill
/// rational-fit approximation of the Academy Color Encoding System RRT/ODT.
/// Per-channel filmic operator that produces the warm, slightly contrasty
/// "cinematic" look used by most modern game engines.
/// </summary>
/// <remarks>
/// <para>
/// Curve form (Narkowicz 2015): <c>(x(ax+b))/(x(cx+d)+e)</c> with the constants
/// <c>a=2.51, b=0.03, c=2.43, d=0.59, e=0.14</c>. Slightly desaturating in the
/// shoulder; matches an exposure-equivalent-to-1 ACES output to within ~1%.
/// </para>
/// <para>
/// Use case: HDR → LDR sRGB for game / film rendering where a film-like response
/// is desired. Cheaper than a full ACES pipeline; the dominant choice for in-engine
/// tonemap since ~2015.
/// </para>
/// <para>Parameter range: <paramref name="exposure"/> 0.1–10 (default 1).</para>
/// </remarks>
[FilterInfo("Aces",
  Author = "Narkowicz / Hill", Year = 2015,
  Url = "https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/",
  Description = "ACES filmic tone mapping curve (Narkowicz approximation)",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Aces : IPixelFilter {
  private readonly float _exposure;

  public Aces() : this(1f) { }

  public Aces(float exposure) {
    this._exposure = Math.Max(0.01f, Math.Min(100f, exposure));
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
    => callback.Invoke(new AcesKernel<TWork, TKey, TPixel, TEncode>(this._exposure));

  public static Aces Default => new();
}

file readonly struct AcesKernel<TWork, TKey, TPixel, TEncode>(float exposure)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Curve(float x) {
    const float A = 2.51f;
    const float B = 0.03f;
    const float C = 2.43f;
    const float D = 0.59f;
    const float E = 0.14f;
    var num = x * (A * x + B);
    var den = x * (C * x + D) + E;
    var v = num / den;
    return v < 0f ? 0f : (v > 1f ? 1f : v);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);

    var or = _Curve(r * exposure);
    var og = _Curve(g * exposure);
    var ob = _Curve(b * exposure);

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
