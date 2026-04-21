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

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Color-domain analogue of <see cref="Hawkynt.ColorProcessing.PaletteLookup{TWork, TMetric}"/>.
/// Caches lookups so repeated probes for the same color are O(1).
/// </summary>
/// <remarks>
/// Designed for tools that need to map arbitrary <see cref="Color"/> inputs onto a fixed
/// indexed palette (e.g. back-filling sub-image regions, building indexed bitmaps with a
/// runtime metric). Use <see cref="ColorMetric"/> via <see cref="ColorMetricExtensions.AsFunc"/>
/// or supply your own <see cref="Func{Color, Color, Int32}"/>.
/// </remarks>
public sealed class PaletteLookup {

  private readonly Color[] _palette;
  private readonly Dictionary<int, int> _cache;
  private readonly Func<Color, Color, int> _metric;

  public PaletteLookup(IEnumerable<Color> palette, Func<Color, Color, int>? metric = null) {
    if (palette == null) throw new ArgumentNullException(nameof(palette));
    var list = new List<Color>();
    foreach (var c in palette)
      list.Add(c);
    this._palette = list.ToArray();
    this._cache = new Dictionary<int, int>(512);
    this._metric = metric ?? _DefaultCompuPhase;
  }

  /// <summary>Number of palette entries.</summary>
  public int Count => this._palette.Length;

  /// <summary>The palette entry at <paramref name="index"/>.</summary>
  public Color this[int index] => this._palette[index];

  /// <summary>
  /// Returns the index of the palette entry closest to <paramref name="color"/> under
  /// the configured metric. Repeated calls for the same color hit the cache.
  /// </summary>
  /// <returns>Index in <c>[0, Count)</c>, or <c>-1</c> when the palette is empty.</returns>
  public int FindClosestColorIndex(Color color) {
    if (this._palette.Length == 0)
      return -1;

    var key = color.ToArgb();
    if (this._cache.TryGetValue(key, out var cached))
      return cached;

    var bestIndex = 0;
    var bestDistance = this._metric(color, this._palette[0]);
    for (var i = 1; i < this._palette.Length; ++i) {
      var d = this._metric(color, this._palette[i]);
      if (d >= bestDistance)
        continue;
      bestDistance = d;
      bestIndex = i;
      if (d <= 1)
        break;
    }

    lock (this._cache)
      this._cache[key] = bestIndex;

    return bestIndex;
  }

  /// <summary>
  /// Default low-cost RGB metric (https://www.compuphase.com/cmetric.htm). Used when
  /// the consumer doesn't pass an explicit metric.
  /// </summary>
  private static readonly Func<Color, Color, int> _DefaultCompuPhase = ColorMetric.CompuPhase.AsFunc();
}
