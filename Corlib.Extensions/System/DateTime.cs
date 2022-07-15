#region (c)2010-2042 Hawkynt
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
// ReSharper disable PartialTypeWithSinglePart

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class DateTimeExtensions {
    /// <summary>
    /// Returns the end of the day of the given date.
    /// </summary>
    /// <param name="this">This DateTime.</param>
    /// <param name="precisionInTicks">the amount of ticks to subtract</param>
    /// <returns>An instance pointing to the last moment of the given day.</returns>
    public static DateTime EndOfDay(this DateTime @this, long precisionInTicks = 1)
          =>
            @this.DayOfYear == DateTime.MinValue.DayOfYear && @this.Year == DateTime.MinValue.Year
            ? @this.AddDays(1).Subtract(@this.TimeOfDay).AddTicks(-precisionInTicks)
            : @this.Subtract(@this.TimeOfDay).AddTicks(-precisionInTicks).AddDays(1)
          ;

    /// <summary>
    /// Returns the start of the day of the given date.
    /// </summary>
    /// <param name="this">This DateTime.</param>
    /// <returns>An instance pointing to the first moment of the given day.</returns>
    public static DateTime StartOfDay(this DateTime @this)
      => @this.Subtract(@this.TimeOfDay)
      ;

    /// <summary>
    /// Adds the specific amount of weeks to the current DateTime
    /// </summary>
    /// <param name="this">The current DateTime object</param>
    /// <param name="weeks">The amount of weeks to be added</param>
    /// <returns>The new DateTime</returns>
    public static DateTime AddWeeks(this DateTime @this, int weeks) => @this.AddDays(7 * weeks);

    /// <summary>
    /// Calculates the DateTime of a specified DayOfWeek for the week of the given date
    /// </summary>
    /// <param name="this">The current DateTime object</param>
    /// <param name="weekDay">The DayOfWeek which should be returned</param>
    /// <returns>The DateTime of the week of day of the week of the given date</returns>
    public static DateTime DateOfDayOfCurrentWeek(this DateTime @this, DayOfWeek weekDay) {
      var weekStart = @this.AddDays(-(int)@this.DayOfWeek);

      return weekStart.AddDays((int)weekDay).EndOfDay();
    }

    /// <summary>
    /// Gets the last day of the current month of the given date
    /// </summary>
    /// <param name="this">The DateTime</param>
    /// <returns>The DateTime representing the last day of the current month of the given date</returns>
    public static DateTime LastDayOfMonth(this DateTime @this) {
      var monthDays = DateTime.DaysInMonth(@this.Year, @this.Month);

      return new DateTime(@this.Year, @this.Month, monthDays);
    }

    /// <summary>
    /// Gets the Monday of the current week 
    /// </summary>
    /// <param name="this">This DateTime</param>
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
    /// Get the given day in the current week.
    /// </summary>
    /// <param name="this">This DateTime</param>
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
    /// Returns the one that is greater.
    /// </summary>
    /// <param name="this">This DateTime</param>
    /// <param name="other">The comparand</param>
    /// <returns>The other value if it is later; otherwise, this instance.</returns>
    public static DateTime Max(this DateTime @this, DateTime other) => other > @this ? other : @this;

    /// <summary>
    /// Retuns the one that is less.
    /// </summary>
    /// <param name="this">This DateTime</param>
    /// <param name="other">The comparand</param>
    /// <returns>The other value if it is earlier; otherwise, this instance.</returns>
    public static DateTime Min(this DateTime @this, DateTime other) => other < @this ? other : @this;


    /// <summary>
    /// Enumerates all days between this and an enddate.
    /// </summary>
    /// <param name="this">this DateTime</param>
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
  }
}