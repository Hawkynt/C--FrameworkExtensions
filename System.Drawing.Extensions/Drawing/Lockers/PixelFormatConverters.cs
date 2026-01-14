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

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Provides fast pixel format conversion between different bitmap formats.
/// </summary>
internal static class PixelFormatConverters {
  
  #region Fast Conversion Dispatcher

  /// <summary>
  /// Attempts fast pixel format conversion using SIMD-optimized routines.
  /// </summary>
  /// <param name="src">Source bitmap locker.</param>
  /// <param name="dst">Destination bitmap locker.</param>
  /// <returns>True if fast conversion was performed; false to fall back to GDI+.</returns>
  internal static bool TryFastConvert(IBitmapLocker src, IBitmapLocker dst) {
    if (src.Width != dst.Width || src.Height != dst.Height)
      return false;

    return (src, dst) switch {
      // Indexed → Rgba32 (very common for loading palette images)
      (Indexed8BitmapLocker s, Argb8888BitmapLocker d) => _ConvertIndexed8ToRgba32(s, d),
      (Indexed4BitmapLocker s, Argb8888BitmapLocker d) => _ConvertIndexed4ToRgba32(s, d),
      (Indexed1BitmapLocker s, Argb8888BitmapLocker d) => _ConvertIndexed1ToRgba32(s, d),

      // Truecolor conversions
      (Argb8888BitmapLocker s, Rgb888BitmapLocker d) => _ConvertRgba32ToRgb24(s, d),
      (Rgb888BitmapLocker s, Argb8888BitmapLocker d) => _ConvertRgb24ToRgba32(s, d),
      (Argb8888BitmapLocker s, Rgb565BitmapLocker d) => _ConvertRgba32ToRgb16(s, d),
      (Rgb565BitmapLocker s, Argb8888BitmapLocker d) => _ConvertRgb16ToRgba32(s, d),
      (Argb8888BitmapLocker s, Rgb555BitmapLocker d) => _ConvertRgba32ToRgb15(s, d),
      (Rgb555BitmapLocker s, Argb8888BitmapLocker d) => _ConvertRgb15ToRgba32(s, d),

      // Pre-multiplied alpha conversions
      (PArgb8888BitmapLocker s, Argb8888BitmapLocker d) => _ConvertPArgb32ToRgba32(s, d),
      (Argb8888BitmapLocker s, PArgb8888BitmapLocker d) => _ConvertRgba32ToPArgb32(s, d),

      // High-depth conversions (to ARGB32 for display)
      (Argb16161616BitmapLocker s, Argb8888BitmapLocker d) => _ConvertRgba64ToRgba32(s, d),
      (PArgb16161616BitmapLocker s, Argb8888BitmapLocker d) => _ConvertPRgba64ToRgba32(s, d),
      (Rgb161616BitmapLocker s, Argb8888BitmapLocker d) => _ConvertRgb48ToRgba32(s, d),

      // Unknown pair → GDI+ fallback
      _ => false
    };
  }

  #endregion

  #region Indexed → Rgba32 Conversions

