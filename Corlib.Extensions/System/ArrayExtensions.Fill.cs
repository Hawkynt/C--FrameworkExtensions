#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#if !UNSAFE
using System.Runtime.InteropServices;
#endif

namespace System;

static partial class ArrayExtensions {

  public static void Fill(this byte[] @this, byte value) {
    Guard.Against.ThisIsNull(@this);

    _FillWithBytes(@this, 0, @this.Length, value);
  }

  public static void Fill(this byte[] @this, byte value, int offset) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.IndexOutOfRange(offset, @this.Length);

    _FillWithBytes(@this, offset, @this.Length - offset, value);
  }

  public static void Fill(this byte[] @this, byte value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.IndexOutOfRange(offset, @this.Length);
    Guard.Against.CountOutOfRange(count, offset + count, @this.Length);
    
    _FillWithBytes(@this, offset, count, value);
  }

  public static void Fill(this byte[] @this, ushort value, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountOutOfRange(count, count << 1, @this.Length);
    
    _FillWithWords(@this, 0, count, value);
  }

  public static void Fill(this byte[] @this, ushort value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.IndexOutOfRange(offset, @this.Length);
    Guard.Against.CountOutOfRange(count, (offset + count) << 1, @this.Length);
    
    _FillWithWords(@this, offset, count, value);
  }

  public static void Fill(this byte[] @this, uint value, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountOutOfRange(count, count << 2, @this.Length);
    
    _FillWithDWords(@this, 0, count, value);
  }

  public static void Fill(this byte[] @this, uint value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.IndexOutOfRange(offset, @this.Length);
    Guard.Against.CountOutOfRange(count, (offset + count) << 2, @this.Length);
    
    _FillWithDWords(@this, offset, count, value);
  }

  public static void Fill(this byte[] @this, ulong value, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountOutOfRange(count, count << 3, @this.Length);

    _FillWithQWords(@this, 0, count, value);
  }

  public static void Fill(this byte[] @this, ulong value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.IndexOutOfRange(offset, @this.Length);
    Guard.Against.CountOutOfRange(count, (offset + count) << 3, @this.Length);
    
    _FillWithQWords(@this, offset, count, value);
  }

  public static void Fill(this IntPtr @this, byte value, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);
    
    _FillBytePointer(@this, 0, count, value);
  }

  public static void Fill(this IntPtr @this, byte value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);

    _FillBytePointer(@this, offset, count, value);
  }

  public static void Fill(this IntPtr @this, ushort value, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);

    _FillWordPointer(@this, 0, count, value);
  }

  public static void Fill(this IntPtr @this, ushort value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);

    _FillWordPointer(@this, offset, count, value);
  }

  public static void Fill(this IntPtr @this, uint value, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);

    _FillDWordPointer(@this, 0, count, value);
  }

  public static void Fill(this IntPtr @this, uint value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);

    _FillDWordPointer(@this, offset, count, value);
  }

  public static void Fill(this IntPtr @this, ulong value, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);

    _FillQWordPointer(@this, 0, count, value);
  }

  public static void Fill(this IntPtr @this, ulong value, int offset, int count) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(count);

    _FillQWordPointer(@this, offset, count, value);
  }

