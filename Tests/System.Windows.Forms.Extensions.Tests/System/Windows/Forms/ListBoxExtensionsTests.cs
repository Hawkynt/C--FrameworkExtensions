#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ListBoxExtensionsTests {
  private ListBox _listBox;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._listBox = new ListBox { SelectionMode = SelectionMode.MultiExtended };
    this._form = new Form();
    this._form.Controls.Add(this._listBox);
    var _ = this._form.Handle;
    _ = this._listBox.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._listBox?.Dispose();
    this._form?.Dispose();
  }

  #region PauseUpdates Tests

  [Test]
  [Category("HappyPath")]
  public void PauseUpdates_ReturnsToken_AndDisposingEndsUpdate() {
    using (var token = this._listBox.PauseUpdates()) {
      Assert.That(token, Is.Not.Null);
      Assert.That(token, Is.InstanceOf<ListBoxExtensions.ISuspendedUpdateToken>());
    }
  }

  [Test]
  [Category("Exception")]
  public void PauseUpdates_NullListBox_ThrowsNullReferenceException() {
    ListBox listBox = null;
    Assert.Throws<NullReferenceException>(() => listBox.PauseUpdates());
  }

  #endregion

  #region EnableExtendedAttributes Tests

  [Test]
  [Category("HappyPath")]
  public void EnableExtendedAttributes_SetsOwnerDrawFixed() {
    this._listBox.EnableExtendedAttributes();
    Assert.That(this._listBox.DrawMode, Is.EqualTo(DrawMode.OwnerDrawFixed));
  }

  [Test]
  [Category("HappyPath")]
  public void EnableExtendedAttributes_CalledMultipleTimes_DoesNotThrow() {
    Assert.DoesNotThrow(() => {
      this._listBox.EnableExtendedAttributes();
      this._listBox.EnableExtendedAttributes();
      this._listBox.EnableExtendedAttributes();
    });
    Assert.That(this._listBox.DrawMode, Is.EqualTo(DrawMode.OwnerDrawFixed));
  }

  #endregion

  #region Selection Tests

  [Test]
  [Category("HappyPath")]
  public void SelectAll_SelectsAllItems() {
    this._listBox.Items.Add("Item1");
    this._listBox.Items.Add("Item2");
    this._listBox.Items.Add("Item3");

    this._listBox.SelectAll();

    Assert.That(this._listBox.SelectedItems.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectAll_SingleSelectionMode_DoesNothing() {
    this._listBox.SelectionMode = SelectionMode.One;
    this._listBox.Items.Add("Item1");
    this._listBox.Items.Add("Item2");

    this._listBox.SelectAll();

    Assert.That(this._listBox.SelectedItems.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectNone_DeselectsAllItems() {
    this._listBox.Items.Add("Item1");
    this._listBox.Items.Add("Item2");
    this._listBox.SetSelected(0, true);
    this._listBox.SetSelected(1, true);

    this._listBox.SelectNone();

    Assert.That(this._listBox.SelectedItems.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectWhere_SelectsMatchingItems() {
    this._listBox.Items.Add("Apple");
    this._listBox.Items.Add("Banana");
    this._listBox.Items.Add("Apricot");

    this._listBox.SelectWhere(item => item.ToString().StartsWith("A"));

    Assert.That(this._listBox.GetSelected(0), Is.True);
    Assert.That(this._listBox.GetSelected(1), Is.False);
    Assert.That(this._listBox.GetSelected(2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedItems_ReturnsTypedItems() {
    var items = new List<TestListBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };
    foreach (var item in items)
      this._listBox.Items.Add(item);
    this._listBox.SetSelected(0, true);

    var selected = this._listBox.GetSelectedItems<TestListBoxItem>().ToList();

    Assert.That(selected.Count, Is.EqualTo(1));
    Assert.That(selected[0].Name, Is.EqualTo("Item1"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedItem_ReturnsSingleItem() {
    this._listBox.SelectionMode = SelectionMode.One;
    var items = new List<TestListBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };
    foreach (var item in items)
      this._listBox.Items.Add(item);
    this._listBox.SelectedIndex = 1;

    var selected = this._listBox.GetSelectedItem<TestListBoxItem>();

    Assert.That(selected, Is.Not.Null);
    Assert.That(selected.Name, Is.EqualTo("Item2"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedItem_NoSelection_ReturnsNull() {
    this._listBox.Items.Add(new TestListBoxItem { Name = "Item1", Value = 10 });

    var selected = this._listBox.GetSelectedItem<TestListBoxItem>();

    Assert.That(selected, Is.Null);
  }

  #endregion

  #region Filtering Tests

  [Test]
  [Category("HappyPath")]
  public void Filter_HidesNonMatchingItems() {
    this._listBox.Items.Add("Apple");
    this._listBox.Items.Add("Banana");
    this._listBox.Items.Add("Apricot");

    this._listBox.Filter(item => item.ToString().StartsWith("A"));

    Assert.That(this._listBox.Items.Count, Is.EqualTo(2));
    Assert.That(this._listBox.Items[0], Is.EqualTo("Apple"));
    Assert.That(this._listBox.Items[1], Is.EqualTo("Apricot"));
  }

  [Test]
  [Category("HappyPath")]
  public void ClearFilter_RestoresAllItems() {
    this._listBox.Items.Add("Apple");
    this._listBox.Items.Add("Banana");
    this._listBox.Items.Add("Apricot");
    this._listBox.Filter(item => item.ToString().StartsWith("A"));

    this._listBox.ClearFilter();

    Assert.That(this._listBox.Items.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Filter_ChainedFilters_AppliesNewFilter() {
    this._listBox.Items.Add("Apple");
    this._listBox.Items.Add("Apricot");
    this._listBox.Items.Add("Avocado");
    this._listBox.Filter(item => item.ToString().StartsWith("A"));

    this._listBox.Filter(item => item.ToString().Length > 5);

    Assert.That(this._listBox.Items.Count, Is.EqualTo(2));
    Assert.That(this._listBox.Items[0], Is.EqualTo("Apricot"));
    Assert.That(this._listBox.Items[1], Is.EqualTo("Avocado"));
  }

  #endregion

  #region ScrollToItem Tests

  [Test]
  [Category("HappyPath")]
  public void ScrollToItem_SetsTopIndex() {
    for (var i = 0; i < 100; ++i)
      this._listBox.Items.Add($"Item{i}");

    var target = this._listBox.Items[50];
    this._listBox.ScrollToItem(target);

    Assert.That(this._listBox.TopIndex, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void ScrollToItem_ItemNotFound_DoesNotChange() {
    this._listBox.Items.Add("Item1");
    this._listBox.Items.Add("Item2");

    this._listBox.ScrollToItem("NotInList");

    Assert.That(this._listBox.TopIndex, Is.EqualTo(0));
  }

  #endregion

  #region Utility Tests

  [Test]
  [Category("HappyPath")]
  public void EnableDoubleBuffering_DoesNotThrow() {
    Assert.DoesNotThrow(() => this._listBox.EnableDoubleBuffering());
  }

  [Test]
  [Category("HappyPath")]
  public void GetBoundData_ReturnsTypedData() {
    var items = new List<TestListBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };
    foreach (var item in items)
      this._listBox.Items.Add(item);

    var boundData = this._listBox.GetBoundData<TestListBoxItem>().ToList();

    Assert.That(boundData.Count, Is.EqualTo(2));
    Assert.That(boundData[0].Name, Is.EqualTo("Item1"));
  }

  #endregion

  #region Test Data Classes

  [ListItemStyle(foreColor: "Blue", conditionalPropertyName: nameof(IsHighlighted))]
  private class TestListBoxItem {
    public string Name { get; set; }
    public int Value { get; set; }
    public bool IsHighlighted { get; set; }

    public override string ToString() => this.Name;
  }

  #endregion
}
