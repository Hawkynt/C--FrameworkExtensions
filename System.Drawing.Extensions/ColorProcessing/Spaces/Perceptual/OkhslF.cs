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
using Hawkynt.ColorProcessing.Metrics;
using UNorm32 = Hawkynt.ColorProcessing.Metrics.UNorm32;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Represents a color in Björn Ottosson's OKHSL polar perceptual color space (2020).
/// </summary>
/// <remarks>
/// <para>OKHSL is a perceptually uniform analogue of HSL based on Oklab.
/// Unlike classical HSL, equal increments of L correspond to equal perceived lightness
/// changes, equal increments of S correspond to equal perceived saturation changes
/// across all hues, and the gamut is normalised so S=1 always lies on the sRGB boundary.</para>
/// <para>Designed primarily for <em>colour-picker UIs</em>: dragging an HSL slider in
/// classical HSL produces visually inconsistent steps; OKHSL fixes that.</para>
/// <para>Reference: Björn Ottosson, "Okhsv and Okhsl" (2020) —
/// <see href="https://bottosson.github.io/posts/colorpicker/"/>.</para>
/// <para>H (hue): 0.0-1.0 representing 0-360 degrees.
/// S (saturation): 0.0-1.0.
/// L (lightness): 0.0-1.0 (toe-mapped, so 0.5 = mid-grey perceptually).</para>
/// </remarks>
/// <param name="H">Hue component (0.0-1.0, representing 0-360 degrees).</param>
/// <param name="S">Saturation component (0.0-1.0).</param>
/// <param name="L">Lightness component (0.0-1.0, toe-mapped perceptual lightness).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct OkhslF(float H, float S, float L) : IColorSpace3F<OkhslF> {

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
  public static OkhslF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.H),
    UNorm32.FromFloat(this.S),
    UNorm32.FromFloat(this.L)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OkhslF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.H * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.S * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.L * ColorConstants.FloatToByte + 0.5f)
  );
}
