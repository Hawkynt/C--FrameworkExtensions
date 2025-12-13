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

#if !SUPPORTS_VECTOR_64_ADVANCED

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics {

public static partial class Vector64Polyfills {
  extension(Vector64) {
    /// <summary>
    /// Restricts a vector between a minimum and a maximum value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Clamp<T>(Vector64<T> value, Vector64<T> min, Vector64<T> max) where T : struct
      => Vector64.Max(Vector64.Min(value, max), min);

    /// <summary>
    /// Rounds each element of a vector to the nearest integral value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector64<float> Round(Vector64<float> vector) {
      const int count = 8 / sizeof(float); // 2 floats in 8 bytes
      Span<float> buffer = stackalloc float[count];
      Unsafe.WriteUnaligned(ref Unsafe.As<float, byte>(ref buffer[0]), vector);

      for (var i = 0; i < count; ++i)
        buffer[i] = MathF.Round(buffer[i]);

      return Unsafe.ReadUnaligned<Vector64<float>>(ref Unsafe.As<float, byte>(ref buffer[0]));
    }

    /// <summary>
    /// Truncates each element of a vector to the integral part.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector64<float> Truncate(Vector64<float> vector) {
      const int count = 8 / sizeof(float); // 2 floats in 8 bytes
      Span<float> buffer = stackalloc float[count];
      Unsafe.WriteUnaligned(ref Unsafe.As<float, byte>(ref buffer[0]), vector);

      for (var i = 0; i < count; ++i)
        buffer[i] = MathF.Truncate(buffer[i]);

      return Unsafe.ReadUnaligned<Vector64<float>>(ref Unsafe.As<float, byte>(ref buffer[0]));
    }

    /// <summary>
    /// Computes (a * b) + c using fused multiply-add for each element.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector64<float> FusedMultiplyAdd(Vector64<float> a, Vector64<float> b, Vector64<float> c) {
      const int count = 8 / sizeof(float); // 2 floats in 8 bytes
      Span<float> bufferA = stackalloc float[count];
      Span<float> bufferB = stackalloc float[count];
      Span<float> bufferC = stackalloc float[count];
      Span<float> result = stackalloc float[count];

      Unsafe.WriteUnaligned(ref Unsafe.As<float, byte>(ref bufferA[0]), a);
      Unsafe.WriteUnaligned(ref Unsafe.As<float, byte>(ref bufferB[0]), b);
      Unsafe.WriteUnaligned(ref Unsafe.As<float, byte>(ref bufferC[0]), c);

      for (var i = 0; i < count; ++i)
        result[i] = bufferA[i] * bufferB[i] + bufferC[i];

      return Unsafe.ReadUnaligned<Vector64<float>>(ref Unsafe.As<float, byte>(ref result[0]));
    }
  }
}

}
#endif
