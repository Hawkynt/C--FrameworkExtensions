namespace Guard;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class Against {
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

}