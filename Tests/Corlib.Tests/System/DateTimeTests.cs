using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public class DateTimeTests {
  private static DateTime Parse(string text) => DateTime.TryParseExact(text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result)
    ? result
    : DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

  [TestCase("20191027", "20191021000000", DayOfWeek.Monday)]
  [TestCase("20191026", "20191021000000", DayOfWeek.Monday)]
  [TestCase("20191025", "20191021000000", DayOfWeek.Monday)]
  [TestCase("20191024", "20191021000000", DayOfWeek.Monday)]
  [TestCase("20191023", "20191021000000", DayOfWeek.Monday)]
  [TestCase("20191022", "20191021000000", DayOfWeek.Monday)]
  [TestCase("20191021000000", "20191021000000", DayOfWeek.Monday)]
  public void TestStartOfWeek(string inputDate, string expectedDate, DayOfWeek startDayOfWeek) {
    var input = Parse(inputDate);
    var expected = Parse(expectedDate);
    var result = input.StartOfWeek(startDayOfWeek);
    Assert.AreEqual(expected, result);
  }

  [TestCase("20191027", "20191021000000", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191026", "20191021000000", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191025", "20191021000000", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191024", "20191021000000", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191023", "20191021000000", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191022", "20191021000000", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191021000000", "20191021000000", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191107", "20191104", DayOfWeek.Monday, DayOfWeek.Monday)]
  [TestCase("20191107", "20191105", DayOfWeek.Tuesday, DayOfWeek.Monday)]
  [TestCase("20191107", "20191110", DayOfWeek.Sunday, DayOfWeek.Monday)]
  [TestCase("20191107", "20191103", DayOfWeek.Sunday, DayOfWeek.Sunday)]
  public void TestDayInCurrentWeek(string inputDate, string expectedDate, DayOfWeek dayOfWeek, DayOfWeek startDayOfWeek) {
    var input = Parse(inputDate);
    var expected = Parse(expectedDate);
    var result = input.DayInCurrentWeek(dayOfWeek, startDayOfWeek);
    Assert.AreEqual(expected, result);
  }

  #region DateTime.Sequence

  [Test]
  [Category("HappyPath")]
  public void Sequence_HourlyForDay_GeneratesCorrectSequence() {
    var start = new DateTime(2024, 1, 1, 0, 0, 0);
    var end = new DateTime(2024, 1, 1, 3, 0, 0);
    var result = DateTime.Sequence(start, end, TimeSpan.FromHours(1)).ToArray();

    Assert.That(result.Length, Is.EqualTo(4));
    Assert.That(result[0], Is.EqualTo(new DateTime(2024, 1, 1, 0, 0, 0)));
    Assert.That(result[1], Is.EqualTo(new DateTime(2024, 1, 1, 1, 0, 0)));
    Assert.That(result[2], Is.EqualTo(new DateTime(2024, 1, 1, 2, 0, 0)));
    Assert.That(result[3], Is.EqualTo(new DateTime(2024, 1, 1, 3, 0, 0)));
  }

  [Test]
  [Category("HappyPath")]
  public void Sequence_DailyForWeek_GeneratesCorrectSequence() {
    var start = new DateTime(2024, 1, 1);
    var end = new DateTime(2024, 1, 7);
    var result = DateTime.Sequence(start, end, TimeSpan.FromDays(1)).ToArray();

    Assert.That(result.Length, Is.EqualTo(7));
    Assert.That(result[0], Is.EqualTo(new DateTime(2024, 1, 1)));
    Assert.That(result[6], Is.EqualTo(new DateTime(2024, 1, 7)));
  }

  [Test]
  [Category("EdgeCase")]
  public void Sequence_SingleElement_ReturnsSingleElement() {
    var date = new DateTime(2024, 1, 1);
    var result = DateTime.Sequence(date, date, TimeSpan.FromDays(1)).ToArray();

    Assert.That(result.Length, Is.EqualTo(1));
    Assert.That(result[0], Is.EqualTo(date));
  }

  [Test]
  [Category("EdgeCase")]
  public void Sequence_EndBeforeStart_ReturnsEmpty() {
    var start = new DateTime(2024, 1, 7);
    var end = new DateTime(2024, 1, 1);
    var result = DateTime.Sequence(start, end, TimeSpan.FromDays(1)).ToArray();

    Assert.That(result, Is.Empty);
  }

  #endregion

  #region DateTime.InfiniteSequence

  [Test]
  [Category("HappyPath")]
  public void InfiniteSequence_TakeFirst5Hours_ReturnsCorrectValues() {
    var start = new DateTime(2024, 1, 1, 10, 0, 0);
    var result = DateTime.InfiniteSequence(start, TimeSpan.FromHours(1)).Take(5).ToArray();

    Assert.That(result.Length, Is.EqualTo(5));
    Assert.That(result[0], Is.EqualTo(new DateTime(2024, 1, 1, 10, 0, 0)));
    Assert.That(result[1], Is.EqualTo(new DateTime(2024, 1, 1, 11, 0, 0)));
    Assert.That(result[4], Is.EqualTo(new DateTime(2024, 1, 1, 14, 0, 0)));
  }

  [Test]
  [Category("HappyPath")]
  public void InfiniteSequence_Every15Minutes_ReturnsCorrectValues() {
    var start = new DateTime(2024, 1, 1, 0, 0, 0);
    var result = DateTime.InfiniteSequence(start, TimeSpan.FromMinutes(15)).Take(4).ToArray();

    Assert.That(result.Length, Is.EqualTo(4));
    Assert.That(result[0], Is.EqualTo(new DateTime(2024, 1, 1, 0, 0, 0)));
    Assert.That(result[1], Is.EqualTo(new DateTime(2024, 1, 1, 0, 15, 0)));
    Assert.That(result[2], Is.EqualTo(new DateTime(2024, 1, 1, 0, 30, 0)));
    Assert.That(result[3], Is.EqualTo(new DateTime(2024, 1, 1, 0, 45, 0)));
  }

  #endregion
}
