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

namespace Hawkynt.ColorProcessing.Filtering.Filters.ToneMap;

/// <summary>
/// Reinhard tone mapping operator (Reinhard, Stark, Shirley &amp; Ferwerda 2002).
/// Implements the global luminance compressor <c>L_out = L_in / (1 + L_in)</c>
/// — the simplest of the Reinhard family — applied uniformly to all pixels in the
/// luminance domain, then redistributed back to RGB by per-pixel scaling.
/// </summary>
/// <remarks>
/// <para>
/// Use case: turning HDR-ish (linear, &gt;1) imagery into LDR sRGB suitable for
/// 8-bit display. Without a white-point parameter, very bright values are
/// compressed asymptotically toward 1.0 — see <see cref="ReinhardExtended"/> for
/// a controllable max-luminance variant.
/// </para>
/// <para>
/// Reference: "Photographic Tone Reproduction for Digital Images",
/// SIGGRAPH 2002, eq. (3).
/// </para>
/// </remarks>
[FilterInfo("Reinhard",
  Author = "Reinhard, Stark, Shirley & Ferwerda", Year = 2002,
  Url = "https://www.cs.utah.edu/docs/techreports/2002/pdf/UUCS-02-001.pdf",
  Description = "Reinhard 2002 global luminance tone mapper L/(1+L)",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Reinhard : IPixelFilter {
  private readonly float _exposure;

  public Reinhard() : this(1f) { }

  public Reinhard(float exposure) {
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
    => callback.Invoke(new ReinhardKernel<TWork, TKey, TPixel, TEncode>(this._exposure));

  public static Reinhard Default => new();
}

file readonly struct ReinhardKernel<TWork, TKey, TPixel, TEncode>(float exposure)
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
    var lum = ColorConverter.LuminanceFromRgb(r, g, b);

    if (lum < 1e-6f) {
      dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(0f, 0f, 0f, a));
      return;
    }

    var lumE = lum * exposure;
    var mapped = lumE / (1f + lumE);
    var scale = mapped / lum;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Min(1f, Math.Max(0f, r * scale)),
      Math.Min(1f, Math.Max(0f, g * scale)),
      Math.Min(1f, Math.Max(0f, b * scale)), a));
  }
}
