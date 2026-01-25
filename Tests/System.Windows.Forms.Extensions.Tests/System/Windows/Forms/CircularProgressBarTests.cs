using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class CircularProgressBarTests {
  private CircularProgressBar _progressBar;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._progressBar = new CircularProgressBar();
    this._form = new Form();
    this._form.Controls.Add(this._progressBar);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._progressBar?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._progressBar.Value, Is.EqualTo(0));
    Assert.That(this._progressBar.Minimum, Is.EqualTo(0));
    Assert.That(this._progressBar.Maximum, Is.EqualTo(100));
    Assert.That(this._progressBar.Thickness, Is.EqualTo(10));
    Assert.That(this._progressBar.ProgressColor, Is.EqualTo(Color.DodgerBlue));
    Assert.That(this._progressBar.TrackColor, Is.EqualTo(Color.LightGray));
    Assert.That(this._progressBar.ShowText, Is.True);
    Assert.That(this._progressBar.TextFormat, Is.EqualTo("{0:0}%"));
    Assert.That(this._progressBar.IsIndeterminate, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Value_CanBeSetAndRetrieved() {
    this._progressBar.Value = 50;
    Assert.That(this._progressBar.Value, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueChanged_IsRaisedWhenValueChanges() {
    var eventRaised = false;
    this._progressBar.ValueChanged += (s, e) => eventRaised = true;

    this._progressBar.Value = 75;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ValueChanged_NotRaisedWhenSameValue() {
    this._progressBar.Value = 50;
    var eventRaised = false;
    this._progressBar.ValueChanged += (s, e) => eventRaised = true;

    this._progressBar.Value = 50;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Value_ClampedToMinimum() {
    this._progressBar.Minimum = 10;
    this._progressBar.Value = 5;
    Assert.That(this._progressBar.Value, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Value_ClampedToMaximum() {
    this._progressBar.Maximum = 100;
    this._progressBar.Value = 150;
    Assert.That(this._progressBar.Value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Minimum_CanBeSetAndRetrieved() {
    this._progressBar.Minimum = 10;
    Assert.That(this._progressBar.Minimum, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Minimum_AdjustsValueWhenIncreased() {
    this._progressBar.Value = 20;
    this._progressBar.Minimum = 30;
    Assert.That(this._progressBar.Value, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void Maximum_CanBeSetAndRetrieved() {
    this._progressBar.Maximum = 200;
    Assert.That(this._progressBar.Maximum, Is.EqualTo(200));
  }

  [Test]
  [Category("EdgeCase")]
  public void Maximum_AdjustsValueWhenDecreased() {
    this._progressBar.Value = 80;
    this._progressBar.Maximum = 50;
    Assert.That(this._progressBar.Value, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void Thickness_CanBeSetAndRetrieved() {
    this._progressBar.Thickness = 20;
    Assert.That(this._progressBar.Thickness, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void Thickness_MinimumIsTwo() {
    this._progressBar.Thickness = 1;
    Assert.That(this._progressBar.Thickness, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ProgressColor_CanBeSetAndRetrieved() {
    var expected = Color.Green;
    this._progressBar.ProgressColor = expected;
    Assert.That(this._progressBar.ProgressColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void TrackColor_CanBeSetAndRetrieved() {
    var expected = Color.DarkGray;
    this._progressBar.TrackColor = expected;
    Assert.That(this._progressBar.TrackColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowText_CanBeSetAndRetrieved() {
    this._progressBar.ShowText = false;
    Assert.That(this._progressBar.ShowText, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TextFormat_CanBeSetAndRetrieved() {
    const string expected = "{0:0.0}%";
    this._progressBar.TextFormat = expected;
    Assert.That(this._progressBar.TextFormat, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void TextFormat_NullValue_BecomesDefault() {
    this._progressBar.TextFormat = null;
    Assert.That(this._progressBar.TextFormat, Is.EqualTo("{0:0}%"));
  }

  [Test]
  [Category("HappyPath")]
  public void IsIndeterminate_CanBeSetAndRetrieved() {
    this._progressBar.IsIndeterminate = true;
    Assert.That(this._progressBar.IsIndeterminate, Is.True);

    this._progressBar.IsIndeterminate = false;
    Assert.That(this._progressBar.IsIndeterminate, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._progressBar.Width, Is.EqualTo(100));
    Assert.That(this._progressBar.Height, Is.EqualTo(100));
  }
}
