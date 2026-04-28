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

using System.Collections.Generic;
using System.Linq;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

#region EGA 64

/// <summary>
/// Full EGA 64-colour master palette (6-bit RGBrgb addressable colours, 1984).
/// </summary>
/// <remarks>
/// <para>
/// The EGA card could simultaneously display 16 of its 64 hardware colours at any
/// time (via the attribute-to-colour lookup table). The existing
/// <see cref="Ega16Quantizer"/> ships the default 16-colour sub-palette; this
/// quantizer exposes the <i>full</i> 64-colour hardware gamut, encoded as two bits
/// per primary (RGBrgb): high bit contributes 0xAA, low bit contributes 0x55,
/// summed per channel.
/// </para>
/// <para>
/// Canonical reference: IBM Enhanced Graphics Adapter Hardware Reference, 1984;
/// see also "EGA_palette" at shikadi.net/moddingwiki.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "EGA 64", Year = 1984, QualityRating = 2)]
public struct Ega64Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      var result = new TWork[64];
      // 6-bit index: bits 5..0 = R' G' B' R G B (high / low-intensity). IBM docs: R+r = 0xAA+0x55=0xFF.
      for (var idx = 0; idx < 64; ++idx) {
        var rH = (idx >> 5) & 1; var gH = (idx >> 4) & 1; var bH = (idx >> 3) & 1;
        var rL = (idx >> 2) & 1; var gL = (idx >> 1) & 1; var bL = (idx >> 0) & 1;
        var r = (byte)(rH * 0xAA + rL * 0x55);
        var g = (byte)(gH * 0xAA + gL * 0x55);
        var b = (byte)(bH * 0xAA + bL * 0x55);
        result[idx] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(r), UNorm32.FromByte(g), UNorm32.FromByte(b), UNorm32.One);
      }
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => colorCount >= _palette.Length ? _palette : _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region Master System 64

/// <summary>
/// Sega Master System / Game Gear VDP 64-colour palette (1985).
/// </summary>
/// <remarks>
/// <para>
/// The SMS's VDP used 2 bits per R/G/B channel (4×4×4 = 64 colours). This is the
/// hardware master set; software could simultaneously display 32 colours (one
/// background palette of 16 plus one sprite palette of 16, each chosen from the
/// 64-colour master). Values are the BlueMSX/Meka canonical RGB translation:
/// each channel scaled as <c>c · 85</c> (0, 85, 170, 255).
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Master System", Year = 1985, QualityRating = 2)]
public struct MasterSystemQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      var result = new TWork[64];
      var idx = 0;
      for (var r = 0; r < 4; ++r)
        for (var g = 0; g < 4; ++g)
          for (var b = 0; b < 4; ++b)
            result[idx++] = ColorFactory.FromNormalized_4<TWork>(
              UNorm32.FromByte((byte)(r * 85)),
              UNorm32.FromByte((byte)(g * 85)),
              UNorm32.FromByte((byte)(b * 85)),
              UNorm32.One);
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => colorCount >= _palette.Length ? _palette : _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region MSX-2

/// <summary>
/// MSX-2 V9938 VDP 256-colour palette (1985).
/// </summary>
/// <remarks>
/// <para>
/// The V9938 "Screen 8" mode used a fixed 256-colour palette with 3 bits red,
/// 3 bits green and 2 bits blue (8×8×4 = 256). Note the reduced blue resolution —
/// the V9938 designer (Nishi Kazuhiko at ASCII Corp.) favoured red/green
/// resolution because early NTSC TVs had poorer chroma response to blue. This
/// is distinct from the RGB-332 layout (which pairs 3-3-2 differently).
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "MSX-2", Year = 1985, QualityRating = 2)]
public struct Msx2Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      var result = new TWork[256];
      var idx = 0;
      // Screen 8 colour index: GGGRRRBB.
      for (var g = 0; g < 8; ++g)
        for (var r = 0; r < 8; ++r)
          for (var b = 0; b < 4; ++b)
            result[idx++] = ColorFactory.FromNormalized_4<TWork>(
              UNorm32.FromByte((byte)(r * 255 / 7)),
              UNorm32.FromByte((byte)(g * 255 / 7)),
              UNorm32.FromByte((byte)(b * 255 / 3)),
              UNorm32.One);
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => colorCount >= _palette.Length ? _palette : _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region Commodore Plus/4

/// <summary>
/// Commodore Plus/4 / C16 TED 121-colour palette (1984).
/// </summary>
/// <remarks>
/// <para>
/// The TED (7360) chip produced 121 on-screen colours from a 15-hue × 8-luminance
/// grid plus a pure black entry (1 + 15 × 8 = 121). Values approximate the
/// "YAPE-accurate" RGB translation used by the Plus/4 emulation community.
/// Distinct from the existing <see cref="C64Quantizer"/> (VIC-II 16-colour
/// palette): Plus/4 has a much richer gamut than the C64 it was briefly meant
/// to replace.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Commodore Plus/4", Year = 1984, QualityRating = 2)]
public struct CommodorePlus4Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      var result = new TWork[121];
      result[0] = ColorFactory.FromNormalized_4<TWork>(UNorm32.Zero, UNorm32.Zero, UNorm32.Zero, UNorm32.One);

      // 15 hues × 8 luminance levels. Hue 0 = grey, 1..14 evenly spaced around the chroma circle.
      var idx = 1;
      double[] luminance = [0.15, 0.23, 0.31, 0.42, 0.50, 0.62, 0.75, 0.88];
      for (var hue = 0; hue < 15; ++hue)
        for (var lum = 0; lum < 8; ++lum) {
          var l = luminance[lum];
          double r, g, b;
          if (hue == 0) {
            r = g = b = l;
          } else {
            var theta = (hue - 1) * 2.0 * System.Math.PI / 14.0 + 0.25;
            var chroma = 0.30 * (1.0 - System.Math.Abs(l - 0.5) * 1.1);
            r = l + chroma * System.Math.Cos(theta);
            g = l + chroma * System.Math.Cos(theta - 2.0 * System.Math.PI / 3.0);
            b = l + chroma * System.Math.Cos(theta + 2.0 * System.Math.PI / 3.0);
          }
          result[idx++] = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)System.Math.Max(0, System.Math.Min(1, r))),
            UNorm32.FromFloatClamped((float)System.Math.Max(0, System.Math.Min(1, g))),
            UNorm32.FromFloatClamped((float)System.Math.Max(0, System.Math.Min(1, b))),
            UNorm32.One);
        }
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => colorCount >= _palette.Length ? _palette : _palette.Take(colorCount).ToArray();
  }
}

#endregion
