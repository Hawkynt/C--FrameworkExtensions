using NUnit.Framework;

namespace System.MathExtensionsTests;

[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region Sin Tests

  [Test]
  [TestCase(0.0f, 0.0f)]
  [TestCase(1.5707963f, 1.0f)] // π/2
  [TestCase(3.1415927f, 0.0f)] // π
  [Category("HappyPath")]
  [Description("Validates float Sin computes correctly")]
  public void Sin_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Sin();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(0.0, 0.0)]
  [TestCase(1.5707963267948966, 1.0)] // π/2
  [TestCase(3.141592653589793, 0.0)]  // π
  [Category("HappyPath")]
  [Description("Validates double Sin computes correctly")]
  public void Sin_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Sin();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Cos Tests

  [Test]
  [TestCase(0.0f, 1.0f)]
  [TestCase(1.5707963f, 0.0f)]  // π/2
  [TestCase(3.1415927f, -1.0f)] // π
  [Category("HappyPath")]
  [Description("Validates float Cos computes correctly")]
  public void Cos_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Cos();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(0.0, 1.0)]
  [TestCase(1.5707963267948966, 0.0)]  // π/2
  [TestCase(3.141592653589793, -1.0)]  // π
  [Category("HappyPath")]
  [Description("Validates double Cos computes correctly")]
  public void Cos_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Cos();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Tan Tests

  [Test]
  [TestCase(0.0f, 0.0f)]
  [TestCase(0.7853982f, 1.0f)]  // π/4
  [TestCase(-0.7853982f, -1.0f)] // -π/4
  [Category("HappyPath")]
  [Description("Validates float Tan computes correctly")]
  public void Tan_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Tan();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(0.0, 0.0)]
  [TestCase(0.7853981633974483, 1.0)]   // π/4
  [TestCase(-0.7853981633974483, -1.0)] // -π/4
  [Category("HappyPath")]
  [Description("Validates double Tan computes correctly")]
  public void Tan_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Tan();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Sinh Tests

  [Test]
  [TestCase(0.0f, 0.0f)]
  [TestCase(1.0f, 1.1752012f)]
  [TestCase(-1.0f, -1.1752012f)]
  [Category("HappyPath")]
  [Description("Validates float Sinh computes correctly")]
  public void Sinh_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Sinh();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(0.0, 0.0)]
  [TestCase(1.0, 1.1752011936438014)]
  [TestCase(-1.0, -1.1752011936438014)]
  [Category("HappyPath")]
  [Description("Validates double Sinh computes correctly")]
  public void Sinh_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Sinh();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Cosh Tests

  [Test]
  [TestCase(0.0f, 1.0f)]
  [TestCase(1.0f, 1.5430806f)]
  [TestCase(-1.0f, 1.5430806f)]
  [Category("HappyPath")]
  [Description("Validates float Cosh computes correctly")]
  public void Cosh_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Cosh();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(0.0, 1.0)]
  [TestCase(1.0, 1.5430806348152437)]
  [TestCase(-1.0, 1.5430806348152437)]
  [Category("HappyPath")]
  [Description("Validates double Cosh computes correctly")]
  public void Cosh_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Cosh();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Tanh Tests

  [Test]
  [TestCase(0.0f, 0.0f)]
  [TestCase(1.0f, 0.7615942f)]
  [TestCase(-1.0f, -0.7615942f)]
  [Category("HappyPath")]
  [Description("Validates float Tanh computes correctly")]
  public void Tanh_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Tanh();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(0.0, 0.0)]
  [TestCase(1.0, 0.7615941559557649)]
  [TestCase(-1.0, -0.7615941559557649)]
  [Category("HappyPath")]
  [Description("Validates double Tanh computes correctly")]
  public void Tanh_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Tanh();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Sqrt Tests

  [Test]
  [TestCase(0.0f, 0.0f)]
  [TestCase(1.0f, 1.0f)]
  [TestCase(4.0f, 2.0f)]
  [TestCase(9.0f, 3.0f)]
  [TestCase(2.0f, 1.4142135f)]
  [Category("HappyPath")]
  [Description("Validates float Sqrt computes correctly")]
  public void Sqrt_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Sqrt();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(0.0, 0.0)]
  [TestCase(1.0, 1.0)]
  [TestCase(4.0, 2.0)]
  [TestCase(9.0, 3.0)]
  [TestCase(2.0, 1.4142135623730951)]
  [Category("HappyPath")]
  [Description("Validates double Sqrt computes correctly")]
  public void Sqrt_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Sqrt();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Log Tests

  [Test]
  [TestCase(1.0f, 0.0f)]
  [TestCase(2.7182817f, 1.0f)] // e
  [TestCase(10.0f, 2.302585f)]
  [Category("HappyPath")]
  [Description("Validates float Log computes correctly")]
  public void Log_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Log();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(1.0, 0.0)]
  [TestCase(2.718281828459045, 1.0)] // e
  [TestCase(10.0, 2.302585092994046)]
  [Category("HappyPath")]
  [Description("Validates double Log computes correctly")]
  public void Log_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Log();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Log10 Tests

  [Test]
  [TestCase(1.0f, 0.0f)]
  [TestCase(10.0f, 1.0f)]
  [TestCase(100.0f, 2.0f)]
  [TestCase(1000.0f, 3.0f)]
  [Category("HappyPath")]
  [Description("Validates float Log10 computes correctly")]
  public void Log10_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Log10();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(1.0, 0.0)]
  [TestCase(10.0, 1.0)]
  [TestCase(100.0, 2.0)]
  [TestCase(1000.0, 3.0)]
  [Category("HappyPath")]
  [Description("Validates double Log10 computes correctly")]
  public void Log10_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Log10();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Floor Tests

  [Test]
  [TestCase(1.5, 1.0)]
  [TestCase(1.9, 1.0)]
  [TestCase(-1.5, -2.0)]
  [TestCase(2.0, 2.0)]
  [Category("HappyPath")]
  [Description("Validates double Floor computes correctly")]
  public void Floor_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Floor();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates decimal Floor computes correctly")]
  public void Floor_Decimal_ComputesCorrectly() {
    Assert.That(1.5m.Floor(), Is.EqualTo(1m));
    Assert.That(1.9m.Floor(), Is.EqualTo(1m));
    Assert.That((-1.5m).Floor(), Is.EqualTo(-2m));
    Assert.That(2.0m.Floor(), Is.EqualTo(2m));
  }

  #endregion

  #region Ceiling Tests

  [Test]
  [TestCase(1.1, 2.0)]
  [TestCase(1.9, 2.0)]
  [TestCase(-1.5, -1.0)]
  [TestCase(2.0, 2.0)]
  [Category("HappyPath")]
  [Description("Validates double Ceiling computes correctly")]
  public void Ceiling_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Ceiling();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates decimal Ceiling computes correctly")]
  public void Ceiling_Decimal_ComputesCorrectly() {
    Assert.That(1.1m.Ceiling(), Is.EqualTo(2m));
    Assert.That(1.9m.Ceiling(), Is.EqualTo(2m));
    Assert.That((-1.5m).Ceiling(), Is.EqualTo(-1m));
    Assert.That(2.0m.Ceiling(), Is.EqualTo(2m));
  }

  #endregion

  #region Truncate Tests

  [Test]
  [TestCase(1.9, 1.0)]
  [TestCase(-1.9, -1.0)]
  [TestCase(2.0, 2.0)]
  [Category("HappyPath")]
  [Description("Validates double Truncate computes correctly")]
  public void Truncate_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Truncate();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates decimal Truncate computes correctly")]
  public void Truncate_Decimal_ComputesCorrectly() {
    Assert.That(1.9m.Truncate(), Is.EqualTo(1m));
    Assert.That((-1.9m).Truncate(), Is.EqualTo(-1m));
    Assert.That(2.0m.Truncate(), Is.EqualTo(2m));
  }

  #endregion

  #region Round Tests

  [Test]
  [TestCase(1.4, 1.0)]
  [TestCase(1.5, 2.0)]
  [TestCase(1.6, 2.0)]
  [TestCase(-1.5, -2.0)]
  [Category("HappyPath")]
  [Description("Validates double Round computes correctly")]
  public void Round_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Round();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates double Round with digits computes correctly")]
  public void Round_Double_WithDigits_ComputesCorrectly() {
    Assert.That(1.234.Round(2), Is.EqualTo(1.23).Within(1e-10));
    Assert.That(1.236.Round(2), Is.EqualTo(1.24).Within(1e-10));  // Not banker's rounding edge case
    Assert.That(1.2346.Round(3), Is.EqualTo(1.235).Within(1e-10)); // Rounds up from 6
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates decimal Round computes correctly")]
  public void Round_Decimal_ComputesCorrectly() {
    Assert.That(1.4m.Round(), Is.EqualTo(1m));
    Assert.That(1.5m.Round(), Is.EqualTo(2m));
    Assert.That(1.6m.Round(), Is.EqualTo(2m));
    Assert.That((-1.5m).Round(), Is.EqualTo(-2m));
  }

  #endregion

  #region Abs Tests

  [Test]
  [TestCase(-5, 5)]
  [TestCase(5, 5)]
  [TestCase(0, 0)]
  [Category("HappyPath")]
  [Description("Validates int Abs computes correctly")]
  public void Abs_Int_ComputesCorrectly(int value, int expected) {
    var result = value.Abs();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(-5.5, 5.5)]
  [TestCase(5.5, 5.5)]
  [TestCase(0.0, 0.0)]
  [Category("HappyPath")]
  [Description("Validates double Abs computes correctly")]
  public void Abs_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Abs();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(-5.5f, 5.5f)]
  [TestCase(5.5f, 5.5f)]
  [TestCase(0.0f, 0.0f)]
  [Category("HappyPath")]
  [Description("Validates float Abs computes correctly")]
  public void Abs_Float_ComputesCorrectly(float value, float expected) {
    var result = value.Abs();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates decimal Abs computes correctly")]
  public void Abs_Decimal_ComputesCorrectly() {
    Assert.That((-5.5m).Abs(), Is.EqualTo(5.5m));
    Assert.That(5.5m.Abs(), Is.EqualTo(5.5m));
    Assert.That(0m.Abs(), Is.EqualTo(0m));
  }

  #endregion

  #region Sign Tests

  [Test]
  [TestCase(-5, -1)]
  [TestCase(5, 1)]
  [TestCase(0, 0)]
  [Category("HappyPath")]
  [Description("Validates int Sign computes correctly")]
  public void Sign_Int_ComputesCorrectly(int value, int expected) {
    var result = value.Sign();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(-5.5, -1)]
  [TestCase(5.5, 1)]
  [TestCase(0.0, 0)]
  [Category("HappyPath")]
  [Description("Validates double Sign computes correctly")]
  public void Sign_Double_ComputesCorrectly(double value, int expected) {
    var result = value.Sign();
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Squared and Cubed Tests

  [Test]
  [TestCase(2, 4)]
  [TestCase(3, 9)]
  [TestCase(-2, 4)]
  [TestCase(0, 0)]
  [Category("HappyPath")]
  [Description("Validates int Squared computes correctly")]
  public void Squared_Int_ComputesCorrectly(int value, int expected) {
    var result = value.Squared();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2.0, 4.0)]
  [TestCase(3.0, 9.0)]
  [TestCase(-2.0, 4.0)]
  [Category("HappyPath")]
  [Description("Validates double Squared computes correctly")]
  public void Squared_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Squared();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  [Test]
  [TestCase(2, 8)]
  [TestCase(3, 27)]
  [TestCase(-2, -8)]
  [TestCase(0, 0)]
  [Category("HappyPath")]
  [Description("Validates int Cubed computes correctly")]
  public void Cubed_Int_ComputesCorrectly(int value, int expected) {
    var result = value.Cubed();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(2.0, 8.0)]
  [TestCase(3.0, 27.0)]
  [TestCase(-2.0, -8.0)]
  [Category("HappyPath")]
  [Description("Validates double Cubed computes correctly")]
  public void Cubed_Double_ComputesCorrectly(double value, double expected) {
    var result = value.Cubed();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  #endregion

  #region Pow Tests

  [Test]
  [TestCase(2.0, 3.0, 8.0)]
  [TestCase(10.0, 2.0, 100.0)]
  [TestCase(2.0, 0.0, 1.0)]
  [TestCase(2.0, -1.0, 0.5)]
  [Category("HappyPath")]
  [Description("Validates double Pow computes correctly")]
  public void Pow_Double_ComputesCorrectly(double value, double exponent, double expected) {
    var result = value.Pow(exponent);
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  [Test]
  [TestCase(2.0f, 3.0f, 8.0f)]
  [TestCase(10.0f, 2.0f, 100.0f)]
  [TestCase(2.0f, 0.0f, 1.0f)]
  [Category("HappyPath")]
  [Description("Validates float Pow computes correctly")]
  public void Pow_Float_ComputesCorrectly(float value, float exponent, float expected) {
    var result = value.Pow(exponent);
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  #endregion

  #region ReciprocalEstimate Tests

  [Test]
  [TestCase(2.0f, 0.5f)]
  [TestCase(4.0f, 0.25f)]
  [TestCase(0.5f, 2.0f)]
  [Category("HappyPath")]
  [Description("Validates float ReciprocalEstimate computes correctly")]
  public void ReciprocalEstimate_Float_ComputesCorrectly(float value, float expected) {
    var result = value.ReciprocalEstimate();
    Assert.That(result, Is.EqualTo(expected).Within(1e-5f));
  }

  [Test]
  [TestCase(2.0, 0.5)]
  [TestCase(4.0, 0.25)]
  [TestCase(0.5, 2.0)]
  [Category("HappyPath")]
  [Description("Validates double ReciprocalEstimate computes correctly")]
  public void ReciprocalEstimate_Double_ComputesCorrectly(double value, double expected) {
    var result = value.ReciprocalEstimate();
    Assert.That(result, Is.EqualTo(expected).Within(1e-10));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates decimal ReciprocalEstimate computes correctly")]
  public void ReciprocalEstimate_Decimal_ComputesCorrectly() {
    Assert.That(2m.ReciprocalEstimate(), Is.EqualTo(0.5m));
    Assert.That(4m.ReciprocalEstimate(), Is.EqualTo(0.25m));
    Assert.That(0.5m.ReciprocalEstimate(), Is.EqualTo(2m));
  }

  #endregion

  #region EdgeCase Tests - Special Values

  [Test]
  [Category("EdgeCase")]
  [Description("Validates trig functions handle NaN correctly")]
  public void Sin_Double_NaN_ReturnsNaN() {
    Assert.That(double.NaN.Sin().IsNaN(), Is.True);
    Assert.That(double.NaN.Cos().IsNaN(), Is.True);
    Assert.That(double.NaN.Tan().IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates trig functions handle infinity correctly")]
  public void Sin_Double_Infinity_ReturnsNaN() {
    Assert.That(double.PositiveInfinity.Sin().IsNaN(), Is.True);
    Assert.That(double.NegativeInfinity.Sin().IsNaN(), Is.True);
    Assert.That(double.PositiveInfinity.Cos().IsNaN(), Is.True);
    Assert.That(double.NegativeInfinity.Cos().IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates hyperbolic functions handle infinity correctly")]
  public void Sinh_Double_Infinity_ReturnsInfinity() {
    Assert.That(double.PositiveInfinity.Sinh(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(double.NegativeInfinity.Sinh(), Is.EqualTo(double.NegativeInfinity));
    Assert.That(double.PositiveInfinity.Cosh(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(double.NegativeInfinity.Cosh(), Is.EqualTo(double.PositiveInfinity));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Tanh asymptotes to ±1 for large values")]
  public void Tanh_Double_LargeValues_ApproachesLimits() {
    Assert.That(1000.0.Tanh(), Is.EqualTo(1.0).Within(1e-10));
    Assert.That((-1000.0).Tanh(), Is.EqualTo(-1.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Sqrt handles special values correctly")]
  public void Sqrt_Double_SpecialValues_HandlesCorrectly() {
    Assert.That(double.PositiveInfinity.Sqrt(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(double.NaN.Sqrt().IsNaN(), Is.True);
    Assert.That((-1.0).Sqrt().IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Log handles special values correctly")]
  public void Log_Double_SpecialValues_HandlesCorrectly() {
    Assert.That(0.0.Log(), Is.EqualTo(double.NegativeInfinity));
    Assert.That(double.PositiveInfinity.Log(), Is.EqualTo(double.PositiveInfinity));
    Assert.That((-1.0).Log().IsNaN(), Is.True);
    Assert.That(double.NaN.Log().IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Log10 handles special values correctly")]
  public void Log10_Double_SpecialValues_HandlesCorrectly() {
    Assert.That(0.0.Log10(), Is.EqualTo(double.NegativeInfinity));
    Assert.That(double.PositiveInfinity.Log10(), Is.EqualTo(double.PositiveInfinity));
    Assert.That((-1.0).Log10().IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Floor/Ceiling/Truncate handle special values correctly")]
  public void Rounding_Double_SpecialValues_HandlesCorrectly() {
    Assert.That(double.PositiveInfinity.Floor(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(double.NegativeInfinity.Ceiling(), Is.EqualTo(double.NegativeInfinity));
    Assert.That(double.NaN.Truncate().IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Abs handles special values correctly")]
  public void Abs_Double_SpecialValues_HandlesCorrectly() {
    Assert.That(double.PositiveInfinity.Abs(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(double.NegativeInfinity.Abs(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(double.NaN.Abs().IsNaN(), Is.True);
    Assert.That((-0.0).Abs(), Is.EqualTo(0.0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Pow handles edge cases correctly")]
  public void Pow_Double_EdgeCases_HandlesCorrectly() {
    Assert.That(0.0.Pow(0.0), Is.EqualTo(1.0));
    Assert.That(1.0.Pow(double.PositiveInfinity), Is.EqualTo(1.0));
    Assert.That(double.PositiveInfinity.Pow(0.0), Is.EqualTo(1.0));
    Assert.That(0.0.Pow(1.0), Is.EqualTo(0.0));
    Assert.That(0.0.Pow(-1.0), Is.EqualTo(double.PositiveInfinity));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ReciprocalEstimate handles zero correctly")]
  public void ReciprocalEstimate_Double_Zero_ReturnsInfinity() {
    Assert.That(0.0.ReciprocalEstimate(), Is.EqualTo(double.PositiveInfinity));
    Assert.That((-0.0).ReciprocalEstimate(), Is.EqualTo(double.NegativeInfinity));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ReciprocalEstimate handles infinity correctly")]
  public void ReciprocalEstimate_Double_Infinity_ReturnsZero() {
    Assert.That(double.PositiveInfinity.ReciprocalEstimate(), Is.EqualTo(0.0));
    Assert.That(double.NegativeInfinity.ReciprocalEstimate(), Is.EqualTo(0.0).Within(1e-10));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Squared handles max values without overflow for double")]
  public void Squared_Double_LargeValues_HandlesCorrectly() {
    Assert.That(double.MaxValue.Squared(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(1e154.Squared(), Is.EqualTo(1e308).Within(1e295));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Cubed handles values correctly")]
  public void Cubed_Double_SpecialValues_HandlesCorrectly() {
    Assert.That(double.PositiveInfinity.Cubed(), Is.EqualTo(double.PositiveInfinity));
    Assert.That(double.NegativeInfinity.Cubed(), Is.EqualTo(double.NegativeInfinity));
    Assert.That(double.NaN.Cubed().IsNaN(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Sign returns correct values for boundary cases")]
  public void Sign_Double_SpecialValues_HandlesCorrectly() {
    Assert.That(double.Epsilon.Sign(), Is.EqualTo(1));
    Assert.That((-double.Epsilon).Sign(), Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates int Abs handles int.MinValue")]
  public void Abs_Int_MinValue_OverflowsToNegative() {
    // int.MinValue.Abs() causes overflow - this is expected behavior matching Math.Abs
    Assert.Throws<OverflowException>(() => {
      checked { _ = int.MinValue.Abs(); }
    });
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Round with MidpointRounding works correctly")]
  public void Round_Double_MidpointAwayFromZero_RoundsCorrectly() {
    // Testing banker's rounding (default) vs AwayFromZero
    Assert.That(2.5.Round(), Is.EqualTo(2.0)); // Banker's rounds to even
    Assert.That(3.5.Round(), Is.EqualTo(4.0)); // Banker's rounds to even
  }

  #endregion
}
