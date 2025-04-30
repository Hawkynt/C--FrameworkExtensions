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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Guard;

/// <summary>
///   This class contains only throw-helpers
///   ( see https://andrewlock.net/exploring-dotnet-6-part-11-callerargumentexpression-and-throw-helpers )
/// </summary>
internal static class AlwaysThrow {
  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentOutOfRangeException(
    string parameterName,
    string message = null,
    [CallerMemberName] string caller = null
  ) => throw new ArgumentOutOfRangeException(
    parameterName,
    $"{(caller == null ? string.Empty : caller + ":")}{message}"
  );

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void
    ArgumentBelowRangeException<T>(string parameterName, T value, T minimum, [CallerMemberName] string caller = null)
    where T : struct =>
    throw new ArgumentOutOfRangeException(
      parameterName,
      $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be below threshold. ({value} < {minimum})"
    ) { Data = { { parameterName, value }, { "Minimum", minimum } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void
    ArgumentBelowOrEqualRangeException<T>(
      string parameterName,
      T value,
      T minimum,
      [CallerMemberName] string caller = null
    ) where T : struct => throw new ArgumentOutOfRangeException(
    parameterName,
    $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be below or equal to threshold. ({value} <= {minimum})"
  ) { Data = { { parameterName, value }, { "Minimum", minimum } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void
    ArgumentAboveRangeException<T>(string parameterName, T value, T maximum, [CallerMemberName] string caller = null)
    where T : struct =>
    throw new ArgumentOutOfRangeException(
      parameterName,
      $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be above threshold. ({value} > {maximum})"
    ) { Data = { { parameterName, value }, { "Maximum", maximum } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void
    ArgumentAboveOrEqualRangeException<T>(
      string parameterName,
      T value,
      T maximum,
      [CallerMemberName] string caller = null
    ) where T : struct => throw new ArgumentOutOfRangeException(
    parameterName,
    $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be above or equal to threshold. ({value} >= {maximum})"
  ) { Data = { { parameterName, value }, { "Maximum", maximum } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentException(string parameterName, string message, [CallerMemberName] string caller = null) =>
    throw new ArgumentException($"{(caller == null ? string.Empty : caller + ":")}{message}", parameterName);

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentNullException(string parameterName, [CallerMemberName] string caller = null) =>
    throw new ArgumentNullException(
      parameterName,
      $"""{(caller == null ? string.Empty : caller + ":")}Parameter "{parameterName}" must not be <null>"""
    );

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void NullReferenceException(string parameterName, [CallerMemberName] string caller = null) =>
    throw new NullReferenceException($"""{(caller == null ? string.Empty : caller + ":")}Instance in "{parameterName}" must not be <null>""");

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexTooLowException(string parameterName, int value, [CallerMemberName] string caller = null) =>
    throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too low {value} < 0") { Data = { { parameterName, value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexTooLowException(string parameterName, long value, [CallerMemberName] string caller = null) =>
    throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too low {value} < 0") { Data = { { parameterName, value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void
    IndexTooHighException(string parameterName, int value, int maxValue, [CallerMemberName] string caller = null) =>
    throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void
    IndexTooHighException(string parameterName, long value, long maxValue, [CallerMemberName] string caller = null) =>
    throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexOutOfRangeException([CallerMemberName] string caller = null) =>
    throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index out of range");

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooLowException(string parameterName, int value, [CallerMemberName] string caller = null)
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Count too low {value} < 0"
    ) { Data = { { parameterName, value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooLowException(string parameterName, long value, [CallerMemberName] string caller = null)
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Count too low {value} < 0"
    ) { Data = { { parameterName, value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooHighException(
    string parameterName,
    int value,
    int maxValue,
    [CallerMemberName] string caller = null
  )
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Count too high {value} >= {maxValue}"
    ) { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooHighException(
    string parameterName,
    long value,
    long maxValue,
    [CallerMemberName] string caller = null
  )
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Count too high {value} >= {maxValue}"
    ) { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooHighException(
    string parameterName,
    int value,
    int checkValue,
    int maxValue,
    [CallerMemberName] string caller = null
  )
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Count too high {checkValue}({value}) >= {maxValue}"
    ) { Data = { { parameterName, value }, { "Maximum", maxValue }, { "Index", checkValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountTooHighException(
    string parameterName,
    long value,
    long checkValue,
    long maxValue,
    [CallerMemberName] string caller = null
  )
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Count too high {checkValue}({value}) >= {maxValue}"
    ) { Data = { { parameterName, value }, { "Maximum", maxValue }, { "Index", checkValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountNotMultipleOfException(
    string parameterName,
    int needed,
    int value,
    [CallerMemberName] string caller = null
  )
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Missing {needed - value} more items to fully cover target items"
    ) { Data = { { parameterName, value }, { "Needed", needed }, { "Having", value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountNotMultipleOfException(
    string parameterName,
    long needed,
    long value,
    [CallerMemberName] string caller = null
  )
    => throw new ArgumentOutOfRangeException(
      parameterName,
      $"{(caller == null ? string.Empty : caller + ":")}Missing {needed - value} more items to fully cover target items"
    ) { Data = { { parameterName, value }, { "Needed", needed }, { "Having", value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void InvalidOperationException(string message, [CallerMemberName] string caller = null) =>
    throw new InvalidOperationException($"{(caller == null ? string.Empty : caller + ":")}{message}");

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void NoElements([CallerMemberName] string caller = null) =>
    throw new InvalidOperationException($"{(caller == null ? string.Empty : caller + ":")}The sequence contains no elements.");

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void UnknownEnumValue<TEnum>(string parameterName, TEnum value, [CallerMemberName] string caller = null)
    where TEnum : Enum =>
    throw new ArgumentException(
      $"{(caller == null ? string.Empty : caller + ":")}The value {value} provided is not part of the enum {typeof(TEnum).Name}",
      parameterName
    ) { Data = { { parameterName, value }, { "AllowedKeys", Enum.GetNames(typeof(TEnum)) }, { "AllowedValues", Enum.GetValues(typeof(TEnum)) } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ObjectDisposedException(string objectName, [CallerMemberName] string caller = null) => 
    throw new ObjectDisposedException(objectName,$"{(caller == null ? string.Empty : caller + ":")} The object {objectName} is already disposed and must no longer be used.");

}
