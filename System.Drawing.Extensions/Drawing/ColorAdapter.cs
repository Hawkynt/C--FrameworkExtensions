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

using System.Drawing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for adapting System.Drawing.Color to the color processing types.
/// </summary>
public static class ColorAdapter {

  extension(Color @this)
  {
    /// <summary>
    /// Converts a System.Drawing.Color to Rgba32.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bgra8888 ToRgba32() => new((uint)@this.ToArgb());

    /// <summary>
    /// Converts a System.Drawing.Color to LinearRgbaF using sRGB gamma expansion.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LinearRgbaF ToLinearRgbaF() {
      var decoder = new Srgb32ToLinearRgbaF();
      return decoder.Decode(@this.ToRgba32());
    }

    /// <summary>
    /// Converts a System.Drawing.Color to Rgb24.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bgr888 ToRgb24() => new(@this.R, @this.G, @this.B);
  }
  
  /// <summary>
  /// Converts Rgba32 to System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this Bgra8888 color) => Color.FromArgb((int)color.Packed);

  /// <summary>
  /// Converts LinearRgbaF to System.Drawing.Color using sRGB gamma compression.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this LinearRgbaF color) {
    var encoder = new LinearRgbaFToSrgb32();
    return encoder.Encode(color).ToColor();
  }

  /// <summary>
  /// Converts Rgb24 to System.Drawing.Color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Color ToColor(this Bgr888 color) => Color.FromArgb(color.R, color.G, color.B);
}
