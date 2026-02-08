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
/// Binary threshold filter that converts pixels to black or white based on luminance.
/// </summary>
/// <remarks>
/// <para>Computes per-pixel luminance via <see cref="ColorConverter.GetLuminance{TWork}"/>
/// and compares it against a configurable threshold level.</para>
/// <para>Pixels with luminance at or above the threshold become white; those below become black.</para>
/// </remarks>
[FilterInfo("Threshold",
  Description = "Binary threshold based on luminance", Category = FilterCategory.Analysis)]
public readonly struct Threshold(float level = 0.5f) : IPixelFilter {
  private readonly float _level = level;

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
    => callback.Invoke(new ThresholdKernel<TWork, TKey, TPixel, TEncode>(this._level));

  /// <summary>Gets the default Threshold filter (0.5 level).</summary>
  public static Threshold Default => new();
}

#region Threshold 1x Kernel

file readonly struct ThresholdKernel<TWork, TKey, TPixel, TEncode>(float level)
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
    var v = lum >= level ? 1f : 0f;
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, a));
  }
}

#endregion
