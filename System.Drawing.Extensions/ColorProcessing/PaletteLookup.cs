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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Provides nearest-neighbor lookup for a color palette with optional caching.
/// </summary>
/// <typeparam name="TWork">The working color space type.</typeparam>
/// <typeparam name="TMetric">The color distance metric type.</typeparam>
/// <remarks>
/// <para>Wraps a palette array and metric to provide efficient nearest-color lookup.</para>
/// <para>When caching is enabled, repeated lookups of the same color are O(1).</para>
/// <para>Use <see cref="FindNearest"/> to get the index of the closest palette color.</para>
/// </remarks>
public readonly struct PaletteLookup<TWork, TMetric>
  where TWork : unmanaged, IColorSpace
  where TMetric : struct, IColorMetric<TWork> {

  private readonly TWork[] _palette;
  private readonly TMetric _metric;
  private readonly Dictionary<TWork, int> _cache;
  
  /// <summary>
  /// Creates a new palette lookup with optional caching.
  /// </summary>
  /// <param name="palette">The palette colors in TWork space.</param>
  /// <param name="metric">The distance metric.</param>
  /// <param name="useCache">If true, caches lookup results for repeated colors.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PaletteLookup(TWork[] palette, TMetric metric) {
    this._palette = palette;
    this._metric = metric;
      // Pre-populate cache with palette colors for O(1) exact match lookups
    this._cache = new(palette.Length);
    for (var i = 0; i < palette.Length; ++i)
      this._cache[palette[i]] = i;
  }

  /// <summary>
  /// Gets the palette color at the specified index.
  /// </summary>
  public TWork this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._palette[index];
  }

  /// <summary>
  /// Gets the number of colors in the palette.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._palette.Length;
  }

  /// <summary>
  /// Finds the index of the nearest palette color.
  /// </summary>
  /// <param name="color">The color to match.</param>
  /// <returns>The zero-based index of the closest palette color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindNearest(in TWork color) {
    // Check cache first
    if (this._cache.TryGetValue(color, out var cached))
      return cached;

    // Linear search for nearest
    var palette = this._palette;
    var metric = this._metric;
    var minDist = UNorm32.One;
    var nearestIdx = 0;

    for (var i = 0; i < palette.Length; ++i) {
      var dist = metric.Distance(color, palette[i]);
      if (dist >= minDist)
        continue;

      minDist = dist;
      nearestIdx = i;

      // Early exit on exact match
      if (dist == UNorm32.Zero)
        break;
    }

    // Cache result
    this._cache.TryAdd(color, nearestIdx);
    return nearestIdx;
  }

  /// <summary>
  /// Finds the nearest palette color.
  /// </summary>
  /// <param name="color">The color to match.</param>
  /// <returns>The closest palette color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork FindNearestColor(in TWork color) => this._palette[this.FindNearest(color)];

  /// <summary>
  /// Finds the nearest palette color and returns both index and color.
  /// </summary>
  /// <param name="color">The color to match.</param>
  /// <param name="nearestColor">The closest palette color.</param>
  /// <returns>The zero-based index of the closest palette color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindNearest(in TWork color, out TWork nearestColor) {
    var idx = this.FindNearest(color);
    nearestColor = this._palette[idx];
    return idx;
  }
}
