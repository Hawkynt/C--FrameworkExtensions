using System;
using NUnit.Framework;
using System.Linq;
using static Corlib.Tests.NUnit.TestUtilities;

namespace Corlib.Tests.System.Linq;

[TestFixture]
public class IQueryableTests {

  private class Dummy {
    public Dummy(string data) => this.Data = data;
    
    public string Data { get; }
    public string DataReversed => new(this.Data.Reverse().ToArray());
  }

  [Test]
  [TestCase(null, null,  null, typeof(NullReferenceException))]
  [TestCase("", null,  "")]
  [TestCase("a", null,  "a")]
  [TestCase("a|b", null,  "a|b")]
  [TestCase("a|b", "a",  "a")]
  [TestCase("a|b", "c",  "")]
  [TestCase("a b|b c|c d", "b",  "a b|b c")]
  [TestCase("a b|b c|c d", "b a",  "a b")]
  [TestCase("a|b", "A",  "")]
  public void FilterIfNeeded(string? input, string? filter, string? expected, Type? exception = null)
    => ExecuteTest(
        () => ((input == null ? null : ConvertFromStringToTestArray(input)?.Select(s => s == null ? null : new Dummy(s)))?.AsQueryable()).FilterIfNeeded(d => d.Data, filter).Select(d => d.Data),
        ConvertFromStringToTestArray(expected),
        exception
      )
    ;

}