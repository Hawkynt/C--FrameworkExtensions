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

namespace Hawkynt.ColorProcessing.Metrics.Lab;

/// <summary>
/// Calculates the DIN99 color-difference (Euclidean distance in DIN99 space).
/// </summary>
/// <remarks>
/// <para>DIN99 is a perceptually-uniform Lab remapping designed so that plain Euclidean
/// distance approximates perceived colour difference. Standardised in DIN 6176:2001.
/// Faster to compute than CIEDE2000 with comparable perceptual fit for moderate ΔE.</para>
/// <para>Reference: G. Cui, M. R. Luo, B. Rigg, G. Roesler &amp; K. Witt, "Uniform colour
/// spaces based on the DIN99 colour-difference formula", Color Research &amp;
/// Application 27(4):282-290, 2002.</para>
/// <para>Returns UNorm32 normalised against ΔE = 100.</para>
/// </remarks>
public readonly struct DIN99Distance : IColorMetric<Din99F>, INormalizedMetric {

  // Practical max distance for normalization
  private const float MaxDistance = 100f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in Din99F a, in Din99F b) {
    var raw = MathF.Sqrt(DIN99DistanceSquared._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw / MaxDistance);
  }
}

/// <summary>
/// Calculates the squared Euclidean distance in DIN99 color space.
/// </summary>
/// <remarks>
/// <para>Faster than DIN99Distance when only comparing distances (no sqrt).</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance² of 10000.</para>
/// </remarks>
public readonly struct DIN99DistanceSquared : IColorMetric<Din99F>, INormalizedMetric {

  // Practical max distance squared for normalization (100²)
  private const float MaxDistanceSquared = 10000f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in Din99F a, in Din99F b) {
    var dL = a.L - b.L;
    var da = a.A - b.A;
    var db = a.B - b.B;
    return dL * dL + da * da + db * db;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in Din99F a, in Din99F b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) / MaxDistanceSquared);
}
