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
