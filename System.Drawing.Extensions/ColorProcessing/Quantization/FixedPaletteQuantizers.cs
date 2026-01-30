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

#region WebSafe

/// <summary>
/// Web-safe 216-color palette quantizer.
/// </summary>
/// <remarks>
/// <para>Uses the standard web-safe palette with 6 levels per RGB channel (6×6×6 = 216 colors).</para>
/// <para>Each RGB component uses values: 0, 51, 102, 153, 204, 255 (hex: 00, 33, 66, 99, CC, FF).</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Web Safe", QualityRating = 2)]
public struct WebSafeQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      byte[] steps = [0, 51, 102, 153, 204, 255];
      return (
        from r in steps
        from g in steps
        from b in steps
        select ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(r),
          UNorm32.FromByte(g),
          UNorm32.FromByte(b),
          UNorm32.One
        )
      ).ToArray();
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      // For full palette request, return all 216 colors
      if (colorCount >= _palette.Length)
        return _palette;

      // Subsample the 6×6×6 cube to get well-distributed colors
      // Calculate optimal levels per channel for requested color count
      var levelsPerChannel = (int)Math.Ceiling(Math.Pow(colorCount, 1.0 / 3.0));
      levelsPerChannel = Math.Max(2, Math.Min(levelsPerChannel, 6));

      var result = new List<TWork>(colorCount);
      var stepIndices = _GetEvenlySpacedIndices(6, levelsPerChannel);

      foreach (var ri in stepIndices)
      foreach (var gi in stepIndices)
      foreach (var bi in stepIndices) {
        var index = ri * 36 + gi * 6 + bi; // Index into 6×6×6 cube
        result.Add(_palette[index]);
        if (result.Count >= colorCount)
          return result.ToArray();
      }

      return result.ToArray();
    }

    private static int[] _GetEvenlySpacedIndices(int total, int count) {
      if (count >= total)
        return Enumerable.Range(0, total).ToArray();
      var result = new int[count];
      for (var i = 0; i < count; ++i)
        result[i] = i * (total - 1) / (count - 1);
      return result;
    }
  }
}

#endregion

#region EGA 16

/// <summary>
/// EGA 16-color palette quantizer.
/// </summary>
/// <remarks>
/// <para>Uses the standard EGA (Enhanced Graphics Adapter) 16-color palette from 1984.</para>
/// <para>Includes 8 dark colors and 8 bright colors using the RGBrgb color model.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "EGA 16", Year = 1984, QualityRating = 1)]
public struct Ega16Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    internal static readonly TWork[] Palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      // Standard EGA 16-color palette (RGBrgb model)
      (byte r, byte g, byte b)[] egaColors = [
        (0, 0, 0),       // Black
        (0, 0, 170),     // Blue
        (0, 170, 0),     // Green
        (0, 170, 170),   // Cyan
        (170, 0, 0),     // Red
        (170, 0, 170),   // Magenta
        (170, 85, 0),    // Brown (dark yellow)
        (170, 170, 170), // Light Gray
        (85, 85, 85),    // Dark Gray
        (85, 85, 255),   // Light Blue
        (85, 255, 85),   // Light Green
        (85, 255, 255),  // Light Cyan
        (255, 85, 85),   // Light Red
        (255, 85, 255),  // Light Magenta
        (255, 255, 85),  // Yellow
        (255, 255, 255)  // White
      ];

      return egaColors.Select(c => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromByte(c.r),
        UNorm32.FromByte(c.g),
        UNorm32.FromByte(c.b),
        UNorm32.One
      )).ToArray();
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => Palette.Take(colorCount).ToArray();
  }
}

#endregion

#region VGA 256

