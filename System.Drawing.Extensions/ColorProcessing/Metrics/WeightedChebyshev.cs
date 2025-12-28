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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

#region 3-Component Float

/// <summary>
/// Weighted Chebyshev (L-infinity) distance metric for 3-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Applies custom weights to each component before calculating Chebyshev distance.
/// Maximum of weighted absolute differences.
/// </remarks>
public readonly struct WeightedChebyshev3F<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the weighted Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = MathF.Abs(a.C1 - b.C1) * w1;
    var d2 = MathF.Abs(a.C2 - b.C2) * w2;
    var d3 = MathF.Abs(a.C3 - b.C3) * w3;
    return d1 > d2 ? (d1 > d3 ? d1 : d3) : (d2 > d3 ? d2 : d3);
  }
}

#endregion

#region 3-Component Byte

/// <summary>
/// Weighted Chebyshev (L-infinity) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// Applies custom weights to each component before calculating Chebyshev distance.
/// Maximum of weighted absolute differences.
/// </remarks>
public readonly struct WeightedChebyshev3B<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3B<TKey> {

  /// <summary>
  /// Calculates the weighted Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1) * w1;
    var d2 = Math.Abs(a.C2 - b.C2) * w2;
    var d3 = Math.Abs(a.C3 - b.C3) * w3;
    return d1 > d2 ? (d1 > d3 ? d1 : d3) : (d2 > d3 ? d2 : d3);
  }
}

#endregion

#region 4-Component Float

/// <summary>
/// Weighted Chebyshev (L-infinity) distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// Applies custom weights to each component including alpha before calculating Chebyshev distance.
/// Maximum of weighted absolute differences.
/// </remarks>
public readonly struct WeightedChebyshev4F<TKey>(float w1, float w2, float w3, float wA) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Calculates the weighted Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = MathF.Abs(a.C1 - b.C1) * w1;
    var d2 = MathF.Abs(a.C2 - b.C2) * w2;
    var d3 = MathF.Abs(a.C3 - b.C3) * w3;
    var da = MathF.Abs(a.A - b.A) * wA;
    return MathF.Max(MathF.Max(d1, d2), MathF.Max(d3, da));
  }
}

#endregion

#region 4-Component Byte

/// <summary>
/// Weighted Chebyshev (L-infinity) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Applies custom weights to each component including alpha before calculating Chebyshev distance.
/// Maximum of weighted absolute differences.
/// </remarks>
public readonly struct WeightedChebyshev4B<TKey>(float w1, float w2, float w3, float wA) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Calculates the weighted Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1) * w1;
    var d2 = Math.Abs(a.C2 - b.C2) * w2;
    var d3 = Math.Abs(a.C3 - b.C3) * w3;
    var da = Math.Abs(a.A - b.A) * wA;
    return Math.Max(Math.Max(d1, d2), Math.Max(d3, da));
  }
}

#endregion
