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
/// Euclidean (L2) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
public readonly struct Euclidean3F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => MathF.Sqrt(EuclideanSquared3F<TKey>._Calculate(a, b));
}

/// <summary>
/// Squared Euclidean distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Faster than Euclidean (no sqrt) when only relative comparison is needed.
/// Use for nearest-neighbor search where absolute distance isn't required.
/// </remarks>
public readonly struct EuclideanSquared3F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Internal squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return d1 * d1 + d2 * d2 + d3 * d3;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b);
}


/// <summary>
/// Euclidean (L2) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
public readonly struct Euclidean3B<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3B<TKey> {

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => MathF.Sqrt(EuclideanSquared3B<TKey>._Calculate(a, b));
}

/// <summary>
/// Squared Euclidean distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Faster than Euclidean (no sqrt) when only relative comparison is needed.
/// Use for nearest-neighbor search where absolute distance isn't required.
/// </remarks>
public readonly struct EuclideanSquared3B<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3B<TKey> {

  /// <summary>
  /// Internal squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return d1 * d1 + d2 * d2 + d3 * d3;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b);
}
#endregion

#region 4-Component Byte

/// <summary>
/// Euclidean (L2) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Calculates the geometric distance across all four components including alpha.
/// More accurate than Manhattan for perceptual comparisons.
/// </remarks>
public readonly struct Euclidean4F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => MathF.Sqrt(EuclideanSquared4F<TKey>._Calculate(a, b));
}

/// <summary>
/// Squared Euclidean distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Omits the square root for faster comparisons when only relative distances matter.
/// </remarks>
public readonly struct EuclideanSquared4F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return d1 * d1 + d2 * d2 + d3 * d3 + da * da;
  }
}

/// <summary>
/// Euclidean (L2) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Calculates the geometric distance across all four components including alpha.
/// More accurate than Manhattan for perceptual comparisons.
/// </remarks>
public readonly struct Euclidean4B<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => MathF.Sqrt(EuclideanSquared4B<TKey>._Calculate(a,b));
}

/// <summary>
/// Squared Euclidean distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Omits the square root for faster comparisons when only relative distances matter.
/// </remarks>
public readonly struct EuclideanSquared4B<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return d1 * d1 + d2 * d2 + d3 * d3 + da * da;
  }
}

#endregion
