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

#if !SUPPORTS_ZIPARCHIVE

namespace System.IO.Compression.Internal;

/// <summary>
/// A stream wrapper that prevents the underlying stream from being closed when disposed.
/// Used for BinaryReader/BinaryWriter compatibility on older .NET frameworks that don't
/// support the leaveOpen parameter.
/// </summary>
internal sealed class NonClosingStreamWrapper(Stream innerStream) : Stream {
  private readonly Stream _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
  private bool _isDisposed;

  public override bool CanRead => !this._isDisposed && this._innerStream.CanRead;
  public override bool CanSeek => !this._isDisposed && this._innerStream.CanSeek;
  public override bool CanWrite => !this._isDisposed && this._innerStream.CanWrite;
  public override long Length => this._innerStream.Length;

  public override long Position {
    get => this._innerStream.Position;
    set => this._innerStream.Position = value;
  }

  public override void Flush() => this._innerStream.Flush();

  public override int Read(byte[] buffer, int offset, int count) => this._innerStream.Read(buffer, offset, count);

  public override long Seek(long offset, SeekOrigin origin) => this._innerStream.Seek(offset, origin);

  public override void SetLength(long value) => this._innerStream.SetLength(value);

  public override void Write(byte[] buffer, int offset, int count) => this._innerStream.Write(buffer, offset, count);

  protected override void Dispose(bool disposing) {
    // Don't dispose the inner stream - that's the whole point of this wrapper
    this._isDisposed = true;
    base.Dispose(disposing);
  }
}

#endif
