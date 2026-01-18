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

using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// No-dithering quantizer that outputs palette indices by finding nearest color without modification.
/// </summary>
/// <remarks>
/// <para>Simply maps each pixel to the nearest palette color without any dithering.</para>
/// <para>Fast and produces sharp results but may show color banding in gradients.</para>
/// </remarks>
[Ditherer("No Dithering", Description = "Nearest-neighbor quantization without dithering", Type = DitheringType.None)]
public readonly struct NoDithering : IDitherer {

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;

    for (var y = startY; y < endY; ++y)
    for (int x = width, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x > 0; ++sourceIdx, ++targetIdx, --x) {
      var color = decoder.Decode(source[sourceIdx]);
      var nearestIdx = lookup.FindNearest(color);
      indices[targetIdx] = (byte)nearestIdx;
    }
  }

  /// <summary>Default instance of no-dithering quantizer.</summary>
  public static NoDithering Instance { get; } = new();
}
