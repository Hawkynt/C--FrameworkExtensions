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
  
  public static void Clear(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    _FillWithBytes(@this, 0, @this.Length, 0);
  }

  public static void Clear(this ushort[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (ushort* pointer = &@this[0])
        _FillWords(pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

  public static void Clear(this short[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (short* pointer = &@this[0])
        _FillWords((ushort*)pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

  public static void Clear(this uint[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (uint* pointer = &@this[0])
        _FillDWords(pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillDWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

  public static void Clear(this int[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (int* pointer = &@this[0])
        _FillDWords((uint*)pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillDWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

  public static void Clear(this float[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (float* pointer = &@this[0])
        _FillDWords((uint*)pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillDWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

  public static void Clear(this ulong[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (ulong* pointer = &@this[0])
        _FillQWords(pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillQWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

  public static void Clear(this long[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (long* pointer = &@this[0])
        _FillQWords((ulong*)pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillQWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

  public static void Clear(this double[] @this) {
    Guard.Against.ThisIsNull(@this);

#if UNSAFE
    unsafe {
      fixed (double* pointer = &@this[0])
        _FillQWords((ulong*)pointer, @this.Length, 0);
    }
#else
    using var sourceFixedPointer = DisposableGCHandle.Pin(@this);
    var pointer = sourceFixedPointer.AddrOfPinnedObject();
    _FillQWordPointer(pointer, 0, @this.Length, 0);
#endif
  }

}
