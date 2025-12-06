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

#if !SUPPORTS_VECTOR_128_TYPE

using System.Runtime.CompilerServices;
using Guard;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;
using Guard;

namespace System.Runtime.Intrinsics {

/// <summary>
/// Provides a set of static methods for creating and working with 128-bit vectors.
/// </summary>
public static class Vector128 {
  internal const int Size = 16;
  internal const int Alignment = 16;

  private static void SkipInit<T>(out T result) => result = default;

  public static bool IsHardwareAccelerated => false;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Abs<T>(Vector128<T> vector) where T : struct {
    if (Scalar<T>.IsUnsigned)
      return vector;

    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var value = Scalar<T>.Abs(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }

    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Add<T>(Vector128<T> left, Vector128<T> right) where T : struct => left + right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> AndNot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
    Vector128<T>.ThrowIfNotSupported();
    return new(left._lower & ~right._lower, left._upper & ~right._upper);
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<TTo> As<TFrom, TTo>(this Vector128<TFrom> vector) where TFrom : struct where TTo : struct {
    Vector128<TFrom>.ThrowIfNotSupported();
    Vector128<TTo>.ThrowIfNotSupported();
    return Unsafe.As<Vector128<TFrom>, Vector128<TTo>>(ref vector);
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> AsByte<T>(this Vector128<T> vector) where T : struct => vector.As<T, byte>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> AsDouble<T>(this Vector128<T> vector) where T : struct => vector.As<T, double>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> AsInt16<T>(this Vector128<T> vector) where T : struct => vector.As<T, short>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> AsInt32<T>(this Vector128<T> vector) where T : struct => vector.As<T, int>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> AsInt64<T>(this Vector128<T> vector) where T : struct => vector.As<T, long>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<nint> AsNInt<T>(this Vector128<T> vector) where T : struct => vector.As<T, nint>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<nuint> AsNUInt<T>(this Vector128<T> vector) where T : struct => vector.As<T, nuint>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> AsSByte<T>(this Vector128<T> vector) where T : struct => vector.As<T, sbyte>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> AsSingle<T>(this Vector128<T> vector) where T : struct => vector.As<T, float>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> AsUInt16<T>(this Vector128<T> vector) where T : struct => vector.As<T, ushort>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> AsUInt32<T>(this Vector128<T> vector) where T : struct => vector.As<T, uint>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> AsUInt64<T>(this Vector128<T> vector) where T : struct => vector.As<T, ulong>();

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> BitwiseAnd<T>(Vector128<T> left, Vector128<T> right) where T : struct => left & right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> BitwiseOr<T>(Vector128<T> left, Vector128<T> right) where T : struct => left | right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Ceiling<T>(Vector128<T> vector) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var value = Scalar<T>.Ceiling(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Create<T>(T value) where T : struct => new(value);

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Create<T>(Vector64<T> lower, Vector64<T> upper) where T : struct {
    var lowerVal = Unsafe.As<Vector64<T>, ulong>(ref lower);
    var upperVal = Unsafe.As<Vector64<T>, ulong>(ref upper);
    return new(lowerVal, upperVal);
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Divide<T>(Vector128<T> left, Vector128<T> right) where T : struct => left / right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T Dot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
    var result = Scalar<T>.Zero();
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var product = Scalar<T>.Multiply(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result = Scalar<T>.Add(result, product);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Equals<T>(Vector128<T> left, Vector128<T> right) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var equals = Scalar<T>.ObjectEquals(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, equals ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Floor<T>(Vector128<T> vector) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var value = Scalar<T>.Floor(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T GetElement<T>(this Vector128<T> vector, int index) where T : struct => vector[index];

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  internal static T GetElementUnsafe<T>(in this Vector128<T> vector, int index) where T : struct {
    ref var address = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
    return Unsafe.Add(ref address, index);
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  internal static void SetElementUnsafe<T>(in this Vector128<T> vector, int index, T value) where T : struct {
    ref var address = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
    Unsafe.Add(ref address, index) = value;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> GreaterThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var greater = Scalar<T>.GreaterThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, greater ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> LessThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var less = Scalar<T>.LessThan(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, less ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Max<T>(Vector128<T> left, Vector128<T> right) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var value = Scalar<T>.Max(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Min<T>(Vector128<T> left, Vector128<T> right) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var value = Scalar<T>.Min(left.GetElementUnsafe(index), right.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Multiply<T>(Vector128<T> left, Vector128<T> right) where T : struct => left * right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Negate<T>(Vector128<T> vector) where T : struct => -vector;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> OnesComplement<T>(Vector128<T> vector) where T : struct => ~vector;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Sqrt<T>(Vector128<T> vector) where T : struct {
    SkipInit(out Vector128<T> result);
    for (var index = 0; index < Vector128<T>.Count; ++index) {
      var value = Scalar<T>.Sqrt(vector.GetElementUnsafe(index));
      result.SetElementUnsafe(index, value);
    }
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Subtract<T>(Vector128<T> left, Vector128<T> right) where T : struct => left - right;

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T Sum<T>(Vector128<T> vector) where T : struct {
    var result = Scalar<T>.Zero();
    for (var index = 0; index < Vector128<T>.Count; ++index)
      result = Scalar<T>.Add(result, vector.GetElementUnsafe(index));
    return result;
  }
  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static T ToScalar<T>(this Vector128<T> vector) where T : struct => vector[0];

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> WithElement<T>(this Vector128<T> vector, int index, T value) where T : struct {
    if ((uint)index >= Vector128<T>.Count)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

    var result = vector;
    result.SetElementUnsafe(index, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Xor<T>(Vector128<T> left, Vector128<T> right) where T : struct => left ^ right;
}
}
#endif

