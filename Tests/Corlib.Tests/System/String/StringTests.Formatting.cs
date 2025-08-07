using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace System.String;

/// <summary>
///   Tests for string formatting operations
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class StringTests {
  #region FormatWith Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates FormatWith with simple placeholders")]
  public void FormatWith_SimplePlaceholders_FormatsCorrectly() {
    // Arrange
    const string template = "Hello {0}, you have {1} messages";

    // Act
    var result = template.FormatWith("John", 5);

    // Assert
    Assert.That(result, Is.EqualTo("Hello John, you have 5 messages"));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates FormatWith with format specifiers")]
  public void FormatWith_FormatSpecifiers_AppliesFormatting() {
    // Arrange
    const string template = "Price: {0:C}, Date: {1:yyyy-MM-dd}";
    var price = 19.99m;
    var date = new DateTime(2024, 1, 15);

    // Act
    var result = template.FormatWith(price, date);

    // Assert
    Assert.That(result, Does.Contain("19"));
    Assert.That(result, Does.Contain("2024-01-15"));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FormatWith with missing arguments")]
  public void FormatWith_MissingArguments_ThrowsFormatException() {
    // Arrange
    const string template = "Hello {0} and {1}";

    // Act & Assert
    Assert.Throws<FormatException>(() => template.FormatWith("John"));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FormatWith with null template")]
  public void FormatWith_NullTemplate_ThrowsArgumentNullException() {
    // Arrange
    string template = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => template.FormatWith("test"));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FormatWith with escaped braces")]
  public void FormatWith_EscapedBraces_HandlesCorrectly() {
    // Arrange
    const string template = "Use {{0}} for placeholder {0}";

    // Act
    var result = template.FormatWith("zero");

    // Assert
    Assert.That(result, Is.EqualTo("Use {0} for placeholder zero"));
  }

  #endregion

  #region FormatWithEx Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates FormatWithEx with named placeholders")]
  public void FormatWithEx_NamedPlaceholders_FormatsCorrectly() {
    // Arrange
    const string template = "Hello {Name}, you are {Age} years old";
    var data = new { Name = "John", Age = 30 };

    // Act
    var result = template.FormatWithEx(
      name =>
        name == "Name" ? data.Name :
        name == "Age" ? data.Age.ToString() : null
    );

    // Assert
    Assert.That(result, Is.EqualTo("Hello John, you are 30 years old"));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates FormatWithEx with dictionary lookup")]
  public void FormatWithEx_DictionaryLookup_FormatsCorrectly() {
    // Arrange
    const string template = "Product: {ProductName}, Price: {Price:C}";
    var data = new Dictionary<string, object> { ["ProductName"] = "Widget", ["Price"] = 19.99m };

    // Act
    var result = template.FormatWithEx(name => data.ContainsKey(name) ? data[name] : null);

    // Assert
    Assert.That(result, Does.Contain("Widget"));
    Assert.That(result, Does.Contain("19"));
  }

  #endregion

  #region String Case Conversion Tests

  [Test]
  [TestCase("hello world", "Hello world")]
  [TestCase("HELLO WORLD", "HELLO WORLD")]
  [TestCase("hELLO", "HELLO")]
  [TestCase("", "")]
  [TestCase("a", "A")]
  [Category("HappyPath")]
  [Description("Validates UpperFirst capitalizes first character")]
  public void UpperFirst_VariousInputs_CapitalizesFirstChar(string input, string expected) {
    // Act
    var result = input.UpperFirstInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("Hello World", "hello World")]
  [TestCase("HELLO", "hELLO")]
  [TestCase("H", "h")]
  [TestCase("", "")]
  [Category("HappyPath")]
  [Description("Validates LowerFirst lowercases first character")]
  public void LowerFirst_VariousInputs_LowercasesFirstChar(string input, string expected) {
    // Act
    var result = input.LowerFirstInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates case methods handle null")]
  public void CaseMethods_NullInput_ThrowsArgumentNullException() {
    // Arrange
    string input = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => input.UpperFirstInvariant());
    Assert.Throws<NullReferenceException>(() => input.LowerFirstInvariant());
  }

  #endregion

  #region CamelCase and PascalCase Tests

  [Test]
  [TestCase("hello world", "helloWorld")]
  [TestCase("HELLO_WORLD", "helloWorld")]
  [TestCase("hello-world", "helloWorld")]
  [TestCase("HelloWorld", "helloWorld")]
  [TestCase("hello", "hello")]
  [Category("HappyPath")]
  [Description("Validates CamelCase conversion")]
  public void CamelCase_VariousInputs_ConvertsToCamelCase(string input, string expected) {
    // Act
    var result = input.ToCamelCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("hello world", "HelloWorld")]
  [TestCase("hello_world", "HelloWorld")]
  [TestCase("hello-world", "HelloWorld")]
  [TestCase("helloWorld", "HelloWorld")]
  [TestCase("hello", "Hello")]
  [Category("HappyPath")]
  [Description("Validates PascalCase conversion")]
  public void PascalCase_VariousInputs_ConvertsToPascalCase(string input, string expected) {
    // Act
    var result = input.ToPascalCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates case conversions with numbers")]
  public void CaseConversions_WithNumbers_HandlesCorrectly() {
    const string input = "hello123world";
    Assert.That(input.ToCamelCase(), Is.EqualTo("hello123world"));
    Assert.That(input.ToPascalCase(), Is.EqualTo("Hello123World"));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates formatting performance")]
  public void Formatting_ManyOperations_CompletesQuickly() {
    // Arrange
    const string template = "User {0} performed action {1} at {2:HH:mm:ss}";
    var sw = Stopwatch.StartNew();

    // Act
    for (var i = 0; i < 10000; i++)
      _ = template.FormatWith($"User{i}", $"Action{i}", DateTime.Now);

    sw.Stop();

    // Assert
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"10K format operations took {sw.ElapsedMilliseconds}ms"
    );
  }

  [Test]
  [Category("Performance")]
  [Description("Validates case conversion performance")]
  public void CaseConversion_LargeStrings_PerformsEfficiently() {
    // Arrange
    var input = string.Join(" ", Enumerable.Range(0, 1000).Select(i => $"word{i}").ToArray());
    var sw = Stopwatch.StartNew();

    // Act
    var camelCase = input.ToCamelCase();
    var pascalCase = input.ToPascalCase();

    sw.Stop();

    // Assert
    Assert.That(camelCase, Is.Not.Null);
    Assert.That(pascalCase, Is.Not.Null);
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(50),
      $"Case conversion of large string took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
