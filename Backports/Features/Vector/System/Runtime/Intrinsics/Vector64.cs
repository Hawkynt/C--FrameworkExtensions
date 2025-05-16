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

#if !SUPPORTS_VECTOR_64

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

/// <summary>
/// Provides a set of static methods for creating and working with 64-bit vectors.
/// </summary>
public static class Vector64 {

  /// <summary>
  /// Gets a value that indicates whether 64-bit vector operations are subject to hardware acceleration through JIT intrinsic support.
  /// </summary>
  /// <value>Always returns <see langword="false"/> for polyfill.</value>
  public static bool IsHardwareAccelerated => false;

  #region Create Methods

  /// <summary>Creates a new <see cref="Vector64{T}"/> instance with all elements initialized to the specified value.</summary>
  /// <param name="value">The value that all elements will be initialized to.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A new <see cref="Vector64{T}"/> with all elements initialized to <paramref name="value"/>.</returns>
  public static Vector64<T> Create<T>(T value) where T : unmanaged => Vector64<T>.Create(value);

  /// <summary>Creates a new <see cref="Vector64{T}"/> from the given <see cref="ReadOnlySpan{T}"/>.</summary>
  /// <param name="values">The readonly span from which to create the vector.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A new <see cref="Vector64{T}"/> with its elements set to the first <see cref="Vector64{T}.Count"/> elements from <paramref name="values"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="values"/> has fewer than <see cref="Vector64{T}.Count"/> elements.</exception>
  public static Vector64<T> Create<T>(ReadOnlySpan<T> values) where T : unmanaged => Vector64<T>.Create(values);

  /// <summary>Creates a new <see cref="Vector64{T}"/> from the given array.</summary>
  /// <param name="values">The array from which to create the vector.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A new <see cref="Vector64{T}"/> with its elements set to the first <see cref="Vector64{T}.Count"/> elements from <paramref name="values"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="values"/> has fewer than <see cref="Vector64{T}.Count"/> elements.</exception>
  public static Vector64<T> Create<T>(T[] values) where T : unmanaged => Vector64<T>.Create(values);

  /// <summary>Creates a new <see cref="Vector64{T}"/> from the given array.</summary>
  /// <param name="values">The array from which to create the vector.</param>
  /// <param name="index">The index in <paramref name="values"/> from which to create the vector.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A new <see cref="Vector64{T}"/> with its elements set to the <see cref="Vector64{T}.Count"/> elements starting from <paramref name="index"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="values"/>, starting from <paramref name="index"/>, is less than <see cref="Vector64{T}.Count"/>.</exception>
  public static Vector64<T> Create<T>(T[] values, int index) where T : unmanaged => Vector64<T>.Create(values, index);

  #endregion

  #region CreateScalar Methods

  /// <summary>Creates a new <see cref="Vector64{T}"/> with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
  /// <param name="value">The value that element 0 will be initialized to.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A new <see cref="Vector64{T}"/> with the first element initialized to <paramref name="value"/> and the remaining elements initialized to zero.</returns>
  public static Vector64<T> CreateScalar<T>(T value) where T : unmanaged => Vector64<T>.CreateScalar(value);

  /// <summary>Creates a new <see cref="Vector64{T}"/> with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
  /// <param name="value">The value that element 0 will be initialized to.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A new <see cref="Vector64{T}"/> with the first element initialized to <paramref name="value"/> and the remaining elements left uninitialized.</returns>
  public static Vector64<T> CreateScalarUnsafe<T>(T value) where T : unmanaged => Vector64<T>.CreateScalarUnsafe(value);

  #endregion

  #region Type-specific Creates

