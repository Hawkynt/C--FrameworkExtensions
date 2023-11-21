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

using System.Diagnostics;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
#if SUPPORTS_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.IO;

using Guard;

/// <summary>
///   Extensions for Streams.
/// </summary>
#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
  static partial class StreamExtensions {

  /// <summary>
  ///   Writes a whole array of bytes to a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="data">The data to write.</param>
  public static void Write(this Stream @this, byte[] data) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(data);
    Against.False(@this.CanWrite);

    @this.Write(data, 0, data.Length);
  }

  /// <summary>
  ///   Fills a whole array with bytes from a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="result">The array where to store the results.</param>
  /// <returns>The number of bytes actually read.</returns>
  public static int Read(this Stream @this, byte[] result) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(result);
    Against.False(@this.CanRead);

    return @this.Read(result, 0, result.Length);
  }

  /// <summary>
  ///   Tries to read a given number of bytes from a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="count">The number of bytes to read.</param>
  /// <returns>The number of bytes actually read.</returns>
  public static byte[] ReadBytes(this Stream @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);
    Against.False(@this.CanRead);

    var result = new byte[count];
    var bytesGot = @this.Read(result, 0, count);
    return bytesGot == count ? result : result.Take(bytesGot).ToArray();
  }
  
  #region Reading/Writing primitives

#if UNSAFE

  /// <summary>
  /// Write a <see cref="int"/> value to the <see cref="Stream"/> in Little-Endian (LE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="value">The value to write.</param>
  public static unsafe void Write(this Stream @this, int value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    var ptr = (byte*)&value;
    @this.WriteByte(*ptr);
    @this.WriteByte(ptr[1]);
    @this.WriteByte(ptr[2]);
    @this.WriteByte(ptr[3]);
  }

  /// <summary>
  /// Write a <see cref="int"/> value to the <see cref="Stream"/> in Big-Endian (BE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="value">The value to write.</param>
  private static unsafe void _Write(this Stream @this, int value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    var ptr = (byte*)&value;
    @this.WriteByte(ptr[3]);
    @this.WriteByte(ptr[2]);
    @this.WriteByte(ptr[1]);
    @this.WriteByte(*ptr);
  }

#else

  /// <summary>
  /// Write a <see cref="int"/> value to the <see cref="Stream"/> in Little-Endian (LE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="value">The value to write.</param>
  public static void Write(this Stream @this, int value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte((byte)value);
    @this.WriteByte((byte)(value >> 8));
    @this.WriteByte((byte)(value >> 16));
    @this.WriteByte((byte)(value >> 24));
  }

  /// <summary>
  /// Write a <see cref="int"/> value to the <see cref="Stream"/> in Big-Endian (BE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="value">The value to write.</param>
  private static void _Write(this Stream @this, int value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte((byte)(value >> 24));
    @this.WriteByte((byte)(value >> 16));
    @this.WriteByte((byte)(value >> 8));
    @this.WriteByte((byte)value);
  }

#endif

  /// <summary>
  /// Write a <see cref="int"/> value to the <see cref="Stream"/>.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="value">The value.</param>
  /// <param name="bigEndian">If set to <see langword="true"/> the int gets written in Big-Endian format; otherwise Little-Endian is used.</param>
  public static void Write(this Stream @this, int value, bool bigEndian) {
    if (bigEndian)
      @this._Write(value);
    else
      @this.Write(value);
  }

#if UNSAFE

  /// <summary>
  /// Reads an <see cref="int"/> value from the <see cref="Stream"/> in Little-Endian (LE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <returns>The value or garbage when the <see cref="Stream"/> ended prematurely.</returns>
  public static unsafe int ReadInt(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    var result = 0;
    var bytePtr = (byte*)&result;

    *bytePtr = (byte)@this.ReadByte();
    bytePtr[1] = (byte)@this.ReadByte();
    bytePtr[2] = (byte)@this.ReadByte();
    bytePtr[3] = (byte)@this.ReadByte();
    
    return result;
  }

  /// <summary>
  /// Reads an <see cref="int"/> value from the <see cref="Stream"/> in Big-Endian (BE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <returns>The value or garbage when the <see cref="Stream"/> ended prematurely.</returns>
  private static unsafe int _ReadInt(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    var result = 0;
    var bytePtr = (byte*)&result;

    bytePtr[3] = (byte)@this.ReadByte();
    bytePtr[2] = (byte)@this.ReadByte();
    bytePtr[1] = (byte)@this.ReadByte();
    *bytePtr = (byte)@this.ReadByte();

    return result;
  }

