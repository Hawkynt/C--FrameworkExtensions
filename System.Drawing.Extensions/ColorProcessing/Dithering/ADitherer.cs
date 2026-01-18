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
/// Arithmetic dithering algorithms using mathematical operations on pixel coordinates.
/// </summary>
/// <remarks>
/// <para>Generates dither patterns using XOR, addition, and multiplication on x/y coordinates.</para>
/// <para>No error diffusion - pure pattern-based dithering.</para>
/// <para>Multiple strategies: XOR Y×149, XY Arithmetic, Uniform, and per-channel variants.</para>
/// </remarks>
public static class ADitherer {

  /// <summary>Pre-configured XOR Y×149 ditherer.</summary>
  public static XorY149Ditherer XorY149 { get; } = new(false);

  /// <summary>Pre-configured XOR Y×149 ditherer with per-channel variation.</summary>
  public static XorY149Ditherer XorY149WithChannel { get; } = new(true);

  /// <summary>Pre-configured XY Arithmetic ditherer.</summary>
  public static XYArithmeticDitherer XYArithmetic { get; } = new(false);

  /// <summary>Pre-configured XY Arithmetic ditherer with per-channel variation.</summary>
  public static XYArithmeticDitherer XYArithmeticWithChannel { get; } = new(true);

  /// <summary>Pre-configured Uniform ditherer (threshold at 0.5).</summary>
  public static UniformDitherer Uniform { get; } = new();
}

/// <summary>
/// XOR-based arithmetic dithering using Y×149 multiplier pattern.
/// </summary>
[Ditherer("XOR Y×149", Description = "Arithmetic XOR dithering with Y×149 multiplier", Type = DitheringType.Ordered)]
public readonly struct XorY149Ditherer : IDitherer {

  private readonly bool _useChannels;

  /// <summary>
  /// Creates an XOR Y×149 ditherer.
  /// </summary>
  /// <param name="useChannels">Whether to use per-channel mask variation.</param>
  public XorY149Ditherer(bool useChannels = false) => this._useChannels = useChannels;

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
    var endY = startY + height;

    const int yMultiplier = 149;
    const double inv511 = 1.0 / 511.0;

    for (var y = startY; y < endY; ++y) {
      var yMultiplied = y * yMultiplier;

      for (var x = 0; x < width; ++x) {
        var pixel = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, alpha) = pixel.ToNormalized();
        var pC1 = (double)c1.ToFloat();
        var pC2 = (double)c2.ToFloat();
        var pC3 = (double)c3.ToFloat();
        var pA = alpha.ToFloat();

        double newC1, newC2, newC3;

        if (this._useChannels) {
          var rMask = (((x + 0 * 17) ^ yMultiplied) * 1234 & 511) * inv511;
          var gMask = (((x + 1 * 17) ^ yMultiplied) * 1234 & 511) * inv511;
          var bMask = (((x + 2 * 17) ^ yMultiplied) * 1234 & 511) * inv511;
          newC1 = (int)Math.Floor(256.0 * pC1 + rMask) / 256.0;
          newC2 = (int)Math.Floor(256.0 * pC2 + gMask) / 256.0;
          newC3 = (int)Math.Floor(256.0 * pC3 + bMask) / 256.0;
        } else {
          var mask = ((x ^ yMultiplied) * 1234 & 511) * inv511;
          newC1 = (int)Math.Floor(256.0 * pC1 + mask) / 256.0;
          newC2 = (int)Math.Floor(256.0 * pC2 + mask) / 256.0;
          newC3 = (int)Math.Floor(256.0 * pC3 + mask) / 256.0;
        }

        newC1 = Math.Max(0, Math.Min(1, newC1));
        newC2 = Math.Max(0, Math.Min(1, newC2));
        newC3 = Math.Max(0, Math.Min(1, newC3));

        var ditheredColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)newC1),
          UNorm32.FromFloatClamped((float)newC2),
          UNorm32.FromFloatClamped((float)newC3),
          UNorm32.FromFloatClamped(pA)
        );

        indices[y * targetStride + x] = (byte)lookup.FindNearest(ditheredColor);
      }
    }
  }
}

