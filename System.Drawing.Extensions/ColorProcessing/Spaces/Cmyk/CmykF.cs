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

namespace Hawkynt.ColorProcessing.Spaces.Cmyk;

/// <summary>
/// Represents a color in CMYK (Cyan, Magenta, Yellow, Key/Black) color space with float components.
/// </summary>
/// <remarks>
/// C (cyan): 0.0-1.0
/// M (magenta): 0.0-1.0
/// Y (yellow): 0.0-1.0
/// K (key/black): 0.0-1.0
/// CMYK is a subtractive color model used in printing.
/// </remarks>
/// <param name="C">Cyan component (0.0-1.0).</param>
/// <param name="M">Magenta component (0.0-1.0).</param>
/// <param name="Y">Yellow component (0.0-1.0).</param>
/// <param name="K">Key/Black component (0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct CmykF(float C, float M, float Y, float K) {

  /// <summary>Gets the first component (Cyan).</summary>
  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.C;
  }

  /// <summary>Gets the second component (Magenta).</summary>
  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.M;
  }

  /// <summary>Gets the third component (Yellow).</summary>
  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

  /// <summary>Gets the fourth component (Key/Black).</summary>
  public float C4 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.K;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CmykF Create(float c1, float c2, float c3, float c4) => new(c1, c2, c3, c4);

  /// <summary>Returns components normalized to 0.0-1.0 range.</summary>
  /// <remarks>Components are already in 0-1 range.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3, UNorm32 C4) ToNormalized() => (
    UNorm32.FromFloat(this.C),
    UNorm32.FromFloat(this.M),
    UNorm32.FromFloat(this.Y),
    UNorm32.FromFloat(this.K)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CmykF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat(),
    c4.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3, byte C4) ToBytes() => (
    (byte)(this.C * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.M * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.Y * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.K * ColorConstants.FloatToByte + 0.5f)
  );
}
