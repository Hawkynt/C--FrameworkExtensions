using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class LoadingSpinnerTests {
  private LoadingSpinner _spinner;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._spinner = new LoadingSpinner();
    this._form = new Form();
    this._form.Controls.Add(this._spinner);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._spinner?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._spinner.IsSpinning, Is.False);
    Assert.That(this._spinner.Style, Is.EqualTo(SpinnerStyle.Circle));
    Assert.That(this._spinner.SpinnerColor, Is.EqualTo(Color.DodgerBlue));
    Assert.That(this._spinner.Speed, Is.EqualTo(100));
    Assert.That(this._spinner.LoadingText, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void IsSpinning_CanBeToggled() {
    this._spinner.IsSpinning = true;
    Assert.That(this._spinner.IsSpinning, Is.True);

    this._spinner.IsSpinning = false;
    Assert.That(this._spinner.IsSpinning, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Start_SetsIsSpinningToTrue() {
    this._spinner.Start();
    Assert.That(this._spinner.IsSpinning, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Stop_SetsIsSpinningToFalse() {
    this._spinner.Start();
    this._spinner.Stop();
    Assert.That(this._spinner.IsSpinning, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Style_CanBeSetAndRetrieved() {
    this._spinner.Style = SpinnerStyle.Dots;
    Assert.That(this._spinner.Style, Is.EqualTo(SpinnerStyle.Dots));

    this._spinner.Style = SpinnerStyle.Bars;
    Assert.That(this._spinner.Style, Is.EqualTo(SpinnerStyle.Bars));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinnerColor_CanBeSetAndRetrieved() {
    var expected = Color.Green;
    this._spinner.SpinnerColor = expected;
    Assert.That(this._spinner.SpinnerColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Speed_CanBeSetAndRetrieved() {
    this._spinner.Speed = 50;
    Assert.That(this._spinner.Speed, Is.EqualTo(50));
  }

  [Test]
  [Category("EdgeCase")]
  public void Speed_ClampedToMinimum() {
    this._spinner.Speed = 5;
    Assert.That(this._spinner.Speed, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Speed_ClampedToMaximum() {
    this._spinner.Speed = 2000;
    Assert.That(this._spinner.Speed, Is.EqualTo(1000));
  }

  [Test]
  [Category("HappyPath")]
  public void LoadingText_CanBeSetAndRetrieved() {
    const string expected = "Loading...";
    this._spinner.LoadingText = expected;
    Assert.That(this._spinner.LoadingText, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void LoadingText_NullValue_BecomesEmptyString() {
    this._spinner.LoadingText = null;
    Assert.That(this._spinner.LoadingText, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._spinner.Width, Is.EqualTo(48));
    Assert.That(this._spinner.Height, Is.EqualTo(48));
  }
}
