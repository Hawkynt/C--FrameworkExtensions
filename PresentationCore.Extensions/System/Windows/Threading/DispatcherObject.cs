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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Windows.Threading;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class DispatcherObjectExtensions {
  /// <summary>
  ///   Safely invokes an action on a dispatcher object.
  /// </summary>
  /// <param name="This">This DispatcherObject.</param>
  /// <param name="action">The action.</param>
  /// <param name="async">
  ///   if set to <c>true</c> we'll be going to make an asynchronous call to the dispatcher; otherwise,
  ///   we'll wait till execution ends.
  /// </param>
  /// <returns><c>true</c> when the task could be executed on the current thread immediately; otherwise, <c>false</c>.</returns>
  public static bool SafelyInvoke(this DispatcherObject This, Action action, bool @async = false, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(This != null);
    Contract.Requires(action != null);
#endif
    var dispatcher = This.Dispatcher;
#if SUPPORTS_CONTRACTS
    Contract.Assume(dispatcher != null);
#endif
    if (dispatcher.CheckAccess()) {
      action();
      return true;
    }

    if (@async)
      dispatcher.BeginInvoke(action, dispatcherPriority);
    else
      dispatcher.Invoke(action, dispatcherPriority);

    return false;
  }

  /// <summary>
  ///   Invokes the action in another thread.
  /// </summary>
  /// <param name="This">The this.</param>
  /// <param name="action">The action.</param>
  /// <returns><c>true</c> when the actual thread was not the GUI thread; otherwise, <c>false</c>.</returns>
  public static bool Async(this DispatcherObject This, Action action) {
    if (This.Dispatcher.CheckAccess()) {
      action.BeginInvoke(action.EndInvoke, null);
      return false;
    }

    action();
    return true;
  }
}