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

namespace Hawkynt.ColorProcessing.Blending;

/// <summary>
/// Defines a per-channel blend mode that operates on normalized [0,1] float values.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface as a <c>readonly struct</c> for zero-allocation blending.
/// The <see cref="Blend"/> method receives background and foreground channel values
/// and returns the blended result.
/// </para>
/// </remarks>
public interface IBlendMode {

  /// <summary>
  /// Blends a single normalized channel value.
  /// </summary>
  /// <param name="bg">The background channel value in [0,1].</param>
  /// <param name="fg">The foreground channel value in [0,1].</param>
  /// <returns>The blended channel value in [0,1].</returns>
  float Blend(float bg, float fg);
}

/// <summary>
/// Defines a full-pixel blend mode that operates on all three RGB channels simultaneously.
/// </summary>
/// <remarks>
/// <para>
/// Used for blend modes that require cross-channel information, such as HSL-based
/// component modes (Hue, Saturation, Color, Luminosity).
/// </para>
/// <para>
/// Extends <see cref="IBlendMode"/> so that a single generic constraint
/// <c>where TMode : struct, IBlendMode</c> covers both per-channel and full-pixel modes.
/// The blending engine checks <c>if (default(TMode) is IFullPixelBlendMode)</c> at runtime
/// (JIT-eliminated for value types).
/// </para>
/// </remarks>
public interface IFullPixelBlendMode : IBlendMode {

  /// <summary>
  /// Blends all three RGB channels simultaneously.
  /// </summary>
  /// <param name="bgR">Background red in [0,1].</param>
  /// <param name="bgG">Background green in [0,1].</param>
  /// <param name="bgB">Background blue in [0,1].</param>
  /// <param name="fgR">Foreground red in [0,1].</param>
  /// <param name="fgG">Foreground green in [0,1].</param>
  /// <param name="fgB">Foreground blue in [0,1].</param>
  /// <returns>The blended RGB values, each in [0,1].</returns>
  (float R, float G, float B) BlendPixel(float bgR, float bgG, float bgB, float fgR, float fgG, float fgB);
}
