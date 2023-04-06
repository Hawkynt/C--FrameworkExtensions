namespace Guard;

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

internal static partial class Against {
  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ThisIsNull([NotNull] object value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value == null)
      AlwaysThrow.NullReferenceException(expression ?? nameof(value), caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNull<T>([NotNull] T value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) where T : class {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrEmpty<T>([NotNull] T[] value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) where T : class {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (value.Length <= 0)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $@"Parameter ""{expression ?? nameof(value)}"" must contains elements", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrEmpty<T>([NotNull] IEnumerable<T> value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) where T : class {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (!value.Any())
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $@"Parameter ""{expression ?? nameof(value)}"" must contains elements", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrEmpty([NotNull] string value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (value.Length <= 0)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $@"Parameter ""{expression ?? nameof(value)}"" must contain contents", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ArgumentIsNullOrWhiteSpace([NotNull] string value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value == null)
      AlwaysThrow.ArgumentNullException(expression ?? nameof(value), caller);
    if (value.Length <= 0)
      AlwaysThrow.ArgumentException(expression ?? nameof(value), $@"Parameter ""{expression ?? nameof(value)}"" must contain contents", caller);
    
    if (value.Any(c => !char.IsWhiteSpace(c)))
      return;

    AlwaysThrow.ArgumentException(expression ?? nameof(value), $@"Parameter ""{expression ?? nameof(value)}"" must contain non-whitespace characters", caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexOutOfRange(int value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexOutOfRange(int value, int maxValue, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
    if (value > maxValue)
      AlwaysThrow.IndexTooHighException(expression ?? nameof(value), value, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexOutOfRange(long value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void IndexOutOfRange(long value, long maxValue, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.IndexTooLowException(expression ?? nameof(value), value, caller);
    if (value > maxValue)
      AlwaysThrow.IndexTooHighException(expression ?? nameof(value), value, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountOutOfRange(int value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountOutOfRange(int value, int maxValue, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
    if (value > maxValue)
      AlwaysThrow.CountTooHighException(expression ?? nameof(value), value, maxValue, caller);
  }

  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void CountOutOfRange(int value, int checkValue, int maxValue, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.CountTooLowException(expression ?? nameof(value), value, caller);
    if (checkValue > maxValue)
      AlwaysThrow.CountTooHighException(expression ?? nameof(value), value, checkValue, maxValue, caller);
  }

}