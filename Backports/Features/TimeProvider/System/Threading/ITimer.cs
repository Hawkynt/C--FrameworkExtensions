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

#if !SUPPORTS_ITIMER

using System.Threading.Tasks;

namespace System.Threading;

/// <summary>
/// Represents a timer that can be used to schedule callbacks.
/// </summary>
/// <remarks>
/// This interface represents a timer that can be controlled via the <see cref="Change"/> method.
/// It can be disposed synchronously via <see cref="IDisposable.Dispose"/> or asynchronously via
/// <see cref="IAsyncDisposable.DisposeAsync"/>.
/// </remarks>
public interface ITimer : IDisposable, IAsyncDisposable {

  /// <summary>
  /// Changes the start time and the interval between method invocations for a timer.
  /// </summary>
  /// <param name="dueTime">
  /// The amount of time to delay before the callback is invoked.
  /// Specify <see cref="Timeout.InfiniteTimeSpan"/> to prevent the timer from restarting.
  /// Specify <see cref="TimeSpan.Zero"/> to restart the timer immediately.
  /// </param>
  /// <param name="period">
  /// The time interval between invocations of the callback.
  /// Specify <see cref="Timeout.InfiniteTimeSpan"/> to disable periodic signaling.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the timer was successfully updated; otherwise, <see langword="false"/>.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="dueTime"/> or <paramref name="period"/> is less than -1.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="dueTime"/> or <paramref name="period"/> is greater than 4294967294 milliseconds.
  /// </exception>
  bool Change(TimeSpan dueTime, TimeSpan period);

}

#endif
