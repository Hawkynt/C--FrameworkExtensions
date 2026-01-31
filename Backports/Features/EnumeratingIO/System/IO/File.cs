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

#if !SUPPORTS_ENUMERATING_IO

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class FilePolyfills {

  extension(File) {

    /// <summary>
    /// Reads the lines of a file.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> ReadLines(string path) => ReadLines(path, Encoding.UTF8);

    /// <summary>
    /// Read the lines of a file that has a specified encoding.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
    /// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> ReadLines(string path, Encoding encoding) {
      using var reader = new StreamReader(path, encoding);
      while (reader.ReadLine() is { } line)
        yield return line;
    }

  }
}

#endif
