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
using System.Linq;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#if SUPPORTS_ASYNC
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

  #region nested types

  private struct BufferHandle : IDisposable {
    public readonly byte[] Buffer;
    public BufferHandle(byte[] buffer) => this.Buffer = buffer;
    public void Dispose() => BufferManager.ReleaseBuffer(this.Buffer);
  }

  private static class BufferManager {
    private const int BufferSize = 64; // Fixed buffer size
    private static readonly byte[] sharedBuffer = new byte[BufferSize];

    private const int _FREE = 0;
    private const int _USED = -1;
    private static int isSharedBufferInUse = _FREE;

    [ThreadStatic]
    private static byte[] threadLocalBuffer;

    public static BufferHandle GetBuffer() {
      if (Interlocked.CompareExchange(ref isSharedBufferInUse, _USED, _FREE) == _FREE)
        return new(sharedBuffer);

      return new(threadLocalBuffer ??= new byte[BufferSize]);
    }

    public static void ReleaseBuffer(byte[] buffer) {
      if (buffer == sharedBuffer)
        Interlocked.Exchange(ref isSharedBufferInUse, _FREE);
    }
  }

  #endregion

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

  #region bool

  public static void Write(this Stream @this, bool value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte(value? (byte)255 : (byte)0);
  }

  public static bool ReadBool(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return @this.ReadByte() != 0;
  }

  #endregion

  #region byte

  public static void Write(this Stream @this, byte value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte(value);
  }

  public static byte ReadUInt8(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (byte)@this.ReadByte();
  }

  #endregion

  #region sbyte

  public static void Write(this Stream @this, sbyte value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    @this.WriteByte((byte)value);
  }

  public static sbyte ReadInt8(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (sbyte)@this.ReadByte();
  }

  #endregion

  #region ushort

#if UNSAFE

  private static unsafe void _WriteLittleEndianU16(Stream stream, ushort value) {
    using var handle = BufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      *(ushort*)bytes = value;
    
    stream.Write(handle.Buffer,0,sizeof(ushort));
  }

  private static unsafe void _WriteBigEndianU16(Stream stream, ushort value) {
    var ptr = (byte*)&value;
    using var handle = BufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      (*bytes, bytes[1]) = (ptr[1], *ptr);

    stream.Write(handle.Buffer, 0, sizeof(ushort));
  }

  private static unsafe ushort _ReadLittleEndianU16(Stream stream) {
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    fixed (byte* bytes = handle.Buffer)
      return *(ushort*)bytes;
  }
  
  private static unsafe ushort _ReadBigEndianU16(Stream stream) {
    var result = (ushort)0;
    var ptr = (byte*)&result;

    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1]) = (bytes[1], *bytes);

    return result;
  }

#else

  private static void _WriteLittleEndianU16(Stream stream, ushort value) {
    using var handle = BufferManager.GetBuffer();
    handle.Buffer[0] = (byte)value;
    handle.Buffer[1] = (byte)(value >> 8);
    stream.Write(handle.Buffer, 0, sizeof(ushort));
  }

  private static void _WriteBigEndianU16(Stream stream, ushort value) {
    using var handle = BufferManager.GetBuffer();
    handle.Buffer[1] = (byte)value;
    handle.Buffer[0] = (byte)(value >> 8);
    stream.Write(handle.Buffer, 0, sizeof(ushort));
  }

  private static ushort _ReadLittleEndianU16(Stream stream) {
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    return (ushort)(handle.Buffer[0] | (handle.Buffer[1] << 8));
  }

  private static ushort _ReadBigEndianU16(Stream stream) {
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ushort));
    return (ushort)(handle.Buffer[1] | (handle.Buffer[0] << 8));
  }