/// <summary>
/// XY Arithmetic-based dithering using addition and multiplication pattern.
/// </summary>
[Ditherer("XY Arithmetic", Description = "Arithmetic dithering with XY addition pattern", Type = DitheringType.Ordered)]
public readonly struct XYArithmeticDitherer : IDitherer {

  private readonly bool _useChannels;

  /// <summary>
  /// Creates an XY Arithmetic ditherer.
  /// </summary>
  /// <param name="useChannels">Whether to use per-channel mask variation.</param>
  public XYArithmeticDitherer(bool useChannels = false) => this._useChannels = useChannels;

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
    var endY = startY + height;

    var yMultiplier = this._useChannels ? 236 : 237;
    const double inv255 = 1.0 / 255.0;

    for (var y = startY; y < endY; ++y) {
      var yMultiplied = y * yMultiplier;

      for (var x = 0; x < width; ++x) {
        var pixel = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, alpha) = pixel.ToNormalized();
        var pC1 = (double)c1.ToFloat();
        var pC2 = (double)c2.ToFloat();
        var pC3 = (double)c3.ToFloat();
        var pA = alpha.ToFloat();

        double newC1, newC2, newC3;

        if (this._useChannels) {
          var rMask = (((x + 0 * 67) + yMultiplied) * 119 & 255) * inv255;
          var gMask = (((x + 1 * 67) + yMultiplied) * 119 & 255) * inv255;
          var bMask = (((x + 2 * 67) + yMultiplied) * 119 & 255) * inv255;
          newC1 = (int)Math.Floor(256.0 * pC1 + rMask) / 256.0;
          newC2 = (int)Math.Floor(256.0 * pC2 + gMask) / 256.0;
          newC3 = (int)Math.Floor(256.0 * pC3 + bMask) / 256.0;
        } else {
          var mask = ((x + yMultiplied) * 119 & 255) * inv255;
          newC1 = (int)Math.Floor(256.0 * pC1 + mask) / 256.0;
          newC2 = (int)Math.Floor(256.0 * pC2 + mask) / 256.0;
          newC3 = (int)Math.Floor(256.0 * pC3 + mask) / 256.0;
        }

        newC1 = Math.Max(0, Math.Min(1, newC1));
        newC2 = Math.Max(0, Math.Min(1, newC2));
        newC3 = Math.Max(0, Math.Min(1, newC3));

        var ditheredColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)newC1),
          UNorm32.FromFloatClamped((float)newC2),
          UNorm32.FromFloatClamped((float)newC3),
          UNorm32.FromFloatClamped(pA)
        );

        indices[y * targetStride + x] = (byte)lookup.FindNearest(ditheredColor);
      }
    }
  }
}

/// <summary>
/// Uniform threshold dithering with fixed 0.5 threshold.
/// </summary>
[Ditherer("Uniform", Description = "Simple threshold dithering at 0.5", Type = DitheringType.None)]
public readonly struct UniformDitherer : IDitherer {

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
    var endY = startY + height;

    const double mask = 0.5;

    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var pC1 = (double)c1.ToFloat();
      var pC2 = (double)c2.ToFloat();
      var pC3 = (double)c3.ToFloat();
      var pA = alpha.ToFloat();

      var newC1 = (int)Math.Floor(256.0 * pC1 + mask) / 256.0;
      var newC2 = (int)Math.Floor(256.0 * pC2 + mask) / 256.0;
      var newC3 = (int)Math.Floor(256.0 * pC3 + mask) / 256.0;

      newC1 = Math.Max(0, Math.Min(1, newC1));
      newC2 = Math.Max(0, Math.Min(1, newC2));
      newC3 = Math.Max(0, Math.Min(1, newC3));

      var ditheredColor = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped((float)newC1),
        UNorm32.FromFloatClamped((float)newC2),
        UNorm32.FromFloatClamped((float)newC3),
        UNorm32.FromFloatClamped(pA)
      );

      indices[y * targetStride + x] = (byte)lookup.FindNearest(ditheredColor);
    }
  }
}
