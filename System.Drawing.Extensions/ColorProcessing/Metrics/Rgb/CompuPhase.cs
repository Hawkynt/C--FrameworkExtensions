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
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

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
#if SUPPORTS_MATHF
    => MathF.Sqrt(CompuPhaseSquared._Calculate(a, b));
#else
    => (float)SysMath.Sqrt(CompuPhaseSquared._Calculate(a, b));
#endif
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
