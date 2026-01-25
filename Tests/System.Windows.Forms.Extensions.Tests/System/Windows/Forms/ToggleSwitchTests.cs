using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ToggleSwitchTests {
  private ToggleSwitch _toggle;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._toggle = new ToggleSwitch();
    this._form = new Form();
    this._form.Controls.Add(this._toggle);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._toggle?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._toggle.Checked, Is.False);
    Assert.That(this._toggle.OnColor, Is.EqualTo(Color.DodgerBlue));
    Assert.That(this._toggle.OffColor, Is.EqualTo(Color.LightGray));
    Assert.That(this._toggle.ThumbColor, Is.EqualTo(Color.White));
    Assert.That(this._toggle.OnText, Is.EqualTo("ON"));
    Assert.That(this._toggle.OffText, Is.EqualTo("OFF"));
    Assert.That(this._toggle.ShowText, Is.False);
    Assert.That(this._toggle.AnimateTransition, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Checked_CanBeToggled() {
    this._toggle.Checked = true;
    Assert.That(this._toggle.Checked, Is.True);

    this._toggle.Checked = false;
    Assert.That(this._toggle.Checked, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CheckedChanged_IsRaisedWhenCheckedChanges() {
    var eventRaised = false;
    this._toggle.CheckedChanged += (s, e) => eventRaised = true;

    this._toggle.Checked = true;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void CheckedChanged_NotRaisedWhenSameValue() {
    this._toggle.Checked = false;
    var eventRaised = false;
    this._toggle.CheckedChanged += (s, e) => eventRaised = true;

    this._toggle.Checked = false;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OnColor_CanBeSetAndRetrieved() {
    var expected = Color.Green;
    this._toggle.OnColor = expected;
    Assert.That(this._toggle.OnColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void OffColor_CanBeSetAndRetrieved() {
    var expected = Color.DarkGray;
    this._toggle.OffColor = expected;
    Assert.That(this._toggle.OffColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ThumbColor_CanBeSetAndRetrieved() {
    var expected = Color.Black;
    this._toggle.ThumbColor = expected;
    Assert.That(this._toggle.ThumbColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void OnText_CanBeSetAndRetrieved() {
    const string expected = "Yes";
    this._toggle.OnText = expected;
    Assert.That(this._toggle.OnText, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void OffText_CanBeSetAndRetrieved() {
    const string expected = "No";
    this._toggle.OffText = expected;
    Assert.That(this._toggle.OffText, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowText_CanBeSetAndRetrieved() {
    this._toggle.ShowText = true;
    Assert.That(this._toggle.ShowText, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void OnText_NullValue_BecomesEmptyString() {
    this._toggle.OnText = null;
    Assert.That(this._toggle.OnText, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("EdgeCase")]
  public void OffText_NullValue_BecomesEmptyString() {
    this._toggle.OffText = null;
    Assert.That(this._toggle.OffText, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._toggle.Width, Is.EqualTo(50));
    Assert.That(this._toggle.Height, Is.EqualTo(24));
  }
}
