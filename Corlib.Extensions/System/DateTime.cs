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

using System.Collections.Generic;

namespace System;

public static partial class DateTimeExtensions {
  private const long _TICKS_PER_MILLISECOND = 10000;

  /// <summary>
  ///   Returns the end of the day of the given date.
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="precisionInTicks">the amount of ticks to subtract</param>
  /// <returns>An instance pointing to the last moment of the given day.</returns>
  public static DateTime EndOfDay(this DateTime @this, long precisionInTicks = 1)
    =>
      @this.DayOfYear == DateTime.MinValue.DayOfYear && @this.Year == DateTime.MinValue.Year
        ? @this.AddDays(1).Subtract(@this.TimeOfDay).AddTicks(-precisionInTicks)
        : @this.Subtract(@this.TimeOfDay).AddTicks(-precisionInTicks).AddDays(1);

  /// <summary>
  ///   Returns the start of the day of the given date.
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <returns>An instance pointing to the first moment of the given day.</returns>
  public static DateTime StartOfDay(this DateTime @this)
    => @this.Subtract(@this.TimeOfDay);

  /// <summary>
  ///   Adds the specific amount of weeks to the current DateTime
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="weeks">The amount of weeks to be added</param>
  /// <returns>The new DateTime</returns>
  public static DateTime AddWeeks(this DateTime @this, int weeks) => @this.AddDays(7 * weeks);

  /// <summary>
  ///   Calculates the DateTime of a specified DayOfWeek for the week of the given date
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="weekDay">The DayOfWeek which should be returned</param>
  /// <returns>The DateTime of the week of day of the week of the given date</returns>
  public static DateTime DateOfDayOfCurrentWeek(this DateTime @this, DayOfWeek weekDay) {
    var weekStart = @this.AddDays(-(int)@this.DayOfWeek);

    return weekStart.AddDays((int)weekDay).EndOfDay();
  }

  /// <summary>
  ///   Gets the first day of the current year of the given date
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <returns>The DateTime representing the first day of the current year of the given date</returns>
  public static DateTime FirstDayOfYear(this DateTime @this) => new(@this.Year, 1, 1);

  /// <summary>
  ///   Gets the last day of the current year of the given date
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <returns>The DateTime representing the last day of the current year of the given date</returns>
  public static DateTime LastDayOfYear(this DateTime @this) => new(@this.Year, 12, 31);

  /// <summary>
  ///   Gets the first day of the current month of the given date
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <returns>The DateTime representing the first day of the current month of the given date</returns>
  public static DateTime FirstDayOfMonth(this DateTime @this) => new(@this.Year, @this.Month, 1);

  /// <summary>
  ///   Gets the last day of the current month of the given date
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <returns>The DateTime representing the last day of the current month of the given date</returns>
  public static DateTime LastDayOfMonth(this DateTime @this) {
    var monthDays = DateTime.DaysInMonth(@this.Year, @this.Month);

    return new(@this.Year, @this.Month, monthDays);
  }

  /// <summary>
  ///   Gets the Monday of the current week
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="startDayOfWeek">The start day of the week; default to DayOfWeek.Monday</param>
  /// <returns>The first day of the week</returns>
  public static DateTime StartOfWeek(this DateTime @this, DayOfWeek startDayOfWeek = DayOfWeek.Monday) {
    if (startDayOfWeek == @this.DayOfWeek)
      return @this;

    var daysToAdjust = (@this.DayOfWeek - startDayOfWeek + 7) % 7;
    var result = @this.AddDays(-daysToAdjust);
    return result;
  }

  /// <summary>
  ///   Get the given day in the current week.
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="dayOfWeek">The day of week to get the date for</param>
  /// <param name="startDayOfWeek">The day that is considered the start of week.</param>
  /// <returns></returns>
  public static DateTime DayInCurrentWeek(this DateTime @this, DayOfWeek dayOfWeek, DayOfWeek startDayOfWeek = DayOfWeek.Monday) {
    if (dayOfWeek == @this.DayOfWeek)
      return @this;

    var start = StartOfWeek(@this, startDayOfWeek);
    var daysToAdjust = (dayOfWeek - startDayOfWeek + 7) % 7;
    var result = start.AddDays(daysToAdjust);
    return result;
  }

  /// <summary>
  ///   Returns the one that is greater.
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="other">The comparand</param>
  /// <returns>The other value if it is later; otherwise, this instance.</returns>
  public static DateTime Max(this DateTime @this, DateTime other) => other > @this ? other : @this;

  /// <summary>
  ///   Returns the one that is less.
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="other">The comparand</param>
  /// <returns>The other value if it is earlier; otherwise, this instance.</returns>
  public static DateTime Min(this DateTime @this, DateTime other) => other < @this ? other : @this;


  /// <summary>
  ///   Enumerates all days between this and an enddate.
  /// </summary>
  /// <param name="this">This <see cref="DateTime" /></param>
  /// <param name="endDate">The endDate to iterate to.</param>
  /// <returns>The enumeration of days between.</returns>
  public static IEnumerable<DateTime> DaysTill(this DateTime @this, DateTime endDate) {
    for (var day = @this.Date; day.Date <= endDate.Date; day = day.AddDays(1))
      yield return day;
  }

  public static DateTime SubstractTicks(this DateTime @this, long value) => @this.AddTicks(-value);
  public static DateTime SubstractMilliseconds(this DateTime @this, double value) => @this.AddMilliseconds(-value);
  public static DateTime SubstractSeconds(this DateTime @this, double value) => @this.AddSeconds(-value);
  public static DateTime SubstractMinutes(this DateTime @this, double value) => @this.AddMinutes(-value);
  public static DateTime SubstractHours(this DateTime @this, double value) => @this.AddHours(-value);
  public static DateTime SubstractDays(this DateTime @this, double value) => @this.AddDays(-value);
  public static DateTime SubstractWeeks(this DateTime @this, int weeks) => @this.AddWeeks(-weeks);
  public static DateTime SubstractMonths(this DateTime @this, int months) => @this.AddMonths(-months);
  public static DateTime SubstractYears(this DateTime @this, int value) => @this.AddYears(-value);

  public static long AsUnixTicksUtc(this DateTime @this) => ((DateTimeOffset)@this.ToUniversalTime()).ToUnixTimeMilliseconds() * DateTimeExtensions._TICKS_PER_MILLISECOND;
  public static long AsUnixMillisecondsUtc(this DateTime @this) => ((DateTimeOffset)@this.ToUniversalTime()).ToUnixTimeMilliseconds();

  public static DateTime FromUnixTicks(long ticks, DateTimeKind kind = DateTimeKind.Unspecified) => DateTimeExtensions._CreateUnixEpochWithKind(kind).AddTicks(ticks);
  public static DateTime FromUnixSeconds(long seconds, DateTimeKind kind = DateTimeKind.Unspecified) => DateTimeExtensions._CreateUnixEpochWithKind(kind).AddSeconds(seconds);

  private static DateTime _CreateUnixEpochWithKind(DateTimeKind kind) => new(1970, 1, 1, 0, 0, 0, kind);
}
