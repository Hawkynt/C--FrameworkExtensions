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
      () => FromString(input).FilterIfNeeded(d => d!.Data, filter, ignoreCase).AsEnumerable().Select(d => d?.Data),
      ConvertFromStringToTestArray(expected),
      exception
    );

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
      () => FromString(input).FilterIfNeeded(filter, ignoreCase, d => d!.Data, d => d!.DataReversed).AsEnumerable().Select(d => d?.Data),
      ConvertFromStringToTestArray(expected),
      exception
    );

  private static IQueryable<Dummy?>? FromString(string? input) {
    var data = ConvertFromStringToTestArray(input);
    if (data == null)
      return null;

    var dummies = data.Select(s => s == null ? null : new Dummy(s));
    var query = dummies.AsQueryable();
    return query;
  }
}
