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

#if !(SUPPORTS_VECTOR || OFFICIAL_VECTOR)

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Represents a single vector of a specified numeric type that is suitable for low-level optimization of parallel algorithms.
/// </summary>
/// <typeparam name="T">The type of the elements in the vector. T can be any primitive numeric type.</typeparam>
/// <remarks>
/// This is a polyfill for <see cref="Vector{T}"/> which is available in .NET Core 1.0+ and .NET Standard 2.1+.
/// This polyfill provides software-based vector operations - <see cref="Vector.IsHardwareAccelerated"/> returns <see langword="false"/>.
/// </remarks>
public readonly struct Vector<T> : IEquatable<Vector<T>> where T : struct {

  // Use 128-bit (16 bytes) as the baseline vector size - matches Vector128<T>
  private const int VECTOR_SIZE_IN_BYTES = 16;

  // Internal storage as two 64-bit values (128 bits total) - matches Vector128<T> pattern
  internal readonly ulong _lower;
  internal readonly ulong _upper;

  /// <summary>
  /// Returns the number of elements stored in the vector.
  /// </summary>
  public static int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => VECTOR_SIZE_IN_BYTES / Unsafe.SizeOf<T>();
  }

  /// <summary>
  /// Returns a vector containing all zeroes.
  /// </summary>
  public static Vector<T> Zero {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => default;
  }

  /// <summary>
  /// Returns a vector containing all ones.
  /// </summary>
  public static Vector<T> One {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(Scalar<T>.One);
  }

  /// <summary>
  /// Returns a vector with all bits set to 1.
  /// </summary>
  public static Vector<T> AllBitsSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(~0ul, ~0ul);
  }

  /// <summary>
  /// Gets the element at the specified index.
  /// </summary>
  /// <param name="index">The index of the element to get.</param>
  /// <returns>The element at the specified index.</returns>
  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (uint)index >= (uint)Count ? throw new IndexOutOfRangeException() : this._GetElement(index);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private T _GetElement(int index) {
    var size = Unsafe.SizeOf<T>();

    // Handle elements that span multiple ulongs (e.g., Int128, UInt128 if supported)
    if (size > 8) {
      var data = (this._lower, this._upper);
      return Unsafe.As<(ulong, ulong), T>(ref data);
    }

    var elementsPerUlong = 8 / size;
    var targetValue = index < elementsPerUlong ? this._lower : this._upper;
    var localIndex = index < elementsPerUlong ? index : index - elementsPerUlong;

    var shift = localIndex * size * 8;
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
    var piece = (targetValue >> shift) & mask;
    return Unsafe.As<ulong, T>(ref piece);
  }

  // Internal constructor from raw ulong values
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Vector(ulong lower, ulong upper) {
    this._lower = lower;
    this._upper = upper;
  }

  /// <summary>
  /// Creates a vector whose components are of a specified value.
  /// </summary>
  /// <param name="value">The value to assign to all elements.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector(T value) {
    var size = Unsafe.SizeOf<T>();

    // Handle elements that span multiple ulongs
    if (size > 8) {
      var data = Unsafe.As<T, (ulong, ulong)>(ref value);
      this._lower = data.Item1;
      this._upper = data.Item2;
      return;
    }

    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
    var chunk = Unsafe.As<T, ulong>(ref value) & mask;

    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;
    var count = Count;

    for (var i = 0; i < elementsPerUlong && i < count; ++i)
      lower |= chunk << (i * size * 8);

    for (var i = 0; i < elementsPerUlong && (i + elementsPerUlong) < count; ++i)
      upper |= chunk << (i * size * 8);

    this._lower = lower;
    this._upper = upper;
  }

  /// <summary>
  /// Creates a vector from a specified array.
  /// </summary>
  /// <param name="values">The source array.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector(T[] values) : this(values, 0) { }

  /// <summary>
  /// Creates a vector from a specified array starting at a specified index position.
  /// </summary>
  /// <param name="values">The source array.</param>
  /// <param name="index">The index position from which to create the vector.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector(T[] values, int index) {
    ArgumentNullException.ThrowIfNull(values);
    var count = Count;
    if (index < 0 || index > values.Length - count)
      throw new IndexOutOfRangeException();

    var size = Unsafe.SizeOf<T>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;

    for (var i = 0; i < count; ++i) {
      var element = values[index + i];
      var chunk = Unsafe.As<T, ulong>(ref element) & (size >= 8 ? ~0UL : (1UL << (size * 8)) - 1);
      var shift = (i % elementsPerUlong) * size * 8;

      if (i < elementsPerUlong)
        lower |= chunk << shift;
      else
        upper |= chunk << shift;
    }

    this._lower = lower;
    this._upper = upper;
  }

  /// <summary>
  /// Creates a vector from a specified span.
  /// </summary>
  /// <param name="values">The source span.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector(ReadOnlySpan<T> values) {
    var count = Count;
    if (values.Length < count)
      throw new IndexOutOfRangeException();

    var size = Unsafe.SizeOf<T>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;

    for (var i = 0; i < count; ++i) {
      var element = values[i];
      var chunk = Unsafe.As<T, ulong>(ref element) & (size >= 8 ? ~0UL : (1UL << (size * 8)) - 1);
      var shift = (i % elementsPerUlong) * size * 8;

      if (i < elementsPerUlong)
        lower |= chunk << shift;
      else
        upper |= chunk << shift;
    }

    this._lower = lower;
    this._upper = upper;
  }

  /// <summary>
  /// Copies the vector to the specified destination array.
  /// </summary>
  /// <param name="destination">The destination array.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(T[] destination) => this.CopyTo(destination, 0);

  /// <summary>
  /// Copies the vector to the specified destination array starting at the specified index.
  /// </summary>
  /// <param name="destination">The destination array.</param>
  /// <param name="startIndex">The starting index in the destination array.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(T[] destination, int startIndex) {
    ArgumentNullException.ThrowIfNull(destination);
    var count = Count;
    if (startIndex < 0 || startIndex > destination.Length - count)
      throw new IndexOutOfRangeException();

    for (var i = 0; i < count; ++i)
      destination[startIndex + i] = this[i];
  }

  /// <summary>
  /// Copies the vector to the specified destination span.
  /// </summary>
  /// <param name="destination">The destination span.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(Span<T> destination) {
    var count = Count;
    if (destination.Length < count)
      throw new ArgumentException("Destination span is too short.");

    for (var i = 0; i < count; ++i)
      destination[i] = this[i];
  }

  #region Operators

  /// <summary>
  /// Adds two vectors together.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator +(Vector<T> left, Vector<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Add);

  /// <summary>
  /// Subtracts the second vector from the first.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator -(Vector<T> left, Vector<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Subtract);

  /// <summary>
  /// Multiplies two vectors together.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator *(Vector<T> left, Vector<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Multiply);

  /// <summary>
  /// Divides the first vector by the second.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator /(Vector<T> left, Vector<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Divide);

  /// <summary>
  /// Negates a vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator -(Vector<T> value)
    => _PerformUnaryOperation(value, Scalar<T>.Negate);

  /// <summary>
  /// Computes the bitwise And of two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator &(Vector<T> left, Vector<T> right)
    => new(left._lower & right._lower, left._upper & right._upper);

  /// <summary>
  /// Computes the bitwise Or of two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator |(Vector<T> left, Vector<T> right)
    => new(left._lower | right._lower, left._upper | right._upper);

  /// <summary>
  /// Computes the bitwise exclusive Or of two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator ^(Vector<T> left, Vector<T> right)
    => new(left._lower ^ right._lower, left._upper ^ right._upper);

  /// <summary>
  /// Computes the ones-complement of a vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> operator ~(Vector<T> value)
    => new(~value._lower, ~value._upper);

  /// <summary>
  /// Determines whether two vectors are equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector<T> left, Vector<T> right) => left.Equals(right);

  /// <summary>
  /// Determines whether two vectors are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector<T> left, Vector<T> right) => !left.Equals(right);

  #endregion

  #region Helper Methods

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector<T> _PerformVectorOperation(Vector<T> left, Vector<T> right, Func<T, T, T> op) {
    var count = Count;
    var size = Unsafe.SizeOf<T>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;

    for (var i = 0; i < count; ++i) {
      var result = op(left[i], right[i]);
      var chunk = Unsafe.As<T, ulong>(ref result) & mask;
      var shift = (i % elementsPerUlong) * size * 8;

      if (i < elementsPerUlong)
        lower |= chunk << shift;
      else
        upper |= chunk << shift;
    }

    return new(lower, upper);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector<T> _PerformUnaryOperation(Vector<T> value, Func<T, T> op) {
    var count = Count;
    var size = Unsafe.SizeOf<T>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;

    for (var i = 0; i < count; ++i) {
      var result = op(value[i]);
      var chunk = Unsafe.As<T, ulong>(ref result) & mask;
      var shift = (i % elementsPerUlong) * size * 8;

      if (i < elementsPerUlong)
        lower |= chunk << shift;
      else
        upper |= chunk << shift;
    }

    return new(lower, upper);
  }

  #endregion

  #region IEquatable<Vector<T>>

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Vector<T> other) => this._lower == other._lower && this._upper == other._upper;

  /// <inheritdoc/>
  public override bool Equals(object? obj) => obj is Vector<T> other && this.Equals(other);

  /// <inheritdoc/>
  public override int GetHashCode() {
    unchecked {
      var hash = 17;
      hash = hash * 31 + this._lower.GetHashCode();
      hash = hash * 31 + this._upper.GetHashCode();
      return hash;
    }
  }

  #endregion

  /// <inheritdoc/>
  public override string ToString() {
    var count = Count;
    var elements = new string[count];
    for (var i = 0; i < count; ++i)
      elements[i] = this[i].ToString()!;

    return $"<{string.Join(", ", elements)}>";
  }

}

