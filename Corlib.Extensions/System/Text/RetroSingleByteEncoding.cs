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

using System.Collections.Generic;

namespace System.Text;

/// <summary>
/// Base for simple single-byte retro character encodings (one byte per character) backed by a 256-entry
/// byte-&gt;Unicode table. Unrepresentable characters encode to <c>'?'</c> (0x3F).
/// </summary>
public abstract class RetroSingleByteEncoding : Encoding {

  private const byte _REPLACEMENT_BYTE = 0x3F; // '?'

  private readonly char[] _toUnicode;
  private readonly Dictionary<char, byte> _fromUnicode;

  /// <param name="toUnicode">A 256-entry byte-&gt;character table.</param>
  protected RetroSingleByteEncoding(char[] toUnicode) {
    if (toUnicode == null)
      throw new ArgumentNullException(nameof(toUnicode));
    if (toUnicode.Length != 256)
      throw new ArgumentException("Table must have exactly 256 entries.", nameof(toUnicode));

    this._toUnicode = toUnicode;
    this._fromUnicode = new Dictionary<char, byte>(256);
    for (var b = 255; b >= 0; --b) // low byte wins on any duplicate glyph
      this._fromUnicode[toUnicode[b]] = (byte)b;
  }

  /// <summary>Converts a Unicode character to its byte (or the code for '?' if unrepresentable).</summary>
  public byte ToByte(char value) => this._fromUnicode.TryGetValue(value, out var b) ? b : _REPLACEMENT_BYTE;

  /// <summary>Converts a byte to its Unicode character.</summary>
  public char ToChar(byte value) => this._toUnicode[value];

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
      bytes[byteIndex + i] = this.ToByte(chars[charIndex + i]);

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
      chars[charIndex + i] = this._toUnicode[bytes[byteIndex + i]];

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
