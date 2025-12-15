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

  /// <param name="this">This <see cref="DateTime" /></param>
  extension(DateTime @this)
  {
    /// <summary>
    ///   Returns the end of the day of the given date.
    /// </summary>
    /// <param name="precisionInTicks">the amount of ticks to subtract</param>
    /// <returns>An instance pointing to the last moment of the given day.</returns>
    public DateTime EndOfDay(long precisionInTicks = 1)
      =>
        @this.DayOfYear == DateTime.MinValue.DayOfYear && @this.Year == DateTime.MinValue.Year
          ? @this.AddDays(1).Subtract(@this.TimeOfDay).AddTicks(-precisionInTicks)
          : @this.Subtract(@this.TimeOfDay).AddTicks(-precisionInTicks).AddDays(1);

    /// <summary>
    ///   Returns the start of the day of the given date.
    /// </summary>
    /// <returns>An instance pointing to the first moment of the given day.</returns>
    public DateTime StartOfDay()
      => @this.Subtract(@this.TimeOfDay);

    /// <summary>
    ///   Adds the specific amount of weeks to the current DateTime
    /// </summary>
    /// <param name="weeks">The amount of weeks to be added</param>
    /// <returns>The new DateTime</returns>
    public DateTime AddWeeks(int weeks) => @this.AddDays(7 * weeks);

    /// <summary>
    ///   Calculates the DateTime of a specified DayOfWeek for the week of the given date
    /// </summary>
    /// <param name="weekDay">The DayOfWeek which should be returned</param>
    /// <returns>The DateTime of the week of day of the week of the given date</returns>
    public DateTime DateOfDayOfCurrentWeek(DayOfWeek weekDay) {
      var weekStart = @this.AddDays(-(int)@this.DayOfWeek);

      return weekStart.AddDays((int)weekDay).EndOfDay();
    }

    /// <summary>
    ///   Gets the first day of the current year of the given date
    /// </summary>
    /// <returns>The DateTime representing the first day of the current year of the given date</returns>
    public DateTime FirstDayOfYear() => new(@this.Year, 1, 1);

    /// <summary>
    ///   Gets the last day of the current year of the given date
    /// </summary>
    /// <returns>The DateTime representing the last day of the current year of the given date</returns>
    public DateTime LastDayOfYear() => new(@this.Year, 12, 31);

    /// <summary>
    ///   Gets the first day of the current month of the given date
    /// </summary>
    /// <returns>The DateTime representing the first day of the current month of the given date</returns>
    public DateTime FirstDayOfMonth() => new(@this.Year, @this.Month, 1);

    /// <summary>
    ///   Gets the last day of the current month of the given date
    /// </summary>
    /// <returns>The DateTime representing the last day of the current month of the given date</returns>
    public DateTime LastDayOfMonth() {
      var monthDays = DateTime.DaysInMonth(@this.Year, @this.Month);

      return new(@this.Year, @this.Month, monthDays);
    }

    /// <summary>
    ///   Gets the Monday of the current week
    /// </summary>
    /// <param name="startDayOfWeek">The start day of the week; default to DayOfWeek.Monday</param>
    /// <returns>The first day of the week</returns>
    public DateTime StartOfWeek(DayOfWeek startDayOfWeek = DayOfWeek.Monday) {
      if (startDayOfWeek == @this.DayOfWeek)
        return @this;

      var daysToAdjust = (@this.DayOfWeek - startDayOfWeek + 7) % 7;
      var result = @this.AddDays(-daysToAdjust);
      return result;
    }

    /// <summary>
    ///   Get the given day in the current week.
    /// </summary>
    /// <param name="dayOfWeek">The day of week to get the date for</param>
    /// <param name="startDayOfWeek">The day that is considered the start of week.</param>
    /// <returns></returns>
    public DateTime DayInCurrentWeek(DayOfWeek dayOfWeek, DayOfWeek startDayOfWeek = DayOfWeek.Monday) {
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
    /// <param name="other">The comparand</param>
    /// <returns>The other value if it is later; otherwise, this instance.</returns>
    public DateTime Max(DateTime other) => other > @this ? other : @this;

    /// <summary>
    ///   Returns the one that is less.
    /// </summary>
    /// <param name="other">The comparand</param>
    /// <returns>The other value if it is earlier; otherwise, this instance.</returns>
    public DateTime Min(DateTime other) => other < @this ? other : @this;

    /// <summary>
    ///   Enumerates all days between this and an enddate.
    /// </summary>
    /// <param name="endDate">The endDate to iterate to.</param>
    /// <returns>The enumeration of days between.</returns>
    public IEnumerable<DateTime> DaysTill(DateTime endDate) {
      for (var day = @this.Date; day.Date <= endDate.Date; day = day.AddDays(1))
        yield return day;
    }

    public DateTime SubstractTicks(long value) => @this.AddTicks(-value);
    public DateTime SubstractMilliseconds(double value) => @this.AddMilliseconds(-value);
    public DateTime SubstractSeconds(double value) => @this.AddSeconds(-value);
    public DateTime SubstractMinutes(double value) => @this.AddMinutes(-value);
    public DateTime SubstractHours(double value) => @this.AddHours(-value);
    public DateTime SubstractDays(double value) => @this.AddDays(-value);
    public DateTime SubstractWeeks(int weeks) => @this.AddWeeks(-weeks);
    public DateTime SubstractMonths(int months) => @this.AddMonths(-months);
    public DateTime SubstractYears(int value) => @this.AddYears(-value);
    public long AsUnixTicksUtc() => ((DateTimeOffset)@this.ToUniversalTime()).ToUnixTimeMilliseconds() * DateTimeExtensions._TICKS_PER_MILLISECOND;
    public long AsUnixMillisecondsUtc() => ((DateTimeOffset)@this.ToUniversalTime()).ToUnixTimeMilliseconds();
  }


  public static DateTime FromUnixTicks(long ticks, DateTimeKind kind = DateTimeKind.Unspecified) => DateTimeExtensions._CreateUnixEpochWithKind(kind).AddTicks(ticks);
  public static DateTime FromUnixSeconds(long seconds, DateTimeKind kind = DateTimeKind.Unspecified) => DateTimeExtensions._CreateUnixEpochWithKind(kind).AddSeconds(seconds);

  private static DateTime _CreateUnixEpochWithKind(DateTimeKind kind) => new(1970, 1, 1, 0, 0, 0, kind);

  extension(DateTime) {

    /// <summary>
    /// Generates a finite sequence of DateTime values from a start value to an end value (inclusive) with a specified step.
    /// </summary>
    /// <param name="start">The value of the first element in the sequence.</param>
    /// <param name="endInclusive">The maximum value of elements in the sequence (inclusive).</param>
    /// <param name="step">The TimeSpan to add to each subsequent element.</param>
    /// <returns>An enumerable of DateTime values from start to end.</returns>
    /// <example>
    /// <code>
    /// // Generate hourly timestamps for a day
    /// var hours = DateTimeExtensions.Sequence(DateTime.Today, DateTime.Today.AddDays(1), TimeSpan.FromHours(1));
    ///
    /// // Generate daily dates for a week
    /// var days = DateTimeExtensions.Sequence(DateTime.Today, DateTime.Today.AddDays(7), TimeSpan.FromDays(1));
    /// </code>
    /// </example>
    public static IEnumerable<DateTime> Sequence(DateTime start, DateTime endInclusive, TimeSpan step) {
      for (var current = start; current <= endInclusive; current += step)
        yield return current;
    }

    /// <summary>
    /// Generates an infinite sequence of DateTime values starting at a specified value with a specified step.
    /// </summary>
    /// <param name="start">The value of the first element in the sequence.</param>
    /// <param name="step">The TimeSpan to add to each subsequent element.</param>
    /// <returns>An infinite enumerable of DateTime values.</returns>
    /// <example>
    /// <code>
    /// // Generate the next 10 hourly timestamps
    /// var nextHours = DateTimeExtensions.InfiniteSequence(DateTime.Now, TimeSpan.FromHours(1)).Take(10);
    ///
    /// // Generate timestamps every 15 minutes
    /// var intervals = DateTimeExtensions.InfiniteSequence(DateTime.Today, TimeSpan.FromMinutes(15)).Take(100);
    /// </code>
    /// </example>
    public static IEnumerable<DateTime> InfiniteSequence(DateTime start, TimeSpan step) {
      var current = start;
      while (true) {
        yield return current;
        current += step;
      }
    }
  }

}
