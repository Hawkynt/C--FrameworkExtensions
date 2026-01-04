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

using System;
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ReaderWriterLockSlim")]
public class ReaderWriterLockSlimTests {

  #region Constructor

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_Constructor_DefaultPolicy_IsNoRecursion() {
    using var rwls = new ReaderWriterLockSlim();
    Assert.That(rwls.RecursionPolicy, Is.EqualTo(LockRecursionPolicy.NoRecursion));
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_Constructor_WithSupportsRecursion_SetsPolicy() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    Assert.That(rwls.RecursionPolicy, Is.EqualTo(LockRecursionPolicy.SupportsRecursion));
  }

  #endregion

  #region Read Lock

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_EnterReadLock_SetsIsReadLockHeld() {
    using var rwls = new ReaderWriterLockSlim();
    rwls.EnterReadLock();
    try {
      Assert.That(rwls.IsReadLockHeld, Is.True);
    } finally {
      rwls.ExitReadLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_ExitReadLock_ClearsIsReadLockHeld() {
    using var rwls = new ReaderWriterLockSlim();
    rwls.EnterReadLock();
    rwls.ExitReadLock();
    Assert.That(rwls.IsReadLockHeld, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_TryEnterReadLock_ReturnsTrue_WhenAvailable() {
    using var rwls = new ReaderWriterLockSlim();
    var result = rwls.TryEnterReadLock(1000);
    try {
      Assert.That(result, Is.True);
      Assert.That(rwls.IsReadLockHeld, Is.True);
    } finally {
      if (result)
        rwls.ExitReadLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_TryEnterReadLock_WithTimeSpan_ReturnsTrue() {
    using var rwls = new ReaderWriterLockSlim();
    var result = rwls.TryEnterReadLock(TimeSpan.FromSeconds(1));
    try {
      Assert.That(result, Is.True);
    } finally {
      if (result)
        rwls.ExitReadLock();
    }
  }

  [Test]
  [Category("Exception")]
  public void ReaderWriterLockSlim_EnterReadLock_RecursiveWithNoRecursion_Throws() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    rwls.EnterReadLock();
    try {
      Assert.Throws<LockRecursionException>(() => rwls.EnterReadLock());
    } finally {
      rwls.ExitReadLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_EnterReadLock_RecursiveWithSupportsRecursion_Succeeds() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    rwls.EnterReadLock();
    try {
      Assert.DoesNotThrow(() => rwls.EnterReadLock());
      rwls.ExitReadLock();
    } finally {
      rwls.ExitReadLock();
    }
  }

  #endregion

  #region Write Lock

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_EnterWriteLock_SetsIsWriteLockHeld() {
    using var rwls = new ReaderWriterLockSlim();
    rwls.EnterWriteLock();
    try {
      Assert.That(rwls.IsWriteLockHeld, Is.True);
    } finally {
      rwls.ExitWriteLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_ExitWriteLock_ClearsIsWriteLockHeld() {
    using var rwls = new ReaderWriterLockSlim();
    rwls.EnterWriteLock();
    rwls.ExitWriteLock();
    Assert.That(rwls.IsWriteLockHeld, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_TryEnterWriteLock_ReturnsTrue_WhenAvailable() {
    using var rwls = new ReaderWriterLockSlim();
    var result = rwls.TryEnterWriteLock(1000);
    try {
      Assert.That(result, Is.True);
      Assert.That(rwls.IsWriteLockHeld, Is.True);
    } finally {
      if (result)
        rwls.ExitWriteLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_TryEnterWriteLock_WithTimeSpan_ReturnsTrue() {
    using var rwls = new ReaderWriterLockSlim();
    var result = rwls.TryEnterWriteLock(TimeSpan.FromSeconds(1));
    try {
      Assert.That(result, Is.True);
    } finally {
      if (result)
        rwls.ExitWriteLock();
    }
  }

  [Test]
  [Category("Exception")]
  public void ReaderWriterLockSlim_EnterWriteLock_RecursiveWithNoRecursion_Throws() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    rwls.EnterWriteLock();
    try {
      Assert.Throws<LockRecursionException>(() => rwls.EnterWriteLock());
    } finally {
      rwls.ExitWriteLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_EnterWriteLock_RecursiveWithSupportsRecursion_Succeeds() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    rwls.EnterWriteLock();
    try {
      Assert.DoesNotThrow(() => rwls.EnterWriteLock());
      rwls.ExitWriteLock();
    } finally {
      rwls.ExitWriteLock();
    }
  }

  #endregion

  #region Upgradeable Read Lock

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_EnterUpgradeableReadLock_SetsIsUpgradeableReadLockHeld() {
    using var rwls = new ReaderWriterLockSlim();
    rwls.EnterUpgradeableReadLock();
    try {
      Assert.That(rwls.IsUpgradeableReadLockHeld, Is.True);
    } finally {
      rwls.ExitUpgradeableReadLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_ExitUpgradeableReadLock_ClearsIsUpgradeableReadLockHeld() {
    using var rwls = new ReaderWriterLockSlim();
    rwls.EnterUpgradeableReadLock();
    rwls.ExitUpgradeableReadLock();
    Assert.That(rwls.IsUpgradeableReadLockHeld, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_TryEnterUpgradeableReadLock_ReturnsTrue_WhenAvailable() {
    using var rwls = new ReaderWriterLockSlim();
    var result = rwls.TryEnterUpgradeableReadLock(1000);
    try {
      Assert.That(result, Is.True);
      Assert.That(rwls.IsUpgradeableReadLockHeld, Is.True);
    } finally {
      if (result)
        rwls.ExitUpgradeableReadLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_TryEnterUpgradeableReadLock_WithTimeSpan_ReturnsTrue() {
    using var rwls = new ReaderWriterLockSlim();
    var result = rwls.TryEnterUpgradeableReadLock(TimeSpan.FromSeconds(1));
    try {
      Assert.That(result, Is.True);
    } finally {
      if (result)
        rwls.ExitUpgradeableReadLock();
    }
  }

  [Test]
  [Category("Exception")]
  public void ReaderWriterLockSlim_EnterUpgradeableReadLock_RecursiveWithNoRecursion_Throws() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    rwls.EnterUpgradeableReadLock();
    try {
      Assert.Throws<LockRecursionException>(() => rwls.EnterUpgradeableReadLock());
    } finally {
      rwls.ExitUpgradeableReadLock();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_EnterUpgradeableReadLock_RecursiveWithSupportsRecursion_Succeeds() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    rwls.EnterUpgradeableReadLock();
    try {
      Assert.DoesNotThrow(() => rwls.EnterUpgradeableReadLock());
      rwls.ExitUpgradeableReadLock();
    } finally {
      rwls.ExitUpgradeableReadLock();
    }
  }

  #endregion

  #region Recursive Counts

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_RecursiveReadCount_TracksReadLockAcquisitions() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    Assert.That(rwls.RecursiveReadCount, Is.EqualTo(0));
    rwls.EnterReadLock();
    Assert.That(rwls.RecursiveReadCount, Is.EqualTo(1));
    rwls.EnterReadLock();
    Assert.That(rwls.RecursiveReadCount, Is.EqualTo(2));
    rwls.ExitReadLock();
    Assert.That(rwls.RecursiveReadCount, Is.EqualTo(1));
    rwls.ExitReadLock();
    Assert.That(rwls.RecursiveReadCount, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_RecursiveWriteCount_TracksWriteLockAcquisitions() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    Assert.That(rwls.RecursiveWriteCount, Is.EqualTo(0));
    rwls.EnterWriteLock();
    Assert.That(rwls.RecursiveWriteCount, Is.EqualTo(1));
    rwls.EnterWriteLock();
    Assert.That(rwls.RecursiveWriteCount, Is.EqualTo(2));
    rwls.ExitWriteLock();
    Assert.That(rwls.RecursiveWriteCount, Is.EqualTo(1));
    rwls.ExitWriteLock();
    Assert.That(rwls.RecursiveWriteCount, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_RecursiveUpgradeCount_TracksUpgradeableLockAcquisitions() {
    using var rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    Assert.That(rwls.RecursiveUpgradeCount, Is.EqualTo(0));
    rwls.EnterUpgradeableReadLock();
    Assert.That(rwls.RecursiveUpgradeCount, Is.EqualTo(1));
    rwls.EnterUpgradeableReadLock();
    Assert.That(rwls.RecursiveUpgradeCount, Is.EqualTo(2));
    rwls.ExitUpgradeableReadLock();
    Assert.That(rwls.RecursiveUpgradeCount, Is.EqualTo(1));
    rwls.ExitUpgradeableReadLock();
    Assert.That(rwls.RecursiveUpgradeCount, Is.EqualTo(0));
  }

  #endregion

  #region Dispose

  [Test]
  [Category("HappyPath")]
  public void ReaderWriterLockSlim_Dispose_CanBeCalledMultipleTimes() {
    var rwls = new ReaderWriterLockSlim();
    rwls.Dispose();
    rwls.Dispose();
    Assert.Pass();
  }

  #endregion

  #region Thread Safety

  [Test]
  [Category("Integration")]
  public void ReaderWriterLockSlim_MultipleReaders_CanAcquireConcurrently() {
    using var rwls = new ReaderWriterLockSlim();
    var readersActive = 0;
    var maxConcurrentReaders = 0;
    var threads = new Thread[5];

    for (var i = 0; i < 5; ++i) {
      threads[i] = new Thread(() => {
        rwls.EnterReadLock();
        try {
          var count = Interlocked.Increment(ref readersActive);
          lock (rwls) {
            if (count > maxConcurrentReaders)
              maxConcurrentReaders = count;
          }
          Thread.Sleep(50);
        } finally {
          Interlocked.Decrement(ref readersActive);
          rwls.ExitReadLock();
        }
      });
    }

    foreach (var thread in threads)
      thread.Start();
    foreach (var thread in threads)
      thread.Join();

    Assert.That(maxConcurrentReaders, Is.GreaterThan(1));
  }

  [Test]
  [Category("Integration")]
  public void ReaderWriterLockSlim_WriterExcludesReaders() {
    using var rwls = new ReaderWriterLockSlim();
    var writerHeld = false;
    var readerEnteredDuringWrite = false;

    var writerThread = new Thread(() => {
      rwls.EnterWriteLock();
      try {
        writerHeld = true;
        Thread.Sleep(100);
      } finally {
        writerHeld = false;
        rwls.ExitWriteLock();
      }
    });

    var readerThread = new Thread(() => {
      Thread.Sleep(20);
      rwls.EnterReadLock();
      try {
        if (writerHeld)
          readerEnteredDuringWrite = true;
      } finally {
        rwls.ExitReadLock();
      }
    });

    writerThread.Start();
    readerThread.Start();
    writerThread.Join();
    readerThread.Join();

    Assert.That(readerEnteredDuringWrite, Is.False);
  }

  #endregion

}
