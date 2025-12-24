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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Lab;

/// <summary>
/// Represents a color in CIE L*a*b* color space with float components.
/// </summary>
/// <remarks>
/// L (lightness): 0-100
/// a (green-red): approximately -128 to +127
/// b (blue-yellow): approximately -128 to +127
/// Uses D65 illuminant as reference white point.
/// </remarks>
/// <param name="L">Lightness component (0-100).</param>
/// <param name="A">Green-red component (approximately -128 to +127).</param>
/// <param name="B">Blue-yellow component (approximately -128 to +127).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct LabF(float L, float A, float B) : IColorSpace3F<LabF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.L;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.A;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.B;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LabF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <summary>Returns components normalized to 0.0-1.0 range.</summary>
  /// <remarks>L: 0-100 -> 0-1, A: -128 to 127 -> 0-1, B: -128 to 127 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float C1, float C2, float C3) ToNormalized() => (this.L / 100f, (this.A + 128f) / 255f, (this.B + 128f) / 255f);

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>L: 0-100 -> 0-255, A: -128 to 127 -> 0-255, B: -128 to 127 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.L / 100f * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.A + 128f + 0.5f),
    (byte)(this.B + 128f + 0.5f)
  );
}
