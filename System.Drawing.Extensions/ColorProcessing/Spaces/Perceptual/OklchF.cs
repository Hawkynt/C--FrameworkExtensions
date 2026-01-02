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
/// Represents a color in the OkLCh cylindrical color space with float components.
/// </summary>
/// <remarks>
/// OkLCh is the cylindrical (polar) representation of Oklab.
/// L (lightness): 0.0-1.0 (0 = black, 1 = white)
/// C (chroma): 0.0 to ~0.4 (colorfulness/saturation)
/// H (hue): 0.0-1.0 representing 0-360 degrees
/// Reference: https://bottosson.github.io/posts/oklab/
/// </remarks>
/// <param name="L">Lightness component (0.0-1.0).</param>
/// <param name="C">Chroma component (0.0-~0.4).</param>
/// <param name="H">Hue component (0.0-1.0, representing 0-360 degrees).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct OklchF(float L, float C, float H) : IColorSpace3F<OklchF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.L;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.C;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.H;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklchF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>L, C (scaled by 2.5), H are in 0-1 range.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.L),
    UNorm32.FromFloat(this.C * 2.5f),
    UNorm32.FromFloat(this.H)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> L, C2: 0-1 -> C (divided by 2.5), C3: 0-1 -> H.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklchF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() / 2.5f,
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.L * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.C * 2.5f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.H * ColorConstants.FloatToByte + 0.5f)
  );
}
