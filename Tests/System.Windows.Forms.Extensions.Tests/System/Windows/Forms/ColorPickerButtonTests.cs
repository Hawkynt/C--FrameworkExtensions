using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ColorPickerButtonTests {
  private ColorPickerButton _colorPicker;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._colorPicker = new ColorPickerButton();
    this._form = new Form();
    this._form.Controls.Add(this._colorPicker);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._colorPicker?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._colorPicker.SelectedColor, Is.EqualTo(Color.Black));
    Assert.That(this._colorPicker.MaxRecentColors, Is.EqualTo(10));
    Assert.That(this._colorPicker.AllowCustomColor, Is.True);
    Assert.That(this._colorPicker.ShowColorName, Is.True);
    Assert.That(this._colorPicker.StandardColors, Is.Not.Null);
    Assert.That(this._colorPicker.StandardColors.Length, Is.GreaterThan(0));
    Assert.That(this._colorPicker.RecentColors, Is.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedColor_CanBeSetAndRetrieved() {
    var expected = Color.Red;
    this._colorPicker.SelectedColor = expected;
    Assert.That(this._colorPicker.SelectedColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedColorChanged_IsRaisedWhenColorChanges() {
    var eventRaised = false;
    this._colorPicker.SelectedColorChanged += (s, e) => eventRaised = true;

    this._colorPicker.SelectedColor = Color.Blue;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void SelectedColorChanged_NotRaisedWhenSameColor() {
    this._colorPicker.SelectedColor = Color.Green;
    var eventRaised = false;
    this._colorPicker.SelectedColorChanged += (s, e) => eventRaised = true;

    this._colorPicker.SelectedColor = Color.Green;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void RecentColors_TracksSelectedColors() {
    this._colorPicker.SelectedColor = Color.Red;
    this._colorPicker.SelectedColor = Color.Blue;
    this._colorPicker.SelectedColor = Color.Green;

    var recent = this._colorPicker.RecentColors;
    Assert.That(recent.Length, Is.EqualTo(3));
    Assert.That(recent[0], Is.EqualTo(Color.Green));
    Assert.That(recent[1], Is.EqualTo(Color.Blue));
    Assert.That(recent[2], Is.EqualTo(Color.Red));
  }

  [Test]
  [Category("EdgeCase")]
  public void RecentColors_DoesNotDuplicateColors() {
    this._colorPicker.SelectedColor = Color.Red;
    this._colorPicker.SelectedColor = Color.Blue;
    this._colorPicker.SelectedColor = Color.Red;

    var recent = this._colorPicker.RecentColors;
    Assert.That(recent.Length, Is.EqualTo(2));
    Assert.That(recent[0], Is.EqualTo(Color.Red));
    Assert.That(recent[1], Is.EqualTo(Color.Blue));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxRecentColors_CanBeSetAndRetrieved() {
    this._colorPicker.MaxRecentColors = 5;
    Assert.That(this._colorPicker.MaxRecentColors, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void MaxRecentColors_LimitsRecentList() {
    this._colorPicker.MaxRecentColors = 2;

    this._colorPicker.SelectedColor = Color.Red;
    this._colorPicker.SelectedColor = Color.Blue;
    this._colorPicker.SelectedColor = Color.Green;

    Assert.That(this._colorPicker.RecentColors.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void MaxRecentColors_NegativeValue_BecomesZero() {
    this._colorPicker.MaxRecentColors = -5;
    Assert.That(this._colorPicker.MaxRecentColors, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AllowCustomColor_CanBeSetAndRetrieved() {
    this._colorPicker.AllowCustomColor = false;
    Assert.That(this._colorPicker.AllowCustomColor, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ShowColorName_CanBeSetAndRetrieved() {
    this._colorPicker.ShowColorName = false;
    Assert.That(this._colorPicker.ShowColorName, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void StandardColors_CanBeSetAndRetrieved() {
    var customColors = new[] { Color.Red, Color.Green, Color.Blue };
    this._colorPicker.StandardColors = customColors;
    Assert.That(this._colorPicker.StandardColors, Is.EqualTo(customColors));
  }

  [Test]
  [Category("EdgeCase")]
  public void StandardColors_NullValue_ResetsToDefault() {
    this._colorPicker.StandardColors = null;
    Assert.That(this._colorPicker.StandardColors, Is.Not.Null);
    Assert.That(this._colorPicker.StandardColors.Length, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._colorPicker.Width, Is.EqualTo(100));
    Assert.That(this._colorPicker.Height, Is.EqualTo(25));
  }
}
