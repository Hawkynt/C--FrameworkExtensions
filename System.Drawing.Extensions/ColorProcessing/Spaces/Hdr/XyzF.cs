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

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Represents a color in CIE 1931 XYZ color space with float components.
/// </summary>
/// <remarks>
/// X: typically 0.0-0.95047 (relative to D65 white)
/// Y: 0.0-1.0 (luminance)
/// Z: typically 0.0-1.08883 (relative to D65 white)
/// Uses D65 illuminant as reference white point.
/// XYZ is the fundamental CIE color space from which Lab, Luv, and other
/// perceptual spaces are derived.
/// </remarks>
/// <param name="X">X tristimulus value (0.0-0.95047 for D65).</param>
/// <param name="Y">Y tristimulus value / luminance (0.0-1.0).</param>
/// <param name="Z">Z tristimulus value (0.0-1.08883 for D65).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct XyzF(float X, float Y, float Z) : IColorSpace3F<XyzF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.X;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Z;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static XyzF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <summary>D65 reference white X component.</summary>
  public const float WhiteX = ColorConstants.D65_Xn;

  /// <summary>D65 reference white Y component.</summary>
  public const float WhiteY = ColorConstants.D65_Yn;

  /// <summary>D65 reference white Z component.</summary>
  public const float WhiteZ = ColorConstants.D65_Zn;

  /// <summary>Returns components normalized to 0.0-1.0 range.</summary>
  /// <remarks>X: 0-0.95047 -> 0-1, Y: 0-1 -> 0-1, Z: 0-1.08883 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float C1, float C2, float C3) ToNormalized() => (this.X / WhiteX, this.Y / WhiteY, this.Z / WhiteZ);

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>X: 0-0.95047 -> 0-255, Y: 0-1 -> 0-255, Z: 0-1.08883 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.X / WhiteX * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.Y / WhiteY * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.Z / WhiteZ * ColorConstants.FloatToByte + 0.5f)
  );
}
