using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class BadgeLabelTests {
  private BadgeLabel _badge;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._badge = new BadgeLabel();
    this._form = new Form();
    this._form.Controls.Add(this._badge);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._badge?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._badge.BadgeValue, Is.EqualTo(0));
    Assert.That(this._badge.BadgeColor, Is.EqualTo(Color.Red));
    Assert.That(this._badge.BadgeTextColor, Is.EqualTo(Color.White));
    Assert.That(this._badge.BadgePosition, Is.EqualTo(ContentAlignment.TopRight));
    Assert.That(this._badge.MaxBadgeValue, Is.EqualTo(99));
    Assert.That(this._badge.HideWhenZero, Is.True);
    Assert.That(this._badge.BadgeSize, Is.EqualTo(18));
  }

  [Test]
  [Category("HappyPath")]
  public void BadgeValue_CanBeSetAndRetrieved() {
    this._badge.BadgeValue = 5;
    Assert.That(this._badge.BadgeValue, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void BadgeValue_NegativeValue_BecomesZero() {
    this._badge.BadgeValue = -10;
    Assert.That(this._badge.BadgeValue, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BadgeColor_CanBeSetAndRetrieved() {
    var expected = Color.Blue;
    this._badge.BadgeColor = expected;
    Assert.That(this._badge.BadgeColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void BadgeTextColor_CanBeSetAndRetrieved() {
    var expected = Color.Black;
    this._badge.BadgeTextColor = expected;
    Assert.That(this._badge.BadgeTextColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void BadgePosition_CanBeSetAndRetrieved() {
    this._badge.BadgePosition = ContentAlignment.BottomLeft;
    Assert.That(this._badge.BadgePosition, Is.EqualTo(ContentAlignment.BottomLeft));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxBadgeValue_CanBeSetAndRetrieved() {
    this._badge.MaxBadgeValue = 50;
    Assert.That(this._badge.MaxBadgeValue, Is.EqualTo(50));
  }

  [Test]
  [Category("EdgeCase")]
  public void MaxBadgeValue_MinimumIsOne() {
    this._badge.MaxBadgeValue = 0;
    Assert.That(this._badge.MaxBadgeValue, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void HideWhenZero_CanBeSetAndRetrieved() {
    this._badge.HideWhenZero = false;
    Assert.That(this._badge.HideWhenZero, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void BadgeSize_CanBeSetAndRetrieved() {
    this._badge.BadgeSize = 24;
    Assert.That(this._badge.BadgeSize, Is.EqualTo(24));
  }

  [Test]
  [Category("EdgeCase")]
  public void BadgeSize_MinimumIsTwelve() {
    this._badge.BadgeSize = 5;
    Assert.That(this._badge.BadgeSize, Is.EqualTo(12));
  }

  [Test]
  [Category("HappyPath")]
  public void Text_CanBeSetAndRetrieved() {
    const string expected = "Notifications";
    this._badge.Text = expected;
    Assert.That(this._badge.Text, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Icon_CanBeSetAndRetrieved() {
    using var bitmap = new Bitmap(16, 16);
    this._badge.Icon = bitmap;
    Assert.That(this._badge.Icon, Is.EqualTo(bitmap));
  }

  [Test]
  [Category("HappyPath")]
  public void Icon_CanBeNull() {
    this._badge.Icon = null;
    Assert.That(this._badge.Icon, Is.Null);
  }
}
