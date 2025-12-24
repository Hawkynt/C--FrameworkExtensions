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
/// Euclidean (L2) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
public readonly struct Euclidean3<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
#if SUPPORTS_MATHF
    => MathF.Sqrt(EuclideanSquared3<TKey>._Calculate(a, b));
#else
    => (float)Math.Sqrt(EuclideanSquared3<TKey>._Calculate(a, b));
#endif
}

/// <summary>
/// Non-generic Euclidean distance for LinearRgbF colors.
/// </summary>
public readonly struct EuclideanRgb : IColorMetric<Working.LinearRgbF> {

  /// <summary>
  /// Calculates the Euclidean distance between two RGB colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in Working.LinearRgbF a, in Working.LinearRgbF b)
#if SUPPORTS_MATHF
    => MathF.Sqrt(EuclideanSquaredRgb._Calculate(a, b));
#else
    => (float)Math.Sqrt(EuclideanSquaredRgb._Calculate(a, b));
#endif
}
