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
/// Wu's color quantizer with configurable parameters.
/// </summary>
/// <remarks>
/// Minimizes the weighted variance of color distribution.
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Wu", Author = "Xiaolin Wu", Year = 1991, QualityRating = 9)]
public struct WuQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets whether to fill unused palette entries with generated colors.
  /// </summary>
  public bool AllowFillingColors { get; set; } = true;

  public WuQuantizer() { }

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
      var smallHistogram = new HistogramEntry[32, 32, 32];

      foreach (var (color, count) in histogram) {
        var (c1N, c2N, c3N, aN) = color.ToNormalized();
        var c1 = _FloatToIndex(c1N.ToFloat());
        var c2 = _FloatToIndex(c2N.ToFloat());
        var c3 = _FloatToIndex(c3N.ToFloat());

        ref var entry = ref smallHistogram[c1, c2, c3];
        entry.Count += count;
        entry.C1Sum += c1N.ToFloat() * count;
        entry.C2Sum += c2N.ToFloat() * count;
        entry.C3Sum += c3N.ToFloat() * count;
        entry.ASum += aN.ToFloat() * count;
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

      return cubes.Select(c => c.AverageColor);
    }

    private static int _FloatToIndex(float value) => Math.Max(0, Math.Min(31, (int)(value * 31.0f + 0.5f)));

    private struct HistogramEntry {
      public uint Count;
      public double C1Sum;
      public double C2Sum;
      public double C3Sum;
      public double ASum;
    }

    private sealed class ColorCube {
      private readonly HistogramEntry[,,] _histogram;
      private readonly int _c1Min, _c1Max, _c2Min, _c2Max, _c3Min, _c3Max;

      public ColorCube(HistogramEntry[,,] histogram, int c1Min = 0, int c1Max = 31, int c2Min = 0, int c2Max = 31, int c3Min = 0, int c3Max = 31) {
        this._histogram = histogram;
        this._c1Min = c1Min;
        this._c1Max = c1Max;
        this._c2Min = c2Min;
        this._c2Max = c2Max;
        this._c3Min = c3Min;
        this._c3Max = c3Max;
      }

      public int Volume => (this._c1Max - this._c1Min) * (this._c2Max - this._c2Min) * (this._c3Max - this._c3Min);

      public TWork AverageColor => this._GetAverageColor();

      private TWork _GetAverageColor() {
        double c1Sum = 0, c2Sum = 0, c3Sum = 0, aSum = 0;
        ulong count = 0;

        for (var c1 = this._c1Min; c1 <= this._c1Max; ++c1)
        for (var c2 = this._c2Min; c2 <= this._c2Max; ++c2)
        for (var c3 = this._c3Min; c3 <= this._c3Max; ++c3) {
          ref var entry = ref this._histogram[c1, c2, c3];
          c1Sum += entry.C1Sum;
          c2Sum += entry.C2Sum;
          c3Sum += entry.C3Sum;
          aSum += entry.ASum;
          count += entry.Count;
        }

        return count == 0
          ? default
          : ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)(c1Sum / count)),
            UNorm32.FromFloatClamped((float)(c2Sum / count)),
            UNorm32.FromFloatClamped((float)(c3Sum / count)),
            UNorm32.FromFloatClamped((float)(aSum / count))
          );
      }

      public IEnumerable<ColorCube> Split() {
        var c1Range = this._c1Max - this._c1Min;
        var c2Range = this._c2Max - this._c2Min;
        var c3Range = this._c3Max - this._c3Min;

        int mid;
        if (c1Range >= c2Range && c1Range >= c3Range) {
          mid = (this._c1Min + c1Range) >> 1;
          return [
            new(this._histogram, this._c1Min, mid, this._c2Min, this._c2Max, this._c3Min, this._c3Max),
            new(this._histogram, mid + 1, this._c1Max, this._c2Min, this._c2Max, this._c3Min, this._c3Max)
          ];
        }

        if (c2Range >= c1Range && c2Range >= c3Range) {
          mid = (this._c2Min + c2Range) >> 1;
          return [
            new(this._histogram, this._c1Min, this._c1Max, this._c2Min, mid, this._c3Min, this._c3Max),
            new(this._histogram, this._c1Min, this._c1Max, mid + 1, this._c2Max, this._c3Min, this._c3Max)
          ];
        }

        mid = (this._c3Min + c3Range) >> 1;
        return [
          new(this._histogram, this._c1Min, this._c1Max, this._c2Min, this._c2Max, this._c3Min, mid),
          new(this._histogram, this._c1Min, this._c1Max, this._c2Min, this._c2Max, mid + 1, this._c3Max)
        ];
      }
    }
  }
}
