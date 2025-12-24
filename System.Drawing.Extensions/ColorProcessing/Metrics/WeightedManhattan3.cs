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
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Weighted Manhattan (L1) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Applies custom weights to each component before calculating Manhattan distance.
/// Sum of weighted absolute differences.
/// </remarks>
public readonly struct WeightedManhattan3<TKey>(float w1, float w2, float w3) : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the weighted Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
#if SUPPORTS_MATHF
    => MathF.Abs(a.C1 - b.C1) * w1 + MathF.Abs(a.C2 - b.C2) * w2 + MathF.Abs(a.C3 - b.C3) * w3;
#else
    => (float)SysMath.Abs(a.C1 - b.C1) * w1 + (float)SysMath.Abs(a.C2 - b.C2) * w2 + (float)SysMath.Abs(a.C3 - b.C3) * w3;
#endif
}

/// <summary>
/// Non-generic weighted Manhattan distance for LinearRgbF colors.
/// </summary>
public readonly struct WeightedManhattanRgb(float wR, float wG, float wB) : IColorMetric<Working.LinearRgbF> {

  /// <summary>
  /// Calculates the weighted Manhattan distance between two RGB colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in Working.LinearRgbF a, in Working.LinearRgbF b)
#if SUPPORTS_MATHF
    => MathF.Abs(a.R - b.R) * wR + MathF.Abs(a.G - b.G) * wG + MathF.Abs(a.B - b.B) * wB;
#else
    => (float)SysMath.Abs(a.R - b.R) * wR + (float)SysMath.Abs(a.G - b.G) * wG + (float)SysMath.Abs(a.B - b.B) * wB;
#endif
}
