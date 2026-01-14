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
/// Implements the Median Cut quantization algorithm.
/// Recursively subdivides the color space along the axis with the largest range,
/// splitting at the median point.
/// </summary>
/// <remarks>
/// <para>Reference: P. Heckbert 1982 "Color Image Quantization for Frame Buffer Display"</para>
/// <para>SIGGRAPH '82, Boston, pp. 297-307</para>
/// <para>See also: https://en.wikipedia.org/wiki/Median_cut</para>
/// </remarks>
[Quantizer(QuantizationType.Splitting, DisplayName = "Median Cut", Author = "Paul Heckbert", Year = 1982, QualityRating = 6)]
public class MedianCutQuantizer : QuantizerBase {

  /// <inheritdoc />
  protected override Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<(Bgra8888 color, uint count)> histogram) {
    var cubes = new List<ColorCube> { new(histogram.Select(h => h.color)) };

    while (cubes.Count < colorCount) {
      var largestCube = cubes.OrderByDescending(c => c.Volume).FirstOrDefault();
      if (largestCube is not { ColorCount: > 1 })
        break;

      cubes.Remove(largestCube);
      var splitCubes = largestCube.Split();
      cubes.AddRange(splitCubes);
    }

    return cubes.Select(c => c.AverageColor).ToArray();
  }

  private sealed class ColorCube {
    private readonly List<Bgra8888> _colors;
    private readonly byte _c1Min, _c1Max, _c2Min, _c2Max, _c3Min, _c3Max;
    private readonly long _c1Sum, _c2Sum, _c3Sum;

    public ColorCube(IEnumerable<Bgra8888> colors) {
      this._colors = colors.ToList();
      if (this._colors.Count == 0) {
        this._c1Min = this._c1Max = this._c2Min = this._c2Max = this._c3Min = this._c3Max = 0;
        this._c1Sum = this._c2Sum = this._c3Sum = 0;
        return;
      }

      var first = this._colors[0];
      byte c1Min = first.C1, c1Max = first.C1;
      byte c2Min = first.C2, c2Max = first.C2;
      byte c3Min = first.C3, c3Max = first.C3;
      long c1Sum = first.C1, c2Sum = first.C2, c3Sum = first.C3;

      for (var i = 1; i < this._colors.Count; ++i) {
        var c = this._colors[i];

        if (c.C1 < c1Min) c1Min = c.C1; else if (c.C1 > c1Max) c1Max = c.C1;
        if (c.C2 < c2Min) c2Min = c.C2; else if (c.C2 > c2Max) c2Max = c.C2;
        if (c.C3 < c3Min) c3Min = c.C3; else if (c.C3 > c3Max) c3Max = c.C3;

        c1Sum += c.C1;
        c2Sum += c.C2;
        c3Sum += c.C3;
      }

      this._c1Min = c1Min; this._c1Max = c1Max;
      this._c2Min = c2Min; this._c2Max = c2Max;
      this._c3Min = c3Min; this._c3Max = c3Max;
      this._c1Sum = c1Sum; this._c2Sum = c2Sum; this._c3Sum = c3Sum;
    }

    public int Volume => (this._c1Max - this._c1Min) * (this._c2Max - this._c2Min) * (this._c3Max - this._c3Min);
    public int ColorCount => this._colors.Count;

    public Bgra8888 AverageColor => this._colors.Count == 0
      ? Bgra8888.Black
      : Bgra8888.Create(
        (byte)(this._c1Sum / this._colors.Count),
        (byte)(this._c2Sum / this._colors.Count),
        (byte)(this._c3Sum / this._colors.Count),
        255
      );

    public IEnumerable<ColorCube> Split() {
      if (this._colors.Count <= 1)
        return [this];

      var c1Range = this._c1Max - this._c1Min;
      var c2Range = this._c2Max - this._c2Min;
      var c3Range = this._c3Max - this._c3Min;

      if (c1Range >= c2Range && c1Range >= c3Range)
        this._colors.Sort((a, b) => a.C1 - b.C1);
      else if (c2Range >= c1Range && c2Range >= c3Range)
        this._colors.Sort((a, b) => a.C2 - b.C2);
      else
        this._colors.Sort((a, b) => a.C3 - b.C3);

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
