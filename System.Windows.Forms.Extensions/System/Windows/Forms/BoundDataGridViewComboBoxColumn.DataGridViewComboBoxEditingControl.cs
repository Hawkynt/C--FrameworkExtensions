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

namespace System.Windows.Forms;

public partial class BoundDataGridViewComboBoxColumn {
  internal class DataGridViewComboBoxEditingControl : ComboBox, IDataGridViewEditingControl {
    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) { }
    public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) => false;
    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) => this.SelectedValue;
    public void PrepareEditingControlForEdit(bool selectAll) { }

    public DataGridView EditingControlDataGridView { get; set; }

    public object EditingControlFormattedValue {
      get => this.GetEditingControlFormattedValue(DataGridViewDataErrorContexts.Formatting);
      set => this.SelectedValue = value;
    }

    public int EditingControlRowIndex { get; set; }

    public bool EditingControlValueChanged { get; set; }

    public Cursor EditingPanelCursor => Cursors.Default;

    public bool RepositionEditingControlOnValueChange => false;

    protected override void OnSelectedValueChanged(EventArgs e) {
      this.EditingControlDataGridView.NotifyCurrentCellDirty(true);

      base.OnSelectedValueChanged(e);
    }
  }
}
