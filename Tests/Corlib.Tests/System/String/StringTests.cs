using System.Diagnostics;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace System.String;

/// <summary>
///   Core string extension tests
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class StringTests {
  #region Null and Empty Checks Tests

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase("   ", false)]
  [TestCase("test", false)]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates IsNullOrEmpty extension method")]
  public void IsNullOrEmpty_VariousInputs_ReturnsCorrectResult(string input, bool expected) {
    // Act
    var result = input.IsNullOrEmpty();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase("   ", true)]
  [TestCase("  \t\n  ", true)]
  [TestCase("test", false)]
  [TestCase(" a ", false)]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates IsNullOrWhitespace extension method")]
  public void IsNullOrWhitespace_VariousInputs_ReturnsCorrectResult(string input, bool expected) {
    // Act
    var result = input.IsNullOrWhiteSpace();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(null, false)]
  [TestCase("", false)]
  [TestCase("   ", true)]
  [TestCase("test", true)]
  [Category("HappyPath")]
  [Description("Validates IsNotNullOrEmpty extension method")]
  public void IsNotNullOrEmpty_VariousInputs_ReturnsCorrectResult(string input, bool expected) {
    // Act
    var result = input.IsNotNullOrEmpty();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Contains and Comparison Tests

  [Test]
  [TestCase("Hello World", "World", StringComparison.Ordinal, true)]
  [TestCase("Hello World", "world", StringComparison.Ordinal, false)]
  [TestCase("Hello World", "world", StringComparison.OrdinalIgnoreCase, true)]
  [TestCase("Hello World", "xyz", StringComparison.Ordinal, false)]
  [TestCase("", "test", StringComparison.Ordinal, false)]
  [TestCase("test", "", StringComparison.Ordinal, true)]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates Contains with string comparison")]
  public void Contains_WithComparison_ReturnsCorrectResult(string input, string what, StringComparison comparison, bool expected) {
    // Act
    var result = input.Contains(what, comparison);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates Contains throws on null haystack")]
  public void Contains_NullHaystack_ThrowsArgumentNullException() {
    // Arrange
    string input = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => input.Contains("test", StringComparison.Ordinal));
  }

  #endregion

  #region IsAnyOf Tests

  [Test]
  [TestCase("apple", "apple|banana|cherry", true)]
  [TestCase("grape", "apple|banana|cherry", false)]
  [TestCase("APPLE", "apple|banana|cherry", false)] // Case sensitive
  [TestCase("", "apple||cherry", true)] // Empty in list
  [TestCase("test", "", false)] // Empty list
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates IsAnyOf checks membership in pipe-separated list")]
  public void IsAnyOf_PipeSeparatedList_ReturnsCorrectResult(string input, string needles, bool expected) {
    // Act
    var result = input.IsAnyOf(needles.Split('|'));

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("APPLE", "apple|banana|cherry", StringComparison.OrdinalIgnoreCase, true)]
  [TestCase("Apple", "apple|banana|cherry", StringComparison.OrdinalIgnoreCase, true)]
  [TestCase("grape", "apple|banana|cherry", StringComparison.OrdinalIgnoreCase, false)]
  [Category("HappyPath")]
  [Description("Validates IsAnyOf with comparison parameter")]
  public void IsAnyOf_WithComparison_ReturnsCorrectResult(string input, string needles, StringComparison comparison, bool expected) {
    // Act
    var result = input.IsAnyOf(needles.Split('|'), comparison);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region StartsWith Tests

  [Test]
  [TestCase("Hello World", "Hello|Hi|Hey", true)]
  [TestCase("Good morning", "Hello|Hi|Hey", false)]
  [TestCase("Hi there", "Hello|Hi|Hey", true)]
  [TestCase("", "Hello|Hi", false)]
  [TestCase("Hello", "", false)] // Empty needles
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates StartsWithAnyOf for strings")]
  public void StartsWithAnyOf_Strings_ReturnsCorrectResult(string input, string needles, bool expected) {
    // Act
    var result = input.StartsWithAny(needles.Split('|'));

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello World", "HhGg", true)] // Starts with 'H'
  [TestCase("Good morning", "HhGg", true)] // Starts with 'G'  
  [TestCase("Bye", "HhGg", false)]
  [TestCase("", "Hh", false)]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates StartsWithAnyOf for characters")]
  public void StartsWithAnyOf_Characters_ReturnsCorrectResult(string input, string needleChars, bool expected) {
    // Act
    var result = input.StartsWithAny(needleChars.ToCharArray());

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region File Name Sanitization Tests

  [Test]
  [TestCase("normal file.txt", "normal file.txt")]
  [TestCase("file<with>bad:chars.txt", "file_with_bad_chars.txt")]
  [TestCase("file|with|pipes.txt", "file_with_pipes.txt")]
  [TestCase("CON.txt", "_CON.txt")] // Reserved name
  [TestCase("file?.txt", "file_.txt")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates SanitizeForFileName cleans invalid characters")]
  public void SanitizeForFileName_VariousInputs_SanitizesCorrectly(string input, string expected) {
    // Act
    var result = input.SanitizeForFileName();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates SanitizeForFileName with custom sanitization character")]
  public void SanitizeForFileName_CustomSanitationChar_UsesCustomChar() {
    // Arrange
    const string input = "file<with>bad:chars.txt";
    const char sanitationChar = '-';

    // Act
    var result = input.SanitizeForFileName(sanitationChar);

    // Assert
    Assert.That(result, Is.EqualTo("file-with-bad-chars.txt"));
  }

  #endregion

  #region Pattern Matching Tests

  [Test]
  [TestCase("file.txt", "*.txt", true)]
  [TestCase("document.pdf", "*.txt", false)]
  [TestCase("test.doc", "test.*", true)]
  [TestCase("readme", "*", true)]
  [TestCase("file.backup.txt", "*.backup.*", true)]
  [Category("HappyPath")]
  [Description("Validates MatchesFilePattern with wildcard patterns")]
  public void MatchesFilePattern_WildcardPatterns_ReturnsCorrectResult(string input, string pattern, bool expected) {
    // Act
    var result = input.MatchesFilePattern(pattern);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("test123", @"\d+", true)]
  [TestCase("no numbers", @"\d+", false)]
  [TestCase("email@domain.com", @"\w+@\w+\.\w+", true)]
  [TestCase("invalid-email", @"\w+@\w+\.\w+", false)]
  [Category("HappyPath")]
  [Description("Validates IsMatch with regex patterns")]
  public void IsMatch_RegexPatterns_ReturnsCorrectResult(string input, string pattern, bool expected) {
    // Act
    var result = input.IsMatch(pattern);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("TEST123", @"test\d+", RegexOptions.IgnoreCase, true)]
  [TestCase("TEST123", @"test\d+", RegexOptions.None, false)]
  [Category("HappyPath")]
  [Description("Validates IsMatch with regex options")]
  public void IsMatch_WithRegexOptions_ReturnsCorrectResult(string input, string pattern, RegexOptions options, bool expected) {
    // Act
    var result = input.IsMatch(pattern, options);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates core string operations performance")]
  public void CoreStringOperations_ManyIterations_CompletesQuickly() {
    // Arrange
    const string input = "Hello World Testing Performance";
    var sw = Stopwatch.StartNew();

    // Act
    for (var i = 0; i < 50000; i++) {
      _ = input.IsNullOrEmpty();
      _ = input.IsNullOrWhiteSpace();
      _ = input.Contains("World", StringComparison.Ordinal);
      _ = input.IsAnyOf("Hello|Test|World");
      _ = input.StartsWithAny("H", "T", "W");
    }

    sw.Stop();

    // Assert
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"50K core string operations took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