  /// <summary>Creates a new <see cref="Vector64{Byte}"/> instance from the given values.</summary>
  /// <param name="e0">The value that element 0 will be initialized to.</param>
  /// <param name="e1">The value that element 1 will be initialized to.</param>
  /// <param name="e2">The value that element 2 will be initialized to.</param>
  /// <param name="e3">The value that element 3 will be initialized to.</param>
  /// <param name="e4">The value that element 4 will be initialized to.</param>
  /// <param name="e5">The value that element 5 will be initialized to.</param>
  /// <param name="e6">The value that element 6 will be initialized to.</param>
  /// <param name="e7">The value that element 7 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{Byte}"/> from the given values.</returns>
  public static Vector64<byte> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7) =>
    Vector64<byte>.Create(e0, e1, e2, e3, e4, e5, e6, e7);

  /// <summary>Creates a new <see cref="Vector64{SByte}"/> instance from the given values.</summary>
  /// <param name="e0">The value that element 0 will be initialized to.</param>
  /// <param name="e1">The value that element 1 will be initialized to.</param>
  /// <param name="e2">The value that element 2 will be initialized to.</param>
  /// <param name="e3">The value that element 3 will be initialized to.</param>
  /// <param name="e4">The value that element 4 will be initialized to.</param>
  /// <param name="e5">The value that element 5 will be initialized to.</param>
  /// <param name="e6">The value that element 6 will be initialized to.</param>
  /// <param name="e7">The value that element 7 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{SByte}"/> from the given values.</returns>
  public static Vector64<sbyte> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7) =>
    Vector64<sbyte>.Create(e0, e1, e2, e3, e4, e5, e6, e7);

  /// <summary>Creates a new <see cref="Vector64{Int16}"/> instance from the given values.</summary>
  /// <param name="e0">The value that element 0 will be initialized to.</param>
  /// <param name="e1">The value that element 1 will be initialized to.</param>
  /// <param name="e2">The value that element 2 will be initialized to.</param>
  /// <param name="e3">The value that element 3 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{Int16}"/> from the given values.</returns>
  public static Vector64<short> Create(short e0, short e1, short e2, short e3) =>
    Vector64<short>.Create(e0, e1, e2, e3);

  /// <summary>Creates a new <see cref="Vector64{UInt16}"/> instance from the given values.</summary>
  /// <param name="e0">The value that element 0 will be initialized to.</param>
  /// <param name="e1">The value that element 1 will be initialized to.</param>
  /// <param name="e2">The value that element 2 will be initialized to.</param>
  /// <param name="e3">The value that element 3 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{UInt16}"/> from the given values.</returns>
  public static Vector64<ushort> Create(ushort e0, ushort e1, ushort e2, ushort e3) =>
    Vector64<ushort>.Create(e0, e1, e2, e3);

  /// <summary>Creates a new <see cref="Vector64{Int32}"/> instance from the given values.</summary>
  /// <param name="e0">The value that element 0 will be initialized to.</param>
  /// <param name="e1">The value that element 1 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{Int32}"/> from the given values.</returns>
  public static Vector64<int> Create(int e0, int e1) => Vector64<int>.Create(e0, e1);

  /// <summary>Creates a new <see cref="Vector64{UInt32}"/> instance from the given values.</summary>
  /// <param name="e0">The value that element 0 will be initialized to.</param>
  /// <param name="e1">The value that element 1 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{UInt32}"/> from the given values.</returns>
  public static Vector64<uint> Create(uint e0, uint e1) => Vector64<uint>.Create(e0, e1);

  /// <summary>Creates a new <see cref="Vector64{Single}"/> instance from the given values.</summary>
  /// <param name="e0">The value that element 0 will be initialized to.</param>
  /// <param name="e1">The value that element 1 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{Single}"/> from the given values.</returns>
  public static Vector64<float> Create(float e0, float e1) => Vector64<float>.Create(e0, e1);

  /// <summary>Creates a new <see cref="Vector64{UInt64}"/> instance from the given values.</summary>
  /// <param name="value">The value that the element will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{UInt64}"/> from the given value.</returns>
  public static Vector64<ulong> Create(ulong value) => Vector64<ulong>.Create(value);

  /// <summary>Creates a new <see cref="Vector64{Int64}"/> instance from the given values.</summary>
  /// <param name="value">The value that the element will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{Int64}"/> from the given value.</returns>
  public static Vector64<long> Create(long value) => Vector64<long>.Create(value);

  /// <summary>Creates a new <see cref="Vector64{Double}"/> instance from the given values.</summary>
  /// <param name="value">The value that the element will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{Double}"/> from the given value.</returns>
  public static Vector64<double> Create(double value) => Vector64<double>.Create(value);

  #endregion

  #region Vector Operations

  /// <summary>Computes the absolute value of each element in a vector.</summary>
  /// <param name="vector">The vector that will have its absolute value computed.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A vector whose elements are the absolute value of the elements in <paramref name="vector"/>.</returns>
  public static Vector64<T> Abs<T>(Vector64<T> vector) where T : unmanaged => Vector64<T>.Abs(vector);

  /// <summary>Adds two vectors to compute their sum.</summary>
  /// <param name="left">The vector to add with <paramref name="right"/>.</param>
  /// <param name="right">The vector to add with <paramref name="left"/>.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
  public static Vector64<T> Add<T>(Vector64<T> left, Vector64<T> right) where T : unmanaged => left + right;

  /// <summary>Computes the bitwise-and of a given vector and the ones complement of another vector.</summary>
  /// <param name="left">The vector to bitwise-and with <paramref name="right"/>.</param>
  /// <param name="right">The vector to that is ones-complemented before being bitwise-and with <paramref name="left"/>.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>The bitwise-and of <paramref name="left"/> and the ones-complement of <paramref name="right"/>.</returns>
  public static Vector64<T> AndNot<T>(Vector64<T> left, Vector64<T> right) where T : unmanaged => left & ~right;

  /// <summary>Reinterprets a <see cref="Vector64{T}"/> as a new <see cref="Vector64{U}"/>.</summary>
  /// <typeparam name="TFrom">The type of the elements in the source vector.</typeparam>
  /// <typeparam name="TTo">The type of the elements in the destination vector.</typeparam>
  /// <param name="vector">The vector to reinterpret.</param>
  /// <returns>
  /// <paramref name="vector"/> reinterpreted as a new <see cref="Vector64{U}"/>.
  /// </returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="vector"/> (<typeparamref name="TFrom"/>) or the type of the target (<typeparamref name="TTo"/>) is not supported.</exception>
  public static Vector64<TTo> As<TFrom, TTo>(this Vector64<TFrom> vector)
    where TFrom : unmanaged
    where TTo : unmanaged => vector.As<TTo>();

  /// <summary>Computes the bitwise-and of two vectors.</summary>
  /// <param name="left">The vector to bitwise-and with <paramref name="right"/>.</param>
  /// <param name="right">The vector to bitwise-and with <paramref name="left"/>.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>The bitwise-and of <paramref name="left"/> and <paramref name="right"/>.</returns>
  public static Vector64<T> BitwiseAnd<T>(Vector64<T> left, Vector64<T> right) where T : unmanaged => left & right;

  /// <summary>Computes the bitwise-or of two vectors.</summary>
  /// <param name="left">The vector to bitwise-or with <paramref name="right"/>.</param>
  /// <param name="right">The vector to bitwise-or with <paramref name="left"/>.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>The bitwise-or of <paramref name="left"/> and <paramref name="right"/>.</returns>
  public static Vector64<T> BitwiseOr<T>(Vector64<T> left, Vector64<T> right) where T : unmanaged => left | right;

  /// <summary>Computes the ceiling of each element in a vector.</summary>
  /// <param name="vector">The vector that will have its ceiling computed.</param>
  /// <returns>A vector whose elements are the ceiling of the elements in <paramref name="vector"/>.</returns>
  public static Vector64<float> Ceiling(Vector64<float> vector) => Vector64<float>.Ceiling(vector);

  /// <summary>Computes the ceiling of each element in a vector.</summary>
  /// <param name="vector">The vector that will have its ceiling computed.</param>
  /// <returns>A vector whose elements are the ceiling of the elements in <paramref name="vector"/>.</returns>
  public static Vector64<double> Ceiling(Vector64<double> vector) => Vector64<double>.Ceiling(vector);

  /// <summary>Compares two vectors to determine if all elements are equal.</summary>
  /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
  /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns><see langword="true"/> if all elements in <paramref name="left"/> were equal to the corresponding element in <paramref name="right"/>.</returns>
  public static bool EqualsAll<T>(Vector64<T> left, Vector64<T> right) where T : unmanaged => left.Equals(right);

  /// <summary>Compares two vectors to determine if any elements are equal.</summary>
  /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
  /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns><see langword="true"/> if any elements in <paramref name="left"/> were equal to the corresponding element in <paramref name="right"/>.</returns>
  public static bool EqualsAny<T>(Vector64<T> left, Vector64<T> right) where T : unmanaged => Vector64<T>.EqualsAny(left, right);

  /// <summary>Gets the element at the specified index.</summary>
  /// <param name="vector">The vector to get the element from.</param>
  /// <param name="index">The index of the element to get.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>The value of the element at <paramref name="index"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was less than zero or greater than the number of elements.</exception>
  public static T GetElement<T>(this Vector64<T> vector, int index) where T : unmanaged => vector.GetElement(index);

  /// <summary>Creates a new <see cref="Vector64{T}"/> with the element at the specified index set to the specified value and the remaining elements set to the same value as that in the given vector.</summary>
  /// <param name="vector">The vector to get the remaining elements from.</param>
  /// <param name="index">The index of the element to set.</param>
  /// <param name="value">The value to set the element to.</param>
  /// <typeparam name="T">The type of the elements in the vector.</typeparam>
  /// <returns>A <see cref="Vector64{T}"/> with the value of the element at <paramref name="index"/> set to <paramref name="value"/> and the remaining elements set to the same value as that in <paramref name="vector"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was less than zero or greater than the number of elements.</exception>
  public static Vector64<T> WithElement<T>(this Vector64<T> vector, int index, T value) where T : unmanaged => vector.WithElement(index, value);

  #endregion

}