#endif

  public static void Write(this Stream @this, ushort value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, value);
  }

  public static void Write(this Stream @this, ushort value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, value);
    else
      _WriteLittleEndianU16(@this, value);
  }

  public static ushort ReadUInt16(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU16(@this);
  }

  public static ushort ReadUInt16(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this);
  }

  #endregion

  #region short

  public static void Write(this Stream @this, short value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, (ushort)value);
  }

  public static void Write(this Stream @this, short value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, (ushort)value);
    else
      _WriteLittleEndianU16(@this, (ushort)value);
  }

  public static short ReadInt16(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (short)_ReadLittleEndianU16(@this);
  }

  public static short ReadInt16(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (short)(bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this));
  }

  #endregion

  #region char

  public static void Write(this Stream @this, char value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU16(@this, value);
  }

  public static void Write(this Stream @this, char value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU16(@this, value);
    else
      _WriteLittleEndianU16(@this, value);
  }

  public static char ReadChar(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (char)_ReadLittleEndianU16(@this);
  }

  public static char ReadChar(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (char)(bigEndian ? _ReadBigEndianU16(@this) : _ReadLittleEndianU16(@this));
  }

  #endregion

  #region uint


#if UNSAFE

  private static unsafe void _WriteLittleEndianU32(Stream stream, uint value) {
    using var handle = BufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      *(uint*)bytes = value;

    stream.Write(handle.Buffer, 0, sizeof(uint));
  }

  private static unsafe void _WriteBigEndianU32(Stream stream, uint value) {
    var ptr = (byte*)&value;
    using var handle = BufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      (*bytes, bytes[1], bytes[2], bytes[3]) = (ptr[3], ptr[2], ptr[1], *ptr);

    stream.Write(handle.Buffer, 0, sizeof(uint));
  }

  private static unsafe uint _ReadLittleEndianU32(Stream stream) {
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    fixed (byte* bytes = handle.Buffer)
      return *(uint*)bytes;
  }

  private static unsafe uint _ReadBigEndianU32(Stream stream) {
    var result = (uint)0;
    var ptr = (byte*)&result;

    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1], ptr[2], ptr[3]) = (bytes[3], bytes[2], bytes[1], *bytes);

    return result;
  }

#else

  private static void _WriteLittleEndianU32(Stream stream, uint value) {
    using var handle = BufferManager.GetBuffer();
    handle.Buffer[0] = (byte)value;
    handle.Buffer[1] = (byte)(value >> 8);
    handle.Buffer[2] = (byte)(value >> 16);
    handle.Buffer[3] = (byte)(value >> 24);
    stream.Write(handle.Buffer, 0, sizeof(uint));
  }

  private static void _WriteBigEndianU32(Stream stream, uint value) {
    using var handle = BufferManager.GetBuffer();
    handle.Buffer[3] = (byte)value;
    handle.Buffer[2] = (byte)(value >> 8);
    handle.Buffer[1] = (byte)(value >> 16);
    handle.Buffer[0] = (byte)(value >> 24);
    stream.Write(handle.Buffer, 0, sizeof(uint));
  }

  private static uint _ReadLittleEndianU32(Stream stream) {
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    return (uint)(handle.Buffer[0] | (handle.Buffer[1] << 8) | (handle.Buffer[2] << 16) | (handle.Buffer[3] << 24));
  }

  private static uint _ReadBigEndianU32(Stream stream) {
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(uint));
    return (uint)(handle.Buffer[3] | (handle.Buffer[2] << 8) | (handle.Buffer[1] << 16) | (handle.Buffer[0] << 24));
  }

#endif

  public static void Write(this Stream @this, uint value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU32(@this, value);
  }

  public static void Write(this Stream @this, uint value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU32(@this, value);
    else
      _WriteLittleEndianU32(@this, value);
  }

  public static uint ReadUInt32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU32(@this);
  }

  public static uint ReadUInt32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU32(@this) : _ReadLittleEndianU32(@this);
  }


  #endregion

  #region int

  public static void Write(this Stream @this, int value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU32(@this, (uint)value);
  }

  public static void Write(this Stream @this, int value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU32(@this, (uint)value);
    else
      _WriteLittleEndianU32(@this, (uint)value);
  }

  public static int ReadInt32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (int)_ReadLittleEndianU32(@this);
  }

  public static int ReadInt32(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (int)(bigEndian ? _ReadBigEndianU32(@this) : _ReadLittleEndianU32(@this));
  }

  #endregion


  #region ulong


