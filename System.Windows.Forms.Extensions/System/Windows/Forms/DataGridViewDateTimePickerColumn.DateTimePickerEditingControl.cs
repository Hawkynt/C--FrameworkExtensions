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

public partial class DataGridViewDateTimePickerColumn {
  private sealed class DateTimePickerEditingControl : DateTimePicker, IDataGridViewEditingControl {
    public int EditingControlRowIndex { get; set; }
    public DataGridView EditingControlDataGridView { get; set; }
    public bool RepositionEditingControlOnValueChange => false;
    public bool EditingControlValueChanged { get; set; }
    public Cursor EditingPanelCursor => base.Cursor;

    public DateTimePickerEditingControl() => this.Format = DateTimePickerFormat.Short;

    public object EditingControlFormattedValue {
      get => this.Value.ToShortDateString();
      set {
        if (value is string s)
          this.Value = DateTime.TryParse(s, out var parsedDate) ? parsedDate : DateTime.Now;
      }
    }

    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
      => this.EditingControlFormattedValue;

    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
      this.Font = dataGridViewCellStyle.Font;
      this.CalendarForeColor = dataGridViewCellStyle.ForeColor;
      this.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
    }

    public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
      // Let the DateTimePicker handle the keys listed.
      => (key & Keys.KeyCode) is Keys.Left or Keys.Up or Keys.Down or Keys.Right or Keys.Home or Keys.End or Keys.PageDown or Keys.PageUp
         || !dataGridViewWantsInputKey;

    protected override void OnValueChanged(EventArgs eventArgs) {
      this.EditingControlValueChanged = true;
      this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
      base.OnValueChanged(eventArgs);
    }

    public void PrepareEditingControlForEdit(bool selectAll) { }
  }
}
