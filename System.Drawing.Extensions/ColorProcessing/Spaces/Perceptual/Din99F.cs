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
/// Represents a color in the DIN99 perceptually-uniform color space with float components.
/// </summary>
/// <remarks>
/// <para>DIN99 is a non-linear remapping of CIELAB designed to give better perceptual
/// uniformity than CIELAB across small-to-moderate colour differences. Standardised in
/// DIN 6176:2001. The Euclidean distance in DIN99 is the DIN99 colour-difference, an
/// industrial alternative to CIEDE2000.</para>
/// <code>
///   L99 = (100/ln(1.0039)) · ln(1 + 0.0039·L*)
///   e   =  a*·cos(16°) + b*·sin(16°)
///   f   =  0.7 · (-a*·sin(16°) + b*·cos(16°))
///   G   =  sqrt(e² + f²)
///   C99 = ln(1 + 0.045·G) / (0.045·0.7)
///   h99 = atan2(f, e) + 16°
///   a99 = C99·cos(h99);  b99 = C99·sin(h99)
/// </code>
/// <para>Reference: G. Cui, M. R. Luo, B. Rigg, G. Roesler &amp; K. Witt,
/// "Uniform colour spaces based on the DIN99 colour-difference formula",
/// Color Research &amp; Application 27(4):282-290, 2002. DIN 6176:2001.</para>
/// <para>L (lightness): 0 to ~105.509. a99: approximately -40 to +40.
/// b99: approximately -40 to +40.</para>
/// </remarks>
/// <param name="L">Lightness component (0 to ~105.509).</param>
/// <param name="A">a99 component (approximately -40 to +40).</param>
/// <param name="B">b99 component (approximately -40 to +40).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct Din99F(float L, float A, float B) : IColorSpace3F<Din99F> {

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
  public static Din99F Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>L: 0-105.509 -> 0-1, A: -40 to 40 -> 0-1, B: -40 to 40 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.L / 105.509f),
    UNorm32.FromFloat((this.A + 40f) / 80f),
    UNorm32.FromFloat((this.B + 40f) / 80f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> L 0-105.509, C2: 0-1 -> A -40 to 40, C3: 0-1 -> B -40 to 40.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Din99F FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat() * 105.509f,
    c2.ToFloat() * 80f - 40f,
    c3.ToFloat() * 80f - 40f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>L: 0-105.509 -> 0-255, A: -40 to 40 -> 0-255, B: -40 to 40 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.L / 105.509f * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.A + 40f) / 80f * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.B + 40f) / 80f * ColorConstants.FloatToByte + 0.5f)
  );
}
