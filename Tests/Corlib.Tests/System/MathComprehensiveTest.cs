using NUnit.Framework;

namespace System;

[TestFixture]
public class MathComprehensiveTest {

  private const float _SINGLE_ACCURACY = 1e-6f;
  private const double _DOUBLE_ACCURACY = 1e-10; 
  private const decimal _DECIMAL_ACCURACY=1e-20m;
  private const decimal TwoPi = 2 * MathEx.Pi;
  private const decimal HalfPi = MathEx.Pi / 2;
  private const decimal Pi_4 = MathEx.Pi / 4;
  private const decimal Pi_8 = MathEx.Pi / 8;

  #region Bit Extraction and Half Operations Tests

  [Test]
  [TestCase((byte)0x00, (byte)0x00)]
  [TestCase((byte)0x0F, (byte)0x0F)]
  [TestCase((byte)0xF0, (byte)0x00)]
  [TestCase((byte)0xAB, (byte)0x0B)]
  [TestCase((byte)0xFF, (byte)0x0F)]
  public void LowerHalf_Byte_ReturnsCorrectNibble(byte value, byte expected) => Assert.AreEqual(expected, value.LowerHalf());

  [Test]
  [TestCase((byte)0x00, (byte)0x00)]
  [TestCase((byte)0x0F, (byte)0x00)]
  [TestCase((byte)0xF0, (byte)0x0F)]
  [TestCase((byte)0xAB, (byte)0x0A)]
  [TestCase((byte)0xFF, (byte)0x0F)]
  public void UpperHalf_Byte_ReturnsCorrectNibble(byte value, byte expected) => Assert.AreEqual(expected, value.UpperHalf());

  [Test]
  [TestCase((ushort)0x0000, (byte)0x00)]
  [TestCase((ushort)0x00FF, (byte)0xFF)]
  [TestCase((ushort)0xFF00, (byte)0x00)]
  [TestCase((ushort)0xABCD, (byte)0xCD)]
  [TestCase((ushort)0xFFFF, (byte)0xFF)]
  public void LowerHalf_UShort_ReturnsCorrectByte(ushort value, byte expected) => Assert.AreEqual(expected, value.LowerHalf());

  [Test]
  [TestCase((ushort)0x0000, (byte)0x00)]
  [TestCase((ushort)0x00FF, (byte)0x00)]
  [TestCase((ushort)0xFF00, (byte)0xFF)]
  [TestCase((ushort)0xABCD, (byte)0xAB)]
  [TestCase((ushort)0xFFFF, (byte)0xFF)]
  public void UpperHalf_UShort_ReturnsCorrectByte(ushort value, byte expected) => Assert.AreEqual(expected, value.UpperHalf());

  [Test]
  [TestCase(0x00000000U, (ushort)0x0000)]
  [TestCase(0x0000FFFFU, (ushort)0xFFFF)]
  [TestCase(0xFFFF0000U, (ushort)0x0000)]
  [TestCase(0xABCDEF12U, (ushort)0xEF12)]
  [TestCase(0xFFFFFFFFU, (ushort)0xFFFF)]
  public void LowerHalf_UInt_ReturnsCorrectUShort(uint value, ushort expected) => Assert.AreEqual(expected, value.LowerHalf());

  [Test]
  [TestCase(0x00000000U, (ushort)0x0000)]
  [TestCase(0x0000FFFFU, (ushort)0x0000)]
  [TestCase(0xFFFF0000U, (ushort)0xFFFF)]
  [TestCase(0xABCDEF12U, (ushort)0xABCD)]
  [TestCase(0xFFFFFFFFU, (ushort)0xFFFF)]
  public void UpperHalf_UInt_ReturnsCorrectUShort(uint value, ushort expected) => Assert.AreEqual(expected, value.UpperHalf());

  [Test]
  [TestCase(0x0000000000000000UL, 0x00000000U)]
  [TestCase(0x00000000FFFFFFFFUL, 0xFFFFFFFFU)]
  [TestCase(0xFFFFFFFF00000000UL, 0x00000000U)]
  [TestCase(0xABCDEF1234567890UL, 0x34567890U)]
  [TestCase(0xFFFFFFFFFFFFFFFFUL, 0xFFFFFFFFU)]
  public void LowerHalf_ULong_ReturnsCorrectUInt(ulong value, uint expected) => Assert.AreEqual(expected, value.LowerHalf());

  [Test]
  [TestCase(0x0000000000000000UL, 0x00000000U)]
  [TestCase(0x00000000FFFFFFFFUL, 0x00000000U)]
  [TestCase(0xFFFFFFFF00000000UL, 0xFFFFFFFFU)]
  [TestCase(0xABCDEF1234567890UL, 0xABCDEF12U)]
  [TestCase(0xFFFFFFFFFFFFFFFFUL, 0xFFFFFFFFU)]
  public void UpperHalf_ULong_ReturnsCorrectUInt(ulong value, uint expected) => Assert.AreEqual(expected, value.UpperHalf());

  #endregion

  #region Bit Counting Tests

