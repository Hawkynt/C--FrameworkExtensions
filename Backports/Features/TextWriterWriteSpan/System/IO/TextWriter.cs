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

#if !SUPPORTS_TEXTWRITER_WRITE_SPAN

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class TextWriterPolyfills {

  extension(TextWriter @this) {

    /// <summary>
    /// Writes a character span to the text stream.
    /// </summary>
    /// <param name="buffer">The character span to write to the text stream.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<char> buffer) {
      Against.ThisIsNull(@this);
      @this.Write(buffer.ToString());
    }

    /// <summary>
    /// Writes the text representation of a character span to the text stream, followed by a line terminator.
    /// </summary>
    /// <param name="buffer">The character span to write to the text stream.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLine(ReadOnlySpan<char> buffer) {
      Against.ThisIsNull(@this);
      @this.WriteLine(buffer.ToString());
    }

  }

}

#endif
