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

}
