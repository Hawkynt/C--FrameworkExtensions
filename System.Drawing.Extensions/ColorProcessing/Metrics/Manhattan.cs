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
/// Manhattan (L1) distance metric for 3-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Sum of absolute differences. Faster than Euclidean and suitable for
/// some perceptual comparisons.
/// </remarks>
public readonly struct Manhattan3F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => Math.Abs(a.C1 - b.C1) + Math.Abs(a.C2 - b.C2) + Math.Abs(a.C3 - b.C3);
}

#endregion

#region 3-Component Byte

/// <summary>
/// Manhattan (L1) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// Sum of absolute differences. Faster than Euclidean and suitable for
/// some perceptual comparisons.
/// </remarks>
public readonly struct Manhattan3B<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3B<TKey> {

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => Math.Abs(a.C1 - b.C1) + Math.Abs(a.C2 - b.C2) + Math.Abs(a.C3 - b.C3);
}

#endregion

#region 4-Component Float

/// <summary>
/// Manhattan (L1) distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// Sum of absolute differences across all four components including alpha.
/// Faster than Euclidean and suitable for threshold-based comparisons.
/// </remarks>
public readonly struct Manhattan4F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => Math.Abs(a.C1 - b.C1) + Math.Abs(a.C2 - b.C2) +
       Math.Abs(a.C3 - b.C3) + Math.Abs(a.A - b.A);
}

#endregion

#region 4-Component Byte

/// <summary>
/// Manhattan (L1) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// Sum of absolute differences across all four components including alpha.
/// Faster than Euclidean and suitable for threshold-based comparisons.
/// </remarks>
public readonly struct Manhattan4B<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => Math.Abs(a.C1 - b.C1) + Math.Abs(a.C2 - b.C2) +
       Math.Abs(a.C3 - b.C3) + Math.Abs(a.A - b.A);
}

#endregion
