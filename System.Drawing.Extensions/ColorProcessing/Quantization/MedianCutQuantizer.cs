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
using System.Drawing;
using System.Linq;

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
  protected override Color[] _ReduceColorsTo(int colorCount, IEnumerable<(Color color, uint count)> histogram) {
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

  private sealed class ColorCube(IEnumerable<Color> colors) {
    private readonly List<Color> _colors = colors.ToList();

    public int Volume => this._GetVolume();
    public int ColorCount => this._colors.Count;
    public Color AverageColor => this._GetAverageColor();

    private int _GetVolume() {
      if (this._colors.Count == 0)
        return 0;

      var rMin = this._colors.Min(c => c.R);
      var rMax = this._colors.Max(c => c.R);
      var gMin = this._colors.Min(c => c.G);
      var gMax = this._colors.Max(c => c.G);
      var bMin = this._colors.Min(c => c.B);
      var bMax = this._colors.Max(c => c.B);

      return (rMax - rMin) * (gMax - gMin) * (bMax - bMin);
    }

    private Color _GetAverageColor() {
      if (this._colors.Count == 0)
        return Color.Black;

      var r = (int)this._colors.Average(c => c.R);
      var g = (int)this._colors.Average(c => c.G);
      var b = (int)this._colors.Average(c => c.B);

      return Color.FromArgb(r, g, b);
    }

    public IEnumerable<ColorCube> Split() {
      if (this._colors.Count <= 1)
        return [this];

      var rRange = this._colors.Max(c => c.R) - this._colors.Min(c => c.R);
      var gRange = this._colors.Max(c => c.G) - this._colors.Min(c => c.G);
      var bRange = this._colors.Max(c => c.B) - this._colors.Min(c => c.B);

      Func<Color, int> getComponent;
      if (rRange >= gRange && rRange >= bRange)
        getComponent = c => c.R;
      else if (gRange >= rRange && gRange >= bRange)
        getComponent = c => c.G;
      else
        getComponent = c => c.B;

      this._colors.Sort((c1, c2) => getComponent(c1).CompareTo(getComponent(c2)));
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
