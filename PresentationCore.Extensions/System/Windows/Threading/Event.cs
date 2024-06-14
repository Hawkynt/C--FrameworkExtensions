#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Threading;

namespace System.Windows.Threading;

public static partial class EventExtensions {
  
  /// <summary>
  ///   Invokes an event safely from no matter what thread and makes sure that the subscribers who needs it, get the event
  ///   invocation in their own threads.
  /// </summary>
  /// <typeparam name="T">The type of event arguments.</typeparam>
  /// <param name="this">This EventHandler.</param>
  /// <param name="sender">The sender.</param>
  /// <param name="eventArgs">The event args.</param>
  public static void SafeInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs) where T : EventArgs {
    if (@this == null)
      // no subscribers
      return;
    
    var copy = @this;
    foreach (var @delegate in copy.GetInvocationList())
      if (@delegate.Target is DispatcherObject dispatcherObject) {
        if (dispatcherObject.Dispatcher.CheckAccess())
          @delegate.DynamicInvoke(sender, eventArgs);
        else {
          var delegateCopy = @delegate;
          dispatcherObject.Dispatcher.Invoke(new Action(() => delegateCopy.DynamicInvoke(sender, eventArgs)), null);
        }
      } else
        @delegate.DynamicInvoke(sender, eventArgs);
  }

  /// <summary>
  ///   Invokes an event safely from no matter what thread and makes sure that the subscribers who needs it, get the event
  ///   invocation in their own threads.
  /// </summary>
  /// <param name="this">This MulticastDelegate.</param>
  /// <param name="arguments">The arguments.</param>
  public static void SafeInvoke(this MulticastDelegate @this, params object[] arguments) {
    if (@this == null)
      // no subscribers
      return;
    
    var copy = @this;
    foreach (var @delegate in copy.GetInvocationList())
      if (@delegate.Target is DispatcherObject dispatcherObject) {
        if (dispatcherObject.Dispatcher.CheckAccess())
          @delegate.DynamicInvoke(arguments);
        else {
          var delegateCopy = @delegate;
          dispatcherObject.Dispatcher.Invoke(new Action(() => delegateCopy.DynamicInvoke(arguments)), null);
        }
      } else
        @delegate.DynamicInvoke(arguments);
  }

  /// <summary>
  ///   Invokes an event safely asynchroneously from no matter what thread and makes sure that the subscribers who needs it,
  ///   get the event invocation in their own threads.
  /// </summary>
  /// <typeparam name="T">The type of event arguments.</typeparam>
  /// <param name="this">This EventHandler.</param>
  /// <param name="sender">The sender.</param>
  /// <param name="eventArgs">The event args.</param>
  public static void AsyncSafeInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs) where T : EventArgs {
    if (@this == null)
      // no subscribers
      return;
    
    var copy = @this;
    foreach (var @delegate in copy.GetInvocationList()) {
      var dispatcherObject = @delegate.Target as DispatcherObject;
      var delegateCopy = @delegate;
      Action call;
      if (dispatcherObject != null)
        call = () => {
          if (dispatcherObject.Dispatcher.CheckAccess())
            delegateCopy.DynamicInvoke(sender, eventArgs);
          else
            dispatcherObject.Dispatcher.BeginInvoke(
              new Action(() => delegateCopy.DynamicInvoke(sender, eventArgs)),
              null
            );
        };
      else
        call = () => delegateCopy.DynamicInvoke(sender, eventArgs);
      call.BeginInvoke(call.EndInvoke, null);
    }
  }

  /// <summary>
  ///   Invokes an event safely asychroneously from no matter what thread and makes sure that the subscribers who needs it,
  ///   get the event invocation in their own threads.
  /// </summary>
  /// <param name="this">This MulticastDelegate.</param>
  /// <param name="arguments">The arguments.</param>
  public static void AsyncSafeInvoke(this MulticastDelegate @this, params object[] arguments) {
    if (@this == null)
      // no subscribers
      return;
    
    var copy = @this;
    foreach (var @delegate in copy.GetInvocationList()) {
      var dispatcherObject = @delegate.Target as DispatcherObject;
      var delegateCopy = @delegate;
      Action call;
      if (dispatcherObject != null)
        call = () => {
          if (dispatcherObject.Dispatcher.CheckAccess())
            delegateCopy.DynamicInvoke(arguments);
          else {
            var i = 30;
            while (i > 0)
              try {
                dispatcherObject.Dispatcher.BeginInvoke(
                  new Action(() => delegateCopy.DynamicInvoke(arguments)),
                  null
                );
                i = 0;
              } catch (Exception) {
                --i;
                Thread.Sleep(0);
              }
          }
        };
      else
        call = () => delegateCopy.DynamicInvoke(arguments);
      call.BeginInvoke(call.EndInvoke, null);
    }
  }
}
