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

#if !OFFICIAL_HASHCODE

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Combines the hash code for multiple values into a single hash code.
/// </summary>
/// <remarks>
/// This is a polyfill for the <see cref="HashCode"/> struct available in .NET Core 2.1+ and .NET Standard 2.1+.
/// Uses an xxHash32-inspired algorithm for good distribution.
/// </remarks>
public struct HashCode {

  // xxHash32 constants
  private const uint Prime1 = 2654435761U;
  private const uint Prime2 = 2246822519U;
  private const uint Prime3 = 3266489917U;
  private const uint Prime4 = 668265263U;
  private const uint Prime5 = 374761393U;

  private static readonly uint _seed = _GenerateSeed();

  private uint _hash;
  private uint _length;

  private static uint _GenerateSeed() {
    // Use a combination of tick count and hash code of a new object for randomness
    unchecked {
      return (uint)Environment.TickCount ^ (uint)new object().GetHashCode();
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _RotateLeft(uint value, int count)
    => (value << count) | (value >> (32 - count));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _Round(uint hash, uint input) {
    hash += input * Prime2;
    hash = _RotateLeft(hash, 13);
    return hash * Prime1;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _MixState(uint v1, uint v2, uint v3, uint v4)
    => _RotateLeft(v1, 1) + _RotateLeft(v2, 7) + _RotateLeft(v3, 12) + _RotateLeft(v4, 18);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _MixFinal(uint hash) {
    hash ^= hash >> 15;
    hash *= Prime2;
    hash ^= hash >> 13;
    hash *= Prime3;
    hash ^= hash >> 16;
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _QueueRound(uint hash, uint input) {
    hash += input * Prime3;
    return _RotateLeft(hash, 17) * Prime4;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _GetHashCode<T>(T value)
    => (uint)(value?.GetHashCode() ?? 0);

  /// <summary>
  /// Diffuses the hash code returned by the specified value.
  /// </summary>
  /// <typeparam name="T1">The type of the value to add the hash code.</typeparam>
  /// <param name="value1">The value to add to the hash code.</param>
  /// <returns>The hash code that represents the single value.</returns>
  public static int Combine<T1>(T1 value1) {
    var hash = _seed + Prime5;
    hash += 4;
    hash = _QueueRound(hash, _GetHashCode(value1));
    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Combines two values into a hash code.
  /// </summary>
  /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
  /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
  /// <param name="value1">The first value to combine into the hash code.</param>
  /// <param name="value2">The second value to combine into the hash code.</param>
  /// <returns>The hash code that represents the two values.</returns>
  public static int Combine<T1, T2>(T1 value1, T2 value2) {
    var hash = _seed + Prime5;
    hash += 8;
    hash = _QueueRound(hash, _GetHashCode(value1));
    hash = _QueueRound(hash, _GetHashCode(value2));
    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Combines three values into a hash code.
  /// </summary>
  /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
  /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
  /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
  /// <param name="value1">The first value to combine into the hash code.</param>
  /// <param name="value2">The second value to combine into the hash code.</param>
  /// <param name="value3">The third value to combine into the hash code.</param>
  /// <returns>The hash code that represents the three values.</returns>
  public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3) {
    var hash = _seed + Prime5;
    hash += 12;
    hash = _QueueRound(hash, _GetHashCode(value1));
    hash = _QueueRound(hash, _GetHashCode(value2));
    hash = _QueueRound(hash, _GetHashCode(value3));
    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Combines four values into a hash code.
  /// </summary>
  /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
  /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
  /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
  /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
  /// <param name="value1">The first value to combine into the hash code.</param>
  /// <param name="value2">The second value to combine into the hash code.</param>
  /// <param name="value3">The third value to combine into the hash code.</param>
  /// <param name="value4">The fourth value to combine into the hash code.</param>
  /// <returns>The hash code that represents the four values.</returns>
  public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4) {
    var v1 = _seed + Prime1 + Prime2;
    var v2 = _seed + Prime2;
    var v3 = _seed;
    var v4 = _seed - Prime1;

    v1 = _Round(v1, _GetHashCode(value1));
    v2 = _Round(v2, _GetHashCode(value2));
    v3 = _Round(v3, _GetHashCode(value3));
    v4 = _Round(v4, _GetHashCode(value4));

    var hash = _MixState(v1, v2, v3, v4);
    hash += 16;

    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Combines five values into a hash code.
  /// </summary>
  /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
  /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
  /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
  /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
  /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
  /// <param name="value1">The first value to combine into the hash code.</param>
  /// <param name="value2">The second value to combine into the hash code.</param>
  /// <param name="value3">The third value to combine into the hash code.</param>
  /// <param name="value4">The fourth value to combine into the hash code.</param>
  /// <param name="value5">The fifth value to combine into the hash code.</param>
  /// <returns>The hash code that represents the five values.</returns>
  public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) {
    var v1 = _seed + Prime1 + Prime2;
    var v2 = _seed + Prime2;
    var v3 = _seed;
    var v4 = _seed - Prime1;

    v1 = _Round(v1, _GetHashCode(value1));
    v2 = _Round(v2, _GetHashCode(value2));
    v3 = _Round(v3, _GetHashCode(value3));
    v4 = _Round(v4, _GetHashCode(value4));

    var hash = _MixState(v1, v2, v3, v4);
    hash += 20;

    hash = _QueueRound(hash, _GetHashCode(value5));

    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Combines six values into a hash code.
  /// </summary>
  /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
  /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
  /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
  /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
  /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
  /// <typeparam name="T6">The type of the sixth value to combine into the hash code.</typeparam>
  /// <param name="value1">The first value to combine into the hash code.</param>
  /// <param name="value2">The second value to combine into the hash code.</param>
  /// <param name="value3">The third value to combine into the hash code.</param>
  /// <param name="value4">The fourth value to combine into the hash code.</param>
  /// <param name="value5">The fifth value to combine into the hash code.</param>
  /// <param name="value6">The sixth value to combine into the hash code.</param>
  /// <returns>The hash code that represents the six values.</returns>
  public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) {
    var v1 = _seed + Prime1 + Prime2;
    var v2 = _seed + Prime2;
    var v3 = _seed;
    var v4 = _seed - Prime1;

    v1 = _Round(v1, _GetHashCode(value1));
    v2 = _Round(v2, _GetHashCode(value2));
    v3 = _Round(v3, _GetHashCode(value3));
    v4 = _Round(v4, _GetHashCode(value4));

    var hash = _MixState(v1, v2, v3, v4);
    hash += 24;

    hash = _QueueRound(hash, _GetHashCode(value5));
    hash = _QueueRound(hash, _GetHashCode(value6));

    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Combines seven values into a hash code.
  /// </summary>
  /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
  /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
  /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
  /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
  /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
  /// <typeparam name="T6">The type of the sixth value to combine into the hash code.</typeparam>
  /// <typeparam name="T7">The type of the seventh value to combine into the hash code.</typeparam>
  /// <param name="value1">The first value to combine into the hash code.</param>
  /// <param name="value2">The second value to combine into the hash code.</param>
  /// <param name="value3">The third value to combine into the hash code.</param>
  /// <param name="value4">The fourth value to combine into the hash code.</param>
  /// <param name="value5">The fifth value to combine into the hash code.</param>
  /// <param name="value6">The sixth value to combine into the hash code.</param>
  /// <param name="value7">The seventh value to combine into the hash code.</param>
  /// <returns>The hash code that represents the seven values.</returns>
  public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) {
    var v1 = _seed + Prime1 + Prime2;
    var v2 = _seed + Prime2;
    var v3 = _seed;
    var v4 = _seed - Prime1;

    v1 = _Round(v1, _GetHashCode(value1));
    v2 = _Round(v2, _GetHashCode(value2));
    v3 = _Round(v3, _GetHashCode(value3));
    v4 = _Round(v4, _GetHashCode(value4));

    var hash = _MixState(v1, v2, v3, v4);
    hash += 28;

    hash = _QueueRound(hash, _GetHashCode(value5));
    hash = _QueueRound(hash, _GetHashCode(value6));
    hash = _QueueRound(hash, _GetHashCode(value7));

    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Combines eight values into a hash code.
  /// </summary>
  /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
  /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
  /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
  /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
  /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
  /// <typeparam name="T6">The type of the sixth value to combine into the hash code.</typeparam>
  /// <typeparam name="T7">The type of the seventh value to combine into the hash code.</typeparam>
  /// <typeparam name="T8">The type of the eighth value to combine into the hash code.</typeparam>
  /// <param name="value1">The first value to combine into the hash code.</param>
  /// <param name="value2">The second value to combine into the hash code.</param>
  /// <param name="value3">The third value to combine into the hash code.</param>
  /// <param name="value4">The fourth value to combine into the hash code.</param>
  /// <param name="value5">The fifth value to combine into the hash code.</param>
  /// <param name="value6">The sixth value to combine into the hash code.</param>
  /// <param name="value7">The seventh value to combine into the hash code.</param>
  /// <param name="value8">The eighth value to combine into the hash code.</param>
  /// <returns>The hash code that represents the eight values.</returns>
  public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) {
    var v1 = _seed + Prime1 + Prime2;
    var v2 = _seed + Prime2;
    var v3 = _seed;
    var v4 = _seed - Prime1;

    v1 = _Round(v1, _GetHashCode(value1));
    v2 = _Round(v2, _GetHashCode(value2));
    v3 = _Round(v3, _GetHashCode(value3));
    v4 = _Round(v4, _GetHashCode(value4));

    v1 = _Round(v1, _GetHashCode(value5));
    v2 = _Round(v2, _GetHashCode(value6));
    v3 = _Round(v3, _GetHashCode(value7));
    v4 = _Round(v4, _GetHashCode(value8));

    var hash = _MixState(v1, v2, v3, v4);
    hash += 32;

    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// Adds a single value to the hash code.
  /// </summary>
  /// <typeparam name="T">The type of the value to add to the hash code.</typeparam>
  /// <param name="value">The value to add to the hash code.</param>
  public void Add<T>(T value) {
    this._hash = _QueueRound(this._hash == 0 ? _seed + Prime5 : this._hash, _GetHashCode(value));
    this._length += 4;
  }

  /// <summary>
  /// Adds a single value to the hash code, specifying the type that provides the hash code function.
  /// </summary>
  /// <typeparam name="T">The type of the value to add to the hash code.</typeparam>
  /// <param name="value">The value to add to the hash code.</param>
  /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use to calculate the hash code.
  /// This value can be a null reference (Nothing in Visual Basic), which will use the default equality comparer for <typeparamref name="T"/>.</param>
  public void Add<T>(T value, IEqualityComparer<T> comparer) {
    var hashCode = comparer == null ? (value?.GetHashCode() ?? 0) : comparer.GetHashCode(value);
    this._hash = _QueueRound(this._hash == 0 ? _seed + Prime5 : this._hash, (uint)hashCode);
    this._length += 4;
  }

  /// <summary>
  /// Calculates the final hash code after consecutive <see cref="Add{T}(T)"/> invocations.
  /// </summary>
  /// <returns>The calculated hash code.</returns>
  public int ToHashCode() {
    var hash = this._hash == 0 ? _seed + Prime5 : this._hash;
    hash += this._length;
    return (int)_MixFinal(hash);
  }

  /// <summary>
  /// This method is not supported and should not be called.
  /// </summary>
  /// <returns>This method will always throw a <see cref="NotSupportedException"/>.</returns>
  /// <exception cref="NotSupportedException">Always thrown when this method is called.</exception>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
  [Obsolete("HashCode is a mutable struct and should not be compared with other HashCodes. Use ToHashCode to retrieve the computed hash code.", true)]
  public override int GetHashCode() => throw new NotSupportedException("HashCode is a mutable struct and should not be compared with other HashCodes.");
#pragma warning restore CS0809

  /// <summary>
  /// This method is not supported and should not be called.
  /// </summary>
  /// <param name="obj">Ignored.</param>
  /// <returns>This method will always throw a <see cref="NotSupportedException"/>.</returns>
  /// <exception cref="NotSupportedException">Always thrown when this method is called.</exception>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
  [Obsolete("HashCode is a mutable struct and should not be compared with other HashCodes.", true)]
  public override bool Equals(object obj) => throw new NotSupportedException("HashCode is a mutable struct and should not be compared with other HashCodes.");
#pragma warning restore CS0809

}

#endif
