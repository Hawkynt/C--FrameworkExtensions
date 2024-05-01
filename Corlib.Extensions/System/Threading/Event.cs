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

namespace System.Threading;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class EventExtensions {
  /// <summary>
  /// Invoke all handlers in parallel.
  /// </summary>
  /// <typeparam name="T">Type of event handlers.</typeparam>
  /// <param name="this">The event.</param>
  /// <param name="sender">The sender.</param>
  /// <param name="eventArgs">The args.</param>
  public static void AsyncInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs) where T : EventArgs {
    var copy = @this;
    copy.GetInvocationList().ForEach(@delegate => {
      Action action = () => @delegate.DynamicInvoke(sender, eventArgs);
      action.BeginInvoke(action.EndInvoke, null);
    });
  }

  /// <summary>
  /// Invoke all handlers in parallel.
  /// </summary>
  /// <param name="this">The event.</param>
  /// <param name="arguments">The args.</param>
  public static void AsyncInvoke(this MulticastDelegate @this, params object[] arguments) {
    var copy = @this;
    copy.GetInvocationList().ForEach(@delegate => {
      Action action = () => @delegate.DynamicInvoke(arguments);
      action.BeginInvoke(action.EndInvoke, null);
    });
  }
}