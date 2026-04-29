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
using Hawkynt.ColorProcessing.Metrics;
using UNorm32 = Hawkynt.ColorProcessing.Metrics.UNorm32;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Represents a color in the CAM16-UCS perceptually-uniform color space.
/// </summary>
/// <remarks>
/// <para>CAM16-UCS is the Uniform Color Space projection of the CAM16 colour-appearance
/// model (Li, Luo et al. 2017). Like Lab and Oklab the axes are (lightness, green-red,
/// blue-yellow), but they're derived from a full appearance model that accounts for
/// adaptation, surround conditions, and contrast effects. Pairs with the
/// <see cref="Hawkynt.ColorProcessing.Metrics.Cam.Cam16UcsDistance"/> metric for
/// perceptually-correct color-difference measurement.</para>
/// <para>Reference: Li, Li, Wang, Zu, Luo, Cui, Melgosa, Brill &amp; Pointer 2017,
/// "Comprehensive color solutions: CAM16, CAT16, and CAM16-UCS", Color Research &amp;
/// Application 42(6):703–718.</para>
/// </remarks>
/// <param name="J">Lightness J' — typically 0..100 for in-gamut colors.</param>
/// <param name="A">Green-red a' — typically -50..+50.</param>
/// <param name="B">Blue-yellow b' — typically -50..+50.</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct Cam16UcsF(float J, float A, float B) : IColorSpace3F<Cam16UcsF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.J;
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
  public static Cam16UcsF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>J: 0..100 → 0..1; a/b: -50..+50 → 0..1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.J / 100f),
    UNorm32.FromFloat((this.A + 50f) / 100f),
    UNorm32.FromFloat((this.B + 50f) / 100f)
  );

  /// <summary>Creates from normalized values.</summary>
  /// <remarks>C1: 0..1 → J 0..100; C2/C3: 0..1 → a/b -50..+50.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Cam16UcsF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat() * 100f,
    c2.ToFloat() * 100f - 50f,
    c3.ToFloat() * 100f - 50f
  );

  /// <summary>Returns components as bytes (0..255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() {
    var (c1, c2, c3) = this.ToNormalized();
    return ((byte)(c1.ToFloat() * 255f + 0.5f), (byte)(c2.ToFloat() * 255f + 0.5f), (byte)(c3.ToFloat() * 255f + 0.5f));
  }
}
