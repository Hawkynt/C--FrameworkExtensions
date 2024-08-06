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

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms;

public partial class DataGridViewBoundComboBoxColumn {
  private sealed class DataGridViewBoundComboBoxCell(
    string dataSourcePropertyName,
    string enabledWhenPropertyName,
    string valueMember,
    string displayMember
  ) : DataGridViewTextBoxCell {
    private string _DataSourcePropertyName { get; set; } = dataSourcePropertyName;
    private string _EnabledWhenPropertyName { get; set; } = enabledWhenPropertyName;
    private string _ValueMember { get; set; } = valueMember;
    private string _DisplayMember { get; set; } = displayMember;

    public override Type EditType => typeof(DataGridViewComboBoxEditingControl);

    public override Type ValueType { get; set; }

    public override object ParseFormattedValue(
      object formattedValue,
      DataGridViewCellStyle cellStyle,
      TypeConverter formattedValueTypeConverter,
      TypeConverter valueTypeConverter
    ) =>
      Convert.ChangeType(formattedValue, this.ValueType);

    public override void InitializeEditingControl(
      int rowIndex,
      object initialFormattedValue,
      DataGridViewCellStyle dataGridViewCellStyle
    ) {
      base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
      if (this.DataGridView?.EditingControl is not ComboBox comboBox)
        return;

      var owningRowDataBoundItem = this.OwningRow.DataBoundItem;
      var source = DataGridViewExtensions.GetPropertyValueOrDefault<IEnumerable>(
        owningRowDataBoundItem,
        this._DataSourcePropertyName,
        null,
        null,
        null,
        null
      );
      comboBox.DataSource = source;


      if (this._EnabledWhenPropertyName != null)
        comboBox.Enabled = DataGridViewExtensions.GetPropertyValueOrDefault(
          owningRowDataBoundItem,
          this._EnabledWhenPropertyName,
          true,
          true,
          true,
          true
        );

      if (source == null)
        return;

      if (this._DisplayMember != null)
        comboBox.DisplayMember = this._DisplayMember;

      if (this._ValueMember != null)
        comboBox.ValueMember = this._ValueMember;


      comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBox.SelectedIndex = 0;

      this.ValueType = source.GetType().GetElementType();
    }

    public override object Clone() {
      var result = (DataGridViewBoundComboBoxCell)base.Clone();
      result._DataSourcePropertyName = this._DataSourcePropertyName;
      result._EnabledWhenPropertyName = this._EnabledWhenPropertyName;
      result._ValueMember = this._ValueMember;
      result._DisplayMember = this._DisplayMember;
      return result;
    }
  }
}
