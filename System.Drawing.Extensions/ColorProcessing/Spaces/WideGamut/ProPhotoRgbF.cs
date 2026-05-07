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

namespace Hawkynt.ColorProcessing.Spaces.WideGamut;

/// <summary>
/// Represents a color in ProPhoto RGB (Reference Output Medium Metric, ROMM RGB) color space.
/// </summary>
/// <remarks>
/// <para>ProPhoto RGB is an ultra-wide-gamut RGB working space developed by Kodak for
/// professional photography workflows. Encompasses ~90% of the CIE 1931 visible gamut
/// (vs ~35% for sRGB) at the cost of having ~13% of imaginary primaries outside the
/// horseshoe. Standardised as ISO 22028-2:2013 ROMM RGB. D50 reference white;
/// gamma 1.8 transfer function with linear toe.</para>
/// <para>Reference: ISO 22028-2:2013 "Photography and graphic technology – Extended
/// colour encodings for digital image storage, manipulation and interchange –
/// Part 2: Reference output medium metric RGB colour image encoding (ROMM RGB)".
/// Primaries: R=(0.7347, 0.2653), G=(0.1596, 0.8404), B=(0.0366, 0.0001).</para>
/// <para>All components linear, 0.0-1.0.</para>
/// </remarks>
/// <param name="R">Red component (0.0-1.0, linear).</param>
/// <param name="G">Green component (0.0-1.0, linear).</param>
/// <param name="B">Blue component (0.0-1.0, linear).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct ProPhotoRgbF(float R, float G, float B) : IColorSpace3F<ProPhotoRgbF> {

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
  public static ProPhotoRgbF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.R),
    UNorm32.FromFloat(this.G),
    UNorm32.FromFloat(this.B)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ProPhotoRgbF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.R * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.G * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.B * ColorConstants.FloatToByte + 0.5f)
  );
}
