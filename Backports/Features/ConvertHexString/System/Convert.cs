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

// Include this file when either ToHexString or ToHexStringLower needs polyfilling
#if !SUPPORTS_CONVERT_HEXSTRING || !SUPPORTS_CONVERT_TOHEXSTRINGLOWER

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class ConvertPolyfills {

  #region arithmetic constants

  // Hex offset for uppercase: 'A' - '0' - 10 = 7
  // Hex offset for lowercase: 'a' - '0' - 10 = 39
  private const int HexOffsetUpper = 'A' - '0' - 10; // 7
  private const int HexOffsetLower = 'a' - '0' - 10; // 39

  #endregion

  #region core implementations

  // Convert nibble to hex char using arithmetic:
  // If nibble < 10: char = nibble + '0'
  // If nibble >= 10: char = nibble + '0' + offset (where offset is 7 for uppercase, 39 for lowercase)
  // Using bit manipulation: offset & ~((nibble - 10) >> 31)
  // When nibble < 10: (nibble - 10) is negative, >> 31 gives -1 (all 1s), ~ gives 0, offset & 0 = 0
  // When nibble >= 10: (nibble - 10) is >= 0, >> 31 gives 0, ~ gives -1, offset & -1 = offset
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _ByteToHexPair(int b, int hexOffset) {
    var hi = b >> 4;
    var lo = b & 0xF;
    var hiChar = hi + '0' + (hexOffset & ~((hi - 10) >> 31));
    var loChar = lo + '0' + (hexOffset & ~((lo - 10) >> 31));
    // Pack 2 chars into uint: first char in low 16 bits, second in high 16 bits (little-endian)
    return (uint)(hiChar | (loChar << 16));
  }

  private static unsafe string _ToHexStringCore(byte[] inArray, int offset, int length, int hexOffset) {
    if (length == 0)
      return string.Empty;

    var result = new char[length * 2];

    fixed (byte* pBytes = &inArray[offset])
    fixed (char* pChars = result) {
      var src = pBytes;
      var dst = (uint*)pChars;
      var end = src + length;

      // Process 8 bytes at a time
      var end8 = src + (length & ~7);
      while (src < end8) {
        dst[0] = _ByteToHexPair(src[0], hexOffset);
        dst[1] = _ByteToHexPair(src[1], hexOffset);
        dst[2] = _ByteToHexPair(src[2], hexOffset);
        dst[3] = _ByteToHexPair(src[3], hexOffset);
        dst[4] = _ByteToHexPair(src[4], hexOffset);
        dst[5] = _ByteToHexPair(src[5], hexOffset);
        dst[6] = _ByteToHexPair(src[6], hexOffset);
        dst[7] = _ByteToHexPair(src[7], hexOffset);
        src += 8;
        dst += 8;
      }

      // Handle remaining bytes
      while (src < end)
        *dst++ = _ByteToHexPair(*src++, hexOffset);
    }

    return new(result);
  }

  private static unsafe string _ToHexStringCore(ReadOnlySpan<byte> bytes, int hexOffset) {
    if (bytes.IsEmpty)
      return string.Empty;

    var length = bytes.Length;
    var result = new char[length * 2];

    fixed (byte* pBytes = bytes)
    fixed (char* pChars = result) {
      var src = pBytes;
      var dst = (uint*)pChars;
      var end = src + length;

      // Process 8 bytes at a time
      var end8 = src + (length & ~7);
      while (src < end8) {
        dst[0] = _ByteToHexPair(src[0], hexOffset);
        dst[1] = _ByteToHexPair(src[1], hexOffset);
        dst[2] = _ByteToHexPair(src[2], hexOffset);
        dst[3] = _ByteToHexPair(src[3], hexOffset);
        dst[4] = _ByteToHexPair(src[4], hexOffset);
        dst[5] = _ByteToHexPair(src[5], hexOffset);
        dst[6] = _ByteToHexPair(src[6], hexOffset);
        dst[7] = _ByteToHexPair(src[7], hexOffset);
        src += 8;
        dst += 8;
      }

      // Handle remaining bytes
      while (src < end)
        *dst++ = _ByteToHexPair(*src++, hexOffset);
    }

    return new(result);
  }

#if !SUPPORTS_CONVERT_HEXSTRING

  // Convert hex char to nibble using arithmetic:
  // Digit '0'-'9': subtract '0' (0x30)
  // Upper 'A'-'F': subtract 'A' - 10 = 55 (0x37)
  // Lower 'a'-'f': subtract 'a' - 10 = 87 (0x57)
  // Normalize to lowercase by OR with 0x20, then check ranges
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _HexCharToNibble(int c) {
    // Check if digit (0x30-0x39)
    var digit = c - '0';
    if ((uint)digit <= 9)
      return digit;

    // Check if letter (normalize to lowercase)
    var letter = (c | 0x20) - 'a';
    if ((uint)letter <= 5)
      return letter + 10;

    return -1; // Invalid
  }

  private static unsafe byte[] _FromHexStringCore(ReadOnlySpan<char> chars) {
    if (chars.IsEmpty)
      return [];

    var charLength = chars.Length;
    if ((charLength & 1) != 0)
      throw new FormatException("The input is not a valid hex string as its length is not a multiple of 2.");

    var byteLength = charLength >> 1;
    var result = new byte[byteLength];

    fixed (char* pChars = chars)
    fixed (byte* pBytes = result) {
      var src = pChars;
      var dst = pBytes;
      var end = dst + byteLength;

      // Process 4 bytes at a time (8 hex chars)
      var end4 = dst + (byteLength & ~3);
      while (dst < end4) {
        var n0 = _HexCharToNibble(src[0]);
        var n1 = _HexCharToNibble(src[1]);
        var n2 = _HexCharToNibble(src[2]);
        var n3 = _HexCharToNibble(src[3]);
        var n4 = _HexCharToNibble(src[4]);
        var n5 = _HexCharToNibble(src[5]);
        var n6 = _HexCharToNibble(src[6]);
        var n7 = _HexCharToNibble(src[7]);

        // Check for invalid nibbles (-1)
        if ((n0 | n1 | n2 | n3 | n4 | n5 | n6 | n7) < 0)
          throw new FormatException("The input is not a valid hex string as it contains a non-hexadecimal character.");

        dst[0] = (byte)((n0 << 4) | n1);
        dst[1] = (byte)((n2 << 4) | n3);
        dst[2] = (byte)((n4 << 4) | n5);
        dst[3] = (byte)((n6 << 4) | n7);

        src += 8;
        dst += 4;
      }

      // Handle remaining bytes
      while (dst < end) {
        var n0 = _HexCharToNibble(src[0]);
        var n1 = _HexCharToNibble(src[1]);

        if ((n0 | n1) < 0)
          throw new FormatException("The input is not a valid hex string as it contains a non-hexadecimal character.");

        *dst++ = (byte)((n0 << 4) | n1);
        src += 2;
      }

      return result;
    }
  }

#endif

  #endregion

}

