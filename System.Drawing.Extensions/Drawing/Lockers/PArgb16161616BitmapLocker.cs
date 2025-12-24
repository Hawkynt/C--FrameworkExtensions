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
/// Provides span-based access to 64-bit pre-multiplied ARGB bitmap data (16 bits per channel).
/// </summary>
/// <remarks>
/// Memory layout: [R:16][G:16][B:16][A:16] in native endianness.
/// Pre-multiplied alpha means R, G, B values are stored multiplied by alpha.
/// </remarks>
internal sealed class PArgb16161616BitmapLocker : TypedBitmapLockerBase<Rgba64> {

  public override unsafe Span<Rgba64> Pixels {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new((void*)this._data.Scan0, this.Stride * this.Height);
  }

  /// <inheritdoc/>
  public override Color this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var pixel = this.Pixels[this.Stride * y + x];
      return this.PixelToColor(pixel);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.Pixels[this.Stride * y + x] = this.ColorToPixel(value);
  }

  public PArgb16161616BitmapLocker(Bitmap bitmap) : this(bitmap, ImageLockMode.ReadWrite) { }

  public PArgb16161616BitmapLocker(Bitmap bitmap, ImageLockMode lockMode)
    : base(bitmap, lockMode, 8, PixelFormat.Format64bppPArgb) { }

  public PArgb16161616BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat format)
    : base(bitmap, rect, lockMode, 8, PixelFormat.Format64bppPArgb, PixelFormat.Format64bppPArgb) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Color PixelToColor(Rgba64 pixel) {
    var a = pixel.AValue;
    if (a == 0)
      return Color.FromArgb(0, 0, 0, 0);
    // Un-premultiply: original = premultiplied * 65535 / alpha, then scale to 8-bit
    var a8 = (byte)(a >> 8);
    if (a == 65535)
      return Color.FromArgb(255, pixel.R, pixel.G, pixel.B);
    return Color.FromArgb(
      a8,
      (byte)((pixel.RValue * 255L) / a),
      (byte)((pixel.GValue * 255L) / a),
      (byte)((pixel.BValue * 255L) / a)
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected override Rgba64 ColorToPixel(Color color) {
    var a = color.A;
    if (a == 0)
      return new Rgba64(0, 0, 0, 0);
    if (a == 255)
      return new Rgba64(color.R, color.G, color.B, 255);
    // Expand to 16-bit then premultiply
    var a16 = (ushort)((a << 8) | a);
    var r16 = (ushort)((color.R << 8) | color.R);
    var g16 = (ushort)((color.G << 8) | color.G);
    var b16 = (ushort)((color.B << 8) | color.B);
    return new Rgba64(
      (ushort)((r16 * a16) / 65535),
      (ushort)((g16 * a16) / 65535),
      (ushort)((b16 * a16) / 65535),
      a16
    );
  }

  #region Vector-Optimized Drawing Methods

  /// <summary>
  /// Packs an Rgba64 pixel into a ulong for fast memory operations.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe ulong PackPixel(Rgba64 pixel) {
    var ptr = (ulong*)&pixel;
    return *ptr;
  }

  /// <inheritdoc/>
  public override unsafe void Clear(Color color) {
    var pixel = this.ColorToPixel(color);
    var packed = PackPixel(pixel);

    var ptr = (ulong*)this._data.Scan0;
    var count = this.Stride * this.Height;

    // Vector512: 8 pixels per store, 4x unrolled = 32 pixels
    if (Vector512.IsHardwareAccelerated && count >= 8) {
      var vec = Vector512.Create(packed);
      var end32 = ptr + (count & ~31);
      while (ptr < end32) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 8);
        Vector512.Store(vec, ptr + 16);
        Vector512.Store(vec, ptr + 24);
        ptr += 32;
      }
      switch ((count >> 3) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 8; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 8; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 8; break;
      }
      count &= 7;
    }

    // Vector256: 4 pixels per store, 4x unrolled = 16 pixels
    if (Vector256.IsHardwareAccelerated && count >= 4) {
      var vec = Vector256.Create(packed);
      var end16 = ptr + (count & ~15);
      while (ptr < end16) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 4);
        Vector256.Store(vec, ptr + 8);
        Vector256.Store(vec, ptr + 12);
        ptr += 16;
      }
      switch ((count >> 2) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 4; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 4; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 4; break;
      }
      count &= 3;
    }

    // Vector128: 2 pixels per store, 4x unrolled = 8 pixels
    if (Vector128.IsHardwareAccelerated && count >= 2) {
      var vec = Vector128.Create(packed);
      var end8 = ptr + (count & ~7);
      while (ptr < end8) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 2);
        Vector128.Store(vec, ptr + 4);
        Vector128.Store(vec, ptr + 6);
        ptr += 8;
      }
      switch ((count >> 1) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 2; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 2; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 2; break;
      }
      count &= 1;
    }

    // Scalar fallback: handles all remaining pixels when no vectors supported
    while (count > 0) {
      *ptr++ = packed;
      --count;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawHorizontalLineUnchecked(int x, int y, int length, Color color) {
    var pixel = this.ColorToPixel(color);
    var packed = PackPixel(pixel);
    var count = length;

    var ptr = (ulong*)this._data.Scan0 + y * this.Stride + x;

    if (Vector512.IsHardwareAccelerated && count >= 8) {
      var vec = Vector512.Create(packed);
      var end32 = ptr + (count & ~31);
      while (ptr < end32) {
        Vector512.Store(vec, ptr);
        Vector512.Store(vec, ptr + 8);
        Vector512.Store(vec, ptr + 16);
        Vector512.Store(vec, ptr + 24);
        ptr += 32;
      }
      switch ((count >> 3) & 3) {
        case 3: Vector512.Store(vec, ptr); ptr += 8; goto case 2;
        case 2: Vector512.Store(vec, ptr); ptr += 8; goto case 1;
        case 1: Vector512.Store(vec, ptr); ptr += 8; break;
      }
      count &= 7;
    }

    if (Vector256.IsHardwareAccelerated && count >= 4) {
      var vec = Vector256.Create(packed);
      var end16 = ptr + (count & ~15);
      while (ptr < end16) {
        Vector256.Store(vec, ptr);
        Vector256.Store(vec, ptr + 4);
        Vector256.Store(vec, ptr + 8);
        Vector256.Store(vec, ptr + 12);
        ptr += 16;
      }
      switch ((count >> 2) & 3) {
        case 3: Vector256.Store(vec, ptr); ptr += 4; goto case 2;
        case 2: Vector256.Store(vec, ptr); ptr += 4; goto case 1;
        case 1: Vector256.Store(vec, ptr); ptr += 4; break;
      }
      count &= 3;
    }

    if (Vector128.IsHardwareAccelerated && count >= 2) {
      var vec = Vector128.Create(packed);
      var end8 = ptr + (count & ~7);
      while (ptr < end8) {
        Vector128.Store(vec, ptr);
        Vector128.Store(vec, ptr + 2);
        Vector128.Store(vec, ptr + 4);
        Vector128.Store(vec, ptr + 6);
        ptr += 8;
      }
      switch ((count >> 1) & 3) {
        case 3: Vector128.Store(vec, ptr); ptr += 2; goto case 2;
        case 2: Vector128.Store(vec, ptr); ptr += 2; goto case 1;
        case 1: Vector128.Store(vec, ptr); ptr += 2; break;
      }
      count &= 1;
    }

    while (count > 0) {
      *ptr++ = packed;
      --count;
    }
  }

  /// <inheritdoc/>
  public override unsafe void DrawVerticalLineUnchecked(int x, int y, int length, Color color) {
    var pixel = this.ColorToPixel(color);
    var packed = PackPixel(pixel);
    var stride = this.Stride;
    var count = length;

    var ptr = (ulong*)this._data.Scan0 + y * stride + x;

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
    var pixel = this.ColorToPixel(color);
    var packed = PackPixel(pixel);
    var stride = this.Stride;
    var endY = y + height;

    var basePtr = (ulong*)this._data.Scan0;

    for (var row = y; row < endY; ++row) {
      var ptr = basePtr + row * stride + x;
      var count = width;

      if (Vector512.IsHardwareAccelerated && count >= 8) {
        var vec = Vector512.Create(packed);
        var end32 = ptr + (count & ~31);
        while (ptr < end32) {
          Vector512.Store(vec, ptr);
          Vector512.Store(vec, ptr + 8);
          Vector512.Store(vec, ptr + 16);
          Vector512.Store(vec, ptr + 24);
          ptr += 32;
        }
        switch ((count >> 3) & 3) {
          case 3: Vector512.Store(vec, ptr); ptr += 8; goto case 2;
          case 2: Vector512.Store(vec, ptr); ptr += 8; goto case 1;
          case 1: Vector512.Store(vec, ptr); ptr += 8; break;
        }
        count &= 7;
      }

      if (Vector256.IsHardwareAccelerated && count >= 4) {
        var vec = Vector256.Create(packed);
        var end16 = ptr + (count & ~15);
        while (ptr < end16) {
          Vector256.Store(vec, ptr);
          Vector256.Store(vec, ptr + 4);
          Vector256.Store(vec, ptr + 8);
          Vector256.Store(vec, ptr + 12);
          ptr += 16;
        }
        switch ((count >> 2) & 3) {
          case 3: Vector256.Store(vec, ptr); ptr += 4; goto case 2;
          case 2: Vector256.Store(vec, ptr); ptr += 4; goto case 1;
          case 1: Vector256.Store(vec, ptr); ptr += 4; break;
        }
        count &= 3;
      }

      if (Vector128.IsHardwareAccelerated && count >= 2) {
        var vec = Vector128.Create(packed);
        var end8 = ptr + (count & ~7);
        while (ptr < end8) {
          Vector128.Store(vec, ptr);
          Vector128.Store(vec, ptr + 2);
          Vector128.Store(vec, ptr + 4);
          Vector128.Store(vec, ptr + 6);
          ptr += 8;
        }
        switch ((count >> 1) & 3) {
          case 3: Vector128.Store(vec, ptr); ptr += 2; goto case 2;
          case 2: Vector128.Store(vec, ptr); ptr += 2; goto case 1;
          case 1: Vector128.Store(vec, ptr); ptr += 2; break;
        }
        count &= 1;
      }

      while (count > 0) {
        *ptr++ = packed;
        --count;
      }
    }
  }

  #endregion
}
