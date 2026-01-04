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

#if !SUPPORTS_READERWRITERLOCKSLIM

namespace System.Threading;

/// <summary>
/// Specifies the lock recursion policy for a <see cref="ReaderWriterLockSlim"/>.
/// </summary>
public enum LockRecursionPolicy {
  /// <summary>
  /// If a thread tries to enter a lock recursively, an exception is thrown.
  /// </summary>
  NoRecursion = 0,
  /// <summary>
  /// A thread can enter a lock recursively.
  /// </summary>
  SupportsRecursion = 1
}

/// <summary>
/// Represents a lock that is used to manage access to a resource, allowing multiple threads for reading
/// or exclusive access for writing.
/// </summary>
/// <remarks>
/// This is a polyfill for .NET 2.0 that wraps <see cref="ReaderWriterLock"/>.
/// </remarks>
public class ReaderWriterLockSlim : IDisposable {
  private readonly ReaderWriterLock _lock = new();
  private readonly LockRecursionPolicy _recursionPolicy;
  private readonly int _defaultTimeout = Timeout.Infinite;

  [ThreadStatic]
  private static int _currentThreadReadCount;
  [ThreadStatic]
  private static int _currentThreadWriteCount;
  [ThreadStatic]
  private static int _currentThreadUpgradeCount;

  /// <summary>
  /// Initializes a new instance with default (NoRecursion) policy.
  /// </summary>
  public ReaderWriterLockSlim() : this(LockRecursionPolicy.NoRecursion) { }

  /// <summary>
  /// Initializes a new instance with the specified recursion policy.
  /// </summary>
  /// <param name="recursionPolicy">The lock recursion policy.</param>
  public ReaderWriterLockSlim(LockRecursionPolicy recursionPolicy)
    => this._recursionPolicy = recursionPolicy;

  /// <summary>
  /// Gets the recursion policy for this lock.
  /// </summary>
  public LockRecursionPolicy RecursionPolicy => this._recursionPolicy;

  /// <summary>
  /// Gets a value indicating whether the current thread has entered the lock in read mode.
  /// </summary>
  public bool IsReadLockHeld => this._lock.IsReaderLockHeld;

  /// <summary>
  /// Gets a value indicating whether the current thread has entered the lock in write mode.
  /// </summary>
  public bool IsWriteLockHeld => this._lock.IsWriterLockHeld;

  /// <summary>
  /// Gets a value indicating whether the current thread has entered the lock in upgradeable mode.
  /// </summary>
  public bool IsUpgradeableReadLockHeld => _currentThreadUpgradeCount > 0;

  /// <summary>
  /// Gets the total number of threads that have entered the lock in read mode.
  /// </summary>
  public int CurrentReadCount => this._lock.IsReaderLockHeld ? 1 : 0;

  /// <summary>
  /// Gets the number of times the current thread has entered the lock in read mode.
  /// </summary>
  public int RecursiveReadCount => _currentThreadReadCount;

  /// <summary>
  /// Gets the number of times the current thread has entered the lock in write mode.
  /// </summary>
  public int RecursiveWriteCount => _currentThreadWriteCount;

  /// <summary>
  /// Gets the number of times the current thread has entered the lock in upgradeable mode.
  /// </summary>
  public int RecursiveUpgradeCount => _currentThreadUpgradeCount;

  /// <summary>
  /// Tries to enter the lock in read mode.
  /// </summary>
  public void EnterReadLock() {
    if (this._recursionPolicy == LockRecursionPolicy.NoRecursion && _currentThreadReadCount > 0)
      throw new LockRecursionException("Recursive read lock not allowed with NoRecursion policy.");

    this._lock.AcquireReaderLock(this._defaultTimeout);
    ++_currentThreadReadCount;
  }

  /// <summary>
  /// Tries to enter the lock in read mode, with an optional timeout.
  /// </summary>
  /// <param name="timeout">The timeout.</param>
  /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
  public bool TryEnterReadLock(TimeSpan timeout) => this.TryEnterReadLock((int)timeout.TotalMilliseconds);

