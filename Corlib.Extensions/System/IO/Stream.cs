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
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using Guard;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.IO;

/// <summary>
///   Extensions for Streams.
/// </summary>
#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
  static partial class StreamExtensions {

  private const int _BUFFER_SIZE = 4 * 1024 * 16;

  #region nested types

#if !SUPPORTS_SPAN || !UNSAFE

  private readonly struct BufferHandle : IDisposable {
    public readonly byte[] Buffer;
    public BufferHandle(byte[] buffer) => this.Buffer = buffer;

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Dispose() => PrimitiveConversionBufferManager.ReleaseBuffer(this.Buffer);
  }

  private static class PrimitiveConversionBufferManager {
    private const int BufferSize = 64; // Fixed buffer size, enough to keep the largest primitive datatype
    private static byte[] _sharedBuffer = new byte[BufferSize];

    [ThreadStatic] private static byte[] threadLocalBuffer;

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static BufferHandle GetBuffer() => new(Interlocked.Exchange(ref _sharedBuffer, null) ?? (threadLocalBuffer ??= new byte[BufferSize]));

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void ReleaseBuffer(byte[] buffer) {
      if (buffer != threadLocalBuffer)
        Interlocked.Exchange(ref _sharedBuffer, buffer);
    }
  }

#endif

  #endregion

  /// <summary>
  ///   Writes a whole array of bytes to a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="data">The data to write.</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
# endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
# endif
  public static int Read(this Stream @this, byte[] result) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(result);
    Against.False(@this.CanRead);

    return @this.Read(result, 0, result.Length);
  }

  /// <summary>
  /// Tries to read a given number of bytes from a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="count">The number of bytes to read.</param>
  /// <returns>The number of bytes actually read.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
