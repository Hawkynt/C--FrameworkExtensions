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
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
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
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
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
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => _palette.Take(colorCount).ToArray();
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
  Palette1High
}

/// <summary>
/// CGA 4-color palette quantizer with configurable palette mode.
/// </summary>
/// <remarks>
/// <para>Uses the standard CGA (Color Graphics Adapter) 4-color palettes from 1981.</para>
/// <para>CGA supported two main palettes, each with low and high intensity variants:</para>
/// <list type="bullet">
///   <item><description>Palette 0: Black, Green, Red, Brown/Yellow</description></item>
///   <item><description>Palette 1: Black, Cyan, Magenta, White/Light Gray</description></item>
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
        CgaPaletteMode.Palette1High or _ => [
          (0, 0, 0),       // Black
          (85, 255, 255),  // Light Cyan
          (255, 85, 255),  // Light Magenta
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
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => this._palette.Take(colorCount).ToArray();
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
