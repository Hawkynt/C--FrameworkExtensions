#region (c)2010-2020 Hawkynt
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
namespace System {
  internal static partial class DateTimeExtensions {

    /// <summary>
    /// Returns the end of the day of the given date.
    /// </summary>
    /// <param name="this">This DateTime.</param>
    /// <returns>An instance pointing to the last moment of the given day.</returns>
    public static DateTime EndOfDay(this DateTime @this)
          =>
            @this.DayOfYear == DateTime.MinValue.DayOfYear && @this.Year == DateTime.MinValue.Year
            ? @this.AddDays(1).Subtract(@this.TimeOfDay).AddTicks(-1)
            : @this.Subtract(@this.TimeOfDay).AddTicks(-1).AddDays(1)
          ;

    /// <summary>
    /// Returns the start of the day of the given date.
    /// </summary>
    /// <param name="this">This DateTime.</param>
    /// <returns>An instance pointing to the first moment of the given day.</returns>
    public static DateTime StartOfDay(this DateTime @this)
      => @this.Subtract(@this.TimeOfDay)
      ;
  }
}