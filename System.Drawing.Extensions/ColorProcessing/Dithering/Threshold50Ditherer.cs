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
/// Hard 50% threshold "line-art" dithering — snaps every channel to 0 or 1 before
/// palette lookup, then maps the resulting corner of the unit cube to the nearest
/// palette colour.
/// </summary>
/// <remarks>
/// <para>
/// This is the classic 1-bit / line-art operator: if the channel value is below
/// 0.5 it becomes 0, otherwise 1. For an RGB image this collapses every pixel to
/// one of the 8 corners of the RGB cube (white, black, primaries, secondaries)
/// which is then snapped to the nearest entry in the supplied palette.
/// </para>
/// <para>
/// Unlike <see cref="NoDithering"/> (which feeds the raw colour to the palette
/// matcher) this ditherer *pre-quantises* to the corners of the cube, producing
/// the crisp look expected for scanned line-art, text, ink drawings and faxable
/// documents. Unlike ordered or error-diffusion ditherers no spatial pattern is
/// introduced — mid-tones are resolved by clean hard edges rather than shading.
/// </para>
/// <para>
/// Reference: ITU-T T.4 / T.6 (Group 3/Group 4 fax) facsimile encoding uses a
/// 50% luminance threshold as its canonical pre-palette operator. See also
/// <a href="https://en.wikipedia.org/wiki/Thresholding_(image_processing)">
/// Thresholding (image processing)</a>.
/// </para>
/// <para>
/// Parallel-friendly (per-pixel operation, no state). Deterministic.
/// </para>
/// </remarks>
[Ditherer("Threshold 50%", Description = "Hard 50% per-channel threshold for line-art / 1-bit output", Type = DitheringType.None)]
public readonly struct Threshold50Ditherer : IDitherer {

  /// <summary>Default instance.</summary>
  public static Threshold50Ditherer Instance { get; } = new();

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
      var (c1, c2, c3, alpha) = color.ToNormalized();

      // Hard 50% threshold per channel (alpha is preserved unchanged so
      // transparent-friendly palettes still work as expected).
      var t1 = c1.ToFloat() < 0.5f ? 0f : 1f;
      var t2 = c2.ToFloat() < 0.5f ? 0f : 1f;
      var t3 = c3.ToFloat() < 0.5f ? 0f : 1f;

      var thresholded = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(t1),
        UNorm32.FromFloatClamped(t2),
        UNorm32.FromFloatClamped(t3),
        alpha
      );

      indices[targetIdx] = (byte)lookup.FindNearest(thresholded);
    }
  }
}
