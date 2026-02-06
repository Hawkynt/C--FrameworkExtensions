using System.Globalization;
using System.Numerics;
using NUnit.Framework;

namespace System;

[TestFixture]
public class ConfigurableFloatingPointTests {

  private const double Tolerance = 0.01;

  // Default IEEE-like mantissa bits for common storage types (exponent computed automatically)
  private const int SByte_M = 3;   // signed 8-bit: 1 sign + 4 exp + 3 mantissa
  private const int Byte_M = 3;    // unsigned 8-bit: 5 exp + 3 mantissa
  private const int Short_M = 10;  // signed 16-bit: 1+5+10 (Half-like)
  private const int UShort_M = 10; // unsigned 16-bit: 6+10
  private const int Int_M = 23;    // signed 32-bit: 1+8+23 (float-like)
  private const int UInt_M = 23;   // unsigned 32-bit: 9+23
  private const int Long_M = 52;   // signed 64-bit: 1+11+52 (double-like)
  private const int ULong_M = 52;  // unsigned 64-bit: 12+52
  private const int Int96_M = 80;  // signed 96-bit: 1+15+80
  private const int UInt96_M = 80; // unsigned 96-bit: 16+80
  private const int Int128_M = 112;  // signed 128-bit: 1+15+112
  private const int UInt128_M = 112; // unsigned 128-bit: 16+112

  #region ConfigurableFloatingPoint<sbyte> (Signed 8-bit)

  [Test]
  public void SignedByte_Constants_AreCorrect() {
    Assert.AreEqual(0.0, (double)ConfigurableFloatingPoint<sbyte>.Zero(SByte_M), Tolerance);
    Assert.AreEqual(1.0, (double)ConfigurableFloatingPoint<sbyte>.One(SByte_M), 0.2);
  }

  [Test]
  public void SignedByte_HasSign_IsTrue() {
    Assert.IsTrue(ConfigurableFloatingPoint<sbyte>.HasSign);
  }

