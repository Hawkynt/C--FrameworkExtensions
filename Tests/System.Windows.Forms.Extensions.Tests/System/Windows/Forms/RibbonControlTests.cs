using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class RibbonControlTests {
  private RibbonControl _ribbon;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._ribbon = new RibbonControl();
    this._form = new Form();
    this._form.Controls.Add(this._ribbon);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._ribbon?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._ribbon.SelectedTabIndex, Is.EqualTo(0));
    Assert.That(this._ribbon.ShowApplicationButton, Is.True);
    Assert.That(this._ribbon.ApplicationButtonText, Is.EqualTo("File"));
    Assert.That(this._ribbon.Minimized, Is.False);
    Assert.That(this._ribbon.AllowMinimize, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void AddTab_CreatesTab() {
    var tab = this._ribbon.AddTab("Home");

    Assert.That(this._ribbon.Tabs.Count, Is.EqualTo(1));
    Assert.That(tab.Text, Is.EqualTo("Home"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddTab_SelectsFirstTab() {
    var tab = this._ribbon.AddTab("Home");

    Assert.That(this._ribbon.SelectedTab, Is.EqualTo(tab));
    Assert.That(this._ribbon.SelectedTabIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AddMultipleTabs_KeepsFirstSelected() {
    var tab1 = this._ribbon.AddTab("Home");
    var tab2 = this._ribbon.AddTab("Insert");

    Assert.That(this._ribbon.SelectedTab, Is.EqualTo(tab1));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedTabIndex_CanBeChanged() {
    this._ribbon.AddTab("Home");
    var tab2 = this._ribbon.AddTab("Insert");

    this._ribbon.SelectedTabIndex = 1;

    Assert.That(this._ribbon.SelectedTab, Is.EqualTo(tab2));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedTab_CanBeSetDirectly() {
    this._ribbon.AddTab("Home");
    var tab2 = this._ribbon.AddTab("Insert");

    this._ribbon.SelectedTab = tab2;

    Assert.That(this._ribbon.SelectedTabIndex, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveTab_RemovesTab() {
    var tab = this._ribbon.AddTab("Home");
    this._ribbon.RemoveTab(tab);

    Assert.That(this._ribbon.Tabs.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Tab_AddGroup_CreatesGroup() {
    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");

    Assert.That(tab.Groups.Count, Is.EqualTo(1));
    Assert.That(group.Text, Is.EqualTo("Clipboard"));
  }

  [Test]
  [Category("HappyPath")]
  public void Tab_RemoveGroup_RemovesGroup() {
    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    tab.RemoveGroup(group);

    Assert.That(tab.Groups.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Group_AddButton_CreatesButton() {
    using var icon = new Bitmap(16, 16);
    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    var button = group.AddButton("Paste", icon, RibbonButtonStyle.Large);

    Assert.That(group.Items.Count, Is.EqualTo(1));
    Assert.That(button.Text, Is.EqualTo("Paste"));
    Assert.That(button.Style, Is.EqualTo(RibbonButtonStyle.Large));
  }

  [Test]
  [Category("HappyPath")]
  public void Group_AddSplitButton_CreatesSplitButton() {
    using var icon = new Bitmap(16, 16);
    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    var button = group.AddSplitButton("Paste", icon);

    Assert.That(group.Items.Count, Is.EqualTo(1));
    Assert.That(button, Is.TypeOf<RibbonSplitButton>());
  }

  [Test]
  [Category("HappyPath")]
  public void Group_AddDropDownButton_CreatesDropDownButton() {
    using var icon = new Bitmap(16, 16);
    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    var button = group.AddDropDownButton("Options", icon);

    Assert.That(group.Items.Count, Is.EqualTo(1));
    Assert.That(button, Is.TypeOf<RibbonDropDownButton>());
  }

  [Test]
  [Category("HappyPath")]
  public void Group_AddSeparator_CreatesSeparator() {
    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    var separator = group.AddSeparator();

    Assert.That(group.Items.Count, Is.EqualTo(1));
    Assert.That(separator, Is.TypeOf<RibbonSeparator>());
  }

  [Test]
  [Category("HappyPath")]
  public void TabSelected_IsRaisedWhenTabChanges() {
    var eventRaised = false;
    RibbonTab receivedTab = null;

    this._ribbon.AddTab("Home");
    var tab2 = this._ribbon.AddTab("Insert");

    this._ribbon.TabSelected += (s, e) => {
      eventRaised = true;
      receivedTab = e.Tab;
    };

    this._ribbon.SelectedTabIndex = 1;

    Assert.That(eventRaised, Is.True);
    Assert.That(receivedTab, Is.EqualTo(tab2));
  }

  [Test]
  [Category("HappyPath")]
  public void Button_Click_RaisesEvent() {
    var clickRaised = false;
    using var icon = new Bitmap(16, 16);

    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    var button = group.AddButton("Paste", icon);

    button.Click += (s, e) => clickRaised = true;
    button.GetType().GetMethod("OnClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(button, null);

    Assert.That(clickRaised, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Minimize_SetsMinimizedState() {
    this._ribbon.Minimize();

    Assert.That(this._ribbon.Minimized, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Restore_ClearsMinimizedState() {
    this._ribbon.Minimized = true;
    this._ribbon.Restore();

    Assert.That(this._ribbon.Minimized, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ShowApplicationButton_CanBeSetAndRetrieved() {
    this._ribbon.ShowApplicationButton = false;
    Assert.That(this._ribbon.ShowApplicationButton, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ApplicationButtonText_CanBeSetAndRetrieved() {
    this._ribbon.ApplicationButtonText = "Start";
    Assert.That(this._ribbon.ApplicationButtonText, Is.EqualTo("Start"));
  }

  [Test]
  [Category("HappyPath")]
  public void ApplicationButtonImage_CanBeSetAndRetrieved() {
    using var icon = new Bitmap(16, 16);
    this._ribbon.ApplicationButtonImage = icon;
    Assert.That(this._ribbon.ApplicationButtonImage, Is.EqualTo(icon));
  }

  [Test]
  [Category("HappyPath")]
  public void ApplicationMenu_CanBeSetAndRetrieved() {
    using var menu = new ContextMenuStrip();
    this._ribbon.ApplicationMenu = menu;
    Assert.That(this._ribbon.ApplicationMenu, Is.EqualTo(menu));
  }

  [Test]
  [Category("HappyPath")]
  public void AllowMinimize_CanBeSetAndRetrieved() {
    this._ribbon.AllowMinimize = false;
    Assert.That(this._ribbon.AllowMinimize, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Minimize_DoesNothing_WhenAllowMinimizeFalse() {
    this._ribbon.AllowMinimize = false;
    this._ribbon.Minimize();

    Assert.That(this._ribbon.Minimized, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void QuickAccessToolbar_AddButton_CreatesButton() {
    using var icon = new Bitmap(16, 16);
    var button = this._ribbon.QuickAccessToolbar.AddButton("Save", icon);

    Assert.That(this._ribbon.QuickAccessToolbar.Items.Count, Is.EqualTo(1));
    Assert.That(button.Text, Is.EqualTo("Save"));
  }

  [Test]
  [Category("HappyPath")]
  public void Tab_IsVisible_CanBeSetAndRetrieved() {
    var tab = this._ribbon.AddTab("Home");
    tab.IsVisible = false;

    Assert.That(tab.IsVisible, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Tab_IsContextual_CanBeSetAndRetrieved() {
    var tab = this._ribbon.AddTab("Picture Tools");
    tab.IsContextual = true;
    tab.ContextualColor = Color.Orange;

    Assert.That(tab.IsContextual, Is.True);
    Assert.That(tab.ContextualColor, Is.EqualTo(Color.Orange));
  }

  [Test]
  [Category("HappyPath")]
  public void Group_ShowDialogLauncher_CanBeSetAndRetrieved() {
    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Font");
    group.ShowDialogLauncher = true;

    Assert.That(group.ShowDialogLauncher, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void RibbonItem_Properties_Work() {
    using var smallIcon = new Bitmap(16, 16);
    using var largeIcon = new Bitmap(32, 32);

    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    var button = group.AddButton("Test", smallIcon);

    button.LargeImage = largeIcon;
    button.ToolTipText = "Test Tooltip";
    button.Enabled = false;
    button.Visible = false;
    button.Tag = "test tag";
    button.Checked = true;
    button.ShortcutKeys = Keys.Control | Keys.T;

    Assert.That(button.SmallImage, Is.EqualTo(smallIcon));
    Assert.That(button.LargeImage, Is.EqualTo(largeIcon));
    Assert.That(button.ToolTipText, Is.EqualTo("Test Tooltip"));
    Assert.That(button.Enabled, Is.False);
    Assert.That(button.Visible, Is.False);
    Assert.That(button.Tag, Is.EqualTo("test tag"));
    Assert.That(button.Checked, Is.True);
    Assert.That(button.ShortcutKeys, Is.EqualTo(Keys.Control | Keys.T));
  }

  [Test]
  [Category("HappyPath")]
  public void SplitButton_DropDownMenu_CanBeSet() {
    using var icon = new Bitmap(16, 16);
    using var menu = new ContextMenuStrip();

    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Clipboard");
    var button = group.AddSplitButton("Paste", icon);
    button.DropDownMenu = menu;

    Assert.That(button.DropDownMenu, Is.EqualTo(menu));
  }

  [Test]
  [Category("HappyPath")]
  public void DropDownButton_DropDownMenu_CanBeSet() {
    using var icon = new Bitmap(16, 16);
    using var menu = new ContextMenuStrip();

    var tab = this._ribbon.AddTab("Home");
    var group = tab.AddGroup("Options");
    var button = group.AddDropDownButton("Settings", icon);
    button.DropDownMenu = menu;

    Assert.That(button.DropDownMenu, Is.EqualTo(menu));
  }
}
