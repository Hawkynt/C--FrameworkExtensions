using NUnit.Framework;

namespace System;

[TestFixture]
public class DateTimeAdditionalTests {
  [Test]
  public void StartAndEndOfDay_ReturnExpectedValues() {
    var sample = new DateTime(2024, 1, 3, 10, 30, 15);
    Assert.AreEqual(new DateTime(2024, 1, 3, 0, 0, 0), sample.StartOfDay());
    Assert.AreEqual(new DateTime(2024, 1, 3).AddDays(1).AddTicks(-1), sample.EndOfDay());
  }

  [Test]
  public void AddWeeks_AddsSevenDaysForEachWeek() {
    var date = new DateTime(2024, 1, 1);
    Assert.AreEqual(new DateTime(2024, 1, 15), date.AddWeeks(2));
  }

  [Test]
  public void DateOfDayOfCurrentWeek_ReturnsCorrectDate() {
    var date = new DateTime(2024, 1, 3); // Wednesday
    var monday = date.DateOfDayOfCurrentWeek(DayOfWeek.Monday);
    Assert.AreEqual(new DateTime(2024, 1, 1).EndOfDay(), monday);
  }

  [Test]
  public void FirstAndLastDayOfMonthAndYear() {
    var date = new DateTime(2024, 5, 20);
    Assert.AreEqual(new DateTime(2024, 5, 1), date.FirstDayOfMonth());
    Assert.AreEqual(new DateTime(2024, 5, 31), date.LastDayOfMonth());
    Assert.AreEqual(new DateTime(2024, 1, 1), date.FirstDayOfYear());
    Assert.AreEqual(new DateTime(2024, 12, 31), date.LastDayOfYear());
  }

  [Test]
  public void UnixConversionRoundTrip() {
    var date = new DateTime(2024, 8, 18, 12, 0, 0, DateTimeKind.Utc);
    var ticks = date.AsUnixTicksUtc();
    var recon = DateTimeExtensions.FromUnixTicks(ticks, DateTimeKind.Utc);
    Assert.AreEqual(date, recon);
  }

  [Test]
  public void MaxMin_ReturnCorrectInstance() {
    var early = new DateTime(2024, 1, 1);
    var late = new DateTime(2024, 1, 2);
    Assert.AreEqual(late, early.Max(late));
    Assert.AreEqual(late, late.Max(early));
    Assert.AreEqual(early, early.Min(late));
    Assert.AreEqual(early, late.Min(early));
  }

  [Test]
  public void DaysTill_ReturnsInclusiveRange() {
    var start = new DateTime(2024, 1, 1);
    var end = new DateTime(2024, 1, 4);
    var expected = new[] { new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), new DateTime(2024, 1, 3), new DateTime(2024, 1, 4) };
    CollectionAssert.AreEqual(expected, start.DaysTill(end));
  }

  [Test]
  public void SubstractHelpers_SubtractCorrectAmounts() {
    var date = new DateTime(2024, 1, 10, 10, 10, 10, DateTimeKind.Utc);
    Assert.AreEqual(date.AddTicks(-5), date.SubstractTicks(5));
    Assert.AreEqual(date.AddMilliseconds(-5), date.SubstractMilliseconds(5));
    Assert.AreEqual(date.AddSeconds(-5), date.SubstractSeconds(5));
    Assert.AreEqual(date.AddMinutes(-5), date.SubstractMinutes(5));
    Assert.AreEqual(date.AddHours(-5), date.SubstractHours(5));
    Assert.AreEqual(date.AddDays(-5), date.SubstractDays(5));
    Assert.AreEqual(date.AddWeeks(-1), date.SubstractWeeks(1));
    Assert.AreEqual(date.AddMonths(-1), date.SubstractMonths(1));
    Assert.AreEqual(date.AddYears(-1), date.SubstractYears(1));
  }

  [Test]
  public void UnixMillisecondsAndSecondsConversions() {
    var date = new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc);
    Assert.AreEqual(86400000, date.AsUnixMillisecondsUtc());
    Assert.AreEqual(date, DateTimeExtensions.FromUnixSeconds(86400, DateTimeKind.Utc));
  }
}
