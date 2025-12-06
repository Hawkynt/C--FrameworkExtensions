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

#if !SUPPORTS_VECTOR_512

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

public readonly struct Vector512<T> : IEquatable<Vector512<T>> where T : struct {

  // Internal storage as eight 64-bit values (512 bits total)
  internal readonly ulong _v0;
  internal readonly ulong _v1;
  internal readonly ulong _v2;
  internal readonly ulong _v3;
  internal readonly ulong _v4;
  internal readonly ulong _v5;
  internal readonly ulong _v6;
  internal readonly ulong _v7;

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

  /// <summary>Gets the number of <typeparamref name="T"/> that are in a <see cref="Vector512{T}"/>.</summary>
  public static int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      _ThrowIfNotSupported();
      return 64 / Unsafe.SizeOf<T>();
    }
  }

  /// <summary>Gets a new <see cref="Vector512{T}"/> with all elements initialized to zero.</summary>
  public static Vector512<T> Zero {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => default;
  }

  /// <summary>Gets a new <see cref="Vector512{T}"/> with all elements initialized to one.</summary>
  public static Vector512<T> One {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(Scalar<T>.One);
  }

  /// <summary>Gets a new <see cref="Vector512{T}"/> with all bits set to 1.</summary>
  public static Vector512<T> AllBitsSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(~0ul, ~0ul, ~0ul, ~0ul, ~0ul, ~0ul, ~0ul, ~0ul);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Vector512(ulong v0, ulong v1, ulong v2, ulong v3, ulong v4, ulong v5, ulong v6, ulong v7) {
    _ThrowIfNotSupported();
    this._v0 = v0;
    this._v1 = v1;
    this._v2 = v2;
    this._v3 = v3;
    this._v4 = v4;
    this._v5 = v5;
    this._v6 = v6;
    this._v7 = v7;
  }

  public Vector512(T value) {
    _ThrowIfNotSupported();
    var size = Unsafe.SizeOf<T>();
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
    var chunk = Unsafe.As<T, ulong>(ref value) & mask;
    var elementsPerUlong = 8 / size;

    ulong v = 0;
    for (var i = 0; i < elementsPerUlong; ++i)
      v |= chunk << (i * size * 8);

    this._v0 = v;
    this._v1 = v;
    this._v2 = v;
    this._v3 = v;
    this._v4 = v;
    this._v5 = v;
    this._v6 = v;
    this._v7 = v;
  }

  public T this[int index] {
    get {
      if ((uint)index >= Count)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

      var size = Unsafe.SizeOf<T>();
      var elementsPerUlong = 8 / size;
      var ulongIndex = index / elementsPerUlong;
      var localIndex = index % elementsPerUlong;

      var targetValue = ulongIndex switch {
        0 => this._v0,
        1 => this._v1,
        2 => this._v2,
        3 => this._v3,
        4 => this._v4,
        5 => this._v5,
        6 => this._v6,
        _ => this._v7
      };

      var shift = localIndex * size * 8;
      var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
      var piece = (targetValue >> shift) & mask;
      return Unsafe.As<ulong, T>(ref piece);
    }
  }

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator &(Vector512<T> left, Vector512<T> right)
    => new(left._v0 & right._v0, left._v1 & right._v1, left._v2 & right._v2, left._v3 & right._v3,
           left._v4 & right._v4, left._v5 & right._v5, left._v6 & right._v6, left._v7 & right._v7);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator |(Vector512<T> left, Vector512<T> right)
    => new(left._v0 | right._v0, left._v1 | right._v1, left._v2 | right._v2, left._v3 | right._v3,
           left._v4 | right._v4, left._v5 | right._v5, left._v6 | right._v6, left._v7 | right._v7);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator ^(Vector512<T> left, Vector512<T> right)
    => new(left._v0 ^ right._v0, left._v1 ^ right._v1, left._v2 ^ right._v2, left._v3 ^ right._v3,
           left._v4 ^ right._v4, left._v5 ^ right._v5, left._v6 ^ right._v6, left._v7 ^ right._v7);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator ~(Vector512<T> vector)
    => new(~vector._v0, ~vector._v1, ~vector._v2, ~vector._v3,
           ~vector._v4, ~vector._v5, ~vector._v6, ~vector._v7);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator +(Vector512<T> left, Vector512<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Add);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator -(Vector512<T> left, Vector512<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Subtract);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator *(Vector512<T> left, Vector512<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Multiply);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator *(T left, Vector512<T> right)
    => _PerformUnaryOperation(right, r => Scalar<T>.Multiply(left, r));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator *(Vector512<T> left, T right)
    => _PerformUnaryOperation(left, l => Scalar<T>.Multiply(l, right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator <<(Vector512<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftLeft(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator >>(Vector512<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightArithmetic(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator >>>(Vector512<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightLogical(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator /(Vector512<T> left, Vector512<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Divide);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator -(Vector512<T> vector) => Zero - vector;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<T> operator +(Vector512<T> vector) => vector;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector512<T> left, Vector512<T> right)
    => left._v0 == right._v0 && left._v1 == right._v1 && left._v2 == right._v2 && left._v3 == right._v3
       && left._v4 == right._v4 && left._v5 == right._v5 && left._v6 == right._v6 && left._v7 == right._v7;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector512<T> left, Vector512<T> right) => !(left == right);

  #endregion

  #region Instance Methods

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Vector512<T> other) => this == other;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => obj is Vector512<T> other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    unchecked {
      var hash = this._v0.GetHashCode();
      hash = (hash * 397) ^ this._v1.GetHashCode();
      hash = (hash * 397) ^ this._v2.GetHashCode();
      hash = (hash * 397) ^ this._v3.GetHashCode();
      hash = (hash * 397) ^ this._v4.GetHashCode();
      hash = (hash * 397) ^ this._v5.GetHashCode();
      hash = (hash * 397) ^ this._v6.GetHashCode();
      hash = (hash * 397) ^ this._v7.GetHashCode();
      return hash;
    }
  }

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

  public Vector256<T> GetLower() {
    var data = (this._v0, this._v1, this._v2, this._v3);
    return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref data);
  }

  public Vector256<T> GetUpper() {
    var data = (this._v4, this._v5, this._v6, this._v7);
    return Unsafe.As<(ulong, ulong, ulong, ulong), Vector256<T>>(ref data);
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
  private Vector512<T> _WithElementUnsafe(int index, T value) {
    var result = this;
    ref var element = ref Unsafe.Add(ref Unsafe.As<Vector512<T>, T>(ref result), index);
    element = value;
    return result;
  }

  private static Vector512<T> _PerformVectorOperation(Vector512<T> left, Vector512<T> right, Func<T, T, T> operation) {
    var result = default(Vector512<T>);
    for (var i = 0; i < Count; ++i) {
      var leftElement = left[i];
      var rightElement = right[i];
      var resultElement = operation(leftElement, rightElement);
      result = result._WithElementUnsafe(i, resultElement);
    }
    return result;
  }

  private static Vector512<T> _PerformUnaryOperation(Vector512<T> vector, Func<T, T> operation) {
    var result = default(Vector512<T>);
    for (var i = 0; i < Count; ++i) {
      var element = vector[i];
      var resultElement = operation(element);
      result = result._WithElementUnsafe(i, resultElement);
    }
    return result;
  }

  #endregion
}

#endif
