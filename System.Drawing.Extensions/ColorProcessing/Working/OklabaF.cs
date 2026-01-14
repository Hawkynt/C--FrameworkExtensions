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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Working;

/// <summary>
/// OkLab color with alpha in float format for perceptually uniform operations.
/// </summary>
/// <remarks>
/// <para>OkLab is a perceptually uniform color space where equal distances
/// correspond to equal perceived color differences.</para>
/// <para>L is in [0,1], a and b are approximately in [-0.4, 0.4].</para>
/// <para>Alpha is always straight (non-premultiplied) in [0,1].</para>
/// </remarks>
/// <param name="L">Lightness component (0.0-1.0).</param>
/// <param name="A">Green-red axis (approximately -0.4 to 0.4).</param>
/// <param name="B">Blue-yellow axis (approximately -0.4 to 0.4).</param>
/// <param name="Alpha">Alpha component (0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct OklabaF(float L, float A, float B, float Alpha) : IColorSpace4F<OklabaF>, IErrorOps<OklabaF> {

  /// <summary>
  /// Constructs an OklabaF with opaque alpha.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabaF(float l, float a, float b) : this(l, a, b, 1f) { }

  #region IColorSpace4F Implementation

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

  float IColorSpace4F<OklabaF>.A {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Alpha;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklabaF Create(float c1, float c2, float c3, float a) => new(c1, c2, c3, a);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3, UNorm32 A) ToNormalized() => (
    UNorm32.FromFloat(this.L),
    UNorm32.FromFloat((this.A + 0.5f)), // Shift a from [-0.5,0.5] to [0,1]
    UNorm32.FromFloat((this.B + 0.5f)), // Shift b from [-0.5,0.5] to [0,1]
    UNorm32.FromFloat(this.Alpha)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklabaF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => new(
    c1.ToFloat(),
    c2.ToFloat() - 0.5f, // Shift back to [-0.5,0.5]
    c3.ToFloat() - 0.5f, // Shift back to [-0.5,0.5]
    a.ToFloat()
  );

  #endregion

  #region IErrorOps Implementation

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklabaF Sub(in OklabaF a, in OklabaF b) => new(
    a.L - b.L,
    a.A - b.A,
    a.B - b.B,
    a.Alpha - b.Alpha
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static OklabaF AddScaled(in OklabaF color, in OklabaF error, float scale) => new(
    color.L + error.L * scale,
    color.A + error.A * scale,
    color.B + error.B * scale,
    color.Alpha + error.Alpha * scale
  );

  #endregion

  /// <summary>
  /// Clamps all components to valid ranges.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public OklabaF Clamp() => new(
    this.L < 0 ? 0 : this.L > 1 ? 1 : this.L,
    this.A < -0.5f ? -0.5f : this.A > 0.5f ? 0.5f : this.A,
    this.B < -0.5f ? -0.5f : this.B > 0.5f ? 0.5f : this.B,
    this.Alpha < 0 ? 0 : this.Alpha > 1 ? 1 : this.Alpha
  );
}
