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
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Working;

/// <summary>
/// Linear RGBA color in float format for working space operations.
/// </summary>
/// <remarks>
/// All math should be performed in this space for correct results.
/// Alpha is always straight (non-premultiplied).
/// Components are typically in 0.0-1.0 range but may exceed during processing.
/// </remarks>
/// <param name="R">Red component (typically 0.0-1.0).</param>
/// <param name="G">Green component (typically 0.0-1.0).</param>
/// <param name="B">Blue component (typically 0.0-1.0).</param>
/// <param name="A">Alpha component (0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct LinearRgbaF(float R, float G, float B, float A) : IColorSpace4F<LinearRgbaF>, IErrorOps<LinearRgbaF> {

  /// <summary>
  /// Constructs a LinearRgbaF with opaque alpha.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbaF(float r, float g, float b) : this(r, g, b, 1f) { }

  #region IColorSpace4F Implementation

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

  float IColorSpace4F<LinearRgbaF>.A {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.A;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbaF Create(float c1, float c2, float c3, float a) => new(c1, c2, c3, a);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3, UNorm32 A) ToNormalized() => (
    UNorm32.FromFloat(this.R),
    UNorm32.FromFloat(this.G),
    UNorm32.FromFloat(this.B),
    UNorm32.FromFloat(this.A)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbaF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat(),
    a.ToFloat()
  );

  #endregion

  #region IErrorOps Implementation

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbaF Sub(in LinearRgbaF a, in LinearRgbaF b) => new(
    a.R - b.R,
    a.G - b.G,
    a.B - b.B,
    a.A - b.A
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbaF AddScaled(in LinearRgbaF color, in LinearRgbaF error, float scale) => new(
    color.R + error.R * scale,
    color.G + error.G * scale,
    color.B + error.B * scale,
    color.A + error.A * scale
  );

  #endregion

  /// <summary>
  /// Linearly interpolates between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbaF Lerp(in LinearRgbaF a, in LinearRgbaF b, float t) {
    var invT = 1f - t;
    return new(
      a.R * invT + b.R * t,
      a.G * invT + b.G * t,
      a.B * invT + b.B * t,
      a.A * invT + b.A * t
    );
  }

  /// <summary>
  /// Clamps all components to 0.0-1.0 range.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbaF Clamp() => new(
    this.R < 0 ? 0 : this.R > 1 ? 1 : this.R,
    this.G < 0 ? 0 : this.G > 1 ? 1 : this.G,
    this.B < 0 ? 0 : this.B > 1 ? 1 : this.B,
    this.A < 0 ? 0 : this.A > 1 ? 1 : this.A
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3, byte A) ToBytes() => (
    (byte)(this.R * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.G * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.B * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.A * ColorConstants.FloatToByte + 0.5f)
  );
}
