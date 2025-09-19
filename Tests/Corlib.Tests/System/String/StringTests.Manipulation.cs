using System.Diagnostics;
using NUnit.Framework;

namespace System.String;

/// <summary>
///   Tests for basic string manipulation operations
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class StringTests {
  #region Substring Operations Tests

  [Test]
  [TestCase("Hello World", 2, 6, "llo Wo")]
  [TestCase("Test", 0, 4, "Test")]
  [TestCase("Test", 1, 2, "es")]
  [TestCase("A", 0, 1, "A")]
  [Category("HappyPath")]
  [Description("Validates SubString with start and end indices")]
  public void SubString_StartEnd_ExtractsCorrectSubstring(string input, int start, int end, string expected) {
    // Act
    var result = input.SubString(start, end);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }
  
  [Test]
  [TestCase("Hello World", 5, "Hello")]
  [TestCase("Test", 10, "Test")] // More than length
  [TestCase("", 5, "")]
  [TestCase("A", 1, "A")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates Left extracts characters from start")]
  public void Left_VariousInputs_ExtractsFromStart(string input, int count, string expected) {
    // Act
    var result = input.Left(count);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello World", 5, "World")]
  [TestCase("Test", 10, "Test")] // More than length
  [TestCase("", 5, "")]
  [TestCase("A", 1, "A")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates Right extracts characters from end")]
  public void Right_VariousInputs_ExtractsFromEnd(string input, int count, string expected) {
    // Act
    var result = input.Right(count);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Remove Operations Tests

  [Test]
  [TestCase("Hello World", 2, "llo World")]
  [TestCase("Test", 1, "est")]
  [TestCase("Test", 0, "Test")]
  [TestCase("Test", 10, "")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates RemoveFirst removes characters from start")]
  public void RemoveFirst_VariousInputs_RemovesFromStart(string input, int count, string expected) {
    // Act
    var result = input.RemoveFirst(count);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello World", 2, "Hello Wor")]
  [TestCase("Test", 1, "Tes")]
  [TestCase("Test", 0, "Test")]
  [TestCase("Test", 10, "")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates RemoveLast removes characters from end")]
  public void RemoveLast_VariousInputs_RemovesFromEnd(string input, int count, string expected) {
    // Act
    var result = input.RemoveLast(count);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates Remove methods handle negative counts")]
  public void RemoveMethods_NegativeCount_ThrowsArgumentOutOfRangeException() {
    // Arrange
    const string input = "Hello";

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => input.RemoveFirst(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => input.RemoveLast(-1));
  }

  #endregion

  #region Repeat Tests

  [Test]
  [TestCase("abc", 3, "abcabcabc")]
  [TestCase("X", 5, "XXXXX")]
  [TestCase("", 3, "")]
  [TestCase("test", 1, "test")]
  [TestCase("hi", 0, "")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates Repeat creates repeated strings")]
  public void Repeat_VariousInputs_RepeatsCorrectly(string input, int count, string expected) {
    // Act
    var result = input.Repeat(count);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates Repeat throws on negative count")]
  public void Repeat_NegativeCount_ThrowsArgumentOutOfRangeException() {
    // Arrange
    const string input = "test";

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => input.Repeat(-1));
  }

  #endregion

  #region Exchange/Replace Operations Tests

  [Test]
  [TestCase("Hello World", 6, "Beautiful", "Hello Beautiful")]
  [TestCase("Hello", 5, " World", "Hello World")]
  [Category("HappyPath")]
  [Description("Validates ExchangeAt replaces characters at position")]
  public void ExchangeAt_StringReplacement_ReplacesCorrectly(string input, int index, string replacement, string expected) {
    // Act
    var result = input.ExchangeAt(index, replacement);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello", 1, 'i', "Hillo")]
  [TestCase("Test", 0, 'B', "Best")]
  [TestCase("ABC", 2, 'D', "ABD")]
  [Category("HappyPath")]
  [Description("Validates ExchangeAt with char replacement")]
  public void ExchangeAt_CharReplacement_ReplacesCorrectly(string input, int index, char replacement, string expected) {
    // Act
    var result = input.ExchangeAt(index, replacement);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello World", 6, 5, "Test", "Hello Test")]
  [Category("HappyPath")]
  [Description("Validates ExchangeAt with length parameter")]
  public void ExchangeAt_WithLength_ReplacesRange(string input, int index, int length, string replacement, string expected) {
    // Act
    var result = input.ExchangeAt(index, length, replacement);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates ExchangeAt throws on invalid index")]
  public void ExchangeAt_InvalidIndex_ThrowsIndexOutOfRangeException() {
    // Arrange
    const string input = "Hello";

    // Act & Assert
    Assert.Throws<IndexOutOfRangeException>(() => input.ExchangeAt(-1, "test"));
  }

  #endregion

  #region Trim Operations Tests

  [Test]
  [TestCase("Hello World", "Hello", StringComparison.Ordinal, " World")]
  [TestCase("TestString", "Test", StringComparison.Ordinal, "String")]
  [TestCase("ABC", "XYZ", StringComparison.Ordinal, "ABC")] // No match
  [TestCase("hello world", "HELLO", StringComparison.OrdinalIgnoreCase, " world")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates TrimStart removes from beginning")]
  public void TrimStart_VariousInputs_TrimsFromStart(string input, string what, StringComparison comparison, string expected) {
    // Act
    var result = input.TrimStart(what, comparison);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello World", "World", StringComparison.Ordinal, "Hello ")]
  [TestCase("TestString", "String", StringComparison.Ordinal, "Test")]
  [TestCase("ABC", "XYZ", StringComparison.Ordinal, "ABC")] // No match
  [TestCase("hello WORLD", "world", StringComparison.OrdinalIgnoreCase, "hello ")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates TrimEnd removes from end")]
  public void TrimEnd_VariousInputs_TrimsFromEnd(string input, string what, StringComparison comparison, string expected) {
    // Act
    var result = input.TrimEnd(what, comparison);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Replace Operations Tests

  [Test]
  [TestCase("Hello World", "Hello", "Hi", StringComparison.Ordinal, "Hi World")]
  [TestCase("Test Test Test", "Test", "Best", StringComparison.Ordinal, "Best Test Test")]
  [TestCase("hello world", "HELLO", "Hi", StringComparison.OrdinalIgnoreCase, "Hi world")]
  [TestCase("No Match", "xyz", "abc", StringComparison.Ordinal, "No Match")]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates ReplaceAtStart replaces at beginning")]
  public void ReplaceAtStart_VariousInputs_ReplacesCorrectly(string input, string what, string replacement, StringComparison comparison, string expected) {
    // Act
    var result = input.ReplaceAtStart(what, replacement, comparison);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello World", "World", "Universe", StringComparison.Ordinal, "Hello Universe")]
  [TestCase("test test test", "test", "best", StringComparison.Ordinal, "test test best")]
  [TestCase("hello world", "WORLD", "universe", StringComparison.OrdinalIgnoreCase, "hello universe")]
  [Category("HappyPath")]
  [Description("Validates ReplaceAtEnd replaces at end")]
  public void ReplaceAtEnd_VariousInputs_ReplacesCorrectly(string input, string what, string replacement, StringComparison comparison, string expected) {
    // Act
    var result = input.ReplaceAtEnd(what, replacement, comparison);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates string manipulation performance")]
  public void StringManipulation_ManyOperations_CompletesQuickly() {
    // Arrange
    const string input = "Hello World Testing String Manipulation";
    var sw = Stopwatch.StartNew();

    // Act
    for (var i = 0; i < 10000; i++) {
      _ = input.Left(10);
      _ = input.Right(10);
      _ = input.RemoveFirst(2);
      _ = input.RemoveLast(2);
      _ = input.ExchangeAt(5, 'X');
    }

    sw.Stop();

    // Assert
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"10K string operations took {sw.ElapsedMilliseconds}ms"
    );
  }

  [Test]
  [Category("Performance")]
  [Category("LargeData")]
  [Description("Validates Repeat with large strings")]
  public void Repeat_LargeString_PerformsReasonably() {
    // Arrange
    const string input = "test string for repetition ";
    var sw = Stopwatch.StartNew();

    // Act
    var result = input.Repeat(1000);

    sw.Stop();

    // Assert
    Assert.That(result.Length, Is.EqualTo(input.Length * 1000));
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"Repeating string 1000 times took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
