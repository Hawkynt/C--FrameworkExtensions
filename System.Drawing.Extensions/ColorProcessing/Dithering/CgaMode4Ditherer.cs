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
/// CGA Mode-4 ordered dither — restricts output to a 4-entry palette and
/// applies a 1×2 (scan-line-paired) Bayer screen that mimics the
/// characteristic "double-row" look of Mode 4 on an NTSC composite monitor.
/// </summary>
/// <remarks>
/// <para>
/// IBM's CGA Mode 4 (320×200, 4 colours) was the workhorse for early PC games.
/// The four-colour palette was a compile-time choice from two predefined
/// palettes plus a single background colour (e.g. palette 1 with cyan /
/// magenta / white / black). Because the pixels were displayed at roughly
/// 1:2 aspect and on low-bandwidth composite monitors, programmers used
/// vertically-paired 1×2 dither patterns: pixel (x, y) and pixel (x, y+1)
/// share the same threshold, producing slightly thicker horizontal grain
/// that survives the NTSC signal better than a full Bayer-4×4 would.
/// </para>
/// <para>
/// The dither itself is a 4×2 Bayer-style screen (8 cells, 8 thresholds) with
/// a vertical double-up so adjacent scan-lines agree on the threshold value.
/// The caller supplies a 4-entry palette (typical CGA palette 1: black, cyan,
/// magenta, white or the "brown fix" palette 0a: black, green, red, brown).
/// Larger palettes are truncated to the first four; smaller ones are used
/// as-is.
/// </para>
/// <para>
/// Artefact profile: classic "DOS-CGA" look with slightly thick horizontal
/// bands. On gradients the 4-colour restriction produces the iconic CGA
/// colour-mix impressions (cyan+magenta ≈ blue-grey, magenta+white ≈ pink).
/// </para>
/// <para>
/// References:
/// <a href="https://en.wikipedia.org/wiki/Color_Graphics_Adapter">Wikipedia:
/// Colour Graphics Adapter</a>;
/// <a href="https://int10h.org/blog/2015/04/cga-in-1024-colors-new-mode-the-illustrated-guide/">
/// int10h: CGA in 1024 colours</a> (historical dithering practice).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("CGA Mode 4", Description = "CGA Mode-4 4-colour palette with scan-line-paired Bayer dither", Type = DitheringType.Ordered)]
public readonly struct CgaMode4Ditherer : IDitherer {

  // Vertically-paired 4x2 Bayer-style screen: rows y and y+1 share thresholds
  // on even-y boundaries, producing the "double scanline" CGA look.
  private static readonly float[] _Screen = {
    -0.4375f, 0.0625f, -0.3125f, 0.1875f,
    -0.4375f, 0.0625f, -0.3125f, 0.1875f,
    0.4375f, -0.1875f, 0.3125f, -0.0625f,
    0.4375f, -0.1875f, 0.3125f, -0.0625f,
  };

  private const int _W = 4;

  /// <summary>Default instance.</summary>
  public static CgaMode4Ditherer Instance { get; } = new();

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

    var cap = palette.Length < 4 ? palette.Length : 4;
    var sub = new TWork[cap];
    for (var i = 0; i < cap; ++i)
      sub[i] = palette[i];
    var lookup = new PaletteLookup<TWork, TMetric>(sub, metric);
    var endY = startY + height;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = decoder.Decode(source[sourceIdx]);
      var (c1, c2, c3, a) = color.ToNormalized();
      var t = _Screen[(y & 3) * _W + (x & (_W - 1))];
      var adj = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1.ToFloat() + t),
        UNorm32.FromFloatClamped(c2.ToFloat() + t),
        UNorm32.FromFloatClamped(c3.ToFloat() + t),
        a);
      indices[targetIdx] = (byte)lookup.FindNearest(adj);
    }
  }
}
