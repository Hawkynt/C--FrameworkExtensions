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

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Represents a color in CIE 1931 xyY chromaticity-plus-luminance color space.
/// </summary>
/// <remarks>
/// <para>xyY separates a colour into pure chromaticity (x, y on the CIE 1931 horseshoe)
/// and luminance (Y). It is the canonical space for plotting gamut diagrams and
/// expressing white-point coordinates. For example: D65 white = (0.31271, 0.32902, 1.0).</para>
/// <para>Distinct from <see cref="XyzF"/> (tristimulus): xyY is a non-linear normalisation
/// where x = X/(X+Y+Z) and y = Y/(X+Y+Z), preserving Y as luminance. Two colours with
/// identical (x, y) are <em>chromatically</em> equal regardless of brightness.</para>
/// <para>Reference: CIE 1931 standard observer (Wright &amp; Guild experiments).</para>
/// <para>x: 0.0-0.8 (chromaticity, x + y ≤ 1).
/// y: 0.0-0.9 (chromaticity).
/// Y: 0.0-1.0 (relative luminance).</para>
/// </remarks>
/// <param name="X">Chromaticity x (0.0-0.8).</param>
/// <param name="Y">Chromaticity y (0.0-0.9).</param>
/// <param name="BigY">Luminance Y (0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct XyYF(float X, float Y, float BigY) : IColorSpace3F<XyYF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.X;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.BigY;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static XyYF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>x: 0-0.8 -> 0-1, y: 0-0.9 -> 0-1, Y: 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.X / 0.8f),
    UNorm32.FromFloat(this.Y / 0.9f),
    UNorm32.FromFloat(this.BigY)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static XyYF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat() * 0.8f,
    c2.ToFloat() * 0.9f,
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.X / 0.8f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.Y / 0.9f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.BigY * ColorConstants.FloatToByte + 0.5f)
  );
}
