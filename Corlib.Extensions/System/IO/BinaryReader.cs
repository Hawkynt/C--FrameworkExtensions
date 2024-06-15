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

using Guard;

namespace System.IO;

public static partial class BinaryReaderExtensions {
  /// <summary>
  ///   Reads all bytes from a binarystream's current position.
  /// </summary>
  /// <param name="this">The reader.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <returns>All read bytes.</returns>
  public static byte[] ReadAllBytes(this BinaryReader @this, uint bufferSize = 65536) {
    Against.ThisIsNull(@this);
    Against.ValueIsZero(bufferSize);

    using MemoryStream result = new();
    var buffer = new byte[bufferSize];

    int count;
    while ((count = @this.Read(buffer, 0, buffer.Length)) != 0)
      result.Write(buffer, 0, count);

    return result.ToArray();
  }
}
