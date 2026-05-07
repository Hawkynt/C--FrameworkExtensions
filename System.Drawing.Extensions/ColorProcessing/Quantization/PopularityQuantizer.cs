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
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Popularity-based color quantizer with near-duplicate suppression.
/// </summary>
/// <remarks>
/// Selects the most frequently occurring colors per Heckbert 1982 §4.1, with the
/// minimum-distance gate the paper specifies — "with care to avoid choosing two colors
/// that are too close together". Without that gate, top-N selection wastes palette
/// slots on perceptually-identical neighbours when an image has a dominant gradient.
/// </remarks>
[Quantizer(QuantizationType.Fixed, DisplayName = "Popularity", QualityRating = 3)]
public struct PopularityQuantizer : IQuantizer {

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>();

  internal sealed class Kernel<TWork> : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    // Minimum squared per-channel distance between any two palette entries. ~6/255
    // squared ≈ 5.5e-4 catches imperceptible duplicates (ΔE ≈ 1) without rejecting
    // legitimate distinct hues.
    private const float MinSquaredDistance = 5.5e-4f;

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount,
        (count, used) => _SelectTopWithMinDistance(used, count));

    private static IEnumerable<TWork> _SelectTopWithMinDistance(
        IEnumerable<(TWork color, uint count)> histogram, int count) {
      var sorted = histogram.OrderByDescending(h => h.count).ToList();
      var picked = new List<TWork>(count);
      foreach (var (color, _) in sorted) {
        if (picked.Count >= count) break;
        var tooClose = false;
        var n1 = color.ToNormalized();
        foreach (var p in picked) {
          var n2 = p.ToNormalized();
          var dr = n1.C1.ToFloat() - n2.C1.ToFloat();
          var dg = n1.C2.ToFloat() - n2.C2.ToFloat();
          var db = n1.C3.ToFloat() - n2.C3.ToFloat();
          var da = n1.A.ToFloat() - n2.A.ToFloat();
          if (dr * dr + dg * dg + db * db + da * da < MinSquaredDistance) {
            tooClose = true;
            break;
          }
        }
        if (!tooClose) picked.Add(color);
      }
      // If suppression left us short of the target, top up from the remaining sorted list.
      if (picked.Count < count) {
        foreach (var (color, _) in sorted) {
          if (picked.Count >= count) break;
          if (!picked.Any(p => System.Collections.Generic.EqualityComparer<TWork>.Default.Equals(p, color)))
            picked.Add(color);
        }
      }
      return picked;
    }
  }
}
