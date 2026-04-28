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
/// Represents a color in Björn Ottosson's OKHSV polar perceptual color space (2020).
/// </summary>
/// <remarks>
/// <para>OKHSV is a perceptually uniform analogue of HSV based on Oklab.
/// Like classical HSV, V=1 corresponds to the brightest in-gamut shade for
/// the given hue, but unlike classical HSV the perceived brightness step is
/// uniform across hues, and S=1 always lies on the sRGB gamut boundary.</para>
/// <para>Designed primarily for <em>colour-picker UIs</em>: classical HSV
/// produces "saturation cliffs" near the gamut hull; OKHSV avoids them.</para>
/// <para>Reference: Björn Ottosson, "Okhsv and Okhsl" (2020) —
/// <see href="https://bottosson.github.io/posts/colorpicker/"/>.</para>
/// <para>H (hue): 0.0-1.0 representing 0-360 degrees.
/// S (saturation): 0.0-1.0.
/// V (value): 0.0-1.0 (toe-mapped, perceptual brightness).</para>
/// </remarks>
/// <param name="H">Hue component (0.0-1.0, representing 0-360 degrees).</param>
/// <param name="S">Saturation component (0.0-1.0).</param>
/// <param name="V">Value component (0.0-1.0, toe-mapped perceptual brightness).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct OkhsvF(float H, float S, float V) : IColorSpace3F<OkhsvF> {

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
    get => this.V;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OkhsvF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.H),
    UNorm32.FromFloat(this.S),
    UNorm32.FromFloat(this.V)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OkhsvF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.H * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.S * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.V * ColorConstants.FloatToByte + 0.5f)
  );
}
