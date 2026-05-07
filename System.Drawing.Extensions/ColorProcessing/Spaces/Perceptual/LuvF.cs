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
/// Represents a color in CIE 1976 L*u*v* color space with float components.
/// </summary>
/// <remarks>
/// <para>CIE L*u*v* is the CIE 1976 perceptually-uniform colour space defined alongside
/// CIELAB. Luv is the preferred space for additive light (displays, projection, lighting,
/// stage design) because chromaticities (u', v') retain the additive-mixing property of
/// CIE 1931 (x, y) — equally-spaced Δu', Δv' on the chromaticity diagram correspond to
/// approximately-equal perceptual differences. Pairs with <see cref="Cylindrical.LchUvF"/>
/// for cylindrical access.</para>
/// <code>
///   u' = 4·X / (X + 15·Y + 3·Z);  v' = 9·Y / (X + 15·Y + 3·Z)
///   L*  = 116·f(Y/Yn) − 16     (same f(t) as Lab)
///   u* = 13·L*·(u' − u'n)
///   v* = 13·L*·(v' − v'n)
/// </code>
/// <para>Reference: CIE 015:2018 §8.2.1.2; ISO/CIE 11664-5. D65 illuminant.</para>
/// <para>L (lightness): 0-100. u: approximately -134 to +220. v: approximately -140 to +122.</para>
/// </remarks>
/// <param name="L">Lightness component (0-100).</param>
/// <param name="U">U chromaticity component (approximately -134 to +220).</param>
/// <param name="V">V chromaticity component (approximately -140 to +122).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct LuvF(float L, float U, float V) : IColorSpace3F<LuvF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.L;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.U;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.V;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LuvF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>L: 0-100 -> 0-1, U: -134 to 220 -> 0-1, V: -140 to 122 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.L / 100f),
    UNorm32.FromFloat((this.U + 134f) / 354f),
    UNorm32.FromFloat((this.V + 140f) / 262f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> L 0-100, C2: 0-1 -> U -134 to 220, C3: 0-1 -> V -140 to 122.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LuvF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat() * 100f,
    c2.ToFloat() * 354f - 134f,
    c3.ToFloat() * 262f - 140f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>L: 0-100 -> 0-255, U: -134 to 220 -> 0-255, V: -140 to 122 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.L / 100f * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.U + 134f) / 354f * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.V + 140f) / 262f * ColorConstants.FloatToByte + 0.5f)
  );
}