  [Test]
  [TestCase((byte)0x00, (byte)8)]
  [TestCase((byte)0x01, (byte)0)]
  [TestCase((byte)0x02, (byte)1)]
  [TestCase((byte)0x04, (byte)2)]
  [TestCase((byte)0x08, (byte)3)]
  [TestCase((byte)0x10, (byte)4)]
  [TestCase((byte)0x20, (byte)5)]
  [TestCase((byte)0x40, (byte)6)]
  [TestCase((byte)0x80, (byte)7)]
  [TestCase((byte)0xFF, (byte)0)]
  public void TrailingZeroCount_Byte_ReturnsCorrectCount(byte value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

  [Test]
  [TestCase((ushort)0x0000, (byte)16)]
  [TestCase((ushort)0x0001, (byte)0)]
  [TestCase((ushort)0x0002, (byte)1)]
  [TestCase((ushort)0x0100, (byte)8)]
  [TestCase((ushort)0x8000, (byte)15)]
  [TestCase((ushort)0xFFFF, (byte)0)]
  public void TrailingZeroCount_UShort_ReturnsCorrectCount(ushort value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

  [Test]
  [TestCase(0x00000000U, (byte)32)]
  [TestCase(0x00000001U, (byte)0)]
  [TestCase(0x00000002U, (byte)1)]
  [TestCase(0x00010000U, (byte)16)]
  [TestCase(0x80000000U, (byte)31)]
  [TestCase(0xFFFFFFFFU, (byte)0)]
  public void TrailingZeroCount_UInt_ReturnsCorrectCount(uint value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

  [Test]
  [TestCase(0x0000000000000000UL, (byte)64)]
  [TestCase(0x0000000000000001UL, (byte)0)]
  [TestCase(0x0000000000000002UL, (byte)1)]
  [TestCase(0x0000000100000000UL, (byte)32)]
  [TestCase(0x8000000000000000UL, (byte)63)]
  [TestCase(0xFFFFFFFFFFFFFFFFUL, (byte)0)]
  public void TrailingZeroCount_ULong_ReturnsCorrectCount(ulong value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

  [Test]
  [TestCase((byte)0x00, (byte)8)]
  [TestCase((byte)0x01, (byte)7)]
  [TestCase((byte)0x02, (byte)6)]
  [TestCase((byte)0x04, (byte)5)]
  [TestCase((byte)0x08, (byte)4)]
  [TestCase((byte)0x10, (byte)3)]
  [TestCase((byte)0x20, (byte)2)]
  [TestCase((byte)0x40, (byte)1)]
  [TestCase((byte)0x80, (byte)0)]
  [TestCase((byte)0xFF, (byte)0)]
  public void LeadingZeroCount_Byte_ReturnsCorrectCount(byte value, byte expected) => Assert.AreEqual(expected, value.LeadingZeroCount());

  [Test]
  [TestCase((byte)0x00, (byte)0)]
  [TestCase((byte)0x01, (byte)1)]
  [TestCase((byte)0x03, (byte)2)]
  [TestCase((byte)0x07, (byte)3)]
  [TestCase((byte)0x0F, (byte)4)]
  [TestCase((byte)0x1F, (byte)5)]
  [TestCase((byte)0x3F, (byte)6)]
  [TestCase((byte)0x7F, (byte)7)]
  [TestCase((byte)0xFF, (byte)8)]
  public void CountSetBits_Byte_ReturnsCorrectCount(byte value, byte expected) => Assert.AreEqual(expected, value.CountSetBits());

  [Test]
  [TestCase((byte)0x00, (byte)8)]
  [TestCase((byte)0x01, (byte)7)]
  [TestCase((byte)0x03, (byte)6)]
  [TestCase((byte)0x07, (byte)5)]
  [TestCase((byte)0x0F, (byte)4)]
  [TestCase((byte)0x1F, (byte)3)]
  [TestCase((byte)0x3F, (byte)2)]
  [TestCase((byte)0x7F, (byte)1)]
  [TestCase((byte)0xFF, (byte)0)]
  public void CountUnsetBits_Byte_ReturnsCorrectCount(byte value, byte expected) => Assert.AreEqual(expected, value.CountUnsetBits());

  #endregion

  #region Parity and Bit Reversal Tests

  [Test]
  [TestCase((byte)0x00, true)]  // 0 bits set = even parity
  [TestCase((byte)0x01, false)] // 1 bit set = odd parity
  [TestCase((byte)0x03, true)]  // 2 bits set = even parity
  [TestCase((byte)0x07, false)] // 3 bits set = odd parity
  [TestCase((byte)0x0F, true)]  // 4 bits set = even parity
  [TestCase((byte)0xFF, true)]  // 8 bits set = even parity
  public void Parity_Byte_ReturnsCorrectParity(byte value, bool expected) => Assert.AreEqual(expected, value.Parity());

  [Test]
  [TestCase((byte)0x00, (byte)0x00)]
  [TestCase((byte)0x01, (byte)0x80)]
  [TestCase((byte)0x80, (byte)0x01)]
  [TestCase((byte)0xAB, (byte)0xD5)] // 10101011 -> 11010101
  [TestCase((byte)0xFF, (byte)0xFF)]
  public void ReverseBits_Byte_ReturnsCorrectValue(byte value, byte expected) => Assert.AreEqual(expected, value.ReverseBits());

  [Test]
  [TestCase((ushort)0x0001, (ushort)0x8000)]
  [TestCase((ushort)0x8000, (ushort)0x0001)]
  [TestCase((ushort)0xABCD, (ushort)0xB3D5)]
  [TestCase((ushort)0xFFFF, (ushort)0xFFFF)]
  public void ReverseBits_UShort_ReturnsCorrectValue(ushort value, ushort expected) => Assert.AreEqual(expected, value.ReverseBits());

  #endregion

  #region Bit Manipulation Tests

  [Test]
  [TestCase((byte)0x00, (byte)0, (byte)0x01)]
  [TestCase((byte)0x00, (byte)7, (byte)0x80)]
  [TestCase((byte)0xFE, (byte)0, (byte)0xFF)]
  [TestCase((byte)0x7F, (byte)7, (byte)0xFF)]
  public void SetBit_Byte_SetsCorrectBit(byte value, byte index, byte expected) => Assert.AreEqual(expected, value.SetBit(index));

  [Test]
  [TestCase((byte)0xFF, (byte)0, (byte)0xFE)]
  [TestCase((byte)0xFF, (byte)7, (byte)0x7F)]
  [TestCase((byte)0x01, (byte)0, (byte)0x00)]
  [TestCase((byte)0x80, (byte)7, (byte)0x00)]
  public void ClearBit_Byte_ClearsCorrectBit(byte value, byte index, byte expected) => Assert.AreEqual(expected, value.ClearBit(index));

  [Test]
  [TestCase((byte)0x00, (byte)0, (byte)0x01)]
  [TestCase((byte)0x01, (byte)0, (byte)0x00)]
  [TestCase((byte)0x7F, (byte)7, (byte)0xFF)]
  [TestCase((byte)0xFF, (byte)7, (byte)0x7F)]
  public void FlipBit_Byte_FlipsCorrectBit(byte value, byte index, byte expected) => Assert.AreEqual(expected, value.FlipBit(index));

  [Test]
  [TestCase((byte)0xAB, (byte)0, (byte)4, (byte)0x0B)] // Extract lower 4 bits
  [TestCase((byte)0xAB, (byte)4, (byte)4, (byte)0x0A)] // Extract upper 4 bits
  [TestCase((byte)0xFF, (byte)0, (byte)8, (byte)0xFF)] // Extract all bits
  [TestCase((byte)0x55, (byte)1, (byte)6, (byte)0x2A)] // Extract middle 6 bits
  public void Bits_Byte_ExtractsCorrectBits(byte value, byte index, byte count, byte expected) => Assert.AreEqual(expected, value.Bits(index, count));

  #endregion

  #region T4-Generated Method Tests - Arithmetic Operations

  [Test]
  [TestCase((sbyte)5, (sbyte)3, (sbyte)8)]
  [TestCase((sbyte)(-5), (sbyte)3, (sbyte)(-2))]
  [TestCase((sbyte)127, (sbyte)1, (sbyte)(-128))] // Overflow test
  public void Add_SByte_ReturnsCorrectSum(sbyte value, sbyte operand, sbyte expected) => Assert.AreEqual(expected, value.Add(operand));

  [Test]
  [TestCase((byte)5, (byte)3, (byte)8)]
  [TestCase((byte)255, (byte)1, (byte)0)] // Overflow test
  [TestCase((byte)100, (byte)50, (byte)150)]
  public void Add_Byte_ReturnsCorrectSum(byte value, byte operand, byte expected) => Assert.AreEqual(expected, value.Add(operand));

  [Test]
  [TestCase((short)1000, (short)500, (short)1500)]
  [TestCase((short)(-1000), (short)500, (short)(-500))]
  [TestCase((short)32767, (short)1, (short)(-32768))] // Overflow test
  public void Add_Short_ReturnsCorrectSum(short value, short operand, short expected) => Assert.AreEqual(expected, value.Add(operand));

  [Test]
  [TestCase(10, 5, 5)]
  [TestCase(-10, 5, -15)]
  [TestCase(0, 10, -10)]
  public void Subtract_Int_ReturnsCorrectDifference(int value, int minuend, int expected) => Assert.AreEqual(expected, value.Subtract(minuend));

  [Test]
  [TestCase(5L, 3L, 15L)]
  [TestCase(-5L, 3L, -15L)]
  [TestCase(0L, 100L, 0L)]
  public void MultipliedWith_Long_ReturnsCorrectProduct(long value, long factor, long expected) => Assert.AreEqual(expected, value.MultipliedWith(factor));

  [Test]
  [TestCase(10.0f, 2.0f, 5.0f)]
  [TestCase(-10.0f, 2.0f, -5.0f)]
  [TestCase(0.0f, 5.0f, 0.0f)]
  public void DividedBy_Float_ReturnsCorrectQuotient(float value, float divisor, float expected) => Assert.AreEqual(expected, value.DividedBy(divisor), _SINGLE_ACCURACY);

  #endregion

  #region Pow, Square, Cube Tests

  [Test]
  [TestCase((sbyte)2, (sbyte)4)]
  [TestCase((sbyte)3, (sbyte)9)]
  [TestCase((sbyte)(-3), (sbyte)9)]
  [TestCase((sbyte)0, (sbyte)0)]
  [TestCase((sbyte)1, (sbyte)1)]
  public void Squared_SByte_ReturnsCorrectSquare(sbyte value, sbyte expected) => Assert.AreEqual(expected, value.Squared());

  [Test]
  [TestCase(2, 8)]
  [TestCase(3, 27)]
  [TestCase(-3, -27)]
  [TestCase(0, 0)]
  [TestCase(1, 1)]
  public void Cubed_Int_ReturnsCorrectCube(int value, int expected) => Assert.AreEqual(expected, value.Cubed());

  [Test]
  [TestCase(2.0f, 3.0f, 8.0f)]
  [TestCase(3.0f, 2.0f, 9.0f)]
  [TestCase(2.0f, 0.0f, 1.0f)]
  [TestCase(0.0f, 5.0f, 0.0f)]
  public void Pow_Float_ReturnsCorrectPower(float baseValue, float exponent, float expected) => Assert.AreEqual(expected, baseValue.Pow(exponent), _SINGLE_ACCURACY);

  #endregion

  #region Sign and Zero Tests

  [Test]
  [TestCase((sbyte)5, (sbyte)1)]
  [TestCase((sbyte)(-5), (sbyte)(-1))]
  [TestCase((sbyte)0, (sbyte)(0))]
  public void Sign_SByte_ReturnsCorrectSign(sbyte value, sbyte expected) => Assert.AreEqual(expected, value.Sign());

  [Test]
  [TestCase(5.0, 5.0)]
  [TestCase(-5.0, 5.0)]
  [TestCase(0.0, 0.0)]
  public void Abs_Double_ReturnsCorrectAbsoluteValue(double value, double expected) => Assert.AreEqual(expected, value.Abs(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(0, true)]
  [TestCase(1, false)]
  [TestCase(-1, false)]
  [TestCase(100, false)]
  public void IsZero_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsZero());

  [Test]
  [TestCase(0, false)]
  [TestCase(1, true)]
  [TestCase(-1, true)]
  [TestCase(100, true)]
  public void IsNotZero_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsNotZero());

  [Test]
  [TestCase(5, true)]
  [TestCase(-5, false)]
  [TestCase(0, false)]
  public void IsPositive_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsPositive());

  [Test]
  [TestCase(5, true)]
  [TestCase(-5, false)]
  [TestCase(0, true)]
  public void IsPositiveOrZero_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsPositiveOrZero());

  [Test]
  [TestCase(5, false)]
  [TestCase(-5, true)]
  [TestCase(0, false)]
  public void IsNegative_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsNegative());

