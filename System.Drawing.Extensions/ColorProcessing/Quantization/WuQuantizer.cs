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
using System.Drawing;
using System.Linq;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Implements Wu's color quantization algorithm (Greedy Orthogonal Bi-Partitioning).
/// Uses a reduced 32x32x32 color space histogram for fast computation,
/// splitting along the axis with the largest range.
/// </summary>
/// <remarks>
/// <para>Reference: X. Wu 1991 "Efficient Statistical Computations for Optimal Color Quantization"</para>
/// <para>Graphics Gems II, Academic Press, pp. 126-133</para>
/// <para>Original implementation: http://www.ece.mcmaster.ca/~xwu/cq.c</para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Wu", Author = "Xiaolin Wu", Year = 1991, QualityRating = 9)]
public class WuQuantizer : QuantizerBase {

  /// <inheritdoc />
  protected override Color[] _ReduceColorsTo(int colorCount, IEnumerable<(Color color, uint count)> histogram) {
    var smallHistogram = new uint[32, 32, 32];

    foreach (var (color, count) in histogram) {
      var r = color.R >> 3;
      var g = color.G >> 3;
      var b = color.B >> 3;
      smallHistogram[r, g, b] += count;
    }

    var cubes = new List<ColorCube> { new(smallHistogram) };
    while (cubes.Count < colorCount) {
      var largestCube = cubes.OrderByDescending(c => c.Volume).First();
      if (largestCube.Volume <= 0)
        break;

      cubes.Remove(largestCube);
      var splitCubes = largestCube.Split();
      cubes.AddRange(splitCubes);
    }

    return cubes.Select(c => c.AverageColor).ToArray();
  }

  private sealed class ColorCube(uint[,,] histogram, int rMin = 0, int rMax = 31, int gMin = 0, int gMax = 31, int bMin = 0, int bMax = 31) {
    public int Volume => (rMax - rMin) * (gMax - gMin) * (bMax - bMin);

    public Color AverageColor => this._GetAverageColor();

    private Color _GetAverageColor() {
      long rSum = 0, gSum = 0, bSum = 0, count = 0;

      for (var r = rMin; r <= rMax; ++r)
      for (var g = gMin; g <= gMax; ++g)
      for (var b = bMin; b <= bMax; ++b) {
        var histCount = histogram[r, g, b];
        rSum += (long)r * histCount;
        gSum += (long)g * histCount;
        bSum += (long)b * histCount;
        count += histCount;
      }

      return count == 0
        ? Color.Transparent
        : Color.FromArgb(
          (int)(rSum / count) << 3,
          (int)(gSum / count) << 3,
          (int)(bSum / count) << 3
        );
    }

    public IEnumerable<ColorCube> Split() {
      var rRange = rMax - rMin;
      var gRange = gMax - gMin;
      var bRange = bMax - bMin;

      int mid;
      if (rRange >= gRange && rRange >= bRange) {
        mid = (rMin + rRange) >> 1;
        return [
          new(histogram, rMin, mid, gMin, gMax, bMin, bMax),
          new(histogram, mid + 1, rMax, gMin, gMax, bMin, bMax)
        ];
      }

      if (gRange >= rRange && gRange >= bRange) {
        mid = (gMin + gRange) >> 1;
        return [
          new(histogram, rMin, rMax, gMin, mid, bMin, bMax),
          new(histogram, rMin, rMax, mid + 1, gMax, bMin, bMax)
        ];
      }

      mid = (bMin + bRange) >> 1;
      return [
        new(histogram, rMin, rMax, gMin, gMax, bMin, mid),
        new(histogram, rMin, rMax, gMin, gMax, mid + 1, bMax)
      ];
    }
  }

}
