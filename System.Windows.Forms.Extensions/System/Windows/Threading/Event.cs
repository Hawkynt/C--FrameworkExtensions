#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.ComponentModel;
using System.Threading;

namespace System.Windows.Threading;

public static partial class EventExtensions {

  private const int _RECALL_RETRY_COUNT = 30;
  private const int _RECALL_RETRY_DELAY_IN_MILLISECONDS = 0;
  
  /// <summary>
  /// Invokes an event safely from any thread and ensures that subscribers who require it receive the event invocation in their own threads.
  /// </summary>
  /// <typeparam name="T">The type of event arguments.</typeparam>
  /// <param name="this">The event handler to invoke.</param>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="eventArgs">The event arguments.</param>
  /// <example>
  /// <code>
  /// public class MyEventArgs : EventArgs
  /// {
  ///     public string Message { get; set; }
  /// }
  ///
  /// public class Publisher
  /// {
  ///     public event EventHandler&lt;MyEventArgs&gt; MyEvent;
  ///
  ///     public void RaiseEvent(string message)
  ///     {
  ///         MyEvent.SafeInvoke(this, new MyEventArgs { Message = message });
  ///     }
  /// }
  ///
  /// public class Subscriber
  /// {
  ///     public void Subscribe(Publisher publisher)
  ///     {
  ///         publisher.MyEvent += (sender, args) =>
  ///         {
  ///             Console.WriteLine($"Received message: {args.Message}");
  ///         };
  ///     }
  /// }
  ///
  /// Publisher pub = new Publisher();
  /// Subscriber sub = new Subscriber();
  /// sub.Subscribe(pub);
  /// pub.RaiseEvent("Hello, world!");
  /// // Output: Received message: Hello, world!
  /// </code>
  /// </example>
  public static void SafeInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs) where T : EventArgs {
    if (@this == null)
      // no subscribers
      return;
    var copy = @this;
    foreach (var @delegate in copy.GetInvocationList())
      if (@delegate.Target is ISynchronizeInvoke dispatcherObject) {
        if (!dispatcherObject.InvokeRequired)
          @delegate.DynamicInvoke(sender, eventArgs);
        else {
          var delegateCopy = @delegate;
          dispatcherObject.BeginInvoke(new Action(() => delegateCopy.DynamicInvoke(sender, eventArgs)), null);
        }
      } else
        @delegate.DynamicInvoke(sender, eventArgs);
  }

  /// <summary>
  /// Invokes an event safely from any thread and ensures that subscribers who require it receive the event invocation in their own threads.
  /// </summary>
  /// <param name="this">The multicast delegate to invoke.</param>
  /// <param name="arguments">The arguments to pass to the event handlers.</param>
  /// <example>
  /// <code>
  /// public class Publisher
  /// {
  ///     public event EventHandler MyEvent;
  ///
  ///     public void RaiseEvent()
  ///     {
  ///         MyEvent.SafeInvoke(this, EventArgs.Empty);
  ///     }
  /// }
  ///
  /// public class Subscriber
  /// {
  ///     public void Subscribe(Publisher publisher)
  ///     {
  ///         publisher.MyEvent += (sender, args) =>
  ///         {
  ///             Console.WriteLine("Event received");
  ///         };
  ///     }
  /// }
  ///
  /// Publisher pub = new Publisher();
  /// Subscriber sub = new Subscriber();
  /// sub.Subscribe(pub);
  /// pub.RaiseEvent();
  /// // Output: Event received
  /// </code>
  /// </example>
  public static void SafeInvoke(this MulticastDelegate @this, params object[] arguments) {
    if (@this == null)
      // no subscribers
      return;
    var copy = @this;
    foreach (var @delegate in copy.GetInvocationList())
      if (@delegate.Target is ISynchronizeInvoke dispatcherObject) {
        if (!dispatcherObject.InvokeRequired)
          @delegate.DynamicInvoke(arguments);
        else {
          var delegateCopy = @delegate;
          dispatcherObject.BeginInvoke(new Action(() => delegateCopy.DynamicInvoke(arguments)), null);
        }
      } else
        @delegate.DynamicInvoke(arguments);
  }

  /// <summary>
  /// Invokes an event safely and asynchronously from any thread, ensuring that subscribers who require it receive the event invocation in their own threads.
  /// </summary>
  /// <typeparam name="T">The type of event arguments.</typeparam>
  /// <param name="this">The event handler to invoke.</param>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="eventArgs">The event arguments.</param>
  /// <example>
  /// <code>
  /// public class MyEventArgs : EventArgs
  /// {
  ///     public string Message { get; set; }
  /// }
  ///
  /// public class Publisher
  /// {
  ///     public event EventHandler&lt;MyEventArgs&gt; MyEvent;
  ///
  ///     public void RaiseEvent(string message)
  ///     {
  ///         MyEvent.AsyncSafeInvoke(this, new MyEventArgs { Message = message });
  ///     }
  /// }
  ///
  /// public class Subscriber
  /// {
  ///     public void Subscribe(Publisher publisher)
  ///     {
  ///         publisher.MyEvent += async (sender, args) =>
  ///         {
  ///             await Task.Delay(100); // Simulate async work
  ///             Console.WriteLine($"Received message: {args.Message}");
  ///         };
  ///     }
  /// }
  ///
  /// Publisher pub = new Publisher();
  /// Subscriber sub = new Subscriber();
  /// sub.Subscribe(pub);
  /// pub.RaiseEvent("Hello, world!");
  /// // Output: Received message: Hello, world!
  /// </code>
  /// </example>
  public static void AsyncSafeInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs) where T : EventArgs {
    var delegates = @this;

    // no subscribers
    if (delegates == null)
      return;

    foreach (var @delegate in delegates.GetInvocationList()) {
      var copy = @delegate;

      Action call = copy.Target is ISynchronizeInvoke dispatcherObject
          ? () => CallSynchronized(dispatcherObject, copy)
          : () => copy.DynamicInvoke(sender, eventArgs)
        ;

      call.BeginInvoke(call.EndInvoke, null);
    }

    return;

    void CallSynchronized(ISynchronizeInvoke dispatcherObject, Delegate delegateCopy) {
      if (!dispatcherObject.InvokeRequired)
        delegateCopy.DynamicInvoke(sender, eventArgs);
      else {
        var i = _RECALL_RETRY_COUNT;
        do {
          try {
            dispatcherObject.BeginInvoke(new Action(() => delegateCopy.DynamicInvoke(sender, eventArgs)), null);
            return;
          } catch (Exception) {
            --i;
            Thread.Sleep(_RECALL_RETRY_DELAY_IN_MILLISECONDS);
          }
        } while (i > 0);
      }
    }

  }

  /// <summary>
  /// Invokes an event safely and asynchronously from any thread, ensuring that subscribers who require it receive the event invocation in their own threads.
  /// </summary>
  /// <param name="this">The multicast delegate to invoke.</param>
  /// <param name="arguments">The arguments to pass to the event handlers.</param>
  /// <example>
  /// <code>
  /// public class Publisher
  /// {
  ///     public event EventHandler MyEvent;
  ///
  ///     public void RaiseEvent()
  ///     {
  ///         MyEvent.AsyncSafeInvoke(this, EventArgs.Empty);
  ///     }
  /// }
  ///
  /// public class Subscriber
  /// {
  ///     public void Subscribe(Publisher publisher)
  ///     {
  ///         publisher.MyEvent += async (sender, args) =>
  ///         {
  ///             await Task.Delay(100); // Simulate async work
  ///             Console.WriteLine("Event received");
  ///         };
  ///     }
  /// }
  ///
  /// Publisher pub = new Publisher();
  /// Subscriber sub = new Subscriber();
  /// sub.Subscribe(pub);
  /// pub.RaiseEvent();
  /// // Output: Event received
  /// </code>
  /// </example>
  public static void AsyncSafeInvoke(this MulticastDelegate @this, params object[] arguments) {
    var delegates = @this;

    // no subscribers
    if (delegates == null)
      return;

    foreach (var @delegate in delegates.GetInvocationList()) {
      var copy = @delegate;
      
      Action call = copy.Target is ISynchronizeInvoke dispatcherObject 
        ? () => CallSynchronized(dispatcherObject, copy) 
        : () => copy.DynamicInvoke(arguments)
        ;

      call.BeginInvoke(call.EndInvoke, null);
    }

    return;

    void CallSynchronized(ISynchronizeInvoke dispatcherObject, Delegate delegateCopy) {
      if (!dispatcherObject.InvokeRequired)
        delegateCopy.DynamicInvoke(arguments);
      else {
        var i = _RECALL_RETRY_COUNT;
        do {
          try {
            dispatcherObject.BeginInvoke(new Action(() => delegateCopy.DynamicInvoke(arguments)), null);
            return;
          } catch (Exception) {
            --i;
            Thread.Sleep(_RECALL_RETRY_DELAY_IN_MILLISECONDS);
          }
        } while (i > 0);
      }
    }

  }
}
