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
public class ListViewExtensionsTests {
  private ListView _listView;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._listView = new ListView { View = View.Details };
    this._form = new Form();
    this._form.Controls.Add(this._listView);
    var _ = this._form.Handle;
    _ = this._listView.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._listView?.Dispose();
    this._form?.Dispose();
  }

  #region PauseUpdates Tests

  [Test]
  [Category("HappyPath")]
  public void PauseUpdates_ReturnsToken_AndDisposingEndsUpdate() {
    using (var token = this._listView.PauseUpdates()) {
      Assert.That(token, Is.Not.Null);
      Assert.That(token, Is.InstanceOf<ListViewExtensions.ISuspendedUpdateToken>());
    }
  }

  [Test]
  [Category("Exception")]
  public void PauseUpdates_NullListView_ThrowsNullReferenceException() {
    ListView listView = null;
    Assert.Throws<NullReferenceException>(() => listView.PauseUpdates());
  }

  #endregion

  #region EnableExtendedAttributes Tests

  [Test]
  [Category("HappyPath")]
  public void EnableExtendedAttributes_SetsOwnerDrawTrue() {
    this._listView.EnableExtendedAttributes();
    Assert.That(this._listView.OwnerDraw, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void EnableExtendedAttributes_CalledMultipleTimes_DoesNotThrow() {
    Assert.DoesNotThrow(() => {
      this._listView.EnableExtendedAttributes();
      this._listView.EnableExtendedAttributes();
      this._listView.EnableExtendedAttributes();
    });
    Assert.That(this._listView.OwnerDraw, Is.True);
  }

  #endregion

  #region ConfigureColumnsFromType Tests

  [Test]
  [Category("HappyPath")]
  public void ConfigureColumnsFromType_CreatesColumnsFromAttributes() {
    this._listView.ConfigureColumnsFromType<TestListItem>();

    Assert.That(this._listView.Columns.Count, Is.EqualTo(3));
    Assert.That(this._listView.Columns[0].Text, Is.EqualTo("Name"));
    Assert.That(this._listView.Columns[1].Text, Is.EqualTo("Value"));
    Assert.That(this._listView.Columns[2].Text, Is.EqualTo("Active"));
  }

  [Test]
  [Category("HappyPath")]
  public void ConfigureColumnsFromType_RespectsColumnWidth() {
    this._listView.ConfigureColumnsFromType<TestListItem>();

    Assert.That(this._listView.Columns[0].Width, Is.EqualTo(150));
    Assert.That(this._listView.Columns[1].Width, Is.EqualTo(80));
  }

  [Test]
  [Category("HappyPath")]
  public void ConfigureColumnsFromType_RespectsColumnAlignment() {
    this._listView.ConfigureColumnsFromType<TestListItem>();

    Assert.That(this._listView.Columns[1].TextAlign, Is.EqualTo(HorizontalAlignment.Right));
  }

  #endregion

  #region SetDataSource/GetDataSource Tests

  [Test]
  [Category("HappyPath")]
  public void SetDataSource_CreatesItemsFromData() {
    var items = new List<TestListItem> {
      new() { Name = "Item1", Value = 10, IsActive = true },
      new() { Name = "Item2", Value = 20, IsActive = false }
    };

    this._listView.SetDataSource(items);

    Assert.That(this._listView.Items.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void SetDataSource_ConfiguresColumnsIfNotPresent() {
    var items = new List<TestListItem> {
      new() { Name = "Test", Value = 42, IsActive = true }
    };

    this._listView.SetDataSource(items);

    Assert.That(this._listView.Columns.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void SetDataSource_StoresDataInTag() {
    var items = new List<TestListItem> {
      new() { Name = "Test", Value = 42, IsActive = true }
    };

    this._listView.SetDataSource(items);

    Assert.That(this._listView.Items[0].Tag, Is.EqualTo(items[0]));
  }

  [Test]
  [Category("HappyPath")]
  public void GetDataSource_ReturnsSetData() {
    var items = new List<TestListItem> {
      new() { Name = "Test", Value = 42, IsActive = true }
    };

    this._listView.SetDataSource(items);
    var result = this._listView.GetDataSource();

    Assert.That(result, Is.SameAs(items));
  }

  [Test]
  [Category("HappyPath")]
  public void SetDataSource_Null_ClearsItems() {
    var items = new List<TestListItem> {
      new() { Name = "Test", Value = 42, IsActive = true }
    };
    this._listView.SetDataSource(items);

    this._listView.SetDataSource(null);

    Assert.That(this._listView.Items.Count, Is.EqualTo(0));
    Assert.That(this._listView.GetDataSource(), Is.Null);
  }

  #endregion

  #region Selection Tests

  [Test]
  [Category("HappyPath")]
  public void SelectAll_SelectsAllItems() {
    this._listView.Items.Add("Item1");
    this._listView.Items.Add("Item2");
    this._listView.Items.Add("Item3");

    this._listView.SelectAll();

    Assert.That(this._listView.SelectedItems.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectNone_DeselectsAllItems() {
    this._listView.Items.Add("Item1").Selected = true;
    this._listView.Items.Add("Item2").Selected = true;

    this._listView.SelectNone();

    Assert.That(this._listView.SelectedItems.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void InvertSelection_InvertsCurrentSelection() {
    this._listView.Items.Add("Item1").Selected = true;
    this._listView.Items.Add("Item2");
    this._listView.Items.Add("Item3").Selected = true;

    this._listView.InvertSelection();

    Assert.That(this._listView.Items[0].Selected, Is.False);
    Assert.That(this._listView.Items[1].Selected, Is.True);
    Assert.That(this._listView.Items[2].Selected, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SelectWhere_SelectsMatchingItems() {
    this._listView.Items.Add("Apple");
    this._listView.Items.Add("Banana");
    this._listView.Items.Add("Apricot");

    this._listView.SelectWhere(item => item.Text.StartsWith("A"));

    Assert.That(this._listView.Items[0].Selected, Is.True);
    Assert.That(this._listView.Items[1].Selected, Is.False);
    Assert.That(this._listView.Items[2].Selected, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedItems_ReturnsTypedItems() {
    var items = new List<TestListItem> {
      new() { Name = "Item1", Value = 10, IsActive = true },
      new() { Name = "Item2", Value = 20, IsActive = false }
    };
    this._listView.SetDataSource(items);
    this._listView.Items[0].Selected = true;

    var selected = this._listView.GetSelectedItems<TestListItem>().ToList();

    Assert.That(selected.Count, Is.EqualTo(1));
    Assert.That(selected[0].Name, Is.EqualTo("Item1"));
  }

  #endregion

  #region CheckAll/UncheckAll Tests

  [Test]
  [Category("HappyPath")]
  public void CheckAll_ChecksAllItems() {
    this._listView.CheckBoxes = true;
    this._listView.Items.Add("Item1");
    this._listView.Items.Add("Item2");

    this._listView.CheckAll();

    Assert.That(this._listView.Items[0].Checked, Is.True);
    Assert.That(this._listView.Items[1].Checked, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void UncheckAll_UnchecksAllItems() {
    this._listView.CheckBoxes = true;
    this._listView.Items.Add("Item1").Checked = true;
    this._listView.Items.Add("Item2").Checked = true;

    this._listView.UncheckAll();

    Assert.That(this._listView.Items[0].Checked, Is.False);
    Assert.That(this._listView.Items[1].Checked, Is.False);
  }

  #endregion

  #region Filtering Tests

  [Test]
  [Category("HappyPath")]
  public void Filter_HidesNonMatchingItems() {
    this._listView.Items.Add("Apple");
    this._listView.Items.Add("Banana");
    this._listView.Items.Add("Apricot");

    this._listView.Filter(item => item.Text.StartsWith("A"));

    Assert.That(this._listView.Items.Count, Is.EqualTo(2));
    Assert.That(this._listView.Items[0].Text, Is.EqualTo("Apple"));
    Assert.That(this._listView.Items[1].Text, Is.EqualTo("Apricot"));
  }

  [Test]
  [Category("HappyPath")]
  public void ClearFilter_RestoresAllItems() {
    this._listView.Items.Add("Apple");
    this._listView.Items.Add("Banana");
    this._listView.Items.Add("Apricot");
    this._listView.Filter(item => item.Text.StartsWith("A"));

    this._listView.ClearFilter();

    Assert.That(this._listView.Items.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void FilterByText_SearchesCaseInsensitive() {
    this._listView.Items.Add("Apple");
    this._listView.Items.Add("Banana");
    this._listView.Items.Add("APRICOT");

    this._listView.FilterByText("ap");

    Assert.That(this._listView.Items.Count, Is.EqualTo(2));
  }

  #endregion

  #region RemoveWhere Tests

  [Test]
  [Category("HappyPath")]
  public void RemoveWhere_RemovesMatchingItems() {
    this._listView.Items.Add("Apple");
    this._listView.Items.Add("Banana");
    this._listView.Items.Add("Apricot");

    var removed = this._listView.RemoveWhere(item => item.Text.StartsWith("A"));

    Assert.That(removed, Is.EqualTo(2));
    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(this._listView.Items[0].Text, Is.EqualTo("Banana"));
  }

  #endregion

  #region AddItem Tests

  [Test]
  [Category("HappyPath")]
  public void AddItem_WithText_AddsItem() {
    this._listView.AddItem("Test");

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(this._listView.Items[0].Text, Is.EqualTo("Test"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddItem_WithSubItems_AddsAllSubItems() {
    this._listView.Columns.Add("Col1");
    this._listView.Columns.Add("Col2");
    this._listView.Columns.Add("Col3");

    this._listView.AddItem("Main", "Sub1", "Sub2");

    Assert.That(this._listView.Items[0].SubItems.Count, Is.EqualTo(3));
    Assert.That(this._listView.Items[0].SubItems[1].Text, Is.EqualTo("Sub1"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddItem_WithDataObject_StoresInTag() {
    this._listView.ConfigureColumnsFromType<TestListItem>();
    var item = new TestListItem { Name = "Test", Value = 42, IsActive = true };

    this._listView.AddItem(item);

    Assert.That(this._listView.Items[0].Tag, Is.SameAs(item));
    Assert.That(this._listView.Items[0].Text, Is.EqualTo("Test"));
  }

  #endregion

  #region Sorting Tests

  [Test]
  [Category("HappyPath")]
  public void SortByColumn_SortsAscending() {
    this._listView.Columns.Add("Name");
    this._listView.Items.Add("Banana");
    this._listView.Items.Add("Apple");
    this._listView.Items.Add("Cherry");

    this._listView.SortByColumn(0, ascending: true);

    Assert.That(this._listView.Items[0].Text, Is.EqualTo("Apple"));
    Assert.That(this._listView.Items[1].Text, Is.EqualTo("Banana"));
    Assert.That(this._listView.Items[2].Text, Is.EqualTo("Cherry"));
  }

  [Test]
  [Category("HappyPath")]
  public void SortByColumn_SortsDescending() {
    this._listView.Columns.Add("Name");
    this._listView.Items.Add("Banana");
    this._listView.Items.Add("Apple");
    this._listView.Items.Add("Cherry");

    this._listView.SortByColumn(0, ascending: false);

    Assert.That(this._listView.Items[0].Text, Is.EqualTo("Cherry"));
    Assert.That(this._listView.Items[1].Text, Is.EqualTo("Banana"));
    Assert.That(this._listView.Items[2].Text, Is.EqualTo("Apple"));
  }

  #endregion

  #region Utility Tests

  [Test]
  [Category("HappyPath")]
  public void EnableDoubleBuffering_DoesNotThrow() {
    Assert.DoesNotThrow(() => this._listView.EnableDoubleBuffering());
  }

  [Test]
  [Category("HappyPath")]
  public void ClearAndDispose_ClearsItems() {
    this._listView.Items.Add("Item1");
    this._listView.Items.Add("Item2");

    this._listView.ClearAndDispose();

    Assert.That(this._listView.Items.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GetAllItems_ReturnsAllItems() {
    this._listView.Items.Add("Item1");
    this._listView.Items.Add("Item2");
    this._listView.Items.Add("Item3");

    var items = this._listView.GetAllItems().ToList();

    Assert.That(items.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void GetBoundData_ReturnsTypedData() {
    var items = new List<TestListItem> {
      new() { Name = "Item1", Value = 10, IsActive = true },
      new() { Name = "Item2", Value = 20, IsActive = false }
    };
    this._listView.SetDataSource(items);

    var boundData = this._listView.GetBoundData<TestListItem>().ToList();

    Assert.That(boundData.Count, Is.EqualTo(2));
    Assert.That(boundData[0].Name, Is.EqualTo("Item1"));
  }

  #endregion

  #region ListViewRepeatedImage Tests

  [Test]
  [Category("HappyPath")]
  public void RepeatedImage_IntegerValue_ShowsCorrectCount() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItem>();

    var imageList = new ImageList();
    imageList.Images.Add("star", new Bitmap(16, 16));

    var item = new RatedItem { Name = "Test", Rating = 3, Images = imageList };
    this._listView.SetDataSource(new[] { item });

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(this._listView.Items[0].Tag, Is.EqualTo(item));
  }

  [Test]
  [Category("HappyPath")]
  public void RepeatedImage_FloatValue_PreservesFractionalPart() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItemFloat>();

    var imageList = new ImageList();
    imageList.Images.Add("star", new Bitmap(16, 16));

    var item = new RatedItemFloat { Name = "Test", Rating = 3.5f, Images = imageList };
    this._listView.SetDataSource(new[] { item });

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(((RatedItemFloat)this._listView.Items[0].Tag).Rating, Is.EqualTo(3.5f));
  }

  [Test]
  [Category("HappyPath")]
  public void RepeatedImage_DoubleValue_PreservesFractionalPart() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItemDouble>();

    var imageList = new ImageList();
    imageList.Images.Add("star", new Bitmap(16, 16));

    var item = new RatedItemDouble { Name = "Test", Rating = 4.75, Images = imageList };
    this._listView.SetDataSource(new[] { item });

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(((RatedItemDouble)this._listView.Items[0].Tag).Rating, Is.EqualTo(4.75));
  }

  [Test]
  [Category("HappyPath")]
  public void RepeatedImage_NegativeValue_StoresNegativeValue() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItemDouble>();

    var imageList = new ImageList();
    imageList.Images.Add("star", new Bitmap(16, 16));

    var item = new RatedItemDouble { Name = "Bad Product", Rating = -2.5, Images = imageList };
    this._listView.SetDataSource(new[] { item });

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(((RatedItemDouble)this._listView.Items[0].Tag).Rating, Is.EqualTo(-2.5));
  }

  [Test]
  [Category("HappyPath")]
  public void RepeatedImage_ZeroValue_ShowsNoImages() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItem>();

    var imageList = new ImageList();
    imageList.Images.Add("star", new Bitmap(16, 16));

    var item = new RatedItem { Name = "No Rating", Rating = 0, Images = imageList };
    this._listView.SetDataSource(new[] { item });

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(((RatedItem)this._listView.Items[0].Tag).Rating, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void RepeatedImage_NullImageList_DoesNotThrow() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItem>();

    var item = new RatedItem { Name = "No Images", Rating = 3, Images = null };
    Assert.DoesNotThrow(() => this._listView.SetDataSource(new[] { item }));
    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void RepeatedImage_ValueExceedsMaxCount_ClampedToMax() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItem>();

    var imageList = new ImageList();
    imageList.Images.Add("star", new Bitmap(16, 16));

    var item = new RatedItem { Name = "Overrated", Rating = 10, Images = imageList };
    this._listView.SetDataSource(new[] { item });

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void RepeatedImage_DecimalValue_SupportsDecimalType() {
    this._listView.EnableExtendedAttributes();
    this._listView.ConfigureColumnsFromType<RatedItemDecimal>();

    var imageList = new ImageList();
    imageList.Images.Add("star", new Bitmap(16, 16));

    var item = new RatedItemDecimal { Name = "Decimal Rating", Rating = 3.25m, Images = imageList };
    this._listView.SetDataSource(new[] { item });

    Assert.That(this._listView.Items.Count, Is.EqualTo(1));
    Assert.That(((RatedItemDecimal)this._listView.Items[0].Tag).Rating, Is.EqualTo(3.25m));
  }

  #endregion

  #region Test Data Classes

  [ListItemStyle(foreColor: "Red", conditionalPropertyName: nameof(IsActive))]
  private class TestListItem {
    [ListViewColumn("Name", Width = 150)]
    public string Name { get; set; }

    [ListViewColumn("Value", Width = 80, Alignment = HorizontalAlignment.Right)]
    public int Value { get; set; }

    [ListViewColumn("Active")]
    [ListViewColumnColor(foreColor: "Green", conditionalPropertyName: nameof(IsActive))]
    public bool IsActive { get; set; }
  }

  private class RatedItem {
    [ListViewColumn("Name", Width = 150)]
    public string Name { get; set; }

    [ListViewColumn("Rating")]
    [ListViewRepeatedImage(nameof(Images), "star", 5)]
    public int Rating { get; set; }

    public ImageList Images { get; set; }
  }

  private class RatedItemFloat {
    [ListViewColumn("Name", Width = 150)]
    public string Name { get; set; }

    [ListViewColumn("Rating")]
    [ListViewRepeatedImage(nameof(Images), "star", 5)]
    public float Rating { get; set; }

    public ImageList Images { get; set; }
  }

  private class RatedItemDouble {
    [ListViewColumn("Name", Width = 150)]
    public string Name { get; set; }

    [ListViewColumn("Rating")]
    [ListViewRepeatedImage(nameof(Images), "star", 5)]
    public double Rating { get; set; }

    public ImageList Images { get; set; }
  }

  private class RatedItemDecimal {
    [ListViewColumn("Name", Width = 150)]
    public string Name { get; set; }

    [ListViewColumn("Rating")]
    [ListViewRepeatedImage(nameof(Images), "star", 5)]
    public decimal Rating { get; set; }

    public ImageList Images { get; set; }
  }

  #endregion
}
