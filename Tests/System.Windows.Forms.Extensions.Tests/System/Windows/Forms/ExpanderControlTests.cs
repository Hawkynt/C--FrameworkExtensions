using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ExpanderControlTests {
  private ExpanderControl _expander;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._expander = new ExpanderControl();
    this._form = new Form();
    this._form.Controls.Add(this._expander);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._expander?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._expander.HeaderText, Is.EqualTo("Header"));
    Assert.That(this._expander.HeaderIcon, Is.Null);
    Assert.That(this._expander.IsExpanded, Is.True);
    Assert.That(this._expander.CollapsedHeight, Is.EqualTo(30));
    Assert.That(this._expander.ExpandedHeight, Is.EqualTo(200));
    Assert.That(this._expander.AnimateExpansion, Is.True);
    Assert.That(this._expander.ContentPanel, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void HeaderText_CanBeSetAndRetrieved() {
    const string expected = "Test Header";
    this._expander.HeaderText = expected;
    Assert.That(this._expander.HeaderText, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void HeaderText_NullValue_BecomesDefault() {
    this._expander.HeaderText = null;
    Assert.That(this._expander.HeaderText, Is.EqualTo("Header"));
  }

  [Test]
  [Category("HappyPath")]
  public void IsExpanded_CanBeSetAndRetrieved() {
    this._expander.AnimateExpansion = false;
    this._expander.IsExpanded = false;
    Assert.That(this._expander.IsExpanded, Is.False);

    this._expander.IsExpanded = true;
    Assert.That(this._expander.IsExpanded, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Toggle_ChangesExpandedState() {
    this._expander.AnimateExpansion = false;
    var initial = this._expander.IsExpanded;
    this._expander.Toggle();
    Assert.That(this._expander.IsExpanded, Is.Not.EqualTo(initial));
  }

  [Test]
  [Category("HappyPath")]
  public void Expanded_IsRaisedWhenExpanding() {
    this._expander.AnimateExpansion = false;
    this._expander.IsExpanded = false;

    var eventRaised = false;
    this._expander.Expanded += (s, e) => eventRaised = true;

    this._expander.IsExpanded = true;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Collapsed_IsRaisedWhenCollapsing() {
    this._expander.AnimateExpansion = false;
    this._expander.IsExpanded = true;

    var eventRaised = false;
    this._expander.Collapsed += (s, e) => eventRaised = true;

    this._expander.IsExpanded = false;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Expanding_CanBeCancelled() {
    this._expander.AnimateExpansion = false;
    this._expander.IsExpanded = false;

    this._expander.Expanding += (s, e) => e.Cancel = true;

    this._expander.IsExpanded = true;

    Assert.That(this._expander.IsExpanded, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Collapsing_CanBeCancelled() {
    this._expander.AnimateExpansion = false;
    this._expander.IsExpanded = true;

    this._expander.Collapsing += (s, e) => e.Cancel = true;

    this._expander.IsExpanded = false;

    Assert.That(this._expander.IsExpanded, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollapsedHeight_CanBeSetAndRetrieved() {
    this._expander.CollapsedHeight = 40;
    Assert.That(this._expander.CollapsedHeight, Is.EqualTo(40));
  }

  [Test]
  [Category("EdgeCase")]
  public void CollapsedHeight_MinimumIsTwenty() {
    this._expander.CollapsedHeight = 10;
    Assert.That(this._expander.CollapsedHeight, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandedHeight_CanBeSetAndRetrieved() {
    this._expander.ExpandedHeight = 300;
    Assert.That(this._expander.ExpandedHeight, Is.EqualTo(300));
  }

  [Test]
  [Category("EdgeCase")]
  public void ExpandedHeight_MinimumIsCollapsedPlusTwenty() {
    this._expander.CollapsedHeight = 30;
    this._expander.ExpandedHeight = 40;
    Assert.That(this._expander.ExpandedHeight, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void AnimateExpansion_CanBeSetAndRetrieved() {
    this._expander.AnimateExpansion = false;
    Assert.That(this._expander.AnimateExpansion, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ContentPanel_IsAccessible() {
    var label = new Label { Text = "Test" };
    this._expander.ContentPanel.Controls.Add(label);
    Assert.That(this._expander.ContentPanel.Controls.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_HasCorrectHeight() {
    Assert.That(this._expander.Height, Is.EqualTo(200));
  }
}
