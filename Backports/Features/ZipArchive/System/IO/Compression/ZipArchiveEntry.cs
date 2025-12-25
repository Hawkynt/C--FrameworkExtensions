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

using System.IO.Compression.Internal;
using System.Text;

namespace System.IO.Compression;

/// <summary>
/// Represents a compressed file within a zip archive.
/// </summary>
public sealed class ZipArchiveEntry {
  private readonly string _fullName;
  private DateTimeOffset _lastWriteTime;
  private int _externalAttributes;
  private bool _isDeleted;

  // Data for entries read from existing archive
  internal ZipCentralDirectoryHeader? CentralDirectoryHeader { get; set; }
  internal long LocalHeaderOffset { get; set; }

  // Data for new entries being written
  internal MemoryStream? PendingData { get; private set; }
  internal bool IsNewEntry { get; }

  /// <summary>
  /// Gets the zip archive that the entry belongs to.
  /// </summary>
  public ZipArchive Archive { get; }

  /// <summary>
  /// Gets the compressed size of the entry in the zip archive.
  /// </summary>
  public long CompressedLength => this.CentralDirectoryHeader?.CompressedSize ?? 0;

  /// <summary>
  /// Gets the CRC-32 checksum of the entry.
  /// </summary>
  public uint Crc32 => this.CentralDirectoryHeader?.Crc32 ?? 0;

  /// <summary>
  /// Gets or sets the external file attributes.
  /// </summary>
  public int ExternalAttributes {
    get => this._externalAttributes;
    set {
      this._ThrowIfInvalidState();
      this._externalAttributes = value;
    }
  }

  /// <summary>
  /// Gets the relative path of the entry in the zip archive.
  /// </summary>
  public string FullName => this._fullName;

  /// <summary>
  /// Gets or sets the last time the entry in the zip archive was changed.
  /// </summary>
  public DateTimeOffset LastWriteTime {
    get => this._lastWriteTime;
    set {
      this._ThrowIfInvalidState();
      this._lastWriteTime = value;
    }
  }

  /// <summary>
  /// Gets the uncompressed size of the entry in the zip archive.
  /// </summary>
  public long Length => this.CentralDirectoryHeader?.UncompressedSize ?? this.PendingData?.Length ?? 0;

  /// <summary>
  /// Gets the file name of the entry in the zip archive.
  /// </summary>
  public string Name {
    get {
      var lastSlash = this._fullName.LastIndexOf('/');
      return lastSlash >= 0 ? this._fullName[(lastSlash + 1)..] : this._fullName;
    }
  }

  /// <summary>
  /// Gets whether this entry has been marked for deletion.
  /// </summary>
  internal bool IsDeleted => this._isDeleted;

  /// <summary>
  /// Creates a new ZipArchiveEntry for reading from an existing archive.
  /// </summary>
  internal ZipArchiveEntry(ZipArchive archive, ZipCentralDirectoryHeader header) {
    this.Archive = archive;
    this._fullName = header.FileName;
    this.CentralDirectoryHeader = header;
    this.LocalHeaderOffset = header.RelativeOffsetOfLocalHeader;
    this._externalAttributes = (int)header.ExternalFileAttributes;
    this._lastWriteTime = ZipConstants.DosDateTimeToDateTime(header.LastModFileDate, header.LastModFileTime);
    this.CompressionLevel = CompressionLevel.Optimal;
    this.IsNewEntry = false;
  }

  /// <summary>
  /// Creates a new ZipArchiveEntry for writing to an archive.
  /// </summary>
  internal ZipArchiveEntry(ZipArchive archive, string entryName, CompressionLevel compressionLevel) {
    this.Archive = archive;
    this._fullName = entryName.Replace('\\', '/');
    this.CompressionLevel = compressionLevel;
    this._lastWriteTime = DateTimeOffset.Now;
    this._externalAttributes = 0;
    this.IsNewEntry = true;
    this.PendingData = new();
  }

