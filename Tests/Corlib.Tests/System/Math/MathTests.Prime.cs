using System.Diagnostics;
using System.Linq;
using Corlib.Tests.TestData;
using NUnit.Framework;
using static System.MathEx;

namespace System.MathExtensionsTests;

/// <summary>
///   Tests for prime number related math operations
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Prime Enumeration Tests

  [Test]
  [Category("HappyPath")]
  [Category("LargeData")]
  [Description("Validates EnumeratePrimes generates correct sequence of 100,000 primes")]
  public void EnumeratePrimes_First100000_MatchesKnownPrimes() {
    var expectedPrimes = PrimeNumbers.GetPrimeArray();
    var actualPrimes = EnumeratePrimes
      .Take(expectedPrimes.Length)
      .ToArray();

    Assert.That(
      actualPrimes.Length,
      Is.EqualTo(expectedPrimes.Length),
      "Generated prime count doesn't match expected count"
    );

    for (var i = 0; i < expectedPrimes.Length; ++i)
      if (actualPrimes[i] != expectedPrimes[i])
        Assert.Fail($"Prime #{i} failed: got {actualPrimes[i]}, expected {expectedPrimes[i]}");

    Assert.Pass($"Successfully validated {expectedPrimes.Length:N0} prime numbers");
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates EnumeratePrimes generates first 100 primes correctly")]
  public void EnumeratePrimes_First100_CorrectSequence() {
    var expectedFirst100 = PrimeNumbers.GetPrimeArray().Take(100).ToArray();
    var actualFirst100 = EnumeratePrimes.Take(100).ToArray();
    CollectionAssert.AreEqual(expectedFirst100, actualFirst100);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates EnumeratePrimes starts with 2")]
  public void EnumeratePrimes_FirstPrime_IsTwo() {
    var firstPrime = EnumeratePrimes.First();
    Assert.That(firstPrime, Is.EqualTo(2UL));
  }

  [Test]
  [Category("Performance")]
  [Description("Validates EnumeratePrimes performance for first 10,000 primes")]
  public void EnumeratePrimes_First10000_CompletesQuickly() {
    var sw = Stopwatch.StartNew();
    var primes = EnumeratePrimes.Take(10000).ToArray();
    sw.Stop();
    Assert.That(primes.Length, Is.EqualTo(10000));
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(10000),
      $"Generating 10,000 primes took {sw.ElapsedMilliseconds}ms, expected < 10s"
    );
  }

  #endregion

  #region IsPrime Tests

  [TestCase(2UL, true)]
  [TestCase(3UL, true)]
  [TestCase(4UL, false)]
  [TestCase(5UL, true)]
  [TestCase(100UL, false)]
  [TestCase(101UL, true)]
  [TestCase(1000UL, false)]
  [TestCase(1009UL, true)]
  [Category("HappyPath")]
  [Description("Validates IsPrime for known prime and non-prime values")]
  public void IsPrime_KnownValues_ReturnsCorrectResult(ulong value, bool expectedIsPrime) {
    // Act
    var actualIsPrime = value.IsPrime();

    // Assert
    Assert.That(actualIsPrime, Is.EqualTo(expectedIsPrime));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsPrime handles 0 and 1 correctly")]
  public void IsPrime_ZeroAndOne_ReturnsFalse() {
    Assert.That(0UL.IsPrime(), Is.False, "0 should not be prime");
    Assert.That(1UL.IsPrime(), Is.False, "1 should not be prime");
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsPrime for large prime numbers")]
  public void IsPrime_LargePrimes_ReturnsTrue() {
    // Test with some large known primes
    Assert.That(1299709UL.IsPrime(), Is.True, "1299709 is prime");
    Assert.That(15485863UL.IsPrime(), Is.True, "15485863 is the millionth prime");
  }

  [Test]
  [Category("Performance")]
  [Description("Validates IsPrime performance for checking many numbers")]
  public void IsPrime_CheckFirst10000Numbers_CompletesQuickly() {
    // Arrange
    var sw = Stopwatch.StartNew();
    var primeCount = 0;

    // Act
    for (ulong i = 0; i < 10000; ++i)
      if (i.IsPrime())
        ++primeCount;

    sw.Stop();

    // Assert
    Assert.That(primeCount, Is.EqualTo(1229), "There are 1229 primes below 10000");
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(5000),
      $"Checking 10000 numbers took {sw.ElapsedMilliseconds}ms, expected < 5s"
    );
  }

  #endregion
}
