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
/// Provides access to 1-bit indexed (2-color palette) bitmap data.
/// </summary>
internal sealed class Indexed1BitmapLocker : IndexedBitmapLockerBase {

  /// <inheritdoc/>
  public override unsafe Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var ptr = (byte*)this._data.Scan0 + y * this.Stride + (x >> 3);
      var bit = 7 - (x & 7);
      var index = (*ptr >> bit) & 1;
      return this._palette[index];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      var colorIndex = this.FindColorIndex(value);
      var ptr = (byte*)this._data.Scan0 + y * this.Stride + (x >> 3);
      var bit = 7 - (x & 7);
      if (colorIndex == 1)
        *ptr |= (byte)(1 << bit);
      else
        *ptr &= (byte)~(1 << bit);
    }
  }

  public Indexed1BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, PixelFormat.Format1bppIndexed) { }

  public Indexed1BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public Indexed1BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, PixelFormat.Format1bppIndexed, PixelFormat.Format1bppIndexed) { }

  #region Vector-Optimized Drawing Methods

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var colorIndex = this.FindColorIndex(color);
    var packedByte = colorIndex == 1 ? (byte)0xFF : (byte)0x00;

    var ptr = (byte*)this._data.Scan0;
    var count = this.Stride * this.Height;

    // Vector512: 64 bytes per store, 4x unrolled = 256 bytes = 2048 pixels
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

    // Vector256: 32 bytes per store, 4x unrolled = 128 bytes = 1024 pixels
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

    // Vector128: 16 bytes per store, 4x unrolled = 64 bytes = 512 pixels
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
    var endX = x + length;
    var colorIndex = this.FindColorIndex(color);
    var stride = this.Stride;

    var rowPtr = (byte*)this._data.Scan0 + y * stride;

    var startByte = x >> 3;
    var endByte = (endX - 1) >> 3;
    var startBit = x & 7;
    var endBit = (endX - 1) & 7;

    // Single byte case
    if (startByte == endByte) {
      // Mask from startBit to endBit (bits 7-startBit to 7-endBit)
      var mask = (byte)(((0xFF >> startBit) & (0xFF << (7 - endBit))));
      if (colorIndex == 1)
        rowPtr[startByte] |= mask;
      else
        rowPtr[startByte] &= (byte)~mask;
      return;
    }

    // Handle start byte (bits 7-startBit down to 0)
    if (startBit != 0) {
      var mask = (byte)(0xFF >> startBit);
      if (colorIndex == 1)
        rowPtr[startByte] |= mask;
      else
        rowPtr[startByte] &= (byte)~mask;
      ++startByte;
    }

    // Handle end byte (bits 7 down to 7-endBit)
    if (endBit != 7) {
      var mask = (byte)(0xFF << (7 - endBit));
      if (colorIndex == 1)
        rowPtr[endByte] |= mask;
      else
        rowPtr[endByte] &= (byte)~mask;
      --endByte;
    }

    // Fill middle bytes
    if (startByte <= endByte) {
      var ptr = rowPtr + startByte;
      var count = endByte - startByte + 1;
      var packedByte = colorIndex == 1 ? (byte)0xFF : (byte)0x00;

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
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var colorIndex = this.FindColorIndex(color);
    var stride = this.Stride;
    var byteOffset = x >> 3;
    var bitMask = (byte)(1 << (7 - (x & 7)));
    var count = length;

    var ptr = (byte*)this._data.Scan0 + y * stride + byteOffset;

    // Precompute stride offsets to enable CPU parallelization
    var stride2 = stride * 2;
    var stride3 = stride * 3;
    var stride4 = stride * 4;
    var stride5 = stride * 5;
    var stride6 = stride * 6;
    var stride7 = stride * 7;
    var stride8 = stride * 8;

    if (colorIndex == 1) {
      // Set bits
      var end8 = count & ~7;
      for (var i = 0; i < end8; i += 8) {
        ptr[0] |= bitMask;
        ptr[stride] |= bitMask;
        ptr[stride2] |= bitMask;
        ptr[stride3] |= bitMask;
        ptr[stride4] |= bitMask;
        ptr[stride5] |= bitMask;
        ptr[stride6] |= bitMask;
        ptr[stride7] |= bitMask;
        ptr += stride8;
      }

      switch (count & 7) {
        case 7: ptr[stride6] |= bitMask; goto case 6;
        case 6: ptr[stride5] |= bitMask; goto case 5;
        case 5: ptr[stride4] |= bitMask; goto case 4;
        case 4: ptr[stride3] |= bitMask; goto case 3;
        case 3: ptr[stride2] |= bitMask; goto case 2;
        case 2: ptr[stride] |= bitMask; goto case 1;
        case 1: ptr[0] |= bitMask; break;
      }
    } else {
      // Clear bits
      var clearMask = (byte)~bitMask;
      var end8 = count & ~7;
      for (var i = 0; i < end8; i += 8) {
        ptr[0] &= clearMask;
        ptr[stride] &= clearMask;
        ptr[stride2] &= clearMask;
        ptr[stride3] &= clearMask;
        ptr[stride4] &= clearMask;
        ptr[stride5] &= clearMask;
        ptr[stride6] &= clearMask;
        ptr[stride7] &= clearMask;
        ptr += stride8;
      }

      switch (count & 7) {
        case 7: ptr[stride6] &= clearMask; goto case 6;
        case 6: ptr[stride5] &= clearMask; goto case 5;
        case 5: ptr[stride4] &= clearMask; goto case 4;
        case 4: ptr[stride3] &= clearMask; goto case 3;
        case 3: ptr[stride2] &= clearMask; goto case 2;
        case 2: ptr[stride] &= clearMask; goto case 1;
        case 1: ptr[0] &= clearMask; break;
      }
    }
  }

  /// <inheritdoc/>
  public override unsafe void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    var endX = x + width;
    var endY = y + height;
    var colorIndex = this.FindColorIndex(color);
    var stride = this.Stride;

    var basePtr = (byte*)this._data.Scan0;

    var startByte = x >> 3;
    var endByte = (endX - 1) >> 3;
    var startBit = x & 7;
    var endBit = (endX - 1) & 7;

    // Precompute masks
    var startMask = (byte)(0xFF >> startBit);
    var endMask = (byte)(0xFF << (7 - endBit));
    var packedByte = colorIndex == 1 ? (byte)0xFF : (byte)0x00;

    for (var row = y; row < endY; ++row) {
      var rowPtr = basePtr + row * stride;
      var currentStartByte = startByte;
      var currentEndByte = endByte;

      // Single byte case
      if (currentStartByte == currentEndByte) {
        var mask = (byte)(startMask & endMask);
        if (colorIndex == 1)
          rowPtr[currentStartByte] |= mask;
        else
          rowPtr[currentStartByte] &= (byte)~mask;
        continue;
      }

      // Handle start byte
      if (startBit != 0) {
        if (colorIndex == 1)
          rowPtr[currentStartByte] |= startMask;
        else
          rowPtr[currentStartByte] &= (byte)~startMask;
        ++currentStartByte;
      }

      // Handle end byte
      if (endBit != 7) {
        if (colorIndex == 1)
          rowPtr[currentEndByte] |= endMask;
        else
          rowPtr[currentEndByte] &= (byte)~endMask;
        --currentEndByte;
      }

      // Fill middle bytes
      if (currentStartByte <= currentEndByte) {
        var ptr = rowPtr + currentStartByte;
        var count = currentEndByte - currentStartByte + 1;

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
    }
  }

  #endregion
}
