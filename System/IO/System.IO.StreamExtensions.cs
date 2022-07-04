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

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.IO {
  /// <summary>
  ///   Extensions for Streams.
  /// </summary>
  internal static partial class StreamExtensions {
    /// <summary>
    ///   Writes a whole array of bytes to a stream.
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
    ///   Fills a whole array with bytes from a stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="result">The array where to store the results.</param>
    /// <returns>The number of bytes actually read.</returns>
    public static int Read(this Stream This, byte[] result) {
      Contract.Requires(This != null);
      Contract.Requires(result != null);
      Contract.Requires(This.CanRead);
      return This.Read(result, 0, result.Length);
    }

    /// <summary>
    ///   Tries to read a given number of bytes from a stream.
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
      return bytesGot == length ? result : result.Take(bytesGot).ToArray();
    }

    /// <summary>
    ///   Writes the given int value to a stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="value">The value.</param>
    /// <param name="bigEndian">
    ///   if set to <c>true</c> the int gets written in big-endian format; otherwise little-endian is
    ///   used (default).
    /// </param>
    public static void Write(this Stream This, int value, bool bigEndian = false) {
      Contract.Requires(This != null);
      Contract.Requires(This.CanWrite);
      if (bigEndian) {
        This.WriteByte((byte) (value >> 24));
        This.WriteByte((byte) (value >> 16));
        This.WriteByte((byte) (value >> 8));
        This.WriteByte((byte) (value >> 0));
      } else {
        This.WriteByte((byte) (value >> 0));
        This.WriteByte((byte) (value >> 8));
        This.WriteByte((byte) (value >> 16));
        This.WriteByte((byte) (value >> 24));
      }
    }

    /// <summary>
    ///   Reads an int value from the stream.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="bigEndian">
    ///   if set to <c>true</c> the int gets read in big-endian format; otherwise little-endian is used
    ///   (default).
    /// </param>
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
      return bigEndian
        ? (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | (bytes[3] << 0)
        : (bytes[0] << 0) | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }

    /// <summary>
    ///   Determines whether the current stream position pointer is at end of the stream or not.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <returns>
    ///   <c>true</c> if the stream was read/written to its end; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAtEndOfStream(this Stream This) {
      Contract.Requires(This != null);
      return This.Position >= This.Length;
    }

    /// <summary>
    ///   Copies the whole stream to an array.
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <returns>The content of the stream.</returns>
    public static byte[] ToArray(this Stream This) {
      Contract.Requires(This != null);
      using (var data = new MemoryStream()) {
        This.CopyTo(data);
        return data.ToArray();
      }
    }

    /// <summary>
    ///   Reads all text from the stream..
    /// </summary>
    /// <param name="This">This Stream.</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns>The text from the stream.</returns>
    public static string ReadAllText(this Stream This, Encoding encoding = null) {
      Contract.Requires(This != null);
      if (encoding == null)
        encoding = Encoding.Default;

      return This.CanRead ? encoding.GetString(This.ToArray()) : null;
    }

    /// <summary>
    ///   Writes all text.
    /// </summary>
    /// <param name="this">This <see cref="Stream">Stream</see>.</param>
    /// <param name="data">The data.</param>
    /// <param name="encoding">The encoding.</param>
    public static void WriteAllText(this Stream @this, string data, Encoding encoding = null) {
      Contract.Requires(@this != null);
      if (encoding == null)
        encoding = Encoding.Default;

      @this.Write(encoding.GetBytes(data));
    }

    /// <summary>
    ///   Reads a struct from the given stream.
    /// </summary>
    /// <typeparam name="TStruct">The type of the structure.</typeparam>
    /// <param name="this">This <see cref="Stream">Stream</see>.</param>
    /// <returns>The filled structure.</returns>
    public static TStruct Read<TStruct>(this Stream @this) where TStruct : struct {
      var size = Marshal.SizeOf(typeof(TStruct));
      var buffer = new byte[size];
      @this.Read(buffer, 0, size);
      return _BytesToStruct<TStruct>(buffer);
    }

    /// <summary>
    ///   Converts a managed byte array to a structure.
    /// </summary>
    /// <typeparam name="TStruct">The type of the structure.</typeparam>
    /// <param name="buffer">The buffer.</param>
    /// <returns>The filled structure</returns>
    private static TStruct _BytesToStruct<TStruct>(byte[] buffer) where TStruct : struct {
      var size = buffer.Length;
      var unmanagedMemory = IntPtr.Zero;
      try {
        unmanagedMemory = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, unmanagedMemory, size);
        var result = (TStruct) Marshal.PtrToStructure(unmanagedMemory, typeof(TStruct));
        return result;
      } finally {
        if (unmanagedMemory != IntPtr.Zero)
          Marshal.FreeHGlobal(unmanagedMemory);
      }
    }

    /// <summary>
    ///   Writes the given structure to the stream.
    /// </summary>
    /// <typeparam name="TStruct">The type of the structure.</typeparam>
    /// <param name="this">This <see cref="Stream">Stream</see>.</param>
    /// <param name="value">The value.</param>
    public static void Write<TStruct>(this Stream @this, TStruct value) where TStruct : struct => @this.Write(_StructToBytes(value));

    /// <summary>
    ///   Converts a structure to a byte array.
    /// </summary>
    /// <typeparam name="TStruct">The type of the structure.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A byte array with the content of the structure.</returns>
    private static byte[] _StructToBytes<TStruct>(TStruct value) where TStruct : struct {
      var size = Marshal.SizeOf(typeof(TStruct));
      var unmanagedMemory = IntPtr.Zero;
      try {
        unmanagedMemory = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(value, unmanagedMemory, false);
        var result = new byte[size];
        Marshal.Copy(unmanagedMemory, result, 0, size);
        return result;
      } finally {
        if (unmanagedMemory != IntPtr.Zero)
          Marshal.FreeHGlobal(unmanagedMemory);
      }
    }

    /// <summary>
    ///   Read Bytes from a given position with a given SeekOrigin in the given buffer
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    // Note: not thread safe
    public static void ReadBytes(this Stream @this, long position, byte[] buffer,  SeekOrigin seekOrigin = SeekOrigin.Begin) 
      => ReadBytes(@this, position, buffer, 0, buffer.Length, seekOrigin)
    ;

    /// <summary>
    ///   Read Bytes from a given position with a given SeekOrigin in the given buffer with an offset
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="offset">The offset in the buffer</param>
    /// <param name="count">The amount of bytes you want to read</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    private static void ReadBytes(Stream @this, long position, byte[] buffer, int offset, int count, SeekOrigin seekOrigin = SeekOrigin.Begin) {
      if (!@this.CanSeek)
        throw new InvalidOperationException("Stream not seekable");

      _SeekToPositionAndCheck(@this, position, count, seekOrigin);
      @this.Read(buffer, offset, count);
    }

