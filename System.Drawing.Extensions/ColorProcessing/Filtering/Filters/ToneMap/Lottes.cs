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
/// Generic four-parameter rational curve <c>L^a / (L^a·d + b)</c> with the
/// constants pre-computed from contrast, shoulder, mid-in, mid-out, hdr-max,
/// hdr-out so the curve hits the target shoulder shape exactly.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses the simplified per-channel form distributed in
/// the GDC slides — `pow`-based with a single contrast knob. Parameter
/// translation: <c>a = contrast</c> giving a generalised pure-power curve
/// with a smooth shoulder rolloff.
/// </para>
/// <para>
/// Use case: cheap, easily-tunable filmic operator. Slightly punchier
/// midtones than ACES; less desaturation in highlights than Hable.
/// </para>
/// <para>Parameter ranges: <paramref name="exposure"/> 0.1–10 (default 1),
/// <paramref name="contrast"/> 0.5–4 (default 1.6) — higher values steepen
/// the toe.</para>
/// </remarks>
[FilterInfo("Lottes",
  Author = "Timothy Lottes", Year = 2016,
  Url = "https://gpuopen.com/learn/optimized-reversible-tonemapper-for-resolve/",
  Description = "Lottes 2016 single-curve filmic tone mapper",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Lottes : IPixelFilter {
  private readonly float _exposure;
  private readonly float _contrast;

  public Lottes() : this(1f, 1.6f) { }

  public Lottes(float exposure = 1f, float contrast = 1.6f) {
    this._exposure = Math.Max(0.1f, Math.Min(10f, exposure));
    this._contrast = Math.Max(0.5f, Math.Min(4f, contrast));
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
    => callback.Invoke(new LottesKernel<TWork, TKey, TPixel, TEncode>(this._exposure, this._contrast));

  public static Lottes Default => new();
}

file readonly struct LottesKernel<TWork, TKey, TPixel, TEncode>(float exposure, float contrast)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Curve(float x, float a) {
    // L^a / (L^a + 1) — smooth saturating curve with a controlled toe steepness.
    if (x <= 0f) return 0f;
    var p = (float)Math.Pow(x, a);
    return p / (p + 1f);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);

    // Normalize so a midtone of 0.18 maps roughly to itself: divide by C(0.18,a)/0.18.
    var anchorIn = 0.18f * exposure;
    var anchor = _Curve(anchorIn, contrast);
    var norm = anchor < 1e-6f ? 1f : (anchorIn / anchor);

    var or = _Curve(r * exposure, contrast) * norm;
    var og = _Curve(g * exposure, contrast) * norm;
    var ob = _Curve(b * exposure, contrast) * norm;

    or = or < 0f ? 0f : (or > 1f ? 1f : or);
    og = og < 0f ? 0f : (og > 1f ? 1f : og);
    ob = ob < 0f ? 0f : (ob > 1f ? 1f : ob);

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
