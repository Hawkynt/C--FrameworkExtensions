using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class RatingControlTests {
  private RatingControl _rating;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._rating = new RatingControl();
    this._form = new Form();
    this._form.Controls.Add(this._rating);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._rating?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._rating.Value, Is.EqualTo(0));
    Assert.That(this._rating.MaxRating, Is.EqualTo(5));
    Assert.That(this._rating.AllowHalfStars, Is.False);
    Assert.That(this._rating.ReadOnly, Is.False);
    Assert.That(this._rating.ImageSize, Is.EqualTo(24));
    Assert.That(this._rating.Spacing, Is.EqualTo(2));
    Assert.That(this._rating.FilledColor, Is.EqualTo(Color.Gold));
    Assert.That(this._rating.EmptyColor, Is.EqualTo(Color.LightGray));
  }

  [Test]
  [Category("HappyPath")]
  public void Value_CanBeSetAndRetrieved() {
    this._rating.Value = 3;
    Assert.That(this._rating.Value, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueChanged_IsRaisedWhenValueChanges() {
    var eventRaised = false;
    this._rating.ValueChanged += (s, e) => eventRaised = true;

    this._rating.Value = 4;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ValueChanged_NotRaisedWhenSameValue() {
    this._rating.Value = 3;
    var eventRaised = false;
    this._rating.ValueChanged += (s, e) => eventRaised = true;

    this._rating.Value = 3;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Value_ClampedToZero_WhenNegative() {
    this._rating.Value = -5;
    Assert.That(this._rating.Value, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Value_ClampedToMaxRating_WhenExceeded() {
    this._rating.MaxRating = 5;
    this._rating.Value = 10;
    Assert.That(this._rating.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxRating_CanBeSetAndRetrieved() {
    this._rating.MaxRating = 10;
    Assert.That(this._rating.MaxRating, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void MaxRating_MinimumIsOne() {
    this._rating.MaxRating = 0;
    Assert.That(this._rating.MaxRating, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Value_AdjustedWhenMaxRatingReduced() {
    this._rating.MaxRating = 10;
    this._rating.Value = 8;
    this._rating.MaxRating = 5;
    Assert.That(this._rating.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void AllowHalfStars_CanBeSetAndRetrieved() {
    this._rating.AllowHalfStars = true;
    Assert.That(this._rating.AllowHalfStars, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnly_CanBeSetAndRetrieved() {
    this._rating.ReadOnly = true;
    Assert.That(this._rating.ReadOnly, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ImageSize_CanBeSetAndRetrieved() {
    this._rating.ImageSize = 32;
    Assert.That(this._rating.ImageSize, Is.EqualTo(32));
  }

  [Test]
  [Category("EdgeCase")]
  public void ImageSize_MinimumIsEight() {
    this._rating.ImageSize = 2;
    Assert.That(this._rating.ImageSize, Is.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Spacing_CanBeSetAndRetrieved() {
    this._rating.Spacing = 5;
    Assert.That(this._rating.Spacing, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Spacing_MinimumIsZero() {
    this._rating.Spacing = -5;
    Assert.That(this._rating.Spacing, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void FilledColor_CanBeSetAndRetrieved() {
    var expected = Color.Yellow;
    this._rating.FilledColor = expected;
    Assert.That(this._rating.FilledColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void EmptyColor_CanBeSetAndRetrieved() {
    var expected = Color.DarkGray;
    this._rating.EmptyColor = expected;
    Assert.That(this._rating.EmptyColor, Is.EqualTo(expected));
  }
}
