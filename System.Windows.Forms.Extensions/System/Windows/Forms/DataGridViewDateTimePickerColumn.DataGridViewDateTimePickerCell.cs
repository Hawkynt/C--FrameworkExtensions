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

public partial class DataGridViewDateTimePickerColumn {
  public class DataGridViewDateTimePickerCell : DataGridViewTextBoxCell {
    public DataGridViewDateTimePickerCell() => this.Style.Format = "d";

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
      base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

      if (this.DataGridView.EditingControl is not DateTimePickerEditingControl ctl)
        return;

      ctl.Value = (DateTime?)this.Value ?? ((DateTime?)this.DefaultNewRowValue ?? DateTime.Now);
    }

    public override Type EditType => typeof(DateTimePickerEditingControl);

    public override Type ValueType => typeof(DateTime);

    public override object DefaultNewRowValue => DateTime.Now;
  }
}
