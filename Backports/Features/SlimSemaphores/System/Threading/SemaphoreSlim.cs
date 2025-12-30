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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
using System.Threading.Tasks;

#if !SUPPORTS_SLIM_SEMAPHORES

namespace System.Threading {

/// <summary>
/// Represents a lightweight alternative to <see cref="Semaphore"/> that limits the number of threads
/// that can access a resource or pool of resources concurrently.
/// </summary>
public class SemaphoreSlim : IDisposable {
  private readonly Semaphore _semaphore;
  private readonly int _maxCount;
  private int _currentCount;
  private volatile bool _isDisposed;
  private ManualResetEvent? _availableWaitHandle;
  private readonly object _lock = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="SemaphoreSlim"/> class, specifying the initial number of requests that can be granted concurrently.
  /// </summary>
  /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> is less than 0.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public SemaphoreSlim(int initialCount) : this(initialCount, int.MaxValue) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="SemaphoreSlim"/> class, specifying the initial and maximum number of requests that can be granted concurrently.
  /// </summary>
  /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
  /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="initialCount"/> is less than 0, or <paramref name="initialCount"/> is greater than <paramref name="maxCount"/>, or <paramref name="maxCount"/> is less than or equal to 0.
  /// </exception>
  public SemaphoreSlim(int initialCount, int maxCount) {
    if (initialCount < 0)
      throw new ArgumentOutOfRangeException(nameof(initialCount), initialCount, "Non-negative number required.");
    if (maxCount <= 0)
      throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount, "Positive number required.");
    if (initialCount > maxCount)
      throw new ArgumentOutOfRangeException(nameof(initialCount), initialCount, "The initial count must be less than or equal to the maximum count.");

    this._semaphore = new(initialCount, maxCount);
    this._maxCount = maxCount;
    this._currentCount = initialCount;
  }

  /// <summary>
  /// Gets the number of remaining threads that can enter the <see cref="SemaphoreSlim"/> object.
  /// </summary>
  public int CurrentCount {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      // Use Interlocked.CompareExchange as a volatile read - it returns the current value
      return Interlocked.CompareExchange(ref this._currentCount, 0, 0);
    }
  }

  /// <summary>
  /// Returns a <see cref="WaitHandle"/> that can be used to wait on the semaphore.
  /// </summary>
  public WaitHandle AvailableWaitHandle {
    get {
      this._ThrowIfDisposed();
      if (this._availableWaitHandle != null)
        return this._availableWaitHandle;

      lock (this._lock) {
        this._availableWaitHandle ??= new(this._currentCount > 0);
        return this._availableWaitHandle;
      }
    }
  }

  /// <summary>
  /// Blocks the current thread until it can enter the <see cref="SemaphoreSlim"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Wait() => this.Wait(Timeout.Infinite, CancellationToken.None);

  /// <summary>
  /// Blocks the current thread until it can enter the <see cref="SemaphoreSlim"/>, while observing a <see cref="CancellationToken"/>.
  /// </summary>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
  /// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Wait(CancellationToken cancellationToken) => this.Wait(Timeout.Infinite, cancellationToken);

  /// <summary>
  /// Blocks the current thread until it can enter the <see cref="SemaphoreSlim"/>, using a <see cref="TimeSpan"/> to specify the timeout.
  /// </summary>
  /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
  /// <returns><see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Wait(TimeSpan timeout) => this.Wait((int)timeout.TotalMilliseconds, CancellationToken.None);

  /// <summary>
  /// Blocks the current thread until it can enter the <see cref="SemaphoreSlim"/>, using a <see cref="TimeSpan"/> to specify the timeout, while observing a <see cref="CancellationToken"/>.
  /// </summary>
  /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
  /// <returns><see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Wait(TimeSpan timeout, CancellationToken cancellationToken) => this.Wait((int)timeout.TotalMilliseconds, cancellationToken);

  /// <summary>
  /// Blocks the current thread until it can enter the <see cref="SemaphoreSlim"/>, using a 32-bit signed integer to specify the timeout.
  /// </summary>
  /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
  /// <returns><see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Wait(int millisecondsTimeout) => this.Wait(millisecondsTimeout, CancellationToken.None);

  /// <summary>
  /// Blocks the current thread until it can enter the <see cref="SemaphoreSlim"/>, using a 32-bit signed integer to specify the timeout, while observing a <see cref="CancellationToken"/>.
  /// </summary>
  /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
  /// <returns><see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
  public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken) {
    this._ThrowIfDisposed();

    if (millisecondsTimeout < -1)
      throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), millisecondsTimeout, "The timeout must be a non-negative number or -1.");

    cancellationToken.ThrowIfCancellationRequested();

    // Try to acquire without waiting first
    if (this._TryAcquire())
      return true;

    if (millisecondsTimeout == 0)
      return false;

    // Need to wait
    var startTime = Environment.TickCount;
    var remainingTimeout = millisecondsTimeout;

    while (true) {
      cancellationToken.ThrowIfCancellationRequested();

      bool acquired;
      if (cancellationToken.CanBeCanceled) {
        // Wait with cancellation support using WaitAny
        var waitHandles = new[] { this._semaphore, cancellationToken.WaitHandle };
        var result = WaitHandle.WaitAny(waitHandles, remainingTimeout);

        if (result == WaitHandle.WaitTimeout)
          return false;

        if (result == 1) {
          // Cancellation was signaled
          cancellationToken.ThrowIfCancellationRequested();
          return false;
        }

        acquired = result == 0;
      } else
        acquired = this._semaphore.WaitOne(remainingTimeout);

      if (acquired) {
        Interlocked.Decrement(ref this._currentCount);
        this._UpdateWaitHandle();
        return true;
      }

      if (millisecondsTimeout == Timeout.Infinite)
        continue;

      // Calculate remaining time
      var elapsed = Environment.TickCount - startTime;
      remainingTimeout = millisecondsTimeout - elapsed;
      if (remainingTimeout <= 0)
        return false;
    }
  }

  /// <summary>
  /// Releases the <see cref="SemaphoreSlim"/> object once.
  /// </summary>
  /// <returns>The previous count of the <see cref="SemaphoreSlim"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int Release() => this.Release(1);

  /// <summary>
  /// Releases the <see cref="SemaphoreSlim"/> object a specified number of times.
  /// </summary>
  /// <param name="releaseCount">The number of times to exit the semaphore.</param>
  /// <returns>The previous count of the <see cref="SemaphoreSlim"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="releaseCount"/> is less than 1.</exception>
  /// <exception cref="SemaphoreFullException">The <see cref="SemaphoreSlim"/> has already reached its maximum size.</exception>
  public int Release(int releaseCount) {
    this._ThrowIfDisposed();

    if (releaseCount < 1)
      throw new ArgumentOutOfRangeException(nameof(releaseCount), releaseCount, "The release count must be a positive number.");

    lock (this._lock) {
      var previousCount = this._currentCount;

      if (this._maxCount - previousCount < releaseCount)
        throw new SemaphoreFullException();

      this._semaphore.Release(releaseCount);
      this._currentCount = previousCount + releaseCount;
      this._UpdateWaitHandle();

      return previousCount;
    }
  }

  private bool _TryAcquire() {
    lock (this._lock) {
      if (this._currentCount <= 0)
        return false;

      if (!this._semaphore.WaitOne(0))
        return false;

      --this._currentCount;
      this._UpdateWaitHandle();
      return true;
    }
  }

  private void _UpdateWaitHandle() {
    if (this._availableWaitHandle == null)
      return;

    if (this._currentCount > 0)
      this._availableWaitHandle.Set();
    else
      this._availableWaitHandle.Reset();
  }

  private void _ThrowIfDisposed() {
    if (this._isDisposed)
      throw new ObjectDisposedException(nameof(SemaphoreSlim));
  }

  /// <summary>
  /// Releases all resources used by the current instance of the <see cref="SemaphoreSlim"/> class.
  /// </summary>
  ~SemaphoreSlim() => this.Dispose(false);

  /// <summary>
  /// Releases all resources used by the current instance of the <see cref="SemaphoreSlim"/> class.
  /// </summary>
  public void Dispose() {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Releases the unmanaged resources used by the <see cref="SemaphoreSlim"/>, and optionally releases the managed resources.
  /// </summary>
  /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
  protected virtual void Dispose(bool disposing) {
    if (this._isDisposed)
      return;

    this._isDisposed = true;

    if (!disposing)
      return;

    this._semaphore.Close();
    this._availableWaitHandle?.Close();
  }
}

}

