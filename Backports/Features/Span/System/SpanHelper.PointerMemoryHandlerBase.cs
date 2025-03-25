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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace System;

internal static unsafe partial class SpanHelper {

  [StructLayout(LayoutKind.Explicit,Size = 64)]
  private struct Block64;

  /// <summary>
  ///   Provides a base class for handling memory using a pointer to a block of memory of type <typeparamref name="T" />.
  /// </summary>
  /// <typeparam name="T">The type of elements pointed to by the memory.</typeparam>
  /// <remarks>
  ///   This class is intended to be inherited by more specific memory handler implementations that operate on pointers.
  ///   It provides common functionality and the basic structure needed to work with unmanaged memory in a managed
  ///   environment,
  ///   adhering to the <see cref="IMemoryHandler{T}" /> interface.
  /// </remarks>
  public abstract class PointerMemoryHandlerBase<T>(T* pointer) : IMemoryHandler<T> {

    private const int BYTE_COPY_THRESHOLD_IN_ITEMS = 16;

    protected T* Pointer => pointer;

    private void CopyTo(PointerMemoryHandlerBase<T> other, int count) {
      switch (count) {
        case < 0:
          throw new ArgumentOutOfRangeException(nameof(count));
        case < BYTE_COPY_THRESHOLD_IN_ITEMS:
          CopyElements(pointer, other.Pointer, count);
          break;
        default:
          var elementSize = Marshal.SizeOf(typeof(T));
          var totalBytes = elementSize * count;
#if SUPPORTS_BUFFER_MEMORYCOPY
          Buffer.MemoryCopy(pointer, other.Pointer, totalBytes, totalBytes);
#else
          CopyBytes((byte*)pointer, (byte*)other.Pointer, totalBytes);
#endif
          break;
      }
      return;
      
      [SuppressMessage("ReSharper", "VariableHidesOuterVariable")]
      void CopyBytes(byte* source, byte* target, int totalBytes) {

        // Copy chunks of 64 * 8 = 512 bytes at a time
        for (; totalBytes >= 512; source += 512, target += 512, totalBytes -= 512) {
          *(Block64*)target     = *(Block64*)source;
          ((Block64*)target)[1] = ((Block64*)source)[1];
          ((Block64*)target)[2] = ((Block64*)source)[2];
          ((Block64*)target)[3] = ((Block64*)source)[3];
          ((Block64*)target)[4] = ((Block64*)source)[4];
          ((Block64*)target)[5] = ((Block64*)source)[5];
          ((Block64*)target)[6] = ((Block64*)source)[6];
          ((Block64*)target)[7] = ((Block64*)source)[7];
        }

        // Copy chunks of 64 bytes at a time
        for (; totalBytes >= 64; source += 64, target += 64, totalBytes -= 64)
          *(Block64*)target = *(Block64*)source;

        // Copy chunks of 8 bytes (ulong) at a time
        for (; totalBytes >= 8; source += 8, target += 8, totalBytes -= 8)
          *(long*)target = *(long*)source;

        // Copy remaining 4 bytes (uint) if possible
        if (totalBytes >= 4) {
          *(int*)target = *(int*)source;
          totalBytes -= 4;
          source += 4;
          target += 4;
        }

        // Copy remaining 2 bytes (ushort) if possible
        if (totalBytes >= 2) {
          *(short*)target = *(short*)source;
          totalBytes -= 2;
          source += 2;
          target += 2;
        }

        // Copy remaining byte if necessary
        if (totalBytes >= 1)
          *target = *source;
      }

      [SuppressMessage("ReSharper", "VariableHidesOuterVariable")]
      void CopyElements(T* source, T* target, int count) {
        for (;;)
          switch (count) {
            case 0: goto ElementsLeft0;
            case 1: goto ElementsLeft1;
            case 2: goto ElementsLeft2;
            case 3: goto ElementsLeft3;
            case 4: goto ElementsLeft4;
            case 5: goto ElementsLeft5;
            case 6: goto ElementsLeft6;
            case 7: goto ElementsLeft7;
            default:
              do {
                *target = *source;
                target[1] = source[1];
                target[2] = source[2];
                target[3] = source[3];
                target[4] = source[4];
                target[5] = source[5];
                target[6] = source[6];
                target[7] = source[7];
                source += 8;
                target += 8;
                count -= 8;
              } while (count >= 8);

              continue;
          }

        ElementsLeft7: target[6] = source[6];
        ElementsLeft6: target[5] = source[5];
        ElementsLeft5: target[4] = source[4];
        ElementsLeft4: target[3] = source[3];
        ElementsLeft3: target[2] = source[2];
        ElementsLeft2: target[1] = source[1];
        ElementsLeft1: target[0] = source[0];
        ElementsLeft0: ;
      }
    }

