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
using Hawkynt.ColorProcessing.Internal;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

#region 3-Component Float

/// <summary>
/// Manhattan (L1) distance metric for 3-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// <para>Sum of absolute differences. Faster than Euclidean and suitable for
/// some perceptual comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 3.0.</para>
/// </remarks>
public readonly struct Manhattan3F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3F<TKey> {

  private const float InverseMaxDistance = 1f / 3f; // 1/(3 components × 1.0 max diff)

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = Math.Abs(a.C1 - b.C1) + Math.Abs(a.C2 - b.C2) + Math.Abs(a.C3 - b.C3);
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

#endregion

#region 3-Component Byte

/// <summary>
/// Manhattan (L1) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Sum of absolute differences. Faster than Euclidean and suitable for
/// some perceptual comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 3 × 255 = 765.</para>
/// </remarks>
public readonly struct Manhattan3B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3B<TKey> {

  private const int MaxDistance = 765; // 3 × 255

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b)
    // Using branchless operations for better performance
    => FixedPointMath.BranchlessAbsDiff(a.C1, b.C1) +
       FixedPointMath.BranchlessAbsDiff(a.C2, b.C2) +
       FixedPointMath.BranchlessAbsDiff(a.C3, b.C3);

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromRaw((uint)((ulong)_Calculate(a, b) * uint.MaxValue / MaxDistance));
}

#endregion

#region 4-Component Float

/// <summary>
/// Manhattan (L1) distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Sum of absolute differences across all four components including alpha.
/// Faster than Euclidean and suitable for threshold-based comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 4.0.</para>
/// </remarks>
public readonly struct Manhattan4F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4F<TKey> {

  private const float InverseMaxDistance = 0.25f; // 1/(4 components × 1.0 max diff)

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = Math.Abs(a.C1 - b.C1) + Math.Abs(a.C2 - b.C2) +
              Math.Abs(a.C3 - b.C3) + Math.Abs(a.A - b.A);
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

#endregion

#region 4-Component Byte

/// <summary>
/// Manhattan (L1) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Sum of absolute differences across all four components including alpha.
/// Faster than Euclidean and suitable for threshold-based comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 4 × 255 = 1,020.</para>
/// </remarks>
public readonly struct Manhattan4B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4B<TKey> {

  private const int MaxDistance = 1020; // 4 × 255

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b)
    // Using branchless operations for better performance
    => FixedPointMath.BranchlessAbsDiff(a.C1, b.C1) +
       FixedPointMath.BranchlessAbsDiff(a.C2, b.C2) +
       FixedPointMath.BranchlessAbsDiff(a.C3, b.C3) +
       FixedPointMath.BranchlessAbsDiff(a.A, b.A);

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromRaw((uint)((ulong)_Calculate(a, b) * uint.MaxValue / MaxDistance));
}

#endregion
