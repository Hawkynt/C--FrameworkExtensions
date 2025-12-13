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

#if SUPPORTS_VECTOR_256_BASE && !SUPPORTS_VECTOR_256_ONE

using System.Runtime.CompilerServices;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics {

public static partial class Vector256Polyfills {
  extension<T>(Vector256<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector256{T}"/> with all elements initialized to one.</summary>
    public static Vector256<T> One {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var one = Scalar<T>.One;
        var count = 32 / Unsafe.SizeOf<T>();
        unsafe {
          var buffer = stackalloc byte[32];
          var ptr = (T*)buffer;
          for (var i = 0; i < count; ++i)
            ptr[i] = one;
          return Unsafe.ReadUnaligned<Vector256<T>>(buffer);
        }
      }
    }
  }
}

}
#endif
