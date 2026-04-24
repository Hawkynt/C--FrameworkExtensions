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
/// Atari 2600 playfield ordered dither — restricts every scan-line to at most
/// four distinct palette entries, matching the TIA chip's playfield + two
/// sprite colour registers that had to be re-written via 6502 inline
/// "racing-the-beam" code every ≈76 CPU cycles per scan-line.
/// </summary>
/// <remarks>
/// <para>
/// The Atari 2600's TIA (Television Interface Adapter) had no frame buffer
/// and no per-pixel colour control: colours were set in registers that the
/// 6502 CPU re-wrote in sync with the CRT horizontal retrace. In practice a
/// programmer could re-colour the playfield plus two missiles plus one ball
/// plus two 8-pixel sprites up to a few times per scan-line, but keeping the
/// effective per-scan-line colour count under four was the standard trick
/// for avoiding the "racing the beam" timing bugs that cost late Atari 2600
/// games so many hours of assembly debugging.
/// </para>
/// <para>
/// This ditherer emulates the per-scan-line 4-colour budget by:
/// </para>
/// <list type="number">
/// <item><description>Taking histogram of the caller's palette usage over the
/// current scan-line.</description></item>
/// <item><description>Keeping the top-4 most-used entries, remapping the rest
/// to the nearest allowed entry.</description></item>
/// <item><description>Applying a horizontally-stretched Bayer-1×4 pattern
/// for intra-row variety (the Atari 2600's horizontal resolution of 160
/// pixels was effectively fat-pixeled 2:1 vs. the vertical ≈192 lines, so
/// horizontal dither at 1:1 is already large-scale by the CRT standards
/// of 1977).</description></item>
/// </list>
/// <para>
/// Artefact profile: deeply retro. Colour clashes happen per-scan-line, not
/// per-tile, so long vertical gradients tend to remain coherent while
/// horizontally-detailed content shows visible per-line colour jumps — the
/// characteristic "wiggle" of TIA-driven games.
/// </para>
/// <para>
/// References:
/// <a href="https://en.wikipedia.org/wiki/Television_Interface_Adaptor">
/// Wikipedia: TIA</a>; S. Wright, <i>Racing the Beam: The Atari Video
/// Computer System</i>, MIT Press 2009.
/// </para>
/// <para>Parallel-friendly (per-row operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Atari 2600 Playfield", Description = "Atari 2600 TIA per-scanline 4-colour restriction with 1x4 Bayer dither", Type = DitheringType.Ordered)]
public readonly struct Atari2600PlayfieldDitherer : IDitherer {

  private const int _PER_SCANLINE = 4;
  private const int _BAYER = 4;

  // Bayer-4x1 row pattern normalised to [-0.5, 0.5].
  private static readonly float[] _Row = { -0.4375f, 0.0625f, -0.3125f, 0.1875f };

  /// <summary>Default instance.</summary>
  public static Atari2600PlayfieldDitherer Instance { get; } = new();

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
    var paletteLen = palette.Length;
    var endY = startY + height;

    // Heap-allocated scratch buffers, reused across rows.
    var picksArr = new byte[width];
    var histArr = new int[256];
    fixed (byte* picks = picksArr)
    fixed (int* hist = histArr)
    for (var y = startY; y < endY; ++y) {
      // Pass 1: pick the nearest-by-dither palette entry for every pixel in
      // this row; accumulate the per-row histogram.
      for (var i = 0; i < 256; ++i) hist[i] = 0;
      var capped = width;

      for (var x = 0; x < capped; ++x) {
        var color = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, a) = color.ToNormalized();
        var t = _Row[x & (_BAYER - 1)];
        var adj = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(c1.ToFloat() + t),
          UNorm32.FromFloatClamped(c2.ToFloat() + t),
          UNorm32.FromFloatClamped(c3.ToFloat() + t),
          a);
        var pick = (byte)lookup.FindNearest(adj);
        picks[x] = pick;
        ++hist[pick];
      }

      // Pass 2: identify the top-4 palette entries for this scanline.
      var allowed = stackalloc byte[_PER_SCANLINE];
      var allowedCount = 0;
      for (var slot = 0; slot < _PER_SCANLINE; ++slot) {
        var bestIdx = -1;
        var bestCount = 0;
        for (var i = 0; i < paletteLen && i < 256; ++i) {
          if (hist[i] <= bestCount)
            continue;
          var already = false;
          for (var a = 0; a < allowedCount; ++a)
            if (allowed[a] == i) { already = true; break; }
          if (already)
            continue;
          bestIdx = i;
          bestCount = hist[i];
        }
        if (bestIdx < 0)
          break;
        allowed[allowedCount++] = (byte)bestIdx;
      }

      // Fallback if palette was empty / trivial.
      if (allowedCount == 0) {
        for (var x = 0; x < capped; ++x)
          indices[y * targetStride + x] = 0;
        continue;
      }

      // Pass 3: write out, remapping disallowed picks to nearest allowed.
      for (var x = 0; x < capped; ++x) {
        var pick = picks[x];
        var ok = false;
        for (var a = 0; a < allowedCount; ++a)
          if (allowed[a] == pick) { ok = true; break; }
        if (!ok) {
          var (p1, p2, p3, _) = palette[pick].ToNormalized();
          var pf1 = p1.ToFloat(); var pf2 = p2.ToFloat(); var pf3 = p3.ToFloat();
          var bestAllowed = allowed[0];
          var bestDist = float.MaxValue;
          for (var a = 0; a < allowedCount; ++a) {
            var (a1, a2, a3, _x) = palette[allowed[a]].ToNormalized();
            var d = System.Math.Abs(pf1 - a1.ToFloat()) + System.Math.Abs(pf2 - a2.ToFloat()) + System.Math.Abs(pf3 - a3.ToFloat());
            if (d >= bestDist)
              continue;
            bestDist = d;
            bestAllowed = allowed[a];
          }
          pick = bestAllowed;
        }
        indices[y * targetStride + x] = pick;
      }
    }
  }
}
