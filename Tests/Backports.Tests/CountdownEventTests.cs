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
[Category("CountdownEvent")]
public class CountdownEventTests {

  #region Constructor

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Constructor_InitializesWithCount() {
    using var cde = new CountdownEvent(5);
    Assert.That(cde.InitialCount, Is.EqualTo(5));
    Assert.That(cde.CurrentCount, Is.EqualTo(5));
    Assert.That(cde.IsSet, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Constructor_WithZeroCount_IsSignaled() {
    using var cde = new CountdownEvent(0);
    Assert.That(cde.CurrentCount, Is.EqualTo(0));
    Assert.That(cde.IsSet, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Constructor_NegativeCount_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownEvent(-1));
  }

  #endregion

  #region Signal

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Signal_DecrementsCount() {
    using var cde = new CountdownEvent(3);
    cde.Signal();
    Assert.That(cde.CurrentCount, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Signal_ReturnsTrue_WhenCountReachesZero() {
    using var cde = new CountdownEvent(1);
    var result = cde.Signal();
    Assert.That(result, Is.True);
    Assert.That(cde.IsSet, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Signal_ReturnsFalse_WhenCountDoesNotReachZero() {
    using var cde = new CountdownEvent(2);
    var result = cde.Signal();
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_SignalMultiple_DecrementsCount() {
    using var cde = new CountdownEvent(5);
    cde.Signal(3);
    Assert.That(cde.CurrentCount, Is.EqualTo(2));
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Signal_WhenAlreadySet_Throws() {
    using var cde = new CountdownEvent(0);
    Assert.Throws<InvalidOperationException>(() => cde.Signal());
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Signal_WithZeroCount_Throws() {
    using var cde = new CountdownEvent(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => cde.Signal(0));
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Signal_WithNegativeCount_Throws() {
    using var cde = new CountdownEvent(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => cde.Signal(-1));
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Signal_MoreThanCurrentCount_Throws() {
    using var cde = new CountdownEvent(2);
    Assert.Throws<InvalidOperationException>(() => cde.Signal(3));
  }

  #endregion

  #region AddCount

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_AddCount_IncrementsCount() {
    using var cde = new CountdownEvent(2);
    cde.AddCount();
    Assert.That(cde.CurrentCount, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_AddCountMultiple_IncrementsCount() {
    using var cde = new CountdownEvent(2);
    cde.AddCount(3);
    Assert.That(cde.CurrentCount, Is.EqualTo(5));
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_AddCount_WhenSet_Throws() {
    using var cde = new CountdownEvent(0);
    Assert.Throws<InvalidOperationException>(() => cde.AddCount());
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_AddCount_WithZero_Throws() {
    using var cde = new CountdownEvent(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => cde.AddCount(0));
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_AddCount_WithNegative_Throws() {
    using var cde = new CountdownEvent(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => cde.AddCount(-1));
  }

  #endregion

  #region TryAddCount

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_TryAddCount_ReturnsTrue_WhenNotSet() {
    using var cde = new CountdownEvent(2);
    var result = cde.TryAddCount();
    Assert.That(result, Is.True);
    Assert.That(cde.CurrentCount, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_TryAddCount_ReturnsFalse_WhenSet() {
    using var cde = new CountdownEvent(0);
    var result = cde.TryAddCount();
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_TryAddCount_WithZero_Throws() {
    using var cde = new CountdownEvent(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => cde.TryAddCount(0));
  }

  #endregion

  #region Wait

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Wait_ReturnsImmediately_WhenAlreadySet() {
    using var cde = new CountdownEvent(0);
    cde.Wait();
    Assert.Pass();
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_WaitWithTimeout_ReturnsTrue_WhenSet() {
    using var cde = new CountdownEvent(0);
    var result = cde.Wait(100);
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_WaitWithTimeout_ReturnsFalse_WhenNotSet() {
    using var cde = new CountdownEvent(1);
    var result = cde.Wait(10);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_WaitWithTimeSpan_ReturnsTrue_WhenSet() {
    using var cde = new CountdownEvent(0);
    var result = cde.Wait(TimeSpan.FromMilliseconds(100));
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Wait_InvalidTimeout_Throws() {
    using var cde = new CountdownEvent(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => cde.Wait(-2));
  }

  #endregion

  #region Reset

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Reset_RestoresToInitialCount() {
    using var cde = new CountdownEvent(3);
    cde.Signal();
    cde.Signal();
    cde.Reset();
    Assert.That(cde.CurrentCount, Is.EqualTo(3));
    Assert.That(cde.IsSet, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_ResetWithNewCount_SetsNewCount() {
    using var cde = new CountdownEvent(3);
    cde.Reset(5);
    Assert.That(cde.CurrentCount, Is.EqualTo(5));
    Assert.That(cde.InitialCount, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_ResetToZero_SetsEvent() {
    using var cde = new CountdownEvent(3);
    cde.Reset(0);
    Assert.That(cde.IsSet, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Reset_NegativeCount_Throws() {
    using var cde = new CountdownEvent(3);
    Assert.Throws<ArgumentOutOfRangeException>(() => cde.Reset(-1));
  }

  #endregion

  #region WaitHandle

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_WaitHandle_ReturnsValidHandle() {
    using var cde = new CountdownEvent(1);
    var handle = cde.WaitHandle;
    Assert.That(handle, Is.Not.Null);
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_WaitHandle_ThrowsWhenDisposed() {
    var cde = new CountdownEvent(1);
    cde.Dispose();
    Assert.Throws<ObjectDisposedException>(() => _ = cde.WaitHandle);
  }

  #endregion

  #region Dispose

  [Test]
  [Category("HappyPath")]
  public void CountdownEvent_Dispose_CanBeCalledMultipleTimes() {
    var cde = new CountdownEvent(1);
    cde.Dispose();
    cde.Dispose();
    Assert.Pass();
  }

  [Test]
  [Category("Exception")]
  public void CountdownEvent_Signal_AfterDispose_Throws() {
    var cde = new CountdownEvent(1);
    cde.Dispose();
    Assert.Throws<ObjectDisposedException>(() => cde.Signal());
  }

  #endregion

  #region Thread Safety

  [Test]
  [Category("Integration")]
  public void CountdownEvent_MultipleThreads_SignalConcurrently() {
    using var cde = new CountdownEvent(10);
    var threads = new Thread[10];
    for (var i = 0; i < 10; ++i) {
      threads[i] = new Thread(() => cde.Signal());
    }

    foreach (var thread in threads)
      thread.Start();
    foreach (var thread in threads)
      thread.Join();

    Assert.That(cde.IsSet, Is.True);
    Assert.That(cde.CurrentCount, Is.EqualTo(0));
  }

  [Test]
  [Category("Integration")]
  public void CountdownEvent_Wait_UnblocksWhenSignaled() {
    using var cde = new CountdownEvent(1);
    var waitCompleted = false;

    var waitThread = new Thread(() => {
      cde.Wait();
      waitCompleted = true;
    });
    waitThread.Start();

    Thread.Sleep(50);
    Assert.That(waitCompleted, Is.False);

    cde.Signal();
    waitThread.Join(1000);
    Assert.That(waitCompleted, Is.True);
  }

  #endregion

}
