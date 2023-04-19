﻿#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

// ReSharper disable UnusedMember.Global
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
  public static void ArgumentBelowRangeException<T>(string parameterName, T value, T minimum, [CallerMemberName] string caller = null) where T : struct => throw new ArgumentOutOfRangeException(parameterName, $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be below threshold. ({value} < {minimum})") { Data = { { parameterName, value }, { "Minimum", minimum } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentBelowOrEqualRangeException<T>(string parameterName, T value, T minimum, [CallerMemberName] string caller = null) where T : struct => throw new ArgumentOutOfRangeException(parameterName, $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be below or equal to threshold. ({value} <= {minimum})") { Data = { { parameterName, value }, { "Minimum", minimum } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentAboveRangeException<T>(string parameterName, T value, T maximum, [CallerMemberName] string caller = null) where T : struct => throw new ArgumentOutOfRangeException(parameterName, $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be above threshold. ({value} > {maximum})") { Data = { { parameterName, value }, { "Maximum", maximum } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void ArgumentAboveOrEqualRangeException<T>(string parameterName, T value, T maximum, [CallerMemberName] string caller = null) where T : struct => throw new ArgumentOutOfRangeException(parameterName, $@"{(caller == null ? string.Empty : caller + ":")}Parameter ""{parameterName}"" must not be above or equal to threshold. ({value} >= {maximum})") { Data = { { parameterName, value }, { "Maximum", maximum } } };

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
  public static void IndexTooLowException(string parameterName, long value, [CallerMemberName] string caller = null) => throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too low {value} < 0") { Data = { { parameterName, value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexTooHighException(string parameterName, int value, int maxValue, [CallerMemberName] string caller = null) => throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexTooHighException(string parameterName, long value, long maxValue, [CallerMemberName] string caller = null) => throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index too high {value} >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void IndexOutOfRangeException([CallerMemberName] string caller = null) => throw new IndexOutOfRangeException($"{(caller == null ? string.Empty : caller + ":")}Index out of range");

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
  public static void CountTooHighException(string parameterName, int value, int checkValue, int maxValue, [CallerMemberName] string caller = null) => throw new ArgumentOutOfRangeException(parameterName, $"{(caller == null ? string.Empty : caller + ":")}Count too high {checkValue}({value}) >= {maxValue}") { Data = { { parameterName, value }, { "Maximum", maxValue }, { "Index", checkValue } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void CountNotMultipleOfException(string parameterName, int needed, int value, [CallerMemberName] string caller = null) => throw new ArgumentOutOfRangeException(parameterName, $"{(caller == null ? string.Empty : caller + ":")}Missing {needed - value} more items to fully cover target items") { Data = { { parameterName, value }, { "Needed", needed }, { "Having", value } } };

  [DebuggerHidden]
  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  public static void InvalidOperationException(string message, [CallerMemberName] string caller = null) => throw new InvalidOperationException($"{(caller == null ? string.Empty : caller + ":")}{message}");
  
}