#endif

#if !SUPPORTS_SEMAPHORESLIM_WAITASYNC

namespace System.Threading {

/// <summary>
/// Provides WaitAsync extension methods for <see cref="SemaphoreSlim"/>.
/// </summary>
public static class SemaphoreSlimPolyfills {

  /// <param name="this">The <see cref="SemaphoreSlim"/> instance.</param>
  extension(SemaphoreSlim @this) {

    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreSlim"/>.
    /// </summary>
    /// <returns>A task that will complete when the semaphore has been entered.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync() => @this.WaitAsync(Timeout.Infinite, CancellationToken.None);

    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreSlim"/>, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that will complete when the semaphore has been entered.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync(CancellationToken cancellationToken) => @this.WaitAsync(Timeout.Infinite, cancellationToken);

    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreSlim"/>, using a 32-bit signed integer to specify the timeout.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
    /// <returns>A task that will complete with a result of <see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>, otherwise with a result of <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> WaitAsync(int millisecondsTimeout) => @this.WaitAsync(millisecondsTimeout, CancellationToken.None);

    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreSlim"/>, using a <see cref="TimeSpan"/> to specify the timeout.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
    /// <returns>A task that will complete with a result of <see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>, otherwise with a result of <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> WaitAsync(TimeSpan timeout) => @this.WaitAsync((int)timeout.TotalMilliseconds, CancellationToken.None);

    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreSlim"/>, using a <see cref="TimeSpan"/> to specify the timeout, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that will complete with a result of <see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>, otherwise with a result of <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) => @this.WaitAsync((int)timeout.TotalMilliseconds, cancellationToken);

    /// <summary>
    /// Asynchronously waits to enter the <see cref="SemaphoreSlim"/>, using a 32-bit signed integer to specify the timeout, while observing a <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that will complete with a result of <see langword="true"/> if the current thread successfully entered the <see cref="SemaphoreSlim"/>, otherwise with a result of <see langword="false"/>.</returns>
    public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken) {
      if (millisecondsTimeout < -1)
        throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), millisecondsTimeout, "The timeout must be a non-negative number or -1.");

      // Try to acquire synchronously first
      if (@this.Wait(0))
        return Task.FromResult(true);

      if (millisecondsTimeout == 0)
        return Task.FromResult(false);

      // Fall back to running Wait on a thread pool thread
      return Task.Run(() => @this.Wait(millisecondsTimeout, cancellationToken), cancellationToken);
    }
  }
}

}

#endif