#else

  /// <summary>
  /// Reads an <see cref="int"/> value from the <see cref="Stream"/> in Little-Endian (LE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <returns>The value or garbage when the <see cref="Stream"/> ended prematurely.</returns>
  public static int ReadInt(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return @this.ReadByte() | (@this.ReadByte() << 8) | (@this.ReadByte() << 16) | (@this.ReadByte() << 24);
  }

  /// <summary>
  /// Reads an <see cref="int"/> value from the <see cref="Stream"/> in Big-Endian (BE) mode.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <returns>The value or garbage when the <see cref="Stream"/> ended prematurely.</returns>
  private static int _ReadInt(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (@this.ReadByte() << 24) | (@this.ReadByte() << 16) | (@this.ReadByte() << 8) | @this.ReadByte();
  }

#endif

  /// <summary>
  ///   Reads an <see cref="int"/> value from the <see cref="Stream"/>.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="bigEndian">If set to <see langword="true"/> the value gets read in Big-Endian format; otherwise Little-Endian is used.</param>
  /// <returns>The value or garbage when the <see cref="Stream"/> ended prematurely.</returns>
  public static int ReadInt(this Stream @this, bool bigEndian) => bigEndian ? _ReadInt(@this) : ReadInt(@this);

  #endregion

  /// <summary>
  ///   Determines whether the current stream position pointer is at end of the stream or not.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <returns>
  ///   <see langword="true"/> if the stream was read/written to its end; otherwise, <see langword="false"/>.
  /// </returns>
  public static bool IsAtEndOfStream(this Stream @this) {
    Against.ThisIsNull(@this);

    return @this.Position >= @this.Length;
  }

  /// <summary>
  ///   Copies the whole stream to an array.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <returns>The content of the stream.</returns>
  public static byte[] ToArray(this Stream @this) {
    Against.ThisIsNull(@this);

    using MemoryStream data = new();
    @this.CopyTo(data);
    return data.ToArray();
  }

  /// <summary>
  ///   Reads all text from the stream..
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="encoding">The encoding.</param>
  /// <returns>The text from the stream.</returns>
  public static string ReadAllText(this Stream @this, Encoding encoding = null) {
    Against.ThisIsNull(@this);

    encoding ??= Encoding.Default;
    return @this.CanRead ? encoding.GetString(@this.ToArray()) : null;
  }

  /// <summary>
  ///   Writes all text.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="data">The data.</param>
  /// <param name="encoding">The encoding.</param>
  public static void WriteAllText(this Stream @this, string data, Encoding encoding = null) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    encoding ??= Encoding.Default;
    @this.Write(encoding.GetBytes(data));
  }

  /// <summary>
  ///   Reads a struct from the given stream.
  /// </summary>
  /// <typeparam name="TStruct">The type of the structure.</typeparam>
  /// <param name="this">This <see cref="Stream">Stream</see>.</param>
  /// <returns>The filled structure.</returns>
  public static TStruct Read<TStruct>(this Stream @this) where TStruct : struct {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    static TStruct BytesToStruct(byte[] buffer) {
      var size = buffer.Length;
      var unmanagedMemory = IntPtr.Zero;
      try {
        unmanagedMemory = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, unmanagedMemory, size);
        var result = (TStruct)Marshal.PtrToStructure(unmanagedMemory, typeof(TStruct));
        return result;
      } finally {
        if (unmanagedMemory != IntPtr.Zero)
          Marshal.FreeHGlobal(unmanagedMemory);
      }
    }

    var size = Marshal.SizeOf(typeof(TStruct));
    var buffer = new byte[size];
    var offset = 0;
    while (size > 0 && !@this.IsAtEndOfStream()) {
      var read = @this.Read(buffer, offset, size);
      size -= read;
      offset += read;
    }

    return BytesToStruct(buffer);
  }

  /// <summary>
  ///   Writes the given structure to the stream.
  /// </summary>
  /// <typeparam name="TStruct">The type of the structure.</typeparam>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="value">The value.</param>
  public static void Write<TStruct>(this Stream @this, TStruct value) where TStruct : struct {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    static byte[] StructToBytes(TStruct value) {
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

    @this.Write(StructToBytes(value));
  }

  /// <summary>
  ///   Read Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  // Note: not thread safe
  public static void ReadBytes(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => _ReadBytes(@this, position, buffer, 0, buffer.Length, seekOrigin);

  /// <summary>
  ///   Read Bytes from a given position with a given SeekOrigin in the given buffer with an offset
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="offset">The offset in the buffer</param>
  /// <param name="count">The amount of bytes you want to read</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  private static void _ReadBytes(Stream @this, long position, byte[] buffer, int offset, int count, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    Against.False(@this.CanRead);

    _SeekToPositionAndCheck(@this, position, count, seekOrigin);
    while (count > 0 && !@this.IsAtEndOfStream()) {
      var read = @this.Read(buffer, offset, count);
      count -= read;
      offset += read;
    }
  }

#if SUPPORTS_STREAM_ASYNC

  /// <summary>
  ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
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
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="offset">The offset in the buffer</param>
  /// <param name="count">The amount of bytes you want to read</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A awaitable Task representing the operation</returns>
  public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, int offset,int count, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    await Task.Run(async () => {
      _SeekToPositionAndCheck(@this,position,count,seekOrigin);
      await @this.ReadAsync(buffer, offset, count);
    });
  }
  
  /// <summary>
  ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
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
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="offset"></param>
  /// <param name="count"></param>
  /// <param name="token">The Cancellation Token</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A awaitable Task representing the operation</returns>
  public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, int offset,int count,CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    await Task.Run(async () => {
      _SeekToPositionAndCheck(@this,position,count,seekOrigin);
      await @this.ReadAsync(buffer, offset, count, token);
    }, token);
  }

