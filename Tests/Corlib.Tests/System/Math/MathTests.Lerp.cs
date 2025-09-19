using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace System.MathExtensionsTests;

/// <summary>
///   Tests for linear interpolation (Lerp) methods
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Lerp Tests - Integer types with integer parameter

  [Test]
  [TestCase((byte)100, (byte)200, (byte)0, (byte)100)]     // t=0 should return start
  [TestCase((byte)100, (byte)200, (byte)255, (byte)200)]   // t=max should return end
  [TestCase((byte)100, (byte)200, (byte)128, (byte)150)]   // t=half should return middle
  [TestCase((byte)0, (byte)255, (byte)64, (byte)64)]       // Quarter point
  [TestCase((byte)255, (byte)0, (byte)128, (byte)127)]     // Reverse direction
  [Category("HappyPath")]
  [Description("Validates byte Lerp with byte parameter")]
  public void Lerp_Byte_WithByteParameter_ReturnsCorrectValue(byte start, byte end, byte t, byte expected) {
    var result = start.Lerp(end, t);
    Assert.That(result, Is.EqualTo(expected).Within(1), $"Lerp({start}, {end}, {t}) should be approximately {expected}");
  }

  [Test]
  [TestCase((ushort)1000, (ushort)9000, (ushort)0, (ushort)1000)]
  [TestCase((ushort)1000, (ushort)9000, (ushort)65535, (ushort)9000)]
  [TestCase((ushort)1000, (ushort)9000, (ushort)32768, (ushort)5000)]
  [Category("HappyPath")]
  [Description("Validates ushort Lerp with ushort parameter")]
  public void Lerp_UShort_WithUShortParameter_ReturnsCorrectValue(ushort start, ushort end, ushort t, ushort expected) {
    var result = start.Lerp(end, t);
    Assert.That(result, Is.EqualTo(expected).Within(10), $"Lerp({start}, {end}, {t}) should be approximately {expected}");
  }

  [Test]
  [TestCase(1000000U, 9000000U, 0U, 1000000U)]
  [TestCase(1000000U, 9000000U, 4294967295U, 9000000U)]
  [TestCase(1000000U, 9000000U, 2147483648U, 5000000U)]
  [Category("HappyPath")]
  [Description("Validates uint Lerp with uint parameter")]
  public void Lerp_UInt_WithUIntParameter_ReturnsCorrectValue(uint start, uint end, uint t, uint expected) {
    var result = start.Lerp(end, t);
    Assert.That(result, Is.EqualTo(expected).Within(1000), $"Lerp({start}, {end}, {t}) should be approximately {expected}");
  }

  [Test]
  [TestCase(100UL, 200UL, 0UL, 100UL)]
  [TestCase(100UL, 200UL, ulong.MaxValue, 200UL)]  
  [TestCase(100UL, 200UL, ulong.MaxValue/2, 150UL)]
  [Category("HappyPath")]
  [Description("Validates ulong Lerp with ulong parameter")]
  public void Lerp_ULong_WithULongParameter_ReturnsCorrectValue(ulong start, ulong end, ulong t, ulong expected) {
    var result = start.Lerp(end, t);
    Assert.That(result, Is.EqualTo(expected).Within(5UL), $"Lerp({start}, {end}, {t}) should be approximately {expected}");
  }

  #endregion

  #region Lerp Tests - With floating-point parameter

  [Test]
  [TestCase((byte)100, (byte)200, 0.0f, (byte)100)]
  [TestCase((byte)100, (byte)200, 1.0f, (byte)200)]
  [TestCase((byte)100, (byte)200, 0.5f, (byte)150)]
  [TestCase((byte)100, (byte)200, 0.25f, (byte)125)]
  [TestCase((byte)100, (byte)200, 0.75f, (byte)175)]
  [TestCase((byte)100, (byte)200, 1.5f, (byte)200)] // Clamped to 1.0
  [TestCase((byte)100, (byte)200, -0.5f, (byte)100)] // Clamped to 0.0
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates byte Lerp with float parameter")]
  public void Lerp_Byte_WithFloatParameter_ReturnsCorrectValue(byte start, byte end, float t, byte expected) {
    var result = start.Lerp(end, t);
    Assert.That(result, Is.EqualTo(expected), $"Lerp({start}, {end}, {t}) should equal {expected}");
  }

  [Test]
  [TestCase((byte)100, (byte)200, 0.0, (byte)100)]
  [TestCase((byte)100, (byte)200, 1.0, (byte)200)]
  [TestCase((byte)100, (byte)200, 0.333, (byte)133)]
  [TestCase((byte)100, (byte)200, 0.666, (byte)166)]
  [Category("HappyPath")]
  [Description("Validates byte Lerp with double parameter")]
  public void Lerp_Byte_WithDoubleParameter_ReturnsCorrectValue(byte start, byte end, double t, byte expected) {
    var result = start.Lerp(end, t);
    Assert.That(result, Is.EqualTo(expected).Within(1), $"Lerp({start}, {end}, {t}) should be approximately {expected}");
  }

  #endregion

  #region LerpUnclamped Tests

  [Test]
  [TestCase((byte)100, (byte)200, 0.0f, (byte)100)]
  [TestCase((byte)100, (byte)200, 1.0f, (byte)200)]
  [TestCase((byte)100, (byte)200, 0.5f, (byte)150)]
  [TestCase((byte)100, (byte)200, 1.5f, (byte)250)] // Extrapolation beyond end
  [TestCase((byte)100, (byte)200, -0.5f, (byte)50)] // Extrapolation before start
  [TestCase((byte)100, (byte)200, 2.0f, (byte)44)]  // Overflows and wraps around due to casting
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates byte LerpUnclamped allows extrapolation")]
  public void LerpUnclamped_Byte_WithFloatParameter_AllowsExtrapolation(byte start, byte end, float t, byte expected) {
    var result = start.LerpUnclamped(end, t);
    Assert.That(result, Is.EqualTo(expected).Within(1), $"LerpUnclamped({start}, {end}, {t}) should be approximately {expected}");
  }

  [Test]
  [TestCase((byte)100, (byte)200, 0.0, (byte)100)]
  [TestCase((byte)100, (byte)200, 1.25, (byte)225)]
  [TestCase((byte)100, (byte)200, -0.25, (byte)75)]
  [Category("HappyPath")]
  [Description("Validates byte LerpUnclamped with double parameter")]
  public void LerpUnclamped_Byte_WithDoubleParameter_AllowsExtrapolation(byte start, byte end, double t, byte expected) {
    var result = start.LerpUnclamped(end, t);
    Assert.That(result, Is.EqualTo(expected).Within(1), $"LerpUnclamped({start}, {end}, {t}) should be approximately {expected}");
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Lerp with identical start and end values")]
  public void Lerp_IdenticalValues_ReturnsSameValue() {
    const byte value = 123;
    Assert.That(value.Lerp(value, 0.0f), Is.EqualTo(value));
    Assert.That(value.Lerp(value, 0.5f), Is.EqualTo(value));
    Assert.That(value.Lerp(value, 1.0f), Is.EqualTo(value));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Lerp with extreme values")]
  public void Lerp_ExtremeValues_HandlesCorrectly() {
    Assert.That(byte.MinValue.Lerp(byte.MaxValue, 0.5f), Is.EqualTo(127).Within(1));
    Assert.That(byte.MaxValue.Lerp(byte.MinValue, 0.5f), Is.EqualTo(127).Within(1));
    
    Assert.That(ushort.MinValue.Lerp(ushort.MaxValue, 0.5f), Is.EqualTo(32767).Within(10));
    Assert.That(uint.MinValue.Lerp(uint.MaxValue, 0.5f), Is.EqualTo(2147483647U).Within(1000));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates Lerp performance with many operations")]
  public void Lerp_ManyOperations_CompletesQuickly() {
    const int iterations = 1_000_000;
    var sw = Stopwatch.StartNew();

    for (var i = 0; i < iterations; i++) {
      var result = ((byte)100).Lerp((byte)200, 0.5f);
      _ = result; // Prevent optimization
    }

    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100), $"1M Lerp operations took {sw.ElapsedMilliseconds}ms");
  }

  [Test]
  [Category("Performance")]
  [Description("Validates LerpUnclamped performance with many operations")]
  public void LerpUnclamped_ManyOperations_CompletesQuickly() {
    const int iterations = 1_000_000;
    var sw = Stopwatch.StartNew();

    for (var i = 0; i < iterations; i++) {
      var result = ((byte)100).LerpUnclamped((byte)200, 0.5f);
      _ = result; // Prevent optimization
    }

    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100), $"1M LerpUnclamped operations took {sw.ElapsedMilliseconds}ms");
  }

  #endregion
}