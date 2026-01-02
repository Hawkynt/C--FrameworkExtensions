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

namespace Hawkynt.ColorProcessing.Spaces.Yuv;

/// <summary>
/// Represents a color in YCbCr (Y'CbCr) color space with float components.
/// </summary>
/// <remarks>
/// YCbCr is the digital video color encoding used in ITU-R BT.601 (SDTV) and BT.709 (HDTV).
/// Y' (luma): 0.0-1.0 (gamma-corrected luminance)
/// Cb (blue-difference chroma): -0.5 to 0.5
/// Cr (red-difference chroma): -0.5 to 0.5
/// Unlike YUV (analog), YCbCr uses gamma-corrected (non-linear) RGB input.
/// Reference: https://en.wikipedia.org/wiki/YCbCr
/// </remarks>
/// <param name="Y">Luma component (0.0-1.0).</param>
/// <param name="Cb">Blue-difference chroma component (-0.5 to 0.5).</param>
/// <param name="Cr">Red-difference chroma component (-0.5 to 0.5).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct YCbCrF(float Y, float Cb, float Cr) : IColorSpace3F<YCbCrF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Cb;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Cr;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YCbCrF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>Y: 0-1 -> 0-1, Cb: -0.5 to 0.5 -> 0-1, Cr: -0.5 to 0.5 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.Y),
    UNorm32.FromFloat(this.Cb + 0.5f),
    UNorm32.FromFloat(this.Cr + 0.5f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> Y, C2: 0-1 -> Cb -0.5 to 0.5, C3: 0-1 -> Cr -0.5 to 0.5.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YCbCrF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() - 0.5f,
    c3.ToFloat() - 0.5f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>Y: 0-1 -> 0-255, Cb: -0.5 to 0.5 -> 0-255, Cr: -0.5 to 0.5 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.Y * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Cb + 0.5f) * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Cr + 0.5f) * ColorConstants.FloatToByte + 0.5f)
  );

  /// <summary>Returns components in studio video range (16-235 for Y, 16-240 for Cb/Cr).</summary>
  /// <remarks>This is the standard encoding used in digital video formats.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte Y, byte Cb, byte Cr) ToStudioRange() => (
    (byte)(16f + this.Y * 219f + 0.5f),
    (byte)(128f + this.Cb * 224f + 0.5f),
    (byte)(128f + this.Cr * 224f + 0.5f)
  );
}
