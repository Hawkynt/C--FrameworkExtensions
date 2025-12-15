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
[Category("Stopwatch")]
public class StopwatchTests {

  #region Stopwatch.Restart

  [Test]
  [Category("HappyPath")]
  public void Restart_RunningStopwatch_ResetsAndRestarts() {
    var sw = new Stopwatch();
    sw.Start();
    Thread.Sleep(10);
    var elapsedBefore = sw.ElapsedMilliseconds;

    sw.Restart();
    var elapsedAfter = sw.ElapsedMilliseconds;

    Assert.That(sw.IsRunning, Is.True);
    Assert.That(elapsedAfter, Is.LessThan(elapsedBefore));
  }

  [Test]
  [Category("HappyPath")]
  public void Restart_StoppedStopwatch_ResetsAndStarts() {
    var sw = new Stopwatch();
    sw.Start();
    Thread.Sleep(10);
    sw.Stop();

    sw.Restart();

    Assert.That(sw.IsRunning, Is.True);
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(10));
  }

  #endregion

  #region Stopwatch.GetElapsedTime

  [Test]
  [Category("HappyPath")]
  public void GetElapsedTime_SingleTimestamp_ReturnsPositiveTimeSpan() {
    var start = Stopwatch.GetTimestamp();
    Thread.Sleep(10);
    var elapsed = Stopwatch.GetElapsedTime(start);

    Assert.That(elapsed.TotalMilliseconds, Is.GreaterThanOrEqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void GetElapsedTime_TwoTimestamps_ReturnsCorrectDuration() {
    var start = Stopwatch.GetTimestamp();
    Thread.Sleep(50);
    var end = Stopwatch.GetTimestamp();

    var elapsed = Stopwatch.GetElapsedTime(start, end);

    Assert.That(elapsed.TotalMilliseconds, Is.GreaterThanOrEqualTo(40));
    Assert.That(elapsed.TotalMilliseconds, Is.LessThan(200));
  }

  [Test]
  [Category("HappyPath")]
  public void GetElapsedTime_SameTimestamp_ReturnsZero() {
    var timestamp = Stopwatch.GetTimestamp();
    var elapsed = Stopwatch.GetElapsedTime(timestamp, timestamp);

    Assert.That(elapsed, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  [Category("EdgeCase")]
  public void GetElapsedTime_ReversedTimestamps_ReturnsNegativeTimeSpan() {
    var start = Stopwatch.GetTimestamp();
    Thread.Sleep(10);
    var end = Stopwatch.GetTimestamp();

    var elapsed = Stopwatch.GetElapsedTime(end, start);

    Assert.That(elapsed.TotalMilliseconds, Is.LessThan(0));
  }

  #endregion

}
