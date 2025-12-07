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

#if !SUPPORTS_VECTOR_256_ONE

using System.Runtime.CompilerServices;
using Guard;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;
using CachedTypeCode = Utilities.CachedTypeCode;

namespace System.Runtime.Intrinsics {

/// <summary>
/// Polyfill for Vector256 One property (added in .NET 8.0).
/// </summary>
public static class Vector256OnePolyfills {

  extension<T>(Vector256<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector256{T}"/> with all elements initialized to one.</summary>
    public static Vector256<T> One {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        switch (TypeCodeCache<T>.Code) {
          case CachedTypeCode.Byte: {
            var temp = Vector256.Create((byte)1);
            return Unsafe.As<Vector256<byte>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.SByte: {
            var temp = Vector256.Create((sbyte)1);
            return Unsafe.As<Vector256<sbyte>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.Int16: {
            var temp = Vector256.Create((short)1);
            return Unsafe.As<Vector256<short>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.UInt16: {
            var temp = Vector256.Create((ushort)1);
            return Unsafe.As<Vector256<ushort>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.Int32: {
            var temp = Vector256.Create(1);
            return Unsafe.As<Vector256<int>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.UInt32: {
            var temp = Vector256.Create(1u);
            return Unsafe.As<Vector256<uint>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.Int64: {
            var temp = Vector256.Create(1L);
            return Unsafe.As<Vector256<long>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.UInt64: {
            var temp = Vector256.Create(1uL);
            return Unsafe.As<Vector256<ulong>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.Single: {
            var temp = Vector256.Create(1.0f);
            return Unsafe.As<Vector256<float>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.Double: {
            var temp = Vector256.Create(1.0);
            return Unsafe.As<Vector256<double>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.Pointer: {
            var temp = Vector256.Create(1L);
            return Unsafe.As<Vector256<long>, Vector256<T>>(ref temp);
          }
          case CachedTypeCode.UPointer: {
            var temp = Vector256.Create(1uL);
            return Unsafe.As<Vector256<ulong>, Vector256<T>>(ref temp);
          }
          default:
            return AlwaysThrow.NotSupportedException<Vector256<T>>($"Type {typeof(T)} is not supported");
        }
      }
    }
  }

}
}
#endif
