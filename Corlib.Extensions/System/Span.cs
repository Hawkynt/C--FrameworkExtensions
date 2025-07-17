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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class SpanExtensions {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotEmpty<T>(this Span<T> @this) => !@this.IsEmpty;

  public static unsafe void Not(this Span<byte> @this) {
    if (@this.IsEmpty)
      return;

    fixed (byte* targetPin = @this) {
      var targetPointer = targetPin;
      
      var count = @this.Length;
      while (count >= 8) {
        var s = (ulong*)targetPointer;
        *s = ~*s;
        count -= 8;
        targetPointer += 8;
      }

      while (count >= 4) {
        var s = (uint*)targetPointer;
        *s = ~*s;
        count -= 4;
        targetPointer += 4;
      }

      while (count >= 2) {
        var s = (ushort*)targetPointer;
        *s = (ushort)~*s;
        count -= 2;
        targetPointer += 2;
      }

      while (count > 0) {
        var s = targetPointer;
        *s = (byte)~*s;
        --count;
        ++targetPointer;
      }
    }
  }

  public static unsafe void Not(this ReadOnlySpan<byte> @this, Span<byte> target) {
    Against.False(@this.Length == target.Length);

    if (@this.IsEmpty)
      return;

    fixed (byte* sourcePin = @this)
    fixed (byte* targetPin = target) {
      var sourcePointer = sourcePin;
      var targetPointer = targetPin;

      var count = @this.Length;
      while (count >= 8) {
        var s = (ulong*)sourcePointer;
        *(ulong*)targetPointer = ~*s;
        count -= 8;
        sourcePointer += 8;
        targetPointer += 8;
      }

      while (count >= 4) {
        var s = (uint*)sourcePointer;
        *(uint*)targetPointer = ~*s;
        count -= 4;
        sourcePointer += 4;
        targetPointer += 4;
      }

      while (count >= 2) {
        var s = (ushort*)sourcePointer;
        *(ushort*)targetPointer = (ushort)~*s;
        count -= 2;
        sourcePointer += 2;
        targetPointer += 2;
      }

      while (count > 0) {
        var s = sourcePointer;
        *targetPointer = (byte)~*s;
        --count;
        ++sourcePointer;
        ++targetPointer;
      }
    }
  }

}