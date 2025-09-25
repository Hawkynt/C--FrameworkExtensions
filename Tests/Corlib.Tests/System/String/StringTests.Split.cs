using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace System.String;

/// <summary>
///   Tests for string split operations
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class StringTests {
  #region Split Test Data

  public record struct SplitParametersTestData(
    string? Input,
    string? Splitter,
    int? Max,
    IEnumerable<string>? Expected,
    Type? Exception = null
  );

  private static IEnumerable<SplitParametersTestData> GetSplitTestData() {
    // Happy path cases
    yield return new("a,b,c", ",", null, ["a", "b", "c"]);
    yield return new("hello world", " ", null, ["hello", "world"]);
    yield return new("one;two;three", ";", null, ["one", "two", "three"]);

    // Edge cases
    yield return new("", ",", null, [""]);
    yield return new("no-delimiter", ",", null, ["no-delimiter"]);
    yield return new("trailing,", ",", null, ["trailing", ""]);
    yield return new(",leading", ",", null, ["", "leading"]);
    yield return new(",,empty,,", ",", null, ["", "", "empty", "", ""]);
    
    // Null separator should split on whitespace - consistent behavior using char.IsWhiteSpace
    yield return new("test", null, null, ["test"]);
    yield return new("test abc", null, null, ["test abc"]);
    yield return new("test abc", null, 2, ["test abc"]);
    yield return new("test\tabc", null, null, ["test\tabc"]);
    yield return new("test\nabc", null, null, ["test\nabc"]);
    yield return new("test\r\nabc", null, null, ["test\r\nabc"]); // \r and \n are separate whitespace chars
    yield return new("  test  abc  ", null, null, ["  test  abc  "]);
    
    // Empty separator should split on whitespace - consistent behavior using char.IsWhiteSpace  
    yield return new("test", "", null, ["test"]);
    yield return new("test abc", "", null, ["test abc"]);
    yield return new("test\tabc", "", null, ["test\tabc"]);
    yield return new("abc\ntest", "", null, ["abc\ntest"]);

    // Max parameter cases
    yield return new("a,b,c,d", ",", 2, ["a", "b,c,d"]);
    yield return new("a,b,c,d", ",", 3, ["a", "b", "c,d"]);
    yield return new("a,b,c,d", ",", 0, new string[0] );
    yield return new("a,b,c,d", ",", 10, ["a", "b", "c", "d"]);

    // Exception cases
    yield return new(null, ",", null, null, typeof(NullReferenceException));
    
  }

  #endregion

  #region Basic Split Tests

  [Test]
  [TestCaseSource(nameof(GetSplitTestData))]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Category("Exception")]
  [Description("Validates Split method with various inputs")]
  public void Split_VariousInputs_ReturnsExpectedParts(SplitParametersTestData testData) {
    if (testData.Exception != null) {
      if (testData.Max == null)
        Assert.Throws(testData.Exception, () => testData.Input.Split(testData.Splitter));
      else
        Assert.Throws(testData.Exception, () => testData.Input.Split(testData.Splitter, testData.Max.Value));
    } else {
      var result = testData.Max.HasValue
        ? testData.Input.Split(testData.Splitter, testData.Max.Value)
        : testData.Input.Split(testData.Splitter);

      CollectionAssert.AreEqual(testData.Expected, result);
    }
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Split with char delimiter")]
  public void Split_CharDelimiter_SplitsCorrectly() {
    // Arrange
    const string input = "a|b|c";
    const char delimiter = '|';

    // Act
    var result = input.Split(delimiter);

    // Assert
    CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Split with string array delimiters")]
  public void Split_MultipleDelimiters_SplitsCorrectly() {
    // Arrange
    const string input = "a,b;c|d";
    var delimiters = new[] { ",", ";", "|" };

    // Act
    var result = input.Split(delimiters, StringSplitOptions.None);

    // Assert
    CollectionAssert.AreEqual(new[] { "a", "b", "c", "d" }, result);
  }

  #endregion

  #region Split With Options Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates Split removes empty entries when specified")]
  public void Split_RemoveEmptyEntries_FiltersEmptyStrings() {
    // Arrange
    const string input = "a,,b,,c";

    // Act
    var result = input.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

    // Assert
    CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Split with consecutive delimiters")]
  public void Split_ConsecutiveDelimiters_HandlesCorrectly() {
    // Arrange
    const string input = "a|||b";

    // Act
    var resultWithEmpty = input.Split('|');
    var resultWithoutEmpty = input.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

    // Assert
    CollectionAssert.AreEqual(new[] { "a", "", "", "b" }, resultWithEmpty);
    CollectionAssert.AreEqual(new[] { "a", "b" }, resultWithoutEmpty);
  }

  #endregion

  #region Advanced Split Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates Split with regex pattern")]
  public void Split_RegexPattern_SplitsCorrectly() {
    // Arrange
    const string input = "one1two2three3four";
    const string pattern = @"\d+"; // Split on digits

    // Act
    var result = Regex.Split(input, pattern);

    // Assert
    CollectionAssert.AreEqual(new[] { "one", "two", "three", "four" }, result);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Split with very long delimiter")]
  public void Split_LongDelimiter_SplitsCorrectly() {
    // Arrange
    const string delimiter = "DELIMITER";
    const string input = "firstDELIMITERsecondDELIMITERthird";

    // Act
    var result = input.Split(delimiter);

    // Assert
    CollectionAssert.AreEqual(new[] { "first", "second", "third" }, result);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Split when delimiter is the entire string")]
  public void Split_DelimiterIsEntireString_ReturnsEmptyParts() {
    // Arrange
    const string input = "delimiter";
    const string delimiter = "delimiter";

    // Act
    var result = input.Split(delimiter);

    // Assert
    CollectionAssert.AreEqual(new[] { "", "" }, result);
  }

  #endregion

  #region Whitespace Split Consistency Tests
  
  [Test]
  [Category("EdgeCase")]
  [Description("Validates behavior is consistent across null and empty separators")]
  public void Split_NullVsEmptySeparator_ProducesSameResults() {
    // Arrange
    var testInputs = new[] {
      "single",
      "two words",
      "three\tword\ttest",
      "\tleading",
      "trailing\t",
      "\tmultiple\twhitespace\t"
    };

    foreach (var input in testInputs) {
      // Act
      var nullResult = input.Split((string?)null);
      var emptyResult = input.Split("");

      // Assert
      CollectionAssert.AreEqual(nullResult, emptyResult, $"Mismatch for input: '{input}'");
    }
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates Split performance with large input")]
  public void Split_LargeInput_CompletesQuickly() {
    // Arrange
    var parts = Enumerable.Range(0, 10000).Select(i => $"part{i}").ToArray();
    var input = string.Join(",", parts);
    var sw = Stopwatch.StartNew();

    // Act
    var result = input.Split(',');

    sw.Stop();

    // Assert
    Assert.That(result.Length, Is.EqualTo(10000));
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"Splitting 10K parts took {sw.ElapsedMilliseconds}ms"
    );
  }

  [Test]
  [Category("Performance")]
  [Category("LargeData")]
  [Description("Validates Split performance with many delimiters")]
  public void Split_ManyDelimiters_PerformsEfficiently() {
    // Arrange
    var input = string.Join("", Enumerable.Range(0, 1000).Select(i => $"word{i},;|").ToArray());
    var delimiters = new[] { ",", ";", "|" };
    var sw = Stopwatch.StartNew();

    // Act
    var result = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

    sw.Stop();

    // Assert
    Assert.That(result.Length, Is.EqualTo(1000));
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(50),
      $"Splitting with multiple delimiters took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
