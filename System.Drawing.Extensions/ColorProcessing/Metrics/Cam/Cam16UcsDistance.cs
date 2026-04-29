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
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using UNorm32 = Hawkynt.ColorProcessing.Metrics.UNorm32;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics.Cam;

/// <summary>
/// Perceptual distance in <see cref="Cam16UcsF"/> — Euclidean ΔE in the CAM16-UCS space.
/// </summary>
/// <remarks>
/// <para>The CAM16-UCS axes (J', a', b') are designed so that Euclidean distance approximates
/// perceptual difference under the chosen viewing conditions. Returns the ΔE value
/// normalised against a practical maximum of 100 (≈ end-to-end black-to-white in J').</para>
/// <para>Pairs naturally with the <see cref="LinearRgbFToCam16UcsF"/> projector for use as
/// a <c>TKey</c> distance in the existing quantizer / nearest-palette infrastructure.</para>
/// <para>Reference: Li, Luo et al. 2017, "Comprehensive color solutions: CAM16, CAT16, and
/// CAM16-UCS", Color Research &amp; Application 42(6).</para>
/// </remarks>
public readonly struct Cam16UcsDistance : IColorMetric<Cam16UcsF>, INormalizedMetric {

  private const float MaxDeltaE = 100f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in Cam16UcsF a, in Cam16UcsF b) {
    var dJ = a.J - b.J;
    var dA = a.A - b.A;
    var dB = a.B - b.B;
    var raw = MathF.Sqrt(dJ * dJ + dA * dA + dB * dB);
    return UNorm32.FromFloatClamped(raw / MaxDeltaE);
  }
}

/// <summary>
/// Squared variant — faster for nearest-neighbour comparisons that only need ordering.
/// </summary>
public readonly struct Cam16UcsDistanceSquared : IColorMetric<Cam16UcsF>, INormalizedMetric {

  private const float MaxDeltaESquared = 10000f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in Cam16UcsF a, in Cam16UcsF b) {
    var dJ = a.J - b.J;
    var dA = a.A - b.A;
    var dB = a.B - b.B;
    return UNorm32.FromFloatClamped((dJ * dJ + dA * dA + dB * dB) / MaxDeltaESquared);
  }
}
