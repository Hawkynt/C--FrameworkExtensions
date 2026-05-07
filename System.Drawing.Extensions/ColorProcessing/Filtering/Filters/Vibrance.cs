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
/// Adjusts saturation with stronger effect on less-saturated pixels (smart saturation).
/// </summary>
[FilterInfo("Vibrance",
  Description = "Smart saturation boost targeting desaturated pixels", Category = FilterCategory.ColorCorrection)]
public readonly struct Vibrance(float amount = 0f) : IPixelFilter {
  private readonly float _amount = amount;

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
    => callback.Invoke(new VibranceKernel<TWork, TKey, TPixel, TEncode>(this._amount));

  public static Vibrance Default => new();
}

file readonly struct VibranceKernel<TWork, TKey, TPixel, TEncode>(float amount)
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
    var (h, s, l) = HslMath.RgbToHsl(r, g, b);
    var boost = amount * (1f - s);

    // Adobe-style skin-tone protection: hues in 0..50° (red-orange) get less
    // saturation boost, since pushing skin saturation looks unnatural.
    // Hue is normalised to [0,1]; skin range is hue ∈ [0, 0.139] OR [0.861, 1]
    // (the hue-wrap neighbourhood of pure red). A smoothstep-shaped attenuation
    // drops the boost to 40% at pure red and ramps back to 100% at hue 0.139
    // (or 0.861 from the wrap side). Reference:
    // https://helpx.adobe.com/camera-raw/using/saturation-vibrance-adjustments.html
    const float SkinEdge = 0.139f; // 50° / 360°
    float distFromRed;
    if (h <= SkinEdge)
      distFromRed = h;
    else if (h >= 1f - SkinEdge)
      distFromRed = 1f - h;
    else
      distFromRed = SkinEdge; // outside skin range → no attenuation

    // smoothstep(0, SkinEdge, distFromRed) ∈ [0,1]; 0 at red, 1 at edge.
    var t = distFromRed / SkinEdge;
    if (t < 0f) t = 0f;
    else if (t > 1f) t = 1f;
    var s01 = t * t * (3f - 2f * t);
    // skinFactor: 0.4 at red, 1.0 at edge and beyond.
    var skinFactor = 0.4f + 0.6f * s01;
    boost *= skinFactor;

    s = ColorConverter.Saturate(s + boost);
    var (or, og, ob) = HslMath.HslToRgb(h, s, l);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
