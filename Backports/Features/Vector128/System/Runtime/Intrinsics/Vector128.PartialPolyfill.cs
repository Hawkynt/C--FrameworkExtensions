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

#if SUPPORTS_VECTOR_128_TYPE && !SUPPORTS_VECTOR_128

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

/// <summary>
/// Partial polyfill for Vector128 static methods and extension operators on .NET Core 3.0-6.
/// The Vector128 type exists but many methods and all operators were added in .NET 7.
/// </summary>
public static class Vector128Polyfills {

  // Static extension methods for Vector128 class
  extension(Vector128) {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Create<T>(T value) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector128.WithElement(result, i, value);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> AndNot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var result = (leftBits.Item1 & ~rightBits.Item1, leftBits.Item2 & ~rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> LessThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector128.GetElement(left, i);
        var rightVal = Vector128.GetElement(right, i);
        var less = Scalar<T>.LessThan(leftVal, rightVal);
        result = Vector128.WithElement(result, i, less ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> GreaterThan<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector128.GetElement(left, i);
        var rightVal = Vector128.GetElement(right, i);
        var greater = Scalar<T>.GreaterThan(leftVal, rightVal);
        result = Vector128.WithElement(result, i, greater ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(1)]
    public static Vector128<T> Equals<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector128.GetElement(left, i);
        var rightVal = Vector128.GetElement(right, i);
        var equals = Scalar<T>.ObjectEquals(leftVal, rightVal);
        result = Vector128.WithElement(result, i, equals ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Add<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector128.GetElement(left, i);
        var rightVal = Vector128.GetElement(right, i);
        var sum = Scalar<T>.Add(leftVal, rightVal);
        result = Vector128.WithElement(result, i, sum);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Subtract<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector128.GetElement(left, i);
        var rightVal = Vector128.GetElement(right, i);
        var diff = Scalar<T>.Subtract(leftVal, rightVal);
        result = Vector128.WithElement(result, i, diff);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Multiply<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector128.GetElement(left, i);
        var rightVal = Vector128.GetElement(right, i);
        var product = Scalar<T>.Multiply(leftVal, rightVal);
        result = Vector128.WithElement(result, i, product);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Divide<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector128.GetElement(left, i);
        var rightVal = Vector128.GetElement(right, i);
        var quotient = Scalar<T>.Divide(leftVal, rightVal);
        result = Vector128.WithElement(result, i, quotient);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Negate<T>(Vector128<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var val = Vector128.GetElement(vector, i);
        var negated = Scalar<T>.Negate(val);
        result = Vector128.WithElement(result, i, negated);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> OnesComplement<T>(Vector128<T> vector) where T : struct {
      var bits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref vector);
      var result = (~bits.Item1, ~bits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> BitwiseAnd<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var result = (leftBits.Item1 & rightBits.Item1, leftBits.Item2 & rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> BitwiseOr<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var result = (leftBits.Item1 | rightBits.Item1, leftBits.Item2 | rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Xor<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var result = (leftBits.Item1 ^ rightBits.Item1, leftBits.Item2 ^ rightBits.Item2);
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Abs<T>(Vector128<T> vector) where T : struct {
      if (Scalar<T>.IsUnsigned)
        return vector;

      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Abs(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Max<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Max(Vector128.GetElement(left, i), Vector128.GetElement(right, i));
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Min<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Min(Vector128.GetElement(left, i), Vector128.GetElement(right, i));
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var result = Scalar<T>.Zero();
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var product = Scalar<T>.Multiply(Vector128.GetElement(left, i), Vector128.GetElement(right, i));
        result = Scalar<T>.Add(result, product);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(Vector128<T> vector) where T : struct {
      var result = Scalar<T>.Zero();
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        result = Scalar<T>.Add(result, Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Sqrt<T>(Vector128<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Sqrt(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Ceiling<T>(Vector128<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Ceiling(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> Floor<T>(Vector128<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Floor(Vector128.GetElement(vector, i));
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }
  }

  // Extension operators for Vector128<T>
  extension<T>(Vector128<T>) where T : struct {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator +(Vector128<T> left, Vector128<T> right)
      => Vector128.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator -(Vector128<T> left, Vector128<T> right)
      => Vector128.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator *(Vector128<T> left, Vector128<T> right)
      => Vector128.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator /(Vector128<T> left, Vector128<T> right)
      => Vector128.Divide(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator &(Vector128<T> left, Vector128<T> right)
      => Vector128.BitwiseAnd(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator |(Vector128<T> left, Vector128<T> right)
      => Vector128.BitwiseOr(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator ^(Vector128<T> left, Vector128<T> right)
      => Vector128.Xor(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator ~(Vector128<T> value)
      => Vector128.OnesComplement(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator -(Vector128<T> value)
      => Vector128.Negate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> operator +(Vector128<T> value) => value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector128<T> left, Vector128<T> right) {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      return leftBits.Item1 == rightBits.Item1 && leftBits.Item2 == rightBits.Item2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector128<T> left, Vector128<T> right) {
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      return leftBits.Item1 != rightBits.Item1 || leftBits.Item2 != rightBits.Item2;
    }
  }
}

#endif
