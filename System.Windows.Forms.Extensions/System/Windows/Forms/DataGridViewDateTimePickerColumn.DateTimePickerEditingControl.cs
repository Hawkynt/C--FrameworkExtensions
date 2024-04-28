#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace System.Windows.Forms;

public partial class DataGridViewDateTimePickerColumn {
  private class DateTimePickerEditingControl : DateTimePicker, IDataGridViewEditingControl {
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
      => this.EditingControlFormattedValue
      ;

    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
      this.Font = dataGridViewCellStyle.Font;
      this.CalendarForeColor = dataGridViewCellStyle.ForeColor;
      this.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
    }

    public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
      // Let the DateTimePicker handle the keys listed.
      => (key & Keys.KeyCode) is Keys.Left or Keys.Up or Keys.Down or Keys.Right or Keys.Home or Keys.End or Keys.PageDown or Keys.PageUp 
         || !dataGridViewWantsInputKey
         ;

    protected override void OnValueChanged(EventArgs eventArgs) {
      this.EditingControlValueChanged = true;
      this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
      base.OnValueChanged(eventArgs);
    }

    public void PrepareEditingControlForEdit(bool selectAll) { }
    
  }
}
