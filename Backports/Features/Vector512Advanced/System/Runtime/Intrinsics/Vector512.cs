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

#if !SUPPORTS_VECTOR_512_ADVANCED

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

public static partial class Vector512AdvancedPolyfills {
  extension(Vector512) {

    /// <summary>Clamps a vector to be within the specified minimum and maximum values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> Clamp<T>(Vector512<T> value, Vector512<T> min, Vector512<T> max) where T : struct
      => Vector512.Max(Vector512.Min(value, max), min);

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Round(Vector512<float> vector) {
      unsafe {
        float* ptr = stackalloc float[16];
        for (var i = 0; i < 16; ++i)
          ptr[i] = MathF.Round(Vector512.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector512<float>>(ptr);
      }
    }

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Round(Vector512<double> vector) {
      unsafe {
        double* ptr = stackalloc double[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = Math.Round(Vector512.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector512<double>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Truncate(Vector512<float> vector) {
      unsafe {
        float* ptr = stackalloc float[16];
        for (var i = 0; i < 16; ++i)
          ptr[i] = MathF.Truncate(Vector512.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector512<float>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Truncate(Vector512<double> vector) {
      unsafe {
        double* ptr = stackalloc double[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = Math.Truncate(Vector512.GetElement(vector, i));
        return Unsafe.ReadUnaligned<Vector512<double>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> FusedMultiplyAdd(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
      unsafe {
        float* ptr = stackalloc float[16];
        for (var i = 0; i < 16; ++i)
          ptr[i] = Vector512.GetElement(a, i) * Vector512.GetElement(b, i) + Vector512.GetElement(c, i);
        return Unsafe.ReadUnaligned<Vector512<float>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> FusedMultiplyAdd(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
      unsafe {
        double* ptr = stackalloc double[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = Vector512.GetElement(a, i) * Vector512.GetElement(b, i) + Vector512.GetElement(c, i);
        return Unsafe.ReadUnaligned<Vector512<double>>(ptr);
      }
    }

  }
}

#endif
