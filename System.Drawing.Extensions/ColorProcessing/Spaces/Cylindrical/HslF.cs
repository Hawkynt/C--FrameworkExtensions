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
using System.Runtime.InteropServices;
using Hawkynt.ColorProcessing.Constants;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Cylindrical;

/// <summary>
/// Represents a color in HSL (Hue, Saturation, Lightness) color space with float components.
/// </summary>
/// <remarks>
/// H (hue): 0.0-1.0 (maps to 0-360 degrees)
/// S (saturation): 0.0-1.0
/// L (lightness): 0.0-1.0
/// HSL is a cylindrical representation where lightness 0.5 gives the most saturated colors.
/// </remarks>
/// <param name="H">Hue component (0.0-1.0, maps to 0-360 degrees).</param>
/// <param name="S">Saturation component (0.0-1.0).</param>
/// <param name="L">Lightness component (0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct HslF(float H, float S, float L) : IColorSpace3F<HslF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.H;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.S;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.L;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HslF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <summary>Returns components normalized to 0.0-1.0 range.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float C1, float C2, float C3) ToNormalized() => (this.H, this.S, this.L);

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.H * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.S * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.L * ColorConstants.FloatToByte + 0.5f)
  );
}
