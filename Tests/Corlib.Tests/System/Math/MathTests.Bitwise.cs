using System.Diagnostics;
using NUnit.Framework;

namespace System.MathExtensionsTests;

/// <summary>
///   Tests for bitwise math operations
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Arithmetic Shift Left Tests

  [Test]
  [TestCase((byte)0b00000001, 0, (byte)0b00000001)]
  [TestCase((byte)0b00000001, 1, (byte)0b00000010)]
  [TestCase((byte)0b00000001, 7, (byte)0b10000000)]
  [TestCase((byte)0b00000001, 8, (byte)0b00000000)]
  [TestCase((byte)0b00000001, 16, (byte)0b00000000)]
  [TestCase((byte)0b10101010, 1, (byte)0b01010100)]
  [TestCase((byte)0b11111111, 1, (byte)0b11111110)]
  [Category("HappyPath")]
  [Description("Validates ArithmeticShiftLeft for byte values")]
  public void ArithmeticShiftLeft_Byte_ReturnsCorrectValue(byte input, byte shiftCount, byte expected) {
    // Act
    var result = input.ArithmeticShiftLeft(shiftCount);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((sbyte)0b00000001, 0, (sbyte)0b00000001)]
  [TestCase((sbyte)0b00000001, 1, (sbyte)0b00000010)]
  [TestCase((sbyte)0b01000000, 1, (sbyte)0b00000000)]
  [TestCase((sbyte)-128, 1, (sbyte)-128)] // Sign bit preserved
  [TestCase((sbyte)0b00000001, 7, (sbyte)0b00000000)]
  [Category("HappyPath")]
  [Description("Validates ArithmeticShiftLeft for signed byte values")]
  public void ArithmeticShiftLeft_SByte_ReturnsCorrectValue(sbyte input, byte shiftCount, sbyte expected) {
    // Act
    var result = input.ArithmeticShiftLeft(shiftCount);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((ushort)0b0000000000000001, 0, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b0000000000000001, 1, (ushort)0b0000000000000010)]
  [TestCase((ushort)0b0000000000000001, 15, (ushort)0b1000000000000000)]
  [TestCase((ushort)0b0000000000000001, 16, (ushort)0b0000000000000000)]
  [Category("HappyPath")]
  [Description("Validates ArithmeticShiftLeft for ushort values")]
  public void ArithmeticShiftLeft_UShort_ReturnsCorrectValue(ushort input, byte shiftCount, ushort expected) {
    // Act
    var result = input.ArithmeticShiftLeft(shiftCount);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Arithmetic Shift Right Tests

  [Test]
  [TestCase((byte)0b10000000, 1, (byte)0b01000000)]
  [TestCase((byte)0b10000000, 7, (byte)0b00000001)]
  [TestCase((byte)0b10000000, 8, (byte)0b00000000)]
  [TestCase((byte)0b11111111, 1, (byte)0b01111111)]
  [Category("HappyPath")]
  [Description("Validates ArithmeticShiftRight for byte values")]
  public void ArithmeticShiftRight_Byte_ReturnsCorrectValue(byte input, byte shiftCount, byte expected) {
    // Act
    var result = input.ArithmeticShiftRight(shiftCount);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((sbyte)-128, 1, (sbyte)-64)] // Sign bit extended
  [TestCase((sbyte)-1, 1, (sbyte)-1)] // All bits stay 1
  [TestCase((sbyte)64, 1, (sbyte)32)]
  [TestCase((sbyte)1, 1, (sbyte)0)]
  [Category("HappyPath")]
  [Category("EdgeCase")]
  [Description("Validates ArithmeticShiftRight preserves sign for signed values")]
  public void ArithmeticShiftRight_SByte_PreservesSign(sbyte input, byte shiftCount, sbyte expected) {
    // Act
    var result = input.ArithmeticShiftRight(shiftCount);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Rotate Tests

  [Test]
  [TestCase((byte)0b00000001, 1, (byte)0b00000010)]
  [TestCase((byte)0b10000000, 1, (byte)0b00000001)] // Wraps around
  [TestCase((byte)0b10101010, 4, (byte)0b10101010)] // Full nibble rotation
  [TestCase((byte)0b11110000, 4, (byte)0b00001111)] // Nibble swap
  [Category("HappyPath")]
  [Description("Validates RotateLeft for byte values")]
  public void RotateLeft_Byte_ReturnsCorrectValue(byte input, byte rotateCount, byte expected) {
    // Act
    var result = input.RotateLeft(rotateCount);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((byte)0b00000001, 1, (byte)0b10000000)] // Wraps around
  [TestCase((byte)0b00000010, 1, (byte)0b00000001)]
  [TestCase((byte)0b10101010, 4, (byte)0b10101010)] // Full nibble rotation
  [Category("HappyPath")]
  [Description("Validates RotateRight for byte values")]
  public void RotateRight_Byte_ReturnsCorrectValue(byte input, byte rotateCount, byte expected) {
    // Act
    var result = input.RotateRight(rotateCount);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Rotate handles full rotation")]
  public void Rotate_FullRotation_ReturnsSameValue() {
    // Arrange
    byte value = 0b10101010;

    // Act
    var rotatedLeft8 = value.RotateLeft(8);
    var rotatedRight8 = value.RotateRight(8);

    // Assert
    Assert.That(rotatedLeft8, Is.EqualTo(value));
    Assert.That(rotatedRight8, Is.EqualTo(value));
  }

  #endregion

  #region Bit Manipulation Tests

  [Test]
  [TestCase((uint)0b00000000, 0, true, (uint)0b00000001)]
  [TestCase((uint)0b00000001, 0, false, (uint)0b00000000)]
  [TestCase((uint)0b11111111, 7, false, (uint)0b01111111)]
  [TestCase((uint)0b00000000, 31, true, (uint)0b10000000_00000000_00000000_00000000)]
  [Category("HappyPath")]
  [Description("Validates SetBit for various bit positions")]
  public void SetBit_VariousPositions_SetsCorrectly(uint input, byte position, bool value, uint expected) {
    // Act
    var result = value ? input.SetBit(position) : input.ClearBit(position);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((uint)0b00000001, 0, true)]
  [TestCase((uint)0b00000001, 1, false)]
  [TestCase((uint)0b10000000_00000000_00000000_00000000, 31, true)]
  [Category("HappyPath")]
  [Description("Validates GetBit returns correct bit value")]
  public void GetBit_VariousPositions_ReturnsCorrectValue(uint input, byte position, bool expected) {
    // Act
    var result = input.GetBit(position);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  [Description("Validates bitwise operations performance")]
  public void BitwiseOperations_ManyIterations_CompletesQuickly() {
    // Arrange
    var sw = Stopwatch.StartNew();
    byte value = 0b10101010;

    // Act
    for (var i = 0; i < 100_000; i++) {
      value = value.RotateLeft(1);
      value = value.ArithmeticShiftRight(1);
      value = value.RotateRight(1);
      value = value.ArithmeticShiftLeft(1);
    }

    sw.Stop();

    // Assert
    Assert.That(
      sw.ElapsedMilliseconds,
      Is.LessThan(100),
      $"100K bitwise operations took {sw.ElapsedMilliseconds}ms"
    );
  }

  #endregion
}
