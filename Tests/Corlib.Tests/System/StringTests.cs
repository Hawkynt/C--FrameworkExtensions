using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Corlib.Tests.System {
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

    static IEnumerable<StringComparisonTestData> TestStringsComparison() {
      //TESTS: Matching Strings ignore Case
      yield return new StringComparisonTestData("Test", 't', StringComparison.CurrentCultureIgnoreCase, true);
      yield return new StringComparisonTestData("Test", 't', StringComparison.InvariantCultureIgnoreCase, true);
      yield return new StringComparisonTestData("Test", 't', StringComparison.OrdinalIgnoreCase, true);
      yield return new StringComparisonTestData("Test", 'T', StringComparison.CurrentCultureIgnoreCase, true);
      yield return new StringComparisonTestData("Test", 'T', StringComparison.InvariantCultureIgnoreCase, true);
      yield return new StringComparisonTestData("Test", 'T', StringComparison.OrdinalIgnoreCase, true);

      yield return new StringComparisonTestData("test", 't', StringComparison.CurrentCultureIgnoreCase, true);
      yield return new StringComparisonTestData("test", 't', StringComparison.InvariantCultureIgnoreCase, true);
      yield return new StringComparisonTestData("test", 't', StringComparison.OrdinalIgnoreCase, true);
      yield return new StringComparisonTestData("test", 'T', StringComparison.CurrentCultureIgnoreCase, true);
      yield return new StringComparisonTestData("test", 'T', StringComparison.InvariantCultureIgnoreCase, true);
      yield return new StringComparisonTestData("test", 'T', StringComparison.OrdinalIgnoreCase, true);
      //TESTS: Matching Strings no ignore Case
      yield return new StringComparisonTestData("Test", 't', StringComparison.CurrentCulture, false);
      yield return new StringComparisonTestData("Test", 't', StringComparison.InvariantCulture, false);
      yield return new StringComparisonTestData("Test", 't', StringComparison.Ordinal, false);
      yield return new StringComparisonTestData("Test", 'T', StringComparison.CurrentCulture, true);
      yield return new StringComparisonTestData("Test", 'T', StringComparison.InvariantCulture, true);
      yield return new StringComparisonTestData("Test", 'T', StringComparison.Ordinal, true);

      yield return new StringComparisonTestData("test", 't', StringComparison.CurrentCulture, true);
      yield return new StringComparisonTestData("test", 't', StringComparison.InvariantCulture, true);
      yield return new StringComparisonTestData("test", 't', StringComparison.Ordinal, true);
      yield return new StringComparisonTestData("test", 'T', StringComparison.CurrentCulture, false);
      yield return new StringComparisonTestData("test", 'T', StringComparison.InvariantCulture, false);
      yield return new StringComparisonTestData("test", 'T', StringComparison.Ordinal, false);
      //TESTS: Not Matching Strings ignore Case
      yield return new StringComparisonTestData("Test", 'c', StringComparison.CurrentCultureIgnoreCase, false);
      yield return new StringComparisonTestData("Test", 'c', StringComparison.InvariantCultureIgnoreCase, false);
      yield return new StringComparisonTestData("Test", 'c', StringComparison.OrdinalIgnoreCase, false);
      yield return new StringComparisonTestData("Test", 'C', StringComparison.CurrentCultureIgnoreCase, false);
      yield return new StringComparisonTestData("Test", 'C', StringComparison.InvariantCultureIgnoreCase, false);
      yield return new StringComparisonTestData("Test", 'C', StringComparison.OrdinalIgnoreCase, false);

      yield return new StringComparisonTestData("test", 'c', StringComparison.CurrentCultureIgnoreCase, false);
      yield return new StringComparisonTestData("test", 'c', StringComparison.InvariantCultureIgnoreCase, false);
      yield return new StringComparisonTestData("test", 'c', StringComparison.OrdinalIgnoreCase, false);
      yield return new StringComparisonTestData("test", 'C', StringComparison.CurrentCultureIgnoreCase, false);
      yield return new StringComparisonTestData("test", 'C', StringComparison.InvariantCultureIgnoreCase, false);
      yield return new StringComparisonTestData("test", 'C', StringComparison.OrdinalIgnoreCase, false);
      //TESTS: Not Matching Strings not ignore Case
      yield return new StringComparisonTestData("Test", 'c', StringComparison.CurrentCulture, false);
      yield return new StringComparisonTestData("Test", 'c', StringComparison.InvariantCulture, false);
      yield return new StringComparisonTestData("Test", 'c', StringComparison.Ordinal, false);
      yield return new StringComparisonTestData("Test", 'C', StringComparison.CurrentCulture, false);
      yield return new StringComparisonTestData("Test", 'C', StringComparison.InvariantCulture, false);
      yield return new StringComparisonTestData("Test", 'C', StringComparison.Ordinal, false);

      yield return new StringComparisonTestData("test", 'c', StringComparison.CurrentCulture, false);
      yield return new StringComparisonTestData("test", 'c', StringComparison.InvariantCulture, false);
      yield return new StringComparisonTestData("test", 'c', StringComparison.Ordinal, false);
      yield return new StringComparisonTestData("test", 'C', StringComparison.CurrentCulture, false);
      yield return new StringComparisonTestData("test", 'C', StringComparison.InvariantCulture, false);
      yield return new StringComparisonTestData("test", 'C', StringComparison.Ordinal, false);
    }

    static IEnumerable<StringComparerTestData> TestStringsComparer() {
      //TESTS: Matching Strings ignore Case
      yield return new StringComparerTestData("Test", 't', StringComparer.CurrentCultureIgnoreCase, true);
      yield return new StringComparerTestData("Test", 't', StringComparer.InvariantCultureIgnoreCase, true);
      yield return new StringComparerTestData("Test", 't', StringComparer.OrdinalIgnoreCase, true);
      yield return new StringComparerTestData("Test", 'T', StringComparer.CurrentCultureIgnoreCase, true);
      yield return new StringComparerTestData("Test", 'T', StringComparer.InvariantCultureIgnoreCase, true);
      yield return new StringComparerTestData("Test", 'T', StringComparer.OrdinalIgnoreCase, true);

      yield return new StringComparerTestData("test", 't', StringComparer.CurrentCultureIgnoreCase, true);
      yield return new StringComparerTestData("test", 't', StringComparer.InvariantCultureIgnoreCase, true);
      yield return new StringComparerTestData("test", 't', StringComparer.OrdinalIgnoreCase, true);
      yield return new StringComparerTestData("test", 'T', StringComparer.CurrentCultureIgnoreCase, true);
      yield return new StringComparerTestData("test", 'T', StringComparer.InvariantCultureIgnoreCase, true);
      yield return new StringComparerTestData("test", 'T', StringComparer.OrdinalIgnoreCase, true);
      //TESTS: Matching Strings no ignore Case
      yield return new StringComparerTestData("Test", 't', StringComparer.CurrentCulture, false);
      yield return new StringComparerTestData("Test", 't', StringComparer.InvariantCulture, false);
      yield return new StringComparerTestData("Test", 't', StringComparer.Ordinal, false);
      yield return new StringComparerTestData("Test", 'T', StringComparer.CurrentCulture, true);
      yield return new StringComparerTestData("Test", 'T', StringComparer.InvariantCulture, true);
      yield return new StringComparerTestData("Test", 'T', StringComparer.Ordinal, true);

      yield return new StringComparerTestData("test", 't', StringComparer.CurrentCulture, true);
      yield return new StringComparerTestData("test", 't', StringComparer.InvariantCulture, true);
      yield return new StringComparerTestData("test", 't', StringComparer.Ordinal, true);
      yield return new StringComparerTestData("test", 'T', StringComparer.CurrentCulture, false);
      yield return new StringComparerTestData("test", 'T', StringComparer.InvariantCulture, false);
      yield return new StringComparerTestData("test", 'T', StringComparer.Ordinal, false);
      //TESTS: Not Matching Strings ignore Case
      yield return new StringComparerTestData("Test", 'c', StringComparer.CurrentCultureIgnoreCase, false);
      yield return new StringComparerTestData("Test", 'c', StringComparer.InvariantCultureIgnoreCase, false);
      yield return new StringComparerTestData("Test", 'c', StringComparer.OrdinalIgnoreCase, false);
      yield return new StringComparerTestData("Test", 'C', StringComparer.CurrentCultureIgnoreCase, false);
      yield return new StringComparerTestData("Test", 'C', StringComparer.InvariantCultureIgnoreCase, false);
      yield return new StringComparerTestData("Test", 'C', StringComparer.OrdinalIgnoreCase, false);

      yield return new StringComparerTestData("test", 'c', StringComparer.CurrentCultureIgnoreCase, false);
      yield return new StringComparerTestData("test", 'c', StringComparer.InvariantCultureIgnoreCase, false);
      yield return new StringComparerTestData("test", 'c', StringComparer.OrdinalIgnoreCase, false);
      yield return new StringComparerTestData("test", 'C', StringComparer.CurrentCultureIgnoreCase, false);
      yield return new StringComparerTestData("test", 'C', StringComparer.InvariantCultureIgnoreCase, false);
      yield return new StringComparerTestData("test", 'C', StringComparer.OrdinalIgnoreCase, false);
      //TESTS: Not Matching Strings not ignore Case
      yield return new StringComparerTestData("test", 'c', StringComparer.CurrentCulture, false);
      yield return new StringComparerTestData("test", 'c', StringComparer.InvariantCulture, false);
      yield return new StringComparerTestData("test", 'c', StringComparer.Ordinal, false);
      yield return new StringComparerTestData("test", 'C', StringComparer.CurrentCulture, false);
      yield return new StringComparerTestData("test", 'C', StringComparer.InvariantCulture, false);
      yield return new StringComparerTestData("test", 'C', StringComparer.Ordinal, false);
    }

    [Test]
    [TestCaseSource(nameof(TestStringsComparer))]
    public void StartsWith_CharacterComparer(StringComparerTestData data) {
      Assert.AreEqual(data.ExpectedResult, data.Haystack.StartsWith(data.Needle, data.Comparer));
    }


    [Test]
    [TestCaseSource(nameof(TestStringsComparison))]
    public void StartsWith_CharacterComparison(StringComparisonTestData data) {
      Assert.AreEqual(data.ExpectedResult, data.Haystack.StartsWith(data.Needle, data.Comparison));
    }

    [Test]
    [TestCase(null,true)]
    [TestCase("", true)]
    [TestCase(" ",false)]
    [TestCase("abc",false)]
    public void IsNullOrEmpty(string? toTest,bool expected) {
      Assert.AreEqual(toTest.IsNullOrEmpty(),expected);
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
      Assert.AreEqual(result,expected,"Error during quoted-printable encoding");
      Assert.AreEqual(result.FromQuotedPrintable(),input, "Error during quoted-printable decoding");
    }

  }
}
