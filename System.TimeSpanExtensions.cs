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
using word = System.UInt16;
using dword = System.UInt32;
using qword = System.UInt64;

namespace System {
  internal static partial class TimeSpanExtensions {
    /// <summary>
    /// Divides the time by a divisor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="divisor">The divisor.</param>
    /// <returns>A fraction of the original timespan.</returns>
    public static TimeSpan DivideBy(this TimeSpan This, qword divisor) {
      return (TimeSpan.FromTicks((long)(This.Ticks / (double)divisor)));
    }

    /// <summary>
    /// Multiplies the time by a given factor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>A multiple of the original timespan.</returns>
    public static TimeSpan MultiplyBy(this TimeSpan This, qword multiplier) {
      return (TimeSpan.FromTicks((long)(This.Ticks * (double)multiplier)));
    }

    /// <summary>
    /// Divides the time by a divisor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="divisor">The divisor.</param>
    /// <returns>A fraction of the original timespan.</returns>
    public static TimeSpan DivideBy(this TimeSpan This, int divisor) {
      return (TimeSpan.FromTicks(This.Ticks / divisor));
    }

    /// <summary>
    /// Multiplies the time by a given factor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>A multiple of the original timespan.</returns>
    public static TimeSpan MultiplyBy(this TimeSpan This, int multiplier) {
      return (TimeSpan.FromTicks(This.Ticks * multiplier));
    }

    /// <summary>
    /// Divides the time by a divisor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="divisor">The divisor.</param>
    /// <returns>A fraction of the original timespan.</returns>
    public static TimeSpan DivideBy(this TimeSpan This, double divisor) {
      return (TimeSpan.FromTicks((long)(This.Ticks / divisor)));
    }

    /// <summary>
    /// Multiplies the time by a given factor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>A multiple of the original timespan.</returns>
    public static TimeSpan MultiplyBy(this TimeSpan This, double multiplier) {
      return (TimeSpan.FromTicks((long)(This.Ticks * multiplier)));
    }

    /// <summary>
    /// Divides the time by a divisor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="divisor">The divisor.</param>
    /// <returns>A fraction of the original timespan.</returns>
    public static TimeSpan DivideBy(this TimeSpan This, decimal divisor) {
      return (This.DivideBy((double)divisor));
    }

    /// <summary>
    /// Multiplies the time by a given factor.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>A multiple of the original timespan.</returns>
    public static TimeSpan MultiplyBy(this TimeSpan This, decimal multiplier) {
      return (This.MultiplyBy((double)multiplier));
    }
  }
}