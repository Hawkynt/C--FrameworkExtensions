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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Average dithering algorithm that computes the average color of a region and uses it as a threshold.
/// </summary>
/// <remarks>
/// <para>
/// This adaptive dithering technique analyzes local regions of the image to compute average
/// color values, which are then used as thresholds for quantization decisions. This approach
/// helps preserve detail and contrast in different areas of the image.
/// </para>
/// <para>
/// The algorithm divides the image into regions of the specified size and computes the average
/// color for each region. Each pixel is then compared to its region's average to determine
/// whether to round up or down to the nearest palette color.
/// </para>
/// <para>
/// Reference: https://www.graphicsacademy.com/what_dithera.php
/// </para>
/// </remarks>
[Ditherer("Average Dithering", Description = "Adaptive dithering using local region averages", Type = DitheringType.Custom)]
public readonly struct AverageDitherer : IDitherer {

  #region properties

  /// <summary>The size of the region to compute averages over.</summary>
  public int RegionSize { get; }

  /// <summary>The threshold adjustment strength (default 16/255).</summary>
  public float Strength { get; }

  #endregion

  #region fluent API

  /// <summary>Returns this ditherer with specified region size.</summary>
  public AverageDitherer WithRegionSize(int regionSize) => new(regionSize, this.Strength);

  /// <summary>Returns this ditherer with specified strength.</summary>
  public AverageDitherer WithStrength(float strength) => new(this.RegionSize, strength);

  #endregion

  #region constructors

  /// <summary>
  /// Creates an average ditherer.
  /// </summary>
  /// <param name="regionSize">The size of the region to compute averages over. Default is 4.</param>
  /// <param name="strength">The threshold adjustment strength (0-1). Default is ~0.063 (16/255).</param>
  public AverageDitherer(int regionSize = 4, float strength = 16f / 255f) {
    this.RegionSize = Math.Max(1, regionSize);
    this.Strength = Math.Max(0, Math.Min(1, strength));
  }

  #endregion

  #region IDitherer

  /// <inheritdoc />
  /// <remarks>
  /// Average dithering is sequential because computing region averages requires access to neighboring pixels.
  /// However, once averages are computed, pixels could be processed in parallel.
  /// </remarks>
  public bool RequiresSequentialProcessing => true;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TMetric>(
    TWork* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
        in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;
    var regionSize = this.RegionSize;
    var halfSize = regionSize / 2;
    var strength = this.Strength;

    // pre-decode the entire region once. Both the central decode AND the per-pixel
    // _ComputeRegionAverage call (which re-decodes O(regionSize²) neighbours) read from
    // the working buffer, paying gamma-LUT cost exactly once per pixel.
    for (var y = startY; y < endY; ++y) {
      var rowSource = source + y * sourceStride;
      for (int x = 0, targetIdx = y * targetStride; x < width; ++x, ++targetIdx) {
        var originalColor = rowSource[x];
        var (c1, c2, c3, c4) = originalColor.ToNormalized();

        // Compute region average from pre-decoded source buffer (no re-decode).
        var (avg1, avg2, avg3) = _ComputeRegionAverageFromBuffer(source, sourceStride, x, y, width, startY + height, halfSize);

        // Get float values for calculations
        var f1 = c1.ToFloat();
        var f2 = c2.ToFloat();
        var f3 = c3.ToFloat();

        // Calculate difference from average
        var diff1 = f1 - avg1;
        var diff2 = f2 - avg2;
        var diff3 = f3 - avg3;

        // Apply threshold adjustment based on whether pixel is brighter or darker than average
        var adj1 = diff1 > 0 ? strength : -strength;
        var adj2 = diff2 > 0 ? strength : -strength;
        var adj3 = diff3 > 0 ? strength : -strength;

        // Create adjusted color using ColorFactory
        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(f1 + adj1),
          UNorm32.FromFloatClamped(f2 + adj2),
          UNorm32.FromFloatClamped(f3 + adj3),
          c4
        );

        indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe (float, float, float) _ComputeRegionAverageFromBuffer<TWork>(
    TWork* source,
    int sourceStride,
    int centerX,
    int centerY,
    int width,
    int endY,
    int halfSize)
    where TWork : unmanaged, IColorSpace4<TWork> {

    var sum1 = 0f;
    var sum2 = 0f;
    var sum3 = 0f;
    var count = 0;

    var startX = Math.Max(0, centerX - halfSize);
    var endX = Math.Min(width - 1, centerX + halfSize);
    var startYLocal = Math.Max(0, centerY - halfSize);
    var endYLocal = Math.Min(endY - 1, centerY + halfSize);

    for (var ry = startYLocal; ry <= endYLocal; ++ry)
    for (var rx = startX; rx <= endX; ++rx) {
      var color = source[ry * sourceStride + rx];
      var (c1, c2, c3, _) = color.ToNormalized();
      sum1 += c1.ToFloat();
      sum2 += c2.ToFloat();
      sum3 += c3.ToFloat();
      ++count;
    }

    if (count == 0)
      return (0.5f, 0.5f, 0.5f);

    return (sum1 / count, sum2 / count, sum3 / count);
  }

  #endregion

  #region pre-configured instances

  /// <summary>Default instance with 4x4 regions.</summary>
  public static AverageDitherer Default { get; } = new(4);

  /// <summary>Fine instance with 2x2 regions (more adaptive).</summary>
  public static AverageDitherer Fine { get; } = new(2);

  /// <summary>Coarse instance with 8x8 regions (less adaptive, more uniform).</summary>
  public static AverageDitherer Coarse { get; } = new(8);

  #endregion
}
