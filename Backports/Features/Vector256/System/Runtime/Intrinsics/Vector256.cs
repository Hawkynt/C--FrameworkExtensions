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

#if !SUPPORTS_VECTOR_256_TYPE

using System.Runtime.CompilerServices;
using Guard;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;
using Guard;

namespace System.Runtime.Intrinsics {

/// <summary>
/// Provides a set of static methods for creating and working with 256-bit vectors.
/// </summary>
public static class Vector256 {
  internal const int Size = 32;
  internal const int Alignment = 32;

  private static void SkipInit<T>(out T result) => result = default;

  /// <summary>Gets a value that indicates whether 256-bit vector operations are subject to hardware acceleration.</summary>
  public static bool IsHardwareAccelerated => false;

  /// <summary>Computes the absolute value of each element in a vector.</summary>
  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Abs<T>(Vector256<T> vector) where T : struct {
    if (Scalar<T>.IsUnsigned)
      return vector;

    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var value = Scalar<T>.Abs(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  /// <summary>Adds two vectors to compute their sum.</summary>
  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Add<T>(Vector256<T> left, Vector256<T> right) where T : struct => left + right;

  /// <summary>Computes the bitwise-and of a given vector and the ones complement of another vector.</summary>
  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> AndNot<T>(Vector256<T> left, Vector256<T> right) where T : struct {
    Vector256<T>.ThrowIfNotSupported();
    return new(left._v0 & ~right._v0, left._v1 & ~right._v1, left._v2 & ~right._v2, left._v3 & ~right._v3);
  }

  /// <summary>Reinterprets a vector as a new vector type.</summary>
  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<TTo> As<TFrom, TTo>(this Vector256<TFrom> vector) where TFrom : struct where TTo : struct {
    Vector256<TFrom>.ThrowIfNotSupported();
    Vector256<TTo>.ThrowIfNotSupported();
    return Unsafe.As<Vector256<TFrom>, Vector256<TTo>>(ref vector);
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> AsByte<T>(this Vector256<T> vector) where T : struct => vector.As<T, byte>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> AsDouble<T>(this Vector256<T> vector) where T : struct => vector.As<T, double>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> AsInt16<T>(this Vector256<T> vector) where T : struct => vector.As<T, short>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> AsInt32<T>(this Vector256<T> vector) where T : struct => vector.As<T, int>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> AsInt64<T>(this Vector256<T> vector) where T : struct => vector.As<T, long>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<nint> AsNInt<T>(this Vector256<T> vector) where T : struct => vector.As<T, nint>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<nuint> AsNUInt<T>(this Vector256<T> vector) where T : struct => vector.As<T, nuint>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> AsSByte<T>(this Vector256<T> vector) where T : struct => vector.As<T, sbyte>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> AsSingle<T>(this Vector256<T> vector) where T : struct => vector.As<T, float>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> AsUInt16<T>(this Vector256<T> vector) where T : struct => vector.As<T, ushort>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> AsUInt32<T>(this Vector256<T> vector) where T : struct => vector.As<T, uint>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> AsUInt64<T>(this Vector256<T> vector) where T : struct => vector.As<T, ulong>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> BitwiseAnd<T>(Vector256<T> left, Vector256<T> right) where T : struct => left & right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> BitwiseOr<T>(Vector256<T> left, Vector256<T> right) where T : struct => left | right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Ceiling<T>(Vector256<T> vector) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var value = Scalar<T>.Ceiling(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Create<T>(T value) where T : struct => new(value);

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Create<T>(Vector128<T> lower, Vector128<T> upper) where T : struct {
    var lowerBytes = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref lower);
    var upperBytes = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref upper);
    return new(lowerBytes.Item1, lowerBytes.Item2, upperBytes.Item1, upperBytes.Item2);
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Divide<T>(Vector256<T> left, Vector256<T> right) where T : struct => left / right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T Dot<T>(Vector256<T> left, Vector256<T> right) where T : struct {
    var result = Scalar<T>.Zero();
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var product = Scalar<T>.Multiply(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result = Scalar<T>.Add(result, product);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Equals<T>(Vector256<T> left, Vector256<T> right) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var equals = Scalar<T>.ObjectEquals(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, equals ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Floor<T>(Vector256<T> vector) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var value = Scalar<T>.Floor(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T GetElement<T>(this Vector256<T> vector, int index) where T : struct => vector[index];

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  internal static T GetElementUnsafe<T>(in this Vector256<T> vector, int index) where T : struct {
    ref var address = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
    return Unsafe.Add(ref address, index);
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  internal static void SetElementUnsafe<T>(in this Vector256<T> vector, int index, T value) where T : struct {
    ref var address = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
    Unsafe.Add(ref address, index) = value;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> GreaterThan<T>(Vector256<T> left, Vector256<T> right) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var greater = Scalar<T>.GreaterThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, greater ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> LessThan<T>(Vector256<T> left, Vector256<T> right) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var less = Scalar<T>.LessThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, less ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Max<T>(Vector256<T> left, Vector256<T> right) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var value = Scalar<T>.Max(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Min<T>(Vector256<T> left, Vector256<T> right) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var value = Scalar<T>.Min(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Multiply<T>(Vector256<T> left, Vector256<T> right) where T : struct => left * right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Negate<T>(Vector256<T> vector) where T : struct => -vector;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> OnesComplement<T>(Vector256<T> vector) where T : struct => ~vector;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Sqrt<T>(Vector256<T> vector) where T : struct {
    SkipInit(out Vector256<T> result);
    for (var index = 0; index < Vector256<T>.Count; ++index) {
      var value = Scalar<T>.Sqrt(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Subtract<T>(Vector256<T> left, Vector256<T> right) where T : struct => left - right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T Sum<T>(Vector256<T> vector) where T : struct {
    var result = Scalar<T>.Zero();
    for (var index = 0; index < Vector256<T>.Count; ++index)
      result = Scalar<T>.Add(result, vector.GetElementUnsafe(index));
    return result;
  }
  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T ToScalar<T>(this Vector256<T> vector) where T : struct => vector[0];

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> WithElement<T>(this Vector256<T> vector, int index, T value) where T : struct {
    if ((uint)index >= Vector256<T>.Count)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

    var result = vector;
    result.SetElementUnsafe(index, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> Xor<T>(Vector256<T> left, Vector256<T> right) where T : struct => left ^ right;
}
}
#endif

