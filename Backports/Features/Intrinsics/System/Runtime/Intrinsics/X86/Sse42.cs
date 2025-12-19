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
//

#if !SUPPORTS_INTRINSICS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of SSE4.2 intrinsics.
/// Provides CRC32, string comparison, and comparison operations.
/// </summary>
public abstract class Sse42 : Sse41 {

  /// <summary>Gets a value indicating whether SSE4.2 instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region Comparison Operations

  /// <summary>Compares packed 64-bit integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> CompareGreaterThan(Vector128<long> left, Vector128<long> right) {
    const long AllBitsSet = unchecked((long)0xFFFFFFFFFFFFFFFF);
    const long NoBitsSet = 0L;
    return Vector128.Create(
      left[0] > right[0] ? AllBitsSet : NoBitsSet,
      left[1] > right[1] ? AllBitsSet : NoBitsSet
    );
  }

  #endregion

  #region CRC32 Operations

  // CRC32C polynomial (iSCSI): 0x1EDC6F41, reflected: 0x82F63B78
  private const uint CRC32C_POLY = 0x82F63B78;

  // Precomputed CRC32C lookup table for byte-at-a-time processing
  private static readonly uint[] Crc32Table = GenerateCrc32Table();

  private static uint[] GenerateCrc32Table() {
    var table = new uint[256];
    for (var i = 0; i < 256; ++i) {
      var crc = (uint)i;
      for (var j = 0; j < 8; ++j)
        crc = (crc >> 1) ^ ((crc & 1) != 0 ? CRC32C_POLY : 0);
      table[i] = crc;
    }
    return table;
  }

  /// <summary>Computes CRC32C checksum with byte input.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Crc32(uint crc, byte data)
    => (crc >> 8) ^ Crc32Table[(byte)(crc ^ data)];

  /// <summary>Computes CRC32C checksum with 16-bit unsigned integer input.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Crc32(uint crc, ushort data) {
    crc = Crc32(crc, (byte)data);
    crc = Crc32(crc, (byte)(data >> 8));
    return crc;
  }

  /// <summary>Computes CRC32C checksum with 32-bit unsigned integer input.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Crc32(uint crc, uint data) {
    crc = Crc32(crc, (byte)data);
    crc = Crc32(crc, (byte)(data >> 8));
    crc = Crc32(crc, (byte)(data >> 16));
    crc = Crc32(crc, (byte)(data >> 24));
    return crc;
  }

  #endregion

  #region String Comparison Operations

  /// <summary>String comparison result type for control byte.</summary>
  private const byte SIDD_UBYTE_OPS = 0x00;
  private const byte SIDD_UWORD_OPS = 0x01;
  private const byte SIDD_SBYTE_OPS = 0x02;
  private const byte SIDD_SWORD_OPS = 0x03;
  private const byte SIDD_CMP_EQUAL_ANY = 0x00;
  private const byte SIDD_CMP_RANGES = 0x04;
  private const byte SIDD_CMP_EQUAL_EACH = 0x08;
  private const byte SIDD_CMP_EQUAL_ORDERED = 0x0C;
  private const byte SIDD_POSITIVE_POLARITY = 0x00;
  private const byte SIDD_NEGATIVE_POLARITY = 0x10;
  private const byte SIDD_MASKED_POSITIVE_POLARITY = 0x20;
  private const byte SIDD_MASKED_NEGATIVE_POLARITY = 0x30;
  private const byte SIDD_LEAST_SIGNIFICANT = 0x00;
  private const byte SIDD_MOST_SIGNIFICANT = 0x40;
  private const byte SIDD_BIT_MASK = 0x00;
  private const byte SIDD_UNIT_MASK = 0x40;

  // Helper to determine element count based on data type
  private static int GetElementCount(byte control) => (control & 0x01) != 0 ? 8 : 16;

  // Helper to get elements from byte vector
  private static int GetByteElement(Vector128<byte> vector, int index) => vector[index];
  private static int GetWordElement(Vector128<ushort> vector, int index) => vector[index];

  /// <summary>
  /// Compares strings with explicit lengths and returns the index of the first match.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int CompareImplicitLengthStrings(Vector128<sbyte> left, Vector128<sbyte> right, byte control) {
    var leftBytes = Vector128.As<sbyte, byte>(left);
    var rightBytes = Vector128.As<sbyte, byte>(right);
    return CompareImplicitLengthStringsCore(leftBytes, rightBytes, control);
  }

  /// <summary>
  /// Compares strings with explicit lengths and returns the index of the first match.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int CompareImplicitLengthStrings(Vector128<byte> left, Vector128<byte> right, byte control)
    => CompareImplicitLengthStringsCore(left, right, control);

  private static int CompareImplicitLengthStringsCore(Vector128<byte> left, Vector128<byte> right, byte control) {
    var elementCount = GetElementCount(control);

    // Find implicit string lengths (null-terminated)
    var leftLen = FindNullTerminator(left, elementCount);
    var rightLen = FindNullTerminator(right, elementCount);

    var result = PerformStringComparison(left, right, leftLen, rightLen, control);

    // Return index based on control flags
    if ((control & SIDD_MOST_SIGNIFICANT) != 0) {
      for (var i = elementCount - 1; i >= 0; --i)
        if ((result & (1 << i)) != 0)
          return i;
    } else {
      for (var i = 0; i < elementCount; ++i)
        if ((result & (1 << i)) != 0)
          return i;
    }
    return elementCount; // No match
  }

  /// <summary>
  /// Compares strings with explicit lengths and returns a mask.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> CompareMask(Vector128<byte> left, Vector128<byte> right, byte control) {
    var elementCount = GetElementCount(control);
    var leftLen = FindNullTerminator(left, elementCount);
    var rightLen = FindNullTerminator(right, elementCount);

    var result = PerformStringComparison(left, right, leftLen, rightLen, control);

    if ((control & SIDD_UNIT_MASK) != 0) {
      // Unit mask - each byte is 0x00 or 0xFF
      var mask = Vector128<byte>.Zero;
      for (var i = 0; i < elementCount; ++i)
        mask = mask.WithElement(i, (result & (1 << i)) != 0 ? (byte)0xFF : (byte)0);
      return mask;
    } else {
      // Bit mask - result in lower bits
      return Vector128.Create((byte)result, (byte)(result >> 8), (byte)0, (byte)0,
        (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
    }
  }

  private static int FindNullTerminator(Vector128<byte> vector, int maxLen) {
    for (var i = 0; i < maxLen; ++i)
      if (vector[i] == 0)
        return i;
    return maxLen;
  }

  private static int PerformStringComparison(Vector128<byte> left, Vector128<byte> right, int leftLen, int rightLen, byte control) {
    var compareMode = control & 0x0C;
    var elementCount = GetElementCount(control);
    var result = 0;

    switch (compareMode) {
      case SIDD_CMP_EQUAL_ANY:
        // Each element in left is compared against all elements in right
        for (var i = 0; i < elementCount; ++i) {
          if (i < rightLen) {
            for (var j = 0; j < leftLen; ++j) {
              if (right[i] == left[j]) {
                result |= 1 << i;
                break;
              }
            }
          }
        }
        break;

      case SIDD_CMP_RANGES:
        // Elements in left define ranges [low, high], check if right elements fall within
        for (var i = 0; i < elementCount; ++i) {
          if (i < rightLen) {
            for (var j = 0; j < leftLen - 1; j += 2) {
              var low = left[j];
              var high = left[j + 1];
              if (right[i] >= low && right[i] <= high) {
                result |= 1 << i;
                break;
              }
            }
          }
        }
        break;

      case SIDD_CMP_EQUAL_EACH:
        // Element-by-element equality comparison
        for (var i = 0; i < elementCount; ++i) {
          if (i < leftLen && i < rightLen && left[i] == right[i])
            result |= 1 << i;
        }
        break;

      case SIDD_CMP_EQUAL_ORDERED:
        // Substring search - find left in right
        for (var i = 0; i <= rightLen - leftLen; ++i) {
          var match = true;
          for (var j = 0; j < leftLen && match; ++j) {
            if (i + j >= rightLen || left[j] != right[i + j])
              match = false;
          }
          if (match)
            result |= 1 << i;
        }
        break;
    }

    // Apply polarity
    var polarity = control & 0x30;
    switch (polarity) {
      case SIDD_NEGATIVE_POLARITY:
        result = ~result & ((1 << elementCount) - 1);
        break;
      case SIDD_MASKED_POSITIVE_POLARITY:
        // Only valid elements
        result &= (1 << Math.Min(leftLen, elementCount)) - 1;
        break;
      case SIDD_MASKED_NEGATIVE_POLARITY:
        result = ~result & ((1 << Math.Min(leftLen, elementCount)) - 1);
        break;
    }

    return result;
  }

  #endregion

  #region X64 Nested Class

  /// <summary>Provides 64-bit specific SSE4.2 operations.</summary>
  public new abstract class X64 : Sse41.X64 {
    /// <summary>Gets a value indicating whether 64-bit SSE4.2 instructions are supported.</summary>
    public new static bool IsSupported => false;

    /// <summary>Computes CRC32C checksum with 64-bit unsigned integer input.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Crc32(ulong crc, ulong data) {
      var crc32 = (uint)crc;
      crc32 = Sse42.Crc32(crc32, (byte)data);
      crc32 = Sse42.Crc32(crc32, (byte)(data >> 8));
      crc32 = Sse42.Crc32(crc32, (byte)(data >> 16));
      crc32 = Sse42.Crc32(crc32, (byte)(data >> 24));
      crc32 = Sse42.Crc32(crc32, (byte)(data >> 32));
      crc32 = Sse42.Crc32(crc32, (byte)(data >> 40));
      crc32 = Sse42.Crc32(crc32, (byte)(data >> 48));
      crc32 = Sse42.Crc32(crc32, (byte)(data >> 56));
      return crc32;
    }
  }

  #endregion
}

#endif
