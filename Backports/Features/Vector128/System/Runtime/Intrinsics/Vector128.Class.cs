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
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;

// Wave 1: Vector128 static class definition
#if !FEATURE_VECTOR128STATIC_WAVE1

namespace System.Runtime.Intrinsics {

/// <summary>
/// Provides a set of static methods for creating and working with 128-bit vectors.
/// </summary>
public static partial class Vector128 {
  internal const int Size = 16;
  internal const int Alignment = 16;

  private static void SkipInit<T>(out T result) => result = default!;

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

  extension<TFrom>(Vector128<TFrom> vector) where TFrom : struct
  {
    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<TTo> As<TTo>() where TTo : struct {
      Vector128<TFrom>.ThrowIfNotSupported();
      Vector128<TTo>.ThrowIfNotSupported();
      return Unsafe.As<Vector128<TFrom>, Vector128<TTo>>(ref vector);
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<byte> AsByte() => vector.As<TFrom, byte>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<double> AsDouble() => vector.As<TFrom, double>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<short> AsInt16() => vector.As<TFrom, short>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<int> AsInt32() => vector.As<TFrom, int>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<long> AsInt64() => vector.As<TFrom, long>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<nint> AsNInt() => vector.As<TFrom, nint>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<nuint> AsNUInt() => vector.As<TFrom, nuint>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<sbyte> AsSByte() => vector.As<TFrom, sbyte>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<float> AsSingle() => vector.As<TFrom, float>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<ushort> AsUInt16() => vector.As<TFrom, ushort>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<uint> AsUInt32() => vector.As<TFrom, uint>();

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<ulong> AsUInt64() => vector.As<TFrom, ulong>();
  }

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

