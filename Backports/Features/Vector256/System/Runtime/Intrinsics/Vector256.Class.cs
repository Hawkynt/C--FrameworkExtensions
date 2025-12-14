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

// ========== WAVE 1 ==========

#if !FEATURE_VECTOR256STATIC_WAVE1

namespace System.Runtime.Intrinsics {

/// <summary>
/// Provides a set of static methods for creating and working with 256-bit vectors.
/// </summary>
public static partial class Vector256 {
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
  public static void CopyTo<T>(this Vector256<T> vector, Span<T> destination) where T : struct {
    if (destination.Length < Vector256<T>.Count)
      AlwaysThrow.ArgumentException(nameof(destination), "Destination too short");

    for (var i = 0; i < Vector256<T>.Count; ++i)
      destination[i] = vector[i];
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static void CopyTo<T>(this Vector256<T> vector, T[] destination) where T : struct {
    if (destination.Length < Vector256<T>.Count)
      AlwaysThrow.ArgumentException(nameof(destination), "Destination too short");

    for (var i = 0; i < Vector256<T>.Count; ++i)
      destination[i] = vector[i];
  }

  [MethodImpl(Utilities.MethodImplOptions.AggressiveInlining)]
  public static void CopyTo<T>(this Vector256<T> vector, T[] destination, int index) where T : struct {
    if (destination.Length - index < Vector256<T>.Count)
      AlwaysThrow.ArgumentException(nameof(destination), "Destination too short");

    for (var i = 0; i < Vector256<T>.Count; ++i)
      destination[index + i] = vector[i];
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

// ========== WAVE 3 ==========

#if !FEATURE_VECTOR256STATIC_WAVE3

namespace System.Runtime.Intrinsics {

#if !FEATURE_VECTOR256STATIC_WAVE1
public static partial class Vector256 {
#else
public static partial class Vector256Polyfills {
  extension(Vector256) {
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector256<T> Load<T>(T* source) where T : struct
      => Unsafe.ReadUnaligned<Vector256<T>>(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector256<T> LoadUnsafe<T>(ref T source) where T : struct
      => Unsafe.ReadUnaligned<Vector256<T>>(ref Unsafe.As<T, byte>(ref source));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Store<T>(Vector256<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreUnsafe<T>(Vector256<T> source, ref T destination) where T : struct
      => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAligned<T>(Vector256<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreAlignedNonTemporal<T>(Vector256<T> source, T* destination) where T : struct
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7, byte e8, byte e9, byte e10, byte e11, byte e12, byte e13, byte e14, byte e15, byte e16, byte e17, byte e18, byte e19, byte e20, byte e21, byte e22, byte e23, byte e24, byte e25, byte e26, byte e27, byte e28, byte e29, byte e30, byte e31) {
      unsafe {
          byte* ptr = stackalloc byte[32];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          ptr[8]=e8; ptr[9]=e9; ptr[10]=e10; ptr[11]=e11; ptr[12]=e12; ptr[13]=e13; ptr[14]=e14; ptr[15]=e15;
          ptr[16]=e16; ptr[17]=e17; ptr[18]=e18; ptr[19]=e19; ptr[20]=e20; ptr[21]=e21; ptr[22]=e22; ptr[23]=e23;
          ptr[24]=e24; ptr[25]=e25; ptr[26]=e26; ptr[27]=e27; ptr[28]=e28; ptr[29]=e29; ptr[30]=e30; ptr[31]=e31;
          return Unsafe.ReadUnaligned<Vector256<byte>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> Create(int e0, int e1, int e2, int e3, int e4, int e5, int e6, int e7) {
      unsafe {
          int* ptr = stackalloc int[8];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          return Unsafe.ReadUnaligned<Vector256<int>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> Create(long e0, long e1, long e2, long e3) {
      unsafe {
          long* ptr = stackalloc long[4];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3;
          return Unsafe.ReadUnaligned<Vector256<long>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ulong> Create(ulong e0, ulong e1, ulong e2, ulong e3) {
      unsafe {
          ulong* ptr = stackalloc ulong[4];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3;
          return Unsafe.ReadUnaligned<Vector256<ulong>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Create(double e0, double e1, double e2, double e3) {
      unsafe {
          double* ptr = stackalloc double[4];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3;
          return Unsafe.ReadUnaligned<Vector256<double>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Create(float e0, float e1, float e2, float e3, float e4, float e5, float e6, float e7) {
      unsafe {
          float* ptr = stackalloc float[8];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          return Unsafe.ReadUnaligned<Vector256<float>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<short> Create(short e0, short e1, short e2, short e3, short e4, short e5, short e6, short e7, short e8, short e9, short e10, short e11, short e12, short e13, short e14, short e15) {
      unsafe {
          short* ptr = stackalloc short[16];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          ptr[8]=e8; ptr[9]=e9; ptr[10]=e10; ptr[11]=e11; ptr[12]=e12; ptr[13]=e13; ptr[14]=e14; ptr[15]=e15;
          return Unsafe.ReadUnaligned<Vector256<short>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ushort> Create(ushort e0, ushort e1, ushort e2, ushort e3, ushort e4, ushort e5, ushort e6, ushort e7, ushort e8, ushort e9, ushort e10, ushort e11, ushort e12, ushort e13, ushort e14, ushort e15) {
      unsafe {
          ushort* ptr = stackalloc ushort[16];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          ptr[8]=e8; ptr[9]=e9; ptr[10]=e10; ptr[11]=e11; ptr[12]=e12; ptr[13]=e13; ptr[14]=e14; ptr[15]=e15;
          return Unsafe.ReadUnaligned<Vector256<ushort>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7, sbyte e8, sbyte e9, sbyte e10, sbyte e11, sbyte e12, sbyte e13, sbyte e14, sbyte e15, sbyte e16, sbyte e17, sbyte e18, sbyte e19, sbyte e20, sbyte e21, sbyte e22, sbyte e23, sbyte e24, sbyte e25, sbyte e26, sbyte e27, sbyte e28, sbyte e29, sbyte e30, sbyte e31) {
      unsafe {
          sbyte* ptr = stackalloc sbyte[32];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          ptr[8]=e8; ptr[9]=e9; ptr[10]=e10; ptr[11]=e11; ptr[12]=e12; ptr[13]=e13; ptr[14]=e14; ptr[15]=e15;
          ptr[16]=e16; ptr[17]=e17; ptr[18]=e18; ptr[19]=e19; ptr[20]=e20; ptr[21]=e21; ptr[22]=e22; ptr[23]=e23;
          ptr[24]=e24; ptr[25]=e25; ptr[26]=e26; ptr[27]=e27; ptr[28]=e28; ptr[29]=e29; ptr[30]=e30; ptr[31]=e31;
          return Unsafe.ReadUnaligned<Vector256<sbyte>>(ptr);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> Create(uint e0, uint e1, uint e2, uint e3, uint e4, uint e5, uint e6, uint e7) {
      unsafe {
          uint* ptr = stackalloc uint[8];
          ptr[0]=e0; ptr[1]=e1; ptr[2]=e2; ptr[3]=e3; ptr[4]=e4; ptr[5]=e5; ptr[6]=e6; ptr[7]=e7;
          return Unsafe.ReadUnaligned<Vector256<uint>>(ptr);
      }
    }

#if !FEATURE_VECTOR256STATIC_WAVE1
    // If Base is not supported, we defined Vector256 in Base.
    // Base doesn't have CreateScalar (checked earlier).
    // If BCL supports Vector256 (e.g. netcoreapp3.1), it lacks CreateScalar.
    // So we ALWAYS need CreateScalar.
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> CreateScalar<T>(T value) where T : struct {
      var result = Vector256<T>.Zero;
      unsafe {
          Unsafe.Write(Unsafe.AsPointer(ref Unsafe.AsRef(in result)), value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> CreateScalarUnsafe<T>(T value) where T : struct {
      return CreateScalar(value);
    }

    // Wave 3 methods that are DUPLICATES of Wave 1
    // Only needed as extensions for WAVE1 frameworks
    // (for !WAVE1, these are already provided by Wave 1 polyfill)
#if FEATURE_VECTOR256STATIC_WAVE1

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Create<T>(T value) where T : struct {
      var count = 32 / Unsafe.SizeOf<T>();
      unsafe {
        var buffer = stackalloc byte[32];
        var ptr = (T*)buffer;
        for (var i = 0; i < count; ++i)
          ptr[i] = value;
        return Unsafe.ReadUnaligned<Vector256<T>>(buffer);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Add<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Add(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Subtract<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Subtract(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Multiply<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Divide<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Divide(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Negate<T>(Vector256<T> vector) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Negate(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Abs<T>(Vector256<T> vector) where T : struct {
       if (Scalar<T>.IsUnsigned)
         return vector;

       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Abs(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Sqrt<T>(Vector256<T> vector) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Sqrt(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(Vector256<T> vector) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Scalar<T>.Zero();
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));

       for(int i=0; i<count; i++)
           result = Scalar<T>.Add(result, Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Scalar<T>.Zero();
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));

       for(int i=0; i<count; i++) {
           var product = Scalar<T>.Multiply(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           result = Scalar<T>.Add(result, product);
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> BitwiseAnd<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
       var res = (leftBits.Item1 & rightBits.Item1, leftBits.Item2 & rightBits.Item2, leftBits.Item3 & rightBits.Item3, leftBits.Item4 & rightBits.Item4);
       return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> BitwiseOr<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
       var res = (leftBits.Item1 | rightBits.Item1, leftBits.Item2 | rightBits.Item2, leftBits.Item3 | rightBits.Item3, leftBits.Item4 | rightBits.Item4);
       return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Xor<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
       var res = (leftBits.Item1 ^ rightBits.Item1, leftBits.Item2 ^ rightBits.Item2, leftBits.Item3 ^ rightBits.Item3, leftBits.Item4 ^ rightBits.Item4);
       return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> OnesComplement<T>(Vector256<T> vector) where T : struct {
       var bits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref vector);
       var res = (~bits.Item1, ~bits.Item2, ~bits.Item3, ~bits.Item4);
       return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> AndNot<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
       var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
       var res = (leftBits.Item1 & ~rightBits.Item1, leftBits.Item2 & ~rightBits.Item2, leftBits.Item3 & ~rightBits.Item3, leftBits.Item4 & ~rightBits.Item4);
       return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Equals<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var eq = Scalar<T>.ObjectEquals(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = eq ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> GreaterThan<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var gt = Scalar<T>.GreaterThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = gt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> LessThan<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var lt = Scalar<T>.LessThan(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));
           Unsafe.Add(ref rRes, i) = lt ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Min<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Min(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Max<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Max(Unsafe.Add(ref rLeft, i), Unsafe.Add(ref rRight, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Floor<T>(Vector256<T> vector) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Floor(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Ceiling<T>(Vector256<T> vector) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++)
           Unsafe.Add(ref rRes, i) = Scalar<T>.Ceiling(Unsafe.Add(ref rVector, i));

       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetElement<T>(Vector256<T> vector, int index) where T : struct {
      if ((uint)index >= 32 / Unsafe.SizeOf<T>())
        throw new ArgumentOutOfRangeException(nameof(index));

      ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
      return Unsafe.Add(ref rVector, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> WithElement<T>(Vector256<T> vector, int index, T value) where T : struct {
      if ((uint)index >= 32 / Unsafe.SizeOf<T>())
        throw new ArgumentOutOfRangeException(nameof(index));

      var result = vector;
      ref var rResult = ref Unsafe.As<Vector256<T>, T>(ref result);
      Unsafe.Add(ref rResult, index) = value;
      return result;
    }
#endif

    // Wave 3 UNIQUE methods - not in Wave 1

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Shuffle<T>(Vector256<T> vector, Vector256<T> indices) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rIndices = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in indices));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var idxVal = Unsafe.Add(ref rIndices, i);
           int idx = Scalar<T>.ToInt32(idxVal) & (count - 1);
           var val = Unsafe.Add(ref rVector, idx);
           Unsafe.Add(ref rRes, i) = val;
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<byte> Shuffle(Vector256<byte> vector, Vector256<byte> indices)
      => Shuffle<byte>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> Shuffle(Vector256<sbyte> vector, Vector256<sbyte> indices)
      => Shuffle<sbyte>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<short> Shuffle(Vector256<short> vector, Vector256<short> indices)
      => Shuffle<short>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ushort> Shuffle(Vector256<ushort> vector, Vector256<ushort> indices)
      => Shuffle<ushort>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> Shuffle(Vector256<int> vector, Vector256<int> indices)
      => Shuffle<int>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> Shuffle(Vector256<uint> vector, Vector256<uint> indices)
      => Shuffle<uint>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> Shuffle(Vector256<long> vector, Vector256<long> indices)
      => Shuffle<long>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ulong> Shuffle(Vector256<ulong> vector, Vector256<ulong> indices)
      => Shuffle<ulong>(vector, indices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits<T>(Vector256<T> vector) where T : struct {
      uint result = 0;
      int count = 32 / Unsafe.SizeOf<T>();
      ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));

      for (var i = 0; i < count; ++i) {
        if (Scalar<T>.ExtractMostSignificantBit(Unsafe.Add(ref rVector, i)))
          result |= 1u << i;
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<byte> vector) => ExtractMostSignificantBits<byte>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<sbyte> vector) => ExtractMostSignificantBits<sbyte>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<short> vector) => ExtractMostSignificantBits<short>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<ushort> vector) => ExtractMostSignificantBits<ushort>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<int> vector) => ExtractMostSignificantBits<int>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<uint> vector) => ExtractMostSignificantBits<uint>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<long> vector) => ExtractMostSignificantBits<long>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<ulong> vector) => ExtractMostSignificantBits<ulong>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<float> vector) => ExtractMostSignificantBits<float>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits(Vector256<double> vector) => ExtractMostSignificantBits<double>(vector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> ConditionalSelect<T>(Vector256<T> condition, Vector256<T> left, Vector256<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
      var condBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref condition);

      var res = (
         (condBits.Item1 & leftBits.Item1) | (~condBits.Item1 & rightBits.Item1),
         (condBits.Item2 & leftBits.Item2) | (~condBits.Item2 & rightBits.Item2),
         (condBits.Item3 & leftBits.Item3) | (~condBits.Item3 & rightBits.Item3),
         (condBits.Item4 & leftBits.Item4) | (~condBits.Item4 & rightBits.Item4)
      );
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> ShiftLeft<T>(Vector256<T> vector, int shiftCount) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftLeft(Unsafe.Add(ref rVector, i), shiftCount);
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> ShiftRightArithmetic<T>(Vector256<T> vector, int shiftCount) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftRightArithmetic(Unsafe.Add(ref rVector, i), shiftCount);
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> ShiftRightLogical<T>(Vector256<T> vector, int shiftCount) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rVector = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           Unsafe.Add(ref rRes, i) = Scalar<T>.ShiftRightLogical(Unsafe.Add(ref rVector, i), shiftCount);
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> GreaterThanOrEqual<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var l = Unsafe.Add(ref rLeft, i);
           var r = Unsafe.Add(ref rRight, i);
           var ge = Scalar<T>.GreaterThanOrEqual(l, r);
           Unsafe.Add(ref rRes, i) = ge ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }
       return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> LessThanOrEqual<T>(Vector256<T> left, Vector256<T> right) where T : struct {
       int count = 32 / Unsafe.SizeOf<T>();
       var result = Vector256<T>.Zero;
       ref var rLeft = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in left));
       ref var rRight = ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in right));
       ref var rRes = ref Unsafe.As<Vector256<T>, T>(ref result);

       for(int i=0; i<count; i++) {
           var l = Unsafe.Add(ref rLeft, i);
           var r = Unsafe.Add(ref rRight, i);
           var le = Scalar<T>.LessThanOrEqual(l, r);
           Unsafe.Add(ref rRes, i) = le ? Scalar<T>.AllBitsSet : Scalar<T>.Zero();
       }
       return result;
    }

    // ===== COMPARISON ALL/ANY METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAll<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.ObjectEquals(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.ObjectEquals(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAll<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThan(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAny<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThan(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAll<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.GreaterThanOrEqual(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanOrEqualAny<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.GreaterThanOrEqual(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAll<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThan(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAny<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThan(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAll<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (!Scalar<T>.LessThanOrEqual(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanOrEqualAny<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      for (var i = 0; i < 32 / Unsafe.SizeOf<T>(); ++i)
        if (Scalar<T>.LessThanOrEqual(Vector256.GetElement(left, i), Vector256.GetElement(right, i)))
          return true;
      return false;
    }

    // ===== TOSCALAR (for BCL case) =====

#if FEATURE_VECTOR256STATIC_WAVE1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToScalar<T>(Vector256<T> vector) where T : struct
      => Vector256.GetElement(vector, 0);
#endif

    // ===== COPYSIGN =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> CopySign(Vector256<float> value, Vector256<float> sign) {
      var count = 32 / sizeof(float);
      var result = Vector256<float>.Zero;
      for (var i = 0; i < count; ++i) {
        var v = Vector256.GetElement(value, i);
        var s = Vector256.GetElement(sign, i);
        var absV = MathF.Abs(v);
        result = Vector256.WithElement(result, i, s < 0 ? -absV : absV);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> CopySign(Vector256<double> value, Vector256<double> sign) {
      var count = 32 / sizeof(double);
      var result = Vector256<double>.Zero;
      for (var i = 0; i < count; ++i) {
        var v = Vector256.GetElement(value, i);
        var s = Vector256.GetElement(sign, i);
        var absV = Math.Abs(v);
        result = Vector256.WithElement(result, i, s < 0 ? -absV : absV);
      }
      return result;
    }

    // ===== CLASSIFICATION METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsNaN(Vector256<float> vector) {
      var count = 32 / sizeof(float);
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isNan = float.IsNaN(Vector256.GetElement(vector, i));
        result = Vector256.WithElement(result, i, isNan ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsNaN(Vector256<double> vector) {
      var count = 32 / sizeof(double);
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var isNan = double.IsNaN(Vector256.GetElement(vector, i));
        result = Vector256.WithElement(result, i, isNan ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsNegative(Vector256<float> vector) {
      var count = 32 / sizeof(float);
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector256.GetElement(vector, i);
        var isNeg = val < 0 || (val == 0 && float.IsNegativeInfinity(1f / val));
        result = Vector256.WithElement(result, i, isNeg ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsNegative(Vector256<double> vector) {
      var count = 32 / sizeof(double);
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var val = Vector256.GetElement(vector, i);
        var isNeg = val < 0 || (val == 0 && double.IsNegativeInfinity(1d / val));
        result = Vector256.WithElement(result, i, isNeg ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsPositive(Vector256<float> vector) {
      var count = 32 / sizeof(float);
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var val = Vector256.GetElement(vector, i);
        var isPos = val > 0 || (val == 0 && float.IsPositiveInfinity(1f / val));
        result = Vector256.WithElement(result, i, isPos ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsPositive(Vector256<double> vector) {
      var count = 32 / sizeof(double);
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var val = Vector256.GetElement(vector, i);
        var isPos = val > 0 || (val == 0 && double.IsPositiveInfinity(1d / val));
        result = Vector256.WithElement(result, i, isPos ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsZero(Vector256<float> vector) {
      var count = 32 / sizeof(float);
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < count; ++i) {
        var isZero = Vector256.GetElement(vector, i) == 0f;
        result = Vector256.WithElement(result, i, isZero ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsZero(Vector256<double> vector) {
      var count = 32 / sizeof(double);
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < count; ++i) {
        var isZero = Vector256.GetElement(vector, i) == 0d;
        result = Vector256.WithElement(result, i, isZero ? allBitsSet : 0d);
      }
      return result;
    }

    // ===== NARROW METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Narrow(Vector256<double> lower, Vector256<double> upper) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector256.WithElement(result, i, (float)Vector256.GetElement(lower, i));
        result = Vector256.WithElement(result, i + 4, (float)Vector256.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> Narrow(Vector256<long> lower, Vector256<long> upper) {
      var result = Vector256<int>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector256.WithElement(result, i, (int)Vector256.GetElement(lower, i));
        result = Vector256.WithElement(result, i + 4, (int)Vector256.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> Narrow(Vector256<ulong> lower, Vector256<ulong> upper) {
      var result = Vector256<uint>.Zero;
      for (var i = 0; i < 4; ++i) {
        result = Vector256.WithElement(result, i, (uint)Vector256.GetElement(lower, i));
        result = Vector256.WithElement(result, i + 4, (uint)Vector256.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<short> Narrow(Vector256<int> lower, Vector256<int> upper) {
      var result = Vector256<short>.Zero;
      for (var i = 0; i < 8; ++i) {
        result = Vector256.WithElement(result, i, (short)Vector256.GetElement(lower, i));
        result = Vector256.WithElement(result, i + 8, (short)Vector256.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ushort> Narrow(Vector256<uint> lower, Vector256<uint> upper) {
      var result = Vector256<ushort>.Zero;
      for (var i = 0; i < 8; ++i) {
        result = Vector256.WithElement(result, i, (ushort)Vector256.GetElement(lower, i));
        result = Vector256.WithElement(result, i + 8, (ushort)Vector256.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> Narrow(Vector256<short> lower, Vector256<short> upper) {
      var result = Vector256<sbyte>.Zero;
      for (var i = 0; i < 16; ++i) {
        result = Vector256.WithElement(result, i, (sbyte)Vector256.GetElement(lower, i));
        result = Vector256.WithElement(result, i + 16, (sbyte)Vector256.GetElement(upper, i));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<byte> Narrow(Vector256<ushort> lower, Vector256<ushort> upper) {
      var result = Vector256<byte>.Zero;
      for (var i = 0; i < 16; ++i) {
        result = Vector256.WithElement(result, i, (byte)Vector256.GetElement(lower, i));
        result = Vector256.WithElement(result, i + 16, (byte)Vector256.GetElement(upper, i));
      }
      return result;
    }

    // ===== WIDEN METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<double> Lower, Vector256<double> Upper) Widen(Vector256<float> vector) {
      var lower = Vector256<double>.Zero;
      var upper = Vector256<double>.Zero;
      for (var i = 0; i < 4; ++i) {
        lower = Vector256.WithElement(lower, i, (double)Vector256.GetElement(vector, i));
        upper = Vector256.WithElement(upper, i, (double)Vector256.GetElement(vector, i + 4));
      }
      return (lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<long> Lower, Vector256<long> Upper) Widen(Vector256<int> vector) {
      var lower = Vector256<long>.Zero;
      var upper = Vector256<long>.Zero;
      for (var i = 0; i < 4; ++i) {
        lower = Vector256.WithElement(lower, i, (long)Vector256.GetElement(vector, i));
        upper = Vector256.WithElement(upper, i, (long)Vector256.GetElement(vector, i + 4));
      }
      return (lower, upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<ulong> Lower, Vector256<ulong> Upper) Widen(Vector256<uint> vector) {
      var lower = Vector256<ulong>.Zero;
      var upper = Vector256<ulong>.Zero;
      for (var i = 0; i < 4; ++i) {
        lower = Vector256.WithElement(lower, i, (ulong)Vector256.GetElement(vector, i));
        upper = Vector256.WithElement(upper, i, (ulong)Vector256.GetElement(vector, i + 4));
      }
      return (lower, upper);
    }

    // ===== CONVERTTO METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> ConvertToInt32(Vector256<float> vector) {
      var result = Vector256<int>.Zero;
      for (var i = 0; i < 8; ++i)
        result = Vector256.WithElement(result, i, (int)Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> ConvertToSingle(Vector256<int> vector) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < 8; ++i)
        result = Vector256.WithElement(result, i, (float)Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> ConvertToSingle(Vector256<uint> vector) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < 8; ++i)
        result = Vector256.WithElement(result, i, (float)Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> ConvertToInt64(Vector256<double> vector) {
      var result = Vector256<long>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector256.WithElement(result, i, (long)Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> ConvertToDouble(Vector256<long> vector) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector256.WithElement(result, i, (double)Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> ConvertToDouble(Vector256<ulong> vector) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector256.WithElement(result, i, (double)Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> ConvertToUInt32(Vector256<float> vector) {
      var result = Vector256<uint>.Zero;
      for (var i = 0; i < 8; ++i)
        result = Vector256.WithElement(result, i, (uint)Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ulong> ConvertToUInt64(Vector256<double> vector) {
      var result = Vector256<ulong>.Zero;
      for (var i = 0; i < 4; ++i)
        result = Vector256.WithElement(result, i, (ulong)Vector256.GetElement(vector, i));
      return result;
    }

    // ===== GET/WITH LOWER/UPPER METHODS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GetLower<T>(Vector256<T> vector) where T : struct {
      var bytes = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref vector);
      var data = (bytes.Item1, bytes.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GetUpper<T>(Vector256<T> vector) where T : struct {
      var bytes = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref vector);
      var data = (bytes.Item3, bytes.Item4);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> WithLower<T>(Vector256<T> vector, Vector128<T> value) where T : struct {
      var bytes = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref vector);
      var valueBytes = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref value);
      var result = (valueBytes.Item1, valueBytes.Item2, bytes.Item3, bytes.Item4);
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> WithUpper<T>(Vector256<T> vector, Vector128<T> value) where T : struct {
      var bytes = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref vector);
      var valueBytes = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref value);
      var result = (bytes.Item1, bytes.Item2, valueBytes.Item1, valueBytes.Item2);
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref result);
    }

    // ===== SHUFFLE NATIVE METHOD =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> ShuffleNative<T>(Vector256<T> vector, Vector256<T> indices) where T : struct
      => Shuffle<T>(vector, indices);

    // ===== AS VECTOR METHODS =====

#if FEATURE_SYSTEM_NUMERICS_VECTOR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Numerics.Vector<T> AsVector<T>(Vector256<T> vector) where T : struct {
      if (Numerics.Vector<T>.Count != Vector256<T>.Count)
        throw new NotSupportedException($"Vector<{typeof(T).Name}> size mismatch: expected {Vector256<T>.Count}, got {Numerics.Vector<T>.Count}");
      return Unsafe.As<Vector256<T>, Numerics.Vector<T>>(ref vector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> AsVector256<T>(Numerics.Vector<T> vector) where T : struct {
      if (Numerics.Vector<T>.Count != Vector256<T>.Count)
        throw new NotSupportedException($"Vector<{typeof(T).Name}> size mismatch: expected {Vector256<T>.Count}, got {Numerics.Vector<T>.Count}");
      return Unsafe.As<Numerics.Vector<T>, Vector256<T>>(ref vector);
    }
#endif

#if FEATURE_VECTOR256STATIC_WAVE1
  }
}
#else
}
#endif

}

#endif

// ========== WAVE 5 ==========

#if !FEATURE_VECTOR256STATIC_WAVE5

namespace System.Runtime.Intrinsics {

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

    // ===== SATURATION ARITHMETIC =====

    /// <summary>Adds two vectors with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> AddSaturate<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      var result = Vector256<T>.Zero;
      for (var i = 0; i < Vector256<T>.Count; ++i) {
        var l = Vector256.GetElement(left, i);
        var r = Vector256.GetElement(right, i);
        result = Vector256.WithElement(result, i, Scalar<T>.AddSaturate(l, r));
      }
      return result;
    }

    /// <summary>Subtracts two vectors with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> SubtractSaturate<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      var result = Vector256<T>.Zero;
      for (var i = 0; i < Vector256<T>.Count; ++i) {
        var l = Vector256.GetElement(left, i);
        var r = Vector256.GetElement(right, i);
        result = Vector256.WithElement(result, i, Scalar<T>.SubtractSaturate(l, r));
      }
      return result;
    }

    // ===== MIN/MAX MAGNITUDE =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MinMagnitude(Vector256<float> x, Vector256<float> y) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, MathF.Abs(xv) < MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> MinMagnitude(Vector256<double> x, Vector256<double> y) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, Math.Abs(xv) < Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MaxMagnitude(Vector256<float> x, Vector256<float> y) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, MathF.Abs(xv) > MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> MaxMagnitude(Vector256<double> x, Vector256<double> y) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, Math.Abs(xv) > Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MinMagnitudeNumber(Vector256<float> x, Vector256<float> y) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        if (float.IsNaN(xv) || float.IsNaN(yv))
          result = Vector256.WithElement(result, i, float.NaN);
        else
          result = Vector256.WithElement(result, i, MathF.Abs(xv) < MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> MinMagnitudeNumber(Vector256<double> x, Vector256<double> y) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        if (double.IsNaN(xv) || double.IsNaN(yv))
          result = Vector256.WithElement(result, i, double.NaN);
        else
          result = Vector256.WithElement(result, i, Math.Abs(xv) < Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MaxMagnitudeNumber(Vector256<float> x, Vector256<float> y) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        if (float.IsNaN(xv) || float.IsNaN(yv))
          result = Vector256.WithElement(result, i, float.NaN);
        else
          result = Vector256.WithElement(result, i, MathF.Abs(xv) > MathF.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> MaxMagnitudeNumber(Vector256<double> x, Vector256<double> y) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        if (double.IsNaN(xv) || double.IsNaN(yv))
          result = Vector256.WithElement(result, i, double.NaN);
        else
          result = Vector256.WithElement(result, i, Math.Abs(xv) > Math.Abs(yv) ? xv : yv);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MinNumber(Vector256<float> x, Vector256<float> y) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, float.IsNaN(xv) ? yv : (float.IsNaN(yv) ? xv : MathF.Min(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> MinNumber(Vector256<double> x, Vector256<double> y) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, double.IsNaN(xv) ? yv : (double.IsNaN(yv) ? xv : Math.Min(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> MaxNumber(Vector256<float> x, Vector256<float> y) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, float.IsNaN(xv) ? yv : (float.IsNaN(yv) ? xv : MathF.Max(xv, yv)));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> MaxNumber(Vector256<double> x, Vector256<double> y) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, double.IsNaN(xv) ? yv : (double.IsNaN(yv) ? xv : Math.Max(xv, yv)));
      }
      return result;
    }

    // ===== NATIVE CLAMP/MIN/MAX =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> ClampNative<T>(Vector256<T> value, Vector256<T> min, Vector256<T> max) where T : struct
      => Vector256.Min(Vector256.Max(value, min), max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> MinNative<T>(Vector256<T> x, Vector256<T> y) where T : struct => Vector256.Min(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> MaxNative<T>(Vector256<T> x, Vector256<T> y) where T : struct => Vector256.Max(x, y);

    // ===== FLOAT PREDICATES =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsFinite(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, float.IsFinite(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsFinite(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, !double.IsNaN(v) && !double.IsInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsInfinity(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, float.IsInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsInfinity(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, double.IsInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsNegativeInfinity(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, float.IsNegativeInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsNegativeInfinity(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, double.IsNegativeInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsPositiveInfinity(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, float.IsPositiveInfinity(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsPositiveInfinity(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, double.IsPositiveInfinity(v) ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsNormal(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, float.IsNormal(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsNormal(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        var bits = BitConverter.DoubleToInt64Bits(v);
        var exponent = (int)((bits >> 52) & 0x7FF);
        var isNormal = exponent != 0 && exponent != 0x7FF && v != 0d;
        result = Vector256.WithElement(result, i, isNormal ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> IsSubnormal(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      var allBitsSet = BitConverter.Int32BitsToSingle(-1);
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        result = Vector256.WithElement(result, i, float.IsSubnormal(v) ? allBitsSet : 0f);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> IsSubnormal(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      var allBitsSet = BitConverter.Int64BitsToDouble(-1L);
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        var bits = BitConverter.DoubleToInt64Bits(v);
        var exponent = (int)((bits >> 52) & 0x7FF);
        var mantissa = bits & 0xFFFFFFFFFFFFF;
        var isSubnormal = exponent == 0 && mantissa != 0;
        result = Vector256.WithElement(result, i, isSubnormal ? allBitsSet : 0d);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> IsOddInteger<T>(Vector256<T> vector) where T : struct {
      var result = Vector256<T>.Zero;
      var allBitsSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector256<T>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        var isOdd = (Convert.ToInt64(v) & 1) == 1;
        result = Vector256.WithElement(result, i, isOdd ? allBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> IsEvenInteger<T>(Vector256<T> vector) where T : struct {
      var result = Vector256<T>.Zero;
      var allBitsSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector256<T>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        var isEven = (Convert.ToInt64(v) & 1) == 0;
        result = Vector256.WithElement(result, i, isEven ? allBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    // ===== TRANSCENDENTAL FUNCTIONS =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Sin(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i)
        result = Vector256.WithElement(result, i, MathF.Sin(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Sin(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i)
        result = Vector256.WithElement(result, i, Math.Sin(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Cos(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i)
        result = Vector256.WithElement(result, i, MathF.Cos(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Cos(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i)
        result = Vector256.WithElement(result, i, Math.Cos(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<float> Sin, Vector256<float> Cos) SinCos(Vector256<float> vector) {
      var sin = Vector256<float>.Zero;
      var cos = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        sin = Vector256.WithElement(sin, i, MathF.Sin(v));
        cos = Vector256.WithElement(cos, i, MathF.Cos(v));
      }
      return (sin, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Vector256<double> Sin, Vector256<double> Cos) SinCos(Vector256<double> vector) {
      var sin = Vector256<double>.Zero;
      var cos = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var v = Vector256.GetElement(vector, i);
        sin = Vector256.WithElement(sin, i, Math.Sin(v));
        cos = Vector256.WithElement(cos, i, Math.Cos(v));
      }
      return (sin, cos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Exp(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i)
        result = Vector256.WithElement(result, i, MathF.Exp(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Exp(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i)
        result = Vector256.WithElement(result, i, Math.Exp(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Log(Vector256<float> vector) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i)
        result = Vector256.WithElement(result, i, MathF.Log(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Log(Vector256<double> vector) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i)
        result = Vector256.WithElement(result, i, Math.Log(Vector256.GetElement(vector, i)));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<float> Hypot(Vector256<float> x, Vector256<float> y) {
      var result = Vector256<float>.Zero;
      for (var i = 0; i < Vector256<float>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, MathF.Sqrt(xv * xv + yv * yv));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> Hypot(Vector256<double> x, Vector256<double> y) {
      var result = Vector256<double>.Zero;
      for (var i = 0; i < Vector256<double>.Count; ++i) {
        var xv = Vector256.GetElement(x, i);
        var yv = Vector256.GetElement(y, i);
        result = Vector256.WithElement(result, i, Math.Sqrt(xv * xv + yv * yv));
      }
      return result;
    }

    // ===== MULTIPLY-ADD =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> MultiplyAddEstimate<T>(Vector256<T> left, Vector256<T> right, Vector256<T> addend) where T : struct
      => Vector256.Add(Vector256.Multiply(left, right), addend);


    /// <summary>
    /// Checks if all elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool All<T>(Vector256<T> vector, T value) where T : struct {
      for (var i = 0; i < Vector256<T>.Count; ++i)
        if (!Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), value))
          return false;
      return true;
    }

    /// <summary>
    /// Checks if any element in the vector equals the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Any<T>(Vector256<T> vector, T value) where T : struct {
      for (var i = 0; i < Vector256<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), value))
          return true;
      return false;
    }

    /// <summary>
    /// Checks if no elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool None<T>(Vector256<T> vector, T value) where T : struct => !Any(vector, value);

    /// <summary>
    /// Checks if all elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllWhereAllBitsSet<T>(Vector256<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector256<T>.Count; ++i)
        if (!Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), allSet))
          return false;
      return true;
    }

    /// <summary>
    /// Checks if any element in the vector has all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AnyWhereAllBitsSet<T>(Vector256<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector256<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), allSet))
          return true;
      return false;
    }

    /// <summary>
    /// Checks if no elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NoneWhereAllBitsSet<T>(Vector256<T> vector) where T : struct => !AnyWhereAllBitsSet(vector);

    /// <summary>
    /// Counts how many elements in the vector equal the specified value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count<T>(Vector256<T> vector, T value) where T : struct {
      var count = 0;
      for (var i = 0; i < Vector256<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), value))
          ++count;
      return count;
    }

    /// <summary>
    /// Counts how many elements in the vector have all bits set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountWhereAllBitsSet<T>(Vector256<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      var count = 0;
      for (var i = 0; i < Vector256<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), allSet))
          ++count;
      return count;
    }

    /// <summary>
    /// Returns the index of the first element that has all bits set, or -1 if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfWhereAllBitsSet<T>(Vector256<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = 0; i < Vector256<T>.Count; ++i)
        if (Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), allSet))
          return i;
      return -1;
    }

    /// <summary>
    /// Returns the index of the last element that has all bits set, or -1 if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOfWhereAllBitsSet<T>(Vector256<T> vector) where T : struct {
      var allSet = Scalar<T>.AllBitsSet;
      for (var i = Vector256<T>.Count - 1; i >= 0; --i)
        if (Scalar<T>.ObjectEquals(Vector256.GetElement(vector, i), allSet))
          return i;
      return -1;
    }

    /// <summary>
    /// Creates a vector with sequential values starting from the specified start value and incrementing by the specified step.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> CreateSequence<T>(T start, T step) where T : struct {
      var result = Vector256<T>.Zero;
      var current = start;
      for (var i = 0; i < Vector256<T>.Count; ++i) {
        result = Vector256.WithElement(result, i, current);
        current = Scalar<T>.Add(current, step);
      }
      return result;
    }

    // ===== NARROWWITHSATURATION METHODS =====

    /// <summary>Narrows two Vector256&lt;int&gt; to one Vector256&lt;short&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<short> NarrowWithSaturation(Vector256<int> lower, Vector256<int> upper) {
      var result = Vector256<short>.Zero;
      var lowerCount = Vector256<int>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(lower, i);
        result = Vector256.WithElement(result, i, (short)Math.Clamp(val, short.MinValue, short.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(upper, i);
        result = Vector256.WithElement(result, i + lowerCount, (short)Math.Clamp(val, short.MinValue, short.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector256&lt;uint&gt; to one Vector256&lt;ushort&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ushort> NarrowWithSaturation(Vector256<uint> lower, Vector256<uint> upper) {
      var result = Vector256<ushort>.Zero;
      var lowerCount = Vector256<uint>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(lower, i);
        result = Vector256.WithElement(result, i, (ushort)Math.Min(val, ushort.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(upper, i);
        result = Vector256.WithElement(result, i + lowerCount, (ushort)Math.Min(val, ushort.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector256&lt;short&gt; to one Vector256&lt;sbyte&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<sbyte> NarrowWithSaturation(Vector256<short> lower, Vector256<short> upper) {
      var result = Vector256<sbyte>.Zero;
      var lowerCount = Vector256<short>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(lower, i);
        result = Vector256.WithElement(result, i, (sbyte)Math.Clamp(val, sbyte.MinValue, sbyte.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(upper, i);
        result = Vector256.WithElement(result, i + lowerCount, (sbyte)Math.Clamp(val, sbyte.MinValue, sbyte.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector256&lt;ushort&gt; to one Vector256&lt;byte&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<byte> NarrowWithSaturation(Vector256<ushort> lower, Vector256<ushort> upper) {
      var result = Vector256<byte>.Zero;
      var lowerCount = Vector256<ushort>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(lower, i);
        result = Vector256.WithElement(result, i, (byte)Math.Min(val, byte.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(upper, i);
        result = Vector256.WithElement(result, i + lowerCount, (byte)Math.Min(val, byte.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector256&lt;long&gt; to one Vector256&lt;int&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> NarrowWithSaturation(Vector256<long> lower, Vector256<long> upper) {
      var result = Vector256<int>.Zero;
      var lowerCount = Vector256<long>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(lower, i);
        result = Vector256.WithElement(result, i, (int)Math.Clamp(val, int.MinValue, int.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(upper, i);
        result = Vector256.WithElement(result, i + lowerCount, (int)Math.Clamp(val, int.MinValue, int.MaxValue));
      }
      return result;
    }

    /// <summary>Narrows two Vector256&lt;ulong&gt; to one Vector256&lt;uint&gt; with saturation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> NarrowWithSaturation(Vector256<ulong> lower, Vector256<ulong> upper) {
      var result = Vector256<uint>.Zero;
      var lowerCount = Vector256<ulong>.Count;
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(lower, i);
        result = Vector256.WithElement(result, i, (uint)Math.Min(val, uint.MaxValue));
      }
      for (var i = 0; i < lowerCount; ++i) {
        var val = Vector256.GetElement(upper, i);
        result = Vector256.WithElement(result, i + lowerCount, (uint)Math.Min(val, uint.MaxValue));
      }
      return result;
    }

    // ===== WIDENLOWER METHODS =====

    /// <summary>Widens the lower half of a Vector256&lt;sbyte&gt; to Vector256&lt;short&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<short> WidenLower(Vector256<sbyte> source) {
      var result = Vector256<short>.Zero;
      var count = Vector256<short>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (short)Vector256.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector256&lt;byte&gt; to Vector256&lt;ushort&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ushort> WidenLower(Vector256<byte> source) {
      var result = Vector256<ushort>.Zero;
      var count = Vector256<ushort>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (ushort)Vector256.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector256&lt;short&gt; to Vector256&lt;int&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> WidenLower(Vector256<short> source) {
      var result = Vector256<int>.Zero;
      var count = Vector256<int>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (int)Vector256.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector256&lt;ushort&gt; to Vector256&lt;uint&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> WidenLower(Vector256<ushort> source) {
      var result = Vector256<uint>.Zero;
      var count = Vector256<uint>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (uint)Vector256.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector256&lt;int&gt; to Vector256&lt;long&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> WidenLower(Vector256<int> source) {
      var result = Vector256<long>.Zero;
      var count = Vector256<long>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (long)Vector256.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector256&lt;uint&gt; to Vector256&lt;ulong&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ulong> WidenLower(Vector256<uint> source) {
      var result = Vector256<ulong>.Zero;
      var count = Vector256<ulong>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (ulong)Vector256.GetElement(source, i));
      return result;
    }

    /// <summary>Widens the lower half of a Vector256&lt;float&gt; to Vector256&lt;double&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> WidenLower(Vector256<float> source) {
      var result = Vector256<double>.Zero;
      var count = Vector256<double>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (double)Vector256.GetElement(source, i));
      return result;
    }

    // ===== WIDENUPPER METHODS =====

    /// <summary>Widens the upper half of a Vector256&lt;sbyte&gt; to Vector256&lt;short&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<short> WidenUpper(Vector256<sbyte> source) {
      var result = Vector256<short>.Zero;
      var count = Vector256<short>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (short)Vector256.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector256&lt;byte&gt; to Vector256&lt;ushort&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ushort> WidenUpper(Vector256<byte> source) {
      var result = Vector256<ushort>.Zero;
      var count = Vector256<ushort>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (ushort)Vector256.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector256&lt;short&gt; to Vector256&lt;int&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<int> WidenUpper(Vector256<short> source) {
      var result = Vector256<int>.Zero;
      var count = Vector256<int>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (int)Vector256.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector256&lt;ushort&gt; to Vector256&lt;uint&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> WidenUpper(Vector256<ushort> source) {
      var result = Vector256<uint>.Zero;
      var count = Vector256<uint>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (uint)Vector256.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector256&lt;int&gt; to Vector256&lt;long&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<long> WidenUpper(Vector256<int> source) {
      var result = Vector256<long>.Zero;
      var count = Vector256<long>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (long)Vector256.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector256&lt;uint&gt; to Vector256&lt;ulong&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<ulong> WidenUpper(Vector256<uint> source) {
      var result = Vector256<ulong>.Zero;
      var count = Vector256<ulong>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (ulong)Vector256.GetElement(source, i + count));
      return result;
    }

    /// <summary>Widens the upper half of a Vector256&lt;float&gt; to Vector256&lt;double&gt;.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<double> WidenUpper(Vector256<float> source) {
      var result = Vector256<double>.Zero;
      var count = Vector256<double>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, (double)Vector256.GetElement(source, i + count));
      return result;
    }

  }
}

}

#endif
