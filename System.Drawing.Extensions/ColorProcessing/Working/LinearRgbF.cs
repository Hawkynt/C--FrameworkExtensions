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
using Hawkynt.ColorProcessing.ColorMath;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Working;

/// <summary>
/// Linear RGB color in float format for working space operations (no alpha).
/// </summary>
/// <remarks>
/// Used when alpha channel is not needed, saving memory bandwidth.
/// Components are typically in 0.0-1.0 range but may exceed during processing.
/// </remarks>
/// <param name="R">Red component (typically 0.0-1.0).</param>
/// <param name="G">Green component (typically 0.0-1.0).</param>
/// <param name="B">Blue component (typically 0.0-1.0).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct LinearRgbF(float R, float G, float B) : IColorSpace3F<LinearRgbF>, IErrorOps<LinearRgbF> {

  #region IColorSpace3F Implementation

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

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  #endregion

  #region IErrorOps Implementation

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbF Sub(in LinearRgbF a, in LinearRgbF b) => new(
    a.R - b.R,
    a.G - b.G,
    a.B - b.B
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbF AddScaled(in LinearRgbF color, in LinearRgbF error, float scale) => new(
    color.R + error.R * scale,
    color.G + error.G * scale,
    color.B + error.B * scale
  );

  #endregion
  
  /// <summary>
  /// Linearly interpolates between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbF Lerp(in LinearRgbF a, in LinearRgbF b, float t) {
    var invT = 1f - t;
    return new(
      a.R * invT + b.R * t,
      a.G * invT + b.G * t,
      a.B * invT + b.B * t
    );
  }

  /// <summary>
  /// Clamps all components to 0.0-1.0 range.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Clamp() => new(
    this.R < 0 ? 0 : this.R > 1 ? 1 : this.R,
    this.G < 0 ? 0 : this.G > 1 ? 1 : this.G,
    this.B < 0 ? 0 : this.B > 1 ? 1 : this.B
  );

  /// <summary>Returns components normalized to 0.0-1.0 range.</summary>
  /// <remarks>Components are already in 0-1 range (clamped if needed).</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float C1, float C2, float C3) ToNormalized() => (this.R, this.G, this.B);

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.R * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.G * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.B * ColorConstants.FloatToByte + 0.5f)
  );
}
