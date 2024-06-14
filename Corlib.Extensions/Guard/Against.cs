#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Guard;

internal static partial class Against {
  /// <summary>
  ///   Checks the given parameter for <see langword="null" /> and throws a <see cref="NullReferenceException" />.
  /// </summary>
  /// <param name="value">The value to check.</param>
  /// <param name="caller">The calling method.</param>
  /// <param name="expression">The name or epxression of the value</param>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="value" /> is <see langword="null" />.</exception>
  /// <remarks>Call this method only for the parameter attributed with the <see langword="this" />-keyword.</remarks>
  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ThisIsNull([NotNull] object? value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value == null)
      AlwaysThrow.NullReferenceException(expression ?? nameof(value), caller);
  }

  /// <summary>
  ///   Checks the given parameter for <see langword="null" /> and throws an <see cref="ArgumentNullException" />.
  /// </summary>
  /// <param name="value">The value to check.</param>
  /// <param name="caller">The calling method.</param>
  /// <param name="expression">The name or epxression of the value</param>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="value" /> is <see langword="null" />.</exception>
  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNull<T>([NotNull] T? value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrEmpty<T>([NotNull] T?[]? value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (value.Length <= 0)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $"Parameter \"{expression ?? nameof(value)}\" must contains elements", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrEmpty<T>([NotNull] IEnumerable<T?>? value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (!value.Any())
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $"Parameter \"{expression ?? nameof(value)}\" must contains elements", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrEmpty([NotNull] string? value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (value.Length <= 0)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $"Parameter \"{expression ?? nameof(value)}\" must contain contents", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrWhiteSpace([NotNull] string? value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (value.Length <= 0)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $"Parameter \"{expression ?? nameof(value)}\" must contain contents", caller);

    if (value.Any(c => !char.IsWhiteSpace(c)))
      return;

    AlwaysThrow.ArgumentException(expression ?? nameof(value), $"Parameter \"{expression ?? nameof(value)}\" must contain non-whitespace characters", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNotOfType<T>(object value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value is not T)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $"Parameter \"{expression ?? nameof(value)}\" must be of type \"{typeof(T)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsOfType<T>(object value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value is T)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $"Parameter \"{expression ?? nameof(value)}\" must not be of type \"{typeof(T)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void False(bool value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (!value)
      AlwaysThrow.InvalidOperationException($"Value \"{expression ?? nameof(value)}\" should not be FALSE", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void True(bool value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value)
      AlwaysThrow.InvalidOperationException($"Value \"{expression ?? nameof(value)}\" should not be TRUE", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void DifferentInstances<T>(T value, T other, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null, [CallerArgumentExpression(nameof(other))] string? otherExpression = null) where T : class {
    if (!ReferenceEquals(value, other))
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should be equal to \"{expression ?? nameof(value)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void SameInstance<T>(T value, T other, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null, [CallerArgumentExpression(nameof(other))] string? otherExpression = null) where T : class {
    if (ReferenceEquals(value, other))
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" must not be equal to \"{expression ?? nameof(value)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ValuesAreNotEqual<T>(T? value, T? other, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null, [CallerArgumentExpression(nameof(other))] string? otherExpression = null) {
    if (ReferenceEquals(value, other))
      return;

    if (value is null || other is null)
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should be equal to \"{expression ?? nameof(value)}\"", caller);

    if (EqualityComparer<T>.Default.Equals(value, other))
      return;

    AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should be equal to \"{expression ?? nameof(value)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ValuesAreNotEqual(string? value, string? other, StringComparison comparisonType, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null, [CallerArgumentExpression(nameof(other))] string? otherExpression = null) {
    if (ReferenceEquals(value, other))
      return;

    if (value is null || other is null)
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should be equal to \"{expression ?? nameof(value)}\"", caller);

    if (string.Equals(value, other, comparisonType))
      return;

    AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should be equal to \"{expression ?? nameof(value)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ValuesAreEqual<T>(T? value, T? other, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null, [CallerArgumentExpression(nameof(other))] string? otherExpression = null) {
    if (ReferenceEquals(value, other))
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should not be equal to \"{expression ?? nameof(value)}\"", caller);

    if (value is null || other is null)
      return;

    if (EqualityComparer<T>.Default.Equals(value, other))
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should not be equal to \"{expression ?? nameof(value)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ValuesAreEqual(string? value, string? other, StringComparison comparisonType, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null, [CallerArgumentExpression(nameof(other))] string? otherExpression = null) {
    if (ReferenceEquals(value, other))
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should not be equal to \"{expression ?? nameof(value)}\"", caller);

    if (value is null || other is null)
      return;

    if (string.Equals(value, other, comparisonType))
      AlwaysThrow.InvalidOperationException($"Value \"{otherExpression ?? nameof(other)}\" should not be equal to \"{expression ?? nameof(value)}\"", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexBelowZero(int value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexOutOfRange(int value, int maxValue, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
    if (value > maxValue)
      AlwaysThrow.IndexTooHighException(expression ?? nameof(value), value, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexBelowZero(long value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexOutOfRange(long value, long maxValue, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
    if (value > maxValue)
      AlwaysThrow.IndexTooHighException(expression ?? nameof(value), value, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountBelowOrEqualZero(int value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value <= 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountBelowOrEqualZero(long value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value <= 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountBelowZero(int value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value < 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountBelowZero(long value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value < 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountOutOfRange(int value, int maxValue, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value <= 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
    if (value > maxValue)
      AlwaysThrow.CountTooHighException(expression ?? nameof(value), value, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountOutOfRange(long value, long maxValue, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value <= 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
    if (value > maxValue)
      AlwaysThrow.CountTooHighException(expression ?? nameof(value), value, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountOutOfRange(int value, int checkValue, int maxValue, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value <= 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
    if (checkValue > maxValue)
      AlwaysThrow.CountTooHighException(expression ?? nameof(value), value, checkValue, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountOutOfRange(long value, long checkValue, long maxValue, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) {
    if (value <= 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
    if (checkValue > maxValue)
      AlwaysThrow.CountTooHighException(expression ?? nameof(value), value, checkValue, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void UnknownEnumValues<TEnum>(TEnum value, [CallerMemberName] string? caller = null, [CallerArgumentExpression(nameof(value))] string? expression = null) where TEnum : Enum {
    if (Enum.IsDefined(typeof(TEnum), value))
      return;

    AlwaysThrow.UnknownEnumValue(expression ?? nameof(value), value, caller);
  }
}
