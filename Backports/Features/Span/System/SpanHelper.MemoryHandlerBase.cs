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

#if !SUPPORTS_SPAN

using Guard;
using System.Runtime.CompilerServices;

namespace System;

partial class SpanHelper {
  /// <summary>
  ///   Defines a mechanism for handling memory buffers that contain elements of type <typeparamref name="T" />.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the memory buffer.</typeparam>
  /// <remarks>
  ///   This interface provides methods and properties to access and manipulate memory buffers in a type-safe manner.
  ///   It allows slicing memory segments, and copying contents to other memory handlers or arrays.
  /// </remarks>
  public abstract class MemoryHandlerBase<T> {

    /// <summary>
    ///   Gets the element at the specified index in the memory buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>A reference to the element at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public abstract ref T GetRef(int index);

    /// <summary>
    ///   Gets the element at the specified index in the memory buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public abstract T GetValue(int index);

    /// <summary>
    ///   Sets the element at the specified index in the memory buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <param name="value">The value to set</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public abstract void SetValue(int index, T value);

    /// <summary>
    /// Gets a pointer to the start of the buffer
    /// </summary>
    /// <returns></returns>
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public abstract unsafe T* Pointer { get; }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

    /// <summary>
    ///   Creates a slice of the current memory buffer starting at the specified offset.
    /// </summary>
    /// <param name="offset">The zero-based starting position of the slice.</param>
    /// <returns>A new <see cref="MemoryHandlerBase{T}" /> representing the slice of the original memory buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the offset is out of range.</exception>
    public abstract MemoryHandlerBase<T> SliceFrom(int offset);

