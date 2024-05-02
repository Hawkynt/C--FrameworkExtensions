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

#if !DEPRECATED_BINARY_FORMATTER

using System.IO;
using Guard;

namespace System.Runtime.Serialization.Formatters.Binary;
/// <summary>
/// Extensions for the BinaryFormatter.
/// </summary>

// ReSharper disable once UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart

public static partial class BinaryFormatterExtensions {
  /// <summary>
  /// Serializes the given object.
  /// </summary>
  /// <param name="this">This BinaryFormatter.</param>
  /// <param name="value">The value to serialize.</param>
  /// <returns>The bytes needed for deserializing the value.</returns>
  public static byte[] Serialize(this BinaryFormatter @this, object value) {
    Against.ThisIsNull(@this);

    using MemoryStream memStream = new();
    @this.Serialize(memStream, value);
    return memStream.ToArray();
  }

  /// <summary>
  /// Serializes the given object and gzips the resulting bytes.
  /// </summary>
  /// <param name="this">This BinaryFormatter.</param>
  /// <param name="value">The value to serialize.</param>
  /// <returns>The bytes needed for deserializing the value.</returns>
  public static byte[] SerializeWithGZip(this BinaryFormatter @this, object value) {
    Against.ThisIsNull(@this);

    var data = @this.Serialize(value);
    return data.GZip();
  }

  /// <summary>
  /// Deserializes the a given byte block.
  /// </summary>
  /// <param name="this">This BinaryFormatter.</param>
  /// <param name="data">The data to deserialize.</param>
  /// <returns>The deserialized value from the byte block.</returns>
  public static object Deserialize(this BinaryFormatter @this, byte[] data) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(data);
    
    using MemoryStream memStream = new(data);
    return @this.Deserialize(memStream);
  }

  /// <summary>
  /// Deserializes the a given gzipped-byte block.
  /// </summary>
  /// <param name="this">This BinaryFormatter.</param>
  /// <param name="data">The data to deserialize.</param>
  /// <returns>The deserialized value from the byte block.</returns>
  public static object DeserializeWithGZip(this BinaryFormatter @this, byte[] data) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(data);

    return @this.Deserialize(data.UnGZip());
  }
}

#endif