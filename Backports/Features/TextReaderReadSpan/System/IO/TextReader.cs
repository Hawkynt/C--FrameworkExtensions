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

#if !SUPPORTS_TEXTREADER_READ_SPAN

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class TextReaderPolyfills {

  extension(TextReader @this) {

    /// <summary>
    /// Reads the characters from the current reader and writes the data to the specified buffer.
    /// </summary>
    /// <param name="buffer">When this method returns, contains the specified span of characters replaced by the characters read from the current source.</param>
    /// <returns>The number of characters that have been read. The number will be less than or equal to <paramref name="buffer"/>.Length, depending on whether all characters have been read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(Span<char> buffer) {
      Against.ThisIsNull(@this);

      var array = new char[buffer.Length];
      var read = @this.Read(array, 0, array.Length);
      array.AsSpan(0, read).CopyTo(buffer);
      return read;
    }

    /// <summary>
    /// Reads all the characters from the input string and writes them to the buffer.
    /// </summary>
    /// <param name="buffer">When this method returns, contains the specified span of characters replaced by the characters read from the current source.</param>
    /// <returns>The number of characters that have been read. The number will be less than or equal to <paramref name="buffer"/>.Length.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadBlock(Span<char> buffer) {
      Against.ThisIsNull(@this);

      var array = new char[buffer.Length];
      var read = @this.ReadBlock(array, 0, array.Length);
      array.AsSpan(0, read).CopyTo(buffer);
      return read;
    }

  }

}

#endif
