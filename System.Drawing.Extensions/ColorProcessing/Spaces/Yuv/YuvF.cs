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
/// Represents a color in YUV color space with float components.
/// </summary>
/// <remarks>
/// Y (luminance): 0.0-1.0
/// U (blue chrominance): -0.5 to 0.5
/// V (red chrominance): -0.5 to 0.5
/// </remarks>
/// <param name="Y">Luminance component (0.0-1.0).</param>
/// <param name="U">Blue chrominance component (-0.5 to 0.5).</param>
/// <param name="V">Red chrominance component (-0.5 to 0.5).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct YuvF(float Y, float U, float V) : IColorSpace3F<YuvF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.U;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.V;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YuvF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>Y: 0-1 -> 0-1, U: -0.5 to 0.5 -> 0-1, V: -0.5 to 0.5 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.Y),
    UNorm32.FromFloat(this.U + 0.5f),
    UNorm32.FromFloat(this.V + 0.5f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> Y, C2: 0-1 -> U -0.5 to 0.5, C3: 0-1 -> V -0.5 to 0.5.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YuvF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() - 0.5f,
    c3.ToFloat() - 0.5f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>Y: 0-1 -> 0-255, U: -0.5 to 0.5 -> 0-255, V: -0.5 to 0.5 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.Y * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.U + 0.5f) * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.V + 0.5f) * ColorConstants.FloatToByte + 0.5f)
  );
}
