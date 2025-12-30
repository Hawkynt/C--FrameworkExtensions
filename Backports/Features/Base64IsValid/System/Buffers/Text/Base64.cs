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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Buffers.Text;

#if !SUPPORTS_BASE64

// creates the most basic type ever been available at the bcl
public static class Base64 {

}

#endif

#if !SUPPORTS_BASE64_ISVALID
/// <summary>
/// Provides static methods for encoding and decoding data as Base64.
/// </summary>
public static class Base64Polyfills {

  extension(Base64) {

    /// <summary>
    /// Validates that the specified span of text is valid Base64 encoded data.
    /// </summary>
    /// <param name="base64Text">A span of text to validate.</param>
    /// <returns><see langword="true"/> if <paramref name="base64Text"/> is valid Base64; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(ReadOnlySpan<char> base64Text)
      => IsValid(base64Text, out _);

    /// <summary>
    /// Validates that the specified span of text is valid Base64 encoded data and returns the decoded length.
    /// </summary>
    /// <param name="base64Text">A span of text to validate.</param>
    /// <param name="decodedLength">When this method returns, contains the length of the decoded data.</param>
    /// <returns><see langword="true"/> if <paramref name="base64Text"/> is valid Base64; otherwise, <see langword="false"/>.</returns>
    /// <remarks>Whitespace characters (space, tab, newline, carriage return) are ignored.</remarks>
    public static bool IsValid(ReadOnlySpan<char> base64Text, out int decodedLength) {
      decodedLength = 0;

      if (base64Text.IsEmpty)
        return true;

      // Count non-whitespace characters and validate
      var effectiveLength = 0;
      var paddingCount = 0;
      var lastNonWhitespaceIndex = -1;

      // First pass: count effective length and find last non-whitespace
      for (var i = 0; i < base64Text.Length; ++i) {
        var c = base64Text[i];
        if (_IsWhitespace(c))
          continue;

        lastNonWhitespaceIndex = i;
        ++effectiveLength;
      }

      // Empty after stripping whitespace is valid
      if (effectiveLength == 0)
        return true;

      // Count padding from the end (skipping trailing whitespace)
      for (var i = lastNonWhitespaceIndex; i >= 0 && paddingCount < 2; --i) {
        var c = base64Text[i];
        if (_IsWhitespace(c))
          continue;

        if (c == '=')
          ++paddingCount;
        else
          break;
      }

      // Validate that effective length is divisible by 4
      if (effectiveLength % 4 != 0)
        return false;

      // Validate each non-whitespace, non-padding character
      var dataCharsSeen = 0;
      var dataCharsExpected = effectiveLength - paddingCount;
      for (var i = 0; i < base64Text.Length; ++i) {
        var c = base64Text[i];
        if (_IsWhitespace(c))
          continue;

        if (dataCharsSeen < dataCharsExpected) {
          if (!_IsValidBase64Char(c))
            return false;
          ++dataCharsSeen;
        } else {
          // Remaining chars must be padding
          if (c != '=')
            return false;
        }
      }

      // Calculate decoded length: every 4 base64 chars = 3 bytes, minus padding
      decodedLength = (effectiveLength / 4) * 3 - paddingCount;
      return true;
    }

    /// <summary>
    /// Validates that the specified span of UTF-8 encoded text is valid Base64 encoded data.
    /// </summary>
    /// <param name="base64TextUtf8">A span of UTF-8 encoded text to validate.</param>
    /// <returns><see langword="true"/> if <paramref name="base64TextUtf8"/> is valid Base64; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(ReadOnlySpan<byte> base64TextUtf8)
      => IsValid(base64TextUtf8, out _);

    /// <summary>
    /// Validates that the specified span of UTF-8 encoded text is valid Base64 encoded data and returns the decoded length.
    /// </summary>
    /// <param name="base64TextUtf8">A span of UTF-8 encoded text to validate.</param>
    /// <param name="decodedLength">When this method returns, contains the length of the decoded data.</param>
    /// <returns><see langword="true"/> if <paramref name="base64TextUtf8"/> is valid Base64; otherwise, <see langword="false"/>.</returns>
    /// <remarks>Whitespace characters (space, tab, newline, carriage return) are ignored.</remarks>
    public static bool IsValid(ReadOnlySpan<byte> base64TextUtf8, out int decodedLength) {
      decodedLength = 0;

      if (base64TextUtf8.IsEmpty)
        return true;

      // Count non-whitespace characters and validate
      var effectiveLength = 0;
      var paddingCount = 0;
      var lastNonWhitespaceIndex = -1;

      // First pass: count effective length and find last non-whitespace
      for (var i = 0; i < base64TextUtf8.Length; ++i) {
        var b = base64TextUtf8[i];
        if (_IsWhitespace(b))
          continue;

        lastNonWhitespaceIndex = i;
        ++effectiveLength;
      }

      // Empty after stripping whitespace is valid
      if (effectiveLength == 0)
        return true;

      // Count padding from the end (skipping trailing whitespace)
      for (var i = lastNonWhitespaceIndex; i >= 0 && paddingCount < 2; --i) {
        var b = base64TextUtf8[i];
        if (_IsWhitespace(b))
          continue;

        if (b == (byte)'=')
          ++paddingCount;
        else
          break;
      }

      // Validate that effective length is divisible by 4
      if (effectiveLength % 4 != 0)
        return false;

      // Validate each non-whitespace, non-padding character
      var dataCharsSeen = 0;
      var dataCharsExpected = effectiveLength - paddingCount;
      for (var i = 0; i < base64TextUtf8.Length; ++i) {
        var b = base64TextUtf8[i];
        if (_IsWhitespace(b))
          continue;

        if (dataCharsSeen < dataCharsExpected) {
          if (!_IsValidBase64Byte(b))
            return false;
          ++dataCharsSeen;
        } else {
          // Remaining bytes must be padding
          if (b != (byte)'=')
            return false;
        }
      }

      // Calculate decoded length: every 4 base64 chars = 3 bytes, minus padding
      decodedLength = (effectiveLength / 4) * 3 - paddingCount;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool _IsValidBase64Char(char c)
      => c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') or '+' or '/';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool _IsValidBase64Byte(byte b)
      => b is (>= (byte)'A' and <= (byte)'Z') or (>= (byte)'a' and <= (byte)'z') or (>= (byte)'0' and <= (byte)'9') or (byte)'+' or (byte)'/';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool _IsWhitespace(char c)
      => c is ' ' or '\t' or '\n' or '\r';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool _IsWhitespace(byte b)
      => b is (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r';

  }
}

#endif
