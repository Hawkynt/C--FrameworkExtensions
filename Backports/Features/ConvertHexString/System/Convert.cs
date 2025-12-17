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

#if !SUPPORTS_CONVERT_HEXSTRING

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

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
      return _ToHexString(inArray, 0, inArray.Length);
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
      return _ToHexString(inArray, offset, length);
    }

    /// <summary>
    /// Converts a span of 8-bit unsigned integers to its equivalent string representation that is encoded with uppercase hexadecimal characters.
    /// </summary>
    /// <param name="bytes">A span of 8-bit unsigned integers.</param>
    /// <returns>The string representation in hexadecimal of the elements in <paramref name="bytes"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(ReadOnlySpan<byte> bytes) {
      if (bytes.IsEmpty)
        return string.Empty;

      var result = new char[bytes.Length * 2];
      var index = 0;
      foreach (var b in bytes) {
        result[index++] = _GetHexChar(b >> 4);
        result[index++] = _GetHexChar(b & 0xF);
      }
      return new(result);
    }

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
      if (s.Length == 0)
        return [];

      if ((s.Length & 1) != 0)
        throw new FormatException("The input is not a valid hex string as its length is not a multiple of 2.");

      var result = new byte[s.Length / 2];
      for (var i = 0; i < result.Length; ++i) {
        var high = _GetHexValue(s[i * 2]);
        var low = _GetHexValue(s[i * 2 + 1]);
        if (high < 0 || low < 0)
          throw new FormatException("The input is not a valid hex string as it contains a non-hexadecimal character.");
        result[i] = (byte)((high << 4) | low);
      }
      return result;
    }

    /// <summary>
    /// Converts the span, which encodes binary data as hexadecimal characters, to an equivalent 8-bit unsigned integer array.
    /// </summary>
    /// <param name="chars">The span to convert.</param>
    /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="chars"/>.</returns>
    /// <exception cref="FormatException">The length of <paramref name="chars"/> is not zero or a multiple of 2, or <paramref name="chars"/> contains a non-hexadecimal character.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] FromHexString(ReadOnlySpan<char> chars) {
      if (chars.IsEmpty)
        return [];

      if ((chars.Length & 1) != 0)
        throw new FormatException("The input is not a valid hex string as its length is not a multiple of 2.");

      var result = new byte[chars.Length / 2];
      for (var i = 0; i < result.Length; ++i) {
        var high = _GetHexValue(chars[i * 2]);
        var low = _GetHexValue(chars[i * 2 + 1]);
        if (high < 0 || low < 0)
          throw new FormatException("The input is not a valid hex string as it contains a non-hexadecimal character.");
        result[i] = (byte)((high << 4) | low);
      }
      return result;
    }

  }

  private static string _ToHexString(byte[] inArray, int offset, int length) {
    if (length == 0)
      return string.Empty;

    var result = new char[length * 2];
    var index = 0;
    for (var i = offset; i < offset + length; ++i) {
      result[index++] = _GetHexChar(inArray[i] >> 4);
      result[index++] = _GetHexChar(inArray[i] & 0xF);
    }
    return new(result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static char _GetHexChar(int value)
    => (char)(value < 10 ? '0' + value : 'A' + value - 10);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _GetHexValue(char c) => c switch {
    >= '0' and <= '9' => c - '0',
    >= 'A' and <= 'F' => c - 'A' + 10,
    >= 'a' and <= 'f' => c - 'a' + 10,
    _ => -1
  };

}

#endif
