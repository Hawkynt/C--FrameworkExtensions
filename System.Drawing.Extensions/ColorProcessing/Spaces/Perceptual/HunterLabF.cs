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
/// Represents a color in the Hunter Lab color space with float components.
/// </summary>
/// <remarks>
/// <para>Hunter Lab is the original opponent-coordinate Lab space, developed by Richard S.
/// Hunter in 1948 as a square-root-based approximation that pre-dated the CIE 1976 cube-root
/// CIELAB formulation by 28 years. Still used in some food, paint, and textile-industry
/// instruments (HunterLab company colorimeters).</para>
/// <code>
///   L = 100 · sqrt(Y / Yn)
///   a = Ka · (X/Xn − Y/Yn) / sqrt(Y / Yn)
///   b = Kb · (Y/Yn − Z/Zn) / sqrt(Y / Yn)
/// </code>
/// where Ka, Kb are illuminant-dependent constants (D65: Ka=172.30, Kb=67.20).
/// <para>Reference: R. S. Hunter, "Photoelectric color difference meter", J. Opt. Soc. Am.
/// 38(7):661, 1948. Note: lib uses Y∈[0,1] white = (Xn, 1, Zn) so L = 10 at white,
/// not the published L=100 convention (documented divergence — see audit memory).</para>
/// <para>L (lightness): 0-100. a (red-green): negative = green, positive = red.
/// b (yellow-blue): negative = blue, positive = yellow. D65 illuminant.</para>
/// </remarks>
/// <param name="L">Lightness component (0-100).</param>
/// <param name="A">Red-green component (negative = green, positive = red).</param>
/// <param name="B">Yellow-blue component (negative = blue, positive = yellow).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct HunterLabF(float L, float A, float B) : IColorSpace3F<HunterLabF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.L;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.A;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.B;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HunterLabF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>L: 0-100 -> 0-1, A: -128 to 127 -> 0-1, B: -128 to 127 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.L / 100f),
    UNorm32.FromFloat((this.A + 128f) / 255f),
    UNorm32.FromFloat((this.B + 128f) / 255f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> L 0-100, C2: 0-1 -> A -128 to 127, C3: 0-1 -> B -128 to 127.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HunterLabF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat() * 100f,
    c2.ToFloat() * 255f - 128f,
    c3.ToFloat() * 255f - 128f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>L: 0-100 -> 0-255, A: -128 to 127 -> 0-255, B: -128 to 127 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.L / 100f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.A + 128f + 0.5f),
    (byte)(this.B + 128f + 0.5f)
  );
}
