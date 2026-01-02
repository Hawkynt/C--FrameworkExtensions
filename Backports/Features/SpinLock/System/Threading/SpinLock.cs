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

#if !SUPPORTS_SLIM_SEMAPHORES

using System.Diagnostics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

/// <summary>
/// Provides a mutual exclusion lock primitive where a thread trying to acquire the lock waits in a loop
/// repeatedly checking until the lock becomes available.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SpinLock"/> should only be used when it's been determined that doing so will improve
/// an application's performance. It's also important to note that <see cref="SpinLock"/> is a value type,
/// for performance reasons. As such, one must be careful not to accidentally copy a SpinLock instance,
/// as the two instances (the original and the copy) would then be completely independent of one another.
/// </para>
/// <para>
/// Do not store SpinLock instances in readonly fields.
/// </para>
/// </remarks>
public struct SpinLock {

  /// <summary>
  /// The lock state. 0 = unlocked, 1 = locked without owner tracking, thread ID = locked with owner tracking.
  /// </summary>
  private volatile int _owner;

  /// <summary>
  /// Whether thread owner tracking is enabled.
  /// </summary>
  private readonly bool _enableThreadOwnerTracking;

  /// <summary>
  /// Special value indicating no owner (unlocked).
  /// </summary>
  private const int _NO_OWNER = 0;

  /// <summary>
  /// Special value indicating locked but not tracking owner.
  /// </summary>
  private const int _LOCKED_NO_TRACKING = 1;

  /// <summary>
  /// Initializes a new instance of the <see cref="SpinLock"/> struct with the option to track thread IDs
  /// to improve debugging.
  /// </summary>
  /// <param name="enableThreadOwnerTracking">
  /// Whether to capture and use thread IDs for debugging purposes.
  /// </param>
  public SpinLock(bool enableThreadOwnerTracking) : this() => this._enableThreadOwnerTracking = enableThreadOwnerTracking;

  /// <summary>
  /// Gets whether the lock is currently held by any thread.
  /// </summary>
  /// <value>
  /// <c>true</c> if the lock is currently held by any thread; otherwise, <c>false</c>.
  /// </value>
  public bool IsHeld {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._owner != _NO_OWNER;
  }

  /// <summary>
  /// Gets whether the lock is held by the current thread.
  /// </summary>
  /// <value>
  /// <c>true</c> if the lock is held by the current thread; otherwise, <c>false</c>.
  /// </value>
  /// <exception cref="InvalidOperationException">Thread ownership tracking is disabled.</exception>
  public bool IsHeldByCurrentThread {
    get {
      if (!this._enableThreadOwnerTracking)
        throw new InvalidOperationException("Thread ownership tracking is disabled.");

      return this._owner == Thread.CurrentThread.ManagedThreadId;
    }
  }

  /// <summary>
  /// Gets whether thread owner tracking is enabled for this instance.
  /// </summary>
  /// <value>
  /// <c>true</c> if thread owner tracking is enabled; otherwise, <c>false</c>.
  /// </value>
  public bool IsThreadOwnerTrackingEnabled {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._enableThreadOwnerTracking;
  }

  /// <summary>
  /// Acquires the lock in a reliable manner, such that even if an exception occurs within the method call,
  /// <paramref name="lockTaken"/> can be examined reliably to determine whether the lock was acquired.
  /// </summary>
  /// <param name="lockTaken">
  /// True if the lock is acquired; otherwise, false. <paramref name="lockTaken"/> must be initialized to false
  /// prior to calling this method.
  /// </param>
  /// <exception cref="ArgumentException"><paramref name="lockTaken"/> must be initialized to false prior to calling Enter.</exception>
  /// <exception cref="LockRecursionException">Thread ownership tracking is enabled, and the current thread has already acquired this lock.</exception>
  public void Enter(ref bool lockTaken) {
    if (lockTaken)
      throw new ArgumentException("The lockTaken argument must be initialized to false prior to calling Enter.", nameof(lockTaken));

    this.ContinueTryEnter(Timeout.Infinite, ref lockTaken);
  }

  /// <summary>
  /// Attempts to acquire the lock in a reliable manner, such that even if an exception occurs within the method call,
  /// <paramref name="lockTaken"/> can be examined reliably to determine whether the lock was acquired.
  /// </summary>
  /// <param name="lockTaken">
  /// True if the lock is acquired; otherwise, false. <paramref name="lockTaken"/> must be initialized to false
  /// prior to calling this method.
  /// </param>
  /// <exception cref="ArgumentException"><paramref name="lockTaken"/> must be initialized to false prior to calling TryEnter.</exception>
  /// <exception cref="LockRecursionException">Thread ownership tracking is enabled, and the current thread has already acquired this lock.</exception>
  public void TryEnter(ref bool lockTaken) {
    if (lockTaken)
      throw new ArgumentException("The lockTaken argument must be initialized to false prior to calling TryEnter.", nameof(lockTaken));

    this.ContinueTryEnter(0, ref lockTaken);
  }

