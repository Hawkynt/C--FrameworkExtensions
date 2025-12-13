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

#if !SUPPORTS_VECTOR_512_BASE

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<T> Load<T>(T* source) where T : struct
    => Unsafe.ReadUnaligned<Vector512<T>>(source);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<T> LoadUnsafe<T>(ref T source) where T : struct
    => Unsafe.ReadUnaligned<Vector512<T>>(ref Unsafe.As<T, byte>(ref source));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<T> LoadAligned<T>(T* source) where T : struct
    => Unsafe.ReadUnaligned<Vector512<T>>(source);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store<T>(Vector512<T> source, T* destination) where T : struct
    => Unsafe.WriteUnaligned(destination, source);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreUnsafe<T>(Vector512<T> source, ref T destination) where T : struct
    => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), source);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned<T>(Vector512<T> source, T* destination) where T : struct
    => Unsafe.WriteUnaligned(destination, source);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal<T>(Vector512<T> source, T* destination) where T : struct
    => Unsafe.WriteUnaligned(destination, source);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> CreateScalar<T>(T value) where T : struct {
    SkipInit(out Vector512<T> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> CreateScalarUnsafe<T>(T value) where T : struct {
    SkipInit(out Vector512<T> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Create(double e0, double e1, double e2, double e3, double e4, double e5, double e6, double e7) {
    unsafe {
        double* ptr = stackalloc double[8];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        return Unsafe.ReadUnaligned<Vector512<double>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Create(long e0, long e1, long e2, long e3, long e4, long e5, long e6, long e7) {
    unsafe {
        long* ptr = stackalloc long[8];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        return Unsafe.ReadUnaligned<Vector512<long>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Create(ulong e0, ulong e1, ulong e2, ulong e3, ulong e4, ulong e5, ulong e6, ulong e7) {
    unsafe {
        ulong* ptr = stackalloc ulong[8];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        return Unsafe.ReadUnaligned<Vector512<ulong>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> Shuffle<T>(Vector512<T> vector, Vector512<T> indices) where T : struct {
    SkipInit(out Vector512<T> result);
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      var index = Scalar<T>.ToInt32(Vector512.GetElement(indices, i)) & (count - 1);
      result = result.WithElement(i, Vector512.GetElement(vector, index));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ExtractMostSignificantBits<T>(Vector512<T> vector) where T : struct {
    ulong result = 0;
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      if (Scalar<T>.ExtractMostSignificantBit(Vector512.GetElement(vector, i)))
        result |= 1UL << i;
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> ConditionalSelect<T>(Vector512<T> condition, Vector512<T> left, Vector512<T> right) where T : struct {
    return Vector512.BitwiseOr(Vector512.BitwiseAnd(condition, left), Vector512.AndNot(right, condition));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> ShiftLeft<T>(Vector512<T> vector, int shiftCount) where T : struct {
    SkipInit(out Vector512<T> result);
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      result = result.WithElement(i, Scalar<T>.ShiftLeft(Vector512.GetElement(vector, i), shiftCount));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> ShiftRightArithmetic<T>(Vector512<T> vector, int shiftCount) where T : struct {
    SkipInit(out Vector512<T> result);
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      result = result.WithElement(i, Scalar<T>.ShiftRightArithmetic(Vector512.GetElement(vector, i), shiftCount));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> ShiftRightLogical<T>(Vector512<T> vector, int shiftCount) where T : struct {
    SkipInit(out Vector512<T> result);
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      result = result.WithElement(i, Scalar<T>.ShiftRightLogical(Vector512.GetElement(vector, i), shiftCount));
    }
    return result;
  }

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

  // ===== COMPARISON ALL/ANY METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EqualsAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.ObjectEquals(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EqualsAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.ObjectEquals(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.GreaterThan(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.GreaterThan(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanOrEqualAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.GreaterThanOrEqual(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanOrEqualAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.GreaterThanOrEqual(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.LessThan(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.LessThan(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanOrEqualAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.LessThanOrEqual(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanOrEqualAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.LessThanOrEqual(Vector512.GetElement(left, i), Vector512.GetElement(right, i)))
        return true;
    return false;
  }

  // ===== COPYSIGN =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CopySign(Vector512<float> value, Vector512<float> sign) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    for (var i = 0; i < count; ++i) {
      var v = Vector512.GetElement(value, i);
      var s = Vector512.GetElement(sign, i);
      var absV = MathF.Abs(v);
      result = Vector512.WithElement(result, i, s < 0 ? -absV : absV);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CopySign(Vector512<double> value, Vector512<double> sign) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    for (var i = 0; i < count; ++i) {
      var v = Vector512.GetElement(value, i);
      var s = Vector512.GetElement(sign, i);
      var absV = Math.Abs(v);
      result = Vector512.WithElement(result, i, s < 0 ? -absV : absV);
    }
    return result;
  }

  // ===== CLASSIFICATION METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> IsNaN(Vector512<float> vector) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(-1);
    for (var i = 0; i < count; ++i) {
      var isNan = float.IsNaN(Vector512.GetElement(vector, i));
      result = Vector512.WithElement(result, i, isNan ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsNaN(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var isNan = double.IsNaN(Vector512.GetElement(vector, i));
      result = Vector512.WithElement(result, i, isNan ? allBitsSet : 0d);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> IsNegative(Vector512<float> vector) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(-1);
    for (var i = 0; i < count; ++i) {
      var val = Vector512.GetElement(vector, i);
      var isNeg = val < 0 || (val == 0 && float.IsNegativeInfinity(1f / val));
      result = Vector512.WithElement(result, i, isNeg ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsNegative(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var val = Vector512.GetElement(vector, i);
      var isNeg = val < 0 || (val == 0 && double.IsNegativeInfinity(1d / val));
      result = Vector512.WithElement(result, i, isNeg ? allBitsSet : 0d);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> IsPositive(Vector512<float> vector) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(-1);
    for (var i = 0; i < count; ++i) {
      var val = Vector512.GetElement(vector, i);
      var isPos = val > 0 || (val == 0 && float.IsPositiveInfinity(1f / val));
      result = Vector512.WithElement(result, i, isPos ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsPositive(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var val = Vector512.GetElement(vector, i);
      var isPos = val > 0 || (val == 0 && double.IsPositiveInfinity(1d / val));
      result = Vector512.WithElement(result, i, isPos ? allBitsSet : 0d);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> IsZero(Vector512<float> vector) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(-1);
    for (var i = 0; i < count; ++i) {
      var isZero = Vector512.GetElement(vector, i) == 0f;
      result = Vector512.WithElement(result, i, isZero ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsZero(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var isZero = Vector512.GetElement(vector, i) == 0d;
      result = Vector512.WithElement(result, i, isZero ? allBitsSet : 0d);
    }
    return result;
  }

  // ===== NARROW METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Narrow(Vector512<double> lower, Vector512<double> upper) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 8; ++i) {
      result = Vector512.WithElement(result, i, (float)Vector512.GetElement(lower, i));
      result = Vector512.WithElement(result, i + 8, (float)Vector512.GetElement(upper, i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Narrow(Vector512<long> lower, Vector512<long> upper) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < 8; ++i) {
      result = Vector512.WithElement(result, i, (int)Vector512.GetElement(lower, i));
      result = Vector512.WithElement(result, i + 8, (int)Vector512.GetElement(upper, i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Narrow(Vector512<ulong> lower, Vector512<ulong> upper) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < 8; ++i) {
      result = Vector512.WithElement(result, i, (uint)Vector512.GetElement(lower, i));
      result = Vector512.WithElement(result, i + 8, (uint)Vector512.GetElement(upper, i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<short> Narrow(Vector512<int> lower, Vector512<int> upper) {
    var result = Vector512<short>.Zero;
    for (var i = 0; i < 16; ++i) {
      result = Vector512.WithElement(result, i, (short)Vector512.GetElement(lower, i));
      result = Vector512.WithElement(result, i + 16, (short)Vector512.GetElement(upper, i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ushort> Narrow(Vector512<uint> lower, Vector512<uint> upper) {
    var result = Vector512<ushort>.Zero;
    for (var i = 0; i < 16; ++i) {
      result = Vector512.WithElement(result, i, (ushort)Vector512.GetElement(lower, i));
      result = Vector512.WithElement(result, i + 16, (ushort)Vector512.GetElement(upper, i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<sbyte> Narrow(Vector512<short> lower, Vector512<short> upper) {
    var result = Vector512<sbyte>.Zero;
    for (var i = 0; i < 32; ++i) {
      result = Vector512.WithElement(result, i, (sbyte)Vector512.GetElement(lower, i));
      result = Vector512.WithElement(result, i + 32, (sbyte)Vector512.GetElement(upper, i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<byte> Narrow(Vector512<ushort> lower, Vector512<ushort> upper) {
    var result = Vector512<byte>.Zero;
    for (var i = 0; i < 32; ++i) {
      result = Vector512.WithElement(result, i, (byte)Vector512.GetElement(lower, i));
      result = Vector512.WithElement(result, i + 32, (byte)Vector512.GetElement(upper, i));
    }
    return result;
  }

  // ===== WIDEN METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (Vector512<double> Lower, Vector512<double> Upper) Widen(Vector512<float> vector) {
    var lower = Vector512<double>.Zero;
    var upper = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i) {
      lower = Vector512.WithElement(lower, i, (double)Vector512.GetElement(vector, i));
      upper = Vector512.WithElement(upper, i, (double)Vector512.GetElement(vector, i + 8));
    }
    return (lower, upper);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (Vector512<long> Lower, Vector512<long> Upper) Widen(Vector512<int> vector) {
    var lower = Vector512<long>.Zero;
    var upper = Vector512<long>.Zero;
    for (var i = 0; i < 8; ++i) {
      lower = Vector512.WithElement(lower, i, (long)Vector512.GetElement(vector, i));
      upper = Vector512.WithElement(upper, i, (long)Vector512.GetElement(vector, i + 8));
    }
    return (lower, upper);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (Vector512<ulong> Lower, Vector512<ulong> Upper) Widen(Vector512<uint> vector) {
    var lower = Vector512<ulong>.Zero;
    var upper = Vector512<ulong>.Zero;
    for (var i = 0; i < 8; ++i) {
      lower = Vector512.WithElement(lower, i, (ulong)Vector512.GetElement(vector, i));
      upper = Vector512.WithElement(upper, i, (ulong)Vector512.GetElement(vector, i + 8));
    }
    return (lower, upper);
  }

  // ===== CONVERTTO METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ConvertToInt32(Vector512<float> vector) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < 16; ++i)
      result = Vector512.WithElement(result, i, (int)Vector512.GetElement(vector, i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> ConvertToSingle(Vector512<int> vector) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 16; ++i)
      result = Vector512.WithElement(result, i, (float)Vector512.GetElement(vector, i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> ConvertToSingle(Vector512<uint> vector) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 16; ++i)
      result = Vector512.WithElement(result, i, (float)Vector512.GetElement(vector, i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ConvertToInt64(Vector512<double> vector) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < 8; ++i)
      result = Vector512.WithElement(result, i, (long)Vector512.GetElement(vector, i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ConvertToDouble(Vector512<long> vector) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i)
      result = Vector512.WithElement(result, i, (double)Vector512.GetElement(vector, i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ConvertToDouble(Vector512<ulong> vector) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i)
      result = Vector512.WithElement(result, i, (double)Vector512.GetElement(vector, i));
    return result;
  }
}

#endif