#if UNSAFE

  private static unsafe void _WriteLittleEndianU64(Stream stream, ulong value) {
    using var handle = BufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      *(ulong*)bytes = value;

    stream.Write(handle.Buffer, 0, sizeof(ulong));
  }

  private static unsafe void _WriteBigEndianU64(Stream stream, ulong value) {
    var ptr = (byte*)&value;
    using var handle = BufferManager.GetBuffer();
    fixed (byte* bytes = handle.Buffer)
      (*bytes, bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]) = (ptr[7], ptr[6], ptr[5], ptr[4], ptr[3], ptr[2], ptr[1], *ptr);
    
    stream.Write(handle.Buffer, 0, sizeof(ulong));
  }

  private static unsafe ulong _ReadLittleEndianU64(Stream stream) {
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ulong));
    fixed (byte* bytes = handle.Buffer)
      return *(ulong*)bytes;
  }

  private static unsafe ulong _ReadBigEndianU64(Stream stream) {
    var result = (ulong)0;
    var ptr = (byte*)&result;
    
    using var handle = BufferManager.GetBuffer();
    // ReSharper disable once MustUseReturnValue
    stream.Read(handle.Buffer, 0, sizeof(ulong));
    fixed (byte* bytes = handle.Buffer)
      (*ptr, ptr[1], ptr[2], ptr[3], ptr[4], ptr[5], ptr[6], ptr[7]) = (bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], *bytes);
    
    return result;
  }

