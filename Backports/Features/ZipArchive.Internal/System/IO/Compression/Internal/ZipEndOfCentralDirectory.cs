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

using System.Text;

namespace System.IO.Compression.Internal;

/// <summary>
/// Represents a ZIP End of Central Directory Record structure.
/// </summary>
internal sealed class ZipEndOfCentralDirectory {
  public ushort DiskNumber { get; set; }
  public ushort DiskWithCentralDirectory { get; set; }
  public ushort EntriesOnDisk { get; set; }
  public ushort TotalEntries { get; set; }
  public uint CentralDirectorySize { get; set; }
  public uint CentralDirectoryOffset { get; set; }
  public string Comment { get; set; } = string.Empty;

  /// <summary>
  /// Gets the total size of this record when written to a stream.
  /// </summary>
  public int TotalSize => ZipConstants.EndOfCentralDirectoryFixedSize + Encoding.UTF8.GetByteCount(this.Comment);

  /// <summary>
  /// Reads an End of Central Directory record from a stream positioned at the signature.
  /// </summary>
  /// <param name="reader">The BinaryReader to read from.</param>
  /// <returns>The parsed record, or null if the signature doesn't match.</returns>
  public static ZipEndOfCentralDirectory? Read(BinaryReader reader) {
    var signature = reader.ReadUInt32();
    if (signature != ZipConstants.EndOfCentralDirectorySignature)
      return null;

    var eocd = new ZipEndOfCentralDirectory {
      DiskNumber = reader.ReadUInt16(),
      DiskWithCentralDirectory = reader.ReadUInt16(),
      EntriesOnDisk = reader.ReadUInt16(),
      TotalEntries = reader.ReadUInt16(),
      CentralDirectorySize = reader.ReadUInt32(),
      CentralDirectoryOffset = reader.ReadUInt32()
    };

    var commentLength = reader.ReadUInt16();
    if (commentLength <= 0)
      return eocd;

    var commentBytes = reader.ReadBytes(commentLength);
    eocd.Comment = Encoding.UTF8.GetString(commentBytes);
    return eocd;
  }

  /// <summary>
  /// Writes this End of Central Directory record to a stream.
  /// </summary>
  /// <param name="writer">The BinaryWriter to write to.</param>
  public void Write(BinaryWriter writer) {
    var commentBytes = Encoding.UTF8.GetBytes(this.Comment);

    writer.Write(ZipConstants.EndOfCentralDirectorySignature);
    writer.Write(this.DiskNumber);
    writer.Write(this.DiskWithCentralDirectory);
    writer.Write(this.EntriesOnDisk);
    writer.Write(this.TotalEntries);
    writer.Write(this.CentralDirectorySize);
    writer.Write(this.CentralDirectoryOffset);
    writer.Write((ushort)commentBytes.Length);
    writer.Write(commentBytes);
  }

  /// <summary>
  /// Locates the End of Central Directory record in a stream by searching backwards from the end.
  /// </summary>
  /// <param name="stream">The stream to search.</param>
  /// <returns>The position of the EOCD signature, or -1 if not found.</returns>
  public static long FindInStream(Stream stream) {
    if (!stream.CanSeek)
      throw new InvalidOperationException("Stream must be seekable to find End of Central Directory.");

    var streamLength = stream.Length;
    if (streamLength < ZipConstants.EndOfCentralDirectoryFixedSize)
      return -1;

    // Search backwards from the end (EOCD can have a comment up to 65535 bytes)
    var maxSearchLength = Math.Min(streamLength, ZipConstants.EndOfCentralDirectoryFixedSize + ZipConstants.MaxCommentLength);
    var buffer = new byte[4];
    var signatureBytes = BitConverter.GetBytes(ZipConstants.EndOfCentralDirectorySignature);

    for (var offset = ZipConstants.EndOfCentralDirectoryFixedSize; offset <= maxSearchLength; ++offset) {
      stream.Position = streamLength - offset;
      if (stream.Read(buffer, 0, 4) != 4)
        continue;

      if (buffer[0] == signatureBytes[0] && buffer[1] == signatureBytes[1] && buffer[2] == signatureBytes[2] && buffer[3] == signatureBytes[3])
        return streamLength - offset;
    }

    return -1;
  }
}

#endif
