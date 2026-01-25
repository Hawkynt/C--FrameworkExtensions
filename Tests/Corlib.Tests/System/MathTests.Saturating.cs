using NUnit.Framework;

namespace System;

[TestFixture]
public class MathSaturatingTests {

  #region byte

  [Test]
  public void SaturatingAdd_Byte_NormalCase_ReturnsSum() {
    byte a = 100;
    Assert.AreEqual((byte)150, a.SaturatingAdd(50));
  }

  [Test]
  public void SaturatingAdd_Byte_Overflow_ReturnsMaxValue() {
    byte a = 250;
    Assert.AreEqual(byte.MaxValue, a.SaturatingAdd(10));
  }

  [Test]
  public void SaturatingAdd_Byte_MaxPlusMax_ReturnsMaxValue() {
    byte a = byte.MaxValue;
    Assert.AreEqual(byte.MaxValue, a.SaturatingAdd(byte.MaxValue));
  }

  [Test]
  public void SaturatingSubtract_Byte_NormalCase_ReturnsDifference() {
    byte a = 100;
    Assert.AreEqual((byte)50, a.SaturatingSubtract(50));
  }

  [Test]
  public void SaturatingSubtract_Byte_Underflow_ReturnsMinValue() {
    byte a = 5;
    Assert.AreEqual(byte.MinValue, a.SaturatingSubtract(10));
  }

  [Test]
  public void SaturatingMultiply_Byte_NormalCase_ReturnsProduct() {
    byte a = 10;
    Assert.AreEqual((byte)50, a.SaturatingMultiply(5));
  }

  [Test]
  public void SaturatingMultiply_Byte_Overflow_ReturnsMaxValue() {
    byte a = 100;
    Assert.AreEqual(byte.MaxValue, a.SaturatingMultiply(3));
  }

  [Test]
  public void SaturatingDivide_Byte_NormalCase_ReturnsQuotient() {
    byte a = 100;
    Assert.AreEqual((byte)20, a.SaturatingDivide(5));
  }

  #endregion

  #region sbyte

  [Test]
  public void SaturatingAdd_SByte_NormalCase_ReturnsSum() {
    sbyte a = 50;
    Assert.AreEqual((sbyte)80, a.SaturatingAdd(30));
  }

  [Test]
  public void SaturatingAdd_SByte_PositiveOverflow_ReturnsMaxValue() {
    sbyte a = 120;
    Assert.AreEqual(sbyte.MaxValue, a.SaturatingAdd(20));
  }

  [Test]
  public void SaturatingAdd_SByte_NegativeUnderflow_ReturnsMinValue() {
    sbyte a = -120;
    Assert.AreEqual(sbyte.MinValue, a.SaturatingAdd(-20));
  }

  [Test]
  public void SaturatingSubtract_SByte_NormalCase_ReturnsDifference() {
    sbyte a = 50;
    Assert.AreEqual((sbyte)20, a.SaturatingSubtract(30));
  }

  [Test]
  public void SaturatingSubtract_SByte_PositiveOverflow_ReturnsMaxValue() {
    sbyte a = 100;
    Assert.AreEqual(sbyte.MaxValue, a.SaturatingSubtract(-50));
  }

  [Test]
  public void SaturatingSubtract_SByte_NegativeUnderflow_ReturnsMinValue() {
    sbyte a = -100;
    Assert.AreEqual(sbyte.MinValue, a.SaturatingSubtract(50));
  }

  [Test]
  public void SaturatingMultiply_SByte_PositiveOverflow_ReturnsMaxValue() {
    sbyte a = 100;
    Assert.AreEqual(sbyte.MaxValue, a.SaturatingMultiply(2));
  }

  [Test]
  public void SaturatingMultiply_SByte_NegativeOverflow_ReturnsMaxValue() {
    sbyte a = -100;
    Assert.AreEqual(sbyte.MaxValue, a.SaturatingMultiply(-2));
  }

  [Test]
  public void SaturatingMultiply_SByte_NegativeUnderflow_ReturnsMinValue() {
    sbyte a = 100;
    Assert.AreEqual(sbyte.MinValue, a.SaturatingMultiply(-2));
  }