#else

  private static void _WriteLittleEndianU64(Stream stream, ulong value) {
    using var handle = BufferManager.GetBuffer();
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

  private static void _WriteBigEndianU64(Stream stream, ulong value) {
    using var handle = BufferManager.GetBuffer();
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

  private static ulong _ReadLittleEndianU64(Stream stream) => _ReadLittleEndianU32(stream) | ((ulong)_ReadLittleEndianU32(stream) << 32);

  private static ulong _ReadBigEndianU64(Stream stream) => ((ulong)_ReadBigEndianU32(stream) << 32) | _ReadBigEndianU32(stream);

#endif

  public static void Write(this Stream @this, ulong value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU64(@this, value);
  }

  public static void Write(this Stream @this, ulong value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU64(@this, value);
    else
      _WriteLittleEndianU64(@this, value);
  }

  public static ulong ReadUInt64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianU64(@this);
  }

  public static ulong ReadUInt64(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianU64(@this) : _ReadLittleEndianU64(@this);
  }


  #endregion

  #region long

  public static void Write(this Stream @this, long value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianU64(@this, (ulong)value);
  }

  public static void Write(this Stream @this, long value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianU64(@this, (ulong)value);
    else
      _WriteLittleEndianU64(@this, (ulong)value);
  }

  public static long ReadInt64(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (long)_ReadLittleEndianU64(@this);
  }

  public static long ReadInt64(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return (long)(bigEndian ? _ReadBigEndianU64(@this) : _ReadLittleEndianU64(@this));
  }

  #endregion

  #region single

  private static void _WriteLittleEndianF32(Stream stream, float value) {
    var bytes = BitConverter.GetBytes(value);
    stream.Write(bytes, 0, sizeof(float));
  }

  private static void _WriteBigEndianF32(Stream stream, float value) {
    var bytes = BitConverter.GetBytes(value);
    (bytes[0], bytes[1], bytes[2], bytes[3]) = (bytes[3], bytes[2], bytes[1], bytes[0]);
    stream.Write(bytes, 0, sizeof(float));
  }

  private static float _ReadLittleEndianF32(Stream stream) {
    var bytes = new byte[sizeof(float)];
    var got = stream.Read(bytes, 0, sizeof(float));
    return got != sizeof(float) ? float.NaN : BitConverter.ToSingle(bytes, 0);
  }

  private static float _ReadBigEndianF32(Stream stream) {
    var bytes = new byte[sizeof(float)];
    var got = stream.Read(bytes, 0, sizeof(float));
    if (got != sizeof(float))
      return float.NaN;

    (bytes[0], bytes[1], bytes[2], bytes[3]) = (bytes[3], bytes[2], bytes[1], bytes[0]);
    return BitConverter.ToSingle(bytes, 0);
  }

  public static void Write(this Stream @this, float value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianF32(@this, value);
  }

  public static void Write(this Stream @this, float value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianF32(@this, value);
    else
      _WriteLittleEndianF32(@this, value);
  }

  public static float ReadFloat16(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianF32(@this);
  }

  public static float ReadFloat16(this Stream @this, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return bigEndian ? _ReadBigEndianF32(@this) : _ReadLittleEndianF32(@this);
  }

  #endregion

  #region double

  private static void _WriteLittleEndianF64(Stream stream, double value) {
    var bytes = BitConverter.GetBytes(value);
    stream.Write(bytes, 0, sizeof(double));
  }

  private static void _WriteBigEndianF64(Stream stream, double value) {
    var bytes = BitConverter.GetBytes(value);
    (bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]) = (bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0]);
    stream.Write(bytes, 0, sizeof(double));
  }

  private static double _ReadLittleEndianF64(Stream stream) {
    var bytes = new byte[sizeof(double)];
    var got = stream.Read(bytes, 0, sizeof(double));
    return got != sizeof(double) ? double.NaN : BitConverter.ToDouble(bytes, 0);
  }

  private static double _ReadBigEndianF64(Stream stream) {
    var bytes = new byte[sizeof(double)];
    var got = stream.Read(bytes, 0, sizeof(double));
    if (got != sizeof(double))
      return double.NaN;

    (bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]) = (bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0]);
    return BitConverter.ToSingle(bytes, 0);
  }

  public static void Write(this Stream @this, double value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianF64(@this, value);
  }

  public static void Write(this Stream @this, double value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianF64(@this, value);
    else
      _WriteLittleEndianF64(@this, value);
  }

  public static double ReadFloat32(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianF64(@this);
  }

  public static double ReadFloat32(this Stream @this, bool bigEndian) {
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

  public static void Write(this Stream @this, decimal value) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    _WriteLittleEndianM128(@this, value);
  }

  public static void Write(this Stream @this, decimal value, bool bigEndian) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanWrite);

    if (bigEndian)
      _WriteBigEndianM128(@this, value);
    else
      _WriteLittleEndianM128(@this, value);
  }

  public static decimal ReadMoney128(this Stream @this) {
    Against.ThisIsNull(@this);
    Against.False(@this.CanRead);

    return _ReadLittleEndianM128(@this);
  }

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

    static byte[] ReadEnoughBytes(Stream stream, int size) {
      var result = new byte[size];
      var offset = 0;
      while (size > 0 && !stream.IsAtEndOfStream()) {
        var read = stream.Read(result, offset, size);
        size -= read;
        offset += read;
      }
      return result;
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
    
    return BytesToStruct(ReadEnoughBytes(@this, Marshal.SizeOf(typeof(TStruct))));
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
  public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = SeekOrigin.Begin)
    => await AsyncReadBytes(@this, position, buffer, 0, buffer.Length, seekOrigin);

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
  public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, int offset, int count, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    await Task.Run(async () => {
      _SeekToPositionAndCheck(@this, position, count, seekOrigin);
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
    => await AsyncReadBytes(@this, position, buffer, 0, buffer.Length, token, seekOrigin);

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
  public static async Task AsyncReadBytes(this Stream @this, long position, byte[] buffer, int offset, int count, CancellationToken token, SeekOrigin seekOrigin = SeekOrigin.Begin) {
    await Task.Run(async () => {
      _SeekToPositionAndCheck(@this, position, count, seekOrigin);
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

