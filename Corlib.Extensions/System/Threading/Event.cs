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

namespace System.Threading;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class EventExtensions {
  /// <summary>
  ///   Invoke all handlers in parallel.
  /// </summary>
  /// <typeparam name="T">Type of event handlers.</typeparam>
  /// <param name="this">The event.</param>
  /// <param name="sender">The sender.</param>
  /// <param name="eventArgs">The args.</param>
  public static void AsyncInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs) where T : EventArgs {
    var copy = @this;
    copy
      .GetInvocationList()
      .ForEach(
        @delegate => {
          Action action = () => @delegate.DynamicInvoke(sender, eventArgs);
          action.BeginInvoke(action.EndInvoke, null);
        }
      );
  }

  /// <summary>
  ///   Invoke all handlers in parallel.
  /// </summary>
  /// <param name="this">The event.</param>
  /// <param name="arguments">The args.</param>
  public static void AsyncInvoke(this MulticastDelegate @this, params object[] arguments) {
    var copy = @this;
    copy
      .GetInvocationList()
      .ForEach(
        @delegate => {
          Action action = () => @delegate.DynamicInvoke(arguments);
          action.BeginInvoke(action.EndInvoke, null);
        }
      );
  }
}
