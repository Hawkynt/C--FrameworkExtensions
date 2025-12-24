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

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Provides equality comparison between two colors in key space.
/// </summary>
/// <typeparam name="TKey">The key color type for comparison.</typeparam>
/// <remarks>
/// Used in quantization for palette matching where exact equality is needed.
/// Implementations may include tolerance-based equality.
/// </remarks>
public interface IColorEquality<TKey> where TKey : unmanaged {

  /// <summary>
  /// Determines whether two colors are equal.
  /// </summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns><c>true</c> if the colors are equal; otherwise, <c>false</c>.</returns>
  bool Equals(in TKey a, in TKey b);
}
