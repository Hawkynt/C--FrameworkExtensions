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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// 3x3 weighted blur filter.
/// </summary>
/// <remarks>
/// <para>Blends each pixel toward the average of its cardinal neighbors,
/// producing a softening effect proportional to the strength parameter.</para>
/// </remarks>
[FilterInfo("Blur",
  Description = "3x3 weighted blur", Category = FilterCategory.Enhancement)]
public readonly struct Blur(float strength = 0.5f) : IPixelFilter {
  private readonly int _blurWeight = (int)(strength * 500);

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
    => callback.Invoke(new BlurKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this._blurWeight));

  /// <summary>Gets the default Blur filter (0.5 strength).</summary>
  public static Blur Default => new();
}

#region Blur Helpers

file static class BlurHelpers {
  public const int WeightScale = 1000;
}

#endregion

#region Blur 1x Kernel

file readonly struct BlurKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, int blurWeight)
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
    var center = window.P0P0.Work;

    // Compute average of cardinal neighbors
    var avgH = lerp.Lerp(window.P0M1.Work, window.P0P1.Work, 500, 500);
    var avgV = lerp.Lerp(window.M1P0.Work, window.P1P0.Work, 500, 500);
    var avgNeighbor = lerp.Lerp(avgH, avgV, 500, 500);

    // Blend center toward neighbor average
    var blurred = lerp.Lerp(center, avgNeighbor, BlurHelpers.WeightScale - blurWeight, blurWeight);

    dest[0] = encoder.Encode(blurred);
  }
}

#endregion
