#region (c)2010-2042 Hawkynt

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

#endregion

#if SUPPORTS_VECTOR_512 && !SUPPORTS_VECTOR_512_BASE

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

/// <summary>
/// Provides a set of static methods for creating and working with 512-bit vectors.
/// </summary>
public static partial class Vector512 {
  internal const int Size = 64;
  internal const int Alignment = 64;

  private static void SkipInit<T>(out T result) => result = default;

  /// <summary>Gets a value that indicates whether 512-bit vector operations are subject to hardware acceleration.</summary>
  public static bool IsHardwareAccelerated => false;

  /// <summary>Computes the absolute value of each element in a vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Abs<T>(Vector512<T> vector) where T : struct {
    if (Scalar<T>.IsUnsigned)
      return vector;

    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var value = Scalar<T>.Abs(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  /// <summary>Adds two vectors to compute their sum.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Add<T>(Vector512<T> left, Vector512<T> right) where T : struct => left + right;

  /// <summary>Computes the bitwise-and of a given vector and the ones complement of another vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> AndNot<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    Vector512<T>.ThrowIfNotSupported();
    return new(left._v0 & ~right._v0, left._v1 & ~right._v1, left._v2 & ~right._v2, left._v3 & ~right._v3,
               left._v4 & ~right._v4, left._v5 & ~right._v5, left._v6 & ~right._v6, left._v7 & ~right._v7);
  }

  /// <summary>Reinterprets a vector as a new vector type.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<TTo> As<TFrom, TTo>(this Vector512<TFrom> vector) where TFrom : struct where TTo : struct {
    Vector512<TFrom>.ThrowIfNotSupported();
    Vector512<TTo>.ThrowIfNotSupported();
    return Unsafe.As<Vector512<TFrom>, Vector512<TTo>>(ref vector);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<byte> AsByte<T>(this Vector512<T> vector) where T : struct => vector.As<T, byte>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> AsDouble<T>(this Vector512<T> vector) where T : struct => vector.As<T, double>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<short> AsInt16<T>(this Vector512<T> vector) where T : struct => vector.As<T, short>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> AsInt32<T>(this Vector512<T> vector) where T : struct => vector.As<T, int>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> AsInt64<T>(this Vector512<T> vector) where T : struct => vector.As<T, long>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<nint> AsNInt<T>(this Vector512<T> vector) where T : struct => vector.As<T, nint>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<nuint> AsNUInt<T>(this Vector512<T> vector) where T : struct => vector.As<T, nuint>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<sbyte> AsSByte<T>(this Vector512<T> vector) where T : struct => vector.As<T, sbyte>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> AsSingle<T>(this Vector512<T> vector) where T : struct => vector.As<T, float>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ushort> AsUInt16<T>(this Vector512<T> vector) where T : struct => vector.As<T, ushort>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> AsUInt32<T>(this Vector512<T> vector) where T : struct => vector.As<T, uint>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> AsUInt64<T>(this Vector512<T> vector) where T : struct => vector.As<T, ulong>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> BitwiseAnd<T>(Vector512<T> left, Vector512<T> right) where T : struct => left & right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> BitwiseOr<T>(Vector512<T> left, Vector512<T> right) where T : struct => left | right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Ceiling<T>(Vector512<T> vector) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var value = Scalar<T>.Ceiling(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Create<T>(T value) where T : struct => new(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Create<T>(Vector256<T> lower, Vector256<T> upper) where T : struct {
    // Use Unsafe to reinterpret the Vector256 as raw bytes, then construct Vector512
    var lowerBytes = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref lower);
    var upperBytes = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref upper);
    return new(lowerBytes.Item1, lowerBytes.Item2, lowerBytes.Item3, lowerBytes.Item4,
               upperBytes.Item1, upperBytes.Item2, upperBytes.Item3, upperBytes.Item4);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Divide<T>(Vector512<T> left, Vector512<T> right) where T : struct => left / right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Dot<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    var result = Scalar<T>.Zero();
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var product = Scalar<T>.Multiply(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result = Scalar<T>.Add(result, product);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Equals<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var equals = Scalar<T>.ObjectEquals(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, equals ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Floor<T>(Vector512<T> vector) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var value = Scalar<T>.Floor(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T GetElement<T>(this Vector512<T> vector, int index) where T : struct => vector[index];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static T GetElementUnsafe<T>(in this Vector512<T> vector, int index) where T : struct {
    ref var address = ref Unsafe.As<Vector512<T>, T>(ref Unsafe.AsRef(in vector));
    return Unsafe.Add(ref address, index);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void SetElementUnsafe<T>(in this Vector512<T> vector, int index, T value) where T : struct {
    ref var address = ref Unsafe.As<Vector512<T>, T>(ref Unsafe.AsRef(in vector));
    Unsafe.Add(ref address, index) = value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> GreaterThan<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var greater = Scalar<T>.GreaterThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, greater ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> LessThan<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var less = Scalar<T>.LessThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, less ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Max<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var value = Scalar<T>.Max(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Min<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var value = Scalar<T>.Min(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Multiply<T>(Vector512<T> left, Vector512<T> right) where T : struct => left * right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Negate<T>(Vector512<T> vector) where T : struct => -vector;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> OnesComplement<T>(Vector512<T> vector) where T : struct => ~vector;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Sqrt<T>(Vector512<T> vector) where T : struct {
    SkipInit(out Vector512<T> result);
    for (var index = 0; index < Vector512<T>.Count; ++index) {
      var value = Scalar<T>.Sqrt(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Subtract<T>(Vector512<T> left, Vector512<T> right) where T : struct => left - right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Sum<T>(Vector512<T> vector) where T : struct {
    var result = Scalar<T>.Zero();
    for (var index = 0; index < Vector512<T>.Count; ++index)
      result = Scalar<T>.Add(result, vector.GetElementUnsafe(index));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ToScalar<T>(this Vector512<T> vector) where T : struct => vector[0];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> WithElement<T>(this Vector512<T> vector, int index, T value) where T : struct {
    if ((uint)index >= Vector512<T>.Count)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

    var result = vector;
    result.SetElementUnsafe(index, value);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Xor<T>(Vector512<T> left, Vector512<T> right) where T : struct => left ^ right;
}

#endif
