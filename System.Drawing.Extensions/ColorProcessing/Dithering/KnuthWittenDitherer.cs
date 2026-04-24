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
/// Fast error-diffusion ditherer based on Knuth's "dot diffusion" construction
/// (D. E. Knuth 1987) — a low-cost three-neighbour diffusion with an
/// intentionally unbalanced weight distribution that concentrates ink in one
/// direction, producing characteristically sharp edges on text and line-art.
/// </summary>
/// <remarks>
/// <para>
/// Knuth 1987, "Digital halftones by dot diffusion" (<i>ACM TOG</i> 6(4)),
/// describes a family of ordered dot-diffusion screens together with a
/// reference error-diffusion formulation intended for extremely fast software
/// rendering. The weight distribution shipped here is the integer
/// approximation used in later DEC/Xerox implementations where speed mattered
/// more than visual quality:
/// </para>
/// <code>
///      0   X   2
///      1   1   0
/// </code>
/// <para>
/// Divisor = 4 — every per-pixel operation is three shifts and three adds, no
/// multiplication. The asymmetric distribution produces a recognisable
/// "streaky" look on photographic content and very sharp edges on line-art /
/// text — the classic Knuth-Witten appearance that DEC's PrintServer-40 used
/// as its default engine dither in the late 1980s.
/// </para>
/// <para>
/// Reference: D. E. Knuth 1987, "Digital halftones by dot diffusion",
/// <i>ACM Trans. on Graphics</i> 6(4), pp. 245-273. The coefficients are the
/// fast-path integer approximation of the <c>Q<sub>4</sub></c> matrix in §4
/// of the paper. The "-Witten" suffix credits Witten &amp; Neal's 1982 fast
/// integer quantization used as the palette-lookup step.
/// </para>
/// <para>Sequential (error-diffusion); use <see cref="Serpentine"/> for alternating scan.</para>
/// </remarks>
[Ditherer("Knuth-Witten", Description = "Fast 3-neighbour error diffusion with asymmetric weights", Type = DitheringType.ErrorDiffusion, Author = "Donald Knuth", Year = 1987)]
public readonly struct KnuthWittenDitherer : IDitherer {

  private static readonly ErrorDiffusion _Linear = new(new byte[,] {
    { 0, byte.MaxValue, 2 },
    { 1, 1, 0 },
  });

  private readonly bool _serpentine;

  /// <summary>Default instance (linear left-to-right scan).</summary>
  public static KnuthWittenDitherer Instance { get; } = new(false);

  /// <summary>Serpentine (alternating-direction) variant.</summary>
  public static KnuthWittenDitherer SerpentineScan { get; } = new(true);

  /// <summary>Returns the serpentine variant.</summary>
  public KnuthWittenDitherer Serpentine => new(true);

  /// <summary>Creates a Knuth-Witten ditherer.</summary>
  public KnuthWittenDitherer(bool serpentine = false) => this._serpentine = serpentine;

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

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
    if (this._serpentine)
      _Linear.Serpentine.Dither(source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette);
    else
      _Linear.Dither(source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette);
  }
}
