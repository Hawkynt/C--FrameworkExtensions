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

#if SUPPORTS_VECTOR_64_TYPE && !SUPPORTS_VECTOR_64

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

/// <summary>
/// Partial polyfill for Vector64 static methods and extension operators on .NET Core 3.1-6.
/// The Vector64 type exists but many methods and all operators were added in .NET 7.
/// </summary>
public static partial class Vector64Polyfills {

  // Static extension methods for Vector64 class
  extension(Vector64) {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Create<T>(T value) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector64.WithElement(result, i, value);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> AndNot<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
      var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
      var result = leftBits & ~rightBits;
      return Unsafe.As<ulong, Vector64<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> LessThan<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector64.GetElement(left, i);
        var rightVal = Vector64.GetElement(right, i);
        var less = Scalar<T>.LessThan(leftVal, rightVal);
        result = Vector64.WithElement(result, i, less ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> GreaterThan<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector64.GetElement(left, i);
        var rightVal = Vector64.GetElement(right, i);
        var greater = Scalar<T>.GreaterThan(leftVal, rightVal);
        result = Vector64.WithElement(result, i, greater ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(1)]
    public static Vector64<T> Equals<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector64.GetElement(left, i);
        var rightVal = Vector64.GetElement(right, i);
        var equals = Scalar<T>.ObjectEquals(leftVal, rightVal);
        result = Vector64.WithElement(result, i, equals ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Add<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector64.GetElement(left, i);
        var rightVal = Vector64.GetElement(right, i);
        var sum = Scalar<T>.Add(leftVal, rightVal);
        result = Vector64.WithElement(result, i, sum);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Subtract<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector64.GetElement(left, i);
        var rightVal = Vector64.GetElement(right, i);
        var diff = Scalar<T>.Subtract(leftVal, rightVal);
        result = Vector64.WithElement(result, i, diff);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Multiply<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector64.GetElement(left, i);
        var rightVal = Vector64.GetElement(right, i);
        var product = Scalar<T>.Multiply(leftVal, rightVal);
        result = Vector64.WithElement(result, i, product);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Divide<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector64.GetElement(left, i);
        var rightVal = Vector64.GetElement(right, i);
        var quotient = Scalar<T>.Divide(leftVal, rightVal);
        result = Vector64.WithElement(result, i, quotient);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Negate<T>(Vector64<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var val = Vector64.GetElement(vector, i);
        var negated = Scalar<T>.Negate(val);
        result = Vector64.WithElement(result, i, negated);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> OnesComplement<T>(Vector64<T> vector) where T : struct {
      var bits = Unsafe.As<Vector64<T>, ulong>(ref vector);
      var result = ~bits;
      return Unsafe.As<ulong, Vector64<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> BitwiseAnd<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
      var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
      var result = leftBits & rightBits;
      return Unsafe.As<ulong, Vector64<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> BitwiseOr<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
      var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
      var result = leftBits | rightBits;
      return Unsafe.As<ulong, Vector64<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Xor<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
      var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
      var result = leftBits ^ rightBits;
      return Unsafe.As<ulong, Vector64<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Abs<T>(Vector64<T> vector) where T : struct {
      if (Scalar<T>.IsUnsigned)
        return vector;

      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Abs(Vector64.GetElement(vector, i));
        result = Vector64.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Max<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Max(Vector64.GetElement(left, i), Vector64.GetElement(right, i));
        result = Vector64.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Min<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Min(Vector64.GetElement(left, i), Vector64.GetElement(right, i));
        result = Vector64.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(Vector64<T> left, Vector64<T> right) where T : struct {
      var result = Scalar<T>.Zero();
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var product = Scalar<T>.Multiply(Vector64.GetElement(left, i), Vector64.GetElement(right, i));
        result = Scalar<T>.Add(result, product);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(Vector64<T> vector) where T : struct {
      var result = Scalar<T>.Zero();
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i)
        result = Scalar<T>.Add(result, Vector64.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Sqrt<T>(Vector64<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Sqrt(Vector64.GetElement(vector, i));
        result = Vector64.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Ceiling<T>(Vector64<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Ceiling(Vector64.GetElement(vector, i));
        result = Vector64.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Floor<T>(Vector64<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector64<T> result);
      var count = Vector64<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Floor(Vector64.GetElement(vector, i));
        result = Vector64.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector64<T> Load<T>(T* source) where T : unmanaged
      => Unsafe.ReadUnaligned<Vector64<T>>(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector64<T> LoadUnsafe<T>(ref T source) where T : struct
      => Unsafe.ReadUnaligned<Vector64<T>>(ref Unsafe.As<T, byte>(ref source));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Store<T>(Vector64<T> source, T* destination) where T : unmanaged
      => Unsafe.WriteUnaligned(destination, source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreUnsafe<T>(Vector64<T> source, ref T destination) where T : struct
      => Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref destination), source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> Shuffle<T>(Vector64<T> vector, Vector64<T> indices) where T : struct {
      var count = Vector64<T>.Count;
      Unsafe.SkipInit(out Vector64<T> result);
      for (var i = 0; i < count; ++i) {
        var idx = Convert.ToInt32(Vector64.GetElement(indices, i)) & (count - 1);
        result = Vector64.WithElement(result, i, Vector64.GetElement(vector, idx));
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits<T>(Vector64<T> vector) where T : struct {
      var bytes = Unsafe.As<Vector64<T>, ulong>(ref vector);
      uint result = 0;
      var elementSize = Unsafe.SizeOf<T>();

      if (elementSize == 1) {
        for (var i = 0; i < 8; ++i)
          if (((bytes >> (i * 8 + 7)) & 1) != 0) result |= 1u << i;
      } else if (elementSize == 2) {
        for (var i = 0; i < 4; ++i)
          if (((bytes >> (i * 16 + 15)) & 1) != 0) result |= 1u << i;
      } else if (elementSize == 4) {
        if (((bytes >> 31) & 1) != 0) result |= 1u;
        if (((bytes >> 63) & 1) != 0) result |= 2u;
      } else if (elementSize == 8) {
        if (((bytes >> 63) & 1) != 0) result |= 1u;
      }
      return result;
    }
  }

  // Extension operators for Vector64<T>
  extension<T>(Vector64<T>) where T : struct {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator +(Vector64<T> left, Vector64<T> right)
      => Vector64.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator -(Vector64<T> left, Vector64<T> right)
      => Vector64.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator *(Vector64<T> left, Vector64<T> right)
      => Vector64.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator /(Vector64<T> left, Vector64<T> right)
      => Vector64.Divide(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator &(Vector64<T> left, Vector64<T> right)
      => Vector64.BitwiseAnd(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator |(Vector64<T> left, Vector64<T> right)
      => Vector64.BitwiseOr(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator ^(Vector64<T> left, Vector64<T> right)
      => Vector64.Xor(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator ~(Vector64<T> value)
      => Vector64.OnesComplement(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator -(Vector64<T> value)
      => Vector64.Negate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector64<T> operator +(Vector64<T> value) => value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector64<T> left, Vector64<T> right) {
      var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
      var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
      return leftBits == rightBits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector64<T> left, Vector64<T> right) {
      var leftBits = Unsafe.As<Vector64<T>, ulong>(ref left);
      var rightBits = Unsafe.As<Vector64<T>, ulong>(ref right);
      return leftBits != rightBits;
    }
  }
}

#endif