  /// <summary>
  /// Tries to enter the lock in read mode, with an optional timeout.
  /// </summary>
  /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
  /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
  public bool TryEnterReadLock(int millisecondsTimeout) {
    if (this._recursionPolicy == LockRecursionPolicy.NoRecursion && _currentThreadReadCount > 0)
      throw new LockRecursionException("Recursive read lock not allowed with NoRecursion policy.");

    try {
      this._lock.AcquireReaderLock(millisecondsTimeout);
      ++_currentThreadReadCount;
      return true;
    } catch (ApplicationException) {
      return false;
    }
  }

  /// <summary>
  /// Reduces the recursion count for read mode, and exits read mode if the resulting count is 0.
  /// </summary>
  public void ExitReadLock() {
    this._lock.ReleaseReaderLock();
    --_currentThreadReadCount;
  }

  /// <summary>
  /// Tries to enter the lock in write mode.
  /// </summary>
  public void EnterWriteLock() {
    if (this._recursionPolicy == LockRecursionPolicy.NoRecursion && _currentThreadWriteCount > 0)
      throw new LockRecursionException("Recursive write lock not allowed with NoRecursion policy.");

    this._lock.AcquireWriterLock(this._defaultTimeout);
    ++_currentThreadWriteCount;
  }

  /// <summary>
  /// Tries to enter the lock in write mode, with an optional timeout.
  /// </summary>
  /// <param name="timeout">The timeout.</param>
  /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
  public bool TryEnterWriteLock(TimeSpan timeout) => this.TryEnterWriteLock((int)timeout.TotalMilliseconds);

  /// <summary>
  /// Tries to enter the lock in write mode, with an optional timeout.
  /// </summary>
  /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
  /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
  public bool TryEnterWriteLock(int millisecondsTimeout) {
    if (this._recursionPolicy == LockRecursionPolicy.NoRecursion && _currentThreadWriteCount > 0)
      throw new LockRecursionException("Recursive write lock not allowed with NoRecursion policy.");

    try {
      this._lock.AcquireWriterLock(millisecondsTimeout);
      ++_currentThreadWriteCount;
      return true;
    } catch (ApplicationException) {
      return false;
    }
  }

  /// <summary>
  /// Reduces the recursion count for write mode, and exits write mode if the resulting count is 0.
  /// </summary>
  public void ExitWriteLock() {
    this._lock.ReleaseWriterLock();
    --_currentThreadWriteCount;
  }

  /// <summary>
  /// Tries to enter the lock in upgradeable mode.
  /// </summary>
  public void EnterUpgradeableReadLock() {
    if (this._recursionPolicy == LockRecursionPolicy.NoRecursion && _currentThreadUpgradeCount > 0)
      throw new LockRecursionException("Recursive upgradeable lock not allowed with NoRecursion policy.");

    this._lock.AcquireReaderLock(this._defaultTimeout);
    ++_currentThreadUpgradeCount;
  }

  /// <summary>
  /// Tries to enter the lock in upgradeable mode, with an optional timeout.
  /// </summary>
  /// <param name="timeout">The timeout.</param>
  /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
  public bool TryEnterUpgradeableReadLock(TimeSpan timeout)
    => this.TryEnterUpgradeableReadLock((int)timeout.TotalMilliseconds);

  /// <summary>
  /// Tries to enter the lock in upgradeable mode, with an optional timeout.
  /// </summary>
  /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
  /// <returns><see langword="true"/> if the lock was acquired; otherwise, <see langword="false"/>.</returns>
  public bool TryEnterUpgradeableReadLock(int millisecondsTimeout) {
    if (this._recursionPolicy == LockRecursionPolicy.NoRecursion && _currentThreadUpgradeCount > 0)
      throw new LockRecursionException("Recursive upgradeable lock not allowed with NoRecursion policy.");

    try {
      this._lock.AcquireReaderLock(millisecondsTimeout);
      ++_currentThreadUpgradeCount;
      return true;
    } catch (ApplicationException) {
      return false;
    }
  }

  /// <summary>
  /// Reduces the recursion count for upgradeable mode, and exits upgradeable mode if the resulting count is 0.
  /// </summary>
  public void ExitUpgradeableReadLock() {
    this._lock.ReleaseReaderLock();
    --_currentThreadUpgradeCount;
  }

  /// <summary>
  /// Releases all resources used by the current instance.
  /// </summary>
  public void Dispose() {
    GC.SuppressFinalize(this);
  }
}

#endif
