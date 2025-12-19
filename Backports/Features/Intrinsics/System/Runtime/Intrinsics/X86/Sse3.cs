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
/// Software fallback implementation of SSE3 (Streaming SIMD Extensions 3) intrinsics.
/// Provides horizontal operations and move/duplicate operations.
/// </summary>
public abstract class Sse3 : Sse2 {

  /// <summary>Gets a value indicating whether SSE3 instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region Horizontal Add Operations

  /// <summary>
  /// Horizontally adds adjacent pairs of single-precision floating-point values.
  /// result[0] = left[0] + left[1]
  /// result[1] = left[2] + left[3]
  /// result[2] = right[0] + right[1]
  /// result[3] = right[2] + right[3]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> HorizontalAdd(Vector128<float> left, Vector128<float> right)
    => Vector128.Create(
      left[0] + left[1],
      left[2] + left[3],
      right[0] + right[1],
      right[2] + right[3]
    );

  /// <summary>
  /// Horizontally adds adjacent pairs of double-precision floating-point values.
  /// result[0] = left[0] + left[1]
  /// result[1] = right[0] + right[1]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> HorizontalAdd(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(
      left[0] + left[1],
      right[0] + right[1]
    );

  #endregion

  #region Horizontal Subtract Operations

  /// <summary>
  /// Horizontally subtracts adjacent pairs of single-precision floating-point values.
  /// result[0] = left[0] - left[1]
  /// result[1] = left[2] - left[3]
  /// result[2] = right[0] - right[1]
  /// result[3] = right[2] - right[3]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> HorizontalSubtract(Vector128<float> left, Vector128<float> right)
    => Vector128.Create(
      left[0] - left[1],
      left[2] - left[3],
      right[0] - right[1],
      right[2] - right[3]
    );

  /// <summary>
  /// Horizontally subtracts adjacent pairs of double-precision floating-point values.
  /// result[0] = left[0] - left[1]
  /// result[1] = right[0] - right[1]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> HorizontalSubtract(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(
      left[0] - left[1],
      right[0] - right[1]
    );

  #endregion

  #region AddSubtract Operations

  /// <summary>
  /// Performs alternating subtract and add on single-precision floating-point values.
  /// result[0] = left[0] - right[0]
  /// result[1] = left[1] + right[1]
  /// result[2] = left[2] - right[2]
  /// result[3] = left[3] + right[3]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> AddSubtract(Vector128<float> left, Vector128<float> right)
    => Vector128.Create(
      left[0] - right[0],
      left[1] + right[1],
      left[2] - right[2],
      left[3] + right[3]
    );

  /// <summary>
  /// Performs alternating subtract and add on double-precision floating-point values.
  /// result[0] = left[0] - right[0]
  /// result[1] = left[1] + right[1]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> AddSubtract(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(
      left[0] - right[0],
      left[1] + right[1]
    );

  #endregion

  #region Move and Duplicate Operations

  /// <summary>
  /// Moves and duplicates odd-indexed single-precision floating-point values.
  /// result[0] = value[1]
  /// result[1] = value[1]
  /// result[2] = value[3]
  /// result[3] = value[3]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MoveHighAndDuplicate(Vector128<float> value)
    => Vector128.Create(value[1], value[1], value[3], value[3]);

  /// <summary>
  /// Moves and duplicates even-indexed single-precision floating-point values.
  /// result[0] = value[0]
  /// result[1] = value[0]
  /// result[2] = value[2]
  /// result[3] = value[2]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MoveLowAndDuplicate(Vector128<float> value)
    => Vector128.Create(value[0], value[0], value[2], value[2]);

  /// <summary>
  /// Duplicates the lower double-precision floating-point value.
  /// result[0] = value[0]
  /// result[1] = value[0]
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MoveAndDuplicate(Vector128<double> value)
    => Vector128.Create(value[0], value[0]);

  #endregion

  #region Load Operations

  /// <summary>
  /// Loads a double-precision floating-point value and duplicates it into both elements.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<double> LoadAndDuplicateToVector128(double* address)
    => Vector128.Create(*address, *address);

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<sbyte> LoadDquVector128(sbyte* address) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<byte> LoadDquVector128(byte* address) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<short> LoadDquVector128(short* address) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ushort> LoadDquVector128(ushort* address) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<int> LoadDquVector128(int* address) {
    var result = Vector128<int>.Zero;
    for (var i = 0; i < Vector128<int>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<uint> LoadDquVector128(uint* address) {
    var result = Vector128<uint>.Zero;
    for (var i = 0; i < Vector128<uint>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<long> LoadDquVector128(long* address) {
    var result = Vector128<long>.Zero;
    for (var i = 0; i < Vector128<long>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>
  /// Loads 128 bits of integer data (unaligned).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ulong> LoadDquVector128(ulong* address) {
    var result = Vector128<ulong>.Zero;
    for (var i = 0; i < Vector128<ulong>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  #endregion

  /// <summary>Provides 64-bit specific SSE3 operations.</summary>
  public new abstract class X64 : Sse2.X64 {

    /// <summary>Gets a value indicating whether 64-bit SSE3 instructions are supported.</summary>
    public new static bool IsSupported => false;
  }
}

#endif
