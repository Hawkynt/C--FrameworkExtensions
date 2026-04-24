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
/// Ordered dithering tuned for the original Nintendo Game Boy (DMG-01)
/// display's four-shade green-tinted LCD: uses a Bayer-2×2 screen to
/// distribute the caller's 4-entry palette in the classic "gameboy shimmer"
/// grain, approximating what on-cart dithering produced on the real hardware.
/// </summary>
/// <remarks>
/// <para>
/// The Game Boy's DMG LCD could show exactly four shades at a time, wired from
/// two bits per pixel. Many commercial Game Boy games used on-cart dithering
/// with a Bayer-2×2 screen because (a) the PPU had no native blending,
/// (b) the 2×2 pattern hid well behind the LCD's slow pixel response, and
/// (c) 2×2 is trivial to unroll with the Z80-derived CPU's bit-math
/// instructions. This ditherer emulates that pipeline:
/// </para>
/// <list type="bullet">
/// <item><description>Caller passes a 4-entry palette (typically the DMG
/// greens: <c>#9BBC0F</c>, <c>#8BAC0F</c>, <c>#306230</c>, <c>#0F380F</c>).
/// Longer palettes are tolerated — only the first four entries are used.
/// </description></item>
/// <item><description>A Bayer-2×2 screen is applied per pixel, then the
/// lookup is quantised to the four-level palette — any other entries are
/// never picked.</description></item>
/// <item><description>The characteristic Game Boy "checkerboard on gradients"
/// grain emerges naturally from the 2×2 Bayer phase.</description></item>
/// </list>
/// <para>
/// Artefact profile: visibly chunky, 4-level staircased, with Bayer-2 grain
/// on every transition. Distinctive retro-handheld look. Pair with a
/// green-tinted palette for the iconic DMG LCD appearance; pair with neutral
/// greys for Super Game Boy / Game Boy Pocket aesthetics.
/// </para>
/// <para>
/// References:
/// <a href="https://gbdev.io/pandocs/Graphics.html">Pandocs: Game Boy
/// Graphics</a>; pixel aesthetics well-documented in GB homebrew community
/// (<a href="https://b13rg.github.io/Gameboy-Color-Palette-Examples/">DMG
/// colour palette examples</a>).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Game Boy", Description = "Bayer-2x2 ordered dither restricted to the caller's first 4 palette entries (DMG LCD style)", Type = DitheringType.Ordered)]
public readonly struct GameBoyDitherer : IDitherer {

  // Bayer-2x2 threshold values normalised to [-0.5, 0.5].
  private static readonly float[] _Bayer2 = { -0.375f, 0.125f, 0.375f, -0.125f };

  /// <summary>Default instance.</summary>
  public static GameBoyDitherer Instance { get; } = new();

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

    // Use only the first 4 palette entries to enforce the 4-shade restriction.
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
      var threshold = _Bayer2[(y & 1) * 2 + (x & 1)];
      var adj = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1.ToFloat() + threshold),
        UNorm32.FromFloatClamped(c2.ToFloat() + threshold),
        UNorm32.FromFloatClamped(c3.ToFloat() + threshold),
        a);
      indices[targetIdx] = (byte)lookup.FindNearest(adj);
    }
  }
}
