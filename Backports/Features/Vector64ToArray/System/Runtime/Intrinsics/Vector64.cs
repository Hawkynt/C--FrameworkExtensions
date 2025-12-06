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

#if !SUPPORTS_VECTOR_64_TOARRAY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

/// <summary>
/// Polyfill for Vector64 ToArray method (added in .NET 9.0).
/// </summary>
public static class Vector64ToArrayPolyfills {

  /// <summary>Copies a <see cref="Vector64{T}"/> to a given array.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] ToArray<T>(this Vector64<T> vector) where T : struct {
    var result = new T[Vector64<T>.Count];
    for (var i = 0; i < Vector64<T>.Count; ++i)
      result[i] = vector.GetElement(i);
    return result;
  }

}
#endif
