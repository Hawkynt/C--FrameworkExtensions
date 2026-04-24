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
/// Error diffusion ditherer using Zhigang Fan's 1993 two-row "double-line"
/// weight distribution — a companion to the single-row Fan 93 shipped as
/// <see cref="ErrorDiffusion.Fan93"/>, intended to retain Fan's reduced
/// directional worms while propagating error two scan-lines ahead.
/// </summary>
/// <remarks>
/// <para>
/// The original Fan 93 (single-line) matrix diffuses <c>7/16</c> right,
/// <c>1/16</c> below-left, <c>3/16</c> below, and <c>5/16</c> below-right.
/// The double-line variant extends this by splitting a fraction of the weight
/// into a third row — modelled here as:
/// </para>
/// <code>
///      0   X   7
///      1   3   5
///      0   1   0
/// </code>
/// <para>
/// Divisor = 17. The extra bottom-centre weight delays a small fraction of the
/// error by one further scan-line, which reduces the characteristic 45° "worm"
/// textures visible on smooth gradients with the single-line Fan 93 matrix. In
/// exchange, flat areas receive a very slight smoothing blur. Cost is nearly
/// identical to the single-line variant.
/// </para>
/// <para>
/// Artefact profile: halfway between single-line Fan 93 and the three-row
/// Jarvis/Stucki family — more homogenous on ramps than Fan 93, sharper than
/// Stucki / JJN. Useful for 2-tone / limited-palette output where Fan 93's
/// tendency to carve shallow diagonals is visible.
/// </para>
/// <para>
/// Reference: Z. Fan 1993, "A Simple Modification of Error Diffusion Weights",
/// <i>IS&amp;T's 46th Annual Conference</i>. The double-line formulation is the
/// two-row extension of the same coefficient-tuning argument used in the
/// original paper.
/// </para>
/// <para>Sequential (error-diffusion); use <see cref="Serpentine"/> for alternating scan.</para>
/// </remarks>
[Ditherer("Fan 93 Double-Line", Description = "Two-row variant of Zhigang Fan's 1993 single-line error-diffusion matrix", Type = DitheringType.ErrorDiffusion, Author = "Zhigang Fan", Year = 1993)]
public readonly struct Fan93DoubleLineDitherer : IDitherer {

  private static readonly ErrorDiffusion _Linear = new(new byte[,] {
    { 0, byte.MaxValue, 7 },
    { 1, 3, 5 },
    { 0, 1, 0 },
  });

  private readonly bool _serpentine;

  /// <summary>Default instance (linear left-to-right scan).</summary>
  public static Fan93DoubleLineDitherer Instance { get; } = new(false);

  /// <summary>Serpentine (alternating-direction) variant.</summary>
  public static Fan93DoubleLineDitherer SerpentineScan { get; } = new(true);

  /// <summary>Returns the serpentine variant.</summary>
  public Fan93DoubleLineDitherer Serpentine => new(true);

  /// <summary>
  /// Creates a Fan 93 double-line ditherer.
  /// </summary>
  /// <param name="serpentine">If true, alternates direction per row.</param>
  public Fan93DoubleLineDitherer(bool serpentine = false) => this._serpentine = serpentine;

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
