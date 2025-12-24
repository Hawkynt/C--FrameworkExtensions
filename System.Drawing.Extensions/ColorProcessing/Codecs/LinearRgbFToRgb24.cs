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

using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Codecs;

/// <summary>
/// Encodes linear LinearRgbF to sRGB Rgb24 with gamma compression.
/// </summary>
/// <remarks>
/// Uses LUT-based gamma compression for performance.
/// Stateless struct for zero-cost abstraction via generic dispatch.
/// </remarks>
public readonly struct LinearRgbFToRgb24 : IEncode<LinearRgbF, Bgr888> {

  /// <summary>
  /// Encodes linear working space to sRGB pixel.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Bgr888 Encode(in LinearRgbF color) => new(
    _GammaCompress(color.R),
    _GammaCompress(color.G),
    _GammaCompress(color.B)
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _GammaCompress(float linear) {
    if (linear <= 0f)
      return 0;
    if (linear >= 1f)
      return 255;
    // Convert to 16.16 fixed-point and use LUT
    var fixed16 = (int)(linear * 65536f);
    return FixedPointMath.GammaCompressionLut[fixed16 >> 8];
  }
}