#endif

  /// <summary>
  ///   Begins reading Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="callback">The callback you want to get called</param>
  /// <param name="state">The given State</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A IAsyncResult representing the operation</returns>
  public static IAsyncResult BeginReadBytes(this Stream @this, long position, byte[] buffer, AsyncCallback callback, object state = null, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => BeginReadBytes(@this, position, buffer, 0, buffer.Length, callback, state, seekOrigin);

  /// <summary>
  ///   Begins reading Bytes from a given position with a given SeekOrigin in the given buffer with an offset
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="offset">The offset in the buffer</param>
  /// <param name="count">The amount of bytes you want to read</param>
  /// <param name="callback">The callback you want to get called</param>
  /// <param name="state">The given State</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns></returns>
  public static IAsyncResult BeginReadBytes(this Stream @this, long position, byte[] buffer, int offset, int count, AsyncCallback callback, object state = null, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    _SeekToPositionAndCheck(@this, position, count, seekOrigin);
    return @this.BeginRead(buffer, offset, count, callback, state);
  }

  /// <summary>
  /// Ends to read bytes
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="result">The IAsyncResult representing the result of the Begin operation</param>
  public static void EndReadBytes(this Stream @this, IAsyncResult result)
    => @this.EndRead(result);

  /// <summary>
  /// Seeks to the gives position and checks if the position is valid
  /// </summary>
  /// <param name="stream">The Stream to seek</param>
  /// <param name="position">The position you want to seek to</param>
  /// <param name="wantedBytes">The amount of bytes you want to read</param>
  /// <param name="origin">The SeekOrigin you want to start seeking</param>
  [DebuggerHidden]
  private static void _SeekToPositionAndCheck(Stream stream, long position, int wantedBytes, SeekOrigin origin) {
    Against.False(stream.CanSeek);

    var absolutePosition = origin switch {
      SeekOrigin.Begin => position,
      SeekOrigin.Current => stream.Position + position,
      SeekOrigin.End => stream.Length - position,
      _ => throw new NotSupportedException()
    };

    if (absolutePosition + wantedBytes > stream.Length)
      throw new ArgumentOutOfRangeException($"offset({absolutePosition}) + count({wantedBytes}) > Stream.Length({stream.Length})");

    stream.Seek(absolutePosition, SeekOrigin.Begin);
  }

#if !SUPPORTS_STREAM_COPY

  /// <summary>
  /// Copies all contents from this <see cref="Stream"/> to another <see cref="Stream"/>.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="target">Target <see cref="Stream"/>.</param>
  public static void CopyTo(this Stream @this, Stream target) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.False(@this.CanRead);
    Against.False(target.CanWrite);

    var buffer = new byte[81920];
    int count;
    while ((count = @this.Read(buffer, 0, buffer.Length)) != 0)
      target.Write(buffer, 0, count);
  }

  /// <summary>
  /// Flushes the <see cref="Stream"/>.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="_">Dummy, ignored</param>
  public static void Flush(this Stream @this, bool _) => @this.Flush();

#endif

}

