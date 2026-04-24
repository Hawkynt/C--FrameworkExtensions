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
/// Ulichney's diagonal-weighted variant of Floyd-Steinberg — retains FS's
/// 4-neighbour shape but shifts weight toward the two diagonal neighbours to
/// reduce the characteristic horizontal / vertical streaking FS produces on
/// near-flat regions.
/// </summary>
/// <remarks>
/// <para>
/// In <i>Digital Halftoning</i> (MIT Press 1987, §5.5), Robert Ulichney
/// analyses the residual-error spectrum of classical Floyd-Steinberg and
/// notes that the <c>7/3/5/1</c> weighting inherently favours horizontal
/// error propagation, producing the "curdling" effect visible on smooth
/// greyscale ramps. He proposes re-weighting the matrix with heavier
/// diagonal coefficients, which spreads the residual energy more evenly
/// across the two-dimensional spectrum. The integer-friendly version of his
/// Table 5.1 diagonal-weighted matrix is:
/// </para>
/// <code>
///      0   X   4
///      3   5   3
/// </code>
/// <para>
/// Divisor = 15. Compared to Floyd-Steinberg (4/16 right; 3/16, 5/16, 1/16
/// below), less error flows straight right and more flows to the two
/// diagonals, which visibly breaks up curdling on long gradients. Cost is
/// identical to FS — same 4 non-zero coefficients.
/// </para>
/// <para>
/// Reference: R. Ulichney 1987, <i>Digital Halftoning</i>, MIT Press, §5.5
/// "Error Diffusion". See also
/// <a href="https://people.cs.vt.edu/~ulichney/publications.html">Ulichney's
/// publication list</a>. The diagonal-weighted construction is the direct
/// precursor to the FS-Serpentine modification recommended later in the
/// same chapter.
/// </para>
/// <para>Sequential (error-diffusion); use <see cref="Serpentine"/> for alternating scan.</para>
/// </remarks>
[Ditherer("Ulichney Diagonal FS", Description = "Diagonal-weighted variant of Floyd-Steinberg that suppresses curdling on ramps", Type = DitheringType.ErrorDiffusion, Author = "Robert Ulichney", Year = 1987)]
public readonly struct UlichneyDiagonalFSDitherer : IDitherer {

  private static readonly ErrorDiffusion _Linear = new(new byte[,] {
    { 0, byte.MaxValue, 4 },
    { 3, 5, 3 },
  });

  private readonly bool _serpentine;

  /// <summary>Default instance (linear left-to-right scan).</summary>
  public static UlichneyDiagonalFSDitherer Instance { get; } = new(false);

  /// <summary>Serpentine (alternating-direction) variant.</summary>
  public static UlichneyDiagonalFSDitherer SerpentineScan { get; } = new(true);

  /// <summary>Returns the serpentine variant.</summary>
  public UlichneyDiagonalFSDitherer Serpentine => new(true);

  /// <summary>Creates an Ulichney diagonal FS ditherer.</summary>
  public UlichneyDiagonalFSDitherer(bool serpentine = false) => this._serpentine = serpentine;

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
