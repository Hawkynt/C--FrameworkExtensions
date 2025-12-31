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

/// <summary>
/// Chebyshev (L-infinity) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Maximum absolute difference. Measures the worst-case channel difference.
/// </remarks>
public readonly struct Chebyshev3F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1);
    var d2 = Math.Abs(a.C2 - b.C2);
    var d3 = Math.Abs(a.C3 - b.C3);
    return d1 > d2 ? (d1 > d3 ? d1 : d3) : (d2 > d3 ? d2 : d3);
  }
}

/// <summary>
/// Chebyshev (L-infinity) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Maximum absolute difference. Measures the worst-case channel difference.</para>
/// <para>Implements both <see cref="IColorMetric{TKey}"/> (float) and
/// <see cref="IColorMetricInt{TKey}"/> (int) for optimal performance
/// in integer-only pipelines.</para>
/// <para>Maximum distance: 255.</para>
/// </remarks>
public readonly struct Chebyshev3B<TKey> : IColorMetric<TKey>, IColorMetricInt<TKey>
  where TKey : unmanaged, IColorSpace3B<TKey> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Calculate(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1);
    var d2 = Math.Abs(a.C2 - b.C2);
    var d3 = Math.Abs(a.C3 - b.C3);
    return d1 > d2 ? (d1 > d3 ? d1 : d3) : (d2 > d3 ? d2 : d3);
  }

  /// <inheritdoc cref="IColorMetricInt{TKey}.Distance"/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IColorMetricInt<TKey>.Distance(in TKey a, in TKey b) => _Calculate(a, b);

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b);
}

/// <summary>
/// Chebyshev (L-infinity) distance metric for 4-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// Maximum absolute difference. Measures the worst-case channel difference.
/// </remarks>
public readonly struct Chebyshev4F<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1);
    var d2 = Math.Abs(a.C2 - b.C2);
    var d3 = Math.Abs(a.C3 - b.C3);
    var d4 = Math.Abs(a.A - b.A);
    return Math.Max(Math.Max(d1, d2), Math.Max(d3, d4));
  }
}

/// <summary>
/// Chebyshev (L-infinity) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Maximum absolute difference. Measures the worst-case channel difference.</para>
/// <para>Implements both <see cref="IColorMetric{TKey}"/> (float) and
/// <see cref="IColorMetricInt{TKey}"/> (int) for optimal performance
/// in integer-only pipelines.</para>
/// <para>Maximum distance: 255.</para>
/// </remarks>
public readonly struct Chebyshev4B<TKey> : IColorMetric<TKey>, IColorMetricInt<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _Calculate(in TKey a, in TKey b) {
    var d1 = Math.Abs(a.C1 - b.C1);
    var d2 = Math.Abs(a.C2 - b.C2);
    var d3 = Math.Abs(a.C3 - b.C3);
    var d4 = Math.Abs(a.A - b.A);
    return Math.Max(Math.Max(d1, d2), Math.Max(d3, d4));
  }

  /// <inheritdoc cref="IColorMetricInt{TKey}.Distance"/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IColorMetricInt<TKey>.Distance(in TKey a, in TKey b) => _Calculate(a, b);

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) => _Calculate(a, b);
}