/// <summary>
/// VGA 256-color palette quantizer.
/// </summary>
/// <remarks>
/// <para>Uses the standard VGA (Video Graphics Array) 256-color palette from 1987.</para>
/// <para>Combines EGA 16 colors, web-safe colors, and a grayscale ramp.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "VGA 256", Year = 1987, QualityRating = 2)]
public struct Vga256Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();

    private static TWork[] _CreatePalette() {
      var list = new List<TWork>(256);

      // Start with EGA 16 colors
      list.AddRange(Ega16Quantizer.Kernel<TWork>.Palette);

      // Add web-safe colors (6×6×6 cube)
      byte[] steps = [0, 51, 102, 153, 204, 255];
      list.AddRange(
        from r in steps
        from g in steps
        from b in steps
        select ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(r),
          UNorm32.FromByte(g),
          UNorm32.FromByte(b),
          UNorm32.One
        )
      );

      // Add 24-step grayscale ramp (8 to 238 in steps of 10)
      for (var i = 0; i < 24; ++i) {
        var v = (byte)(8 + i * 10);
        list.Add(ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(v),
          UNorm32.FromByte(v),
          UNorm32.FromByte(v),
          UNorm32.One
        ));
      }

      return list.ToArray();
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      // For full palette request, return all colors
      if (colorCount >= _palette.Length)
        return _palette;

      // VGA 256 is composed of: 16 EGA + 216 web-safe + 24 grayscale
      // For smaller requests, prioritize: some EGA, some web-safe cube, some grayscale
      var result = new List<TWork>(colorCount);

      // Always include black and white from EGA
      result.Add(_palette[0]);  // Black
      result.Add(_palette[15]); // White

      // Add evenly distributed colors from web-safe portion (indices 16-231)
      var webSafeCount = Math.Max(0, colorCount - 6); // Reserve 6 slots for EGA primaries + grays
      if (webSafeCount > 0) {
        var levelsPerChannel = (int)Math.Ceiling(Math.Pow(webSafeCount, 1.0 / 3.0));
        levelsPerChannel = Math.Max(2, Math.Min(levelsPerChannel, 6));
        var stepIndices = _GetEvenlySpacedIndices(6, levelsPerChannel);

        foreach (var ri in stepIndices)
        foreach (var gi in stepIndices)
        foreach (var bi in stepIndices) {
          var index = 16 + ri * 36 + gi * 6 + bi; // Offset by 16 for EGA colors
          if (!result.Contains(_palette[index]))
            result.Add(_palette[index]);
          if (result.Count >= colorCount)
            return result.ToArray();
        }
      }

      // Fill remaining with evenly spaced grayscale
      var grayscaleStart = 16 + 216; // After EGA and web-safe
      var grayscaleIndices = _GetEvenlySpacedIndices(24, Math.Max(1, colorCount - result.Count));
      foreach (var gi in grayscaleIndices) {
        if (!result.Contains(_palette[grayscaleStart + gi]))
          result.Add(_palette[grayscaleStart + gi]);
        if (result.Count >= colorCount)
          return result.ToArray();
      }

      // Fill any remaining with EGA primaries
      for (var i = 1; i < 15 && result.Count < colorCount; ++i)
        if (!result.Contains(_palette[i]))
          result.Add(_palette[i]);

      return result.ToArray();
    }

    private static int[] _GetEvenlySpacedIndices(int total, int count) {
      if (count >= total)
        return Enumerable.Range(0, total).ToArray();
      var result = new int[count];
      for (var i = 0; i < count; ++i)
        result[i] = i * (total - 1) / (count - 1);
      return result;
    }
  }
}

#endregion

#region Mac 8-Bit

/// <summary>
/// Macintosh 8-bit palette quantizer.
/// </summary>
/// <remarks>
/// <para>Uses the classic Macintosh System 7 256-color palette.</para>
/// <para>Features 8 levels for red/green and 4 levels for blue (8×8×4 = 256 colors).</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Mac 8-Bit", Year = 1991, QualityRating = 2)]
public struct Mac8BitQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = _CreatePalette();
    private const int RgLevels = 8;
    private const int BlueLevels = 4;

    private static TWork[] _CreatePalette() {
      // Mac 8-bit: 8 levels for R/G, 4 levels for B
      byte[] rg = [0, 36, 73, 109, 146, 182, 219, 255];
      byte[] b = [0, 85, 170, 255];

      return (
        from r in rg
        from g in rg
        from blue in b
        select ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(r),
          UNorm32.FromByte(g),
          UNorm32.FromByte(blue),
          UNorm32.One
        )
      ).ToArray();
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      // For full palette request, return all 256 colors
      if (colorCount >= _palette.Length)
        return _palette;

      // Subsample the 8×8×4 cube to get well-distributed colors
      // Calculate optimal levels for each channel based on requested color count
      // Target: rLevels * gLevels * bLevels ≈ colorCount, with r:g:b ratio similar to 8:8:4
      var totalRatio = Math.Pow(colorCount, 1.0 / 3.0);
      var rLevels = Math.Max(2, Math.Min((int)Math.Ceiling(totalRatio * 1.26), RgLevels)); // 8/6.35 ≈ 1.26
      var gLevels = Math.Max(2, Math.Min((int)Math.Ceiling(totalRatio * 1.26), RgLevels));
      var bLevels = Math.Max(2, Math.Min((int)Math.Ceiling(totalRatio * 0.63), BlueLevels)); // 4/6.35 ≈ 0.63

      var result = new List<TWork>(colorCount);
      var rIndices = _GetEvenlySpacedIndices(RgLevels, rLevels);
      var gIndices = _GetEvenlySpacedIndices(RgLevels, gLevels);
      var bIndices = _GetEvenlySpacedIndices(BlueLevels, bLevels);

      foreach (var ri in rIndices)
      foreach (var gi in gIndices)
      foreach (var bi in bIndices) {
        var index = ri * RgLevels * BlueLevels + gi * BlueLevels + bi;
        result.Add(_palette[index]);
        if (result.Count >= colorCount)
          return result.ToArray();
      }

      return result.ToArray();
    }

    private static int[] _GetEvenlySpacedIndices(int total, int count) {
      if (count >= total)
        return Enumerable.Range(0, total).ToArray();
      var result = new int[count];
      for (var i = 0; i < count; ++i)
        result[i] = i * (total - 1) / (count - 1);
      return result;
    }
  }
}