  [Test]
  public void SaturatingDivide_SByte_MinValueDividedByMinusOne_ReturnsMaxValue() {
    sbyte a = sbyte.MinValue;
    Assert.AreEqual(sbyte.MaxValue, a.SaturatingDivide(-1));
  }

  [Test]
  public void SaturatingNegate_SByte_MinValue_ReturnsMaxValue() {
    sbyte a = sbyte.MinValue;
    Assert.AreEqual(sbyte.MaxValue, a.SaturatingNegate());
  }

  [Test]
  public void SaturatingNegate_SByte_NormalCase_ReturnsNegation() {
    sbyte a = 50;
    Assert.AreEqual((sbyte)(-50), a.SaturatingNegate());
  }

  #endregion

  #region ushort

  [Test]
  public void SaturatingAdd_UShort_Overflow_ReturnsMaxValue() {
    ushort a = 65000;
    Assert.AreEqual(ushort.MaxValue, a.SaturatingAdd(1000));
  }

  [Test]
  public void SaturatingSubtract_UShort_Underflow_ReturnsMinValue() {
    ushort a = 100;
    Assert.AreEqual(ushort.MinValue, a.SaturatingSubtract(200));
  }

  [Test]
  public void SaturatingMultiply_UShort_Overflow_ReturnsMaxValue() {
    ushort a = 1000;
    Assert.AreEqual(ushort.MaxValue, a.SaturatingMultiply(100));
  }

  #endregion

  #region short

  [Test]
  public void SaturatingAdd_Short_PositiveOverflow_ReturnsMaxValue() {
    short a = 32000;
    Assert.AreEqual(short.MaxValue, a.SaturatingAdd(1000));
  }

  [Test]
  public void SaturatingAdd_Short_NegativeUnderflow_ReturnsMinValue() {
    short a = -32000;
    Assert.AreEqual(short.MinValue, a.SaturatingAdd(-1000));
  }

  [Test]
  public void SaturatingSubtract_Short_PositiveOverflow_ReturnsMaxValue() {
    short a = 30000;
    Assert.AreEqual(short.MaxValue, a.SaturatingSubtract(-10000));
  }

  [Test]
  public void SaturatingSubtract_Short_NegativeUnderflow_ReturnsMinValue() {
    short a = -30000;
    Assert.AreEqual(short.MinValue, a.SaturatingSubtract(10000));
  }

  [Test]
  public void SaturatingMultiply_Short_PositiveOverflow_ReturnsMaxValue() {
    short a = 20000;
    Assert.AreEqual(short.MaxValue, a.SaturatingMultiply(2));
  }

  [Test]
  public void SaturatingMultiply_Short_NegativeUnderflow_ReturnsMinValue() {
    short a = 20000;
    Assert.AreEqual(short.MinValue, a.SaturatingMultiply(-2));
  }

  [Test]
  public void SaturatingDivide_Short_MinValueDividedByMinusOne_ReturnsMaxValue() {
    short a = short.MinValue;
    Assert.AreEqual(short.MaxValue, a.SaturatingDivide(-1));
  }

  [Test]
  public void SaturatingNegate_Short_MinValue_ReturnsMaxValue() {
    short a = short.MinValue;
    Assert.AreEqual(short.MaxValue, a.SaturatingNegate());
  }

  #endregion

  #region uint

  [Test]
  public void SaturatingAdd_UInt_NormalCase_ReturnsSum() {
    uint a = 100;
    Assert.AreEqual(150u, a.SaturatingAdd(50));
  }

  [Test]
  public void SaturatingAdd_UInt_Overflow_ReturnsMaxValue() {
    uint a = uint.MaxValue - 5;
    Assert.AreEqual(uint.MaxValue, a.SaturatingAdd(10));
  }

  [Test]
  public void SaturatingSubtract_UInt_Underflow_ReturnsMinValue() {
    uint a = 5;
    Assert.AreEqual(uint.MinValue, a.SaturatingSubtract(10));
  }

