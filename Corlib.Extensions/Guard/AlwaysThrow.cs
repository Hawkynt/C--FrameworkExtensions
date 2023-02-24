using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Corlib.Extensions.Guard {
  /// <summary>
  /// This class contains only throw-helpers (see https://andrewlock.net/exploring-dotnet-6-part-11-callerargumentexpression-and-throw-helpers)
  /// </summary>
  internal static class AlwaysThrow {

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void ArgumentOutOfRangeException(string parameterName, string message) => throw new ArgumentOutOfRangeException(parameterName, message);

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void ArgumentException(string parameterName, string message) => throw new ArgumentException(message, parameterName);

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void ArgumentNullException(string parameterName) => throw new ArgumentNullException(parameterName);

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void NullReferenceException(string parameterName) => throw new NullReferenceException($@"""{parameterName}"" must not be <null>");

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void IndexTooLowException(string parameterName, int value) => throw new IndexOutOfRangeException($"Index too low {value} < 0") { Data = { { parameterName, value } } };

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void IndexTooHighException(string parameterName, int value, int maxValue) => throw new IndexOutOfRangeException($"Index too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void CountTooLowException(string parameterName, int value) => throw new ArgumentOutOfRangeException(parameterName, $"Count too low {value} < 0") { Data = { { parameterName, value } } };

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void CountTooHighException(string parameterName, int value, int maxValue) => throw new ArgumentOutOfRangeException(parameterName, $"Count too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void CountNotMultipleOfException(string parameterName, int needed, int value) => throw new ArgumentOutOfRangeException(parameterName, $"Missing {needed - value} more items to fully cover target items") { Data = { { parameterName, value }, { "Needed", needed }, { "Having", value } } };

  }
}
