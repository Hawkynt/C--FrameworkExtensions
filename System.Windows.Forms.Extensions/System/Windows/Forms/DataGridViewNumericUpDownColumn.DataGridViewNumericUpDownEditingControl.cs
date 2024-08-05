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

using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms;

public partial class DataGridViewNumericUpDownColumn {
  internal class DataGridViewNumericUpDownEditingControl : NumericUpDown, IDataGridViewEditingControl {
    
    // Needed to forward keyboard messages to the child TextBox control.
    [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    // The grid that owns this editing control
    private DataGridView _dataGridView;

    // Stores whether the editing control's value has changed or not
    private bool _valueChanged;
    // Stores the row index in which the editing control resides

    /// <summary>
    ///   Constructor of the editing control class
    /// </summary>
    public DataGridViewNumericUpDownEditingControl() =>
      // The editing control must not be part of the tabbing loop
      this.TabStop = false;

    // Beginning of the IDataGridViewEditingControl interface implementation

    /// <summary>
    ///   Property which caches the grid that uses this editing control
    /// </summary>
    public virtual DataGridView EditingControlDataGridView {
      get => this._dataGridView;
      set => this._dataGridView = value;
    }

    /// <summary>
    ///   Property which represents the current formatted value of the editing control
    /// </summary>
    public virtual object EditingControlFormattedValue {
      get => this.GetEditingControlFormattedValue(DataGridViewDataErrorContexts.Formatting);
      set => this.Text = (string)value;
    }

    /// <summary>
    ///   Property which represents the row in which the editing control resides
    /// </summary>
    public virtual int EditingControlRowIndex { get; set; }

    /// <summary>
    ///   Property which indicates whether the value of the editing control has changed or not
    /// </summary>
    public virtual bool EditingControlValueChanged {
      get => this._valueChanged;
      set => this._valueChanged = value;
    }

    /// <summary>
    ///   Property which determines which cursor must be used for the editing panel,
    ///   i.e. the parent of the editing control.
    /// </summary>
    public virtual Cursor EditingPanelCursor => Cursors.Default;

    /// <summary>
    ///   Property which indicates whether the editing control needs to be repositioned
    ///   when its value changes.
    /// </summary>
    public virtual bool RepositionEditingControlOnValueChange => false;

    /// <summary>
    ///   Method called by the grid before the editing control is shown so it can adapt to the
    ///   provided cell style.
    /// </summary>
    public virtual void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
      this.Font = dataGridViewCellStyle.Font;
      if (dataGridViewCellStyle.BackColor.A < 255) {
        // The NumericUpDown control does not support transparent back colors
        var opaqueBackColor = Color.FromArgb(255, dataGridViewCellStyle.BackColor);
        this.BackColor = opaqueBackColor;
        this._dataGridView.EditingPanel.BackColor = opaqueBackColor;
      } else
        this.BackColor = dataGridViewCellStyle.BackColor;

      this.ForeColor = dataGridViewCellStyle.ForeColor;
      this.TextAlign = DataGridViewNumericUpDownCell.TranslateAlignment(dataGridViewCellStyle.Alignment);
    }

    /// <summary>
    ///   Method called by the grid on keystrokes to determine if the editing control is
    ///   interested in the key or not.
    /// </summary>
    public virtual bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) {
      switch (keyData & Keys.KeyCode) {
        case Keys.Right: {
          if (this.Controls[1] is TextBox textBox)
            // If the end of the selection is at the end of the string,
            // let the DataGridView treat the key message
            if ((this.RightToLeft == RightToLeft.No && !(textBox.SelectionLength == 0 && textBox.SelectionStart == textBox.Text.Length)) || (this.RightToLeft == RightToLeft.Yes && !(textBox.SelectionLength == 0 && textBox.SelectionStart == 0)))
              return true;

          break;
        }

        case Keys.Left: {
          if (this.Controls[1] is TextBox textBox)
            // If the end of the selection is at the begining of the string
            // or if the entire text is selected and we did not start editing,
            // send this character to the dataGridView, else process the key message
            if ((this.RightToLeft == RightToLeft.No && !(textBox.SelectionLength == 0 && textBox.SelectionStart == 0)) || (this.RightToLeft == RightToLeft.Yes && !(textBox.SelectionLength == 0 && textBox.SelectionStart == textBox.Text.Length)))
              return true;

          break;
        }

        case Keys.Down:
          // If the current value hasn't reached its minimum yet, handle the key. Otherwise let
          // the grid handle it.
          if (this.Value > this.Minimum)
            return true;

          break;

        case Keys.Up:
          // If the current value hasn't reached its maximum yet, handle the key. Otherwise let
          // the grid handle it.
          if (this.Value < this.Maximum)
            return true;

          break;

        case Keys.Home:
        case Keys.End: {
          // Let the grid handle the key if the entire text is selected.
          if (this.Controls[1] is TextBox textBox)
            if (textBox.SelectionLength != textBox.Text.Length)
              return true;

          break;
        }

        case Keys.Delete: {
          // Let the grid handle the key if the carret is at the end of the text.
          if (this.Controls[1] is TextBox textBox)
            if (textBox.SelectionLength > 0 || textBox.SelectionStart < textBox.Text.Length)
              return true;

          break;
        }
      }

      return !dataGridViewWantsInputKey;
    }

