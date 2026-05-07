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
/// Represents a color in ACEScg (ACES Compositing Color Space, AP1 primaries) color space.
/// </summary>
/// <remarks>
/// <para>ACEScg is the Academy Color Encoding System "Working Space" for CGI rendering,
/// compositing, and VFX, defined by AMPAS in S-2014-004 ("ACEScg, A Working Space for
/// CGI Render and Compositing"). Linear scene-referred encoding with AP1 primaries
/// (a slightly narrower variant of the AP0 primaries used by ACES2065-1) and the
/// ACES D60 white point (CIE x=0.32168, y=0.33767).</para>
/// <para>Reference: AMPAS Specification S-2014-004 (ACEScg). Bradford D65→D60 chromatic
/// adaptation is applied during sRGB conversion (see audit memory: white drifts to
/// ≈(1.0045, 0.998, 1.003) — a documented colorimetric divergence, not a bug).</para>
/// <para>All components are linear (no gamma) and can exceed 0.0-1.0 for HDR signals.</para>
/// </remarks>
/// <param name="R">Red component (linear, scene-referred).</param>
/// <param name="G">Green component (linear, scene-referred).</param>
/// <param name="B">Blue component (linear, scene-referred).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct AcesCgF(float R, float G, float B) : IColorSpace3F<AcesCgF> {

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
  public static AcesCgF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>ACEScg can have values outside 0-1, but this clamps to that range.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.R),
    UNorm32.FromFloat(this.G),
    UNorm32.FromFloat(this.B)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static AcesCgF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() {
    var r = this.R < 0 ? 0 : this.R > 1 ? 1 : this.R;
    var g = this.G < 0 ? 0 : this.G > 1 ? 1 : this.G;
    var b = this.B < 0 ? 0 : this.B > 1 ? 1 : this.B;
    return (
      (byte)(r * ColorConstants.FloatToByte + 0.5f),
      (byte)(g * ColorConstants.FloatToByte + 0.5f),
      (byte)(b * ColorConstants.FloatToByte + 0.5f)
    );
  }
}
