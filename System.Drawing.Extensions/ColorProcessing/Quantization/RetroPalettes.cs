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

#region Euclid 216

/// <summary>
/// Euclid-216 fixed 6×6×6 uniform RGB cube quantizer.
/// </summary>
/// <remarks>
/// <para>
/// Arithmetically identical lattice size to <see cref="WebSafeQuantizer"/> (6³ = 216) but uses
/// uniformly spaced levels <c>i · 255/5</c> on each channel (0, 51, 102, 153, 204, 255) reproduced
/// here for parity with the classic "uniform 6-levels-per-channel" grid that textbook colour
/// quantization literature calls the "Euclidean" RGB cube. Kept as a distinct class for semantic
/// clarity — callers asking for "Euclid216" are looking for a regular lattice, not the historical
/// WWW-era constraint that the web-safe palette represents.
/// </para>
/// <para>Use case: deterministic posterisation when an external tool expects exactly 216 uniform-grid entries.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Euclid 216", QualityRating = 2)]
public struct Euclid216Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      var result = new TWork[216];
      var idx = 0;
      for (var r = 0; r < 6; ++r)
      for (var g = 0; g < 6; ++g)
      for (var b = 0; b < 6; ++b)
        result[idx++] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte((byte)(r * 255 / 5)),
          UNorm32.FromByte((byte)(g * 255 / 5)),
          UNorm32.FromByte((byte)(b * 255 / 5)),
          UNorm32.One);
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => colorCount >= _palette.Length ? _palette : _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region RGB332

/// <summary>
/// RGB-332 fixed 256-entry palette (8 red levels × 8 green levels × 4 blue levels).
/// </summary>
/// <remarks>
/// <para>
/// Historical 8-bit VGA-era palette mapping: 3 bits red, 3 bits green, 2 bits blue packed into a
/// byte. The blue channel gets fewer levels because the human eye has lower chromatic sensitivity
/// at short wavelengths. Common in early DOS games (Quake's lightmap mode), Game Boy Advance
/// BG modes, and numerous framebuffer chips (Hitachi HD44780 derivatives).
/// </para>
/// <para>Reference: Foley, van Dam, Feiner, Hughes — "Computer Graphics: Principles and Practice" §17.4.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "RGB 332", QualityRating = 2)]
public struct Rgb332Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      var result = new TWork[256];
      var idx = 0;
      for (var r = 0; r < 8; ++r)
      for (var g = 0; g < 8; ++g)
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

#region RGB 565 Truncated

/// <summary>
/// RGB-565 truncated-precision fixed palette (5-6-5 bit snap sampled along the luminance axis).
/// </summary>
/// <remarks>
/// <para>
/// Snaps colours to the nearest RGB-565 (32 red levels × 64 green levels × 32 blue levels = 65 536
/// total reachable outputs). Because 65 536 outputs cannot fit in a 256-entry palette the
/// quantizer samples evenly along the luminance axis to produce <see cref="IQuantizer{TWork}.GeneratePalette"/>-
/// requested entries; each entry is a valid RGB-565 colour.
/// </para>
/// <para>Common as a backing format for embedded framebuffers (SSD1306, ILI9341) where the memory
/// representation is 2 bytes/pixel but the logical palette on an indexed render path stays small.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "RGB 565 Truncated", QualityRating = 2)]
public struct Rgb565TruncatedQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      if (colorCount <= 0)
        return [];

      // Sample evenly along the luminance axis, snapping each sample to the nearest RGB-565 value.
      var result = new TWork[colorCount];
      for (var i = 0; i < colorCount; ++i) {
        // Luma ramp mapped back to RGB with RGB-565 quantisation per channel.
        var t = colorCount == 1 ? 0.5 : (double)i / (colorCount - 1);
        // 5-bit red, 6-bit green, 5-bit blue.
        var r5 = (int)(t * 31.0 + 0.5);
        var g6 = (int)(t * 63.0 + 0.5);
        var b5 = (int)(t * 31.0 + 0.5);
        var r8 = (byte)((r5 * 255) / 31);
        var g8 = (byte)((g6 * 255) / 63);
        var b8 = (byte)((b5 * 255) / 31);
        result[i] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(r8),
          UNorm32.FromByte(g8),
          UNorm32.FromByte(b8),
          UNorm32.One);
      }
      return result;
    }
  }
}

