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
/// Represents a color in NTSC YIQ analogue-broadcast color space with float components.
/// </summary>
/// <remarks>
/// <para>YIQ was the colour-encoding standard of NTSC analogue television (FCC 1953;
/// SMPTE 170M). It rotates the (U, V) chrominance plane 33° relative to YUV so that
/// the I axis aligns with the orange-blue direction of greatest human-eye acuity,
/// allowing a wider I bandwidth (1.3 MHz) and narrower Q bandwidth (0.4 MHz) for
/// efficient analogue transmission.</para>
/// <para>Distinct from <see cref="YuvF"/>: YUV is the PAL/SECAM-family chroma encoding
/// used by BT.601/709 and most digital video; YIQ is the NTSC-family equivalent.</para>
/// <para>Y (luma): 0.0-1.0.
/// I (in-phase, orange-blue chroma): approximately -0.596 to +0.596.
/// Q (quadrature, purple-green chroma): approximately -0.523 to +0.523.</para>
/// </remarks>
/// <param name="Y">Luma component (0.0-1.0).</param>
/// <param name="I">In-phase chrominance (approximately -0.596 to +0.596).</param>
/// <param name="Q">Quadrature chrominance (approximately -0.523 to +0.523).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct YiqF(float Y, float I, float Q) : IColorSpace3F<YiqF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.I;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Q;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YiqF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>Y: 0-1, I: -0.596 to 0.596 -> 0-1, Q: -0.523 to 0.523 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.Y),
    UNorm32.FromFloat((this.I + 0.596f) / 1.192f),
    UNorm32.FromFloat((this.Q + 0.523f) / 1.046f)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YiqF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() * 1.192f - 0.596f,
    c3.ToFloat() * 1.046f - 0.523f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.Y * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.I + 0.596f) / 1.192f * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Q + 0.523f) / 1.046f * ColorConstants.FloatToByte + 0.5f)
  );
}