  extension<T>(Vector128<T> vector) where T : struct
  {
    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> destination) {
      if (destination.Length < Vector128<T>.Count)
        AlwaysThrow.ArgumentException(nameof(destination), "Destination too short");

      for (var i = 0; i < Vector128<T>.Count; ++i)
        destination[i] = vector[i];
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] destination) {
      if (destination.Length < Vector128<T>.Count)
        AlwaysThrow.ArgumentException(nameof(destination), "Destination too short");

      for (var i = 0; i < Vector128<T>.Count; ++i)
        destination[i] = vector[i];
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] destination, int index) {
      if (destination.Length - index < Vector128<T>.Count)
        AlwaysThrow.ArgumentException(nameof(destination), "Destination too short");

      for (var i = 0; i < Vector128<T>.Count; ++i)
        destination[index + i] = vector[i];
    }
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> CreateScalar<T>(T value) where T : struct {
    SkipInit(out Vector128<T> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> CreateScalarUnsafe(byte value) {
    SkipInit(out Vector128<byte> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> CreateScalarUnsafe(sbyte value) {
    SkipInit(out Vector128<sbyte> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> CreateScalarUnsafe(short value) {
    SkipInit(out Vector128<short> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> CreateScalarUnsafe(ushort value) {
    SkipInit(out Vector128<ushort> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> CreateScalarUnsafe(int value) {
    SkipInit(out Vector128<int> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> CreateScalarUnsafe(uint value) {
    SkipInit(out Vector128<uint> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> CreateScalarUnsafe(long value) {
    SkipInit(out Vector128<long> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> CreateScalarUnsafe(ulong value) {
    SkipInit(out Vector128<ulong> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> CreateScalarUnsafe(float value) {
    SkipInit(out Vector128<float> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CreateScalarUnsafe(double value) {
    SkipInit(out Vector128<double> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<nint> CreateScalarUnsafe(nint value) {
    SkipInit(out Vector128<nint> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<nuint> CreateScalarUnsafe(nuint value) {
    SkipInit(out Vector128<nuint> result);
    result.SetElementUnsafe(0, value);
    return result;
  }

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

  extension<T>(Vector128<T> vector) where T : struct
  {
    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public T GetElement(int index) => vector.GetElementUnsafe(index);
  }

  extension<T>(in Vector128<T> vector) where T : struct
  {
    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    internal T GetElementUnsafe(int index) {
      ref var address = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      return Unsafe.Add(ref address, index);
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    internal void SetElementUnsafe(int index, T value) {
      ref var address = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      Unsafe.Add(ref address, index) = value;
    }
  }

  extension<T>(Vector128<T> vector) where T : struct
  {
    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector64<T> GetLower() {
      Vector128<T>.ThrowIfNotSupported();
      return Unsafe.As<Vector128<T>, Vector64<T>>(ref vector);
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector64<T> GetUpper() {
      Vector128<T>.ThrowIfNotSupported();
      ref var upper = ref Unsafe.Add(ref Unsafe.As<Vector128<T>, Vector64<T>>(ref vector), 1);
      return upper;
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<T> WithLower(Vector64<T> value) {
      Vector128<T>.ThrowIfNotSupported();
      var lowerVal = Unsafe.As<Vector64<T>, ulong>(ref value);
      var upperVal = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref vector).Item2;
      return new(lowerVal, upperVal);
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<T> WithUpper(Vector64<T> value) {
      Vector128<T>.ThrowIfNotSupported();
      var lowerVal = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref vector).Item1;
      var upperVal = Unsafe.As<Vector64<T>, ulong>(ref value);
      return new(lowerVal, upperVal);
    }
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
  extension<T>(Vector128<T> vector) where T : struct
  {
    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public T ToScalar() => vector.GetElementUnsafe(0);

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector256<T> ToVector256() {
      Vector128<T>.ThrowIfNotSupported();
      var bits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref vector);
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref Unsafe.AsRef((bits.Item1, bits.Item2, 0UL, 0UL)));
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector256<T> ToVector256Unsafe() {
      Vector128<T>.ThrowIfNotSupported();
      SkipInit(out Vector256<T> result);
      Unsafe.As<Vector256<T>, Vector128<T>>(ref result) = vector;
      return result;
    }

    [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
    public Vector128<T> WithElement(int index, T value) {
      if ((uint)index >= Vector128<T>.Count)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

      var result = vector;
      result.SetElementUnsafe(index, value);
      return result;
    }
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> Xor<T>(Vector128<T> left, Vector128<T> right) where T : struct => left ^ right;
}

}
#endif

// Wave 3: Load/Store and Create methods
#if !FEATURE_VECTOR128STATIC_WAVE3

namespace System.Runtime.Intrinsics {

#if !FEATURE_VECTOR128STATIC_WAVE1
public static partial class Vector128 {
#else
public static partial class Vector128Polyfills {
  extension(Vector128) {
    private static void SkipInit<T>(out T result) => result = default!;
    /// <summary>Gets a value that indicates whether 128-bit vector operations are subject to hardware acceleration.</summary>
    public static bool IsHardwareAccelerated => System.Numerics.Vector.IsHardwareAccelerated && System.Numerics.Vector<byte>.Count >= 16;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<T> Load<T>(T* source) where T : unmanaged
      => Unsafe.ReadUnaligned<Vector128<T>>(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector128<T> LoadUnsafe<T>(ref T source) where T : struct
      => Unsafe.ReadUnaligned<Vector128<T>>(ref Unsafe.As<T, byte>(ref source));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Store<T>(Vector128<T> source, T* destination) where T : unmanaged
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreUnsafe<T>(Vector128<T> source, ref T destination) where T : struct
      => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAligned<T>(Vector128<T> source, T* destination) where T : unmanaged
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAlignedNonTemporal<T>(Vector128<T> source, T* destination) where T : unmanaged
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7, byte e8, byte e9, byte e10, byte e11, byte e12, byte e13, byte e14, byte e15) {
      unsafe {
        byte* ptr = stackalloc byte[16];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        return Unsafe.ReadUnaligned<Vector128<byte>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Create(float e0, float e1, float e2, float e3) {
      unsafe {
        float* ptr = stackalloc float[4];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3;
        return Unsafe.ReadUnaligned<Vector128<float>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Create(double e0, double e1) {
      unsafe {
        double* ptr = stackalloc double[2];
        ptr[0] = e0; ptr[1] = e1;
        return Unsafe.ReadUnaligned<Vector128<double>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> Create(long e0, long e1) {
      unsafe {
        long* ptr = stackalloc long[2];
        ptr[0] = e0; ptr[1] = e1;
        return Unsafe.ReadUnaligned<Vector128<long>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> Create(ulong e0, ulong e1) {
      unsafe {
        ulong* ptr = stackalloc ulong[2];
        ptr[0] = e0; ptr[1] = e1;
        return Unsafe.ReadUnaligned<Vector128<ulong>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> Create(int e0, int e1, int e2, int e3) {
      unsafe {
        int* ptr = stackalloc int[4];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3;
        return Unsafe.ReadUnaligned<Vector128<int>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> Create(uint e0, uint e1, uint e2, uint e3) {
      unsafe {
        uint* ptr = stackalloc uint[4];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3;
        return Unsafe.ReadUnaligned<Vector128<uint>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> Create(short e0, short e1, short e2, short e3, short e4, short e5, short e6, short e7) {
      unsafe {
        short* ptr = stackalloc short[8];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        return Unsafe.ReadUnaligned<Vector128<short>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> Create(ushort e0, ushort e1, ushort e2, ushort e3, ushort e4, ushort e5, ushort e6, ushort e7) {
      unsafe {
        ushort* ptr = stackalloc ushort[8];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        return Unsafe.ReadUnaligned<Vector128<ushort>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7, sbyte e8, sbyte e9, sbyte e10, sbyte e11, sbyte e12, sbyte e13, sbyte e14, sbyte e15) {
      unsafe {
        sbyte* ptr = stackalloc sbyte[16];
        ptr[0] = e0; ptr[1] = e1; ptr[2] = e2; ptr[3] = e3; ptr[4] = e4; ptr[5] = e5; ptr[6] = e6; ptr[7] = e7;
        ptr[8] = e8; ptr[9] = e9; ptr[10] = e10; ptr[11] = e11; ptr[12] = e12; ptr[13] = e13; ptr[14] = e14; ptr[15] = e15;
        return Unsafe.ReadUnaligned<Vector128<sbyte>>(ptr);
      }
    }

#if !FEATURE_VECTOR128STATIC_WAVE1
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> CreateScalar<T>(T value) where T : struct {
      var result = Vector128<T>.Zero;
      unsafe {
        Unsafe.Write(Unsafe.AsPointer(ref Unsafe.AsRef(in result)), value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> CreateScalarUnsafe<T>(T value) where T : struct {
      return CreateScalar(value);
    }
#endif

    // Only add these methods when BCL has Vector128 but lacks these operations
    #if FEATURE_VECTOR128STATIC_WAVE1

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Create<T>(T value) where T : struct {
      var count = 16 / Unsafe.SizeOf<T>();
      unsafe {
        var buffer = stackalloc byte[16];
        var ptr = (T*)buffer;
        for (var i = 0; i < count; ++i)
          ptr[i] = value;
        return Unsafe.ReadUnaligned<Vector128<T>>(buffer);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Add<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Add(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Subtract<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Subtract(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Multiply<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Divide<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Divide(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Negate<T>(Vector128<T> vector) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Negate(Unsafe.Add(ref rVector, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Abs<T>(Vector128<T> vector) where T : struct {
      if (Scalar<T>.IsUnsigned)
        return vector;

      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Abs(Unsafe.Add(ref rVector, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Sqrt<T>(Vector128<T> vector) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Sqrt(Unsafe.Add(ref rVector, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(Vector128<T> vector) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Scalar<T>.Zero();
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));

      for (int i = 0; i < count; ++i)
        result = Scalar<T>.Add(result, Unsafe.Add(ref rVector, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Scalar<T>.Zero();
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));

      for (int i = 0; i < count; ++i) {
        var product = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
        result = Scalar<T>.Add(result, product);
      }

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> BitwiseAnd<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var res = (leftBits.Item1 & rightBits.Item1, leftBits.Item2 & rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> BitwiseOr<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var res = (leftBits.Item1 | rightBits.Item1, leftBits.Item2 | rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Xor<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var res = (leftBits.Item1 ^ rightBits.Item1, leftBits.Item2 ^ rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> OnesComplement<T>(Vector128<T> vector) where T : struct {
      var bits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref vector);
      var res = (~bits.Item1, ~bits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> AndNot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var res = (leftBits.Item1 & ~rightBits.Item1, leftBits.Item2 & ~rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Equals<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var eq = Scalar<T>.ObjectEquals(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
        Unsafe.Add(ref rRes, i) = eq ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
      }

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GreaterThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var gt = Scalar<T>.GreaterThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
        Unsafe.Add(ref rRes, i) = gt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
      }

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GreaterThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var ge = Scalar<T>.GreaterThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
        Unsafe.Add(ref rRes, i) = ge ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
      }

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> LessThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var lt = Scalar<T>.LessThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
        Unsafe.Add(ref rRes, i) = lt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
      }

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> LessThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var l = Unsafe.Add(ref rLeft, i);
        var r = Unsafe.Add(ref rRight, i);
        var le = Scalar<T>.LessThanOrEqual(l, r);
        Unsafe.Add(ref rRes, i) = le ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Min<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Min(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Max<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Max(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Floor<T>(Vector128<T> vector) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Floor(Unsafe.Add(ref rVector, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Ceiling<T>(Vector128<T> vector) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i)
        Unsafe.Add(ref rRes, i) = Scalar<T>.Ceiling(Unsafe.Add(ref rVector, i));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetElement<T>(Vector128<T> vector, int index) where T : struct {
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)(16 / Unsafe.SizeOf<T>()), nameof(index));

      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      return Unsafe.Add(ref rVector, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> WithElement<T>(Vector128<T> vector, int index, T value) where T : struct {
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)(16 / Unsafe.SizeOf<T>()), nameof(index));

      var result = vector;
      ref var rResult = ref Unsafe.As<Vector128<T>, T>(ref result);
      Unsafe.Add(ref rResult, index) = value;
      return result;
    }

    #endif

    // GreaterThanOrEqual and LessThanOrEqual: only when we define Vector128 ourselves
    #if !FEATURE_VECTOR128STATIC_WAVE1

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GreaterThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var ge = Scalar<T>.GreaterThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
        Unsafe.Add(ref rRes, i) = ge ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
      }

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> LessThanOrEqual<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rLeft = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in left));
      ref var rRight = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in right));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var le = Scalar<T>.LessThanOrEqual(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
        Unsafe.Add(ref rRes, i) = le ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
      }

      return result;
    }

    #endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Shuffle<T>(Vector128<T> vector, Vector128<T> indices) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rIndices = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in indices));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        var idxVal = Unsafe.Add(ref rIndices, i);
        int idx = Scalar<T>.ToInt32(idxVal) & (count - 1);
        var val = Unsafe.Add(ref rVector, idx);
        Unsafe.Add(ref rRes, i) = val;
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShuffleNative<T>(Vector128<T> vector, Vector128<T> indices) where T : struct => Shuffle(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits<T>(Vector128<T> vector) where T : struct {
      uint result = 0;
      int count = 16 / Unsafe.SizeOf<T>();
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));

      for (var i = 0; i < count; ++i) {
        if (Scalar<T>.ExtractMostSignificantBit(Unsafe.Add(ref rVector, i)))
          result |= 1u << i;
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ConditionalSelect<T>(Vector128<T> condition, Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var condBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref condition);

      var res = (
        (condBits.Item1 & leftBits.Item1) | (~condBits.Item1 & rightBits.Item1),
        (condBits.Item2 & leftBits.Item2) | (~condBits.Item2 & rightBits.Item2)
      );
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftLeft<T>(Vector128<T> vector, int shiftCount) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftLeft(Unsafe.Add(ref rVector, i), shiftCount);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftRightArithmetic<T>(Vector128<T> vector, int shiftCount) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftRightArithmetic(Unsafe.Add(ref rVector, i), shiftCount);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftRightLogical<T>(Vector128<T> vector, int shiftCount) where T : struct {
      int count = 16 / Unsafe.SizeOf<T>();
      var result = Vector128<T>.Zero;
      ref var rVector = ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector));
      ref var rRes = ref Unsafe.As<Vector128<T>, T>(ref result);

      for (int i = 0; i < count; ++i) {
        Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftRightLogical(Unsafe.Add(ref rVector, i), shiftCount);
      }
      return result;
    }

    // ===== COMPARISON ALL/ANY METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.ObjectEquals(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      for (var i = 0; i < 16 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThanOrEqual(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    // ===== TOSCALAR (for BCL case) =====

#if FEATURE_VECTOR128STATIC_WAVE1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToScalar<T>(Vector128<T> vector) where T : struct
      => Vector128.GetElement(vector, 0);
#endif

    // ===== COPYSIGN =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> CopySign(Vector128<float> value, Vector128<float> sign) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      for (var i = 0; i < count; ++i) {
        var v = Vector128.GetElement(value, i);
        var s = Vector128.GetElement(sign, i);
        var absV = MathF.Abs(v);
        result = Vector128.WithElement(result, i, s < 0 ? -absV : absV);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> CopySign(Vector128<double> value, Vector128<double> sign) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      for (var i = 0; i < count; ++i) {
        var v = Vector128.GetElement(value, i);
        var s = Vector128.GetElement(sign, i);
        var absV = Math.Abs(v);
        result = Vector128.WithElement(result, i, s < 0 ? -absV : absV);
      }
      return result;
    }

    // ===== CLASSIFICATION METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsNaN(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isNan = float.IsNaN(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, isNan ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsNaN(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var isNan = double.IsNaN(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, isNan ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsNegative(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isNeg = val < 0 || (val == 0 && float.IsNegativeInfinity(1f / val));
        result = Vector128.WithElement(result, i, isNeg ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsNegative(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isNeg = val < 0 || (val == 0 && double.IsNegativeInfinity(1d / val));
        result = Vector128.WithElement(result, i, isNeg ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsPositive(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isPos = val > 0 || (val == 0 && float.IsPositiveInfinity(1f / val));
        result = Vector128.WithElement(result, i, isPos ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsPositive(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var isPos = val > 0 || (val == 0 && double.IsPositiveInfinity(1d / val));
        result = Vector128.WithElement(result, i, isPos ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsZero(Vector128<float> vector) {
      var count = 16 / sizeof(float);
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isZero = Vector128.GetElement(vector, i) == 0f;
        result = Vector128.WithElement(result, i, isZero ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsZero(Vector128<double> vector) {
      var count = 16 / sizeof(double);
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var isZero = Vector128.GetElement(vector, i) == 0d;
        result = Vector128.WithElement(result, i, isZero ? allBitsSet : 0d);
      }
      return result;
    }

    // ===== NARROW METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Narrow(Vector128<double> lower, Vector128<double> upper) {
      var result = Vector128<float>.Zero;
      result = Vector128.WithElement(result, 0, (float)Vector128.GetElement(lower, 0));
      result = Vector128.WithElement(result, 1, (float)Vector128.GetElement(lower, 1));
      result = Vector128.WithElement(result, 2, (float)Vector128.GetElement(upper, 0));
      result = Vector128.WithElement(result, 3, (float)Vector128.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> Narrow(Vector128<long> lower, Vector128<long> upper) {
      var result = Vector128<int>.Zero;
      result = Vector128.WithElement(result, 0, (int)Vector128.GetElement(lower, 0));
      result = Vector128.WithElement(result, 1, (int)Vector128.GetElement(lower, 1));
      result = Vector128.WithElement(result, 2, (int)Vector128.GetElement(upper, 0));
      result = Vector128.WithElement(result, 3, (int)Vector128.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> Narrow(Vector128<ulong> lower, Vector128<ulong> upper) {
      var result = Vector128<uint>.Zero;
      result = Vector128.WithElement(result, 0, (uint)Vector128.GetElement(lower, 0));
      result = Vector128.WithElement(result, 1, (uint)Vector128.GetElement(lower, 1));
      result = Vector128.WithElement(result, 2, (uint)Vector128.GetElement(upper, 0));
      result = Vector128.WithElement(result, 3, (uint)Vector128.GetElement(upper, 1));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> Narrow(Vector128<int> lower, Vector128<int> upper) {
      var result = Vector128<short>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector128.WithElement(result, i, (short)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 4, (short)Vector128.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> Narrow(Vector128<uint> lower, Vector128<uint> upper) {
      var result = Vector128<ushort>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector128.WithElement(result, i, (ushort)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 4, (ushort)Vector128.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> Narrow(Vector128<short> lower, Vector128<short> upper) {
      var result = Vector128<sbyte>.Zero;
      for (var i = 0; i < 8; ++i) {
        result = Vector128.WithElement(result, i, (sbyte)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 8, (sbyte)Vector128.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Narrow(Vector128<ushort> lower, Vector128<ushort> upper) {
      var result = Vector128<byte>.Zero;
      for (var i = 0; i < 8; ++i) {
        result = Vector128.WithElement(result, i, (byte)Vector128.GetElement(lower, i));
        result = Vector128.WithElement(result, i + 8, (byte)Vector128.GetElement(upper, i));
      }
      return result;
    }

    // ===== WIDEN METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<double> Lower, Vector128<double> Upper) Widen(Vector128<float> vector) {
      var lower = Vector128<double>.Zero;
      var upper = Vector128<double>.Zero;
      lower = Vector128.WithElement(lower, 0, (double)Vector128.GetElement(vector, 0));
      lower = Vector128.WithElement(lower, 1, (double)Vector128.GetElement(vector, 1));
      upper = Vector128.WithElement(upper, 0, (double)Vector128.GetElement(vector, 2));
      upper = Vector128.WithElement(upper, 1, (double)Vector128.GetElement(vector, 3));
      return (lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<long> Lower, Vector128<long> Upper) Widen(Vector128<int> vector) {
      var lower = Vector128<long>.Zero;
      var upper = Vector128<long>.Zero;
      lower = Vector128.WithElement(lower, 0, (long)Vector128.GetElement(vector, 0));
      lower = Vector128.WithElement(lower, 1, (long)Vector128.GetElement(vector, 1));
      upper = Vector128.WithElement(upper, 0, (long)Vector128.GetElement(vector, 2));
      upper = Vector128.WithElement(upper, 1, (long)Vector128.GetElement(vector, 3));
      return (lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<ulong> Lower, Vector128<ulong> Upper) Widen(Vector128<uint> vector) {
      var lower = Vector128<ulong>.Zero;
      var upper = Vector128<ulong>.Zero;
      lower = Vector128.WithElement(lower, 0, (ulong)Vector128.GetElement(vector, 0));
      lower = Vector128.WithElement(lower, 1, (ulong)Vector128.GetElement(vector, 1));
      upper = Vector128.WithElement(upper, 0, (ulong)Vector128.GetElement(vector, 2));
      upper = Vector128.WithElement(upper, 1, (ulong)Vector128.GetElement(vector, 3));
      return (lower, upper);
    }

    // ===== CONVERTTO METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> ConvertToInt32(Vector128<float> vector) {
      var result = Vector128<int>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (int)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> ConvertToSingle(Vector128<int> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (float)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> ConvertToSingle(Vector128<uint> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (float)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> ConvertToInt64(Vector128<double> vector) {
      var result = Vector128<long>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (long)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> ConvertToDouble(Vector128<long> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (double)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> ConvertToDouble(Vector128<ulong> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (double)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> ConvertToUInt32(Vector128<float> vector) {
      var result = Vector128<uint>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (uint)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> ConvertToUInt64(Vector128<double> vector) {
      var result = Vector128<ulong>.Zero;
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (ulong)Vector128.GetElement(vector, i));
      return result;
    }

#if FEATURE_VECTOR128STATIC_WAVE1
  }

  // Extension operators for Vector128<T>
  extension<T>(Vector128<T>) where T : struct {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator +(Vector128<T> left, Vector128<T> right) => Vector128.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator -(Vector128<T> left, Vector128<T> right) => Vector128.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator *(Vector128<T> left, Vector128<T> right) => Vector128.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator /(Vector128<T> left, Vector128<T> right) => Vector128.Divide(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator -(Vector128<T> vector) => Vector128.Negate(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator &(Vector128<T> left, Vector128<T> right) => Vector128.BitwiseAnd(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator |(Vector128<T> left, Vector128<T> right) => Vector128.BitwiseOr(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator ^(Vector128<T> left, Vector128<T> right) => Vector128.Xor(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator ~(Vector128<T> vector) => Vector128.OnesComplement(vector);
  }
}
#else
}
#endif

}
#endif

// Wave 5: Advanced polyfills
#if !FEATURE_VECTOR128STATIC_WAVE5

namespace System.Runtime.Intrinsics {

public static partial class Vector128Polyfills {
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

    // ===== SATURATION ARITHMETIC =====

    /// <summary>Adds two vectors with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> AddSaturate<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var result = Vector128<T>.Zero;
      for (var i = 0; i < Vector128<T>.Count; ++i) {
        var l = Vector128.GetElement(left, i);
        var r = Vector128.GetElement(right, i);
        result = Vector128.WithElement(result, i, Scalar<T>.AddSaturate(l, r));
      }
      return result;
    }

    /// <summary>Subtracts two vectors with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> SubtractSaturate<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var result = Vector128<T>.Zero;
      for (var i = 0; i < Vector128<T>.Count; ++i) {
        var l = Vector128.GetElement(left, i);
        var r = Vector128.GetElement(right, i);
        result = Vector128.WithElement(result, i, Scalar<T>.SubtractSaturate(l, r));
      }
      return result;
    }

    // ===== MIN/MAX MAGNITUDE =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> MinMagnitude(Vector128<float> x, Vector128<float> y) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, MathF.Abs(xv) < MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> MinMagnitude(Vector128<double> x, Vector128<double> y) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, Math.Abs(xv) < Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> MaxMagnitude(Vector128<float> x, Vector128<float> y) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, MathF.Abs(xv) > MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> MaxMagnitude(Vector128<double> x, Vector128<double> y) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, Math.Abs(xv) > Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> MinMagnitudeNumber(Vector128<float> x, Vector128<float> y) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        if (float.IsNaN(xv) || float.IsNaN(yv))
          result = Vector128.WithElement(result, i, float.NaN);
        else
          result = Vector128.WithElement(result, i, MathF.Abs(xv) < MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> MinMagnitudeNumber(Vector128<double> x, Vector128<double> y) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        if (double.IsNaN(xv) || double.IsNaN(yv))
          result = Vector128.WithElement(result, i, double.NaN);
        else
          result = Vector128.WithElement(result, i, Math.Abs(xv) < Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> MaxMagnitudeNumber(Vector128<float> x, Vector128<float> y) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        if (float.IsNaN(xv) || float.IsNaN(yv))
          result = Vector128.WithElement(result, i, float.NaN);
        else
          result = Vector128.WithElement(result, i, MathF.Abs(xv) > MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> MaxMagnitudeNumber(Vector128<double> x, Vector128<double> y) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        if (double.IsNaN(xv) || double.IsNaN(yv))
          result = Vector128.WithElement(result, i, double.NaN);
        else
          result = Vector128.WithElement(result, i, Math.Abs(xv) > Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> MinNumber(Vector128<float> x, Vector128<float> y) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, float.IsNaN(xv) ? yv : (float.IsNaN(yv) ? xv : MathF.Min(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> MinNumber(Vector128<double> x, Vector128<double> y) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, double.IsNaN(xv) ? yv : (double.IsNaN(yv) ? xv : Math.Min(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> MaxNumber(Vector128<float> x, Vector128<float> y) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, float.IsNaN(xv) ? yv : (float.IsNaN(yv) ? xv : MathF.Max(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> MaxNumber(Vector128<double> x, Vector128<double> y) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, double.IsNaN(xv) ? yv : (double.IsNaN(yv) ? xv : Math.Max(xv, yv)));
      }
      return result;
    }

    // ===== NATIVE CLAMP/MIN/MAX =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ClampNative<T>(Vector128<T> value, Vector128<T> min, Vector128<T> max) where T : struct
      => Vector128.Min(Vector128.Max(value, min), max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> MinNative<T>(Vector128<T> x, Vector128<T> y) where T : struct => Vector128.Min(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> MaxNative<T>(Vector128<T> x, Vector128<T> y) where T : struct => Vector128.Max(x, y);

    // ===== FLOAT PREDICATES =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsFinite(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, float.IsFinite(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsFinite(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, !double.IsNaN(v) && !double.IsInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsInfinity(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, float.IsInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsInfinity(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, double.IsInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsNegativeInfinity(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, float.IsNegativeInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsNegativeInfinity(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, double.IsNegativeInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsPositiveInfinity(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, float.IsPositiveInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsPositiveInfinity(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, double.IsPositiveInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsNormal(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, float.IsNormal(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsNormal(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        var bits = BitConverter.DoubleToInt64Bits(v);
        var exponent = (int)((bits >> 52) & 0x7FF);
        var isNormal = exponent != 0 && exponent != 0x7FF && v != 0d;
        result = Vector128.WithElement(result, i, isNormal ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> IsSubnormal(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        result = Vector128.WithElement(result, i, float.IsSubnormal(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> IsSubnormal(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        var bits = BitConverter.DoubleToInt64Bits(v);
        var exponent = (int)((bits >> 52) & 0x7FF);
        var mantissa = bits & 0xFFFFFFFFFFFFF;
        var isSubnormal = exponent == 0 && mantissa != 0;
        result = Vector128.WithElement(result, i, isSubnormal ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> IsOddInteger<T>(Vector128<T> vector) where T : struct {
      var result = Vector128<T>.Zero;
      var allBitsSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector128<T>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        var isOdd = (Convert.ToInt64(v) & 1) == 1;
        result = Vector128.WithElement(result, i, isOdd ? allBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> IsEvenInteger<T>(Vector128<T> vector) where T : struct {
      var result = Vector128<T>.Zero;
      var allBitsSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector128<T>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        var isEven = (Convert.ToInt64(v) & 1) == 0;
        result = Vector128.WithElement(result, i, isEven ? allBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    // ===== TRANSCENDENTAL FUNCTIONS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Sin(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i)
        result = Vector128.WithElement(result, i, MathF.Sin(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Sin(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i)
        result = Vector128.WithElement(result, i, Math.Sin(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Cos(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i)
        result = Vector128.WithElement(result, i, MathF.Cos(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Cos(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i)
        result = Vector128.WithElement(result, i, Math.Cos(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<float> Sin, Vector128<float> Cos) SinCos(Vector128<float> vector) {
      var sin = Vector128<float>.Zero;
      var cos = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        sin = Vector128.WithElement(sin, i, MathF.Sin(v));
        cos = Vector128.WithElement(cos, i, MathF.Cos(v));
      }
      return (sin, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector128<double> Sin, Vector128<double> Cos) SinCos(Vector128<double> vector) {
      var sin = Vector128<double>.Zero;
      var cos = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var v = Vector128.GetElement(vector, i);
        sin = Vector128.WithElement(sin, i, Math.Sin(v));
        cos = Vector128.WithElement(cos, i, Math.Cos(v));
      }
      return (sin, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Exp(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i)
        result = Vector128.WithElement(result, i, MathF.Exp(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Exp(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i)
        result = Vector128.WithElement(result, i, Math.Exp(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Log(Vector128<float> vector) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i)
        result = Vector128.WithElement(result, i, MathF.Log(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Log(Vector128<double> vector) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i)
        result = Vector128.WithElement(result, i, Math.Log(Vector128.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Hypot(Vector128<float> x, Vector128<float> y) {
      var result = Vector128<float>.Zero;
      for (var i = 0; i < Vector128<float>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, MathF.Sqrt(xv * xv + yv * yv));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Hypot(Vector128<double> x, Vector128<double> y) {
      var result = Vector128<double>.Zero;
      for (var i = 0; i < Vector128<double>.Count; ++i) {
        var xv = Vector128.GetElement(x, i);
        var yv = Vector128.GetElement(y, i);
        result = Vector128.WithElement(result, i, Math.Sqrt(xv * xv + yv * yv));
      }
      return result;
    }

    // ===== MULTIPLY-ADD =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> MultiplyAddEstimate<T>(Vector128<T> left, Vector128<T> right, Vector128<T> addend) where T : struct
      => Vector128.Add(Vector128.Multiply(left, right), addend);


    /// <summary>
    /// Checks if all elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool All<T>(Vector128<T> vector, T value) where T : struct {
      for (var i = 0; i < Vector128<T>.Count; ++i)
        if (!Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), value))
          return false;
      return true;
    }

    /// <summary>
    /// Checks if any element in the vector equals the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Any<T>(Vector128<T> vector, T value) where T : struct {
      for (var i = 0; i < Vector128<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), value))
          return true;
      return false;
    }

    /// <summary>
    /// Checks if no elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool None<T>(Vector128<T> vector, T value) where T : struct => !Any(vector, value);

    /// <summary>
    /// Checks if all elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllWhereAllBitsSet<T>(Vector128<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector128<T>.Count; ++i)
        if (!Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), allSet))
          return false;
      return true;
    }

    /// <summary>
    /// Checks if any element in the vector has all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyWhereAllBitsSet<T>(Vector128<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector128<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), allSet))
          return true;
      return false;
    }

    /// <summary>
    /// Checks if no elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NoneWhereAllBitsSet<T>(Vector128<T> vector) where T : struct => !AnyWhereAllBitsSet(vector);

    /// <summary>
    /// Counts how many elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count<T>(Vector128<T> vector, T value) where T : struct {
      var count = 0;
      for (var i = 0; i < Vector128<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), value))
          ++count;
      return count;
    }

    /// <summary>
    /// Counts how many elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountWhereAllBitsSet<T>(Vector128<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      var count = 0;
      for (var i = 0; i < Vector128<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), allSet))
          ++count;
      return count;
    }

    /// <summary>
    /// Returns the index of the first element that has all bits set, or -1 if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfWhereAllBitsSet<T>(Vector128<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector128<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), allSet))
          return i;
      return -1;
    }

    /// <summary>
    /// Returns the index of the last element that has all bits set, or -1 if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOfWhereAllBitsSet<T>(Vector128<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = Vector128<T>.Count - 1; i >= 0; --i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(vector, i), allSet))
          return i;
      return -1;
    }

    /// <summary>
    /// Creates a vector with sequential values starting from the specified start value and incrementing by the specified step.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> CreateSequence<T>(T start, T step) where T : struct {
      var result = Vector128<T>.Zero;
      var current = start;
      for (var i = 0; i < Vector128<T>.Count; ++i) {
        result = Vector128.WithElement(result, i, current);
        current = Scalar<T>.Add(current, step);
      }
      return result;
    }

    // ===== NARROWWITHSATURATION METHODS =====

    /// <summary>Narrows two Vector128&lt;int&gt; to one Vector128&lt;short&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> NarrowWithSaturation(Vector128<int> lower, Vector128<int> upper) {
      var result = Vector128<short>.Zero;
      var lowerCount = Vector128<int>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(lower, i);
        result = Vector128.WithElement(result, i, (short)Math.Clamp(val, short.MinValue, short.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(upper, i);
        result = Vector128.WithElement(result, i + lowerCount, (short)Math.Clamp(val, short.MinValue, short.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector128&lt;uint&gt; to one Vector128&lt;ushort&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> NarrowWithSaturation(Vector128<uint> lower, Vector128<uint> upper) {
      var result = Vector128<ushort>.Zero;
      var lowerCount = Vector128<uint>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(lower, i);
        result = Vector128.WithElement(result, i, (ushort)Math.Min(val, ushort.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(upper, i);
        result = Vector128.WithElement(result, i + lowerCount, (ushort)Math.Min(val, ushort.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector128&lt;short&gt; to one Vector128&lt;sbyte&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> NarrowWithSaturation(Vector128<short> lower, Vector128<short> upper) {
      var result = Vector128<sbyte>.Zero;
      var lowerCount = Vector128<short>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(lower, i);
        result = Vector128.WithElement(result, i, (sbyte)Math.Clamp(val, sbyte.MinValue, sbyte.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(upper, i);
        result = Vector128.WithElement(result, i + lowerCount, (sbyte)Math.Clamp(val, sbyte.MinValue, sbyte.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector128&lt;ushort&gt; to one Vector128&lt;byte&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> NarrowWithSaturation(Vector128<ushort> lower, Vector128<ushort> upper) {
      var result = Vector128<byte>.Zero;
      var lowerCount = Vector128<ushort>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(lower, i);
        result = Vector128.WithElement(result, i, (byte)Math.Min(val, byte.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(upper, i);
        result = Vector128.WithElement(result, i + lowerCount, (byte)Math.Min(val, byte.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector128&lt;long&gt; to one Vector128&lt;int&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> NarrowWithSaturation(Vector128<long> lower, Vector128<long> upper) {
      var result = Vector128<int>.Zero;
      var lowerCount = Vector128<long>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(lower, i);
        result = Vector128.WithElement(result, i, (int)Math.Clamp(val, int.MinValue, int.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(upper, i);
        result = Vector128.WithElement(result, i + lowerCount, (int)Math.Clamp(val, int.MinValue, int.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector128&lt;ulong&gt; to one Vector128&lt;uint&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> NarrowWithSaturation(Vector128<ulong> lower, Vector128<ulong> upper) {
      var result = Vector128<uint>.Zero;
      var lowerCount = Vector128<ulong>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(lower, i);
        result = Vector128.WithElement(result, i, (uint)Math.Min(val, uint.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector128.GetElement(upper, i);
        result = Vector128.WithElement(result, i + lowerCount, (uint)Math.Min(val, uint.MaxValue));
      }
      return result;
    }

    // ===== WIDENLOWER METHODS =====

    /// <summary>Widens the lower half of a Vector128&lt;sbyte&gt; to Vector128&lt;short&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> WidenLower(Vector128<sbyte> source) {
      var result = Vector128<short>.Zero;
      var count = Vector128<short>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (short)Vector128.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector128&lt;byte&gt; to Vector128&lt;ushort&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> WidenLower(Vector128<byte> source) {
      var result = Vector128<ushort>.Zero;
      var count = Vector128<ushort>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (ushort)Vector128.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector128&lt;short&gt; to Vector128&lt;int&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> WidenLower(Vector128<short> source) {
      var result = Vector128<int>.Zero;
      var count = Vector128<int>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (int)Vector128.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector128&lt;ushort&gt; to Vector128&lt;uint&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> WidenLower(Vector128<ushort> source) {
      var result = Vector128<uint>.Zero;
      var count = Vector128<uint>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (uint)Vector128.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector128&lt;int&gt; to Vector128&lt;long&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> WidenLower(Vector128<int> source) {
      var result = Vector128<long>.Zero;
      var count = Vector128<long>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (long)Vector128.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector128&lt;uint&gt; to Vector128&lt;ulong&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> WidenLower(Vector128<uint> source) {
      var result = Vector128<ulong>.Zero;
      var count = Vector128<ulong>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (ulong)Vector128.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector128&lt;float&gt; to Vector128&lt;double&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> WidenLower(Vector128<float> source) {
      var result = Vector128<double>.Zero;
      var count = Vector128<double>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (double)Vector128.GetElement(source, i));
      return result;
    }

    // ===== WIDENUPPER METHODS =====

    /// <summary>Widens the upper half of a Vector128&lt;sbyte&gt; to Vector128&lt;short&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> WidenUpper(Vector128<sbyte> source) {
      var result = Vector128<short>.Zero;
      var count = Vector128<short>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (short)Vector128.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector128&lt;byte&gt; to Vector128&lt;ushort&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> WidenUpper(Vector128<byte> source) {
      var result = Vector128<ushort>.Zero;
      var count = Vector128<ushort>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (ushort)Vector128.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector128&lt;short&gt; to Vector128&lt;int&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> WidenUpper(Vector128<short> source) {
      var result = Vector128<int>.Zero;
      var count = Vector128<int>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (int)Vector128.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector128&lt;ushort&gt; to Vector128&lt;uint&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> WidenUpper(Vector128<ushort> source) {
      var result = Vector128<uint>.Zero;
      var count = Vector128<uint>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (uint)Vector128.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector128&lt;int&gt; to Vector128&lt;long&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> WidenUpper(Vector128<int> source) {
      var result = Vector128<long>.Zero;
      var count = Vector128<long>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (long)Vector128.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector128&lt;uint&gt; to Vector128&lt;ulong&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> WidenUpper(Vector128<uint> source) {
      var result = Vector128<ulong>.Zero;
      var count = Vector128<ulong>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (ulong)Vector128.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector128&lt;float&gt; to Vector128&lt;double&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> WidenUpper(Vector128<float> source) {
      var result = Vector128<double>.Zero;
      var count = Vector128<double>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, (double)Vector128.GetElement(source, i + count));
      return result;
    }

  }
}

}
#endif
