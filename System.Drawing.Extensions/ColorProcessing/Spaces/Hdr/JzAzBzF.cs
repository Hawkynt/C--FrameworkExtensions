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
/// Represents a color in the JzAzBz perceptual color space with float components.
/// </summary>
/// <remarks>
/// JzAzBz is an HDR-capable perceptual color space designed by Safdar et al. in 2017.
/// It uses the Perceptual Quantizer (PQ) transfer function for HDR support.
/// Jz (lightness): 0.0-1.0 (0 = black, 1 = white at reference luminance)
/// Az (green-red): approximately -0.5 to +0.5
/// Bz (blue-yellow): approximately -0.5 to +0.5
/// Reference: https://observablehq.com/@jrus/jzazbz
/// </remarks>
/// <param name="Jz">Lightness component (0.0-1.0).</param>
/// <param name="Az">Green-red component (approximately -0.5 to +0.5).</param>
/// <param name="Bz">Blue-yellow component (approximately -0.5 to +0.5).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct JzAzBzF(float Jz, float Az, float Bz) : IColorSpace3F<JzAzBzF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Jz;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Az;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Bz;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static JzAzBzF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>Jz: 0-1 -> 0-1, Az: -0.5 to 0.5 -> 0-1, Bz: -0.5 to 0.5 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.Jz),
    UNorm32.FromFloat(this.Az + 0.5f),
    UNorm32.FromFloat(this.Bz + 0.5f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> Jz, C2: 0-1 -> Az -0.5 to 0.5, C3: 0-1 -> Bz -0.5 to 0.5.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static JzAzBzF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() - 0.5f,
    c3.ToFloat() - 0.5f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>Jz: 0-1 -> 0-255, Az: -0.5 to 0.5 -> 0-255, Bz: -0.5 to 0.5 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.Jz * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Az + 0.5f) * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Bz + 0.5f) * ColorConstants.FloatToByte + 0.5f)
  );
}
