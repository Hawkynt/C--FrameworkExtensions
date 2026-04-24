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
using System.Collections.Generic;
using System.Linq;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Perceptually-spread fixed palette using a golden-ratio hue sequence in OkLab-like polar
/// coordinates.
/// </summary>
/// <remarks>
/// <para>
/// Builds an adaptive palette of <c>k</c> colours by walking the Lab hue circle in increments of
/// <c>φ⁻¹ · 360°</c> (≈ 222.49°). This low-discrepancy sequence distributes hues maximally-
/// uniformly for any prefix length, so a 5-entry palette and a 16-entry palette both look
/// visually balanced.
/// </para>
/// <para>
/// Lightness cycles through three tiers (dark / mid / light) so the palette retains tonal
/// diversity rather than collapsing to one luma band. Chroma is held near a perceptually-vivid
/// constant. The resulting colours are emitted in sRGB after a standard Lab → RGB path.
/// </para>
/// <para>
/// Independent of the input image, this quantizer is classified as <see cref="QuantizationType.Fixed"/>
/// — it is ideal for categorical-data visualisations (charts, heatmaps) where perceptual hue
/// separation matters more than input-adaptivity.
/// </para>
/// <para>Reference: Martin Roberts (2018) — "The Unreasonable Effectiveness of Quasirandom
/// Sequences" (extra.sh); golden-ratio hue spacing is the 1-D specialisation of the <c>R₂</c>
/// Kronecker sequence.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Golden Ratio Palette", Author = "Roberts", Year = 2018, QualityRating = 5)]
public struct GoldenRatioPaletteQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private const double GoldenRatioConjugate = 0.6180339887498949; // 1/φ

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      if (colorCount <= 0)
        return [];

      var result = new TWork[colorCount];
      var hue = 0.0;
      for (var i = 0; i < colorCount; ++i) {
        // Low-discrepancy hue in [0, 1).
        hue = (hue + GoldenRatioConjugate) % 1.0;
        // Three-tier lightness cycle (~0.35 / 0.55 / 0.75) for tonal variety.
        var tier = i % 3;
        var lightness = 0.35 + 0.2 * tier;
        // Chroma held near Lab a/b range that corresponds to vivid but in-gamut sRGB.
        var chroma = 0.18;
        var theta = hue * 2.0 * Math.PI;
        var a = chroma * Math.Cos(theta);
        var b = chroma * Math.Sin(theta);
        // Lab → linear RGB (D65) approximation, then clamp.
        _LabToLinearRgb(lightness, a, b, out var lr, out var lg, out var lb);
        // Linearise → sRGB gamma.
        var r = _LinearToSrgb(lr);
        var g = _LinearToSrgb(lg);
        var bl = _LinearToSrgb(lb);
        result[i] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, r))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, g))),
          UNorm32.FromFloatClamped((float)Math.Max(0, Math.Min(1, bl))),
          UNorm32.One);
      }
      return result;
    }

    // Minimal D65 Lab→linear-RGB (CIE 1931 XYZ path). Not pipeline-accurate OkLab but sufficient
    // for a "visibly well-spread" palette; the palette is then rendered through the caller's
    // TWork conversion which re-projects to OkLab internally if applicable.
    private static void _LabToLinearRgb(double L, double a, double b, out double r, out double g, out double bl) {
      // L in [0,1] mapped to CIE L* in [0,100], a/b here are on a ~[-0.5, 0.5] scale — keep as is.
      var y = (L + 0.16) / 1.16;
      var x = a / 5.0 + y;
      var z = y - b / 2.0;
      double Finv(double t) => t > 6.0 / 29.0 ? t * t * t : 3.0 * (6.0 / 29.0) * (6.0 / 29.0) * (t - 4.0 / 29.0);
      var X = 0.95047 * Finv(x);
      var Y = 1.00000 * Finv(y);
      var Z = 1.08883 * Finv(z);
      r = 3.2406 * X - 1.5372 * Y - 0.4986 * Z;
      g = -0.9689 * X + 1.8758 * Y + 0.0415 * Z;
      bl = 0.0557 * X - 0.2040 * Y + 1.0570 * Z;
    }

    private static double _LinearToSrgb(double u) {
      if (u <= 0) return 0;
      if (u >= 1) return 1;
      return u <= 0.0031308 ? 12.92 * u : 1.055 * Math.Pow(u, 1.0 / 2.4) - 0.055;
    }
  }
}
