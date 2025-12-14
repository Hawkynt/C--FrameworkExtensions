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

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Runtime.CompilerServices;
using Guard;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

// Wave 1: Vector128<T> struct definition
#if !FEATURE_VECTOR128_WAVE1

public readonly struct Vector128<T> : IEquatable<Vector128<T>> where T : struct {

  // Internal storage as two 64-bit values (128 bits total)
  internal readonly ulong _lower;
  internal readonly ulong _upper;

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

  /// <summary>Gets the number of <typeparamref name="T"/> that are in a <see cref="Vector128{T}"/>.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      _ThrowIfNotSupported();
      return 16 / Unsafe.SizeOf<T>();
    }
  }

  /// <summary>Gets a new <see cref="Vector128{T}"/> with all elements initialized to zero.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector128<T> Zero {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => default;
  }

  /// <summary>Gets a new <see cref="Vector128{T}"/> with all elements initialized to one.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector128<T> One {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(Scalar<T>.One);
  }

  /// <summary>Gets a new <see cref="Vector128{T}"/> with all bits set to 1.</summary>
  /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T"/>) is not supported.</exception>
  public static Vector128<T> AllBitsSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(~0ul, ~0ul);
  }

  // Internal constructor from raw ulong values
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Vector128(ulong lower, ulong upper) {
    _ThrowIfNotSupported();
    this._lower = lower;
    this._upper = upper;
  }

  public Vector128(T value) {
    _ThrowIfNotSupported();
    var size = Unsafe.SizeOf<T>();
    var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
    var chunk = Unsafe.As<T, ulong>(ref value) & mask;

    ulong lower = 0;
    ulong upper = 0;
    var elementsPerUlong = 8 / size;

    for (var i = 0; i < elementsPerUlong && i < Count; ++i)
      lower |= chunk << (i * size * 8);

    for (var i = 0; i < elementsPerUlong && (i + elementsPerUlong) < Count; ++i)
      upper |= chunk << (i * size * 8);

    this._lower = lower;
    this._upper = upper;
  }

  public T this[int index] {
    get {
      if ((uint)index >= Count)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

      var size = Unsafe.SizeOf<T>();
      var elementsPerUlong = 8 / size;
      var targetValue = index < elementsPerUlong ? this._lower : this._upper;
      var localIndex = index < elementsPerUlong ? index : index - elementsPerUlong;

      var shift = localIndex * size * 8;
      var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
      var piece = (targetValue >> shift) & mask;
      return Unsafe.As<ulong, T>(ref piece);
    }
  }

  #region Operators

  /// <summary>Computes the bitwise-and of two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator &(Vector128<T> left, Vector128<T> right)
    => new(left._lower & right._lower, left._upper & right._upper);

  /// <summary>Computes the bitwise-or of two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator |(Vector128<T> left, Vector128<T> right)
    => new(left._lower | right._lower, left._upper | right._upper);

  /// <summary>Computes the exclusive-or of two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator ^(Vector128<T> left, Vector128<T> right)
    => new(left._lower ^ right._lower, left._upper ^ right._upper);

  /// <summary>Computes the ones-complement of a vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator ~(Vector128<T> vector)
    => new(~vector._lower, ~vector._upper);

  /// <summary>Adds two vectors to compute their sum.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator +(Vector128<T> left, Vector128<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Add);

  /// <summary>Subtracts two vectors to compute their difference.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator -(Vector128<T> left, Vector128<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Subtract);

  /// <summary>Multiplies two vectors to compute their element-wise product.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator *(Vector128<T> left, Vector128<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Multiply);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator *(T left, Vector128<T> right)
    => _PerformUnaryOperation(right, r => Scalar<T>.Multiply(left, r));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator *(Vector128<T> left, T right)
    => _PerformUnaryOperation(left, l => Scalar<T>.Multiply(l, right));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator <<(Vector128<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftLeft(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator >>(Vector128<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightArithmetic(x, count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator >>>(Vector128<T> value, int count)
    => _PerformUnaryOperation(value, x => Scalar<T>.ShiftRightLogical(x, count));

  /// <summary>Divides two vectors to compute their quotient.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator /(Vector128<T> left, Vector128<T> right)
    => _PerformVectorOperation(left, right, Scalar<T>.Divide);

  /// <summary>Computes the unary negation of a vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator -(Vector128<T> vector)
    => Zero - vector;

  /// <summary>Computes the unary plus of a vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<T> operator +(Vector128<T> vector)
    => vector;

  /// <summary>Compares two vectors to determine if they are equal on a per-element basis.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector128<T> left, Vector128<T> right)
    => left._lower == right._lower && left._upper == right._upper;

  /// <summary>Compares two vectors to determine if they are not equal on a per-element basis.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector128<T> left, Vector128<T> right)
    => left._lower != right._lower || left._upper != right._upper;

  #endregion

  #region Instance Methods

  /// <summary>Returns a value that indicates whether this instance and another vector are equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Vector128<T> other) => this._lower == other._lower && this._upper == other._upper;

  /// <summary>Returns a value that indicates whether this instance and a specified object are equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => obj is Vector128<T> other && this.Equals(other);

  /// <summary>Returns the hash code for this instance.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    unchecked {
      return (this._lower.GetHashCode() * 397) ^ this._upper.GetHashCode();
    }
  }

  /// <summary>Converts the vector to a string representation.</summary>
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

  /// <summary>Gets the lower 64 bits of this vector as a Vector64.</summary>
  public Vector64<T> GetLower() {
    var data = this._lower;
    return Unsafe.As<ulong, Vector64<T>>(ref data);
  }

  /// <summary>Gets the upper 64 bits of this vector as a Vector64.</summary>
  public Vector64<T> GetUpper() {
    var data = this._upper;
    return Unsafe.As<ulong, Vector64<T>>(ref data);
  }

  #endregion

  #region Internal/Private Methods

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static void _ThrowIfNotSupported() {
    if (!IsSupported)
      AlwaysThrow.NotSupportedException("Unsupported vector type");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void ThrowIfNotSupported() => _ThrowIfNotSupported();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Vector128<T> _WithElementUnsafe(int index, T value) {
    var result = this;
    ref var element = ref Unsafe.Add(ref Unsafe.As<Vector128<T>, T>(ref result), index);
    element = value;
    return result;
  }

  private static Vector128<T> _PerformVectorOperation(Vector128<T> left, Vector128<T> right, Func<T, T, T> operation) {
    var result = default(Vector128<T>);
    for (var i = 0; i < Count; ++i) {
      var leftElement = left[i];
      var rightElement = right[i];
      var resultElement = operation(leftElement, rightElement);
      result = result._WithElementUnsafe(i, resultElement);
    }
    return result;
  }

  private static Vector128<T> _PerformUnaryOperation(Vector128<T> vector, Func<T, T> operation) {
    var result = default(Vector128<T>);
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

// Wave 2: AllBitsSet property polyfill
#if !FEATURE_VECTOR128_WAVE2

/// <summary>
/// Polyfill for Vector128 AllBitsSet property (added in .NET 5.0).
/// </summary>
public static partial class Vector128Polyfills {

  extension<T>(Vector128<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector128{T}"/> with all bits set to 1.</summary>
    public static Vector128<T> AllBitsSet {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Unsafe.As<Vector128<byte>, Vector128<T>>(ref Unsafe.AsRef(Vector128.Create((byte)byte.MaxValue)));
    }
  }

}

#endif

// Wave 4: One property polyfill
#if !FEATURE_VECTOR128_WAVE4

public static partial class Vector128Polyfills {
  extension<T>(Vector128<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector128{T}"/> with all elements initialized to one.</summary>
    public static Vector128<T> One {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var one = Scalar<T>.One;
        var count = 16 / Unsafe.SizeOf<T>();
        unsafe {
          var buffer = stackalloc byte[16];
          var ptr = (T*)buffer;
          for (var i = 0; i < count; ++i)
            ptr[i] = one;
          return Unsafe.ReadUnaligned<Vector128<T>>(buffer);
        }
      }
    }
  }
}

#endif

// Wave 5: Indices property polyfill
#if !FEATURE_VECTOR128_WAVE5

public static partial class Vector128Polyfills {
  extension<T>(Vector128<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector128{T}"/> with the elements set to their index.</summary>
    public static Vector128<T> Indices {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var result = Vector128<T>.Zero;
        for (var i = 0; i < Vector128<T>.Count; ++i)
          result = result.WithElement(i, Scalar<T>.From<int>(i));
        return result;
      }
    }
  }
}

#endif
