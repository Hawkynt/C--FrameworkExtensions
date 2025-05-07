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
//

#if !SUPPORTS_TO_UNIX_TIME_MILLISECONDS

using System.Diagnostics;
using MethodImplOptions = Utilities.MethodImplOptions;
using System.Runtime.CompilerServices;

namespace System;

public static partial class DateTimeOffsetPolyfills {

  private const int DaysPerYear = 365;
  private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
  private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
  private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097
  private const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162
  private const long UnixEpochTicks = TimeSpan.TicksPerDay * DaysTo1970; // 621,355,968,000,000,000
  private const long UnixEpochSeconds = UnixEpochTicks / TimeSpan.TicksPerSecond; // 62,135,596,800
  private const long UnixEpochMilliseconds = UnixEpochTicks / TimeSpan.TicksPerMillisecond; // 62,135,596,800,000

  /// <summary>Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</summary>
  /// <returns>The number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.</returns>
  [DebuggerStepThrough]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long ToUnixTimeMilliseconds(this DateTimeOffset @this) => @this.UtcDateTime.Ticks / TimeSpan.TicksPerMillisecond - UnixEpochMilliseconds;

  /// <summary>Returns the number of seconds that have elapsed since 1970-01-01T00:00:00.000Z.</summary>
  /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00.000Z.</returns>
  [DebuggerStepThrough]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long ToUnixTimeSeconds(this DateTimeOffset @this) => @this.UtcDateTime.Ticks / TimeSpan.TicksPerSecond - UnixEpochSeconds;

}


#endif