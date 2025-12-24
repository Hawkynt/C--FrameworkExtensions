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
/// Manhattan (L1) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// Sum of absolute differences. Faster than Euclidean and suitable for
/// some perceptual comparisons.
/// </remarks>
public readonly struct Manhattan3<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  /// <summary>
  /// Calculates the Manhattan distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b)
    => SysMath.Abs(a.C1 - b.C1) + SysMath.Abs(a.C2 - b.C2) + SysMath.Abs(a.C3 - b.C3);
}

/// <summary>
/// Non-generic Manhattan distance for LinearRgbF colors.
/// </summary>
public readonly struct ManhattanRgb : IColorMetric<Working.LinearRgbF> {

  /// <summary>
  /// Calculates the Manhattan distance between two RGB colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in Working.LinearRgbF a, in Working.LinearRgbF b)
    => SysMath.Abs(a.R - b.R) + SysMath.Abs(a.G - b.G) + SysMath.Abs(a.B - b.B);
}
