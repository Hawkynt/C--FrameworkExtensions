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

#if !SUPPORTS_COUNTDOWN_EVENT

namespace System.Threading;

/// <summary>
/// Represents a synchronization primitive that is signaled when its count reaches zero.
/// </summary>
/// <remarks>
/// This is a polyfill for .NET 2.0/3.5 that simulates CountdownEvent using ManualResetEvent.
/// </remarks>
public class CountdownEvent : IDisposable {
  private readonly object _lock = new();
  private readonly ManualResetEvent _event;
  private int _currentCount;
  private int _initialCount;
  private bool _disposed;

  /// <summary>
  /// Initializes a new instance with the specified count.
  /// </summary>
  /// <param name="initialCount">The number of signals initially required to set the event.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is less than 0.</exception>
  public CountdownEvent(int initialCount) {
    ArgumentOutOfRangeException.ThrowIfNegative(initialCount);
    this._initialCount = initialCount;
    this._currentCount = initialCount;
    this._event = new(initialCount == 0);
  }

  /// <summary>
  /// Gets the number of remaining signals required to set the event.
  /// </summary>
  public int CurrentCount {
    get {
      lock (this._lock)
        return this._currentCount;
    }
  }

  /// <summary>
  /// Gets the number of signals initially required to set the event.
  /// </summary>
  public int InitialCount {
    get {
      lock (this._lock)
        return this._initialCount;
    }
  }

  /// <summary>
  /// Gets whether the event is set.
  /// </summary>
  public bool IsSet {
    get {
      lock (this._lock)
        return this._currentCount == 0;
    }
  }

  /// <summary>
  /// Gets a <see cref="WaitHandle"/> that is used to wait for the event to be set.
  /// </summary>
  public WaitHandle WaitHandle {
    get {
      ObjectDisposedException.ThrowIf(this._disposed, this);
      return this._event;
    }
  }

  /// <summary>
  /// Increments the <see cref="CurrentCount"/> by one.
  /// </summary>
  /// <exception cref="InvalidOperationException">The current instance is already set.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public void AddCount() => this.AddCount(1);

