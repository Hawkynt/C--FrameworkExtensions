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

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Threading {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class TimerExtensions {
    /// <summary>
    /// Stops the specified <see cref="Timer">Timer</see>.
    /// </summary>
    /// <param name="this">This <see cref="Timer">Timer</see>.</param>
    public static void Stop(this Timer @this) => @this.Change(Timeout.Infinite, Timeout.Infinite);

    /// <summary>
    /// Starts the specified <see cref="Timer">Timer</see>.
    /// </summary>
    /// <param name="this">This <see cref="Timer">Timer</see>.</param>
    /// <param name="timeout">The timeout before the first run occurs.</param>
    public static void Start(this Timer @this, TimeSpan timeout) => @this.Change((long)timeout.TotalMilliseconds, Timeout.Infinite);

    /// <summary>
    /// Starts the specified <see cref="Timer">Timer</see>.
    /// </summary>
    /// <param name="this">This <see cref="Timer">Timer</see>.</param>
    /// <param name="timeoutInMilliseconds">The timeoutInMilliseconds before the first run occurs.</param>
    public static void Start(this Timer @this, int timeoutInMilliseconds) => @this.Change(timeoutInMilliseconds, Timeout.Infinite);
  }
}