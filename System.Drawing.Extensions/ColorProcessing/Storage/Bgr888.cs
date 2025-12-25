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
/// 24-bit RGB pixel format with 8 bits per channel.
/// </summary>
/// <remarks>
/// Memory layout: [B, G, R] (BGR order for compatibility with GDI+).
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
public readonly struct Bgr888 : IColorSpace3B<Bgr888>, IStorageSpace {

  /// <summary>Reciprocal of 255 for fast byte-to-normalized-float conversion.</summary>
  public const float ByteToNormalized = ColorConstants.ByteToFloat;

  public readonly byte B;
  public readonly byte G;
  public readonly byte R;

  #region IColorSpace3B Implementation

  byte IColorSpace3B<Bgr888>.C1 => this.R;
  byte IColorSpace3B<Bgr888>.C2 => this.G;
  byte IColorSpace3B<Bgr888>.C3 => this.B;
  public static Bgr888 Create(byte c1, byte c2, byte c3) => new(c1, c2, c3);

  #endregion

  /// <summary>
  /// Constructs an Rgb24 from individual byte components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Bgr888(byte r, byte g, byte b) {
    this.B = b;
    this.G = g;
    this.R = r;
  }

  /// <summary>Gets the red component normalized to 0.0-1.0 range.</summary>
  public float RNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.R * ByteToNormalized;
  }

  /// <summary>Gets the green component normalized to 0.0-1.0 range.</summary>
  public float GNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.G * ByteToNormalized;
  }

  /// <summary>Gets the blue component normalized to 0.0-1.0 range.</summary>
  public float BNormalized {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.B * ByteToNormalized;
  }

  /// <summary>
  /// Linearly interpolates between two colors (50/50 blend).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgr888 Lerp(Bgr888 c1, Bgr888 c2) => new(
    (byte)((c1.R + c2.R) >> 1),
    (byte)((c1.G + c2.G) >> 1),
    (byte)((c1.B + c2.B) >> 1)
  );

  /// <summary>
  /// Linearly interpolates between two colors using a normalized factor (0.0-1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Bgr888 Lerp(Bgr888 c1, Bgr888 c2, float factor) {
    var invFactor = 1f - factor;
    return new(
      (byte)(c1.R * invFactor + c2.R * factor + 0.5f),
      (byte)(c1.G * invFactor + c2.G * factor + 0.5f),
      (byte)(c1.B * invFactor + c2.B * factor + 0.5f)
    );
  }
}
