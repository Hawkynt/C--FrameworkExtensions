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
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces;

/// <summary>
/// A palette wrapper that caches color-to-index lookups for fast repeated queries.
/// </summary>
/// <typeparam name="TDistanceCalculator">
/// The type of distance calculator to use. Must be a struct implementing <see cref="IColorDistanceCalculator"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// For image processing where millions of pixels need to be mapped to palette indices,
/// the same colors often appear repeatedly. This class caches lookup results to eliminate
/// redundant distance calculations.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var palette = new[] { Color.Red, Color.Green, Color.Blue, Color.White, Color.Black };
/// var cached = new CachedPalette&lt;EuclideanDistance&lt;Yuv&gt;&gt;(palette);
///
/// // First call calculates distance, subsequent calls are O(1)
/// foreach (var pixel in imagePixels) {
///   var paletteIndex = cached.GetIndex(pixel);
///   // ... use index
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class CachedPalette<TDistanceCalculator>
  where TDistanceCalculator : struct, IColorDistanceCalculator {
  private readonly Dictionary<int, int> _cache;

  /// <summary>
  /// Initializes a new instance of the <see cref="CachedPalette{TDistanceCalculator}"/> class.
  /// </summary>
  /// <param name="palette">The palette of colors to search.</param>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">Thrown when <paramref name="palette"/> is empty.</exception>
  public CachedPalette(Color[] palette) {
    Against.ArgumentIsNull(palette);
    if (palette.Length == 0)
      throw new ArgumentException("Palette cannot be empty", nameof(palette));

    this.Palette = palette;
    this._cache = new();
  }

  /// <summary>
  /// Gets the palette associated with this cache.
  /// </summary>
  public Color[] Palette { get; }

  /// <summary>
  /// Gets the number of unique colors currently cached.
  /// </summary>
  public int CacheSize => this._cache.Count;

  /// <summary>
  /// Gets the index of the most similar color in the palette to the target color.
  /// Results are cached for O(1) lookup on subsequent calls with the same color.
  /// </summary>
  /// <param name="color">The color to find the closest match for.</param>
  /// <returns>The index of the most similar color in the palette.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int GetIndex(Color color) {
    var key = color.ToArgb();
    if (this._cache.TryGetValue(key, out var index))
      return index;

    index = PaletteSearch.GetMostSimilarColorIndex<TDistanceCalculator>(this.Palette, color);
    this._cache[key] = index;
    return index;
  }

  /// <summary>
  /// Gets the most similar color in the palette to the target color.
  /// Results are cached for O(1) lookup on subsequent calls with the same color.
  /// </summary>
  /// <param name="color">The color to find the closest match for.</param>
  /// <returns>The most similar color in the palette.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Color GetColor(Color color) => this.Palette[this.GetIndex(color)];

  /// <summary>
  /// Clears the lookup cache.
  /// </summary>
  /// <remarks>
  /// Use this if the palette contents are modified after construction
  /// (though it's generally better to create a new instance).
  /// </remarks>
  public void ClearCache() => this._cache.Clear();

}
