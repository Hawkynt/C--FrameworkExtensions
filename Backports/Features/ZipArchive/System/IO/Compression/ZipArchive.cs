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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression.Internal;
using System.Text;
using Guard;

namespace System.IO.Compression;

/// <summary>
/// Represents a package of compressed files in the zip archive format.
/// </summary>
public sealed class ZipArchive : IDisposable {
  private readonly Stream _stream;
  private readonly bool _leaveOpen;
  private readonly List<ZipArchiveEntry> _entries;
  private ReadOnlyCollection<ZipArchiveEntry>? _entriesReadOnly;
  private bool _isDisposed;

  /// <summary>
  /// Gets the underlying stream for this archive.
  /// </summary>
  internal Stream? ArchiveStream => this._isDisposed ? null : this._stream;

  /// <summary>
  /// Gets whether this archive has been disposed.
  /// </summary>
  internal bool IsDisposed => this._isDisposed;

  /// <summary>
  /// Initializes a new instance of the <see cref="ZipArchive"/> class from the specified stream.
  /// </summary>
  /// <param name="stream">The stream that contains the archive to be read.</param>
  public ZipArchive(Stream stream)
    : this(stream, ZipArchiveMode.Read, leaveOpen: false, entryNameEncoding: null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ZipArchive"/> class from the specified stream and with the specified mode.
  /// </summary>
  /// <param name="stream">The input or output stream.</param>
  /// <param name="mode">One of the enumeration values that indicates whether the zip archive is used to read, create, or update entries.</param>
  public ZipArchive(Stream stream, ZipArchiveMode mode)
    : this(stream, mode, leaveOpen: false, entryNameEncoding: null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ZipArchive"/> class on the specified stream for the specified mode, and optionally leaves the stream open.
  /// </summary>
  /// <param name="stream">The input or output stream.</param>
  /// <param name="mode">One of the enumeration values that indicates whether the zip archive is used to read, create, or update entries.</param>
  /// <param name="leaveOpen">true to leave the stream open after the <see cref="ZipArchive"/> object is disposed; otherwise, false.</param>
  public ZipArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen)
    : this(stream, mode, leaveOpen, entryNameEncoding: null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ZipArchive"/> class on the specified stream for the specified mode, uses the specified encoding for entry names, and optionally leaves the stream open.
  /// </summary>
  /// <param name="stream">The input or output stream.</param>
  /// <param name="mode">One of the enumeration values that indicates whether the zip archive is used to read, create, or update entries.</param>
  /// <param name="leaveOpen">true to leave the stream open after the <see cref="ZipArchive"/> object is disposed; otherwise, false.</param>
  /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive.</param>
  public ZipArchive(Stream stream, ZipArchiveMode mode, bool leaveOpen, Encoding? entryNameEncoding) {
    Against.ArgumentIsNull(stream);

    _ValidateStreamCapabilities(stream, mode);

    this._stream = stream;
    this.Mode = mode;
    this._leaveOpen = leaveOpen;
    this._entries = [];

    if (mode is ZipArchiveMode.Read or ZipArchiveMode.Update)
      this._ReadCentralDirectory();
  }

  /// <summary>
  /// Gets the collection of entries that are currently in the zip archive.
  /// </summary>
  public ReadOnlyCollection<ZipArchiveEntry> Entries {
    get {
      this._ThrowIfDisposed();

      if (this.Mode == ZipArchiveMode.Create)
        throw new NotSupportedException("Entries cannot be enumerated in Create mode.");

      return this._entriesReadOnly ??= new(this._entries);
    }
  }

  /// <summary>
  /// Gets a value that describes the type of action the zip archive can perform on entries.
  /// </summary>
  public ZipArchiveMode Mode { get; }

  /// <summary>
  /// Creates an empty entry that has the specified path and entry name in the zip archive.
  /// </summary>
  /// <param name="entryName">A path, relative to the root of the archive, that specifies the name of the entry to be created.</param>
  /// <returns>An empty entry in the zip archive.</returns>
  public ZipArchiveEntry CreateEntry(string entryName) => this.CreateEntry(entryName, CompressionLevel.Optimal);

  /// <summary>
  /// Creates an empty entry that has the specified entry name and compression level in the zip archive.
  /// </summary>
  /// <param name="entryName">A path, relative to the root of the archive, that specifies the name of the entry to be created.</param>
  /// <param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression effectiveness when creating the entry.</param>
  /// <returns>An empty entry in the zip archive.</returns>
  public ZipArchiveEntry CreateEntry(string entryName, CompressionLevel compressionLevel) {
    Against.ArgumentIsNull(entryName);
    Against.ArgumentIsNullOrWhiteSpace(entryName);

    this._ThrowIfDisposed();

    if (this.Mode == ZipArchiveMode.Read)
      throw new NotSupportedException("Cannot create entries in Read mode.");

    var entry = new ZipArchiveEntry(this, entryName, compressionLevel);
    this._entries.Add(entry);
    this._entriesReadOnly = null;

    return entry;
  }

  /// <summary>
  /// Retrieves a wrapper for the specified entry in the zip archive.
  /// </summary>
  /// <param name="entryName">A path, relative to the root of the archive, that identifies the entry to retrieve.</param>
  /// <returns>A wrapper for the specified entry in the archive; null if the entry does not exist in the archive.</returns>
  public ZipArchiveEntry? GetEntry(string entryName) {
    Against.ArgumentIsNull(entryName);

    this._ThrowIfDisposed();

    if (this.Mode == ZipArchiveMode.Create)
      throw new NotSupportedException("Cannot get entries in Create mode.");

    // Normalize path separators
    entryName = entryName.Replace('\\', '/');

    foreach (var entry in this._entries)
      if (string.Equals(entry.FullName, entryName, StringComparison.Ordinal))
        return entry;

    return null;
  }

  /// <summary>
  /// Removes an entry from the internal list.
  /// </summary>
  internal void RemoveEntry(ZipArchiveEntry entry) {
    this._entries.Remove(entry);
    this._entriesReadOnly = null;
  }

  /// <summary>
  /// Releases the resources used by the current instance of the <see cref="ZipArchive"/> class.
  /// </summary>
  public void Dispose() {
    this.Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Called by the <see cref="Dispose()"/> and <see cref="object.Finalize()"/> methods to release the resources used by the current instance of the <see cref="ZipArchive"/> class.
  /// </summary>
  /// <param name="disposing">true to release managed and unmanaged resources; false to release only unmanaged resources.</param>
  private void Dispose(bool disposing) {
    if (this._isDisposed)
      return;

    if (disposing) {
      if (this.Mode == ZipArchiveMode.Create || this.Mode == ZipArchiveMode.Update)
        this._WriteArchive();

      if (!this._leaveOpen)
        this._stream.Dispose();
    }

    this._isDisposed = true;
  }

  private void _ThrowIfDisposed() {
    if (this._isDisposed)
      throw new ObjectDisposedException(nameof(ZipArchive));
  }

  private static void _ValidateStreamCapabilities(Stream stream, ZipArchiveMode mode) {
    switch (mode) {
      case ZipArchiveMode.Read:
        if (!stream.CanRead)
          throw new ArgumentException("Stream must be readable for Read mode.", nameof(stream));
        break;
      case ZipArchiveMode.Create:
        if (!stream.CanWrite)
          throw new ArgumentException("Stream must be writable for Create mode.", nameof(stream));
        break;
      case ZipArchiveMode.Update:
        if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek)
          throw new ArgumentException("Stream must be readable, writable, and seekable for Update mode.", nameof(stream));
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(mode));
    }
  }

  private void _ReadCentralDirectory() {
    // Find End of Central Directory
    var eocdPosition = ZipEndOfCentralDirectory.FindInStream(this._stream);
    if (eocdPosition < 0)
      throw new InvalidDataException("End of central directory not found. The archive may be corrupt.");

    this._stream.Position = eocdPosition;
    using var reader = new BinaryReader(new NonClosingStreamWrapper(this._stream), Encoding.UTF8);
    var eocd = ZipEndOfCentralDirectory.Read(reader);
    if (eocd == null)
      throw new InvalidDataException("Failed to read end of central directory.");

    // Zip bomb protection: Check entry count
    if (eocd.TotalEntries > ZipConstants.MaxEntryCount)
      throw new InvalidDataException($"Archive claims {eocd.TotalEntries:N0} entries, which exceeds the maximum allowed count of {ZipConstants.MaxEntryCount:N0}. This may indicate a zip bomb.");

    // Read central directory
    this._stream.Position = eocd.CentralDirectoryOffset;

    for (var i = 0; i < eocd.TotalEntries; ++i) {
      var header = ZipCentralDirectoryHeader.Read(reader);
      if (header == null)
        throw new InvalidDataException("Failed to read central directory header.");

      var entry = new ZipArchiveEntry(this, header);
      this._entries.Add(entry);
    }
  }

  private void _WriteArchive() {
    // For Create mode, write all entries to the stream
    // For Update mode, we need to rewrite the entire archive

    // For Update mode, we need to read all existing entry data BEFORE truncating
    var cachedEntryData = new Dictionary<ZipArchiveEntry, (ZipLocalFileHeader Header, byte[] Data)>();
    if (this.Mode == ZipArchiveMode.Update && this._stream.CanSeek) {
      foreach (var entry in this._entries) {
        if (entry.IsDeleted || entry.IsNewEntry)
          continue;

        var data = this._ReadExistingEntryData(entry);
        if (data.HasValue)
          cachedEntryData[entry] = data.Value;
      }

      // Now truncate the stream
      this._stream.Position = 0;
      this._stream.SetLength(0);
    }

    using var writer = new BinaryWriter(new NonClosingStreamWrapper(this._stream), Encoding.UTF8);

    // Track positions for central directory
    var entryOffsets = new List<long>();

    // Write local file headers and data
    foreach (var entry in this._entries) {
      if (entry.IsDeleted)
        continue;

      entryOffsets.Add(this._stream.Position);

      if (entry.IsNewEntry)
        this._WriteNewEntry(writer, entry);
      else if (cachedEntryData.TryGetValue(entry, out var cached))
        _WriteCachedEntry(writer, cached.Header, cached.Data);
      else
        this._WriteCopiedEntry(writer, entry);
    }

    // Write central directory
    var centralDirectoryStart = this._stream.Position;
    var entryIndex = 0;

    foreach (var entry in this._entries) {
      if (entry.IsDeleted)
        continue;

      if (entry.CentralDirectoryHeader == null)
        throw new InvalidOperationException("Entry has no central directory header.");

      // Update the local header offset
      entry.CentralDirectoryHeader.RelativeOffsetOfLocalHeader = (uint)entryOffsets[entryIndex];
      entry.CentralDirectoryHeader.Write(writer);
      ++entryIndex;
    }

    var centralDirectoryEnd = this._stream.Position;

    // Write End of Central Directory
    var eocd = new ZipEndOfCentralDirectory {
      DiskNumber = 0,
      DiskWithCentralDirectory = 0,
      EntriesOnDisk = (ushort)entryIndex,
      TotalEntries = (ushort)entryIndex,
      CentralDirectorySize = (uint)(centralDirectoryEnd - centralDirectoryStart),
      CentralDirectoryOffset = (uint)centralDirectoryStart,
      Comment = string.Empty
    };
    eocd.Write(writer);
  }

  private void _WriteNewEntry(BinaryWriter writer, ZipArchiveEntry entry) {
    // Get pending data
    var data = entry.PendingData?.ToArray() ?? [];

    // Calculate CRC and compress
    var crc32 = data.Length > 0 ? Crc32.Compute(data) : 0u;
    var uncompressedSize = (uint)data.Length;

    byte[] compressedData;
    ushort compressionMethod;

    if (entry.CompressionLevel == CompressionLevel.NoCompression || data.Length == 0) {
      compressedData = data;
      compressionMethod = ZipConstants.CompressionMethodStore;
    } else {
      using var compressedStream = new MemoryStream();
      using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, leaveOpen: true))
        deflateStream.Write(data, 0, data.Length);
      compressedData = compressedStream.ToArray();
      compressionMethod = ZipConstants.CompressionMethodDeflate;

      // If compression didn't help, store uncompressed
      if (compressedData.Length >= data.Length) {
        compressedData = data;
        compressionMethod = ZipConstants.CompressionMethodStore;
      }
    }

