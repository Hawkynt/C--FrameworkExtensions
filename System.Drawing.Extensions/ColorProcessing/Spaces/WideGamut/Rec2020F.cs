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

namespace Hawkynt.ColorProcessing.Spaces.WideGamut;

/// <summary>
/// Represents a color in the ITU-R BT.2020 (Rec.2020) wide-gamut color space.
/// </summary>
/// <remarks>
/// Rec.2020 is the standard color space for UHDTV and HDR. It uses the same D65
/// white point as sRGB but with substantially wider primaries:
/// R = (0.708, 0.292), G = (0.170, 0.797), B = (0.131, 0.046).
/// <para>This struct stores LINEAR Rec.2020 values; the BT.2020 / BT.1886 OETF
/// (a power-1.4-region piecewise curve) is applied at byte-encoding boundaries
/// by separate projectors. Most HDR pipelines pair Rec.2020 primaries with the
/// PQ or HLG transfer functions (see <see cref="Hdr.HlgF"/>).</para>
/// <para>Reference: ITU-R BT.2020-2 (10/2015) Annex 1.</para>
/// </remarks>
/// <param name="R">Red component (0.0-1.0, linear).</param>
/// <param name="G">Green component (0.0-1.0, linear).</param>
/// <param name="B">Blue component (0.0-1.0, linear).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct Rec2020F(float R, float G, float B) : IColorSpace3F<Rec2020F> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.R;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.G;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.B;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rec2020F Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.R),
    UNorm32.FromFloat(this.G),
    UNorm32.FromFloat(this.B)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rec2020F FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.R * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.G * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.B * ColorConstants.FloatToByte + 0.5f)
  );
}