# endif
  public static byte[] ReadBytes(this Stream @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);
    Against.False(@this.CanRead);

    return _ReadBytes(@this, count);
  }

  /// <summary>
  /// Reads all bytes from the current position of the given <see cref="Stream"/> and returns them as a byte array.
  /// </summary>
  /// <param name="this">The <see cref="Stream"/> instance on which the extension method is called.</param>
  /// <returns>A byte array containing the bytes read from the <see cref="Stream"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when the stream's available number of bytes exceeds 2GB, which is the maximum length supported by a single array in .NET.
  /// </exception>
  /// <remarks>
  /// The method reads bytes into a byte array, which has a maximum indexable length of <see cref="Int32.MaxValue"/> (2,147,483,647) elements,
  /// roughly equating to a 2GB size limit. Attempting to read a stream larger than this limit will result in an overflow of the array index.
  /// </remarks>
  /// <example>
  /// This example shows how to use the <see cref="ReadAllBytes"/> extension method
  /// to read all bytes from a file stream and store them in a byte array.
  /// <code>
  /// using System;
  /// using System.IO;
  ///
  /// class Program
  /// {
  ///     static void Main()
  ///     {
  ///         using (FileStream fileStream = File.OpenRead("example.txt"))
  ///         {
  ///             byte[] fileContents = fileStream.ReadAllBytes();
  ///             // Use fileContents as needed
  ///         }
  ///     }
  /// }
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static byte[] ReadAllBytes(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return @this.CanSeek ? _ReadBytesSeekable(@this, @this.Length - @this.Position) : _ReadAllBytesNonSeekable(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static byte[] _ReadAllBytesNonSeekable(Stream @this) {
    using MemoryStream data = new(_BUFFER_SIZE);
    @this.CopyTo(data);
    return data.ToArray();
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static byte[] _ReadBytes(Stream @this, long count)
    => @this.CanSeek ? _ReadBytesSeekable(@this, count) : _ReadBytesNonSeekable(@this, count)
    ;

  /// <summary>
  /// Some non-seekable streams have problems when reading into array buffers, so they need slow byte-by-byte reading
  /// </summary>
  /// <param name="this"></param>
  /// <param name="count"></param>
  /// <returns></returns>
  private static byte[] _ReadBytesNonSeekable(Stream @this, long count) {
    using MemoryStream data = new(_BUFFER_SIZE);
    while (count-- > 0) {
      var @byte = @this.ReadByte();
      if (@byte < 0)
        break;

      data.WriteByte((byte)@byte);
    }

    return data.ToArray();
  }

  private static byte[] _ReadBytesSeekable(Stream @this, long count) {
    Against.ValuesAbove(count, int.MaxValue);

    var smallCount = (int)count;
    var result = new byte[smallCount];
    var offset = _ReadBytesToArraySeekable(@this, result, 0, smallCount);

    if (offset >= smallCount)
      return result;

    // stream ended too early - shrink block
    var finalBlock = new byte[offset];
    result.CopyTo(0, finalBlock, 0, offset);
    return finalBlock;
  }

  private static int _ReadBytesToArraySeekable(Stream @this, byte[] target, int offset, int count) {
    while (count > 0) {
      var bytesRead = @this.Read(target, offset, count);
      if (bytesRead == 0)
        break;

      offset += bytesRead;
      count -= bytesRead;
    }

    return offset;
  }

  #region Reading/Writing primitives

  #region bool

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, bool value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte(value ? (byte)255 : (byte)0);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool ReadBool(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return @this.ReadByte() != 0;
  }

  #endregion

  #region byte

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, byte value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte(value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static byte ReadUInt8(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (byte)@this.ReadByte();
  }

  #endregion

  #region sbyte

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, sbyte value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte((byte)value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static sbyte ReadInt8(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (sbyte)@this.ReadByte();
  }

  #endregion

  #region ushort

#if UNSAFE

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteLittleEndianU16(Stream stream, ushort value) {
#if SUPPORTS_SPAN
    var bytes = new ReadOnlySpan<byte>(&value, sizeof(ushort));
    stream.Write(bytes);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      *(ushort*)bytes = value;

    stream.Write(handle.Buffer, 0, sizeof(ushort));
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteBigEndianU16(Stream stream, ushort value) {
    var ptr = (byte*)&value;

#if SUPPORTS_SPAN
    (*ptr, ptr[1]) = (ptr[1], *ptr);
    var bytes = new ReadOnlySpan<byte>(ptr, sizeof(ushort));
    stream.Write(bytes);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      (*bytes, bytes[1]) = (ptr[1], *ptr);

    stream.Write(handle.Buffer, 0, sizeof(ushort));
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe ushort _ReadLittleEndianU16(Stream stream) {
#if SUPPORTS_SPAN
    ushort result = 0;
    var bytes = new Span<byte>(&result, sizeof(ushort));
    stream.Read(bytes);
    return result;
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    fixed (byte* bytes = handle.Buffer)
      return *(ushort*)bytes;
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe ushort _ReadBigEndianU16(Stream stream) {
    ushort result = 0;
    var ptr = (byte*)&result;

#if SUPPORTS_SPAN
    var bytes = new Span<byte>(ptr, sizeof(ushort));
    stream.Read(bytes);
    (*ptr, ptr[1]) = (ptr[1], *ptr);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1]) = (bytes[1], *bytes);
#endif

    return result;
  }

#else

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteLittleEndianU16(Stream stream, ushort value) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    handle.Buffer[0] = (byte)value;
    handle.Buffer[1] = (byte)(value >> 8);
    stream.Write(handle.Buffer, 0, sizeof(ushort));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteBigEndianU16(Stream stream, ushort value) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    handle.Buffer[1] = (byte)value;
    handle.Buffer[0] = (byte)(value >> 8);
    stream.Write(handle.Buffer, 0, sizeof(ushort));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static ushort _ReadLittleEndianU16(Stream stream) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    return (ushort)(handle.Buffer[0] | (handle.Buffer[1] << 8));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static ushort _ReadBigEndianU16(Stream stream) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    return (ushort)(handle.Buffer[1] | (handle.Buffer[0] << 8));
  }

#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, ushort value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, ushort value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, value);
    else
      _WriteLittleEndianU16(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static ushort ReadUInt16(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU16(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static ushort ReadUInt16(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this);
  }

  #endregion

  #region short

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, short value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, (ushort)value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, short value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, (ushort)value);
    else
      _WriteLittleEndianU16(@this, (ushort)value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static short ReadInt16(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (short)_ReadLittleEndianU16(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static short ReadInt16(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (short)(bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this));
  }

  #endregion

  #region char

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, char value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, char value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, value);
    else
      _WriteLittleEndianU16(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static char ReadChar(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (char)_ReadLittleEndianU16(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static char ReadChar(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (char)(bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this));
  }

  #endregion

  #region uint


#if UNSAFE

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteLittleEndianU32(Stream stream, uint value) {
#if SUPPORTS_SPAN
    var bytes = new ReadOnlySpan<byte>(&value, sizeof(uint));
    stream.Write(bytes);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      *(uint*)bytes = value;

    stream.Write(handle.Buffer, 0, sizeof(uint));
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteBigEndianU32(Stream stream, uint value) {
    var ptr = (byte*)&value;

#if SUPPORTS_SPAN
    (*ptr, ptr[1], ptr[2], ptr[3]) = (ptr[3], ptr[2], ptr[1], *ptr);
    var bytes = new ReadOnlySpan<byte>(ptr, sizeof(uint));
    stream.Write(bytes);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      (*bytes, bytes[1], bytes[2], bytes[3]) = (ptr[3], ptr[2], ptr[1], *ptr);

    stream.Write(handle.Buffer, 0, sizeof(uint));
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe uint _ReadLittleEndianU32(Stream stream) {
#if SUPPORTS_SPAN
    var result = 0U;
    var bytes = new Span<byte>(&result, sizeof(uint));
    stream.Read(bytes);
    return result;
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    fixed (byte* bytes = handle.Buffer)
      return *(uint*)bytes;
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe uint _ReadBigEndianU32(Stream stream) {
    var result = 0U;
    var ptr = (byte*)&result;

#if SUPPORTS_SPAN
    var bytes = new Span<byte>(ptr, sizeof(uint));
    stream.Read(bytes);
    (*ptr, ptr[1], ptr[2], ptr[3]) = (ptr[3], ptr[2], ptr[1], *ptr);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1], ptr[2], ptr[3]) = (bytes[3], bytes[2], bytes[1], *bytes);
#endif

    return result;
  }

#else

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteLittleEndianU32(Stream stream, uint value) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    handle.Buffer[0] = (byte)value;
    handle.Buffer[1] = (byte)(value >> 8);
    handle.Buffer[2] = (byte)(value >> 16);
    handle.Buffer[3] = (byte)(value >> 24);
    stream.Write(handle.Buffer, 0, sizeof(uint));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteBigEndianU32(Stream stream, uint value) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    handle.Buffer[3] = (byte)value;
    handle.Buffer[2] = (byte)(value >> 8);
    handle.Buffer[1] = (byte)(value >> 16);
    handle.Buffer[0] = (byte)(value >> 24);
    stream.Write(handle.Buffer, 0, sizeof(uint));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static uint _ReadLittleEndianU32(Stream stream) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    return (uint)(handle.Buffer[0] | (handle.Buffer[1] << 8) | (handle.Buffer[2] << 16) | (handle.Buffer[3] << 24));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static uint _ReadBigEndianU32(Stream stream) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    return (uint)(handle.Buffer[3] | (handle.Buffer[2] << 8) | (handle.Buffer[1] << 16) | (handle.Buffer[0] << 24));
  }

#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, uint value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU32(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, uint value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU32(@this, value);
    else
      _WriteLittleEndianU32(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static uint ReadUInt32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU32(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static uint ReadUInt32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU32(@this) : _ReadLittleEndianU32(@this);
  }


  #endregion

  #region int

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, int value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU32(@this, (uint)value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, int value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU32(@this, (uint)value);
    else
      _WriteLittleEndianU32(@this, (uint)value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int ReadInt32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (int)_ReadLittleEndianU32(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int ReadInt32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (int)(bigEndian ? _ReadBigEndianU32(@this) : _ReadLittleEndianU32(@this));
  }

  #endregion

  #region ulong


#if UNSAFE

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteLittleEndianU64(Stream stream, ulong value) {
#if SUPPORTS_SPAN
    var bytes = new ReadOnlySpan<byte>(&value, sizeof(ulong));
    stream.Write(bytes);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      *(ulong*)bytes = value;

    stream.Write(handle.Buffer, 0, sizeof(ulong));
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteBigEndianU64(Stream stream, ulong value) {
    var ptr = (byte*)&value;

#if SUPPORTS_SPAN
    (*ptr, ptr[1], ptr[2], ptr[3], ptr[4], ptr[5], ptr[6], ptr[7]) = (ptr[7], ptr[6], ptr[5], ptr[4], ptr[3], ptr[2], ptr[1], *ptr);
    var bytes = new ReadOnlySpan<byte>(ptr, sizeof(ulong));
    stream.Write(bytes);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      (*bytes, bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]) = (ptr[7], ptr[6], ptr[5], ptr[4], ptr[3], ptr[2], ptr[1], *ptr);

    stream.Write(handle.Buffer, 0, sizeof(ulong));
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe ulong _ReadLittleEndianU64(Stream stream) {
#if SUPPORTS_SPAN
    var result = 0UL;
    var bytes = new Span<byte>(&result, sizeof(ulong));
    stream.Read(bytes);
    return result;
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ulong));
    fixed (byte* bytes = handle.Buffer)
      return *(ulong*)bytes;
#endif
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe ulong _ReadBigEndianU64(Stream stream) {
    var result = 0UL;
    var ptr = (byte*)&result;

#if SUPPORTS_SPAN
    var bytes = new Span<byte>(ptr, sizeof(ulong));
    stream.Read(bytes);
    (*ptr, ptr[1], ptr[2], ptr[3], ptr[4], ptr[5], ptr[6], ptr[7]) = (ptr[7], ptr[6], ptr[5], ptr[4], ptr[3], ptr[2], ptr[1], *ptr);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ulong));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1], ptr[2], ptr[3], ptr[4], ptr[5], ptr[6], ptr[7]) = (bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], *bytes);
#endif

    return result;
  }

#else

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteLittleEndianU64(Stream stream, ulong value) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    handle.Buffer[0] = (byte)value;
    handle.Buffer[1] = (byte)(value >> 8);
    handle.Buffer[2] = (byte)(value >> 16);
    handle.Buffer[3] = (byte)(value >> 24);
    handle.Buffer[4] = (byte)(value >> 32);
    handle.Buffer[5] = (byte)(value >> 40);
    handle.Buffer[6] = (byte)(value >> 48);
    handle.Buffer[7] = (byte)(value >> 56);
    stream.Write(handle.Buffer, 0, sizeof(ulong));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteBigEndianU64(Stream stream, ulong value) {
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    handle.Buffer[7] = (byte)value;
    handle.Buffer[6] = (byte)(value >> 8);
    handle.Buffer[5] = (byte)(value >> 16);
    handle.Buffer[4] = (byte)(value >> 24);
    handle.Buffer[3] = (byte)(value >> 32);
    handle.Buffer[2] = (byte)(value >> 40);
    handle.Buffer[1] = (byte)(value >> 48);
    handle.Buffer[0] = (byte)(value >> 56);
    stream.Write(handle.Buffer, 0, sizeof(ulong));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static ulong _ReadLittleEndianU64(Stream stream) => _ReadLittleEndianU32(stream) | ((ulong)_ReadLittleEndianU32(stream) << 32);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static ulong _ReadBigEndianU64(Stream stream) => ((ulong)_ReadBigEndianU32(stream) << 32) | _ReadBigEndianU32(stream);

#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, ulong value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU64(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, ulong value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU64(@this, value);
    else
      _WriteLittleEndianU64(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static ulong ReadUInt64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU64(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static ulong ReadUInt64(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU64(@this) : _ReadLittleEndianU64(@this);
  }


  #endregion

  #region long

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, long value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU64(@this, (ulong)value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, long value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU64(@this, (ulong)value);
    else
      _WriteLittleEndianU64(@this, (ulong)value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long ReadInt64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (long)_ReadLittleEndianU64(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long ReadInt64(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (long)(bigEndian ? _ReadBigEndianU64(@this) : _ReadLittleEndianU64(@this));
  }

  #endregion

  #region single

#if UNSAFE

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteLittleEndianF32(Stream stream, float value) => _WriteLittleEndianU32(stream, *(uint*)&value);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteBigEndianF32(Stream stream, float value) => _WriteBigEndianU32(stream, *(uint*)&value);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe float _ReadLittleEndianF32(Stream stream) {
    var result = _ReadLittleEndianU32(stream);
    return *(float*)&result;
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe float _ReadBigEndianF32(Stream stream) {
    var result = _ReadBigEndianU32(stream);
    return *(float*)&result;
  }

#else

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteLittleEndianF32(Stream stream, float value) {
    var bytes = BitConverter.GetBytes(value);
    stream.Write(bytes, 0, sizeof(float));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteBigEndianF32(Stream stream, float value) {
    var bytes = BitConverter.GetBytes(value);
    (bytes[0], bytes[1], bytes[2], bytes[3]) = (bytes[3], bytes[2], bytes[1], bytes[0]);
    stream.Write(bytes, 0, sizeof(float));
  }
  
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static float _ReadLittleEndianF32(Stream stream) {
    var bytes = new byte[sizeof(float)];
    // ReSharper disable once MustUseReturnValue
    stream.Read(bytes, 0, sizeof(float));
    return BitConverter.ToSingle(bytes, 0);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static float _ReadBigEndianF32(Stream stream) {
    var bytes = new byte[sizeof(float)];
    // ReSharper disable once MustUseReturnValue
    stream.Read(bytes, 0, sizeof(float));
    (bytes[0], bytes[1], bytes[2], bytes[3]) = (bytes[3], bytes[2], bytes[1], bytes[0]);
    return BitConverter.ToSingle(bytes, 0);
  }

#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, float value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianF32(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, float value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianF32(@this, value);
    else
      _WriteLittleEndianF32(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static float ReadFloat32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianF32(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static float ReadFloat32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianF32(@this) : _ReadLittleEndianF32(@this);
  }

  #endregion

  #region double

#if UNSAFE

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteLittleEndianF64(Stream stream, double value) => _WriteLittleEndianU64(stream, *(ulong*)&value);
  
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe void _WriteBigEndianF64(Stream stream, double value) => _WriteBigEndianU64(stream, *(ulong*)&value);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe double _ReadLittleEndianF64(Stream stream) {
    var result = _ReadLittleEndianU64(stream);
    return *(double*)&result;
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static unsafe double _ReadBigEndianF64(Stream stream) {
    var result = _ReadBigEndianU64(stream);
    return *(double*)&result;
  }

#else

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteLittleEndianF64(Stream stream, double value) {
    var bytes = BitConverter.GetBytes(value);
    stream.Write(bytes, 0, sizeof(double));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static void _WriteBigEndianF64(Stream stream, double value) {
    var bytes = BitConverter.GetBytes(value);
    (bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]) = (bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0]);
    stream.Write(bytes, 0, sizeof(double));
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static double _ReadLittleEndianF64(Stream stream) {
    var bytes = new byte[sizeof(double)];
    // ReSharper disable once MustUseReturnValue
    stream.Read(bytes, 0, sizeof(double));
    return BitConverter.ToDouble(bytes, 0);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static double _ReadBigEndianF64(Stream stream) {
    var bytes = new byte[sizeof(double)];
    // ReSharper disable once MustUseReturnValue
    stream.Read(bytes, 0, sizeof(double));
    (bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]) = (bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0]);
    return BitConverter.ToDouble(bytes, 0);
  }

#endif

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, double value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianF64(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, double value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianF64(@this, value);
    else
      _WriteLittleEndianF64(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double ReadFloat64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianF64(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double ReadFloat64(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianF64(@this) : _ReadLittleEndianF64(@this);
  }

  #endregion

  #region decimal

  private static void _WriteLittleEndianM128(Stream stream, decimal value) {
    var blocks = decimal.GetBits(value);
    foreach (var block in blocks)
      _WriteLittleEndianU32(stream, (uint)block);
  }

  private static void _WriteBigEndianM128(Stream stream, decimal value) {
    var blocks = decimal.GetBits(value);
    for (var i = blocks.Length - 1; i >= 0; --i)
      _WriteBigEndianU32(stream, (uint)blocks[i]);
  }

  private static decimal _ReadLittleEndianM128(Stream stream) {
    var blocks = new int[4];
    for (var i = 0; i < blocks.Length; ++i)
      blocks[i] = (int)_ReadLittleEndianU32(stream);

    return new(blocks);
  }

  private static decimal _ReadBigEndianM128(Stream stream) {
    var blocks = new int[4];
    for (var i = blocks.Length - 1; i >= 0; --i)
      blocks[i] = (int)_ReadBigEndianU32(stream);

    return new(blocks);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, decimal value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianM128(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Write(this Stream @this, decimal value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianM128(@this, value);
    else
      _WriteLittleEndianM128(@this, value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static decimal ReadMoney128(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianM128(@this);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static decimal ReadMoney128(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianM128(@this) : _ReadLittleEndianM128(@this);
  }

  #endregion

  #endregion

  /// <summary>
  /// Determines whether the current <see cref="Stream"/> position pointer is at the end of the <see cref="Stream"/>.
  /// This method is applicable to streams that support seeking and specific <see cref="Stream"/> types like <see cref="NetworkStream"/>.
  /// For other non-seekable streams, the method throws an <see cref="InvalidOperationException"/>, as checking for the end-of-stream
  /// without altering the <see cref="Stream"/> state may not be possible.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <returns>
  /// <see langword="true"/> if the <see cref="Stream"/> position is at the end for seekable streams or if no more data is available in a <see cref="NetworkStream"/>;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the <see cref="Stream"/> does not support seeking or is not a recognized type (like <see cref="NetworkStream"/>) that allows safe EOF checking.
  /// </exception>
  /// <remarks>
  /// For seekable streams, this method checks if the current position is at or beyond the end of the <see cref="Stream"/>.
  /// For <see cref="NetworkStream"/>, it checks the availability of data to read.
  /// For other non-seekable stream types, the caller should ensure an appropriate method to check for the end of the <see cref="Stream"/>, 
  /// as this method will throw an <see cref="InvalidOperationException"/>.
  /// </remarks>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsAtEndOfStream(this Stream @this) {
    Against.ThisIsNull(@this);

    if (@this.CanSeek)
      return @this.Position >= @this.Length;

    if (@this is NetworkStream n)
      return !n.DataAvailable;

    AlwaysThrow.InvalidOperationException("Stream doesn't support EOF-checking");
    return true;
  }

  /// <summary>
  /// Reads all bytes from the starting position of the given <see cref="Stream"/> and returns them as a byte array.
  /// </summary>
  /// <param name="this">The <see cref="Stream"/> instance on which the extension method is called.</param>
  /// <returns>A byte array containing the bytes read from the <see cref="Stream"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when the stream's available number of bytes exceeds 2GB, which is the maximum length supported by a single array in .NET.
  /// </exception>
  /// <remarks>
  /// If the <see cref="Stream"/> is not seekable, the bytes are read from the current position.
  /// The method reads bytes into a byte array, which has a maximum indexable length of <see cref="Int32.MaxValue"/> (2,147,483,647) elements,
  /// roughly equating to a 2GB size limit. Attempting to read a stream larger than this limit will result in an overflow of the array index.
  /// </remarks>
  /// <example>
  /// This example shows how to use the <see cref="ToArray"/> extension method
  /// to read all bytes from a file stream and store them in a byte array.
  /// <code>
  /// using System;
  /// using System.IO;
  ///
  /// class Program
  /// {
  ///     static void Main()
  ///     {
  ///         using (FileStream fileStream = File.OpenRead("example.txt"))
  ///         {
  ///             byte[] fileContents = fileStream.ToArray();
  ///             // Use fileContents as needed
  ///         }
  ///     }
  /// }
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static byte[] ToArray(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    if (!@this.CanSeek)
      return _ReadAllBytesNonSeekable(@this);

    @this.Position = 0;
    return _ReadBytesSeekable(@this, @this.Length);
  }

  /// <summary>
  ///   Reads all text from the stream..
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="encoding">The encoding.</param>
  /// <returns>The text from the stream.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static string ReadAllText(this Stream @this, Encoding encoding = null) {
    Against.ThisIsNull(@this);

    encoding ??= Encoding.Default;
    return @this.CanRead ? encoding.GetString(@this.ReadAllBytes()) : null;
  }

  /// <summary>
  ///   Writes all text.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="data">The data.</param>
  /// <param name="encoding">The encoding.</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

    if (typeof(TStruct) == typeof(byte))
      return (TStruct)(object)(byte)@this.ReadByte();
    if (typeof(TStruct) == typeof(bool))
      return (TStruct)(object)(@this.ReadByte() != 0);
    if (typeof(TStruct) == typeof(sbyte))
      return (TStruct)(object)(sbyte)@this.ReadByte();
    if (typeof(TStruct) == typeof(ushort))
      return (TStruct)(object)_ReadLittleEndianU16(@this);
    if (typeof(TStruct) == typeof(short))
      return (TStruct)(object)(short)_ReadLittleEndianU16(@this);
    if (typeof(TStruct) == typeof(char))
      return (TStruct)(object)(char)_ReadLittleEndianU16(@this);
    if (typeof(TStruct) == typeof(uint))
      return (TStruct)(object)_ReadLittleEndianU32(@this);
    if (typeof(TStruct) == typeof(int))
      return (TStruct)(object)(int)_ReadLittleEndianU32(@this);
    if (typeof(TStruct) == typeof(ulong))
      return (TStruct)(object)_ReadLittleEndianU64(@this);
    if (typeof(TStruct) == typeof(long))
      return (TStruct)(object)(long)_ReadLittleEndianU64(@this);
    if (typeof(TStruct) == typeof(float))
      return (TStruct)(object)_ReadLittleEndianF32(@this);
    if (typeof(TStruct) == typeof(double))
      return (TStruct)(object)_ReadLittleEndianF64(@this);
    if (typeof(TStruct) == typeof(decimal))
      return (TStruct)(object)_ReadLittleEndianM128(@this);

    return BytesToStruct(_ReadBytes(@this, Marshal.SizeOf(typeof(TStruct))));
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

    if (typeof(TStruct) == typeof(byte))
      @this.WriteByte((byte)(object)value);
    else if (typeof(TStruct) == typeof(bool))
      @this.WriteByte((bool)(object)value ? (byte)255 : (byte)0);
    else if (typeof(TStruct) == typeof(sbyte))
      @this.WriteByte((byte)(sbyte)(object)value);
    else if (typeof(TStruct) == typeof(ushort))
      _WriteLittleEndianU16(@this, (ushort)(object)value);
    else if (typeof(TStruct) == typeof(short))
      _WriteLittleEndianU16(@this, (ushort)(short)(object)value);
    else if (typeof(TStruct) == typeof(char))
      _WriteLittleEndianU16(@this, (char)(object)value);
    else if (typeof(TStruct) == typeof(uint))
      _WriteLittleEndianU32(@this, (uint)(object)value);
    else if (typeof(TStruct) == typeof(int))
      _WriteLittleEndianU32(@this, (uint)(int)(object)value);
    else if (typeof(TStruct) == typeof(ulong))
      _WriteLittleEndianU64(@this, (ulong)(object)value);
    else if (typeof(TStruct) == typeof(long))
      _WriteLittleEndianU64(@this, (ulong)(long)(object)value);
    else if (typeof(TStruct) == typeof(float))
      _WriteLittleEndianF32(@this, (float)(object)value);
    else if (typeof(TStruct) == typeof(double))
      _WriteLittleEndianF64(@this, (double)(object)value);
    else if (typeof(TStruct) == typeof(decimal))
      _WriteLittleEndianM128(@this, (decimal)(object)value);
    else
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ReadBytes(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    Against.False(@this.CanRead);

    _SeekToPositionAndCheck(@this, position, buffer.Length, seekOrigin);
    _ReadBytesToArraySeekable(@this, buffer, 0, buffer.Length);
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static async Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => await ReadBytesAsync(@this, position, buffer, 0, buffer.Length, seekOrigin);

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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static async Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, int offset, int count, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    return await Task.Run(async () => {
      _SeekToPositionAndCheck(@this, position, count, seekOrigin);
      return await @this.ReadAsync(buffer, offset, count);
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static async Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => await ReadBytesAsync(@this, position, buffer, 0, buffer.Length, token, seekOrigin);

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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static async Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, int offset, int count, CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    return await Task.Run(async () => {
      _SeekToPositionAndCheck(@this, position, count, seekOrigin);
      return await @this.ReadAsync(buffer, offset, count, token);
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

    var buffer = new byte[_BUFFER_SIZE];
    int count;
    while ((count = @this.Read(buffer, 0, buffer.Length)) != 0)
      target.Write(buffer, 0, count);
  }

  /// <summary>
  /// Flushes the <see cref="Stream"/>.
  /// </summary>
  /// <param name="this">This <see cref="Stream"/>.</param>
  /// <param name="_">Dummy, ignored</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Flush(this Stream @this, bool _) => @this.Flush();

#endif

}

