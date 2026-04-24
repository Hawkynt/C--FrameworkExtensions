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
/// MSX SCREEN 2 ordered dither — honours the TMS9918/9928 VDP's "two colours
/// per 8×1 row" rule: within every 8-pixel horizontal row, at most two
/// distinct palette entries may appear. Emulates the classic "colour-clash"
/// behaviour seen in MSX1 / ColecoVision / SG-1000 games.
/// </summary>
/// <remarks>
/// <para>
/// The Texas Instruments TMS9918/9928 VDP used in the MSX-1 family stored
/// pattern + colour tables such that every 8×1 row within an 8×8 tile could
/// only use two colours: one "foreground" bit-1 colour and one "background"
/// bit-0 colour. Dithering for this target therefore has to pick two palette
/// entries per 8×1 row and binary-assign each pixel to one of them — a very
/// restrictive but visually distinctive constraint that gave MSX-1 its
/// characteristic horizontally-striped colour appearance.
/// </para>
/// <para>
/// This ditherer implements the rule by:
/// </para>
/// <list type="number">
/// <item><description>For every 8×1 row (y fixed, x = 8k..8k+7), find the two
/// palette entries whose combined quantization error is minimised — a cheap
/// exhaustive pairwise scan over the first 16 palette entries (MSX-1's
/// master palette size).</description></item>
/// <item><description>Apply a Bayer-4 threshold within the row to pick
/// between the two chosen entries, giving mid-tones.</description></item>
/// </list>
/// <para>
/// Artefact profile: the characteristic MSX-1 "horizontal colour stripe" look.
/// Very distinctive. 2 colours per 8-pixel row means long horizontal
/// gradients can span many colour-pair transitions, producing visible
/// quantization bands at every row boundary — this is authentic hardware
/// behaviour, not a bug.
/// </para>
/// <para>
/// References:
/// <a href="https://www.msx.org/wiki/SCREEN_2">MSX Wiki: SCREEN 2</a>;
/// <a href="https://en.wikipedia.org/wiki/TMS9918">Wikipedia: TMS9918</a>.
/// </para>
/// <para>Parallel-friendly (per-row operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("MSX Screen 2", Description = "MSX SCREEN 2 / TMS9918 per-8x1-row 2-colour-pair restriction", Type = DitheringType.Ordered)]
public readonly struct MsxScreen2Ditherer : IDitherer {

  private const int _ROW_WIDTH = 8;

  // Bayer-4x1 values across the 8-pixel row, normalised to [-0.5, 0.5].
  // Flat per-row pattern: 4-level threshold repeated twice.
  private static readonly float[] _Row = { -0.4375f, 0.0625f, -0.3125f, 0.1875f, -0.4375f, 0.0625f, -0.3125f, 0.1875f };

  /// <summary>Default instance.</summary>
  public static MsxScreen2Ditherer Instance { get; } = new();

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

    // MSX-1 master palette has 16 entries; accept up to that many.
    var cap = palette.Length < 16 ? palette.Length : 16;
    var endY = startY + height;

    // Process each 8×1 row independently. Choosing the optimal colour-pair
    // per row is the work that makes the algorithm distinctive.
    for (var y = startY; y < endY; ++y)
    for (var rowX = 0; rowX < width; rowX += _ROW_WIDTH) {
      var rowEnd = Math.Min(rowX + _ROW_WIDTH, width);

      // Decode the pixels of this row once into a tiny stack scratch buffer.
      var pixels = stackalloc float[_ROW_WIDTH * 3];
      var count = rowEnd - rowX;
      for (var i = 0; i < count; ++i) {
        var color = decoder.Decode(source[y * sourceStride + rowX + i]);
        var (c1, c2, c3, _) = color.ToNormalized();
        pixels[i * 3] = c1.ToFloat();
        pixels[i * 3 + 1] = c2.ToFloat();
        pixels[i * 3 + 2] = c3.ToFloat();
      }

      // Exhaustive best-pair search: O(cap² · count) with cap ≤ 16 and
      // count ≤ 8 → at most 1024 ops per row, vastly cheaper than a
      // per-pixel palette lookup.
      var bestA = 0;
      var bestB = cap > 1 ? 1 : 0;
      var bestErr = double.MaxValue;
      for (var a = 0; a < cap; ++a)
      for (var b = a; b < cap; ++b) {
        var (a1, a2, a3, _) = palette[a].ToNormalized();
        var (b1, b2, b3, _) = palette[b].ToNormalized();
        var af1 = a1.ToFloat(); var af2 = a2.ToFloat(); var af3 = a3.ToFloat();
        var bf1 = b1.ToFloat(); var bf2 = b2.ToFloat(); var bf3 = b3.ToFloat();
        var err = 0.0;
        for (var i = 0; i < count; ++i) {
          var px1 = pixels[i * 3]; var px2 = pixels[i * 3 + 1]; var px3 = pixels[i * 3 + 2];
          var da = (px1 - af1) * (px1 - af1) + (px2 - af2) * (px2 - af2) + (px3 - af3) * (px3 - af3);
          var db = (px1 - bf1) * (px1 - bf1) + (px2 - bf2) * (px2 - bf2) + (px3 - bf3) * (px3 - bf3);
          err += da < db ? da : db;
        }
        if (err >= bestErr)
          continue;
        bestErr = err;
        bestA = a;
        bestB = b;
      }

      // Binary-assign each pixel to either bestA or bestB using a 1-D Bayer
      // threshold applied to the pixel's projection onto the A-B line.
      var (ba1, ba2, ba3, _1) = palette[bestA].ToNormalized();
      var (bb1, bb2, bb3, _2) = palette[bestB].ToNormalized();
      var baf1 = ba1.ToFloat(); var baf2 = ba2.ToFloat(); var baf3 = ba3.ToFloat();
      var bbf1 = bb1.ToFloat(); var bbf2 = bb2.ToFloat(); var bbf3 = bb3.ToFloat();
      var dirX = bbf1 - baf1; var dirY = bbf2 - baf2; var dirZ = bbf3 - baf3;
      var norm = (float)Math.Sqrt(dirX * dirX + dirY * dirY + dirZ * dirZ);
      if (norm < 1e-6f) norm = 1f;
      dirX /= norm; dirY /= norm; dirZ /= norm;

      for (var i = 0; i < count; ++i) {
        var px1 = pixels[i * 3]; var px2 = pixels[i * 3 + 1]; var px3 = pixels[i * 3 + 2];
        // Project pixel onto A-B axis, get parameter t ∈ [0, 1].
        var relX = px1 - baf1; var relY = px2 - baf2; var relZ = px3 - baf3;
        var t = relX * dirX + relY * dirY + relZ * dirZ;
        t = norm > 1e-6f ? t / norm : 0f;
        t = Math.Max(0f, Math.Min(1f, t));
        // Bayer threshold flips the decision at t == 0.5.
        t += _Row[(rowX + i) & 7];
        var pick = t > 0.5f ? bestB : bestA;
        indices[y * targetStride + rowX + i] = (byte)pick;
      }
    }
  }
}
