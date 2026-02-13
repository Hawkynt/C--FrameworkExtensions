#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Hawkynt.ColorProcessing.FrequencyDomain;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("FrequencyDomain")]
public class FrequencyDomainTests {

  #region Complex Tests

  [Test]
  [Category("HappyPath")]
  public void Complex_Addition_ProducesCorrectResult() {
    var a = new Complex(3f, 4f);
    var b = new Complex(1f, 2f);
    var c = a + b;
    Assert.That(c.Real, Is.EqualTo(4f).Within(0.001f));
    Assert.That(c.Imaginary, Is.EqualTo(6f).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Subtraction_ProducesCorrectResult() {
    var a = new Complex(5f, 3f);
    var b = new Complex(2f, 1f);
    var c = a - b;
    Assert.That(c.Real, Is.EqualTo(3f).Within(0.001f));
    Assert.That(c.Imaginary, Is.EqualTo(2f).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Multiplication_ProducesCorrectResult() {
    var a = new Complex(3f, 2f);
    var b = new Complex(1f, 4f);
    var c = a * b;
    Assert.That(c.Real, Is.EqualTo(-5f).Within(0.001f));
    Assert.That(c.Imaginary, Is.EqualTo(14f).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Magnitude_ProducesCorrectResult() {
    var c = new Complex(3f, 4f);
    Assert.That(c.Magnitude, Is.EqualTo(5f).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Phase_ProducesCorrectResult() {
    var c = new Complex(1f, 1f);
    Assert.That(c.Phase, Is.EqualTo((float)(Math.PI / 4)).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_Conjugate_ProducesCorrectResult() {
    var c = new Complex(3f, 4f);
    var conj = c.Conjugate;
    Assert.That(conj.Real, Is.EqualTo(3f).Within(0.001f));
    Assert.That(conj.Imaginary, Is.EqualTo(-4f).Within(0.001f));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_FromPolar_ProducesCorrectResult() {
    var c = Complex.FromPolar(5f, (float)(Math.PI / 2));
    Assert.That(c.Real, Is.EqualTo(0f).Within(0.01f));
    Assert.That(c.Imaginary, Is.EqualTo(5f).Within(0.01f));
  }

  [Test]
  [Category("HappyPath")]
  public void Complex_ScalarMultiplication_ProducesCorrectResult() {
    var c = new Complex(2f, 3f);
    var r = c * 2f;
    Assert.That(r.Real, Is.EqualTo(4f).Within(0.001f));
    Assert.That(r.Imaginary, Is.EqualTo(6f).Within(0.001f));
  }

  #endregion

  #region Fft1D Tests

  [Test]
  [Category("HappyPath")]
  public void Fft1D_DcSignal_ProducesDcComponent() {
    var data = new Complex[] { new(1, 0), new(1, 0), new(1, 0), new(1, 0) };
    Fft1D.Forward(data);
    Assert.That(data[0].Magnitude, Is.EqualTo(4f).Within(0.01f));
    Assert.That(data[1].Magnitude, Is.EqualTo(0f).Within(0.01f));
  }

  [Test]
  [Category("HappyPath")]
  public void Fft1D_ForwardInverseRoundTrip_PreservesData() {
    var original = new Complex[] { new(1, 0), new(2, 0), new(3, 0), new(4, 0) };
    var data = (Complex[])original.Clone();

    Fft1D.Forward(data);
    Fft1D.Inverse(data);

    for (var i = 0; i < original.Length; ++i) {
      Assert.That(data[i].Real, Is.EqualTo(original[i].Real).Within(0.01f));
      Assert.That(data[i].Imaginary, Is.EqualTo(original[i].Imaginary).Within(0.01f));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void Fft1D_SingleFrequency_ProducesExpectedSpectrum() {
    var n = 8;
    var data = new Complex[n];
    for (var i = 0; i < n; ++i)
      data[i] = new Complex((float)Math.Cos(2 * Math.PI * i / n), 0);

    Fft1D.Forward(data);
    Assert.That(data[1].Magnitude, Is.GreaterThan(3f));
  }

  [Test]
  [Category("HappyPath")]
  public void Fft1D_ParsevalTheorem_EnergyConserved() {
    var data = new Complex[] { new(1, 0), new(2, 0), new(3, 0), new(4, 0) };
    var timeEnergy = 0f;
    foreach (var c in data)
      timeEnergy += c.Magnitude * c.Magnitude;

    Fft1D.Forward(data);
    var freqEnergy = 0f;
    foreach (var c in data)
      freqEnergy += c.Magnitude * c.Magnitude;
    freqEnergy /= data.Length;

    Assert.That(freqEnergy, Is.EqualTo(timeEnergy).Within(0.1f));
  }

  #endregion

  #region Fft2D Tests

  [Test]
  [Category("HappyPath")]
  public void Fft2D_ForwardInverseRoundTrip_PreservesData() {
    var data = new Complex[4, 4];
    for (var r = 0; r < 4; ++r)
    for (var c = 0; c < 4; ++c)
      data[r, c] = new Complex(r * 4 + c + 1, 0);

    var original = (Complex[,])data.Clone();
    Fft2D.Forward(data);
    Fft2D.Inverse(data);

    for (var r = 0; r < 4; ++r)
    for (var c = 0; c < 4; ++c) {
      Assert.That(data[r, c].Real, Is.EqualTo(original[r, c].Real).Within(0.1f));
      Assert.That(data[r, c].Imaginary, Is.EqualTo(original[r, c].Imaginary).Within(0.1f));
    }
  }

  #endregion

  #region Dct1D Tests

  [Test]
  [Category("HappyPath")]
  public void Dct1D_ForwardInverseRoundTrip_PreservesData() {
    var original = new float[] { 1f, 2f, 3f, 4f };
    var data = (float[])original.Clone();

    Dct1D.Forward(data);
    Dct1D.Inverse(data);

    for (var i = 0; i < original.Length; ++i)
      Assert.That(data[i], Is.EqualTo(original[i]).Within(0.1f));
  }

  [Test]
  [Category("HappyPath")]
  public void Dct1D_DcSignal_ConcentratesEnergy() {
    var data = new float[] { 1f, 1f, 1f, 1f };
    Dct1D.Forward(data);
    Assert.That(Math.Abs(data[0]), Is.GreaterThan(1f));
    Assert.That(Math.Abs(data[1]), Is.LessThan(0.1f));
    Assert.That(Math.Abs(data[2]), Is.LessThan(0.1f));
    Assert.That(Math.Abs(data[3]), Is.LessThan(0.1f));
  }

  #endregion

  #region Dct2D Tests

  [Test]
  [Category("HappyPath")]
  public void Dct2D_ForwardInverseRoundTrip_PreservesData() {
    var data = new float[4, 4];
    for (var r = 0; r < 4; ++r)
    for (var c = 0; c < 4; ++c)
      data[r, c] = r * 4 + c + 1;

    var original = (float[,])data.Clone();
    Dct2D.Forward(data);
    Dct2D.Inverse(data);

    for (var r = 0; r < 4; ++r)
    for (var c = 0; c < 4; ++c)
      Assert.That(data[r, c], Is.EqualTo(original[r, c]).Within(0.5f));
  }

  #endregion

  #region Bitmap Extension Tests

  [Test]
  [Category("HappyPath")]
  public void ToFrequencyDomain_SolidBitmap_DcComponentDominates() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.White);
    var spectrum = source.ToFrequencyDomain();

    Assert.That(spectrum[0, 0].Magnitude, Is.GreaterThan(1f));
  }

  [Test]
  [Category("HappyPath")]
  public void FromFrequencyDomain_RoundTrip_PreservesApproximateValues() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.FromArgb(128, 128, 128));
    var spectrum = source.ToFrequencyDomain();
    using var result = BitmapFrequencyDomainExtensions.FromFrequencyDomain(spectrum, 8, 8);

    using var locker = result.Lock();
    var c = locker[4, 4];
    Assert.That(c.R, Is.EqualTo(128).Within(30));
  }

  [Test]
  [Category("HappyPath")]
  public void GetMagnitudeSpectrum_ProducesValidBitmap() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.White);
    using var spectrum = source.GetMagnitudeSpectrum();

    Assert.That(spectrum.Width, Is.GreaterThan(0));
    Assert.That(spectrum.Height, Is.GreaterThan(0));
    Assert.That(spectrum.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void GetPhaseSpectrum_ProducesValidBitmap() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.White);
    using var spectrum = source.GetPhaseSpectrum();

    Assert.That(spectrum.Width, Is.GreaterThan(0));
    Assert.That(spectrum.Height, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ToDctDomain_FromDctDomain_RoundTrip_PreservesApproximateValues() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.FromArgb(200, 200, 200));
    var coefficients = source.ToDctDomain();
    using var result = BitmapFrequencyDomainExtensions.FromDctDomain(coefficients, 8, 8);

    using var locker = result.Lock();
    var c = locker[4, 4];
    Assert.That(c.R, Is.EqualTo(200).Within(30));
  }

  #endregion
}
