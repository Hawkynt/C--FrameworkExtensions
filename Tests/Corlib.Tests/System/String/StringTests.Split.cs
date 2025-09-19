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
    yield return new("a,b,c", ",", null, new[] { "a", "b", "c" });
    yield return new("hello world", " ", null, new[] { "hello", "world" });
    yield return new("one;two;three", ";", null, new[] { "one", "two", "three" });

    // Edge cases
    yield return new("", ",", null, new[] { "" });
    yield return new("no-delimiter", ",", null, new[] { "no-delimiter" });
    yield return new("trailing,", ",", null, new[] { "trailing", "" });
    yield return new(",leading", ",", null, new[] { "", "leading" });
    yield return new(",,empty,,", ",", null, new[] { "", "", "empty", "", "" });
    yield return new("test", null, null, new[] { "test" });
    yield return new("test abc", null, null, new[] { "test", "abc" });
    yield return new("abc\ntest", "", null, new[] { "abc", "test" });

    // Max parameter cases
    yield return new("a,b,c,d", ",", 2, new[] { "a", "b,c,d" });
    yield return new("a,b,c,d", ",", 3, new[] { "a", "b", "c,d" });
    yield return new("a,b,c,d", ",", 0, new string[0] );
    yield return new("a,b,c,d", ",", 10, new[] { "a", "b", "c", "d" });

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
