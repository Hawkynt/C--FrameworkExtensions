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

#if !SUPPORTS_STREAM_READ_SPAN
using Guard;
using System.Buffers;

namespace System.IO;

public static partial class StreamPolyfills {
  extension(Stream @this)
  {
    public int Read(Span<byte> buffer) {
      Against.ThisIsNull(@this);

      if (buffer.IsEmpty)
        return 0;

      var size = buffer.Length;
      byte[] token = null;
      try {
        
        // TODO: size may exceed MaxChunkSize (1MB), in those cases - loop
        token = ArrayPool<byte>.Shared.Rent(size);
        var bytesRead = @this.Read(token, 0, size);
        token.AsSpan()[..bytesRead].CopyTo(buffer[..bytesRead]);
        return bytesRead;
      } finally {
        if (token != null)
          ArrayPool<byte>.Shared.Return(token);
      }

    }

    public void Write(ReadOnlySpan<byte> buffer) {
      Against.ThisIsNull(@this);

      if (buffer.IsEmpty)
        return;

      const int MaxChunkSize = 1024 * 1024; // 1MB
      byte[] rented = null;
      try {
        rented = ArrayPool<byte>.Shared.Rent(MaxChunkSize);
        var span = rented.AsSpan(0, MaxChunkSize);
        while (!buffer.IsEmpty) {
          var chunkSize = Math.Min(buffer.Length, MaxChunkSize);
          buffer[..chunkSize].CopyTo(span[..chunkSize]);
          @this.Write(rented, 0, chunkSize);
          buffer = buffer[chunkSize..];
        }
      } finally {
        if (rented != null)
          ArrayPool<byte>.Shared.Return(rented);
      }

    }
  }
}

#endif
