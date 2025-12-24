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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
using SysMath = System.Math;

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Chebyshev (L-infinity) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Maximum absolute difference. Measures the worst-case channel difference.
/// </remarks>
public readonly struct Chebyshev3<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the Chebyshev distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = SysMath.Abs(a.C1 - b.C1);
    var d2 = SysMath.Abs(a.C2 - b.C2);
    var d3 = SysMath.Abs(a.C3 - b.C3);
    return d1 > d2 ? (d1 > d3 ? d1 : d3) : (d2 > d3 ? d2 : d3);
  }
}

/// <summary>
/// Non-generic Chebyshev distance for LinearRgbF colors.
/// </summary>
public readonly struct ChebyshevRgb : IColorMetric<Working.LinearRgbF> {

  /// <summary>
  /// Calculates the Chebyshev distance between two RGB colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in Working.LinearRgbF a, in Working.LinearRgbF b) {
    var dr = SysMath.Abs(a.R - b.R);
    var dg = SysMath.Abs(a.G - b.G);
    var db = SysMath.Abs(a.B - b.B);
    return dr > dg ? (dr > db ? dr : db) : (dg > db ? dg : db);
  }
}
