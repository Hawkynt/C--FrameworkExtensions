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

using System.Collections.Generic;
using Guard;

namespace System.IO;

/// <summary>
///   Extensions for Streams.
/// </summary>
public static partial class TextReaderExtensions {
  /// <summary>
  ///   Reads the lines of a text reader.
  /// </summary>
  /// <param name="this">This TextReader.</param>
  /// <returns>One line after the other until end of stream is reached.</returns>
  public static IEnumerable<string> ReadLines(this TextReader @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<string> Invoke(TextReader @this) {
      for (;;) {
        var line = @this.ReadLine();
        if (line == null)
          yield break;

        yield return line;
      }
    }
  }
}
