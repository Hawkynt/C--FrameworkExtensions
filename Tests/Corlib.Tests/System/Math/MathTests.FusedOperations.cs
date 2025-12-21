using NUnit.Framework;

namespace System.MathExtensionsTests;

[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region FusedMultiplyAdd Tests - All numeric types

  [Test]
  [TestCase((sbyte)2, (sbyte)3, (sbyte)4, (sbyte)10)]   // 2*3+4 = 10
  [TestCase((sbyte)0, (sbyte)5, (sbyte)3, (sbyte)3)]    // 0*5+3 = 3
  [TestCase((sbyte)-2, (sbyte)3, (sbyte)4, (sbyte)-2)]  // -2*3+4 = -2
  [TestCase((sbyte)5, (sbyte)-2, (sbyte)3, (sbyte)-7)]  // 5*-2+3 = -7
  [Category("HappyPath")]
  [Description("Validates sbyte FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_SByte_ComputesCorrectly(sbyte value, sbyte factor, sbyte addend, sbyte expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((byte)2, (byte)3, (byte)4, (byte)10)]
  [TestCase((byte)0, (byte)5, (byte)3, (byte)3)]
  [TestCase((byte)10, (byte)2, (byte)5, (byte)25)]
  [Category("HappyPath")]
  [Description("Validates byte FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_Byte_ComputesCorrectly(byte value, byte factor, byte addend, byte expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((short)2, (short)3, (short)4, (short)10)]
  [TestCase((short)0, (short)5, (short)3, (short)3)]
  [TestCase((short)-10, (short)5, (short)60, (short)10)]
  [Category("HappyPath")]
  [Description("Validates short FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_Short_ComputesCorrectly(short value, short factor, short addend, short expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((ushort)2, (ushort)3, (ushort)4, (ushort)10)]
  [TestCase((ushort)0, (ushort)5, (ushort)3, (ushort)3)]
  [TestCase((ushort)100, (ushort)10, (ushort)50, (ushort)1050)]
  [Category("HappyPath")]
  [Description("Validates ushort FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_UShort_ComputesCorrectly(ushort value, ushort factor, ushort addend, ushort expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2, 3, 4, 10)]
  [TestCase(0, 5, 3, 3)]
  [TestCase(-10, 5, 60, 10)]
  [TestCase(1000, 1000, 1000, 1001000)]
  [Category("HappyPath")]
  [Description("Validates int FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_Int_ComputesCorrectly(int value, int factor, int addend, int expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2U, 3U, 4U, 10U)]
  [TestCase(0U, 5U, 3U, 3U)]
  [TestCase(1000U, 1000U, 1000U, 1001000U)]
  [Category("HappyPath")]
  [Description("Validates uint FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_UInt_ComputesCorrectly(uint value, uint factor, uint addend, uint expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2L, 3L, 4L, 10L)]
  [TestCase(0L, 5L, 3L, 3L)]
  [TestCase(-10L, 5L, 60L, 10L)]
  [TestCase(1000000L, 1000000L, 1000000L, 1000001000000L)]
  [Category("HappyPath")]
  [Description("Validates long FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_Long_ComputesCorrectly(long value, long factor, long addend, long expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2UL, 3UL, 4UL, 10UL)]
  [TestCase(0UL, 5UL, 3UL, 3UL)]
  [TestCase(1000000UL, 1000000UL, 1000000UL, 1000001000000UL)]
  [Category("HappyPath")]
  [Description("Validates ulong FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_ULong_ComputesCorrectly(ulong value, ulong factor, ulong addend, ulong expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2.0f, 3.0f, 4.0f, 10.0f)]
  [TestCase(0.0f, 5.0f, 3.0f, 3.0f)]
  [TestCase(-2.5f, 4.0f, 15.0f, 5.0f)]
  [TestCase(1.5f, 2.0f, 0.5f, 3.5f)]
  [Category("HappyPath")]
  [Description("Validates float FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_Float_ComputesCorrectly(float value, float factor, float addend, float expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected).Within(1e-6f));
  }

  [Test]
  [TestCase(2.0, 3.0, 4.0, 10.0)]
  [TestCase(0.0, 5.0, 3.0, 3.0)]
  [TestCase(-2.5, 4.0, 15.0, 5.0)]
  [TestCase(1.5, 2.0, 0.5, 3.5)]
  [Category("HappyPath")]
  [Description("Validates double FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_Double_ComputesCorrectly(double value, double factor, double addend, double expected) {
    var result = value.FusedMultiplyAdd(factor, addend);
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates decimal FusedMultiplyAdd computes correctly")]
  public void FusedMultiplyAdd_Decimal_ComputesCorrectly() {
    Assert.That(2m.FusedMultiplyAdd(3m, 4m), Is.EqualTo(10m));
    Assert.That(0m.FusedMultiplyAdd(5m, 3m), Is.EqualTo(3m));
    Assert.That((-2.5m).FusedMultiplyAdd(4m, 15m), Is.EqualTo(5m));
    Assert.That(1.5m.FusedMultiplyAdd(2m, 0.5m), Is.EqualTo(3.5m));
  }

  #endregion

  #region FusedMultiplySubtract Tests

  [Test]
  [TestCase(2, 3, 1, 5)]   // 2*3-1 = 5
  [TestCase(5, 4, 10, 10)] // 5*4-10 = 10
  [TestCase(0, 5, 3, -3)]  // 0*5-3 = -3
  [Category("HappyPath")]
  [Description("Validates int FusedMultiplySubtract computes correctly")]
  public void FusedMultiplySubtract_Int_ComputesCorrectly(int value, int factor, int subtrahend, int expected) {
    var result = value.FusedMultiplySubtract(factor, subtrahend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2.0f, 3.0f, 1.0f, 5.0f)]
  [TestCase(5.0f, 4.0f, 10.0f, 10.0f)]
  [TestCase(1.5f, 2.0f, 0.5f, 2.5f)]
  [Category("HappyPath")]
  [Description("Validates float FusedMultiplySubtract computes correctly")]
  public void FusedMultiplySubtract_Float_ComputesCorrectly(float value, float factor, float subtrahend, float expected) {
    var result = value.FusedMultiplySubtract(factor, subtrahend);
    Assert.That(result, Is.EqualTo(expected).Within(1e-6f));
  }

  [Test]
  [TestCase(2.0, 3.0, 1.0, 5.0)]
  [TestCase(5.0, 4.0, 10.0, 10.0)]
  [TestCase(1.5, 2.0, 0.5, 2.5)]
  [Category("HappyPath")]
  [Description("Validates double FusedMultiplySubtract computes correctly")]
  public void FusedMultiplySubtract_Double_ComputesCorrectly(double value, double factor, double subtrahend, double expected) {
    var result = value.FusedMultiplySubtract(factor, subtrahend);
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region FusedDivideAdd Tests

  [Test]
  [TestCase(10, 2, 3, 8)]   // 10/2+3 = 8
  [TestCase(20, 4, 5, 10)]  // 20/4+5 = 10
  [TestCase(15, 3, 0, 5)]   // 15/3+0 = 5
  [Category("HappyPath")]
  [Description("Validates int FusedDivideAdd computes correctly")]
  public void FusedDivideAdd_Int_ComputesCorrectly(int value, int divisor, int addend, int expected) {
    var result = value.FusedDivideAdd(divisor, addend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(10.0f, 2.0f, 3.0f, 8.0f)]
  [TestCase(20.0f, 4.0f, 5.0f, 10.0f)]
  [TestCase(9.0f, 3.0f, 0.5f, 3.5f)]
  [Category("HappyPath")]
  [Description("Validates float FusedDivideAdd computes correctly")]
  public void FusedDivideAdd_Float_ComputesCorrectly(float value, float divisor, float addend, float expected) {
    var result = value.FusedDivideAdd(divisor, addend);
    // Uses ReciprocalEstimate internally so larger tolerance needed
    Assert.That(result, Is.EqualTo(expected).Within(0.01f));
  }

  [Test]
  [TestCase(10.0, 2.0, 3.0, 8.0)]
  [TestCase(20.0, 4.0, 5.0, 10.0)]
  [TestCase(9.0, 3.0, 0.5, 3.5)]
  [Category("HappyPath")]
  [Description("Validates double FusedDivideAdd computes correctly")]
  public void FusedDivideAdd_Double_ComputesCorrectly(double value, double divisor, double addend, double expected) {
    var result = value.FusedDivideAdd(divisor, addend);
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region FusedDivideSubtract Tests

  [Test]
  [TestCase(10, 2, 3, 2)]   // 10/2-3 = 2
  [TestCase(20, 4, 5, 0)]   // 20/4-5 = 0
  [TestCase(15, 3, 2, 3)]   // 15/3-2 = 3
  [Category("HappyPath")]
  [Description("Validates int FusedDivideSubtract computes correctly")]
  public void FusedDivideSubtract_Int_ComputesCorrectly(int value, int divisor, int subtrahend, int expected) {
    var result = value.FusedDivideSubtract(divisor, subtrahend);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(10.0f, 2.0f, 3.0f, 2.0f)]
  [TestCase(20.0f, 4.0f, 5.0f, 0.0f)]
  [TestCase(9.0f, 3.0f, 0.5f, 2.5f)]
  [Category("HappyPath")]
  [Description("Validates float FusedDivideSubtract computes correctly")]
  public void FusedDivideSubtract_Float_ComputesCorrectly(float value, float divisor, float subtrahend, float expected) {
    var result = value.FusedDivideSubtract(divisor, subtrahend);
    // Uses ReciprocalEstimate internally so larger tolerance needed
    Assert.That(result, Is.EqualTo(expected).Within(0.01f));
  }

  [Test]
  [TestCase(10.0, 2.0, 3.0, 2.0)]
  [TestCase(20.0, 4.0, 5.0, 0.0)]
  [TestCase(9.0, 3.0, 0.5, 2.5)]
  [Category("HappyPath")]
  [Description("Validates double FusedDivideSubtract computes correctly")]
  public void FusedDivideSubtract_Double_ComputesCorrectly(double value, double divisor, double subtrahend, double expected) {
    var result = value.FusedDivideSubtract(divisor, subtrahend);
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region EdgeCase Tests - Fused Operations

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedMultiplyAdd with zero factor returns addend")]
  public void FusedMultiplyAdd_Double_ZeroFactor_ReturnsAddend() {
    Assert.That(100.0.FusedMultiplyAdd(0.0, 42.0), Is.EqualTo(42.0).Within(1e-10));
    Assert.That(0.0.FusedMultiplyAdd(100.0, 42.0), Is.EqualTo(42.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedMultiplyAdd with zero addend returns product")]
  public void FusedMultiplyAdd_Double_ZeroAddend_ReturnsProduct() {
    Assert.That(5.0.FusedMultiplyAdd(4.0, 0.0), Is.EqualTo(20.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedMultiplyAdd with negative values")]
  public void FusedMultiplyAdd_Double_NegativeValues_ComputesCorrectly() {
    Assert.That((-5.0).FusedMultiplyAdd(-4.0, -10.0), Is.EqualTo(10.0).Within(1e-10)); // 20 - 10 = 10
    Assert.That((-5.0).FusedMultiplyAdd(4.0, 10.0), Is.EqualTo(-10.0).Within(1e-10)); // -20 + 10 = -10
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedMultiplyAdd handles infinity correctly")]
  public void FusedMultiplyAdd_Double_Infinity_HandlesCorrectly() {
    Assert.That(double.PositiveInfinity.FusedMultiplyAdd(1.0, 0.0), Is.EqualTo(double.PositiveInfinity));
    Assert.That(1.0.FusedMultiplyAdd(double.PositiveInfinity, 0.0), Is.EqualTo(double.PositiveInfinity));
    Assert.That(1.0.FusedMultiplyAdd(1.0, double.PositiveInfinity), Is.EqualTo(double.PositiveInfinity));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedMultiplyAdd handles NaN correctly")]
  public void FusedMultiplyAdd_Double_NaN_ReturnsNaN() {
    Assert.That(double.NaN.FusedMultiplyAdd(1.0, 1.0).IsNaN(), Is.True);
    Assert.That(1.0.FusedMultiplyAdd(double.NaN, 1.0).IsNaN(), Is.True);
    Assert.That(1.0.FusedMultiplyAdd(1.0, double.NaN).IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedMultiplySubtract with zero subtrahend returns product")]
  public void FusedMultiplySubtract_Double_ZeroSubtrahend_ReturnsProduct() {
    Assert.That(5.0.FusedMultiplySubtract(4.0, 0.0), Is.EqualTo(20.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedDivideAdd with divisor of 1 returns sum")]
  public void FusedDivideAdd_Double_DivisorOne_ReturnsSumOfQuotientAndAddend() {
    Assert.That(10.0.FusedDivideAdd(1.0, 5.0), Is.EqualTo(15.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedDivideAdd handles zero dividend")]
  public void FusedDivideAdd_Double_ZeroDividend_ReturnsAddend() {
    Assert.That(0.0.FusedDivideAdd(5.0, 10.0), Is.EqualTo(10.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates FusedDivideSubtract with divisor of 1")]
  public void FusedDivideSubtract_Double_DivisorOne_ReturnsDifference() {
    Assert.That(10.0.FusedDivideSubtract(1.0, 3.0), Is.EqualTo(7.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates int FusedMultiplyAdd handles boundary values")]
  public void FusedMultiplyAdd_Int_BoundaryValues_ComputesCorrectly() {
    Assert.That(0.FusedMultiplyAdd(int.MaxValue, int.MaxValue), Is.EqualTo(int.MaxValue));
    Assert.That(1.FusedMultiplyAdd(1, int.MaxValue - 1), Is.EqualTo(int.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates long FusedMultiplyAdd with large values")]
  public void FusedMultiplyAdd_Long_LargeValues_ComputesCorrectly() {
    Assert.That(0L.FusedMultiplyAdd(long.MaxValue, 100L), Is.EqualTo(100L));
    Assert.That(1L.FusedMultiplyAdd(0L, long.MaxValue), Is.EqualTo(long.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates decimal FusedMultiplyAdd precision")]
  public void FusedMultiplyAdd_Decimal_HighPrecision_MaintainsPrecision() {
    Assert.That(0.1m.FusedMultiplyAdd(0.2m, 0.3m), Is.EqualTo(0.32m));
    Assert.That(1.000001m.FusedMultiplyAdd(1.000001m, 0.000001m), Is.EqualTo(1.000003000001m));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates float FusedMultiplyAdd handles small numbers")]
  public void FusedMultiplyAdd_Float_SmallNumbers_ComputesCorrectly() {
    Assert.That(1e-20f.FusedMultiplyAdd(1e-20f, 0f), Is.EqualTo(1e-40f).Within(1e-45f));
  }

  #endregion

  #region Exception Tests - Division by Zero

  [Test]
  [Category("Exception")]
  [Description("Validates FusedDivideAdd with zero divisor returns infinity for double")]
  public void FusedDivideAdd_Double_ZeroDivisor_ReturnsInfinity() {
    Assert.That(10.0.FusedDivideAdd(0.0, 5.0), Is.EqualTo(double.PositiveInfinity));
    Assert.That((-10.0).FusedDivideAdd(0.0, 5.0), Is.EqualTo(double.NegativeInfinity));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates FusedDivideSubtract with zero divisor returns infinity for double")]
  public void FusedDivideSubtract_Double_ZeroDivisor_ReturnsInfinity() {
    Assert.That(10.0.FusedDivideSubtract(0.0, 5.0), Is.EqualTo(double.PositiveInfinity));
    Assert.That((-10.0).FusedDivideSubtract(0.0, 5.0), Is.EqualTo(double.NegativeInfinity));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates int FusedDivideAdd with zero divisor throws")]
  public void FusedDivideAdd_Int_ZeroDivisor_ThrowsDivideByZero() {
    Assert.Throws<DivideByZeroException>(() => 10.FusedDivideAdd(0, 5));
  }

  [Test]
  [Category("Exception")]
  [Description("Validates int FusedDivideSubtract with zero divisor throws")]
  public void FusedDivideSubtract_Int_ZeroDivisor_ThrowsDivideByZero() {
    Assert.Throws<DivideByZeroException>(() => 10.FusedDivideSubtract(0, 5));
  }

  #endregion
}
