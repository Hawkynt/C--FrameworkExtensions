using System.Diagnostics;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace System;

/// <summary>
///   Tests for string parsing extension methods
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class StringTests {
  #region ParseFloat Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates ParseFloat handles standard decimal values")]
  public void ParseFloat_ValidDecimal_ReturnsCorrectValue() {

    // Arrange
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    const string input = "3.14";
    const float expected = 3.14f;

    // Act
    var result = input.ParseFloat();

    // Assert
    Assert.That(result, Is.EqualTo(expected).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates ParseFloat handles scientific notation")]
  public void ParseFloat_ScientificNotation_ReturnsCorrectValue() {
    // Arrange
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    const string input = "1.23e-4";
    const float expected = 0.000123f;

    // Act
    var result = input.ParseFloat();

    // Assert
    Assert.That(result, Is.EqualTo(expected).Within(0.000001f));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ParseFloat handles empty string")]
  public void ParseFloat_EmptyString_ThrowsFormatException() {
    // Arrange
    const string input = "";

    // Act & Assert
    Assert.Throws<FormatException>(() => input.ParseFloat());
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ParseFloat handles whitespace")]
  public void ParseFloat_Whitespace_ThrowsFormatException() {
    // Arrange
    const string input = "   ";

    // Act & Assert
    Assert.Throws<FormatException>(() => input.ParseFloat());
  }

  [Test]
  [Category("Exception")]
  [Description("Validates ParseFloat throws on null input")]
  public void ParseFloat_Null_ThrowsNullReferenceException() {
    // Arrange
    string? input = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => input.ParseFloat());
  }
  
  #endregion

  #region ParseDouble Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates ParseDouble handles standard decimal values")]
  public void ParseDouble_ValidDecimal_ReturnsCorrectValue() {
    // Arrange
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    const string input = "3.14159265359";
    const double expected = 3.14159265359;

    // Act
    var result = input.ParseDouble();

    // Assert
    Assert.That(result, Is.EqualTo(expected).Within(0.0000000001));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ParseDouble handles very large numbers")]
  public void ParseDouble_VeryLargeNumber_ReturnsCorrectValue() {
    // Arrange
    const string input = "1.7976931348623157E+308"; // Near Double.MaxValue
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

    // Act
    var result = input.ParseDouble();

    // Assert
    Assert.That(result, Is.GreaterThan(1E+308));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates ParseDouble throws on null input")]
  public void ParseDouble_Null_ThrowsNullReferenceException() {
    // Arrange
    string? input = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => input.ParseDouble());
  }

  #endregion

  #region ParseInt Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates ParseInt handles positive integers")]
  public void ParseInt_PositiveInteger_ReturnsCorrectValue() {
    // Arrange
    const string input = "12345";
    const int expected = 12345;

    // Act
    var result = input.ParseInt();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates ParseInt handles negative integers")]
  public void ParseInt_NegativeInteger_ReturnsCorrectValue() {
    // Arrange
    const string input = "-12345";
    const int expected = -12345;

    // Act
    var result = input.ParseInt();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ParseInt handles maximum value")]
  public void ParseInt_MaxValue_ReturnsCorrectValue() {
    // Arrange
    var input = int.MaxValue.ToString();

    // Act
    var result = input.ParseInt();

    // Assert
    Assert.That(result, Is.EqualTo(int.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ParseInt handles minimum value")]
  public void ParseInt_MinValue_ReturnsCorrectValue() {
    // Arrange
    var input = int.MinValue.ToString();

    // Act
    var result = input.ParseInt();

    // Assert
    Assert.That(result, Is.EqualTo(int.MinValue));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates parsing performance with multiple iterations")]
  public void ParseMethods_ThousandIterations_CompletesQuickly() {
    // Arrange
    const int iterations = 1000;
    var sw = Stopwatch.StartNew();

    // Act
    for (var i = 0; i < iterations; i++) {
      _ = "123".ParseInt();
      _ = "3.14".ParseFloat();
      _ = "2.71828".ParseDouble();
    }

    sw.Stop();

    // Assert - should complete in under 100ms
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"Parsing {iterations} iterations took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