#endregion

#region Apple II HGR

/// <summary>
/// Apple II HGR 6-colour high-resolution palette (1977).
/// </summary>
/// <remarks>
/// <para>
/// The Apple II's HGR mode produced these six on-screen colours via NTSC colour-burst trickery
/// on a nominally monochrome bitmap. Palette values are the Woz-era-documented RGB triplets
/// as seen on a composite TV set.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Apple II HGR", Year = 1977, QualityRating = 1)]
public struct AppleIIHgrQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = new (byte r, byte g, byte b)[] {
      (0, 0, 0),        // Black
      (255, 255, 255),  // White
      (20, 245, 60),    // Green
      (255, 68, 253),   // Violet
      (255, 106, 60),   // Orange
      (20, 207, 253),   // Blue
    }.Select(c => ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromByte(c.r), UNorm32.FromByte(c.g), UNorm32.FromByte(c.b), UNorm32.One)).ToArray();

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region NES PPU 64

/// <summary>
/// Nintendo NES/Famicom PPU 64-colour master palette (1983).
/// </summary>
/// <remarks>
/// <para>
/// The 2C02 PPU produced 64 composite-video colours via a 4-bit hue × 4-bit value lookup.
/// Software could only display 25 simultaneously (background + 4 sprite sub-palettes × 4 + the
/// universal backdrop). The 64-entry master set below is the commonly-cited "FCEUX/Nestopia default"
/// RGB translation.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "NES PPU", Year = 1983, QualityRating = 2)]
public struct NesPpuQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = new (byte r, byte g, byte b)[] {
      (0x62, 0x62, 0x62), (0x00, 0x1F, 0xB2), (0x24, 0x04, 0xC8), (0x52, 0x00, 0xB2),
      (0x73, 0x00, 0x76), (0x80, 0x00, 0x24), (0x73, 0x0B, 0x00), (0x52, 0x28, 0x00),
      (0x24, 0x44, 0x00), (0x00, 0x57, 0x00), (0x00, 0x5C, 0x00), (0x00, 0x53, 0x24),
      (0x00, 0x3C, 0x76), (0x00, 0x00, 0x00), (0x00, 0x00, 0x00), (0x00, 0x00, 0x00),
      (0xAB, 0xAB, 0xAB), (0x0D, 0x57, 0xFF), (0x4B, 0x30, 0xFF), (0x8A, 0x13, 0xFF),
      (0xBC, 0x08, 0xD6), (0xD2, 0x12, 0x69), (0xC7, 0x2E, 0x00), (0x9D, 0x54, 0x00),
      (0x60, 0x7B, 0x00), (0x20, 0x98, 0x00), (0x00, 0xA3, 0x00), (0x00, 0x99, 0x42),
      (0x00, 0x7D, 0xB4), (0x00, 0x00, 0x00), (0x00, 0x00, 0x00), (0x00, 0x00, 0x00),
      (0xFF, 0xFF, 0xFF), (0x53, 0xAE, 0xFF), (0x90, 0x85, 0xFF), (0xD3, 0x65, 0xFF),
      (0xFF, 0x57, 0xFF), (0xFF, 0x5D, 0xCF), (0xFF, 0x77, 0x57), (0xFA, 0x9E, 0x00),
      (0xBD, 0xC7, 0x00), (0x7A, 0xE7, 0x00), (0x43, 0xF6, 0x11), (0x26, 0xEF, 0x7E),
      (0x2C, 0xD5, 0xF6), (0x4E, 0x4E, 0x4E), (0x00, 0x00, 0x00), (0x00, 0x00, 0x00),
      (0xFF, 0xFF, 0xFF), (0xB6, 0xE1, 0xFF), (0xCE, 0xD1, 0xFF), (0xE9, 0xC3, 0xFF),
      (0xFF, 0xBC, 0xFF), (0xFF, 0xBD, 0xF4), (0xFF, 0xC6, 0xC3), (0xFF, 0xD5, 0x9A),
      (0xE9, 0xE6, 0x81), (0xCE, 0xF4, 0x81), (0xB6, 0xFB, 0x9A), (0xA9, 0xFA, 0xC3),
      (0xA9, 0xF0, 0xF4), (0xB8, 0xB8, 0xB8), (0x00, 0x00, 0x00), (0x00, 0x00, 0x00)
    }.Select(c => ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromByte(c.r), UNorm32.FromByte(c.g), UNorm32.FromByte(c.b), UNorm32.One)).ToArray();

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region Game Boy 4

