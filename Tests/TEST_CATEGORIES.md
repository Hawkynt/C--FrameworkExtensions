# Test Categories Definition

## Primary Categories (Test Type)

These define WHAT kind of test it is:

```csharp
[Category("Unit")]           // Isolated, single method/class tests
[Category("Integration")]    // Tests involving multiple components
[Category("EndToEnd")]      // Full workflow tests
[Category("Performance")]    // Benchmarks and timing tests
[Category("Regression")]     // Tests that validate specific bug fixes
```

## Secondary Categories (Test Scenario)

These define HOW the test exercises the code:

```csharp
[Category("HappyPath")]      // Normal, expected usage
[Category("EdgeCase")]       // Boundary conditions, limits, ±1 checks
[Category("Exception")]      // Error handling and exceptions
```

## Additional Descriptive Categories

```csharp
[Category("CultureSensitive")]  // Tests affected by culture/locale
[Category("LargeData")]        // Tests with significant memory/CPU usage
[Category("SlowTest")]         // Tests that take >1 second
[Category("PlatformSpecific")] // Windows/Linux/Mac specific tests
```

## Usage Examples

### Basic Unit Test
```csharp
[Test]
[Category("Unit")]
[Category("HappyPath")]
public void Split_CommaSeparatedString_ReturnsExpectedParts()
{
    var result = "a,b,c".Split(',');
    Assert.That(result, Is.EqualTo(new[] { "a", "b", "c" }));
}
```

### Edge Case Test
```csharp
[Test]
[Category("Unit")]
[Category("EdgeCase")]
public void Split_EmptyString_ReturnsEmptyArray()
{
    var result = "".Split(',');
    Assert.That(result, Is.EqualTo(new[] { "" }));
}
```

### Exception Test
```csharp
[Test]
[Category("Unit")]
[Category("Exception")]
public void Split_NullString_ThrowsArgumentNullException()
{
    string input = null;
    Assert.Throws<ArgumentNullException>(() => input.Split(','));
}
```

### Performance Test
```csharp
[Test]
[Category("Performance")]
[Category("LargeData")]
public void Split_OneMillionCharacterString_CompletesUnder100ms()
{
    var input = new string('a', 1_000_000);
    var sw = Stopwatch.StartNew();
    
    var result = input.Split('b');
    
    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100));
}
```

### Regression Test
```csharp
[Test]
[Category("Regression")]
[Category("EdgeCase")]
[Description("Fix for issue #123: RemoveFirstLines corrupted data with AutoDetect")]
public void RemoveFirstLines_AutoDetectWithSpecificContent_PreservesCorrectData()
{
    // Test that validates the fix
}
```

## Test Execution Filters

```bash
# Development - Fast feedback
dotnet test --filter "Category=Unit&Category=HappyPath"

# Pre-commit - All unit tests
dotnet test --filter "Category=Unit"

# CI Pipeline - All except slow/performance
dotnet test --filter "Category!=Performance&Category!=SlowTest"

# Nightly - Everything including performance
dotnet test

# Bug validation - Regression tests only
dotnet test --filter "Category=Regression"

# Edge case validation
dotnet test --filter "Category=EdgeCase|Category=Exception"
```

## File Organization by Category

```
Tests/
+-- Unit/                    # Fast, isolated tests
¦   +-- String/
¦   ¦   +-- StringTests.cs
¦   ¦   +-- StringTests.Parsing.cs
¦   ¦   +-- StringTests.Formatting.cs
¦   +-- Math/
¦       +-- MathTests.cs
¦       +-- MathTests.Bitwise.cs
+-- Integration/            # Multi-component tests
¦   +-- IO/
¦       +-- FileSystemIntegrationTests.cs
+-- Performance/            # Performance benchmarks
¦   +-- StringPerformanceTests.cs
¦   +-- MathPerformanceTests.cs
+-- Regression/            # Bug fix validation
    +-- RemoveFirstLinesRegressionTests.cs
```