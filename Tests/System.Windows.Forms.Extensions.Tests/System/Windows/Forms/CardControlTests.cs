using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class CardControlTests {
  private CardControl _card;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._card = new CardControl();
    this._form = new Form();
    this._form.Controls.Add(this._card);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._card?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._card.Title, Is.EqualTo(string.Empty));
    Assert.That(this._card.TitleIcon, Is.Null);
    Assert.That(this._card.ShowShadow, Is.True);
    Assert.That(this._card.ShadowDepth, Is.EqualTo(5));
    Assert.That(this._card.CornerRadius, Is.EqualTo(8));
    Assert.That(this._card.CardColor, Is.EqualTo(Color.White));
    Assert.That(this._card.ContentPanel, Is.Not.Null);
    Assert.That(this._card.ActionPanel, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Title_CanBeSetAndRetrieved() {
    const string expected = "Test Title";
    this._card.Title = expected;
    Assert.That(this._card.Title, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void Title_NullValue_BecomesEmpty() {
    this._card.Title = null;
    Assert.That(this._card.Title, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowShadow_CanBeSetAndRetrieved() {
    this._card.ShowShadow = false;
    Assert.That(this._card.ShowShadow, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ShadowDepth_CanBeSetAndRetrieved() {
    this._card.ShadowDepth = 10;
    Assert.That(this._card.ShadowDepth, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void ShadowDepth_ClampedToMinimum() {
    this._card.ShadowDepth = -5;
    Assert.That(this._card.ShadowDepth, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void ShadowDepth_ClampedToMaximum() {
    this._card.ShadowDepth = 50;
    Assert.That(this._card.ShadowDepth, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void CornerRadius_CanBeSetAndRetrieved() {
    this._card.CornerRadius = 12;
    Assert.That(this._card.CornerRadius, Is.EqualTo(12));
  }

  [Test]
  [Category("EdgeCase")]
  public void CornerRadius_MinimumIsZero() {
    this._card.CornerRadius = -5;
    Assert.That(this._card.CornerRadius, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CardColor_CanBeSetAndRetrieved() {
    var expected = Color.LightBlue;
    this._card.CardColor = expected;
    Assert.That(this._card.CardColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ContentPanel_IsAccessible() {
    var label = new Label { Text = "Test" };
    this._card.ContentPanel.Controls.Add(label);
    Assert.That(this._card.ContentPanel.Controls.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ActionPanel_IsAccessible() {
    var button = new Button { Text = "OK" };
    this._card.ActionPanel.Controls.Add(button);
    Assert.That(this._card.ActionPanel.Controls.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowActionPanel_SetsHeight() {
    this._card.ShowActionPanel(50);
    Assert.That(this._card.ActionPanel.Height, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void HideActionPanel_SetsHeightToZero() {
    this._card.ShowActionPanel(50);
    this._card.HideActionPanel();
    Assert.That(this._card.ActionPanel.Height, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._card.Width, Is.EqualTo(300));
    Assert.That(this._card.Height, Is.EqualTo(200));
  }
}
