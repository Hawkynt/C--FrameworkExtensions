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
/// Represents a color in Munsell (Hue, Value, Chroma) perceptual notation, backed by the full
/// ASTM D1535 / Newhall-Nickerson-Judd 1943 renotation table for colorimetric-grade conversion.
/// </summary>
/// <remarks>
/// <para>The Munsell colour system (A. H. Munsell, "A Color Notation", 1905) describes a
/// surface colour by three perceptual coordinates:
/// <list type="bullet">
///   <item><description><b>Hue</b> (H): one of ten primary hue families
///     R, YR, Y, GY, G, BG, B, PB, P, RP - each subdivided into 10 finer steps,
///     giving a 0-100 cyclic hue scale (encoded here as 0.0-1.0).</description></item>
///   <item><description><b>Value</b> (V): perceptual lightness on a 0 (black) to 10 (white)
///     scale, encoded here as 0.0-1.0 (V/10).</description></item>
///   <item><description><b>Chroma</b> (C): colorfulness, theoretically unbounded but
///     practically 0-30 for surface colours. Encoded here scaled by 1/30.</description></item>
/// </list>
/// </para>
/// <para><b>Implementation note - full renotation LUT.</b> Conversion to and from CIE XYZ uses
/// the official Newhall-Nickerson-Judd 1943 renotation data (4995 chips, including
/// extrapolated chromas) embedded as a resource and parsed lazily on first use. The forward
/// map (Munsell -&gt; xyY) is trilinear in (Hue, Value, Chroma); the inverse first inverts the
/// ASTM D1535-08 Y(V) polynomial then refines (Hue, Chroma) by 2D Newton in xy. Round-trip
/// accuracy is at table precision (sub-&#916;E2000 1 across natural-image content).</para>
/// <para>The renotation data is referenced to CIE illuminant C (1931 2&#176; observer); when
/// composed with the rest of this library's projectors (which assume D65) a Bradford
/// chromatic-adaptation transform is applied automatically.</para>
/// <para>Data source: <c>http://www.rit-mcsl.org/MunsellRenotation/all.dat</c> (public domain
/// US-government compilation, 1943).</para>
/// <para>H (hue): 0.0-1.0 (cyclic; 0 = 5R, 0.1 = 5YR, ..., 0.9 = 5RP).
/// V (value): 0.0-1.0 (V/10).
/// C (chroma): 0.0-1.0 (C/30 - typical ceiling for surface colours).</para>
/// </remarks>
/// <param name="H">Hue component (0.0-1.0, cyclic).</param>
/// <param name="V">Value component (0.0-1.0, perceptual lightness).</param>
/// <param name="C">Chroma component (0.0-1.0, normalized colorfulness).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct MunsellF(float H, float V, float C) : IColorSpace3F<MunsellF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.H;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.V;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.C;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static MunsellF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.H),
    UNorm32.FromFloat(this.V),
    UNorm32.FromFloat(this.C)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static MunsellF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat(),
    c3.ToFloat()
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.H * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.V * ColorConstants.FloatToByte + 0.5f),
    (byte)(this.C * ColorConstants.FloatToByte + 0.5f)
  );
}
