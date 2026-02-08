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
/// Extracts a single color channel as a grayscale image.
/// </summary>
/// <remarks>
/// <para>Isolates one color channel (Red, Green, Blue, or Alpha) from each pixel
/// and outputs the channel intensity as a grayscale value with all RGB channels set to it.</para>
/// </remarks>
[FilterInfo("Channel Extraction",
  Description = "Extract single color channel as grayscale", Category = FilterCategory.Analysis)]
public readonly struct ChannelExtraction(ColorChannel channel = ColorChannel.Red) : IPixelFilter {
  private readonly ColorChannel _channel = channel;

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
    => callback.Invoke(new ChannelKernel<TWork, TKey, TPixel, TEncode>(this._channel));

  /// <summary>Gets the default Channel Extraction filter (Red channel).</summary>
  public static ChannelExtraction Default => new();
}

#region Channel Kernel

file readonly struct ChannelKernel<TWork, TKey, TPixel, TEncode>(ColorChannel channel)
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
    var v = channel switch {
      ColorChannel.Red => r,
      ColorChannel.Green => g,
      ColorChannel.Blue => b,
      ColorChannel.Alpha => a,
      _ => r
    };
    var outA = channel == ColorChannel.Alpha ? 1f : a;
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, outA));
  }
}

#endregion
