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
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("TimeProvider")]
public class TimeProviderTests {

  #region TimeProvider.System Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_System_IsNotNull() {
    Assert.That(TimeProvider.System, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_System_ReturnsSameInstance() {
    var first = TimeProvider.System;
    var second = TimeProvider.System;
    Assert.That(first, Is.SameAs(second));
  }

  #endregion

  #region GetUtcNow Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_GetUtcNow_ReturnsUtcTime() {
    var provider = TimeProvider.System;
    var before = DateTimeOffset.UtcNow;
    var result = provider.GetUtcNow();
    var after = DateTimeOffset.UtcNow;

    Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
    Assert.That(result, Is.GreaterThanOrEqualTo(before));
    Assert.That(result, Is.LessThanOrEqualTo(after));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_GetUtcNow_ReturnsCurrentTime() {
    var provider = TimeProvider.System;
    var systemTime = DateTimeOffset.UtcNow;
    var providerTime = provider.GetUtcNow();

    var difference = Math.Abs((providerTime - systemTime).TotalMilliseconds);
    Assert.That(difference, Is.LessThan(100));
  }

  #endregion

  #region GetLocalNow Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_GetLocalNow_ReturnsLocalTime() {
    var provider = TimeProvider.System;
    var before = DateTimeOffset.Now;
    var result = provider.GetLocalNow();
    var after = DateTimeOffset.Now;

    Assert.That(result.Offset, Is.EqualTo(TimeZoneInfo.Local.GetUtcOffset(result)));
    Assert.That(result, Is.GreaterThanOrEqualTo(before.AddSeconds(-1)));
    Assert.That(result, Is.LessThanOrEqualTo(after.AddSeconds(1)));
  }

  #endregion

  #region LocalTimeZone Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_LocalTimeZone_ReturnsLocalTimeZone() {
    var provider = TimeProvider.System;
    Assert.That(provider.LocalTimeZone, Is.EqualTo(TimeZoneInfo.Local));
  }

  #endregion

  #region TimestampFrequency Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_TimestampFrequency_MatchesStopwatch() {
    var provider = TimeProvider.System;
    Assert.That(provider.TimestampFrequency, Is.EqualTo(Stopwatch.Frequency));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_TimestampFrequency_IsPositive() {
    var provider = TimeProvider.System;
    Assert.That(provider.TimestampFrequency, Is.GreaterThan(0));
  }

  #endregion

  #region GetTimestamp Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_GetTimestamp_ReturnsPositiveValue() {
    var provider = TimeProvider.System;
    var timestamp = provider.GetTimestamp();
    Assert.That(timestamp, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_GetTimestamp_Increases() {
    var provider = TimeProvider.System;
    var first = provider.GetTimestamp();
    Thread.Sleep(1);
    var second = provider.GetTimestamp();
    Assert.That(second, Is.GreaterThan(first));
  }

  #endregion

  #region GetElapsedTime Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_GetElapsedTime_ReturnsPositiveTimeSpan() {
    var provider = TimeProvider.System;
    var start = provider.GetTimestamp();
    Thread.Sleep(10);
    var elapsed = provider.GetElapsedTime(start);

    Assert.That(elapsed, Is.GreaterThan(TimeSpan.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_GetElapsedTime_WithTwoTimestamps_ReturnsCorrectDuration() {
    var provider = TimeProvider.System;
    var start = provider.GetTimestamp();
    Thread.Sleep(50);
    var end = provider.GetTimestamp();

    var elapsed = provider.GetElapsedTime(start, end);
    Assert.That(elapsed.TotalMilliseconds, Is.GreaterThan(40));
    Assert.That(elapsed.TotalMilliseconds, Is.LessThan(200));
  }

  [Test]
  [Category("EdgeCase")]
  public void TimeProvider_GetElapsedTime_SameTimestamp_ReturnsZero() {
    var provider = TimeProvider.System;
    var timestamp = provider.GetTimestamp();
    var elapsed = provider.GetElapsedTime(timestamp, timestamp);

    Assert.That(elapsed, Is.EqualTo(TimeSpan.Zero));
  }

  #endregion

  #region CreateTimer Tests

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_CreateTimer_ReturnsITimer() {
    var provider = TimeProvider.System;
    using var timer = provider.CreateTimer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    Assert.That(timer, Is.Not.Null);
    Assert.That(timer, Is.InstanceOf<ITimer>());
  }

  [Test]
  [Category("HappyPath")]
  public void TimeProvider_CreateTimer_CallbackIsInvoked() {
    var provider = TimeProvider.System;
    var callbackInvoked = new ManualResetEventSlim(false);

    using var timer = provider.CreateTimer(
      _ => callbackInvoked.Set(),
      null,
      TimeSpan.FromMilliseconds(10),
      Timeout.InfiniteTimeSpan
    );

    var wasInvoked = callbackInvoked.Wait(TimeSpan.FromSeconds(1));
    Assert.That(wasInvoked, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void TimeProvider_CreateTimer_NullCallback_ThrowsArgumentNullException() {
    var provider = TimeProvider.System;

    Assert.Throws<ArgumentNullException>(() =>
      provider.CreateTimer(null!, null, TimeSpan.Zero, TimeSpan.Zero)
    );
  }

  [Test]
  [Category("Exception")]
  public void TimeProvider_CreateTimer_NegativeDueTime_ThrowsArgumentOutOfRangeException() {
    var provider = TimeProvider.System;

    Assert.Throws<ArgumentOutOfRangeException>(() =>
      provider.CreateTimer(_ => { }, null, TimeSpan.FromMilliseconds(-2), TimeSpan.Zero)
    );
  }

  [Test]
  [Category("Exception")]
  public void TimeProvider_CreateTimer_NegativePeriod_ThrowsArgumentOutOfRangeException() {
    var provider = TimeProvider.System;

    Assert.Throws<ArgumentOutOfRangeException>(() =>
      provider.CreateTimer(_ => { }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-2))
    );
  }

  #endregion

  #region ITimer Tests

  [Test]
  [Category("HappyPath")]
  public void ITimer_Change_ReturnsTrue() {
    var provider = TimeProvider.System;
    using var timer = provider.CreateTimer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    var result = timer.Change(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan);
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ITimer_Change_AfterDispose_ReturnsFalse() {
    var provider = TimeProvider.System;
    var timer = provider.CreateTimer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    timer.Dispose();

    var result = timer.Change(TimeSpan.FromMilliseconds(100), Timeout.InfiniteTimeSpan);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ITimer_Dispose_CanBeCalledMultipleTimes() {
    var provider = TimeProvider.System;
    var timer = provider.CreateTimer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    Assert.DoesNotThrow(() => {
      timer.Dispose();
      timer.Dispose();
      timer.Dispose();
    });
  }

  [Test]
  [Category("HappyPath")]
  public void ITimer_DisposeAsync_CompletesSuccessfully() {
    var provider = TimeProvider.System;
    var timer = provider.CreateTimer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    var task = timer.DisposeAsync();
    Assert.That(task.IsCompleted, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void ITimer_Change_NegativeDueTime_ThrowsArgumentOutOfRangeException() {
    var provider = TimeProvider.System;
    using var timer = provider.CreateTimer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    Assert.Throws<ArgumentOutOfRangeException>(() =>
      timer.Change(TimeSpan.FromMilliseconds(-2), TimeSpan.Zero)
    );
  }

  #endregion

}