  /// <summary>
  /// Increments the <see cref="CurrentCount"/> by a specified value.
  /// </summary>
  /// <param name="signalCount">The value by which to increase <see cref="CurrentCount"/>.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="signalCount"/> is less than or equal to 0.</exception>
  /// <exception cref="InvalidOperationException">The current instance is already set.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public void AddCount(int signalCount) {
    if (!this.TryAddCount(signalCount))
      throw new InvalidOperationException("The event is already signaled and cannot be incremented.");
  }

  /// <summary>
  /// Attempts to increment <see cref="CurrentCount"/> by one.
  /// </summary>
  /// <returns><see langword="true"/> if the increment succeeded; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool TryAddCount() => this.TryAddCount(1);

  /// <summary>
  /// Attempts to increment <see cref="CurrentCount"/> by a specified value.
  /// </summary>
  /// <param name="signalCount">The value by which to increase <see cref="CurrentCount"/>.</param>
  /// <returns><see langword="true"/> if the increment succeeded; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="signalCount"/> is less than or equal to 0.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool TryAddCount(int signalCount) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(signalCount);
    ObjectDisposedException.ThrowIf(this._disposed, this);

    lock (this._lock) {
      if (this._currentCount == 0)
        return false;

      this._currentCount += signalCount;
      return true;
    }
  }

  /// <summary>
  /// Registers a signal with the <see cref="CountdownEvent"/>, decrementing the value of <see cref="CurrentCount"/>.
  /// </summary>
  /// <returns><see langword="true"/> if the signal caused the count to reach zero; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="InvalidOperationException">The current instance is already set.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool Signal() => this.Signal(1);

  /// <summary>
  /// Registers multiple signals with the <see cref="CountdownEvent"/>, decrementing the value of <see cref="CurrentCount"/>.
  /// </summary>
  /// <param name="signalCount">The number of signals to register.</param>
  /// <returns><see langword="true"/> if the signals caused the count to reach zero; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="signalCount"/> is less than 1.</exception>
  /// <exception cref="InvalidOperationException">The current instance is already set, or <paramref name="signalCount"/> is greater than <see cref="CurrentCount"/>.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool Signal(int signalCount) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(signalCount);
    ObjectDisposedException.ThrowIf(this._disposed, this);

    lock (this._lock) {
      if (this._currentCount == 0)
        throw new InvalidOperationException("The event is already signaled and cannot be decremented.");

      if (signalCount > this._currentCount)
        throw new InvalidOperationException("The signalCount argument is greater than the current count.");

      this._currentCount -= signalCount;
      if (this._currentCount == 0) {
        this._event.Set();
        return true;
      }

      return false;
    }
  }

  /// <summary>
  /// Resets the <see cref="CurrentCount"/> to the value of <see cref="InitialCount"/>.
  /// </summary>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public void Reset() {
    lock (this._lock)
      this.Reset(this._initialCount);
  }

  /// <summary>
  /// Resets the <see cref="CurrentCount"/> to a specified value.
  /// </summary>
  /// <param name="count">The new value of <see cref="CurrentCount"/>.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public void Reset(int count) {
    ArgumentOutOfRangeException.ThrowIfNegative(count);
    ObjectDisposedException.ThrowIf(this._disposed, this);

    lock (this._lock) {
      this._initialCount = count;
      this._currentCount = count;
      if (count == 0)
        this._event.Set();
      else
        this._event.Reset();
    }
  }

  /// <summary>
  /// Blocks the current thread until the <see cref="CountdownEvent"/> is set.
  /// </summary>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public void Wait() => this.Wait(Timeout.Infinite, CancellationToken.None);

  /// <summary>
  /// Blocks the current thread until the <see cref="CountdownEvent"/> is set, using a <see cref="TimeSpan"/> to measure the timeout.
  /// </summary>
  /// <param name="timeout">A <see cref="TimeSpan"/> that represents the timeout.</param>
  /// <returns><see langword="true"/> if the event was set; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative value other than -1 milliseconds.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool Wait(TimeSpan timeout) => this.Wait((int)timeout.TotalMilliseconds, CancellationToken.None);

  /// <summary>
  /// Blocks the current thread until the <see cref="CountdownEvent"/> is set, using a 32-bit signed integer to measure the timeout.
  /// </summary>
  /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
  /// <returns><see langword="true"/> if the event was set; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative value other than -1.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool Wait(int millisecondsTimeout) => this.Wait(millisecondsTimeout, CancellationToken.None);

  /// <summary>
  /// Blocks the current thread until the <see cref="CountdownEvent"/> is set, while observing a <see cref="CancellationToken"/>.
  /// </summary>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
  /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public void Wait(CancellationToken cancellationToken) => this.Wait(Timeout.Infinite, cancellationToken);

  /// <summary>
  /// Blocks the current thread until the <see cref="CountdownEvent"/> is set, using a <see cref="TimeSpan"/> to measure the timeout, while observing a <see cref="CancellationToken"/>.
  /// </summary>
  /// <param name="timeout">A <see cref="TimeSpan"/> that represents the timeout.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
  /// <returns><see langword="true"/> if the event was set; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative value other than -1 milliseconds.</exception>
  /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
    => this.Wait((int)timeout.TotalMilliseconds, cancellationToken);

  /// <summary>
  /// Blocks the current thread until the <see cref="CountdownEvent"/> is set, using a 32-bit signed integer to measure the timeout, while observing a <see cref="CancellationToken"/>.
  /// </summary>
  /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
  /// <returns><see langword="true"/> if the event was set; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative value other than -1.</exception>
  /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
  /// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
  public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken) {
    ArgumentOutOfRangeException.ThrowIfLessThan(millisecondsTimeout, Timeout.Infinite);
    ObjectDisposedException.ThrowIf(this._disposed, this);
    cancellationToken.ThrowIfCancellationRequested();

    // Quick check - already signaled
    lock (this._lock) {
      if (this._currentCount == 0)
        return true;
    }

    if (millisecondsTimeout == 0)
      return false;

    // Handle infinite timeout without cancellation
    if (millisecondsTimeout == Timeout.Infinite && !cancellationToken.CanBeCanceled)
      return this._event.WaitOne();

    // Use polling for cancellation support
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    while (true) {
      cancellationToken.ThrowIfCancellationRequested();

      var remaining = millisecondsTimeout == Timeout.Infinite
        ? 100
        : Math.Max(0, millisecondsTimeout - (int)stopwatch.ElapsedMilliseconds);

      var waitTime = Math.Min(remaining, 100);
      if (this._event.WaitOne(waitTime, false))
        return true;

      if (millisecondsTimeout != Timeout.Infinite && stopwatch.ElapsedMilliseconds >= millisecondsTimeout)
        return false;
    }
  }

  /// <summary>
  /// Releases all resources used by the current instance.
  /// </summary>
  public void Dispose() {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Releases the unmanaged resources used by the <see cref="CountdownEvent"/>, and optionally releases the managed resources.
  /// </summary>
  /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
  protected virtual void Dispose(bool disposing) {
    if (this._disposed)
      return;

    if (disposing)
      this._event.Close();

    this._disposed = true;
  }
}

#endif
