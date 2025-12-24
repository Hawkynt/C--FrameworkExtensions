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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Provides span-based access to 48-bit RGB bitmap data (16 bits per channel).
/// </summary>
/// <remarks>
/// Memory layout: [R:16][G:16][B:16] in native endianness.
/// Used for high dynamic range imaging without alpha.
/// 48-bit pixels have awkward 6-byte alignment, so SIMD is limited.
/// </remarks>
internal sealed class Rgb161616BitmapLocker : TypedBitmapLockerBase<Rgb48> {
  private readonly int _strideBytes;

  public override unsafe Span<Rgb48> Pixels {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new((void*)this._data.Scan0, this._strideBytes * this.Height / 6);
  }

  /// <inheritdoc/>
  public override unsafe Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var pointer = (ushort*)((byte*)this._data.Scan0 + y * this._strideBytes + x * 6);
      return Color.FromArgb(255, (byte)(pointer[0] >> 8), (byte)(pointer[1] >> 8), (byte)(pointer[2] >> 8));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      var pointer = (ushort*)((byte*)this._data.Scan0 + y * this._strideBytes + x * 6);
      pointer[0] = (ushort)((value.R << 8) | value.R);
      pointer[1] = (ushort)((value.G << 8) | value.G);
      pointer[2] = (ushort)((value.B << 8) | value.B);
    }
  }

  public Rgb161616BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public Rgb161616BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, 6, PixelFormat.Format48bppRgb) {
    this._strideBytes = this._data.Stride;
  }

  public Rgb161616BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, 6, PixelFormat.Format48bppRgb, PixelFormat.Format48bppRgb) {
    this._strideBytes = this._data.Stride;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Color PixelToColor(Rgb48 pixel) => Color.FromArgb(255, pixel.R, pixel.G, pixel.B);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Rgb48 ColorToPixel(Color color) => new(color.R, color.G, color.B);

  #region Vector-Optimized Drawing Methods

  // 48-bit pixels (6 bytes each) have awkward alignment.
  // LCM of 6 and 8 is 24, so we write 4 pixels (24 bytes) as 3 ulong writes.
  // Pattern: [R0G0][B0R1][G1B1][R2G2][B2R3][G3B3] = 6 ushorts per 2 pixels, or 3 ulongs per 4 pixels.
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void Create4PixelPattern(ushort r, ushort g, ushort b, out ulong ul0, out ulong ul1, out ulong ul2) {
    // 4 pixels = 24 bytes = 3 ulongs
    // Layout per pixel: R16 G16 B16
    ul0 = (ulong)r | ((ulong)g << 16) | ((ulong)b << 32) | ((ulong)r << 48);
    ul1 = (ulong)g | ((ulong)b << 16) | ((ulong)r << 32) | ((ulong)g << 48);
    ul2 = (ulong)b | ((ulong)r << 16) | ((ulong)g << 32) | ((ulong)b << 48);
  }

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var r = (ushort)((color.R << 8) | color.R);
    var g = (ushort)((color.G << 8) | color.G);
    var b = (ushort)((color.B << 8) | color.B);
    Create4PixelPattern(r, g, b, out var ul0, out var ul1, out var ul2);
    var width = this.Width;
    var strideBytes = this._strideBytes;

    var basePtr = (byte*)this._data.Scan0;

    for (var row = 0; row < this.Height; ++row) {
      var bytePtr = basePtr + row * strideBytes;
      var count = width;

      // 4x unrolled: 16 pixels (96 bytes) per iteration
      var count4 = count / 4;
      var end16 = count4 & ~3;
      for (var i = 0; i < end16; i += 4) {
        var ulPtr = (ulong*)bytePtr;
        ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2;
        ulPtr[3] = ul0; ulPtr[4] = ul1; ulPtr[5] = ul2;
        ulPtr[6] = ul0; ulPtr[7] = ul1; ulPtr[8] = ul2;
        ulPtr[9] = ul0; ulPtr[10] = ul1; ulPtr[11] = ul2;
        bytePtr += 96;
      }

      // Handle remaining 0-3 groups of 4 pixels
      switch (count4 & 3) {
        case 3: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; goto case 2; }
        case 2: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; goto case 1; }
        case 1: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; break; }
      }

      // Handle remaining 0-3 pixels (0-18 bytes)
      var usPtr = (ushort*)bytePtr;
      switch (count & 3) {
        case 3: usPtr[6] = r; usPtr[7] = g; usPtr[8] = b; goto case 2;
        case 2: usPtr[3] = r; usPtr[4] = g; usPtr[5] = b; goto case 1;
        case 1: usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; break;
      }
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    var r = (ushort)((color.R << 8) | color.R);
    var g = (ushort)((color.G << 8) | color.G);
    var b = (ushort)((color.B << 8) | color.B);
    Create4PixelPattern(r, g, b, out var ul0, out var ul1, out var ul2);
    var count = length;

    var bytePtr = (byte*)this._data.Scan0 + y * this._strideBytes + x * 6;

    // Align to 8-byte boundary first
    while (count > 0 && ((nint)bytePtr & 7) != 0) {
      var usPtr = (ushort*)bytePtr;
      usPtr[0] = r; usPtr[1] = g; usPtr[2] = b;
      bytePtr += 6;
      --count;
    }

    var count4 = count / 4;

    // 4x unrolled: 16 pixels per iteration
    var end16 = count4 & ~3;
    for (var i = 0; i < end16; i += 4) {
      var ulPtr = (ulong*)bytePtr;
      ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2;
      ulPtr[3] = ul0; ulPtr[4] = ul1; ulPtr[5] = ul2;
      ulPtr[6] = ul0; ulPtr[7] = ul1; ulPtr[8] = ul2;
      ulPtr[9] = ul0; ulPtr[10] = ul1; ulPtr[11] = ul2;
      bytePtr += 96;
    }

    switch (count4 & 3) {
      case 3: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; goto case 2; }
      case 2: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; goto case 1; }
      case 1: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; break; }
    }

    var usP = (ushort*)bytePtr;
    switch (count & 3) {
      case 3: usP[6] = r; usP[7] = g; usP[8] = b; goto case 2;
      case 2: usP[3] = r; usP[4] = g; usP[5] = b; goto case 1;
      case 1: usP[0] = r; usP[1] = g; usP[2] = b; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var r = (ushort)((color.R << 8) | color.R);
    var g = (ushort)((color.G << 8) | color.G);
    var b = (ushort)((color.B << 8) | color.B);
    var strideBytes = this._strideBytes;
    var count = length;

    var ptr = (byte*)this._data.Scan0 + y * strideBytes + x * 6;

    // Precompute stride offsets to enable CPU parallelization
    var stride2 = strideBytes * 2;
    var stride3 = strideBytes * 3;
    var stride4 = strideBytes * 4;
    var stride5 = strideBytes * 5;
    var stride6 = strideBytes * 6;
    var stride7 = strideBytes * 7;
    var stride8 = strideBytes * 8;

    // 8x unrolled with parallel stores
    var end8 = count & ~7;
    for (var i = 0; i < end8; i += 8) {
      var usPtr0 = (ushort*)ptr;
      var usPtr1 = (ushort*)(ptr + strideBytes);
      var usPtr2 = (ushort*)(ptr + stride2);
      var usPtr3 = (ushort*)(ptr + stride3);
      var usPtr4 = (ushort*)(ptr + stride4);
      var usPtr5 = (ushort*)(ptr + stride5);
      var usPtr6 = (ushort*)(ptr + stride6);
      var usPtr7 = (ushort*)(ptr + stride7);
      usPtr0[0] = r; usPtr0[1] = g; usPtr0[2] = b;
      usPtr1[0] = r; usPtr1[1] = g; usPtr1[2] = b;
      usPtr2[0] = r; usPtr2[1] = g; usPtr2[2] = b;
      usPtr3[0] = r; usPtr3[1] = g; usPtr3[2] = b;
      usPtr4[0] = r; usPtr4[1] = g; usPtr4[2] = b;
      usPtr5[0] = r; usPtr5[1] = g; usPtr5[2] = b;
      usPtr6[0] = r; usPtr6[1] = g; usPtr6[2] = b;
      usPtr7[0] = r; usPtr7[1] = g; usPtr7[2] = b;
      ptr += stride8;
    }

    switch (count & 7) {
      case 7: { var usPtr = (ushort*)(ptr + stride6); usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; goto case 6; }
      case 6: { var usPtr = (ushort*)(ptr + stride5); usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; goto case 5; }
      case 5: { var usPtr = (ushort*)(ptr + stride4); usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; goto case 4; }
      case 4: { var usPtr = (ushort*)(ptr + stride3); usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; goto case 3; }
      case 3: { var usPtr = (ushort*)(ptr + stride2); usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; goto case 2; }
      case 2: { var usPtr = (ushort*)(ptr + strideBytes); usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; goto case 1; }
      case 1: { var usPtr = (ushort*)ptr; usPtr[0] = r; usPtr[1] = g; usPtr[2] = b; break; }
    }
  }

  /// <inheritdoc/>
  public override unsafe void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    var r = (ushort)((color.R << 8) | color.R);
    var g = (ushort)((color.G << 8) | color.G);
    var b = (ushort)((color.B << 8) | color.B);
    Create4PixelPattern(r, g, b, out var ul0, out var ul1, out var ul2);
    var strideBytes = this._strideBytes;
    var endY = y + height;

    var basePtr = (byte*)this._data.Scan0;

    for (var row = y; row < endY; ++row) {
      var bytePtr = basePtr + row * strideBytes + x * 6;
      var count = width;

      // Align to 8-byte boundary first
      while (count > 0 && ((nint)bytePtr & 7) != 0) {
        var usPtr = (ushort*)bytePtr;
        usPtr[0] = r; usPtr[1] = g; usPtr[2] = b;
        bytePtr += 6;
        --count;
      }

      var count4 = count / 4;

      // 4x unrolled: 16 pixels per iteration
      var end16 = count4 & ~3;
      for (var i = 0; i < end16; i += 4) {
        var ulPtr = (ulong*)bytePtr;
        ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2;
        ulPtr[3] = ul0; ulPtr[4] = ul1; ulPtr[5] = ul2;
        ulPtr[6] = ul0; ulPtr[7] = ul1; ulPtr[8] = ul2;
        ulPtr[9] = ul0; ulPtr[10] = ul1; ulPtr[11] = ul2;
        bytePtr += 96;
      }

      switch (count4 & 3) {
        case 3: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; goto case 2; }
        case 2: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; goto case 1; }
        case 1: { var ulPtr = (ulong*)bytePtr; ulPtr[0] = ul0; ulPtr[1] = ul1; ulPtr[2] = ul2; bytePtr += 24; break; }
      }

      var usP = (ushort*)bytePtr;
      switch (count & 3) {
        case 3: usP[6] = r; usP[7] = g; usP[8] = b; goto case 2;
        case 2: usP[3] = r; usP[4] = g; usP[5] = b; goto case 1;
        case 1: usP[0] = r; usP[1] = g; usP[2] = b; break;
      }
    }
  }

  #endregion
}
