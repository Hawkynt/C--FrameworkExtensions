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
/// Represents a color in CIE LCh (cylindrical Lab) color space with float components.
/// </summary>
/// <remarks>
/// L (lightness): 0-100
/// C (chroma): 0-~128 (typical maximum)
/// H (hue): 0.0-1.0 (normalized, maps to 0-360 degrees)
/// LCh is the polar/cylindrical representation of CIE Lab.
/// </remarks>
/// <param name="L">Lightness component (0-100).</param>
/// <param name="C">Chroma component (0-~128).</param>
/// <param name="H">Hue component (0.0-1.0, normalized).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct LchF(float L, float C, float H) : IColorSpace3F<LchF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.L;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.C;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.H;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LchF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <summary>Returns components normalized to 0.0-1.0 range.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float C1, float C2, float C3) ToNormalized() => (this.L / 100f, this.C / 128f, this.H);

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.L / 100f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.C / 128f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.H * ColorConstants.FloatToByte + 0.5f)
  );
}
