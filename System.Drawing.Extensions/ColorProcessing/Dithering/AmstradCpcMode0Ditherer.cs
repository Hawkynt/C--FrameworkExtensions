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
/// Amstrad CPC Mode-0 ordered dither — 160×200 effective resolution with
/// 16 on-screen colours and a 2×1 "fat-pixel" dither phase that mimics the
/// characteristic horizontally-stretched pixel shape of Mode 0.
/// </summary>
/// <remarks>
/// <para>
/// The Amstrad CPC's Mode 0 displays 16 colours at 160×200 (2:1 pixel aspect).
/// Because Mode 0 pixels were rendered as two CRT-scan-line-width horizontal
/// strips, programmers used dither patterns that treated adjacent horizontal
/// pixels as a single unit — per-pixel Bayer-style dither would produce an
/// unwanted vertical stripe on the composite output. This ditherer preserves
/// that convention: every two horizontally-adjacent pixels see the same
/// threshold value.
/// </para>
/// <para>
/// The underlying screen is an 8×8 Bayer matrix evaluated at the 2×1 "fat
/// pixel" granularity: <c>t(x, y) = Bayer8(x >> 1, y)</c>. The caller supplies
/// a 16-entry palette (typical CPC Mode 0 hardware palette selection from the
/// 27-colour master palette); anything beyond 16 is truncated, anything
/// shorter is used as-is.
/// </para>
/// <para>
/// Artefact profile: Bayer-8 dither with 2× horizontal cell doubling.
/// Gradients show characteristic CPC "fat-pixel" grain; flat regions look
/// like slightly blockier Bayer-8. 16 palette entries are more than enough
/// for a convincing colour image.
/// </para>
/// <para>
/// References:
/// <a href="https://www.cpcwiki.eu/index.php/Video_Display_Unit">CPCWiki:
/// Video Display Unit</a> (mode palette / pixel aspect);
/// <a href="https://en.wikipedia.org/wiki/Amstrad_CPC">Wikipedia: Amstrad
/// CPC</a>.
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Amstrad CPC Mode 0", Description = "Amstrad CPC Mode-0 16-colour palette with 2x1 fat-pixel Bayer-8 dither", Type = DitheringType.Ordered)]
public readonly struct AmstradCpcMode0Ditherer : IDitherer {

  private const int _BAYER = 8;
  private static readonly float[] _Bayer8 = _BuildBayer();

  /// <summary>Default instance.</summary>
  public static AmstradCpcMode0Ditherer Instance { get; } = new();

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
      // Fat-pixel: adjacent horizontal cells share threshold.
      var t = _Bayer8[(y & (_BAYER - 1)) * _BAYER + ((x >> 1) & (_BAYER - 1))];
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
