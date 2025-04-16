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
//

#nullable enable

using System.Buffers;
using Guard;

namespace System.IO;

/// <summary>
/// A buffered stream wrapper that optimizes small reads and writes using a buffer.
/// </summary>
public sealed class BufferedStreamEx(Stream underlyingStream, int bufferSize = 8192, bool dontDisposeUnderlyingStream = false) : Stream {
  private readonly byte[] _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
  private long _position = underlyingStream.CanSeek ? underlyingStream.Position : 0;
  private int _bufferLength;
  private long _bufferStartPositionInStream = -1;
  private bool _isDirty;

  public override long Position {
    get => this._position;
    set {
      Against.IndexBelowZero(value);
      this._position = value;
    }
  }

  public override long Length => underlyingStream.Length;
  public override bool CanRead => underlyingStream.CanRead;
  public override bool CanSeek => underlyingStream.CanSeek;
  public override bool CanWrite => underlyingStream.CanWrite;

  public override void Flush() {
    this._FlushBuffer();
    underlyingStream.Flush();
  }

  private void _FlushBuffer() {
    if (!this._isDirty)
      return;

    if (underlyingStream.Position != this._bufferStartPositionInStream) {
      Against.False(underlyingStream.CanSeek);
      underlyingStream.Position = this._bufferStartPositionInStream;
    }

    underlyingStream.Write(this._buffer, 0, this._bufferLength);
    this._isDirty = false;
  }

  private void _EnsureBufferLoaded() {
    var wantedPosition = this._position;
    var bufferStartInStream = this._bufferStartPositionInStream;

    if (bufferStartInStream >= 0) {
      if (wantedPosition >= bufferStartInStream && wantedPosition < bufferStartInStream + bufferSize)
        return;

      this._FlushBuffer();
    }

    var alignedStart = wantedPosition - (wantedPosition % bufferSize);
    if (underlyingStream.Position != alignedStart) {
      Against.False(underlyingStream.CanSeek);
      underlyingStream.Position = alignedStart;
    }

    this._bufferStartPositionInStream = alignedStart;
    this._bufferLength = 0;
    if (!underlyingStream.CanRead)
      return;

    var maxToRead = (int)Math.Min(bufferSize, underlyingStream.Length - alignedStart);
    if (maxToRead > 0)
      this._bufferLength = underlyingStream.Read(this._buffer, 0, maxToRead);
  }

  public override int ReadByte() {
    Against.False(this.CanRead);

    this._EnsureBufferLoaded();
    var offset = (int)(this._position - this._bufferStartPositionInStream);
    if (offset >= this._bufferLength)
      return -1;

    var result = this._buffer[offset];
    ++this._position;
    return result;
  }

  public override void WriteByte(byte value) {
    Against.False(this.CanWrite);

    this._EnsureBufferLoaded();
    var offset = (int)(this._position - this._bufferStartPositionInStream);
    this._buffer[offset] = value;
    ++this._position;

    this._bufferLength = Math.Max(this._bufferLength, offset + 1);
    this._isDirty = true;
  }

  public override int Read(byte[] dest, int offset, int count) {
    Against.ArgumentIsNull(dest);
    Against.IndexBelowZero(offset);
    Against.IndexOutOfRange(offset, dest.Length - 1);
    Against.CountBelowZero(count);
    Against.CountOutOfRange(count, offset, dest.Length - 1);
    Against.False(this.CanRead);

    if (count == 0)
      return 0;

    var destSpan = new Span<byte>(dest, offset, count);
    return this.Read(destSpan);
  }

