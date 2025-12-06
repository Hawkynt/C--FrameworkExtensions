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

#if !SUPPORTS_MATH_CLAMP

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathPolyfills {
  extension(Math) {
  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  /// <param name="value">The value to be clamped.</param>
  /// <param name="min">The lower bound of the result.</param>
  /// <param name="max">The upper bound of the result.</param>
  /// <returns>
  /// <paramref name="value"/> if <paramref name="min"/> ≤ <paramref name="value"/> ≤ <paramref name="max"/>.
  /// -or-
  /// <paramref name="min"/> if <paramref name="value"/> &lt; <paramref name="min"/>.
  /// -or-
  /// <paramref name="max"/> if <paramref name="max"/> &lt; <paramref name="value"/>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Clamp(byte value, byte min, byte max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte Clamp(sbyte value, sbyte min, sbyte max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short Clamp(short value, short min, short max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort Clamp(ushort value, ushort min, ushort max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Clamp(int value, int min, int max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Clamp(uint value, uint min, uint max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long Clamp(long value, long min, long max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Clamp(ulong value, ulong min, ulong max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Clamp(float value, float min, float max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Clamp(double value, double min, double max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  /// <summary>
  /// Returns value clamped to the inclusive range of min and max.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal Clamp(decimal value, decimal min, decimal max) {
    if (min > max)
      _ThrowMinMaxException(min, max);
    if (value < min)
      return min;
    if (value > max)
      return max;
    return value;
  }

  private static void _ThrowMinMaxException<T>(T min, T max) =>
    throw new ArgumentException($"'{min}' cannot be greater than {max}.");
  }
}

#endif
