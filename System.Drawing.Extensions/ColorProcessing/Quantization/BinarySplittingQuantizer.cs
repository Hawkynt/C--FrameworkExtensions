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
/// Binary splitting color quantizer using variance-based principal axis splitting.
/// </summary>
/// <remarks>
/// <para>
/// Splits color cubes along the principal axis (direction of maximum variance).
/// This is a simplified implementation that uses the component with maximum variance
/// as an approximation of the principal eigenvector.
/// </para>
/// <para>
/// Full PCA-based binary splitting requires eigenvalue decomposition support.
/// This simplified approach provides good results for most use cases.
/// </para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Binary Splitting", QualityRating = 6)]
public struct BinarySplittingQuantizer : IQuantizer {

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
        // Find cube with highest total variance
        var largestCube = cubes.OrderByDescending(c => c.TotalVariance).FirstOrDefault();
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
      private readonly double _totalVariance;
      private readonly long _totalCount;

      public ColorCube(List<(TWork color, uint count)> colors) {
        this._colors = colors;

        if (colors.Count == 0) {
          this._avgC1 = this._avgC2 = this._avgC3 = this._avgA = 0;
          this._varC1 = this._varC2 = this._varC3 = 0;
          this._totalVariance = 0;
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
          this._totalVariance = 0;
          return;
        }

        this._avgC1 = (float)(sumC1 / totalCount);
        this._avgC2 = (float)(sumC2 / totalCount);
        this._avgC3 = (float)(sumC3 / totalCount);
        this._avgA = (float)(sumA / totalCount);

        // Calculate variance for each component
        double varC1 = 0, varC2 = 0, varC3 = 0;
        foreach (var (color, count) in colors) {
          var (c1N, c2N, c3N, _) = color.ToNormalized();
          var c1 = c1N.ToFloat();
          var c2 = c2N.ToFloat();
          var c3 = c3N.ToFloat();

          varC1 += Math.Pow(c1 - this._avgC1, 2) * count;
          varC2 += Math.Pow(c2 - this._avgC2, 2) * count;
          varC3 += Math.Pow(c3 - this._avgC3, 2) * count;
        }

        this._varC1 = varC1 / totalCount;
        this._varC2 = varC2 / totalCount;
        this._varC3 = varC3 / totalCount;
        this._totalVariance = this._varC1 + this._varC2 + this._varC3;
      }

      public double TotalVariance => this._totalVariance;
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

        // Find axis with maximum variance (approximates principal eigenvector)
        Func<(TWork color, uint count), float> getComponent;
        float meanValue;

        if (this._varC1 >= this._varC2 && this._varC1 >= this._varC3) {
          getComponent = item => item.color.ToNormalized().C1.ToFloat();
          meanValue = this._avgC1;
        } else if (this._varC2 >= this._varC1 && this._varC2 >= this._varC3) {
          getComponent = item => item.color.ToNormalized().C2.ToFloat();
          meanValue = this._avgC2;
        } else {
          getComponent = item => item.color.ToNormalized().C3.ToFloat();
          meanValue = this._avgC3;
        }

        // Split colors at the mean value along the principal axis
        var firstHalf = new List<(TWork color, uint count)>();
        var secondHalf = new List<(TWork color, uint count)>();

        foreach (var item in this._colors) {
          if (getComponent(item) < meanValue)
            firstHalf.Add(item);
          else
            secondHalf.Add(item);
        }

        // Ensure both halves are non-empty
        if (firstHalf.Count == 0 || secondHalf.Count == 0)
          return [this];

        return [new ColorCube(firstHalf), new ColorCube(secondHalf)];
      }
    }
  }
}
