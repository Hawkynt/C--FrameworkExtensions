using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class SearchTextBoxTests {
  private SearchTextBox _searchBox;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._searchBox = new SearchTextBox();
    this._form = new Form();
    this._form.Controls.Add(this._searchBox);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._searchBox?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._searchBox.Text, Is.EqualTo(string.Empty));
    Assert.That(this._searchBox.PlaceholderText, Is.EqualTo("Search..."));
    Assert.That(this._searchBox.ShowClearButton, Is.True);
    Assert.That(this._searchBox.SearchDelay, Is.EqualTo(300));
  }

  [Test]
  [Category("HappyPath")]
  public void Text_CanBeSetAndRetrieved() {
    this._searchBox.Text = "test query";
    Assert.That(this._searchBox.Text, Is.EqualTo("test query"));
  }

  [Test]
  [Category("HappyPath")]
  public void PlaceholderText_CanBeSetAndRetrieved() {
    const string expected = "Find...";
    this._searchBox.PlaceholderText = expected;
    Assert.That(this._searchBox.PlaceholderText, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void PlaceholderText_NullValue_BecomesDefault() {
    this._searchBox.PlaceholderText = null;
    Assert.That(this._searchBox.PlaceholderText, Is.EqualTo("Search..."));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowClearButton_CanBeSetAndRetrieved() {
    this._searchBox.ShowClearButton = false;
    Assert.That(this._searchBox.ShowClearButton, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SearchDelay_CanBeSetAndRetrieved() {
    this._searchBox.SearchDelay = 500;
    Assert.That(this._searchBox.SearchDelay, Is.EqualTo(500));
  }

  [Test]
  [Category("EdgeCase")]
  public void SearchDelay_NegativeValue_BecomesZero() {
    this._searchBox.SearchDelay = -100;
    Assert.That(this._searchBox.SearchDelay, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Text_EmptyWhenPlaceholderShown() {
    Assert.That(this._searchBox.Text, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Text_ClearedCorrectly() {
    this._searchBox.Text = "some text";
    this._searchBox.Text = string.Empty;
    Assert.That(this._searchBox.Text, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._searchBox.Width, Is.EqualTo(200));
    Assert.That(this._searchBox.Height, Is.EqualTo(26));
  }
}
