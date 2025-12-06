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

#if !SUPPORTS_VECTOR_64_ONE

using System.Runtime.CompilerServices;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;
using CachedTypeCode = Utilities.CachedTypeCode;

namespace System.Runtime.Intrinsics {

/// <summary>
/// Polyfill for Vector64 One property (added in .NET 8.0).
/// </summary>
public static class Vector64OnePolyfills {

  extension<T>(Vector64<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector64{T}"/> with all elements initialized to one.</summary>
    public static Vector64<T> One {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => TypeCodeCache<T>.Code switch {
        CachedTypeCode.Byte => Unsafe.As<Vector64<byte>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create((byte)1))),
        CachedTypeCode.SByte => Unsafe.As<Vector64<sbyte>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create((sbyte)1))),
        CachedTypeCode.Int16 => Unsafe.As<Vector64<short>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create((short)1))),
        CachedTypeCode.UInt16 => Unsafe.As<Vector64<ushort>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create((ushort)1))),
        CachedTypeCode.Int32 => Unsafe.As<Vector64<int>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1))),
        CachedTypeCode.UInt32 => Unsafe.As<Vector64<uint>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1u))),
        CachedTypeCode.Int64 => Unsafe.As<Vector64<long>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1L))),
        CachedTypeCode.UInt64 => Unsafe.As<Vector64<ulong>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1uL))),
        CachedTypeCode.Single => Unsafe.As<Vector64<float>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1.0f))),
        CachedTypeCode.Double => Unsafe.As<Vector64<double>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1.0))),
        CachedTypeCode.Pointer => Unsafe.As<Vector64<long>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1L))),
        CachedTypeCode.UPointer => Unsafe.As<Vector64<ulong>, Vector64<T>>(ref Unsafe.AsRef(Vector64.Create(1uL))),
        _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
      };
    }
  }

}
}
#endif
