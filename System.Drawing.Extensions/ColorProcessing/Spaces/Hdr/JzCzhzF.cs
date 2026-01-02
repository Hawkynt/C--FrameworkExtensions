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
/// Represents a color in the JzCzhz cylindrical color space with float components.
/// </summary>
/// <remarks>
/// JzCzhz is the cylindrical (polar) representation of JzAzBz.
/// Jz (lightness): 0.0-1.0 (0 = black, 1 = white)
/// Cz (chroma): 0.0 to ~0.5 (colorfulness/saturation)
/// Hz (hue): 0.0-1.0 representing 0-360 degrees
/// Reference: https://observablehq.com/@jrus/jzazbz
/// </remarks>
/// <param name="Jz">Lightness component (0.0-1.0).</param>
/// <param name="Cz">Chroma component (0.0-~0.5).</param>
/// <param name="Hz">Hue component (0.0-1.0, representing 0-360 degrees).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct JzCzhzF(float Jz, float Cz, float Hz) : IColorSpace3F<JzCzhzF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Jz;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Cz;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Hz;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static JzCzhzF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>Jz: 0-1 -> 0-1, Cz: 0-0.5 scaled by 2 -> 0-1, Hz: 0-1 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.Jz),
    UNorm32.FromFloat(this.Cz * 2f),
    UNorm32.FromFloat(this.Hz)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> Jz, C2: 0-1 -> Cz 0-0.5, C3: 0-1 -> Hz.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static JzCzhzF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() * 0.5f,
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.Jz * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.Cz * 2f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.Hz * ColorConstants.FloatToByte + 0.5f)
  );
}
