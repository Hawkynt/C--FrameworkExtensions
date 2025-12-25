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

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// 16-bit grayscale pixel format.
/// </summary>
/// <remarks>
/// Single 16-bit luminance value (0-65535).
/// </remarks>
[StructLayout(LayoutKind.Sequential, Size = 2)]
public readonly struct Gray16 : IStorageSpace {

  /// <summary>Reciprocal of 65535 for fast 16-bit-to-normalized-float conversion.</summary>
  public const float ValueToNormalized = 1f / 65535f;

  /// <summary>Multiplier for normalized-float-to-16-bit conversion.</summary>
  public const float NormalizedToValue = 65535f;

  /// <summary>The luminance value (0-65535).</summary>
  public readonly ushort Value;

  /// <summary>
  /// Constructs a Gray16 from a 16-bit luminance value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Gray16(ushort value) => this.Value = value;

  /// <summary>
  /// Constructs a Gray16 from an 8-bit luminance value (scaled to 16-bit).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Gray16(byte value) => this.Value = (ushort)((value << 8) | value);

  /// <summary>
  /// Constructs a Gray16 from RGB components using standard luminance weights.
  /// </summary>
  /// <remarks>
  /// Uses BT.601 coefficients: Y = 0.299*R + 0.587*G + 0.114*B
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Gray16(byte r, byte g, byte b) {
    var y8 = (byte)((r * 77 + g * 150 + b * 29) >> 8);
    this.Value = (ushort)((y8 << 8) | y8);
  }

  /// <summary>Gets the luminance normalized to 0.0-1.0 range.</summary>
  public float Normalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Value * ValueToNormalized;
  }

  /// <summary>Gets the luminance as an 8-bit value (0-255).</summary>
  public byte ToByte {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (byte)(this.Value >> 8);
  }

  /// <summary>
  /// Linearly interpolates between two grayscale values (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray16 Lerp(Gray16 c1, Gray16 c2)
    => new((ushort)((c1.Value + c2.Value) >> 1));

  /// <summary>
  /// Linearly interpolates between two grayscale values using a normalized factor (0.0-1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Gray16 Lerp(Gray16 c1, Gray16 c2, float factor) {
    var invFactor = 1f - factor;
    return new((ushort)(c1.Value * invFactor + c2.Value * factor + 0.5f));
  }
}
