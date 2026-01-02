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

/// <summary>
/// Chebyshev (L-infinity) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// <para>Maximum absolute difference. Measures the worst-case channel difference.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 1.0.</para>
/// </remarks>
public readonly struct Chebyshev3F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1);
    var d2 = Math.Abs(a.C2 - b.C2);
    var d3 = Math.Abs(a.C3 - b.C3);
    var max = d1 > d2 ? (d1 > d3 ? d1 : d3) : (d2 > d3 ? d2 : d3);
    return UNorm32.FromFloatClamped(max);
  }
}

/// <summary>
/// Chebyshev (L-infinity) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Maximum absolute difference. Measures the worst-case channel difference.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 255.</para>
/// </remarks>
public readonly struct Chebyshev3B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3B<TKey> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b) {
    // Using branchless operations for better performance
    var d1 = FixedPointMath.BranchlessAbsDiff(a.C1, b.C1);
    var d2 = FixedPointMath.BranchlessAbsDiff(a.C2, b.C2);
    var d3 = FixedPointMath.BranchlessAbsDiff(a.C3, b.C3);
    return FixedPointMath.BranchlessMax(FixedPointMath.BranchlessMax(d1, d2), d3);
  }

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromByte((byte)_Calculate(a, b));
}

/// <summary>
/// Chebyshev (L-infinity) distance metric for 4-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Maximum absolute difference. Measures the worst-case channel difference.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 1.0.</para>
/// </remarks>
public readonly struct Chebyshev4F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1);
    var d2 = Math.Abs(a.C2 - b.C2);
    var d3 = Math.Abs(a.C3 - b.C3);
    var d4 = Math.Abs(a.A - b.A);
    var max = Math.Max(Math.Max(d1, d2), Math.Max(d3, d4));
    return UNorm32.FromFloatClamped(max);
  }
}

/// <summary>
/// Chebyshev (L-infinity) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Maximum absolute difference. Measures the worst-case channel difference.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 255.</para>
/// </remarks>
public readonly struct Chebyshev4B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4B<TKey> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b) {
    // Using branchless operations for better performance
    var d1 = FixedPointMath.BranchlessAbsDiff(a.C1, b.C1);
    var d2 = FixedPointMath.BranchlessAbsDiff(a.C2, b.C2);
    var d3 = FixedPointMath.BranchlessAbsDiff(a.C3, b.C3);
    var d4 = FixedPointMath.BranchlessAbsDiff(a.A, b.A);
    return FixedPointMath.BranchlessMax(
      FixedPointMath.BranchlessMax(d1, d2),
      FixedPointMath.BranchlessMax(d3, d4)
    );
  }

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromByte((byte)_Calculate(a, b));
}
