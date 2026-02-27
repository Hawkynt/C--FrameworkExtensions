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
/// Simulates cross-processed film (E6 film in C41 chemistry) with shifted color curves and boosted saturation.
/// </summary>
[FilterInfo("CrossProcess",
  Description = "Simulate cross-processed film look with shifted color curves", Category = FilterCategory.Artistic)]
public readonly struct CrossProcess(float intensity = 1f) : IPixelFilter {
  private readonly float _intensity = Math.Max(0f, Math.Min(1f, intensity));

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

  public static CrossProcess Default => new();
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

    var (h, s, l) = HslMath.RgbToHsl(
      Math.Max(0f, Math.Min(1f, nr)),
      Math.Max(0f, Math.Min(1f, ng)),
      Math.Max(0f, Math.Min(1f, nb)));
    s = Math.Min(1f, s * (1f + 0.3f * intensity));
    var (or, og, ob) = HslMath.HslToRgb(h, s, l);

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Max(0f, Math.Min(1f, or)),
      Math.Max(0f, Math.Min(1f, og)),
      Math.Max(0f, Math.Min(1f, ob)), a));
  }
}