    var compressedSize = (uint)compressedData.Length;

    // Finalize the entry
    entry.FinalizeEntry(compressedData, crc32, compressedSize, uncompressedSize, compressionMethod);

    // Write local file header
    ZipConstants.DateTimeToDosDateTime(entry.LastWriteTime.DateTime, out var dosDate, out var dosTime);

    var localHeader = new ZipLocalFileHeader {
      VersionNeeded = ZipConstants.VersionNeededToExtract,
      GeneralPurposeBitFlag = ZipConstants.Utf8EncodingFlag,
      CompressionMethod = compressionMethod,
      LastModFileTime = dosTime,
      LastModFileDate = dosDate,
      Crc32 = crc32,
      CompressedSize = compressedSize,
      UncompressedSize = uncompressedSize,
      FileName = entry.FullName,
      ExtraField = []
    };
    localHeader.Write(writer);

    // Write compressed data
    writer.Write(compressedData);
  }

  private void _WriteCopiedEntry(BinaryWriter writer, ZipArchiveEntry entry) {
    if (entry.CentralDirectoryHeader == null)
      throw new InvalidOperationException("Entry has no central directory header.");

    // Read the original data from the archive
    var originalPosition = this._stream.Position;

    // Seek to the original local header
    this._stream.Position = entry.LocalHeaderOffset;
    using var reader = new BinaryReader(new NonClosingStreamWrapper(this._stream), Encoding.UTF8);
    var localHeader = ZipLocalFileHeader.Read(reader);
    if (localHeader == null)
      throw new InvalidDataException("Invalid local file header.");

    // Read compressed data
    var compressedData = reader.ReadBytes((int)entry.CentralDirectoryHeader.CompressedSize);

    // Return to write position
    this._stream.Position = originalPosition;

    // Write local header
    localHeader.Write(writer);

    // Write compressed data
    writer.Write(compressedData);
  }

  private (ZipLocalFileHeader Header, byte[] Data)? _ReadExistingEntryData(ZipArchiveEntry entry) {
    if (entry.CentralDirectoryHeader == null)
      return null;

    // Seek to the original local header
    this._stream.Position = entry.LocalHeaderOffset;
    using var reader = new BinaryReader(new NonClosingStreamWrapper(this._stream), Encoding.UTF8);
    var localHeader = ZipLocalFileHeader.Read(reader);
    if (localHeader == null)
      return null;

    // Read compressed data
    var compressedData = reader.ReadBytes((int)entry.CentralDirectoryHeader.CompressedSize);

    return (localHeader, compressedData);
  }

  private static void _WriteCachedEntry(BinaryWriter writer, ZipLocalFileHeader header, byte[] compressedData) {
    // Write local header
    header.Write(writer);

    // Write compressed data
    writer.Write(compressedData);
  }
}

#endif
