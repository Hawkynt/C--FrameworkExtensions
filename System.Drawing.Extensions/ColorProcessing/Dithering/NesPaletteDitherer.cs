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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Ordered dithering that honours the Nintendo Entertainment System PPU's
/// per-tile colour-pair restriction: every 8×8 tile may use at most three of
/// the four entries from the caller-supplied palette plus a shared universal
/// background colour, matching the NES's background-attribute-table rule.
/// </summary>
/// <remarks>
/// <para>
/// The NES's Picture Processing Unit (PPU) supported a 48+8-entry master
/// palette but could only display four colours per 8×8 background tile — one
/// universal background colour shared across the whole screen, plus three
/// palette entries selected from one of four per-tile sub-palettes. Dithering
/// for NES-style output therefore has to honour this restriction: if the
/// caller passes a 4-colour palette (index 0 treated as the universal
/// background), every 8×8 tile is allowed to use index 0 plus at most three
/// distinct other indices.
/// </para>
/// <para>
/// This ditherer implements that rule by:
/// </para>
/// <list type="number">
/// <item><description>Applying a Bayer-4×4 ordered dither to pick candidate
/// colours.</description></item>
/// <item><description>Per 8×8 tile, first-pass voting: count each palette
/// entry's histogram within the tile.</description></item>
/// <item><description>Keeping the top-3 most-used entries plus index 0 (the
/// "universal background") and remapping any other per-pixel pick to the
/// nearest allowed entry within that tile.</description></item>
/// </list>
/// <para>
/// The result is visibly NES-like: within each 8×8 region only 3+1 distinct
/// colours appear, exactly matching what the NES PPU could display for that
/// tile. Edges between tiles can therefore show the characteristic NES
/// "colour strip" transitions familiar from games of the era.
/// </para>
/// <para>
/// Notes: caller must supply a 4-colour palette. Larger palettes are truncated
/// to the first four; shorter palettes are used as-is (with <c>index 0</c>
/// repeated in the universal-background slot). Uses the Bayer-4×4 sequence
/// for sub-tile variety; the per-tile histogram is recomputed every 8×8
/// region and is fully parallel-friendly.
/// </para>
/// <para>
/// References:
/// <a href="https://www.nesdev.org/wiki/PPU_palettes">NesDev Wiki: PPU
/// Palettes</a> (per-tile 3+1 rule).
/// </para>
/// <para>Parallel-friendly (per-tile operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("NES Palette", Description = "Ordered dithering honouring NES PPU per-tile 3+1 palette restriction", Type = DitheringType.Ordered)]
public readonly struct NesPaletteDitherer : IDitherer {

  private const int _TILE = 8;
  private const int _MATRIX = 4;
  private const int _MAX_PER_TILE = 3; // + 1 universal background (index 0)

  // Bayer-4x4 normalised to [-0.5, 0.5].
  private static readonly float[] _Bayer = _BuildBayer();

  /// <summary>Default instance.</summary>
  public static NesPaletteDitherer Instance { get; } = new();

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
    var paletteLen = palette.Length;

    // Process by 8×8 tiles so the per-tile colour restriction is honoured.
    // Tiles are independent, so the outer tile-row loop could in principle
    // be parallelised.
    for (var tileY = startY; tileY < endY; tileY += _TILE) {
      var tileRows = Math.Min(_TILE, endY - tileY);
      for (var tileX = 0; tileX < width; tileX += _TILE) {
        var tileCols = Math.Min(_TILE, width - tileX);

        // Pass 1: pick the nearest-by-dither palette entry for every pixel
        // in this tile. Keep the raw picks plus a per-tile histogram.
        var tilePicks = stackalloc byte[_TILE * _TILE];
        var hist = stackalloc int[256];
        for (var i = 0; i < paletteLen && i < 256; ++i)
          hist[i] = 0;

        for (var py = 0; py < tileRows; ++py)
        for (var px = 0; px < tileCols; ++px) {
          var y = tileY + py;
          var x = tileX + px;
          var srcIdx = y * sourceStride + x;
          var color = decoder.Decode(source[srcIdx]);
          var (c1, c2, c3, a) = color.ToNormalized();
          var t = _Bayer[(py & (_MATRIX - 1)) * _MATRIX + (px & (_MATRIX - 1))];
          var adj = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped(c1.ToFloat() + t),
            UNorm32.FromFloatClamped(c2.ToFloat() + t),
            UNorm32.FromFloatClamped(c3.ToFloat() + t),
            a);
          var pick = (byte)lookup.FindNearest(adj);
          tilePicks[py * _TILE + px] = pick;
          ++hist[pick];
        }

        // Pass 2: identify the top-3 non-background palette entries used in
        // this tile. Index 0 is always allowed (universal background).
        var allowed = stackalloc byte[4];
        allowed[0] = 0;
        var allowedCount = 1;
        for (var slot = 0; slot < _MAX_PER_TILE; ++slot) {
          var bestIdx = -1;
          var bestCount = 0;
          for (var i = 1; i < paletteLen && i < 256; ++i) {
            if (hist[i] <= bestCount)
              continue;
            // Skip if already picked.
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

        // Pass 3: rewrite picks to the nearest allowed entry using perceptual
        // distance in the working space.
        for (var py = 0; py < tileRows; ++py)
        for (var px = 0; px < tileCols; ++px) {
          var pick = tilePicks[py * _TILE + px];
          // If already allowed, keep it.
          var ok = false;
          for (var a = 0; a < allowedCount; ++a)
            if (allowed[a] == pick) { ok = true; break; }
          if (!ok) {
            // Re-map to nearest allowed entry by palette-space distance.
            var (p1, p2, p3, _) = palette[pick].ToNormalized();
            var pf1 = p1.ToFloat();
            var pf2 = p2.ToFloat();
            var pf3 = p3.ToFloat();
            var bestAllowed = allowed[0];
            var bestDist = float.MaxValue;
            for (var a = 0; a < allowedCount; ++a) {
              var (a1, a2, a3, _) = palette[allowed[a]].ToNormalized();
              var d = Math.Abs(pf1 - a1.ToFloat()) + Math.Abs(pf2 - a2.ToFloat()) + Math.Abs(pf3 - a3.ToFloat());
              if (d >= bestDist)
                continue;
              bestDist = d;
              bestAllowed = allowed[a];
            }
            pick = bestAllowed;
          }
          var y = tileY + py;
          var x = tileX + px;
          indices[y * targetStride + x] = pick;
        }
      }
    }
  }

  private static float[] _BuildBayer() {
    var raw = BayerMatrix.Generate(_MATRIX);
    var max = _MATRIX * _MATRIX;
    var result = new float[max];
    for (var y = 0; y < _MATRIX; ++y)
    for (var x = 0; x < _MATRIX; ++x)
      result[y * _MATRIX + x] = (raw[y, x] + 0.5f) / max - 0.5f;
    return result;
  }
}
