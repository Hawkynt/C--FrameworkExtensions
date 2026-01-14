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
using Hawkynt.ColorProcessing.Storage;

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
  protected override Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<(Bgra8888 color, uint count)> histogram) {
    var smallHistogram = new uint[32, 32, 32];

    foreach (var (color, count) in histogram) {
      var c1 = color.C1 >> 3;
      var c2 = color.C2 >> 3;
      var c3 = color.C3 >> 3;
      smallHistogram[c1, c2, c3] += count;
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

  private sealed class ColorCube(uint[,,] histogram, int c1Min = 0, int c1Max = 31, int c2Min = 0, int c2Max = 31, int c3Min = 0, int c3Max = 31) {
    public int Volume => (c1Max - c1Min) * (c2Max - c2Min) * (c3Max - c3Min);

    public Bgra8888 AverageColor => this._GetAverageColor();

    private Bgra8888 _GetAverageColor() {
      long c1Sum = 0, c2Sum = 0, c3Sum = 0, count = 0;

      for (var c1 = c1Min; c1 <= c1Max; ++c1)
      for (var c2 = c2Min; c2 <= c2Max; ++c2)
      for (var c3 = c3Min; c3 <= c3Max; ++c3) {
        var histCount = histogram[c1, c2, c3];
        c1Sum += (long)c1 * histCount;
        c2Sum += (long)c2 * histCount;
        c3Sum += (long)c3 * histCount;
        count += histCount;
      }

      return count == 0
        ? Bgra8888.Transparent
        : Bgra8888.Create(
          (byte)((c1Sum / count) << 3),
          (byte)((c2Sum / count) << 3),
          (byte)((c3Sum / count) << 3),
          255
        );
    }

    public IEnumerable<ColorCube> Split() {
      var c1Range = c1Max - c1Min;
      var c2Range = c2Max - c2Min;
      var c3Range = c3Max - c3Min;

      int mid;
      if (c1Range >= c2Range && c1Range >= c3Range) {
        mid = (c1Min + c1Range) >> 1;
        return [
          new(histogram, c1Min, mid, c2Min, c2Max, c3Min, c3Max),
          new(histogram, mid + 1, c1Max, c2Min, c2Max, c3Min, c3Max)
        ];
      }

      if (c2Range >= c1Range && c2Range >= c3Range) {
        mid = (c2Min + c2Range) >> 1;
        return [
          new(histogram, c1Min, c1Max, c2Min, mid, c3Min, c3Max),
          new(histogram, c1Min, c1Max, mid + 1, c2Max, c3Min, c3Max)
        ];
      }

      mid = (c3Min + c3Range) >> 1;
      return [
        new(histogram, c1Min, c1Max, c2Min, c2Max, c3Min, mid),
        new(histogram, c1Min, c1Max, c2Min, c2Max, mid + 1, c3Max)
      ];
    }
  }

}
