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

#if !SUPPORTS_ARGUMENTOUTOFRANGEEXCEPTION_THROWIF

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class ArgumentOutOfRangeExceptionPolyfills {
  extension(ArgumentOutOfRangeException) {

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.
    /// </summary>
    /// <param name="value">The argument to validate as non-zero.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct, IComparable<T> {
      // Fast path for known numeric types using JIT-optimizable type dispatch
      if (TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.ObjectEquals(value, Scalar<T>.Zero()))
          _Throw(paramName, value, " must be a non-zero value.");
        return;
      }
      // Slow path fallback for other IComparable<T> types
      if (value.CompareTo(default) == 0)
        _Throw(paramName, value, " must be a non-zero value.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
    /// </summary>
    /// <param name="value">The argument to validate as non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct, IComparable<T> {
      // Fast path for known numeric types using JIT-optimizable type dispatch
      if (TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.LessThan(value, Scalar<T>.Zero()))
          _Throw(paramName, value, " must be a non-negative value.");
        return;
      }
      // Slow path fallback for other IComparable<T> types
      if (value.CompareTo(default) < 0)
        _Throw(paramName, value, " must be a non-negative value.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.
    /// </summary>
    /// <param name="value">The argument to validate as non-negative and non-zero.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative or zero.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct, IComparable<T> {
      // Fast path for known numeric types using JIT-optimizable type dispatch
      if (TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.LessThanOrEqual(value, Scalar<T>.Zero()))
          _Throw(paramName, value, " must be a non-negative and non-zero value.");
        return;
      }
      // Slow path fallback for other IComparable<T> types
      if (value.CompareTo(default) <= 0)
        _Throw(paramName, value, " must be a non-negative and non-zero value.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as less than or equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than <paramref name="other"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> {
      // Fast path for known numeric struct types using JIT-optimizable type dispatch
      if (typeof(T).IsValueType && TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.GreaterThan(value, other))
          _Throw(paramName, value, $" must be less than or equal to '{other}'.");
        return;
      }
      // Slow path fallback for reference types and other IComparable<T> types
      if (value.CompareTo(other) > 0)
        _Throw(paramName, value, $" must be less than or equal to '{other}'.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than or equal to <paramref name="other"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> {
      // Fast path for known numeric struct types using JIT-optimizable type dispatch
      if (typeof(T).IsValueType && TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.GreaterThanOrEqual(value, other))
          _Throw(paramName, value, $" must be less than '{other}'.");
        return;
      }
      // Slow path fallback for reference types and other IComparable<T> types
      if (value.CompareTo(other) >= 0)
        _Throw(paramName, value, $" must be less than '{other}'.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as greater than or equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="other"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> {
      // Fast path for known numeric struct types using JIT-optimizable type dispatch
      if (typeof(T).IsValueType && TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.LessThan(value, other))
          _Throw(paramName, value, $" must be greater than or equal to '{other}'.");
        return;
      }
      // Slow path fallback for reference types and other IComparable<T> types
      if (value.CompareTo(other) < 0)
        _Throw(paramName, value, $" must be greater than or equal to '{other}'.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as greater than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to <paramref name="other"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> {
      // Fast path for known numeric struct types using JIT-optimizable type dispatch
      if (typeof(T).IsValueType && TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.LessThanOrEqual(value, other))
          _Throw(paramName, value, $" must be greater than '{other}'.");
        return;
      }
      // Slow path fallback for reference types and other IComparable<T> types
      if (value.CompareTo(other) <= 0)
        _Throw(paramName, value, $" must be greater than '{other}'.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is equal to <paramref name="other"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IEquatable<T>? {
      // Fast path for known numeric struct types using JIT-optimizable type dispatch
      if (typeof(T).IsValueType && TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (Scalar<T>.ObjectEquals(value, other))
          _Throw(paramName, value, $" must not be equal to '{other}'.");
        return;
      }
      // Slow path fallback for reference types and other IEquatable<T> types
      if (EqualityComparer<T>.Default.Equals(value, other))
        _Throw(paramName, value, $" must not be equal to '{other}'.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.
    /// </summary>
    /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IEquatable<T>? {
      // Fast path for known numeric struct types using JIT-optimizable type dispatch
      if (typeof(T).IsValueType && TypeCodeCache<T>.Code != CachedTypeCode.Unknown) {
        if (!Scalar<T>.ObjectEquals(value, other))
          _Throw(paramName, value, $" must be equal to '{other}'.");
        return;
      }
      // Slow path fallback for reference types and other IEquatable<T> types
      if (!EqualityComparer<T>.Default.Equals(value, other))
        _Throw(paramName, value, $" must be equal to '{other}'.");
    }

  }

  [DoesNotReturn]
  private static void _Throw<T>(string? paramName, T value, string messageSuffix)
    => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}'){messageSuffix}");
}

#endif