/// <summary>
/// Provides a collection of static convenience methods for creating, manipulating, combining, and converting generic vectors.
/// </summary>
/// <remarks>
/// This is a polyfill for <see cref="Vector"/> which is available in .NET Core 1.0+ and .NET Standard 2.1+.
/// This polyfill provides software-based vector operations - <see cref="IsHardwareAccelerated"/> returns <see langword="false"/>.
/// </remarks>
public static class Vector {

  /// <summary>
  /// Gets a value that indicates whether vector operations are subject to hardware acceleration through JIT intrinsic support.
  /// </summary>
  /// <value>
  /// <see langword="true"/> if vector operations are subject to hardware acceleration; otherwise, <see langword="false"/>.
  /// </value>
  /// <remarks>
  /// This polyfill always returns <see langword="false"/> since there is no hardware acceleration available.
  /// However, the vector operations are still functional and will process data correctly using software implementations.
  /// </remarks>
  public static bool IsHardwareAccelerated {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => false;
  }

  /// <summary>
  /// Returns a new vector whose elements signal whether the elements in left were greater than or equal to their corresponding elements in right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> GreaterThanOrEqual<T>(Vector<T> left, Vector<T> right) where T : struct
    => _CompareOp(left, right, Scalar<T>.GreaterThanOrEqual);

