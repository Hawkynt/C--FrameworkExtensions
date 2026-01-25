using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class PlaceholderTextBoxTests {
  private PlaceholderTextBox _textBox;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._textBox = new PlaceholderTextBox();
    this._form = new Form();
    this._form.Controls.Add(this._textBox);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._textBox?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._textBox.PlaceholderText, Is.EqualTo(string.Empty));
    Assert.That(this._textBox.PlaceholderColor, Is.EqualTo(SystemColors.GrayText));
    Assert.That(this._textBox.ShowPlaceholderOnFocus, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void PlaceholderText_CanBeSetAndRetrieved() {
    const string expected = "Enter your name...";
    this._textBox.PlaceholderText = expected;
    Assert.That(this._textBox.PlaceholderText, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void PlaceholderColor_CanBeSetAndRetrieved() {
    var expected = Color.Red;
    this._textBox.PlaceholderColor = expected;
    Assert.That(this._textBox.PlaceholderColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowPlaceholderOnFocus_CanBeSetAndRetrieved() {
    this._textBox.ShowPlaceholderOnFocus = true;
    Assert.That(this._textBox.ShowPlaceholderOnFocus, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void PlaceholderText_NullValue_BecomesEmptyString() {
    this._textBox.PlaceholderText = null;
    Assert.That(this._textBox.PlaceholderText, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Text_WhenSet_DoesNotAffectPlaceholder() {
    this._textBox.PlaceholderText = "Placeholder";
    this._textBox.Text = "Actual text";
    Assert.That(this._textBox.PlaceholderText, Is.EqualTo("Placeholder"));
    Assert.That(this._textBox.Text, Is.EqualTo("Actual text"));
  }
}
