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

#if !SUPPORTS_VECTOR_128_ONE

using System.Runtime.CompilerServices;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;
using CachedTypeCode = Utilities.CachedTypeCode;

namespace System.Runtime.Intrinsics {

/// <summary>
/// Polyfill for Vector128 One property (added in .NET 8.0).
/// </summary>
public static class Vector128OnePolyfills {

  extension<T>(Vector128<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector128{T}"/> with all elements initialized to one.</summary>
    public static Vector128<T> One {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => TypeCodeCache<T>.Code switch {
        CachedTypeCode.Byte => Unsafe.As<Vector128<byte>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create((byte)1))),
        CachedTypeCode.SByte => Unsafe.As<Vector128<sbyte>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create((sbyte)1))),
        CachedTypeCode.Int16 => Unsafe.As<Vector128<short>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create((short)1))),
        CachedTypeCode.UInt16 => Unsafe.As<Vector128<ushort>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create((ushort)1))),
        CachedTypeCode.Int32 => Unsafe.As<Vector128<int>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1))),
        CachedTypeCode.UInt32 => Unsafe.As<Vector128<uint>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1u))),
        CachedTypeCode.Int64 => Unsafe.As<Vector128<long>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1L))),
        CachedTypeCode.UInt64 => Unsafe.As<Vector128<ulong>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1uL))),
        CachedTypeCode.Single => Unsafe.As<Vector128<float>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1.0f))),
        CachedTypeCode.Double => Unsafe.As<Vector128<double>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1.0))),
        CachedTypeCode.Pointer => Unsafe.As<Vector128<long>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1L))),
        CachedTypeCode.UPointer => Unsafe.As<Vector128<ulong>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create(1uL))),
        _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
      };
    }
  }

}
}
#endif