  [Test]
  public void SaturatingMultiply_UInt_Overflow_ReturnsMaxValue() {
    uint a = uint.MaxValue / 2;
    Assert.AreEqual(uint.MaxValue, a.SaturatingMultiply(3));
  }

  #endregion

  #region int

  [Test]
  public void SaturatingAdd_Int_NormalCase_ReturnsSum() {
    var a = 100;
    Assert.AreEqual(150, a.SaturatingAdd(50));
  }

  [Test]
  public void SaturatingAdd_Int_PositiveOverflow_ReturnsMaxValue() {
    var a = int.MaxValue - 5;
    Assert.AreEqual(int.MaxValue, a.SaturatingAdd(10));
  }

  [Test]
  public void SaturatingAdd_Int_NegativeUnderflow_ReturnsMinValue() {
    var a = int.MinValue + 5;
    Assert.AreEqual(int.MinValue, a.SaturatingAdd(-10));
  }

  [Test]
  public void SaturatingSubtract_Int_PositiveOverflow_ReturnsMaxValue() {
    var a = int.MaxValue - 5;
    Assert.AreEqual(int.MaxValue, a.SaturatingSubtract(-10));
  }

  [Test]
  public void SaturatingSubtract_Int_NegativeUnderflow_ReturnsMinValue() {
    var a = int.MinValue + 5;
    Assert.AreEqual(int.MinValue, a.SaturatingSubtract(10));
  }

  [Test]
  public void SaturatingMultiply_Int_PositiveOverflow_ReturnsMaxValue() {
    var a = int.MaxValue / 2;
    Assert.AreEqual(int.MaxValue, a.SaturatingMultiply(3));
  }

  [Test]
  public void SaturatingMultiply_Int_NegativeUnderflow_ReturnsMinValue() {
    var a = int.MaxValue / 2;
    Assert.AreEqual(int.MinValue, a.SaturatingMultiply(-3));
  }

  [Test]
  public void SaturatingMultiply_Int_NegativeTimesNegative_PositiveOverflow_ReturnsMaxValue() {
    var a = int.MinValue / 2;
    Assert.AreEqual(int.MaxValue, a.SaturatingMultiply(-3));
  }

  [Test]
  public void SaturatingDivide_Int_MinValueDividedByMinusOne_ReturnsMaxValue() {
    var a = int.MinValue;
    Assert.AreEqual(int.MaxValue, a.SaturatingDivide(-1));
  }

  [Test]
  public void SaturatingDivide_Int_NormalCase_ReturnsQuotient() {
    var a = 100;
    Assert.AreEqual(20, a.SaturatingDivide(5));
  }

  [Test]
  public void SaturatingNegate_Int_MinValue_ReturnsMaxValue() {
    var a = int.MinValue;
    Assert.AreEqual(int.MaxValue, a.SaturatingNegate());
  }

  [Test]
  public void SaturatingNegate_Int_NormalCase_ReturnsNegation() {
    var a = 50;
    Assert.AreEqual(-50, a.SaturatingNegate());
  }

  #endregion

  #region ulong

  [Test]
  public void SaturatingAdd_ULong_NormalCase_ReturnsSum() {
    ulong a = 100;
    Assert.AreEqual(150UL, a.SaturatingAdd(50));
  }

  [Test]
  public void SaturatingAdd_ULong_Overflow_ReturnsMaxValue() {
    ulong a = ulong.MaxValue - 5;
    Assert.AreEqual(ulong.MaxValue, a.SaturatingAdd(10));
  }

  [Test]
  public void SaturatingSubtract_ULong_Underflow_ReturnsMinValue() {
    ulong a = 5;
    Assert.AreEqual(ulong.MinValue, a.SaturatingSubtract(10));
  }

  [Test]
  public void SaturatingMultiply_ULong_NormalCase_ReturnsProduct() {
    ulong a = 100;
    Assert.AreEqual(500UL, a.SaturatingMultiply(5));
  }

