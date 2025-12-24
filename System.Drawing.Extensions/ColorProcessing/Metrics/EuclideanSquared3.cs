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

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Squared Euclidean distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Faster than Euclidean (no sqrt) when only relative comparison is needed.
/// Use for nearest-neighbor search where absolute distance isn't required.
/// </remarks>
public readonly struct EuclideanSquared3<TKey> : IColorMetric<TKey>
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
/// Non-generic squared Euclidean distance for LinearRgbF colors.
/// </summary>
public readonly struct EuclideanSquaredRgb : IColorMetric<Working.LinearRgbF> {

  /// <summary>
  /// Internal squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in Working.LinearRgbF a, in Working.LinearRgbF b) {
    var dr = a.R - b.R;
    var dg = a.G - b.G;
    var db = a.B - b.B;
    return dr * dr + dg * dg + db * db;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two RGB colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in Working.LinearRgbF a, in Working.LinearRgbF b) => _Calculate(a, b);
}
