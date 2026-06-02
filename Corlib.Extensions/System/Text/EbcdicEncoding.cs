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
/// Provides access to the IBM EBCDIC code page 037 (CP037) encoding (USA/Canada).
/// </summary>
/// <remarks>
/// This is a single-byte encoding: each <see cref="char"/> maps to exactly one byte and vice versa.
/// Characters that cannot be represented are encoded as the EBCDIC code for <c>'?'</c> (<c>0x6F</c>).
/// </remarks>
public sealed class EbcdicEncoding : Encoding {

  /// <summary>
  /// EBCDIC code for the question mark, used as the replacement byte for unmappable characters.
  /// </summary>
  private const byte _REPLACEMENT_BYTE = 0x6F;

  /// <summary>
  /// Lookup table mapping an EBCDIC (CP037) byte to its Unicode character (as a code point).
  /// Index = EBCDIC byte value, value = Unicode code point. Control codes use their numeric values to keep
  /// the source portable (no raw control characters embedded in literals).
  /// </summary>
  private static readonly char[] _toUnicode = BuildTable([
    /* 0x00 */ 0x0000, 0x0001, 0x0002, 0x0003, 0x009C, 0x0009, 0x0086, 0x007F,
    /* 0x08 */ 0x0097, 0x008D, 0x008E, 0x000B, 0x000C, 0x000D, 0x000E, 0x000F,
    /* 0x10 */ 0x0010, 0x0011, 0x0012, 0x0013, 0x009D, 0x0085, 0x0008, 0x0087,
    /* 0x18 */ 0x0018, 0x0019, 0x0092, 0x008F, 0x001C, 0x001D, 0x001E, 0x001F,
    /* 0x20 */ 0x0080, 0x0081, 0x0082, 0x0083, 0x0084, 0x000A, 0x0017, 0x001B,
    /* 0x28 */ 0x0088, 0x0089, 0x008A, 0x008B, 0x008C, 0x0005, 0x0006, 0x0007,
    /* 0x30 */ 0x0090, 0x0091, 0x0016, 0x0093, 0x0094, 0x0095, 0x0096, 0x0004,
    /* 0x38 */ 0x0098, 0x0099, 0x009A, 0x009B, 0x0014, 0x0015, 0x009E, 0x001A,
    /* 0x40 */ 0x0020, 0x00A0, 0x00E2, 0x00E4, 0x00E0, 0x00E1, 0x00E3, 0x00E5,
    /* 0x48 */ 0x00E7, 0x00F1, 0x00A2, 0x002E, 0x003C, 0x0028, 0x002B, 0x007C,
    /* 0x50 */ 0x0026, 0x00E9, 0x00EA, 0x00EB, 0x00E8, 0x00ED, 0x00EE, 0x00EF,
    /* 0x58 */ 0x00EC, 0x00DF, 0x0021, 0x0024, 0x002A, 0x0029, 0x003B, 0x00AC,
    /* 0x60 */ 0x002D, 0x002F, 0x00C2, 0x00C4, 0x00C0, 0x00C1, 0x00C3, 0x00C5,
    /* 0x68 */ 0x00C7, 0x00D1, 0x00A6, 0x002C, 0x0025, 0x005F, 0x003E, 0x003F,
    /* 0x70 */ 0x00F8, 0x00C9, 0x00CA, 0x00CB, 0x00C8, 0x00CD, 0x00CE, 0x00CF,
    /* 0x78 */ 0x00CC, 0x0060, 0x003A, 0x0023, 0x0040, 0x0027, 0x003D, 0x0022,
    /* 0x80 */ 0x00D8, 0x0061, 0x0062, 0x0063, 0x0064, 0x0065, 0x0066, 0x0067,
    /* 0x88 */ 0x0068, 0x0069, 0x00AB, 0x00BB, 0x00F0, 0x00FD, 0x00FE, 0x00B1,
    /* 0x90 */ 0x00B0, 0x006A, 0x006B, 0x006C, 0x006D, 0x006E, 0x006F, 0x0070,
    /* 0x98 */ 0x0071, 0x0072, 0x00AA, 0x00BA, 0x00E6, 0x00B8, 0x00C6, 0x00A4,
    /* 0xA0 */ 0x00B5, 0x007E, 0x0073, 0x0074, 0x0075, 0x0076, 0x0077, 0x0078,
    /* 0xA8 */ 0x0079, 0x007A, 0x00A1, 0x00BF, 0x00D0, 0x00DD, 0x00DE, 0x00AE,
    /* 0xB0 */ 0x005E, 0x00A3, 0x00A5, 0x00B7, 0x00A9, 0x00A7, 0x00B6, 0x00BC,
    /* 0xB8 */ 0x00BD, 0x00BE, 0x005B, 0x005D, 0x00AF, 0x00A8, 0x00B4, 0x00D7,
    /* 0xC0 */ 0x007B, 0x0041, 0x0042, 0x0043, 0x0044, 0x0045, 0x0046, 0x0047,
    /* 0xC8 */ 0x0048, 0x0049, 0x00AD, 0x00F4, 0x00F6, 0x00F2, 0x00F3, 0x00F5,
    /* 0xD0 */ 0x007D, 0x004A, 0x004B, 0x004C, 0x004D, 0x004E, 0x004F, 0x0050,
    /* 0xD8 */ 0x0051, 0x0052, 0x00B9, 0x00FB, 0x00FC, 0x00F9, 0x00FA, 0x00FF,
    /* 0xE0 */ 0x005C, 0x00F7, 0x0053, 0x0054, 0x0055, 0x0056, 0x0057, 0x0058,
    /* 0xE8 */ 0x0059, 0x005A, 0x00B2, 0x00D4, 0x00D6, 0x00D2, 0x00D3, 0x00D5,
    /* 0xF0 */ 0x0030, 0x0031, 0x0032, 0x0033, 0x0034, 0x0035, 0x0036, 0x0037,
    /* 0xF8 */ 0x0038, 0x0039, 0x00B3, 0x00DB, 0x00DC, 0x00D9, 0x00DA, 0x009F
  ]);