  /// <summary>
  /// Deletes the entry from the zip archive.
  /// </summary>
  /// <exception cref="NotSupportedException">The zip archive for this entry was opened in a mode other than <see cref="ZipArchiveMode.Update"/>.</exception>
  /// <exception cref="ObjectDisposedException">The zip archive for this entry has been disposed.</exception>
  public void Delete() {
    this._ThrowIfInvalidState();

    if (this.Archive.Mode != ZipArchiveMode.Update)
      throw new NotSupportedException("Entries can only be deleted when the archive is opened in Update mode.");

    this._isDeleted = true;
    this.Archive.RemoveEntry(this);
  }

  /// <summary>
  /// Opens the entry from the zip archive.
  /// </summary>
  /// <returns>The stream that represents the contents of the entry.</returns>
  /// <exception cref="ObjectDisposedException">The zip archive for this entry has been disposed.</exception>
  /// <exception cref="InvalidDataException">The entry is missing from the archive or is corrupt.</exception>
  /// <exception cref="IOException">The entry is already currently open for writing.</exception>
  public Stream Open() {
    this._ThrowIfInvalidState();

    // Return a stream for reading
    if (!this.IsNewEntry && this.Archive.Mode != ZipArchiveMode.Create)
      return this._OpenForReading();

    // Return a stream for writing
    this.PendingData ??= new();
    return new ZipEntryWriteStream(this.PendingData);
  }

  /// <summary>
  /// Returns a string that represents the current entry.
  /// </summary>
  public override string ToString() => this._fullName;

  private void _ThrowIfInvalidState() {
    ObjectDisposedException.ThrowIf(this.Archive.IsDisposed, this);
    if (this._isDeleted)
      throw new InvalidOperationException("Entry has been deleted.");
  }

