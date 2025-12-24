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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Provides access to 4-bit indexed (16-color palette) bitmap data.
/// </summary>
internal sealed class Indexed4BitmapLocker : IndexedBitmapLockerBase {

  /// <inheritdoc/>
  public override unsafe Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var ptr = (byte*)this._data.Scan0 + y * this.Stride + (x >> 1);
      var highNibble = (x & 1) == 0;
      var index = highNibble ? *ptr >> 4 : *ptr & 0x0F;
      return this._palette[index];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      var colorIndex = this.FindColorIndex(value);
      var ptr = (byte*)this._data.Scan0 + y * this.Stride + (x >> 1);
      *ptr = (x & 1) == 0
        ? (byte)((*ptr & 0x0F) | (colorIndex << 4))
        : (byte)((*ptr & 0xF0) | (colorIndex & 0x0F));
    }
  }

  public Indexed4BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, PixelFormat.Format4bppIndexed) { }

  public Indexed4BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public Indexed4BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, PixelFormat.Format4bppIndexed, PixelFormat.Format4bppIndexed) { }

  #region Vector-Optimized Drawing Methods

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);
    var packedByte = (byte)((colorIndex << 4) | colorIndex); // Both nibbles same

    var ptr = (byte*)this._data.Scan0;
    var count = this.Stride * this.Height;

    // Vector512: 64 bytes per store, 4x unrolled = 256 bytes = 512 pixels
    if (Vector512.IsHardwareAccelerated && count >= 64) {
      var vec = Vector512.Create(packedByte);
      var end256 = ptr + (count & ~255);
      while (ptr < end256) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 64);
        Vector512.Store(vec, ptr + 128);
        Vector512.Store(vec, ptr + 192);
        ptr += 256;
      }
      switch ((count >> 6) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 64; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 64; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 64; break;
      }
      count &= 63;
    }

    // Vector256: 32 bytes per store, 4x unrolled = 128 bytes
    if (Vector256.IsHardwareAccelerated && count >= 32) {
      var vec = Vector256.Create(packedByte);
      var end128 = ptr + (count & ~127);
      while (ptr < end128) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 32);
        Vector256.Store(vec, ptr + 64);
        Vector256.Store(vec, ptr + 96);
        ptr += 128;
      }
      switch ((count >> 5) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 32; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 32; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 32; break;
      }
      count &= 31;
    }

    // Vector128: 16 bytes per store, 4x unrolled = 64 bytes
    if (Vector128.IsHardwareAccelerated && count >= 16) {
      var vec = Vector128.Create(packedByte);
      var end64 = ptr + (count & ~63);
      while (ptr < end64) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 16);
        Vector128.Store(vec, ptr + 32);
        Vector128.Store(vec, ptr + 48);
        ptr += 64;
      }
      switch ((count >> 4) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 16; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 16; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 16; break;
      }
      count &= 15;
    }

    // 64-bit cleanup: count is 0-15, ulong holds 8 bytes
    var packed64 = 0x0101010101010101UL * packedByte;
    if (count >= 8) {
      *(ulong*)ptr = packed64;
      ptr += 8;
      count -= 8;
    }

    switch (count) {
      case 7: ptr[6] = packedByte; goto case 6;
      case 6: ptr[5] = packedByte; goto case 5;
      case 5: ptr[4] = packedByte; goto case 4;
      case 4: ptr[3] = packedByte; goto case 3;
      case 3: ptr[2] = packedByte; goto case 2;
      case 2: ptr[1] = packedByte; goto case 1;
      case 1: ptr[0] = packedByte; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);
    var packedByte = (byte)((colorIndex << 4) | colorIndex);
    var count = length;

    var ptr = (byte*)this._data.Scan0 + y * this.Stride + (x >> 1);

    // Handle odd start (low nibble only)
    if ((x & 1) != 0) {
      *ptr = (byte)((*ptr & 0xF0) | colorIndex);
      ++ptr;
      --count;
    }

    // Main loop: process pairs of pixels (full bytes)
    var byteCount = count >> 1;

    if (Vector512.IsHardwareAccelerated && byteCount >= 64) {
      var vec = Vector512.Create(packedByte);
      var end256 = ptr + (byteCount & ~255);
      while (ptr < end256) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 64);
        Vector512.Store(vec, ptr + 128);
        Vector512.Store(vec, ptr + 192);
        ptr += 256;
      }
      switch ((byteCount >> 6) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 64; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 64; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 64; break;
      }
      byteCount &= 63;
    }

    if (Vector256.IsHardwareAccelerated && byteCount >= 32) {
      var vec = Vector256.Create(packedByte);
      var end128 = ptr + (byteCount & ~127);
      while (ptr < end128) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 32);
        Vector256.Store(vec, ptr + 64);
        Vector256.Store(vec, ptr + 96);
        ptr += 128;
      }
      switch ((byteCount >> 5) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 32; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 32; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 32; break;
      }
      byteCount &= 31;
    }

    if (Vector128.IsHardwareAccelerated && byteCount >= 16) {
      var vec = Vector128.Create(packedByte);
      var end64 = ptr + (byteCount & ~63);
      while (ptr < end64) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 16);
        Vector128.Store(vec, ptr + 32);
        Vector128.Store(vec, ptr + 48);
        ptr += 64;
      }
      switch ((byteCount >> 4) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 16; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 16; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 16; break;
      }
      byteCount &= 15;
    }

    // 64-bit cleanup: byteCount is 0-15, ulong holds 8 bytes
    var packed64 = 0x0101010101010101UL * packedByte;
    if (byteCount >= 8) {
      *(ulong*)ptr = packed64;
      ptr += 8;
      byteCount -= 8;
    }

    switch (byteCount) {
      case 7: ptr[6] = packedByte; goto case 6;
      case 6: ptr[5] = packedByte; goto case 5;
      case 5: ptr[4] = packedByte; goto case 4;
      case 4: ptr[3] = packedByte; goto case 3;
      case 3: ptr[2] = packedByte; goto case 2;
      case 2: ptr[1] = packedByte; goto case 1;
      case 1: ptr[0] = packedByte; ++ptr; break;
    }

    // Handle odd end (high nibble only)
    if ((count & 1) != 0)
      *ptr = (byte)((*ptr & 0x0F) | (colorIndex << 4));
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);
    var isHighNibble = (x & 1) == 0;
    var mask = isHighNibble ? (byte)0x0F : (byte)0xF0;
    var value = isHighNibble ? (byte)(colorIndex << 4) : colorIndex;
    var stride = this.Stride;
    var byteX = x >> 1;
    var count = length;

    var ptr = (byte*)this._data.Scan0 + y * stride + byteX;

    // Precompute stride offsets
    var stride2 = stride * 2;
    var stride3 = stride * 3;
    var stride4 = stride * 4;
    var stride5 = stride * 5;
    var stride6 = stride * 6;
    var stride7 = stride * 7;
    var stride8 = stride * 8;

    // 8x unrolled with parallel read-modify-write operations
    var end8 = count & ~7;
    for (var i = 0; i < end8; i += 8) {
      ptr[0] = (byte)((ptr[0] & mask) | value);
      ptr[stride] = (byte)((ptr[stride] & mask) | value);
      ptr[stride2] = (byte)((ptr[stride2] & mask) | value);
      ptr[stride3] = (byte)((ptr[stride3] & mask) | value);
      ptr[stride4] = (byte)((ptr[stride4] & mask) | value);
      ptr[stride5] = (byte)((ptr[stride5] & mask) | value);
      ptr[stride6] = (byte)((ptr[stride6] & mask) | value);
      ptr[stride7] = (byte)((ptr[stride7] & mask) | value);
      ptr += stride8;
    }

    switch (count & 7) {
      case 7: ptr[stride6] = (byte)((ptr[stride6] & mask) | value); goto case 6;
      case 6: ptr[stride5] = (byte)((ptr[stride5] & mask) | value); goto case 5;
      case 5: ptr[stride4] = (byte)((ptr[stride4] & mask) | value); goto case 4;
      case 4: ptr[stride3] = (byte)((ptr[stride3] & mask) | value); goto case 3;
      case 3: ptr[stride2] = (byte)((ptr[stride2] & mask) | value); goto case 2;
      case 2: ptr[stride] = (byte)((ptr[stride] & mask) | value); goto case 1;
      case 1: ptr[0] = (byte)((ptr[0] & mask) | value); break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);
    var packedByte = (byte)((colorIndex << 4) | colorIndex);
    var stride = this.Stride;
    var startByteX = x >> 1;
    var hasOddStart = (x & 1) != 0;
    var hasOddEnd = ((x + width) & 1) != 0;
    var endY = y + height;

    var basePtr = (byte*)this._data.Scan0;

    for (var row = y; row < endY; ++row) {
      var ptr = basePtr + row * stride + startByteX;
      var count = width;

      // Handle odd start
      if (hasOddStart) {
        *ptr = (byte)((*ptr & 0xF0) | colorIndex);
        ++ptr;
        --count;
      }

      // Main loop: full bytes
      var byteCount = count >> 1;

      if (Vector512.IsHardwareAccelerated && byteCount >= 64) {
        var vec = Vector512.Create(packedByte);
        var end256 = ptr + (byteCount & ~255);
        while (ptr < end256) {
          Vector512.Store(vec, ptr);
          Vector512.Store(vec, ptr + 64);
          Vector512.Store(vec, ptr + 128);
          Vector512.Store(vec, ptr + 192);
          ptr += 256;
        }
        switch ((byteCount >> 6) & 3) {
          case 3: Vector512.Store(vec, ptr); ptr += 64; goto case 2;
          case 2: Vector512.Store(vec, ptr); ptr += 64; goto case 1;
          case 1: Vector512.Store(vec, ptr); ptr += 64; break;
        }
        byteCount &= 63;
      }

      if (Vector256.IsHardwareAccelerated && byteCount >= 32) {
        var vec = Vector256.Create(packedByte);
        var end128 = ptr + (byteCount & ~127);
        while (ptr < end128) {
          Vector256.Store(vec, ptr);
          Vector256.Store(vec, ptr + 32);
          Vector256.Store(vec, ptr + 64);
          Vector256.Store(vec, ptr + 96);
          ptr += 128;
        }
        switch ((byteCount >> 5) & 3) {
          case 3: Vector256.Store(vec, ptr); ptr += 32; goto case 2;
          case 2: Vector256.Store(vec, ptr); ptr += 32; goto case 1;
          case 1: Vector256.Store(vec, ptr); ptr += 32; break;
        }
        byteCount &= 31;
      }

      if (Vector128.IsHardwareAccelerated && byteCount >= 16) {
        var vec = Vector128.Create(packedByte);
        var end64 = ptr + (byteCount & ~63);
        while (ptr < end64) {
          Vector128.Store(vec, ptr);
          Vector128.Store(vec, ptr + 16);
          Vector128.Store(vec, ptr + 32);
          Vector128.Store(vec, ptr + 48);
          ptr += 64;
        }
        switch ((byteCount >> 4) & 3) {
          case 3: Vector128.Store(vec, ptr); ptr += 16; goto case 2;
          case 2: Vector128.Store(vec, ptr); ptr += 16; goto case 1;
          case 1: Vector128.Store(vec, ptr); ptr += 16; break;
        }
        byteCount &= 15;
      }

      // 64-bit cleanup: byteCount is 0-15, ulong holds 8 bytes
      var packed64 = 0x0101010101010101UL * packedByte;
      if (byteCount >= 8) {
        *(ulong*)ptr = packed64;
        ptr += 8;
        byteCount -= 8;
      }

      switch (byteCount) {
        case 7: ptr[6] = packedByte; goto case 6;
        case 6: ptr[5] = packedByte; goto case 5;
        case 5: ptr[4] = packedByte; goto case 4;
        case 4: ptr[3] = packedByte; goto case 3;
        case 3: ptr[2] = packedByte; goto case 2;
        case 2: ptr[1] = packedByte; goto case 1;
        case 1: ptr[0] = packedByte; ++ptr; break;
      }

      // Handle odd end
      if (hasOddEnd)
        *ptr = (byte)((*ptr & 0x0F) | (colorIndex << 4));
    }
  }

  #endregion
}