  [Test]
  public void SignedByte_SpecialValues_DetectedCorrectly() {
    var nan = ConfigurableFloatingPoint<sbyte>.NaN;
    var inf = ConfigurableFloatingPoint<sbyte>.PositiveInfinity;
    var negInf = ConfigurableFloatingPoint<sbyte>.NegativeInfinity;
    var zero = ConfigurableFloatingPoint<sbyte>.Zero(SByte_M);

    Assert.IsTrue(ConfigurableFloatingPoint<sbyte>.IsNaN(nan));
    Assert.IsTrue(ConfigurableFloatingPoint<sbyte>.IsInfinity(inf));
    Assert.IsTrue(ConfigurableFloatingPoint<sbyte>.IsPositiveInfinity(inf));
    Assert.IsTrue(ConfigurableFloatingPoint<sbyte>.IsNegativeInfinity(negInf));
    Assert.IsTrue(ConfigurableFloatingPoint<sbyte>.IsZero(zero));
    Assert.IsFalse(ConfigurableFloatingPoint<sbyte>.IsNaN(zero));
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(0.5)]
  [TestCase(-0.5)]
  public void SignedByte_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFloatingPoint<sbyte>.FromDouble(value, SByte_M);
    Assert.AreEqual(value, fp.ToDouble(), 0.5);
  }

  [Test]
  public void SignedByte_Negation_Works() {
    var positive = ConfigurableFloatingPoint<sbyte>.FromDouble(1.5, SByte_M);
    var negated = -positive;
    Assert.IsTrue(ConfigurableFloatingPoint<sbyte>.IsNegative(negated));
  }

  #endregion

  #region ConfigurableFloatingPoint<byte> (Unsigned 8-bit)

  [Test]
  public void UnsignedByte_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFloatingPoint<byte>.HasSign);
  }

  [Test]
  public void UnsignedByte_NegativeValue_ClampsToZero() {
    var result = ConfigurableFloatingPoint<byte>.FromDouble(-1.0, Byte_M);
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedByte_Subtraction_Saturates() {
    var small = ConfigurableFloatingPoint<byte>.FromDouble(1.0, Byte_M);
    var large = ConfigurableFloatingPoint<byte>.FromDouble(2.0, Byte_M);
    var result = small - large;
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedByte_Addition_Works() {
    var a = ConfigurableFloatingPoint<byte>.FromDouble(1.0, Byte_M);
    var b = ConfigurableFloatingPoint<byte>.FromDouble(2.0, Byte_M);
    var result = a + b;
    Assert.AreEqual(3.0, result.ToDouble(), 0.5);
  }

  #endregion

  #region ConfigurableFloatingPoint<short> (Signed 16-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14)]
  [TestCase(-3.14)]
  [TestCase(100.0)]
  [TestCase(-100.0)]
  public void SignedShort_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFloatingPoint<short>.FromDouble(value, Short_M);
    Assert.AreEqual(value, fp.ToDouble(), 0.05);
  }

  [Test]
  public void SignedShort_SpecialValues_DetectedCorrectly() {
    var nan = ConfigurableFloatingPoint<short>.NaN;
    var inf = ConfigurableFloatingPoint<short>.PositiveInfinity;
    Assert.IsTrue(ConfigurableFloatingPoint<short>.IsNaN(nan));
    Assert.IsTrue(ConfigurableFloatingPoint<short>.IsInfinity(inf));
    Assert.IsTrue(ConfigurableFloatingPoint<short>.IsFinite(ConfigurableFloatingPoint<short>.One(Short_M)));
  }

  [Test]
  public void SignedShort_Arithmetic_Works() {
    var a = ConfigurableFloatingPoint<short>.FromDouble(5.0, Short_M);
    var b = ConfigurableFloatingPoint<short>.FromDouble(3.0, Short_M);
    Assert.AreEqual(8.0, (a + b).ToDouble(), 0.05);
    Assert.AreEqual(2.0, (a - b).ToDouble(), 0.05);
    Assert.AreEqual(15.0, (a * b).ToDouble(), 0.1);
  }

  [Test]
  public void SignedShort_Division_Works() {
    var a = ConfigurableFloatingPoint<short>.FromDouble(10.0, Short_M);
    var b = ConfigurableFloatingPoint<short>.FromDouble(3.0, Short_M);
    Assert.AreEqual(3.333, (a / b).ToDouble(), 0.05);
  }

  #endregion

  #region ConfigurableFloatingPoint<ushort> (Unsigned 16-bit)

  [Test]
  public void UnsignedShort_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFloatingPoint<ushort>.HasSign);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.14)]
  [TestCase(100.0)]
  public void UnsignedShort_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFloatingPoint<ushort>.FromDouble(value, UShort_M);
    Assert.AreEqual(value, fp.ToDouble(), 0.05);
  }

  [Test]
  public void UnsignedShort_NegativeValue_ClampsToZero() {
    var result = ConfigurableFloatingPoint<ushort>.FromDouble(-5.0, UShort_M);
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  #endregion

  #region ConfigurableFloatingPoint<int> (Signed 32-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14159265)]
  [TestCase(-3.14159265)]
  [TestCase(1000.5)]
  [TestCase(-1000.5)]
  public void SignedInt_FromDouble_HighPrecision(double value) {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(value, Int_M);
    Assert.AreEqual(value, fp.ToDouble(), 0.0001);
  }

  [Test]
  public void SignedInt_SpecialValues_Work() {
    var max = ConfigurableFloatingPoint<int>.MaxValue(Int_M);
    var min = ConfigurableFloatingPoint<int>.MinValue(Int_M);
    var eps = ConfigurableFloatingPoint<int>.Epsilon(Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsFinite(max));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsFinite(min));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsSubnormal(eps));
  }

  #endregion

  #region ConfigurableFloatingPoint<uint> (Unsigned 32-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.14159265)]
  [TestCase(1000.5)]
  public void UnsignedInt_FromDouble_HighPrecision(double value) {
    var fp = ConfigurableFloatingPoint<uint>.FromDouble(value, UInt_M);
    Assert.AreEqual(value, fp.ToDouble(), 0.0001);
  }

  #endregion

  #region ConfigurableFloatingPoint<long> (Signed 64-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.141592653589793)]
  [TestCase(-2.718281828459045)]
  [TestCase(1e10)]
  [TestCase(-1e10)]
  public void SignedLong_FromDouble_VeryHighPrecision(double value) {
    var fp = ConfigurableFloatingPoint<long>.FromDouble(value, Long_M);
    Assert.AreEqual(value, fp.ToDouble(), Math.Abs(value) * 1e-10 + 1e-15);
  }

  [Test]
  public void SignedLong_Modulo_Works() {
    var a = ConfigurableFloatingPoint<long>.FromDouble(10.0, Long_M);
    var b = ConfigurableFloatingPoint<long>.FromDouble(3.0, Long_M);
    Assert.AreEqual(1.0, (a % b).ToDouble(), 0.001);
  }

  #endregion

  #region ConfigurableFloatingPoint<ulong> (Unsigned 64-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.141592653589793)]
  [TestCase(1e10)]
  public void UnsignedLong_FromDouble_VeryHighPrecision(double value) {
    var fp = ConfigurableFloatingPoint<ulong>.FromDouble(value, ULong_M);
    Assert.AreEqual(value, fp.ToDouble(), Math.Abs(value) * 1e-10 + 1e-15);
  }

  #endregion

  #region Comparison

  [Test]
  public void Comparison_LessThan_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    Assert.IsTrue(a < b);
    Assert.IsFalse(b < a);
  }

  [Test]
  public void Comparison_GreaterThan_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    var b = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    Assert.IsTrue(a > b);
    Assert.IsFalse(b > a);
  }

  [Test]
  public void Comparison_Equality_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var b = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    Assert.IsTrue(a == b);
    Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
  }

  [Test]
  public void Comparison_NaN_IsUnordered() {
    var nan = ConfigurableFloatingPoint<int>.NaN;
    var one = ConfigurableFloatingPoint<int>.One(Int_M);
    Assert.IsFalse(nan < one);
    Assert.IsFalse(nan > one);
    Assert.IsFalse(nan == one);
    Assert.IsTrue(nan != one);
  }

  #endregion

  #region Min/Max/Abs/Clamp/CopySign

  [Test]
  public void Min_ReturnsSmaller() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    Assert.AreEqual(1.0, ConfigurableFloatingPoint<int>.Min(a, b).ToDouble(), Tolerance);
  }

  [Test]
  public void Max_ReturnsLarger() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    Assert.AreEqual(2.0, ConfigurableFloatingPoint<int>.Max(a, b).ToDouble(), Tolerance);
  }

  [Test]
  public void Abs_ReturnsPositive() {
    var neg = ConfigurableFloatingPoint<int>.FromDouble(-5.0, Int_M);
    Assert.AreEqual(5.0, ConfigurableFloatingPoint<int>.Abs(neg).ToDouble(), Tolerance);
  }

  [Test]
  public void Clamp_ClampsToRange() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(10.0, Int_M);
    var min = ConfigurableFloatingPoint<int>.FromDouble(0.0, Int_M);
    var max = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    Assert.AreEqual(5.0, ConfigurableFloatingPoint<int>.Clamp(value, min, max).ToDouble(), Tolerance);
  }

  [Test]
  public void CopySign_CopiesSignBit() {
    var magnitude = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var negative = ConfigurableFloatingPoint<int>.FromDouble(-1.0, Int_M);
    var result = ConfigurableFloatingPoint<int>.CopySign(magnitude, negative);
    Assert.AreEqual(-5.0, result.ToDouble(), Tolerance);
  }

  #endregion

  #region Increment / Decrement

  [Test]
  public void Increment_Works() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    ++value;
    Assert.AreEqual(6.0, value.ToDouble(), Tolerance);
  }

  [Test]
  public void Decrement_Works() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    --value;
    Assert.AreEqual(4.0, value.ToDouble(), Tolerance);
  }

  #endregion

  #region Mixed-Type Arithmetic

  [Test]
  public void MixedType_Double_Arithmetic() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    Assert.AreEqual(7.0, (fp + 2.0).ToDouble(), Tolerance);
    Assert.AreEqual(3.0, (fp - 2.0).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (fp * 2.0).ToDouble(), Tolerance);
    Assert.AreEqual(2.5, (fp / 2.0).ToDouble(), Tolerance);
  }

  [Test]
  public void MixedType_Int_Arithmetic() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    Assert.AreEqual(7.0, (fp + 2).ToDouble(), Tolerance);
    Assert.AreEqual(3.0, (fp - 2).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (fp * 2).ToDouble(), Tolerance);
    Assert.AreEqual(2.5, (fp / 2).ToDouble(), Tolerance);
  }

  #endregion

  #region Parsing

  [Test]
  public void Parse_ValidNumber_Succeeds() {
    var result = ConfigurableFloatingPoint<int>.Parse("3.14", CultureInfo.InvariantCulture);
    Assert.AreEqual(3.14, result.ToDouble(), 0.001);
  }

  [Test]
  public void TryParse_ValidNumber_ReturnsTrue() {
    Assert.IsTrue(ConfigurableFloatingPoint<int>.TryParse("42.5", CultureInfo.InvariantCulture, out var result));
    Assert.AreEqual(42.5, result.ToDouble(), 0.001);
  }

  [Test]
  public void TryParse_InvalidNumber_ReturnsFalse() {
    Assert.IsFalse(ConfigurableFloatingPoint<int>.TryParse("not_a_number", CultureInfo.InvariantCulture, out _));
  }

  [Test]
  public void Parse_WithProvider_Succeeds() {
    var result = ConfigurableFloatingPoint<int>.Parse("3.14", CultureInfo.InvariantCulture);
    Assert.AreEqual(3.14, result.ToDouble(), 0.001);
  }

  #endregion

  #region ToString / Formatting

  [Test]
  public void ToString_ReturnsFormattedString() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var str = value.ToString("F2", CultureInfo.InvariantCulture);
    Assert.IsNotNull(str);
    Assert.AreEqual("3.14", str);
  }

  [Test]
  public void ToString_WithFormat_Works() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var str = value.ToString("F2", CultureInfo.InvariantCulture);
    Assert.AreEqual("3.14", str);
  }

  [Test]
  public void TryFormat_Works() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    Span<char> buffer = stackalloc char[32];
    Assert.IsTrue(value.TryFormat(buffer, out var written, "F2", CultureInfo.InvariantCulture));
    Assert.AreEqual("3.14", buffer[..written].ToString());
  }

  #endregion

  #region Explicit Conversions

  [Test]
  public void ExplicitConversion_ToFloat_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var result = (float)fp;
    Assert.AreEqual(3.14f, result, 0.001f);
  }

  [Test]
  public void ExplicitConversion_ToDecimal_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var result = (decimal)fp;
    Assert.AreEqual(3.14, (double)result, 0.001);
  }

  [Test]
  public void ExplicitConversion_ToInt_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(42.7, Int_M);
    var result = (int)fp;
    Assert.AreEqual(42, result);
  }

  #endregion

  #region Instance Config Properties

  [Test]
  public void InstanceConfig_MantissaBits_ReturnsCorrectValue() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    Assert.AreEqual(Int_M, fp.MantissaBits);
    Assert.AreEqual(8, fp.ExponentBits);
  }

  [Test]
  public void InstanceConfig_ExponentBias_IsCorrect() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    Assert.AreEqual(127, fp.ExponentBias);
  }

  [Test]
  public void InstanceConfig_TotalBits_IsCorrect() {
    Assert.AreEqual(32, ConfigurableFloatingPoint<int>.TotalBits);
    Assert.AreEqual(8, ConfigurableFloatingPoint<sbyte>.TotalBits);
    Assert.AreEqual(16, ConfigurableFloatingPoint<short>.TotalBits);
    Assert.AreEqual(64, ConfigurableFloatingPoint<long>.TotalBits);
  }

  [Test]
  public void InstanceConfig_CustomLayout_Works() {
    var fp = ConfigurableFloatingPoint<sbyte>.FromDouble(1.0, 2);
    Assert.AreEqual(2, fp.MantissaBits);
    Assert.AreEqual(5, fp.ExponentBits);
    Assert.AreEqual(15, fp.ExponentBias);
  }

  #endregion

  #region Mixed-Config Operations

  [Test]
  public void MixedConfig_Addition_UsesLeftConfig() {
    var a = ConfigurableFloatingPoint<sbyte>.FromDouble(1.0, 3);
    var b = ConfigurableFloatingPoint<sbyte>.FromDouble(1.0, 2);
    var result = a + b;
    Assert.AreEqual(2.0, result.ToDouble(), 0.5);
    Assert.AreEqual(3, result.MantissaBits);
  }

  [Test]
  public void MixedConfig_Equality_ComparesValues() {
    var a = ConfigurableFloatingPoint<sbyte>.FromDouble(1.0, 3);
    var b = ConfigurableFloatingPoint<sbyte>.FromDouble(1.0, 2);
    Assert.AreEqual(a, b);
    Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
  }

  #endregion

  #region Default(T) Behavior

  [Test]
  public void DefaultValue_UsesDefaultConfig() {
    var def = default(ConfigurableFloatingPoint<int>);
    Assert.AreEqual(0.0, def.ToDouble(), Tolerance);
    Assert.AreEqual(Int_M, def.MantissaBits);
    Assert.AreEqual(8, def.ExponentBits);
  }

  #endregion

  #region FromMemory / ToMemory

  [Test]
  public void FromMemory_ToMemory_RoundTrips_Byte() {
    var original = ConfigurableFloatingPoint<byte>.FromDouble(1.5, Byte_M);
    var bytes = original.ToMemory();
    var restored = ConfigurableFloatingPoint<byte>.FromMemory(bytes, Byte_M);
    Assert.AreEqual(original.ToDouble(), restored.ToDouble(), Tolerance);
  }

  [Test]
  public void FromMemory_ToMemory_RoundTrips_Int() {
    var original = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var bytes = original.ToMemory();
    Assert.AreEqual(4, bytes.Length);
    var restored = ConfigurableFloatingPoint<int>.FromMemory(bytes, Int_M);
    Assert.AreEqual(original.ToDouble(), restored.ToDouble(), 0.0001);
  }

  [Test]
  public void ToMemory_Span_Works() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(42.0, Int_M);
    Span<byte> buffer = stackalloc byte[4];
    var written = value.ToMemory(buffer);
    Assert.AreEqual(4, written);
    var restored = ConfigurableFloatingPoint<int>.FromMemory(buffer, Int_M);
    Assert.AreEqual(42.0, restored.ToDouble(), 0.0001);
  }

  [Test]
  public void FromMemory_TooSmallSpan_Throws() {
    Assert.Throws<ArgumentException>(() => ConfigurableFloatingPoint<int>.FromMemory(new byte[2], Int_M));
  }

  #endregion

  #region Constructor Validation

  [Test]
  public void Constructor_ZeroMantissa_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurableFloatingPoint<int>(0));
  }

  [Test]
  public void Constructor_TooManyMantissaBits_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurableFloatingPoint<int>(31));
  }

  #endregion

  #region Cross-Type Conversions

  [Test]
  public void ToFixedPoint_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.75, Int_M);
    var fixedPt = fp.ToFixedPoint(16);
    Assert.AreEqual(3.75, fixedPt.ToDouble(), 0.01);
  }

  [Test]
  public void ToQuarter_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(1.5, Int_M);
    var quarter = fp.ToQuarter();
    Assert.AreEqual(1.5, quarter.ToSingle(), 0.5);
  }

  [Test]
  public void ToHalf_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var half = fp.ToHalf();
    Assert.AreEqual(3.14, (double)half, 0.01);
  }

  [Test]
  public void ToBFloat16_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var bf16 = fp.ToBFloat16();
    Assert.AreEqual(3.14, bf16.ToSingle(), 0.1);
  }

  [Test]
  public void FromQuarter_Works() {
    var quarter = Quarter.FromSingle(1.5f);
    var fp = ConfigurableFloatingPoint<int>.FromQuarter(quarter, Int_M);
    Assert.AreEqual(1.5, fp.ToDouble(), 0.5);
  }

  [Test]
  public void FromBFloat16_Works() {
    var bf16 = BFloat16.FromSingle(3.14f);
    var fp = ConfigurableFloatingPoint<int>.FromBFloat16(bf16, Int_M);
    Assert.AreEqual(3.14, fp.ToDouble(), 0.1);
  }

  #endregion

  #region AsXxx Instance Convenience

  [Test]
  public void AsZero_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var zero = fp.AsZero;
    Assert.AreEqual(0.0, zero.ToDouble(), Tolerance);
    Assert.AreEqual(Int_M, zero.MantissaBits);
  }

  [Test]
  public void AsOne_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var one = fp.AsOne;
    Assert.AreEqual(1.0, one.ToDouble(), 0.001);
    Assert.AreEqual(Int_M, one.MantissaBits);
  }

  [Test]
  public void AsNaN_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var nan = fp.AsNaN;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan));
    Assert.AreEqual(Int_M, nan.MantissaBits);
  }

  #endregion

  #region CreateFromDouble Instance Method

  [Test]
  public void CreateFromDouble_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    var result = fp.CreateFromDouble(3.14);
    Assert.AreEqual(3.14, result.ToDouble(), 0.001);
    Assert.AreEqual(Int_M, result.MantissaBits);
  }

  #endregion

  #region FromRaw

  [Test]
  public void FromRaw_CreatesCorrectValue() {
    var one = ConfigurableFloatingPoint<int>.One(Int_M);
    var fromRaw = ConfigurableFloatingPoint<int>.FromRaw(one.RawValue, Int_M);
    Assert.AreEqual(one.ToDouble(), fromRaw.ToDouble(), Tolerance);
  }

  #endregion

  #region FromComponents

  [Test]
  public void FromComponents_CreatesCorrectValue() {
    var one = ConfigurableFloatingPoint<int>.One(Int_M);
    var fromComp = ConfigurableFloatingPoint<int>.FromComponents(0, 127, false, Int_M);
    Assert.AreEqual(1.0, fromComp.ToDouble(), Tolerance);
    Assert.AreEqual(one.RawValue, fromComp.RawValue);
  }

  #endregion

  #region Special Arithmetic Edge Cases

  [Test]
  public void Infinity_Plus_NegativeInfinity_IsNaN() {
    var posInf = ConfigurableFloatingPoint<int>.PositiveInfinity;
    var negInf = ConfigurableFloatingPoint<int>.NegativeInfinity;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(posInf + negInf));
  }

  [Test]
  public void Infinity_Minus_Infinity_IsNaN() {
    var inf = ConfigurableFloatingPoint<int>.PositiveInfinity;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(inf - inf));
  }

  [Test]
  public void Zero_Divided_By_Zero_IsNaN() {
    var zero = ConfigurableFloatingPoint<int>.Zero(Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(zero / zero));
  }

  [Test]
  public void One_Divided_By_Zero_IsInfinity() {
    var one = ConfigurableFloatingPoint<int>.One(Int_M);
    var zero = ConfigurableFloatingPoint<int>.Zero(Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsInfinity(one / zero));
  }

  [Test]
  public void NaN_Propagates_Through_Operations() {
    var nan = ConfigurableFloatingPoint<int>.NaN;
    var one = ConfigurableFloatingPoint<int>.One(Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan + one));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan - one));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan * one));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan / one));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan % one));
  }

  #endregion

  #region Infinity_Times_Zero

  [Test]
  public void Infinity_Times_Zero_IsNaN() {
    var inf = ConfigurableFloatingPoint<int>.PositiveInfinity;
    var zero = ConfigurableFloatingPoint<int>.Zero(Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(inf * zero));
  }

  #endregion

  #region MantissaBitsFromExponent

  [Test]
  public void MantissaBitsFromExponent_ReturnsCorrectValue() {
    Assert.AreEqual(23, ConfigurableFloatingPoint<int>.MantissaBitsFromExponent(8));
    Assert.AreEqual(52, ConfigurableFloatingPoint<long>.MantissaBitsFromExponent(11));
    Assert.AreEqual(10, ConfigurableFloatingPoint<short>.MantissaBitsFromExponent(5));
    Assert.AreEqual(3, ConfigurableFloatingPoint<sbyte>.MantissaBitsFromExponent(4));
  }

  #endregion

  #region ConvertTo

  [Test]
  public void ConvertTo_ChangesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, 23);
    var converted = fp.ConvertTo(20);
    Assert.AreEqual(20, converted.MantissaBits);
    Assert.AreEqual(3.14, converted.ToDouble(), 0.01);
  }

  [Test]
  public void ConvertTo_PreservesValue() {
    var fp = ConfigurableFloatingPoint<short>.FromDouble(1.5, 10);
    var converted = fp.ConvertTo(8);
    Assert.AreEqual(8, converted.MantissaBits);
    Assert.AreEqual(1.5, converted.ToDouble(), 0.1);
  }

  #endregion

  #region Cross-Config Arithmetic

  [Test]
  public void CrossConfig_Addition_ResultUsesLeftConfig() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(2.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(3.0, 15);
    var result = a + b;
    Assert.AreEqual(20, result.MantissaBits);
    Assert.AreEqual(5.0, result.ToDouble(), 0.1);
  }

  [Test]
  public void CrossConfig_Subtraction_ResultUsesLeftConfig() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(5.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(3.0, 15);
    var result = a - b;
    Assert.AreEqual(20, result.MantissaBits);
    Assert.AreEqual(2.0, result.ToDouble(), 0.1);
  }

  [Test]
  public void CrossConfig_Multiplication_ResultUsesLeftConfig() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(3.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(4.0, 15);
    var result = a * b;
    Assert.AreEqual(20, result.MantissaBits);
    Assert.AreEqual(12.0, result.ToDouble(), 0.1);
  }

  [Test]
  public void CrossConfig_Division_ResultUsesLeftConfig() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(10.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(4.0, 15);
    var result = a / b;
    Assert.AreEqual(20, result.MantissaBits);
    Assert.AreEqual(2.5, result.ToDouble(), 0.1);
  }

  [Test]
  public void CrossConfig_LessThan_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(2.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(3.0, 15);
    Assert.IsTrue(a < b);
    Assert.IsFalse(b < a);
  }

  [Test]
  public void CrossConfig_GreaterThan_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(3.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, 15);
    Assert.IsTrue(a > b);
    Assert.IsFalse(b > a);
  }

  [Test]
  public void CrossConfig_LessOrEqual_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(2.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(3.0, 15);
    var c = ConfigurableFloatingPoint<int>.FromDouble(2.0, 15);
    Assert.IsTrue(a <= b);
    Assert.IsTrue(a <= c);
    Assert.IsFalse(b <= a);
  }

  [Test]
  public void CrossConfig_GreaterOrEqual_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(3.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, 15);
    var c = ConfigurableFloatingPoint<int>.FromDouble(3.0, 15);
    Assert.IsTrue(a >= b);
    Assert.IsTrue(a >= c);
    Assert.IsFalse(b >= a);
  }

  [Test]
  public void CrossConfig_Equality_SameValue_DifferentConfig() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(1.0, 15);
    Assert.IsTrue(a == b);
    Assert.IsFalse(a != b);
    Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
  }

  [Test]
  public void CrossConfig_Inequality_DifferentValues() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, 15);
    Assert.IsTrue(a != b);
    Assert.IsFalse(a == b);
  }

  [Test]
  public void CrossConfig_Min_ReturnsSmaller() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, 15);
    var result = ConfigurableFloatingPoint<int>.Min(a, b);
    Assert.AreEqual(1.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void CrossConfig_Max_ReturnsLarger() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, 15);
    var result = ConfigurableFloatingPoint<int>.Max(a, b);
    Assert.AreEqual(2.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void CrossConfig_Clamp_ClampsToRange() {
    var value = ConfigurableFloatingPoint<int>.FromDouble(10.0, 20);
    var min = ConfigurableFloatingPoint<int>.FromDouble(0.0, 15);
    var max = ConfigurableFloatingPoint<int>.FromDouble(5.0, 18);
    var result = ConfigurableFloatingPoint<int>.Clamp(value, min, max);
    Assert.AreEqual(5.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void CrossConfig_CopySign_Works() {
    var magnitude = ConfigurableFloatingPoint<int>.FromDouble(5.0, 20);
    var negative = ConfigurableFloatingPoint<int>.FromDouble(-1.0, 15);
    var result = ConfigurableFloatingPoint<int>.CopySign(magnitude, negative);
    Assert.AreEqual(-5.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void CrossConfig_CompareTo_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(2.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(3.0, 15);
    Assert.IsTrue(a.CompareTo(b) < 0);
    Assert.IsTrue(b.CompareTo(a) > 0);
  }

  [Test]
  public void CrossConfig_CompareTo_EqualValues() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, 20);
    var b = ConfigurableFloatingPoint<int>.FromDouble(1.0, 15);
    Assert.AreEqual(0, a.CompareTo(b));
  }

  #endregion

  #region Cross-Type Arithmetic (Floating + Fixed)

  [Test]
  public void CrossType_FloatingPlusFixed_ReturnsFloating() {
    var floating = ConfigurableFloatingPoint<int>.FromDouble(2.5, Int_M);
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(1.5, 16);
    var result = floating + fixedPt;
    Assert.AreEqual(4.0, result.ToDouble(), 0.01);
    Assert.AreEqual(Int_M, result.MantissaBits);
  }

  [Test]
  public void CrossType_FloatingMinusFixed_ReturnsFloating() {
    var floating = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(2.0, 16);
    var result = floating - fixedPt;
    Assert.AreEqual(3.0, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FloatingTimesFixed_ReturnsFloating() {
    var floating = ConfigurableFloatingPoint<int>.FromDouble(3.0, Int_M);
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(4.0, 16);
    var result = floating * fixedPt;
    Assert.AreEqual(12.0, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FloatingDividedByFixed_ReturnsFloating() {
    var floating = ConfigurableFloatingPoint<int>.FromDouble(10.0, Int_M);
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(4.0, 16);
    var result = floating / fixedPt;
    Assert.AreEqual(2.5, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FloatingModuloFixed_ReturnsFloating() {
    var floating = ConfigurableFloatingPoint<int>.FromDouble(10.0, Int_M);
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    var result = floating % fixedPt;
    Assert.AreEqual(1.0, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FloatingComparedToFixed_Works() {
    var floating = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    Assert.IsTrue(floating.CompareTo(fixedPt) < 0);
  }

  #endregion

  #region Negative Zero Equality

  [Test]
  public void NegativeZero_Equals_PositiveZero() {
    var posZero = ConfigurableFloatingPoint<int>.Zero(Int_M);
    var negZero = ConfigurableFloatingPoint<int>.FromDouble(-0.0, Int_M);
    Assert.AreEqual(posZero, negZero);
    Assert.AreEqual(posZero.GetHashCode(), negZero.GetHashCode());
  }

  #endregion

  #region FromDecimal Precision

  [Test]
  public void FromDecimal_PreservesDecimalPrecision() {
    var decValue = 3.14159265358979323846m;
    var fp = ConfigurableFloatingPoint<long>.FromDecimal(decValue, Long_M);
    Assert.AreEqual((double)decValue, fp.ToDouble(), 1e-10);
  }

  [Test]
  public void FromDecimal_Zero_IsZero() {
    var fp = ConfigurableFloatingPoint<int>.FromDecimal(0m, Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsZero(fp));
  }

  [Test]
  public void FromDecimal_Integer_IsExact() {
    var fp = ConfigurableFloatingPoint<int>.FromDecimal(42m, Int_M);
    Assert.AreEqual(42.0, fp.ToDouble(), 0.001);
  }

  #endregion

  #region 96-bit and 128-bit Storage Types

  [Test]
  public void Int96_HasSign_IsTrue() {
    Assert.IsTrue(ConfigurableFloatingPoint<Int96>.HasSign);
  }

  [Test]
  public void UInt96_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFloatingPoint<UInt96>.HasSign);
  }

  [Test]
  public void Int128_HasSign_IsTrue() {
    Assert.IsTrue(ConfigurableFloatingPoint<Int128>.HasSign);
  }

  [Test]
  public void UInt128_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFloatingPoint<UInt128>.HasSign);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void Int96_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFloatingPoint<Int96>.FromDouble(value, Int96_M);
    Assert.AreEqual(value, fp.ToDouble(), Math.Abs(value) * 1e-10 + 1e-15);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void UInt96_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFloatingPoint<UInt96>.FromDouble(value, UInt96_M);
    Assert.AreEqual(value, fp.ToDouble(), Math.Abs(value) * 1e-10 + 1e-15);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void Int128_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFloatingPoint<Int128>.FromDouble(value, Int128_M);
    Assert.AreEqual(value, fp.ToDouble(), Math.Abs(value) * 1e-10 + 1e-15);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void UInt128_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFloatingPoint<UInt128>.FromDouble(value, UInt128_M);
    Assert.AreEqual(value, fp.ToDouble(), Math.Abs(value) * 1e-10 + 1e-15);
  }

  [Test]
  public void Int96_SpecialValues_Work() {
    Assert.IsTrue(ConfigurableFloatingPoint<Int96>.IsNaN(ConfigurableFloatingPoint<Int96>.NaN));
    Assert.IsTrue(ConfigurableFloatingPoint<Int96>.IsPositiveInfinity(ConfigurableFloatingPoint<Int96>.PositiveInfinity));
    Assert.IsTrue(ConfigurableFloatingPoint<Int96>.IsNegativeInfinity(ConfigurableFloatingPoint<Int96>.NegativeInfinity));
    Assert.IsTrue(ConfigurableFloatingPoint<Int96>.IsZero(ConfigurableFloatingPoint<Int96>.Zero(Int96_M)));
  }

  [Test]
  public void Int128_SpecialValues_Work() {
    Assert.IsTrue(ConfigurableFloatingPoint<Int128>.IsNaN(ConfigurableFloatingPoint<Int128>.NaN));
    Assert.IsTrue(ConfigurableFloatingPoint<Int128>.IsPositiveInfinity(ConfigurableFloatingPoint<Int128>.PositiveInfinity));
    Assert.IsTrue(ConfigurableFloatingPoint<Int128>.IsZero(ConfigurableFloatingPoint<Int128>.Zero(Int128_M)));
  }

  [Test]
  public void Int96_Arithmetic_Works() {
    var a = ConfigurableFloatingPoint<Int96>.FromDouble(5.0, Int96_M);
    var b = ConfigurableFloatingPoint<Int96>.FromDouble(3.0, Int96_M);
    Assert.AreEqual(8.0, (a + b).ToDouble(), 1e-10);
    Assert.AreEqual(2.0, (a - b).ToDouble(), 1e-10);
    Assert.AreEqual(15.0, (a * b).ToDouble(), 1e-10);
    Assert.AreEqual(5.0 / 3.0, (a / b).ToDouble(), 1e-10);
  }

  [Test]
  public void Int128_Arithmetic_Works() {
    var a = ConfigurableFloatingPoint<Int128>.FromDouble(5.0, Int128_M);
    var b = ConfigurableFloatingPoint<Int128>.FromDouble(3.0, Int128_M);
    Assert.AreEqual(8.0, (a + b).ToDouble(), 1e-10);
    Assert.AreEqual(2.0, (a - b).ToDouble(), 1e-10);
    Assert.AreEqual(15.0, (a * b).ToDouble(), 1e-10);
  }

  [Test]
  public void UInt96_NegativeValue_ClampsToZero() {
    var result = ConfigurableFloatingPoint<UInt96>.FromDouble(-1.0, UInt96_M);
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void UInt128_NegativeValue_ClampsToZero() {
    var result = ConfigurableFloatingPoint<UInt128>.FromDouble(-1.0, UInt128_M);
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void UInt96_Subtraction_Saturates() {
    var small = ConfigurableFloatingPoint<UInt96>.FromDouble(1.0, UInt96_M);
    var large = ConfigurableFloatingPoint<UInt96>.FromDouble(2.0, UInt96_M);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UInt128_Subtraction_Saturates() {
    var small = ConfigurableFloatingPoint<UInt128>.FromDouble(1.0, UInt128_M);
    var large = ConfigurableFloatingPoint<UInt128>.FromDouble(2.0, UInt128_M);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void TotalBits_96_128() {
    Assert.AreEqual(96, ConfigurableFloatingPoint<Int96>.TotalBits);
    Assert.AreEqual(96, ConfigurableFloatingPoint<UInt96>.TotalBits);
    Assert.AreEqual(128, ConfigurableFloatingPoint<Int128>.TotalBits);
    Assert.AreEqual(128, ConfigurableFloatingPoint<UInt128>.TotalBits);
  }

  #endregion

  #region All Explicit Conversions (Floating-Point)

  [Test]
  public void ExplicitConversion_ToByte_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(42.7, Int_M);
    Assert.AreEqual((byte)42, (byte)fp);
  }

  [Test]
  public void ExplicitConversion_ToSByte_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(-42.7, Int_M);
    Assert.AreEqual((sbyte)-42, (sbyte)fp);
  }

  [Test]
  public void ExplicitConversion_ToShort_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(1000.7, Int_M);
    Assert.AreEqual((short)1000, (short)fp);
  }

  [Test]
  public void ExplicitConversion_ToUShort_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(1000.7, Int_M);
    Assert.AreEqual((ushort)1000, (ushort)fp);
  }

  [Test]
  public void ExplicitConversion_ToUInt_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(42.7, Int_M);
    Assert.AreEqual(42u, (uint)fp);
  }

  [Test]
  public void ExplicitConversion_ToLong_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(100000.7, Int_M);
    Assert.AreEqual(100000L, (long)fp);
  }

  [Test]
  public void ExplicitConversion_ToULong_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(100000.7, Int_M);
    Assert.AreEqual(100000UL, (ulong)fp);
  }

  [Test]
  public void ExplicitConversion_ToHalf_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var half = (Half)fp;
    Assert.AreEqual(3.14, (double)half, 0.01);
  }

  [Test]
  public void ExplicitConversion_ToQuarter_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(1.5, Int_M);
    var quarter = (Quarter)fp;
    Assert.AreEqual(1.5, quarter.ToSingle(), 0.5);
  }

  [Test]
  public void ExplicitConversion_ToE4M3_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    var e4m3 = (E4M3)fp;
    Assert.AreEqual(2.0, e4m3.ToSingle(), 0.5);
  }

  [Test]
  public void ExplicitConversion_ToBFloat8_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    var bf8 = (BFloat8)fp;
    Assert.AreEqual(2.0, bf8.ToSingle(), 0.5);
  }

  [Test]
  public void ExplicitConversion_ToBFloat16_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    var bf16 = (BFloat16)fp;
    Assert.AreEqual(3.14, bf16.ToSingle(), 0.1);
  }

  [Test]
  public void ExplicitConversion_ToBFloat32_Works() {
    var fp = ConfigurableFloatingPoint<long>.FromDouble(3.14, Long_M);
    var bf32 = (BFloat32)fp;
    Assert.AreEqual(3.14, bf32.ToSingle(), 0.001);
  }

  [Test]
  public void ExplicitConversion_ToBFloat64_Works() {
    var fp = ConfigurableFloatingPoint<long>.FromDouble(3.14, Long_M);
    var bf64 = (BFloat64)fp;
    Assert.AreEqual(3.14, bf64.ToDouble(), 0.001);
  }

  #endregion

  #region All From* Factory Methods (Floating-Point)

  [Test]
  public void FromSingle_RoundTrips() {
    var fp = ConfigurableFloatingPoint<int>.FromSingle(1.5f, Int_M);
    Assert.AreEqual(1.5f, fp.ToSingle(), 0.001f);
  }

  [Test]
  public void FromHalf_RoundTrips() {
    var half = (Half)3.14;
    var fp = ConfigurableFloatingPoint<int>.FromHalf(half, Int_M);
    Assert.AreEqual((double)half, fp.ToDouble(), 0.01);
  }

  [Test]
  public void FromE4M3_RoundTrips() {
    var e4m3 = E4M3.FromSingle(2.0f);
    var fp = ConfigurableFloatingPoint<int>.FromE4M3(e4m3, Int_M);
    Assert.AreEqual(2.0, fp.ToDouble(), 0.5);
  }

  [Test]
  public void FromBFloat8_RoundTrips() {
    var bf8 = BFloat8.FromSingle(2.0f);
    var fp = ConfigurableFloatingPoint<int>.FromBFloat8(bf8, Int_M);
    Assert.AreEqual(2.0, fp.ToDouble(), 0.5);
  }

  [Test]
  public void FromBFloat32_RoundTrips() {
    var bf32 = BFloat32.FromSingle(3.14f);
    var fp = ConfigurableFloatingPoint<long>.FromBFloat32(bf32, Long_M);
    Assert.AreEqual(3.14, fp.ToDouble(), 0.001);
  }

  [Test]
  public void FromBFloat64_RoundTrips() {
    var bf64 = BFloat64.FromDouble(3.14);
    var fp = ConfigurableFloatingPoint<long>.FromBFloat64(bf64, Long_M);
    Assert.AreEqual(3.14, fp.ToDouble(), 0.001);
  }

  #endregion

  #region All To* Instance Methods (Floating-Point)

  [Test]
  public void ToSingle_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(3.14, Int_M);
    Assert.AreEqual(3.14f, fp.ToSingle(), 0.001f);
  }

  [Test]
  public void ToE4M3_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    Assert.AreEqual(2.0, fp.ToE4M3().ToSingle(), 0.5);
  }

  [Test]
  public void ToBFloat8_Works() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    Assert.AreEqual(2.0, fp.ToBFloat8().ToSingle(), 0.5);
  }

  [Test]
  public void ToBFloat32_Works() {
    var fp = ConfigurableFloatingPoint<long>.FromDouble(3.14, Long_M);
    Assert.AreEqual(3.14, fp.ToBFloat32().ToSingle(), 0.001);
  }

  [Test]
  public void ToBFloat64_Works() {
    var fp = ConfigurableFloatingPoint<long>.FromDouble(3.14, Long_M);
    Assert.AreEqual(3.14, fp.ToBFloat64().ToDouble(), 0.001);
  }

  #endregion

  #region Primitive-on-Left-Side Operators (Floating-Point)

  [Test]
  public void DoublePrimitive_LeftSide_Arithmetic() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    Assert.AreEqual(7.0, (2.0 + fp).ToDouble(), Tolerance);
    Assert.AreEqual(-3.0, (2.0 - fp).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (2.0 * fp).ToDouble(), Tolerance);
    Assert.AreEqual(0.4, (2.0 / fp).ToDouble(), Tolerance);
  }

  [Test]
  public void IntPrimitive_LeftSide_Arithmetic() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    Assert.AreEqual(7.0, (2 + fp).ToDouble(), Tolerance);
    Assert.AreEqual(-3.0, (2 - fp).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (2 * fp).ToDouble(), Tolerance);
    Assert.AreEqual(0.4, (2 / fp).ToDouble(), Tolerance);
  }

  #endregion

  #region Unsigned Saturation Across All Unsigned Types (Floating-Point)

  [Test]
  public void UnsignedShort_Subtraction_Saturates() {
    var small = ConfigurableFloatingPoint<ushort>.FromDouble(1.0, UShort_M);
    var large = ConfigurableFloatingPoint<ushort>.FromDouble(2.0, UShort_M);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedInt_Subtraction_Saturates() {
    var small = ConfigurableFloatingPoint<uint>.FromDouble(1.0, UInt_M);
    var large = ConfigurableFloatingPoint<uint>.FromDouble(2.0, UInt_M);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedLong_Subtraction_Saturates() {
    var small = ConfigurableFloatingPoint<ulong>.FromDouble(1.0, ULong_M);
    var large = ConfigurableFloatingPoint<ulong>.FromDouble(2.0, ULong_M);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedInt_NegativeValue_ClampsToZero() {
    Assert.AreEqual(0.0, ConfigurableFloatingPoint<uint>.FromDouble(-5.0, UInt_M).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedLong_NegativeValue_ClampsToZero() {
    Assert.AreEqual(0.0, ConfigurableFloatingPoint<ulong>.FromDouble(-5.0, ULong_M).ToDouble(), Tolerance);
  }

  #endregion

  #region Cross-Type Edge Cases (NaN/Infinity + Fixed)

  [Test]
  public void CrossType_NaN_Plus_Fixed_IsNaN() {
    var nan = ConfigurableFloatingPoint<int>.NaN;
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(1.0, 16);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan + fixedPt));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan * fixedPt));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan - fixedPt));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNaN(nan / fixedPt));
  }

  [Test]
  public void CrossType_Infinity_Plus_Fixed_IsInfinity() {
    var inf = ConfigurableFloatingPoint<int>.PositiveInfinity;
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(1.0, 16);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsPositiveInfinity(inf + fixedPt));
  }

  #endregion

  #region Unary Plus Operator

  [Test]
  public void UnaryPlus_PreservesValue() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    Assert.AreEqual(5.0, (+fp).ToDouble(), Tolerance);
  }

  [Test]
  public void UnaryPlus_PreservesNegative() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(-5.0, Int_M);
    Assert.AreEqual(-5.0, (+fp).ToDouble(), Tolerance);
  }

  #endregion

  #region CompareTo(object)

  [Test]
  public void CompareTo_Object_Works() {
    var a = ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M);
    var b = ConfigurableFloatingPoint<int>.FromDouble(2.0, Int_M);
    Assert.IsTrue(a.CompareTo((object)b) < 0);
    Assert.IsTrue(b.CompareTo((object)a) > 0);
    Assert.AreEqual(0, a.CompareTo((object)a));
  }

  #endregion

  #region Precision Test (>52-bit Mantissa)

  [Test]
  public void Int96_HighPrecision_ArithmeticWorks() {
    var a = ConfigurableFloatingPoint<Int96>.FromDouble(1.0, Int96_M);
    var b = ConfigurableFloatingPoint<Int96>.FromDouble(1.0, Int96_M);
    Assert.AreEqual(2.0, (a + b).ToDouble(), 1e-10);
    Assert.AreEqual(0.0, (a - b).ToDouble(), 1e-10);
    Assert.AreEqual(1.0, (a * b).ToDouble(), 1e-10);
    Assert.AreEqual(1.0, (a / b).ToDouble(), 1e-10);
  }

  [Test]
  public void Int128_HighPrecision_ArithmeticWorks() {
    var a = ConfigurableFloatingPoint<Int128>.FromDouble(1.0, Int128_M);
    var b = ConfigurableFloatingPoint<Int128>.FromDouble(2.0, Int128_M);
    Assert.AreEqual(3.0, (a + b).ToDouble(), 1e-10);
    Assert.AreEqual(-1.0, (a - b).ToDouble(), 1e-10);
    Assert.AreEqual(2.0, (a * b).ToDouble(), 1e-10);
    Assert.AreEqual(0.5, (a / b).ToDouble(), 1e-10);
  }

  #endregion

  #region Epsilon and Subnormal

  [Test]
  public void Epsilon_IsSubnormal_And_Positive() {
    var eps = ConfigurableFloatingPoint<int>.Epsilon(Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsSubnormal(eps));
    Assert.IsTrue(eps.ToDouble() > 0);
  }

  [Test]
  public void Epsilon_GreaterThan_Zero() {
    var eps = ConfigurableFloatingPoint<int>.Epsilon(Int_M);
    var zero = ConfigurableFloatingPoint<int>.Zero(Int_M);
    Assert.IsTrue(eps > zero);
  }

  [Test]
  public void Normal_Value_IsNormal() {
    var one = ConfigurableFloatingPoint<int>.One(Int_M);
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNormal(one));
    Assert.IsFalse(ConfigurableFloatingPoint<int>.IsNormal(ConfigurableFloatingPoint<int>.Zero(Int_M)));
  }

  #endregion

  #region IsNegative/IsFinite Comprehensive

  [Test]
  public void IsNegative_Comprehensive() {
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNegative(ConfigurableFloatingPoint<int>.FromDouble(-1.0, Int_M)));
    Assert.IsFalse(ConfigurableFloatingPoint<int>.IsNegative(ConfigurableFloatingPoint<int>.FromDouble(1.0, Int_M)));
    Assert.IsFalse(ConfigurableFloatingPoint<int>.IsNegative(ConfigurableFloatingPoint<int>.Zero(Int_M)));
  }

  [Test]
  public void IsFinite_Comprehensive() {
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsFinite(ConfigurableFloatingPoint<int>.One(Int_M)));
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsFinite(ConfigurableFloatingPoint<int>.Zero(Int_M)));
    Assert.IsFalse(ConfigurableFloatingPoint<int>.IsFinite(ConfigurableFloatingPoint<int>.NaN));
    Assert.IsFalse(ConfigurableFloatingPoint<int>.IsFinite(ConfigurableFloatingPoint<int>.PositiveInfinity));
  }

  #endregion

  #region AsXxx Remaining Properties

  [Test]
  public void AsEpsilon_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var eps = fp.AsEpsilon;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsSubnormal(eps));
    Assert.AreEqual(Int_M, eps.MantissaBits);
  }

  [Test]
  public void AsPositiveInfinity_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var inf = fp.AsPositiveInfinity;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsPositiveInfinity(inf));
    Assert.AreEqual(Int_M, inf.MantissaBits);
  }

  [Test]
  public void AsNegativeInfinity_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var negInf = fp.AsNegativeInfinity;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsNegativeInfinity(negInf));
    Assert.AreEqual(Int_M, negInf.MantissaBits);
  }

  [Test]
  public void AsMaxValue_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var max = fp.AsMaxValue;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsFinite(max));
    Assert.AreEqual(Int_M, max.MantissaBits);
  }

  [Test]
  public void AsMinValue_PreservesConfig() {
    var fp = ConfigurableFloatingPoint<int>.FromDouble(5.0, Int_M);
    var min = fp.AsMinValue;
    Assert.IsTrue(ConfigurableFloatingPoint<int>.IsFinite(min));
    Assert.AreEqual(Int_M, min.MantissaBits);
  }

  #endregion

}

