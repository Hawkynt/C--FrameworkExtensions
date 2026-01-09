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

// Encoding.TryGetBytes/TryGetChars was added in .NET 8.0
#if !SUPPORTS_ENCODING_TRYGET

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

public static partial class EncodingPolyfills {

  extension(Encoding @this) {

    /// <summary>
    /// Encodes a span of characters into a span of bytes.
    /// </summary>
    /// <param name="chars">The span of characters to encode.</param>
    /// <param name="bytes">The span to write the resulting bytes into.</param>
    /// <param name="bytesWritten">When this method returns, contains the number of bytes written into the span.</param>
    /// <returns><see langword="true"/> if all the characters were encoded into the destination span; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NullReferenceException">The encoding instance is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetBytes(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten) {
      if (@this == null)
        throw new NullReferenceException();

      if (chars.IsEmpty) {
        bytesWritten = 0;
        return true;
      }

      // Calculate required byte count
      var requiredBytes = @this.GetByteCount(chars.ToArray());
      if (bytes.Length < requiredBytes) {
        bytesWritten = 0;
        return false;
      }

      // Encode the characters
      var result = @this.GetBytes(chars.ToArray());
      result.AsSpan().CopyTo(bytes);
      bytesWritten = result.Length;
      return true;
    }

    /// <summary>
    /// Decodes a span of bytes into a span of characters.
    /// </summary>
    /// <param name="bytes">The span of bytes to decode.</param>
    /// <param name="chars">The span to write the resulting characters into.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written into the span.</param>
    /// <returns><see langword="true"/> if all the bytes were decoded into the destination span; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NullReferenceException">The encoding instance is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetChars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten) {
      if (@this == null)
        throw new NullReferenceException();

      if (bytes.IsEmpty) {
        charsWritten = 0;
        return true;
      }

      // Calculate required char count
      var requiredChars = @this.GetCharCount(bytes.ToArray());
      if (chars.Length < requiredChars) {
        charsWritten = 0;
        return false;
      }

      // Decode the bytes
      var result = @this.GetChars(bytes.ToArray());
      result.AsSpan().CopyTo(chars);
      charsWritten = result.Length;
      return true;
    }

  }

}

#endif
