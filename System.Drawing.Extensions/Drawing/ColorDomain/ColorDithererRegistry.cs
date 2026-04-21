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
using Hawkynt.ColorProcessing.Dithering;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Color-domain wrapper around <see cref="DithererRegistry"/>. Resolves a registry
/// name (full or short suffix) to an <see cref="IColorDitherer"/> instance ready to use
/// against <see cref="System.Drawing.Color"/> palettes.
/// </summary>
public static class ColorDithererRegistry {

  /// <summary>All ditherer descriptors in the underlying registry.</summary>
  public static IEnumerable<DithererDescriptor> All => DithererRegistry.All;

  /// <summary>
  /// Resolves a ditherer by name. Tries an exact match first; if none found, tries a
  /// suffix match against the registry (e.g. <c>"FloydSteinberg"</c> → <c>"ErrorDiffusion_FloydSteinberg"</c>)
  /// when unambiguous. Returns <see langword="null"/> when nothing resolves.
  /// </summary>
  /// <exception cref="ArgumentException">Thrown when a suffix match is ambiguous
  /// (multiple registry entries end with <c>_<paramref name="name"/></c>).</exception>
  public static IColorDitherer? FindByName(string name) {
    var exact = DithererRegistry.FindByName(name);
    if (exact != null)
      return new ColorDithererAdapter(exact.CreateDefault());

    var suffix = "_" + name;
    var suffixMatches = All
      .Where(d => d.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
      .ToArray();
    if (suffixMatches.Length == 1)
      return new ColorDithererAdapter(suffixMatches[0].CreateDefault());
    if (suffixMatches.Length > 1)
      throw new ArgumentException(
        $"Ditherer name '{name}' is ambiguous. Candidates: {string.Join(", ", suffixMatches.Select(d => d.Name))}.");

    return null;
  }
}
