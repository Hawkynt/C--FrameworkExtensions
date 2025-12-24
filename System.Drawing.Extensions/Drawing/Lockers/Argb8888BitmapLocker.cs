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
/// Provides span-based access to 32-bit ARGB bitmap data.
/// </summary>
internal sealed class Argb8888BitmapLocker : TypedBitmapLockerBase<Bgra8888> {

  public override unsafe Span<Bgra8888> Pixels {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new((void*)this._data.Scan0, this.Stride * this.Height);
  }

  /// <inheritdoc/>
  public override unsafe Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var pointer = (int*)this._data.Scan0;
      return Color.FromArgb(pointer[this.Stride * y + x]);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      var pointer = (int*)this._data.Scan0;
      pointer[this.Stride * y + x] = value.ToArgb();
    }
  }

  public Argb8888BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public Argb8888BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, 4, PixelFormat.Format32bppArgb, PixelFormat.Format32bppRgb) { }

  public Argb8888BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, 4, PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, PixelFormat.Format32bppRgb) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Color PixelToColor(Bgra8888 pixel) => Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Bgra8888 ColorToPixel(Color color) => new(color.R, color.G, color.B, color.A);

  #region Vector-Optimized Drawing Methods

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var packed = this.ColorToPixel(color).Packed;
    var ptr = (uint*)this._data.Scan0;
    var count = this.Stride * this.Height;

    // Vector512: 16 pixels per store, 4x unrolled = 64 pixels
    if (Vector512.IsHardwareAccelerated && count >= 16) {
      var vec = Vector512.Create(packed);
      var end64 = ptr + (count & ~63);
      while (ptr < end64) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 16);
        Vector512.Store(vec, ptr + 32);
        Vector512.Store(vec, ptr + 48);
        ptr += 64;
      }
      switch ((count >> 4) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 16; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 16; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 16; break;
      }
      count &= 15;
    }

    // Vector256: 8 pixels per store, 4x unrolled = 32 pixels
    if (Vector256.IsHardwareAccelerated && count >= 8) {
      var vec = Vector256.Create(packed);
      var end32 = ptr + (count & ~31);
      while (ptr < end32) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 8);
        Vector256.Store(vec, ptr + 16);
        Vector256.Store(vec, ptr + 24);
        ptr += 32;
      }
      switch ((count >> 3) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 8; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 8; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 8; break;
      }
      count &= 7;
    }

    // Vector128: 4 pixels per store, 4x unrolled = 16 pixels
    if (Vector128.IsHardwareAccelerated && count >= 4) {
      var vec = Vector128.Create(packed);
      var end16 = ptr + (count & ~15);
      while (ptr < end16) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 4);
        Vector128.Store(vec, ptr + 8);
        Vector128.Store(vec, ptr + 12);
        ptr += 16;
      }
      switch ((count >> 2) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 4; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 4; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 4; break;
      }
      count &= 3;
    }

    // 64-bit tier: 2 pixels per ulong, 4x unrolled = 8 pixels
    var packed64 = packed | ((ulong)packed << 32);
    if (count >= 2) {
      var end8 = ptr + (count & ~7);
      while (ptr < end8) {
        *(ulong*)ptr = packed64;
        *(ulong*)(ptr + 2) = packed64;
        *(ulong*)(ptr + 4) = packed64;
        *(ulong*)(ptr + 6) = packed64;
        ptr += 8;
      }
      switch ((count >> 1) & 3) {
        case 3: *(ulong*)ptr = packed64; ptr += 2; goto case 2;
        case 2: *(ulong*)ptr = packed64; ptr += 2; goto case 1;
        case 1: *(ulong*)ptr = packed64; ptr += 2; break;
      }
      count &= 1;
    }

    // Scalar cleanup: 0-1 pixels
    if (count > 0)
      *ptr = packed;
  }

  /// <inheritdoc/>
  public override unsafe void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    var packed = this.ColorToPixel(color).Packed;
    var count = length;
    var ptr = (uint*)this._data.Scan0 + y * this.Stride + x;

    if (Vector512.IsHardwareAccelerated && count >= 16) {
      var vec = Vector512.Create(packed);
      var end64 = ptr + (count & ~63);
      while (ptr < end64) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 16);
        Vector512.Store(vec, ptr + 32);
        Vector512.Store(vec, ptr + 48);
        ptr += 64;
      }
      switch ((count >> 4) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 16; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 16; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 16; break;
      }
      count &= 15;
    }

    if (Vector256.IsHardwareAccelerated && count >= 8) {
      var vec = Vector256.Create(packed);
      var end32 = ptr + (count & ~31);
      while (ptr < end32) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 8);
        Vector256.Store(vec, ptr + 16);
        Vector256.Store(vec, ptr + 24);
        ptr += 32;
      }
      switch ((count >> 3) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 8; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 8; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 8; break;
      }
      count &= 7;
    }

    if (Vector128.IsHardwareAccelerated && count >= 4) {
      var vec = Vector128.Create(packed);
      var end16 = ptr + (count & ~15);
      while (ptr < end16) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 4);
        Vector128.Store(vec, ptr + 8);
        Vector128.Store(vec, ptr + 12);
        ptr += 16;
      }
      switch ((count >> 2) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 4; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 4; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 4; break;
      }
      count &= 3;
    }

    // 64-bit tier: 2 pixels per ulong, 4x unrolled = 8 pixels
    var packed64 = packed | ((ulong)packed << 32);
    if (count >= 2) {
      var end8 = ptr + (count & ~7);
      while (ptr < end8) {
        *(ulong*)ptr = packed64;
        *(ulong*)(ptr + 2) = packed64;
        *(ulong*)(ptr + 4) = packed64;
        *(ulong*)(ptr + 6) = packed64;
        ptr += 8;
      }
      switch ((count >> 1) & 3) {
        case 3: *(ulong*)ptr = packed64; ptr += 2; goto case 2;
        case 2: *(ulong*)ptr = packed64; ptr += 2; goto case 1;
        case 1: *(ulong*)ptr = packed64; ptr += 2; break;
      }
      count &= 1;
    }

    // Scalar cleanup: 0-1 pixels
    if (count > 0)
      *ptr = packed;
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var packed = this.ColorToPixel(color).Packed;
    var stride = this.Stride;
    var count = length;
    var ptr = (uint*)this._data.Scan0 + y * stride + x;

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
    var basePtr = (uint*)this._data.Scan0;

    for (var row = y; row < endY; ++row) {
      var ptr = basePtr + row * stride + x;
      var count = width;

      if (Vector512.IsHardwareAccelerated && count >= 16) {
        var vec = Vector512.Create(packed);
        var end64 = ptr + (count & ~63);
        while (ptr < end64) {
          Vector512.Store(vec, ptr);
          Vector512.Store(vec, ptr + 16);
          Vector512.Store(vec, ptr + 32);
          Vector512.Store(vec, ptr + 48);
          ptr += 64;
        }
        switch ((count >> 4) & 3) {
          case 3: Vector512.Store(vec, ptr); ptr += 16; goto case 2;
          case 2: Vector512.Store(vec, ptr); ptr += 16; goto case 1;
          case 1: Vector512.Store(vec, ptr); ptr += 16; break;
        }
        count &= 15;
      }

      if (Vector256.IsHardwareAccelerated && count >= 8) {
        var vec = Vector256.Create(packed);
        var end32 = ptr + (count & ~31);
        while (ptr < end32) {
          Vector256.Store(vec, ptr);
          Vector256.Store(vec, ptr + 8);
          Vector256.Store(vec, ptr + 16);
          Vector256.Store(vec, ptr + 24);
          ptr += 32;
        }
        switch ((count >> 3) & 3) {
          case 3: Vector256.Store(vec, ptr); ptr += 8; goto case 2;
          case 2: Vector256.Store(vec, ptr); ptr += 8; goto case 1;
          case 1: Vector256.Store(vec, ptr); ptr += 8; break;
        }
        count &= 7;
      }

      if (Vector128.IsHardwareAccelerated && count >= 4) {
        var vec = Vector128.Create(packed);
        var end16 = ptr + (count & ~15);
        while (ptr < end16) {
          Vector128.Store(vec, ptr);
          Vector128.Store(vec, ptr + 4);
          Vector128.Store(vec, ptr + 8);
          Vector128.Store(vec, ptr + 12);
          ptr += 16;
        }
        switch ((count >> 2) & 3) {
          case 3: Vector128.Store(vec, ptr); ptr += 4; goto case 2;
          case 2: Vector128.Store(vec, ptr); ptr += 4; goto case 1;
          case 1: Vector128.Store(vec, ptr); ptr += 4; break;
        }
        count &= 3;
      }

      // 64-bit tier: 2 pixels per ulong, 4x unrolled = 8 pixels
      var packed64 = packed | ((ulong)packed << 32);
      if (count >= 2) {
        var end8 = ptr + (count & ~7);
        while (ptr < end8) {
          *(ulong*)ptr = packed64;
          *(ulong*)(ptr + 2) = packed64;
          *(ulong*)(ptr + 4) = packed64;
          *(ulong*)(ptr + 6) = packed64;
          ptr += 8;
        }
        switch ((count >> 1) & 3) {
          case 3: *(ulong*)ptr = packed64; ptr += 2; goto case 2;
          case 2: *(ulong*)ptr = packed64; ptr += 2; goto case 1;
          case 1: *(ulong*)ptr = packed64; ptr += 2; break;
        }
        count &= 1;
      }

      // Scalar cleanup: 0-1 pixels
      if (count > 0)
        *ptr = packed;
    }
  }

  #endregion
}
