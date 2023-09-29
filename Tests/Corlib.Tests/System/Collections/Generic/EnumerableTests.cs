using static Corlib.Tests.NUnit.TestUtilities;
using NUnit.Framework;
using System.Linq;

namespace System.Collections.Generic;

[TestFixture]
public class EnumerableTests {

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsSingle(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsNoSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsNoSingle(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsMultiple(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsNoMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsNoMultiple(), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", false)]
  public void HasSingle(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasSingle(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", true)]
  public void HasNoSingle(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoSingle(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", true)]
  public void HasMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasMultiple(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", false)]
  public void HasNoMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoMultiple(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", false)]
  public void HasSinglePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasSingle(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", true)]
  public void HasNoSinglePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoSingle(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", true)]
  public void HasMultiplePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasMultiple(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", false)]
  public void HasNoMultiplePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoMultiple(s => s == search), expected, exception);
  
  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "a")]
  public void TryGetFirst(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetFirst(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "b")]
  public void TryGetLast(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetLast(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, 0, false, null, typeof(NullReferenceException))]
  [TestCase("", -1, false, null, typeof(IndexOutOfRangeException))]
  [TestCase("a", 1, false, null)]
  [TestCase("a|b|c", 1, true, "b")]
  public void TryGetItem(string? input, int index, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetItem(index, out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a|b", true, "b")]
  [TestCase("b|a", true, "b")]
  public void TryGetMax(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMax(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a|b", true, "a")]
  [TestCase("b|a", true, "a")]
  public void TryGetMin(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMin(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("ab|ba", true, "ab")]
  [TestCase("ba|ab", true, "ab")]
  public void TryGetMaxBy(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMaxBy(i=>i![1], out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("ab|ba", true, "ba")]
  [TestCase("ba|ab", true, "ba")]
  public void TryGetMinBy(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMinBy(i => i![1], out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("a", "b", "b|a")]
  public void Prepend(string? input, string data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Prepend(data).Join("|"), expected, exception)
  ;

  [Test]
  [TestCase(null,null,null,typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("a", "b", "a|b")]
  public void Append(string? input, string data, string? expected, Type? exception = null) 
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Append(data).Join("|"), expected, exception)
    ;

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "",typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "b|a")]
  public void PrependArray(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Prepend(ConvertFromStringToTestArray(data)?.ToArray()).Join("|"), expected, exception)
  ;

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "", typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "a|b")]
  public void AppendArray(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Append(ConvertFromStringToTestArray(data)?.ToArray()).Join("|"), expected, exception)
  ;

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "", typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "b|a")]
  public void PrependEnumerable(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Prepend(ConvertFromStringToTestArray(data)).Join("|"), expected, exception)
  ;

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "", typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "a|b")]
  public void AppendEnumerable(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Append(ConvertFromStringToTestArray(data)).Join("|"), expected, exception)
  ;

  [Test]
  [TestCase(null,null,null,typeof(NullReferenceException))]
  [TestCase("", "", null)]
  [TestCase("", "", "")]
  [TestCase("a", "a", null)]
  [TestCase("a|b", "ab", null)]
  [TestCase("a|b", "ab", "")]
  [TestCase("a|b", "a,b", ",")]
  public void Join(string? input,string? expected,string delimiter=",",Type?exception=null)
    => ExecuteTest(()=>ConvertFromStringToTestArray(input).Join(delimiter),expected,exception)
    ;

  [Test]
  [TestCase(null,null,null,typeof(NullReferenceException))]
  [TestCase(null, null, StringComparison.OrdinalIgnoreCase, typeof(NullReferenceException))]
  [TestCase("", "")]
  [TestCase("", "", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a", "a")]
  [TestCase("a", "a", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a|b", "a|b")]
  [TestCase("a|b", "a|b", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a|a", "a")]
  [TestCase("a|a", "a", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a|A", "a|A")]
  [TestCase("a|A", "a", StringComparison.OrdinalIgnoreCase)]
  public void ToHashSet(string? input,string? expected,StringComparison? comparison=null,Type? exception=null)
    =>ExecuteTest(()=>
      (
        comparison==null
          ? ConvertFromStringToTestArray(input)
        .ToHashSet()
          : ConvertFromStringToTestArray(input)
            .ToHashSet(FromComparison(comparison.Value))
        )
        .OrderBy()
        .Join("|"),
      expected,
      exception
      )
  ;

}