namespace Guard;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
///   This class contains only throw-helpers
///   ( see https://andrewlock.net/exploring-dotnet-6-part-11-callerargumentexpression-and-throw-helpers )
/// </summary>
internal static class AlwaysThrow {
  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentOutOfRangeException(string parameterName, string message, [CallerMemberName] string caller = null) => throw new ArgumentOutOfRangeException(parameterName, $"{(caller == null ? string.Empty : caller + ":")}{message}");

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentException(string parameterName, string message, [CallerMemberName] string caller = null) => throw new ArgumentException($"{(caller == null ? string.Empty : caller + ":")}{message}", parameterName);

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentNullException(string parameterName, [CallerMemberName] string caller = null) => throw new ArgumentNullException(parameterName, $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be <null>");

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void NullReferenceException(string parameterName, [CallerMemberName] string caller = null) => throw new NullReferenceException($@"{(caller == null ? string.Empty : caller + ":")}Instance in ""{parameterName}"" must not be <null>");

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexTooLowException(string parameterName, int value, [CallerMemberName] string caller = null) => throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too low {value} < 0") { Data = { { parameterName, value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexTooHighException(string parameterName, int value, int maxValue, [CallerMemberName] string caller = null) => throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooLowException(string parameterName, int value, [CallerMemberName] string caller = null) => throw new ArgumentOutOfRangeException(parameterName, $"{(caller == null ? string.Empty : caller + ":")}Count too low {value} < 0") { Data = { { parameterName, value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooHighException(string parameterName, int value, int maxValue, [CallerMemberName] string caller = null) => throw new ArgumentOutOfRangeException(parameterName, $"{(caller == null ? string.Empty : caller + ":")}Count too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountNotMultipleOfException(string parameterName, int needed, int value, [CallerMemberName] string caller = null) => throw new ArgumentOutOfRangeException(parameterName, $"{(caller == null ? string.Empty : caller + ":")}Missing {needed - value} more items to fully cover target items") { Data = { { parameterName, value }, { "Needed", needed }, { "Having", value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void InvalidOperationException(string message, [CallerMemberName] string caller = null) => throw new InvalidOperationException($"{(caller == null ? string.Empty : caller + ":")}{message}");
  
}
