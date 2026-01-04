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

#if !SUPPORTS_LOCK

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

/// <summary>
/// Provides a way to get mutual exclusion in regions of code between different threads.
/// A lock may be held by one thread at a time.
/// </summary>
/// <remarks>
/// Threads that cannot immediately enter the lock may wait for the lock to be exited or until a specified timeout expires.
/// A thread that holds a lock may enter it repeatedly without exiting it, but the lock should be exited the same number of times as it was entered.
/// </remarks>
/// <example>
/// <code>
/// private readonly Lock _lock = new();
///
/// public void DoSomething() {
///   lock (_lock) {
///     // Critical section
///   }
/// }
/// </code>
/// </example>
public sealed class Lock {

  private readonly object _syncRoot = new();
  private volatile int _ownerThreadId;
  private volatile int _recursionCount;

  /// <summary>
  /// Initializes a new instance of the <see cref="Lock"/> class.
  /// </summary>
  public Lock() { }

  /// <summary>
  /// Gets a value that indicates whether the lock is held by the current thread.
  /// </summary>
  /// <value><see langword="true"/> if the lock is held by the current thread; otherwise, <see langword="false"/>.</value>
  public bool IsHeldByCurrentThread {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._ownerThreadId == Environment.CurrentManagedThreadId;
  }

  /// <summary>
  /// Enters the lock, waiting if necessary until the lock can be entered.
  /// </summary>
  /// <remarks>
  /// If the lock cannot be entered immediately, the calling thread waits for the lock to be exited.
  /// If the lock is already held by the calling thread, the lock is entered again and the recursion count is incremented.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Enter() {
    var currentThreadId = Environment.CurrentManagedThreadId;

    if (this._ownerThreadId == currentThreadId) {
      Interlocked.Increment(ref this._recursionCount);
      return;
    }

    Monitor.Enter(this._syncRoot);
    this._ownerThreadId = currentThreadId;
    this._recursionCount = 1;
  }

  /// <summary>
  /// Enters the lock, waiting if necessary until the lock can be entered.
  /// </summary>
  /// <returns>A <see cref="Scope"/> that can be disposed to exit the lock.</returns>
  /// <remarks>
  /// It is recommended to use this method with a language construct that automatically disposes the returned <see cref="Scope"/>
  /// such as the C# <c>using</c> keyword, or to use the C# <c>lock</c> keyword.
  /// </remarks>
  /// <example>
  /// <code>
  /// using (myLock.EnterScope()) {
  ///   // Critical section
  /// }
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Scope EnterScope() {
    this.Enter();
    return new(this);
  }

  /// <summary>
  /// Exits the lock.
  /// </summary>
  /// <exception cref="SynchronizationLockException">The lock is not held by the current thread.</exception>
  /// <remarks>
  /// If the current thread holds the lock multiple times, such as recursively, the lock is exited only once.
  /// The calling thread should exit the lock as many times as it entered the lock.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Exit() {
    if (this._ownerThreadId != Environment.CurrentManagedThreadId)
      throw new SynchronizationLockException("The lock is not held by the current thread.");

    if (Interlocked.Decrement(ref this._recursionCount) != 0)
      return;

    this._ownerThreadId = 0;
    Monitor.Exit(this._syncRoot);
  }

  /// <summary>
  /// Tries to enter the lock without waiting.
  /// </summary>
  /// <returns><see langword="true"/> if the lock was entered; otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// If the lock cannot be entered immediately, the method returns <see langword="false"/> without waiting.
  /// If the lock is already held by the calling thread, the lock is entered again and the recursion count is incremented.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryEnter() {
    var currentThreadId = Environment.CurrentManagedThreadId;

    if (this._ownerThreadId == currentThreadId) {
      Interlocked.Increment(ref this._recursionCount);
      return true;
    }

    if (!Monitor.TryEnter(this._syncRoot))
      return false;

    this._ownerThreadId = currentThreadId;
    this._recursionCount = 1;
    return true;
  }

  /// <summary>
  /// Tries to enter the lock, waiting if necessary for the specified number of milliseconds until the lock can be entered.
  /// </summary>
  /// <param name="millisecondsTimeout">
  /// The number of milliseconds to wait until the lock can be entered.
  /// Specify <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or 0 to not wait.
  /// </param>
  /// <returns><see langword="true"/> if the lock was entered; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="millisecondsTimeout"/> is a negative number other than -1.
  /// </exception>
  /// <remarks>
  /// If the lock cannot be entered immediately, the calling thread waits for the lock to be exited or until the timeout expires.
  /// If the lock is already held by the calling thread, the lock is entered again and the recursion count is incremented.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryEnter(int millisecondsTimeout) {
    if (millisecondsTimeout < -1)
      throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), "The timeout must be greater than or equal to -1.");

    var currentThreadId = Environment.CurrentManagedThreadId;

    if (this._ownerThreadId == currentThreadId) {
      Interlocked.Increment(ref this._recursionCount);
      return true;
    }

    if (!Monitor.TryEnter(this._syncRoot, millisecondsTimeout))
      return false;

    this._ownerThreadId = currentThreadId;
    this._recursionCount = 1;
    return true;
  }

  /// <summary>
  /// Tries to enter the lock, waiting if necessary until the lock can be entered or until the specified timeout expires.
  /// </summary>
  /// <param name="timeout">
  /// The interval to wait until the lock can be entered.
  /// Specify a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to not wait.
  /// </param>
  /// <returns><see langword="true"/> if the lock was entered; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="timeout"/> is a negative number of milliseconds other than -1.
  /// -or-
  /// <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/> milliseconds.
  /// </exception>
  /// <remarks>
  /// If the lock cannot be entered immediately, the calling thread waits for the lock to be exited or until the timeout expires.
  /// If the lock is already held by the calling thread, the lock is entered again and the recursion count is incremented.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryEnter(TimeSpan timeout) {
    var totalMilliseconds = timeout.TotalMilliseconds;
    if (totalMilliseconds is < -1 or > int.MaxValue)
      throw new ArgumentOutOfRangeException(nameof(timeout), "The timeout must be greater than or equal to -1 and less than or equal to Int32.MaxValue milliseconds.");

    return this.TryEnter((int)totalMilliseconds);
  }

  /// <summary>
  /// A disposable ref struct that exits the lock when disposed.
  /// </summary>
  /// <remarks>
  /// This type is used with the <see cref="EnterScope"/> method for use with the C# <c>using</c> keyword.
  /// </remarks>
  public ref struct Scope {
    private Lock? _lock;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Scope(Lock @lock) => this._lock = @lock;

    /// <summary>
    /// Exits the lock if the <see cref="Scope"/> represents a lock that was entered.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
      var @lock = this._lock;
      if (@lock == null)
        return;

      this._lock = null;
      @lock.Exit();
    }
  }

}

#endif
