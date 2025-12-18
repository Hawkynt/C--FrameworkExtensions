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

// ArmBase.Yield (added in .NET 7.0)
// On older frameworks without FEATURE_ARMBASE_WAVE1, the full polyfill includes Yield.
// On net5.0-net6.0 (WAVE1 defined but not YIELD), this extension provides Yield.

#if !FEATURE_ARMBASE_YIELD

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.Arm;

public static class ArmBaseYieldPolyfill {

  extension(ArmBase) {
    /// <summary>
    /// Provides a hint that the current thread should yield.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Yield() {
      // Software fallback - no-op on non-ARM platforms
    }
  }
}

#endif