    /// <summary>
    ///   Returns the current value of the editing control.
    /// </summary>
    public virtual object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) {
      var userEdit = this.UserEdit;
      try {
        // Prevent the Value from being set to Maximum or Minimum when the cell is being painted.
        this.UserEdit = (context & DataGridViewDataErrorContexts.Display) == 0;
        return this.Value.ToString((this.ThousandsSeparator ? "N" : "F") + this.DecimalPlaces);
      } finally {
        this.UserEdit = userEdit;
      }
    }

    /// <summary>
    ///   Called by the grid to give the editing control a chance to prepare itself for
    ///   the editing session.
    /// </summary>
    public virtual void PrepareEditingControlForEdit(bool selectAll) {
      if (this.Controls[1] is not TextBox textBox)
        return;

      if (selectAll)
        textBox.SelectAll();
      else
        // Do not select all the text, but
        // position the caret at the end of the text
        textBox.SelectionStart = textBox.Text.Length;
    }

    // End of the IDataGridViewEditingControl interface implementation

    /// <summary>
    ///   Small utility function that updates the local dirty state and
    ///   notifies the grid of the value change.
    /// </summary>
    private void NotifyDataGridViewOfValueChange() {
      if (this._valueChanged)
        return;

      this._valueChanged = true;
      this._dataGridView.NotifyCurrentCellDirty(true);
    }

    /// <summary>
    ///   Listen to the KeyPress notification to know when the value changed, and
    ///   notify the grid of the change.
    /// </summary>
    protected override void OnKeyPress(KeyPressEventArgs e) {
      base.OnKeyPress(e);

      // The value changes when a digit, the decimal separator, the group separator or
      // the negative sign is pressed.
      var notifyValueChange = false;
      if (char.IsDigit(e.KeyChar))
        notifyValueChange = true;
      else {
        var numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
        var decimalSeparatorStr = numberFormatInfo.NumberDecimalSeparator;
        var groupSeparatorStr = numberFormatInfo.NumberGroupSeparator;
        var negativeSignStr = numberFormatInfo.NegativeSign;
        if (!string.IsNullOrEmpty(decimalSeparatorStr) && decimalSeparatorStr.Length == 1)
          notifyValueChange = decimalSeparatorStr[0] == e.KeyChar;

        if (!notifyValueChange && !string.IsNullOrEmpty(groupSeparatorStr) && groupSeparatorStr.Length == 1)
          notifyValueChange = groupSeparatorStr[0] == e.KeyChar;

        if (!notifyValueChange && !string.IsNullOrEmpty(negativeSignStr) && negativeSignStr.Length == 1)
          notifyValueChange = negativeSignStr[0] == e.KeyChar;
      }

      if (notifyValueChange)
        // Let the DataGridView know about the value change
        this.NotifyDataGridViewOfValueChange();
    }

    /// <summary>
    ///   Listen to the ValueChanged notification to forward the change to the grid.
    /// </summary>
    protected override void OnValueChanged(EventArgs e) {
      base.OnValueChanged(e);
      if (this.Focused)
        // Let the DataGridView know about the value change
        this.NotifyDataGridViewOfValueChange();
    }

    /// <summary>
    ///   A few keyboard messages need to be forwarded to the inner textbox of the
    ///   NumericUpDown control so that the first character pressed appears in it.
    /// </summary>
    protected override bool ProcessKeyEventArgs(ref Message m) {
      if (this.Controls[1] is not TextBox textBox)
        return base.ProcessKeyEventArgs(ref m);

      SendMessage(textBox.Handle, m.Msg, m.WParam, m.LParam);
      return true;
    }
  }
}