  /// <summary>
  /// Returns a new vector whose elements signal whether the elements in left were less than or equal to their corresponding elements in right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> LessThanOrEqual<T>(Vector<T> left, Vector<T> right) where T : struct
    => _CompareOp(left, right, Scalar<T>.LessThanOrEqual);

  /// <summary>
  /// Returns a new vector whose elements signal whether the elements in left were greater than their corresponding elements in right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> GreaterThan<T>(Vector<T> left, Vector<T> right) where T : struct
    => _CompareOp(left, right, Scalar<T>.GreaterThan);

  /// <summary>
  /// Returns a new vector whose elements signal whether the elements in left were less than their corresponding elements in right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> LessThan<T>(Vector<T> left, Vector<T> right) where T : struct
    => _CompareOp(left, right, Scalar<T>.LessThan);

  /// <summary>
  /// Returns a new vector whose elements signal whether the elements in left and right were equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Equals<T>(Vector<T> left, Vector<T> right) where T : struct
    => _CompareOp(left, right, Scalar<T>.ObjectEquals);

  /// <summary>
  /// Returns a value that indicates whether all elements in the vectors are pair-wise equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EqualsAll<T>(Vector<T> left, Vector<T> right) where T : struct => left.Equals(right);

  /// <summary>
  /// Returns a value that indicates whether any single pair of elements in the given vectors is equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EqualsAny<T>(Vector<T> left, Vector<T> right) where T : struct {
    for (var i = 0; i < Vector<T>.Count; ++i)
      if (Scalar<T>.ObjectEquals(left[i], right[i]))
        return true;
    return false;
  }

  /// <summary>
  /// Returns a value that indicates whether all elements in the vector are greater than their corresponding elements in the second vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanAll<T>(Vector<T> left, Vector<T> right) where T : struct {
    for (var i = 0; i < Vector<T>.Count; ++i)
      if (!Scalar<T>.GreaterThan(left[i], right[i]))
        return false;
    return true;
  }

  /// <summary>
  /// Returns a value that indicates whether any element in the first vector is greater than its corresponding element in the second vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanAny<T>(Vector<T> left, Vector<T> right) where T : struct {
    for (var i = 0; i < Vector<T>.Count; ++i)
      if (Scalar<T>.GreaterThan(left[i], right[i]))
        return true;
    return false;
  }

  /// <summary>
  /// Returns a value that indicates whether all elements in the first vector are less than their corresponding elements in the second vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanAll<T>(Vector<T> left, Vector<T> right) where T : struct {
    for (var i = 0; i < Vector<T>.Count; ++i)
      if (!Scalar<T>.LessThan(left[i], right[i]))
        return false;
    return true;
  }

  /// <summary>
  /// Returns a value that indicates whether any element in the first vector is less than its corresponding element in the second vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanAny<T>(Vector<T> left, Vector<T> right) where T : struct {
    for (var i = 0; i < Vector<T>.Count; ++i)
      if (Scalar<T>.LessThan(left[i], right[i]))
        return true;
    return false;
  }

  /// <summary>
  /// Returns a new vector by selecting values from an input vector using a set of indices.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Abs<T>(Vector<T> value) where T : struct
    => _UnaryOp(value, Scalar<T>.Abs);

  /// <summary>
  /// Creates a new vector from the bitwise AND of the inverted first vector and the second vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> AndNot<T>(Vector<T> left, Vector<T> right) where T : struct
    => ~left & right;

  /// <summary>
  /// Creates a new vector by reinterpreting the source vector as a vector of the target type.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<TTo> As<TFrom, TTo>(Vector<TFrom> vector) where TFrom : struct where TTo : struct
    => new(vector._lower, vector._upper);

  /// <summary>
  /// Creates a new <see cref="Vector{Byte}"/> from the given vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<byte> AsVectorByte<T>(Vector<T> value) where T : struct => As<T, byte>(value);

  /// <summary>
  /// Creates a new <see cref="Vector{Int32}"/> from the given vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<int> AsVectorInt32<T>(Vector<T> value) where T : struct => As<T, int>(value);

  /// <summary>
  /// Creates a new <see cref="Vector{Int64}"/> from the given vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<long> AsVectorInt64<T>(Vector<T> value) where T : struct => As<T, long>(value);

  /// <summary>
  /// Creates a new <see cref="Vector{Single}"/> from the given vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<float> AsVectorSingle<T>(Vector<T> value) where T : struct => As<T, float>(value);

  /// <summary>
  /// Creates a new <see cref="Vector{Double}"/> from the given vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<double> AsVectorDouble<T>(Vector<T> value) where T : struct => As<T, double>(value);

  /// <summary>
  /// Conditionally selects a value from two vectors on a bitwise basis.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> ConditionalSelect<T>(Vector<T> condition, Vector<T> left, Vector<T> right) where T : struct
    => (left & condition) | (right & ~condition);

  /// <summary>
  /// Computes the sum of all elements in a vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Sum<T>(Vector<T> value) where T : struct {
    var result = Scalar<T>.Zero();
    for (var i = 0; i < Vector<T>.Count; ++i)
      result = Scalar<T>.Add(result, value[i]);
    return result;
  }

  /// <summary>
  /// Computes the dot product of two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Dot<T>(Vector<T> left, Vector<T> right) where T : struct
    => Sum(left * right);

  /// <summary>
  /// Returns a new vector whose elements are the minimum of each pair of elements from the two input vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Min<T>(Vector<T> left, Vector<T> right) where T : struct
    => _BinaryOp(left, right, Scalar<T>.Min);

  /// <summary>
  /// Returns a new vector whose elements are the maximum of each pair of elements from the two input vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Max<T>(Vector<T> left, Vector<T> right) where T : struct
    => _BinaryOp(left, right, Scalar<T>.Max);

  /// <summary>
  /// Returns a new vector whose elements are the square root of each element in the input vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> SquareRoot<T>(Vector<T> value) where T : struct
    => _UnaryOp(value, Scalar<T>.Sqrt);

  /// <summary>
  /// Negates a vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Negate<T>(Vector<T> value) where T : struct => -value;

  /// <summary>
  /// Returns a new vector by performing a bitwise AND NOT operation on two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> OnesComplement<T>(Vector<T> value) where T : struct => ~value;

  /// <summary>
  /// Computes the bitwise AND of two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> BitwiseAnd<T>(Vector<T> left, Vector<T> right) where T : struct => left & right;

  /// <summary>
  /// Computes the bitwise OR of two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> BitwiseOr<T>(Vector<T> left, Vector<T> right) where T : struct => left | right;

  /// <summary>
  /// Computes the bitwise XOR of two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Xor<T>(Vector<T> left, Vector<T> right) where T : struct => left ^ right;

  /// <summary>
  /// Adds two vectors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Add<T>(Vector<T> left, Vector<T> right) where T : struct => left + right;

  /// <summary>
  /// Subtracts the second vector from the first.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Subtract<T>(Vector<T> left, Vector<T> right) where T : struct => left - right;

  /// <summary>
  /// Multiplies two vectors element-wise.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Multiply<T>(Vector<T> left, Vector<T> right) where T : struct => left * right;

  /// <summary>
  /// Multiplies a vector by a scalar.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Multiply<T>(Vector<T> left, T right) where T : struct => left * new Vector<T>(right);

  /// <summary>
  /// Multiplies a scalar by a vector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Multiply<T>(T left, Vector<T> right) where T : struct => new Vector<T>(left) * right;

  /// <summary>
  /// Divides the first vector by the second.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<T> Divide<T>(Vector<T> left, Vector<T> right) where T : struct => left / right;

  /// <summary>
  /// Narrows two <see cref="Vector{Double}"/> instances into one <see cref="Vector{Single}"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector<float> Narrow(Vector<double> low, Vector<double> high) {
    var count = Vector<float>.Count;
    var halfCount = Vector<double>.Count;
    var size = Unsafe.SizeOf<float>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;

    for (var i = 0; i < halfCount; ++i) {
      var val = (float)low[i];
      var chunk = Unsafe.As<float, ulong>(ref val) & 0xFFFFFFFF;
      var shift = (i % elementsPerUlong) * size * 8;
      lower |= chunk << shift;
    }

    for (var i = 0; i < halfCount; ++i) {
      var val = (float)high[i];
      var chunk = Unsafe.As<float, ulong>(ref val) & 0xFFFFFFFF;
      var shift = (i % elementsPerUlong) * size * 8;
      upper |= chunk << shift;
    }

    return new(lower, upper);
  }

  /// <summary>
  /// Widens a <see cref="Vector{Single}"/> into two <see cref="Vector{Double}"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Widen(Vector<float> source, out Vector<double> low, out Vector<double> high) {
    var halfCount = Vector<double>.Count;

    ulong lowLower = 0, lowUpper = 0;
    ulong highLower = 0, highUpper = 0;

    for (var i = 0; i < halfCount; ++i) {
      var val = (double)source[i];
      var bits = BitConverter.DoubleToInt64Bits(val);
      if (i == 0)
        lowLower = (ulong)bits;
      else
        lowUpper = (ulong)bits;
    }

    for (var i = 0; i < halfCount; ++i) {
      var val = (double)source[i + halfCount];
      var bits = BitConverter.DoubleToInt64Bits(val);
      if (i == 0)
        highLower = (ulong)bits;
      else
        highUpper = (ulong)bits;
    }

    low = new(lowLower, lowUpper);
    high = new(highLower, highUpper);
  }

  #region Private Helpers

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector<T> _CompareOp<T>(Vector<T> left, Vector<T> right, Func<T, T, bool> compare) where T : struct {
    var count = Vector<T>.Count;
    var size = Unsafe.SizeOf<T>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;
    var allBitsSet = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;

    for (var i = 0; i < count; ++i) {
      var chunk = compare(left[i], right[i]) ? allBitsSet : 0UL;
      var shift = (i % elementsPerUlong) * size * 8;

      if (i < elementsPerUlong)
        lower |= chunk << shift;
      else
        upper |= chunk << shift;
    }

    return new(lower, upper);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector<T> _BinaryOp<T>(Vector<T> left, Vector<T> right, Func<T, T, T> op) where T : struct {
    var count = Vector<T>.Count;
    var size = Unsafe.SizeOf<T>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;

    for (var i = 0; i < count; ++i) {
      var result = op(left[i], right[i]);
      var chunk = Unsafe.As<T, ulong>(ref result) & mask;
      var shift = (i % elementsPerUlong) * size * 8;

      if (i < elementsPerUlong)
        lower |= chunk << shift;
      else
        upper |= chunk << shift;
    }

    return new(lower, upper);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector<T> _UnaryOp<T>(Vector<T> value, Func<T, T> op) where T : struct {
    var count = Vector<T>.Count;
    var size = Unsafe.SizeOf<T>();
    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;

    for (var i = 0; i < count; ++i) {
      var result = op(value[i]);
      var chunk = Unsafe.As<T, ulong>(ref result) & mask;
      var shift = (i % elementsPerUlong) * size * 8;

      if (i < elementsPerUlong)
        lower |= chunk << shift;
      else
        upper |= chunk << shift;
    }

    return new(lower, upper);
  }

  #endregion

}

#endif