    /// <inheritdoc />
    public ref T this[int index] => ref this.Pointer[index];

    /// <inheritdoc />
    public void CopyTo(T[] target, int count) {
      for (var i = 0; i < count; ++i)
        target[i] = pointer[i];
    }

    /// <inheritdoc />
    public abstract IMemoryHandler<T> SliceFrom(int offset);

    /// <inheritdoc />
    public void CopyTo(IMemoryHandler<T> other, int length) {
      if (other is PointerMemoryHandlerBase<T> pointerBased)
        this.CopyTo(pointerBased, length);

      for (var i = 0; i < length; ++i)
        other[i] = this[i];
    }

    internal bool CompareAsBytesTo(PointerMemoryHandlerBase<T> otherHandler, int byteCount) {
      var thisPointer = (byte*)pointer;
      var otherPointer = (byte*)otherHandler.Pointer;

      // Compare in 64-byte chunks using ulong for maximum performance
      for (; byteCount >= 64; thisPointer += 64, otherPointer += 64, byteCount -= 64) {

        // Load all values from both memory areas first
        // This improves memory-level parallelism as CPU can execute multiple loads concurrently
        var t0 = LoadQWord(thisPointer, 0);
        var t1 = LoadQWord(thisPointer, 8);
        var t2 = LoadQWord(thisPointer, 16);
        var t3 = LoadQWord(thisPointer, 24);
        var t4 = LoadQWord(thisPointer, 32);
        var t5 = LoadQWord(thisPointer, 40);
        var t6 = LoadQWord(thisPointer, 48);
        var t7 = LoadQWord(thisPointer, 56);

        var o0 = LoadQWord(otherPointer, 0);
        var o1 = LoadQWord(otherPointer, 8);
        var o2 = LoadQWord(otherPointer, 16);
        var o3 = LoadQWord(otherPointer, 24);
        var o4 = LoadQWord(otherPointer, 32);
        var o5 = LoadQWord(otherPointer, 40);
        var o6 = LoadQWord(otherPointer, 48);
        var o7 = LoadQWord(otherPointer, 56);

        // Now perform XOR operations with loaded values
        Xor(ref t0, o0);
        Xor(ref t1, o1);
        Xor(ref t2, o2);
        Xor(ref t3, o3);
        Xor(ref t4, o4);
        Xor(ref t5, o5);
        Xor(ref t6, o6);
        Xor(ref t7, o7);

        // This improves memory-level parallelism as CPU can execute multiple ORs concurrently
        Or(ref t0, t1);
        Or(ref t2, t3);
        Or(ref t4, t5);
        Or(ref t6, t7);

        Or(ref t0, t2);
        Or(ref t4, t6);

        Or(ref t0, t4);

       // Combine results - will be non-zero if any comparison failed
        if (t0 != 0)
          return false;
      }

      // Accumulate differences for remaining bytes
      ulong result = 0;

      // Compare in 8-byte chunks using ulong
      for (; byteCount >= 8; thisPointer += 8, otherPointer += 8, byteCount -= 8)
        Or(ref result, LoadQWord(thisPointer) ^ LoadQWord(otherPointer));

      // Compare in 4-byte chunks using uint
      if (byteCount >= 4) {
        Or(ref result, LoadDWord(thisPointer) ^ LoadDWord(otherPointer));
        thisPointer += 4;
        otherPointer += 4;
        byteCount -= 4;
      }

      // Compare in 2-byte chunks using ushort
      if (byteCount >= 2) {
        Or(ref result, (ulong)LoadWord(thisPointer) ^ LoadWord(otherPointer));
        thisPointer += 2;
        otherPointer += 2;
        byteCount -= 2;
      }

      // Handle remaining byte, if any
      // ReSharper disable once InvertIf
      if (byteCount > 0)
        Or(ref result, (ulong)LoadByte(thisPointer) ^ LoadByte(otherPointer));

      return result == 0;

      ulong LoadQWord(byte* adress, int offset = 0) => *(ulong*)(adress + offset);
      uint LoadDWord(byte* adress, int offset = 0) => *(uint*)(adress + offset);
      ushort LoadWord(byte* adress, int offset = 0) => *(ushort*)(adress + offset);
      byte LoadByte(byte* adress, int offset = 0) => *(adress + offset);

      void Xor(ref ulong source, ulong operand) => source ^= operand;
      void Or(ref ulong source, ulong operand) => source |= operand;

    }
  }
}

#endif
