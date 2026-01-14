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
using Hawkynt.ColorProcessing.Storage;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Implements the Uniform quantization algorithm.
/// Divides the RGB color space into a uniform grid.
/// </summary>
/// <remarks>
/// <para>Creates evenly spaced colors regardless of the input image.</para>
/// <para>Very fast but ignores the actual color distribution.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Uniform")]
public class UniformQuantizer : QuantizerBase {

  /// <inheritdoc />
  protected override Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<(Bgra8888 color, uint count)> histogram) {
    var levelsPerChannel = (int)Math.Ceiling(Math.Pow(colorCount, 1.0 / 3.0));
    levelsPerChannel = Math.Max(2, Math.Min(levelsPerChannel, 8));

    var step = 255.0 / (levelsPerChannel - 1);
    var result = new List<Bgra8888>(levelsPerChannel * levelsPerChannel * levelsPerChannel);

    for (var c1 = 0; c1 < levelsPerChannel; ++c1)
    for (var c2 = 0; c2 < levelsPerChannel; ++c2)
    for (var c3 = 0; c3 < levelsPerChannel; ++c3) {
      result.Add(Bgra8888.Create(
        (byte)(c1 * step + 0.5),
        (byte)(c2 * step + 0.5),
        (byte)(c3 * step + 0.5),
        255
      ));

      if (result.Count >= colorCount)
        return result.ToArray();
    }

    return result.ToArray();
  }

}
