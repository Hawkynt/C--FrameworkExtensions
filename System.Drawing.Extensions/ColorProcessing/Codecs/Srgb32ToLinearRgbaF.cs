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
/// Decodes sRGB Rgba32 to linear LinearRgbaF with gamma expansion.
/// </summary>
/// <remarks>
/// Uses LUT-based gamma expansion for performance.
/// Stateless struct for zero-cost abstraction via generic dispatch.
/// </remarks>
public readonly struct Srgb32ToLinearRgbaF : IDecode<Bgra8888, LinearRgbaF> {

  private const float FixedToFloat = 1f / 65536f;

  /// <summary>
  /// Decodes sRGB pixel to linear working space.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbaF Decode(in Bgra8888 pixel) => new(
    FixedPointMath.GammaExpansionLut[pixel.R] * FixedToFloat,
    FixedPointMath.GammaExpansionLut[pixel.G] * FixedToFloat,
    FixedPointMath.GammaExpansionLut[pixel.B] * FixedToFloat,
    pixel.A * Bgra8888.ByteToNormalized
  );
}