#if NET45_OR_GREATER

    /// <summary>
    ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    /// <returns>A awaitable Task representing the operation</returns>
    public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer,SeekOrigin seekOrigin = SeekOrigin.Begin)
      =>await AsyncReadBytes( @this, position, buffer,0,buffer.Length, seekOrigin)
    ;

    /// <summary>
    ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer with an offset
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="offset">The offset in the buffer</param>
    /// <param name="count">The amount of bytes you want to read</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    /// <returns>A awaitable Task representing the operation</returns>
    public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, int offset,int count, SeekOrigin seekOrigin = SeekOrigin.Begin) {
      await Task.Run(async () => {
        if (!@this.CanSeek)
          throw new InvalidOperationException("Stream not seekable");

        _SeekToPositionAndCheck(@this,position,count,seekOrigin);
        await @this.ReadAsync(buffer, offset, count);
      });
    }
    
    /// <summary>
    ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="token">The Cancellation Token</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    /// <returns>A awaitable Task representing the operation</returns>
    public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin)
      =>await AsyncReadBytes(@this, position, buffer,0,buffer.Length, token, seekOrigin ) 
    ;

    /// <summary>
    ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer with an offset
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <param name="token">The Cancellation Token</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    /// <returns>A awaitable Task representing the operation</returns>
    public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, int offset,int count,CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin) {
      await Task.Run(async () => {
        if (!@this.CanSeek)
          throw new InvalidOperationException("Stream not seekable");

        _SeekToPositionAndCheck(@this,position,count,seekOrigin);
        await @this.ReadAsync(buffer, offset, count, token);
      }, token);
    }

#endif

    /// <summary>
    ///   Begins reading Bytes from a given position with a given SeekOrigin in the given buffer
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="callback">The callback you want to get called</param>
    /// <param name="state">The given State</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    /// <returns>A IAsyncResult representing the operation</returns>
    public static IAsyncResult BeginReadBytes(this Stream @this,long position,byte[] buffer,AsyncCallback callback,object state=null,SeekOrigin seekOrigin=SeekOrigin.Begin)
      =>BeginReadBytes(@this,position,buffer,0,buffer.Length,callback,state,seekOrigin)
    ;

    /// <summary>
    ///   Begins reading Bytes from a given position with a given SeekOrigin in the given buffer with an offset
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="position">The position from which you want to read</param>
    /// <param name="buffer">The buffer where the result is written in</param>
    /// <param name="offset">The offset in the buffer</param>
    /// <param name="count">The amount of bytes you want to read</param>
    /// <param name="callback">The callback you want to get called</param>
    /// <param name="state">The given State</param>
    /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
    /// <returns></returns>
    public static IAsyncResult BeginReadBytes(this Stream @this,long position,byte[] buffer,int offset,int count,AsyncCallback callback,object state=null,SeekOrigin seekOrigin=SeekOrigin.Begin) {
      if (!@this.CanSeek)
        throw new InvalidOperationException("Stream not seekable");

      _SeekToPositionAndCheck(@this,position,count,seekOrigin);
      return @this.BeginRead(buffer, offset, count, callback, state);
    }

    /// <summary>
    /// Ends to read bytes
    /// </summary>
    /// <param name="this">This Stream</param>
    /// <param name="result">The IAsyncResult representing the result of the Begin operation</param>
    public static void EndReadBytes(this Stream @this, IAsyncResult result)
      => @this.EndRead(result)
    ;

    /// <summary>
    /// Seeks to the gives position and checks if the position is valid
    /// </summary>
    /// <param name="stream">The Stream to seek</param>
    /// <param name="position">The position you want to seek to</param>
    /// <param name="wantedBytes">The amount of bytes you want to read</param>
    /// <param name="origin">The SeekOrigin you want to start seeking</param>
    [DebuggerHidden]
    private static void _SeekToPositionAndCheck(Stream stream, long position,int wantedBytes, SeekOrigin origin) {
      long absolutePosition;
      switch (origin) {
        case SeekOrigin.Begin:
          absolutePosition = position;
          break;
        case SeekOrigin.Current:
          absolutePosition = stream.Position + position;
          break;
        case SeekOrigin.End:
          absolutePosition = stream.Length - position;
          break;
        default:
          throw new NotSupportedException();
      }

      if (absolutePosition + wantedBytes > stream.Length)
        throw new ArgumentOutOfRangeException($"offset({absolutePosition}) + count({wantedBytes}) > Stream.Length({stream.Length})");

      stream.Seek(absolutePosition, SeekOrigin.Begin);
    }
  }
}
