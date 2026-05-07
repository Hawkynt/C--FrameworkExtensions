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
/// Reinhard extended tone mapping operator with explicit white-point control
/// (Reinhard, Stark, Shirley &amp; Ferwerda 2002, eq. 4).
/// Maps <c>L_out = L * (1 + L / W²) / (1 + L)</c> where <c>W</c> is the smallest
/// luminance to be mapped to 1.0 — values at or above <c>W</c> burn out cleanly.
/// </summary>
/// <remarks>
/// <para>
/// Use case: HDR → LDR conversion when you can specify the brightest scene
/// luminance you want to preserve detail in. Unlike the basic <see cref="Reinhard"/>
/// operator this one guarantees mapped output reaches 1.0 at the white point.
/// </para>
/// <para>
/// Parameter ranges: <paramref name="exposure"/> 0.1–10 (default 1),
/// <paramref name="whitePoint"/> 0.5–10 (default 4 — generous; lower values
/// compress more aggressively).
/// </para>
/// </remarks>
[FilterInfo("ReinhardExtended",
  Author = "Reinhard, Stark, Shirley & Ferwerda", Year = 2002,
  Url = "https://www.cs.utah.edu/docs/techreports/2002/pdf/UUCS-02-001.pdf",
  Description = "Reinhard 2002 extended tone mapper with white-point parameter",
  Category = FilterCategory.ColorCorrection)]
public readonly struct ReinhardExtended : IPixelFilter {
  private readonly float _exposure;
  private readonly float _whitePointSq;

  public ReinhardExtended() : this(1f, 4f) { }

  public ReinhardExtended(float exposure = 1f, float whitePoint = 4f) {
    this._exposure = Math.Max(0.1f, Math.Min(10f, exposure));
    var wp = Math.Max(0.5f, Math.Min(10f, whitePoint));
    this._whitePointSq = wp * wp;
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
    => callback.Invoke(new ReinhardExtendedKernel<TWork, TKey, TPixel, TEncode>(this._exposure, this._whitePointSq));

  public static ReinhardExtended Default => new();
}

file readonly struct ReinhardExtendedKernel<TWork, TKey, TPixel, TEncode>(float exposure, float whitePointSq)
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
    var mapped = lumE * (1f + lumE / whitePointSq) / (1f + lumE);
    var scale = mapped / lum;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Min(1f, Math.Max(0f, r * scale)),
      Math.Min(1f, Math.Max(0f, g * scale)),
      Math.Min(1f, Math.Max(0f, b * scale)), a));
  }
}
