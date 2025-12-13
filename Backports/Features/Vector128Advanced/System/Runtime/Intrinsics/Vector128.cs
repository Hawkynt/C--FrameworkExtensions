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

#if !SUPPORTS_VECTOR_128_ADVANCED

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

public static partial class Vector128AdvancedPolyfills {
  extension(Vector128) {

    /// <summary>Clamps a vector to be within the specified minimum and maximum values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Clamp<T>(Vector128<T> value, Vector128<T> min, Vector128<T> max) where T : struct
      => Vector128.Max(Vector128.Min(value, max), min);

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Round(Vector128<float> vector) {
      unsafe {
        float* ptr = stackalloc float[4];
        for (var i = 0; i < 4; ++i)
          ptr[i] = MathF.Round(Vector128.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector128<float>>(ptr);
      }
    }

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Round(Vector128<double> vector) {
      unsafe {
        double* ptr = stackalloc double[2];
        for (var i = 0; i < 2; ++i)
          ptr[i] = Math.Round(Vector128.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector128<double>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Truncate(Vector128<float> vector) {
      unsafe {
        float* ptr = stackalloc float[4];
        for (var i = 0; i < 4; ++i)
          ptr[i] = MathF.Truncate(Vector128.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector128<float>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Truncate(Vector128<double> vector) {
      unsafe {
        double* ptr = stackalloc double[2];
        for (var i = 0; i < 2; ++i)
          ptr[i] = Math.Truncate(Vector128.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector128<double>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> FusedMultiplyAdd(Vector128<float> a, Vector128<float> b, Vector128<float> c) {
      unsafe {
        float* ptr = stackalloc float[4];
        for (var i = 0; i < 4; ++i)
          ptr[i] = Vector128.GetElement(a, i) * Vector128.GetElement(b, i) + Vector128.GetElement(c, i);
        return Unsafe.ReadUnaligned<Vector128<float>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> FusedMultiplyAdd(Vector128<double> a, Vector128<double> b, Vector128<double> c) {
      unsafe {
        double* ptr = stackalloc double[2];
        for (var i = 0; i < 2; ++i)
          ptr[i] = Vector128.GetElement(a, i) * Vector128.GetElement(b, i) + Vector128.GetElement(c, i);
        return Unsafe.ReadUnaligned<Vector128<double>>(ptr);
      }
    }

  }
}

#endif
