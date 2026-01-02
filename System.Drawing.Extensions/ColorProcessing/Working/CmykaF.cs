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
/// CMYKA color in float format for working space operations.
/// </summary>
/// <remarks>
/// Used for print-oriented color workflows requiring high precision.
/// Components are typically in 0.0-1.0 range.
/// Alpha is always straight (non-premultiplied).
/// </remarks>
/// <param name="C">Cyan component (0.0-1.0).</param>
/// <param name="M">Magenta component (0.0-1.0).</param>
/// <param name="Y">Yellow component (0.0-1.0).</param>
/// <param name="K">Key/Black component (0.0-1.0).</param>
/// <param name="A">Alpha component (0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 20)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct CmykaF(float C, float M, float Y, float K, float A) : IColorSpace5F<CmykaF>, IErrorOps<CmykaF> {

  /// <summary>
  /// Constructs a CmykaF with opaque alpha.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykaF(float c, float m, float y, float k) : this(c, m, y, k, 1f) { }

  #region IColorSpace5F Implementation

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.C;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.M;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

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

  #endregion

  #region IErrorOps Implementation

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CmykaF Sub(in CmykaF a, in CmykaF b) => new(
    a.C - b.C,
    a.M - b.M,
    a.Y - b.Y,
    a.K - b.K,
    a.A - b.A
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CmykaF AddScaled(in CmykaF color, in CmykaF error, float scale) => new(
    color.C + error.C * scale,
    color.M + error.M * scale,
    color.Y + error.Y * scale,
    color.K + error.K * scale,
    color.A + error.A * scale
  );

  #endregion

  /// <summary>
  /// Linearly interpolates between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CmykaF Lerp(in CmykaF a, in CmykaF b, float t) {
    var invT = 1f - t;
    return new(
      a.C * invT + b.C * t,
      a.M * invT + b.M * t,
      a.Y * invT + b.Y * t,
      a.K * invT + b.K * t,
      a.A * invT + b.A * t
    );
  }

  /// <summary>
  /// Clamps all components to 0.0-1.0 range.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykaF Clamp() => new(
    this.C < 0 ? 0 : this.C > 1 ? 1 : this.C,
    this.M < 0 ? 0 : this.M > 1 ? 1 : this.M,
    this.Y < 0 ? 0 : this.Y > 1 ? 1 : this.Y,
    this.K < 0 ? 0 : this.K > 1 ? 1 : this.K,
    this.A < 0 ? 0 : this.A > 1 ? 1 : this.A
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
