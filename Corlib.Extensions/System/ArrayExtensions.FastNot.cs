﻿#region (c)2010-2042 Hawkynt
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

#if SUPPORTS_INLINING && !UNSAFE
using System.Runtime.CompilerServices;
#endif

#if SUPPORTS_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace System; 

static partial class ArrayExtensions {


#if !UNSAFE

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _NotManaged(byte[] data, int offset, int count) => FastNot.ProcessInChunks(data, offset, count);
  
#endif

  private static class FastNot {
      
    private static void _DoBytes(byte[] source, int offset, int length) {
      var end = offset + length;
      for (var i = offset; i < end; ++i)
        source[i] ^= 0xff;
    }

    private static void _DoWords(ushort[] source) {
#if SUPPORTS_ASYNC
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
      for (var i = 0; i < source.Length; ++i)
        source[i] ^= 0xffff;
#if SUPPORTS_ASYNC
          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;

        void Action() {
        // TODO: bad choice because of cache and data locality
        var start = Interlocked.Increment(ref index) - 1;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] ^= 0xffff;
        }

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = Action;

        Parallel.Invoke(actions);
#endif
    }

    private static void _DoDWords(uint[] source) {
#if SUPPORTS_ASYNC
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
      for (var i = 0; i < source.Length; ++i)
        source[i] = ~source[i];
#if SUPPORTS_ASYNC
          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        
        void Action() {
          // TODO: bad choice because of cache and data locality
          var start = Interlocked.Increment(ref index) - 1;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] = ~source[i];
        }

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = Action;

        Parallel.Invoke(actions);
#endif
    }

    private static void _DoQWords(ulong[] source) {

#if SUPPORTS_ASYNC
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
      for (var i = 0; i < source.Length; ++i)
        source[i] = ~source[i];
#if SUPPORTS_ASYNC
          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        
        void Action() {
          // TODO: bad choice because of cache and data locality
          var start =Interlocked.Increment(ref index) - 1;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] = ~source[i];
        }

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = Action;

        Parallel.Invoke(actions);
#endif
    }
    
    public static void ProcessInChunks(byte[] source, int offset, int count, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, offset, count);
          return;
        }
        
        // long part
        if (RuntimeConfiguration.Has64BitRegisters && count > RuntimeConfiguration.ALLOCATION_QWORD) {

          var chunk = new ulong[maxChunkSize >> 3];

          while (count > RuntimeConfiguration.BLOCKCOPY_QWORD) {

            var chunkLength = Math.Min(count, maxChunkSize);
            var itemCount = chunkLength >> 3;
            chunkLength = itemCount << 3;

            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            _DoQWords(chunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            count -= chunkLength;
            offset += chunkLength;
          }

        }

        // int part
        if (RuntimeConfiguration.Has32BitRegisters && count > RuntimeConfiguration.ALLOCATION_DWORD) {
          var chunk = new uint[maxChunkSize >> 2];

          while (count > RuntimeConfiguration.BLOCKCOPY_DWORD) {

            var chunkLength = Math.Min(count, maxChunkSize);
            var itemCount = chunkLength >> 2;
            chunkLength = itemCount << 2;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            _DoDWords(chunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            count -= chunkLength;
            offset += chunkLength;
          }
        }

        // short part
        if (RuntimeConfiguration.Has16BitRegisters && count > RuntimeConfiguration.ALLOCATION_WORD) {
          var chunk = new ushort[maxChunkSize >> 1];

          while (count > RuntimeConfiguration.BLOCKCOPY_WORD) {

            var chunkLength = Math.Min(count, maxChunkSize);
            var itemCount = chunkLength >> 1;
            chunkLength = itemCount << 1;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            _DoWords(chunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            count -= chunkLength;
            offset += chunkLength;
          }
        }

        // remaining bytes
        if (count > 0)
          _DoBytes(source, offset, count);

    }

  }
}