  private static bool _ConvertIndexed8ToRgba32(Indexed8BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (byte*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      // Pre-convert palette to packed uint for fast lookup
      var srcPalette = src.Palette;
      var palette = new uint[256];
      for (var i = 0; i < srcPalette.Length; ++i)
        palette[i] = (uint)srcPalette[i].ToArgb();

      fixed (uint* palettePtr = palette)
        for (var y = 0; y < height; ++y) {
          var srcRow = srcBase + y * srcStride;
          var dstRow = dstBase + y * dstStride;
          var count = width;

          // Scalar conversion (palette lookup is already fast)
          while (count >= 8) {
            dstRow[0] = palettePtr[srcRow[0]];
            dstRow[1] = palettePtr[srcRow[1]];
            dstRow[2] = palettePtr[srcRow[2]];
            dstRow[3] = palettePtr[srcRow[3]];
            dstRow[4] = palettePtr[srcRow[4]];
            dstRow[5] = palettePtr[srcRow[5]];
            dstRow[6] = palettePtr[srcRow[6]];
            dstRow[7] = palettePtr[srcRow[7]];
            srcRow += 8;
            dstRow += 8;
            count -= 8;
          }

          switch (count) {
            case 7: dstRow[6] = palettePtr[srcRow[6]]; goto case 6;
            case 6: dstRow[5] = palettePtr[srcRow[5]]; goto case 5;
            case 5: dstRow[4] = palettePtr[srcRow[4]]; goto case 4;
            case 4: dstRow[3] = palettePtr[srcRow[3]]; goto case 3;
            case 3: dstRow[2] = palettePtr[srcRow[2]]; goto case 2;
            case 2: dstRow[1] = palettePtr[srcRow[1]]; goto case 1;
            case 1: dstRow[0] = palettePtr[srcRow[0]]; break;
          }
        }
    }

    return true;
  }

  private static bool _ConvertIndexed4ToRgba32(Indexed4BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (byte*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      // Pre-convert palette to packed uint
      var srcPalette = src.Palette;
      var palette = new uint[16];
      for (var i = 0; i < srcPalette.Length && i < 16; ++i)
        palette[i] = (uint)srcPalette[i].ToArgb();

      fixed (uint* palettePtr = palette)
        for (var y = 0; y < height; ++y) {
          var srcRow = srcBase + y * srcStride;
          var dstRow = dstBase + y * dstStride;

          for (var x = 0; x < width; ++x) {
            var byteIndex = x >> 1;
            var nibble = (x & 1) == 0
              ? (srcRow[byteIndex] >> 4) & 0x0F
              : srcRow[byteIndex] & 0x0F;
            dstRow[x] = palettePtr[nibble];
          }
        }
    }

    return true;
  }

  private static bool _ConvertIndexed1ToRgba32(Indexed1BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (byte*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      // Pre-convert 2-color palette
      var srcPalette = src.Palette;
      var color0 = srcPalette.Length > 0 ? (uint)srcPalette[0].ToArgb() : 0xFF000000u;
      var color1 = srcPalette.Length > 1 ? (uint)srcPalette[1].ToArgb() : 0xFFFFFFFFu;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;

        for (var x = 0; x < width; ++x) {
          var byteIndex = x >> 3;
          var bitIndex = 7 - (x & 7);
          var bit = (srcRow[byteIndex] >> bitIndex) & 1;
          dstRow[x] = bit == 0 ? color0 : color1;
        }
      }
    }

