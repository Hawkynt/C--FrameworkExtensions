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
using Guard;
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
      get {
        switch (TypeCodeCache<T>.Code) {
          case CachedTypeCode.Byte: {
            var temp = Vector128.Create((byte)1);
            return Unsafe.As<Vector128<byte>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.SByte: {
            var temp = Vector128.Create((sbyte)1);
            return Unsafe.As<Vector128<sbyte>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.Int16: {
            var temp = Vector128.Create((short)1);
            return Unsafe.As<Vector128<short>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.UInt16: {
            var temp = Vector128.Create((ushort)1);
            return Unsafe.As<Vector128<ushort>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.Int32: {
            var temp = Vector128.Create(1);
            return Unsafe.As<Vector128<int>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.UInt32: {
            var temp = Vector128.Create(1u);
            return Unsafe.As<Vector128<uint>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.Int64: {
            var temp = Vector128.Create(1L);
            return Unsafe.As<Vector128<long>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.UInt64: {
            var temp = Vector128.Create(1uL);
            return Unsafe.As<Vector128<ulong>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.Single: {
            var temp = Vector128.Create(1.0f);
            return Unsafe.As<Vector128<float>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.Double: {
            var temp = Vector128.Create(1.0);
            return Unsafe.As<Vector128<double>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.Pointer: {
            var temp = Vector128.Create(1L);
            return Unsafe.As<Vector128<long>, Vector128<T>>(ref temp);
          }
          case CachedTypeCode.UPointer: {
            var temp = Vector128.Create(1uL);
            return Unsafe.As<Vector128<ulong>, Vector128<T>>(ref temp);
          }
          default:
            return AlwaysThrow.NotSupportedException<Vector128<T>>($"Type {typeof(T)} is not supported");
        }
      }
    }
  }

}
}
#endif
