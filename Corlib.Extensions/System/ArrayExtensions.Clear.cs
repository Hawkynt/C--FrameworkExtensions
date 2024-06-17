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

using Guard;

namespace System;

static partial class ArrayExtensions {
  public static void Clear(this byte[] @this) {
    Against.ThisIsNull(@this);

    _FillWithBytes(@this, 0, @this.Length, 0);
  }

  public static unsafe void Clear(this ushort[] @this) {
    Against.ThisIsNull(@this);

    fixed (ushort* pointer = &@this[0])
      _FillWords(pointer, @this.Length, 0);
  }

  public static unsafe void Clear(this short[] @this) {
    Against.ThisIsNull(@this);

    fixed (short* pointer = &@this[0])
      _FillWords((ushort*)pointer, @this.Length, 0);
  }

  public static unsafe void Clear(this uint[] @this) {
    Against.ThisIsNull(@this);

    fixed (uint* pointer = &@this[0])
      _FillDWords(pointer, @this.Length, 0);
  }

  public static unsafe void Clear(this int[] @this) {
    Against.ThisIsNull(@this);

    fixed (int* pointer = &@this[0])
      _FillDWords((uint*)pointer, @this.Length, 0);
  }

  public static unsafe void Clear(this float[] @this) {
    Against.ThisIsNull(@this);

    fixed (float* pointer = &@this[0])
      _FillDWords((uint*)pointer, @this.Length, 0);
  }

  public static unsafe void Clear(this ulong[] @this) {
    Against.ThisIsNull(@this);

    fixed (ulong* pointer = &@this[0])
      _FillQWords(pointer, @this.Length, 0);
  }

  public static unsafe void Clear(this long[] @this) {
    Against.ThisIsNull(@this);

    fixed (long* pointer = &@this[0])
      _FillQWords((ulong*)pointer, @this.Length, 0);
  }

  public static unsafe void Clear(this double[] @this) {
    Against.ThisIsNull(@this);

    fixed (double* pointer = &@this[0])
      _FillQWords((ulong*)pointer, @this.Length, 0);
  }
}
