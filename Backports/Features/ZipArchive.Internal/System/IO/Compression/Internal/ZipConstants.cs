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
/// Constants used in the ZIP file format.
/// </summary>
internal static class ZipConstants {
  /// <summary>
  /// Signature for Local File Header (0x04034B50).
  /// </summary>
  public const uint LocalFileHeaderSignature = 0x04034B50;

  /// <summary>
  /// Signature for Central Directory File Header (0x02014B50).
  /// </summary>
  public const uint CentralDirectoryHeaderSignature = 0x02014B50;

  /// <summary>
  /// Signature for End of Central Directory Record (0x06054B50).
  /// </summary>
  public const uint EndOfCentralDirectorySignature = 0x06054B50;

  /// <summary>
  /// Signature for Data Descriptor (0x08074B50).
  /// </summary>
  public const uint DataDescriptorSignature = 0x08074B50;

  /// <summary>
  /// Size of the fixed portion of the Local File Header.
  /// </summary>
  public const int LocalFileHeaderFixedSize = 30;

  /// <summary>
  /// Size of the fixed portion of the Central Directory Header.
  /// </summary>
  public const int CentralDirectoryHeaderFixedSize = 46;

  /// <summary>
  /// Size of the fixed portion of the End of Central Directory Record.
  /// </summary>
  public const int EndOfCentralDirectoryFixedSize = 22;

  /// <summary>
  /// Size of the Data Descriptor (without signature).
  /// </summary>
  public const int DataDescriptorSize = 12;

  /// <summary>
  /// Size of the Data Descriptor with signature.
  /// </summary>
  public const int DataDescriptorWithSignatureSize = 16;

  /// <summary>
  /// Compression method: Store (no compression).
  /// </summary>
  public const ushort CompressionMethodStore = 0;

  /// <summary>
  /// Compression method: Deflate.
  /// </summary>
  public const ushort CompressionMethodDeflate = 8;

  /// <summary>
  /// Version needed to extract (2.0 for Deflate).
  /// </summary>
  public const ushort VersionNeededToExtract = 20;

  /// <summary>
  /// Version made by (2.0 for compatibility).
  /// </summary>
  public const ushort VersionMadeBy = 20;

  /// <summary>
  /// General purpose bit flag: UTF-8 encoding for file names and comments.
  /// </summary>
  public const ushort Utf8EncodingFlag = 0x0800;

  /// <summary>
  /// General purpose bit flag: Data descriptor follows the compressed data.
  /// </summary>
  public const ushort DataDescriptorFlag = 0x0008;

  /// <summary>
  /// Maximum comment length for End of Central Directory.
  /// </summary>
  public const int MaxCommentLength = 65535;

  #region Zip Bomb Protection

  /// <summary>
  /// Maximum allowed compression ratio (uncompressed/compressed).
  /// A ratio higher than this indicates a potential zip bomb.
  /// Default: 1000:1 - allows highly compressible legitimate content
  /// while catching extreme zip bombs (which often exceed 10000:1).
  /// Note: The streaming decompression size limit provides the primary protection.
  /// </summary>
  public const int MaxCompressionRatio = 1000;

  /// <summary>
  /// Maximum allowed size for a single decompressed entry in bytes.
  /// Default: 1 GB (1,073,741,824 bytes).
  /// </summary>
  public const long MaxSingleEntrySize = 1L << 30;

  /// <summary>
  /// Maximum number of entries allowed in an archive.
  /// Default: 65,535 (ZIP format theoretical limit for standard ZIP).
  /// </summary>
  public const int MaxEntryCount = 65530;

  #endregion

  /// <summary>
  /// Converts a DateTime to MS-DOS date/time format.
  /// </summary>
  /// <param name="dateTime">The DateTime to convert.</param>
  /// <param name="dosDate">The MS-DOS date.</param>
  /// <param name="dosTime">The MS-DOS time.</param>
  public static void DateTimeToDosDateTime(DateTime dateTime, out ushort dosDate, out ushort dosTime) {
    // MS-DOS date format: bits 0-4 = day, bits 5-8 = month, bits 9-15 = year - 1980
    // MS-DOS time format: bits 0-4 = seconds/2, bits 5-10 = minutes, bits 11-15 = hours
    var year = dateTime.Year;
    switch (year) {
      case < 1980:
        dosDate = 0x0021; // January 1, 1980
        dosTime = 0x0000;
        return;
      case > 2107:
        year = 2107;
        break;
    }

    dosDate = (ushort)(dateTime.Day | (dateTime.Month << 5) | ((year - 1980) << 9));
    dosTime = (ushort)((dateTime.Second / 2) | (dateTime.Minute << 5) | (dateTime.Hour << 11));
  }

  /// <summary>
  /// Converts MS-DOS date/time format to a DateTime.
  /// </summary>
  /// <param name="dosDate">The MS-DOS date.</param>
  /// <param name="dosTime">The MS-DOS time.</param>
  /// <returns>The converted DateTime.</returns>
  public static DateTime DosDateTimeToDateTime(ushort dosDate, ushort dosTime) {
    if (dosDate == 0)
      return new(1980, 1, 1, 0, 0, 0, DateTimeKind.Local);

    var year = ((dosDate >> 9) & 0x7F) + 1980;
    var month = (dosDate >> 5) & 0x0F;
    var day = dosDate & 0x1F;

    var hour = (dosTime >> 11) & 0x1F;
    var minute = (dosTime >> 5) & 0x3F;
    var second = (dosTime & 0x1F) * 2;

    // Validate ranges
    if (month is < 1 or > 12)
      month = 1;
    if (day is < 1 or > 31)
      day = 1;
    if (hour > 23)
      hour = 0;
    if (minute > 59)
      minute = 0;
    if (second > 59)
      second = 0;

    try {
      return new(year, month, day, hour, minute, second, DateTimeKind.Local);
    } catch {
      return new(1980, 1, 1, 0, 0, 0, DateTimeKind.Local);
    }
  }
}

#endif
