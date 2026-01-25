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
public class ComboBoxExtensionsTests {
  private ComboBox _comboBox;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._comboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    this._form = new Form();
    this._form.Controls.Add(this._comboBox);
    var _ = this._form.Handle;
    _ = this._comboBox.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._comboBox?.Dispose();
    this._form?.Dispose();
  }

  #region PauseUpdates Tests

  [Test]
  [Category("HappyPath")]
  public void PauseUpdates_ReturnsToken_AndDisposingEndsUpdate() {
    using (var token = this._comboBox.PauseUpdates()) {
      Assert.That(token, Is.Not.Null);
      Assert.That(token, Is.InstanceOf<ComboBoxExtensions.ISuspendedUpdateToken>());
    }
  }

  [Test]
  [Category("Exception")]
  public void PauseUpdates_NullComboBox_ThrowsNullReferenceException() {
    ComboBox comboBox = null;
    Assert.Throws<NullReferenceException>(() => comboBox.PauseUpdates());
  }

  #endregion

  #region EnableExtendedAttributes Tests

  [Test]
  [Category("HappyPath")]
  public void EnableExtendedAttributes_SetsOwnerDrawFixed() {
    this._comboBox.EnableExtendedAttributes();
    Assert.That(this._comboBox.DrawMode, Is.EqualTo(DrawMode.OwnerDrawFixed));
  }

  [Test]
  [Category("HappyPath")]
  public void EnableExtendedAttributes_CalledMultipleTimes_DoesNotThrow() {
    Assert.DoesNotThrow(() => {
      this._comboBox.EnableExtendedAttributes();
      this._comboBox.EnableExtendedAttributes();
      this._comboBox.EnableExtendedAttributes();
    });
    Assert.That(this._comboBox.DrawMode, Is.EqualTo(DrawMode.OwnerDrawFixed));
  }

  #endregion

  #region Selection Tests

  [Test]
  [Category("HappyPath")]
  public void SelectNone_DeselectsCurrentItem() {
    this._comboBox.Items.Add("Item1");
    this._comboBox.Items.Add("Item2");
    this._comboBox.SelectedIndex = 1;

    this._comboBox.SelectNone();

    Assert.That(this._comboBox.SelectedIndex, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectWhere_SelectsFirstMatchingItem() {
    this._comboBox.Items.Add("Apple");
    this._comboBox.Items.Add("Banana");
    this._comboBox.Items.Add("Apricot");

    this._comboBox.SelectWhere(item => item.ToString().StartsWith("B"));

    Assert.That(this._comboBox.SelectedIndex, Is.EqualTo(1));
    Assert.That(this._comboBox.SelectedItem, Is.EqualTo("Banana"));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectWhere_NoMatch_DeselectsAll() {
    this._comboBox.Items.Add("Apple");
    this._comboBox.Items.Add("Banana");
    this._comboBox.SelectedIndex = 0;

    this._comboBox.SelectWhere(item => item.ToString().StartsWith("Z"));

    Assert.That(this._comboBox.SelectedIndex, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedItem_ReturnsSingleItem() {
    var items = new List<TestComboBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };
    foreach (var item in items)
      this._comboBox.Items.Add(item);
    this._comboBox.SelectedIndex = 1;

    var selected = this._comboBox.GetSelectedItem<TestComboBoxItem>();

    Assert.That(selected, Is.Not.Null);
    Assert.That(selected.Name, Is.EqualTo("Item2"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedItem_NoSelection_ReturnsDefault() {
    this._comboBox.Items.Add(new TestComboBoxItem { Name = "Item1", Value = 10 });

    var selected = this._comboBox.GetSelectedItem<TestComboBoxItem>();

    Assert.That(selected, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedItem_WrongType_ReturnsDefault() {
    this._comboBox.Items.Add("StringItem");
    this._comboBox.SelectedIndex = 0;

    var selected = this._comboBox.GetSelectedItem<TestComboBoxItem>();

    Assert.That(selected, Is.Null);
  }

  #endregion

  #region Utility Tests

  [Test]
  [Category("HappyPath")]
  public void EnableDoubleBuffering_DoesNotThrow() {
    Assert.DoesNotThrow(() => this._comboBox.EnableDoubleBuffering());
  }

  [Test]
  [Category("HappyPath")]
  public void GetBoundData_ReturnsTypedData() {
    var items = new List<TestComboBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };
    foreach (var item in items)
      this._comboBox.Items.Add(item);

    var boundData = this._comboBox.GetBoundData<TestComboBoxItem>().ToList();

    Assert.That(boundData.Count, Is.EqualTo(2));
    Assert.That(boundData[0].Name, Is.EqualTo("Item1"));
  }

  [Test]
  [Category("HappyPath")]
  public void AutoAdjustWidth_AdjustsWidthToFitContent() {
    this._comboBox.Items.Add("Short");
    this._comboBox.Items.Add("A much longer item text");

    var initialWidth = this._comboBox.Width;
    this._comboBox.AutoAdjustWidth();

    Assert.That(this._comboBox.Width, Is.GreaterThan(0));
  }

  #endregion

  #region Existing Extension Method Tests

  [Test]
  [Category("HappyPath")]
  public void DataSource_SetsDataSource() {
    var items = new List<TestComboBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };

    this._comboBox.DataSource(items, nameof(TestComboBoxItem.Name), nameof(TestComboBoxItem.Value));

    Assert.That(this._comboBox.Items.Count, Is.EqualTo(2));
    Assert.That(this._comboBox.SelectedIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SetSelectedItem_SelectsByPredicate() {
    var items = new List<TestComboBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };
    this._comboBox.DataSource(items, nameof(TestComboBoxItem.Name), nameof(TestComboBoxItem.Value));

    this._comboBox.SetSelectedItem<TestComboBoxItem>(i => i.Value == 20);

    var selected = this._comboBox.SelectedItem as TestComboBoxItem;
    Assert.That(selected, Is.Not.Null);
    Assert.That(selected.Value, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetSelectedItem_ReturnsTrue_WhenItemSelected() {
    var items = new List<TestComboBoxItem> {
      new() { Name = "Item1", Value = 10 },
      new() { Name = "Item2", Value = 20 }
    };
    foreach (var item in items)
      this._comboBox.Items.Add(item);
    this._comboBox.SelectedIndex = 1;

    var result = this._comboBox.TryGetSelectedItem<TestComboBoxItem>(out var selected);

    Assert.That(result, Is.True);
    Assert.That(selected.Name, Is.EqualTo("Item2"));
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetSelectedItem_ReturnsFalse_WhenNoSelection() {
    this._comboBox.Items.Add(new TestComboBoxItem { Name = "Item1", Value = 10 });

    var result = this._comboBox.TryGetSelectedItem<TestComboBoxItem>(out var selected);

    Assert.That(result, Is.False);
    Assert.That(selected, Is.Null);
  }

  #endregion

  #region Test Data Classes

  [ListItemStyle(foreColor: "Purple", conditionalPropertyName: nameof(IsSpecial))]
  private class TestComboBoxItem {
    public string Name { get; set; }
    public int Value { get; set; }
    public bool IsSpecial { get; set; }

    public override string ToString() => this.Name;
  }

  #endregion
}
