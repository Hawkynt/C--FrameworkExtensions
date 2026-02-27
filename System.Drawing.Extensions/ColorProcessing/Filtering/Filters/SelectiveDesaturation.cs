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
/// Desaturates all colors except a chosen hue range, producing a "grayscale with selective coloring" effect.
/// </summary>
[FilterInfo("SelectiveDesaturation",
  Description = "Desaturate all colors except a chosen hue range", Category = FilterCategory.Artistic)]
public readonly struct SelectiveDesaturation(float targetHue, float hueRange = 30f, float strength = 1f) : IPixelFilter {
  private readonly float _targetHue = ((targetHue % 360f) + 360f) % 360f;
  private readonly float _hueRange = Math.Max(0f, Math.Min(180f, hueRange));
  private readonly float _strength = Math.Max(0f, Math.Min(1f, strength));

  public SelectiveDesaturation() : this(0f, 30f, 1f) { }

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
    => callback.Invoke(new SelectiveDesaturationKernel<TWork, TKey, TPixel, TEncode>(
      this._targetHue, this._hueRange, this._strength));

  public static SelectiveDesaturation Default => new();
}

file readonly struct SelectiveDesaturationKernel<TWork, TKey, TPixel, TEncode>(
  float targetHue, float hueRange, float strength)
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

    var hueDeg = h * 360f;
    var dist = Math.Abs(hueDeg - targetHue);
    if (dist > 180f)
      dist = 360f - dist;

    var keep = 1f - Math.Max(0f, Math.Min(1f, (dist - hueRange * 0.5f) / Math.Max(hueRange * 0.5f, 0.001f)));
    var newS = s * (1f - strength * (1f - keep));
    var (or, og, ob) = HslMath.HslToRgb(h, newS, l);

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
