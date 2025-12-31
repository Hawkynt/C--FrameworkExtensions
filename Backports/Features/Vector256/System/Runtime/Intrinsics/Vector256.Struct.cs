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
using Guard;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;

// ========== WAVE 1 ==========

#if !SUPPORTS_VECTOR256_WAVE1

namespace System.Runtime.Intrinsics {

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
           || typeof(T) == typeof(nuint)
           || typeof(T) == typeof(Half)
           || typeof(T) == typeof(UInt128)
           || typeof(T) == typeof(Int128);
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

    // Handle elements that span multiple ulongs (e.g., Int128, UInt128)
    if (size > 8) {
      var data = Unsafe.As<T, (ulong, ulong)>(ref value);
      // Replicate the 16-byte value across both element slots
      this._v0 = data.Item1;
      this._v1 = data.Item2;
      this._v2 = data.Item1;
      this._v3 = data.Item2;
      return;
    }

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

  public static Vector256<T> Indices {
    get {
      ulong v0 = 0, v1 = 0, v2 = 0, v3 = 0;
      var size = Unsafe.SizeOf<T>();
      var mask = size >= 8 ? ~0UL : (1UL << (size * 8)) - 1;
      var elementsPerUlong = 8 / size;

      for (var i = 0; i < elementsPerUlong && i < Count; ++i) {
        var tval = Scalar<T>.From(i);
        var chunk = Unsafe.As<T, ulong>(ref tval) & mask;
        v0 |= chunk << (i * size * 8);
      }

      for (var i = 0; i < elementsPerUlong && (i + elementsPerUlong) < Count; ++i) {
        var tval = Scalar<T>.From(i + elementsPerUlong);
        var chunk = Unsafe.As<T, ulong>(ref tval) & mask;
        v1 |= chunk << (i * size * 8);
      }

      for (var i = 0; i < elementsPerUlong && (i + elementsPerUlong * 2) < Count; ++i) {
        var tval = Scalar<T>.From(i + elementsPerUlong * 2);
        var chunk = Unsafe.As<T, ulong>(ref tval) & mask;
        v2 |= chunk << (i * size * 8);
      }

      for (var i = 0; i < elementsPerUlong && (i + elementsPerUlong * 3) < Count; ++i) {
        var tval = Scalar<T>.From(i + elementsPerUlong * 3);
        var chunk = Unsafe.As<T, ulong>(ref tval) & mask;
        v3 |= chunk << (i * size * 8);
      }

      return new(v0, v1, v2, v3);
    }
  }

  public T this[int index] {
    get {
      if ((uint)index >= Count)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(index));

      var size = Unsafe.SizeOf<T>();

      // Handle elements that span multiple ulongs (e.g., Int128, UInt128)
      if (size > 8) {
        var ulongsPerElement = size / 8;
        var baseUlongIndex = index * ulongsPerElement;

        // For 16-byte elements (Int128, UInt128)
        var low = baseUlongIndex == 0 ? this._v0 : this._v2;
        var high = baseUlongIndex == 0 ? this._v1 : this._v3;
        var data = (low, high);
        return Unsafe.As<(ulong, ulong), T>(ref data);
      }

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

}
#endif

// ========== WAVE 2 ==========

#if !SUPPORTS_VECTOR256_WAVE2

namespace System.Runtime.Intrinsics {

/// <summary>
/// Polyfill for Vector256 AllBitsSet property (added in .NET 5.0).
/// </summary>
public static class Vector256AllBitsSetPolyfills {

  extension<T>(Vector256<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector256{T}"/> with all bits set to 1.</summary>
    public static Vector256<T> AllBitsSet {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Unsafe.As<Vector256<byte>, Vector256<T>>(ref Unsafe.AsRef(Vector256.Create(byte.MaxValue)));
    }
  }

}

}
#endif

// ========== WAVE 4 ==========

#if SUPPORTS_VECTOR256_WAVE1 && !SUPPORTS_VECTOR256_WAVE4

namespace System.Runtime.Intrinsics {

public static partial class Vector256Polyfills {
  extension<T>(Vector256<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector256{T}"/> with all elements initialized to one.</summary>
    public static Vector256<T> One {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var one = Scalar<T>.One;
        var count = 32 / Unsafe.SizeOf<T>();
        unsafe {
          var buffer = stackalloc byte[32];
          var ptr = (T*)buffer;
          for (var i = 0; i < count; ++i)
            ptr[i] = one;
          return Unsafe.ReadUnaligned<Vector256<T>>(buffer);
        }
      }
    }
  }
}

}
#endif

// ========== WAVE 5 ==========

#if !SUPPORTS_VECTOR256_WAVE5

namespace System.Runtime.Intrinsics {

public static partial class Vector256Polyfills {
  extension<T>(Vector256<T>) where T : struct {
    /// <summary>Gets a new <see cref="Vector256{T}"/> with the elements set to their index.</summary>
    public static Vector256<T> Indices {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        var result = Vector256<T>.Zero;
        for (var i = 0; i < Vector256<T>.Count; ++i)
          result = result.WithElement(i, Scalar<T>.From<int>(i));
        return result;
      }
    }
  }
}

}
#endif
