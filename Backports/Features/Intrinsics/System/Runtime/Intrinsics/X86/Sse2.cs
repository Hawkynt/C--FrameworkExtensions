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
/// Software fallback implementation of SSE2 intrinsics.
/// </summary>
public static class Sse2 {
  public static bool IsSupported => false;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<byte> LoadVector128(byte* source) {
    var result = Vector128.Create(
      source[0], source[1], source[2], source[3],
      source[4], source[5], source[6], source[7],
      source[8], source[9], source[10], source[11],
      source[12], source[13], source[14], source[15]
    );
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<sbyte> LoadVector128(sbyte* source) {
    var result = Vector128.Create(
      source[0], source[1], source[2], source[3],
      source[4], source[5], source[6], source[7],
      source[8], source[9], source[10], source[11],
      source[12], source[13], source[14], source[15]
    );
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<short> LoadVector128(short* source) {
    var result = Vector128.Create(
      source[0], source[1], source[2], source[3],
      source[4], source[5], source[6], source[7]
    );
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ushort> LoadVector128(ushort* source) {
    var result = Vector128.Create(
      source[0], source[1], source[2], source[3],
      source[4], source[5], source[6], source[7]
    );
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<int> LoadVector128(int* source) {
    var result = Vector128.Create(source[0], source[1], source[2], source[3]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<uint> LoadVector128(uint* source) {
    var result = Vector128.Create(source[0], source[1], source[2], source[3]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<long> LoadVector128(long* source) {
    var result = Vector128.Create(source[0], source[1]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ulong> LoadVector128(ulong* source) {
    var result = Vector128.Create(source[0], source[1]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<double> LoadVector128(double* source) {
    var result = Vector128.Create(source[0], source[1]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(byte* destination, Vector128<byte> source) {
    for (var i = 0; i < 16; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(sbyte* destination, Vector128<sbyte> source) {
    for (var i = 0; i < 16; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(short* destination, Vector128<short> source) {
    for (var i = 0; i < 8; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(ushort* destination, Vector128<ushort> source) {
    for (var i = 0; i < 8; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(int* destination, Vector128<int> source) {
    for (var i = 0; i < 4; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(uint* destination, Vector128<uint> source) {
    for (var i = 0; i < 4; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(long* destination, Vector128<long> source) {
    for (var i = 0; i < 2; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(ulong* destination, Vector128<ulong> source) {
    for (var i = 0; i < 2; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(double* destination, Vector128<double> source) {
    for (var i = 0; i < 2; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Xor(Vector128<byte> left, Vector128<byte> right) => Vector128.Xor(left, right);
}

#endif