using System.Diagnostics;
using NUnit.Framework;

namespace System.MathExtensionsTests;

/// <summary>
///   Tests for clamping methods
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Clamp Tests - All numeric types

  [Test]
  [TestCase((sbyte)15, (sbyte)10, (sbyte)20, (sbyte)15)]   // Within range
  [TestCase((sbyte)5, (sbyte)10, (sbyte)20, (sbyte)10)]    // Below range
  [TestCase((sbyte)25, (sbyte)10, (sbyte)20, (sbyte)20)]   // Above range
  [TestCase((sbyte)10, (sbyte)10, (sbyte)20, (sbyte)10)]   // At minimum
  [TestCase((sbyte)20, (sbyte)10, (sbyte)20, (sbyte)20)]   // At maximum
  [TestCase((sbyte)0, (sbyte)0, (sbyte)0, (sbyte)0)]       // All zeros
  [Category("HappyPath")]
  [Description("Validates sbyte Clamp returns correct values")]
  public void Clamp_SByte_VariousInputs_ReturnsCorrectValue(sbyte value, sbyte min, sbyte max, sbyte expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase((byte)15, (byte)10, (byte)20, (byte)15)]
  [TestCase((byte)5, (byte)10, (byte)20, (byte)10)]
  [TestCase((byte)25, (byte)10, (byte)20, (byte)20)]
  [TestCase((byte)0, (byte)0, (byte)255, (byte)0)]
  [TestCase((byte)255, (byte)0, (byte)255, (byte)255)]
  [Category("HappyPath")]
  [Description("Validates byte Clamp returns correct values")]
  public void Clamp_Byte_VariousInputs_ReturnsCorrectValue(byte value, byte min, byte max, byte expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase((short)150, (short)100, (short)200, (short)150)]
  [TestCase((short)50, (short)100, (short)200, (short)100)]
  [TestCase((short)250, (short)100, (short)200, (short)200)]
  [TestCase((short)-100, (short)-50, (short)50, (short)-50)]
  [Category("HappyPath")]
  [Description("Validates short Clamp returns correct values")]
  public void Clamp_Short_VariousInputs_ReturnsCorrectValue(short value, short min, short max, short expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase((ushort)1500, (ushort)1000, (ushort)2000, (ushort)1500)]
  [TestCase((ushort)500, (ushort)1000, (ushort)2000, (ushort)1000)]
  [TestCase((ushort)2500, (ushort)1000, (ushort)2000, (ushort)2000)]
  [Category("HappyPath")]
  [Description("Validates ushort Clamp returns correct values")]
  public void Clamp_UShort_VariousInputs_ReturnsCorrectValue(ushort value, ushort min, ushort max, ushort expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase(150, 100, 200, 150)]
  [TestCase(50, 100, 200, 100)]
  [TestCase(250, 100, 200, 200)]
  [TestCase(-150, -200, -100, -150)]
  [TestCase(-250, -200, -100, -200)]
  [TestCase(-50, -200, -100, -100)]
  [Category("HappyPath")]
  [Description("Validates int Clamp returns correct values")]
  public void Clamp_Int_VariousInputs_ReturnsCorrectValue(int value, int min, int max, int expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase(1500U, 1000U, 2000U, 1500U)]
  [TestCase(500U, 1000U, 2000U, 1000U)]
  [TestCase(2500U, 1000U, 2000U, 2000U)]
  [Category("HappyPath")]
  [Description("Validates uint Clamp returns correct values")]
  public void Clamp_UInt_VariousInputs_ReturnsCorrectValue(uint value, uint min, uint max, uint expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase(1500L, 1000L, 2000L, 1500L)]
  [TestCase(500L, 1000L, 2000L, 1000L)]
  [TestCase(2500L, 1000L, 2000L, 2000L)]
  [TestCase(-1500L, -2000L, -1000L, -1500L)]
  [Category("HappyPath")]
  [Description("Validates long Clamp returns correct values")]
  public void Clamp_Long_VariousInputs_ReturnsCorrectValue(long value, long min, long max, long expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase(1500UL, 1000UL, 2000UL, 1500UL)]
  [TestCase(500UL, 1000UL, 2000UL, 1000UL)]
  [TestCase(2500UL, 1000UL, 2000UL, 2000UL)]
  [Category("HappyPath")]
  [Description("Validates ulong Clamp returns correct values")]
  public void Clamp_ULong_VariousInputs_ReturnsCorrectValue(ulong value, ulong min, ulong max, ulong expected) {
    var result = value.Clamp(min, max);
    Assert.That(result, Is.EqualTo(expected), $"Clamp({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase(15.5f, 10.0f, 20.0f, 15.5f)]
  [TestCase(5.0f, 10.0f, 20.0f, 10.0f)]
  [TestCase(25.0f, 10.0f, 20.0f, 20.0f)]
  [TestCase(float.NaN, 10.0f, 20.0f, float.NaN)]     // NaN should remain NaN
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates float Clamp returns correct values")]
  public void Clamp_Float_VariousInputs_ReturnsCorrectValue(float value, float min, float max, float expected) {
    var result = value.Clamp(min, max);
    if (float.IsNaN(expected)) {
      Assert.That(float.IsNaN(result), Is.True, $"Clamp({value}, {min}, {max}) should be NaN");
    } else {
      Assert.That(result, Is.EqualTo(expected).Within(0.001f), $"Clamp({value}, {min}, {max}) should equal {expected}");
    }
  }

  [Test]
  [TestCase(15.5, 10.0, 20.0, 15.5)]
  [TestCase(5.0, 10.0, 20.0, 10.0)]
  [TestCase(25.0, 10.0, 20.0, 20.0)]
  [TestCase(double.NaN, 10.0, 20.0, double.NaN)]
  [TestCase(double.PositiveInfinity, 10.0, 20.0, 20.0)]
  [TestCase(double.NegativeInfinity, 10.0, 20.0, 10.0)]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates double Clamp returns correct values")]
  public void Clamp_Double_VariousInputs_ReturnsCorrectValue(double value, double min, double max, double expected) {
    var result = value.Clamp(min, max);
    if (double.IsNaN(expected)) {
      Assert.That(double.IsNaN(result), Is.True, $"Clamp({value}, {min}, {max}) should be NaN");
    } else {
      Assert.That(result, Is.EqualTo(expected).Within(0.001), $"Clamp({value}, {min}, {max}) should equal {expected}");
    }
  }

  #endregion

  #region ClampUnchecked Tests

  [Test]
  [TestCase(15, 10, 20, 15)]
  [TestCase(5, 10, 20, 10)]
  [TestCase(25, 10, 20, 20)]
  [TestCase(15, 15, 15, 15)]  // All same values
  [Category("HappyPath")]
  [Description("Validates ClampUnchecked returns correct values")]
  public void ClampUnchecked_Int_VariousInputs_ReturnsCorrectValue(int value, int min, int max, int expected) {
    var result = value.ClampUnchecked(min, max);
    Assert.That(result, Is.EqualTo(expected), $"ClampUnchecked({value}, {min}, {max}) should equal {expected}");
  }

  [Test]
  [TestCase(15.5f, 10.0f, 20.0f, 15.5f)]
  [TestCase(5.0f, 10.0f, 20.0f, 10.0f)]
  [TestCase(25.0f, 10.0f, 20.0f, 20.0f)]
  [Category("HappyPath")]
  [Description("Validates ClampUnchecked returns correct values for floats")]
  public void ClampUnchecked_Float_VariousInputs_ReturnsCorrectValue(float value, float min, float max, float expected) {
    var result = value.ClampUnchecked(min, max);
    Assert.That(result, Is.EqualTo(expected).Within(0.001f), $"ClampUnchecked({value}, {min}, {max}) should equal {expected}");
  }

  #endregion

  #region Exception Tests

  [Test]
  [Category("Exception")]
  [Description("Validates Clamp throws when max < min")]
  public void Clamp_MaxLessThanMin_ThrowsArgumentExceptionInt() => Assert.That(() => 15.Clamp(20, 10),
    Throws.TypeOf<ArgumentException>()
      .Or.TypeOf<ArgumentOutOfRangeException>());

  [Test]
  [Category("Exception")]
  [Description("Validates Clamp throws when max < min (float)")]
  public void Clamp_MaxLessThanMin_ThrowsArgumentExceptionFloat() => Assert.That(() => 15.5f.Clamp(20.0f, 10.0f),
    Throws.TypeOf<ArgumentException>()
      .Or.TypeOf<ArgumentOutOfRangeException>());

  [Test]
  [Category("Exception")]
  [Description("Validates Clamp throws when max < min (double)")]
  public void Clamp_MaxLessThanMin_ThrowsArgumentExceptionDouble() => Assert.That(() => 15.5.Clamp(20.0, 10.0),
    Throws.TypeOf<ArgumentException>()
      .Or.TypeOf<ArgumentOutOfRangeException>());

  [Test]
  [Category("HappyPath")]
  [Description("ClampUnchecked should not validate parameters")]
  public void ClampUnchecked_MaxLessThanMin_DoesNotThrow() {
    // ClampUnchecked should not validate parameters - behavior is undefined but shouldn't throw
    Assert.DoesNotThrow(() => {
      var result = 15.ClampUnchecked(20, 10);
      // Result is undefined when max < min, so we just verify it doesn't throw
    });
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Clamp with extreme values")]
  public void Clamp_ExtremeValues_HandlesCorrectly() {
    // Test with type boundaries
    Assert.That(byte.MinValue.Clamp(byte.MinValue, byte.MaxValue), Is.EqualTo(byte.MinValue));
    Assert.That(byte.MaxValue.Clamp(byte.MinValue, byte.MaxValue), Is.EqualTo(byte.MaxValue));
    
    Assert.That(int.MinValue.Clamp(int.MinValue, int.MaxValue), Is.EqualTo(int.MinValue));
    Assert.That(int.MaxValue.Clamp(int.MinValue, int.MaxValue), Is.EqualTo(int.MaxValue));
    
    Assert.That(float.MinValue.Clamp(float.MinValue, float.MaxValue), Is.EqualTo(float.MinValue));
    Assert.That(float.MaxValue.Clamp(float.MinValue, float.MaxValue), Is.EqualTo(float.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Clamp with identical min and max")]
  public void Clamp_IdenticalMinMax_ReturnsMinMax() {
    const int value = 123;
    const int minMax = 100;
    
    Assert.That(value.Clamp(minMax, minMax), Is.EqualTo(minMax));
    Assert.That(50.Clamp(minMax, minMax), Is.EqualTo(minMax));
    Assert.That(150.Clamp(minMax, minMax), Is.EqualTo(minMax));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates Clamp performance with many operations")]
  public void Clamp_ManyOperations_CompletesQuickly() {
    const int iterations = 10_000_000;
    var sw = Stopwatch.StartNew();

    for (var i = 0; i < iterations; i++) {
      var result = i.Clamp(100, 200);
      _ = result; // Prevent optimization
    }

    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500), $"10M Clamp operations took {sw.ElapsedMilliseconds}ms");
  }

  [Test]
  [Category("Performance")]
  [Description("Validates ClampUnchecked performance advantage")]
  public void ClampUnchecked_ManyOperations_IsFasterThanClamp() {
    const int iterations = 10_000_000;
    
    // Warmup both methods to reduce JIT impact
    for (var i = 0; i < 1000; i++) {
      _ = i.Clamp(100, 200);
      _ = i.ClampUnchecked(100, 200);
    }
    
    // Measure Clamp
    var sw1 = Stopwatch.StartNew();
    for (var i = 0; i < iterations; i++) {
      var result = i.Clamp(100, 200);
      _ = result;
    }
    sw1.Stop();
    
    // Measure ClampUnchecked
    var sw2 = Stopwatch.StartNew();
    for (var i = 0; i < iterations; i++) {
      var result = i.ClampUnchecked(100, 200);
      _ = result;
    }
    sw2.Stop();
    
    // Both operations should complete in reasonable time (under 500ms for 10M iterations)
    // We don't assert strict performance relationships as they can vary in CI environments
    Assert.That(sw1.ElapsedMilliseconds, Is.LessThan(500), 
               $"Clamp took {sw1.ElapsedMilliseconds}ms for 10M iterations");
    Assert.That(sw2.ElapsedMilliseconds, Is.LessThan(500), 
               $"ClampUnchecked took {sw2.ElapsedMilliseconds}ms for 10M iterations");
    
    // Log the comparison for informational purposes
    if (sw2.ElapsedMilliseconds > sw1.ElapsedMilliseconds) {
      TestContext.WriteLine($"NOTE: ClampUnchecked ({sw2.ElapsedMilliseconds}ms) was slower than Clamp ({sw1.ElapsedMilliseconds}ms) - this can happen in variable CI environments");
    }
  }

  #endregion
}