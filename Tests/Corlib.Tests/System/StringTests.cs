using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Corlib.Tests.System;

[TestFixture]
public class StringTests {
  public struct StringComparisonTestData {
    public string Haystack { get; }
    public char Needle { get; }
    public StringComparison Comparison { get; }
    public bool ExpectedResult { get; }

    public StringComparisonTestData(string haystack, char needle, StringComparison comparison, bool expectedResult) {
      this.Haystack = haystack;
      this.Needle = needle;
      this.Comparison = comparison;
      this.ExpectedResult = expectedResult;
    }
  }

  public struct StringComparerTestData {
    public string Haystack { get; }
    public char Needle { get; }
    public StringComparer Comparer { get; }
    public bool ExpectedResult { get; }

    public StringComparerTestData(string haystack, char needle, StringComparer comparer, bool expectedResult) {
      this.Haystack = haystack;
      this.Needle = needle;
      this.Comparer = comparer;
      this.ExpectedResult = expectedResult;
    }
  }

  private static IEnumerable<StringComparisonTestData> _TestStringsComparison() {
    //TESTS: Matching Strings ignore Case
    yield return new("Test", 't', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparison.OrdinalIgnoreCase, true);
    yield return new("Test", 'T', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparison.OrdinalIgnoreCase, true);

    yield return new("test", 't', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("test", 't', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("test", 't', StringComparison.OrdinalIgnoreCase, true);
    yield return new("test", 'T', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparison.OrdinalIgnoreCase, true);
    //TESTS: Matching Strings no ignore Case
    yield return new("Test", 't', StringComparison.CurrentCulture, false);
    yield return new("Test", 't', StringComparison.InvariantCulture, false);
    yield return new("Test", 't', StringComparison.Ordinal, false);
    yield return new("Test", 'T', StringComparison.CurrentCulture, true);
    yield return new("Test", 'T', StringComparison.InvariantCulture, true);
    yield return new("Test", 'T', StringComparison.Ordinal, true);

    yield return new("test", 't', StringComparison.CurrentCulture, true);
    yield return new("test", 't', StringComparison.InvariantCulture, true);
    yield return new("test", 't', StringComparison.Ordinal, true);
    yield return new("test", 'T', StringComparison.CurrentCulture, false);
    yield return new("test", 'T', StringComparison.InvariantCulture, false);
    yield return new("test", 'T', StringComparison.Ordinal, false);
    //TESTS: Not Matching Strings ignore Case
    yield return new("Test", 'c', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparison.OrdinalIgnoreCase, false);
    yield return new("Test", 'C', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparison.OrdinalIgnoreCase, false);

    yield return new("test", 'c', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparison.OrdinalIgnoreCase, false);
    yield return new("test", 'C', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparison.OrdinalIgnoreCase, false);
    //TESTS: Not Matching Strings not ignore Case
    yield return new("Test", 'c', StringComparison.CurrentCulture, false);
    yield return new("Test", 'c', StringComparison.InvariantCulture, false);
    yield return new("Test", 'c', StringComparison.Ordinal, false);
    yield return new("Test", 'C', StringComparison.CurrentCulture, false);
    yield return new("Test", 'C', StringComparison.InvariantCulture, false);
    yield return new("Test", 'C', StringComparison.Ordinal, false);

    yield return new("test", 'c', StringComparison.CurrentCulture, false);
    yield return new("test", 'c', StringComparison.InvariantCulture, false);
    yield return new("test", 'c', StringComparison.Ordinal, false);
    yield return new("test", 'C', StringComparison.CurrentCulture, false);
    yield return new("test", 'C', StringComparison.InvariantCulture, false);
    yield return new("test", 'C', StringComparison.Ordinal, false);
  }

  private static IEnumerable<StringComparerTestData> _TestStringsComparer() {
    //TESTS: Matching Strings ignore Case
    yield return new("Test", 't', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparer.OrdinalIgnoreCase, true);
    yield return new("Test", 'T', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparer.OrdinalIgnoreCase, true);

    yield return new("test", 't', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("test", 't', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("test", 't', StringComparer.OrdinalIgnoreCase, true);
    yield return new("test", 'T', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparer.OrdinalIgnoreCase, true);
    //TESTS: Matching Strings no ignore Case
    yield return new("Test", 't', StringComparer.CurrentCulture, false);
    yield return new("Test", 't', StringComparer.InvariantCulture, false);
    yield return new("Test", 't', StringComparer.Ordinal, false);
    yield return new("Test", 'T', StringComparer.CurrentCulture, true);
    yield return new("Test", 'T', StringComparer.InvariantCulture, true);
    yield return new("Test", 'T', StringComparer.Ordinal, true);

    yield return new("test", 't', StringComparer.CurrentCulture, true);
    yield return new("test", 't', StringComparer.InvariantCulture, true);
    yield return new("test", 't', StringComparer.Ordinal, true);
    yield return new("test", 'T', StringComparer.CurrentCulture, false);
    yield return new("test", 'T', StringComparer.InvariantCulture, false);
    yield return new("test", 'T', StringComparer.Ordinal, false);
    //TESTS: Not Matching Strings ignore Case
    yield return new("Test", 'c', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparer.OrdinalIgnoreCase, false);
    yield return new("Test", 'C', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparer.OrdinalIgnoreCase, false);

    yield return new("test", 'c', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparer.OrdinalIgnoreCase, false);
    yield return new("test", 'C', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparer.OrdinalIgnoreCase, false);
    //TESTS: Not Matching Strings not ignore Case
    yield return new("test", 'c', StringComparer.CurrentCulture, false);
    yield return new("test", 'c', StringComparer.InvariantCulture, false);
    yield return new("test", 'c', StringComparer.Ordinal, false);
    yield return new("test", 'C', StringComparer.CurrentCulture, false);
    yield return new("test", 'C', StringComparer.InvariantCulture, false);
    yield return new("test", 'C', StringComparer.Ordinal, false);
  }

  [Test]
  [TestCaseSource(nameof(_TestStringsComparer))]
  public void StartsWith_CharacterComparer(StringComparerTestData data) {
    Assert.AreEqual(data.ExpectedResult, data.Haystack.StartsWith(data.Needle, data.Comparer));
  }


  [Test]
  [TestCaseSource(nameof(_TestStringsComparison))]
  public void StartsWith_CharacterComparison(StringComparisonTestData data) {
    Assert.AreEqual(data.ExpectedResult, data.Haystack.StartsWith(data.Needle, data.Comparison));
  }

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase(" ", false)]
  [TestCase("abc", false)]
  public void IsNullOrEmpty(string? toTest, bool expected) {
    Assert.AreEqual(toTest.IsNullOrEmpty(), expected);
  }

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase(" ", true)]
  [TestCase("abc", false)]
  public void IsNullOrWhitespace(string? toTest, bool expected) {
    Assert.AreEqual(toTest.IsNullOrWhiteSpace(), expected);
  }

  [Test]
  [TestCase("", "")]
  [TestCase("abc", "abc")]
  [TestCase("a\nb", "a=0Ab")]
  [TestCase("a=b", "a=3Db")]
  public void QuotedPrintable(string input, string expected) {
    var result = input.ToQuotedPrintable();
    Assert.AreEqual(result, expected, "Error during quoted-printable encoding");
    Assert.AreEqual(result.FromQuotedPrintable(), input, "Error during quoted-printable decoding");
  }

  [Test]
  [TestCase(null, 0, null, null, typeof(NullReferenceException))]
  [TestCase("", 1, "a", "a")]
  [TestCase("ABC", -1, "a", "a", typeof(ArgumentOutOfRangeException))]
  [TestCase("ABC", 0, "a", "a", typeof(ArgumentOutOfRangeException))]
  [TestCase("ABC", 1, null, "A")]
  [TestCase("ABC", 1, "a", "Aa")]
  [TestCase("ABC", 10, "a", "ABCa")]
  public void ExchangeAt_WithFullStringReplacement(string input, int index, string replacement, string expected, Type? exception = null) {
    if (exception == null)
      Assert.That(input.ExchangeAt(index, replacement), Is.EqualTo(expected));
    else
      Assert.That(() => input.ExchangeAt(index, replacement), Throws.TypeOf(exception));
  }

  [Test]
  [TestCase(null, 0, 'a', "a", typeof(NullReferenceException))]
  [TestCase("ABC", -1, 'a', "aABC", typeof(ArgumentOutOfRangeException))]
  [TestCase("ABC", 0, 'a', "aBC")]
  [TestCase("", 0, 'a', "a")]
  [TestCase("A", 0, 'a', "a")]
  [TestCase("A", 1, 'a', "Aa")]
  [TestCase("ABC", 1, 'a', "AaC")]
  [TestCase("ABC", 2, 'a', "ABa")]
  [TestCase("ABC", 10, 'a', "ABCa")]
  public void ExchangeAt_WithSingleCharacterReplacement(string input, int index, char replacement, string expected, Type? exception = null) {
    if (exception == null)
      Assert.That(input.ExchangeAt(index, replacement), Is.EqualTo(expected));
    else
      Assert.That(() => input.ExchangeAt(index, replacement), Throws.TypeOf(exception));
  }

  [Test]
  [TestCase(null, 0, 0, null, null, typeof(NullReferenceException))]
  [TestCase("A", -1, 0, null, null, typeof(ArgumentOutOfRangeException))]
  [TestCase("A", 0, 0, null, null, typeof(ArgumentOutOfRangeException))]
  [TestCase("", 0, 1, "a", "a")]
  [TestCase("ABC", 0, 1, "a", "aBC")]
  [TestCase("ABC", 0, 1, "aa", "aaBC")]
  [TestCase("ABC", 0, 2, "a", "aC")]
  [TestCase("ABC", 2, 1, "a", "ABa")]
  [TestCase("ABC", 2, 2, "a", "ABa")]
  [TestCase("ABC", 3, 1, "a", "ABCa")]
  [TestCase("ABC", 3, 1, "aa", "ABCaa")]
  public void ExchangeAt_WithStringCountReplacement(string input, int index, int length, string replacement, string expected, Type? exception = null) {
    if (exception == null)
      Assert.That(input.ExchangeAt(index, length, replacement), Is.EqualTo(expected));
    else
      Assert.That(() => input.ExchangeAt(index, length, replacement), Throws.TypeOf(exception));
  }

  [Test]
  [TestCase(null,null,false,typeof(NullReferenceException))]
  [TestCase("a", null, false, typeof(ArgumentNullException))]
  [TestCase("a", "", false)]
  [TestCase("a", "a,b", true)]
  [TestCase("a", "b,c", false)]
  public void IsIn(string needle, string? haystack, bool expected, Type? exception = null) {
    var hay = haystack?.Split(',').Select(i => i.Trim());
    if(exception==null)
      Assert.That(needle.IsIn(hay), Is.EqualTo(expected));
    else
      Assert.That(()=>needle.IsIn(hay), Throws.TypeOf(exception));
  }

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", null, false, typeof(ArgumentNullException))]
  [TestCase("a", "", true)]
  [TestCase("a", "a,b", false)]
  [TestCase("a", "b,c", true)]
  public void IsNotIn(string needle, string? haystack, bool expected, Type? exception = null) {
    var hay = haystack?.Split(',').Select(i => i.Trim());
    if (exception == null)
      Assert.That(needle.IsNotIn(hay), Is.EqualTo(expected));
    else
      Assert.That(() => needle.IsNotIn(hay), Throws.TypeOf(exception));
  }


}