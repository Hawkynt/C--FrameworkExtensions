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
/// Represents a color in CMYK with straight (non-premultiplied) alpha — the 5F variant
/// of <see cref="CmykF"/>.
/// </summary>
/// <remarks>
/// CMYK is a subtractive color model used in printing; alpha is preserved through
/// conversion as a separate channel (it does not interact with the K-component
/// black-extraction). Round-trip via <see cref="LinearRgbaF"/> preserves alpha exactly.
/// </remarks>
/// <param name="C">Cyan component (0.0-1.0).</param>
/// <param name="M">Magenta component (0.0-1.0).</param>
/// <param name="Y">Yellow component (0.0-1.0).</param>
/// <param name="K">Key/Black component (0.0-1.0).</param>
/// <param name="A">Alpha component (0.0-1.0, straight).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 20)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct CmykaF(float C, float M, float Y, float K, float A) : IColorSpace5F<CmykaF> {

  /// <summary>Constructs a CmykaF with opaque alpha.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykaF(float c, float m, float y, float k) : this(c, m, y, k, 1f) { }

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

  float IColorSpace5F<CmykaF>.A {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.A;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CmykaF Create(float c1, float c2, float c3, float c4, float a) => new(c1, c2, c3, c4, a);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3, UNorm32 C4, UNorm32 A) ToNormalized() => (
    UNorm32.FromFloat(this.C),
    UNorm32.FromFloat(this.M),
    UNorm32.FromFloat(this.Y),
    UNorm32.FromFloat(this.K),
    UNorm32.FromFloat(this.A)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CmykaF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat(),
    c4.ToFloat(),
    a.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3, byte C4, byte A) ToBytes() => (
    (byte)(this.C * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.M * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.Y * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.K * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.A * ColorConstants.FloatToByte + 0.5f)
  );
}
