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
using Hawkynt.ColorProcessing.Quantization;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Color-domain wrapper around <see cref="QuantizerRegistry"/>. Resolves a registry
/// name (full or short suffix) to an <see cref="IColorQuantizer"/> ready to operate on
/// <see cref="System.Drawing.Color"/> histograms.
/// </summary>
public static class ColorQuantizerRegistry {

  /// <summary>All quantizer descriptors in the underlying registry.</summary>
  public static IEnumerable<QuantizerDescriptor> All => QuantizerRegistry.All;

  /// <summary>
  /// Resolves a quantizer by name (case-insensitive). Tries an exact match first; if
  /// none found, tries a suffix match against the registry when unambiguous.
  /// </summary>
  /// <param name="name">Registry name (e.g. <c>"Octree"</c>, <c>"Median Cut"</c>, <c>"Wu"</c>).</param>
  /// <param name="allowFillingColors">Pass-through to <see cref="ColorQuantizerAdapter"/>;
  /// when <see langword="true"/> (default), short palettes are padded with the
  /// PaletteFiller sequence.</param>
  /// <returns>The adapter, or <see langword="null"/> when nothing matches.</returns>
  /// <exception cref="ArgumentException">Thrown when a suffix match is ambiguous.</exception>
  public static IColorQuantizer? FindByName(string name, bool allowFillingColors = true) {
    var exact = QuantizerRegistry.FindByName(name);
    if (exact != null)
      return new ColorQuantizerAdapter(exact.CreateDefault(), allowFillingColors);

    var suffix = "_" + name;
    var suffixMatches = All
      .Where(q => q.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
      .ToArray();
    if (suffixMatches.Length == 1)
      return new ColorQuantizerAdapter(suffixMatches[0].CreateDefault(), allowFillingColors);
    if (suffixMatches.Length > 1)
      throw new ArgumentException(
        $"Quantizer name '{name}' is ambiguous. Candidates: {string.Join(", ", suffixMatches.Select(q => q.Name))}.");

    return null;
  }
}
