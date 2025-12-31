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
/// Provides integer-only weighted accumulation for fast resampling operations.
/// </summary>
/// <typeparam name="TAccum">The accumulator type (mutable, accumulates values).</typeparam>
/// <typeparam name="TColor">The color type being accumulated.</typeparam>
/// <remarks>
/// <para>
/// Used in fast/LQ scaling algorithms where float arithmetic is undesirable.
/// All operations use pure integer math with no float conversions.
/// </para>
/// <para>
/// Integer weights should be scaled appropriately (e.g., 0-256 or 0-65536)
/// to maintain precision during accumulation.
/// </para>
/// </remarks>
public interface IAccumInt<TAccum, TColor>
  where TAccum : unmanaged, IAccumInt<TAccum, TColor>
  where TColor : unmanaged, IColorSpace {

  /// <summary>
  /// Adds a weighted color to this accumulator using integer weights: acc += color * weight.
  /// </summary>
  /// <param name="color">The color to add.</param>
  /// <param name="weight">The integer weight to apply.</param>
  /// <remarks>This method mutates the accumulator in place.</remarks>
  void AddMul(in TColor color, int weight);

  /// <summary>
  /// Adds a color with weight 1 to this accumulator: acc += color.
  /// </summary>
  /// <param name="color">The color to add.</param>
  /// <remarks>Equivalent to <c>AddMul(color, 1)</c> but avoids the weight parameter on the stack.</remarks>
  void Add(in TColor color);

  /// <summary>
  /// Finalizes the accumulation and returns the result color.
  /// </summary>
  /// <remarks>
  /// Performs integer division by accumulated weight sum and clamps to valid range.
  /// </remarks>
  TColor Result { get; }
}
