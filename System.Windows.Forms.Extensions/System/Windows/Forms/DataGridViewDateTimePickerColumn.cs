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

public class DataGridViewDateTimePickerColumn : DataGridViewColumn {
  public DataGridViewDateTimePickerColumn() : base(new DataGridViewDateTimePickerCell()) { }

  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewDateTimePickerCell)))
        throw new InvalidCastException("Must be a DataGridViewDateTimePickerCell");
      base.CellTemplate = value;
    }
  }

  public class DataGridViewDateTimePickerCell : DataGridViewTextBoxCell {
    public DataGridViewDateTimePickerCell() => this.Style.Format = "d";

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue,
      DataGridViewCellStyle dataGridViewCellStyle) {
      base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

      if (this.DataGridView.EditingControl is not DateTimePickerEditingControl ctl)
        return;

      ctl.Value = (DateTime?)this.Value ?? ((DateTime?)this.DefaultNewRowValue ?? DateTime.Now);
    }

    public override Type EditType => typeof(DateTimePickerEditingControl);

    public override Type ValueType => typeof(DateTime);

    public override object DefaultNewRowValue => DateTime.Now;
  }

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
        if (value is not string)
          return;

        this.Value = DateTime.TryParse((string)value, out var parsedDate) ? parsedDate : DateTime.Now;
      }
    }

    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) =>
      this.EditingControlFormattedValue;

    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
      this.Font = dataGridViewCellStyle.Font;
      this.CalendarForeColor = dataGridViewCellStyle.ForeColor;
      this.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
    }

    public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey) {
      // Let the DateTimePicker handle the keys listed.
      switch (key & Keys.KeyCode) {
        case Keys.Left:
        case Keys.Up:
        case Keys.Down:
        case Keys.Right:
        case Keys.Home:
        case Keys.End:
        case Keys.PageDown:
        case Keys.PageUp:
          return true;
        default:
          return !dataGridViewWantsInputKey;
      }
    }

    protected override void OnValueChanged(EventArgs eventArgs) {
      this.EditingControlValueChanged = true;
      this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
      base.OnValueChanged(eventArgs);
    }

    public void PrepareEditingControlForEdit(bool selectAll) { }
  }
}