  public override void Write(byte[] src, int offset, int count) {
    Against.ArgumentIsNull(src);
    Against.IndexBelowZero(offset);
    Against.IndexOutOfRange(offset, src.Length - 1);
    Against.CountBelowZero(count);
    Against.CountOutOfRange(count, offset, src.Length - 1);
    Against.False(this.CanWrite);

    if (count == 0)
      return;

    var srcSpan = new ReadOnlySpan<byte>(src, offset, count);
    this.Write(srcSpan);
  }

#if SUPPORTS_SPAN
  public override int Read(Span<byte> buffer) {
#else
  public int Read(Span<byte> buffer) {
#endif
    Against.False(this.CanRead);

    if (buffer.IsEmpty)
      return 0;

    var totalBytesRead = 0;
    var remaining = buffer.Length;
    var bufferOffset = 0;

    while (remaining > 0) {
      // Ensure we have data in the buffer
      this._EnsureBufferLoaded();

      // Calculate how much we can read from the current buffer
      var streamOffset = this._position - this._bufferStartPositionInStream;
      if (streamOffset >= this._bufferLength)
        break; // End of stream reached

      var bytesAvailable = this._bufferLength - (int)streamOffset;
      var bytesToRead = Math.Min(bytesAvailable, remaining);

      // Copy data from our buffer to the destination buffer
      new ReadOnlySpan<byte>(this._buffer, (int)streamOffset, bytesToRead)
        .CopyTo(buffer.Slice(bufferOffset, bytesToRead));

      // Update positions and counters
      this._position += bytesToRead;
      bufferOffset += bytesToRead;
      remaining -= bytesToRead;
      totalBytesRead += bytesToRead;

      // If we've read less than available or less than requested, we're at the end of the stream
      if (bytesToRead < bytesAvailable || bytesToRead == 0)
        break;
    }

    return totalBytesRead;
  }

#if SUPPORTS_SPAN
  public override void Write(ReadOnlySpan<byte> buffer) {
#else
  public void Write(ReadOnlySpan<byte> buffer) {
#endif
    Against.False(this.CanWrite);

    throw new NotImplementedException();

    if (buffer.IsEmpty)
      return;

    var remaining = buffer.Length;
    var bufferOffset = 0;

    while (remaining > 0) {
      // Ensure we have the correct buffer loaded
      this._EnsureBufferLoaded();

      // Calculate offset into our buffer
      var streamOffset = (int)(this._position - this._bufferStartPositionInStream);

      // Calculate how much we can write to the current buffer
      var bytesAvailable = bufferSize - streamOffset;
      var bytesToWrite = Math.Min(bytesAvailable, remaining);

      // Copy data from source buffer to our buffer
      buffer.Slice(bufferOffset, bytesToWrite)
        .CopyTo(new Span<byte>(this._buffer, streamOffset, bytesToWrite));

      // Update positions and counters
      this._position += bytesToWrite;
      bufferOffset += bytesToWrite;
      remaining -= bytesToWrite;

      // Update buffer state
      this._bufferLength = Math.Max(this._bufferLength, streamOffset + bytesToWrite);
      this._isDirty = true;

      // If we've filled our buffer, flush it
      if (bytesToWrite == bytesAvailable)
        this._FlushBuffer();
    }
  }

  public override long Seek(long offset, SeekOrigin origin) {
    Against.UnknownEnumValues(origin);

    var target = origin switch {
      SeekOrigin.Begin => offset,
      SeekOrigin.Current => this._position + offset,
      SeekOrigin.End => this.Length + offset,
      _ => 0 /* is prevented by pre-condition check */
    };

    Against.IndexBelowZero(target);

    this._position = target;
    return this._position;
  }

  public override void SetLength(long value) {
    Against.IndexBelowZero(value);

    this._FlushBuffer();
    underlyingStream.SetLength(value);

    // If current position is beyond new length, adjust it
    if (this._position > value)
      this._position = value;

    // If our buffer contains data beyond new length, update it
    if (this._bufferStartPositionInStream >= 0 &&
        this._bufferStartPositionInStream + this._bufferLength > value) {
      if (this._bufferStartPositionInStream < value) {
        // Partial invalidation - truncate buffer
        this._bufferLength = (int)(value - this._bufferStartPositionInStream);
      } else {
        // Complete invalidation
        this._bufferStartPositionInStream = -1;
        this._bufferLength = 0;
      }
    }
  }

  protected override void Dispose(bool disposing) {
    if (disposing) {
      try {
        this.Flush();
        if (!dontDisposeUnderlyingStream)
          underlyingStream.Dispose();
      } finally {
        ArrayPool<byte>.Shared.Return(this._buffer);
      }
    }

    base.Dispose(disposing);
  }
}