using NUnit.Framework;

namespace System.MathExtensionsTests;

[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region ArithmeticShiftLeft Tests

  [Test]
  [TestCase((sbyte)1, (byte)1, (sbyte)2)]
  [TestCase((sbyte)1, (byte)2, (sbyte)4)]
  [TestCase((sbyte)0, (byte)5, (sbyte)0)]
  [Category("HappyPath")]
  [Description("Validates sbyte ArithmeticShiftLeft computes correctly")]
  public void ArithmeticShiftLeft_SByte_ComputesCorrectly(sbyte value, byte shift, sbyte expected) {
    var result = value.ArithmeticShiftLeft(shift);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((short)1, (byte)1, (short)2)]
  [TestCase((short)1, (byte)4, (short)16)]
  [Category("HappyPath")]
  [Description("Validates short ArithmeticShiftLeft computes correctly")]
  public void ArithmeticShiftLeft_Short_ComputesCorrectly(short value, byte shift, short expected) {
    var result = value.ArithmeticShiftLeft(shift);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int ArithmeticShiftLeft computes correctly")]
  public void ArithmeticShiftLeft_Int_ComputesCorrectly() {
    Assert.That(1.ArithmeticShiftLeft(1), Is.EqualTo(2));
    Assert.That(1.ArithmeticShiftLeft(4), Is.EqualTo(16));
    Assert.That(0.ArithmeticShiftLeft(10), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates long ArithmeticShiftLeft computes correctly")]
  public void ArithmeticShiftLeft_Long_ComputesCorrectly() {
    Assert.That(1L.ArithmeticShiftLeft(1), Is.EqualTo(2L));
    Assert.That(1L.ArithmeticShiftLeft(4), Is.EqualTo(16L));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates byte ArithmeticShiftLeft computes correctly")]
  public void ArithmeticShiftLeft_Byte_ComputesCorrectly() {
    Assert.That(((byte)1).ArithmeticShiftLeft(1), Is.EqualTo((byte)2));
    Assert.That(((byte)1).ArithmeticShiftLeft(4), Is.EqualTo((byte)16));
    Assert.That(((byte)128).ArithmeticShiftLeft(1), Is.EqualTo((byte)0));
  }

  #endregion

  #region ArithmeticShiftRight Tests

  [Test]
  [TestCase((sbyte)4, (byte)1, (sbyte)2)]
  [TestCase((sbyte)16, (byte)2, (sbyte)4)]
  [TestCase((sbyte)-4, (byte)1, (sbyte)-2)]
  [TestCase((sbyte)-1, (byte)1, (sbyte)-1)] // Sign extension preserves -1
  [Category("HappyPath")]
  [Description("Validates sbyte ArithmeticShiftRight computes correctly")]
  public void ArithmeticShiftRight_SByte_ComputesCorrectly(sbyte value, byte shift, sbyte expected) {
    var result = value.ArithmeticShiftRight(shift);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int ArithmeticShiftRight computes correctly")]
  public void ArithmeticShiftRight_Int_ComputesCorrectly() {
    Assert.That(4.ArithmeticShiftRight(1), Is.EqualTo(2));
    Assert.That(16.ArithmeticShiftRight(2), Is.EqualTo(4));
    Assert.That((-4).ArithmeticShiftRight(1), Is.EqualTo(-2));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates long ArithmeticShiftRight computes correctly")]
  public void ArithmeticShiftRight_Long_ComputesCorrectly() {
    Assert.That(4L.ArithmeticShiftRight(1), Is.EqualTo(2L));
    Assert.That(16L.ArithmeticShiftRight(2), Is.EqualTo(4L));
    Assert.That((-4L).ArithmeticShiftRight(1), Is.EqualTo(-2L));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates byte ArithmeticShiftRight computes correctly")]
  public void ArithmeticShiftRight_Byte_ComputesCorrectly() {
    Assert.That(((byte)4).ArithmeticShiftRight(1), Is.EqualTo((byte)2));
    Assert.That(((byte)16).ArithmeticShiftRight(2), Is.EqualTo((byte)4));
    Assert.That(((byte)255).ArithmeticShiftRight(1), Is.EqualTo((byte)127));
  }

  #endregion

  #region LogicalShiftLeft Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates sbyte LogicalShiftLeft computes correctly")]
  public void LogicalShiftLeft_SByte_ComputesCorrectly() {
    Assert.That(((sbyte)1).LogicalShiftLeft(1), Is.EqualTo((sbyte)2));
    Assert.That(((sbyte)1).LogicalShiftLeft(4), Is.EqualTo((sbyte)16));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates short LogicalShiftLeft computes correctly")]
  public void LogicalShiftLeft_Short_ComputesCorrectly() {
    Assert.That(((short)1).LogicalShiftLeft(1), Is.EqualTo((short)2));
    Assert.That(((short)1).LogicalShiftLeft(4), Is.EqualTo((short)16));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int LogicalShiftLeft computes correctly")]
  public void LogicalShiftLeft_Int_ComputesCorrectly() {
    Assert.That(1.LogicalShiftLeft(1), Is.EqualTo(2));
    Assert.That(1.LogicalShiftLeft(4), Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates long LogicalShiftLeft computes correctly")]
  public void LogicalShiftLeft_Long_ComputesCorrectly() {
    Assert.That(1L.LogicalShiftLeft(1), Is.EqualTo(2L));
    Assert.That(1L.LogicalShiftLeft(4), Is.EqualTo(16L));
  }

  #endregion

  #region LogicalShiftRight Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates sbyte LogicalShiftRight computes correctly")]
  public void LogicalShiftRight_SByte_ComputesCorrectly() {
    Assert.That(((sbyte)4).LogicalShiftRight(1), Is.EqualTo((sbyte)2));
    Assert.That(((sbyte)16).LogicalShiftRight(2), Is.EqualTo((sbyte)4));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates short LogicalShiftRight computes correctly")]
  public void LogicalShiftRight_Short_ComputesCorrectly() {
    Assert.That(((short)4).LogicalShiftRight(1), Is.EqualTo((short)2));
    Assert.That(((short)16).LogicalShiftRight(2), Is.EqualTo((short)4));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int LogicalShiftRight computes correctly")]
  public void LogicalShiftRight_Int_ComputesCorrectly() {
    Assert.That(4.LogicalShiftRight(1), Is.EqualTo(2));
    Assert.That(16.LogicalShiftRight(2), Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates long LogicalShiftRight computes correctly")]
  public void LogicalShiftRight_Long_ComputesCorrectly() {
    Assert.That(4L.LogicalShiftRight(1), Is.EqualTo(2L));
    Assert.That(16L.LogicalShiftRight(2), Is.EqualTo(4L));
  }

  #endregion

  #region EdgeCase Tests - Shift Boundaries

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ArithmeticShiftLeft with zero shift returns original value")]
  public void ArithmeticShiftLeft_Int_ZeroShift_ReturnsOriginal() {
    Assert.That(12345.ArithmeticShiftLeft(0), Is.EqualTo(12345));
    Assert.That((-12345).ArithmeticShiftLeft(0), Is.EqualTo(-12345));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ArithmeticShiftRight with zero shift returns original value")]
  public void ArithmeticShiftRight_Int_ZeroShift_ReturnsOriginal() {
    Assert.That(12345.ArithmeticShiftRight(0), Is.EqualTo(12345));
    Assert.That((-12345).ArithmeticShiftRight(0), Is.EqualTo(-12345));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ArithmeticShiftLeft returns zero for large shifts")]
  public void ArithmeticShiftLeft_Int_LargeShift_ReturnsZero() {
    // Implementation returns 0 when count >= type_size
    Assert.That(1.ArithmeticShiftLeft(32), Is.EqualTo(0));
    Assert.That(int.MaxValue.ArithmeticShiftLeft(32), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ArithmeticShiftRight returns zero for large shifts")]
  public void ArithmeticShiftRight_Int_LargeShift_ReturnsZero() {
    // Implementation returns 0 when count >= type_size
    Assert.That((-1).ArithmeticShiftRight(32), Is.EqualTo(0));
    Assert.That(int.MaxValue.ArithmeticShiftRight(32), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ArithmeticShiftRight handles small shifts on negative numbers")]
  public void ArithmeticShiftRight_Int_Negative_SmallShifts() {
    Assert.That((-4).ArithmeticShiftRight(1), Is.EqualTo(-2));
    Assert.That((-128).ArithmeticShiftRight(1), Is.EqualTo(-64));
    Assert.That((-1024).ArithmeticShiftRight(2), Is.EqualTo(-256));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates LogicalShiftRight clears high bits for negative numbers")]
  public void LogicalShiftRight_Int_Negative_ClearsHighBits() {
    Assert.That((-1).LogicalShiftRight(1), Is.EqualTo(int.MaxValue));
    Assert.That(int.MinValue.LogicalShiftRight(31), Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates shifts with boundary values")]
  public void Shifts_Int_BoundaryValues_HandleCorrectly() {
    Assert.That(int.MaxValue.ArithmeticShiftRight(1), Is.EqualTo(int.MaxValue / 2));
    Assert.That(int.MinValue.ArithmeticShiftRight(1), Is.EqualTo(int.MinValue / 2));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates long shifts return zero for large shifts")]
  public void ArithmeticShiftLeft_Long_LargeShift_ReturnsZero() {
    // Implementation returns 0 when count >= type_size
    Assert.That(1L.ArithmeticShiftLeft(64), Is.EqualTo(0L));
    Assert.That(long.MaxValue.ArithmeticShiftLeft(64), Is.EqualTo(0L));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates byte shifts return zero for large shifts")]
  public void ArithmeticShiftLeft_Byte_LargeShift_ReturnsZero() {
    // Implementation returns 0 when count >= type_size
    Assert.That(((byte)1).ArithmeticShiftLeft(8), Is.EqualTo((byte)0));
    Assert.That(((byte)255).ArithmeticShiftLeft(8), Is.EqualTo((byte)0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates sbyte arithmetic shift right for small shifts")]
  public void ArithmeticShiftRight_SByte_SmallShifts_WorksCorrectly() {
    Assert.That(((sbyte)-128).ArithmeticShiftRight(1), Is.EqualTo((sbyte)-64));
    Assert.That(((sbyte)-64).ArithmeticShiftRight(1), Is.EqualTo((sbyte)-32));
    Assert.That(((sbyte)-2).ArithmeticShiftRight(1), Is.EqualTo((sbyte)-1));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates short shifts return zero for large shifts")]
  public void ArithmeticShiftLeft_Short_LargeShift_ReturnsZero() {
    // Implementation returns 0 when count >= type_size
    Assert.That(((short)1).ArithmeticShiftLeft(16), Is.EqualTo((short)0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates logical shift left with zero value")]
  public void LogicalShiftLeft_Int_ZeroValue_ReturnsZero() {
    Assert.That(0.LogicalShiftLeft(1), Is.EqualTo(0));
    Assert.That(0.LogicalShiftLeft(31), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates logical shift right with zero value")]
  public void LogicalShiftRight_Int_ZeroValue_ReturnsZero() {
    Assert.That(0.LogicalShiftRight(1), Is.EqualTo(0));
    Assert.That(0.LogicalShiftRight(31), Is.EqualTo(0));
  }

  #endregion
}
