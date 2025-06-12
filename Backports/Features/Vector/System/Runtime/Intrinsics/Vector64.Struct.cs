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

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

public readonly struct Vector64<T> : IEquatable<Vector64<T>> {
  
  // Internal storage as a 64-bit value
  internal readonly ulong _value;

  public static bool IsSupported {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => typeof(T) == typeof(byte)
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
  }

  /// <summary>Gets the number of <typeparamref name="T"/> that are in a <see cref="Vector64{T}"/>.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      _ThrowIfNotSupported();
      return 8 / Unsafe.SizeOf<T>();
    }
  }

  /// <summary>Gets a new <see cref="Vector64{T}"/> with all elements initialized to zero.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> Zero {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => default;
  }

  /// <summary>Gets a new <see cref="Vector64{T}"/> with all elements initialized to one.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> One {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(Scalar<T>.One);
  }

  /// <summary>Gets a new <see cref="Vector64{T}"/> with all bits set to 1.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector64<T> AllBitsSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(~0ul);
  }

  // Internal constructor from raw ulong
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Vector64(ulong value) {
    _ThrowIfNotSupported();
    this._value = value;
  }

  public Vector64(T value) {
    _ThrowIfNotSupported();
    var size = Unsafe.SizeOf<T>();
    var mask = (1UL << (size * 8)) - 1;
    var chunk = Unsafe.As<T, ulong>(ref value) & mask;
    ulong val = 0;
    for (var i = 0; i < Count; ++i)
      val |= chunk << (i * size * 8);
    
    this._value = val;
  }

  public static Vector64<T> Indices {
    get {
      ulong val = 0;
      var size = Unsafe.SizeOf<T>();
      var mask = (1UL << (size * 8)) - 1;
      for (var i = 0; i < Count; ++i) {
        var tval = Scalar<T>.From(i);
        var chunk = Unsafe.As<T, ulong>(ref tval) & mask;
        val |= chunk << (i * size * 8);
      }
      return new(val);
    }
  }

  public T this[int index] {
    get {
      if ((uint)index >= Count)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

      var shift = index * Unsafe.SizeOf<T>() * 8;
      var mask = (1UL << (Unsafe.SizeOf<T>() * 8)) - 1;
      var piece = (this._value >> shift) & mask;
      return Unsafe.As<ulong, T>(ref piece);
    }
  }

  #region Operators

  /// <summary>Computes the bitwise-and of two vectors.</summary>
  /// <param name="left">The vector to bitwise-and with <paramref name="right"/>.</param>
  /// <param name="right">The vector to bitwise-and with <paramref name="left"/>.</param>
  /// <returns>The bitwise-and of <paramref name="left"/> and <paramref name="right"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator &(Vector64<T> left, Vector64<T> right) 
    => new(left._value & right._value);

  /// <summary>Computes the bitwise-or of two vectors.</summary>
  /// <param name="left">The vector to bitwise-or with <paramref name="right"/>.</param>
  /// <param name="right">The vector to bitwise-or with <paramref name="left"/>.</param>
  /// <returns>The bitwise-or of <paramref name="left"/> and <paramref name="right"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator |(Vector64<T> left, Vector64<T> right)
    => new(left._value | right._value);

  /// <summary>Computes the exclusive-or of two vectors.</summary>
  /// <param name="left">The vector to exclusive-or with <paramref name="right"/>.</param>
  /// <param name="right">The vector to exclusive-or with <paramref name="left"/>.</param>
  /// <returns>The exclusive-or of <paramref name="left"/> and <paramref name="right"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator ^(Vector64<T> left, Vector64<T> right) 
    => new(left._value ^ right._value);

  /// <summary>Computes the ones-complement of a vector.</summary>
  /// <param name="vector">The vector whose ones-complement is to be computed.</param>
  /// <returns>A vector whose elements are the ones-complement of the corresponding elements in <paramref name="vector"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator ~(Vector64<T> vector) 
    => new(~vector._value);

  /// <summary>Adds two vectors to compute their sum.</summary>
  /// <param name="left">The vector to add with <paramref name="right"/>.</param>
  /// <param name="right">The vector to add with <paramref name="left"/>.</param>
  /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator +(Vector64<T> left, Vector64<T> right) 
    => _PerformVectorOperation(left, right, Scalar<T>.Add);

  /// <summary>Subtracts two vectors to compute their difference.</summary>
  /// <param name="left">The vector from which <paramref name="right"/> will be subtracted.</param>
  /// <param name="right">The vector to subtract from <paramref name="left"/>.</param>
  /// <returns>The difference of <paramref name="left"/> and <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator -(Vector64<T> left, Vector64<T> right) 
    => _PerformVectorOperation(left, right, Scalar<T>.Subtract);

  /// <summary>Multiplies two vectors to compute their element-wise product.</summary>
  /// <param name="left">The vector to multiply with <paramref name="right"/>.</param>
  /// <param name="right">The vector to multiply with <paramref name="left"/>.</param>
  /// <returns>The element-wise product of <paramref name="left"/> and <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator *(Vector64<T> left, Vector64<T> right) 
    => _PerformVectorOperation(left, right, Scalar<T>.Multiply);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator *(T left, Vector64<T> right)
    => _PerformUnaryOperation(right, r => Scalar<T>.Multiply(left, r));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator *(Vector64<T> left, T right)
    => _PerformUnaryOperation(left, l => Scalar<T>.Multiply(l, right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator <<(Vector64<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftLeft(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator >>(Vector64<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightArithmetic(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator >>>(Vector64<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightLogical(x, count));

  /// <summary>Divides two vectors to compute their quotient.</summary>
  /// <param name="left">The vector that will be divided by <paramref name="right"/>.</param>
  /// <param name="right">The vector that will divide <paramref name="left"/>.</param>
  /// <returns>The quotient of <paramref name="left"/> divided by <paramref name="right"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="left"/> and <paramref name="right"/> (<typeparamref name="T"/>) is not supported.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator /(Vector64<T> left, Vector64<T> right) 
    => _PerformVectorOperation(left, right, Scalar<T>.Divide);

  /// <summary>Computes the unary negation of a vector.</summary>
  /// <param name="vector">The vector to negate.</param>
  /// <returns>A vector whose elements are the unary negation of the corresponding elements in <paramref name="vector"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="vector"/> (<typeparamref name="T"/>) is not supported.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator -(Vector64<T> vector) 
    => Zero - vector;

  /// <summary>Computes the unary plus of a vector.</summary>
  /// <param name="vector">The vector for which to compute its unary plus.</param>
  /// <returns>A copy of <paramref name="vector"/>.</returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="vector"/> (<typeparamref name="T"/>) is not supported.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<T> operator +(Vector64<T> vector) 
    => vector;

  /// <summary>Compares two vectors to determine if they are equal on a per-element basis.</summary>
  /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
  /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
  /// <returns><see langword="true"/> if all corresponding elements were equal; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector64<T> left, Vector64<T> right) 
    => left._value == right._value;

  /// <summary>Compares two vectors to determine if they are not equal on a per-element basis.</summary>
  /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
  /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
  /// <returns><see langword="true"/> if any corresponding elements were not equal; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector64<T> left, Vector64<T> right)
    => left._value != right._value;

  #endregion

  #region Instance Methods

  /// <summary>Returns a value that indicates whether this instance and another vector are equal.</summary>
  /// <param name="other">The other vector.</param>
  /// <returns><see langword="true"/> if the two vectors are equal; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Vector64<T> other) => this._value == other._value;

  /// <summary>Returns a value that indicates whether this instance and a specified object are equal.</summary>
  /// <param name="obj">The object to compare with the current instance.</param>
  /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Vector64{T}"/> and the two vectors are equal; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => obj is Vector64<T> other && this.Equals(other);

  /// <summary>Returns the hash code for this instance.</summary>
  /// <returns>The hash code for this instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this._value.GetHashCode();
  
  /// <summary>Converts the vector to a string representation.</summary>
  /// <returns>The string representation of the vector.</returns>
  public override string ToString() {
    _ThrowIfNotSupported();
    var separator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator + " ";
    var builder = new StringBuilder();

    builder.Append('<');
    for (var i = 0; i < Count; ++i) {
      if (i > 0)
        builder.Append(separator);

      builder.Append(this[i]);
    }
    builder.Append('>');

    return builder.ToString();
  }

  #endregion

  #region Internal/Private Methods

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static void _ThrowIfNotSupported() {
    if (!IsSupported)
      throw new NotSupportedException("Unsupported vector type");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)] 
  internal static void ThrowIfNotSupported() => _ThrowIfNotSupported();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Vector64<T> _WithElementUnsafe(int index, T value) {
    var result = this;
    ref var element = ref Unsafe.Add(ref Unsafe.As<Vector64<T>, T>(ref result), index);
    element = value;
    return result;
  }
  
  private static Vector64<T> _PerformVectorOperation(Vector64<T> left, Vector64<T> right, Func<T, T, T> operation) {
    var result = default(Vector64<T>);
    for (var i = 0; i < Count; ++i) {
      var leftElement = left[i];
      var rightElement = right[i];
      var resultElement = operation(leftElement, rightElement);
      result = result._WithElementUnsafe(i, resultElement);
    }
    return result;
  }

  private static Vector64<T> _PerformUnaryOperation(Vector64<T> vector, Func<T, T> operation) {
    var result = default(Vector64<T>);
    for (var i = 0; i < Count; ++i) {
      var element = vector[i];
      var resultElement = operation(element);
      result = result._WithElementUnsafe(i, resultElement);
    }
    return result;
  }
  
  #endregion
}
/*
public static class Vector64Extensions {

  /// <summary>Reinterprets a <see cref="Vector64{T}"/> as a new <see cref="Vector64{U}"/>.</summary>
  /// <typeparam name="TFrom">The type of the elements in the source vector.</typeparam>
  /// <typeparam name="TTo">The type of the elements in the destination vector.</typeparam>
  /// <param name="vector">The vector to reinterpret.</param>
  /// <returns>
  /// <paramref name="vector"/> reinterpreted as a new <see cref="Vector64{U}"/>.
  /// </returns>
  /// <exception cref="NotSupportedException">The type of <paramref name="vector"/> (<typeparamref name="TFrom"/>) or the type of the target (<typeparamref name="TTo"/>) is not supported.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector64<TTo> As<TFrom, TTo>(this Vector64<TFrom> vector)
    where TFrom : unmanaged where TTo : unmanaged
    => Unsafe.As<Vector64<TFrom>, Vector64<TTo>>(ref Unsafe.AsRef(vector));

  [MethodImpl(MethodImplOptions.AggressiveInlining)] 
  public static Vector64<byte> AsByte<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, byte>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<sbyte> AsSByte<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, sbyte>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)] 
  public static Vector64<short> AsInt16<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, short>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<ushort> AsUInt16<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, ushort>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<int> AsInt32<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, int>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<uint> AsUInt32<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, uint>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<long> AsInt64<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, long>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<ulong> AsUInt64<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, ulong>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<IntPtr> AsNInt<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, IntPtr>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<UIntPtr> AsNUInt<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, UIntPtr>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<float> AsSingle<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, float>();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector64<double> AsDouble<T>(this Vector64<T> vector) where T : unmanaged => vector.As<T, double>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)] 
  public static T GetElement<T>(this Vector64<T> vector, int index) where T : unmanaged => vector[index];

  public static void CopyTo<T>(this Vector64<T> vector, T[] destination) where T : unmanaged {
    if (destination is null)
      AlwaysThrow.ArgumentNullException(nameof(destination));
    if (destination.Length < Vector64<T>.Count)
      AlwaysThrow.ArgumentException(nameof(destination), "Destination array is too small.");

    for (var i = 0; i < Vector64<T>.Count; ++i)
      destination[i] = vector[i];
  }

  public static bool TryCopyTo<T>(this Vector64<T> vector, Span<T> destination) where T : unmanaged {
    if (destination.Length < Vector64<T>.Count)
      return false;

    for (var i = 0; i < Vector64<T>.Count; ++i)
      destination[i] = vector[i];

    return true;
  }

  public static Vector64<T> WithElement<T>(this Vector64<T> vector, int index, T value) where T : unmanaged {
    if ((uint)index >= Vector64<T>.Count)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

    var size = Unsafe.SizeOf<T>();
    var shift = index * size * 8;
    var mask = ((1UL << (size * 8)) - 1UL) << shift;
    
    var newBits = Scalar<ulong>.From(value) << shift;
    var cleared = vector._value & ~mask;
    var result = cleared | (newBits & mask);

    return new(result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ToScalar<T>(this Vector64<T> vector) where T : unmanaged => vector[0];

  public static ulong ExtractMostSignificantBits<T>(this Vector64<T> vector) where T : unmanaged {
    ulong result = 0;
    for (var i = 0; i < Vector64<T>.Count; ++i) {
      var bit = Scalar<T>.ExtractMostSignificantBit(vector[i]);
      result |= (bit ? 1UL : 0UL) << i;
    }
    return result;
  }

}
*/
#endif