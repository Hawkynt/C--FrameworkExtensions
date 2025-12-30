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

#if SUPPORTS_CANCELLATIONTOKENSOURCE && !SUPPORTS_CANCELLATIONTOKENSOURCE_CANCELAFTER

namespace System.Threading;

/// <summary>
/// Polyfills for <see cref="CancellationTokenSource.CancelAfter"/> method added in .NET 4.5.
/// </summary>
public static partial class CancellationTokenSourcePolyfills {

  extension(CancellationTokenSource source) {

    /// <summary>
    /// Schedules a cancel operation on this <see cref="CancellationTokenSource"/> after the specified time span.
    /// </summary>
    /// <param name="delay">The time span to wait before canceling this <see cref="CancellationTokenSource"/>.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="CancellationTokenSource"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is less than -1 milliseconds.</exception>
    public void CancelAfter(TimeSpan delay) => source.CancelAfter((int)delay.TotalMilliseconds);

    /// <summary>
    /// Schedules a cancel operation on this <see cref="CancellationTokenSource"/> after the specified number of milliseconds.
    /// </summary>
    /// <param name="millisecondsDelay">The time in milliseconds to wait before canceling this <see cref="CancellationTokenSource"/>.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="CancellationTokenSource"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsDelay"/> is less than -1.</exception>
    public void CancelAfter(int millisecondsDelay) {
      if (millisecondsDelay < -1)
        throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));

      if (source.IsCancellationRequested)
        return;

      if (millisecondsDelay == Timeout.Infinite)
        return;

      var timer = new Timer(_ => {
        try {
          source.Cancel();
        } catch (ObjectDisposedException) {
          // CancellationTokenSource was disposed before timer fired
        }
      }, null, millisecondsDelay, Timeout.Infinite);

      // Register cleanup when cancellation is requested or source is disposed
      source.Token.Register(() => timer.Dispose());
    }

  }

}

#endif
