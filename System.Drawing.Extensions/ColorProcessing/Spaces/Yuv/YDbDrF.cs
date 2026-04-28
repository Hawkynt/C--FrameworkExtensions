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

namespace Hawkynt.ColorProcessing.Spaces.Yuv;

/// <summary>
/// Represents a color in SECAM YDbDr analogue-broadcast color space with float components.
/// </summary>
/// <remarks>
/// <para>YDbDr was the colour-encoding standard of SECAM analogue television
/// (France, USSR, parts of the Middle East and Africa). It encodes the same
/// information as YUV but with chroma scaled by ~3.06 and ~−2.17 (so Db ≈ 3.059·U,
/// Dr ≈ −2.169·V), aligning the chroma swing with SECAM's frequency-modulated
/// subcarrier (Db on one line, Dr on the next).</para>
/// <para>Distinct from <see cref="YuvF"/> (PAL/BT.601) and <see cref="YiqF"/>
/// (NTSC). The analogue family of three remains relevant for legacy media archives,
/// retro emulators, and broadcast historians.</para>
/// <para>Y (luma): 0.0-1.0.
/// Db: approximately -1.333 to +1.333.
/// Dr: approximately -1.333 to +1.333.</para>
/// </remarks>
/// <param name="Y">Luma component (0.0-1.0).</param>
/// <param name="Db">Blue chroma difference (approximately -1.333 to +1.333).</param>
/// <param name="Dr">Red chroma difference (approximately -1.333 to +1.333).</param>
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly record struct YDbDrF(float Y, float Db, float Dr) : IColorSpace3F<YDbDrF> {

  public float C1 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Y;
  }

  public float C2 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Db;
  }

  public float C3 {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Dr;
  }

  /// <summary>Creates a new instance from component values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YDbDrF Create(float c1, float c2, float c3) => new(c1, c2, c3);

  /// <inheritdoc />
  /// <remarks>Y: 0-1, Db/Dr: -1.333 to 1.333 -> 0-1.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized() => (
    UNorm32.FromFloat(this.Y),
    UNorm32.FromFloat((this.Db + 1.333f) / 2.666f),
    UNorm32.FromFloat((this.Dr + 1.333f) / 2.666f)
  );

  /// <summary>Creates from normalized values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static YDbDrF FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => new(
    c1.ToFloat(),
    c2.ToFloat() * 2.666f - 1.333f,
    c3.ToFloat() * 2.666f - 1.333f
  );

  /// <summary>Returns components as bytes (0-255).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (byte C1, byte C2, byte C3) ToBytes() => (
    (byte)(this.Y * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Db + 1.333f) / 2.666f * ColorConstants.FloatToByte + 0.5f),
    (byte)((this.Dr + 1.333f) / 2.666f * ColorConstants.FloatToByte + 0.5f)
  );
}
