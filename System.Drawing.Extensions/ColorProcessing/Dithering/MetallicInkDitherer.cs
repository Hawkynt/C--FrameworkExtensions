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
/// Metallic-ink dithering — noise ditherer that uses a strongly anisotropic
/// noise kernel (long horizontal autocorrelation, short vertical) to simulate
/// the brushed / flaked appearance of metallic-ink printing such as bronze,
/// silver or pearlescent spot colours on packaging.
/// </summary>
/// <remarks>
/// <para>
/// Metallic inks contain aligned metal flakes suspended in a lacquer carrier.
/// When printed the flakes settle with their long axis parallel to the print
/// direction (the roller's rotation), producing a characteristic horizontal
/// streaking under raking light — the "brushed metal" look. Simulating this
/// as a noise ditherer requires anisotropic noise: the per-pixel threshold
/// should stay correlated over a ≈8-pixel horizontal window but decorrelate
/// between adjacent scan-lines.
/// </para>
/// <para>
/// The algorithm generates a base white-noise field from a seeded hash, then
/// convolves it with an asymmetric 1-D kernel along the horizontal axis only
/// (implemented on-the-fly by averaging the 4 horizontally-adjacent hashes —
/// no FFT, no pre-computed table, no error buffer). The vertical axis is
/// untouched so adjacent rows remain independent. The output resembles a
/// fine horizontal grain with a slight flicker under motion — exactly the
/// appearance a sheet of metallic-ink paper has.
/// </para>
/// <para>
/// Artefact profile: fine horizontal grain visible on flat regions, softer
/// than pure white noise, anisotropic. Works well for packaging mockups,
/// faux-foil effects, and "holographic" pixel-art styling. Parallel-friendly
/// — no per-row state.
/// </para>
/// <para>
/// References: the anisotropic convolution approach is a direct application
/// of the Gabor noise formulation in A. Lagae, P. Dutré 2011 "Gabor noise:
/// procedural texture synthesis by example", <i>ACM TOG</i> 30(6); metallic-
/// flake appearance modelling is well-covered in E. Reinhard et al.,
/// <i>High Dynamic Range Imaging</i>, Morgan Kaufmann 2010, §13 (physical
/// material models for printed output).
/// </para>
/// <para>Parallel-friendly (per-pixel operation, no sequential state). Deterministic.</para>
/// </remarks>
[Ditherer("Metallic Ink", Description = "Anisotropic noise dithering simulating brushed-metal ink grain", Type = DitheringType.Noise)]
public readonly struct MetallicInkDitherer : IDitherer {

  private readonly float _strength;
  private readonly int _seed;

  /// <summary>Default instance (strength 0.5, seed 42).</summary>
  public static MetallicInkDitherer Instance { get; } = new();

  /// <summary>Creates a metallic-ink ditherer.</summary>
  /// <param name="strength">Noise strength in [0, 1]. Default 0.5.</param>
  /// <param name="seed">RNG seed for reproducible grain. Default 42.</param>
  public MetallicInkDitherer(float strength = 0.5f, int seed = 42) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._seed = seed;
  }

  /// <summary>Returns this ditherer with the specified strength.</summary>
  public MetallicInkDitherer WithStrength(float strength) => new(strength, this._seed);

  /// <summary>Returns this ditherer with the specified seed.</summary>
  public MetallicInkDitherer WithSeed(int seed) => new(this._strength, seed);

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
    var strength = this._strength > 0 ? this._strength : 0.5f;
    var seed = this._seed;
    var endY = startY + height;

    for (var y = startY; y < endY; ++y)
    for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
      var color = decoder.Decode(source[sourceIdx]);
      var (c1, c2, c3, alpha) = color.ToNormalized();

      // Anisotropic noise: average 4 horizontally-adjacent white-noise
      // samples. This introduces a ≈4-pixel horizontal autocorrelation
      // while leaving vertical adjacency independent.
      var n0 = _Hash(x - 1, y, seed) & 0xFFFF;
      var n1 = _Hash(x, y, seed) & 0xFFFF;
      var n2 = _Hash(x + 1, y, seed) & 0xFFFF;
      var n3 = _Hash(x + 2, y, seed) & 0xFFFF;
      var avg = (n0 + n1 + n2 + n3) / (4f * 65536f);
      var noise = (avg - 0.5f) * strength;

      var adj = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(c1.ToFloat() + noise),
        UNorm32.FromFloatClamped(c2.ToFloat() + noise),
        UNorm32.FromFloatClamped(c3.ToFloat() + noise),
        alpha);

      indices[targetIdx] = (byte)lookup.FindNearest(adj);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Hash(int x, int y, int seed) {
    var h = seed;
    h ^= x * 374761393;
    h ^= y * 668265263;
    h = (h ^ (h >> 15)) * 1103515245;
    h ^= h >> 13;
    return h;
  }
}
