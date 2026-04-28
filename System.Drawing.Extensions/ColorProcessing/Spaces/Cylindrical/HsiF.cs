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

namespace Hawkynt.ColorProcessing.Spaces.Cylindrical;

/// <summary>
/// Represents a color in HSI (Hue, Saturation, Intensity) color space with float components.
/// </summary>
/// <remarks>
/// <para>HSI is a cylindrical RGB-derived space distinct from HSL and HSV.
/// Intensity is the arithmetic mean of the RGB channels (I = (R+G+B)/3) and
/// saturation is defined relative to the minimum channel
/// (S = 1 − min(R,G,B)/I), so a pure-red pixel has I = 1/3 and S = 1.</para>
/// <para>HSI was introduced by Gonzalez &amp; Woods (Digital Image Processing,
/// 1992) and remains common in computer-vision pipelines (segmentation,
/// machine-vision colour analysis) precisely because intensity decouples
/// linearly from chrominance — unlike HSL/HSV's max/min mix.</para>
/// <para>H (hue): 0.0-1.0 (maps to 0-360 degrees).
/// S (saturation): 0.0-1.0.
/// I (intensity): 0.0-1.0 (arithmetic mean of channels).</para>
/// </remarks>
/// <param name="H">Hue component (0.0-1.0, maps to 0-360 degrees).</param>
/// <param name="S">Saturation component (0.0-1.0).</param>
/// <param name="I">Intensity component (0.0-1.0, arithmetic mean of RGB).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct HsiF(float H, float S, float I) : IColorSpace3F<HsiF> {

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
    get => this.I;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HsiF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.H),
    UNorm32.FromFloat(this.S),
    UNorm32.FromFloat(this.I)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HsiF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.H * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.S * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.I * ColorConstants.FloatToByte + 0.5f)
  );
}
