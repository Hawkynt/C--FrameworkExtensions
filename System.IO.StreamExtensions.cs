#region (c)2010-2020 Hawkynt
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

using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using qword = System.UInt64;
namespace System.IO {
  /// <summary>
  /// Extensions for Streams.
  /// </summary>
  internal static partial class StreamExtensions {
    /// <summary>
    /// Writes a whole array of bytes to a stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="data">The data to write.</param>
    public static void Write(this Stream This, byte[] data) {
      Contract.Requires(This != null);
      Contract.Requires(data != null);
      Contract.Requires(This.CanWrite);
      This.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Fills a whole array with bytes from a stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="result">The array where to store the results.</param>
    /// <returns>The number of bytes actually read.</returns>
    public static int Read(this Stream This, byte[] result) {
      Contract.Requires(This != null);
      Contract.Requires(result != null);
      Contract.Requires(This.CanRead);
      return (This.Read(result, 0, result.Length));
    }

    /// <summary>
    /// Tries to read a given number of bytes from a stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The number of bytes actually read.</returns>
    public static byte[] ReadBytes(this Stream This, int length) {
      Contract.Requires(This != null);
      Contract.Requires(length >= 0);
      Contract.Requires(This.CanRead);
      var result = new byte[length];
      var bytesGot = This.Read(result, 0, length);
      return (bytesGot == length ? result : result.Take(bytesGot).ToArray());
    }

    /// <summary>
    /// Writes the given int value to a stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="value">The value.</param>
    /// <param name="bigEndian">if set to <c>true</c> the int gets written in big-endian format; otherwise little-endian is used (default).</param>
    public static void Write(this Stream This, int value, bool bigEndian = false) {
      Contract.Requires(This != null);
      Contract.Requires(This.CanWrite);
      if (bigEndian) {
        This.WriteByte((byte)(value >> 24));
        This.WriteByte((byte)(value >> 16));
        This.WriteByte((byte)(value >> 8));
        This.WriteByte((byte)(value >> 0));
      } else {
        This.WriteByte((byte)(value >> 0));
        This.WriteByte((byte)(value >> 8));
        This.WriteByte((byte)(value >> 16));
        This.WriteByte((byte)(value >> 24));
      }
    }

    /// <summary>
    /// Reads an int value from the stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="bigEndian">if set to <c>true</c> the int gets read in big-endian format; otherwise little-endian is used (default).</param>
    /// <returns>The int-value that was read.</returns>
    public static int ReadInt(this Stream This, bool bigEndian = false) {
      Contract.Requires(This != null);
      Contract.Requires(This.CanRead);
      var bytes = new[] {
        This.ReadByte(),
        This.ReadByte(),
        This.ReadByte(),
        This.ReadByte()
      };
      return (
        bigEndian
        ? (bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3] << 0)
        : (bytes[0] << 0 | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24)
      );
    }

    /// <summary>
    /// Determines whether the current stream position pointer is at end of the stream or not.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <returns>
    ///   <c>true</c> if the stream was read/written to its end; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAtEndOfStream(this Stream This) {
      Contract.Requires(This != null);
      return (This.Position >= This.Length);
    }

    /// <summary>
    /// Copies the whole stream to an array.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <returns>The content of the stream.</returns>
    public static byte[] ToArray(this Stream This) {
      Contract.Requires(This != null);
      using (var data = new MemoryStream()) {
        This.CopyTo(data);
        return (data.ToArray());
      }
    }

    /// <summary>
    /// Reads all text from the stream..
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns>The text from the stream.</returns>
    public static string ReadAllText(this Stream This, Encoding encoding = null) {
      Contract.Requires(This != null);
      if (encoding == null)
        encoding = Encoding.Default;

      return (This.CanRead ? encoding.GetString(This.ToArray()) : null);
    }


    public static void WriteAllText(this Stream This, string data, Encoding encoding = null) {
      Contract.Requires(This != null);
      if (encoding == null)
        encoding = Encoding.Default;

      This.Write(encoding.GetBytes(data));
    }
  }

}