#if !SUPPORTS_CONVERT_HEXSTRING

public static partial class ConvertPolyfills {

  extension(Convert) {

    /// <summary>
    /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with uppercase hexadecimal characters.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <returns>The string representation in hexadecimal of the elements in <paramref name="inArray"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(byte[] inArray) {
      ArgumentNullException.ThrowIfNull(inArray);
      return _ToHexStringCore(inArray, 0, inArray.Length, HexOffsetUpper);
    }

    /// <summary>
    /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with uppercase hexadecimal characters.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <param name="offset">An offset in <paramref name="inArray"/>.</param>
    /// <param name="length">The number of elements of <paramref name="inArray"/> to convert.</param>
    /// <returns>The string representation in hexadecimal of <paramref name="length"/> elements of <paramref name="inArray"/>, starting at position <paramref name="offset"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="length"/> is negative, or <paramref name="offset"/> plus <paramref name="length"/> is greater than the length of <paramref name="inArray"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(byte[] inArray, int offset, int length) {
      ArgumentNullException.ThrowIfNull(inArray);
      ArgumentOutOfRangeException.ThrowIfNegative(offset);
      ArgumentOutOfRangeException.ThrowIfNegative(length);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + length, inArray.Length);
      return _ToHexStringCore(inArray, offset, length, HexOffsetUpper);
    }

    /// <summary>
    /// Converts a span of 8-bit unsigned integers to its equivalent string representation that is encoded with uppercase hexadecimal characters.
    /// </summary>
    /// <param name="bytes">A span of 8-bit unsigned integers.</param>
    /// <returns>The string representation in hexadecimal of the elements in <paramref name="bytes"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(ReadOnlySpan<byte> bytes) => _ToHexStringCore(bytes, HexOffsetUpper);

    /// <summary>
    /// Converts the specified string, which encodes binary data as hexadecimal characters, to an equivalent 8-bit unsigned integer array.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="s"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The length of <paramref name="s"/> is not zero or a multiple of 2, or <paramref name="s"/> contains a non-hexadecimal character.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] FromHexString(string s) {
      ArgumentNullException.ThrowIfNull(s);
      return _FromHexStringCore(s.AsSpan());
    }

    /// <summary>
    /// Converts the span, which encodes binary data as hexadecimal characters, to an equivalent 8-bit unsigned integer array.
    /// </summary>
    /// <param name="chars">The span to convert.</param>
    /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="chars"/>.</returns>
    /// <exception cref="FormatException">The length of <paramref name="chars"/> is not zero or a multiple of 2, or <paramref name="chars"/> contains a non-hexadecimal character.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] FromHexString(ReadOnlySpan<char> chars) => _FromHexStringCore(chars);

  }

}

