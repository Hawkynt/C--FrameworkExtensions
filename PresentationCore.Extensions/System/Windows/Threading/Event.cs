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

namespace System.Windows.Threading {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class EventExtensions {
    /// <summary>
    /// Invokes an event safely from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads.
    /// </summary>
    /// <typeparam name="T">The type of event arguments.</typeparam>
    /// <param name="This">This EventHandler.</param>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">The event args.</param>
    public static void SafeInvoke<T>(this EventHandler<T> This, object sender, T eventArgs) where T : EventArgs {
      if (This == null) {
        // no subscribers
        return;
      }
      var copy = This;
      foreach (var @delegate in copy.GetInvocationList()) {
        var dispatcherObject = @delegate.Target as DispatcherObject;
        if (dispatcherObject != null) {
          if (dispatcherObject.Dispatcher.CheckAccess())
            @delegate.DynamicInvoke(sender, eventArgs);
          else {
            var delegateCopy = @delegate;
            dispatcherObject.Dispatcher.Invoke(new Action(() => delegateCopy.DynamicInvoke(sender, eventArgs)), null);
          }
        } else
          @delegate.DynamicInvoke(sender, eventArgs);
      }
    }

    /// <summary>
    /// Invokes an event safely from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads.
    /// </summary>
    /// <param name="This">This MulticastDelegate.</param>
    /// <param name="arguments">The arguments.</param>
    public static void SafeInvoke(this MulticastDelegate This, params object[] arguments) {
      if (This == null) {
        // no subscribers
        return;
      }
      var copy = This;
      foreach (var @delegate in copy.GetInvocationList()) {
        var dispatcherObject = @delegate.Target as DispatcherObject;
        if (dispatcherObject != null) {
          if (dispatcherObject.Dispatcher.CheckAccess())
            @delegate.DynamicInvoke(arguments);
          else {
            var delegateCopy = @delegate;
            dispatcherObject.Dispatcher.Invoke(new Action(() => delegateCopy.DynamicInvoke(arguments)), null);
          }
        } else
          @delegate.DynamicInvoke(arguments);
      }
    }

    /// <summary>
    /// Invokes an event safely asynchroneously from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads.
    /// </summary>
    /// <typeparam name="T">The type of event arguments.</typeparam>
    /// <param name="This">This EventHandler.</param>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">The event args.</param>
    public static void AsyncSafeInvoke<T>(this EventHandler<T> This, object sender, T eventArgs) where T : EventArgs {
      if (This == null) {
        // no subscribers
        return;
      }
      var copy = This;
      foreach (var @delegate in copy.GetInvocationList()) {
        var dispatcherObject = @delegate.Target as DispatcherObject;
        var delegateCopy = @delegate;
        Action call;
        if (dispatcherObject != null) {
          call = () => {
            if (dispatcherObject.Dispatcher.CheckAccess())
              delegateCopy.DynamicInvoke(sender, eventArgs);
            else
              dispatcherObject.Dispatcher.BeginInvoke(new Action(() => delegateCopy.DynamicInvoke(sender, eventArgs)), null);
          };
        } else
          call = () => delegateCopy.DynamicInvoke(sender, eventArgs);
        call.BeginInvoke(call.EndInvoke, null);
      }
    }

    /// <summary>
    /// Invokes an event safely asychroneously from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads.
    /// </summary>
    /// <param name="This">This MulticastDelegate.</param>
    /// <param name="arguments">The arguments.</param>
    public static void AsyncSafeInvoke(this MulticastDelegate This, params object[] arguments) {
      if (This == null) {
        // no subscribers
        return;
      }
      var copy = This;
      foreach (var @delegate in copy.GetInvocationList()) {
        var dispatcherObject = @delegate.Target as DispatcherObject;
        var delegateCopy = @delegate;
        Action call;
        if (dispatcherObject != null) {
          call = () => {
            if (dispatcherObject.Dispatcher.CheckAccess())
              delegateCopy.DynamicInvoke(arguments);
            else {
              var intI = 30;
              while (intI > 0) {
                try {
                  dispatcherObject.Dispatcher.BeginInvoke(new Action(() => delegateCopy.DynamicInvoke(arguments)), null);
                  intI = 0;
                } catch (Exception) {
                  intI--;
                  System.Threading.Thread.Sleep(0);
                }
              }
            }
          };
        } else
          call = () => delegateCopy.DynamicInvoke(arguments);
        call.BeginInvoke(call.EndInvoke, null);
      }
    }
  }
}
