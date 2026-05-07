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
/// Reinhard-extended tone mapping operator for compressing high dynamic range values.
/// Implements <c>L' = L·(1 + L/W²) / (1 + L)</c> per Reinhard et al. 2002 eq. (4),
/// applied via per-pixel luminance scaling.
/// </summary>
/// <remarks>
/// This is the same operator as <see cref="ToneMap.ReinhardExtended"/> exposed under a
/// different filter ID for backwards compatibility. New code should prefer
/// <see cref="ToneMap.ReinhardExtended"/> for clarity.
/// </remarks>
[FilterInfo("HDRToneMap",
  Author = "Reinhard, Stark, Shirley & Ferwerda", Year = 2002,
  Url = "https://www.cs.utah.edu/docs/techreports/2002/pdf/UUCS-02-001.pdf",
  Description = "Reinhard-extended HDR tone mapping (alias of ReinhardExtended) with exposure and white point controls",
  Category = FilterCategory.ColorCorrection)]
public readonly struct HDRToneMap : IPixelFilter {
  private readonly float _exposure;
  private readonly float _whitePointSq;

  public HDRToneMap() : this(1f, 4f) { }

  public HDRToneMap(float exposure, float whitePoint = 4f) {
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
    => callback.Invoke(new HDRToneMapKernel<TWork, TKey, TPixel, TEncode>(this._exposure, this._whitePointSq));

  public static HDRToneMap Default => new();
}

file readonly struct HDRToneMapKernel<TWork, TKey, TPixel, TEncode>(float exposure, float whitePointSq)
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

    if (lum < 0.001f) {
      dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(0f, 0f, 0f, a));
      return;
    }

    var lumE = lum * exposure;
    var mapped = lumE * (1f + lumE / whitePointSq) / (1f + lumE);
    var scale = mapped / lum;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Min(1f, r * scale),
      Math.Min(1f, g * scale),
      Math.Min(1f, b * scale), a));
  }
}
