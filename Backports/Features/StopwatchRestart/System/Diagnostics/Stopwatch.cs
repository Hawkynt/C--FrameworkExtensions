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

#if !SUPPORTS_STOPWATCH_RESTART

namespace System.Diagnostics;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class StopwatchPolyfills {

  /// <summary>
  /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
  /// </summary>
  /// <param name="this">This <see cref="Stopwatch"/></param>
  public static void Restart(this Stopwatch @this) {
    if (@this == null)
      throw new NullReferenceException();
    
    @this.Reset();
    @this.Start();
  }

}

#endif