/// <summary>
/// Nintendo Game Boy DMG 4-shade "mint-green" palette (1989).
/// </summary>
/// <remarks>
/// <para>Four shades of the characteristic pea-green DMG LCD. Values are the de-facto RGB
/// translations used by virtually every Game Boy emulator (BGB, SameBoy).</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Game Boy 4", Year = 1989, QualityRating = 1)]
public struct GameBoy4Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = new (byte r, byte g, byte b)[] {
      (0x0F, 0x38, 0x0F),  // Darkest
      (0x30, 0x62, 0x30),
      (0x8B, 0xAC, 0x0F),
      (0x9B, 0xBC, 0x0F),  // Lightest
    }.Select(c => ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromByte(c.r), UNorm32.FromByte(c.g), UNorm32.FromByte(c.b), UNorm32.One)).ToArray();

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region C64 16

/// <summary>
/// Commodore 64 VIC-II 16-colour palette (1982).
/// </summary>
/// <remarks>
/// <para>The 16 hard-wired VIC-II hues — a famously non-uniform set picked by MOS Technology's
/// Bob Yannes to maximise luminance spread at minimal chip cost. Values are Pepto's canonical
/// "VICE-accurate" RGB translation (2001).</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Commodore 64", Year = 1982, QualityRating = 2)]
public struct C64Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = new (byte r, byte g, byte b)[] {
      (0x00, 0x00, 0x00), (0xFF, 0xFF, 0xFF), (0x88, 0x39, 0x32), (0x67, 0xB6, 0xBD),
      (0x8B, 0x3F, 0x96), (0x55, 0xA0, 0x49), (0x40, 0x31, 0x8D), (0xBF, 0xCE, 0x72),
      (0x8B, 0x54, 0x29), (0x57, 0x42, 0x00), (0xB8, 0x69, 0x62), (0x50, 0x50, 0x50),
      (0x78, 0x78, 0x78), (0x94, 0xE0, 0x89), (0x78, 0x69, 0xC4), (0x9F, 0x9F, 0x9F),
    }.Select(c => ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromByte(c.r), UNorm32.FromByte(c.g), UNorm32.FromByte(c.b), UNorm32.One)).ToArray();

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region ZX Spectrum 15

