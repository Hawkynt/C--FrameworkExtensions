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

#if !SUPPORTS_VECTOR_256_ALLBITSSET

using System.Runtime.CompilerServices;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics {

/// <summary>
/// Polyfill for Vector256 AllBitsSet property (added in .NET 5.0).
/// </summary>
public static class Vector256AllBitsSetPolyfills {

  extension<T>(Vector256<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector256{T}"/> with all bits set to 1.</summary>
    public static Vector256<T> AllBitsSet {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Unsafe.As<Vector256<byte>, Vector256<T>>(ref Unsafe.AsRef(Vector256.Create(byte.MaxValue)));
    }
  }

}
}
#endif