  private static char[] BuildTable(int[] codePoints) {
    var result = new char[codePoints.Length];
    for (var i = 0; i < codePoints.Length; ++i)
      result[i] = (char)codePoints[i];
    return result;
  }

  /// <summary>
  /// Reverse lookup table mapping a Unicode code point (0x0000-0xFFFF) to its EBCDIC (CP037) byte.
  /// Unmapped characters default to <see cref="_REPLACEMENT_BYTE"/>.
  /// </summary>
  private static readonly byte[] _fromUnicode;

  static EbcdicEncoding() {
    var reverse = new byte[char.MaxValue + 1];
    for (var i = 0; i < reverse.Length; ++i)
      reverse[i] = _REPLACEMENT_BYTE;

    // Walk backwards so that, on Unicode collisions, the lowest EBCDIC byte wins for a given char.
    for (var b = _toUnicode.Length - 1; b >= 0; --b)
      reverse[_toUnicode[b]] = (byte)b;

    _fromUnicode = reverse;
  }

  /// <summary>
  /// Gets a shared, thread-safe <see cref="EbcdicEncoding"/> instance for code page 037.
  /// </summary>
  public static EbcdicEncoding CP037 { get; } = new();

  /// <summary>
  /// Converts a single Unicode character to its EBCDIC (CP037) byte.
  /// </summary>
  /// <param name="value">The character to convert.</param>
  /// <returns>The EBCDIC byte, or the EBCDIC code for <c>'?'</c> if the character cannot be represented.</returns>
  public static byte ToEbcdic(char value) => _fromUnicode[value];

  /// <summary>
  /// Converts a single EBCDIC (CP037) byte to its Unicode character.
  /// </summary>
  /// <param name="value">The EBCDIC byte to convert.</param>
  /// <returns>The corresponding Unicode character.</returns>
  public static char FromEbcdic(byte value) => _toUnicode[value];

  #region Encoding overrides

  /// <inheritdoc />
  public override int GetByteCount(char[] chars, int index, int count) {
    if (chars == null)
      throw new ArgumentNullException(nameof(chars));
    if (index < 0)
      throw new ArgumentOutOfRangeException(nameof(index));
    if (count < 0)
      throw new ArgumentOutOfRangeException(nameof(count));
    if (index > chars.Length - count)
      throw new ArgumentOutOfRangeException(nameof(chars));

    return count;
  }

  /// <inheritdoc />
  public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
    if (chars == null)
      throw new ArgumentNullException(nameof(chars));
    if (bytes == null)
      throw new ArgumentNullException(nameof(bytes));
    if (charIndex < 0)
      throw new ArgumentOutOfRangeException(nameof(charIndex));
    if (charCount < 0)
      throw new ArgumentOutOfRangeException(nameof(charCount));
    if (charIndex > chars.Length - charCount)
      throw new ArgumentOutOfRangeException(nameof(chars));
    if (byteIndex < 0 || byteIndex > bytes.Length)
      throw new ArgumentOutOfRangeException(nameof(byteIndex));
    if (bytes.Length - byteIndex < charCount)
      throw new ArgumentException("The output byte buffer is too small.", nameof(bytes));

    for (var i = 0; i < charCount; ++i)
      bytes[byteIndex + i] = _fromUnicode[chars[charIndex + i]];

    return charCount;
  }

  /// <inheritdoc />
  public override int GetCharCount(byte[] bytes, int index, int count) {
    if (bytes == null)
      throw new ArgumentNullException(nameof(bytes));
    if (index < 0)
      throw new ArgumentOutOfRangeException(nameof(index));
    if (count < 0)
      throw new ArgumentOutOfRangeException(nameof(count));
    if (index > bytes.Length - count)
      throw new ArgumentOutOfRangeException(nameof(bytes));

    return count;
  }

  /// <inheritdoc />
  public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
    if (bytes == null)
      throw new ArgumentNullException(nameof(bytes));
    if (chars == null)
      throw new ArgumentNullException(nameof(chars));
    if (byteIndex < 0)
      throw new ArgumentOutOfRangeException(nameof(byteIndex));
    if (byteCount < 0)
      throw new ArgumentOutOfRangeException(nameof(byteCount));
    if (byteIndex > bytes.Length - byteCount)
      throw new ArgumentOutOfRangeException(nameof(bytes));
    if (charIndex < 0 || charIndex > chars.Length)
      throw new ArgumentOutOfRangeException(nameof(charIndex));
    if (chars.Length - charIndex < byteCount)
      throw new ArgumentException("The output char buffer is too small.", nameof(chars));

    for (var i = 0; i < byteCount; ++i)
      chars[charIndex + i] = _toUnicode[bytes[byteIndex + i]];

    return byteCount;
  }

  /// <inheritdoc />
  public override int GetMaxByteCount(int charCount) {
    if (charCount < 0)
      throw new ArgumentOutOfRangeException(nameof(charCount));

    return charCount;
  }

  /// <inheritdoc />
  public override int GetMaxCharCount(int byteCount) {
    if (byteCount < 0)
      throw new ArgumentOutOfRangeException(nameof(byteCount));

    return byteCount;
  }

  #endregion

}
