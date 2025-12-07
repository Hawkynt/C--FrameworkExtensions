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

#if SUPPORTS_VECTOR_256_TYPE && !SUPPORTS_VECTOR_256

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

/// <summary>
/// Partial polyfill for Vector256 static methods and extension operators on .NET Core 3.0-6.
/// The Vector256 type exists but many methods and all operators were added in .NET 7.
/// </summary>
public static class Vector256Polyfills {

  // Static extension methods for Vector256 class
  extension(Vector256) {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Create<T>(T value) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i)
        result = Vector256.WithElement(result, i, value);
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> AndNot<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
      var result = (
        leftBits.Item1 & ~rightBits.Item1,
        leftBits.Item2 & ~rightBits.Item2,
        leftBits.Item3 & ~rightBits.Item3,
        leftBits.Item4 & ~rightBits.Item4
      );
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> LessThan<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector256.GetElement(left, i);
        var rightVal = Vector256.GetElement(right, i);
        var less = Scalar<T>.LessThan(leftVal, rightVal);
        result = Vector256.WithElement(result, i, less ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> GreaterThan<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector256.GetElement(left, i);
        var rightVal = Vector256.GetElement(right, i);
        var greater = Scalar<T>.GreaterThan(leftVal, rightVal);
        result = Vector256.WithElement(result, i, greater ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [OverloadResolutionPriority(1)]
    public static Vector256<T> Equals<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector256.GetElement(left, i);
        var rightVal = Vector256.GetElement(right, i);
        var equals = Scalar<T>.ObjectEquals(leftVal, rightVal);
        result = Vector256.WithElement(result, i, equals ? Scalar<T>.AllBitsSet : Scalar<T>.Zero());
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Add<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector256.GetElement(left, i);
        var rightVal = Vector256.GetElement(right, i);
        var sum = Scalar<T>.Add(leftVal, rightVal);
        result = Vector256.WithElement(result, i, sum);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Subtract<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector256.GetElement(left, i);
        var rightVal = Vector256.GetElement(right, i);
        var diff = Scalar<T>.Subtract(leftVal, rightVal);
        result = Vector256.WithElement(result, i, diff);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Multiply<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector256.GetElement(left, i);
        var rightVal = Vector256.GetElement(right, i);
        var product = Scalar<T>.Multiply(leftVal, rightVal);
        result = Vector256.WithElement(result, i, product);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Divide<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var leftVal = Vector256.GetElement(left, i);
        var rightVal = Vector256.GetElement(right, i);
        var quotient = Scalar<T>.Divide(leftVal, rightVal);
        result = Vector256.WithElement(result, i, quotient);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Negate<T>(Vector256<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var val = Vector256.GetElement(vector, i);
        var negated = Scalar<T>.Negate(val);
        result = Vector256.WithElement(result, i, negated);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> OnesComplement<T>(Vector256<T> vector) where T : struct {
      var bits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref vector);
      var result = (~bits.Item1, ~bits.Item2, ~bits.Item3, ~bits.Item4);
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> BitwiseAnd<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
      var result = (
        leftBits.Item1 & rightBits.Item1,
        leftBits.Item2 & rightBits.Item2,
        leftBits.Item3 & rightBits.Item3,
        leftBits.Item4 & rightBits.Item4
      );
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> BitwiseOr<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
      var result = (
        leftBits.Item1 | rightBits.Item1,
        leftBits.Item2 | rightBits.Item2,
        leftBits.Item3 | rightBits.Item3,
        leftBits.Item4 | rightBits.Item4
      );
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Xor<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
      var result = (
        leftBits.Item1 ^ rightBits.Item1,
        leftBits.Item2 ^ rightBits.Item2,
        leftBits.Item3 ^ rightBits.Item3,
        leftBits.Item4 ^ rightBits.Item4
      );
      return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Abs<T>(Vector256<T> vector) where T : struct {
      if (Scalar<T>.IsUnsigned)
        return vector;

      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Abs(Vector256.GetElement(vector, i));
        result = Vector256.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Max<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Max(Vector256.GetElement(left, i), Vector256.GetElement(right, i));
        result = Vector256.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Min<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Min(Vector256.GetElement(left, i), Vector256.GetElement(right, i));
        result = Vector256.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Dot<T>(Vector256<T> left, Vector256<T> right) where T : struct {
      var result = Scalar<T>.Zero();
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var product = Scalar<T>.Multiply(Vector256.GetElement(left, i), Vector256.GetElement(right, i));
        result = Scalar<T>.Add(result, product);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(Vector256<T> vector) where T : struct {
      var result = Scalar<T>.Zero();
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i)
        result = Scalar<T>.Add(result, Vector256.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Sqrt<T>(Vector256<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Sqrt(Vector256.GetElement(vector, i));
        result = Vector256.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Ceiling<T>(Vector256<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Ceiling(Vector256.GetElement(vector, i));
        result = Vector256.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> Floor<T>(Vector256<T> vector) where T : struct {
      Unsafe.SkipInit(out Vector256<T> result);
      var count = Vector256<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.Floor(Vector256.GetElement(vector, i));
        result = Vector256.WithElement(result, i, value);
      }
      return result;
    }
  }

  // Extension operators for Vector256<T>
  extension<T>(Vector256<T>) where T : struct {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator +(Vector256<T> left, Vector256<T> right)
      => Vector256.Add(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator -(Vector256<T> left, Vector256<T> right)
      => Vector256.Subtract(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator *(Vector256<T> left, Vector256<T> right)
      => Vector256.Multiply(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator /(Vector256<T> left, Vector256<T> right)
      => Vector256.Divide(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator &(Vector256<T> left, Vector256<T> right)
      => Vector256.BitwiseAnd(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator |(Vector256<T> left, Vector256<T> right)
      => Vector256.BitwiseOr(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator ^(Vector256<T> left, Vector256<T> right)
      => Vector256.Xor(left, right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator ~(Vector256<T> value)
      => Vector256.OnesComplement(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator -(Vector256<T> value)
      => Vector256.Negate(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<T> operator +(Vector256<T> value) => value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector256<T> left, Vector256<T> right) {
      var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
      return leftBits.Item1 == rightBits.Item1
             && leftBits.Item2 == rightBits.Item2
             && leftBits.Item3 == rightBits.Item3
             && leftBits.Item4 == rightBits.Item4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector256<T> left, Vector256<T> right) {
      var leftBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector256<T>, (ulong, ulong, ulong, ulong)>(ref right);
      return leftBits.Item1 != rightBits.Item1
             || leftBits.Item2 != rightBits.Item2
             || leftBits.Item3 != rightBits.Item3
             || leftBits.Item4 != rightBits.Item4;
    }
  }
}

#endif
