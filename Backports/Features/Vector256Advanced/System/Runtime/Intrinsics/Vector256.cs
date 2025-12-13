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

#if !SUPPORTS_VECTOR_256_ADVANCED

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

public static partial class Vector256AdvancedPolyfills {
  extension(Vector256) {

    /// <summary>Clamps a vector to be within the specified minimum and maximum values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Clamp<T>(Vector256<T> value, Vector256<T> min, Vector256<T> max) where T : struct
      => Vector256.Max(Vector256.Min(value, max), min);

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Round(Vector256<float> vector) {
      unsafe {
        float* ptr = stackalloc float[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = MathF.Round(Vector256.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector256<float>>(ptr);
      }
    }

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Round(Vector256<double> vector) {
      unsafe {
        double* ptr = stackalloc double[4];
        for (var i = 0; i < 4; ++i)
          ptr[i] = Math.Round(Vector256.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector256<double>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Truncate(Vector256<float> vector) {
      unsafe {
        float* ptr = stackalloc float[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = MathF.Truncate(Vector256.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector256<float>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Truncate(Vector256<double> vector) {
      unsafe {
        double* ptr = stackalloc double[4];
        for (var i = 0; i < 4; ++i)
          ptr[i] = Math.Truncate(Vector256.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector256<double>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> FusedMultiplyAdd(Vector256<float> a, Vector256<float> b, Vector256<float> c) {
      unsafe {
        float* ptr = stackalloc float[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = Vector256.GetElement(a, i) * Vector256.GetElement(b, i) + Vector256.GetElement(c, i);
        return Unsafe.ReadUnaligned<Vector256<float>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> FusedMultiplyAdd(Vector256<double> a, Vector256<double> b, Vector256<double> c) {
      unsafe {
        double* ptr = stackalloc double[4];
        for (var i = 0; i < 4; ++i)
          ptr[i] = Vector256.GetElement(a, i) * Vector256.GetElement(b, i) + Vector256.GetElement(c, i);
        return Unsafe.ReadUnaligned<Vector256<double>>(ptr);
      }
    }

  }
}

#endif
