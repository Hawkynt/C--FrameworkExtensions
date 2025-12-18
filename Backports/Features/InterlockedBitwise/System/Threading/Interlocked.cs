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

#if !SUPPORTS_INTERLOCKED_BITWISE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

public static partial class InterlockedPolyfills {

  extension(Interlocked) {

    /// <summary>
    /// Bitwise "ands" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int And(ref int location1, int value) {
      int current;
      int newValue;
      do {
        current = location1;
        newValue = current & value;
      } while (Interlocked.CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    /// <summary>
    /// Bitwise "ands" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long And(ref long location1, long value) {
      long current;
      long newValue;
      do {
        current = location1;
        newValue = current & value;
      } while (Interlocked.CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    /// <summary>
    /// Bitwise "ands" two 32-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint And(ref uint location1, uint value) {
      uint current;
      uint newValue;
      do {
        current = location1;
        newValue = current & value;
      } while (_CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    /// <summary>
    /// Bitwise "ands" two 64-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong And(ref ulong location1, ulong value) {
      ulong current;
      ulong newValue;
      do {
        current = location1;
        newValue = current & value;
      } while (_CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    /// <summary>
    /// Bitwise "ors" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Or(ref int location1, int value) {
      int current;
      int newValue;
      do {
        current = location1;
        newValue = current | value;
      } while (Interlocked.CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    /// <summary>
    /// Bitwise "ors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Or(ref long location1, long value) {
      long current;
      long newValue;
      do {
        current = location1;
        newValue = current | value;
      } while (Interlocked.CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    /// <summary>
    /// Bitwise "ors" two 32-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Or(ref uint location1, uint value) {
      uint current;
      uint newValue;
      do {
        current = location1;
        newValue = current | value;
      } while (_CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    /// <summary>
    /// Bitwise "ors" two 64-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
    /// </summary>
    /// <param name="location1">A variable containing the first value to be combined.</param>
    /// <param name="value">The value to be combined with the integer at <paramref name="location1"/>.</param>
    /// <returns>The original value in <paramref name="location1"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Or(ref ulong location1, ulong value) {
      ulong current;
      ulong newValue;
      do {
        current = location1;
        newValue = current | value;
      } while (_CompareExchange(ref location1, newValue, current) != current);
      return current;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint _CompareExchange(ref uint location1, uint value, uint comparand) {
      fixed (uint* ptr = &location1)
        return (uint)Interlocked.CompareExchange(ref *(int*)ptr, (int)value, (int)comparand);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong _CompareExchange(ref ulong location1, ulong value, ulong comparand) {
      fixed (ulong* ptr = &location1)
        return (ulong)Interlocked.CompareExchange(ref *(long*)ptr, (long)value, (long)comparand);
    }

  }

}

#endif
