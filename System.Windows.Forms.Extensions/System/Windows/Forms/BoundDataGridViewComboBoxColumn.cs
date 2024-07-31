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

namespace System.Windows.Forms;

/// <summary>
/// Represents a data-bound <see cref="DataGridViewComboBoxColumn"/> with additional properties for specifying the data source, enabling condition, value member, and display member.
/// </summary>
public partial class BoundDataGridViewComboBoxColumn : DataGridViewColumn {
  
  /// <summary>
  /// Gets the name of the property to use as the data source for the combo box items.
  /// </summary>
  public string DataSourcePropertyName { get; }

  /// <summary>
  /// Gets the name of the property that determines when the combo box is enabled.
  /// </summary>
  public string EnabledWhenPropertyName { get; }

  /// <summary>
  /// Gets the name of the property to use as the value member for the combo box items.
  /// </summary>
  public string ValueMember { get; }

  /// <summary>
  /// Gets the name of the property to use as the display member for the combo box items.
  /// </summary>
  public string DisplayMember { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="BoundDataGridViewComboBoxColumn"/> class with the specified properties.
  /// </summary>
  /// <param name="dataSourcePropertyName">The name of the property to use as the data source for the combo box items.</param>
  /// <param name="enabledWhenPropertyName">The name of the property that determines when the combo box is enabled.</param>
  /// <param name="valueMember">The name of the property to use as the value member for the combo box items.</param>
  /// <param name="displayMember">The name of the property to use as the display member for the combo box items.</param>
  /// <example>
  /// <code>
  /// // Define a record type for the combo box items
  /// public readonly record ComboBoxItem(string DisplayText, int Value);
  /// 
  /// // Define a record type for the data grid view rows
  /// public record DataRow(int Id, string Name, bool IsEnabled, ComboBoxItem[] ComboBoxItems, int SelectedValue);
  /// 
  /// // Create an array of ComboBoxItem instances
  /// ComboBoxItem[] comboBoxItems = [
  ///     new ("Option 1", 1),
  ///     new ("Option 2", 2)
  /// ];
  /// 
  /// // Create an array of DataRow instances
  /// DataRow[] dataRows = [
  ///     new (1, "Row 1", true, comboBoxItems, 1),
  ///     new (2, "Row 2", false, comboBoxItems, 2)
  /// ];
  /// 
  /// // Create a DataGridView and set its data source
  /// var dataGridView = new DataGridView
  /// {
  ///     DataSource = dataRows
  /// };
  /// 
  /// // Create a BoundDataGridViewComboBoxColumn and set its properties
  /// var comboBoxColumn = new BoundDataGridViewComboBoxColumn(
  ///     nameof(DataRow.ComboBoxItems),
  ///     nameof(DataRow.IsEnabled),
  ///     nameof(ComboBoxItem.Value),
  ///     nameof(ComboBoxItem.DisplayText)
  /// )
  /// {
  ///     DataPropertyName = nameof(DataRow.SelectedValue),
  ///     IsDataBound = true
  /// };
  /// 
  /// dataGridView.Columns.Add(comboBoxColumn);
  /// </code>
  /// This example demonstrates how to create a <see cref="BoundDataGridViewComboBoxColumn"/> and add it to a <see cref="DataGridView"/>.
  /// It uses a custom `ComboBoxItem` record for the combo box items and a custom `DataRow` record for the data grid view rows, 
  /// with properties referenced using the <see langword="nameof"/> operator. The `DataPropertyName` is set to the property that carries the selected value.
  /// </example>
  public BoundDataGridViewComboBoxColumn(
    string dataSourcePropertyName,
    string enabledWhenPropertyName,
    string valueMember,
    string displayMember
  ) {
    this.DataSourcePropertyName = dataSourcePropertyName;
    this.EnabledWhenPropertyName = enabledWhenPropertyName;
    this.ValueMember = valueMember;
    this.DisplayMember = displayMember;

    var cell = new BoundDataGridViewComboBoxCell(
      dataSourcePropertyName,
      enabledWhenPropertyName,
      valueMember,
      displayMember
    );
    
    // ReSharper disable once VirtualMemberCallInConstructor
    this.CellTemplate = cell;
  }

  #region Overrides of DataGridViewColumn

  public override object Clone() {
    var result = new BoundDataGridViewComboBoxColumn(
      this.DataSourcePropertyName,
      this.EnabledWhenPropertyName,
      this.ValueMember,
      this.DisplayMember
    ) { 
      Name = this.Name, 
      DisplayIndex = this.DisplayIndex, 
      HeaderText = this.HeaderText, 
      DataPropertyName = this.DataPropertyName, 
      AutoSizeMode = this.AutoSizeMode, 
      SortMode = this.SortMode, 
      FillWeight = this.FillWeight
    };
    return result;
  }

  #endregion

  #region Overrides of DataGridViewBand

  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      if (value is not BoundDataGridViewComboBoxCell)
        throw new InvalidCastException(nameof(BoundDataGridViewComboBoxCell));

      base.CellTemplate = value;
    }
  }

  #endregion
}
