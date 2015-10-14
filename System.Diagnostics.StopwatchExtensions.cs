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

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Diagnostics {
  internal static partial class StopwatchExtensions {

    private const long TicksPerMillisecond = 10000L;
    private const long TicksPerSecond = 1000 * TicksPerMillisecond;

    private static double tickFrequency => Stopwatch.IsHighResolution ? TicksPerSecond / (double)Stopwatch.Frequency : 1;

    /// <summary>
    /// Gets the elapsed fractional milliseconds.
    /// </summary>
    /// <param name="This">This TimeSpan.</param>
    /// <returns></returns>
    public static double GetElapsedMilliseconds(this Stopwatch This) => This.ElapsedTicks * tickFrequency / TicksPerMillisecond;
  }
}