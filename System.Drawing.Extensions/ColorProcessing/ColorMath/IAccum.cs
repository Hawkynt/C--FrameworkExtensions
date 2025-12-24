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

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// Provides weighted accumulation for resampling operations.
/// </summary>
/// <typeparam name="T">The color type to accumulate.</typeparam>
/// <remarks>
/// Used in convolution-based scaling algorithms (Lanczos, Bicubic, etc.)
/// where multiple source pixels are weighted and summed.
/// </remarks>
public interface IAccum<T> where T : unmanaged {

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  /// <summary>Gets the zero/identity value for accumulation.</summary>
  static abstract T Zero { get; }

  /// <summary>
  /// Adds a weighted color to an accumulator: acc + x * weight.
  /// </summary>
  /// <param name="acc">The current accumulator value.</param>
  /// <param name="x">The color to add.</param>
  /// <param name="weight">The weight to apply.</param>
  /// <returns>The updated accumulator.</returns>
  static abstract T AddMul(in T acc, in T x, float weight);
#endif
}
