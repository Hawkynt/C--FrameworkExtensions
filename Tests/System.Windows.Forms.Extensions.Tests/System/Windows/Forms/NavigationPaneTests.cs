using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class NavigationPaneTests {
  private NavigationPane _navPane;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._navPane = new NavigationPane();
    this._form = new Form();
    this._form.Controls.Add(this._navPane);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._navPane?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._navPane.DisplayMode, Is.EqualTo(NavigationPaneDisplayMode.Expanded));
    Assert.That(this._navPane.CollapsedWidth, Is.EqualTo(48));
    Assert.That(this._navPane.ExpandedWidth, Is.EqualTo(200));
    Assert.That(this._navPane.AllowCollapse, Is.True);
    Assert.That(this._navPane.AnimateCollapse, Is.True);
    Assert.That(this._navPane.SelectedItem, Is.Null);
    Assert.That(this._navPane.SelectedColor, Is.EqualTo(SystemColors.Highlight));
    Assert.That(this._navPane.HoverColor, Is.EqualTo(SystemColors.ControlLight));
  }

  [Test]
  [Category("HappyPath")]
  public void AddGroup_CreatesGroup() {
    var group = this._navPane.AddGroup("Test Group");

    Assert.That(this._navPane.Groups.Count, Is.EqualTo(1));
    Assert.That(group.Header, Is.EqualTo("Test Group"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddGroup_WithIcon_SetsIcon() {
    using var icon = new Bitmap(16, 16);
    var group = this._navPane.AddGroup("Test", icon);

    Assert.That(group.Icon, Is.EqualTo(icon));
  }

  [Test]
  [Category("HappyPath")]
  public void Group_AddItem_CreatesItem() {
    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Item 1");

    Assert.That(group.Items.Count, Is.EqualTo(1));
    Assert.That(item.Text, Is.EqualTo("Item 1"));
  }

  [Test]
  [Category("HappyPath")]
  public void Group_AddItem_WithAllProperties_SetsProperties() {
    using var icon = new Bitmap(16, 16);
    var tag = new object();

    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Item", icon, tag);

    Assert.That(item.Icon, Is.EqualTo(icon));
    Assert.That(item.Tag, Is.EqualTo(tag));
  }

  [Test]
  [Category("HappyPath")]
  public void Item_AddSubItem_CreatesSubItem() {
    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Parent");
    var subItem = item.AddSubItem("Child");

    Assert.That(item.SubItems.Count, Is.EqualTo(1));
    Assert.That(subItem.Text, Is.EqualTo("Child"));
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveGroup_RemovesGroup() {
    var group = this._navPane.AddGroup("To Remove");
    this._navPane.RemoveGroup(group);

    Assert.That(this._navPane.Groups.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Group_RemoveItem_RemovesItem() {
    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Item");
    group.RemoveItem(item);

    Assert.That(group.Items.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedItem_CanBeSetAndRetrieved() {
    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Item");

    this._navPane.SelectedItem = item;

    Assert.That(this._navPane.SelectedItem, Is.EqualTo(item));
    Assert.That(item.IsSelected, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedItem_ClearsOldSelection() {
    var group = this._navPane.AddGroup("Group");
    var item1 = group.AddItem("Item 1");
    var item2 = group.AddItem("Item 2");

    this._navPane.SelectedItem = item1;
    this._navPane.SelectedItem = item2;

    Assert.That(item1.IsSelected, Is.False);
    Assert.That(item2.IsSelected, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ItemSelected_IsRaisedWhenItemSelected() {
    var eventRaised = false;
    NavigationItem receivedItem = null;

    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Item");

    this._navPane.ItemSelected += (s, e) => {
      eventRaised = true;
      receivedItem = e.Item;
    };

    this._navPane.SelectedItem = item;

    Assert.That(eventRaised, Is.True);
    Assert.That(receivedItem, Is.EqualTo(item));
  }

  [Test]
  [Category("HappyPath")]
  public void Expand_SetsDisplayModeToExpanded() {
    this._navPane.Collapse();
    this._navPane.Expand();

    Assert.That(this._navPane.DisplayMode, Is.EqualTo(NavigationPaneDisplayMode.Expanded));
  }

  [Test]
  [Category("HappyPath")]
  public void Collapse_SetsDisplayModeToCollapsed() {
    this._navPane.Collapse();

    Assert.That(this._navPane.DisplayMode, Is.EqualTo(NavigationPaneDisplayMode.Collapsed));
  }

  [Test]
  [Category("HappyPath")]
  public void Toggle_SwitchesDisplayMode() {
    this._navPane.Toggle();
    Assert.That(this._navPane.DisplayMode, Is.EqualTo(NavigationPaneDisplayMode.Collapsed));

    this._navPane.Toggle();
    Assert.That(this._navPane.DisplayMode, Is.EqualTo(NavigationPaneDisplayMode.Expanded));
  }

  [Test]
  [Category("HappyPath")]
  public void DisplayModeChanged_IsRaisedWhenModeChanges() {
    var eventRaised = false;
    this._navPane.DisplayModeChanged += (s, e) => eventRaised = true;

    this._navPane.Collapse();

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void DisplayModeChanged_NotRaisedWhenSameMode() {
    var eventRaised = false;
    this._navPane.DisplayModeChanged += (s, e) => eventRaised = true;

    this._navPane.DisplayMode = NavigationPaneDisplayMode.Expanded;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollapsedWidth_CanBeSetAndRetrieved() {
    this._navPane.CollapsedWidth = 60;
    Assert.That(this._navPane.CollapsedWidth, Is.EqualTo(60));
  }

  [Test]
  [Category("EdgeCase")]
  public void CollapsedWidth_HasMinimumValue() {
    this._navPane.CollapsedWidth = 10;
    Assert.That(this._navPane.CollapsedWidth, Is.EqualTo(24));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandedWidth_CanBeSetAndRetrieved() {
    this._navPane.ExpandedWidth = 250;
    Assert.That(this._navPane.ExpandedWidth, Is.EqualTo(250));
  }

  [Test]
  [Category("EdgeCase")]
  public void ExpandedWidth_HasMinimumValue() {
    this._navPane.ExpandedWidth = 50;
    Assert.That(this._navPane.ExpandedWidth, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void AllowCollapse_CanBeSetAndRetrieved() {
    this._navPane.AllowCollapse = false;
    Assert.That(this._navPane.AllowCollapse, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AnimateCollapse_CanBeSetAndRetrieved() {
    this._navPane.AnimateCollapse = false;
    Assert.That(this._navPane.AnimateCollapse, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedColor_CanBeSetAndRetrieved() {
    this._navPane.SelectedColor = Color.Blue;
    Assert.That(this._navPane.SelectedColor, Is.EqualTo(Color.Blue));
  }

  [Test]
  [Category("HappyPath")]
  public void HoverColor_CanBeSetAndRetrieved() {
    this._navPane.HoverColor = Color.LightBlue;
    Assert.That(this._navPane.HoverColor, Is.EqualTo(Color.LightBlue));
  }

  [Test]
  [Category("HappyPath")]
  public void Group_IsExpanded_CanBeToggled() {
    var group = this._navPane.AddGroup("Group");

    Assert.That(group.IsExpanded, Is.True);

    group.IsExpanded = false;
    Assert.That(group.IsExpanded, Is.False);

    group.IsExpanded = true;
    Assert.That(group.IsExpanded, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Group_IsVisible_CanBeSetAndRetrieved() {
    var group = this._navPane.AddGroup("Group");
    group.IsVisible = false;

    Assert.That(group.IsVisible, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Item_BadgeCount_CanBeSetAndRetrieved() {
    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Item");
    item.BadgeCount = 5;

    Assert.That(item.BadgeCount, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Item_IsEnabled_CanBeSetAndRetrieved() {
    var group = this._navPane.AddGroup("Group");
    var item = group.AddItem("Item");
    item.IsEnabled = false;

    Assert.That(item.IsEnabled, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._navPane.Width, Is.EqualTo(200));
    Assert.That(this._navPane.Height, Is.EqualTo(300));
  }
}
