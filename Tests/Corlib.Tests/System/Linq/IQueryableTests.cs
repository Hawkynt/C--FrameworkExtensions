using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace System.Linq;

[TestFixture]
public class IQueryableTests {

  private sealed class Dummy(string data) {
    public string Data { get; } = data;
    public string DataReversed => new(this.Data.Reverse().ToArray());
  }

  [Test]
  [TestCase(null, null, false, null, typeof(NullReferenceException))]
  [TestCase("", null, false, "")]
  [TestCase("a", null, false, "a")]
  [TestCase("a|b", null, false, "a|b")]
  [TestCase("a|b", "a", false, "a")]
  [TestCase("a|b", "c", false, "")]
  [TestCase("a b|b c|c d", "b", false, "a b|b c")]
  [TestCase("a b|b c|c d", "b a", false, "a b")]
  [TestCase("a|b", "A", false, "")]
  [TestCase("a|b", "A", true, "a")]
  public void FilterIfNeeded(string? input, string? filter, bool ignoreCase, string? expected, Type? exception = null)
    => ExecuteTest(
        () => ((input == null ? null : ConvertFromStringToTestArray(input)?.Select(s => s == null ? null : new Dummy(s)))?.AsQueryable()).FilterIfNeeded(d => d!.Data, filter, ignoreCase).Select(d => d!.Data),
        ConvertFromStringToTestArray(expected),
        exception
      )
    ;

  [Test]
  [TestCase(null, null, false, null, typeof(NullReferenceException))]
  [TestCase("", null, false, "")]
  [TestCase("a", null, false, "a")]
  [TestCase("a|b", null, false, "a|b")]
  [TestCase("a|b", "a", false, "a")]
  [TestCase("a|b", "c", false, "")]
  [TestCase("a b|b c|c d", "b", false, "a b|b c")]
  [TestCase("a b|b c|c d", "b a", false, "a b")]
  [TestCase("a|b", "A", false, "")]
  [TestCase("a|b", "A", true, "a")]
  [TestCase("ab|bc", "BA", false, "")]
  [TestCase("ab|bc", "ba", false, "ab")]
  [TestCase("ab|bc", "BA", true, "ab")]
  public void FilterIfNeeded2(string? input, string? filter, bool ignoreCase, string? expected, Type? exception = null)
    => ExecuteTest(
      () => ((input == null ? null : ConvertFromStringToTestArray(input)?.Select(s => s == null ? null : new Dummy(s)))?.AsQueryable()).FilterIfNeeded(filter, ignoreCase, d => d!.Data, d=>d!.DataReversed).Select(d => d!.Data),
      ConvertFromStringToTestArray(expected),
      exception
    )
  ;


}