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
[Category("SpinLock")]
public class SpinLockTests {

  #region Constructor

  [Test]
  [Category("HappyPath")]
  public void SpinLock_DefaultConstructor_CreatesUnlockedLock() {
    var spinLock = new SpinLock();
    Assert.That(spinLock.IsHeld, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_WithTracking_EnablesThreadOwnerTracking() {
    var spinLock = new SpinLock(true);
    Assert.That(spinLock.IsThreadOwnerTrackingEnabled, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_WithoutTracking_DisablesThreadOwnerTracking() {
    var spinLock = new SpinLock(false);
    Assert.That(spinLock.IsThreadOwnerTrackingEnabled, Is.False);
  }

  #endregion

  #region IsHeld Property

  [Test]
  [Category("HappyPath")]
  public void SpinLock_IsHeld_FalseWhenNotLocked() {
    var spinLock = new SpinLock();
    Assert.That(spinLock.IsHeld, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_IsHeld_TrueWhenLocked() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    try {
      Assert.That(spinLock.IsHeld, Is.True);
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_IsHeld_FalseAfterExit() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    spinLock.Exit();
    Assert.That(spinLock.IsHeld, Is.False);
  }

  #endregion

  #region IsHeldByCurrentThread Property

  [Test]
  [Category("HappyPath")]
  public void SpinLock_IsHeldByCurrentThread_TrueWhenHeldByCurrentThread() {
    var spinLock = new SpinLock(true);
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    try {
      Assert.That(spinLock.IsHeldByCurrentThread, Is.True);
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_IsHeldByCurrentThread_FalseWhenNotHeld() {
    var spinLock = new SpinLock(true);
    Assert.That(spinLock.IsHeldByCurrentThread, Is.False);
  }

  [Test]
  [Category("Exception")]
  public void SpinLock_IsHeldByCurrentThread_ThrowsWhenTrackingDisabled() {
    var spinLock = new SpinLock(false);
    Assert.Throws<InvalidOperationException>(() => _ = spinLock.IsHeldByCurrentThread);
  }

  #endregion

  #region Enter Method

  [Test]
  [Category("HappyPath")]
  public void SpinLock_Enter_AcquiresLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    try {
      Assert.That(lockTaken, Is.True);
      Assert.That(spinLock.IsHeld, Is.True);
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  [Test]
  [Category("Exception")]
  public void SpinLock_Enter_ThrowsWhenLockTakenIsTrue() {
    var spinLock = new SpinLock();
    var lockTaken = true;
    Assert.Throws<ArgumentException>(() => spinLock.Enter(ref lockTaken));
  }

  [Test]
  [Category("Exception")]
  public void SpinLock_Enter_ThrowsOnRecursiveLock() {
    var spinLock = new SpinLock(true);
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    try {
      var lockTaken2 = false;
      Assert.Throws<LockRecursionException>(() => spinLock.Enter(ref lockTaken2));
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  #endregion

  #region TryEnter Method

  [Test]
  [Category("HappyPath")]
  public void SpinLock_TryEnter_AcquiresAvailableLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.TryEnter(ref lockTaken);
    try {
      Assert.That(lockTaken, Is.True);
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  [Test]
  [Category("Exception")]
  public void SpinLock_TryEnter_ThrowsWhenLockTakenIsTrue() {
    var spinLock = new SpinLock();
    var lockTaken = true;
    Assert.Throws<ArgumentException>(() => spinLock.TryEnter(ref lockTaken));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_TryEnterWithTimeout_AcquiresAvailableLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.TryEnter(1000, ref lockTaken);
    try {
      Assert.That(lockTaken, Is.True);
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_TryEnterWithZeroTimeout_FailsOnContendedLock() {
    var spinLock = new SpinLock();
    var lockTaken1 = false;
    spinLock.Enter(ref lockTaken1);
    try {
      var acquired = false;
      var thread = new Thread(() => {
        var lockTaken2 = false;
        spinLock.TryEnter(0, ref lockTaken2);
        acquired = lockTaken2;
        if (lockTaken2)
          spinLock.Exit();
      });
      thread.Start();
      thread.Join();
      Assert.That(acquired, Is.False);
    } finally {
      if (lockTaken1)
        spinLock.Exit();
    }
  }

  [Test]
  [Category("Exception")]
  public void SpinLock_TryEnterWithTimeout_ThrowsOnNegativeTimeout() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    Assert.Throws<ArgumentOutOfRangeException>(() => spinLock.TryEnter(-2, ref lockTaken));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_TryEnterWithTimeSpan_AcquiresAvailableLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.TryEnter(TimeSpan.FromMilliseconds(1000), ref lockTaken);
    try {
      Assert.That(lockTaken, Is.True);
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  [Test]
  [Category("Exception")]
  public void SpinLock_TryEnterWithTimeSpan_ThrowsOnNegativeTimeSpan() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    Assert.Throws<ArgumentOutOfRangeException>(() => spinLock.TryEnter(TimeSpan.FromMilliseconds(-2), ref lockTaken));
  }

  #endregion

  #region Exit Method

  [Test]
  [Category("HappyPath")]
  public void SpinLock_Exit_ReleasesLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    spinLock.Exit();
    Assert.That(spinLock.IsHeld, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_ExitWithMemoryBarrier_ReleasesLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    spinLock.Exit(true);
    Assert.That(spinLock.IsHeld, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinLock_ExitWithoutMemoryBarrier_ReleasesLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.Enter(ref lockTaken);
    spinLock.Exit(false);
    Assert.That(spinLock.IsHeld, Is.False);
  }

  [Test]
  [Category("Exception")]
  public void SpinLock_Exit_ThrowsWhenNotOwnerAndTrackingEnabled() {
    var spinLock = new SpinLock(true);
    Assert.Throws<SynchronizationLockException>(() => spinLock.Exit());
  }

  #endregion

  #region Thread Safety

  [Test]
  [Category("Integration")]
  public void SpinLock_MultipleThreads_MaintainsMutualExclusion() {
    var spinLock = new SpinLock();
    var counter = 0;
    const int iterations = 1000;
    const int threadCount = 4;

    var threads = new Thread[threadCount];
    for (var i = 0; i < threadCount; ++i) {
      threads[i] = new Thread(() => {
        for (var j = 0; j < iterations; ++j) {
          var lockTaken = false;
          spinLock.Enter(ref lockTaken);
          try {
            ++counter;
          } finally {
            if (lockTaken)
              spinLock.Exit();
          }
        }
      });
    }

    foreach (var thread in threads)
      thread.Start();
    foreach (var thread in threads)
      thread.Join();

    Assert.That(counter, Is.EqualTo(threadCount * iterations));
  }

  [Test]
  [Category("Integration")]
  public void SpinLock_TryEnterWithTimeout_EventuallyAcquiresLock() {
    var spinLock = new SpinLock();
    var lockTaken1 = false;
    spinLock.Enter(ref lockTaken1);

    var acquired = false;
    var thread = new Thread(() => {
      Thread.Sleep(50);
      var lockTaken2 = false;
      spinLock.TryEnter(500, ref lockTaken2);
      acquired = lockTaken2;
      if (lockTaken2)
        spinLock.Exit();
    });
    thread.Start();

    Thread.Sleep(100);
    if (lockTaken1)
      spinLock.Exit();

    thread.Join();
    Assert.That(acquired, Is.True);
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void SpinLock_MultipleEnterExit_WorksCorrectly() {
    var spinLock = new SpinLock();
    for (var i = 0; i < 10; ++i) {
      var lockTaken = false;
      spinLock.Enter(ref lockTaken);
      Assert.That(lockTaken, Is.True);
      spinLock.Exit();
      Assert.That(spinLock.IsHeld, Is.False);
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void SpinLock_TryEnterWithInfiniteTimeout_AcquiresLock() {
    var spinLock = new SpinLock();
    var lockTaken = false;
    spinLock.TryEnter(Timeout.Infinite, ref lockTaken);
    try {
      Assert.That(lockTaken, Is.True);
    } finally {
      if (lockTaken)
        spinLock.Exit();
    }
  }

  #endregion

}