#if UNSAFE

  private static unsafe void _FillWithBytes(byte[] source, int offset, int count, byte value) {
    fixed (byte* pointer = &source[offset])
      _FillBytes(pointer, count, value);
  }

  private static unsafe void _FillWithWords(byte[] source, int offset, int count, ushort value) {
    fixed (byte* pointer = &source[offset << 1])
      _FillWords((ushort*)pointer, count, value);
  }

  private static unsafe void _FillWithDWords(byte[] source, int offset, int count, uint value) {
    fixed (byte* pointer = &source[offset << 2])
      _FillDWords((uint*)pointer, count, value);
  }

  private static unsafe void _FillWithQWords(byte[] source, int offset, int count, ulong value) {
    fixed (byte* pointer = &source[offset << 3])
      _FillQWords((ulong*)pointer, count, value);
  }

  private static unsafe void _FillBytePointer(IntPtr source, int offset, int count, byte value) => _FillBytes((byte*)source.ToPointer() + offset, count, value);
  private static unsafe void _FillWordPointer(IntPtr source, int offset, int count, ushort value) => _FillWords((ushort*)source.ToPointer() + offset, count, value);
  private static unsafe void _FillDWordPointer(IntPtr source, int offset, int count, uint value) => _FillDWords((uint*)source.ToPointer() + offset, count, value);
  private static unsafe void _FillQWordPointer(IntPtr source, int offset, int count, ulong value) => _FillQWords((ulong*)source.ToPointer() + offset, count, value);

  private static unsafe void _FillBytes(byte* source, int count, byte value) {
    if (count >= 64) {
      var localCount = count >> 6;
      var localSource = (Block64*)source;

      _Fill64ByteBlocks(ref localSource, localCount, new(value));
      count &= 0b111111;
      source = (byte*)localSource;
    }

    while (count >= 8) {
      *source = value;
      source[1] = value;
      source[2] = value;
      source[3] = value;
      source[4] = value;
      source[5] = value;
      source[6] = value;
      source[7] = value;
      source += 8;
      count -= 8;
    }

    while (--count >= 0)
      *source++ = value;
  }

  private static unsafe void _FillWords(ushort* source, int count, ushort value) {
    if (count >= 64) {
      var localCount = count >> 5;
      var localSource = (Block64*)source;
      _Fill64ByteBlocks(ref localSource, localCount, new(value));
      count &= 0b11111;
      source = (ushort*)localSource;
    }

    while (count >= 8) {
      *source = value;
      source[1] = value;
      source[2] = value;
      source[3] = value;
      source[4] = value;
      source[5] = value;
      source[6] = value;
      source[7] = value;
      source += 8;
      count -= 8;
    }

    while (--count >= 0)
      *source++ = value;
  }

  private static unsafe void _FillDWords(uint* source, int count, uint value) {
    if (count >= 64) {
      var localCount = count >> 4;
      var localSource = (Block64*)source;
      _Fill64ByteBlocks(ref localSource, localCount, new(value));
      count &= 0b1111;
      source = (uint*)localSource;
    }

    while (count >= 8) {
      *source = value;
      source[1] = value;
      source[2] = value;
      source[3] = value;
      source[4] = value;
      source[5] = value;
      source[6] = value;
      source[7] = value;
      source += 8;
      count -= 8;
    }

    while (--count >= 0)
      *source++ = value;
  }

  private static unsafe void _FillQWords(ulong* source, int count, ulong value) {
    if (count >= 64) {
      var localCount = count >> 3;
      var localSource = (Block64*)source;
      _Fill64ByteBlocks(ref localSource, localCount, new(value));
      count &= 0b111;
      source = (ulong*)localSource;
    }

    while (count >= 8) {
      *source = value;
      source[1] = value;
      source[2] = value;
      source[3] = value;
      source[4] = value;
      source[5] = value;
      source[6] = value;
      source[7] = value;
      source += 8;
      count -= 8;
    }

    while (--count >= 0)
      *source++ = value;
  }

  private static unsafe void _Fill64ByteBlocks(ref Block64* source, int count, Block64 value) {
    while (count >= 16) {
      *source = value;
      source[1] = value;
      source[2] = value;
      source[3] = value;
      source[4] = value;
      source[5] = value;
      source[6] = value;
      source[7] = value;
      source[8] = value;
      source[9] = value;
      source[10] = value;
      source[11] = value;
      source[12] = value;
      source[13] = value;
      source[14] = value;
      source[15] = value;
      source += 16;
      count -= 16;
    }

    if (count >= 8) {
      *source = value;
      source[1] = value;
      source[2] = value;
      source[3] = value;
      source[4] = value;
      source[5] = value;
      source[6] = value;
      source[7] = value;
      source += 8;
      count -= 8;
    }

    while (--count >= 0)
      *source++ = value;
  }

