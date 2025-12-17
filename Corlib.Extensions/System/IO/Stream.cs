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

using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Guard;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

/// <summary>
///   Extensions for Streams.
/// </summary>
public static partial class StreamExtensions {
  private const int _BUFFER_SIZE = 4 * 1024 * 16;

  #region nested types

#if !SUPPORTS_SPAN

  private readonly struct BufferHandle(byte[] buffer) : IDisposable {
    public readonly byte[] Buffer = buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => PrimitiveConversionBufferManager.ReleaseBuffer(this.Buffer);
  }

  private static class PrimitiveConversionBufferManager {
    private const int BufferSize = 64; // Fixed buffer size, enough to keep the largest primitive datatype
    private static byte[] _sharedBuffer = new byte[BufferSize];

    [ThreadStatic] private static byte[] threadLocalBuffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BufferHandle GetBuffer() => new(Interlocked.Exchange(ref _sharedBuffer, null) ?? (threadLocalBuffer ??= new byte[BufferSize]));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReleaseBuffer(byte[] buffer) {
      if (buffer != threadLocalBuffer)
        Interlocked.Exchange(ref _sharedBuffer, buffer);
    }
  }

#endif

  #endregion

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static unsafe void ThrowEndOfStreamIfNeeded<TType>(int size) where TType:unmanaged {
    if(size < sizeof(TType))
      throw new EndOfStreamException("Read past end of stream");
  }
  
  /// <summary>
  ///   Writes a whole array of bytes to a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="data">The data to write.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, byte[] data) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(data);
    Against.False(@this.CanWrite);

