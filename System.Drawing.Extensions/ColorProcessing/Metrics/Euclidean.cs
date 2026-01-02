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
/// <remarks>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(3) ≈ 1.732.</para>
/// </remarks>
public readonly struct Euclidean3F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3F<TKey> {

  private const float InverseMaxDistance = 1f / 1.7320508075688772935f; // 1/sqrt(3)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared3F<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// <para>Faster than Euclidean (no sqrt) when only relative comparison is needed.
/// Use for nearest-neighbor search where absolute distance isn't required.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 3.0.</para>
/// </remarks>
public readonly struct EuclideanSquared3F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3F<TKey> {

  private const float InverseMaxDistance = 1f / 3f; // 1/(3 × 1.0²)

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
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) * InverseMaxDistance);
}

#endregion

#region 3-Component Byte

/// <summary>
/// Euclidean (L2) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(195075) ≈ 441.67.</para>
/// </remarks>
public readonly struct Euclidean3B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3B<TKey> {

  private const float InverseMaxDistance = 1f / 441.6729559300637f; // 1/sqrt(3 × 255²)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared3B<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Faster than Euclidean (no sqrt) when only relative comparison is needed.
/// Use for nearest-neighbor search where absolute distance isn't required.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 3 × 255² = 195,075.</para>
/// </remarks>
public readonly struct EuclideanSquared3B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3B<TKey> {

  private const int MaxDistance = 195075; // 3 × 255²

  /// <summary>
  /// Internal squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return d1 * d1 + d2 * d2 + d3 * d3;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromRaw((uint)((ulong)_Calculate(a, b) * uint.MaxValue / MaxDistance));
}

#endregion

#region 4-Component Float

/// <summary>
/// Euclidean (L2) distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Calculates the geometric distance across all four components including alpha.
/// More accurate than Manhattan for perceptual comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(4) = 2.0.</para>
/// </remarks>
public readonly struct Euclidean4F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4F<TKey> {

  private const float InverseMaxDistance = 0.5f; // 1/sqrt(4)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared4F<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Omits the square root for faster comparisons when only relative distances matter.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 4.0.</para>
/// </remarks>
public readonly struct EuclideanSquared4F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4F<TKey> {

  private const float InverseMaxDistance = 0.25f; // 1/(4 × 1.0²)

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return d1 * d1 + d2 * d2 + d3 * d3 + da * da;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) * InverseMaxDistance);
}

#endregion

#region 4-Component Byte

/// <summary>
/// Euclidean (L2) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Calculates the geometric distance across all four components including alpha.
/// More accurate than Manhattan for perceptual comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(260100) ≈ 510.</para>
/// </remarks>
public readonly struct Euclidean4B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4B<TKey> {

  private const float InverseMaxDistance = 1f / 510f; // 1/sqrt(4 × 255²)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared4B<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Omits the square root for faster comparisons when only relative distances matter.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 4 × 255² = 260,100.</para>
/// </remarks>
public readonly struct EuclideanSquared4B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4B<TKey> {

  private const int MaxDistance = 260100; // 4 × 255²

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return d1 * d1 + d2 * d2 + d3 * d3 + da * da;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromRaw((uint)((ulong)_Calculate(a, b) * uint.MaxValue / MaxDistance));
}

#endregion
