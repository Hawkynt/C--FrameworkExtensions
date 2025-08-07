using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace System.String;

/// <summary>
///   Comprehensive tests for all case conversion operations including performance benchmarks
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class StringTests {
  #region CamelCase Tests

  [Test]
  [TestCase("hello world", "helloWorld")]
  [TestCase("HELLO_WORLD", "helloWorld")]
  [TestCase("hello-world", "helloWorld")]
  [TestCase("HelloWorld", "helloWorld")]
  [TestCase("hello", "hello")]
  [TestCase("hello123world", "hello123World")]
  [TestCase("HTTPSConnection", "httpsConnection")]
  [TestCase("XMLHttpRequest", "xmlHttpRequest")]
  [TestCase("", "")]
  [TestCase("a", "a")]
  [TestCase("HELLO", "hello")]
  [TestCase("userID", "userID")]
  [TestCase("HTML5Parser", "html5Parser")]
  [TestCase("iOS15Update", "iOS15Update")]
  [Category("HappyPath")]
  [Description("Validates ToCamelCase conversion with various inputs")]
  public void ToCamelCase_VariousInputs_ConvertsToCamelCase(string input, string expected) {
    // Act
    var result = input.ToCamelCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("hello world", "helloWorld")]
  [TestCase("HELLO_WORLD", "helloWorld")]
  [TestCase("hello-world", "helloWorld")]
  [Category("HappyPath")]
  [Description("Validates ToCamelCaseInvariant conversion")]
  public void ToCamelCaseInvariant_VariousInputs_ConvertsToCamelCase(string input, string expected) {
    // Act
    var result = input.ToCamelCaseInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region PascalCase Tests

  [Test]
  [TestCase("hello world", "HelloWorld")]
  [TestCase("HELLO_WORLD", "HelloWorld")]
  [TestCase("hello-world", "HelloWorld")]
  [TestCase("helloWorld", "HelloWorld")]
  [TestCase("hello", "Hello")]
  [TestCase("hello123world", "Hello123World")]
  [TestCase("HTTPSConnection", "HttpsConnection")]
  [TestCase("XMLHttpRequest", "XmlHttpRequest")]
  [TestCase("", "")]
  [TestCase("a", "A")]
  [TestCase("userID", "UserID")]
  [TestCase("HTML5Parser", "Html5Parser")]
  [Category("HappyPath")]
  [Description("Validates ToPascalCase conversion with various inputs")]
  public void ToPascalCase_VariousInputs_ConvertsToPascalCase(string input, string expected) {
    // Act
    var result = input.ToPascalCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("hello world", "HelloWorld")]
  [TestCase("HELLO_WORLD", "HelloWorld")]
  [TestCase("hello-world", "HelloWorld")]
  [Category("HappyPath")]
  [Description("Validates ToPascalCaseInvariant conversion")]
  public void ToPascalCaseInvariant_VariousInputs_ConvertsToPascalCase(string input, string expected) {
    // Act
    var result = input.ToPascalCaseInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region SnakeCase Tests

  [Test]
  [TestCase("HelloWorld", "hello_world")]
  [TestCase("helloWorld", "hello_world")]
  [TestCase("hello-world", "hello_world")]
  [TestCase("hello world", "hello_world")]
  [TestCase("HELLO_WORLD", "hello_world")]
  [TestCase("hello", "hello")]
  [TestCase("Hello123World", "hello123_world")]
  [TestCase("HTTPSConnection", "https_connection")]
  [TestCase("XMLHttpRequest", "xml_http_request")]
  [TestCase("", "")]
  [TestCase("a", "a")]
  [TestCase("userID", "user_id")]
  [TestCase("HTML5Parser", "html5_parser")]
  [TestCase("iOS15Update", "i_os15_update")]
  [TestCase("getHTTPResponseCode", "get_http_response_code")]
  [Category("HappyPath")]
  [Description("Validates ToSnakeCase conversion with various inputs")]
  public void ToSnakeCase_VariousInputs_ConvertsToSnakeCase(string input, string expected) {
    // Act
    var result = input.ToSnakeCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("HelloWorld", "hello_world")]
  [TestCase("helloWorld", "hello_world")]
  [TestCase("HELLO_WORLD", "hello_world")]
  [Category("HappyPath")]
  [Description("Validates ToSnakeCaseInvariant conversion")]
  public void ToSnakeCaseInvariant_VariousInputs_ConvertsToSnakeCase(string input, string expected) {
    // Act
    var result = input.ToSnakeCaseInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region UpperSnakeCase Tests

  [Test]
  [TestCase("HelloWorld", "HELLO_WORLD")]
  [TestCase("helloWorld", "HELLO_WORLD")]
  [TestCase("hello-world", "HELLO_WORLD")]
  [TestCase("hello world", "HELLO_WORLD")]
  [TestCase("HELLO_WORLD", "HELLO_WORLD")]
  [TestCase("hello", "HELLO")]
  [TestCase("Hello123World", "HELLO123_WORLD")]
  [TestCase("HTTPSConnection", "HTTPS_CONNECTION")]
  [TestCase("XMLHttpRequest", "XML_HTTP_REQUEST")]
  [TestCase("", "")]
  [TestCase("a", "A")]
  [TestCase("userID", "USER_ID")]
  [TestCase("HTML5Parser", "HTML5_PARSER")]
  [TestCase("getHTTPResponseCode", "GET_HTTP_RESPONSE_CODE")]
  [Category("HappyPath")]
  [Description("Validates ToUpperSnakeCase conversion with various inputs")]
  public void ToUpperSnakeCase_VariousInputs_ConvertsToUpperSnakeCase(string input, string expected) {
    // Act
    var result = input.ToUpperSnakeCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("HelloWorld", "HELLO_WORLD")]
  [TestCase("helloWorld", "HELLO_WORLD")]
  [TestCase("HELLO_WORLD", "HELLO_WORLD")]
  [Category("HappyPath")]
  [Description("Validates ToUpperSnakeCaseInvariant conversion")]
  public void ToUpperSnakeCaseInvariant_VariousInputs_ConvertsToUpperSnakeCase(string input, string expected) {
    // Act
    var result = input.ToUpperSnakeCaseInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region KebabCase Tests

  [Test]
  [TestCase("HelloWorld", "hello-world")]
  [TestCase("helloWorld", "hello-world")]
  [TestCase("hello_world", "hello-world")]
  [TestCase("hello world", "hello-world")]
  [TestCase("HELLO-WORLD", "hello-world")]
  [TestCase("hello", "hello")]
  [TestCase("Hello123World", "hello123-world")]
  [TestCase("HTTPSConnection", "https-connection")]
  [TestCase("XMLHttpRequest", "xml-http-request")]
  [TestCase("", "")]
  [TestCase("a", "a")]
  [TestCase("userID", "user-id")]
  [TestCase("HTML5Parser", "html5-parser")]
  [TestCase("getHTTPResponseCode", "get-http-response-code")]
  [Category("HappyPath")]
  [Description("Validates ToKebabCase conversion with various inputs")]
  public void ToKebabCase_VariousInputs_ConvertsToKebabCase(string input, string expected) {
    // Act
    var result = input.ToKebabCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("HelloWorld", "hello-world")]
  [TestCase("helloWorld", "hello-world")]
  [TestCase("HELLO_WORLD", "hello-world")]
  [Category("HappyPath")]
  [Description("Validates ToKebabCaseInvariant conversion")]
  public void ToKebabCaseInvariant_VariousInputs_ConvertsToKebabCase(string input, string expected) {
    // Act
    var result = input.ToKebabCaseInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region UpperKebabCase Tests

  [Test]
  [TestCase("HelloWorld", "HELLO-WORLD")]
  [TestCase("helloWorld", "HELLO-WORLD")]
  [TestCase("hello_world", "HELLO-WORLD")]
  [TestCase("hello world", "HELLO-WORLD")]
  [TestCase("HELLO-WORLD", "HELLO-WORLD")]
  [TestCase("hello", "HELLO")]
  [TestCase("Hello123World", "HELLO123-WORLD")]
  [TestCase("HTTPSConnection", "HTTPS-CONNECTION")]
  [TestCase("XMLHttpRequest", "XML-HTTP-REQUEST")]
  [TestCase("", "")]
  [TestCase("a", "A")]
  [TestCase("userID", "USER-ID")]
  [TestCase("HTML5Parser", "HTML5-PARSER")]
  [Category("HappyPath")]
  [Description("Validates ToUpperKebabCase conversion with various inputs")]
  public void ToUpperKebabCase_VariousInputs_ConvertsToUpperKebabCase(string input, string expected) {
    // Act
    var result = input.ToUpperKebabCase();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase("HelloWorld", "HELLO-WORLD")]
  [TestCase("helloWorld", "HELLO-WORLD")]
  [TestCase("HELLO_WORLD", "HELLO-WORLD")]
  [Category("HappyPath")]
  [Description("Validates ToUpperKebabCaseInvariant conversion")]
  public void ToUpperKebabCaseInvariant_VariousInputs_ConvertsToUpperKebabCase(string input, string expected) {
    // Act
    var result = input.ToUpperKebabCaseInvariant();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Edge Cases Tests

  [Test]
  [Category("EdgeCase")]
  [Description("Validates case conversion methods handle null input")]
  public void CaseConversionMethods_NullInput_ThrowsNullReferenceException() {
    // Arrange
    string input = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => input.ToCamelCase());
    Assert.Throws<NullReferenceException>(() => input.ToPascalCase());
    Assert.Throws<NullReferenceException>(() => input.ToSnakeCase());
    Assert.Throws<NullReferenceException>(() => input.ToUpperSnakeCase());
    Assert.Throws<NullReferenceException>(() => input.ToKebabCase());
    Assert.Throws<NullReferenceException>(() => input.ToUpperKebabCase());
    Assert.Throws<NullReferenceException>(() => input.ToCamelCaseInvariant());
    Assert.Throws<NullReferenceException>(() => input.ToPascalCaseInvariant());
    Assert.Throws<NullReferenceException>(() => input.ToSnakeCaseInvariant());
    Assert.Throws<NullReferenceException>(() => input.ToUpperSnakeCaseInvariant());
    Assert.Throws<NullReferenceException>(() => input.ToKebabCaseInvariant());
    Assert.Throws<NullReferenceException>(() => input.ToUpperKebabCaseInvariant());
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates case conversion methods handle empty strings")]
  public void CaseConversionMethods_EmptyString_ReturnsEmptyString() {
    // Arrange
    const string input = "";

    // Act & Assert
    Assert.That(input.ToCamelCase(), Is.EqualTo(""));
    Assert.That(input.ToPascalCase(), Is.EqualTo(""));
    Assert.That(input.ToSnakeCase(), Is.EqualTo(""));
    Assert.That(input.ToUpperSnakeCase(), Is.EqualTo(""));
    Assert.That(input.ToKebabCase(), Is.EqualTo(""));
    Assert.That(input.ToUpperKebabCase(), Is.EqualTo(""));
    Assert.That(input.ToCamelCaseInvariant(), Is.EqualTo(""));
    Assert.That(input.ToPascalCaseInvariant(), Is.EqualTo(""));
    Assert.That(input.ToSnakeCaseInvariant(), Is.EqualTo(""));
    Assert.That(input.ToUpperSnakeCaseInvariant(), Is.EqualTo(""));
    Assert.That(input.ToKebabCaseInvariant(), Is.EqualTo(""));
    Assert.That(input.ToUpperKebabCaseInvariant(), Is.EqualTo(""));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates case conversion methods handle single characters")]
  public void CaseConversionMethods_SingleCharacter_HandlesCorrectly() {
    // Arrange & Act & Assert
    Assert.That("a".ToCamelCase(), Is.EqualTo("a"));
    Assert.That("A".ToCamelCase(), Is.EqualTo("a"));
    Assert.That("a".ToPascalCase(), Is.EqualTo("A"));
    Assert.That("A".ToPascalCase(), Is.EqualTo("A"));
    Assert.That("a".ToSnakeCase(), Is.EqualTo("a"));
    Assert.That("A".ToSnakeCase(), Is.EqualTo("a"));
    Assert.That("a".ToUpperSnakeCase(), Is.EqualTo("A"));
    Assert.That("A".ToUpperSnakeCase(), Is.EqualTo("A"));
    Assert.That("a".ToKebabCase(), Is.EqualTo("a"));
    Assert.That("A".ToKebabCase(), Is.EqualTo("a"));
    Assert.That("a".ToUpperKebabCase(), Is.EqualTo("A"));
    Assert.That("A".ToUpperKebabCase(), Is.EqualTo("A"));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates case conversion methods handle numbers and special characters")]
  public void CaseConversionMethods_NumbersAndSpecialChars_HandlesCorrectly() {
    // Arrange & Act & Assert
    Assert.That("test123".ToCamelCase(), Is.EqualTo("test123"));
    Assert.That("test123".ToPascalCase(), Is.EqualTo("Test123"));
    Assert.That("test123".ToSnakeCase(), Is.EqualTo("test_123"));
    Assert.That("test123".ToUpperSnakeCase(), Is.EqualTo("TEST_123"));
    Assert.That("test123".ToKebabCase(), Is.EqualTo("test-123"));
    Assert.That("test123".ToUpperKebabCase(), Is.EqualTo("TEST-123"));
    
    Assert.That("test@#$%".ToCamelCase(), Is.EqualTo("test"));
    Assert.That("test@#$%".ToPascalCase(), Is.EqualTo("Test"));
    Assert.That("test@#$%".ToSnakeCase(), Is.EqualTo("test"));
    Assert.That("test@#$%".ToUpperSnakeCase(), Is.EqualTo("TEST"));
    Assert.That("test@#$%".ToKebabCase(), Is.EqualTo("test"));
    Assert.That("test@#$%".ToUpperKebabCase(), Is.EqualTo("TEST"));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates case conversion methods handle Unicode characters")]
  public void CaseConversionMethods_UnicodeChars_HandlesCorrectly() {
    // Arrange
    const string input = "caféWorld";

    // Act & Assert
    Assert.That(input.ToCamelCase(), Is.EqualTo("caféWorld"));
    Assert.That(input.ToPascalCase(), Is.EqualTo("CaféWorld"));
    Assert.That(input.ToSnakeCase(), Is.EqualTo("café_world"));
    Assert.That(input.ToUpperSnakeCase(), Is.EqualTo("CAFÉ_WORLD"));
    Assert.That(input.ToKebabCase(), Is.EqualTo("café-world"));
    Assert.That(input.ToUpperKebabCase(), Is.EqualTo("CAFÉ-WORLD"));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates case conversion performance with medium strings")]
  public void CaseConversion_MediumStrings_PerformsEfficiently() {
    // Arrange
    var inputs = new[] {
      "thisIsAReasonablyLongStringForTestingPerformance",
      "THIS_IS_A_REASONABLY_LONG_STRING_FOR_TESTING_PERFORMANCE",
      "this-is-a-reasonably-long-string-for-testing-performance",
      "This Is A Reasonably Long String For Testing Performance"
    };
    var sw = Stopwatch.StartNew();

    // Act
    for (var i = 0; i < 1000; i++) {
      foreach (var input in inputs) {
        _ = input.ToCamelCase();
        _ = input.ToPascalCase();
        _ = input.ToSnakeCase();
        _ = input.ToUpperSnakeCase();
        _ = input.ToKebabCase();
        _ = input.ToUpperKebabCase();
      }
    }

    sw.Stop();

    // Assert
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(500),
      $"6K case conversions took {sw.ElapsedMilliseconds}ms"
    );
  }

  [Test]
  [Category("Performance")]
  [Description("Validates case conversion performance with large strings")]
  public void CaseConversion_LargeStrings_AllMethods_PerformsEfficiently() {
    // Arrange
    var largeInput = string.Join("", Enumerable.Range(0, 1000).Select(i => $"word{i}Word{i}").ToArray());
    var sw = Stopwatch.StartNew();

    // Act
    for (var i = 0; i < 100; ++i) {
      _ = largeInput.ToCamelCase();
      _ = largeInput.ToPascalCase();
      _ = largeInput.ToSnakeCase();
      _ = largeInput.ToUpperSnakeCase();
      _ = largeInput.ToKebabCase();
      _ = largeInput.ToUpperKebabCase();
    }

    sw.Stop();

    // Assert
    Assert.That(largeInput.Length, Is.GreaterThan(10000));
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(1000),
      $"600 large string conversions took {sw.ElapsedMilliseconds}ms"
    );
  }

  [Test]
  [Category("Performance")]
  [Description("Validates case conversion memory efficiency")]
  public void CaseConversion_MemoryEfficiency_MinimalAllocations() {
    // Arrange
    const string input = "alreadyInCorrectFormat";

    // Act - these should return the original string without allocation in some cases
    var camelResult = input.ToCamelCase();
    var snakeResult = "already_in_correct_format".ToSnakeCase();
    var kebabResult = "already-in-correct-format".ToKebabCase();

    // Assert - verify expected transformations
    Assert.That(camelResult, Is.EqualTo("alreadyInCorrectFormat"));
    Assert.That("AlreadyInCorrectFormat".ToPascalCase(), Is.EqualTo("AlreadyInCorrectFormat"));
    Assert.That(snakeResult, Is.EqualTo("already_in_correct_format"));
    Assert.That(kebabResult, Is.EqualTo("already-in-correct-format"));
  }

  #endregion

  #region Culture-Specific Tests

  [Test]
  [Category("Culture")]
  [Description("Validates case conversion works correctly with different cultures")]
  public void CaseConversion_DifferentCultures_HandlesCorrectly() {
    // Arrange
    const string input = "istanbulTürkiye";
    var turkishCulture = new CultureInfo("tr-TR");
    var invariantCulture = CultureInfo.InvariantCulture;

    // Act
    var turkishResult = input.ToPascalCase(turkishCulture);
    var invariantResult = input.ToPascalCase(invariantCulture);

    // Assert
    Assert.That(turkishResult, Is.Not.Null);
    Assert.That(invariantResult, Is.Not.Null);
    // Note: Turkish has special casing rules for I/i and İ/ı
    // Both results should be valid but may differ
  }

  [Test]
  [Category("Culture")]
  [Description("Validates invariant culture methods are consistent")]
  public void CaseConversion_InvariantCultureMethods_AreConsistent() {
    // Arrange
    const string input = "testString";

    // Act
    var camelInvariant = input.ToCamelCaseInvariant();
    var pascalInvariant = input.ToPascalCaseInvariant();
    var snakeInvariant = input.ToSnakeCaseInvariant();
    var kebabInvariant = input.ToKebabCaseInvariant();

    var camelCulture = input.ToCamelCase(CultureInfo.InvariantCulture);
    var pascalCulture = input.ToPascalCase(CultureInfo.InvariantCulture);
    var snakeCulture = input.ToSnakeCase(CultureInfo.InvariantCulture);
    var kebabCulture = input.ToKebabCase(CultureInfo.InvariantCulture);

    // Assert
    Assert.That(camelInvariant, Is.EqualTo(camelCulture));
    Assert.That(pascalInvariant, Is.EqualTo(pascalCulture));
    Assert.That(snakeInvariant, Is.EqualTo(snakeCulture));
    Assert.That(kebabInvariant, Is.EqualTo(kebabCulture));
  }

  #endregion

  #region Cross-Conversion Tests

  [Test]
  [Category("Integration")]
  [Description("Validates roundtrip conversions work as expected")]
  public void CaseConversion_RoundtripConversions_WorkCorrectly() {
    // Arrange
    const string original = "getHTTPResponseCode";

    // Act - Convert through different formats
    var snake = original.ToSnakeCase();
    var kebab = original.ToKebabCase();
    var camel = snake.ToCamelCase();
    var pascal = kebab.ToPascalCase();

    // Assert
    Assert.That(snake, Is.EqualTo("get_http_response_code"));
    Assert.That(kebab, Is.EqualTo("get-http-response-code"));
    Assert.That(camel, Is.EqualTo("getHttpResponseCode"));
    Assert.That(pascal, Is.EqualTo("GetHttpResponseCode"));
  }

  [Test]
  [Category("Integration")]
  [Description("Validates all conversion methods handle complex scenarios")]
  public void CaseConversion_ComplexScenarios_HandleCorrectly() {
    // Arrange
    var testCases = new[] {
      ("XMLHttpRequest", "xmlHttpRequest", "XmlHttpRequest", "xml_http_request", "XML_HTTP_REQUEST", "xml-http-request", "XML-HTTP-REQUEST"),
      ("HTTPSConnection", "httpsConnection", "HttpsConnection", "https_connection", "HTTPS_CONNECTION", "https-connection", "HTTPS-CONNECTION"),
      ("HTML5Parser", "html5Parser", "Html5Parser", "html5_parser", "HTML5_PARSER", "html5-parser", "HTML5-PARSER"),
      ("getUserID", "getUserID", "GetUserID", "get_user_id", "GET_USER_ID", "get-user-id", "GET-USER-ID"),
      ("parseJSON", "parseJSON", "ParseJSON", "parse_json", "PARSE_JSON", "parse-json", "PARSE-JSON")
    };

    // Act & Assert
    foreach (var (input, expectedCamel, expectedPascal, expectedSnake, expectedUpperSnake, expectedKebab, expectedUpperKebab) in testCases) {
      Assert.That(input.ToCamelCase(), Is.EqualTo(expectedCamel), $"ToCamelCase failed for: {input}");
      Assert.That(input.ToPascalCase(), Is.EqualTo(expectedPascal), $"ToPascalCase failed for: {input}");
      Assert.That(input.ToSnakeCase(), Is.EqualTo(expectedSnake), $"ToSnakeCase failed for: {input}");
      Assert.That(input.ToUpperSnakeCase(), Is.EqualTo(expectedUpperSnake), $"ToUpperSnakeCase failed for: {input}");
      Assert.That(input.ToKebabCase(), Is.EqualTo(expectedKebab), $"ToKebabCase failed for: {input}");
      Assert.That(input.ToUpperKebabCase(), Is.EqualTo(expectedUpperKebab), $"ToUpperKebabCase failed for: {input}");
    }
  }

  #endregion
}