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
/// Represents a color in the ICtCp perceptual color space with float components.
/// </summary>
/// <remarks>
/// ICtCp is Dolby's HDR color space specified in ITU-R BT.2100.
/// It uses the Perceptual Quantizer (PQ) transfer function.
/// I (intensity): 0.0-1.0 (luma-like component)
/// Ct (blue-yellow): approximately -0.5 to +0.5
/// Cp (green-magenta): approximately -0.5 to +0.5
/// Reference: https://professional.dolby.com/siteassets/pdfs/ictcp_dolbywhitepaper_v071.pdf
/// </remarks>
/// <param name="I">Intensity/lightness component (0.0-1.0).</param>
/// <param name="Ct">Blue-yellow chrominance (approximately -0.5 to +0.5).</param>
/// <param name="Cp">Green-magenta chrominance (approximately -0.5 to +0.5).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct ICtCpF(float I, float Ct, float Cp) : IColorSpace3F<ICtCpF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.I;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Ct;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Cp;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ICtCpF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <summary>Returns components normalized to 0.0-1.0 range.</summary>
  /// <remarks>I: 0-1 -> 0-1, Ct: -0.5 to 0.5 -> 0-1, Cp: -0.5 to 0.5 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float C1, float C2, float C3) ToNormalized() => (this.I, this.Ct + 0.5f, this.Cp + 0.5f);

  /// <summary>Returns components as bytes (0-255).</summary>
  /// <remarks>I: 0-1 -> 0-255, Ct: -0.5 to 0.5 -> 0-255, Cp: -0.5 to 0.5 -> 0-255.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.I * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Ct + 0.5f) * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Cp + 0.5f) * ColorConstants.FloatToByte + 0.5f)
  );
}
