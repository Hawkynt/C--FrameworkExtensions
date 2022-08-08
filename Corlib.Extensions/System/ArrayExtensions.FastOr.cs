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

#if NET45_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_INLINING
#endif
#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#define SUPPORTS_POINTER_ARITHMETIC
#define SUPPORTS_ASYNC
#endif

#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif

namespace System; 

static partial class ArrayExtensions {
  private static class FastOr {
    private static void _DoBytes(byte[] source, byte[] operand, int offset, int length) {
      var end = offset + length;
      for (var i = offset; i < end; ++i)
        source[i] |= operand[i];
    }

    private static void _DoWords(ushort[] source, ushort[] operand) {
#if SUPPORTS_ASYNC
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
      for (var i = 0; i < source.Length; i++)
        source[i] |= operand[i];
#if SUPPORTS_ASYNC
          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] |= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
#endif
    }

    private static void _DoDWords(uint[] source, uint[] operand) {
#if SUPPORTS_ASYNC
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
      for (var i = 0; i < source.Length; i++)
        source[i] |= operand[i];
#if SUPPORTS_ASYNC
          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] |= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
#endif
    }

    private static void _DoQWords(ulong[] source, ulong[] operand) {

#if SUPPORTS_ASYNC
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
      for (var i = 0; i < source.Length; i++)
        source[i] |= operand[i];
#if SUPPORTS_ASYNC
          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] |= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
#endif
    }

    public static void ProcessBytewise(byte[] source, byte[] operand) {
      _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
    }

#if UNSAFE
    public static unsafe void ProcessInUnsafeChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
      if (maxChunkSize < 1)
        maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

      if (maxChunkSize < 2) {
        _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
        return;
      }

      var bytesLeft = Math.Min(source.Length, operand.Length);
      var offset = 0;

      fixed (byte* srcPointer = source, opPointer = operand) {

        if (RuntimeConfiguration.Has64BitRegisters) {
          var sourcePtr = (ulong*)(srcPointer + offset);
          var operandPtr = (ulong*)(opPointer + offset);
          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {
            *sourcePtr |= *operandPtr;
            sourcePtr++;
            operandPtr++;
            bytesLeft -= 8;
            offset += 8;
          }
        }
        if (RuntimeConfiguration.Has32BitRegisters) {
          var sourcePtr = (uint*)(srcPointer + offset);
          var operandPtr = (uint*)(opPointer + offset);
          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {
            *sourcePtr |= *operandPtr;
            sourcePtr++;
            operandPtr++;
            bytesLeft -= 4;
            offset += 4;
          }
        }
        if (RuntimeConfiguration.Has16BitRegisters) {
          var sourcePtr = (ushort*)(srcPointer + offset);
          var operandPtr = (ushort*)(opPointer + offset);
          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {
            *sourcePtr |= *operandPtr;
            sourcePtr++;
            operandPtr++;
            bytesLeft -= 2;
            offset += 2;
          }
        }
        {
          var sourcePtr = srcPointer + offset;
          var operandPtr = opPointer + offset;
          while (bytesLeft > 0) {
            *sourcePtr |= *operandPtr;
            sourcePtr++;
            operandPtr++;
            bytesLeft--;
            offset++;
          }
        }
      }
    }
#endif

    public static void ProcessInChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
      if (maxChunkSize < 1)
        maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

      if (maxChunkSize < 2) {
        _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
        return;
      }

      var bytesLeft = Math.Min(source.Length, operand.Length);
      var offset = 0;

      // long part
      if (RuntimeConfiguration.Has64BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_QWORD) {

        var chunk = new ulong[maxChunkSize >> 3];
        var secondChunk = new ulong[maxChunkSize >> 3];

        while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {

          var chunkLength = Math.Min(bytesLeft, maxChunkSize);
          var itemCount = chunkLength >> 3;
          chunkLength = itemCount << 3;

          Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
          Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
          _DoQWords(chunk, secondChunk);
          Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

          bytesLeft -= chunkLength;
          offset += chunkLength;
        }

      }

      // int part
      if (RuntimeConfiguration.Has32BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_DWORD) {
        var chunk = new uint[maxChunkSize >> 2];
        var secondChunk = new uint[maxChunkSize >> 2];

        while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {

          var chunkLength = Math.Min(bytesLeft, maxChunkSize);
          var itemCount = chunkLength >> 2;
          chunkLength = itemCount << 2;


          Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
          Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
          _DoDWords(chunk, secondChunk);
          Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

          bytesLeft -= chunkLength;
          offset += chunkLength;
        }
      }

      // short part
      if (RuntimeConfiguration.Has16BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_WORD) {
        var chunk = new ushort[maxChunkSize >> 1];
        var secondChunk = new ushort[maxChunkSize >> 1];

        while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {

          var chunkLength = Math.Min(bytesLeft, maxChunkSize);
          var itemCount = chunkLength >> 1;
          chunkLength = itemCount << 1;


          Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
          Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
          _DoWords(chunk, secondChunk);
          Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

          bytesLeft -= chunkLength;
          offset += chunkLength;
        }
      }

      // remaining bytes
      if (bytesLeft > 0)
        _DoBytes(source, operand, offset, bytesLeft);

    }
  }
}