#endregion

#region CGA 4

/// <summary>
/// Specifies the CGA palette mode.
/// </summary>
public enum CgaPaletteMode : byte {
  /// <summary>Palette 0 low intensity: Black, Green, Red, Brown.</summary>
  Palette0Low,
  /// <summary>Palette 0 high intensity: Black, Light Green, Light Red, Yellow.</summary>
  Palette0High,
  /// <summary>Palette 1 low intensity: Black, Cyan, Magenta, Light Gray.</summary>
  Palette1Low,
  /// <summary>Palette 1 high intensity: Black, Light Cyan, Light Magenta, White.</summary>
  Palette1High,
  /// <summary>Mode 5 / Palette 2 low intensity: Black, Cyan, Red, Light Gray.</summary>
  Palette2Low,
  /// <summary>Mode 5 / Palette 2 high intensity: Black, Light Cyan, Light Red, White.</summary>
  Palette2High
}

/// <summary>
/// CGA 4-color palette quantizer with configurable palette mode.
/// </summary>
/// <remarks>
/// <para>Uses the standard CGA (Color Graphics Adapter) 4-color palettes from 1981.</para>
/// <para>CGA supported three main palettes, each with low and high intensity variants:</para>
/// <list type="bullet">
///   <item><description>Palette 0: Black, Green, Red, Brown/Yellow</description></item>
///   <item><description>Palette 1: Black, Cyan, Magenta, White/Light Gray</description></item>
///   <item><description>Mode 5 / Palette 2: Black, Cyan, Red, White/Light Gray</description></item>
/// </list>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "CGA 4", Year = 1981, QualityRating = 1)]
public readonly struct Cga4Quantizer : IQuantizer {
  /// <summary>
  /// Creates a CGA quantizer with the default palette (Palette 1 High).
  /// </summary>
  public Cga4Quantizer() : this(CgaPaletteMode.Palette1High) { }

  /// <summary>
  /// Creates a CGA quantizer with the specified palette mode.
  /// </summary>
  /// <param name="mode">The CGA palette mode to use.</param>
  public Cga4Quantizer(CgaPaletteMode mode) => this.Mode = mode;

  /// <summary>
  /// Gets the palette mode.
  /// </summary>
  public CgaPaletteMode Mode { get; }

  #region Static Presets

  /// <summary>Gets CGA Palette 0 Low: Black, Green, Red, Brown.</summary>
  public static Cga4Quantizer Palette0Low => new(CgaPaletteMode.Palette0Low);

  /// <summary>Gets CGA Palette 0 High: Black, Light Green, Light Red, Yellow.</summary>
  public static Cga4Quantizer Palette0High => new(CgaPaletteMode.Palette0High);

  /// <summary>Gets CGA Palette 1 Low: Black, Cyan, Magenta, Light Gray.</summary>
  public static Cga4Quantizer Palette1Low => new(CgaPaletteMode.Palette1Low);

  /// <summary>Gets CGA Palette 1 High: Black, Light Cyan, Light Magenta, White (default).</summary>
  public static Cga4Quantizer Palette1High => new(CgaPaletteMode.Palette1High);

  /// <summary>Gets CGA Palette 2 Low (Mode 5): Black, Cyan, Red, Light Gray.</summary>
  public static Cga4Quantizer Palette2Low => new(CgaPaletteMode.Palette2Low);

  /// <summary>Gets CGA Palette 2 High (Mode 5): Black, Light Cyan, Light Red, White.</summary>
  public static Cga4Quantizer Palette2High => new(CgaPaletteMode.Palette2High);

  #endregion

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.Mode);

  internal sealed class Kernel<TWork>(CgaPaletteMode mode) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private readonly TWork[] _palette = _CreatePalette(mode);

    private static TWork[] _CreatePalette(CgaPaletteMode mode) {
      // CGA color definitions
      (byte r, byte g, byte b)[] colors = mode switch {
        CgaPaletteMode.Palette0Low => [
          (0, 0, 0),       // Black
          (0, 170, 0),     // Green
          (170, 0, 0),     // Red
          (170, 85, 0)     // Brown
        ],
        CgaPaletteMode.Palette0High => [
          (0, 0, 0),       // Black
          (85, 255, 85),   // Light Green
          (255, 85, 85),   // Light Red
          (255, 255, 85)   // Yellow
        ],
        CgaPaletteMode.Palette1Low => [
          (0, 0, 0),       // Black
          (0, 170, 170),   // Cyan
          (170, 0, 170),   // Magenta
          (170, 170, 170)  // Light Gray
        ],
        CgaPaletteMode.Palette1High => [
          (0, 0, 0),       // Black
          (85, 255, 255),  // Light Cyan
          (255, 85, 255),  // Light Magenta
          (255, 255, 255)  // White
        ],
        CgaPaletteMode.Palette2Low => [
          (0, 0, 0),       // Black
          (0, 170, 170),   // Cyan
          (170, 0, 0),     // Red
          (170, 170, 170)  // Light Gray
        ],
        CgaPaletteMode.Palette2High or _ => [
          (0, 0, 0),       // Black
          (85, 255, 255),  // Light Cyan
          (255, 85, 85),   // Light Red
          (255, 255, 255)  // White
        ]
      };

      return colors.Select(c => ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromByte(c.r),
        UNorm32.FromByte(c.g),
        UNorm32.FromByte(c.b),
        UNorm32.One
      )).ToArray();
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => this._palette.Take(colorCount).ToArray();
  }
}

