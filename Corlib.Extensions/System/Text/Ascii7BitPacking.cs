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

#nullable enable

using System.Numerics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

/// <summary>
/// Provides 7-bit ASCII packing and unpacking operations.
/// </summary>
/// <remarks>
/// Packs 7-bit ASCII characters (0-127) into a compact format where 8 characters occupy 7 bytes.
/// This saves 12.5% memory compared to storing one byte per character.
/// Uses SIMD operations when available for faster processing.
/// </remarks>
internal static class Ascii7BitPacking {

  // Vector constants for SIMD operations
  private static readonly Vector<byte> _asciiMask = new(0x7F);
  private static readonly Vector<byte> _highBitMask = new(0x80);

  /// <summary>
  /// Gets the number of bytes required to store the specified number of characters in packed format.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GetPackedByteCount(int charCount) => (charCount * 7 + 7) >> 3;

  /// <summary>
  /// Gets the character at the specified index from packed data.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte GetCharAt(byte[] packed, int index) {
    // Each character occupies 7 bits starting at bit position (index * 7)
    var bitPosition = index * 7;
    var byteIndex = bitPosition >> 3;
    var bitOffset = bitPosition & 7;

    // If the character fits entirely within one byte
    if (bitOffset <= 1)
      return (byte)((packed[byteIndex] >> bitOffset) & 0x7F);

