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

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

namespace System;

unsafe partial class SpanHelper {
  /// <summary>
  ///   Represents a memory handler for unmanaged memory blocks, providing access to elements of type
  ///   <typeparamref name="T" /> using a pointer.
  /// </summary>
  /// <typeparam name="T">The type of elements stored in the unmanaged memory block.</typeparam>
  /// <remarks>
  ///   This class enables managed code to read from and write to a block of unmanaged memory. It is crucial to ensure that
  ///   the lifecycle of the memory block
  ///   is properly managed to avoid memory leaks or accessing invalid memory locations.
  /// </remarks>
#pragma warning disable CS8500
  public sealed class UnmanagedPointerMemoryHandler<T>(T* pointer):MemoryHandlerBase<T> {
    
    internal bool CompareAsBytesTo(MemoryHandlerBase<T> otherHandler, int byteCount) {
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

      // Handle blocks of 8 bytes with efficient labels instead of cascading switch
      switch (byteCount >> 3) {
        case 0:
          goto ProcessRemainder;
        case 1:
          goto Process8Plus;
        case 2:
          goto Process16Plus;
        case 3:
          goto Process24Plus;
        case 4:
          goto Process32Plus;
        case 5:
          goto Process40Plus;
        case 6:
          goto Process48Plus;
        default:
          goto Process56Plus;
      }

// Process byte blocks with true fall-through logic (no goto in assembly)
Process56Plus:
      result |= LoadQWord(thisPointer, 48) ^ LoadQWord(otherPointer, 48);
Process48Plus:
      result |= LoadQWord(thisPointer, 40) ^ LoadQWord(otherPointer, 40);
Process40Plus:
      result |= LoadQWord(thisPointer, 32) ^ LoadQWord(otherPointer, 32);
Process32Plus:
      result |= LoadQWord(thisPointer, 24) ^ LoadQWord(otherPointer, 24);
Process24Plus:
      result |= LoadQWord(thisPointer, 16) ^ LoadQWord(otherPointer, 16);
Process16Plus:
      result |= LoadQWord(thisPointer, 8) ^ LoadQWord(otherPointer, 8);
Process8Plus:
      result |= LoadQWord(thisPointer) ^ LoadQWord(otherPointer);

      var processedBytes = byteCount & ~0x7;
      thisPointer += processedBytes;
      otherPointer += processedBytes;
      byteCount &= 0x7;

ProcessRemainder:
      switch (byteCount) {
        case 0:
          goto ProcessDone;
        case 1:
          goto Process1Byte;
        case 2:
          goto Process2Bytes;
        case 3:
          goto Process3Bytes;
        case 4:
          goto Process4Bytes;
        case 5:
          goto Process5Bytes;
        case 6:
          goto Process6Bytes;
        case 7:
          goto Process7Bytes;
      }

Process7Bytes:
      result |= (uint)(LoadByte(thisPointer, 6) ^ LoadByte(otherPointer, 6));
Process6Bytes:
      result |= (uint)(LoadWord(thisPointer, 4) ^ LoadWord(otherPointer, 4));
      goto Process4Bytes;
Process5Bytes:
      result |= (uint)(LoadByte(thisPointer, 4) ^ LoadByte(otherPointer, 4));
Process4Bytes:
      result |= LoadDWord(thisPointer) ^ LoadDWord(otherPointer);
      goto ProcessDone;
Process3Bytes:
      result |= (uint)(LoadByte(thisPointer, 2) ^ LoadByte(otherPointer, 2));
Process2Bytes:
      result |= (uint)(LoadWord(thisPointer) ^ LoadWord(otherPointer));
      goto ProcessDone;
Process1Byte:
      result |= (uint)(LoadByte(thisPointer) ^ LoadByte(otherPointer));
ProcessDone:

      return result == 0;

      ulong LoadQWord(byte* adress, int offset = 0) => *(ulong*)(adress + offset);
      uint LoadDWord(byte* adress, int offset = 0) => *(uint*)(adress + offset);
      ushort LoadWord(byte* adress, int offset = 0) => *(ushort*)(adress + offset);
      byte LoadByte(byte* adress, int offset = 0) => *(adress + offset);

      void Xor(ref ulong source, ulong operand) => source ^= operand;
      void Or(ref ulong source, ulong operand) => source |= operand;

    }

    #region Overrides of IMemoryHandler<T>

    /// <inheritdoc />
    public override ref T GetRef(int index) => ref *(pointer + index);

    /// <inheritdoc />
    public override T GetValue(int index) => pointer[index];

    /// <inheritdoc />
    public override void SetValue(int index, T value) => pointer[index] = value;

    /// <inheritdoc />
    public override T* Pointer => pointer;

    /// <inheritdoc />
    public override MemoryHandlerBase<T> SliceFrom(int offset) => new UnmanagedPointerMemoryHandler<T>(pointer + offset);

    #endregion
  }
}

#endif
