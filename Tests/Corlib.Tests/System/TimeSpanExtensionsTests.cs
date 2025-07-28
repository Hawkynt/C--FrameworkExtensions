using NUnit.Framework;
using System.Diagnostics;

namespace System;

[TestFixture]
public class TimeSpanExtensionsTests {
  [Test]
  public void NumericExtensionMethods_CreateExpectedTimeSpans() {
    Assert.AreEqual(TimeSpan.FromSeconds(5), 5.Seconds());
    Assert.AreEqual(TimeSpan.FromMinutes(2), 2.Minutes());
    Assert.AreEqual(TimeSpan.FromHours(3), 3.Hours());
    Assert.AreEqual(TimeSpan.FromDays(4), 4.Days());
    Assert.AreEqual(TimeSpan.FromMilliseconds(150), 150.Milliseconds());
    Assert.AreEqual(TimeSpan.FromDays(14), 2.Weeks());
    Assert.AreEqual(TimeSpan.FromSeconds(1.5), 1.5.Seconds());
    Assert.AreEqual(TimeSpan.FromDays(0.5), 0.5.Days());
  }

  [Test]
  public void MultiplyAndDivideTimeSpan_ReturnsExpectedValue() {
    var ts = TimeSpan.FromSeconds(10);
    Assert.AreEqual(TimeSpan.FromSeconds(30), ts.MultipliedWith(3));
    Assert.AreEqual(TimeSpan.FromSeconds(2), ts.DividedBy(5));
  }

  [Test]
  public void DivideByTimeSpan_ReturnsDoubleRatio() {
    var ts1 = TimeSpan.FromSeconds(10);
    var ts2 = TimeSpan.FromSeconds(2);
    Assert.AreEqual(5.0, ts1.DividedBy(ts2));
  }

  [Test]
  public void FromNow_AddsSpanToCurrentTime() {
    var span = TimeSpan.FromMilliseconds(100);
    var expected = DateTime.Now + span;
    var result = span.FromNow();
    Assert.That(result - expected, Is.LessThan(TimeSpan.FromMilliseconds(20)));
  }

  [Test]
  public void FromUtcNow_AddsSpanToCurrentUtcTime() {
    var span = TimeSpan.FromMilliseconds(100);
    var expected = DateTime.UtcNow + span;
    var result = span.FromUtcNow();
    Assert.That(result - expected, Is.LessThan(TimeSpan.FromMilliseconds(20)));
  }

  [Test]
  public void StopwatchAndIterationHelpers_WorkAsExpected() {
    var span = TimeSpan.FromMilliseconds(50);
    var before = Stopwatch.GetTimestamp();
    var timestamp = span.FromStopwatchTimeStamp();
    var expectedAdvance = span.TotalSeconds * Stopwatch.Frequency;
    Assert.That(Math.Abs(timestamp - before - expectedAdvance), Is.LessThan(Stopwatch.Frequency * 0.01));

    var iteration = span.CurrenIteration();
    var calc = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency / span.TotalSeconds;
    Assert.That(Math.Abs(iteration - calc), Is.LessThan(0.1));

    var mod = span.CurrenIteration(5);
    Assert.That(mod, Is.GreaterThanOrEqualTo(0).And.LessThan(5));

    var drift = span.CurrenDrift();
    var expectedFraction = iteration - Math.Floor(iteration);
    Assert.That(Math.Abs(drift.TotalSeconds - expectedFraction), Is.LessThan(0.1));
  }
}
