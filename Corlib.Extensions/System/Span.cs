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

using Guard;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class SpanExtensions {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotEmpty<T>(this Span<T> @this) => !@this.IsEmpty;

  /// <summary>
  /// Performs an in-place bitwise NOT operation on the span.
  /// </summary>
  /// <param name="this">The span to complement.</param>
  public static unsafe void Not(this Span<byte> @this) {
    if (@this.IsEmpty)
      return;

    fixed (byte* targetPin = @this)
      _UnmanagedNot(targetPin, (uint)@this.Length);
  }

  /// <summary>
  /// Performs a bitwise NOT operation, storing the result in the target span.
  /// </summary>
  /// <param name="this">The source span.</param>
  /// <param name="target">The target span to store results.</param>
  public static unsafe void Not(this ReadOnlySpan<byte> @this, Span<byte> target) {
    Against.False(@this.Length == target.Length);

    if (@this.IsEmpty)
      return;

    fixed (byte* sourcePin = @this)
    fixed (byte* targetPin = target)
      _UnmanagedNot(targetPin, sourcePin, (uint)@this.Length);
  }

  private static unsafe void _UnmanagedNot(byte* targetPointer, uint count) {
    if (Vector512.IsHardwareAccelerated)
      while (count >= 64) {
        var s = *(Vector512<byte>*)targetPointer;
        *(Vector512<byte>*)targetPointer = Vector512.OnesComplement(s);
        targetPointer += 64;
        count -= 64;
      }

    if (Vector256.IsHardwareAccelerated)
      while (count >= 32) {
        var s = *(Vector256<byte>*)targetPointer;
        *(Vector256<byte>*)targetPointer = Vector256.OnesComplement(s);
        targetPointer += 32;
        count -= 32;
      }

    if (Vector128.IsHardwareAccelerated)
      while (count >= 16) {
        var s = *(Vector128<byte>*)targetPointer;
        *(Vector128<byte>*)targetPointer = Vector128.OnesComplement(s);
        targetPointer += 16;
        count -= 16;
      }

    _UnmanagedNotScalar(targetPointer, count);
  }

  private static unsafe void _UnmanagedNot(byte* targetPointer, byte* sourcePointer, uint count) {
    if (Vector512.IsHardwareAccelerated)
      while (count >= 64) {
        var s = *(Vector512<byte>*)sourcePointer;
        *(Vector512<byte>*)targetPointer = Vector512.OnesComplement(s);
        sourcePointer += 64;
        targetPointer += 64;
        count -= 64;
      }

    if (Vector256.IsHardwareAccelerated)
      while (count >= 32) {
        var s = *(Vector256<byte>*)sourcePointer;
        *(Vector256<byte>*)targetPointer = Vector256.OnesComplement(s);
        sourcePointer += 32;
        targetPointer += 32;
        count -= 32;
      }

    if (Vector128.IsHardwareAccelerated)
      while (count >= 16) {
        var s = *(Vector128<byte>*)sourcePointer;
        *(Vector128<byte>*)targetPointer = Vector128.OnesComplement(s);
        sourcePointer += 16;
        targetPointer += 16;
        count -= 16;
      }

    _UnmanagedNotScalar(targetPointer, sourcePointer, count);
  }

  private static unsafe void _UnmanagedNotScalar(byte* targetPointer, uint count) {
    // Process using 64-bit operations on 64-bit platforms
    if (Utilities.Runtime.Is64BitArchitecture)
      while (count >= 8) {
        var s = (ulong*)targetPointer;
        *s = ~*s;
        count -= 8;
        targetPointer += 8;
      }

    // Process using 32-bit operations
    while (count >= 4) {
      var s = (uint*)targetPointer;
      *s = ~*s;
      count -= 4;
      targetPointer += 4;
    }

    // Process using 16-bit operations
    while (count >= 2) {
      var s = (ushort*)targetPointer;
      *s = (ushort)~*s;
      count -= 2;
      targetPointer += 2;
    }

    // Process remaining byte
    if (count > 0)
      *targetPointer = (byte)~*targetPointer;
  }

  private static unsafe void _UnmanagedNotScalar(byte* targetPointer, byte* sourcePointer, uint count) {
    // Process using 64-bit operations on 64-bit platforms
    if (Utilities.Runtime.Is64BitArchitecture)
      while (count >= 8) {
        var s = (ulong*)sourcePointer;
        *(ulong*)targetPointer = ~*s;
        count -= 8;
        sourcePointer += 8;
        targetPointer += 8;
      }

    // Process using 32-bit operations
    while (count >= 4) {
      var s = (uint*)sourcePointer;
      *(uint*)targetPointer = ~*s;
      count -= 4;
      sourcePointer += 4;
      targetPointer += 4;
    }

    // Process using 16-bit operations
    while (count >= 2) {
      var s = (ushort*)sourcePointer;
      *(ushort*)targetPointer = (ushort)~*s;
      count -= 2;
      sourcePointer += 2;
      targetPointer += 2;
    }

    // Process remaining byte
    if (count > 0)
      *targetPointer = (byte)~*sourcePointer;
  }

}