#endif

#if !SUPPORTS_CONVERT_TOHEXSTRINGLOWER

public static partial class ConvertPolyfills {

  extension(Convert) {

    /// <summary>
    /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with lowercase hexadecimal characters.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <returns>The string representation in lowercase hexadecimal of the elements in <paramref name="inArray"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexStringLower(byte[] inArray) {
      ArgumentNullException.ThrowIfNull(inArray);
      return _ToHexStringCore(inArray, 0, inArray.Length, HexOffsetLower);
    }

    /// <summary>
    /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with lowercase hexadecimal characters.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <param name="offset">An offset in <paramref name="inArray"/>.</param>
    /// <param name="length">The number of elements of <paramref name="inArray"/> to convert.</param>
    /// <returns>The string representation in lowercase hexadecimal of <paramref name="length"/> elements of <paramref name="inArray"/>, starting at position <paramref name="offset"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="length"/> is negative, or <paramref name="offset"/> plus <paramref name="length"/> is greater than the length of <paramref name="inArray"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexStringLower(byte[] inArray, int offset, int length) {
      ArgumentNullException.ThrowIfNull(inArray);
      ArgumentOutOfRangeException.ThrowIfNegative(offset);
      ArgumentOutOfRangeException.ThrowIfNegative(length);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + length, inArray.Length);
      return _ToHexStringCore(inArray, offset, length, HexOffsetLower);
    }

    /// <summary>
    /// Converts a span of 8-bit unsigned integers to its equivalent string representation that is encoded with lowercase hexadecimal characters.
    /// </summary>
    /// <param name="bytes">A span of 8-bit unsigned integers.</param>
    /// <returns>The string representation in lowercase hexadecimal of the elements in <paramref name="bytes"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexStringLower(ReadOnlySpan<byte> bytes) => _ToHexStringCore(bytes, HexOffsetLower);

  }

}

#endif

#endif
