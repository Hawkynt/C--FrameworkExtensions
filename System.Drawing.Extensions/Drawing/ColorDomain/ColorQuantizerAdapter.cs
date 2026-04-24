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
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Storage;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Wraps any <see cref="Hawkynt.ColorProcessing.IQuantizer"/> and exposes it via the
/// Color-domain <see cref="IColorQuantizer"/> contract.
/// </summary>
/// <remarks>
/// Internally:
/// <list type="bullet">
/// <item>Aggregates input by ARGB to deduplicate the histogram.</item>
/// <item>Converts each entry to <see cref="Bgra8888"/>.</item>
/// <item>Calls <see cref="IQuantizer.CreateKernel{TWork}"/> with <c>TWork = Bgra8888</c>
/// to obtain the quantizer's worker.</item>
/// <item>Calls <see cref="IQuantizer{TWork}.GeneratePalette"/> to produce the
/// algorithm's raw palette.</item>
/// <item>Pads the palette to <paramref name="numberOfColors"/> using
/// <see cref="PaletteFiller.GenerateFinalPalette{TWork}"/> when
/// <see cref="AllowFillingColors"/> is <see langword="true"/>; otherwise unused slots
/// become fully transparent.</item>
/// </list>
/// </remarks>
public sealed class ColorQuantizerAdapter : IColorQuantizer {

  private readonly Hawkynt.ColorProcessing.IQuantizer _inner;

  public ColorQuantizerAdapter(Hawkynt.ColorProcessing.IQuantizer inner, bool allowFillingColors = true) {
    this._inner = inner ?? throw new ArgumentNullException(nameof(inner));
    this.AllowFillingColors = allowFillingColors;
  }

  /// <summary>The wrapped extension quantizer.</summary>
  public Hawkynt.ColorProcessing.IQuantizer Inner => this._inner;

  /// <summary>
  /// When <see langword="true"/> (default), short palettes are padded with the
  /// PaletteFiller sequence (Black, White, Transparent, primaries, hash). When
  /// <see langword="false"/>, unused slots become fully transparent.
  /// </summary>
  public bool AllowFillingColors { get; }

  public Color[] ReduceColorsTo(ushort numberOfColors, IEnumerable<Color> usedColors)
    => this.ReduceColorsTo(numberOfColors, usedColors.Select(c => (c, 1u)));

  public Color[] ReduceColorsTo(ushort numberOfColors, IEnumerable<(Color color, uint count)> histogram) {
    if (numberOfColors == 0)
      return [];

    var bgraHistogram = histogram
      .GroupBy(h => h.color.ToArgb())
      .Select(g => (color: new Bgra8888(g.First().color), count: (uint)g.Sum(h => h.count)))
      .ToArray();

    if (bgraHistogram.Length == 0)
      return [];

    var kernel = this._inner.CreateKernel<Bgra8888>();
    var rawPalette = kernel.GeneratePalette(bgraHistogram, numberOfColors);
    var filled = PaletteFiller.GenerateFinalPalette(rawPalette, numberOfColors, this.AllowFillingColors);
    return Array.ConvertAll(filled, b => b.ToColor());
  }
}
