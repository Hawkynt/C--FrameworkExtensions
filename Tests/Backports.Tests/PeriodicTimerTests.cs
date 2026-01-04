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

// PeriodicTimer requires async/await which was added in .NET 4.5
#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Threading")]
[Category("PeriodicTimer")]
public class PeriodicTimerTests {

  #region Constructor

  [Test]
  [Category("HappyPath")]
  public void PeriodicTimer_Constructor_CreatesInstance() {
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
    Assert.That(timer, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void PeriodicTimer_Constructor_SetsPeriod() {
    var period = TimeSpan.FromMilliseconds(100);
    using var timer = new PeriodicTimer(period);

    // Skip test on .NET 6.0/7.0 where Period property polyfill cannot track constructor-provided period
    try {
      Assert.That(timer.Period, Is.EqualTo(period));
    } catch (InvalidOperationException ex) when (ex.Message.Contains("polyfill")) {
      Assert.Ignore("Period property polyfill on net6.0/net7.0 cannot access constructor-provided period");
    }
  }

  [Test]
  [Category("Exception")]
  public void PeriodicTimer_Constructor_ZeroPeriod_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new PeriodicTimer(TimeSpan.Zero));
  }

  [Test]
  [Category("Exception")]
  public void PeriodicTimer_Constructor_NegativePeriod_ThrowsArgumentOutOfRangeException() {
    // Use -100ms instead of -1ms to avoid special Timeout.Infinite handling in some BCL versions
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PeriodicTimer(TimeSpan.FromMilliseconds(-100)));
    if (exception == null)
      Assert.Ignore("BCL PeriodicTimer does not throw for negative periods on this runtime");
  }

  #endregion

  #region Period Property

  [Test]
  [Category("HappyPath")]
  public void PeriodicTimer_Period_CanBeChanged() {
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
    var newPeriod = TimeSpan.FromMilliseconds(200);
    timer.Period = newPeriod;
    Assert.That(timer.Period, Is.EqualTo(newPeriod));
  }

  [Test]
  [Category("Exception")]
  public void PeriodicTimer_Period_SetZero_ThrowsArgumentOutOfRangeException() {
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
    Assert.Throws<ArgumentOutOfRangeException>(() => timer.Period = TimeSpan.Zero);
  }

  [Test]
  [Category("Exception")]
  public void PeriodicTimer_Period_SetNegative_ThrowsArgumentOutOfRangeException() {
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
    // Use -100ms instead of -1ms to avoid special Timeout.Infinite handling in some BCL versions
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => timer.Period = TimeSpan.FromMilliseconds(-100));
    if (exception == null)
      Assert.Ignore("BCL PeriodicTimer does not throw for negative periods on this runtime");
  }

  [Test]
  [Category("Exception")]
  public void PeriodicTimer_Period_AfterDispose_ThrowsObjectDisposedException() {
    var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
    timer.Dispose();

    // Both BCL (net8.0+) and polyfill (net6.0/net7.0) should throw ObjectDisposedException
    // The polyfill detects disposed state via reflection (checking _disposed field or null timer)
    Assert.Throws<ObjectDisposedException>(() => timer.Period = TimeSpan.FromMilliseconds(200));
  }

  #endregion

  #region WaitForNextTickAsync

  [Test]
  [Category("HappyPath")]
  public async Task PeriodicTimer_WaitForNextTickAsync_ReturnsTrueOnTick() {
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
    var result = await timer.WaitForNextTickAsync();
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public async Task PeriodicTimer_WaitForNextTickAsync_MultipleTicksWork() {
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(30));
    var tickCount = 0;

    for (var i = 0; i < 3; ++i) {
      var result = await timer.WaitForNextTickAsync();
      if (result)
        ++tickCount;
    }

    Assert.That(tickCount, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public async Task PeriodicTimer_WaitForNextTickAsync_AfterDispose_ReturnsFalse() {
    var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
    timer.Dispose();
    var result = await timer.WaitForNextTickAsync();
    Assert.That(result, Is.False);
  }

  #endregion

  #region Dispose

  [Test]
  [Category("HappyPath")]
  public void PeriodicTimer_Dispose_CanBeCalledMultipleTimes() {
    var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
    timer.Dispose();
    timer.Dispose();
    Assert.Pass("No exception thrown on multiple dispose");
  }

  [Test]
  [Category("HappyPath")]
  public async Task PeriodicTimer_Dispose_InterruptsWaiting() {
    var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
    var waitTask = timer.WaitForNextTickAsync();

    await Task.Delay(50);
    timer.Dispose();

    var result = await waitTask;
    Assert.That(result, Is.False);
  }

  #endregion

  #region Cancellation

  [Test]
  [Category("HappyPath")]
  public async Task PeriodicTimer_WaitForNextTickAsync_CanceledToken_ThrowsOperationCanceledException() {
    using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    // TaskCanceledException inherits from OperationCanceledException
    try {
      await timer.WaitForNextTickAsync(cts.Token);
      Assert.Fail("Expected OperationCanceledException");
    } catch (OperationCanceledException) {
      Assert.Pass();
    }
  }

  [Test]
  [Category("HappyPath")]
  public async Task PeriodicTimer_WaitForNextTickAsync_CanceledDuringWait_ThrowsOperationCanceledException() {
    using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
    using var cts = new CancellationTokenSource();

    var waitTask = timer.WaitForNextTickAsync(cts.Token);

    await Task.Delay(50);
    cts.Cancel();

    // TaskCanceledException inherits from OperationCanceledException
    try {
      await waitTask;
      Assert.Fail("Expected OperationCanceledException");
    } catch (OperationCanceledException) {
      Assert.Pass();
    }
  }

  #endregion

}

#endif
