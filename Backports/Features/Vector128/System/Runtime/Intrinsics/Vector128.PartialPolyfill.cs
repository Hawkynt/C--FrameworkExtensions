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
public static partial class Vector128Polyfills {

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        if (!Scalar<T>.GreaterThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GreaterThanAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        if (Scalar<T>.GreaterThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        if (!Scalar<T>.LessThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThanAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        if (Scalar<T>.LessThan(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAll<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        if (!Scalar<T>.ObjectEquals(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return false;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(Vector128<T> left, Vector128<T> right) where T : struct {
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i)
        if (Scalar<T>.ObjectEquals(Vector128.GetElement(left, i), Vector128.GetElement(right, i)))
          return true;
      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftRightArithmetic<T>(Vector128<T> vector, int shiftCount) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.ShiftRightArithmetic(Vector128.GetElement(vector, i), shiftCount);
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Shuffle(Vector128<byte> vector, Vector128<byte> indices) {
      Unsafe.SkipInit(out Vector128<byte> result);
      for (var i = 0; i < 16; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index < 16 ? Vector128.GetElement(vector, index) : (byte)0;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> Shuffle(Vector128<sbyte> vector, Vector128<sbyte> indices) {
      Unsafe.SkipInit(out Vector128<sbyte> result);
      for (var i = 0; i < 16; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index >= 0 && index < 16 ? Vector128.GetElement(vector, index) : (sbyte)0;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> Shuffle(Vector128<short> vector, Vector128<short> indices) {
      Unsafe.SkipInit(out Vector128<short> result);
      for (var i = 0; i < 8; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index >= 0 && index < 8 ? Vector128.GetElement(vector, index) : (short)0;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> Shuffle(Vector128<ushort> vector, Vector128<ushort> indices) {
      Unsafe.SkipInit(out Vector128<ushort> result);
      for (var i = 0; i < 8; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index < 8 ? Vector128.GetElement(vector, (int)index) : (ushort)0;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> Shuffle(Vector128<int> vector, Vector128<int> indices) {
      Unsafe.SkipInit(out Vector128<int> result);
      for (var i = 0; i < 4; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index >= 0 && index < 4 ? Vector128.GetElement(vector, index) : 0;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> Shuffle(Vector128<uint> vector, Vector128<uint> indices) {
      Unsafe.SkipInit(out Vector128<uint> result);
      for (var i = 0; i < 4; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index < 4 ? Vector128.GetElement(vector, (int)index) : 0u;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> Shuffle(Vector128<long> vector, Vector128<long> indices) {
      Unsafe.SkipInit(out Vector128<long> result);
      for (var i = 0; i < 2; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index >= 0 && index < 2 ? Vector128.GetElement(vector, (int)index) : 0L;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> Shuffle(Vector128<ulong> vector, Vector128<ulong> indices) {
      Unsafe.SkipInit(out Vector128<ulong> result);
      for (var i = 0; i < 2; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index < 2 ? Vector128.GetElement(vector, (int)index) : 0UL;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Shuffle(Vector128<float> vector, Vector128<int> indices) {
      Unsafe.SkipInit(out Vector128<float> result);
      for (var i = 0; i < 4; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index >= 0 && index < 4 ? Vector128.GetElement(vector, index) : 0f;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> Shuffle(Vector128<double> vector, Vector128<long> indices) {
      Unsafe.SkipInit(out Vector128<double> result);
      for (var i = 0; i < 2; ++i) {
        var index = Vector128.GetElement(indices, i);
        var value = index >= 0 && index < 2 ? Vector128.GetElement(vector, (int)index) : 0d;
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<short> Narrow(Vector128<int> lower, Vector128<int> upper) {
      Unsafe.SkipInit(out Vector128<short> result);
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (short)Vector128.GetElement(lower, i));
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i + 4, (short)Vector128.GetElement(upper, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ushort> Narrow(Vector128<uint> lower, Vector128<uint> upper) {
      Unsafe.SkipInit(out Vector128<ushort> result);
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (ushort)Vector128.GetElement(lower, i));
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i + 4, (ushort)Vector128.GetElement(upper, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> Narrow(Vector128<long> lower, Vector128<long> upper) {
      Unsafe.SkipInit(out Vector128<int> result);
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (int)Vector128.GetElement(lower, i));
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i + 2, (int)Vector128.GetElement(upper, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<uint> Narrow(Vector128<ulong> lower, Vector128<ulong> upper) {
      Unsafe.SkipInit(out Vector128<uint> result);
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (uint)Vector128.GetElement(lower, i));
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i + 2, (uint)Vector128.GetElement(upper, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> Narrow(Vector128<double> lower, Vector128<double> upper) {
      Unsafe.SkipInit(out Vector128<float> result);
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (float)Vector128.GetElement(lower, i));
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i + 2, (float)Vector128.GetElement(upper, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<sbyte> Narrow(Vector128<short> lower, Vector128<short> upper) {
      Unsafe.SkipInit(out Vector128<sbyte> result);
      for (var i = 0; i < 8; ++i)
        result = Vector128.WithElement(result, i, (sbyte)Vector128.GetElement(lower, i));
      for (var i = 0; i < 8; ++i)
        result = Vector128.WithElement(result, i + 8, (sbyte)Vector128.GetElement(upper, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> Narrow(Vector128<ushort> lower, Vector128<ushort> upper) {
      Unsafe.SkipInit(out Vector128<byte> result);
      for (var i = 0; i < 8; ++i)
        result = Vector128.WithElement(result, i, (byte)Vector128.GetElement(lower, i));
      for (var i = 0; i < 8; ++i)
        result = Vector128.WithElement(result, i + 8, (byte)Vector128.GetElement(upper, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> ConvertToInt32(Vector128<float> vector) {
      Unsafe.SkipInit(out Vector128<int> result);
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (int)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<float> ConvertToSingle(Vector128<int> vector) {
      Unsafe.SkipInit(out Vector128<float> result);
      for (var i = 0; i < 4; ++i)
        result = Vector128.WithElement(result, i, (float)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> ConvertToInt64(Vector128<double> vector) {
      Unsafe.SkipInit(out Vector128<long> result);
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (long)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> ConvertToDouble(Vector128<long> vector) {
      Unsafe.SkipInit(out Vector128<double> result);
      for (var i = 0; i < 2; ++i)
        result = Vector128.WithElement(result, i, (double)Vector128.GetElement(vector, i));
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftLeft<T>(Vector128<T> vector, int shiftCount) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.ShiftLeft(Vector128.GetElement(vector, i), shiftCount);
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ShiftRightLogical<T>(Vector128<T> vector, int shiftCount) where T : struct {
      Unsafe.SkipInit(out Vector128<T> result);
      var count = Vector128<T>.Count;
      for (var i = 0; i < count; ++i) {
        var value = Scalar<T>.ShiftRightLogical(Vector128.GetElement(vector, i), shiftCount);
        result = Vector128.WithElement(result, i, value);
      }
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<T> ConditionalSelect<T>(Vector128<T> condition, Vector128<T> left, Vector128<T> right) where T : struct {
      var conditionBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref condition);
      var leftBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref left);
      var rightBits = Unsafe.As<Vector128<T>, (ulong, ulong)>(ref right);
      var result = ((leftBits.Item1 & conditionBits.Item1) | (rightBits.Item1 & ~conditionBits.Item1),
                    (leftBits.Item2 & conditionBits.Item2) | (rightBits.Item2 & ~conditionBits.Item2));
      return Unsafe.As<(ulong, ulong), Vector128<T>>(ref result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ExtractMostSignificantBits<T>(Vector128<T> vector) where T : struct {
      // Get the raw bytes of the vector
      var bytes = Unsafe.As<Vector128<T>, (ulong lo, ulong hi)>(ref vector);
      uint result = 0;
      var elementSize = Unsafe.SizeOf<T>();
      var count = 16 / elementSize;

      // Extract sign bits based on element size
      if (elementSize == 1) {
        // 16 bytes - check bit 7 of each byte
        for (var i = 0; i < 8; ++i) {
          if (((bytes.lo >> (i * 8 + 7)) & 1) != 0) result |= 1u << i;
          if (((bytes.hi >> (i * 8 + 7)) & 1) != 0) result |= 1u << (i + 8);
        }
      } else if (elementSize == 2) {
        // 8 shorts - check bit 15 of each short
        for (var i = 0; i < 4; ++i) {
          if (((bytes.lo >> (i * 16 + 15)) & 1) != 0) result |= 1u << i;
          if (((bytes.hi >> (i * 16 + 15)) & 1) != 0) result |= 1u << (i + 4);
        }
      } else if (elementSize == 4) {
        // 4 ints - check bit 31 of each int
        if (((bytes.lo >> 31) & 1) != 0) result |= 1u;
        if (((bytes.lo >> 63) & 1) != 0) result |= 2u;
        if (((bytes.hi >> 31) & 1) != 0) result |= 4u;
        if (((bytes.hi >> 63) & 1) != 0) result |= 8u;
      } else if (elementSize == 8) {
        // 2 longs - check bit 63 of each long
        if (((bytes.lo >> 63) & 1) != 0) result |= 1u;
        if (((bytes.hi >> 63) & 1) != 0) result |= 2u;
      }
      return result;
    }

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
    public static Vector128<T> Shuffle<T>(Vector128<T> vector, Vector128<T> indices) where T : struct {
      var count = Vector128<T>.Count;
      Unsafe.SkipInit(out Vector128<T> result);
      for (var i = 0; i < count; ++i) {
        var idx = Convert.ToInt32(Vector128.GetElement(indices, i)) & (count - 1);
        result = Vector128.WithElement(result, i, Vector128.GetElement(vector, idx));
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