  [Test]
  public void SaturatingMultiply_ULong_Overflow_ReturnsMaxValue() {
    ulong a = ulong.MaxValue / 2;
    Assert.AreEqual(ulong.MaxValue, a.SaturatingMultiply(3));
  }

  [Test]
  public void SaturatingMultiply_ULong_ByZero_ReturnsZero() {
    ulong a = ulong.MaxValue;
    Assert.AreEqual(0UL, a.SaturatingMultiply(0));
  }

  #endregion

  #region long

  [Test]
  public void SaturatingAdd_Long_NormalCase_ReturnsSum() {
    long a = 100;
    Assert.AreEqual(150L, a.SaturatingAdd(50));
  }

  [Test]
  public void SaturatingAdd_Long_PositiveOverflow_ReturnsMaxValue() {
    long a = long.MaxValue - 5;
    Assert.AreEqual(long.MaxValue, a.SaturatingAdd(10));
  }

  [Test]
  public void SaturatingAdd_Long_NegativeUnderflow_ReturnsMinValue() {
    long a = long.MinValue + 5;
    Assert.AreEqual(long.MinValue, a.SaturatingAdd(-10));
  }

  [Test]
  public void SaturatingSubtract_Long_PositiveOverflow_ReturnsMaxValue() {
    long a = long.MaxValue - 5;
    Assert.AreEqual(long.MaxValue, a.SaturatingSubtract(-10));
  }

  [Test]
  public void SaturatingSubtract_Long_NegativeUnderflow_ReturnsMinValue() {
    long a = long.MinValue + 5;
    Assert.AreEqual(long.MinValue, a.SaturatingSubtract(10));
  }

  [Test]
  public void SaturatingMultiply_Long_PositiveOverflow_ReturnsMaxValue() {
    long a = long.MaxValue / 2;
    Assert.AreEqual(long.MaxValue, a.SaturatingMultiply(3));
  }

  [Test]
  public void SaturatingMultiply_Long_NegativeUnderflow_ReturnsMinValue() {
    long a = long.MaxValue / 2;
    Assert.AreEqual(long.MinValue, a.SaturatingMultiply(-3));
  }

  [Test]
  public void SaturatingMultiply_Long_NegativeTimesNegative_PositiveOverflow_ReturnsMaxValue() {
    long a = long.MinValue / 2;
    Assert.AreEqual(long.MaxValue, a.SaturatingMultiply(-3));
  }

  [Test]
  public void SaturatingMultiply_Long_ByZero_ReturnsZero() {
    long a = long.MaxValue;
    Assert.AreEqual(0L, a.SaturatingMultiply(0));
  }

  [Test]
  public void SaturatingDivide_Long_MinValueDividedByMinusOne_ReturnsMaxValue() {
    long a = long.MinValue;
    Assert.AreEqual(long.MaxValue, a.SaturatingDivide(-1));
  }

  [Test]
  public void SaturatingNegate_Long_MinValue_ReturnsMaxValue() {
    long a = long.MinValue;
    Assert.AreEqual(long.MaxValue, a.SaturatingNegate());
  }

  #endregion

  #region UInt96

