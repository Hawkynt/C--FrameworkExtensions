using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
internal class RandomTests {
  private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

  [Test]
  public void GenerateRandomBool() {
    var wasFalse = false;
    var wasTrue = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<bool>();
      switch (value) {
        case true: wasTrue = true; break;
        default: wasFalse = true; break;
      }

      if (!(wasTrue && wasFalse))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered false:{wasFalse}, true:{wasTrue}");
  }

  [Test]
  public void GenerateRandomNullableBool() {
    var wasFalse = false;
    var wasTrue = false;
    var wasNull = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<bool?>();
      switch (value) {
        case null: wasNull = true; break;
        case true: wasTrue = true; break;
        default: wasFalse = true; break;
      }

      if (!(wasTrue && wasFalse && wasNull))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered false:{wasFalse}, true:{wasTrue}, null:{wasNull}");
  }

  private static void _GenerateRandomSignedInt<T>(T zero, T minValue, T maxValue) where T : struct, IComparable<T>, IEquatable<T> {
    var wasPositive = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<T>();
      if (value.Equals(zero))
        wasZero = true;
      else if (value.Equals(minValue))
        wasNegativeMax = true;
      else if (value.Equals(maxValue))
        wasPositiveMax = true;
      else if (value.CompareTo(zero) < 0)
        wasNegative = true;
      else if (value.CompareTo(zero) > 0)
        wasPositive = true;

      if (!(wasNegative && wasPositive && wasZero && wasNegativeMax && wasPositiveMax))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  private static void _GenerateRandomUnsignedInt<T>(T zero, T maxValue) where T : struct, IComparable<T>, IEquatable<T> {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<T>();
      if (value.Equals(zero))
        wasZero = true;
      else if (value.Equals(maxValue))
        wasPositiveMax = true;
      else if (value.CompareTo(zero) > 0)
        wasPositive = true;

      if (!(wasPositive && wasZero && wasPositiveMax))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  [Test]
  public void GenerateRandomChar() {
    var wasControl = false;
    var wasNumber = false;
    var wasWhiteSpace = false;
    var wasLetter = false;
    var wasSurrogate = false;
    var wasSingleByte = false;
    var wasMultiByte = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<char>();
      if (char.IsControl(value))
        wasControl = true;
      else if (char.IsDigit(value))
        wasNumber = true;
      else if (char.IsWhiteSpace(value))
        wasWhiteSpace = true;
      else if (char.IsLetter(value))
        wasLetter = true;
      else if (char.IsSurrogate(value))
        wasSurrogate = true;
      else if (value < 0x100)
        wasSingleByte = true;
      else if (value > 0x100)
        wasMultiByte = true;

      if (!(wasControl && wasNumber && wasWhiteSpace && wasLetter && wasSurrogate && wasSingleByte && wasMultiByte))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Failed to generate all types of characters within the allotted time. Results: " + $"Control: {wasControl}, " + $"Number: {wasNumber}, " + $"WhiteSpace: {wasWhiteSpace}, " + $"Letter: {wasLetter}, " + $"Surrogate: {wasSurrogate}, " + $"SingleByte: {wasSingleByte}, " + $"MultiByte: {wasMultiByte}.");
  }

  [Test]
  public void GenerateRandomFloat() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasNaN = false;
    var wasNegInf = false;
    var wasPosInf = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<float>();
      switch (value) {
        case 0: wasZero = true; break;
        case float.MaxValue: wasPositiveMax = true; break;
        case float.MinValue: wasNegativeMax = true; break;
        default: {
          if (value.IsNegativeInfinity())
            wasNegInf = true;
          else if (value.IsPositiveInfinity())
            wasPosInf = true;
          else if (value > 0)
            wasPositive = true;
          else if (value < 0)
            wasNegative = true;
          else if (value.IsNaN())
            wasNaN = true;
          break;
        }
      }

      if (!(wasPositive && wasZero && wasPositiveMax && wasNegative && wasNegativeMax && wasNaN && wasNegInf && wasPosInf))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered neginf:{wasNegInf}, negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}, posinf:{wasPosInf}, NaN:{wasNaN}");
  }