#endregion

#region CGA Composite 16

/// <summary>
/// CGA Composite 16-color palette quantizer.
/// </summary>
/// <remarks>
/// <para>Uses the composite artifact colors from CGA 640x200 display mode.</para>
/// <para>These colors result from the NTSC composite video encoding quirks of the original CGA hardware.</para>
/// <para>Reference: https://lospec.com/palette-list/cga-composite</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "CGA Composite 16", Year = 1981, QualityRating = 2)]
public readonly struct CgaComposite16Quantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  /// <summary>Gets the CGA Composite 16 palette.</summary>
  public static CgaComposite16Quantizer Default => new();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    // CGA Composite colors from NTSC artifact coloring (640x200 mode)
    private static readonly TWork[] _palette = [
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x00), UNorm32.FromByte(0x00), UNorm32.FromByte(0x00), UNorm32.One), // #000000 Black
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x00), UNorm32.FromByte(0x6E), UNorm32.FromByte(0x2D), UNorm32.One), // #006e2d Dark Green
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x2D), UNorm32.FromByte(0x02), UNorm32.FromByte(0xFF), UNorm32.One), // #2d02ff Blue
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x00), UNorm32.FromByte(0x8B), UNorm32.FromByte(0xFF), UNorm32.One), // #008bff Light Blue
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0xA9), UNorm32.FromByte(0x00), UNorm32.FromByte(0x2D), UNorm32.One), // #a9002d Dark Red
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x77), UNorm32.FromByte(0x76), UNorm32.FromByte(0x77), UNorm32.One), // #777677 Gray
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0xEC), UNorm32.FromByte(0x09), UNorm32.FromByte(0xFF), UNorm32.One), // #ec09ff Magenta
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0xBB), UNorm32.FromByte(0x92), UNorm32.FromByte(0xFD), UNorm32.One), // #bb92fd Light Purple
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x2D), UNorm32.FromByte(0x5A), UNorm32.FromByte(0x00), UNorm32.One), // #2d5a00 Olive
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x00), UNorm32.FromByte(0xDC), UNorm32.FromByte(0x00), UNorm32.One), // #00dc00 Green
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x76), UNorm32.FromByte(0x77), UNorm32.FromByte(0x77), UNorm32.One), // #767777 Gray 2
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0x45), UNorm32.FromByte(0xF4), UNorm32.FromByte(0xB9), UNorm32.One), // #45f4b9 Cyan
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0xEA), UNorm32.FromByte(0x65), UNorm32.FromByte(0x02), UNorm32.One), // #ea6502 Orange
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0xBC), UNorm32.FromByte(0xE5), UNorm32.FromByte(0x00), UNorm32.One), // #bce500 Yellow-Green
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0xFF), UNorm32.FromByte(0x80), UNorm32.FromByte(0xBC), UNorm32.One), // #ff80bc Pink
      ColorFactory.FromNormalized_4<TWork>(UNorm32.FromByte(0xFF), UNorm32.FromByte(0xFF), UNorm32.FromByte(0xFF), UNorm32.One)  // #ffffff White
    ];

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region Monochrome

