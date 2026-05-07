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
/// Represents a color in the Oklab perceptual color space with float components.
/// </summary>
/// <remarks>
/// <para>Oklab is a perceptually-uniform colour space designed by Björn Ottosson in 2020,
/// fitted via least-squares to the IPT and CAM16 colour-difference data sets. It
/// outperforms CIELAB at hue-uniformity (no chroma-induced hue shifts under linear
/// interpolation) while retaining a closed-form polynomial transform. Pairs with
/// <see cref="OklchF"/> for cylindrical access and is the basis of OKHSL/OKHSV
/// (<see cref="OkhslF"/>, <see cref="OkhsvF"/>).</para>
/// <code>
///   linear sRGB → LMS' (3×3 matrix M1) → cube-root → Oklab (3×3 matrix M2)
/// </code>
/// <para>Reference: B. Ottosson, "A perceptual color space for image processing", 2020.
/// <see href="https://bottosson.github.io/posts/oklab/"/>. Specified in CSS Color
/// Module Level 4.</para>
/// <para>L (lightness): 0.0-1.0 (0 = black, 1 = white). a (green-red): ~-0.4 to +0.4.
/// b (blue-yellow): ~-0.4 to +0.4.</para>
/// </remarks>
/// <param name="L">Lightness component (0.0-1.0).</param>
/// <param name="A">Green-red component (approximately -0.4 to +0.4).</param>
/// <param name="B">Blue-yellow component (approximately -0.4 to +0.4).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct OklabF(float L, float A, float B) : IColorSpace3F<OklabF> {

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
  public static OklabF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>L: 0-1 -> 0-1, A: -0.4 to 0.4 -> 0-1, B: -0.4 to 0.4 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.L),
    UNorm32.FromFloat((this.A + 0.4f) / 0.8f),
    UNorm32.FromFloat((this.B + 0.4f) / 0.8f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0-1 -> L 0-1, C2: 0-1 -> A -0.4 to 0.4, C3: 0-1 -> B -0.4 to 0.4.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklabF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() * 0.8f - 0.4f,
    c3.ToFloat() * 0.8f - 0.4f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>L: 0-1 -> 0-255, A: -0.4 to 0.4 -> 0-255, B: -0.4 to 0.4 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.L * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.A + 0.4f) / 0.8f * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.B + 0.4f) / 0.8f * ColorConstants.FloatToByte + 0.5f)
  );
}
