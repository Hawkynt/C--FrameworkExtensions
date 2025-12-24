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
/// Provides access to 8-bit indexed (palette) bitmap data.
/// </summary>
internal sealed class Indexed8BitmapLocker : IndexedBitmapLockerBase {

  /// <inheritdoc/>
  public override unsafe Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var ptr = (byte*)this._data.Scan0 + y * this.Stride + x;
      return this._palette[*ptr];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      var colorIndex = this.FindColorIndex(value);
      var ptr = (byte*)this._data.Scan0 + y * this.Stride + x;
      *ptr = (byte)colorIndex;
    }
  }

  public Indexed8BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, PixelFormat.Format8bppIndexed) { }

  public Indexed8BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public Indexed8BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, PixelFormat.Format8bppIndexed, PixelFormat.Format8bppIndexed) { }

  #region Vector-Optimized Drawing Methods

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);

    var ptr = (byte*)this._data.Scan0;
    var count = this.Stride * this.Height;

    // Vector512: 64 bytes per store, 4x unrolled = 256 bytes
    if (Vector512.IsHardwareAccelerated && count >= 64) {
      var vec = Vector512.Create(colorIndex);
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
      var vec = Vector256.Create(colorIndex);
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
      var vec = Vector128.Create(colorIndex);
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
    var packed64 = 0x0101010101010101UL * colorIndex;
    if (count >= 8) {
      *(ulong*)ptr = packed64;
      ptr += 8;
      count -= 8;
    }

    switch (count) {
      case 7: ptr[6] = colorIndex; goto case 6;
      case 6: ptr[5] = colorIndex; goto case 5;
      case 5: ptr[4] = colorIndex; goto case 4;
      case 4: ptr[3] = colorIndex; goto case 3;
      case 3: ptr[2] = colorIndex; goto case 2;
      case 2: ptr[1] = colorIndex; goto case 1;
      case 1: ptr[0] = colorIndex; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);
    var count = length;

    var ptr = (byte*)this._data.Scan0 + y * this.Stride + x;

    if (Vector512.IsHardwareAccelerated && count >= 64) {
      var vec = Vector512.Create(colorIndex);
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
      var vec = Vector256.Create(colorIndex);
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
      var vec = Vector128.Create(colorIndex);
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
    var packed64 = 0x0101010101010101UL * colorIndex;
    if (count >= 8) {
      *(ulong*)ptr = packed64;
      ptr += 8;
      count -= 8;
    }

    switch (count) {
      case 7: ptr[6] = colorIndex; goto case 6;
      case 6: ptr[5] = colorIndex; goto case 5;
      case 5: ptr[4] = colorIndex; goto case 4;
      case 4: ptr[3] = colorIndex; goto case 3;
      case 3: ptr[2] = colorIndex; goto case 2;
      case 2: ptr[1] = colorIndex; goto case 1;
      case 1: ptr[0] = colorIndex; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);
    var stride = this.Stride;
    var count = length;

    var ptr = (byte*)this._data.Scan0 + y * stride + x;

    // Precompute stride offsets to enable CPU parallelization
    var stride2 = stride * 2;
    var stride3 = stride * 3;
    var stride4 = stride * 4;
    var stride5 = stride * 5;
    var stride6 = stride * 6;
    var stride7 = stride * 7;
    var stride8 = stride * 8;

    // 8x unrolled with parallel stores
    var end8 = count & ~7;
    for (var i = 0; i < end8; i += 8) {
      ptr[0] = colorIndex;
      ptr[stride] = colorIndex;
      ptr[stride2] = colorIndex;
      ptr[stride3] = colorIndex;
      ptr[stride4] = colorIndex;
      ptr[stride5] = colorIndex;
      ptr[stride6] = colorIndex;
      ptr[stride7] = colorIndex;
      ptr += stride8;
    }

    switch (count & 7) {
      case 7: ptr[stride6] = colorIndex; goto case 6;
      case 6: ptr[stride5] = colorIndex; goto case 5;
      case 5: ptr[stride4] = colorIndex; goto case 4;
      case 4: ptr[stride3] = colorIndex; goto case 3;
      case 3: ptr[stride2] = colorIndex; goto case 2;
      case 2: ptr[stride] = colorIndex; goto case 1;
      case 1: ptr[0] = colorIndex; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    var colorIndex = (byte)this.FindColorIndex(color);
    var stride = this.Stride;
    var endY = y + height;

    var basePtr = (byte*)this._data.Scan0;

    for (var row = y; row < endY; ++row) {
      var ptr = basePtr + row * stride + x;
      var count = width;

      if (Vector512.IsHardwareAccelerated && count >= 64) {
        var vec = Vector512.Create(colorIndex);
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
        var vec = Vector256.Create(colorIndex);
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
        var vec = Vector128.Create(colorIndex);
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
      var packed64 = 0x0101010101010101UL * colorIndex;
      if (count >= 8) {
        *(ulong*)ptr = packed64;
        ptr += 8;
        count -= 8;
      }

      switch (count) {
        case 7: ptr[6] = colorIndex; goto case 6;
        case 6: ptr[5] = colorIndex; goto case 5;
        case 5: ptr[4] = colorIndex; goto case 4;
        case 4: ptr[3] = colorIndex; goto case 3;
        case 3: ptr[2] = colorIndex; goto case 2;
        case 2: ptr[1] = colorIndex; goto case 1;
        case 1: ptr[0] = colorIndex; break;
      }
    }
  }

  #endregion
}
