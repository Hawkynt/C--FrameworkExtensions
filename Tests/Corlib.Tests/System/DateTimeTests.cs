using System.Globalization;
using NUnit.Framework;

namespace System; 

[TestFixture]
public class DateTimeTests {
  private static DateTime Parse(string text) => DateTime.TryParseExact(text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result)
    ? result
    : DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

  [Test]
  public void TestStartOfWeek() {
    var testcases = new[] {
      Tuple.Create("20191027", "20191021000000",DayOfWeek.Monday),
      Tuple.Create("20191026","20191021000000",DayOfWeek.Monday),
      Tuple.Create("20191025","20191021000000",DayOfWeek.Monday),
      Tuple.Create("20191024","20191021000000",DayOfWeek.Monday),
      Tuple.Create("20191023","20191021000000",DayOfWeek.Monday),
      Tuple.Create("20191022","20191021000000",DayOfWeek.Monday),
      Tuple.Create("20191021000000","20191021000000",DayOfWeek.Monday)
    };

    foreach (var @case in testcases) {
      var input = Parse(@case.Item1);
      var expected = Parse(@case.Item2);
      var startDayOfWeek = @case.Item3;
      var result = input.StartOfWeek(startDayOfWeek);
      Assert.AreEqual(result, expected);
    }
  }

  [Test]
  public void TestDayInCurrentWeek() {
    var testcases = new[] {
      Tuple.Create("20191027", "20191021000000",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191026","20191021000000",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191025","20191021000000",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191024","20191021000000",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191023","20191021000000",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191022","20191021000000",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191021000000","20191021000000",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191107","20191104",DayOfWeek.Monday,DayOfWeek.Monday),
      Tuple.Create("20191107","20191105",DayOfWeek.Tuesday,DayOfWeek.Monday),
      Tuple.Create("20191107","20191110",DayOfWeek.Sunday,DayOfWeek.Monday),
      Tuple.Create("20191107","20191103",DayOfWeek.Sunday,DayOfWeek.Sunday),
    };

    foreach (var @case in testcases) {
      var input = Parse(@case.Item1);
      var expected = Parse(@case.Item2);
      var startDayOfWeek = @case.Item4;
      var dayOfWeek = @case.Item3;
      var result = input.DayInCurrentWeek(dayOfWeek, startDayOfWeek);
      Assert.AreEqual(result, expected);
    }
  }


}