    /// <summary>
    ///   Copies a specified number of elements to another <see cref="MemoryHandlerBase{T}" /> starting from the beginning.
    /// </summary>
    /// <param name="other">The target <see cref="MemoryHandlerBase{T}" /> to which elements are copied.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="other" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the length is greater than the size of either memory buffer.</exception>
    public unsafe void CopyTo(MemoryHandlerBase<T> other, int count) {
      switch (count) {
        case < 0:
          AlwaysThrow.ArgumentOutOfRangeException(nameof(count));
          return;
        case 0:
          return;
      }

      const int BYTE_COPY_THRESHOLD_IN_ITEMS = 16;
      if (count < BYTE_COPY_THRESHOLD_IN_ITEMS)
        CopyElements(other, count);
      else if (!IsValueType<T>())
        switch (this) {
          case ManagedArrayHandler<T> mahS when other is ManagedArrayHandler<T> mahT: {
            fixed (T* source = mahS.source)
            fixed (T* target = mahT.source) {
              var sourcePtr = source + mahS.start;
              var targetPtr = target + mahT.start;
              CopyPointerElements(sourcePtr, targetPtr, count);
            }
            break;
          }
          default:
            CopyElements(other, count);
            break;
        }
      else {
        var bytes = Unsafe.SizeOf<T>() * count;
#if SUPPORTS_BUFFER_MEMORYCOPY
        Buffer.MemoryCopy(this.Pointer, other.Pointer, bytes, bytes);
#else
        CopyBytes((byte*)this.Pointer, (byte*)other.Pointer, bytes);
#endif
      }

      return;

      static void CopyBytes(byte* source, byte* target, int totalBytes) {

        // Copy chunks of 64 * 8 = 512 bytes at a time
        for (; totalBytes >= 512; source += 512, target += 512, totalBytes -= 512) {
          *(Block64*)target = *(Block64*)source;
          ((Block64*)target)[1] = ((Block64*)source)[1];
          ((Block64*)target)[2] = ((Block64*)source)[2];
          ((Block64*)target)[3] = ((Block64*)source)[3];
          ((Block64*)target)[4] = ((Block64*)source)[4];
          ((Block64*)target)[5] = ((Block64*)source)[5];
          ((Block64*)target)[6] = ((Block64*)source)[6];
          ((Block64*)target)[7] = ((Block64*)source)[7];
        }

        // count is < 512 from here on
        var iterations64 = totalBytes / 64;
        switch (iterations64) {
          case 0:
            goto CopyLessThan64;
          case 1:
            goto Copy64;
          case 2:
            goto Copy128;
          case 3:
            goto Copy192;
          case 4:
            goto Copy256;
          case 5:
            goto Copy320;
          case 6:
            goto Copy384;
          case 7:
            goto Copy448;
          default:
            goto CopyLessThan64; // Avoid compiler warning and trigger optimization - Never gonna get here
        }

Copy448:
        ((Block64*)target)[6] = ((Block64*)source)[6];
Copy384:
        ((Block64*)target)[5] = ((Block64*)source)[5];
Copy320:
        ((Block64*)target)[4] = ((Block64*)source)[4];
Copy256:
        ((Block64*)target)[3] = ((Block64*)source)[3];
Copy192:
        ((Block64*)target)[2] = ((Block64*)source)[2];
Copy128:
        ((Block64*)target)[1] = ((Block64*)source)[1];
Copy64:
        *(Block64*)target = *(Block64*)source;

        iterations64 *= 64;
        source += iterations64;
        target += iterations64;
        totalBytes -= iterations64;

CopyLessThan64:
// count is < 64 from here on
        switch (totalBytes) {
          case 0:
            goto CopyDone;
          case 1:
            goto Copy1;
          case 2:
            goto Copy2;
          case 3:
            goto Copy3;
          case 4:
            goto Copy4;
          case 5:
            goto Copy5;
          case 6:
            goto Copy6;
          case 7:
            goto Copy7;
          case 8:
            goto Copy8;
          case 9:
            goto Copy9;
          case 10:
            goto Copy10;
          case 11:
            goto Copy11;
          case 12:
            goto Copy12;
          case 13:
            goto Copy13;
          case 14:
            goto Copy14;
          case 15:
            goto Copy15;
          case 16:
            goto Copy16;
          case 17:
            goto Copy17;
          case 18:
            goto Copy18;
          case 19:
            goto Copy19;
          case 20:
            goto Copy20;
          case 21:
            goto Copy21;
          case 22:
            goto Copy22;
          case 23:
            goto Copy23;
          case 24:
            goto Copy24;
          case 25:
            goto Copy25;
          case 26:
            goto Copy26;
          case 27:
            goto Copy27;
          case 28:
            goto Copy28;
          case 29:
            goto Copy29;
          case 30:
            goto Copy30;
          case 31:
            goto Copy31;
          case 32:
            goto Copy32;
          case 33:
            goto Copy33;
          case 34:
            goto Copy34;
          case 35:
            goto Copy35;
          case 36:
            goto Copy36;
          case 37:
            goto Copy37;
          case 38:
            goto Copy38;
          case 39:
            goto Copy39;
          case 40:
            goto Copy40;
          case 41:
            goto Copy41;
          case 42:
            goto Copy42;
          case 43:
            goto Copy43;
          case 44:
            goto Copy44;
          case 45:
            goto Copy45;
          case 46:
            goto Copy46;
          case 47:
            goto Copy47;
          case 48:
            goto Copy48;
          case 49:
            goto Copy49;
          case 50:
            goto Copy50;
          case 51:
            goto Copy51;
          case 52:
            goto Copy52;
          case 53:
            goto Copy53;
          case 54:
            goto Copy54;
          case 55:
            goto Copy55;
          case 56:
            goto Copy56;
          case 57:
            goto Copy57;
          case 58:
            goto Copy58;
          case 59:
            goto Copy59;
          case 60:
            goto Copy60;
          case 61:
            goto Copy61;
          case 62:
            goto Copy62;
          case 63:
            goto Copy63;
          default:
            goto CopyDone;
        }

Copy63:
        target[62] = source[62];
Copy62:
        *(short*)(target + 60) = *(short*)(source + 60);
        goto Copy60;
Copy61:
        target[60] = source[60];
Copy60:
        *(int*)(target + 56) = *(int*)(source + 56);
        goto Copy56;
Copy59:
        target[58] = source[58];
Copy58:
        *(short*)(target + 56) = *(short*)(source + 56);
        goto Copy56;
Copy57:
        target[56] = source[56];
Copy56:
        *(long*)(target + 48) = *(long*)(source + 48);
        goto Copy48;
Copy55:
        target[54] = source[54];
Copy54:
        *(short*)(target + 52) = *(short*)(source + 52);
        goto Copy52;
Copy53:
        target[52] = source[52];
Copy52:
        *(int*)(target + 48) = *(int*)(source + 48);
        goto Copy48;
Copy51:
        target[50] = source[50];
Copy50:
        *(short*)(target + 48) = *(short*)(source + 48);
        goto Copy48;
Copy49:
        target[48] = source[48];
Copy48:
        *(Block16*)(target + 32) = *(Block16*)(source + 32);
        goto Copy32;
Copy47:
        target[46] = source[46];
Copy46:
        *(short*)(target + 44) = *(short*)(source + 44);
        goto Copy44;
Copy45:
        target[44] = source[44];
Copy44:
        *(int*)(target + 40) = *(int*)(source + 40);
        goto Copy40;
Copy43:
        target[42] = source[42];
Copy42:
        *(short*)(target + 40) = *(short*)(source + 40);
        goto Copy40;
Copy41:
        target[40] = source[40];
Copy40:
        *(long*)(target + 32) = *(long*)(source + 32);
        goto Copy32;
Copy39:
        target[38] = source[38];
Copy38:
        *(short*)(target + 36) = *(short*)(source + 36);
        goto Copy36;
Copy37:
        target[36] = source[36];
Copy36:
        *(int*)(target + 32) = *(int*)(source + 32);
        goto Copy32;
Copy35:
        target[34] = source[34];
Copy34:
        *(short*)(target + 32) = *(short*)(source + 32);
        goto Copy32;
Copy33:
        target[32] = source[32];
Copy32:
        *(Block32*)target = *(Block32*)source;
        goto CopyDone;
Copy31:
        target[30] = source[30];
Copy30:
        *(short*)(target + 28) = *(short*)(source + 28);
        goto Copy28;
Copy29:
        target[28] = source[28];
Copy28:
        *(int*)(target + 24) = *(int*)(source + 24);
        goto Copy24;
Copy27:
        target[26] = source[26];
Copy26:
        *(short*)(target + 24) = *(short*)(source + 24);
        goto Copy24;
Copy25:
        target[24] = source[24];
Copy24:
        *(long*)(target + 16) = *(long*)(source + 16);
        goto Copy16;
Copy23:
        target[22] = source[22];
Copy22:
        *(short*)(target + 20) = *(short*)(source + 20);
        goto Copy20;
Copy21:
        target[20] = source[20];
Copy20:
        *(int*)(target + 16) = *(int*)(source + 16);
        goto Copy16;
Copy19:
        target[18] = source[18];
Copy18:
        *(short*)(target + 16) = *(short*)(source + 16);
        goto Copy16;
Copy17:
        target[16] = source[16];
Copy16:
        *(Block16*)target = *(Block16*)source;
        goto CopyDone;
Copy15:
        target[14] = source[14];
Copy14:
        *(short*)(target + 12) = *(short*)(source + 12);
        goto Copy12;
Copy13:
        target[12] = source[12];
Copy12:
        *(int*)(target + 8) = *(int*)(source + 8);
        goto Copy8;
Copy11:
        target[10] = source[10];
Copy10:
        *(short*)(target + 8) = *(short*)(source + 8);
        goto Copy8;
Copy9:
        target[8] = source[8];
Copy8:
        *(long*)target = *(long*)source;
        goto CopyDone;
Copy7:
        target[6] = source[6];
Copy6:
        *(short*)(target + 4) = *(short*)(source + 4);
        goto Copy4;
Copy5:
        target[4] = source[4];
Copy4:
        *(int*)target = *(int*)source;
        goto CopyDone;
Copy3:
        target[2] = source[2];
Copy2:
        *(short*)target = *(short*)source;
        goto CopyDone;
Copy1:
        *target = *source;
CopyDone:
        ;
      }

      static void CopyPointerElements(T* source, T* target, int elements) {
        // Calculate iterations for chunks of 8 with bit trick (length / 8)
        var iterations = elements >> 3;
        
        // Check remainder using bit trick (length % 8)
        switch (elements & 7) {
          case 0:
            goto Copy0or8;
          case 7:
            goto Copy7;
          case 6:
            goto Copy6;
          case 5:
            goto Copy5;
          case 4:
            goto Copy4;
          case 3:
            goto Copy3;
          case 2:
            goto Copy2;
          case 1:
            goto Copy1;
          default:
            goto CopyDone; // Avoid compiler warning and trigger optimization - Never gonna get here
        }

        Copy0or8:
        if (iterations-- <= 0)
          goto CopyDone;

        *target++ = *source++;
        Copy7: *target++ = *source++;
        Copy6: *target++ = *source++;
        Copy5: *target++ = *source++;
        Copy4: *target++ = *source++;
        Copy3: *target++ = *source++;
        Copy2: *target++ = *source++;
        Copy1: *target++ = *source++;
        goto Copy0or8;
        CopyDone:
        ;
      }

      void CopyElements(MemoryHandlerBase<T> target, int elements) {

        // Call overhead would ruin any nice tricks we could pull
        for (var i = 0; i < elements; ++i)
          target.SetValue(i, this.GetValue(i));
      }

    }

  /// <summary>
  ///   Copies a specified number of elements from the memory buffer to a target array starting at the array's beginning.
  /// </summary>
  /// <param name="target">The target array to which elements are copied.</param>
  /// <param name="count">The number of elements to copy to the target array.</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="target" /> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  ///   Thrown if the count is greater than the size of the memory buffer or the
  ///   target array.
  /// </exception>
  public void CopyTo(T[] target, int count) => this.CopyTo(new ManagedArrayHandler<T>(target,0),count);

   }

}

#endif