  /// <summary>
  /// Attempts to acquire the lock in a reliable manner, such that even if an exception occurs within the method call,
  /// <paramref name="lockTaken"/> can be examined reliably to determine whether the lock was acquired.
  /// </summary>
  /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/>
  /// that represents -1 milliseconds to wait indefinitely.</param>
  /// <param name="lockTaken">
  /// True if the lock is acquired; otherwise, false. <paramref name="lockTaken"/> must be initialized to false
  /// prior to calling this method.
  /// </param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds.</exception>
  /// <exception cref="ArgumentException"><paramref name="lockTaken"/> must be initialized to false prior to calling TryEnter.</exception>
  /// <exception cref="LockRecursionException">Thread ownership tracking is enabled, and the current thread has already acquired this lock.</exception>
  public void TryEnter(TimeSpan timeout, ref bool lockTaken) {
    var totalMilliseconds = (long)timeout.TotalMilliseconds;
    if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
      throw new ArgumentOutOfRangeException(nameof(timeout));

    this.TryEnter((int)totalMilliseconds, ref lockTaken);
  }

  /// <summary>
  /// Attempts to acquire the lock in a reliable manner, such that even if an exception occurs within the method call,
  /// <paramref name="lockTaken"/> can be examined reliably to determine whether the lock was acquired.
  /// </summary>
  /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
  /// <param name="lockTaken">
  /// True if the lock is acquired; otherwise, false. <paramref name="lockTaken"/> must be initialized to false
  /// prior to calling this method.
  /// </param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
  /// <exception cref="ArgumentException"><paramref name="lockTaken"/> must be initialized to false prior to calling TryEnter.</exception>
  /// <exception cref="LockRecursionException">Thread ownership tracking is enabled, and the current thread has already acquired this lock.</exception>
  public void TryEnter(int millisecondsTimeout, ref bool lockTaken) {
    if (lockTaken)
      throw new ArgumentException("The lockTaken argument must be initialized to false prior to calling TryEnter.", nameof(lockTaken));
    if (millisecondsTimeout < -1)
      throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

    this.ContinueTryEnter(millisecondsTimeout, ref lockTaken);
  }

  /// <summary>
  /// Core lock acquisition logic.
  /// </summary>
  private void ContinueTryEnter(int millisecondsTimeout, ref bool lockTaken) {
    var currentThreadId = Thread.CurrentThread.ManagedThreadId;
    var lockValue = this._enableThreadOwnerTracking ? currentThreadId : _LOCKED_NO_TRACKING;

    // Check for recursive lock attempt
    if (this._enableThreadOwnerTracking && this._owner == currentThreadId)
      throw new LockRecursionException("Recursive lock attempt detected. SpinLock does not support recursive locking.");

    // Fast path: try to acquire immediately
    if (Interlocked.CompareExchange(ref this._owner, lockValue, _NO_OWNER) == _NO_OWNER) {
      lockTaken = true;
      return;
    }

    // If timeout is 0, we're done
    if (millisecondsTimeout == 0)
      return;

    // Slow path: spin and wait
    var spinner = new SpinWait();
    var startTicks = millisecondsTimeout == Timeout.Infinite ? 0 : Environment.TickCount;

    do {
      spinner.SpinOnce();

      // Check for recursive lock attempt (owner could have changed)
      if (this._enableThreadOwnerTracking && this._owner == currentThreadId)
        throw new LockRecursionException("Recursive lock attempt detected. SpinLock does not support recursive locking.");

      if (Interlocked.CompareExchange(ref this._owner, lockValue, _NO_OWNER) == _NO_OWNER) {
        lockTaken = true;
        return;
      }

      // Check timeout
      if (millisecondsTimeout != Timeout.Infinite) {
        var elapsed = Environment.TickCount - startTicks;
        if (elapsed >= millisecondsTimeout)
          return;
      }
    } while (true);
  }

  /// <summary>
  /// Releases the lock.
  /// </summary>
  /// <exception cref="SynchronizationLockException">Thread ownership tracking is enabled, and the current thread is not the owner of this lock.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Exit() => this.Exit(true);

  /// <summary>
  /// Releases the lock.
  /// </summary>
  /// <param name="useMemoryBarrier">
  /// A Boolean value that indicates whether a memory fence should be issued in order to immediately
  /// publish the exit operation to other threads.
  /// </param>
  /// <exception cref="SynchronizationLockException">Thread ownership tracking is enabled, and the current thread is not the owner of this lock.</exception>
  public void Exit(bool useMemoryBarrier) {
    // Validate ownership if tracking is enabled
    if (this._enableThreadOwnerTracking) {
      var currentThreadId = Thread.CurrentThread.ManagedThreadId;
      if (this._owner != currentThreadId)
        throw new SynchronizationLockException("The calling thread does not own the lock.");
    }

    if (useMemoryBarrier) {
      // Full memory barrier before releasing
      Thread.MemoryBarrier();
      this._owner = _NO_OWNER;
    } else
      this._owner = _NO_OWNER;
  }

}

#endif
