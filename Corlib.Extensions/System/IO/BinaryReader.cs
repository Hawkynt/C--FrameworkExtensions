#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using Guard;

namespace System.IO;

public static partial class BinaryReaderExtensions {

  /// <summary>
  /// Reads all bytes from a binarystream's current position.
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