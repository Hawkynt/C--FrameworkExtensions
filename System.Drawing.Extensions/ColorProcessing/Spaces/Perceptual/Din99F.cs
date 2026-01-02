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
/// Represents a color in the DIN99 color space with float components.
/// </summary>
/// <remarks>
/// DIN99 is a German standard (DIN 6176) optimized for small color differences.
/// It transforms Lab coordinates into a more perceptually uniform space.
/// L (lightness): 0 to ~105.509
/// A (a99): approximately -40 to +40
/// B (b99): approximately -40 to +40
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
