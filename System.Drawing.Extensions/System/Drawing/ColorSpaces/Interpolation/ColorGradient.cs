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

namespace System.Drawing.ColorSpaces.Interpolation;

/// <summary>
/// Utility class for generating color gradients using any interpolation method.
/// </summary>
/// <remarks>
/// <para>
/// Example usage:
/// <code>
/// // Generate a 256-step gradient using Lab interpolation
/// var gradient = ColorGradient.Generate&lt;LabLerp&gt;(start, end, 256);
///
/// // Generate using circular HSV interpolation for smooth hue transitions
/// var rainbow = ColorGradient.Generate&lt;HsvCircularLerp&gt;(red, magenta, 100);
/// </code>
/// </para>
/// </remarks>
public static class ColorGradient {

  /// <summary>
  /// Generates a gradient between two colors using the specified interpolation method.
  /// </summary>
  /// <typeparam name="TLerp">The lerp implementation to use for interpolation.</typeparam>
  /// <param name="start">The starting color of the gradient.</param>
  /// <param name="end">The ending color of the gradient.</param>
  /// <param name="steps">The number of colors to generate (including start and end).</param>
  /// <returns>An array of interpolated colors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color[] Generate<TLerp>(Color start, Color end, int steps)
    where TLerp : struct, IColorLerp {

    if (steps < 2)
      return steps == 1 ? [start] : [];

    var lerp = default(TLerp);
    var result = new Color[steps];
    result[0] = start;
    result[steps - 1] = end;

    var divisor = steps - 1;
    for (var i = 1; i < steps - 1; ++i) {
      var t = (byte)((i * 255 + divisor / 2) / divisor);
      result[i] = lerp.Lerp(start, end, t);
    }

    return result;
  }

  /// <summary>
  /// Generates a multi-stop gradient using the specified interpolation method.
  /// </summary>
  /// <typeparam name="TLerp">The lerp implementation to use for interpolation.</typeparam>
  /// <param name="stops">Array of color stops (must have at least 2 colors).</param>
  /// <param name="totalSteps">The total number of colors to generate.</param>
  /// <returns>An array of interpolated colors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color[] GenerateMultiStop<TLerp>(Color[] stops, int totalSteps)
    where TLerp : struct, IColorLerp {

    if (stops == null || stops.Length < 2)
      throw new ArgumentException("At least two color stops are required.", nameof(stops));

    if (totalSteps < 2)
      return totalSteps == 1 ? [stops[0]] : [];

    var segments = stops.Length - 1;
    var stepsPerSegment = (totalSteps - 1) / segments;
    var remainder = (totalSteps - 1) % segments;

    var result = new Color[totalSteps];
    var lerp = default(TLerp);
    var index = 0;

    for (var seg = 0; seg < segments; ++seg) {
      var segSteps = stepsPerSegment + (seg < remainder ? 1 : 0);
      var segStart = stops[seg];
      var segEnd = stops[seg + 1];

      for (var i = 0; i < segSteps; ++i) {
        var t = (byte)((i * 255 + segSteps / 2) / segSteps);
        result[index++] = lerp.Lerp(segStart, segEnd, t);
      }
    }

    result[totalSteps - 1] = stops[^1];
    return result;
  }

  /// <summary>
  /// Fills a span with gradient colors using the specified interpolation method.
  /// Zero allocations after initial setup.
  /// </summary>
  /// <typeparam name="TLerp">The lerp implementation to use for interpolation.</typeparam>
  /// <param name="destination">The span to fill with gradient colors.</param>
  /// <param name="start">The starting color of the gradient.</param>
  /// <param name="end">The ending color of the gradient.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Fill<TLerp>(Span<Color> destination, Color start, Color end)
    where TLerp : struct, IColorLerp {

    var steps = destination.Length;
    if (steps == 0)
      return;

    if (steps == 1) {
      destination[0] = start;
      return;
    }

    var lerp = default(TLerp);
    destination[0] = start;
    destination[steps - 1] = end;

    var divisor = steps - 1;
    for (var i = 1; i < steps - 1; ++i) {
      var t = (byte)((i * 255 + divisor / 2) / divisor);
      destination[i] = lerp.Lerp(start, end, t);
    }
  }
}
