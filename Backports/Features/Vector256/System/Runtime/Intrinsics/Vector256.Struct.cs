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

#if !SUPPORTS_VECTOR_256_TYPE

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

public readonly struct Vector256<T> : IEquatable<Vector256<T>> where T : struct {

  // Internal storage as four 64-bit values (256 bits total)
  internal readonly ulong _v0;
  internal readonly ulong _v1;
  internal readonly ulong _v2;
  internal readonly ulong _v3;

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

  /// <summary>Gets the number of <typeparamref name="T"/> that are in a <see cref="Vector256{T}"/>.</summary>
  public static int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      _ThrowIfNotSupported();
      return 32 / Unsafe.SizeOf<T>();
    }
  }

  /// <summary>Gets a new <see cref="Vector256{T}"/> with all elements initialized to zero.</summary>
  public static Vector256<T> Zero {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => default;
  }

  /// <summary>Gets a new <see cref="Vector256{T}"/> with all elements initialized to one.</summary>
  public static Vector256<T> One {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(Scalar<T>.One);
  }

  /// <summary>Gets a new <see cref="Vector256{T}"/> with all bits set to 1.</summary>
  public static Vector256<T> AllBitsSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(~0ul, ~0ul, ~0ul, ~0ul);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Vector256(ulong v0, ulong v1, ulong v2, ulong v3) {
    _ThrowIfNotSupported();
    this._v0 = v0;
    this._v1 = v1;
    this._v2 = v2;
    this._v3 = v3;
  }

  public Vector256(T value) {
    _ThrowIfNotSupported();
    var size = Unsafe.SizeOf<T>();
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
    var chunk = Unsafe.As<T, ulong>(ref value) & mask;
    var elementsPerUlong = 8 / size;

    ulong v0 = 0, v1 = 0, v2 = 0, v3 = 0;

    for (var i = 0; i < elementsPerUlong; ++i) {
      var shifted = chunk << (i * size * 8);
      v0 |= shifted;
      v1 |= shifted;
      v2 |= shifted;
      v3 |= shifted;
    }

    this._v0 = v0;
    this._v1 = v1;
    this._v2 = v2;
    this._v3 = v3;
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
        _ => this._v3
      };

      var shift = localIndex * size * 8;
      var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
      var piece = (targetValue >> shift) & mask;
      return Unsafe.As<ulong, T>(ref piece);
    }
  }

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator &(Vector256<T> left, Vector256<T> right)
    => new(left._v0 & right._v0, left._v1 & right._v1, left._v2 & right._v2, left._v3 & right._v3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator |(Vector256<T> left, Vector256<T> right)
    => new(left._v0 | right._v0, left._v1 | right._v1, left._v2 | right._v2, left._v3 | right._v3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator ^(Vector256<T> left, Vector256<T> right)
    => new(left._v0 ^ right._v0, left._v1 ^ right._v1, left._v2 ^ right._v2, left._v3 ^ right._v3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator ~(Vector256<T> vector)
    => new(~vector._v0, ~vector._v1, ~vector._v2, ~vector._v3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator +(Vector256<T> left, Vector256<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Add);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator -(Vector256<T> left, Vector256<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Subtract);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator *(Vector256<T> left, Vector256<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Multiply);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator *(T left, Vector256<T> right)
    => _PerformUnaryOperation(right, r => Scalar<T>.Multiply(left, r));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator *(Vector256<T> left, T right)
    => _PerformUnaryOperation(left, l => Scalar<T>.Multiply(l, right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator <<(Vector256<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftLeft(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator >>(Vector256<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightArithmetic(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator >>>(Vector256<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightLogical(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator /(Vector256<T> left, Vector256<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Divide);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator -(Vector256<T> vector) => Zero - vector;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<T> operator +(Vector256<T> vector) => vector;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector256<T> left, Vector256<T> right)
    => left._v0 == right._v0 && left._v1 == right._v1 && left._v2 == right._v2 && left._v3 == right._v3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector256<T> left, Vector256<T> right)
    => left._v0 != right._v0 || left._v1 != right._v1 || left._v2 != right._v2 || left._v3 != right._v3;

  #endregion

  #region Instance Methods

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Vector256<T> other) => this == other;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => obj is Vector256<T> other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    unchecked {
      var hash = this._v0.GetHashCode();
      hash = (hash * 397) ^ this._v1.GetHashCode();
      hash = (hash * 397) ^ this._v2.GetHashCode();
      hash = (hash * 397) ^ this._v3.GetHashCode();
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

  public Vector128<T> GetLower() {
    var data = (this._v0, this._v1);
    return Unsafe.As<(ulong, ulong), Vector128<T>>(ref data);
  }

  public Vector128<T> GetUpper() {
    var data = (this._v2, this._v3);
    return Unsafe.As<(ulong, ulong), Vector128<T>>(ref data);
  }

  #endregion

  #region Internal/Private Methods

  private static void _ThrowIfNotSupported() {
    if (!IsSupported)
      AlwaysThrow.NotSupportedException("Unsupported vector type");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void ThrowIfNotSupported() => _ThrowIfNotSupported();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Vector256<T> _WithElementUnsafe(int index, T value) {
    var result = this;
    ref var element = ref Unsafe.Add(ref Unsafe.As<Vector256<T>, T>(ref result), index);
    element = value;
    return result;
  }

  private static Vector256<T> _PerformVectorOperation(Vector256<T> left, Vector256<T> right, Func<T, T, T> operation) {
    var result = default(Vector256<T>);
    for (var i = 0; i < Count; ++i) {
      var leftElement = left[i];
      var rightElement = right[i];
      var resultElement = operation(leftElement, rightElement);
      result = result._WithElementUnsafe(i, resultElement);
    }
    return result;
  }

  private static Vector256<T> _PerformUnaryOperation(Vector256<T> vector, Func<T, T> operation) {
    var result = default(Vector256<T>);
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