[TestFixture]
public class ConfigurableFixedPointTests {

  private const double Tolerance = 0.01;

  // Default fractional bits for common tests
  private const int SByte_F = 4;
  private const int Byte_F = 4;
  private const int Short_F = 8;
  private const int UShort_F = 8;
  private const int Int_F = 16;
  private const int UInt_F = 16;
  private const int Long_F = 32;
  private const int ULong_F = 32;
  private const int Int96_F = 48;
  private const int UInt96_F = 48;
  private const int Int128_F = 64;
  private const int UInt128_F = 64;

  #region ConfigurableFixedPoint<sbyte> (Signed 8-bit)

  [Test]
  public void SignedByte_HasSign_IsTrue() {
    Assert.IsTrue(ConfigurableFixedPoint<sbyte>.HasSign);
  }

  [Test]
  public void SignedByte_Constants_AreCorrect() {
    Assert.AreEqual(0.0, (double)ConfigurableFixedPoint<sbyte>.Zero(SByte_F), Tolerance);
    Assert.AreEqual(1.0, (double)ConfigurableFixedPoint<sbyte>.One(SByte_F), Tolerance);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(0.5)]
  [TestCase(-0.5)]
  public void SignedByte_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFixedPoint<sbyte>.FromDouble(value, SByte_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.25);
  }

  [Test]
  public void SignedByte_Negation_Works() {
    var positive = ConfigurableFixedPoint<sbyte>.FromDouble(1.5, SByte_F);
    var negated = -positive;
    Assert.AreEqual(-1.5, negated.ToDouble(), 0.25);
  }

  #endregion

  #region ConfigurableFixedPoint<byte> (Unsigned 8-bit)

  [Test]
  public void UnsignedByte_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFixedPoint<byte>.HasSign);
  }

  [Test]
  public void UnsignedByte_NegativeValue_ClampsToZero() {
    var result = ConfigurableFixedPoint<byte>.FromDouble(-1.0, Byte_F);
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedByte_Subtraction_Saturates() {
    var small = ConfigurableFixedPoint<byte>.FromDouble(1.0, Byte_F);
    var large = ConfigurableFixedPoint<byte>.FromDouble(2.0, Byte_F);
    var result = small - large;
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  #endregion

  #region ConfigurableFixedPoint<short> (Signed 16-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14)]
  [TestCase(-3.14)]
  [TestCase(100.5)]
  public void SignedShort_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFixedPoint<short>.FromDouble(value, Short_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.01);
  }

  [Test]
  public void SignedShort_Arithmetic_Works() {
    var a = ConfigurableFixedPoint<short>.FromDouble(5.0, Short_F);
    var b = ConfigurableFixedPoint<short>.FromDouble(3.0, Short_F);
    Assert.AreEqual(8.0, (a + b).ToDouble(), 0.01);
    Assert.AreEqual(2.0, (a - b).ToDouble(), 0.01);
    Assert.AreEqual(15.0, (a * b).ToDouble(), 0.1);
  }

  [Test]
  public void SignedShort_Division_Works() {
    var a = ConfigurableFixedPoint<short>.FromDouble(10.0, Short_F);
    var b = ConfigurableFixedPoint<short>.FromDouble(4.0, Short_F);
    Assert.AreEqual(2.5, (a / b).ToDouble(), 0.01);
  }

  #endregion

  #region ConfigurableFixedPoint<ushort> (Unsigned 16-bit)

  [Test]
  public void UnsignedShort_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFixedPoint<ushort>.HasSign);
  }

  [Test]
  public void UnsignedShort_NegativeValue_ClampsToZero() {
    var result = ConfigurableFixedPoint<ushort>.FromDouble(-5.0, UShort_F);
    Assert.AreEqual(0.0, result.ToDouble(), Tolerance);
  }

  #endregion

  #region ConfigurableFixedPoint<int> (Signed 32-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14159265)]
  [TestCase(-3.14159265)]
  [TestCase(1000.5)]
  public void SignedInt_FromDouble_HighPrecision(double value) {
    var fp = ConfigurableFixedPoint<int>.FromDouble(value, Int_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.001);
  }

  [Test]
  public void SignedInt_MaxValue_MinValue_Work() {
    var max = ConfigurableFixedPoint<int>.MaxValue(Int_F);
    var min = ConfigurableFixedPoint<int>.MinValue(Int_F);
    Assert.IsTrue(max.ToDouble() > 0);
    Assert.IsTrue(min.ToDouble() < 0);
  }

  #endregion

  #region ConfigurableFixedPoint<uint> (Unsigned 32-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.14159265)]
  [TestCase(1000.5)]
  public void UnsignedInt_FromDouble_HighPrecision(double value) {
    var fp = ConfigurableFixedPoint<uint>.FromDouble(value, UInt_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.001);
  }

  #endregion

  #region ConfigurableFixedPoint<long> (Signed 64-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.141592653589793)]
  [TestCase(-3.141592653589793)]
  [TestCase(100000.5)]
  public void SignedLong_FromDouble_VeryHighPrecision(double value) {
    var fp = ConfigurableFixedPoint<long>.FromDouble(value, Long_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.000001);
  }

  [Test]
  public void SignedLong_Modulo_Works() {
    var a = ConfigurableFixedPoint<long>.FromDouble(10.0, Long_F);
    var b = ConfigurableFixedPoint<long>.FromDouble(3.0, Long_F);
    Assert.AreEqual(1.0, (a % b).ToDouble(), 0.001);
  }

  #endregion

  #region ConfigurableFixedPoint<ulong> (Unsigned 64-bit)

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.141592653589793)]
  [TestCase(100000.5)]
  public void UnsignedLong_FromDouble_VeryHighPrecision(double value) {
    var fp = ConfigurableFixedPoint<ulong>.FromDouble(value, ULong_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.000001);
  }

  #endregion

  #region Comparison

  [Test]
  public void Comparison_LessThan_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.IsTrue(a < b);
    Assert.IsFalse(b < a);
  }

  [Test]
  public void Comparison_Equality_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    Assert.IsTrue(a == b);
    Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
  }

  #endregion

  #region Min/Max/Abs/Clamp

  [Test]
  public void Min_ReturnsSmaller() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.AreEqual(1.0, ConfigurableFixedPoint<int>.Min(a, b).ToDouble(), Tolerance);
  }

  [Test]
  public void Max_ReturnsLarger() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.AreEqual(2.0, ConfigurableFixedPoint<int>.Max(a, b).ToDouble(), Tolerance);
  }

  [Test]
  public void Abs_ReturnsPositive() {
    var neg = ConfigurableFixedPoint<int>.FromDouble(-5.0, Int_F);
    Assert.AreEqual(5.0, ConfigurableFixedPoint<int>.Abs(neg).ToDouble(), Tolerance);
  }

  [Test]
  public void Clamp_ClampsToRange() {
    var value = ConfigurableFixedPoint<int>.FromDouble(10.0, Int_F);
    var min = ConfigurableFixedPoint<int>.FromDouble(0.0, Int_F);
    var max = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    Assert.AreEqual(5.0, ConfigurableFixedPoint<int>.Clamp(value, min, max).ToDouble(), Tolerance);
  }

  #endregion

  #region Floor/Ceiling/Round/Truncate

  [Test]
  public void Floor_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.7, Int_F);
    Assert.AreEqual(3.0, ConfigurableFixedPoint<int>.Floor(value).ToDouble(), Tolerance);
  }

  [Test]
  public void Floor_Negative_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(-3.3, Int_F);
    Assert.AreEqual(-4.0, ConfigurableFixedPoint<int>.Floor(value).ToDouble(), Tolerance);
  }

  [Test]
  public void Ceiling_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.3, Int_F);
    Assert.AreEqual(4.0, ConfigurableFixedPoint<int>.Ceiling(value).ToDouble(), Tolerance);
  }

  [Test]
  public void Round_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.5, Int_F);
    Assert.AreEqual(4.0, ConfigurableFixedPoint<int>.Round(value).ToDouble(), Tolerance);
  }

  [Test]
  public void Truncate_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.7, Int_F);
    Assert.AreEqual(3.0, ConfigurableFixedPoint<int>.Truncate(value).ToDouble(), Tolerance);
  }

  [Test]
  public void FractionalPart_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.75, Int_F);
    Assert.AreEqual(0.75, ConfigurableFixedPoint<int>.FractionalPart(value).ToDouble(), 0.01);
  }

  #endregion

  #region Mixed-Type Arithmetic

  [Test]
  public void MixedType_Int_Arithmetic() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    Assert.AreEqual(7.0, (fp + 2).ToDouble(), Tolerance);
    Assert.AreEqual(3.0, (fp - 2).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (fp * 2).ToDouble(), Tolerance);
    Assert.AreEqual(2.5, (fp / 2).ToDouble(), Tolerance);
  }

  [Test]
  public void MixedType_Double_Arithmetic() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    Assert.AreEqual(7.0, (fp + 2.0).ToDouble(), Tolerance);
    Assert.AreEqual(3.0, (fp - 2.0).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (fp * 2.0).ToDouble(), Tolerance);
    Assert.AreEqual(2.5, (fp / 2.0).ToDouble(), Tolerance);
  }

  #endregion

  #region Parsing

  [Test]
  public void Parse_ValidNumber_Succeeds() {
    var result = ConfigurableFixedPoint<int>.Parse("3.14", CultureInfo.InvariantCulture);
    Assert.AreEqual(3.14, result.ToDouble(), 0.01);
  }

  [Test]
  public void TryParse_ValidNumber_ReturnsTrue() {
    Assert.IsTrue(ConfigurableFixedPoint<int>.TryParse("42.5", CultureInfo.InvariantCulture, out var result));
    Assert.AreEqual(42.5, result.ToDouble(), 0.01);
  }

  [Test]
  public void TryParse_InvalidNumber_ReturnsFalse() {
    Assert.IsFalse(ConfigurableFixedPoint<int>.TryParse("not_a_number", CultureInfo.InvariantCulture, out _));
  }

  #endregion

  #region ToString / Formatting

  [Test]
  public void ToString_ReturnsFormattedString() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var str = value.ToString("F2", CultureInfo.InvariantCulture);
    Assert.IsNotNull(str);
    Assert.AreEqual("3.14", str);
  }

  [Test]
  public void ToString_WithFormat_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var str = value.ToString("F2", CultureInfo.InvariantCulture);
    Assert.AreEqual("3.14", str);
  }

  #endregion

  #region Explicit Conversions

  [Test]
  public void ExplicitConversion_ToFloat_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var result = (float)fp;
    Assert.AreEqual(3.14f, result, 0.01f);
  }

  [Test]
  public void ExplicitConversion_ToDecimal_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var result = (decimal)fp;
    Assert.AreEqual(3.14, (double)result, 0.01);
  }

  [Test]
  public void ExplicitConversion_ToInt_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(42.7, Int_F);
    var result = (int)fp;
    Assert.AreEqual(42, result);
  }

  #endregion

  #region Instance Config Properties

  [Test]
  public void InstanceConfig_FractionalBits_ReturnsCorrectValue() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    Assert.AreEqual(Int_F, fp.FractionalBits);
    Assert.AreEqual(32 - 1 - Int_F, fp.IntegerBits);
  }

  [Test]
  public void InstanceConfig_Scale_IsCorrect() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    Assert.AreEqual(1 << Int_F, (int)fp.Scale);
  }

  [Test]
  public void InstanceConfig_TotalBits_IsCorrect() {
    Assert.AreEqual(32, ConfigurableFixedPoint<int>.TotalBits);
    Assert.AreEqual(8, ConfigurableFixedPoint<sbyte>.TotalBits);
    Assert.AreEqual(16, ConfigurableFixedPoint<short>.TotalBits);
    Assert.AreEqual(64, ConfigurableFixedPoint<long>.TotalBits);
  }

  #endregion

  #region Mixed-Config Operations

  [Test]
  public void MixedConfig_Addition_UsesLeftConfig() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(1.0, 16);
    var result = a + b;
    Assert.AreEqual(2.0, result.ToDouble(), 0.01);
    Assert.AreEqual(8, result.FractionalBits);
  }

  [Test]
  public void MixedConfig_Equality_ComparesValues() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(1.0, 16);
    Assert.AreEqual(a, b);
    Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
  }

  #endregion

  #region Default(T) Behavior

  [Test]
  public void DefaultValue_UsesDefaultConfig() {
    var def = default(ConfigurableFixedPoint<int>);
    Assert.AreEqual(0.0, def.ToDouble(), Tolerance);
    Assert.AreEqual(16, def.FractionalBits);
  }

  #endregion

  #region FromMemory / ToMemory

  [Test]
  public void FromMemory_ToMemory_RoundTrips_Byte() {
    var original = ConfigurableFixedPoint<byte>.FromDouble(1.5, Byte_F);
    var bytes = original.ToMemory();
    var restored = ConfigurableFixedPoint<byte>.FromMemory(bytes, Byte_F);
    Assert.AreEqual(original.ToDouble(), restored.ToDouble(), Tolerance);
  }

  [Test]
  public void FromMemory_ToMemory_RoundTrips_Int() {
    var original = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var bytes = original.ToMemory();
    Assert.AreEqual(4, bytes.Length);
    var restored = ConfigurableFixedPoint<int>.FromMemory(bytes, Int_F);
    Assert.AreEqual(original.ToDouble(), restored.ToDouble(), 0.001);
  }

  [Test]
  public void FromMemory_ToMemory_Negative_RoundTrips() {
    var original = ConfigurableFixedPoint<int>.FromDouble(-7.25, Int_F);
    var bytes = original.ToMemory();
    var restored = ConfigurableFixedPoint<int>.FromMemory(bytes, Int_F);
    Assert.AreEqual(original.ToDouble(), restored.ToDouble(), 0.001);
  }

  [Test]
  public void ToMemory_Span_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(42.0, Int_F);
    Span<byte> buffer = stackalloc byte[4];
    var written = value.ToMemory(buffer);
    Assert.AreEqual(4, written);
    var restored = ConfigurableFixedPoint<int>.FromMemory(buffer, Int_F);
    Assert.AreEqual(42.0, restored.ToDouble(), 0.001);
  }

  [Test]
  public void FromMemory_TooSmallSpan_Throws() {
    Assert.Throws<ArgumentException>(() => ConfigurableFixedPoint<int>.FromMemory(new byte[2], Int_F));
  }

  #endregion

  #region Constructor Validation

  [Test]
  public void Constructor_NegativeFractionalBits_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurableFixedPoint<int>(-1));
  }

  [Test]
  public void Constructor_TooManyFractionalBits_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurableFixedPoint<int>(33));
  }

  #endregion

  #region Cross-Type Conversions

  [Test]
  public void ToFloatingPoint_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.75, Int_F);
    var floatingPt = fp.ToFloatingPoint(23);
    Assert.AreEqual(3.75, floatingPt.ToDouble(), 0.01);
  }

  [Test]
  public void ToQuarter_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1.5, Int_F);
    var quarter = fp.ToQuarter();
    Assert.AreEqual(1.5, quarter.ToSingle(), 0.5);
  }

  [Test]
  public void ToHalf_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var half = fp.ToHalf();
    Assert.AreEqual(3.14, (double)half, 0.01);
  }

  [Test]
  public void FromQuarter_Works() {
    var quarter = Quarter.FromSingle(1.5f);
    var fp = ConfigurableFixedPoint<int>.FromQuarter(quarter, Int_F);
    Assert.AreEqual(1.5, fp.ToDouble(), 0.5);
  }

  [Test]
  public void FromBFloat16_Works() {
    var bf16 = BFloat16.FromSingle(3.14f);
    var fp = ConfigurableFixedPoint<int>.FromBFloat16(bf16, Int_F);
    Assert.AreEqual(3.14, fp.ToDouble(), 0.1);
  }

  #endregion

  #region AsXxx Instance Convenience

  [Test]
  public void AsZero_PreservesConfig() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var zero = fp.AsZero;
    Assert.AreEqual(0.0, zero.ToDouble(), Tolerance);
    Assert.AreEqual(Int_F, zero.FractionalBits);
  }

  [Test]
  public void AsOne_PreservesConfig() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var one = fp.AsOne;
    Assert.AreEqual(1.0, one.ToDouble(), Tolerance);
    Assert.AreEqual(Int_F, one.FractionalBits);
  }

  #endregion

  #region CreateFromDouble Instance Method

  [Test]
  public void CreateFromDouble_PreservesConfig() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var result = fp.CreateFromDouble(3.14);
    Assert.AreEqual(3.14, result.ToDouble(), 0.001);
    Assert.AreEqual(Int_F, result.FractionalBits);
  }

  #endregion

  #region FromRaw

  [Test]
  public void FromRaw_CreatesCorrectValue() {
    var one = ConfigurableFixedPoint<int>.One(Int_F);
    var fromRaw = ConfigurableFixedPoint<int>.FromRaw(one.RawValue, Int_F);
    Assert.AreEqual(one.ToDouble(), fromRaw.ToDouble(), Tolerance);
  }

  #endregion

  #region Integer Conversions

  [Test]
  public void FromInt32_Works() {
    var fp = ConfigurableFixedPoint<int>.FromInt32(42, Int_F);
    Assert.AreEqual(42.0, fp.ToDouble(), Tolerance);
  }

  [Test]
  public void FromInt64_Works() {
    var fp = ConfigurableFixedPoint<long>.FromInt64(100L, Long_F);
    Assert.AreEqual(100.0, fp.ToDouble(), Tolerance);
  }

  [Test]
  public void ToInt32_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(42.7, Int_F);
    Assert.AreEqual(42, fp.ToInt32());
  }

  [Test]
  public void ToInt64_Works() {
    var fp = ConfigurableFixedPoint<long>.FromDouble(100.5, Long_F);
    Assert.AreEqual(100L, fp.ToInt64());
  }

  [Test]
  public void ToIntegerPart_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(42.75, Int_F);
    Assert.AreEqual(42, (int)fp.ToIntegerPart());
  }

  #endregion

  #region DivideByZero

  [Test]
  public void DivideByZero_Throws() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var zero = ConfigurableFixedPoint<int>.Zero(Int_F);
    Assert.Throws<DivideByZeroException>(() => { var _ = a / zero; });
  }

  [Test]
  public void ModuloByZero_Throws() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var zero = ConfigurableFixedPoint<int>.Zero(Int_F);
    Assert.Throws<DivideByZeroException>(() => { var _ = a % zero; });
  }

  #endregion

  #region NaN / Infinity Rejection

  [Test]
  public void FromDouble_NaN_Throws() {
    Assert.Throws<ArgumentException>(() => ConfigurableFixedPoint<int>.FromDouble(double.NaN, Int_F));
  }

  [Test]
  public void FromDouble_Infinity_Throws() {
    Assert.Throws<ArgumentException>(() => ConfigurableFixedPoint<int>.FromDouble(double.PositiveInfinity, Int_F));
  }

  #endregion

  #region Cross-Config Arithmetic

  [Test]
  public void CrossConfig_Addition_ResultUsesLeftConfig() {
    var a = ConfigurableFixedPoint<int>.FromDouble(2.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    var result = a + b;
    Assert.AreEqual(8, result.FractionalBits);
    Assert.AreEqual(5.0, result.ToDouble(), 0.05);
  }

  [Test]
  public void CrossConfig_Subtraction_ResultUsesLeftConfig() {
    var a = ConfigurableFixedPoint<int>.FromDouble(5.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    var result = a - b;
    Assert.AreEqual(8, result.FractionalBits);
    Assert.AreEqual(2.0, result.ToDouble(), 0.05);
  }

  [Test]
  public void CrossConfig_Multiplication_ResultUsesLeftConfig() {
    var a = ConfigurableFixedPoint<int>.FromDouble(3.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(4.0, 16);
    var result = a * b;
    Assert.AreEqual(8, result.FractionalBits);
    Assert.AreEqual(12.0, result.ToDouble(), 0.1);
  }

  [Test]
  public void CrossConfig_Division_ResultUsesLeftConfig() {
    var a = ConfigurableFixedPoint<int>.FromDouble(10.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(4.0, 16);
    var result = a / b;
    Assert.AreEqual(8, result.FractionalBits);
    Assert.AreEqual(2.5, result.ToDouble(), 0.05);
  }

  [Test]
  public void CrossConfig_LessThan_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(2.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    Assert.IsTrue(a < b);
    Assert.IsFalse(b < a);
  }

  [Test]
  public void CrossConfig_GreaterThan_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(3.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, 16);
    Assert.IsTrue(a > b);
    Assert.IsFalse(b > a);
  }

  [Test]
  public void CrossConfig_LessOrEqual_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(2.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    var c = ConfigurableFixedPoint<int>.FromDouble(2.0, 16);
    Assert.IsTrue(a <= b);
    Assert.IsTrue(a <= c);
    Assert.IsFalse(b <= a);
  }

  [Test]
  public void CrossConfig_GreaterOrEqual_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(3.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, 16);
    var c = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    Assert.IsTrue(a >= b);
    Assert.IsTrue(a >= c);
    Assert.IsFalse(b >= a);
  }

  [Test]
  public void CrossConfig_Equality_SameValue_DifferentConfig() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(1.0, 16);
    Assert.IsTrue(a == b);
    Assert.IsFalse(a != b);
    Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
  }

  [Test]
  public void CrossConfig_Inequality_DifferentValues() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, 16);
    Assert.IsTrue(a != b);
    Assert.IsFalse(a == b);
  }

  [Test]
  public void CrossConfig_CompareTo_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(2.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    Assert.IsTrue(a.CompareTo(b) < 0);
    Assert.IsTrue(b.CompareTo(a) > 0);
  }

  [Test]
  public void CrossConfig_CompareTo_EqualValues() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(1.0, 16);
    Assert.AreEqual(0, a.CompareTo(b));
  }

  [Test]
  public void CrossConfig_Min_ReturnsSmaller() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, 16);
    var result = ConfigurableFixedPoint<int>.Min(a, b);
    Assert.AreEqual(1.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void CrossConfig_Max_ReturnsLarger() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, 16);
    var result = ConfigurableFixedPoint<int>.Max(a, b);
    Assert.AreEqual(2.0, result.ToDouble(), Tolerance);
  }

  [Test]
  public void CrossConfig_Clamp_ClampsToRange() {
    var value = ConfigurableFixedPoint<int>.FromDouble(10.0, 8);
    var min = ConfigurableFixedPoint<int>.FromDouble(0.0, 16);
    var max = ConfigurableFixedPoint<int>.FromDouble(5.0, 12);
    var result = ConfigurableFixedPoint<int>.Clamp(value, min, max);
    Assert.AreEqual(5.0, result.ToDouble(), 0.05);
  }

  [Test]
  public void CrossConfig_Modulo_ResultUsesLeftConfig() {
    var a = ConfigurableFixedPoint<int>.FromDouble(10.0, 8);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.0, 16);
    var result = a % b;
    Assert.AreEqual(8, result.FractionalBits);
    Assert.AreEqual(1.0, result.ToDouble(), 0.05);
  }

  #endregion

  #region Cross-Type Arithmetic (Fixed + Floating)

  [Test]
  public void CrossType_FixedPlusFloating_ReturnsFixed() {
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(2.5, Int_F);
    var floating = ConfigurableFloatingPoint<int>.FromDouble(1.5, 23);
    var result = fixedPt + floating;
    Assert.AreEqual(4.0, result.ToDouble(), 0.01);
    Assert.AreEqual(Int_F, result.FractionalBits);
  }

  [Test]
  public void CrossType_FixedMinusFloating_ReturnsFixed() {
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var floating = ConfigurableFloatingPoint<int>.FromDouble(2.0, 23);
    var result = fixedPt - floating;
    Assert.AreEqual(3.0, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FixedTimesFloating_ReturnsFixed() {
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(3.0, Int_F);
    var floating = ConfigurableFloatingPoint<int>.FromDouble(4.0, 23);
    var result = fixedPt * floating;
    Assert.AreEqual(12.0, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FixedDividedByFloating_ReturnsFixed() {
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(10.0, Int_F);
    var floating = ConfigurableFloatingPoint<int>.FromDouble(4.0, 23);
    var result = fixedPt / floating;
    Assert.AreEqual(2.5, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FixedModuloFloating_ReturnsFixed() {
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(10.0, Int_F);
    var floating = ConfigurableFloatingPoint<int>.FromDouble(3.0, 23);
    var result = fixedPt % floating;
    Assert.AreEqual(1.0, result.ToDouble(), 0.01);
  }

  [Test]
  public void CrossType_FixedComparedToFloating_Works() {
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    var floating = ConfigurableFloatingPoint<int>.FromDouble(3.0, 23);
    Assert.IsTrue(fixedPt.CompareTo(floating) < 0);
  }

  #endregion

  #region ConvertTo

  [Test]
  public void ConvertTo_ChangesConfig() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, 16);
    var converted = fp.ConvertTo(8);
    Assert.AreEqual(8, converted.FractionalBits);
    Assert.AreEqual(3.14, converted.ToDouble(), 0.01);
  }

  [Test]
  public void ConvertTo_IncreasePrecision() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1.5, 8);
    var converted = fp.ConvertTo(16);
    Assert.AreEqual(16, converted.FractionalBits);
    Assert.AreEqual(1.5, converted.ToDouble(), 0.001);
  }

  #endregion

  #region 96-bit and 128-bit Storage Types (Fixed-Point)

  [Test]
  public void Int96_HasSign_IsTrue() {
    Assert.IsTrue(ConfigurableFixedPoint<Int96>.HasSign);
  }

  [Test]
  public void UInt96_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFixedPoint<UInt96>.HasSign);
  }

  [Test]
  public void Int128_HasSign_IsTrue() {
    Assert.IsTrue(ConfigurableFixedPoint<Int128>.HasSign);
  }

  [Test]
  public void UInt128_HasSign_IsFalse() {
    Assert.IsFalse(ConfigurableFixedPoint<UInt128>.HasSign);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void Int96_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFixedPoint<Int96>.FromDouble(value, Int96_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.000001);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void UInt96_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFixedPoint<UInt96>.FromDouble(value, UInt96_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.000001);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(-1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void Int128_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFixedPoint<Int128>.FromDouble(value, Int128_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.000001);
  }

  [Test]
  [TestCase(0.0)]
  [TestCase(1.0)]
  [TestCase(3.14)]
  [TestCase(1000.5)]
  public void UInt128_FromDouble_RoundTrips(double value) {
    var fp = ConfigurableFixedPoint<UInt128>.FromDouble(value, UInt128_F);
    Assert.AreEqual(value, fp.ToDouble(), 0.000001);
  }

  [Test]
  public void Int96_Arithmetic_Works() {
    var a = ConfigurableFixedPoint<Int96>.FromDouble(5.0, Int96_F);
    var b = ConfigurableFixedPoint<Int96>.FromDouble(3.0, Int96_F);
    Assert.AreEqual(8.0, (a + b).ToDouble(), 0.000001);
    Assert.AreEqual(2.0, (a - b).ToDouble(), 0.000001);
    Assert.AreEqual(15.0, (a * b).ToDouble(), 0.001);
    Assert.AreEqual(5.0 / 3.0, (a / b).ToDouble(), 0.000001);
  }

  [Test]
  public void Int128_Arithmetic_Works() {
    var a = ConfigurableFixedPoint<Int128>.FromDouble(5.0, Int128_F);
    var b = ConfigurableFixedPoint<Int128>.FromDouble(3.0, Int128_F);
    Assert.AreEqual(8.0, (a + b).ToDouble(), 0.000001);
    Assert.AreEqual(2.0, (a - b).ToDouble(), 0.000001);
    Assert.AreEqual(15.0, (a * b).ToDouble(), 0.001);
  }

  [Test]
  public void UInt96_NegativeValue_ClampsToZero() {
    Assert.AreEqual(0.0, ConfigurableFixedPoint<UInt96>.FromDouble(-1.0, UInt96_F).ToDouble(), Tolerance);
  }

  [Test]
  public void UInt128_NegativeValue_ClampsToZero() {
    Assert.AreEqual(0.0, ConfigurableFixedPoint<UInt128>.FromDouble(-1.0, UInt128_F).ToDouble(), Tolerance);
  }

  [Test]
  public void UInt96_Subtraction_Saturates() {
    var small = ConfigurableFixedPoint<UInt96>.FromDouble(1.0, UInt96_F);
    var large = ConfigurableFixedPoint<UInt96>.FromDouble(2.0, UInt96_F);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UInt128_Subtraction_Saturates() {
    var small = ConfigurableFixedPoint<UInt128>.FromDouble(1.0, UInt128_F);
    var large = ConfigurableFixedPoint<UInt128>.FromDouble(2.0, UInt128_F);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void TotalBits_96_128() {
    Assert.AreEqual(96, ConfigurableFixedPoint<Int96>.TotalBits);
    Assert.AreEqual(96, ConfigurableFixedPoint<UInt96>.TotalBits);
    Assert.AreEqual(128, ConfigurableFixedPoint<Int128>.TotalBits);
    Assert.AreEqual(128, ConfigurableFixedPoint<UInt128>.TotalBits);
  }

  #endregion

  #region All Explicit Conversions (Fixed-Point)

  [Test]
  public void ExplicitConversion_ToByte_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(42.7, Int_F);
    Assert.AreEqual((byte)42, (byte)fp);
  }

  [Test]
  public void ExplicitConversion_ToSByte_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(-42.0, Int_F);
    Assert.AreEqual((sbyte)-42, (sbyte)fp);
  }

  [Test]
  public void ExplicitConversion_ToShort_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1000.7, Int_F);
    Assert.AreEqual((short)1000, (short)fp);
  }

  [Test]
  public void ExplicitConversion_ToUShort_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1000.7, Int_F);
    Assert.AreEqual((ushort)1000, (ushort)fp);
  }

  [Test]
  public void ExplicitConversion_ToUInt_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(42.7, Int_F);
    Assert.AreEqual(42u, (uint)fp);
  }

  [Test]
  public void ExplicitConversion_ToLong_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(100.7, Int_F);
    Assert.AreEqual(100L, (long)fp);
  }

  [Test]
  public void ExplicitConversion_ToULong_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(100.7, Int_F);
    Assert.AreEqual(100UL, (ulong)fp);
  }

  [Test]
  public void ExplicitConversion_ToHalf_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var half = (Half)fp;
    Assert.AreEqual(3.14, (double)half, 0.01);
  }

  [Test]
  public void ExplicitConversion_ToQuarter_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(1.5, Int_F);
    var quarter = (Quarter)fp;
    Assert.AreEqual(1.5, quarter.ToSingle(), 0.5);
  }

  [Test]
  public void ExplicitConversion_ToE4M3_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    var e4m3 = (E4M3)fp;
    Assert.AreEqual(2.0, e4m3.ToSingle(), 0.5);
  }

  [Test]
  public void ExplicitConversion_ToBFloat8_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    var bf8 = (BFloat8)fp;
    Assert.AreEqual(2.0, bf8.ToSingle(), 0.5);
  }

  [Test]
  public void ExplicitConversion_ToBFloat16_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    var bf16 = (BFloat16)fp;
    Assert.AreEqual(3.14, bf16.ToSingle(), 0.1);
  }

  [Test]
  public void ExplicitConversion_ToBFloat32_Works() {
    var fp = ConfigurableFixedPoint<long>.FromDouble(3.14, Long_F);
    var bf32 = (BFloat32)fp;
    Assert.AreEqual(3.14, bf32.ToSingle(), 0.001);
  }

  [Test]
  public void ExplicitConversion_ToBFloat64_Works() {
    var fp = ConfigurableFixedPoint<long>.FromDouble(3.14, Long_F);
    var bf64 = (BFloat64)fp;
    Assert.AreEqual(3.14, bf64.ToDouble(), 0.001);
  }

  #endregion

  #region All From* Factory Methods (Fixed-Point)

  [Test]
  public void FromSingle_RoundTrips() {
    var fp = ConfigurableFixedPoint<int>.FromSingle(1.5f, Int_F);
    Assert.AreEqual(1.5f, fp.ToSingle(), 0.001f);
  }

  [Test]
  public void FromHalf_RoundTrips() {
    var half = (Half)3.14;
    var fp = ConfigurableFixedPoint<int>.FromHalf(half, Int_F);
    Assert.AreEqual((double)half, fp.ToDouble(), 0.01);
  }

  [Test]
  public void FromE4M3_RoundTrips() {
    var e4m3 = E4M3.FromSingle(2.0f);
    var fp = ConfigurableFixedPoint<int>.FromE4M3(e4m3, Int_F);
    Assert.AreEqual(2.0, fp.ToDouble(), 0.5);
  }

  [Test]
  public void FromBFloat8_RoundTrips() {
    var bf8 = BFloat8.FromSingle(2.0f);
    var fp = ConfigurableFixedPoint<int>.FromBFloat8(bf8, Int_F);
    Assert.AreEqual(2.0, fp.ToDouble(), 0.5);
  }

  [Test]
  public void FromBFloat32_RoundTrips() {
    var bf32 = BFloat32.FromSingle(3.14f);
    var fp = ConfigurableFixedPoint<long>.FromBFloat32(bf32, Long_F);
    Assert.AreEqual(3.14, fp.ToDouble(), 0.01);
  }

  [Test]
  public void FromBFloat64_RoundTrips() {
    var bf64 = BFloat64.FromDouble(3.14);
    var fp = ConfigurableFixedPoint<long>.FromBFloat64(bf64, Long_F);
    Assert.AreEqual(3.14, fp.ToDouble(), 0.01);
  }

  [Test]
  public void FromDecimal_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDecimal(3.14m, Int_F);
    Assert.AreEqual(3.14, fp.ToDouble(), 0.001);
  }

  [Test]
  public void FromUInt32_Works() {
    var fp = ConfigurableFixedPoint<uint>.FromUInt32(42u, UInt_F);
    Assert.AreEqual(42.0, fp.ToDouble(), Tolerance);
  }

  [Test]
  public void FromUInt64_Works() {
    var fp = ConfigurableFixedPoint<ulong>.FromUInt64(100UL, ULong_F);
    Assert.AreEqual(100.0, fp.ToDouble(), Tolerance);
  }

  [Test]
  public void FromBigInteger_Works() {
    var fp = ConfigurableFixedPoint<int>.FromBigInteger(new BigInteger(42), Int_F);
    Assert.AreEqual(42.0, fp.ToDouble(), Tolerance);
  }

  #endregion

  #region All To* Instance Methods (Fixed-Point)

  [Test]
  public void ToSingle_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    Assert.AreEqual(3.14f, fp.ToSingle(), 0.01f);
  }

  [Test]
  public void ToE4M3_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.AreEqual(2.0, fp.ToE4M3().ToSingle(), 0.5);
  }

  [Test]
  public void ToBFloat8_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.AreEqual(2.0, fp.ToBFloat8().ToSingle(), 0.5);
  }

  [Test]
  public void ToBFloat16_Works() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    Assert.AreEqual(3.14, fp.ToBFloat16().ToSingle(), 0.1);
  }

  [Test]
  public void ToBFloat32_Works() {
    var fp = ConfigurableFixedPoint<long>.FromDouble(3.14, Long_F);
    Assert.AreEqual(3.14, fp.ToBFloat32().ToSingle(), 0.001);
  }

  [Test]
  public void ToBFloat64_Works() {
    var fp = ConfigurableFixedPoint<long>.FromDouble(3.14, Long_F);
    Assert.AreEqual(3.14, fp.ToBFloat64().ToDouble(), 0.001);
  }

  [Test]
  public void ToUInt32_Works() {
    var fp = ConfigurableFixedPoint<uint>.FromDouble(42.7, UInt_F);
    Assert.AreEqual(42u, fp.ToUInt32());
  }

  [Test]
  public void ToUInt64_Works() {
    var fp = ConfigurableFixedPoint<ulong>.FromDouble(100.5, ULong_F);
    Assert.AreEqual(100UL, fp.ToUInt64());
  }

  #endregion

  #region Primitive-on-Left-Side Operators (Fixed-Point)

  [Test]
  public void DoublePrimitive_LeftSide_Arithmetic() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    Assert.AreEqual(7.0, (2.0 + fp).ToDouble(), Tolerance);
    Assert.AreEqual(-3.0, (2.0 - fp).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (2.0 * fp).ToDouble(), Tolerance);
    Assert.AreEqual(0.4, (2.0 / fp).ToDouble(), Tolerance);
  }

  [Test]
  public void IntPrimitive_LeftSide_Arithmetic() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    Assert.AreEqual(7.0, (2 + fp).ToDouble(), Tolerance);
    Assert.AreEqual(-3.0, (2 - fp).ToDouble(), Tolerance);
    Assert.AreEqual(10.0, (2 * fp).ToDouble(), Tolerance);
  }

  #endregion

  #region Unsigned Saturation Across All Unsigned Types (Fixed-Point)

  [Test]
  public void UnsignedShort_Subtraction_Saturates() {
    var small = ConfigurableFixedPoint<ushort>.FromDouble(1.0, UShort_F);
    var large = ConfigurableFixedPoint<ushort>.FromDouble(2.0, UShort_F);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedInt_Subtraction_Saturates() {
    var small = ConfigurableFixedPoint<uint>.FromDouble(1.0, UInt_F);
    var large = ConfigurableFixedPoint<uint>.FromDouble(2.0, UInt_F);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedLong_Subtraction_Saturates() {
    var small = ConfigurableFixedPoint<ulong>.FromDouble(1.0, ULong_F);
    var large = ConfigurableFixedPoint<ulong>.FromDouble(2.0, ULong_F);
    Assert.AreEqual(0.0, (small - large).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedInt_NegativeValue_ClampsToZero() {
    Assert.AreEqual(0.0, ConfigurableFixedPoint<uint>.FromDouble(-5.0, UInt_F).ToDouble(), Tolerance);
  }

  [Test]
  public void UnsignedLong_NegativeValue_ClampsToZero() {
    Assert.AreEqual(0.0, ConfigurableFixedPoint<ulong>.FromDouble(-5.0, ULong_F).ToDouble(), Tolerance);
  }

  #endregion

  #region Cross-Type Edge Cases (Fixed + NaN/Infinity Floating)

  [Test]
  public void CrossType_Fixed_Plus_NaN_Throws() {
    var fixedPt = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var nan = ConfigurableFloatingPoint<int>.NaN;
    Assert.Throws<ArgumentException>(() => { var _ = fixedPt + nan; });
  }

  #endregion

  #region Unary Plus Operator

  [Test]
  public void UnaryPlus_PreservesValue() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    Assert.AreEqual(5.0, (+fp).ToDouble(), Tolerance);
  }

  [Test]
  public void UnaryPlus_PreservesNegative() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(-5.0, Int_F);
    Assert.AreEqual(-5.0, (+fp).ToDouble(), Tolerance);
  }

  #endregion

  #region CompareTo(object)

  [Test]
  public void CompareTo_Object_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.IsTrue(a.CompareTo((object)b) < 0);
    Assert.IsTrue(b.CompareTo((object)a) > 0);
    Assert.AreEqual(0, a.CompareTo((object)a));
  }

  #endregion

  #region Increment / Decrement

  [Test]
  public void Increment_AddsOneLsb() {
    var value = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var before = value.ToDouble();
    ++value;
    var epsilon = ConfigurableFixedPoint<int>.Epsilon(Int_F).ToDouble();
    Assert.AreEqual(before + epsilon, value.ToDouble(), epsilon * 0.1);
  }

  [Test]
  public void Decrement_SubtractsOneLsb() {
    var value = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var before = value.ToDouble();
    --value;
    var epsilon = ConfigurableFixedPoint<int>.Epsilon(Int_F).ToDouble();
    Assert.AreEqual(before - epsilon, value.ToDouble(), epsilon * 0.1);
  }

  #endregion

  #region Modulo (int storage)

  [Test]
  public void Modulo_Int_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(10.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(3.0, Int_F);
    Assert.AreEqual(1.0, (a % b).ToDouble(), 0.01);
  }

  #endregion

  #region AsXxx Remaining Properties

  [Test]
  public void AsEpsilon_PreservesConfig() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var eps = fp.AsEpsilon;
    Assert.IsTrue(eps.ToDouble() > 0);
    Assert.AreEqual(Int_F, eps.FractionalBits);
  }

  [Test]
  public void AsMaxValue_PreservesConfig() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var max = fp.AsMaxValue;
    Assert.IsTrue(max.ToDouble() > 0);
    Assert.AreEqual(Int_F, max.FractionalBits);
  }

  [Test]
  public void AsMinValue_PreservesConfig() {
    var fp = ConfigurableFixedPoint<int>.FromDouble(5.0, Int_F);
    var min = fp.AsMinValue;
    Assert.IsTrue(min.ToDouble() < 0);
    Assert.AreEqual(Int_F, min.FractionalBits);
  }

  #endregion

  #region TryFormat

  [Test]
  public void TryFormat_Works() {
    var value = ConfigurableFixedPoint<int>.FromDouble(3.14, Int_F);
    Span<char> buffer = stackalloc char[32];
    Assert.IsTrue(value.TryFormat(buffer, out var written, "F2", CultureInfo.InvariantCulture));
    Assert.AreEqual("3.14", buffer[..written].ToString());
  }

  #endregion

  #region GreaterThan and LessOrEqual/GreaterOrEqual

  [Test]
  public void Comparison_GreaterThan_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    Assert.IsTrue(a > b);
    Assert.IsFalse(b > a);
  }

  [Test]
  public void Comparison_LessOrEqual_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    var c = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    Assert.IsTrue(a <= b);
    Assert.IsTrue(a <= c);
    Assert.IsFalse(b <= a);
  }

  [Test]
  public void Comparison_GreaterOrEqual_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var c = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.IsTrue(a >= b);
    Assert.IsTrue(a >= c);
    Assert.IsFalse(b >= a);
  }

  [Test]
  public void Comparison_Inequality_Works() {
    var a = ConfigurableFixedPoint<int>.FromDouble(1.0, Int_F);
    var b = ConfigurableFixedPoint<int>.FromDouble(2.0, Int_F);
    Assert.IsTrue(a != b);
    Assert.IsFalse(a != a);
  }

  #endregion

  #region Epsilon

  [Test]
  public void Epsilon_IsSmallPositive() {
    var eps = ConfigurableFixedPoint<int>.Epsilon(Int_F);
    Assert.IsTrue(eps.ToDouble() > 0);
    var zero = ConfigurableFixedPoint<int>.Zero(Int_F);
    Assert.IsTrue(eps > zero);
  }

  #endregion

}