  [Test]
  public void SaturatingAdd_UInt96_NormalCase_ReturnsSum() {
    UInt96 a = new(0, 100);
    UInt96 b = new(0, 50);
    Assert.AreEqual(new UInt96(0, 150), a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingAdd_UInt96_Overflow_ReturnsMaxValue() {
    var a = UInt96.MaxValue;
    var b = UInt96.One;
    Assert.AreEqual(UInt96.MaxValue, a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingSubtract_UInt96_Underflow_ReturnsMinValue() {
    var a = UInt96.One;
    var b = new UInt96(0, 10);
    Assert.AreEqual(UInt96.MinValue, a.SaturatingSubtract(b));
  }

  [Test]
  public void SaturatingMultiply_UInt96_ByZero_ReturnsZero() {
    var a = UInt96.MaxValue;
    Assert.AreEqual(UInt96.Zero, a.SaturatingMultiply(UInt96.Zero));
  }

  #endregion

  #region Int96

  [Test]
  public void SaturatingAdd_Int96_PositiveOverflow_ReturnsMaxValue() {
    var a = Int96.MaxValue;
    var b = Int96.One;
    Assert.AreEqual(Int96.MaxValue, a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingAdd_Int96_NegativeUnderflow_ReturnsMinValue() {
    var a = Int96.MinValue;
    var b = Int96.NegativeOne;
    Assert.AreEqual(Int96.MinValue, a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingNegate_Int96_MinValue_ReturnsMaxValue() {
    var a = Int96.MinValue;
    Assert.AreEqual(Int96.MaxValue, a.SaturatingNegate());
  }

  [Test]
  public void SaturatingDivide_Int96_MinValueDividedByMinusOne_ReturnsMaxValue() {
    var a = Int96.MinValue;
    Assert.AreEqual(Int96.MaxValue, a.SaturatingDivide(Int96.NegativeOne));
  }

  #endregion

  #region UInt128

  [Test]
  public void SaturatingAdd_UInt128_NormalCase_ReturnsSum() {
    UInt128 a = 100;
    UInt128 b = 50;
    Assert.AreEqual((UInt128)150, a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingAdd_UInt128_Overflow_ReturnsMaxValue() {
    var a = UInt128.MaxValue;
    var b = UInt128.One;
    Assert.AreEqual(UInt128.MaxValue, a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingSubtract_UInt128_Underflow_ReturnsMinValue() {
    UInt128 a = 5;
    UInt128 b = 10;
    Assert.AreEqual(UInt128.MinValue, a.SaturatingSubtract(b));
  }

  [Test]
  public void SaturatingMultiply_UInt128_Overflow_ReturnsMaxValue() {
    var a = UInt128.MaxValue / 2;
    UInt128 b = 3;
    Assert.AreEqual(UInt128.MaxValue, a.SaturatingMultiply(b));
  }

  #endregion

  #region Int128

  [Test]
  public void SaturatingAdd_Int128_PositiveOverflow_ReturnsMaxValue() {
    var a = Int128.MaxValue - 5;
    Int128 b = 10;
    Assert.AreEqual(Int128.MaxValue, a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingAdd_Int128_NegativeUnderflow_ReturnsMinValue() {
    var a = Int128.MinValue + 5;
    Int128 b = -10;
    Assert.AreEqual(Int128.MinValue, a.SaturatingAdd(b));
  }

  [Test]
  public void SaturatingSubtract_Int128_PositiveOverflow_ReturnsMaxValue() {
    var a = Int128.MaxValue - 5;
    Int128 b = -10;
    Assert.AreEqual(Int128.MaxValue, a.SaturatingSubtract(b));
  }

  [Test]
  public void SaturatingSubtract_Int128_NegativeUnderflow_ReturnsMinValue() {
    var a = Int128.MinValue + 5;
    Int128 b = 10;
    Assert.AreEqual(Int128.MinValue, a.SaturatingSubtract(b));
  }

  [Test]
  public void SaturatingNegate_Int128_MinValue_ReturnsMaxValue() {
    var a = Int128.MinValue;
    Assert.AreEqual(Int128.MaxValue, a.SaturatingNegate());
  }

  [Test]
  public void SaturatingDivide_Int128_MinValueDividedByMinusOne_ReturnsMaxValue() {
    var a = Int128.MinValue;
    Assert.AreEqual(Int128.MaxValue, a.SaturatingDivide(Int128.NegativeOne));
  }

  [Test]
  public void SaturatingMultiply_Int128_PositiveOverflow_ReturnsMaxValue() {
    var a = Int128.MaxValue / 2;
    Int128 b = 3;
    Assert.AreEqual(Int128.MaxValue, a.SaturatingMultiply(b));
  }

  [Test]
  public void SaturatingMultiply_Int128_NegativeUnderflow_ReturnsMinValue() {
    var a = Int128.MaxValue / 2;
    Int128 b = -3;
    Assert.AreEqual(Int128.MinValue, a.SaturatingMultiply(b));
  }

  #endregion

}
