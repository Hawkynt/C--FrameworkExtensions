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

/// <summary>
/// Median Cut color quantizer with configurable parameters.
/// </summary>
/// <remarks>
/// Recursively subdivides the color space along the axis with the largest range,
/// splitting at the median point.
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Median Cut", Author = "Paul Heckbert", Year = 1982, QualityRating = 6)]
public struct MedianCutQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets whether to fill unused palette entries with generated colors.
  /// </summary>
  public bool AllowFillingColors { get; set; } = true;

  public MedianCutQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.AllowFillingColors);

  internal sealed class Kernel<TWork>(bool allowFillingColors) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var result = QuantizerHelper.TryHandleSimpleCases(histogram, colorCount, allowFillingColors, out var used);
      if (result != null)
        return result;

      var reduced = _ReduceColorsTo(colorCount, used);
      return PaletteFiller.GenerateFinalPalette(reduced, colorCount, allowFillingColors);
    }

    private static IEnumerable<TWork> _ReduceColorsTo(int colorCount, IEnumerable<(TWork color, uint count)> histogram) {
      var cubes = new List<ColorCube> { new(histogram.Select(h => h.color)) };

      while (cubes.Count < colorCount) {
        var largestCube = cubes.OrderByDescending(c => c.Volume).FirstOrDefault();
        if (largestCube is not { ColorCount: > 1 })
          break;

        cubes.Remove(largestCube);
        var splitCubes = largestCube.Split();
        cubes.AddRange(splitCubes);
      }

      return cubes.Select(c => c.AverageColor);
    }

    private sealed class ColorCube {
      private readonly List<TWork> _colors;
      private readonly float _c1Min, _c1Max, _c2Min, _c2Max, _c3Min, _c3Max;
      private readonly double _c1Sum, _c2Sum, _c3Sum, _aSum;

      public ColorCube(IEnumerable<TWork> colors) {
        this._colors = colors.ToList();
        if (this._colors.Count == 0) {
          this._c1Min = this._c1Max = this._c2Min = this._c2Max = this._c3Min = this._c3Max = 0;
          this._c1Sum = this._c2Sum = this._c3Sum = this._aSum = 0;
          return;
        }

        var (firstC1, firstC2, firstC3, firstA) = this._colors[0].ToNormalized();
        var c1Min = firstC1.ToFloat();
        var c1Max = c1Min;
        var c2Min = firstC2.ToFloat();
        var c2Max = c2Min;
        var c3Min = firstC3.ToFloat();
        var c3Max = c3Min;
        double c1Sum = c1Min, c2Sum = c2Min, c3Sum = c3Min, aSum = firstA.ToFloat();

        for (var i = 1; i < this._colors.Count; ++i) {
          var (c1N, c2N, c3N, aN) = this._colors[i].ToNormalized();
          var c1 = c1N.ToFloat();
          var c2 = c2N.ToFloat();
          var c3 = c3N.ToFloat();
          var a = aN.ToFloat();

          if (c1 < c1Min) c1Min = c1; else if (c1 > c1Max) c1Max = c1;
          if (c2 < c2Min) c2Min = c2; else if (c2 > c2Max) c2Max = c2;
          if (c3 < c3Min) c3Min = c3; else if (c3 > c3Max) c3Max = c3;

          c1Sum += c1;
          c2Sum += c2;
          c3Sum += c3;
          aSum += a;
        }

        this._c1Min = c1Min; this._c1Max = c1Max;
        this._c2Min = c2Min; this._c2Max = c2Max;
        this._c3Min = c3Min; this._c3Max = c3Max;
        this._c1Sum = c1Sum; this._c2Sum = c2Sum; this._c3Sum = c3Sum;
        this._aSum = aSum;
      }

      public float Volume => (this._c1Max - this._c1Min) * (this._c2Max - this._c2Min) * (this._c3Max - this._c3Min);
      public int ColorCount => this._colors.Count;

      public TWork AverageColor => this._colors.Count == 0
        ? default
        : ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)(this._c1Sum / this._colors.Count)),
          UNorm32.FromFloatClamped((float)(this._c2Sum / this._colors.Count)),
          UNorm32.FromFloatClamped((float)(this._c3Sum / this._colors.Count)),
          UNorm32.FromFloatClamped((float)(this._aSum / this._colors.Count))
        );

      public IEnumerable<ColorCube> Split() {
        if (this._colors.Count <= 1)
          return [this];

        var c1Range = this._c1Max - this._c1Min;
        var c2Range = this._c2Max - this._c2Min;
        var c3Range = this._c3Max - this._c3Min;

        if (c1Range >= c2Range && c1Range >= c3Range)
          this._colors.Sort((a, b) => a.ToNormalized().C1.CompareTo(b.ToNormalized().C1));
        else if (c2Range >= c1Range && c2Range >= c3Range)
          this._colors.Sort((a, b) => a.ToNormalized().C2.CompareTo(b.ToNormalized().C2));
        else
          this._colors.Sort((a, b) => a.ToNormalized().C3.CompareTo(b.ToNormalized().C3));

        var medianIndex = this._colors.Count >> 1;

        if (medianIndex == 0)
          medianIndex = 1;
        if (medianIndex >= this._colors.Count)
          medianIndex = this._colors.Count - 1;

        return [
          new(this._colors.Take(medianIndex)),
          new(this._colors.Skip(medianIndex))
        ];
      }
    }
  }
}
