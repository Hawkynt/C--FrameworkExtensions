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

#if !SUPPORTS_ARMBASE_MULTIPLYHIGH

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.Arm;

public static class Arm64MultiplyHighPolyfill {

  extension(ArmBase.Arm64) {
    /// <summary>
    /// Multiplies two 64-bit signed integers and returns the high 64 bits of the 128-bit result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long MultiplyHigh(long left, long right) {
      var isNegative = (left < 0) ^ (right < 0);
      var absLeft = (ulong)(left < 0 ? -left : left);
      var absRight = (ulong)(right < 0 ? -right : right);
      var high = MultiplyHigh(absLeft, absRight);

      if (!isNegative)
        return (long)high;

      // For negative results, we need to adjust
      var low = (ulong)left * (ulong)right;
      if (low != 0)
        high = ~high;
      else
        high = ~high + 1;

      return (long)high;
    }

    /// <summary>
    /// Multiplies two 64-bit unsigned integers and returns the high 64 bits of the 128-bit result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MultiplyHigh(ulong left, ulong right) {
      // Split into 32-bit parts
      var leftLow = (uint)left;
      var leftHigh = (uint)(left >> 32);
      var rightLow = (uint)right;
      var rightHigh = (uint)(right >> 32);

      // Compute partial products
      var lowLow = (ulong)leftLow * rightLow;
      var lowHigh = (ulong)leftLow * rightHigh;
      var highLow = (ulong)leftHigh * rightLow;
      var highHigh = (ulong)leftHigh * rightHigh;

      // Add middle terms with proper carry handling
      var carry = (lowLow >> 32) + (uint)lowHigh + (uint)highLow;
      var high = highHigh + (lowHigh >> 32) + (highLow >> 32) + (carry >> 32);

      return high;
    }
  }
}

#endif
