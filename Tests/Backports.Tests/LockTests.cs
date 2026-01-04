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
[Category("Threading")]
[Category("Lock")]
public class LockTests {

  #region Constructor

  [Test]
  [Category("HappyPath")]
  public void Lock_Constructor_CreatesInstance() {
    var @lock = new Lock();
    Assert.That(@lock, Is.Not.Null);
  }

  #endregion

  #region IsHeldByCurrentThread

  [Test]
  [Category("HappyPath")]
  public void Lock_IsHeldByCurrentThread_FalseWhenNotHeld() {
    var @lock = new Lock();
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_IsHeldByCurrentThread_TrueWhenHeld() {
    var @lock = new Lock();
    @lock.Enter();
    try {
      Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    } finally {
      @lock.Exit();
    }
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_IsHeldByCurrentThread_FalseAfterExit() {
    var @lock = new Lock();
    @lock.Enter();
    @lock.Exit();
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
  }

  #endregion

  #region Enter and Exit

  [Test]
  [Category("HappyPath")]
  public void Lock_Enter_AcquiresLock() {
    var @lock = new Lock();
    @lock.Enter();
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    @lock.Exit();
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_EnterMultipleTimes_SupportsRecursion() {
    var @lock = new Lock();
    @lock.Enter();
    @lock.Enter();
    @lock.Enter();
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    @lock.Exit();
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    @lock.Exit();
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    @lock.Exit();
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
  }

  [Test]
  [Category("Exception")]
  public void Lock_Exit_ThrowsWhenNotHeld() {
    var @lock = new Lock();
    Assert.Throws<SynchronizationLockException>(() => @lock.Exit());
  }

  #endregion

  #region EnterScope

  [Test]
  [Category("HappyPath")]
  public void Lock_EnterScope_AcquiresLock() {
    var @lock = new Lock();
    using (@lock.EnterScope()) {
      Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    }
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_EnterScope_DisposesCorrectly() {
    var @lock = new Lock();
    var scope = @lock.EnterScope();
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    scope.Dispose();
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_EnterScope_DoubleDisposeIsNoOp() {
    var @lock = new Lock();
    var scope = @lock.EnterScope();
    scope.Dispose();
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
    scope.Dispose();
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
  }

  #endregion

  #region TryEnter

  [Test]
  [Category("HappyPath")]
  public void Lock_TryEnter_ReturnsTrueAndAcquiresLock() {
    var @lock = new Lock();
    var result = @lock.TryEnter();
    Assert.That(result, Is.True);
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    @lock.Exit();
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_TryEnter_SupportsRecursion() {
    var @lock = new Lock();
    @lock.Enter();
    var result = @lock.TryEnter();
    Assert.That(result, Is.True);
    @lock.Exit();
    @lock.Exit();
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_TryEnterWithTimeout_ReturnsTrueWhenAvailable() {
    var @lock = new Lock();
    var result = @lock.TryEnter(100);
    Assert.That(result, Is.True);
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    @lock.Exit();
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_TryEnterWithTimeSpan_ReturnsTrueWhenAvailable() {
    var @lock = new Lock();
    var result = @lock.TryEnter(TimeSpan.FromMilliseconds(100));
    Assert.That(result, Is.True);
    Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    @lock.Exit();
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_TryEnterWithZeroTimeout_ReturnsTrueWhenAvailable() {
    var @lock = new Lock();
    var result = @lock.TryEnter(0);
    Assert.That(result, Is.True);
    @lock.Exit();
  }

  [Test]
  [Category("Exception")]
  public void Lock_TryEnterWithNegativeTimeout_ThrowsArgumentOutOfRangeException() {
    var @lock = new Lock();
    Assert.Throws<ArgumentOutOfRangeException>(() => @lock.TryEnter(-2));
  }

  [Test]
  [Category("HappyPath")]
  public void Lock_TryEnterWithInfiniteTimeout_ReturnsTrueWhenAvailable() {
    var @lock = new Lock();
    var result = @lock.TryEnter(Timeout.Infinite);
    Assert.That(result, Is.True);
    @lock.Exit();
  }

  [Test]
  [Category("Exception")]
  public void Lock_TryEnterWithInvalidTimeSpan_ThrowsArgumentOutOfRangeException() {
    var @lock = new Lock();
    Assert.Throws<ArgumentOutOfRangeException>(() => @lock.TryEnter(TimeSpan.FromMilliseconds(-2)));
  }

  #endregion

  #region Concurrency

  [Test]
  [Category("Integration")]
  [Category("Threading")]
  public void Lock_ConcurrentAccess_ProtectsResource() {
    var @lock = new Lock();
    var counter = 0;
    const int iterations = 100;
    const int threadCount = 4;
    var threads = new Thread[threadCount];

    for (var i = 0; i < threadCount; ++i)
      threads[i] = new Thread(() => {
        for (var j = 0; j < iterations; ++j) {
          @lock.Enter();
          try {
            var temp = counter;
            Thread.Sleep(0);
            counter = temp + 1;
          } finally {
            @lock.Exit();
          }
        }
      });

    foreach (var thread in threads)
      thread.Start();

    foreach (var thread in threads)
      thread.Join();

    Assert.That(counter, Is.EqualTo(threadCount * iterations));
  }

  [Test]
  [Category("Integration")]
  [Category("Threading")]
  public void Lock_TryEnterWithTimeout_ReturnsFalseWhenHeldByOther() {
    var @lock = new Lock();
    var otherThreadEntered = new ManualResetEvent(false);
    var otherThreadExit = new ManualResetEvent(false);
    var tryEnterResult = true;

    var otherThread = new Thread(() => {
      @lock.Enter();
      otherThreadEntered.Set();
      otherThreadExit.WaitOne();
      @lock.Exit();
    });

    otherThread.Start();
    otherThreadEntered.WaitOne();

    tryEnterResult = @lock.TryEnter(10);
    otherThreadExit.Set();
    otherThread.Join();

    Assert.That(tryEnterResult, Is.False);
  }

  #endregion

  #region lock Keyword Compatibility

  [Test]
  [Category("HappyPath")]
  public void Lock_WithLockKeyword_WorksCorrectly() {
    var @lock = new Lock();
    var executed = false;

    lock (@lock) {
      executed = true;
      Assert.That(@lock.IsHeldByCurrentThread, Is.True);
    }

    Assert.That(executed, Is.True);
    Assert.That(@lock.IsHeldByCurrentThread, Is.False);
  }

  #endregion

}