/// <summary>
/// Monochrome (black and white) palette quantizer.
/// </summary>
/// <remarks>
/// <para>Uses only black and white, suitable for 1-bit displays.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Monochrome", QualityRating = 1)]
public struct MonochromeQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private static readonly TWork[] _palette = [
      ColorFactory.FromNormalized_4<TWork>(UNorm32.Zero, UNorm32.Zero, UNorm32.Zero, UNorm32.One),
      ColorFactory.FromNormalized_4<TWork>(UNorm32.One, UNorm32.One, UNorm32.One, UNorm32.One)
    ];

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
  }
}

#endregion

#region Grayscale

/// <summary>
/// Grayscale palette quantizer with configurable levels.
/// </summary>
/// <remarks>
/// <para>Generates a uniform grayscale palette from black to white.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Grayscale", QualityRating = 2)]
public readonly struct GrayscaleQuantizer : IQuantizer {

  private readonly byte _levels;

  /// <summary>
  /// Creates a grayscale quantizer with default 256 levels.
  /// </summary>
  public GrayscaleQuantizer() : this(0) { }

  /// <summary>
  /// Creates a grayscale quantizer with the specified number of levels.
  /// </summary>
  /// <param name="levels">Number of grayscale levels (2-256). 0 defaults to 256.</param>
  public GrayscaleQuantizer(byte levels) => this._levels = levels;

  /// <summary>
  /// Gets the number of grayscale levels.
  /// </summary>
  public int Levels => this._levels == 0 ? 256 : this._levels;

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.Levels);

  internal sealed class Kernel<TWork>(int levels) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private readonly int _levels = levels;
    private readonly TWork[] _palette = _CreatePalette(levels);

    private static TWork[] _CreatePalette(int levels) {
      var result = new TWork[levels];
      for (var i = 0; i < levels; ++i) {
        var v = levels == 1 ? UNorm32.One : UNorm32.FromRaw((uint)(i * uint.MaxValue / (levels - 1)));
        result[i] = ColorFactory.FromNormalized_4<TWork>(v, v, v, UNorm32.One);
      }
      return result;
    }

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      // For full palette request or if colorCount >= levels, return full palette
      if (colorCount >= this._levels)
        return this._palette;

      // Create evenly-spaced grayscale levels for smaller requests
      var result = new TWork[colorCount];
      for (var i = 0; i < colorCount; ++i) {
        // Map i to the full range [0, levels-1]
        var sourceIndex = i * (this._levels - 1) / (colorCount - 1);
        result[i] = this._palette[sourceIndex];
      }
      return result;
    }
  }
}

#endregion

#region Custom

/// <summary>
/// Custom fixed palette quantizer using a user-provided palette.
/// </summary>
/// <remarks>
/// <para>Allows using any predefined palette of colors.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Custom", QualityRating = 5)]
public readonly struct CustomPaletteQuantizer : IQuantizer {

  private readonly (byte r, byte g, byte b, byte a)[] _palette;

  /// <summary>
  /// Creates a custom quantizer with the specified RGBA palette.
  /// </summary>
  /// <param name="palette">Array of RGBA color tuples.</param>
  public CustomPaletteQuantizer((byte r, byte g, byte b, byte a)[] palette) => this._palette = palette;

  /// <summary>
  /// Creates a custom quantizer with the specified RGB palette (opaque colors).
  /// </summary>
  /// <param name="palette">Array of RGB color tuples.</param>
  public CustomPaletteQuantizer((byte r, byte g, byte b)[] palette)
    : this(palette.Select(c => (c.r, c.g, c.b, (byte)255)).ToArray()) { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this._palette ?? []);

  internal sealed class Kernel<TWork>((byte r, byte g, byte b, byte a)[] palette) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    private readonly TWork[] _palette = palette.Select(c => ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromByte(c.r),
      UNorm32.FromByte(c.g),
      UNorm32.FromByte(c.b),
      UNorm32.FromByte(c.a)
    )).ToArray();

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => this._palette.Take(colorCount).ToArray();
  }
}

#endregion
