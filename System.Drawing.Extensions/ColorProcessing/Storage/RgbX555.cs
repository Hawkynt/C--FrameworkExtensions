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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// 15-bit RGB pixel format (5-5-5) packed in 16 bits.
/// </summary>
/// <remarks>
/// Bit layout: [X:1][R:5][G:5][B:5] where X is unused.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 2)]
public readonly struct RgbX555 : IColorSpace3B<RgbX555> {

  private readonly ushort _packed;

  private const int RShift = 10;
  private const int GShift = 5;
  private const int BShift = 0;
  private const int Mask5 = 0x1F;

  #region IColorSpace3B Implementation

  byte IColorSpace3B<RgbX555>.C1 => this.R;
  byte IColorSpace3B<RgbX555>.C2 => this.G;
  byte IColorSpace3B<RgbX555>.C3 => this.B;
  public static RgbX555 Create(byte c1, byte c2, byte c3) => new(c1, c2, c3);

  #endregion

  /// <summary>Gets the packed 16-bit value.</summary>
  public ushort Packed {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._packed;
  }

  /// <summary>Gets the red component (0-31 scaled to 0-255).</summary>
  public byte R {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var r5 = (this._packed >> RShift) & Mask5;
      return (byte)((r5 << 3) | (r5 >> 2));
    }
  }

  /// <summary>Gets the green component (0-31 scaled to 0-255).</summary>
  public byte G {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var g5 = (this._packed >> GShift) & Mask5;
      return (byte)((g5 << 3) | (g5 >> 2));
    }
  }

  /// <summary>Gets the blue component (0-31 scaled to 0-255).</summary>
  public byte B {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var b5 = (this._packed >> BShift) & Mask5;
      return (byte)((b5 << 3) | (b5 >> 2));
    }
  }

  /// <summary>
  /// Constructs an Rgb15 from a packed 16-bit value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public RgbX555(ushort packed) => this._packed = packed;

  /// <summary>
  /// Constructs an Rgb15 from 8-bit components (truncated to 5 bits each).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public RgbX555(byte r, byte g, byte b) => this._packed = (ushort)(
    ((r >> 3) << RShift) |
    ((g >> 3) << GShift) |
    ((b >> 3) << BShift)
  );

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static RgbX555 Lerp(RgbX555 c1, RgbX555 c2) => new(
    (byte)((c1.R + c2.R) >> 1),
    (byte)((c1.G + c2.G) >> 1),
    (byte)((c1.B + c2.B) >> 1)
  );
}
