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

using Guard;

namespace System.Windows.Threading;

public static partial class DispatcherObjectExtensions {
  /// <summary>
  ///   Safely invokes an action on a dispatcher object.
  /// </summary>
  /// <param name="this">This DispatcherObject.</param>
  /// <param name="action">The action.</param>
  /// <param name="async">
  ///   if set to <c>true</c> we'll be going to make an asynchronous call to the dispatcher; otherwise,
  ///   we'll wait till execution ends.
  /// </param>
  /// <returns><c>true</c> when the task could be executed on the current thread immediately; otherwise, <c>false</c>.</returns>
  public static bool SafelyInvoke(this DispatcherObject @this, Action action, bool async = false, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    var dispatcher = @this.Dispatcher;
    if (dispatcher.CheckAccess()) {
      action();
      return true;
    }

    if (async)
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
