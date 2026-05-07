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
/// Represents a color encoded with the Hybrid Log-Gamma (HLG) transfer function
/// per ITU-R BT.2100 Annex 2.
/// </summary>
/// <remarks>
/// HLG is one of two HDR transfer functions standardised in ITU-R BT.2100
/// (the other being PQ / SMPTE ST.2084). Unlike PQ, HLG is scene-referred and
/// backward-compatible with SDR displays — its lower half (E ≤ 1/12) is a pure
/// power-of-two gamma matching SDR Rec.709 / Rec.2020 BT.1886, and its upper
/// half is a logarithmic curve.
/// <para>OETF (linear scene light → encoded signal):</para>
/// <code>
///   E' = √(3·E)                          for 0 ≤ E ≤ 1/12
///   E' = a·ln(12·E − b) + c              for 1/12 &lt; E ≤ 1
/// </code>
/// where a = 0.17883277, b = 0.28466892, c = 0.55991073. The two branches meet
/// at E = 1/12 → E' = 0.5 (continuous and differentiable).
/// <para>Reference: ITU-R BT.2100-3 (02/2025) Annex 2.</para>
/// <para>All components are in the encoded [0.0, 1.0] range.</para>
/// </remarks>
/// <param name="R">Red component (HLG-encoded, 0.0-1.0).</param>
/// <param name="G">Green component (HLG-encoded, 0.0-1.0).</param>
/// <param name="B">Blue component (HLG-encoded, 0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct HlgF(float R, float G, float B) : IColorSpace3F<HlgF> {

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
  public static HlgF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.R),
    UNorm32.FromFloat(this.G),
    UNorm32.FromFloat(this.B)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static HlgF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
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
