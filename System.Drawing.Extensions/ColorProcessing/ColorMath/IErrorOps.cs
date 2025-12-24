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
/// Provides error operations for error-diffusion dithering.
/// </summary>
/// <typeparam name="T">The color type for error calculations.</typeparam>
/// <remarks>
/// Used in dithering algorithms (Floyd-Steinberg, Atkinson, etc.)
/// to calculate and propagate quantization error.
/// </remarks>
public interface IErrorOps<T> where T : unmanaged {

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  /// <summary>
  /// Subtracts one color from another: a - b.
  /// </summary>
  /// <param name="a">The minuend.</param>
  /// <param name="b">The subtrahend.</param>
  /// <returns>The difference (error).</returns>
  static abstract T Sub(in T a, in T b);

  /// <summary>
  /// Adds a scaled error to a color: color + error * scale.
  /// </summary>
  /// <param name="color">The base color.</param>
  /// <param name="error">The error to add.</param>
  /// <param name="scale">The scale factor.</param>
  /// <returns>The color with error applied.</returns>
  static abstract T AddScaled(in T color, in T error, float scale);
#endif
}
