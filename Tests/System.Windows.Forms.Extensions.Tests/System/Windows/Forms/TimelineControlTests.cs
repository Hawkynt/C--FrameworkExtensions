using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class TimelineControlTests {
  private TimelineControl _timeline;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._timeline = new TimelineControl();
    this._form = new Form();
    this._form.Controls.Add(this._timeline);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._timeline?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._timeline.Items, Is.Empty);
    Assert.That(this._timeline.Layout, Is.EqualTo(TimelineLayout.Left));
    Assert.That(this._timeline.LineColor, Is.EqualTo(Color.LightGray));
    Assert.That(this._timeline.NodeSize, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void AddItem_CreatesItem() {
    var date = DateTime.Now;
    var item = this._timeline.AddItem(date, "Test", "Description");

    Assert.That(item, Is.Not.Null);
    Assert.That(item.Title, Is.EqualTo("Test"));
    Assert.That(item.Description, Is.EqualTo("Description"));
    Assert.That(item.Date, Is.EqualTo(date));
    Assert.That(this._timeline.Items.Length, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void AddItem_MultipleItems() {
    this._timeline.AddItem(DateTime.Now, "Item 1");
    this._timeline.AddItem(DateTime.Now.AddDays(1), "Item 2");
    this._timeline.AddItem(DateTime.Now.AddDays(2), "Item 3");

    Assert.That(this._timeline.Items.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void AddItem_SortsByDate() {
    var later = DateTime.Now.AddDays(2);
    var earlier = DateTime.Now;
    var middle = DateTime.Now.AddDays(1);

    this._timeline.AddItem(later, "Later");
    this._timeline.AddItem(earlier, "Earlier");
    this._timeline.AddItem(middle, "Middle");

    Assert.That(this._timeline.Items[0].Title, Is.EqualTo("Earlier"));
    Assert.That(this._timeline.Items[1].Title, Is.EqualTo("Middle"));
    Assert.That(this._timeline.Items[2].Title, Is.EqualTo("Later"));
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveItem_RemovesItem() {
    var item = this._timeline.AddItem(DateTime.Now, "Test");
    this._timeline.RemoveItem(item);

    Assert.That(this._timeline.Items.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ClearItems_RemovesAllItems() {
    this._timeline.AddItem(DateTime.Now, "Item 1");
    this._timeline.AddItem(DateTime.Now, "Item 2");
    this._timeline.ClearItems();

    Assert.That(this._timeline.Items.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Layout_CanBeSetAndRetrieved() {
    this._timeline.Layout = TimelineLayout.Right;
    Assert.That(this._timeline.Layout, Is.EqualTo(TimelineLayout.Right));

    this._timeline.Layout = TimelineLayout.Alternating;
    Assert.That(this._timeline.Layout, Is.EqualTo(TimelineLayout.Alternating));
  }

  [Test]
  [Category("HappyPath")]
  public void LineColor_CanBeSetAndRetrieved() {
    var expected = Color.DarkGray;
    this._timeline.LineColor = expected;
    Assert.That(this._timeline.LineColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void NodeSize_CanBeSetAndRetrieved() {
    this._timeline.NodeSize = 24;
    Assert.That(this._timeline.NodeSize, Is.EqualTo(24));
  }

  [Test]
  [Category("EdgeCase")]
  public void NodeSize_MinimumIsEight() {
    this._timeline.NodeSize = 4;
    Assert.That(this._timeline.NodeSize, Is.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void TimelineItem_Properties() {
    var item = new TimelineItem {
      Date = DateTime.Now,
      Title = "Title",
      Description = "Description",
      NodeColor = Color.Green,
      Tag = "tag"
    };

    Assert.That(item.Title, Is.EqualTo("Title"));
    Assert.That(item.Description, Is.EqualTo("Description"));
    Assert.That(item.NodeColor, Is.EqualTo(Color.Green));
    Assert.That(item.Tag, Is.EqualTo("tag"));
    Assert.That(item.Icon, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void TimelineItem_ParameterizedConstructor() {
    var date = DateTime.Now;
    var item = new TimelineItem(date, "Title", "Description");

    Assert.That(item.Date, Is.EqualTo(date));
    Assert.That(item.Title, Is.EqualTo("Title"));
    Assert.That(item.Description, Is.EqualTo("Description"));
  }

  [Test]
  [Category("HappyPath")]
  public void ItemClicked_EventArgs_ContainsCorrectData() {
    var item = this._timeline.AddItem(DateTime.Now, "Test");
    var args = new TimelineItemEventArgs(item, 0);

    Assert.That(args.Item, Is.EqualTo(item));
    Assert.That(args.Index, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._timeline.Width, Is.EqualTo(300));
    Assert.That(this._timeline.Height, Is.EqualTo(400));
  }
}
