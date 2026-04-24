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
/// Pico-8 ordered dither — applies a 4×4 Bayer pattern and restricts output
/// to the caller's first 16 palette entries, matching the Pico-8 fantasy
/// console's fixed 16-colour palette and software-rendered dither aesthetic.
/// </summary>
/// <remarks>
/// <para>
/// Lexaloffle's Pico-8 virtual fantasy console defines a fixed 16-colour
/// palette (<c>0..15</c>) as part of its hardware specification; all graphics
/// must use entries from this palette and no others. The Pico-8 runtime
/// natively supports a "fillp" (fill pattern) feature that uses a 4×4
/// Bayer-like pattern to select between two palette entries per pixel —
/// community style guides recommend mapping gradients through fillp instead
/// of adding new colours to the master palette.
/// </para>
/// <para>
/// This ditherer emulates that style by:
/// </para>
/// <list type="bullet">
/// <item><description>Capping output to the first 16 palette entries of the
/// caller's palette (Pico-8's master palette size).</description></item>
/// <item><description>Applying a 4×4 Bayer threshold (matching the Pico-8
/// fillp screen granularity).</description></item>
/// </list>
/// <para>
/// Artefact profile: unmistakably "Pico-8-y" — blocky 4×4 dither on gradients
/// between the 16 hard-coded palette entries. Works well when the caller
/// provides the real Pico-8 palette (<c>#000000</c>, <c>#1d2b53</c>,
/// <c>#7e2553</c>, <c>#008751</c>, <c>#ab5236</c>, <c>#5f574f</c>,
/// <c>#c2c3c7</c>, <c>#fff1e8</c>, <c>#ff004d</c>, <c>#ffa300</c>,
/// <c>#ffec27</c>, <c>#00e436</c>, <c>#29adff</c>, <c>#83769c</c>,
/// <c>#ff77a8</c>, <c>#ffccaa</c>).
/// </para>
/// <para>
/// References:
/// <a href="https://www.lexaloffle.com/pico-8.php">Lexaloffle: Pico-8
/// Manual</a>; palette reference at
/// <a href="https://pico-8.fandom.com/wiki/Palette">pico-8.fandom.com</a>.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Pico-8", Description = "Pico-8 fantasy console 16-colour palette with 4x4 Bayer fill-pattern dither", Type = DitheringType.Ordered)]
public readonly struct Pico8Ditherer : IDitherer {

  private const int _BAYER = 4;
  private static readonly float[] _Bayer4 = _BuildBayer();

  /// <summary>Default instance.</summary>
  public static Pico8Ditherer Instance { get; } = new();

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

    var cap = palette.Length < 16 ? palette.Length : 16;
    var sub = new TWork[cap];
    for (var i = 0; i < cap; ++i)
      sub[i] = palette[i];
    var lookup = new PaletteLookup<TWork, TMetric>(sub, metric);
    var endY = startY + height;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = decoder.Decode(source[sourceIdx]);
      var (c1, c2, c3, a) = color.ToNormalized();
      var t = _Bayer4[(y & (_BAYER - 1)) * _BAYER + (x & (_BAYER - 1))];
      var adj = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1.ToFloat() + t),
        UNorm32.FromFloatClamped(c2.ToFloat() + t),
        UNorm32.FromFloatClamped(c3.ToFloat() + t),
        a);
      indices[targetIdx] = (byte)lookup.FindNearest(adj);
    }
  }

  private static float[] _BuildBayer() {
    var raw = BayerMatrix.Generate(_BAYER);
    var max = _BAYER * _BAYER;
    var result = new float[max];
    for (var y = 0; y < _BAYER; ++y)
    for (var x = 0; x < _BAYER; ++x)
      result[y * _BAYER + x] = (raw[y, x] + 0.5f) / max - 0.5f;
    return result;
  }
}