  [Test]
  [TestCase(5, false)]
  [TestCase(-5, true)]
  [TestCase(0, true)]
  public void IsNegativeOrZero_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsNegativeOrZero());

  #endregion

  #region Odd/Even Tests

  [Test]
  [TestCase(0, false)]
  [TestCase(1, true)]
  [TestCase(2, false)]
  [TestCase(3, true)]
  [TestCase(-1, true)]
  [TestCase(-2, false)]
  public void IsOdd_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsOdd());

  [Test]
  [TestCase(0, true)]
  [TestCase(1, false)]
  [TestCase(2, true)]
  [TestCase(3, false)]
  [TestCase(-1, false)]
  [TestCase(-2, true)]
  public void IsEven_Int_ReturnsCorrectResult(int value, bool expected) => Assert.AreEqual(expected, value.IsEven());

  [Test]
  [TestCase(0.0f, true)]
  [TestCase(1.0f, false)]
  [TestCase(2.0f, true)]
  [TestCase(3.5f, false)]
  [TestCase(-2.0f, true)]
  public void IsEven_Float_ReturnsCorrectResult(float value, bool expected) => Assert.AreEqual(expected, value.IsEven());

  [Test]
  [TestCase(0.0, false)]
  [TestCase(1.0, true)]
  [TestCase(2.0, false)]
  [TestCase(3.5, true)]
  [TestCase(-1.0, true)]
  public void IsOdd_Double_ReturnsCorrectResult(double value, bool expected) => Assert.AreEqual(expected, value.IsOdd());

  #endregion

  #region Comparison Tests

  [Test]
  [TestCase(5, 10, true)]
  [TestCase(10, 10, false)]
  [TestCase(15, 10, false)]
  public void IsBelow_Int_ReturnsCorrectResult(int value, int limit, bool expected) => Assert.AreEqual(expected, value.IsBelow(limit));

  [Test]
  [TestCase(5, 10, true)]
  [TestCase(10, 10, true)]
  [TestCase(15, 10, false)]
  public void IsBelowOrEqual_Int_ReturnsCorrectResult(int value, int limit, bool expected) => Assert.AreEqual(expected, value.IsBelowOrEqual(limit));

  [Test]
  [TestCase(15, 10, true)]
  [TestCase(10, 10, false)]
  [TestCase(5, 10, false)]
  public void IsAbove_Int_ReturnsCorrectResult(int value, int limit, bool expected) => Assert.AreEqual(expected, value.IsAbove(limit));

  [Test]
  [TestCase(15, 10, true)]
  [TestCase(10, 10, true)]
  [TestCase(5, 10, false)]
  public void IsAboveOrEqual_Int_ReturnsCorrectResult(int value, int limit, bool expected) => Assert.AreEqual(expected, value.IsAboveOrEqual(limit));

  [Test]
  [TestCase(5, 1, 10, true)]
  [TestCase(1, 1, 10, false)]
  [TestCase(10, 1, 10, false)]
  [TestCase(0, 1, 10, false)]
  [TestCase(11, 1, 10, false)]
  public void IsBetween_Int_ReturnsCorrectResult(int value, int lower, int upper, bool expected) => Assert.AreEqual(expected, value.IsBetween(lower, upper));

  [Test]
  [TestCase(5, 1, 10, true)]
  [TestCase(1, 1, 10, true)]
  [TestCase(10, 1, 10, true)]
  [TestCase(0, 1, 10, false)]
  [TestCase(11, 1, 10, false)]
  public void IsInRange_Int_ReturnsCorrectResult(int value, int lower, int upper, bool expected) => Assert.AreEqual(expected, value.IsInRange(lower, upper));

  #endregion

  #region Bitwise Operations Tests

  [Test]
  [TestCase((byte)0xF0, (byte)0x0F, (byte)0x00)]
  [TestCase((byte)0xFF, (byte)0xFF, (byte)0xFF)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0x00)]
  public void And_Byte_ReturnsCorrectResult(byte left, byte right, byte expected) => Assert.AreEqual(expected, left.And(right));

  [Test]
  [TestCase((byte)0xF0, (byte)0x0F, (byte)0xFF)]
  [TestCase((byte)0x00, (byte)0x00, (byte)0x00)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0xFF)]
  public void Or_Byte_ReturnsCorrectResult(byte left, byte right, byte expected) => Assert.AreEqual(expected, left.Or(right));

  [Test]
  [TestCase((byte)0xF0, (byte)0x0F, (byte)0xFF)]
  [TestCase((byte)0xFF, (byte)0xFF, (byte)0x00)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0xFF)]
  public void Xor_Byte_ReturnsCorrectResult(byte left, byte right, byte expected) => Assert.AreEqual(expected, left.Xor(right));

  [Test]
  [TestCase((byte)0xF0, (byte)0x0F, (byte)0xFF)]
  [TestCase((byte)0xFF, (byte)0xFF, (byte)0x00)]
  [TestCase((byte)0x00, (byte)0x00, (byte)0xFF)]
  public void Nand_Byte_ReturnsCorrectResult(byte left, byte right, byte expected) => Assert.AreEqual(expected, left.Nand(right));

  [Test]
  [TestCase((byte)0xF0, (byte)0x0F, (byte)0x00)]
  [TestCase((byte)0x00, (byte)0x00, (byte)0xFF)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0x00)]
  public void Nor_Byte_ReturnsCorrectResult(byte left, byte right, byte expected) => Assert.AreEqual(expected, left.Nor(right));

  [Test]
  [TestCase((byte)0xF0, (byte)0x0F, (byte)0x00)]
  [TestCase((byte)0xFF, (byte)0xFF, (byte)0xFF)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0x00)]
  public void Equ_Byte_ReturnsCorrectResult(byte left, byte right, byte expected) => Assert.AreEqual(expected, left.Equ(right));

  [Test]
  [TestCase((byte)0x00, (byte)0xFF)]
  [TestCase((byte)0xFF, (byte)0x00)]
  [TestCase((byte)0xAA, (byte)0x55)]
  public void Not_Byte_ReturnsCorrectResult(byte value, byte expected) => Assert.AreEqual(expected, value.Not());

  #endregion

  #region Floating Point Special Values Tests

  [Test]
  [TestCase(3.14f, true)]
  [TestCase(float.NaN, false)]
  [TestCase(float.PositiveInfinity, false)]
  [TestCase(float.NegativeInfinity, false)]
  public void IsNumeric_Float_ReturnsCorrectResult(float value, bool expected) => Assert.AreEqual(expected, value.IsNumeric());

  [Test]
  [TestCase(3.14, false)]
  [TestCase(double.NaN, true)]
  [TestCase(double.PositiveInfinity, true)]
  [TestCase(double.NegativeInfinity, true)]
  public void IsNonNumeric_Double_ReturnsCorrectResult(double value, bool expected) => Assert.AreEqual(expected, value.IsNonNumeric());

  [Test]
  [TestCase(double.NaN, true)]
  [TestCase(3.14, false)]
  [TestCase(double.PositiveInfinity, false)]
  public void IsNaN_Double_ReturnsCorrectResult(double value, bool expected) => Assert.AreEqual(expected, value.IsNaN());

  [Test]
  [TestCase(double.PositiveInfinity, true)]
  [TestCase(double.NegativeInfinity, true)]
  [TestCase(3.14, false)]
  [TestCase(double.NaN, false)]
  public void IsInfinity_Double_ReturnsCorrectResult(double value, bool expected) => Assert.AreEqual(expected, value.IsInfinity());

  [Test]
  [TestCase(float.PositiveInfinity, true)]
  [TestCase(float.NegativeInfinity, false)]
  [TestCase(3.14f, false)]
  public void IsPositiveInfinity_Float_ReturnsCorrectResult(float value, bool expected) => Assert.AreEqual(expected, value.IsPositiveInfinity());

  [Test]
  [TestCase(double.NegativeInfinity, true)]
  [TestCase(double.PositiveInfinity, false)]
  [TestCase(3.14, false)]
  public void IsNegativeInfinity_Double_ReturnsCorrectResult(double value, bool expected) => Assert.AreEqual(expected, value.IsNegativeInfinity());

  #endregion

  #region Math Function Tests - Reciprocal

  [Test]
  [TestCase(2.0f, 0.5f)]
  [TestCase(4.0f, 0.25f)]
  [TestCase(0.5f, 2.0f)]
  [TestCase(-2.0f, -0.5f)]
  public void ReciprocalEstimate_Float_ReturnsCorrectResult(float value, float expected) => Assert.AreEqual(expected, value.ReciprocalEstimate(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.0, 0.5)]
  [TestCase(4.0, 0.25)]
  [TestCase(0.5, 2.0)]
  [TestCase(-2.0, -0.5)]
  public void ReciprocalEstimate_Double_ReturnsCorrectResult(double value, double expected) => Assert.AreEqual(expected, value.ReciprocalEstimate(), _DOUBLE_ACCURACY);

  #endregion

  #region Decimal Advanced Math Tests

  [Test]
  [TestCase(2.0, 3.0, 8.0)]
  [TestCase(3.0, 2.0, 9.0)]
  [TestCase(2.0, 0.0, 1.0)]
  [TestCase(1.0, 100.0, 1.0)]
  [TestCase(0.0, 5.0, 0.0)]
  [TestCase(-2.0, 5.0, -32)]
  public void Pow_Decimal_ReturnsCorrectPower(decimal baseValue, decimal exponent, decimal expected) => Assert.That(baseValue.Pow(exponent),Is.EqualTo(expected));

  [Test]
  [TestCase(4.0, 2.0)]
  [TestCase(9.0, 3.0)]
  [TestCase(16.0, 4.0)]
  [TestCase(25.0, 5.0)]
  [TestCase(1.0, 1.0)]
  [TestCase(0.0, 0.0)]
  public void Sqrt_Decimal_ReturnsCorrectSquareRoot(decimal value, decimal expected) => Assert.That(value.Sqrt(), Is.EqualTo(expected));

  [Test]
  public void Sqrt_Decimal_NegativeValue_ThrowsException() => Assert.Throws<ArgumentOutOfRangeException>(() => (-1.0m).Sqrt());

  private static readonly (decimal input, decimal expected)[] _SINE_TEST_TABLE = [
    (-TwoPi, 0m),
    (-MathEx.Pi, 0m),
    (-HalfPi, -1m),
    (-MathEx.Pi / 3m, -0.8660254037844386467637231707m),   // -π/3
    (-Pi_4, -0.7071067811865475244008443621m),            // -π/4
    (-MathEx.Pi / 6m, -0.5m),                              // -π/6
    (-Pi_8, -0.3826834323650897717284599840m),            // -π/8
    (-0.0001m, -0.000099999999833333334166666m),      // small angle
    (0.0m, 0m),
    (0.0001m, 0.000099999999833333334166666m),
    (Pi_8, 0.3826834323650897717284599840m),
    (MathEx.Pi / 6m, 0.5m),
    (Pi_4, 0.7071067811865475244008443621m),
    (MathEx.Pi / 3m, 0.8660254037844386467637231707m),
    (HalfPi, 1m),
    (2 * MathEx.Pi / 3m, 0.8660254037844386467637231707m),
    (3 * Pi_4, 0.7071067811865475244008443621m),
    (5 * MathEx.Pi / 6m, 0.5m),
    (MathEx.Pi, 0m),
    (TwoPi, 0m),
    (3 * MathEx.Pi / 2m, -1m),
    (4 * MathEx.Pi / 3m, -0.8660254037844386467637231707m)
  ];

  [Test]
  public void Sin_Decimal_ReturnsCorrectSine() {
    foreach (var (input, expected) in _SINE_TEST_TABLE) {
      var actual = input.Sin();
      Assert.That(Math.Abs(actual - expected), Is.LessThan(_DECIMAL_ACCURACY),
        $"Sin({input}) = {actual}, expected {expected}");
    }
  }

  [Test]
  public void Cos_Decimal_ReturnsCorrectCosine() {
    foreach (var (input, expected) in _SINE_TEST_TABLE) {
      var actual = (input - MathEx.Pi / 2m).Cos();

      Assert.That(Math.Abs(actual - expected), Is.LessThan(_DECIMAL_ACCURACY),
        $"Cos({input}) = {actual}, expected {expected}");
    }
  }

  [Test]
  public void Tan_Decimal_ReturnsCorrectTangent() {
    foreach (var (input, expectedSin) in _SINE_TEST_TABLE) {
      var sin = expectedSin;
      var cos = (input + MathEx.Pi / 2m).Sin(); // cos(x) = sin(x + π/2)

      if (Math.Abs(cos) < _DECIMAL_ACCURACY)
        continue; // skip singularities at ±π/2, ±3π/2, etc.

      var expectedTan = sin / cos;
      var actual = input.Tan();

      Assert.That(Math.Abs(actual - expectedTan), Is.LessThan(_DECIMAL_ACCURACY),
        $"Tan({input}) = {actual}, expected {expectedTan}");
    }
  }

  [Test]
  public void Atan_Decimal_ReturnsCorrectArcTangent() {
    var testCases = new (decimal input, decimal expected)[]
    {
      (0.0m, 0.0m),
      (1.0m, 0.7853981633974483096156608458m),    // π/4
      (-1.0m, -0.7853981633974483096156608458m),   // -π/4
      (0.5m, 0.4636476090008061162142562315m),
      (-0.5m, -0.4636476090008061162142562315m),
      (MathEx.Sqrt3, 1.0471975511965977461542144611m), // atan(√3) = π/3
      (-MathEx.Sqrt3, -1.0471975511965977461542144611m),
      (10.0m, 1.471127674303734591852875738m),
      (-10.0m, -1.471127674303734591852875738m),
    };

    foreach (var (input, expected) in testCases) {
      var actual = input.Atan();
      Assert.That(Math.Abs(actual - expected), Is.LessThan(_DECIMAL_ACCURACY),
        $"Atan({input}) = {actual}, expected {expected}");
    }
  }

  [Test]
  public void Exp_Decimal_ReturnsCorrectExponential() {
    Assert.That(0m.Exp(), Is.EqualTo(1m));
    Assert.That(1m.Exp(), Is.EqualTo(2.71828182845904523536028747135m));
    Assert.That(2m.Exp(),Is.EqualTo(7.3890560989306502272304274608m));
  }

  [Test]
  public void Log_Decimal_ReturnsCorrectNaturalLogarithm() {
    Assert.That(1m.Log(), Is.EqualTo(0m));
    Assert.That(2.71828182845904523536028747135m.Log(), Is.EqualTo(1m));
    Assert.That(7.3890560989306502272304274608m.Log(), Is.EqualTo(2m));
  }

  [Test]
  public void Log_Decimal_NonPositiveValue_ThrowsException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => 0.0m.Log());
    Assert.Throws<ArgumentOutOfRangeException>(() => (-1.0m).Log());
  }

  [Test]
  [TestCase(10.0, 1.0)]
  [TestCase(100.0, 2.0)]
  [TestCase(1000.0, 3.0)]
  public void Log10_Decimal_ReturnsCorrectLog10(decimal value, decimal expected) => Assert.True(Math.Abs(value.Log10() - expected) < _DECIMAL_ACCURACY);

  [Test]
  [TestCase(2.0, 1.0)]
  [TestCase(4.0, 2.0)]
  [TestCase(8.0, 3.0)]
  [TestCase(16.0, 4.0)]
  public void Log2_Decimal_ReturnsCorrectLog2(decimal value, decimal expected) => Assert.True(Math.Abs(value.Log2() - expected) < _DECIMAL_ACCURACY);

  [Test]
  [TestCase(8.0, 2.0, 3.0)] // log₂(8) = 3
  [TestCase(27.0, 3.0, 3.0)] // log₃(27) = 3
  [TestCase(100.0, 10.0, 2.0)] // log₁₀(100) = 2
  public void LogN_Decimal_ReturnsCorrectLogarithm(decimal value, decimal baseValue, decimal expected) => Assert.True(Math.Abs(value.LogN(baseValue) - expected) < _DECIMAL_ACCURACY);

  #endregion

  #region Floating Point Math Tests - Rounding and Advanced Functions

  [Test]
  [TestCase(2.1f, 2.0f)]
  [TestCase(2.7f, 2.0f)]
  [TestCase(-2.1f, -3.0f)]
  [TestCase(-2.7f, -3.0f)]
  public void Floor_Float_ReturnsCorrectFloor(float value, float expected) => Assert.AreEqual(expected, value.Floor(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.1f, 3.0f)]
  [TestCase(2.7f, 3.0f)]
  [TestCase(-2.1f, -2.0f)]
  [TestCase(-2.7f, -2.0f)]
  public void Ceiling_Float_ReturnsCorrectCeiling(float value, float expected) => Assert.AreEqual(expected, value.Ceiling(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.1f, 2.0f)]
  [TestCase(2.7f, 2.0f)]
  [TestCase(-2.1f, -2.0f)]
  [TestCase(-2.7f, -2.0f)]
  public void Truncate_Float_ReturnsCorrectTruncation(float value, float expected) => Assert.AreEqual(expected, value.Truncate(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.1f, 2.0f)]
  [TestCase(2.5f, 2.0f)] // Even rounding
  [TestCase(2.7f, 3.0f)]
  [TestCase(3.5f, 4.0f)] // Even rounding
  public void Round_Float_ReturnsCorrectRounding(float value, float expected) => Assert.AreEqual(expected, value.Round(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.146f, 2, 2.15f)]
  [TestCase(2.144f, 2, 2.14f)]
  [TestCase(123.456f, 1, 123.5f)]
  public void Round_Float_WithDigits_ReturnsCorrectRounding(float value, int digits, float expected) => Assert.AreEqual(expected, value.Round(digits), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.5f, MidpointRounding.AwayFromZero, 3.0f)]
  [TestCase(2.5f, MidpointRounding.ToEven, 2.0f)]
  [TestCase(3.5f, MidpointRounding.AwayFromZero, 4.0f)]
  [TestCase(3.5f, MidpointRounding.ToEven, 4.0f)]
  public void Round_Float_WithMidpointRounding_ReturnsCorrectRounding(float value, MidpointRounding method, float expected) => Assert.AreEqual(expected, value.Round(method), _SINGLE_ACCURACY);

  [Test]
  [TestCase(8.0f, 2.0f, 3.0f)]
  [TestCase(27.0f, 3.0f, 3.0f)]
  [TestCase(100.0f, 10.0f, 2.0f)]
  public void LogN_Float_ReturnsCorrectLogarithm(float value, float baseValue, float expected) => Assert.AreEqual(expected, value.LogN(baseValue), _SINGLE_ACCURACY);

  [Test]
  [TestCase(8.0, 2.0, 3.0)]
  [TestCase(27.0, 3.0, 3.0)]
  [TestCase(100.0, 10.0, 2.0)]
  public void LogN_Double_ReturnsCorrectLogarithm(double value, double baseValue, double expected) => Assert.AreEqual(expected, value.LogN(baseValue), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(2.0f, 1.0f)]
  [TestCase(4.0f, 2.0f)]
  [TestCase(8.0f, 3.0f)]
  public void Log2_Float_ReturnsCorrectLog2(float value, float expected) => Assert.AreEqual(expected, value.Log2(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.0, 1.0)]
  [TestCase(4.0, 2.0)]
  [TestCase(8.0, 3.0)]
  public void Log2_Double_ReturnsCorrectLog2(double value, double expected) => Assert.AreEqual(expected, value.Log2(), _DOUBLE_ACCURACY);

  #endregion

  #region IsPowerOfTwo Tests

  [Test]
  [TestCase((byte)1, true)]
  [TestCase((byte)2, true)]
  [TestCase((byte)4, true)]
  [TestCase((byte)8, true)]
  [TestCase((byte)16, true)]
  [TestCase((byte)32, true)]
  [TestCase((byte)64, true)]
  [TestCase((byte)128, true)]
  [TestCase((byte)0, false)]
  [TestCase((byte)3, false)]
  [TestCase((byte)5, false)]
  [TestCase((byte)255, false)]
  public void IsPowerOfTwo_Byte_ReturnsCorrectResult(byte value, bool expected) => Assert.AreEqual(expected, value.IsPowerOfTwo());

  [Test]
  [TestCase((ushort)1, true)]
  [TestCase((ushort)1024, true)]
  [TestCase((ushort)32768, true)]
  [TestCase((ushort)0, false)]
  [TestCase((ushort)3, false)]
  [TestCase((ushort)1023, false)]
  public void IsPowerOfTwo_UShort_ReturnsCorrectResult(ushort value, bool expected) => Assert.AreEqual(expected, value.IsPowerOfTwo());

  [Test]
  [TestCase(1U, true)]
  [TestCase(1048576U, true)] // 2^20
  [TestCase(2147483648U, true)] // 2^31
  [TestCase(0U, false)]
  [TestCase(3U, false)]
  [TestCase(1048575U, false)]
  public void IsPowerOfTwo_UInt_ReturnsCorrectResult(uint value, bool expected) => Assert.AreEqual(expected, value.IsPowerOfTwo());

  [Test]
  [TestCase(1UL, true)]
  [TestCase(1099511627776UL, true)] // 2^40
  [TestCase(9223372036854775808UL, true)] // 2^63
  [TestCase(0UL, false)]
  [TestCase(3UL, false)]
  public void IsPowerOfTwo_ULong_ReturnsCorrectResult(ulong value, bool expected) => Assert.AreEqual(expected, value.IsPowerOfTwo());

  #endregion

  #region Integer Log2 Tests

  [Test]
  [TestCase((byte)1, 0)]
  [TestCase((byte)2, 1)]
  [TestCase((byte)4, 2)]
  [TestCase((byte)8, 3)]
  [TestCase((byte)16, 4)]
  [TestCase((byte)255, 7)]
  public void Log2_Byte_ReturnsCorrectResult(byte value, int expected) => Assert.AreEqual(expected, value.Log2());

  [Test]
  public void Log2_Byte_Zero_Returns_Minus_One() => Assert.AreEqual(((byte)0).Log2(), -1);

  [Test]
  [TestCase((ushort)1, 0)]
  [TestCase((ushort)1024, 10)]
  [TestCase((ushort)32768, 15)]
  [TestCase((ushort)65535, 15)]
  public void Log2_UShort_ReturnsCorrectResult(ushort value, int expected) => Assert.AreEqual(expected, value.Log2());

  [Test]
  [TestCase(1U, 0)]
  [TestCase(1048576U, 20)]
  [TestCase(2147483648U, 31)]
  [TestCase(4294967295U, 31)]
  public void Log2_UInt_ReturnsCorrectResult(uint value, int expected) => Assert.AreEqual(expected, value.Log2());

  [Test]
  [TestCase(1UL, 0)]
  [TestCase(1099511627776UL, 40)]
  [TestCase(9223372036854775808UL, 63)]
  public void Log2_ULong_ReturnsCorrectResult(ulong value, int expected) => Assert.AreEqual(expected, value.Log2());

  #endregion

  #region Cube Root Tests

  [Test]
  [TestCase(8.0f, 2.0f)]
  [TestCase(27.0f, 3.0f)]
  [TestCase(-8.0f, -2.0f)]
  [TestCase(0.0f, 0.0f)]
  [TestCase(1.0f, 1.0f)]
  public void Cbrt_Float_ReturnsCorrectCubeRoot(float value, float expected) => Assert.AreEqual(expected, value.Cbrt(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(8.0, 2.0)]
  [TestCase(27.0, 3.0)]
  [TestCase(-8.0, -2.0)]
  [TestCase(0.0, 0.0)]
  [TestCase(1.0, 1.0)]
  public void Cbrt_Double_ReturnsCorrectCubeRoot(double value, double expected) => Assert.AreEqual(expected, value.Cbrt(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(8.0, 2.0)]
  [TestCase(27.0, 3.0)]
  [TestCase(-8.0, -2.0)]
  [TestCase(0.0, 0.0)]
  [TestCase(1.0, 1.0)]
  public void Cbrt_Decimal_ReturnsCorrectCubeRoot(decimal value, decimal expected) => Assert.True(Math.Abs(value.Cbrt() - expected) < _DECIMAL_ACCURACY);

  #endregion

  #region Trigonometric Function Tests

  [Test]
  [TestCase(1.0, 0.6420926159343306)] // cot(1) ≈ 0.642
  [TestCase(Math.PI / 4, 1.0)] // cot(π/4) = 1
  public void Cot_Double_ReturnsCorrectCotangent(double value, double expected) => Assert.AreEqual(expected, value.Cot(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(1.0, 1.1883951057781212)] // csc(1) ≈ 1.188
  [TestCase(Math.PI / 2, 1.0)] // csc(π/2) = 1
  public void Csc_Double_ReturnsCorrectCosecant(double value, double expected) => Assert.AreEqual(expected, value.Csc(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(1.0, 1.8508157176809255)] // sec(1) ≈ 1.851
  [TestCase(0.0, 1.0)] // sec(0) = 1
  public void Sec_Double_ReturnsCorrectSecant(double value, double expected) => Assert.AreEqual(expected, value.Sec(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(1.0, 1.3130352854993313)] // coth(1) ≈ 1.313
  public void Coth_Double_ReturnsCorrectHyperbolicCotangent(double value, double expected) => Assert.AreEqual(expected, value.Coth(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(1.0, 0.8509181282393216)] // csch(1) ≈ 0.851
  public void Csch_Double_ReturnsCorrectHyperbolicCosecant(double value, double expected) => Assert.AreEqual(expected, value.Csch(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(1.0, 0.6480542736638855)] // sech(1) ≈ 0.648
  [TestCase(0.0, 1.0)] // sech(0) = 1
  public void Sech_Double_ReturnsCorrectHyperbolicSecant(double value, double expected) => Assert.AreEqual(expected, value.Sech(), _DOUBLE_ACCURACY);

  #endregion

  #region Fused Operations Tests

  [Test]
  [TestCase((sbyte)2, (sbyte)3, (sbyte)4, (sbyte)10)] // 2*3+4 = 10
  [TestCase((sbyte)5, (sbyte)2, (sbyte)1, (sbyte)11)] // 5*2+1 = 11
  public void FusedMultiplyAdd_SByte_ReturnsCorrectResult(sbyte value, sbyte factor, sbyte addend, sbyte expected) => Assert.AreEqual(expected, value.FusedMultiplyAdd(factor, addend));

  [Test]
  [TestCase((byte)2, (byte)3, (byte)4, (byte)10)] // 2*3+4 = 10
  [TestCase((byte)5, (byte)2, (byte)1, (byte)11)] // 5*2+1 = 11
  public void FusedMultiplyAdd_Byte_ReturnsCorrectResult(byte value, byte factor, byte addend, byte expected) => Assert.AreEqual(expected, value.FusedMultiplyAdd(factor, addend));

  [Test]
  [TestCase(2.0f, 3.0f, 4.0f, 10.0f)] // 2*3+4 = 10
  [TestCase(5.5f, 2.0f, 1.5f, 12.5f)] // 5.5*2+1.5 = 12.5
  public void FusedMultiplyAdd_Float_ReturnsCorrectResult(float value, float factor, float addend, float expected) => Assert.AreEqual(expected, value.FusedMultiplyAdd(factor, addend), _SINGLE_ACCURACY);

  [Test]
  [TestCase(2.0, 3.0, 4.0, 10.0)] // 2*3+4 = 10
  [TestCase(5.5, 2.0, 1.5, 12.5)] // 5.5*2+1.5 = 12.5
  public void FusedMultiplyAdd_Double_ReturnsCorrectResult(double value, double factor, double addend, double expected) => Assert.AreEqual(expected, value.FusedMultiplyAdd(factor, addend), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(10, 2, 1, 4)] // 10/2-1 = 4
  [TestCase(15, 3, 2, 3)] // 15/3-2 = 3
  public void FusedDivideSubtract_Int_ReturnsCorrectResult(int value, int divisor, int subtrahend, int expected) => Assert.AreEqual(expected, value.FusedDivideSubtract(divisor, subtrahend));

  #endregion

  #region Min/Max/Average Array Operations Tests

  [Test]
  public void Min_ByteArray_ReturnsSmallestValue() {
    var values = new byte[] { 5, 2, 8, 1, 9 };
    Assert.AreEqual(1, MathEx.Min(values));
  }

  [Test]
  public void Min_EmptyByteArray_ReturnsZero() {
    var values = new byte[] { };
    Assert.AreEqual(0, MathEx.Min(values));
  }

  [Test]
  public void Max_ByteArray_ReturnsLargestValue() {
    var values = new byte[] { 5, 2, 8, 1, 9 };
    Assert.AreEqual(9, MathEx.Max(values));
  }

  [Test]
  public void Max_EmptyByteArray_ReturnsZero() {
    var values = new byte[] { };
    Assert.AreEqual(0, MathEx.Max(values));
  }

  [Test]
  public void Average_ByteArray_ReturnsCorrectAverage() {
    var values = new byte[] { 2, 4, 6, 8 };
    Assert.AreEqual(5, MathEx.Average(values));
  }

  [Test]
  public void Average_EmptyByteArray_ReturnsZero() {
    var values = new byte[] { };
    Assert.AreEqual(0, MathEx.Average(values));
  }

  [Test]
  public void Min_IntArray_ReturnsSmallestValue() {
    var values = new int[] { -5, 2, -8, 1, 9 };
    Assert.AreEqual(-8, MathEx.Min(values));
  }

  [Test]
  public void Max_DoubleArray_ReturnsLargestValue() {
    var values = new double[] { 5.5, 2.1, 8.9, 1.2, 9.7 };
    Assert.AreEqual(9.7, MathEx.Max(values), _DOUBLE_ACCURACY);
  }

  [Test]
  public void Average_FloatArray_ReturnsCorrectAverage() {
    var values = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };
    Assert.AreEqual(2.5f, MathEx.Average(values), _SINGLE_ACCURACY);
  }

  #endregion

  #region IsIn/IsNotIn Tests

  [Test]
  public void IsIn_ByteArray_ValueExists_ReturnsTrue() {
    var values = new byte[] { 1, 2, 3, 4, 5 };
    Assert.IsTrue(MathEx.IsIn((byte)3, values));
  }

  [Test]
  public void IsIn_ByteArray_ValueDoesNotExist_ReturnsFalse() {
    var values = new byte[] { 1, 2, 3, 4, 5 };
    Assert.IsFalse(MathEx.IsIn((byte)6, values));
  }

  [Test]
  public void IsIn_EmptyArray_ReturnsFalse() {
    var values = new byte[] { };
    Assert.IsFalse(MathEx.IsIn((byte)1, values));
  }

  [Test]
  public void IsNotIn_ByteArray_ValueExists_ReturnsFalse() {
    var values = new byte[] { 1, 2, 3, 4, 5 };
    Assert.IsFalse(MathEx.IsNotIn((byte)3, values));
  }

  [Test]
  public void IsNotIn_ByteArray_ValueDoesNotExist_ReturnsTrue() {
    var values = new byte[] { 1, 2, 3, 4, 5 };
    Assert.IsTrue(MathEx.IsNotIn((byte)6, values));
  }

  [Test]
  public void IsNotIn_EmptyArray_ReturnsTrue() {
    var values = new byte[] { };
    Assert.IsTrue(MathEx.IsNotIn((byte)1, values));
  }

  #endregion

  #region Floor/Ceiling/Truncate for Double and Decimal

  [Test]
  [TestCase(2.1, 2.0)]
  [TestCase(2.7, 2.0)]
  [TestCase(-2.1, -3.0)]
  [TestCase(-2.7, -3.0)]
  public void Floor_Double_ReturnsCorrectFloor(double value, double expected) => Assert.AreEqual(expected, value.Floor(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(2.1, 3.0)]
  [TestCase(2.7, 3.0)]
  [TestCase(-2.1, -2.0)]
  [TestCase(-2.7, -2.0)]
  public void Ceiling_Double_ReturnsCorrectCeiling(double value, double expected) => Assert.AreEqual(expected, value.Ceiling(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(2.1, 2.0)]
  [TestCase(2.7, 2.0)]
  [TestCase(-2.1, -2.0)]
  [TestCase(-2.7, -2.0)]
  public void Truncate_Double_ReturnsCorrectTruncation(double value, double expected) => Assert.AreEqual(expected, value.Truncate(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(2.1, 2.0)]
  [TestCase(2.5, 2.0)] // Even rounding
  [TestCase(2.7, 3.0)]
  [TestCase(3.5, 4.0)] // Even rounding
  public void Round_Double_ReturnsCorrectRounding(double value, double expected) => Assert.AreEqual(expected, value.Round(), _DOUBLE_ACCURACY);

  #endregion

  #region Math Function Tests - Sqrt, Sin, Cos, Tan, etc.

  [Test]
  [TestCase(4.0f, 2.0f)]
  [TestCase(9.0f, 3.0f)]
  [TestCase(0.0f, 0.0f)]
  [TestCase(1.0f, 1.0f)]
  public void Sqrt_Float_ReturnsCorrectSquareRoot(float value, float expected) => Assert.AreEqual(expected, value.Sqrt(), _SINGLE_ACCURACY);

  [Test]
  [TestCase(0.0, 0.0)]
  [TestCase(Math.PI / 6, 0.5)] // sin(π/6) = 0.5
  [TestCase(Math.PI / 2, 1.0)] // sin(π/2) = 1
  public void Sin_Double_ReturnsCorrectSine(double value, double expected) => Assert.AreEqual(expected, value.Sin(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(0.0, 1.0)] // cos(0) = 1
  [TestCase(Math.PI / 3, 0.5)] // cos(π/3) = 0.5
  [TestCase(Math.PI / 2, 0.0)] // cos(π/2) = 0
  public void Cos_Double_ReturnsCorrectCosine(double value, double expected) => Assert.AreEqual(expected, value.Cos(), _DOUBLE_ACCURACY);

  [Test]
  [TestCase(0.0, 0.0)] // tan(0) = 0
  [TestCase(Math.PI / 4, 1.0)] // tan(π/4) = 1
  public void Tan_Double_ReturnsCorrectTangent(double value, double expected) => Assert.AreEqual(expected, value.Tan(), _DOUBLE_ACCURACY);

  #endregion

  #region Edge Cases and Error Conditions

  [Test]
  public void DividedBy_Int_DivideByZero_ThrowsException() => Assert.Throws<DivideByZeroException>(() => 10.DividedBy(0));
  
  [Test]
  public void Round_Double_InvalidDigits_ThrowsException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => 3.14.Round(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => 3.14.Round(16));
  }

  #endregion
}