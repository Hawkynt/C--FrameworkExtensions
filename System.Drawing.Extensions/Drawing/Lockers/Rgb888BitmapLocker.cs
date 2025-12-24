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
using System.Runtime.Intrinsics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Provides span-based access to 24-bit RGB bitmap data.
/// </summary>
internal sealed class Rgb888BitmapLocker : TypedBitmapLockerBase<Bgr888> {
  private readonly int _strideBytes;

  // Note: stride may include padding bytes, so we return width*height pixels
  // Callers need to account for stride when accessing rows
  public override unsafe Span<Bgr888> Pixels {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new((void*)this._data.Scan0, this._strideBytes * this.Height / 3);
  }

  /// <inheritdoc/>
  public override unsafe Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var pointer = (byte*)this._data.Scan0 + y * this._strideBytes + x * 3;
      return Color.FromArgb(255, pointer[2], pointer[1], pointer[0]);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      var pointer = (byte*)this._data.Scan0 + y * this._strideBytes + x * 3;
      pointer[0] = value.B;
      pointer[1] = value.G;
      pointer[2] = value.R;
    }
  }

  public Rgb888BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public Rgb888BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, 3, PixelFormat.Format24bppRgb) {
    this._strideBytes = this._data.Stride;
  }

  public Rgb888BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, 3, PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb) {
    this._strideBytes = this._data.Stride;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Color PixelToColor(Bgr888 pixel) => Color.FromArgb(255, pixel.R, pixel.G, pixel.B);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Bgr888 ColorToPixel(Color color) => new(color.R, color.G, color.B);

  #region Vector-Optimized Drawing Methods

  // For 24-bit pixels, we create a 12-byte pattern (4 pixels) that can be written as 1 ulong + 1 uint
  // This is the LCM of 3 (bytes per pixel) and 4 (bytes per uint)
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void CreatePackedPattern(byte b, byte g, byte r, out ulong ulong0, out uint uint0) {
    // Memory layout (little-endian): B G R B G R B G | R B G R
    ulong0 = (ulong)b | ((ulong)g << 8) | ((ulong)r << 16) | ((ulong)b << 24) |
             ((ulong)g << 32) | ((ulong)r << 40) | ((ulong)b << 48) | ((ulong)g << 56);
    uint0 = (uint)(r | (b << 8) | (g << 16) | (r << 24));
  }

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var b = color.B;
    var g = color.G;
    var r = color.R;
    CreatePackedPattern(b, g, r, out var ulong0, out var uint0);
    var width = this.Width;
    var strideBytes = this._strideBytes;

    var basePtr = (byte*)this._data.Scan0;

    // Process each row independently to handle stride padding correctly
    for (var row = 0; row < this.Height; ++row) {
      var bytePtr = basePtr + row * strideBytes;
      var count = width;

      var count4 = count / 4; // Number of 4-pixel groups (12 bytes each)

      // 4x unrolled: 16 pixels (48 bytes) per iteration, using ulong+uint writes
      var end16 = count4 & ~3;
      for (var i = 0; i < end16; i += 4) {
        *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0;
        *(ulong*)(bytePtr + 12) = ulong0; *(uint*)(bytePtr + 20) = uint0;
        *(ulong*)(bytePtr + 24) = ulong0; *(uint*)(bytePtr + 32) = uint0;
        *(ulong*)(bytePtr + 36) = ulong0; *(uint*)(bytePtr + 44) = uint0;
        bytePtr += 48;
      }

      // Handle remaining 0-3 groups of 4 pixels
      switch (count4 & 3) {
        case 3: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; goto case 2;
        case 2: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; goto case 1;
        case 1: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; break;
      }

      // Handle remaining 0-3 pixels (0-9 bytes)
      switch (count & 3) {
        case 3: bytePtr[6] = b; bytePtr[7] = g; bytePtr[8] = r; goto case 2;
        case 2: bytePtr[3] = b; bytePtr[4] = g; bytePtr[5] = r; goto case 1;
        case 1: bytePtr[0] = b; bytePtr[1] = g; bytePtr[2] = r; break;
      }
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    var b = color.B;
    var g = color.G;
    var r = color.R;
    CreatePackedPattern(b, g, r, out var ulong0, out var uint0);
    var count = length;

    var bytePtr = (byte*)this._data.Scan0 + y * this._strideBytes + x * 3;

    // Handle unaligned start pixels
    while (count > 0 && ((nint)bytePtr & 7) != 0) {
      bytePtr[0] = b; bytePtr[1] = g; bytePtr[2] = r;
      bytePtr += 3;
      --count;
    }

    var count4 = count / 4; // Groups of 4 pixels

    // 4x unrolled using ulong+uint writes
    var end16 = count4 & ~3;
    for (var i = 0; i < end16; i += 4) {
      *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0;
      *(ulong*)(bytePtr + 12) = ulong0; *(uint*)(bytePtr + 20) = uint0;
      *(ulong*)(bytePtr + 24) = ulong0; *(uint*)(bytePtr + 32) = uint0;
      *(ulong*)(bytePtr + 36) = ulong0; *(uint*)(bytePtr + 44) = uint0;
      bytePtr += 48;
    }

    switch (count4 & 3) {
      case 3: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; goto case 2;
      case 2: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; goto case 1;
      case 1: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; break;
    }

    // Handle remaining 0-3 pixels
    switch (count & 3) {
      case 3: bytePtr[6] = b; bytePtr[7] = g; bytePtr[8] = r; goto case 2;
      case 2: bytePtr[3] = b; bytePtr[4] = g; bytePtr[5] = r; goto case 1;
      case 1: bytePtr[0] = b; bytePtr[1] = g; bytePtr[2] = r; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var b = color.B;
    var g = color.G;
    var r = color.R;
    var strideBytes = this._strideBytes;
    var count = length;

    var ptr = (byte*)this._data.Scan0 + y * strideBytes + x * 3;

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
      ptr[0] = b; ptr[1] = g; ptr[2] = r;
      ptr[strideBytes] = b; ptr[strideBytes + 1] = g; ptr[strideBytes + 2] = r;
      ptr[stride2] = b; ptr[stride2 + 1] = g; ptr[stride2 + 2] = r;
      ptr[stride3] = b; ptr[stride3 + 1] = g; ptr[stride3 + 2] = r;
      ptr[stride4] = b; ptr[stride4 + 1] = g; ptr[stride4 + 2] = r;
      ptr[stride5] = b; ptr[stride5 + 1] = g; ptr[stride5 + 2] = r;
      ptr[stride6] = b; ptr[stride6 + 1] = g; ptr[stride6 + 2] = r;
      ptr[stride7] = b; ptr[stride7 + 1] = g; ptr[stride7 + 2] = r;
      ptr += stride8;
    }

    switch (count & 7) {
      case 7: ptr[stride6] = b; ptr[stride6 + 1] = g; ptr[stride6 + 2] = r; goto case 6;
      case 6: ptr[stride5] = b; ptr[stride5 + 1] = g; ptr[stride5 + 2] = r; goto case 5;
      case 5: ptr[stride4] = b; ptr[stride4 + 1] = g; ptr[stride4 + 2] = r; goto case 4;
      case 4: ptr[stride3] = b; ptr[stride3 + 1] = g; ptr[stride3 + 2] = r; goto case 3;
      case 3: ptr[stride2] = b; ptr[stride2 + 1] = g; ptr[stride2 + 2] = r; goto case 2;
      case 2: ptr[strideBytes] = b; ptr[strideBytes + 1] = g; ptr[strideBytes + 2] = r; goto case 1;
      case 1: ptr[0] = b; ptr[1] = g; ptr[2] = r; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    var b = color.B;
    var g = color.G;
    var r = color.R;
    CreatePackedPattern(b, g, r, out var ulong0, out var uint0);
    var strideBytes = this._strideBytes;
    var endY = y + height;

    var basePtr = (byte*)this._data.Scan0;

    for (var row = y; row < endY; ++row) {
      var bytePtr = basePtr + row * strideBytes + x * 3;
      var count = width;

      // Handle unaligned start pixels
      while (count > 0 && ((nint)bytePtr & 7) != 0) {
        bytePtr[0] = b; bytePtr[1] = g; bytePtr[2] = r;
        bytePtr += 3;
        --count;
      }

      var count4 = count / 4;

      // 4x unrolled using ulong+uint writes
      var end16 = count4 & ~3;
      for (var i = 0; i < end16; i += 4) {
        *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0;
        *(ulong*)(bytePtr + 12) = ulong0; *(uint*)(bytePtr + 20) = uint0;
        *(ulong*)(bytePtr + 24) = ulong0; *(uint*)(bytePtr + 32) = uint0;
        *(ulong*)(bytePtr + 36) = ulong0; *(uint*)(bytePtr + 44) = uint0;
        bytePtr += 48;
      }

      switch (count4 & 3) {
        case 3: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; goto case 2;
        case 2: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; goto case 1;
        case 1: *(ulong*)bytePtr = ulong0; *(uint*)(bytePtr + 8) = uint0; bytePtr += 12; break;
      }

      switch (count & 3) {
        case 3: bytePtr[6] = b; bytePtr[7] = g; bytePtr[8] = r; goto case 2;
        case 2: bytePtr[3] = b; bytePtr[4] = g; bytePtr[5] = r; goto case 1;
        case 1: bytePtr[0] = b; bytePtr[1] = g; bytePtr[2] = r; break;
      }
    }
  }

  #endregion
}
