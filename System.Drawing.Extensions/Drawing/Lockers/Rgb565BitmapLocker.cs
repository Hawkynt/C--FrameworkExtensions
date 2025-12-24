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
/// Provides span-based access to 16-bit RGB565 bitmap data.
/// </summary>
internal sealed class Rgb565BitmapLocker : TypedBitmapLockerBase<Rgb565> {

  public override unsafe Span<Rgb565> Pixels {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new((void*)this._data.Scan0, this.Stride * this.Height);
  }

  /// <inheritdoc/>
  public override Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var pixel = this.Pixels[this.Stride * y + x];
      return Color.FromArgb(255, pixel.R, pixel.G, pixel.B);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.Pixels[this.Stride * y + x] = new Rgb565(value.R, value.G, value.B);
  }

  public Rgb565BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, 2, PixelFormat.Format16bppRgb565) { }

  public Rgb565BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public Rgb565BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, 2, PixelFormat.Format16bppRgb565, PixelFormat.Format16bppRgb565) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Color PixelToColor(Rgb565 pixel) => Color.FromArgb(255, pixel.R, pixel.G, pixel.B);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Rgb565 ColorToPixel(Color color) => new(color.R, color.G, color.B);

  #region Vector-Optimized Drawing Methods

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var packed = this.ColorToPixel(color).Packed;

    var ptr = (ushort*)this._data.Scan0;
    var count = this.Stride * this.Height;

    // Vector512: 32 pixels per store, 4x unrolled = 128 pixels
    if (Vector512.IsHardwareAccelerated && count >= 32) {
      var vec = Vector512.Create(packed);
      var end128 = ptr + (count & ~127);
      while (ptr < end128) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 32);
        Vector512.Store(vec, ptr + 64);
        Vector512.Store(vec, ptr + 96);
        ptr += 128;
      }
      switch ((count >> 5) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 32; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 32; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 32; break;
      }
      count &= 31;
    }

    // Vector256: 16 pixels per store, 4x unrolled = 64 pixels
    if (Vector256.IsHardwareAccelerated && count >= 16) {
      var vec = Vector256.Create(packed);
      var end64 = ptr + (count & ~63);
      while (ptr < end64) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 16);
        Vector256.Store(vec, ptr + 32);
        Vector256.Store(vec, ptr + 48);
        ptr += 64;
      }
      switch ((count >> 4) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 16; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 16; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 16; break;
      }
      count &= 15;
    }

    // Vector128: 8 pixels per store, 4x unrolled = 32 pixels
    if (Vector128.IsHardwareAccelerated && count >= 8) {
      var vec = Vector128.Create(packed);
      var end32 = ptr + (count & ~31);
      while (ptr < end32) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 8);
        Vector128.Store(vec, ptr + 16);
        Vector128.Store(vec, ptr + 24);
        ptr += 32;
      }
      switch ((count >> 3) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 8; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 8; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 8; break;
      }
      count &= 7;
    }

    // Scalar fallback: use 64-bit writes for 4 pixels at a time
    var packed64 = (ulong)packed | ((ulong)packed << 16) | ((ulong)packed << 32) | ((ulong)packed << 48);
    while (count >= 4) {
      *(ulong*)ptr = packed64;
      ptr += 4;
      count -= 4;
    }
    // count is now 0-3
    switch (count) {
      case 3: ptr[0] = packed; ptr[1] = packed; ptr[2] = packed; break;
      case 2: ptr[0] = packed; ptr[1] = packed; break;
      case 1: ptr[0] = packed; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    var packed = this.ColorToPixel(color).Packed;
    var count = length;

    var ptr = (ushort*)this._data.Scan0 + y * this.Stride + x;

    if (Vector512.IsHardwareAccelerated && count >= 32) {
      var vec = Vector512.Create(packed);
      var end128 = ptr + (count & ~127);
      while (ptr < end128) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 32);
        Vector512.Store(vec, ptr + 64);
        Vector512.Store(vec, ptr + 96);
        ptr += 128;
      }
      switch ((count >> 5) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 32; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 32; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 32; break;
      }
      count &= 31;
    }

    if (Vector256.IsHardwareAccelerated && count >= 16) {
      var vec = Vector256.Create(packed);
      var end64 = ptr + (count & ~63);
      while (ptr < end64) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 16);
        Vector256.Store(vec, ptr + 32);
        Vector256.Store(vec, ptr + 48);
        ptr += 64;
      }
      switch ((count >> 4) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 16; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 16; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 16; break;
      }
      count &= 15;
    }

    if (Vector128.IsHardwareAccelerated && count >= 8) {
      var vec = Vector128.Create(packed);
      var end32 = ptr + (count & ~31);
      while (ptr < end32) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 8);
        Vector128.Store(vec, ptr + 16);
        Vector128.Store(vec, ptr + 24);
        ptr += 32;
      }
      switch ((count >> 3) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 8; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 8; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 8; break;
      }
      count &= 7;
    }

    // Scalar fallback: use 64-bit writes for 4 pixels at a time
    var packed64 = (ulong)packed | ((ulong)packed << 16) | ((ulong)packed << 32) | ((ulong)packed << 48);
    while (count >= 4) {
      *(ulong*)ptr = packed64;
      ptr += 4;
      count -= 4;
    }
    // count is now 0-3
    switch (count) {
      case 3: ptr[0] = packed; ptr[1] = packed; ptr[2] = packed; break;
      case 2: ptr[0] = packed; ptr[1] = packed; break;
      case 1: ptr[0] = packed; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var packed = this.ColorToPixel(color).Packed;
    var stride = this.Stride;
    var count = length;

    var ptr = (ushort*)this._data.Scan0 + y * stride + x;

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
      ptr[0] = packed;
      ptr[stride] = packed;
      ptr[stride2] = packed;
      ptr[stride3] = packed;
      ptr[stride4] = packed;
      ptr[stride5] = packed;
      ptr[stride6] = packed;
      ptr[stride7] = packed;
      ptr += stride8;
    }

    switch (count & 7) {
      case 7: ptr[stride6] = packed; goto case 6;
      case 6: ptr[stride5] = packed; goto case 5;
      case 5: ptr[stride4] = packed; goto case 4;
      case 4: ptr[stride3] = packed; goto case 3;
      case 3: ptr[stride2] = packed; goto case 2;
      case 2: ptr[stride] = packed; goto case 1;
      case 1: ptr[0] = packed; break;
    }
  }

  /// <inheritdoc/>
  public override unsafe void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
    var packed = this.ColorToPixel(color).Packed;
    var stride = this.Stride;
    var endY = y + height;

    var basePtr = (ushort*)this._data.Scan0;

    for (var row = y; row < endY; ++row) {
      var ptr = basePtr + row * stride + x;
      var count = width;

      if (Vector512.IsHardwareAccelerated && count >= 32) {
        var vec = Vector512.Create(packed);
        var end128 = ptr + (count & ~127);
        while (ptr < end128) {
          Vector512.Store(vec, ptr);
          Vector512.Store(vec, ptr + 32);
          Vector512.Store(vec, ptr + 64);
          Vector512.Store(vec, ptr + 96);
          ptr += 128;
        }
        switch ((count >> 5) & 3) {
          case 3: Vector512.Store(vec, ptr); ptr += 32; goto case 2;
          case 2: Vector512.Store(vec, ptr); ptr += 32; goto case 1;
          case 1: Vector512.Store(vec, ptr); ptr += 32; break;
        }
        count &= 31;
      }

      if (Vector256.IsHardwareAccelerated && count >= 16) {
        var vec = Vector256.Create(packed);
        var end64 = ptr + (count & ~63);
        while (ptr < end64) {
          Vector256.Store(vec, ptr);
          Vector256.Store(vec, ptr + 16);
          Vector256.Store(vec, ptr + 32);
          Vector256.Store(vec, ptr + 48);
          ptr += 64;
        }
        switch ((count >> 4) & 3) {
          case 3: Vector256.Store(vec, ptr); ptr += 16; goto case 2;
          case 2: Vector256.Store(vec, ptr); ptr += 16; goto case 1;
          case 1: Vector256.Store(vec, ptr); ptr += 16; break;
        }
        count &= 15;
      }

      if (Vector128.IsHardwareAccelerated && count >= 8) {
        var vec = Vector128.Create(packed);
        var end32 = ptr + (count & ~31);
        while (ptr < end32) {
          Vector128.Store(vec, ptr);
          Vector128.Store(vec, ptr + 8);
          Vector128.Store(vec, ptr + 16);
          Vector128.Store(vec, ptr + 24);
          ptr += 32;
        }
        switch ((count >> 3) & 3) {
          case 3: Vector128.Store(vec, ptr); ptr += 8; goto case 2;
          case 2: Vector128.Store(vec, ptr); ptr += 8; goto case 1;
          case 1: Vector128.Store(vec, ptr); ptr += 8; break;
        }
        count &= 7;
      }

      // Scalar fallback: use 64-bit writes for 4 pixels at a time
      var packed64 = (ulong)packed | ((ulong)packed << 16) | ((ulong)packed << 32) | ((ulong)packed << 48);
      while (count >= 4) {
        *(ulong*)ptr = packed64;
        ptr += 4;
        count -= 4;
      }
      // count is now 0-3
      switch (count) {
        case 3: ptr[0] = packed; ptr[1] = packed; ptr[2] = packed; break;
        case 2: ptr[0] = packed; ptr[1] = packed; break;
        case 1: ptr[0] = packed; break;
      }
    }
  }

  #endregion
}
