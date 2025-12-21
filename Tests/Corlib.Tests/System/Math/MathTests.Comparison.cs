using NUnit.Framework;

namespace System.MathExtensionsTests;

[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region IsZero Tests

  [Test]
  [TestCase(0, true)]
  [TestCase(1, false)]
  [TestCase(-1, false)]
  [Category("HappyPath")]
  [Description("Validates int IsZero returns correctly")]
  public void IsZero_Int_ReturnsCorrectly(int value, bool expected) {
    var result = value.IsZero();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(0.0, true)]
  [TestCase(1.0, false)]
  [TestCase(-1.0, false)]
  [Category("HappyPath")]
  [Description("Validates double IsZero returns correctly")]
  public void IsZero_Double_ReturnsCorrectly(double value, bool expected) {
    var result = value.IsZero();
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsNotZero Tests

  [Test]
  [TestCase(0, false)]
  [TestCase(1, true)]
  [TestCase(-1, true)]
  [Category("HappyPath")]
  [Description("Validates int IsNotZero returns correctly")]
  public void IsNotZero_Int_ReturnsCorrectly(int value, bool expected) {
    var result = value.IsNotZero();
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsPositive Tests

  [Test]
  [TestCase(1, true)]
  [TestCase(0, false)]
  [TestCase(-1, false)]
  [Category("HappyPath")]
  [Description("Validates int IsPositive returns correctly")]
  public void IsPositive_Int_ReturnsCorrectly(int value, bool expected) {
    var result = value.IsPositive();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(1.0, true)]
  [TestCase(0.0, false)]
  [TestCase(-1.0, false)]
  [Category("HappyPath")]
  [Description("Validates double IsPositive returns correctly")]
  public void IsPositive_Double_ReturnsCorrectly(double value, bool expected) {
    var result = value.IsPositive();
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsNegative Tests

  [Test]
  [TestCase(-1, true)]
  [TestCase(0, false)]
  [TestCase(1, false)]
  [Category("HappyPath")]
  [Description("Validates int IsNegative returns correctly")]
  public void IsNegative_Int_ReturnsCorrectly(int value, bool expected) {
    var result = value.IsNegative();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(-1.0, true)]
  [TestCase(0.0, false)]
  [TestCase(1.0, false)]
  [Category("HappyPath")]
  [Description("Validates double IsNegative returns correctly")]
  public void IsNegative_Double_ReturnsCorrectly(double value, bool expected) {
    var result = value.IsNegative();
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsOdd / IsEven Tests

  [Test]
  [TestCase(0, true)]
  [TestCase(1, false)]
  [TestCase(2, true)]
  [TestCase(-2, true)]
  [TestCase(-3, false)]
  [Category("HappyPath")]
  [Description("Validates int IsEven returns correctly")]
  public void IsEven_Int_ReturnsCorrectly(int value, bool expected) {
    var result = value.IsEven();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(0, false)]
  [TestCase(1, true)]
  [TestCase(2, false)]
  [TestCase(-1, true)]
  [TestCase(-2, false)]
  [Category("HappyPath")]
  [Description("Validates int IsOdd returns correctly")]
  public void IsOdd_Int_ReturnsCorrectly(int value, bool expected) {
    var result = value.IsOdd();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase((byte)0, true)]
  [TestCase((byte)1, false)]
  [TestCase((byte)2, true)]
  [TestCase((byte)255, false)]
  [Category("HappyPath")]
  [Description("Validates byte IsEven returns correctly")]
  public void IsEven_Byte_ReturnsCorrectly(byte value, bool expected) {
    var result = value.IsEven();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(0L, true)]
  [TestCase(1L, false)]
  [TestCase(2L, true)]
  [TestCase(-2L, true)]
  [Category("HappyPath")]
  [Description("Validates long IsEven returns correctly")]
  public void IsEven_Long_ReturnsCorrectly(long value, bool expected) {
    var result = value.IsEven();
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsBelow / IsBelowOrEqual Tests

  [Test]
  [TestCase(5, 10, true)]
  [TestCase(10, 10, false)]
  [TestCase(15, 10, false)]
  [Category("HappyPath")]
  [Description("Validates int IsBelow returns correctly")]
  public void IsBelow_Int_ReturnsCorrectly(int value, int other, bool expected) {
    var result = value.IsBelow(other);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(5, 10, true)]
  [TestCase(10, 10, true)]
  [TestCase(15, 10, false)]
  [Category("HappyPath")]
  [Description("Validates int IsBelowOrEqual returns correctly")]
  public void IsBelowOrEqual_Int_ReturnsCorrectly(int value, int other, bool expected) {
    var result = value.IsBelowOrEqual(other);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsAbove / IsAboveOrEqual Tests

  [Test]
  [TestCase(15, 10, true)]
  [TestCase(10, 10, false)]
  [TestCase(5, 10, false)]
  [Category("HappyPath")]
  [Description("Validates int IsAbove returns correctly")]
  public void IsAbove_Int_ReturnsCorrectly(int value, int other, bool expected) {
    var result = value.IsAbove(other);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(15, 10, true)]
  [TestCase(10, 10, true)]
  [TestCase(5, 10, false)]
  [Category("HappyPath")]
  [Description("Validates int IsAboveOrEqual returns correctly")]
  public void IsAboveOrEqual_Int_ReturnsCorrectly(int value, int other, bool expected) {
    var result = value.IsAboveOrEqual(other);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsBetween / IsInRange Tests

  [Test]
  [TestCase(5, 1, 10, true)]
  [TestCase(1, 1, 10, false)]  // Exclusive of min
  [TestCase(10, 1, 10, false)] // Exclusive of max
  [TestCase(0, 1, 10, false)]
  [TestCase(11, 1, 10, false)]
  [Category("HappyPath")]
  [Description("Validates int IsBetween (exclusive) returns correctly")]
  public void IsBetween_Int_ReturnsCorrectly(int value, int min, int max, bool expected) {
    var result = value.IsBetween(min, max);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(5, 1, 10, true)]
  [TestCase(1, 1, 10, true)]   // Inclusive of min
  [TestCase(10, 1, 10, true)]  // Inclusive of max
  [TestCase(0, 1, 10, false)]
  [TestCase(11, 1, 10, false)]
  [Category("HappyPath")]
  [Description("Validates int IsInRange (inclusive) returns correctly")]
  public void IsInRange_Int_ReturnsCorrectly(int value, int min, int max, bool expected) {
    var result = value.IsInRange(min, max);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region IsNaN / IsInfinity Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates float IsNaN returns correctly")]
  public void IsNaN_Float_ReturnsCorrectly() {
    Assert.That(float.NaN.IsNaN(), Is.True);
    Assert.That(1.0f.IsNaN(), Is.False);
    Assert.That(float.PositiveInfinity.IsNaN(), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double IsNaN returns correctly")]
  public void IsNaN_Double_ReturnsCorrectly() {
    Assert.That(double.NaN.IsNaN(), Is.True);
    Assert.That(1.0.IsNaN(), Is.False);
    Assert.That(double.PositiveInfinity.IsNaN(), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates float IsInfinity returns correctly")]
  public void IsInfinity_Float_ReturnsCorrectly() {
    Assert.That(float.PositiveInfinity.IsInfinity(), Is.True);
    Assert.That(float.NegativeInfinity.IsInfinity(), Is.True);
    Assert.That(1.0f.IsInfinity(), Is.False);
    Assert.That(float.NaN.IsInfinity(), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double IsInfinity returns correctly")]
  public void IsInfinity_Double_ReturnsCorrectly() {
    Assert.That(double.PositiveInfinity.IsInfinity(), Is.True);
    Assert.That(double.NegativeInfinity.IsInfinity(), Is.True);
    Assert.That(1.0.IsInfinity(), Is.False);
    Assert.That(double.NaN.IsInfinity(), Is.False);
  }

  #endregion

  #region IsNumeric / IsNonNumeric Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates float IsNumeric returns correctly")]
  public void IsNumeric_Float_ReturnsCorrectly() {
    Assert.That(1.0f.IsNumeric(), Is.True);
    Assert.That(0.0f.IsNumeric(), Is.True);
    Assert.That(float.NaN.IsNumeric(), Is.False);
    Assert.That(float.PositiveInfinity.IsNumeric(), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double IsNumeric returns correctly")]
  public void IsNumeric_Double_ReturnsCorrectly() {
    Assert.That(1.0.IsNumeric(), Is.True);
    Assert.That(0.0.IsNumeric(), Is.True);
    Assert.That(double.NaN.IsNumeric(), Is.False);
    Assert.That(double.PositiveInfinity.IsNumeric(), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates float IsNonNumeric returns correctly")]
  public void IsNonNumeric_Float_ReturnsCorrectly() {
    Assert.That(float.NaN.IsNonNumeric(), Is.True);
    Assert.That(float.PositiveInfinity.IsNonNumeric(), Is.True);
    Assert.That(1.0f.IsNonNumeric(), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double IsNonNumeric returns correctly")]
  public void IsNonNumeric_Double_ReturnsCorrectly() {
    Assert.That(double.NaN.IsNonNumeric(), Is.True);
    Assert.That(double.PositiveInfinity.IsNonNumeric(), Is.True);
    Assert.That(1.0.IsNonNumeric(), Is.False);
  }

  #endregion

  #region Min / Max Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates int Min with params returns correctly")]
  public void Min_Int_ParamsArray_ReturnsCorrectly() {
    Assert.That(MathEx.Min(5, 10), Is.EqualTo(5));
    Assert.That(MathEx.Min(10, 5), Is.EqualTo(5));
    Assert.That(MathEx.Min(5, 5), Is.EqualTo(5));
    Assert.That(MathEx.Min(5, 10, 3, 8), Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int Max with params returns correctly")]
  public void Max_Int_ParamsArray_ReturnsCorrectly() {
    Assert.That(MathEx.Max(5, 10), Is.EqualTo(10));
    Assert.That(MathEx.Max(10, 5), Is.EqualTo(10));
    Assert.That(MathEx.Max(5, 5), Is.EqualTo(5));
    Assert.That(MathEx.Max(5, 10, 3, 8), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double Min with params returns correctly")]
  public void Min_Double_ParamsArray_ReturnsCorrectly() {
    Assert.That(MathEx.Min(5.5, 10.5), Is.EqualTo(5.5));
    Assert.That(MathEx.Min(10.5, 5.5), Is.EqualTo(5.5));
    Assert.That(MathEx.Min(5.5, 10.5, 3.5, 8.5), Is.EqualTo(3.5));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double Max with params returns correctly")]
  public void Max_Double_ParamsArray_ReturnsCorrectly() {
    Assert.That(MathEx.Max(5.5, 10.5), Is.EqualTo(10.5));
    Assert.That(MathEx.Max(10.5, 5.5), Is.EqualTo(10.5));
    Assert.That(MathEx.Max(5.5, 10.5, 3.5, 8.5), Is.EqualTo(10.5));
  }

  #endregion

  #region Average Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates int Average with params returns correctly")]
  public void Average_Int_ParamsArray_ReturnsCorrectly() {
    Assert.That(MathEx.Average(10, 20), Is.EqualTo(15));
    Assert.That(MathEx.Average(0, 100), Is.EqualTo(50));
    Assert.That(MathEx.Average(10, 20, 30), Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double Average with params returns correctly")]
  public void Average_Double_ParamsArray_ReturnsCorrectly() {
    Assert.That(MathEx.Average(10.0, 20.0), Is.EqualTo(15.0).Within(1e-10));
    Assert.That(MathEx.Average(0.0, 100.0), Is.EqualTo(50.0).Within(1e-10));
    Assert.That(MathEx.Average(10.0, 20.0, 30.0), Is.EqualTo(20.0).Within(1e-10));
  }

  #endregion

  #region IsIn / IsNotIn Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates int IsIn with array returns correctly")]
  public void IsIn_Int_ArrayParameter_ReturnsCorrectly() {
    Assert.That(MathEx.IsIn(5, [1, 3, 5, 7, 9]), Is.True);
    Assert.That(MathEx.IsIn(4, [1, 3, 5, 7, 9]), Is.False);
    Assert.That(MathEx.IsIn(1, [1]), Is.True);
    Assert.That(MathEx.IsIn(2, [1]), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int IsNotIn with array returns correctly")]
  public void IsNotIn_Int_ArrayParameter_ReturnsCorrectly() {
    Assert.That(MathEx.IsNotIn(4, [1, 3, 5, 7, 9]), Is.True);
    Assert.That(MathEx.IsNotIn(5, [1, 3, 5, 7, 9]), Is.False);
  }

  #endregion

  #region EdgeCase Tests - Comparison Boundaries

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsZero handles negative zero correctly")]
  public void IsZero_Double_NegativeZero_ReturnsTrue() {
    Assert.That((-0.0).IsZero(), Is.True);
    Assert.That(0.0.IsZero(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsZero handles very small values correctly")]
  public void IsZero_Double_Epsilon_ReturnsFalse() {
    Assert.That(double.Epsilon.IsZero(), Is.False);
    Assert.That((-double.Epsilon).IsZero(), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsPositive/IsNegative handles NaN correctly")]
  public void IsPositiveNegative_Double_NaN_ReturnsFalse() {
    Assert.That(double.NaN.IsPositive(), Is.False);
    Assert.That(double.NaN.IsNegative(), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsPositive/IsNegative handles infinity correctly")]
  public void IsPositiveNegative_Double_Infinity_ReturnsCorrectly() {
    Assert.That(double.PositiveInfinity.IsPositive(), Is.True);
    Assert.That(double.PositiveInfinity.IsNegative(), Is.False);
    Assert.That(double.NegativeInfinity.IsPositive(), Is.False);
    Assert.That(double.NegativeInfinity.IsNegative(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsOdd/IsEven handles boundary values correctly")]
  public void IsOddEven_Int_BoundaryValues_ReturnsCorrectly() {
    Assert.That(int.MaxValue.IsOdd(), Is.True);  // 2147483647 is odd
    Assert.That(int.MinValue.IsEven(), Is.True); // -2147483648 is even
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsOdd/IsEven handles long boundary values correctly")]
  public void IsOddEven_Long_BoundaryValues_ReturnsCorrectly() {
    Assert.That(long.MaxValue.IsOdd(), Is.True);  // 9223372036854775807 is odd
    Assert.That(long.MinValue.IsEven(), Is.True); // -9223372036854775808 is even
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsBetween handles equal min/max correctly")]
  public void IsBetween_Int_EqualMinMax_ReturnsFalse() {
    Assert.That(5.IsBetween(5, 5), Is.False); // Exclusive on both ends
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsInRange handles equal min/max correctly")]
  public void IsInRange_Int_EqualMinMax_ReturnsTrue() {
    Assert.That(5.IsInRange(5, 5), Is.True); // Inclusive on both ends
    Assert.That(4.IsInRange(5, 5), Is.False);
    Assert.That(6.IsInRange(5, 5), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsBetween handles reversed range correctly")]
  public void IsBetween_Int_ReversedRange_ReturnsFalse() {
    // When max < min, nothing should be between
    Assert.That(5.IsBetween(10, 1), Is.False);
    Assert.That(0.IsBetween(10, 1), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Min/Max with single element returns that element")]
  public void MinMax_Int_SingleElement_ReturnsSame() {
    Assert.That(MathEx.Min(42), Is.EqualTo(42));
    Assert.That(MathEx.Max(42), Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Min/Max handles int boundary values correctly")]
  public void MinMax_Int_BoundaryValues_ReturnsCorrectly() {
    Assert.That(MathEx.Min(int.MaxValue, int.MinValue), Is.EqualTo(int.MinValue));
    Assert.That(MathEx.Max(int.MaxValue, int.MinValue), Is.EqualTo(int.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Average handles moderate values correctly")]
  public void Average_Long_ModerateValues_ComputesCorrectly() {
    var result = MathEx.Average(100L, 200L, 300L);
    Assert.That(result, Is.EqualTo(200L));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsIn with empty array returns false")]
  public void IsIn_Int_EmptyArray_ReturnsFalse() {
    Assert.That(MathEx.IsIn(5, Array.Empty<int>()), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsNotIn with empty array returns true")]
  public void IsNotIn_Int_EmptyArray_ReturnsTrue() {
    Assert.That(MathEx.IsNotIn(5, Array.Empty<int>()), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsNaN returns false for regular numbers")]
  public void IsNaN_Float_RegularNumbers_ReturnsFalse() {
    Assert.That(0.0f.IsNaN(), Is.False);
    Assert.That(float.MaxValue.IsNaN(), Is.False);
    Assert.That(float.MinValue.IsNaN(), Is.False);
    Assert.That(float.Epsilon.IsNaN(), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsInfinity returns false for large finite numbers")]
  public void IsInfinity_Double_LargeFinite_ReturnsFalse() {
    Assert.That(double.MaxValue.IsInfinity(), Is.False);
    Assert.That(double.MinValue.IsInfinity(), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates IsNumeric returns false for all non-numeric values")]
  public void IsNumeric_Double_AllNonNumeric_ReturnsFalse() {
    Assert.That(double.NaN.IsNumeric(), Is.False);
    Assert.That(double.PositiveInfinity.IsNumeric(), Is.False);
    Assert.That(double.NegativeInfinity.IsNumeric(), Is.False);
  }

  #endregion

  #region EdgeCase Tests - Empty Arrays

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Average returns zero for empty params array")]
  public void Average_Int_EmptyParams_ReturnsZero() {
    Assert.That(MathEx.Average(Array.Empty<int>()), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Min returns zero for empty params array")]
  public void Min_Int_EmptyParams_ReturnsZero() {
    Assert.That(MathEx.Min(Array.Empty<int>()), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Max returns zero for empty params array")]
  public void Max_Int_EmptyParams_ReturnsZero() {
    Assert.That(MathEx.Max(Array.Empty<int>()), Is.EqualTo(0));
  }

  #endregion
}
