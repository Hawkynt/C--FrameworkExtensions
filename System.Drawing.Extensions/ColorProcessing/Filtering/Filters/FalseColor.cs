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
/// Maps luminance to a three-stop color gradient (low → mid → high).
/// </summary>
[FilterInfo("FalseColor",
  Description = "Map luminance to three-stop color gradient", Category = FilterCategory.Artistic)]
public readonly struct FalseColor(
  float lowR = 0f, float lowG = 0f, float lowB = 1f,
  float midR = 0f, float midG = 1f, float midB = 0f,
  float highR = 1f, float highG = 0f, float highB = 0f
) : IPixelFilter {
  private readonly float _lr = lowR, _lg = lowG, _lb = lowB;
  private readonly float _mr = midR, _mg = midG, _mb = midB;
  private readonly float _hr = highR, _hg = highG, _hb = highB;

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
    => callback.Invoke(new FalseColorKernel<TWork, TKey, TPixel, TEncode>(
      this._lr, this._lg, this._lb, this._mr, this._mg, this._mb, this._hr, this._hg, this._hb));

  public static FalseColor Default => new();
}

file readonly struct FalseColorKernel<TWork, TKey, TPixel, TEncode>(
  float lr, float lg, float lb, float mr, float mg, float mb, float hr, float hg, float hb)
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
    var lum = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;

    float or, og, ob;
    if (lum < 0.5f) {
      var t = lum * 2f;
      or = lr + (mr - lr) * t;
      og = lg + (mg - lg) * t;
      ob = lb + (mb - lb) * t;
    } else {
      var t = (lum - 0.5f) * 2f;
      or = mr + (hr - mr) * t;
      og = mg + (hg - mg) * t;
      ob = mb + (hb - mb) * t;
    }

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