    return true;
  }

  #endregion

  #region Rgba32 ↔ Rgb24 Conversions

  private static bool _ConvertRgba32ToRgb24(Argb8888BitmapLocker src, Rgb888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStrideBytes = dst.BitmapData.Stride;

    unsafe {
      var srcBase = (uint*)src.BitmapData.Scan0;
      var dstBase = (byte*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStrideBytes;
        var count = width;

        // Process 4 pixels at a time (16 bytes → 12 bytes)
        while (count >= 4) {
          var p0 = srcRow[0];
          var p1 = srcRow[1];
          var p2 = srcRow[2];
          var p3 = srcRow[3];

          // Memory layout: BGRA → BGR (drop A)
          dstRow[0] = (byte)p0;         // B0
          dstRow[1] = (byte)(p0 >> 8);  // G0
          dstRow[2] = (byte)(p0 >> 16); // R0
          dstRow[3] = (byte)p1;         // B1
          dstRow[4] = (byte)(p1 >> 8);  // G1
          dstRow[5] = (byte)(p1 >> 16); // R1
          dstRow[6] = (byte)p2;         // B2
          dstRow[7] = (byte)(p2 >> 8);  // G2
          dstRow[8] = (byte)(p2 >> 16); // R2
          dstRow[9] = (byte)p3;         // B3
          dstRow[10] = (byte)(p3 >> 8); // G3
          dstRow[11] = (byte)(p3 >> 16);// R3

          srcRow += 4;
          dstRow += 12;
          count -= 4;
        }

        // Handle remaining 0-3 pixels
        while (count > 0) {
          var p = *srcRow++;
          dstRow[0] = (byte)p;
          dstRow[1] = (byte)(p >> 8);
          dstRow[2] = (byte)(p >> 16);
          dstRow += 3;
          --count;
        }
      }
    }

    return true;
  }

  private static bool _ConvertRgb24ToRgba32(Rgb888BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStrideBytes = src.BitmapData.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (byte*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStrideBytes;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 4 pixels at a time (12 bytes → 16 bytes)
        while (count >= 4) {
          // Memory layout: BGR → BGRA (add 0xFF alpha)
          dstRow[0] = srcRow[0] | ((uint)srcRow[1] << 8) | ((uint)srcRow[2] << 16) | 0xFF000000u;
          dstRow[1] = srcRow[3] | ((uint)srcRow[4] << 8) | ((uint)srcRow[5] << 16) | 0xFF000000u;
          dstRow[2] = srcRow[6] | ((uint)srcRow[7] << 8) | ((uint)srcRow[8] << 16) | 0xFF000000u;
          dstRow[3] = srcRow[9] | ((uint)srcRow[10] << 8) | ((uint)srcRow[11] << 16) | 0xFF000000u;

          srcRow += 12;
          dstRow += 4;
          count -= 4;
        }

        // Handle remaining 0-3 pixels
        while (count > 0) {
          *dstRow++ = srcRow[0] | ((uint)srcRow[1] << 8) | ((uint)srcRow[2] << 16) | 0xFF000000u;
          srcRow += 3;
          --count;
        }
      }
    }

    return true;
  }

  #endregion

  #region Rgba32 ↔ Rgb16 (565) Conversions

  private static bool _ConvertRgba32ToRgb16(Argb8888BitmapLocker src, Rgb565BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (uint*)src.BitmapData.Scan0;
      var dstBase = (ushort*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 8 pixels at a time
        while (count >= 8) {
          for (var i = 0; i < 8; ++i) {
            var p = srcRow[i];
            var r5 = (p >> 19) & 0x1F;  // R8 >> 3
            var g6 = (p >> 10) & 0x3F;  // G8 >> 2
            var b5 = (p >> 3) & 0x1F;   // B8 >> 3
            dstRow[i] = (ushort)((r5 << 11) | (g6 << 5) | b5);
          }
          srcRow += 8;
          dstRow += 8;
          count -= 8;
        }

        // Handle remaining pixels
        while (count > 0) {
          var p = *srcRow++;
          var r5 = (p >> 19) & 0x1F;
          var g6 = (p >> 10) & 0x3F;
          var b5 = (p >> 3) & 0x1F;
          *dstRow++ = (ushort)((r5 << 11) | (g6 << 5) | b5);
          --count;
        }
      }
    }

    return true;
  }

  private static bool _ConvertRgb16ToRgba32(Rgb565BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (ushort*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 8 pixels at a time
        while (count >= 8) {
          for (var i = 0; i < 8; ++i) {
            var p = srcRow[i];
            var r5 = (p >> 11) & 0x1F;
            var g6 = (p >> 5) & 0x3F;
            var b5 = p & 0x1F;
            // Expand to 8-bit: replicate high bits to low bits
            var r8 = (r5 << 3) | (r5 >> 2);
            var g8 = (g6 << 2) | (g6 >> 4);
            var b8 = (b5 << 3) | (b5 >> 2);
            dstRow[i] = (uint)(b8 | (g8 << 8) | (r8 << 16) | 0xFF000000);
          }
          srcRow += 8;
          dstRow += 8;
          count -= 8;
        }

        // Handle remaining pixels
        while (count > 0) {
          var p = *srcRow++;
          var r5 = (p >> 11) & 0x1F;
          var g6 = (p >> 5) & 0x3F;
          var b5 = p & 0x1F;
          var r8 = (r5 << 3) | (r5 >> 2);
          var g8 = (g6 << 2) | (g6 >> 4);
          var b8 = (b5 << 3) | (b5 >> 2);
          *dstRow++ = (uint)(b8 | (g8 << 8) | (r8 << 16) | 0xFF000000);
          --count;
        }
      }
    }

    return true;
  }

  #endregion

  #region Rgba32 ↔ Rgb15 (555) Conversions

  private static bool _ConvertRgba32ToRgb15(Argb8888BitmapLocker src, Rgb555BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (uint*)src.BitmapData.Scan0;
      var dstBase = (ushort*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 8 pixels at a time
        while (count >= 8) {
          for (var i = 0; i < 8; ++i) {
            var p = srcRow[i];
            var r5 = (p >> 19) & 0x1F;  // R8 >> 3
            var g5 = (p >> 11) & 0x1F;  // G8 >> 3
            var b5 = (p >> 3) & 0x1F;   // B8 >> 3
            dstRow[i] = (ushort)((r5 << 10) | (g5 << 5) | b5);
          }
          srcRow += 8;
          dstRow += 8;
          count -= 8;
        }

        // Handle remaining pixels
        while (count > 0) {
          var p = *srcRow++;
          var r5 = (p >> 19) & 0x1F;
          var g5 = (p >> 11) & 0x1F;
          var b5 = (p >> 3) & 0x1F;
          *dstRow++ = (ushort)((r5 << 10) | (g5 << 5) | b5);
          --count;
        }
      }
    }

    return true;
  }

  private static bool _ConvertRgb15ToRgba32(Rgb555BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (ushort*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 8 pixels at a time
        while (count >= 8) {
          for (var i = 0; i < 8; ++i) {
            var p = srcRow[i];
            var r5 = (p >> 10) & 0x1F;
            var g5 = (p >> 5) & 0x1F;
            var b5 = p & 0x1F;
            // Expand to 8-bit: replicate high bits to low bits
            var r8 = (r5 << 3) | (r5 >> 2);
            var g8 = (g5 << 3) | (g5 >> 2);
            var b8 = (b5 << 3) | (b5 >> 2);
            dstRow[i] = (uint)(b8 | (g8 << 8) | (r8 << 16) | 0xFF000000);
          }
          srcRow += 8;
          dstRow += 8;
          count -= 8;
        }

        // Handle remaining pixels
        while (count > 0) {
          var p = *srcRow++;
          var r5 = (p >> 10) & 0x1F;
          var g5 = (p >> 5) & 0x1F;
          var b5 = p & 0x1F;
          var r8 = (r5 << 3) | (r5 >> 2);
          var g8 = (g5 << 3) | (g5 >> 2);
          var b8 = (b5 << 3) | (b5 >> 2);
          *dstRow++ = (uint)(b8 | (g8 << 8) | (r8 << 16) | 0xFF000000);
          --count;
        }
      }
    }

    return true;
  }

  #endregion

  #region PArgb32 ↔ Rgba32 Conversions (Pre-multiplied Alpha)

  private static bool _ConvertPArgb32ToRgba32(PArgb8888BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (uint*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 4 pixels at a time
        while (count >= 4) {
          for (var i = 0; i < 4; ++i) {
            var p = srcRow[i];
            var a = (byte)(p >> 24);
            if (a == 0) {
              dstRow[i] = 0;
            } else if (a == 255) {
              dstRow[i] = p;
            } else {
              // Un-premultiply: original = premultiplied * 255 / alpha
              var b = (byte)p;
              var g = (byte)(p >> 8);
              var r = (byte)(p >> 16);
              dstRow[i] = (uint)((uint)((b * 255) / a) | ((uint)((g * 255) / a) << 8) | ((uint)((r * 255) / a) << 16) | ((uint)a << 24));
            }
          }
          srcRow += 4;
          dstRow += 4;
          count -= 4;
        }

        // Handle remaining pixels
        while (count > 0) {
          var p = *srcRow++;
          var a = (byte)(p >> 24);
          if (a == 0) {
            *dstRow++ = 0;
          } else if (a == 255) {
            *dstRow++ = p;
          } else {
            var b = (byte)p;
            var g = (byte)(p >> 8);
            var r = (byte)(p >> 16);
            *dstRow++ = (uint)((uint)((b * 255) / a) | ((uint)((g * 255) / a) << 8) | ((uint)((r * 255) / a) << 16) | ((uint)a << 24));
          }
          --count;
        }
      }
    }

    return true;
  }

  private static bool _ConvertRgba32ToPArgb32(Argb8888BitmapLocker src, PArgb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (uint*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 4 pixels at a time
        while (count >= 4) {
          for (var i = 0; i < 4; ++i) {
            var p = srcRow[i];
            var a = (byte)(p >> 24);
            if (a == 0) {
              dstRow[i] = 0;
            } else if (a == 255) {
              dstRow[i] = p;
            } else {
              // Premultiply: stored = original * alpha / 255
              var b = (byte)p;
              var g = (byte)(p >> 8);
              var r = (byte)(p >> 16);
              dstRow[i] = (uint)((uint)((b * a) / 255) | ((uint)((g * a) / 255) << 8) | ((uint)((r * a) / 255) << 16) | ((uint)a << 24));
            }
          }
          srcRow += 4;
          dstRow += 4;
          count -= 4;
        }

        // Handle remaining pixels
        while (count > 0) {
          var p = *srcRow++;
          var a = (byte)(p >> 24);
          if (a == 0) {
            *dstRow++ = 0;
          } else if (a == 255) {
            *dstRow++ = p;
          } else {
            var b = (byte)p;
            var g = (byte)(p >> 8);
            var r = (byte)(p >> 16);
            *dstRow++ = (uint)((uint)((b * a) / 255) | ((uint)((g * a) / 255) << 8) | ((uint)((r * a) / 255) << 16) | ((uint)a << 24));
          }
          --count;
        }
      }
    }

    return true;
  }

  #endregion

  #region High-Depth → Rgba32 Conversions

  private static bool _ConvertRgba64ToRgba32(Argb16161616BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (ulong*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 4 pixels at a time
        while (count >= 4) {
          for (var i = 0; i < 4; ++i) {
            var p = srcRow[i];
            // Extract 16-bit channels and scale to 8-bit (>> 8)
            var r = (byte)((p >> 8) & 0xFF);
            var g = (byte)((p >> 24) & 0xFF);
            var b = (byte)((p >> 40) & 0xFF);
            var a = (byte)((p >> 56) & 0xFF);
            dstRow[i] = (uint)b | ((uint)g << 8) | ((uint)r << 16) | ((uint)a << 24);
          }
          srcRow += 4;
          dstRow += 4;
          count -= 4;
        }

        // Handle remaining pixels
        while (count > 0) {
          var p = *srcRow++;
          var r = (byte)((p >> 8) & 0xFF);
          var g = (byte)((p >> 24) & 0xFF);
          var b = (byte)((p >> 40) & 0xFF);
          var a = (byte)((p >> 56) & 0xFF);
          *dstRow++ = (uint)b | ((uint)g << 8) | ((uint)r << 16) | ((uint)a << 24);
          --count;
        }
      }
    }

    return true;
  }

  private static bool _ConvertPRgba64ToRgba32(PArgb16161616BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStride = src.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (ulong*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = srcBase + y * srcStride;
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 2 pixels at a time (64-bit premultiplied needs division)
        while (count >= 2) {
          for (var i = 0; i < 2; ++i) {
            var p = srcRow[i];
            // Extract 16-bit channels
            var r16 = (ushort)(p & 0xFFFF);
            var g16 = (ushort)((p >> 16) & 0xFFFF);
            var b16 = (ushort)((p >> 32) & 0xFFFF);
            var a16 = (ushort)((p >> 48) & 0xFFFF);

            if (a16 == 0) {
              dstRow[i] = 0;
            } else {
              // Un-premultiply and scale to 8-bit
              var a8 = (byte)(a16 >> 8);
              if (a16 == 65535) {
                dstRow[i] = (uint)((b16 >> 8) | ((g16 >> 8) << 8) | ((r16 >> 8) << 16) | 0xFF000000u);
              } else {
                var r8 = (byte)((r16 * 255L) / a16);
                var g8 = (byte)((g16 * 255L) / a16);
                var b8 = (byte)((b16 * 255L) / a16);
                dstRow[i] = (uint)b8 | ((uint)g8 << 8) | ((uint)r8 << 16) | ((uint)a8 << 24);
              }
            }
          }
          srcRow += 2;
          dstRow += 2;
          count -= 2;
        }

        // Handle remaining pixel
        if (count > 0) {
          var p = *srcRow;
          var r16 = (ushort)(p & 0xFFFF);
          var g16 = (ushort)((p >> 16) & 0xFFFF);
          var b16 = (ushort)((p >> 32) & 0xFFFF);
          var a16 = (ushort)((p >> 48) & 0xFFFF);

          if (a16 == 0) {
            *dstRow = 0;
          } else {
            var a8 = (byte)(a16 >> 8);
            if (a16 == 65535) {
              *dstRow = (uint)((b16 >> 8) | ((g16 >> 8) << 8) | ((r16 >> 8) << 16) | 0xFF000000u);
            } else {
              var r8 = (byte)((r16 * 255L) / a16);
              var g8 = (byte)((g16 * 255L) / a16);
              var b8 = (byte)((b16 * 255L) / a16);
              *dstRow = (uint)b8 | ((uint)g8 << 8) | ((uint)r8 << 16) | ((uint)a8 << 24);
            }
          }
        }
      }
    }

    return true;
  }

  private static bool _ConvertRgb48ToRgba32(Rgb161616BitmapLocker src, Argb8888BitmapLocker dst) {
    var width = src.Width;
    var height = src.Height;
    var srcStrideBytes = src.BitmapData.Stride;
    var dstStride = dst.Stride;

    unsafe {
      var srcBase = (byte*)src.BitmapData.Scan0;
      var dstBase = (uint*)dst.BitmapData.Scan0;

      for (var y = 0; y < height; ++y) {
        var srcRow = (ushort*)(srcBase + y * srcStrideBytes);
        var dstRow = dstBase + y * dstStride;
        var count = width;

        // Process 4 pixels at a time (24 bytes → 16 bytes)
        while (count >= 4) {
          // Memory layout: [R16][G16][B16] per pixel
          dstRow[0] = ((uint)srcRow[2] >> 8) | (((uint)srcRow[1] >> 8) << 8) | (((uint)srcRow[0] >> 8) << 16) | 0xFF000000u;
          dstRow[1] = ((uint)srcRow[5] >> 8) | (((uint)srcRow[4] >> 8) << 8) | (((uint)srcRow[3] >> 8) << 16) | 0xFF000000u;
          dstRow[2] = ((uint)srcRow[8] >> 8) | (((uint)srcRow[7] >> 8) << 8) | (((uint)srcRow[6] >> 8) << 16) | 0xFF000000u;
          dstRow[3] = ((uint)srcRow[11] >> 8) | (((uint)srcRow[10] >> 8) << 8) | (((uint)srcRow[9] >> 8) << 16) | 0xFF000000u;

          srcRow += 12;
          dstRow += 4;
          count -= 4;
        }

        // Handle remaining 0-3 pixels
        while (count > 0) {
          // R16, G16, B16 → B8, G8, R8, A8(0xFF)
          *dstRow++ = ((uint)srcRow[2] >> 8) | (((uint)srcRow[1] >> 8) << 8) | (((uint)srcRow[0] >> 8) << 16) | 0xFF000000u;
          srcRow += 3;
          --count;
        }
      }
    }

    return true;
  }

  #endregion
}
