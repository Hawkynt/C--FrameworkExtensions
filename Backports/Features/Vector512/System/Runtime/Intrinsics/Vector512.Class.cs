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

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

#if !FEATURE_VECTOR512STATIC_WAVE1

namespace System.Runtime.Intrinsics {

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
      var index = Scalar<T>.ToInt32(indices.GetElement(i)) & (count - 1);
      result = result.WithElement(i, vector.GetElement(index));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ExtractMostSignificantBits<T>(Vector512<T> vector) where T : struct {
    ulong result = 0;
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      if (Scalar<T>.ExtractMostSignificantBit(vector.GetElement(i)))
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
      result = result.WithElement(i, Scalar<T>.ShiftLeft(vector.GetElement(i), shiftCount));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> ShiftRightArithmetic<T>(Vector512<T> vector, int shiftCount) where T : struct {
    SkipInit(out Vector512<T> result);
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      result = result.WithElement(i, Scalar<T>.ShiftRightArithmetic(vector.GetElement(i), shiftCount));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> ShiftRightLogical<T>(Vector512<T> vector, int shiftCount) where T : struct {
    SkipInit(out Vector512<T> result);
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      result = result.WithElement(i, Scalar<T>.ShiftRightLogical(vector.GetElement(i), shiftCount));
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
      if (!Scalar<T>.ObjectEquals(left.GetElement(i), right.GetElement(i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EqualsAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.ObjectEquals(left.GetElement(i), right.GetElement(i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.GreaterThan(left.GetElement(i), right.GetElement(i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.GreaterThan(left.GetElement(i), right.GetElement(i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanOrEqualAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.GreaterThanOrEqual(left.GetElement(i), right.GetElement(i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanOrEqualAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.GreaterThanOrEqual(left.GetElement(i), right.GetElement(i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.LessThan(left.GetElement(i), right.GetElement(i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.LessThan(left.GetElement(i), right.GetElement(i)))
        return true;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanOrEqualAll<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (!Scalar<T>.LessThanOrEqual(left.GetElement(i), right.GetElement(i)))
        return false;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanOrEqualAny<T>(Vector512<T> left, Vector512<T> right) where T : struct {
    for (var i = 0; i < 64 / Unsafe.SizeOf<T>(); ++i)
      if (Scalar<T>.LessThanOrEqual(left.GetElement(i), right.GetElement(i)))
        return true;
    return false;
  }

  // ===== COPYSIGN =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CopySign(Vector512<float> value, Vector512<float> sign) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    for (var i = 0; i < count; ++i) {
      var v = value.GetElement(i);
      var s = sign.GetElement(i);
      var absV = MathF.Abs(v);
      result = result.WithElement(i, s < 0 ? -absV : absV);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CopySign(Vector512<double> value, Vector512<double> sign) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    for (var i = 0; i < count; ++i) {
      var v = value.GetElement(i);
      var s = sign.GetElement(i);
      var absV = Math.Abs(v);
      result = result.WithElement(i, s < 0 ? -absV : absV);
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
      var isNan = float.IsNaN(vector.GetElement(i));
      result = result.WithElement(i, isNan ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsNaN(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var isNan = double.IsNaN(vector.GetElement(i));
      result = result.WithElement(i, isNan ? allBitsSet : 0d);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> IsNegative(Vector512<float> vector) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(-1);
    for (var i = 0; i < count; ++i) {
      var val = vector.GetElement(i);
      var isNeg = val < 0 || (val == 0 && float.IsNegativeInfinity(1f / val));
      result = result.WithElement(i, isNeg ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsNegative(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var val = vector.GetElement(i);
      var isNeg = val < 0 || (val == 0 && double.IsNegativeInfinity(1d / val));
      result = result.WithElement(i, isNeg ? allBitsSet : 0d);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> IsPositive(Vector512<float> vector) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(-1);
    for (var i = 0; i < count; ++i) {
      var val = vector.GetElement(i);
      var isPos = val > 0 || (val == 0 && float.IsPositiveInfinity(1f / val));
      result = result.WithElement(i, isPos ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsPositive(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var val = vector.GetElement(i);
      var isPos = val > 0 || (val == 0 && double.IsPositiveInfinity(1d / val));
      result = result.WithElement(i, isPos ? allBitsSet : 0d);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> IsZero(Vector512<float> vector) {
    var count = 64 / sizeof(float);
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(-1);
    for (var i = 0; i < count; ++i) {
      var isZero = vector.GetElement(i) == 0f;
      result = result.WithElement(i, isZero ? allBitsSet : 0f);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> IsZero(Vector512<double> vector) {
    var count = 64 / sizeof(double);
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
    for (var i = 0; i < count; ++i) {
      var isZero = vector.GetElement(i) == 0d;
      result = result.WithElement(i, isZero ? allBitsSet : 0d);
    }
    return result;
  }

  // ===== NARROW METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Narrow(Vector512<double> lower, Vector512<double> upper) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(i, (float)lower.GetElement(i));
      result = result.WithElement(i + 8, (float)upper.GetElement(i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Narrow(Vector512<long> lower, Vector512<long> upper) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(i, (int)lower.GetElement(i));
      result = result.WithElement(i + 8, (int)upper.GetElement(i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Narrow(Vector512<ulong> lower, Vector512<ulong> upper) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(i, (uint)lower.GetElement(i));
      result = result.WithElement(i + 8, (uint)upper.GetElement(i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<short> Narrow(Vector512<int> lower, Vector512<int> upper) {
    var result = Vector512<short>.Zero;
    for (var i = 0; i < 16; ++i) {
      result = result.WithElement(i, (short)lower.GetElement(i));
      result = result.WithElement(i + 16, (short)upper.GetElement(i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ushort> Narrow(Vector512<uint> lower, Vector512<uint> upper) {
    var result = Vector512<ushort>.Zero;
    for (var i = 0; i < 16; ++i) {
      result = result.WithElement(i, (ushort)lower.GetElement(i));
      result = result.WithElement(i + 16, (ushort)upper.GetElement(i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<sbyte> Narrow(Vector512<short> lower, Vector512<short> upper) {
    var result = Vector512<sbyte>.Zero;
    for (var i = 0; i < 32; ++i) {
      result = result.WithElement(i, (sbyte)lower.GetElement(i));
      result = result.WithElement(i + 32, (sbyte)upper.GetElement(i));
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<byte> Narrow(Vector512<ushort> lower, Vector512<ushort> upper) {
    var result = Vector512<byte>.Zero;
    for (var i = 0; i < 32; ++i) {
      result = result.WithElement(i, (byte)lower.GetElement(i));
      result = result.WithElement(i + 32, (byte)upper.GetElement(i));
    }
    return result;
  }

  // ===== WIDEN METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (Vector512<double> Lower, Vector512<double> Upper) Widen(Vector512<float> vector) {
    var lower = Vector512<double>.Zero;
    var upper = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i) {
      lower = lower.WithElement(i, (double)vector.GetElement(i));
      upper = upper.WithElement(i, (double)vector.GetElement(i + 8));
    }
    return (lower, upper);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (Vector512<long> Lower, Vector512<long> Upper) Widen(Vector512<int> vector) {
    var lower = Vector512<long>.Zero;
    var upper = Vector512<long>.Zero;
    for (var i = 0; i < 8; ++i) {
      lower = lower.WithElement(i, (long)vector.GetElement(i));
      upper = upper.WithElement(i, (long)vector.GetElement(i + 8));
    }
    return (lower, upper);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (Vector512<ulong> Lower, Vector512<ulong> Upper) Widen(Vector512<uint> vector) {
    var lower = Vector512<ulong>.Zero;
    var upper = Vector512<ulong>.Zero;
    for (var i = 0; i < 8; ++i) {
      lower = lower.WithElement(i, (ulong)vector.GetElement(i));
      upper = upper.WithElement(i, (ulong)vector.GetElement(i + 8));
    }
    return (lower, upper);
  }

  // ===== CONVERTTO METHODS =====

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ConvertToInt32(Vector512<float> vector) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < 16; ++i)
      result = result.WithElement(i, (int)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> ConvertToSingle(Vector512<int> vector) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 16; ++i)
      result = result.WithElement(i, (float)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> ConvertToSingle(Vector512<uint> vector) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 16; ++i)
      result = result.WithElement(i, (float)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ConvertToInt64(Vector512<double> vector) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, (long)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ConvertToDouble(Vector512<long> vector) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, (double)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ConvertToDouble(Vector512<ulong> vector) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, (double)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> ConvertToUInt32(Vector512<float> vector) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < 16; ++i)
      result = result.WithElement(i, (uint)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> ConvertToUInt64(Vector512<double> vector) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, (ulong)vector.GetElement(i));
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> ShuffleNative<T>(Vector512<T> vector, Vector512<T> indices) where T : struct {
    SkipInit(out Vector512<T> result);
    var count = Vector512<T>.Count;
    for (var i = 0; i < count; ++i) {
      var index = Scalar<T>.ToInt32(indices.GetElement(i)) & (count - 1);
      result = result.WithElement(i, vector.GetElement(index));
    }
    return result;
  }

#if NET6_0_OR_GREATER
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Numerics.Vector<T> AsVector<T>(Vector512<T> vector) where T : struct {
    Vector512<T>.ThrowIfNotSupported();
    return Unsafe.As<Vector512<T>, Numerics.Vector<T>>(ref vector);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> AsVector512<T>(Numerics.Vector<T> vector) where T : struct {
    Vector512<T>.ThrowIfNotSupported();
    return Unsafe.As<Numerics.Vector<T>, Vector512<T>>(ref vector);
  }
#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void CopyTo<T>(this Vector512<T> vector, T[] destination) where T : struct {
    if (destination == null)
      AlwaysThrow.ArgumentNullException(nameof(destination));

    CopyTo(vector, destination, 0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void CopyTo<T>(this Vector512<T> vector, T[] destination, int startIndex) where T : struct {
    if (destination == null)
      AlwaysThrow.ArgumentNullException(nameof(destination));

    if ((uint)startIndex >= (uint)destination.Length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));

    if ((destination.Length - startIndex) < Vector512<T>.Count)
      AlwaysThrow.ArgumentException(nameof(destination), "Destination array is not long enough");

    for (var i = 0; i < Vector512<T>.Count; ++i)
      destination[startIndex + i] = vector.GetElementUnsafe(i);
  }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void CopyTo<T>(this Vector512<T> vector, Span<T> destination) where T : struct {
    if (destination.Length < Vector512<T>.Count)
      AlwaysThrow.ArgumentException(nameof(destination), "Destination span is not long enough");

    for (var i = 0; i < Vector512<T>.Count; ++i)
      destination[i] = vector.GetElementUnsafe(i);
  }
#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Create(float e0, float e1, float e2, float e3, float e4, float e5, float e6, float e7, float e8, float e9, float e10, float e11, float e12, float e13, float e14, float e15) {
    unsafe {
        float* ptr = stackalloc float[16];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        return Unsafe.ReadUnaligned<Vector512<float>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Create(int e0, int e1, int e2, int e3, int e4, int e5, int e6, int e7, int e8, int e9, int e10, int e11, int e12, int e13, int e14, int e15) {
    unsafe {
        int* ptr = stackalloc int[16];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        return Unsafe.ReadUnaligned<Vector512<int>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Create(uint e0, uint e1, uint e2, uint e3, uint e4, uint e5, uint e6, uint e7, uint e8, uint e9, uint e10, uint e11, uint e12, uint e13, uint e14, uint e15) {
    unsafe {
        uint* ptr = stackalloc uint[16];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        return Unsafe.ReadUnaligned<Vector512<uint>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<short> Create(short e0, short e1, short e2, short e3, short e4, short e5, short e6, short e7, short e8, short e9, short e10, short e11, short e12, short e13, short e14, short e15, short e16, short e17, short e18, short e19, short e20, short e21, short e22, short e23, short e24, short e25, short e26, short e27, short e28, short e29, short e30, short e31) {
    unsafe {
        short* ptr = stackalloc short[32];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        ptr[16] = e16; ptr[17] = e17; ptr[18] = e18; ptr[19] = e19; ptr[20] = e20; ptr[21] = e21; ptr[22] = e22; ptr[23] = e23;
        ptr[24] = e24; ptr[25] = e25; ptr[26] = e26; ptr[27] = e27; ptr[28] = e28; ptr[29] = e29; ptr[30] = e30; ptr[31] = e31;
        return Unsafe.ReadUnaligned<Vector512<short>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ushort> Create(ushort e0, ushort e1, ushort e2, ushort e3, ushort e4, ushort e5, ushort e6, ushort e7, ushort e8, ushort e9, ushort e10, ushort e11, ushort e12, ushort e13, ushort e14, ushort e15, ushort e16, ushort e17, ushort e18, ushort e19, ushort e20, ushort e21, ushort e22, ushort e23, ushort e24, ushort e25, ushort e26, ushort e27, ushort e28, ushort e29, ushort e30, ushort e31) {
    unsafe {
        ushort* ptr = stackalloc ushort[32];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        ptr[16] = e16; ptr[17] = e17; ptr[18] = e18; ptr[19] = e19; ptr[20] = e20; ptr[21] = e21; ptr[22] = e22; ptr[23] = e23;
        ptr[24] = e24; ptr[25] = e25; ptr[26] = e26; ptr[27] = e27; ptr[28] = e28; ptr[29] = e29; ptr[30] = e30; ptr[31] = e31;
        return Unsafe.ReadUnaligned<Vector512<ushort>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7, byte e8, byte e9, byte e10, byte e11, byte e12, byte e13, byte e14, byte e15, byte e16, byte e17, byte e18, byte e19, byte e20, byte e21, byte e22, byte e23, byte e24, byte e25, byte e26, byte e27, byte e28, byte e29, byte e30, byte e31, byte e32, byte e33, byte e34, byte e35, byte e36, byte e37, byte e38, byte e39, byte e40, byte e41, byte e42, byte e43, byte e44, byte e45, byte e46, byte e47, byte e48, byte e49, byte e50, byte e51, byte e52, byte e53, byte e54, byte e55, byte e56, byte e57, byte e58, byte e59, byte e60, byte e61, byte e62, byte e63) {
    unsafe {
        byte* ptr = stackalloc byte[64];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        ptr[16] = e16; ptr[17] = e17; ptr[18] = e18; ptr[19] = e19; ptr[20] = e20; ptr[21] = e21; ptr[22] = e22; ptr[23] = e23;
        ptr[24] = e24; ptr[25] = e25; ptr[26] = e26; ptr[27] = e27; ptr[28] = e28; ptr[29] = e29; ptr[30] = e30; ptr[31] = e31;
        ptr[32] = e32; ptr[33] = e33; ptr[34] = e34; ptr[35] = e35; ptr[36] = e36; ptr[37] = e37; ptr[38] = e38; ptr[39] = e39;
        ptr[40] = e40; ptr[41] = e41; ptr[42] = e42; ptr[43] = e43; ptr[44] = e44; ptr[45] = e45; ptr[46] = e46; ptr[47] = e47;
        ptr[48] = e48; ptr[49] = e49; ptr[50] = e50; ptr[51] = e51; ptr[52] = e52; ptr[53] = e53; ptr[54] = e54; ptr[55] = e55;
        ptr[56] = e56; ptr[57] = e57; ptr[58] = e58; ptr[59] = e59; ptr[60] = e60; ptr[61] = e61; ptr[62] = e62; ptr[63] = e63;
        return Unsafe.ReadUnaligned<Vector512<byte>>(ptr);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<sbyte> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7, sbyte e8, sbyte e9, sbyte e10, sbyte e11, sbyte e12, sbyte e13, sbyte e14, sbyte e15, sbyte e16, sbyte e17, sbyte e18, sbyte e19, sbyte e20, sbyte e21, sbyte e22, sbyte e23, sbyte e24, sbyte e25, sbyte e26, sbyte e27, sbyte e28, sbyte e29, sbyte e30, sbyte e31, sbyte e32, sbyte e33, sbyte e34, sbyte e35, sbyte e36, sbyte e37, sbyte e38, sbyte e39, sbyte e40, sbyte e41, sbyte e42, sbyte e43, sbyte e44, sbyte e45, sbyte e46, sbyte e47, sbyte e48, sbyte e49, sbyte e50, sbyte e51, sbyte e52, sbyte e53, sbyte e54, sbyte e55, sbyte e56, sbyte e57, sbyte e58, sbyte e59, sbyte e60, sbyte e61, sbyte e62, sbyte e63) {
    unsafe {
        sbyte* ptr = stackalloc sbyte[64];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        ptr[16] = e16; ptr[17] = e17; ptr[18] = e18; ptr[19] = e19; ptr[20] = e20; ptr[21] = e21; ptr[22] = e22; ptr[23] = e23;
        ptr[24] = e24; ptr[25] = e25; ptr[26] = e26; ptr[27] = e27; ptr[28] = e28; ptr[29] = e29; ptr[30] = e30; ptr[31] = e31;
        ptr[32] = e32; ptr[33] = e33; ptr[34] = e34; ptr[35] = e35; ptr[36] = e36; ptr[37] = e37; ptr[38] = e38; ptr[39] = e39;
        ptr[40] = e40; ptr[41] = e41; ptr[42] = e42; ptr[43] = e43; ptr[44] = e44; ptr[45] = e45; ptr[46] = e46; ptr[47] = e47;
        ptr[48] = e48; ptr[49] = e49; ptr[50] = e50; ptr[51] = e51; ptr[52] = e52; ptr[53] = e53; ptr[54] = e54; ptr[55] = e55;
        ptr[56] = e56; ptr[57] = e57; ptr[58] = e58; ptr[59] = e59; ptr[60] = e60; ptr[61] = e61; ptr[62] = e62; ptr[63] = e63;
        return Unsafe.ReadUnaligned<Vector512<sbyte>>(ptr);
    }
  }
}

}
#endif

#if !FEATURE_VECTOR512STATIC_WAVE2

namespace System.Runtime.Intrinsics {

public static partial class Vector512AdvancedPolyfills {
  extension(Vector512) {

    /// <summary>Clamps a vector to be within the specified minimum and maximum values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> Clamp<T>(Vector512<T> value, Vector512<T> min, Vector512<T> max) where T : struct {
      var result = value;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var v = value.GetElement(i);
        var lo = min.GetElement(i);
        var hi = max.GetElement(i);
        if (Scalar<T>.LessThan(v, lo))
          result = result.WithElement(i, lo);
        else if (Scalar<T>.GreaterThan(v, hi))
          result = result.WithElement(i, hi);
      }
      return result;
    }

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Round(Vector512<float> vector) {
      unsafe {
        float* ptr = stackalloc float[16];
        for (var i = 0; i < 16; ++i)
          ptr[i] = MathF.Round(vector.GetElement(i));
        return Unsafe.ReadUnaligned<Vector512<float>>(ptr);
      }
    }

    /// <summary>Rounds each element of a vector to the nearest integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Round(Vector512<double> vector) {
      unsafe {
        double* ptr = stackalloc double[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = Math.Round(vector.GetElement(i));
        return Unsafe.ReadUnaligned<Vector512<double>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Truncate(Vector512<float> vector) {
      unsafe {
        float* ptr = stackalloc float[16];
        for (var i = 0; i < 16; ++i)
          ptr[i] = MathF.Truncate(vector.GetElement(i));
        return Unsafe.ReadUnaligned<Vector512<float>>(ptr);
      }
    }

    /// <summary>Truncates each element of a vector to the integer component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Truncate(Vector512<double> vector) {
      unsafe {
        double* ptr = stackalloc double[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = Math.Truncate(vector.GetElement(i));
        return Unsafe.ReadUnaligned<Vector512<double>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> FusedMultiplyAdd(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
      unsafe {
        float* ptr = stackalloc float[16];
        for (var i = 0; i < 16; ++i)
          ptr[i] = a.GetElement(i) * b.GetElement(i) + c.GetElement(i);
        return Unsafe.ReadUnaligned<Vector512<float>>(ptr);
      }
    }

    /// <summary>Computes (a * b) + c for each element of the vectors.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> FusedMultiplyAdd(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
      unsafe {
        double* ptr = stackalloc double[8];
        for (var i = 0; i < 8; ++i)
          ptr[i] = a.GetElement(i) * b.GetElement(i) + c.GetElement(i);
        return Unsafe.ReadUnaligned<Vector512<double>>(ptr);
      }
    }

    /// <summary>
    /// Checks if all elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool All<T>(Vector512<T> vector, T value) where T : struct {
      for (var i = 0; i < Vector512<T>.Count; ++i)
        if (!Scalar<T>.ObjectEquals(vector.GetElement(i), value))
          return false;
      return true;
    }

    /// <summary>
    /// Checks if any element in the vector equals the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Any<T>(Vector512<T> vector, T value) where T : struct {
      for (var i = 0; i < Vector512<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(vector.GetElement(i), value))
          return true;
      return false;
    }

    /// <summary>
    /// Checks if no elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool None<T>(Vector512<T> vector, T value) where T : struct => !Any(vector, value);

    /// <summary>
    /// Checks if all elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllWhereAllBitsSet<T>(Vector512<T> vector) where T : struct => vector == Vector512<T>.AllBitsSet;

    /// <summary>
    /// Checks if any element in the vector has all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyWhereAllBitsSet<T>(Vector512<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector512<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(vector.GetElement(i), allSet))
          return true;
      return false;
    }

    /// <summary>
    /// Checks if no elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NoneWhereAllBitsSet<T>(Vector512<T> vector) where T : struct => !AnyWhereAllBitsSet(vector);

    /// <summary>
    /// Counts how many elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count<T>(Vector512<T> vector, T value) where T : struct {
      var count = 0;
      for (var i = 0; i < Vector512<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(vector.GetElement(i), value))
          ++count;
      return count;
    }

    /// <summary>
    /// Counts how many elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountWhereAllBitsSet<T>(Vector512<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      var count = 0;
      for (var i = 0; i < Vector512<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(vector.GetElement(i), allSet))
          ++count;
      return count;
    }

    /// <summary>
    /// Returns the index of the first element that has all bits set, or -1 if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfWhereAllBitsSet<T>(Vector512<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector512<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(vector.GetElement(i), allSet))
          return i;
      return -1;
    }

    /// <summary>
    /// Returns the index of the last element that has all bits set, or -1 if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOfWhereAllBitsSet<T>(Vector512<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = Vector512<T>.Count - 1; i >= 0; --i)
        if (Scalar<T>.ObjectEquals(vector.GetElement(i), allSet))
          return i;
      return -1;
    }

    /// <summary>
    /// Creates a vector with sequential values starting from the specified start value and incrementing by the specified step.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> CreateSequence<T>(T start, T step) where T : struct {
      var result = Vector512<T>.Zero;
      var current = start;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        result = result.WithElement(i, current);
        current = Scalar<T>.Add(current, step);
      }
      return result;
    }

    // ===== NARROWWITHSATURATION METHODS =====

    /// <summary>Narrows two Vector512&lt;int&gt; to one Vector512&lt;short&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<short> NarrowWithSaturation(Vector512<int> lower, Vector512<int> upper) {
      var result = Vector512<short>.Zero;
      var lowerCount = Vector512<int>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = lower.GetElement(i);
        result = result.WithElement(i, (short)Math.Clamp(val, short.MinValue, short.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = upper.GetElement(i);
        result = result.WithElement(i + lowerCount, (short)Math.Clamp(val, short.MinValue, short.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector512&lt;uint&gt; to one Vector512&lt;ushort&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<ushort> NarrowWithSaturation(Vector512<uint> lower, Vector512<uint> upper) {
      var result = Vector512<ushort>.Zero;
      var lowerCount = Vector512<uint>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = lower.GetElement(i);
        result = result.WithElement(i, (ushort)Math.Min(val, ushort.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = upper.GetElement(i);
        result = result.WithElement(i + lowerCount, (ushort)Math.Min(val, ushort.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector512&lt;short&gt; to one Vector512&lt;sbyte&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<sbyte> NarrowWithSaturation(Vector512<short> lower, Vector512<short> upper) {
      var result = Vector512<sbyte>.Zero;
      var lowerCount = Vector512<short>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = lower.GetElement(i);
        result = result.WithElement(i, (sbyte)Math.Clamp(val, sbyte.MinValue, sbyte.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = upper.GetElement(i);
        result = result.WithElement(i + lowerCount, (sbyte)Math.Clamp(val, sbyte.MinValue, sbyte.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector512&lt;ushort&gt; to one Vector512&lt;byte&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<byte> NarrowWithSaturation(Vector512<ushort> lower, Vector512<ushort> upper) {
      var result = Vector512<byte>.Zero;
      var lowerCount = Vector512<ushort>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = lower.GetElement(i);
        result = result.WithElement(i, (byte)Math.Min(val, byte.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = upper.GetElement(i);
        result = result.WithElement(i + lowerCount, (byte)Math.Min(val, byte.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector512&lt;long&gt; to one Vector512&lt;int&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<int> NarrowWithSaturation(Vector512<long> lower, Vector512<long> upper) {
      var result = Vector512<int>.Zero;
      var lowerCount = Vector512<long>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = lower.GetElement(i);
        result = result.WithElement(i, (int)Math.Clamp(val, int.MinValue, int.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = upper.GetElement(i);
        result = result.WithElement(i + lowerCount, (int)Math.Clamp(val, int.MinValue, int.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector512&lt;ulong&gt; to one Vector512&lt;uint&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<uint> NarrowWithSaturation(Vector512<ulong> lower, Vector512<ulong> upper) {
      var result = Vector512<uint>.Zero;
      var lowerCount = Vector512<ulong>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = lower.GetElement(i);
        result = result.WithElement(i, (uint)Math.Min(val, uint.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = upper.GetElement(i);
        result = result.WithElement(i + lowerCount, (uint)Math.Min(val, uint.MaxValue));
      }
      return result;
    }

    // ===== WIDENLOWER METHODS =====

    /// <summary>Widens the lower half of a Vector512&lt;sbyte&gt; to Vector512&lt;short&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<short> WidenLower(Vector512<sbyte> source) {
      var result = Vector512<short>.Zero;
      var count = Vector512<short>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (short)source.GetElement(i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector512&lt;byte&gt; to Vector512&lt;ushort&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<ushort> WidenLower(Vector512<byte> source) {
      var result = Vector512<ushort>.Zero;
      var count = Vector512<ushort>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (ushort)source.GetElement(i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector512&lt;short&gt; to Vector512&lt;int&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<int> WidenLower(Vector512<short> source) {
      var result = Vector512<int>.Zero;
      var count = Vector512<int>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (int)source.GetElement(i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector512&lt;ushort&gt; to Vector512&lt;uint&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<uint> WidenLower(Vector512<ushort> source) {
      var result = Vector512<uint>.Zero;
      var count = Vector512<uint>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (uint)source.GetElement(i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector512&lt;int&gt; to Vector512&lt;long&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<long> WidenLower(Vector512<int> source) {
      var result = Vector512<long>.Zero;
      var count = Vector512<long>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (long)source.GetElement(i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector512&lt;uint&gt; to Vector512&lt;ulong&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<ulong> WidenLower(Vector512<uint> source) {
      var result = Vector512<ulong>.Zero;
      var count = Vector512<ulong>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (ulong)source.GetElement(i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector512&lt;float&gt; to Vector512&lt;double&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> WidenLower(Vector512<float> source) {
      var result = Vector512<double>.Zero;
      var count = Vector512<double>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (double)source.GetElement(i));
      return result;
    }

    // ===== WIDENUPPER METHODS =====

    /// <summary>Widens the upper half of a Vector512&lt;sbyte&gt; to Vector512&lt;short&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<short> WidenUpper(Vector512<sbyte> source) {
      var result = Vector512<short>.Zero;
      var count = Vector512<short>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (short)source.GetElement(i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector512&lt;byte&gt; to Vector512&lt;ushort&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<ushort> WidenUpper(Vector512<byte> source) {
      var result = Vector512<ushort>.Zero;
      var count = Vector512<ushort>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (ushort)source.GetElement(i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector512&lt;short&gt; to Vector512&lt;int&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<int> WidenUpper(Vector512<short> source) {
      var result = Vector512<int>.Zero;
      var count = Vector512<int>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (int)source.GetElement(i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector512&lt;ushort&gt; to Vector512&lt;uint&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<uint> WidenUpper(Vector512<ushort> source) {
      var result = Vector512<uint>.Zero;
      var count = Vector512<uint>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (uint)source.GetElement(i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector512&lt;int&gt; to Vector512&lt;long&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<long> WidenUpper(Vector512<int> source) {
      var result = Vector512<long>.Zero;
      var count = Vector512<long>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (long)source.GetElement(i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector512&lt;uint&gt; to Vector512&lt;ulong&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<ulong> WidenUpper(Vector512<uint> source) {
      var result = Vector512<ulong>.Zero;
      var count = Vector512<ulong>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (ulong)source.GetElement(i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector512&lt;float&gt; to Vector512&lt;double&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> WidenUpper(Vector512<float> source) {
      var result = Vector512<double>.Zero;
      var count = Vector512<double>.Count;
      for (var i = 0; i < count; ++i)
        result = result.WithElement(i, (double)source.GetElement(i + count));
      return result;
    }

  }
}

}


#if !FEATURE_VECTOR512STATIC_WAVE5

namespace System.Runtime.Intrinsics {

public static partial class Vector512AdvancedPolyfills {
  extension(Vector512) {

    // ===== SATURATION ARITHMETIC =====

    /// <summary>Adds two vectors with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> AddSaturate<T>(Vector512<T> left, Vector512<T> right) where T : struct {
      var result = Vector512<T>.Zero;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var l = left.GetElement(i);
        var r = right.GetElement(i);
        result = result.WithElement(i, Scalar<T>.AddSaturate(l, r));
      }
      return result;
    }

    /// <summary>Subtracts two vectors with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> SubtractSaturate<T>(Vector512<T> left, Vector512<T> right) where T : struct {
      var result = Vector512<T>.Zero;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var l = left.GetElement(i);
        var r = right.GetElement(i);
        result = result.WithElement(i, Scalar<T>.SubtractSaturate(l, r));
      }
      return result;
    }

    // ===== MIN/MAX MAGNITUDE =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> MinMagnitude(Vector512<float> x, Vector512<float> y) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, MathF.Abs(xv) < MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> MinMagnitude(Vector512<double> x, Vector512<double> y) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, Math.Abs(xv) < Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> MaxMagnitude(Vector512<float> x, Vector512<float> y) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, MathF.Abs(xv) > MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> MaxMagnitude(Vector512<double> x, Vector512<double> y) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, Math.Abs(xv) > Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> MinMagnitudeNumber(Vector512<float> x, Vector512<float> y) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        if (float.IsNaN(xv) || float.IsNaN(yv))
          result = result.WithElement(i, float.NaN);
        else
          result = result.WithElement(i, MathF.Abs(xv) < MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> MinMagnitudeNumber(Vector512<double> x, Vector512<double> y) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        if (double.IsNaN(xv) || double.IsNaN(yv))
          result = result.WithElement(i, double.NaN);
        else
          result = result.WithElement(i, Math.Abs(xv) < Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> MaxMagnitudeNumber(Vector512<float> x, Vector512<float> y) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        if (float.IsNaN(xv) || float.IsNaN(yv))
          result = result.WithElement(i, float.NaN);
        else
          result = result.WithElement(i, MathF.Abs(xv) > MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> MaxMagnitudeNumber(Vector512<double> x, Vector512<double> y) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        if (double.IsNaN(xv) || double.IsNaN(yv))
          result = result.WithElement(i, double.NaN);
        else
          result = result.WithElement(i, Math.Abs(xv) > Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> MinNumber(Vector512<float> x, Vector512<float> y) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, float.IsNaN(xv) ? yv : (float.IsNaN(yv) ? xv : MathF.Min(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> MinNumber(Vector512<double> x, Vector512<double> y) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, double.IsNaN(xv) ? yv : (double.IsNaN(yv) ? xv : Math.Min(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> MaxNumber(Vector512<float> x, Vector512<float> y) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, float.IsNaN(xv) ? yv : (float.IsNaN(yv) ? xv : MathF.Max(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> MaxNumber(Vector512<double> x, Vector512<double> y) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, double.IsNaN(xv) ? yv : (double.IsNaN(yv) ? xv : Math.Max(xv, yv)));
      }
      return result;
    }

    // ===== NATIVE CLAMP/MIN/MAX =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> ClampNative<T>(Vector512<T> value, Vector512<T> min, Vector512<T> max) where T : struct
      => Vector512.Clamp(value, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> MinNative<T>(Vector512<T> x, Vector512<T> y) where T : struct {
      var result = x;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var a = x.GetElement(i);
        var b = y.GetElement(i);
        if (Scalar<T>.LessThan(b, a))
          result = result.WithElement(i, b);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> MaxNative<T>(Vector512<T> x, Vector512<T> y) where T : struct {
      var result = x;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var a = x.GetElement(i);
        var b = y.GetElement(i);
        if (Scalar<T>.GreaterThan(b, a))
          result = result.WithElement(i, b);
      }
      return result;
    }

    // ===== FLOAT PREDICATES =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> IsFinite(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, float.IsFinite(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> IsFinite(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, !double.IsNaN(v) && !double.IsInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> IsInfinity(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, float.IsInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> IsInfinity(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, double.IsInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> IsNegativeInfinity(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, float.IsNegativeInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> IsNegativeInfinity(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, double.IsNegativeInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> IsPositiveInfinity(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, float.IsPositiveInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> IsPositiveInfinity(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, double.IsPositiveInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> IsNormal(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, float.IsNormal(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> IsNormal(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var v = vector.GetElement(i);
        var bits = BitConverter.DoubleToInt64Bits(v);
        var exponent = (int)((bits >> 52) & 0x7FF);
        var isNormal = exponent != 0 && exponent != 0x7FF && v != 0d;
        result = result.WithElement(i, isNormal ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> IsSubnormal(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var v = vector.GetElement(i);
        result = result.WithElement(i, float.IsSubnormal(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> IsSubnormal(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var v = vector.GetElement(i);
        var bits = BitConverter.DoubleToInt64Bits(v);
        var exponent = (int)((bits >> 52) & 0x7FF);
        var mantissa = bits & 0xFFFFFFFFFFFFF;
        var isSubnormal = exponent == 0 && mantissa != 0;
        result = result.WithElement(i, isSubnormal ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> IsOddInteger<T>(Vector512<T> vector) where T : struct {
      var result = Vector512<T>.Zero;
      var allBitsSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var v = vector.GetElement(i);
        var isOdd = (Convert.ToInt64(v) & 1) == 1;
        result = result.WithElement(i, isOdd ? allBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> IsEvenInteger<T>(Vector512<T> vector) where T : struct {
      var result = Vector512<T>.Zero;
      var allBitsSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var v = vector.GetElement(i);
        var isEven = (Convert.ToInt64(v) & 1) == 0;
        result = result.WithElement(i, isEven ? allBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    // ===== TRANSCENDENTAL FUNCTIONS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Sin(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i)
        result = result.WithElement(i, MathF.Sin(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Sin(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i)
        result = result.WithElement(i, Math.Sin(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Cos(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i)
        result = result.WithElement(i, MathF.Cos(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Cos(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i)
        result = result.WithElement(i, Math.Cos(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector512<float> Sin, Vector512<float> Cos) SinCos(Vector512<float> vector) {
      var sin = Vector512<float>.Zero;
      var cos = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var v = vector.GetElement(i);
        sin = sin.WithElement(i, MathF.Sin(v));
        cos = cos.WithElement(i, MathF.Cos(v));
      }
      return (sin, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector512<double> Sin, Vector512<double> Cos) SinCos(Vector512<double> vector) {
      var sin = Vector512<double>.Zero;
      var cos = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var v = vector.GetElement(i);
        sin = sin.WithElement(i, Math.Sin(v));
        cos = cos.WithElement(i, Math.Cos(v));
      }
      return (sin, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Exp(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i)
        result = result.WithElement(i, MathF.Exp(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Exp(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i)
        result = result.WithElement(i, Math.Exp(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Log(Vector512<float> vector) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i)
        result = result.WithElement(i, MathF.Log(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Log(Vector512<double> vector) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i)
        result = result.WithElement(i, Math.Log(vector.GetElement(i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<float> Hypot(Vector512<float> x, Vector512<float> y) {
      var result = Vector512<float>.Zero;
      for (var i = 0; i < Vector512<float>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, MathF.Sqrt(xv * xv + yv * yv));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> Hypot(Vector512<double> x, Vector512<double> y) {
      var result = Vector512<double>.Zero;
      for (var i = 0; i < Vector512<double>.Count; ++i) {
        var xv = x.GetElement(i);
        var yv = y.GetElement(i);
        result = result.WithElement(i, Math.Sqrt(xv * xv + yv * yv));
      }
      return result;
    }

    // ===== MULTIPLY-ADD =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<T> MultiplyAddEstimate<T>(Vector512<T> left, Vector512<T> right, Vector512<T> addend) where T : struct {
      var result = addend;
      for (var i = 0; i < Vector512<T>.Count; ++i) {
        var l = left.GetElement(i);
        var r = right.GetElement(i);
        var a = addend.GetElement(i);
        result = result.WithElement(i, Scalar<T>.Add(Scalar<T>.Multiply(l, r), a));
      }
      return result;
    }

  }
}

}
#endif
#endif
