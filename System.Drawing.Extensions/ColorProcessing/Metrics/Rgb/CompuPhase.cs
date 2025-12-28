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
using MethodImplOptions = Utilities.MethodImplOptions;

// For IColorSpace4B
using Hawkynt.ColorProcessing;
using static Hawkynt.ColorProcessing.Constants.ColorConstants;

namespace Hawkynt.ColorProcessing.Metrics.Rgb;

/// <summary>
/// Calculates color distance using the CompuPhase low-cost approximation algorithm.
/// </summary>
/// <remarks>
/// This algorithm provides a fast approximation of perceptual color difference
/// by weighting RGB components based on the mean red value.
/// The algorithm adjusts red and blue weights based on average red intensity:
/// - Higher red values increase red weight and decrease blue weight
/// - Lower red values decrease red weight and increase blue weight
/// - Green is weighted consistently at 4x
/// Reference: https://www.compuphase.com/cmetric.htm
/// </remarks>
public readonly struct CompuPhase : IColorMetric<LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LinearRgbF a, in LinearRgbF b)
    => MathF.Sqrt(CompuPhaseSquared._Calculate(a, b));
}

/// <summary>
/// Calculates squared color distance using the CompuPhase algorithm.
/// </summary>
/// <remarks>
/// Faster than CompuPhase when only comparing distances (no sqrt).
/// </remarks>
public readonly struct CompuPhaseSquared : IColorMetric<LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in LinearRgbF a, in LinearRgbF b) {
    var rMean = (a.R + b.R) * 0.5f;
    var dr = a.R - b.R;
    var dg = a.G - b.G;
    var db = a.B - b.B;

    var rWeight = 2f + rMean;
    var bWeight = 3f - rMean;
    const float gWeight = 4f;

    return rWeight * dr * dr + gWeight * dg * dg + bWeight * db * db;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LinearRgbF a, in LinearRgbF b) => _Calculate(a, b);
}

/// <summary>
/// CompuPhase perceptual color distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Uses red-weighted distance formula for better perceptual matching.
/// C1=R, C2=G, C3=B ordering is expected per IColorSpace4B convention.
/// Reference: https://www.compuphase.com/cmetric.htm
/// </remarks>
public readonly struct CompuPhase4<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Calculates the CompuPhase perceptual distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => MathF.Sqrt(CompuPhaseSquared4<TKey>._Calculate(a, b));
}

/// <summary>
/// Squared CompuPhase perceptual color distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Omits the square root for faster comparisons when only relative distances matter.
/// </remarks>
public readonly struct CompuPhaseSquared4<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Calculates the squared CompuPhase distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b);

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
