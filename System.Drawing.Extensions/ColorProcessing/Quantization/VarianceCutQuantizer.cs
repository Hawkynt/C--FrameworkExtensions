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
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Variance Cut color quantizer using maximum variance splitting.
/// </summary>
/// <remarks>
/// <para>
/// Recursively splits the color cube with the highest sum of squared error (SSE),
/// subdividing along the axis with maximum variance.
/// </para>
/// <para>
/// This produces better visual quality than simple median cut by prioritizing
/// regions with the most color variation.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Variance, DisplayName = "Variance Cut", QualityRating = 7)]
public struct VarianceCutQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, _ReduceColorsTo);

    private static IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var cubes = new List<ColorCube> { new(histogram.ToList()) };

      while (cubes.Count < colorCount) {
        // Find cube with highest sum of squared error
        var largestCube = cubes.OrderByDescending(c => c.SumOfSquaredError).FirstOrDefault();
        if (largestCube is not { ColorCount: > 1 })
          break;

        cubes.Remove(largestCube);
        var splitCubes = largestCube.Split();
        cubes.AddRange(splitCubes);
      }

      return cubes.Select(c => c.AverageColor);
    }

    private sealed class ColorCube {
      private readonly List<(TWork color, uint count)> _colors;
      private readonly float _avgC1, _avgC2, _avgC3, _avgA;
      private readonly double _varC1, _varC2, _varC3;
      private readonly double _sse;
      private readonly long _totalCount;

      public ColorCube(List<(TWork color, uint count)> colors) {
        this._colors = colors;

        if (colors.Count == 0) {
          this._avgC1 = this._avgC2 = this._avgC3 = this._avgA = 0;
          this._varC1 = this._varC2 = this._varC3 = 0;
          this._sse = 0;
          this._totalCount = 0;
          return;
        }

        // Calculate weighted sums for average
        double sumC1 = 0, sumC2 = 0, sumC3 = 0, sumA = 0;
        long totalCount = 0;

        foreach (var (color, count) in colors) {
          var (c1N, c2N, c3N, aN) = color.ToNormalized();
          var c1 = c1N.ToFloat();
          var c2 = c2N.ToFloat();
          var c3 = c3N.ToFloat();
          var a = aN.ToFloat();

          sumC1 += c1 * count;
          sumC2 += c2 * count;
          sumC3 += c3 * count;
          sumA += a * count;
          totalCount += count;
        }

        this._totalCount = totalCount;
        if (totalCount == 0) {
          this._avgC1 = this._avgC2 = this._avgC3 = this._avgA = 0;
          this._varC1 = this._varC2 = this._varC3 = 0;
          this._sse = 0;
          return;
        }

        this._avgC1 = (float)(sumC1 / totalCount);
        this._avgC2 = (float)(sumC2 / totalCount);
        this._avgC3 = (float)(sumC3 / totalCount);
        this._avgA = (float)(sumA / totalCount);

        // Calculate variance and SSE
        double varC1 = 0, varC2 = 0, varC3 = 0, sse = 0;
        foreach (var (color, count) in colors) {
          var (c1N, c2N, c3N, _) = color.ToNormalized();
          var c1 = c1N.ToFloat();
          var c2 = c2N.ToFloat();
          var c3 = c3N.ToFloat();

          var diffC1 = c1 - this._avgC1;
          var diffC2 = c2 - this._avgC2;
          var diffC3 = c3 - this._avgC3;

          varC1 += diffC1 * diffC1 * count;
          varC2 += diffC2 * diffC2 * count;
          varC3 += diffC3 * diffC3 * count;
          sse += (diffC1 * diffC1 + diffC2 * diffC2 + diffC3 * diffC3) * count;
        }

        this._varC1 = varC1 / totalCount;
        this._varC2 = varC2 / totalCount;
        this._varC3 = varC3 / totalCount;
        this._sse = sse;
      }

      public double SumOfSquaredError => this._sse;
      public int ColorCount => this._colors.Count;

      public TWork AverageColor => this._colors.Count == 0
        ? default
        : ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(this._avgC1),
          UNorm32.FromFloatClamped(this._avgC2),
          UNorm32.FromFloatClamped(this._avgC3),
          UNorm32.FromFloatClamped(this._avgA)
        );

      public IEnumerable<ColorCube> Split() {
        if (this._colors.Count <= 1)
          return [this];

        // Find axis with maximum variance
        Func<(TWork color, uint count), float> getComponent;
        if (this._varC1 >= this._varC2 && this._varC1 >= this._varC3)
          getComponent = item => item.color.ToNormalized().C1.ToFloat();
        else if (this._varC2 >= this._varC1 && this._varC2 >= this._varC3)
          getComponent = item => item.color.ToNormalized().C2.ToFloat();
        else
          getComponent = item => item.color.ToNormalized().C3.ToFloat();

        // Sort by selected component
        this._colors.Sort((a, b) => getComponent(a).CompareTo(getComponent(b)));

        // Find split point based on mean value along the axis
        float meanValue;
        if (this._varC1 >= this._varC2 && this._varC1 >= this._varC3)
          meanValue = this._avgC1;
        else if (this._varC2 >= this._varC1 && this._varC2 >= this._varC3)
          meanValue = this._avgC2;
        else
          meanValue = this._avgC3;

        var splitIndex = this._colors.FindIndex(item => getComponent(item) >= meanValue);

        // Fallback to median split if mean-based split doesn't work
        if (splitIndex <= 0 || splitIndex >= this._colors.Count)
          splitIndex = this._colors.Count / 2;

        // Ensure we make progress
        if (splitIndex == 0)
          splitIndex = 1;
        if (splitIndex >= this._colors.Count)
          splitIndex = this._colors.Count - 1;

        return [
          new(this._colors.Take(splitIndex).ToList()),
          new(this._colors.Skip(splitIndex).ToList())
        ];
      }
    }
  }
}