    // Character spans two bytes
    var lowBits = packed[byteIndex] >> bitOffset;
    var highBits = packed[byteIndex + 1] << (8 - bitOffset);
    return (byte)((lowBits | highBits) & 0x7F);
  }

  /// <summary>
  /// Sets the character at the specified index in packed data.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void SetCharAt(byte[] packed, int index, byte value) {
    var bitPosition = index * 7;
    var byteIndex = bitPosition >> 3;
    var bitOffset = bitPosition & 7;

    // Mask off the 7 bits we'll write
    value &= 0x7F;

    if (bitOffset <= 1) {
      // Character fits entirely within one byte
      var mask = (byte)~(0x7F << bitOffset);
      packed[byteIndex] = (byte)((packed[byteIndex] & mask) | (value << bitOffset));
    } else {
      // Character spans two bytes
      var lowMask = (byte)(0xFF >> (8 - bitOffset));
      var highMask = (byte)(0xFF << (bitOffset - 1));

      packed[byteIndex] = (byte)((packed[byteIndex] & lowMask) | (value << bitOffset));
      packed[byteIndex + 1] = (byte)((packed[byteIndex + 1] & highMask) | (value >> (8 - bitOffset)));
    }
  }

  /// <summary>
  /// Packs 8 unpacked ASCII bytes into 7 packed bytes.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _Pack8(byte[] unpacked, int unpackedOffset, byte[] packed, int packedOffset) {
    // TODO: optimize using simd/pshufb/pext/vector
    var c0 = unpacked[unpackedOffset];
    var c1 = unpacked[unpackedOffset + 1];
    var c2 = unpacked[unpackedOffset + 2];
    var c3 = unpacked[unpackedOffset + 3];
    var c4 = unpacked[unpackedOffset + 4];
    var c5 = unpacked[unpackedOffset + 5];
    var c6 = unpacked[unpackedOffset + 6];
    var c7 = unpacked[unpackedOffset + 7];

    packed[packedOffset] = (byte)(c0 | (c1 << 7));
    packed[packedOffset + 1] = (byte)((c1 >> 1) | (c2 << 6));
    packed[packedOffset + 2] = (byte)((c2 >> 2) | (c3 << 5));
    packed[packedOffset + 3] = (byte)((c3 >> 3) | (c4 << 4));
    packed[packedOffset + 4] = (byte)((c4 >> 4) | (c5 << 3));
    packed[packedOffset + 5] = (byte)((c5 >> 5) | (c6 << 2));
    packed[packedOffset + 6] = (byte)((c6 >> 6) | (c7 << 1));
  }

  /// <summary>
  /// Unpacks 7 packed bytes into 8 unpacked ASCII bytes.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _Unpack8(byte[] packed, int packedOffset, byte[] unpacked, int unpackedOffset) {
    // TODO: optimize using simd/pshufb/pext/vector
    var b0 = packed[packedOffset];
    var b1 = packed[packedOffset + 1];
    var b2 = packed[packedOffset + 2];
    var b3 = packed[packedOffset + 3];
    var b4 = packed[packedOffset + 4];
    var b5 = packed[packedOffset + 5];
    var b6 = packed[packedOffset + 6];

    unpacked[unpackedOffset] = (byte)(b0 & 0x7F);
    unpacked[unpackedOffset + 1] = (byte)(((b0 >> 7) | (b1 << 1)) & 0x7F);
    unpacked[unpackedOffset + 2] = (byte)(((b1 >> 6) | (b2 << 2)) & 0x7F);
    unpacked[unpackedOffset + 3] = (byte)(((b2 >> 5) | (b3 << 3)) & 0x7F);
    unpacked[unpackedOffset + 4] = (byte)(((b3 >> 4) | (b4 << 4)) & 0x7F);
    unpacked[unpackedOffset + 5] = (byte)(((b4 >> 3) | (b5 << 5)) & 0x7F);
    unpacked[unpackedOffset + 6] = (byte)(((b5 >> 2) | (b6 << 6)) & 0x7F);
    unpacked[unpackedOffset + 7] = (byte)((b6 >> 1) & 0x7F);
  }

  /// <summary>
  /// Packs an array of unpacked ASCII bytes into 7-bit packed format.
  /// </summary>
  public static byte[] Pack(byte[] unpacked) {
    if (unpacked == null || unpacked.Length == 0)
      return [];

    var packedLength = GetPackedByteCount(unpacked.Length);
    var packed = new byte[packedLength];

    // Process 8 characters at a time (packs into 7 bytes)
    var fullGroups = unpacked.Length >> 3;
    var unpackedIdx = 0;
    var packedIdx = 0;

    for (var g = 0; g < fullGroups; ++g) {
      _Pack8(unpacked, unpackedIdx, packed, packedIdx);
      unpackedIdx += 8;
      packedIdx += 7;
    }

    // Handle remaining characters
    for (var i = unpackedIdx; i < unpacked.Length; ++i)
      SetCharAt(packed, i, unpacked[i]);

    return packed;
  }

  /// <summary>
  /// Packs a span of unpacked ASCII bytes into 7-bit packed format.
  /// </summary>
  public static byte[] Pack(ReadOnlySpan<byte> unpacked) {
    if (unpacked.IsEmpty)
      return [];

    var packedLength = GetPackedByteCount(unpacked.Length);
    var packed = new byte[packedLength];

    // Copy to array for fast processing
    var unpackedArray = unpacked.ToArray();

    // Process 8 characters at a time
    var fullGroups = unpacked.Length >> 3;
    var unpackedIdx = 0;
    var packedIdx = 0;

    for (var g = 0; g < fullGroups; ++g) {
      _Pack8(unpackedArray, unpackedIdx, packed, packedIdx);
      unpackedIdx += 8;
      packedIdx += 7;
    }

    // Handle remaining characters
    for (var i = unpackedIdx; i < unpacked.Length; ++i)
      SetCharAt(packed, i, unpackedArray[i]);

    return packed;
  }

  /// <summary>
  /// Packs ASCII bytes into an existing packed buffer.
  /// </summary>
  public static void PackInto(ReadOnlySpan<byte> unpacked, byte[] packed, int charCount) {
    var count = unpacked.Length < charCount ? unpacked.Length : charCount;

    // Copy to array for fast processing
    var unpackedArray = unpacked.ToArray();

    // Process 8 characters at a time
    var fullGroups = count >> 3;
    var unpackedIdx = 0;
    var packedIdx = 0;

    for (var g = 0; g < fullGroups; ++g) {
      _Pack8(unpackedArray, unpackedIdx, packed, packedIdx);
      unpackedIdx += 8;
      packedIdx += 7;
    }

    // Handle remaining characters
    for (var i = unpackedIdx; i < count; ++i)
      SetCharAt(packed, i, unpackedArray[i]);
  }

  /// <summary>
  /// Unpacks 7-bit packed data into a byte array with one byte per character.
  /// </summary>
  public static byte[] Unpack(byte[] packed, int charCount) {
    if (packed == null || charCount == 0)
      return [];

    var unpacked = new byte[charCount];

    // Process 8 characters at a time (unpacks from 7 bytes)
    var fullGroups = charCount >> 3;
    var unpackedIdx = 0;
    var packedIdx = 0;

    for (var g = 0; g < fullGroups; ++g) {
      _Unpack8(packed, packedIdx, unpacked, unpackedIdx);
      unpackedIdx += 8;
      packedIdx += 7;
    }

    // Handle remaining characters
    for (var i = unpackedIdx; i < charCount; ++i)
      unpacked[i] = GetCharAt(packed, i);

    return unpacked;
  }

  /// <summary>
  /// Unpacks 7-bit packed data into a byte array with one byte per character.
  /// </summary>
  public static byte[] Unpack(ReadOnlySpan<byte> packed, int charCount) {
    if (packed.IsEmpty || charCount == 0)
      return [];

    // Need to copy to array for fast processing
    var packedArray = packed.ToArray();
    var unpacked = new byte[charCount];

    // Process 8 characters at a time
    var fullGroups = charCount >> 3;
    var unpackedIdx = 0;
    var packedIdx = 0;

    for (var g = 0; g < fullGroups; ++g) {
      _Unpack8(packedArray, packedIdx, unpacked, unpackedIdx);
      unpackedIdx += 8;
      packedIdx += 7;
    }

    // Handle remaining characters
    for (var i = unpackedIdx; i < charCount; ++i)
      unpacked[i] = GetCharAt(packedArray, i);

    return unpacked;
  }

  /// <summary>
  /// Checks if all bytes in the array are valid ASCII (0-127).
  /// Uses SIMD operations when available.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsValidAscii(byte[] bytes) {
    if (bytes == null || bytes.Length == 0)
      return true;

    var i = 0;
    var vectorSize = Vector<byte>.Count;

    // Process vectors
    if (bytes.Length >= vectorSize) {
      var end = bytes.Length - vectorSize;
      for (; i <= end; i += vectorSize) {
        var v = new Vector<byte>(bytes, i);
        if (Vector.GreaterThanOrEqualAny(v, _highBitMask))
          return false;
      }
    }

    // Process remaining bytes
    for (; i < bytes.Length; ++i)
      if (bytes[i] > 127)
        return false;

    return true;
  }

  /// <summary>
  /// Checks if all bytes in the span are valid ASCII (0-127).
  /// Uses SIMD operations when available.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsValidAscii(ReadOnlySpan<byte> bytes) {
    if (bytes.IsEmpty)
      return true;

    var i = 0;
    var vectorSize = Vector<byte>.Count;

    // Process vectors
    if (bytes.Length >= vectorSize) {
      var end = bytes.Length - vectorSize;
      for (; i <= end; i += vectorSize) {
        // Check each byte in the slice
        var anyInvalid = false;
        for (var j = 0; j < vectorSize; ++j)
          if (bytes[i + j] > 127) {
            anyInvalid = true;
            break;
          }
        if (anyInvalid)
          return false;
      }
    }

    // Process remaining bytes
    for (; i < bytes.Length; ++i)
      if (bytes[i] > 127)
        return false;

    return true;
  }

  /// <summary>
  /// Finds the index of the first null byte in the array.
  /// Uses SIMD operations when available.
  /// </summary>
  /// <returns>The index of the first null byte, or -1 if not found.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int FindFirstNull(byte[] bytes) {
    if (bytes == null || bytes.Length == 0)
      return -1;

    var i = 0;
    var vectorSize = Vector<byte>.Count;

    // Process vectors
    if (bytes.Length >= vectorSize) {
      var zero = Vector<byte>.Zero;
      var end = bytes.Length - vectorSize;
      for (; i <= end; i += vectorSize) {
        var v = new Vector<byte>(bytes, i);
        if (Vector.EqualsAny(v, zero)) {
          // Found a null in this vector, find exact position
          for (var j = 0; j < vectorSize; ++j)
            if (bytes[i + j] == 0)
              return i + j;
        }
      }
    }

    // Process remaining bytes
    for (; i < bytes.Length; ++i)
      if (bytes[i] == 0)
        return i;

    return -1;
  }

  /// <summary>
  /// Finds the index of the first null byte in the span.
  /// Uses SIMD operations when available.
  /// </summary>
  /// <returns>The index of the first null byte, or -1 if not found.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int FindFirstNull(ReadOnlySpan<byte> bytes) {
    if (bytes.IsEmpty)
      return -1;

    // Simple linear scan for span (can't use Vector constructor directly with span)
    for (var i = 0; i < bytes.Length; ++i)
      if (bytes[i] == 0)
        return i;

    return -1;
  }

  /// <summary>
  /// Copies bytes from source to destination, validating that all are ASCII.
  /// Uses SIMD operations when available.
  /// </summary>
  /// <returns>True if all bytes were valid ASCII and copied; false if a non-ASCII byte was found.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryCopyValidatingAscii(byte[] source, byte[] destination, int length) {
    if (length == 0)
      return true;

    var i = 0;
    var vectorSize = Vector<byte>.Count;

    // Process vectors
    if (length >= vectorSize) {
      var end = length - vectorSize;
      for (; i <= end; i += vectorSize) {
        var v = new Vector<byte>(source, i);
        if (Vector.GreaterThanOrEqualAny(v, _highBitMask))
          return false;
        v.CopyTo(destination, i);
      }
    }

    // Process remaining bytes
    for (; i < length; ++i) {
      if (source[i] > 127)
        return false;
      destination[i] = source[i];
    }

    return true;
  }

}
