#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms;

public partial class BoundDataGridViewComboBoxColumn {
  internal class BoundDataGridViewComboBoxCell : DataGridViewTextBoxCell {
    public string DataSourcePropertyName { get; set; }
    public string EnabledWhenPropertyName { get; set; }
    public string ValueMember { get; set; }
    public string DisplayMember { get; set; }

    public BoundDataGridViewComboBoxCell() { }

    public BoundDataGridViewComboBoxCell(
      string dataSourcePropertyName,
      string enabledWhenPropertyName,
      string valueMember,
      string displayMember
    ) {
      this.DataSourcePropertyName = dataSourcePropertyName;
      this.EnabledWhenPropertyName = enabledWhenPropertyName;
      this.ValueMember = valueMember;
      this.DisplayMember = displayMember;
    }

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
      if (this.DataGridView.EditingControl is not ComboBox comboBox)
        return;

      var owningRowDataBoundItem = this.OwningRow.DataBoundItem;
      var source = DataGridViewExtensions.GetPropertyValueOrDefault<IEnumerable>(
        owningRowDataBoundItem,
        this.DataSourcePropertyName,
        null,
        null,
        null,
        null
      );
      comboBox.DataSource = source;


      if (this.EnabledWhenPropertyName != null)
        comboBox.Enabled = DataGridViewExtensions.GetPropertyValueOrDefault(
          owningRowDataBoundItem,
          this.EnabledWhenPropertyName,
          true,
          true,
          true,
          true
        );

      if (source == null)
        return;

      if (this.DisplayMember != null)
        comboBox.DisplayMember = this.DisplayMember;

      if (this.ValueMember != null)
        comboBox.ValueMember = this.ValueMember;


      comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBox.SelectedIndex = 0;

      this.ValueType = source.GetType().GetElementType();
    }

    public override object Clone() {
      var cell = (BoundDataGridViewComboBoxCell)base.Clone();
      cell.DataSourcePropertyName = this.DataSourcePropertyName;
      cell.EnabledWhenPropertyName = this.EnabledWhenPropertyName;
      cell.ValueMember = this.ValueMember;
      cell.DisplayMember = this.DisplayMember;
      return cell;
    }
  }
}
