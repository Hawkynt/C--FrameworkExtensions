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
/// Represents a ZIP Central Directory File Header structure.
/// </summary>
internal sealed class ZipCentralDirectoryHeader {
  public ushort VersionMadeBy { get; set; }
  public ushort VersionNeeded { get; set; }
  public ushort GeneralPurposeBitFlag { get; set; }
  public ushort CompressionMethod { get; set; }
  public ushort LastModFileTime { get; set; }
  public ushort LastModFileDate { get; set; }
  public uint Crc32 { get; set; }
  public uint CompressedSize { get; set; }
  public uint UncompressedSize { get; set; }
  public ushort DiskNumberStart { get; set; }
  public ushort InternalFileAttributes { get; set; }
  public uint ExternalFileAttributes { get; set; }
  public uint RelativeOffsetOfLocalHeader { get; set; }
  public string FileName { get; set; } = string.Empty;
  public byte[] ExtraField { get; set; } = [];
  public string FileComment { get; set; } = string.Empty;

  /// <summary>
  /// Gets the total size of this header when written to a stream.
  /// </summary>
  public int TotalSize {
    get {
      var encoding = _GetEncoding(this.GeneralPurposeBitFlag);
      return ZipConstants.CentralDirectoryHeaderFixedSize
             + encoding.GetByteCount(this.FileName)
             + this.ExtraField.Length
             + encoding.GetByteCount(this.FileComment);
    }
  }

  /// <summary>
  /// Reads a Central Directory Header from a stream.
  /// </summary>
  /// <param name="reader">The BinaryReader to read from.</param>
  /// <returns>The parsed header, or null if the signature doesn't match.</returns>
  public static ZipCentralDirectoryHeader? Read(BinaryReader reader) {
    var signature = reader.ReadUInt32();
    if (signature != ZipConstants.CentralDirectoryHeaderSignature)
      return null;

    var header = new ZipCentralDirectoryHeader {
      VersionMadeBy = reader.ReadUInt16(),
      VersionNeeded = reader.ReadUInt16(),
      GeneralPurposeBitFlag = reader.ReadUInt16(),
      CompressionMethod = reader.ReadUInt16(),
      LastModFileTime = reader.ReadUInt16(),
      LastModFileDate = reader.ReadUInt16(),
      Crc32 = reader.ReadUInt32(),
      CompressedSize = reader.ReadUInt32(),
      UncompressedSize = reader.ReadUInt32()
    };

    var fileNameLength = reader.ReadUInt16();
    var extraFieldLength = reader.ReadUInt16();
    var fileCommentLength = reader.ReadUInt16();

    header.DiskNumberStart = reader.ReadUInt16();
    header.InternalFileAttributes = reader.ReadUInt16();
    header.ExternalFileAttributes = reader.ReadUInt32();
    header.RelativeOffsetOfLocalHeader = reader.ReadUInt32();

    var encoding = _GetEncoding(header.GeneralPurposeBitFlag);

    var fileNameBytes = reader.ReadBytes(fileNameLength);
    header.FileName = encoding.GetString(fileNameBytes);

    header.ExtraField = reader.ReadBytes(extraFieldLength);

    var fileCommentBytes = reader.ReadBytes(fileCommentLength);
    header.FileComment = encoding.GetString(fileCommentBytes);

    return header;
  }

  /// <summary>
  /// Writes this Central Directory Header to a stream.
  /// </summary>
  /// <param name="writer">The BinaryWriter to write to.</param>
  public void Write(BinaryWriter writer) {
    var encoding = _GetEncoding(this.GeneralPurposeBitFlag);
    var fileNameBytes = encoding.GetBytes(this.FileName);
    var fileCommentBytes = encoding.GetBytes(this.FileComment);

    writer.Write(ZipConstants.CentralDirectoryHeaderSignature);
    writer.Write(this.VersionMadeBy);
    writer.Write(this.VersionNeeded);
    writer.Write(this.GeneralPurposeBitFlag);
    writer.Write(this.CompressionMethod);
    writer.Write(this.LastModFileTime);
    writer.Write(this.LastModFileDate);
    writer.Write(this.Crc32);
    writer.Write(this.CompressedSize);
    writer.Write(this.UncompressedSize);
    writer.Write((ushort)fileNameBytes.Length);
    writer.Write((ushort)this.ExtraField.Length);
    writer.Write((ushort)fileCommentBytes.Length);
    writer.Write(this.DiskNumberStart);
    writer.Write(this.InternalFileAttributes);
    writer.Write(this.ExternalFileAttributes);
    writer.Write(this.RelativeOffsetOfLocalHeader);
    writer.Write(fileNameBytes);
    writer.Write(this.ExtraField);
    writer.Write(fileCommentBytes);
  }

  private static Encoding _GetEncoding(ushort flags)
    => (flags & ZipConstants.Utf8EncodingFlag) != 0
      ? Encoding.UTF8
      :
#if DEPRECATED_UTF7
        Encoding.GetEncoding(437); // DOS/OEM code page
#else
        Encoding.ASCII;
#endif
}

#endif
