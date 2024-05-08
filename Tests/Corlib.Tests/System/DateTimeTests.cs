using System.Globalization;
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


}