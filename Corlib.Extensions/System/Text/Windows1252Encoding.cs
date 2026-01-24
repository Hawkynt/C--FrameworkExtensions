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

#nullable enable

namespace System.Text;

/// <summary>
/// Provides access to Windows-1252 (Western European) encoding.
/// </summary>
internal static class Windows1252Encoding {

  /// <summary>
  /// Lookup table for Windows-1252 to Unicode conversion (only the 0x80-0x9F range differs from Latin-1).
  /// </summary>
  private static readonly char[] _mappingTable = [
    '\u20AC', // 0x80 -> Euro Sign
    '\u0081', // 0x81 -> Unused (mapped to control char)
    '\u201A', // 0x82 -> Single Low-9 Quotation Mark
    '\u0192', // 0x83 -> Latin Small Letter F With Hook
    '\u201E', // 0x84 -> Double Low-9 Quotation Mark
    '\u2026', // 0x85 -> Horizontal Ellipsis
    '\u2020', // 0x86 -> Dagger
    '\u2021', // 0x87 -> Double Dagger
    '\u02C6', // 0x88 -> Modifier Letter Circumflex Accent
    '\u2030', // 0x89 -> Per Mille Sign
    '\u0160', // 0x8A -> Latin Capital Letter S With Caron
    '\u2039', // 0x8B -> Single Left-Pointing Angle Quotation Mark
    '\u0152', // 0x8C -> Latin Capital Ligature OE
    '\u008D', // 0x8D -> Unused (mapped to control char)
    '\u017D', // 0x8E -> Latin Capital Letter Z With Caron
    '\u008F', // 0x8F -> Unused (mapped to control char)
    '\u0090', // 0x90 -> Unused (mapped to control char)
    '\u2018', // 0x91 -> Left Single Quotation Mark
    '\u2019', // 0x92 -> Right Single Quotation Mark
    '\u201C', // 0x93 -> Left Double Quotation Mark
    '\u201D', // 0x94 -> Right Double Quotation Mark
    '\u2022', // 0x95 -> Bullet
    '\u2013', // 0x96 -> En Dash
    '\u2014', // 0x97 -> Em Dash
    '\u02DC', // 0x98 -> Small Tilde
    '\u2122', // 0x99 -> Trade Mark Sign
    '\u0161', // 0x9A -> Latin Small Letter S With Caron
    '\u203A', // 0x9B -> Single Right-Pointing Angle Quotation Mark
    '\u0153', // 0x9C -> Latin Small Ligature OE
    '\u009D', // 0x9D -> Unused (mapped to control char)
    '\u017E', // 0x9E -> Latin Small Letter Z With Caron
    '\u0178', // 0x9F -> Latin Capital Letter Y With Diaeresis
  ];

  /// <summary>
  /// Converts a byte in Windows-1252 encoding to its Unicode character equivalent.
  /// </summary>
  public static char ToChar(byte value)
    => value is >= 0x80 and <= 0x9F
      ? _mappingTable[value - 0x80]
      : (char)value;

  /// <summary>
  /// Converts a Unicode character to its Windows-1252 byte equivalent.
  /// Returns the replacement character '?' (0x3F) if the character cannot be represented.
  /// </summary>
  public static byte ToByte(char value) {
    // Direct mapping for ASCII and Latin-1 Supplement (except 0x80-0x9F range)
    if (value <= 0x7F || (value >= 0xA0 && value <= 0xFF))
      return (byte)value;

    // Search the mapping table for special characters
    for (var i = 0; i < _mappingTable.Length; ++i)
      if (_mappingTable[i] == value)
        return (byte)(i + 0x80);

    // Character cannot be represented in Windows-1252
    return (byte)'?';
  }

  /// <summary>
  /// Tries to convert a Unicode character to its Windows-1252 byte equivalent.
  /// </summary>
  /// <returns><see langword="true"/> if the character can be represented; otherwise, <see langword="false"/>.</returns>
  public static bool TryToByte(char value, out byte result) {
    // Direct mapping for ASCII and Latin-1 Supplement (except 0x80-0x9F range)
    if (value <= 0x7F || (value >= 0xA0 && value <= 0xFF)) {
      result = (byte)value;
      return true;
    }

    // Search the mapping table for special characters
    for (var i = 0; i < _mappingTable.Length; ++i)
      if (_mappingTable[i] == value) {
        result = (byte)(i + 0x80);
        return true;
      }

    result = 0;
    return false;
  }

  /// <summary>
  /// Converts a Windows-1252 encoded byte array to a string.
  /// </summary>
  public static string GetString(byte[] bytes) {
    if (bytes == null || bytes.Length == 0)
      return string.Empty;

    var chars = new char[bytes.Length];
    for (var i = 0; i < bytes.Length; ++i)
      chars[i] = ToChar(bytes[i]);

    return new(chars);
  }

  /// <summary>
  /// Converts a Windows-1252 encoded byte array segment to a string.
  /// </summary>
  public static string GetString(byte[] bytes, int index, int count) {
    if (bytes == null || count == 0)
      return string.Empty;

    var chars = new char[count];
    for (var i = 0; i < count; ++i)
      chars[i] = ToChar(bytes[index + i]);

    return new(chars);
  }

  /// <summary>
  /// Converts a Windows-1252 encoded byte span to a string.
  /// </summary>
  public static string GetString(ReadOnlySpan<byte> bytes) {
    if (bytes.IsEmpty)
      return string.Empty;

    var chars = new char[bytes.Length];
    for (var i = 0; i < bytes.Length; ++i)
      chars[i] = ToChar(bytes[i]);

    return new(chars);
  }

  /// <summary>
  /// Converts a string to a Windows-1252 encoded byte array.
  /// Characters that cannot be represented are replaced with '?' (0x3F).
  /// </summary>
  public static byte[] GetBytes(string value) {
    if (string.IsNullOrEmpty(value))
      return [];

    var bytes = new byte[value.Length];
    for (var i = 0; i < value.Length; ++i)
      bytes[i] = ToByte(value[i]);

    return bytes;
  }

  /// <summary>
  /// Converts a character span to a Windows-1252 encoded byte array.
  /// Characters that cannot be represented are replaced with '?' (0x3F).
  /// </summary>
  public static byte[] GetBytes(ReadOnlySpan<char> value) {
    if (value.IsEmpty)
      return [];

    var bytes = new byte[value.Length];
    for (var i = 0; i < value.Length; ++i)
      bytes[i] = ToByte(value[i]);

    return bytes;
  }

}