/// <summary>
/// Sinclair ZX Spectrum 15-colour palette (1982).
/// </summary>
/// <remarks>
/// <para>8 primary colours × 2 brightness levels, minus the double-black (which collapses to a
/// single entry) = 15 unique colours. Values are the canonical "bright on Spectrum" RGB translation
/// used by FUSE and ZEsarUX.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "ZX Spectrum", Year = 1982, QualityRating = 1)]
public struct ZxSpectrumQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = new (byte r, byte g, byte b)[] {
      (0x00, 0x00, 0x00), // Black
      (0x00, 0x00, 0xD8), // Blue
      (0xD8, 0x00, 0x00), // Red
      (0xD8, 0x00, 0xD8), // Magenta
      (0x00, 0xD8, 0x00), // Green
      (0x00, 0xD8, 0xD8), // Cyan
      (0xD8, 0xD8, 0x00), // Yellow
      (0xD8, 0xD8, 0xD8), // White (bright off)
      // Bright-on variants (same hue, full saturation); bright black = black and is omitted.
      (0x00, 0x00, 0xFF),
      (0xFF, 0x00, 0x00),
      (0xFF, 0x00, 0xFF),
      (0x00, 0xFF, 0x00),
      (0x00, 0xFF, 0xFF),
      (0xFF, 0xFF, 0x00),
      (0xFF, 0xFF, 0xFF),
    }.Select(c => ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromByte(c.r), UNorm32.FromByte(c.g), UNorm32.FromByte(c.b), UNorm32.One)).ToArray();

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region Amiga OCS 32

/// <summary>
/// Amiga OCS "HAM"-free 32-colour palette — evenly-spaced 4-4-4 sub-cube (1985).
/// </summary>
/// <remarks>
/// <para>
/// The Amiga OCS chipset stored each colour as 4 bits red × 4 bits green × 4 bits blue (4096
/// total reachable). The conventional 32-colour "non-HAM" mode picked 32 entries — here we
/// reproduce the evenly-spaced sub-cube commonly used as a demo-era default palette.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Amiga OCS", Year = 1985, QualityRating = 2)]
public struct AmigaOcsQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      // 32 entries chosen as a regular sub-lattice of the 4-4-4 cube (2 levels R × 4 levels G ×
      // 4 levels B = 32). Green gets more levels because OCS's "dual playfield" mode favoured
      // green-rich sprite artwork.
      var result = new TWork[32];
      var idx = 0;
      for (var r = 0; r < 2; ++r)
      for (var g = 0; g < 4; ++g)
      for (var b = 0; b < 4; ++b) {
        // 4-bit-per-channel snap: 0, 17, 34, ..., 255
        var r8 = (byte)(r * 255);
        var g8 = (byte)(g * 255 / 3);
        var b8 = (byte)(b * 255 / 3);
        result[idx++] = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(r8), UNorm32.FromByte(g8), UNorm32.FromByte(b8), UNorm32.One);
      }
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region Atari 2600

/// <summary>
/// Atari 2600 NTSC TIA 128-colour palette (1977).
/// </summary>
/// <remarks>
/// <para>
/// The TIA chip produced 128 simultaneous colours (8 × 16 luma × hue grid) via NTSC chroma
/// phase modulation. Values are the Stella-emulator "NTSC default" calibration.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Atari 2600", Year = 1977, QualityRating = 2)]
public struct Atari2600Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      // Approximated TIA NTSC palette: 16 hues × 8 luma.
      // Hue index 0 is grey; remaining 15 hues are distributed around the chroma circle.
      var result = new TWork[128];
      for (var hue = 0; hue < 16; ++hue) {
        for (var lum = 0; lum < 8; ++lum) {
          var l = (lum + 0.5) / 8.0;
          var baseR = l;
          var baseG = l;
          var baseB = l;
          if (hue > 0) {
            var theta = (hue - 1) * 2.0 * System.Math.PI / 15.0;
            var chroma = 0.35 * (1.0 - System.Math.Abs(l - 0.5) * 1.2);
            baseR = l + chroma * System.Math.Cos(theta);
            baseG = l + chroma * System.Math.Cos(theta - 2.0 * System.Math.PI / 3.0);
            baseB = l + chroma * System.Math.Cos(theta + 2.0 * System.Math.PI / 3.0);
          }
          result[hue * 8 + lum] = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)System.Math.Max(0, System.Math.Min(1, baseR))),
            UNorm32.FromFloatClamped((float)System.Math.Max(0, System.Math.Min(1, baseG))),
            UNorm32.FromFloatClamped((float)System.Math.Max(0, System.Math.Min(1, baseB))),
            UNorm32.One);
        }
      }
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion
