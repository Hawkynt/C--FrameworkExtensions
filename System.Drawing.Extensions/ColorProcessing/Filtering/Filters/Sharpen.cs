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
/// 3x3 unsharp mask sharpening filter.
/// </summary>
/// <remarks>
/// <para>Computes the average of cardinal neighbors, then extrapolates the center pixel
/// away from that average: result = center + strength * (center - average).
/// This enhances edges and fine detail.</para>
/// </remarks>
[FilterInfo("Sharpen",
  Description = "3x3 unsharp mask sharpening", Category = FilterCategory.Enhancement)]
public readonly struct Sharpen(float strength = 0.5f) : IPixelFilter {
  private readonly float _strength = strength;

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
    => callback.Invoke(new SharpenKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this._strength));

  /// <summary>Gets the default Sharpen filter (0.5 strength).</summary>
  public static Sharpen Default => new();
}

#region Sharpen 1x Kernel

file readonly struct SharpenKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float strength)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {

    var avgH = lerp.Lerp(window.P0M1.Work, window.P0P1.Work, 500, 500);
    var avgV = lerp.Lerp(window.M1P0.Work, window.P1P0.Work, 500, 500);
    var avg = lerp.Lerp(avgH, avgV, 500, 500);

    var pixel = window.P0P0.Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in pixel);
    var (ar, ag, ab, _) = ColorConverter.GetNormalizedRgba(in avg);

    var sr = Math.Max(0f, Math.Min(1f, cr + strength * (cr - ar)));
    var sg = Math.Max(0f, Math.Min(1f, cg + strength * (cg - ag)));
    var sb = Math.Max(0f, Math.Min(1f, cb + strength * (cb - ab)));

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(sr, sg, sb, ca));
  }
}

#endregion
