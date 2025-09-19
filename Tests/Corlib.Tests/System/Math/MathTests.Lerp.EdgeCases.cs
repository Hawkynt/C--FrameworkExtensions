using System.Collections.Generic;
using NUnit.Framework;

namespace System.MathExtensionsTests;

/// <summary>
///   Edge case tests for linear interpolation (Lerp) methods with integer parameters
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Integer Lerp Edge Cases - Only Integer Parameter Methods

  [Test]
  [Category("EdgeCase")]
  [Description("Validates byte Lerp edge cases with min/max boundaries")]
  public void Lerp_Byte_EdgeCases_ReturnsCorrectValues() {
    const byte min = byte.MinValue; // 0
    const byte max = byte.MaxValue; // 255
    
    // (min,min,min)=min, (min,min,max)=min, (min,max,min)=min, (min,max,max)=max, (max,min,min)=max, (max,min,max)=min
    Assert.That(min.Lerp(min, min), Is.EqualTo(min), "(min,min,min) should equal min");
    Assert.That(min.Lerp(min, max), Is.EqualTo(min), "(min,min,max) should equal min");
    Assert.That(min.Lerp(max, min), Is.EqualTo(min), "(min,max,min) should equal min");
    Assert.That(min.Lerp(max, max), Is.EqualTo(max), "(min,max,max) should equal max");
    Assert.That(max.Lerp(min, min), Is.EqualTo(max), "(max,min,min) should equal max");
    Assert.That(max.Lerp(min, max), Is.EqualTo(min), "(max,min,max) should equal min");
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ushort Lerp edge cases with min/max boundaries")]
  public void Lerp_UShort_EdgeCases_ReturnsCorrectValues() {
    const ushort min = ushort.MinValue; // 0
    const ushort max = ushort.MaxValue; // 65535
    
    Assert.That(min.Lerp(min, min), Is.EqualTo(min), "(min,min,min) should equal min");
    Assert.That(min.Lerp(min, max), Is.EqualTo(min), "(min,min,max) should equal min");
    Assert.That(min.Lerp(max, min), Is.EqualTo(min), "(min,max,min) should equal min");
    Assert.That(min.Lerp(max, max), Is.EqualTo(max), "(min,max,max) should equal max");
    Assert.That(max.Lerp(min, min), Is.EqualTo(max), "(max,min,min) should equal max");
    Assert.That(max.Lerp(min, max), Is.EqualTo(min), "(max,min,max) should equal min");
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates uint Lerp edge cases with min/max boundaries")]
  public void Lerp_UInt_EdgeCases_ReturnsCorrectValues() {
    const uint min = uint.MinValue; // 0
    const uint max = uint.MaxValue; // 4294967295
    
    Assert.That(min.Lerp(min, min), Is.EqualTo(min), "(min,min,min) should equal min");
    Assert.That(min.Lerp(min, max), Is.EqualTo(min), "(min,min,max) should equal min");
    Assert.That(min.Lerp(max, min), Is.EqualTo(min), "(min,max,min) should equal min");
    Assert.That(min.Lerp(max, max), Is.EqualTo(max), "(min,max,max) should equal max");
    Assert.That(max.Lerp(min, min), Is.EqualTo(max), "(max,min,min) should equal max");
    Assert.That(max.Lerp(min, max), Is.EqualTo(min), "(max,min,max) should equal min");
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ulong Lerp edge cases with min/max boundaries")]
  public void Lerp_ULong_EdgeCases_ReturnsCorrectValues() {
    const ulong min = ulong.MinValue; // 0
    const ulong max = ulong.MaxValue; // 18446744073709551615
    
    Assert.That(min.Lerp(min, min), Is.EqualTo(min), "(min,min,min) should equal min");
    Assert.That(min.Lerp(min, max), Is.EqualTo(min), "(min,min,max) should equal min");
    Assert.That(min.Lerp(max, min), Is.EqualTo(min), "(min,max,min) should equal min");
    Assert.That(min.Lerp(max, max), Is.EqualTo(max), "(min,max,max) should equal max");
    Assert.That(max.Lerp(min, min), Is.EqualTo(max), "(max,min,min) should equal max");
    Assert.That(max.Lerp(min, max), Is.EqualTo(min), "(max,min,max) should equal min");
  }

  #endregion

  #region Additional Edge Cases - Precision and Overflow

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Lerp handles large value differences correctly")]
  public void Lerp_LargeValueDifferences_HandlesCorrectly() {
    // Test with maximum possible differences for unsigned integer types only
    Assert.That(byte.MinValue.Lerp(byte.MaxValue, (byte)(byte.MaxValue / 2)), 
                Is.EqualTo(127).Within(1), "Byte max difference should interpolate correctly");
    
    Assert.That(ushort.MinValue.Lerp(ushort.MaxValue, (ushort)(ushort.MaxValue / 2)), 
                Is.EqualTo(32767).Within(10), "UShort max difference should interpolate correctly");
    
    Assert.That(uint.MinValue.Lerp(uint.MaxValue, uint.MaxValue / 2), 
                Is.EqualTo(2147483647U).Within(1000), "UInt max difference should interpolate correctly");
    
    // For ulong, test with smaller values due to precision limits
    Assert.That(1000UL.Lerp(ulong.MaxValue, ulong.MaxValue / 2), 
                Is.GreaterThan(1000UL), "ULong should interpolate towards max value");
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Lerp with identical start and end values")]
  public void Lerp_IdenticalValues_ReturnsValue() {
    const byte value = 123;
    const ushort value2 = 12345;
    const uint value3 = 1234567890;
    const ulong value4 = 12345678901234567;
    
    Assert.That(value.Lerp(value, (byte)0), Is.EqualTo(value), "Identical byte values with t=0");
    Assert.That(value.Lerp(value, byte.MaxValue), Is.EqualTo(value), "Identical byte values with t=max");
    Assert.That(value.Lerp(value, (byte)(byte.MaxValue / 2)), Is.EqualTo(value), "Identical byte values with t=mid");
    
    Assert.That(value2.Lerp(value2, (ushort)0), Is.EqualTo(value2), "Identical ushort values with t=0");
    Assert.That(value2.Lerp(value2, ushort.MaxValue), Is.EqualTo(value2), "Identical ushort values with t=max");
    
    Assert.That(value3.Lerp(value3, 0U), Is.EqualTo(value3), "Identical uint values with t=0");
    Assert.That(value3.Lerp(value3, uint.MaxValue), Is.EqualTo(value3), "Identical uint values with t=max");
    
    Assert.That(value4.Lerp(value4, 0UL), Is.EqualTo(value4), "Identical ulong values with t=0");
    Assert.That(value4.Lerp(value4, ulong.MaxValue), Is.EqualTo(value4), "Identical ulong values with t=max");
  }

  [Test]
  [Category("EdgeCase")]  
  [Description("Validates Lerp parameter boundary conditions")]
  public void Lerp_ParameterBoundaries_HandlesCorrectly() {
    // Test t parameter at exact boundaries for unsigned integer types only
    Assert.That(((byte)100).Lerp((byte)200, (byte)0), Is.EqualTo(100), "t=0 should return start value");
    Assert.That(((byte)100).Lerp((byte)200, byte.MaxValue), Is.EqualTo(200), "t=max should return end value");
    
    Assert.That(((ushort)1000).Lerp((ushort)2000, (ushort)0), Is.EqualTo(1000), "t=0 should return start value for ushort");
    Assert.That(((ushort)1000).Lerp((ushort)2000, ushort.MaxValue), Is.EqualTo(2000), "t=max should return end value for ushort");
    
    Assert.That(1000000U.Lerp(2000000U, 0U), Is.EqualTo(1000000U), "t=0 should return start value for uint");
    Assert.That(1000000U.Lerp(2000000U, uint.MaxValue), Is.EqualTo(2000000U), "t=max should return end value for uint");
    
    Assert.That(1000000UL.Lerp(2000000UL, 0UL), Is.EqualTo(1000000UL), "t=0 should return start value for ulong");
    Assert.That(1000000UL.Lerp(2000000UL, ulong.MaxValue), Is.EqualTo(2000000UL), "t=max should return end value for ulong");
  }

  #endregion
}