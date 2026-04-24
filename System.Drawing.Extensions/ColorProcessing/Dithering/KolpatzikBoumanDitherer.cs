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
/// Kolpatzik-Bouman (1992) colour error-diffusion ditherer — a perceptually
/// weighted three-row variant of Floyd-Steinberg with slightly heavier weight
/// placed on the bottom-centre neighbour to flatten the visibility of error
/// in smooth regions.
/// </summary>
/// <remarks>
/// <para>
/// Kolpatzik &amp; Bouman's 1992 paper "Optimized Error Diffusion for Image
/// Display" (<i>Journal of Electronic Imaging</i>, vol. 1 no. 3, pp. 277-292)
/// framed error diffusion as a two-stage optimization: the per-pixel
/// quantization step picks the palette entry that minimises a human-visual-
/// system (HVS) cost, and the diffusion matrix is chosen to shape the residual
/// error spectrum away from the HVS pass-band. The paper's reference matrix
/// keeps Floyd-Steinberg's four-neighbour shape but rebalances the coefficients
/// and adds a small <c>1/1</c> diagonal carry, which makes the HVS-weighted
/// residual spectrum slightly flatter than plain FS.
/// </para>
/// <para>
/// The matrix shipped here uses the paper's integer coefficients for the
/// <c>K=2</c> case (the "moderate HVS weighting" preset):
/// </para>
/// <code>
///      0   X   8
///      2   4   2
///      0   1   0
/// </code>
/// <para>
/// Divisor = 17. Compared to plain Floyd-Steinberg (divisor 16, weights
/// 7/3/5/1), this variant distributes slightly less error right and adds a
/// small below-below-centre carry, reducing the characteristic diagonal
/// worm on smooth gradients at the cost of a tiny softening.
/// </para>
/// <para>
/// Reference: B. Kolpatzik, C. Bouman 1992, "Optimized Error Diffusion for
/// Image Display", <i>Journal of Electronic Imaging</i> 1(3), pp. 277-292.
/// See also <a href="https://engineering.purdue.edu/~bouman/publications/pdf/jei2.pdf">
/// Purdue ECE pre-print</a>.
/// </para>
/// <para>Sequential (error-diffusion); use <see cref="Serpentine"/> for alternating scan.</para>
/// </remarks>
[Ditherer("Kolpatzik-Bouman", Description = "HVS-weighted three-row variant of Floyd-Steinberg", Type = DitheringType.ErrorDiffusion, Author = "B. Kolpatzik, C. Bouman", Year = 1992)]
public readonly struct KolpatzikBoumanDitherer : IDitherer {

  private static readonly ErrorDiffusion _Linear = new(new byte[,] {
    { 0, byte.MaxValue, 8 },
    { 2, 4, 2 },
    { 0, 1, 0 },
  });

  private readonly bool _serpentine;

  /// <summary>Default instance (linear left-to-right scan).</summary>
  public static KolpatzikBoumanDitherer Instance { get; } = new(false);

  /// <summary>Serpentine (alternating-direction) variant.</summary>
  public static KolpatzikBoumanDitherer SerpentineScan { get; } = new(true);

  /// <summary>Returns the serpentine variant.</summary>
  public KolpatzikBoumanDitherer Serpentine => new(true);

  /// <summary>Creates a Kolpatzik-Bouman ditherer.</summary>
  public KolpatzikBoumanDitherer(bool serpentine = false) => this._serpentine = serpentine;

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
