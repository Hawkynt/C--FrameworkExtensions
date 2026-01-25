using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class BreadcrumbControlTests {
  private BreadcrumbControl _breadcrumb;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._breadcrumb = new BreadcrumbControl();
    this._form = new Form();
    this._form.Controls.Add(this._breadcrumb);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._breadcrumb?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._breadcrumb.Items, Is.Empty);
    Assert.That(this._breadcrumb.Separator, Is.EqualTo(" > "));
    Assert.That(this._breadcrumb.ClickableItems, Is.True);
    Assert.That(this._breadcrumb.OverflowMode, Is.EqualTo(BreadcrumbOverflowMode.Ellipsis));
  }

  [Test]
  [Category("HappyPath")]
  public void Push_AddsItem() {
    this._breadcrumb.Push("Home");

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(1));
    Assert.That(this._breadcrumb.Items[0].Text, Is.EqualTo("Home"));
  }

  [Test]
  [Category("HappyPath")]
  public void Push_MultipleItems() {
    this._breadcrumb.Push("Home");
    this._breadcrumb.Push("Documents");
    this._breadcrumb.Push("Reports");

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Push_WithTag() {
    var tag = new object();
    this._breadcrumb.Push("Home", tag);

    Assert.That(this._breadcrumb.Items[0].Tag, Is.EqualTo(tag));
  }

  [Test]
  [Category("HappyPath")]
  public void Pop_RemovesLastItem() {
    this._breadcrumb.Push("Home");
    this._breadcrumb.Push("Documents");
    this._breadcrumb.Pop();

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(1));
    Assert.That(this._breadcrumb.Items[0].Text, Is.EqualTo("Home"));
  }

  [Test]
  [Category("EdgeCase")]
  public void Pop_OnEmptyList_DoesNothing() {
    this._breadcrumb.Pop();

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void NavigateTo_RemovesItemsAfterIndex() {
    this._breadcrumb.Push("Home");
    this._breadcrumb.Push("Documents");
    this._breadcrumb.Push("Reports");
    this._breadcrumb.Push("2024");

    this._breadcrumb.NavigateTo(1);

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(2));
    Assert.That(this._breadcrumb.Items[1].Text, Is.EqualTo("Documents"));
  }

  [Test]
  [Category("EdgeCase")]
  public void NavigateTo_InvalidIndex_DoesNothing() {
    this._breadcrumb.Push("Home");
    this._breadcrumb.Push("Documents");

    this._breadcrumb.NavigateTo(5);

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void NavigateTo_NegativeIndex_DoesNothing() {
    this._breadcrumb.Push("Home");

    this._breadcrumb.NavigateTo(-1);

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_RemovesAllItems() {
    this._breadcrumb.Push("Home");
    this._breadcrumb.Push("Documents");
    this._breadcrumb.Clear();

    Assert.That(this._breadcrumb.Items.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Separator_CanBeSetAndRetrieved() {
    this._breadcrumb.Separator = " / ";
    Assert.That(this._breadcrumb.Separator, Is.EqualTo(" / "));
  }

  [Test]
  [Category("EdgeCase")]
  public void Separator_NullValue_BecomesDefault() {
    this._breadcrumb.Separator = null;
    Assert.That(this._breadcrumb.Separator, Is.EqualTo(" > "));
  }

  [Test]
  [Category("HappyPath")]
  public void ClickableItems_CanBeSetAndRetrieved() {
    this._breadcrumb.ClickableItems = false;
    Assert.That(this._breadcrumb.ClickableItems, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OverflowMode_CanBeSetAndRetrieved() {
    this._breadcrumb.OverflowMode = BreadcrumbOverflowMode.Scroll;
    Assert.That(this._breadcrumb.OverflowMode, Is.EqualTo(BreadcrumbOverflowMode.Scroll));
  }

  [Test]
  [Category("HappyPath")]
  public void BreadcrumbItem_Properties() {
    var item = new BreadcrumbItem("Test", "tag");

    Assert.That(item.Text, Is.EqualTo("Test"));
    Assert.That(item.Tag, Is.EqualTo("tag"));
    Assert.That(item.Icon, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._breadcrumb.Width, Is.EqualTo(300));
    Assert.That(this._breadcrumb.Height, Is.EqualTo(24));
  }
}