#else // Managed stuff

    private static void _FillWithBytes(byte[] source, int offset, int count, byte value) {
      using var sourceFixedPointer = DisposableGCHandle.Pin(source);
      _FillBytePointer(sourceFixedPointer.AddrOfPinnedObject(), offset, count, value);
    }

    private static void _FillWithWords(byte[] source, int offset, int count, ushort value) {
      using var sourceFixedPointer = DisposableGCHandle.Pin(source);
      _FillWordPointer(sourceFixedPointer.AddrOfPinnedObject(), offset, count, value);
    }

    private static void _FillWithDWords(byte[] source, int offset, int count, uint value) {
      using var sourceFixedPointer = DisposableGCHandle.Pin(source);
      _FillDWordPointer(sourceFixedPointer.AddrOfPinnedObject(), offset, count, value);
    }

    private static void _FillWithQWords(byte[] source, int offset, int count, ulong value) {
      offset <<= 3;
      using var sourceFixedPointer = DisposableGCHandle.Pin(source);
      _FillWithQWords(sourceFixedPointer.AddrOfPinnedObject(), ref offset, count, value);
    }

    private static void _FillBytePointer(IntPtr source, int offset, int count, byte value) {
      if (count >= 8) {
        var localCount = count >> 3;
        _FillWithQWords(source, ref offset, localCount, (ulong)value << 56 | (ulong)value << 48 | (ulong)value << 40 | (ulong)value << 32 | (ulong)value << 24 | (ulong)value << 16 | (ulong)value << 8 | value);
        count &= 0b111;
      }

      while (--count >= 0)
        Marshal.WriteByte(source, offset++, value);
    }

    private static void _FillWordPointer(IntPtr source, int offset, int count, ushort value) {
      offset = offset << 1;

      if (count >= 4) {
        var localCount = count >> 2;
        _FillWithQWords(source, ref offset, localCount, (ulong)value << 32 | (ulong)value << 24 | (ulong)value << 16 | value);
        count &= 0b11;
      }

      while (--count >= 0) {
        Marshal.WriteInt16(source, offset, (short)value);
        offset += 2;
      }
    }

    private static void _FillDWordPointer(IntPtr source, int offset, int count, uint value) {
      offset = offset << 2;

      if (count >= 2) {
        var localCount = count >> 1;
        _FillWithQWords(source, ref offset, localCount, (ulong)value << 32 | value);
        count &= 0b1;
      }

      while (--count >= 0) {
        Marshal.WriteInt32(source, offset, (int)value);
        offset += 4;
      }
    }

    private static void _FillQWordPointer(IntPtr source, int offset, int count, ulong value) {
      offset <<= 3;
      _FillWithQWords(source, ref offset, count, value);
    }

#if !SUPPORTS_POINTER_ARITHMETIC
    private static void _Add(ref IntPtr src, int count) => src=new IntPtr(src.ToInt64() + count);
#endif

    private static void _FillWithQWords(IntPtr source, ref int offset, int count, ulong value) {
      var v = (long)value;
#if SUPPORTS_POINTER_ARITHMETIC
      source += offset;
#else
      _Add(ref source,offset);
#endif
      offset += count << 3;

      if (count >= 64) {
        Marshal.WriteInt64(source, 0, v);
        Marshal.WriteInt64(source, 8, v);
        Marshal.WriteInt64(source, 16, v);
        Marshal.WriteInt64(source, 24, v);
        Marshal.WriteInt64(source, 32, v);
        Marshal.WriteInt64(source, 40, v);
        Marshal.WriteInt64(source, 48, v);
        Marshal.WriteInt64(source, 56, v);

        var sizeInBytes = 64;
        var start = source;
#if SUPPORTS_POINTER_ARITHMETIC
        source += sizeInBytes;
#else
        _Add(ref source, sizeInBytes);
#endif
        count -= 8;

        var countInBytes = count << 3;
        while (countInBytes > sizeInBytes) {
          _CopyTo(start, 0, source, 0, sizeInBytes);
#if SUPPORTS_POINTER_ARITHMETIC
          source += sizeInBytes;
#else
          _Add(ref source, sizeInBytes);
#endif
          countInBytes -= sizeInBytes;
          sizeInBytes <<= 1;
        }
        _CopyTo(start, 0, source, 0, countInBytes);
        return;
      }

      while (count >= 8) {
        Marshal.WriteInt64(source, 0, v);
        Marshal.WriteInt64(source, 8, v);
        Marshal.WriteInt64(source, 16, v);
        Marshal.WriteInt64(source, 24, v);
        Marshal.WriteInt64(source, 32, v);
        Marshal.WriteInt64(source, 40, v);
        Marshal.WriteInt64(source, 48, v);
        Marshal.WriteInt64(source, 56, v);
#if SUPPORTS_POINTER_ARITHMETIC
        source += 64;
#else
        _Add(ref source, 64);
#endif
        count -= 8;
      }

      while (count > 0) {
        Marshal.WriteInt64(source, 0, v);
#if SUPPORTS_POINTER_ARITHMETIC
        source += 8;
#else
        _Add(ref source, 8);
#endif
        --count;
      }
    }

#endif

}
