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

public partial class BoundDataGridViewComboBoxColumn : DataGridViewColumn {
  public string DataSourcePropertyName { get; }
  public string EnabledWhenPropertyName { get; }
  public string ValueMember { get; }
  public string DisplayMember { get; }

  public BoundDataGridViewComboBoxColumn(
    string dataSourcePropertyName,
    string enabledWhenPropertyName,
    string valueMember,
    string displayMember
  ) {
    var cell = new BoundDataGridViewComboBoxCell(
      dataSourcePropertyName,
      enabledWhenPropertyName,
      valueMember,
      displayMember
    );

    this.DataSourcePropertyName = dataSourcePropertyName;
    this.EnabledWhenPropertyName = enabledWhenPropertyName;
    this.ValueMember = valueMember;
    this.DisplayMember = displayMember;

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
      // Ensure that the cell used for the template is a BoundDataGridViewComboBoxCell.
      if (value != null && !value.GetType().IsAssignableFrom(typeof(BoundDataGridViewComboBoxCell)))
        throw new InvalidCastException(nameof(BoundDataGridViewComboBoxCell));

      base.CellTemplate = value;
    }
  }

  #endregion
}
