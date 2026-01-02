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
using Hawkynt.ColorProcessing.Working;
using UNorm32 = Hawkynt.ColorProcessing.Metrics.UNorm32;
using MethodImplOptions = Utilities.MethodImplOptions;

// For IColorSpace4B
using Hawkynt.ColorProcessing;
using static Hawkynt.ColorProcessing.Constants.ColorConstants;

namespace Hawkynt.ColorProcessing.Metrics.Rgb;

/// <summary>
/// Calculates color distance using the CompuPhase low-cost approximation algorithm.
/// </summary>
/// <remarks>
/// <para>This algorithm provides a fast approximation of perceptual color difference
/// by weighting RGB components based on the mean red value.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Reference: https://www.compuphase.com/cmetric.htm</para>
/// </remarks>
public readonly struct CompuPhase : IColorMetric<LinearRgbF>, INormalizedMetric {

  // Max squared distance is 9.0 (when rMean=1, weights are 3,4,2 and all diffs are 1)
  // sqrt(9) = 3, but normalized by /9 in squared version, so max = sqrt(1) = 1
  private const float MaxDistance = 1f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LinearRgbF a, in LinearRgbF b) {
    var raw = MathF.Sqrt(CompuPhaseSquared._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw / MaxDistance);
  }
}

/// <summary>
/// Calculates squared color distance using the CompuPhase algorithm.
/// </summary>
/// <remarks>
/// <para>Faster than CompuPhase when only comparing distances (no sqrt).</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// </remarks>
public readonly struct CompuPhaseSquared : IColorMetric<LinearRgbF>, INormalizedMetric {

  // Max raw is 9.0 (3*1 + 4*1 + 2*1 with max weights and max diffs)
  // Normalized by /9 = 1.0
  private const float MaxDistance = 1f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in LinearRgbF a, in LinearRgbF b) {
    var rMean = (a.R + b.R) * 0.5f;
    var dr = a.R - b.R;
    var dg = a.G - b.G;
    var db = a.B - b.B;

    var rWeight = 2f + rMean;
    var bWeight = 3f - rMean;
    const float gWeight = 4f;

    // Normalize by max possible weight sum (9) to get 0-1 range
    return (rWeight * dr * dr + gWeight * dg * dg + bWeight * db * db) / 9f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LinearRgbF a, in LinearRgbF b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) / MaxDistance);
}

/// <summary>
/// CompuPhase perceptual color distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Uses red-weighted distance formula for better perceptual matching.
/// C1=R, C2=G, C3=B ordering is expected per IColorSpace4B convention.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Reference: https://www.compuphase.com/cmetric.htm</para>
/// </remarks>
public readonly struct CompuPhase4<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4B<TKey> {

  private const float MaxDistance = 1f;

  /// <summary>
  /// Calculates the CompuPhase perceptual distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(CompuPhaseSquared4<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw / MaxDistance);
  }
}

/// <summary>
/// Squared CompuPhase perceptual color distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Omits the square root for faster comparisons when only relative distances matter.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>The integer version uses weights scaled by 510 to avoid fractional arithmetic.</para>
/// </remarks>
public readonly struct CompuPhaseSquared4<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4B<TKey> {

  // Already normalized to 0-1 range by _Calculate (divides by 9)
  private const float MaxDistance = 1f;

  /// <summary>
  /// Internal integer calculation using scaled weights.
  /// </summary>
  /// <remarks>
  /// <para>Original formula (normalized):</para>
  /// <code>
  /// rMean = (rA + rB) / 510
  /// result = (2 + rMean) * dR² + 4 * dG² + (3 - rMean) * dB²
  /// </code>
  /// <para>Scaled by 510 to eliminate division:</para>
  /// <code>
  /// weightR = 1020 + rA + rB  (range [1020, 1530])
  /// weightG = 2040            (constant)
  /// weightB = 1530 - rA - rB  (range [1020, 1530])
  /// </code>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _CalculateInt(in TKey a, in TKey b) {
    // Weights scaled by 510 to stay in integer domain
    var rSum = a.C1 + b.C1;  // [0, 510]
    var weightR = 1020 + rSum;  // 2*510 + rSum
    const int weightG = 2040;   // 4*510
    var weightB = 1530 - rSum;  // 3*510 - rSum

    var dR = a.C1 - b.C1;
    var dG = a.C2 - b.C2;
    var dB = a.C3 - b.C3;

    return weightR * dR * dR + weightG * dG * dG + weightB * dB * dB;
  }

  /// <summary>
  /// Calculates the squared CompuPhase distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) / MaxDistance);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    // C1=R, C2=G, C3=B per IColorSpace4B convention
    var rMean = 0.5f * (a.C1 + b.C1) * ByteToFloat;
    var dR = (a.C1 - b.C1) * ByteToFloat;
    var dG = (a.C2 - b.C2) * ByteToFloat;
    var dB = (a.C3 - b.C3) * ByteToFloat;

    var weightR = 2f + rMean;
    var weightG = 4f;
    var weightB = 3f - rMean;

    return (weightR * dR * dR + weightG * dG * dG + weightB * dB * dB) / 9f;
  }
}