  private Stream _OpenForReading() {
    if (this.CentralDirectoryHeader == null)
      throw new InvalidDataException("Entry data is not available.");

    var archiveStream = this.Archive.ArchiveStream;
    if (archiveStream is not { CanSeek: true })
      throw new InvalidOperationException("Archive stream is not available or seekable.");

    // Seek to local file header
    archiveStream.Position = this.LocalHeaderOffset;

    using var reader = new BinaryReader(new NonClosingStreamWrapper(archiveStream), Encoding.UTF8);
    var localHeader = ZipLocalFileHeader.Read(reader);
    if (localHeader == null)
      throw new InvalidDataException("Invalid local file header.");

    // Position is now at the start of compressed data
    var compressedSize = this.CentralDirectoryHeader.CompressedSize;
    var uncompressedSize = this.CentralDirectoryHeader.UncompressedSize;
    var compressionMethod = this.CentralDirectoryHeader.CompressionMethod;

    // Zip bomb protection: Check claimed uncompressed size
    if (uncompressedSize > ZipConstants.MaxSingleEntrySize)
      throw new InvalidDataException($"Entry '{this._fullName}' claims uncompressed size of {uncompressedSize:N0} bytes, which exceeds the maximum allowed size of {ZipConstants.MaxSingleEntrySize:N0} bytes. This may indicate a zip bomb.");

    // Zip bomb protection: Check compression ratio for compressed entries
    if (compressionMethod != ZipConstants.CompressionMethodStore && compressedSize > 0) {
      var ratio = (double)uncompressedSize / compressedSize;
      if (ratio > ZipConstants.MaxCompressionRatio)
        throw new InvalidDataException($"Entry '{this._fullName}' has a compression ratio of {ratio:N1}:1, which exceeds the maximum allowed ratio of {ZipConstants.MaxCompressionRatio}:1. This may indicate a zip bomb.");
    }

    // Read compressed data into memory
    var compressedData = new byte[compressedSize];
    var bytesRead = archiveStream.Read(compressedData, 0, (int)compressedSize);
    if (bytesRead != compressedSize)
      throw new InvalidDataException("Could not read all compressed data.");

    switch (compressionMethod) {
      // No compression - return data directly
      case ZipConstants.CompressionMethodStore:
        return new MemoryStream(compressedData, writable: false);
      case ZipConstants.CompressionMethodDeflate: {
        // Deflate compression - decompress with size limit verification
        var compressedStream = new MemoryStream(compressedData);
        var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);

        // Stream decompress with limit checking - don't trust header's uncompressedSize for allocation
        // Use a reasonable initial buffer, then grow as needed with hard limit
        var outputStream = new MemoryStream();
        var buffer = new byte[81920]; // 80KB buffer
        long totalBytesWritten = 0;

        int bytesReadFromDeflate;
        while ((bytesReadFromDeflate = deflateStream.Read(buffer, 0, buffer.Length)) > 0) {
          totalBytesWritten += bytesReadFromDeflate;

          // Zip bomb protection: Verify actual decompressed size during streaming
          if (totalBytesWritten > ZipConstants.MaxSingleEntrySize)
            throw new InvalidDataException($"Entry '{this._fullName}' actual decompressed size exceeds the maximum allowed size of {ZipConstants.MaxSingleEntrySize:N0} bytes. This may indicate a zip bomb.");

          outputStream.Write(buffer, 0, bytesReadFromDeflate);
        }

        outputStream.Position = 0;
        return outputStream;
      }
      default:
        throw new InvalidDataException($"Unsupported compression method: {compressionMethod}");
    }
  }

  /// <summary>
  /// Gets the compression level for this entry.
  /// </summary>
  internal CompressionLevel CompressionLevel { get; }

  /// <summary>
  /// Finalizes the entry data after writing is complete.
  /// </summary>
  internal void FinalizeEntry(byte[] compressedData, uint crc32, uint compressedSize, uint uncompressedSize, ushort compressionMethod) {
    ZipConstants.DateTimeToDosDateTime(this._lastWriteTime.DateTime, out var dosDate, out var dosTime);

    this.CentralDirectoryHeader = new() {
      VersionMadeBy = ZipConstants.VersionMadeBy,
      VersionNeeded = ZipConstants.VersionNeededToExtract,
      GeneralPurposeBitFlag = ZipConstants.Utf8EncodingFlag,
      CompressionMethod = compressionMethod,
      LastModFileTime = dosTime,
      LastModFileDate = dosDate,
      Crc32 = crc32,
      CompressedSize = compressedSize,
      UncompressedSize = uncompressedSize,
      FileName = this._fullName,
      ExtraField = [],
      FileComment = string.Empty,
      DiskNumberStart = 0,
      InternalFileAttributes = 0,
      ExternalFileAttributes = (uint)this._externalAttributes,
      RelativeOffsetOfLocalHeader = 0 // Will be set when writing
    };
  }
}

/// <summary>
/// A write stream for a ZipArchiveEntry.
/// </summary>
internal sealed class ZipEntryWriteStream(MemoryStream targetStream) : Stream {
  private bool _isDisposed;

  public override bool CanRead => false;
  public override bool CanSeek => true;
  public override bool CanWrite => true;
  public override long Length => targetStream.Length;

  public override long Position {
    get => targetStream.Position;
    set => targetStream.Position = value;
  }

  public override void Flush() => targetStream.Flush();

  public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

  public override long Seek(long offset, SeekOrigin origin) => targetStream.Seek(offset, origin);

  public override void SetLength(long value) => targetStream.SetLength(value);

  public override void Write(byte[] buffer, int offset, int count) {
    if (this._isDisposed)
      throw new ObjectDisposedException(nameof(ZipEntryWriteStream));
    targetStream.Write(buffer, offset, count);
  }

  protected override void Dispose(bool disposing) {
    if (!this._isDisposed && disposing) {
      this._isDisposed = true;
      // Don't dispose the target stream - it's owned by the entry
    }

    base.Dispose(disposing);
  }
}

#endif
