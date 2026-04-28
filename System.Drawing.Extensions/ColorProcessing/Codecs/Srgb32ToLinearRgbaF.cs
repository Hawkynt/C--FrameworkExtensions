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
using System.Runtime.CompilerServices;
using Guard;
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
public readonly struct Srgb32ToLinearRgbaF : IDecode<Bgra8888, LinearRgbaF>, IBatchDecode<Bgra8888, LinearRgbaF> {

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

  /// <inheritdoc />
  /// <remarks>
  /// 4-way unrolled span-to-span decode. Bit-exact with <see cref="Decode"/> by construction
  /// (same LUT, same scaling constants). Produces identical output to a per-pixel call loop.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DecodeBatch(ReadOnlySpan<Bgra8888> source, Span<LinearRgbaF> destination) {
    var n = source.Length;
    Against.CountBelow(destination.Length, n);

    var lut = FixedPointMath.GammaExpansionLut;
    const float inv = FixedToFloat;
    const float invByte = Bgra8888.ByteToNormalized;

    var i = 0;
    var unrolledEnd = n - (n & 3);
    for (; i < unrolledEnd; i += 4) {
      ref readonly var p0 = ref source[i];
      ref readonly var p1 = ref source[i + 1];
      ref readonly var p2 = ref source[i + 2];
      ref readonly var p3 = ref source[i + 3];
      destination[i]     = new LinearRgbaF(lut[p0.R] * inv, lut[p0.G] * inv, lut[p0.B] * inv, p0.A * invByte);
      destination[i + 1] = new LinearRgbaF(lut[p1.R] * inv, lut[p1.G] * inv, lut[p1.B] * inv, p1.A * invByte);
      destination[i + 2] = new LinearRgbaF(lut[p2.R] * inv, lut[p2.G] * inv, lut[p2.B] * inv, p2.A * invByte);
      destination[i + 3] = new LinearRgbaF(lut[p3.R] * inv, lut[p3.G] * inv, lut[p3.B] * inv, p3.A * invByte);
    }

    for (; i < n; ++i) {
      ref readonly var p = ref source[i];
      destination[i] = new LinearRgbaF(lut[p.R] * inv, lut[p.G] * inv, lut[p.B] * inv, p.A * invByte);
    }
  }
}