public readonly struct Vector64<T> : IEquatable<Vector64<T>> where T : unmanaged {
  
  // Internal storage as a 64-bit value
  private readonly ulong _value;

  public static bool IsSupported => typeof(T) == typeof(byte)
                                    || typeof(T) == typeof(double)
                                    || typeof(T) == typeof(short)
                                    || typeof(T) == typeof(int)
                                    || typeof(T) == typeof(long)
                                    || typeof(T) == typeof(nint)
                                    || typeof(T) == typeof(sbyte)
                                    || typeof(T) == typeof(float)
                                    || typeof(T) == typeof(ushort)
                                    || typeof(T) == typeof(uint)
                                    || typeof(T) == typeof(ulong)
                                    || typeof(T) == typeof(nuint);

  /// <summary>Gets the number of <typeparamref name="T"/> that are in a <see cref="Vector64{T}"/>.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static int Count {
    get {
      ThrowIfNotSupported();
      return 8 / Unsafe.SizeOf<T>();
    }
  }

  /// <summary>Gets a new <see cref="Vector64{T}"/> with all elements initialized to zero.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> Zero {
    get {
      ThrowIfNotSupported();
      return default;
    }
  }

  /// <summary>Gets a new <see cref="Vector64{T}"/> with all elements initialized to one.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> One {
    get {
      ThrowIfNotSupported();
      return Create(GetOneValue());
    }
  }

  /// <summary>Gets a new <see cref="Vector64{T}"/> with all bits set to 1.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> AllBitsSet => new(~0ul);

  // Internal constructor from raw ulong
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Vector64(ulong value) => this._value = value;
 
  #region Operators

  /// <summary>Computes the bitwise-and of two vectors.</summary>
  /// <param name="left">The vector to bitwise-and with <paramref name="right"/>.</param>
  /// <param name="right">The vector to bitwise-and with <paramref name="left"/>.</param>
  /// <returns>The bitwise-and of <paramref name="left"/> and <paramref name="right"/>.</returns>
  public static Vector64<T> operator &(Vector64<T> left, Vector64<T> right) => new(left._value & right._value);

  /// <summary>Computes the bitwise-or of two vectors.</summary>
  /// <param name="left">The vector to bitwise-or with <paramref name="right"/>.</param>
  /// <param name="right">The vector to bitwise-or with <paramref name="left"/>.</param>
  /// <returns>The bitwise-or of <paramref name="left"/> and <paramref name="right"/>.</returns>
  public static Vector64<T> operator |(Vector64<T> left, Vector64<T> right) => new(left._value | right._value);

  /// <summary>Computes the exclusive-or of two vectors.</summary>
  /// <param name="left">The vector to exclusive-or with <paramref name="right"/>.</param>
  /// <param name="right">The vector to exclusive-or with <paramref name="left"/>.</param>
  /// <returns>The exclusive-or of <paramref name="left"/> and <paramref name="right"/>.</returns>
  public static Vector64<T> operator ^(Vector64<T> left, Vector64<T> right) => new(left._value ^ right._value);

  /// <summary>Computes the ones-complement of a vector.</summary>
  /// <param name="vector">The vector whose ones-complement is to be computed.</param>
  /// <returns>A vector whose elements are the ones-complement of the corresponding elements in <paramref name="vector"/>.</returns>
  public static Vector64<T> operator ~(Vector64<T> vector) => new(~vector._value);

  /// <summary>Adds two vectors to compute their sum.</summary>
  /// <param name="left">The vector to add with <paramref name="right"/>.</param>
  /// <param name="right">The vector to add with <paramref name="left"/>.</param>
  /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> operator +(Vector64<T> left, Vector64<T> right) {
    ThrowIfNotSupported();
    return PerformVectorOperation(left, right, AddScalar);
  }

  /// <summary>Subtracts two vectors to compute their difference.</summary>
  /// <param name="left">The vector from which <paramref name="right"/> will be subtracted.</param>
  /// <param name="right">The vector to subtract from <paramref name="left"/>.</param>
  /// <returns>The difference of <paramref name="left"/> and <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> operator -(Vector64<T> left, Vector64<T> right) {
    ThrowIfNotSupported();
    return PerformVectorOperation(left, right, SubtractScalar);
  }

  /// <summary>Multiplies two vectors to compute their element-wise product.</summary>
  /// <param name="left">The vector to multiply with <paramref name="right"/>.</param>
  /// <param name="right">The vector to multiply with <paramref name="left"/>.</param>
  /// <returns>The element-wise product of <paramref name="left"/> and <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> operator *(Vector64<T> left, Vector64<T> right) {
    ThrowIfNotSupported();
    return PerformVectorOperation(left, right, MultiplyScalar);
  }

  /// <summary>Divides two vectors to compute their quotient.</summary>
  /// <param name="left">The vector that will be divided by <paramref name="right"/>.</param>
  /// <param name="right">The vector that will divide <paramref name="left"/>.</param>
  /// <returns>The quotient of <paramref name="left"/> divided by <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> operator /(Vector64<T> left, Vector64<T> right) {
    ThrowIfNotSupported();
    return PerformVectorOperation(left, right, DivideScalar);
  }

  /// <summary>Computes the unary negation of a vector.</summary>
  /// <param name="vector">The vector to negate.</param>
  /// <returns>A vector whose elements are the unary negation of the corresponding elements in <paramref name="vector"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="vector"/> (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> operator -(Vector64<T> vector) {
    ThrowIfNotSupported();
    return Zero - vector;
  }

  /// <summary>Computes the unary plus of a vector.</summary>
  /// <param name="vector">The vector for which to compute its unary plus.</param>
  /// <returns>A copy of <paramref name="vector"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="vector"/> (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> operator +(Vector64<T> vector) {
    ThrowIfNotSupported();
    return vector;
  }

  /// <summary>Compares two vectors to determine if they are equal on a per-element basis.</summary>
  /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
  /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
  /// <returns><see langword="true"/> if all corresponding elements were equal; otherwise, <see langword="false"/>.</returns>
  public static bool operator ==(Vector64<T> left, Vector64<T> right) => left._value == right._value;

  /// <summary>Compares two vectors to determine if they are not equal on a per-element basis.</summary>
  /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
  /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
  /// <returns><see langword="true"/> if any corresponding elements were not equal; otherwise, <see langword="false"/>.</returns>
  public static bool operator !=(Vector64<T> left, Vector64<T> right) => left._value != right._value;

  #endregion

  #region Factory Methods

  /// <summary>Creates a new <see cref="Vector64{T}"/> instance with all elements initialized to the specified value.</summary>
  /// <param name="value">The value that all elements will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{T}"/> with all elements initialized to <paramref name="value"/>.</returns>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> Create(T value) {
    ThrowIfNotSupported();
    var vector = default(Vector64<T>);
    return FillVector(vector, value);
  }

  /// <summary>Creates a new <see cref="Vector64{T}"/> from the given <see cref="ReadOnlySpan{T}"/>.</summary>
  /// <param name="values">The readonly span from which to create the vector.</param>
  /// <returns>A new <see cref="Vector64{T}"/> with its elements set to the first <see cref="Count"/> elements from <paramref name="values"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="values"/> has fewer than <see cref="Count"/> elements.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> Create(ReadOnlySpan<T> values) {
    ThrowIfNotSupported();
    return CreateFromSpan(values);
  }

  /// <summary>Creates a new <see cref="Vector64{T}"/> from the given array.</summary>
  /// <param name="values">The array from which to create the vector.</param>
  /// <returns>A new <see cref="Vector64{T}"/> with its elements set to the first <see cref="Count"/> elements from <paramref name="values"/>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="values"/> has fewer than <see cref="Count"/> elements.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> Create(T[] values) {
    if(values==null)
      AlwaysThrow.ArgumentNullException(nameof(values));
    
    return Create(new ReadOnlySpan<T>(values));
  }

  /// <summary>Creates a new <see cref="Vector64{T}"/> from the given array.</summary>
  /// <param name="values">The array from which to create the vector.</param>
  /// <param name="index">The index in <paramref name="values"/> from which to create the vector.</param>
  /// <returns>A new <see cref="Vector64{T}"/> with its elements set to the <see cref="Count"/> elements starting from <paramref name="index"/>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="values"/>, starting from <paramref name="index"/>, is less than <see cref="Count"/>.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> Create(T[] values, int index) => Create(new ReadOnlySpan<T>(values, index, Count));

  /// <summary>Creates a new <see cref="Vector64{T}"/> with the first element initialized to the specified value and the remaining elements initialized to zero.</summary>
  /// <param name="value">The value that element 0 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{T}"/> with the first element initialized to <paramref name="value"/> and the remaining elements initialized to zero.</returns>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> CreateScalar(T value) {
    ThrowIfNotSupported();
    var vector = Zero;
    return vector.WithElement(0, value);
  }

  /// <summary>Creates a new <see cref="Vector64{T}"/> with the first element initialized to the specified value and the remaining elements left uninitialized.</summary>
  /// <param name="value">The value that element 0 will be initialized to.</param>
  /// <returns>A new <see cref="Vector64{T}"/> with the first element initialized to <paramref name="value"/> and the remaining elements left uninitialized.</returns>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> CreateScalarUnsafe(T value) {
    ThrowIfNotSupported();
    var vector = default(Vector64<T>);
    return vector.WithElement(0, value);
  }

  #endregion

  #region Type-specific Creates

  /// <summary>Creates a new <see cref="Vector64{Byte}"/> instance from the given values.</summary>
  public static Vector64<T> Create(byte e0, byte e1, byte e2, byte e3, byte e4, byte e5, byte e6, byte e7) => Vector64<byte>.CreateFromSpan([e0, e1, e2, e3, e4, e5, e6, e7]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{SByte}"/> instance from the given values.</summary>
  public static Vector64<T> Create(sbyte e0, sbyte e1, sbyte e2, sbyte e3, sbyte e4, sbyte e5, sbyte e6, sbyte e7) => Vector64<sbyte>.CreateFromSpan([e0, e1, e2, e3, e4, e5, e6, e7]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{Int16}"/> instance from the given values.</summary>
  public static Vector64<T> Create(short e0, short e1, short e2, short e3) => Vector64<short>.CreateFromSpan([e0, e1, e2, e3]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{UInt16}"/> instance from the given values.</summary>
  public static Vector64<T> Create(ushort e0, ushort e1, ushort e2, ushort e3) => Vector64<ushort>.CreateFromSpan([e0, e1, e2, e3]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{Int32}"/> instance from the given values.</summary>
  public static Vector64<T> Create(int e0, int e1) => Vector64<int>.CreateFromSpan([e0, e1]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{UInt32}"/> instance from the given values.</summary>
  public static Vector64<T> Create(uint e0, uint e1) => Vector64<uint>.CreateFromSpan([e0, e1]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{Single}"/> instance from the given values.</summary>
  public static Vector64<T> Create(float e0, float e1) => Vector64<float>.CreateFromSpan([e0, e1]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{UInt64}"/> instance from the given value.</summary>
  public static Vector64<T> Create(ulong value) => Vector64<ulong>.CreateFromSpan([value]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{Int64}"/> instance from the given value.</summary>
  public static Vector64<T> Create(long value) => Vector64<long>.CreateFromSpan([value]).As<T>();

  /// <summary>Creates a new <see cref="Vector64{Double}"/> instance from the given value.</summary>
  public static Vector64<T> Create(double value) => Vector64<double>.CreateFromSpan([value]).As<T>();

  #endregion

  #region Vector Operations

  /// <summary>Computes the absolute value of each element in a vector.</summary>
  /// <param name="vector">The vector that will have its absolute value computed.</param>
  /// <returns>A vector whose elements are the absolute value of the elements in <paramref name="vector"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="vector"/> (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> Abs(Vector64<T> vector) {
    ThrowIfNotSupported();
    return PerformUnaryOperation(vector, AbsScalar);
  }

  /// <summary>Computes the ceiling of each element in a vector.</summary>
  /// <param name="vector">The vector that will have its ceiling computed.</param>
  /// <returns>A vector whose elements are the ceiling of the elements in <paramref name="vector"/>.</returns>
  public static Vector64<float> Ceiling(Vector64<float> vector) {
    ThrowIfWrongBaseType<float>();
    return PerformUnaryOperation(vector.As<T>(), CeilingScalar).As<float>();
  }

  /// <summary>Computes the ceiling of each element in a vector.</summary>
  /// <param name="vector">The vector that will have its ceiling computed.</param>
  /// <returns>A vector whose elements are the ceiling of the elements in <paramref name="vector"/>.</returns>
  public static Vector64<double> Ceiling(Vector64<double> vector) {
    ThrowIfWrongBaseType<double>();
    return PerformUnaryOperation(vector.As<T>(), CeilingScalar).As<double>();
  }

  /// <summary>Compares two vectors to determine if any elements are equal.</summary>
  /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
  /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
  /// <returns><see langword="true"/> if any elements in <paramref name="left"/> were equal to the corresponding element in <paramref name="right"/>.</returns>
  public static bool EqualsAny(Vector64<T> left, Vector64<T> right) {
    ThrowIfNotSupported();
    for (var i = 0; i < Count; ++i)
      if (EqualityComparer<T>.Default.Equals(left.GetElement(i), right.GetElement(i)))
        return true;

    return false;
  }

  #endregion

  #region Instance Methods

  /// <summary>Copies the elements of this vector to a specified array.</summary>
  /// <param name="destination">The destination array.</param>
  /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">The number of elements in the current instance is greater than in the array.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public void CopyTo(T[] destination) {
    if(destination==null)
      AlwaysThrow.ArgumentNullException(nameof(destination));

    this.CopyTo(destination, 0);
  }

  /// <summary>Copies the elements of this vector to a specified array starting at the specified destination index.</summary>
  /// <param name="destination">The destination array.</param>
  /// <param name="startIndex">The index at which to copy the first element of the vector.</param>
  /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> is less than zero or greater than the length of the array.</exception>
  /// <exception cref="ArgumentException">The number of elements in the current instance is greater than in the array.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public void CopyTo(T[] destination, int startIndex) {
    if (destination == null)
      AlwaysThrow.ArgumentNullException(nameof(destination));

    this.CopyTo(new Span<T>(destination, startIndex, Count));
  }

  /// <summary>Copies the elements of this vector to a specified span of memory.</summary>
  /// <param name="destination">The destination span of memory.</param>
  /// <exception cref="ArgumentException">The number of elements in the current instance is greater than in the span.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public void CopyTo(Span<T> destination) {
    ThrowIfNotSupported();
    for (var i = 0; i < Count; ++i)
      destination[i] = this.GetElement(i);
  }

  /// <summary>Returns a value that indicates whether this instance and another vector are equal.</summary>
  /// <param name="other">The other vector.</param>
  /// <returns><see langword="true"/> if the two vectors are equal; otherwise, <see langword="false"/>.</returns>
  public bool Equals(Vector64<T> other) => this._value == other._value;

  /// <summary>Returns a value that indicates whether this instance and a specified object are equal.</summary>
  /// <param name="obj">The object to compare with the current instance.</param>
  /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Vector64{T}"/> and the two vectors are equal; otherwise, <see langword="false"/>.</returns>
  public override bool Equals(object? obj) => obj is Vector64<T> other && this.Equals(other);

  /// <summary>Gets the element at the specified index.</summary>
  /// <param name="index">The index of the element to get.</param>
  /// <returns>The value of the element at <paramref name="index"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was less than zero or greater than the number of elements.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public T GetElement(int index) {
    ThrowIfNotSupported();
    if((uint)index>=(ulong)Count)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

    return this.GetElementUnsafe(index);
  }

  /// <summary>Returns the hash code for this instance.</summary>
  /// <returns>The hash code for this instance.</returns>
  public override int GetHashCode() => this._value.GetHashCode();

  /// <summary>Converts the current vector to a scalar containing the value of the first element.</summary>
  /// <returns>A scalar <typeparamref name="T"/> containing the value of the first element.</returns>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public T ToScalar() {
    ThrowIfNotSupported();
    return this.GetElement(0);
  }

  /// <summary>Converts the vector to a string representation.</summary>
  /// <returns>The string representation of the vector.</returns>
  public override string ToString() {
    ThrowIfNotSupported();
    var separator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator + " ";
    var builder = new StringBuilder();

    builder.Append('<');
    for (var i = 0; i < Count; ++i) {
      if (i > 0)
        builder.Append(separator);

      builder.Append(this.GetElement(i));
    }
    builder.Append('>');

    return builder.ToString();
  }

  /// <summary>Tries to copy the elements of this vector to a specified span of memory.</summary>
  /// <param name="destination">The destination span of memory.</param>
  /// <returns><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public bool TryCopyTo(Span<T> destination) {
    ThrowIfNotSupported();

    if (destination.Length < Count)
      return false;

    this.CopyTo(destination);
    return true;
  }

  /// <summary>Creates a new <see cref="Vector64{T}"/> with the element at the specified index set to the specified value and the remaining elements set to the same value as that in the given vector.</summary>
  /// <param name="index">The index of the element to set.</param>
  /// <param name="value">The value to set the element to.</param>
  /// <returns>A <see cref="Vector64{T}"/> with the value of the element at <paramref name="index"/> set to <paramref name="value"/> and the remaining elements set to the same value as that in the given vector.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was less than zero or greater than the number of elements.</exception>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public Vector64<T> WithElement(int index, T value) {
    ThrowIfNotSupported();
    if ((uint)index >= (ulong)Count)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

    return this.WithElementUnsafe(index, value);
  }

  /// <summary>Reinterprets a <see cref="Vector64{T}"/> as a new <see cref="Vector64{U}"/>.</summary>
  /// <typeparam name="U">The type of the elements in the destination vector.</typeparam>
  /// <returns>This vector reinterpreted as a new <see cref="Vector64{U}"/>.</returns>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) or the type of the target (<typeparamref name="U"/>) is not supported.</exception>
  public Vector64<U> As<U>() where U : unmanaged {
    ThrowIfNotSupported();
    return new(this._value);
  }

  #endregion

  #region Internal/Private Methods

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void ThrowIfNotSupported() {
    if (!IsSupported)
      throw new NotSupportedException("Unsupported vector type");
  }

  private static void ThrowIfWrongBaseType<TBase>() {
    if(typeof(T)!=typeof(TBase))
      throw new NotSupportedException("Wrong base type");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe T GetElementUnsafe(int index) {
    ref var raw = ref Unsafe.As<Vector64<T>, ulong>(ref Unsafe.AsRef<Vector64<T>>( this));
    return Unsafe.ReadUnaligned<T>(ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.As<ulong, T>(ref raw), index))));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Vector64<T> WithElementUnsafe(int index, T value) {
    var result = this;
    ref var element = ref Unsafe.Add(ref Unsafe.As<Vector64<T>, T>(ref result), index);
    element = value;
    return result;
  }

  private static unsafe Vector64<T> CreateFromSpan(ReadOnlySpan<T> values) {
    var result = default(Vector64<T>);
    var destinationSpan = new Span<T>(Unsafe.AsPointer(ref result), Count);
    values.Slice(0, Count).CopyTo(destinationSpan);
    return result;
  }

  private static Vector64<T> FillVector(Vector64<T> vector, T value) {
    var result = vector;
    for (var i = 0; i < Count; ++i)
      result = result.WithElementUnsafe(i, value);

    return result;
  }

  private static Vector64<T> PerformVectorOperation(Vector64<T> left, Vector64<T> right, Func<T, T, T> operation) {
    var result = default(Vector64<T>);
    for (var i = 0; i < Count; ++i) {
      var leftElement = left.GetElementUnsafe(i);
      var rightElement = right.GetElementUnsafe(i);
      var resultElement = operation(leftElement, rightElement);
      result = result.WithElementUnsafe(i, resultElement);
    }
    return result;
  }

  private static Vector64<T> PerformUnaryOperation(Vector64<T> vector, Func<T, T> operation) {
    var result = default(Vector64<T>);
    for (var i = 0; i < Count; ++i) {
      var element = vector.GetElementUnsafe(i);
      var resultElement = operation(element);
      result = result.WithElementUnsafe(i, resultElement);
    }
    return result;
  }

  private static T GetOneValue() {
    if (typeof(T) == typeof(byte))
      return (T)(object)(byte)1;
    if (typeof(T) == typeof(sbyte))
      return (T)(object)(sbyte)1;
    if (typeof(T) == typeof(short))
      return (T)(object)(short)1;
    if (typeof(T) == typeof(ushort))
      return (T)(object)(ushort)1;
    if (typeof(T) == typeof(int))
      return (T)(object)1;
    if (typeof(T) == typeof(uint))
      return (T)(object)1u;
    if (typeof(T) == typeof(long))
      return (T)(object)1L;
    if (typeof(T) == typeof(ulong))
      return (T)(object)1ul;
    if (typeof(T) == typeof(float))
      return (T)(object)1.0f;
    if (typeof(T) == typeof(double))
      return (T)(object)1.0;

    throw new NotSupportedException($"The type {typeof(T)} is not supported.");
  }

  // Scalar operation implementations
  private static T AddScalar<T>(T left, T right) {
    if (typeof(T) == typeof(byte))
      return (T)(object)((byte)(object)left + (byte)(object)right);
    if (typeof(T) == typeof(sbyte))
      return (T)(object)((sbyte)(object)left + (sbyte)(object)right);
    if (typeof(T) == typeof(short))
      return (T)(object)((short)(object)left + (short)(object)right);
    if (typeof(T) == typeof(ushort))
      return (T)(object)((ushort)(object)left + (ushort)(object)right);
    if (typeof(T) == typeof(int))
      return (T)(object)((int)(object)left + (int)(object)right);
    if (typeof(T) == typeof(uint))
      return (T)(object)((uint)(object)left + (uint)(object)right);
    if (typeof(T) == typeof(long))
      return (T)(object)((long)(object)left + (long)(object)right);
    if (typeof(T) == typeof(ulong))
      return (T)(object)((ulong)(object)left + (ulong)(object)right);
    if (typeof(T) == typeof(float))
      return (T)(object)((float)(object)left + (float)(object)right);
    if (typeof(T) == typeof(double))
      return (T)(object)((double)(object)left + (double)(object)right);

    throw new NotSupportedException($"AddScalar: The type {typeof(T)} is not supported.");
  }

  private static T SubtractScalar<T>(T left, T right) {
    if (typeof(T) == typeof(byte))
      return (T)(object)((byte)(object)left - (byte)(object)right);
    if (typeof(T) == typeof(sbyte))
      return (T)(object)((sbyte)(object)left - (sbyte)(object)right);
    if (typeof(T) == typeof(short))
      return (T)(object)((short)(object)left - (short)(object)right);
    if (typeof(T) == typeof(ushort))
      return (T)(object)((ushort)(object)left - (ushort)(object)right);
    if (typeof(T) == typeof(int))
      return (T)(object)((int)(object)left - (int)(object)right);
    if (typeof(T) == typeof(uint))
      return (T)(object)((uint)(object)left - (uint)(object)right);
    if (typeof(T) == typeof(long))
      return (T)(object)((long)(object)left - (long)(object)right);
    if (typeof(T) == typeof(ulong))
      return (T)(object)((ulong)(object)left - (ulong)(object)right);
    if (typeof(T) == typeof(float))
      return (T)(object)((float)(object)left - (float)(object)right);
    if (typeof(T) == typeof(double))
      return (T)(object)((double)(object)left - (double)(object)right);

    throw new NotSupportedException($"SubtractScalar: The type {typeof(T)} is not supported.");
  }

  private static T MultiplyScalar<T>(T left, T right) {
    if (typeof(T) == typeof(byte))
      return (T)(object)((byte)(object)left * (byte)(object)right);
    if (typeof(T) == typeof(sbyte))
      return (T)(object)((sbyte)(object)left * (sbyte)(object)right);
    if (typeof(T) == typeof(short))
      return (T)(object)((short)(object)left * (short)(object)right);
    if (typeof(T) == typeof(ushort))
      return (T)(object)((ushort)(object)left * (ushort)(object)right);
    if (typeof(T) == typeof(int))
      return (T)(object)((int)(object)left * (int)(object)right);
    if (typeof(T) == typeof(uint))
      return (T)(object)((uint)(object)left * (uint)(object)right);
    if (typeof(T) == typeof(long))
      return (T)(object)((long)(object)left * (long)(object)right);
    if (typeof(T) == typeof(ulong))
      return (T)(object)((ulong)(object)left * (ulong)(object)right);
    if (typeof(T) == typeof(float))
      return (T)(object)((float)(object)left * (float)(object)right);
    if (typeof(T) == typeof(double))
      return (T)(object)((double)(object)left * (double)(object)right);

    throw new NotSupportedException($"MultiplyScalar: The type {typeof(T)} is not supported.");
  }

  private static T DivideScalar<T>(T left, T right) {
    if (typeof(T) == typeof(byte))
      return (T)(object)((byte)(object)left / (byte)(object)right);
    if (typeof(T) == typeof(sbyte))
      return (T)(object)((sbyte)(object)left / (sbyte)(object)right);
    if (typeof(T) == typeof(short))
      return (T)(object)((short)(object)left / (short)(object)right);
    if (typeof(T) == typeof(ushort))
      return (T)(object)((ushort)(object)left / (ushort)(object)right);
    if (typeof(T) == typeof(int))
      return (T)(object)((int)(object)left / (int)(object)right);
    if (typeof(T) == typeof(uint))
      return (T)(object)((uint)(object)left / (uint)(object)right);
    if (typeof(T) == typeof(long))
      return (T)(object)((long)(object)left / (long)(object)right);
    if (typeof(T) == typeof(ulong))
      return (T)(object)((ulong)(object)left / (ulong)(object)right);
    if (typeof(T) == typeof(float))
      return (T)(object)((float)(object)left / (float)(object)right);
    if (typeof(T) == typeof(double))
      return (T)(object)((double)(object)left / (double)(object)right);

    throw new NotSupportedException($"DivideScalar: The type {typeof(T)} is not supported.");
  }

  private static T AbsScalar(T value) {
    if (typeof(T) == typeof(byte))
      return value; // Always positive
    if (typeof(T) == typeof(sbyte))
      return (T)(object)Math.Abs((sbyte)(object)value);
    if (typeof(T) == typeof(short))
      return (T)(object)Math.Abs((short)(object)value);
    if (typeof(T) == typeof(ushort))
      return value; // Always positive
    if (typeof(T) == typeof(int))
      return (T)(object)Math.Abs((int)(object)value);
    if (typeof(T) == typeof(uint))
      return value; // Always positive
    if (typeof(T) == typeof(long))
      return (T)(object)Math.Abs((long)(object)value);
    if (typeof(T) == typeof(ulong))
      return value; // Always positive
    if (typeof(T) == typeof(float))
      return (T)(object)Math.Abs((float)(object)value);
    if (typeof(T) == typeof(double))
      return (T)(object)Math.Abs((double)(object)value);

    throw new NotSupportedException($"The type {typeof(T)} is not supported.");
  }

  private static T CeilingScalar(T value) {
    if (typeof(T) == typeof(float))
      return (T)(object)(float)Math.Ceiling((float)(object)value);
    if (typeof(T) == typeof(double))
      return (T)(object)Math.Ceiling((double)(object)value);

    throw new NotSupportedException($"Ceiling is only supported for float and double types.");
  }

  // Debug display string
  private string DisplayString {
    get {
      try {
        return this.ToString();
      } catch {
        return $"Vector64<{typeof(T).Name}>";
      }
    }
  }

  #endregion
}

#endif