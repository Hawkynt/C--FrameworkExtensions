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
/// <typeparam name="TAccum">The accumulator type (mutable, accumulates values).</typeparam>
/// <typeparam name="TColor">The color type being accumulated.</typeparam>
/// <remarks>
/// <para>
/// Used in convolution-based scaling algorithms (Lanczos, Bicubic, etc.)
/// where multiple source pixels are weighted and summed.
/// </para>
/// <para>
/// The accumulator is mutable - <see cref="AddMul"/> modifies the accumulator in place
/// for efficiency. Call <see cref="Result"/> to finalize and get the output color.
/// </para>
/// <para>
/// For float-based color types (e.g., LinearRgbaF), the type can be its own accumulator
/// where <typeparamref name="TAccum"/> equals <typeparamref name="TColor"/>.
/// For byte-based types (e.g., Bgra8888), a separate float-precision accumulator
/// prevents rounding errors during accumulation.
/// </para>
/// </remarks>
public interface IAccum<TAccum, TColor>
  where TAccum : unmanaged, IAccum<TAccum, TColor>
  where TColor : unmanaged, IColorSpace {

  /// <summary>
  /// Adds a weighted color to this accumulator: acc += color * weight.
  /// </summary>
  /// <param name="color">The color to add.</param>
  /// <param name="weight">The weight to apply.</param>
  /// <remarks>This method mutates the accumulator in place.</remarks>
  void AddMul(in TColor color, float weight);

  /// <summary>
  /// Finalizes the accumulation and returns the result color.
  /// </summary>
  /// <remarks>
  /// For byte-based accumulators, this is where clamping and rounding occurs.
  /// For float-based self-accumulators, this may simply return the accumulator value.
  /// </remarks>
  TColor Result { get; }
}
