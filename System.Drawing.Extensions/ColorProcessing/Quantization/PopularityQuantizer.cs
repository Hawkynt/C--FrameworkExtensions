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
/// Implements the Popularity quantization algorithm.
/// Selects the most frequently occurring colors from the histogram.
/// </summary>
/// <remarks>
/// <para>This is one of the simplest quantization methods.</para>
/// <para>Fast and deterministic, but may not represent the full color range well.</para>
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Popularity")]
public class PopularityQuantizer : QuantizerBase {

  /// <inheritdoc />
  protected override Bgra8888[] _ReduceColorsTo(int colorCount, IEnumerable<(Bgra8888 color, uint count)> histogram)
    => histogram
      .OrderByDescending(h => h.count)
      .Take(colorCount)
      .Select(h => h.color)
      .ToArray();

}
