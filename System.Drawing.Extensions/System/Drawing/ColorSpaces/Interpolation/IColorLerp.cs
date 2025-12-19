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

namespace System.Drawing.ColorSpaces.Interpolation;

/// <summary>
/// Interface for zero-cost color interpolation implementations.
/// Implementations should be readonly structs for JIT inlining.
/// </summary>
/// <remarks>
/// <para>
/// All implementations perform linear interpolation (lerp) between two colors.
/// The interpolation factor t ranges from 0-255 where:
/// - t=0 returns color1
/// - t=255 returns color2
/// - t=128 returns the midpoint (50% blend)
/// </para>
/// <para>
/// Example usage:
/// <code>
/// // Direct static call (fastest)
/// var blend = HsvLerp.Blend(red, blue, 128);
///
/// // Generic polymorphic usage
/// void ProcessImage&lt;TLerp&gt;(TLerp lerp) where TLerp : struct, IColorLerp
/// {
///     var blended = lerp.Lerp(source, target, t);
/// }
/// </code>
/// </para>
/// </remarks>
public interface IColorLerp {
  /// <summary>
  /// Linearly interpolates between two colors.
  /// </summary>
  /// <param name="color1">The starting color (returned when t=0).</param>
  /// <param name="color2">The ending color (returned when t=255).</param>
  /// <param name="t">Interpolation factor (0-255).</param>
  /// <returns>The interpolated color.</returns>
  Color Lerp(Color color1, Color color2, byte t);
}