    @this.Write(data, 0, data.Length);
  }

  /// <summary>
  ///   Fills a whole array with bytes from a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="result">The array where to store the results.</param>
  /// <returns>The number of bytes actually read.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Read(this Stream @this, byte[] result) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(result);
    Against.False(@this.CanRead);

    return @this.Read(result, 0, result.Length);
  }

  /// <summary>
  ///   Tries to read a given number of bytes from a stream.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="count">The number of bytes to read.</param>
  /// <returns>The number of bytes actually read.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ReadBytes(this Stream @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);
    Against.False(@this.CanRead);

    return _ReadBytes(@this, count);
  }

  /// <summary>
  ///   Reads all bytes from the current position of the given <see cref="Stream" /> and returns them as a byte array.
  /// </summary>
  /// <param name="this">The <see cref="Stream" /> instance on which the extension method is called.</param>
  /// <returns>A byte array containing the bytes read from the <see cref="Stream" />.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  ///   Thrown when the stream's available number of bytes exceeds 2GB, which is the maximum length supported by a single
  ///   array in .NET.
  /// </exception>
  /// <remarks>
  ///   The method reads bytes into a byte array, which has a maximum indexable length of <see cref="int.MaxValue" />
  ///   (2,147,483,647) elements,
  ///   roughly equating to a 2GB size limit. Attempting to read a stream larger than this limit will result in an overflow
  ///   of the array index.
  /// </remarks>
  /// <example>
  ///   This example shows how to use the <see cref="ReadAllBytes" /> extension method
  ///   to read all bytes from a file stream and store them in a byte array.
  ///   <code>
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ReadAllBytes(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return @this.CanSeek ? _ReadBytesSeekable(@this, @this.Length - @this.Position) : _ReadAllBytesNonSeekable(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] _ReadAllBytesNonSeekable(Stream @this) {
    using MemoryStream data = new(_BUFFER_SIZE);
    @this.CopyTo(data);
    return data.ToArray();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte[] _ReadBytes(Stream @this, long count)
    => @this.CanSeek ? _ReadBytesSeekable(@this, count) : _ReadBytesNonSeekable(@this, count);

  /// <summary>
  ///   Some non-seekable streams have problems when reading into array buffers, so they need slow byte-by-byte reading
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, bool value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte(value ? (byte)255 : (byte)0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ReadBool(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return @this.ReadByte() != 0;
  }

  #endregion

  #region byte

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, byte value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte(value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ReadUInt8(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (byte)@this.ReadByte();
  }

  #endregion

  #region sbyte

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, sbyte value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte((byte)value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte ReadInt8(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (sbyte)@this.ReadByte();
  }

  #endregion

  #region ushort

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe ushort _ReadLittleEndianU16(Stream stream) {
#if SUPPORTS_SPAN
    ushort result = 0;
    var bytes = new Span<byte>(&result, sizeof(ushort));
    ThrowEndOfStreamIfNeeded<ushort>(stream.Read(bytes));
    return result;
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    ThrowEndOfStreamIfNeeded<ushort>(stream.Read(handle.Buffer, 0, sizeof(ushort)));
    fixed (byte* bytes = handle.Buffer)
      return *(ushort*)bytes;
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe ushort _ReadBigEndianU16(Stream stream) {
    ushort result = 0;
    var ptr = (byte*)&result;

#if SUPPORTS_SPAN
    var bytes = new Span<byte>(ptr, sizeof(ushort));
    ThrowEndOfStreamIfNeeded<ushort>(stream.Read(bytes));
    (*ptr, ptr[1]) = (ptr[1], *ptr);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    ThrowEndOfStreamIfNeeded<ushort>(stream.Read(handle.Buffer, 0, sizeof(ushort)));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1]) = (bytes[1], *bytes);
#endif

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, ushort value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, ushort value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, value);
    else
      _WriteLittleEndianU16(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort ReadUInt16(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU16(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort ReadUInt16(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this);
  }

  #endregion

  #region short

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, short value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, (ushort)value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, short value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, (ushort)value);
    else
      _WriteLittleEndianU16(@this, (ushort)value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short ReadInt16(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (short)_ReadLittleEndianU16(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short ReadInt16(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (short)(bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this));
  }

  #endregion

  #region char

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, char value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, char value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, value);
    else
      _WriteLittleEndianU16(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char ReadChar(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (char)_ReadLittleEndianU16(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char ReadChar(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (char)(bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this));
  }

  #endregion

  #region uint

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe uint _ReadLittleEndianU32(Stream stream) {
#if SUPPORTS_SPAN
    var result = 0U;
    var bytes = new Span<byte>(&result, sizeof(uint));
    ThrowEndOfStreamIfNeeded<uint>(stream.Read(bytes));
    return result;
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    ThrowEndOfStreamIfNeeded<uint>(stream.Read(handle.Buffer, 0, sizeof(uint)));
    fixed (byte* bytes = handle.Buffer)
      return *(uint*)bytes;
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe uint _ReadBigEndianU32(Stream stream) {
    var result = 0U;
    var ptr = (byte*)&result;

#if SUPPORTS_SPAN
    var bytes = new Span<byte>(ptr, sizeof(uint));
    ThrowEndOfStreamIfNeeded<uint>(stream.Read(bytes));
    (*ptr, ptr[1], ptr[2], ptr[3]) = (ptr[3], ptr[2], ptr[1], *ptr);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    ThrowEndOfStreamIfNeeded<uint>(stream.Read(handle.Buffer, 0, sizeof(uint)));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1], ptr[2], ptr[3]) = (bytes[3], bytes[2], bytes[1], *bytes);
#endif

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, uint value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU32(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, uint value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU32(@this, value);
    else
      _WriteLittleEndianU32(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReadUInt32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU32(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReadUInt32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU32(@this) : _ReadLittleEndianU32(@this);
  }

  #endregion

  #region int

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, int value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU32(@this, (uint)value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, int value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU32(@this, (uint)value);
    else
      _WriteLittleEndianU32(@this, (uint)value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ReadInt32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (int)_ReadLittleEndianU32(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ReadInt32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (int)(bigEndian ? _ReadBigEndianU32(@this) : _ReadLittleEndianU32(@this));
  }

  #endregion

  #region ulong

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe ulong _ReadLittleEndianU64(Stream stream) {
#if SUPPORTS_SPAN
    var result = 0UL;
    var bytes = new Span<byte>(&result, sizeof(ulong));
    ThrowEndOfStreamIfNeeded<ulong>(stream.Read(bytes));
    return result;
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    ThrowEndOfStreamIfNeeded<ulong>(stream.Read(handle.Buffer, 0, sizeof(ulong)));
    fixed (byte* bytes = handle.Buffer)
      return *(ulong*)bytes;
#endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe ulong _ReadBigEndianU64(Stream stream) {
    var result = 0UL;
    var ptr = (byte*)&result;

#if SUPPORTS_SPAN
    var bytes = new Span<byte>(ptr, sizeof(ulong));
    ThrowEndOfStreamIfNeeded<ulong>(stream.Read(bytes));
    (*ptr, ptr[1], ptr[2], ptr[3], ptr[4], ptr[5], ptr[6], ptr[7]) = (ptr[7], ptr[6], ptr[5], ptr[4], ptr[3], ptr[2], ptr[1], *ptr);
#else
    using var handle = PrimitiveConversionBufferManager.GetBuffer();
    ThrowEndOfStreamIfNeeded<ulong>(stream.Read(handle.Buffer, 0, sizeof(ulong)));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1], ptr[2], ptr[3], ptr[4], ptr[5], ptr[6], ptr[7]) = (bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], *bytes);
#endif

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, ulong value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU64(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, ulong value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU64(@this, value);
    else
      _WriteLittleEndianU64(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ReadUInt64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU64(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ReadUInt64(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU64(@this) : _ReadLittleEndianU64(@this);
  }

  #endregion

  #region long

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, long value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU64(@this, (ulong)value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, long value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU64(@this, (ulong)value);
    else
      _WriteLittleEndianU64(@this, (ulong)value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long ReadInt64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (long)_ReadLittleEndianU64(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long ReadInt64(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (long)(bigEndian ? _ReadBigEndianU64(@this) : _ReadLittleEndianU64(@this));
  }

  #endregion

  #region single

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _WriteLittleEndianF32(Stream stream, float value) => _WriteLittleEndianU32(stream, *(uint*)&value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _WriteBigEndianF32(Stream stream, float value) => _WriteBigEndianU32(stream, *(uint*)&value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float _ReadLittleEndianF32(Stream stream) {
    var result = _ReadLittleEndianU32(stream);
    return *(float*)&result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float _ReadBigEndianF32(Stream stream) {
    var result = _ReadBigEndianU32(stream);
    return *(float*)&result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, float value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianF32(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, float value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianF32(@this, value);
    else
      _WriteLittleEndianF32(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float ReadFloat32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianF32(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float ReadFloat32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianF32(@this) : _ReadLittleEndianF32(@this);
  }

  #endregion

  #region double

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _WriteLittleEndianF64(Stream stream, double value) => _WriteLittleEndianU64(stream, *(ulong*)&value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _WriteBigEndianF64(Stream stream, double value) => _WriteBigEndianU64(stream, *(ulong*)&value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe double _ReadLittleEndianF64(Stream stream) {
    var result = _ReadLittleEndianU64(stream);
    return *(double*)&result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe double _ReadBigEndianF64(Stream stream) {
    var result = _ReadBigEndianU64(stream);
    return *(double*)&result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, double value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianF64(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, double value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianF64(@this, value);
    else
      _WriteLittleEndianF64(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double ReadFloat64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianF64(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, decimal value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianM128(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write(this Stream @this, decimal value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianM128(@this, value);
    else
      _WriteLittleEndianM128(@this, value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal ReadMoney128(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianM128(@this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal ReadMoney128(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianM128(@this) : _ReadLittleEndianM128(@this);
  }

  #endregion

  #region Strings

  public static void WriteLengthPrefixedString(this Stream @this, string data, Encoding encoding = null) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);
    Against.ArgumentIsNull(data);

    encoding ??= Encoding.UTF8;
    var rawData = encoding.GetBytes(data);
    _WriteLittleEndianU32(@this, (uint)rawData.Length);
    @this.Write(rawData, 0, rawData.Length);
  }

  public static string ReadLengthPrefixedString(this Stream @this, Encoding encoding = null) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    encoding ??= Encoding.UTF8;

    var length = _ReadLittleEndianU32(@this);
    var buffer = new byte[length];
    var bytesRead = @this.Read(buffer, 0, (int)length);
    if (bytesRead != length)
      throw new EndOfStreamException("Unexpected end of stream while reading length-prefixed string.");

    return encoding.GetString(buffer);
  }
  
  public static void WriteZeroTerminatedString(this Stream @this, string data, Encoding encoding = null) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);
    Against.ArgumentIsNull(data);
    Against.True(data.Contains('\0'));

    encoding ??= Encoding.UTF8;
    var rawData = encoding.GetBytes(data + '\0');
    @this.Write(rawData, 0, rawData.Length);
  }

  public static string ReadZeroTerminatedString(this Stream @this, Encoding encoding = null) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    encoding ??= Encoding.UTF8;
    var @null = encoding.GetBytes("\0");
    using var buffer = new MemoryStream(_BUFFER_SIZE);
    
    if (encoding.IsSingleByte) {
      var nullPattern = @null[0];
      for (;;) {
        var data = @this.ReadByte();
        if (data < 0)
          throw new EndOfStreamException("Unexpected end of stream while reading zero-terminated string.");

        if (data == nullPattern)
          return buffer.Length == 0 
            ? string.Empty 
            : encoding.GetString(buffer.GetBuffer(),0,(int)buffer.Length)
            ;

        buffer.WriteByte((byte)data);
      }
    }

    var nullPatternLength = @null.Length;
    for (;;) {
      var data = @this.ReadByte();
      if (data < 0)
        throw new EndOfStreamException("Unexpected end of stream while reading zero-terminated string.");

      buffer.WriteByte((byte)data);
      var nullPatternOffsetInBuffer = (int)(buffer.Length - nullPatternLength);

      // not enough bytes yet?
      if (nullPatternOffsetInBuffer < 0)
        continue;
      
      // does the buffer end with the null sequence?
      if (!@null.SequenceEqual(0, buffer.GetBuffer(), nullPatternOffsetInBuffer, nullPatternLength))
        continue;

      return nullPatternOffsetInBuffer == 0 
        ? string.Empty 
        : encoding.GetString(buffer.GetBuffer(),0, nullPatternOffsetInBuffer)
        ;
    }
  }

  private class ByteEncoding : Encoding {

    public static ByteEncoding Instance => new();

    public override int GetByteCount(char[] chars, int index, int count) => count;

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
      for (var i = 0; i < charCount; ++i)
        bytes[byteIndex + i] = (byte)chars[charIndex + i];

      return charCount;
    }

    public override int GetCharCount(byte[] bytes, int index, int count) => count;

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
      for (var i = 0; i < byteCount; ++i)
        chars[charIndex + i] = (char)bytes[byteIndex + i];

      return byteCount;
    }

    public override int GetMaxByteCount(int charCount) => charCount;

    public override int GetMaxCharCount(int byteCount) => byteCount;
  }

  public static void WriteFixedLengthString(this Stream @this, string data, int length, char padding='\0', Encoding encoding = null) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);
    Against.ArgumentIsNull(data);
    Against.CountOutOfRange(data.Length,length);

    encoding ??= ByteEncoding.Instance;
    
    var rawData = encoding.GetBytes(data.PadRight(length,padding));
    if (rawData.Length != encoding.GetMaxByteCount(length))
      throw new ArgumentException($"Encoding '{encoding.EncodingName}' is variable-length and cannot be used for fixed-length strings.");

    @this.Write(rawData, 0, rawData.Length);
  }

  public static string ReadFixedLengthString(this Stream @this, int length, char padding = '\0', Encoding encoding = null) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    encoding ??= ByteEncoding.Instance;
    var buffer = new byte[encoding.GetMaxByteCount(length)];

    var bytesRead = @this.Read(buffer, 0, length);
    if (bytesRead != length)
      throw new EndOfStreamException("Unexpected end of stream while reading fixed-length string.");

    return encoding.GetString(buffer).TrimEnd(padding);
  }


  #endregion

  #endregion

  /// <summary>
  ///   Determines whether the current <see cref="Stream" /> position pointer is at the end of the <see cref="Stream" />.
  ///   This method is applicable to streams that support seeking and specific <see cref="Stream" /> types like
  ///   <see cref="NetworkStream" />.
  ///   For other non-seekable streams, the method throws an <see cref="InvalidOperationException" />, as checking for the
  ///   end-of-stream
  ///   without altering the <see cref="Stream" /> state may not be possible.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the <see cref="Stream" /> position is at the end for seekable streams or if no more data
  ///   is available in a <see cref="NetworkStream" />;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  ///   Thrown when the <see cref="Stream" /> does not support seeking or is not a recognized type (like
  ///   <see cref="NetworkStream" />) that allows safe EOF checking.
  /// </exception>
  /// <remarks>
  ///   For seekable streams, this method checks if the current position is at or beyond the end of the <see cref="Stream" />
  ///   .
  ///   For <see cref="NetworkStream" />, it checks the availability of data to read.
  ///   For other non-seekable stream types, the caller should ensure an appropriate method to check for the end of the
  ///   <see cref="Stream" />,
  ///   as this method will throw an <see cref="InvalidOperationException" />.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
  ///   Reads all bytes from the starting position of the given <see cref="Stream" /> and returns them as a byte array.
  /// </summary>
  /// <param name="this">The <see cref="Stream" /> instance on which the extension method is called.</param>
  /// <returns>A byte array containing the bytes read from the <see cref="Stream" />.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  ///   Thrown when the stream's available number of bytes exceeds 2GB, which is the maximum length supported by a single
  ///   array in .NET.
  /// </exception>
  /// <remarks>
  ///   If the <see cref="Stream" /> is not seekable, the bytes are read from the current position.
  ///   The method reads bytes into a byte array, which has a maximum indexable length of <see cref="int.MaxValue" />
  ///   (2,147,483,647) elements,
  ///   roughly equating to a 2GB size limit. Attempting to read a stream larger than this limit will result in an overflow
  ///   of the array index.
  /// </remarks>
  /// <example>
  ///   This example shows how to use the <see cref="ToArray" /> extension method
  ///   to read all bytes from a file stream and store them in a byte array.
  ///   <code>
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="encoding">The encoding.</param>
  /// <returns>The text from the stream.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ReadAllText(this Stream @this, Encoding encoding = null) {
    Against.ThisIsNull(@this);

    encoding ??= Encoding.Default;
    return @this.CanRead ? encoding.GetString(@this.ReadAllBytes()) : null;
  }

  /// <summary>
  ///   Writes all text.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="data">The data.</param>
  /// <param name="encoding">The encoding.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    static TStruct BytesToStruct(byte[] buffer) {
      var size = buffer.Length;
      var unmanagedMemory = IntPtr.Zero;
      try {
        unmanagedMemory = Marshal.AllocHGlobal(size);
        Marshal.Copy(buffer, 0, unmanagedMemory, size);
#if NETCOREAPP
        var result = Marshal.PtrToStructure<TStruct>(unmanagedMemory);
#else
        var result = (TStruct)Marshal.PtrToStructure(unmanagedMemory, typeof(TStruct));
#endif
        return result;
      } finally {
        if (unmanagedMemory != IntPtr.Zero)
          Marshal.FreeHGlobal(unmanagedMemory);
      }
    }
  }

  /// <summary>
  ///   Writes the given structure to the stream.
  /// </summary>
  /// <typeparam name="TStruct">The type of the structure.</typeparam>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="value">The value.</param>
  public static void Write<TStruct>(this Stream @this, TStruct value) where TStruct : struct {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

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
    return;

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
  }

  /// <summary>
  ///   Read Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  // Note: not thread safe
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ReadBytes(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    Against.False(@this.CanRead);

    _SeekToPositionAndCheck(@this, position, (int)Math.Min(buffer.Length, @this.Length - position), seekOrigin);
    _ReadBytesToArraySeekable(@this, buffer, 0, buffer.Length);
  }

#if SUPPORTS_STREAM_ASYNC

  /// <summary>
  ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A awaitable Task representing the operation</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => ReadBytesAsync(@this, position, buffer, 0, (int)Math.Min(buffer.Length, @this.Length - position), seekOrigin);

  /// <summary>
  ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer with an offset
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="offset">The offset in the buffer</param>
  /// <param name="count">The amount of bytes you want to read</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A awaitable Task representing the operation</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, int offset, int count, SeekOrigin seekOrigin = SeekOrigin.Begin) 
    => Task.Run(
      () => {
        _SeekToPositionAndCheck(@this, position, count, seekOrigin);
        return @this.ReadAsync(buffer, offset, count);
      }
    );

  /// <summary>
  ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="token">The Cancellation Token</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A awaitable Task representing the operation</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => ReadBytesAsync(@this, position, buffer, 0, buffer.Length, token, seekOrigin);

  /// <summary>
  ///   Reads async Bytes from a given position with a given SeekOrigin in the given buffer with an offset
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="offset"></param>
  /// <param name="count"></param>
  /// <param name="token">The Cancellation Token</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A awaitable Task representing the operation</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, int offset, int count, CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin) 
    => Task.Run(
      () => {
        _SeekToPositionAndCheck(@this, position, count, seekOrigin);
        return @this.ReadAsync(buffer, offset, count, token);
      },
      token
    );
  
#endif

  /// <summary>
  ///   Begins reading Bytes from a given position with a given SeekOrigin in the given buffer
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="callback">The callback you want to get called</param>
  /// <param name="state">The given State</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns>A IAsyncResult representing the operation</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IAsyncResult BeginReadBytes(this Stream @this, long position, byte[] buffer, AsyncCallback callback, object state = null, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => BeginReadBytes(@this, position, buffer, 0, buffer.Length, callback, state, seekOrigin);

  /// <summary>
  ///   Begins reading Bytes from a given position with a given SeekOrigin in the given buffer with an offset
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="position">The position from which you want to read</param>
  /// <param name="buffer">The buffer where the result is written in</param>
  /// <param name="offset">The offset in the buffer</param>
  /// <param name="count">The amount of bytes you want to read</param>
  /// <param name="callback">The callback you want to get called</param>
  /// <param name="state">The given State</param>
  /// <param name="seekOrigin">The SeekOrigin from where did you want to start</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IAsyncResult BeginReadBytes(this Stream @this, long position, byte[] buffer, int offset, int count, AsyncCallback callback, object state = null, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    _SeekToPositionAndCheck(@this, position, count, seekOrigin);
    return @this.BeginRead(buffer, offset, count, callback, state);
  }

  /// <summary>
  ///   Ends to read bytes
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="result">The IAsyncResult representing the result of the Begin operation</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void EndReadBytes(this Stream @this, IAsyncResult result)
    => @this.EndRead(result);

  /// <summary>
  ///   Seeks to the gives position and checks if the position is valid
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

    Against.CountOutOfRange(absolutePosition + wantedBytes, stream.Length);
    stream.Seek(absolutePosition, SeekOrigin.Begin);
  }
}