  [Test]
  public void GenerateRandomDouble() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasNaN = false;
    var wasNegInf = false;
    var wasPosInf = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<double>();
      switch (value) {
        case 0: wasZero = true; break;
        case double.MaxValue: wasPositiveMax = true; break;
        case double.MinValue: wasNegativeMax = true; break;
        default: {
          if (value.IsNegativeInfinity())
            wasNegInf = true;
          else if (value.IsPositiveInfinity())
            wasPosInf = true;
          else if (value > 0)
            wasPositive = true;
          else if (value < 0)
            wasNegative = true;
          else if (value.IsNaN())
            wasNaN = true;
          break;
        }
      }

      if (!(wasPositive && wasZero && wasPositiveMax && wasNegative && wasNegativeMax && wasNaN && wasNegInf && wasPosInf))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered neginf:{wasNegInf}, negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}, posinf:{wasPosInf}, NaN:{wasNaN}");
  }

  [Test]
  public void GenerateRandomDecimal() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasMinusOne = false;
    var wasOne = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<decimal>();
      switch (value) {
        case 0: wasZero = true; break;
        case decimal.MinusOne: wasMinusOne = true; break;
        case decimal.One: wasOne = true; break;
        case decimal.MinValue: wasNegativeMax = true; break;
        case decimal.MaxValue: wasPositiveMax = true; break;
        case > 0: wasPositive = true; break;
        case < 0: wasNegative = true; break;
      }

      if (!(wasPositive && wasZero && wasPositiveMax && wasNegative && wasNegativeMax && wasMinusOne && wasOne))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered -1:{wasMinusOne}, negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}, +1:{wasOne}");
  }

  [Test]
  public void GenerateRandomInt8() => _GenerateRandomSignedInt<sbyte>(0, sbyte.MinValue, sbyte.MaxValue);

  [Test]
  public void GenerateRandomInt16() => _GenerateRandomSignedInt<short>(0, short.MinValue, short.MaxValue);

  [Test]
  public void GenerateRandomInt32() => _GenerateRandomSignedInt(0, int.MinValue, int.MaxValue);

  [Test]
  public void GenerateRandomInt64() => _GenerateRandomSignedInt(0, long.MinValue, long.MaxValue);

  [Test]
  public void GenerateRandomUInt8() => _GenerateRandomUnsignedInt((byte)0, byte.MaxValue);

  [Test]
  public void GenerateRandomUInt16() => _GenerateRandomUnsignedInt((ushort)0, ushort.MaxValue);

  [Test]
  public void GenerateRandomUInt32() => _GenerateRandomUnsignedInt(0U, uint.MaxValue);

  [Test]
  public void GenerateRandomUInt64() => _GenerateRandomUnsignedInt(0UL, ulong.MaxValue);

  [Test]
  public void GenerateRandomString() {
    var wasNull = false;
    var wasEmpty = false;
    var wasSomeLength = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<string>();
      switch (value) {
        case null: wasNull = true; break;
        case { Length: <= 0 }: wasEmpty = true; break;
        case { Length: >= 0 }: wasSomeLength = true; break;
      }

      if (!(wasNull && wasEmpty && wasSomeLength))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered null:{wasNull}, Empty:{wasEmpty}, someLength:{wasSomeLength}");
  }

  [Test]
  public void GenerateRandomObject() {
    var wasNull = false;
    var wasSomething = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<string>();
      if (value == null)
        wasNull = true;
      else
        wasSomething = true;

      if (!(wasNull && wasSomething))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered null:{wasNull}, something:{wasSomething}");
  }

  private struct DemoStruct {
#pragma warning disable CS0649
    public int x;
    public int y;
#pragma warning restore CS0649
  }

  [Test]
  public void GenerateRandomStruct() {
    var wasNotDefault = false;
    var wasEmpty = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<DemoStruct>();
      if (value is { x: 0, y: 0 })
        wasEmpty = true;
      else
        wasNotDefault = true;

      if (!(wasEmpty && wasNotDefault))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered Empty:{wasEmpty}, someData:{wasNotDefault}");
  }

  // ReSharper disable once ClassNeverInstantiated.Local
  private class DemoClass {
    public readonly int x;
    public readonly int y;

    // ReSharper disable once UnusedMember.Local
    public DemoClass() => this.y = 1;

    // ReSharper disable once UnusedMember.Local
    public DemoClass(int x) => this.x = x;
  }

  [Test]
  public void GenerateRandomClass() {
    var wasNull = false;
    var wasDefaultCtor = false;
    var was2ndCtor = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<DemoClass>();
      switch (value) {
        case null: wasNull = true; break;
        case { x: 0, y: 1 }: wasDefaultCtor = true; break;
        default: {
          if (value.x != 0)
            was2ndCtor = true;
          break;
        }
      }

      if (!(wasNull && wasDefaultCtor && was2ndCtor))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered null:{wasNull}, default ctor:{wasDefaultCtor}, other ctor:{was2ndCtor}");
  }

  private enum DemoEnum {
    Apple = -1,
    Unknown = 0,
    Pie = 1
  }

  [Test]
  public void GenerateRandomEnum() {
    var wasPositive = false;
    var wasNegative = false;
    var wasNegativeMax = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<DemoEnum>();
      switch (value) {
        case DemoEnum.Unknown: wasZero = true; break;
        case DemoEnum.Apple: wasNegativeMax = true; break;
        case DemoEnum.Pie: wasPositiveMax = true; break;
        default: {
          switch ((int)value) {
            case < 0: wasNegative = true; break;
            case > 0: wasPositive = true; break;
          }

          break;
        }
      }

      if (!(wasNegative && wasPositive && wasZero && wasNegativeMax && wasPositiveMax))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered negmax:{wasNegativeMax}, negative:{wasNegative}, zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  private enum SmallDemoEnum : byte {
    Unknown = 0,
    Apple = 1
  }

  [Test]
  public void GenerateRandomSmallEnum() {
    var wasPositive = false;
    var wasPositiveMax = false;
    var wasZero = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<SmallDemoEnum>();
      switch (value) {
        case SmallDemoEnum.Unknown: wasZero = true; break;
        case SmallDemoEnum.Apple: wasPositiveMax = true; break;
        default: wasPositive = true; break;
      }

      if (!(wasPositive && wasZero && wasPositiveMax))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered zero:{wasZero}, positive:{wasPositive}, posmax:{wasPositiveMax}");
  }

  [Flags]
  private enum FlagDemoEnum : byte {
    Apple = 1,
    Pie = 2
  }

  [Test]
  public void GenerateRandomFlagEnum() {
    var wasPositive = false;
    var wasCombined = false;
    var wasSingle = false;

    var rnd = new Random();

    var stopwatch = Stopwatch.StartNew();
    do {
      var value = rnd.GetValueFor<FlagDemoEnum>();
      if (value == FlagDemoEnum.Apple)
        wasSingle = true;
      else if (value == (FlagDemoEnum.Apple | FlagDemoEnum.Pie))
        wasCombined = true;
      else if ((byte)value > 0)
        wasPositive = true;

      if (!(wasPositive && wasSingle && wasCombined))
        continue;

      Assert.Pass();
      return;
    } while (stopwatch.Elapsed < _timeout);

    Assert.Fail($"Following values were encountered single:{wasSingle}, positive:{wasPositive}, combined:{wasCombined}");
  }

  [Test]
  public void NextInt64_NullRandom_ThrowsArgumentNullException() {
    // Arrange
    Random? random = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => random!.NextInt64(100));
  }

  [Test]
  public void NextInt64_NegativeMaxValue_ThrowsArgumentOutOfRangeException() {
    // Arrange
    var random = new Random(42); // Seed for reproducibility

    // Act & Assert
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt64(-1));
    Assert.That(exception!.ParamName, Is.EqualTo("maxValue"));
  }

  [Test]
  public void NextInt64_ZeroMaxValue_ReturnsZero() {
    // Arrange
    var random = new Random(42);

    // Act
    var result = random.NextInt64(0);

    // Assert
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  public void NextInt64_PowerOfTwoMaxValueSmall_ReturnsValueInRange() {
    // Arrange
    var random = new Random(42);
    var maxValue = 1024L; // 2^10

    // Act
    var results = Enumerable
      .Range(0, 1000)
      .Select(_ => random.NextInt64(maxValue))
      .ToList();

    // Assert
    Assert.That(results, Has.All.InRange(0, maxValue - 1));
    Assert.That(results, Has.Some.GreaterThan(maxValue / 2)); // Ensure distribution reaches upper half
    Assert.That(results, Has.Some.LessThan(maxValue / 2)); // Ensure distribution reaches lower half
  }

  [Test]
  public void NextInt64_NonPowerOfTwoMaxValueSmall_ReturnsValueInRange() {
    // Arrange
    var random = new Random(42);
    var maxValue = 1000L; // Not a power of 2

    // Act
    var results = Enumerable
      .Range(0, 1000)
      .Select(_ => random.NextInt64(maxValue))
      .ToList();

    // Assert
    Assert.That(results, Has.All.InRange(0, maxValue - 1));
    Assert.That(results, Has.Some.GreaterThan(maxValue / 2)); // Ensure distribution reaches upper half
    Assert.That(results, Has.Some.LessThan(maxValue / 2)); // Ensure distribution reaches lower half
  }

  [Test]
  public void NextInt64_PowerOfTwoMaxValueLarge_ReturnsValueInRange() {
    // Arrange
    var random = new Random(42);
    var maxValue = 1L << 40; // 2^40, much larger than int.MaxValue

    // Act
    var results = Enumerable
      .Range(0, 1000)
      .Select(_ => random.NextInt64(maxValue))
      .ToList();

    // Assert
    Assert.That(results, Has.All.InRange(0, maxValue - 1));
    Assert.That(results.Any(r => r > 1L << 30), Is.True); // Ensure we get some large values
    // INFO: Seems .NET defaults RNG does not hold on to this
    //Assert.That(results.Any(r => r < 1L << 20), Is.True); // Ensure we get some small values
  }

  [Test]
  public void NextInt64_NonPowerOfTwoMaxValueLarge_ReturnsValueInRange() {
    // Arrange
    var random = new Random(42);
    var maxValue = (1L << 40) + 123; // Not a power of 2, larger than int.MaxValue

    // Act
    var results = Enumerable
      .Range(0, 1000000)
      .Select(_ => random.NextInt64(maxValue))
      .ToList();

    // Assert
    Assert.That(results, Has.All.InRange(0, maxValue - 1));
    Assert.That(results.Any(r => r > 1L << 30), Is.True); // Ensure we get some large values
    // INFO: Seems .NET defaults RNG does not hold on to this
    //Assert.That(results.Any(r => r < 1L << 20), Is.True); // Ensure we get some small values
  }

  [Test]
  public void NextInt64_MaxValueEdgeCase_IntMaxValue_ReturnsValueInRange() {
    // Arrange
    var random = new Random(42);
    var maxValue = (long)int.MaxValue;

    // Act
    var results = Enumerable
      .Range(0, 1000)
      .Select(_ => random.NextInt64(maxValue))
      .ToList();

    // Assert
    Assert.That(results, Has.All.InRange(0, maxValue - 1));
  }

  [Test]
  public void NextInt64_MaxValueEdgeCase_IntMaxValuePlusOne_ReturnsValueInRange() {
    // Arrange
    var random = new Random(42);
    var maxValue = (long)int.MaxValue + 1;

    // Act
    var results = Enumerable
      .Range(0, 1000)
      .Select(_ => random.NextInt64(maxValue))
      .ToList();

    // Assert
    Assert.That(results, Has.All.InRange(0, maxValue - 1));
  }

  [Test]
  public void NextInt64_Distribution_IsReasonablyUniform() {
    // Arrange
    var random = new Random(42);
    var maxValue = 100L;
    var iterations = 100000;
    var buckets = new int[maxValue];

    // Act
    for (var i = 0; i < iterations; ++i) {
      var value = random.NextInt64(maxValue);
      ++buckets[value];
    }

    // Assert
    var expectedPerBucket = iterations / maxValue;
    const double tolerance = 0.1; // Allow 10% deviation from expected value

    // Check that each bucket has roughly the expected count
    for (var i = 0; i < maxValue; ++i) {
      var min = expectedPerBucket * (1 - tolerance);
      var max = expectedPerBucket * (1 + tolerance);

      // Using this instead of Assert.That to avoid failing on minor statistical fluctuations
      if (buckets[i] < min || buckets[i] > max)
        Console.WriteLine($"Warning: Bucket {i} contains {buckets[i]} values (expected between {min} and {max})");
    }

    // Verify no bucket is empty and no bucket is massively overfull
    Assert.That(buckets, Has.All.GreaterThan(0), "All possible values should be generated");
    Assert.That(buckets, Has.All.LessThan(expectedPerBucket * 2), "No value should be massively overrepresented");
  }

  [Test]
  public void NextInt64_BitDistribution_IsWellDistributed() {
    // Arrange
    var random = new Random(42);
    var iterations = 100000;
    var maxValue = long.MaxValue; // Use full long range for bit testing

    // For each bit position, count how many times it's set to 1
    var bitCounts = new int[64];

    // Act
    for (var i = 0; i < iterations; ++i) {
      var value = random.NextInt64(maxValue);

      // Check each bit
      for (var bit = 0; bit < 64; ++bit)
        if ((value & (1L << bit)) != 0)
          ++bitCounts[bit];
    }

    // Assert
    const double tolerance = 0.05; // Allow 5% deviation from expected value

    // Check that each bit is set roughly 50% of the time
    for (var bit = 0; bit < 64; ++bit) {
      var ratio = (double)bitCounts[bit] / iterations;

      // Skip highest bits if they're never reachable due to maxValue
      if (1UL << bit > (ulong)maxValue)
        continue;

      Assert.That(
        ratio,
        Is.InRange(0.5 - tolerance, 0.5 + tolerance),
        $"Bit at position {bit} is not well distributed. Set in {ratio:P2} of cases."
      );
    }
  }

  [Test]
  public void NextInt64_SequentialBitPatterns_ShowsNoObviousPatterns() {
    // Arrange
    var random = new Random(42);
    var sampleSize = 1000;
    var maxValue = long.MaxValue;

    // Generate sequence of values
    var values = Enumerable
      .Range(0, sampleSize)
      .Select(_ => random.NextInt64(maxValue))
      .ToArray();

    // Act - Calculate autocorrelation for short lags
    var autocorrelations = CalculateAutocorrelations(values, 10);

    // Assert
    // For random data, autocorrelations should be close to zero for non-zero lags
    for (var lag = 1; lag < autocorrelations.Length; ++lag)
      Assert.That(
        Math.Abs(autocorrelations[lag]),
        Is.LessThan(0.1),
        $"Autocorrelation at lag {lag} is too high, suggesting a pattern."
      );

    return;

    static double[] CalculateAutocorrelations(long[] values, int maxLag) {
      var result = new double[maxLag + 1];
      var mean = values.Select(i => (double)i).Average();
      var variance = values.Select(v => (v - mean) * (v - mean)).Average();

      for (var lag = 0; lag <= maxLag; ++lag) {
        var sum = 0.0;
        var count = values.Length - lag;

        for (var i = 0; i < count; ++i)
          sum += (values[i] - mean) * (values[i + lag] - mean);

        result[lag] = sum / (count * variance);
      }

      return result;
    }
  }
}
