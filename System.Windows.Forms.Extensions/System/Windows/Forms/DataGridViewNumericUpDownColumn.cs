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

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms;

public class DataGridViewNumericUpDownColumn : DataGridViewColumn {
  /// <summary>
  ///   Constructor for the DataGridViewNumericUpDownColumn class.
  /// </summary>
  public DataGridViewNumericUpDownColumn() : base(new DataGridViewNumericUpDownCell()) { }

  /// <summary>
  ///   Represents the implicit cell that gets cloned when adding rows to the grid.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      if (value != null && value is not DataGridViewNumericUpDownCell)
        throw new InvalidCastException("Value provided for CellTemplate must be of type DataGridViewNumericUpDownElements.DataGridViewNumericUpDownCell or derive from it.");

      base.CellTemplate = value;
    }
  }

  /// <summary>
  ///   Replicates the DecimalPlaces property of the DataGridViewNumericUpDownCell cell type.
  /// </summary>
  [Category("Appearance")]
  [DefaultValue(DataGridViewNumericUpDownCell.DATAGRIDVIEWNUMERICUPDOWNCELL_defaultDecimalPlaces)]
  [Description("Indicates the number of decimal places to display.")]
  public int DecimalPlaces {
    get {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.DecimalPlaces;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      // Update the template cell so that subsequent cloned cells use the new value.
      this.NumericUpDownCellTemplate.DecimalPlaces = value;
      var dataGridView = this.DataGridView;
      if (dataGridView == null)
        return;

      // Update all the existing DataGridViewNumericUpDownCell cells in the column accordingly.
      var dataGridViewRows = dataGridView.Rows;

      var rowCount = dataGridViewRows.Count;
      for (var rowIndex = 0; rowIndex < rowCount; rowIndex++) {
        // Be careful not to unshare rows unnecessarily. 
        // This could have severe performance repercussions.
        var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
        if (dataGridViewRow.Cells[this.Index] is DataGridViewNumericUpDownCell dataGridViewCell)
          // Call the SetDecimalPlaces method instead of the property to avoid invalidation 
          // of each cell. The whole column is invalidated later in a single operation for better performance.
          dataGridViewCell.SetDecimalPlaces(rowIndex, value);
      }

      dataGridView.InvalidateColumn(this.Index);
      // TODO: Call the grid's autosizing methods to autosize the column, rows, column headers / row headers as needed.
    }
  }

  /// <summary>
  ///   Replicates the Increment property of the DataGridViewNumericUpDownCell cell type.
  /// </summary>
  [Category("Data")]
  [Description("Indicates the amount to increment or decrement on each button click.")]
  public decimal Increment {
    get {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.Increment;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      this.NumericUpDownCellTemplate.Increment = value;
      var dataGridView = this.DataGridView;
      if (dataGridView == null)
        return;

      var dataGridViewRows = dataGridView.Rows;
      var rowCount = dataGridViewRows.Count;
      for (var rowIndex = 0; rowIndex < rowCount; rowIndex++) {
        var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
        if (dataGridViewRow.Cells[this.Index] is DataGridViewNumericUpDownCell dataGridViewCell)
          dataGridViewCell.SetIncrement(rowIndex, value);
      }
    }
  }

  /// Indicates whether the Increment property should be persisted.
  private bool ShouldSerializeIncrement()
    => !this.Increment.Equals(DataGridViewNumericUpDownCell.DATAGRIDVIEWNUMERICUPDOWNCELL_defaultIncrement);

  /// <summary>
  ///   Replicates the Maximum property of the DataGridViewNumericUpDownCell cell type.
  /// </summary>
  [Category("Data")]
  [Description("Indicates the maximum value for the numeric up-down cells.")]
  [RefreshProperties(RefreshProperties.All)]
  public decimal Maximum {
    get {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.Maximum;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      this.NumericUpDownCellTemplate.Maximum = value;
      var dataGridView = this.DataGridView;
      if (dataGridView == null)
        return;

      var dataGridViewRows = dataGridView.Rows;
      var rowCount = dataGridViewRows.Count;
      for (var rowIndex = 0; rowIndex < rowCount; rowIndex++) {
        var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
        if (dataGridViewRow.Cells[this.Index] is DataGridViewNumericUpDownCell dataGridViewCell)
          dataGridViewCell.SetMaximum(rowIndex, value);
      }

      dataGridView.InvalidateColumn(this.Index);
      // TODO: This column and/or grid rows may need to be autosized depending on their
      //       autosize settings. Call the autosizing methods to autosize the column, rows, 
      //       column headers / row headers as needed.
    }
  }

  /// Indicates whether the Maximum property should be persisted.
  private bool ShouldSerializeMaximum()
    => !this.Maximum.Equals(DataGridViewNumericUpDownCell.DATAGRIDVIEWNUMERICUPDOWNCELL_defaultMaximum);

  /// <summary>
  ///   Replicates the Minimum property of the DataGridViewNumericUpDownCell cell type.
  /// </summary>
  [Category("Data")]
  [Description("Indicates the minimum value for the numeric up-down cells.")]
  [RefreshProperties(RefreshProperties.All)]
  public decimal Minimum {
    get {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.Minimum;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      this.NumericUpDownCellTemplate.Minimum = value;
      if (this.DataGridView == null)
        return;

      var dataGridViewRows = this.DataGridView.Rows;
      var rowCount = dataGridViewRows.Count;
      for (var rowIndex = 0; rowIndex < rowCount; rowIndex++) {
        var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
        if (dataGridViewRow.Cells[this.Index] is DataGridViewNumericUpDownCell dataGridViewCell)
          dataGridViewCell.SetMinimum(rowIndex, value);
      }

      this.DataGridView.InvalidateColumn(this.Index);
      // TODO: This column and/or grid rows may need to be autosized depending on their
      //       autosize settings. Call the autosizing methods to autosize the column, rows, 
      //       column headers / row headers as needed.
    }
  }

  /// Indicates whether the Maximum property should be persisted.
  private bool ShouldSerializeMinimum()
    => !this.Minimum.Equals(DataGridViewNumericUpDownCell.DATAGRIDVIEWNUMERICUPDOWNCELL_defaultMinimum);

  /// <summary>
  ///   Replicates the ThousandsSeparator property of the DataGridViewNumericUpDownCell cell type.
  /// </summary>
  [Category("Data")]
  [DefaultValue(DataGridViewNumericUpDownCell.DATAGRIDVIEWNUMERICUPDOWNCELL_defaultThousandsSeparator)]
  [Description("Indicates whether the thousands separator will be inserted between every three decimal digits.")]
  public bool ThousandsSeparator {
    get {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.ThousandsSeparator;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      this.NumericUpDownCellTemplate.ThousandsSeparator = value;
      if (this.DataGridView == null)
        return;

      var dataGridViewRows = this.DataGridView.Rows;
      var rowCount = dataGridViewRows.Count;
      for (var rowIndex = 0; rowIndex < rowCount; rowIndex++) {
        var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
        if (dataGridViewRow.Cells[this.Index] is DataGridViewNumericUpDownCell dataGridViewCell)
          dataGridViewCell.SetThousandsSeparator(rowIndex, value);
      }

      this.DataGridView.InvalidateColumn(this.Index);
      // TODO: This column and/or grid rows may need to be autosized depending on their
      //       autosize settings. Call the autosizing methods to autosize the column, rows, 
      //       column headers / row headers as needed.
    }
  }

  /// <summary>
  ///   Small utility function that returns the template cell as a DataGridViewNumericUpDownCell
  /// </summary>
  private DataGridViewNumericUpDownCell NumericUpDownCellTemplate
    => (DataGridViewNumericUpDownCell)this.CellTemplate;

  /// <summary>
  ///   Returns a standard compact string representation of the column.
  /// </summary>
  public override string ToString() {
    var sb = new StringBuilder(100);
    sb.Append("DataGridViewNumericUpDownColumn { Name=");
    sb.Append(this.Name);
    sb.Append(", Index=");
    sb.Append(this.Index.ToString(CultureInfo.CurrentCulture));
    sb.Append(" }");
    return sb.ToString();
  }

  internal class DataGridViewNumericUpDownEditingControl : NumericUpDown, IDataGridViewEditingControl {
    // Needed to forward keyboard messages to the child TextBox control.
    [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    // The grid that owns this editing control
    private DataGridView dataGridView;

    // Stores whether the editing control's value has changed or not
    private bool valueChanged;
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
      get => this.dataGridView;
      set => this.dataGridView = value;
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
      get => this.valueChanged;
      set => this.valueChanged = value;
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
        this.dataGridView.EditingPanel.BackColor = opaqueBackColor;
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
      if (this.valueChanged)
        return;

      this.valueChanged = true;
      this.dataGridView.NotifyCurrentCellDirty(true);
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

  internal class DataGridViewNumericUpDownCell : DataGridViewTextBoxCell {
    // Used in KeyEntersEditMode function
    [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
    private static extern short VkKeyScan(char key);

    // Used in TranslateAlignment function
    private static readonly DataGridViewContentAlignment anyRight = DataGridViewContentAlignment.TopRight | DataGridViewContentAlignment.MiddleRight | DataGridViewContentAlignment.BottomRight;

    private static readonly DataGridViewContentAlignment anyCenter = DataGridViewContentAlignment.TopCenter | DataGridViewContentAlignment.MiddleCenter | DataGridViewContentAlignment.BottomCenter;

    // Default dimensions of the static rendering bitmap used for the painting of the non-edited cells
    private const int DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapWidth = 100;
    private const int DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapHeight = 22;

    // Default value of the DecimalPlaces property
    internal const int DATAGRIDVIEWNUMERICUPDOWNCELL_defaultDecimalPlaces = 0;

    // Default value of the Increment property
    internal const decimal DATAGRIDVIEWNUMERICUPDOWNCELL_defaultIncrement = decimal.One;

    // Default value of the Maximum property
    internal const decimal DATAGRIDVIEWNUMERICUPDOWNCELL_defaultMaximum = (decimal)100.0;

    // Default value of the Minimum property
    internal const decimal DATAGRIDVIEWNUMERICUPDOWNCELL_defaultMinimum = decimal.Zero;

    // Default value of the ThousandsSeparator property
    internal const bool DATAGRIDVIEWNUMERICUPDOWNCELL_defaultThousandsSeparator = false;

    // Type of this cell's editing control
    private static readonly Type defaultEditType = typeof(DataGridViewNumericUpDownEditingControl);

    // Type of this cell's value. The formatted value type is string, the same as the base class DataGridViewTextBoxCell
    private static readonly Type defaultValueType = typeof(decimal);

    // The bitmap used to paint the non-edited cells via a call to NumericUpDown.DrawToBitmap
    [ThreadStatic] private static Bitmap renderingBitmap;

    // The NumericUpDown control used to paint the non-edited cells via a call to NumericUpDown.DrawToBitmap
    [ThreadStatic] private static NumericUpDown paintingNumericUpDown;

    private int decimalPlaces; // Caches the value of the DecimalPlaces property
    private decimal increment; // Caches the value of the Increment property
    private decimal minimum; // Caches the value of the Minimum property
    private decimal maximum; // Caches the value of the Maximum property
    private bool thousandsSeparator; // Caches the value of the ThousandsSeparator property

    /// <summary>
    ///   Constructor for the DataGridViewNumericUpDownCell cell type
    /// </summary>
    public DataGridViewNumericUpDownCell() {
      // GetSingleResult a thread specific bitmap used for the painting of the non-edited cells
      renderingBitmap ??= new(
        DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapWidth,
        DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapHeight
      );

      // GetSingleResult a thread specific NumericUpDown control used for the painting of the non-edited cells
      paintingNumericUpDown ??= new() {
        BorderStyle = BorderStyle.None, 
        Maximum = decimal.MaxValue / 10, 
        Minimum = decimal.MinValue / 10
      };

      // Set the default values of the properties:
      this.decimalPlaces = DATAGRIDVIEWNUMERICUPDOWNCELL_defaultDecimalPlaces;
      this.increment = DATAGRIDVIEWNUMERICUPDOWNCELL_defaultIncrement;
      this.minimum = DATAGRIDVIEWNUMERICUPDOWNCELL_defaultMinimum;
      this.maximum = DATAGRIDVIEWNUMERICUPDOWNCELL_defaultMaximum;
      this.thousandsSeparator = DATAGRIDVIEWNUMERICUPDOWNCELL_defaultThousandsSeparator;
    }

    /// <summary>
    ///   The DecimalPlaces property replicates the one from the NumericUpDown control
    /// </summary>
    [DefaultValue(DATAGRIDVIEWNUMERICUPDOWNCELL_defaultDecimalPlaces)]
    public int DecimalPlaces {
      get => this.decimalPlaces;
      set {
        if (value is < 0 or > 99)
          throw new ArgumentOutOfRangeException(
            nameof(value),
            "The DecimalPlaces property cannot be smaller than 0 or larger than 99."
          );

        if (this.decimalPlaces == value)
          return;

        this.SetDecimalPlaces(this.RowIndex, value);
        this.OnCommonChange(); // Assure that the cell or column gets repainted and autosized if needed
      }
    }

    /// <summary>
    ///   Returns the current DataGridView EditingControl as a DataGridViewNumericUpDownEditingControl control
    /// </summary>
    private DataGridViewNumericUpDownEditingControl EditingNumericUpDown {
      get {
        var dataGridView = this.DataGridView;
        return dataGridView.EditingControl as DataGridViewNumericUpDownEditingControl;
      }
    }

    /// <summary>
    ///   Define the type of the cell's editing control
    /// </summary>
    public override Type EditType => defaultEditType;

    /// <summary>
    ///   The Increment property replicates the one from the NumericUpDown control
    /// </summary>
    public decimal Increment {
      get => this.increment;
      set {
        if (value < 0m)
          throw new ArgumentOutOfRangeException(nameof(value), "The Increment property cannot be smaller than 0.");

        this.SetIncrement(this.RowIndex, value);
        // No call to OnCommonChange is needed since the increment value does not affect the rendering of the cell.
      }
    }

    /// <summary>
    ///   The Maximum property replicates the one from the NumericUpDown control
    /// </summary>
    public decimal Maximum {
      get => this.maximum;
      set {
        if (this.maximum == value)
          return;

        this.SetMaximum(this.RowIndex, value);
        this.OnCommonChange();
      }
    }

    /// <summary>
    ///   The Minimum property replicates the one from the NumericUpDown control
    /// </summary>
    public decimal Minimum {
      get => this.minimum;
      set {
        if (this.minimum == value)
          return;

        this.SetMinimum(this.RowIndex, value);
        this.OnCommonChange();
      }
    }

    /// <summary>
    ///   The ThousandsSeparator property replicates the one from the NumericUpDown control
    /// </summary>
    [DefaultValue(DATAGRIDVIEWNUMERICUPDOWNCELL_defaultThousandsSeparator)]
    public bool ThousandsSeparator {
      get => this.thousandsSeparator;
      set {
        if (this.thousandsSeparator == value)
          return;

        this.SetThousandsSeparator(this.RowIndex, value);
        this.OnCommonChange();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Returns the type of the cell's Value property
    /// </summary>
    public override Type ValueType => base.ValueType ?? defaultValueType;

    /// <summary>
    ///   Clones a DataGridViewNumericUpDownCell cell, copies all the custom properties.
    /// </summary>
    public override object Clone() {
      var result = base.Clone();
      if (result is not DataGridViewNumericUpDownCell dataGridViewCell)
        return result;

      dataGridViewCell.DecimalPlaces = this.DecimalPlaces;
      dataGridViewCell.Increment = this.Increment;
      dataGridViewCell.Maximum = this.Maximum;
      dataGridViewCell.Minimum = this.Minimum;
      dataGridViewCell.ThousandsSeparator = this.ThousandsSeparator;
      return dataGridViewCell;
    }

    /// <summary>
    ///   Returns the provided value constrained to be within the min and max.
    /// </summary>
    private decimal Constrain(decimal value) {
      if (this.minimum > this.maximum)
        Debug.Fail("min must be <= max");

      if (value < this.minimum)
        value = this.minimum;

      if (value > this.maximum)
        value = this.maximum;

      return value;
    }

    /// <summary>
    ///   DetachEditingControl gets called by the DataGridView control when the editing session is ending
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public override void DetachEditingControl() {
      var dataGridView = this.DataGridView;
      if (dataGridView?.EditingControl == null)
        throw new InvalidOperationException("Cell is detached or its grid has no editing control.");

      if (dataGridView.EditingControl is NumericUpDown numericUpDown)
        // Editing controls get recycled. Indeed, when a DataGridViewNumericUpDownCell cell gets edited
        // after another DataGridViewNumericUpDownCell cell, the same editing control gets reused for 
        // performance reasons (to avoid an unnecessary control destruction and creation). 
        // Here the undo buffer of the TextBox inside the NumericUpDown control gets cleared to avoid
        // interferences between the editing sessions.
        if (numericUpDown.Controls[1] is TextBox textBox)
          textBox.ClearUndo();

      base.DetachEditingControl();
    }

    /// <summary>
    ///   Adjusts the location and size of the editing control given the alignment characteristics of the cell
    /// </summary>
    private static Rectangle GetAdjustedEditingControlBounds(Rectangle editingControlBounds, DataGridViewCellStyle cellStyle) {
      // Add a 1 pixel padding on the left and right of the editing control
      editingControlBounds.X += 1;
      editingControlBounds.Width = Math.Max(0, editingControlBounds.Width - 2);

      // Adjust the vertical location of the editing control:
      var preferredHeight = cellStyle.Font.Height + 3;
      if (preferredHeight >= editingControlBounds.Height)
        return editingControlBounds;

      switch (cellStyle.Alignment) {
        case DataGridViewContentAlignment.MiddleLeft:
        case DataGridViewContentAlignment.MiddleCenter:
        case DataGridViewContentAlignment.MiddleRight:
          editingControlBounds.Y += (editingControlBounds.Height - preferredHeight) / 2;
          break;
        case DataGridViewContentAlignment.BottomLeft:
        case DataGridViewContentAlignment.BottomCenter:
        case DataGridViewContentAlignment.BottomRight:
          editingControlBounds.Y += editingControlBounds.Height - preferredHeight;
          break;
      }

      return editingControlBounds;
    }

    /// <summary>
    ///   Customized implementation of the GetErrorIconBounds function in order to draw the potential
    ///   error icon next to the up/down buttons and not on top of them.
    /// </summary>
    protected override Rectangle GetErrorIconBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
      const int ButtonsWidth = 16;

      var errorIconBounds = base.GetErrorIconBounds(graphics, cellStyle, rowIndex);
      if (this.DataGridView.RightToLeft == RightToLeft.Yes)
        errorIconBounds.X = errorIconBounds.Left + ButtonsWidth;
      else
        errorIconBounds.X = errorIconBounds.Left - ButtonsWidth;
      return errorIconBounds;
    }

    /// <summary>
    ///   Customized implementation of the GetFormattedValue function in order to include the decimal and thousand separator
    ///   characters in the formatted representation of the cell value.
    /// </summary>
    protected override object GetFormattedValue(
      object value,
      int rowIndex,
      ref DataGridViewCellStyle cellStyle,
      TypeConverter valueTypeConverter,
      TypeConverter formattedValueTypeConverter,
      DataGridViewDataErrorContexts context
    ) {
      // By default, the base implementation converts the Decimal 1234.5 into the string "1234.5"
      var formattedValue = base.GetFormattedValue(
        value,
        rowIndex,
        ref cellStyle,
        valueTypeConverter,
        formattedValueTypeConverter,
        context
      );
      var formattedNumber = formattedValue as string;
      if (string.IsNullOrEmpty(formattedNumber) || value == null)
        return formattedValue;

      var unformattedDecimal = Convert.ToDecimal(value);
      return unformattedDecimal.ToString((this.ThousandsSeparator ? "N" : "F") + this.DecimalPlaces);
    }

    /// <summary>
    ///   Custom implementation of the GetPreferredSize function. This implementation uses the preferred size of the base
    ///   DataGridViewTextBoxCell cell and adds room for the up/down buttons.
    /// </summary>
    protected override Size GetPreferredSize(
      Graphics graphics,
      DataGridViewCellStyle cellStyle,
      int rowIndex,
      Size constraintSize
    ) {
      if (this.DataGridView == null)
        return new(-1, -1);

      var preferredSize = base.GetPreferredSize(graphics, cellStyle, rowIndex, constraintSize);
      if (constraintSize.Width != 0)
        return preferredSize;

      const int ButtonsWidth = 16; // Account for the width of the up/down buttons.
      const int ButtonMargin = 8; // Account for some blank pixels between the text and buttons.
      preferredSize.Width += ButtonsWidth + ButtonMargin;
      return preferredSize;
    }

    /// <summary>
    ///   Custom implementation of the InitializeEditingControl function. This function is called by the DataGridView control
    ///   at the beginning of an editing session. It makes sure that the properties of the NumericUpDown editing control are
    ///   set according to the cell properties.
    /// </summary>
    public override void InitializeEditingControl(
      int rowIndex,
      object initialFormattedValue,
      DataGridViewCellStyle dataGridViewCellStyle
    ) {
      base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
      if (this.DataGridView.EditingControl is not NumericUpDown numericUpDown)
        return;

      numericUpDown.BorderStyle = BorderStyle.None;
      numericUpDown.DecimalPlaces = this.DecimalPlaces;
      numericUpDown.Increment = this.Increment;
      numericUpDown.Maximum = this.Maximum;
      numericUpDown.Minimum = this.Minimum;
      numericUpDown.ThousandsSeparator = this.ThousandsSeparator;
      numericUpDown.Text = initialFormattedValue as string ?? string.Empty;
    }

    /// <summary>
    ///   Custom implementation of the KeyEntersEditMode function. This function is called by the DataGridView control
    ///   to decide whether a keystroke must start an editing session or not. In this case, a new session is started when
    ///   a digit or negative sign key is hit.
    /// </summary>
    public override bool KeyEntersEditMode(KeyEventArgs e) {
      var numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
      var negativeSignKey = Keys.None;
      var negativeSignStr = numberFormatInfo.NegativeSign;
      if (!string.IsNullOrEmpty(negativeSignStr) && negativeSignStr.Length == 1)
        negativeSignKey = (Keys)VkKeyScan(negativeSignStr[0]);

      return (
               char.IsDigit((char)e.KeyCode)
               || e.KeyCode is >= Keys.NumPad0 and <= Keys.NumPad9
               || negativeSignKey == e.KeyCode
               || Keys.Subtract == e.KeyCode
             )
             && !e.Shift
             && e is { Alt: false, Control: false }
        ;
    }

    /// <summary>
    ///   Called when a cell characteristic that affects its rendering and/or preferred size has changed.
    ///   This implementation only takes care of repainting the cells. The DataGridView's autosizing methods
    ///   also need to be called in cases where some grid elements autosize.
    /// </summary>
    private void OnCommonChange() {
      if (this.DataGridView == null || this.DataGridView.IsDisposed || this.DataGridView.Disposing)
        return;

      if (this.RowIndex == -1)
        // Invalidate and autosize column
        this.DataGridView.InvalidateColumn(this.ColumnIndex);
      // TODO: Add code to autosize the cell's column, the rows, the column headers 
      // and the row headers depending on their autosize settings.
      // The DataGridView control does not expose a public method that takes care of this.
      else
        // The DataGridView control exposes a public method called UpdateCellValue
        // that invalidates the cell so that it gets repainted and also triggers all
        // the necessary autosizing: the cell's column and/or row, the column headers
        // and the row headers are autosized depending on their autosize settings.
        this.DataGridView.UpdateCellValue(this.ColumnIndex, this.RowIndex);
    }

    /// <summary>
    ///   Determines whether this cell, at the given row index, shows the grid's editing control or not.
    ///   The row index needs to be provided as a parameter because this cell may be shared among multiple rows.
    /// </summary>
    private bool OwnsEditingNumericUpDown(int rowIndex) {
      if (rowIndex == -1 || this.DataGridView == null)
        return false;

      return
        this.DataGridView.EditingControl is DataGridViewNumericUpDownEditingControl numericUpDownEditingControl
        && rowIndex == ((IDataGridViewEditingControl)numericUpDownEditingControl).EditingControlRowIndex
        ;
    }

    /// <summary>
    ///   Custom paints the cell. The base implementation of the DataGridViewTextBoxCell type is called first,
    ///   dropping the icon error and content foreground parts. Those two parts are painted by this custom implementation.
    ///   In this sample, the non-edited NumericUpDown control is painted by using a call to Control.DrawToBitmap. This is
    ///   an easy solution for painting controls but it's not necessarily the most performant. An alternative would be to paint
    ///   the NumericUpDown control piece by piece (text and up/down buttons).
    /// </summary>
    protected override void Paint(
      Graphics graphics,
      Rectangle clipBounds,
      Rectangle cellBounds,
      int rowIndex,
      DataGridViewElementStates cellState,
      object value,
      object formattedValue,
      string errorText,
      DataGridViewCellStyle cellStyle,
      DataGridViewAdvancedBorderStyle advancedBorderStyle,
      DataGridViewPaintParts paintParts
    ) {
      if (this.DataGridView == null)
        return;

      // First paint the borders and background of the cell.
      base.Paint(
        graphics,
        clipBounds,
        cellBounds,
        rowIndex,
        cellState,
        value,
        formattedValue,
        errorText,
        cellStyle,
        advancedBorderStyle,
        paintParts & ~(DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.ContentForeground)
      );

      var ptCurrentCell = this.DataGridView.CurrentCellAddress;
      var cellCurrent = ptCurrentCell.X == this.ColumnIndex && ptCurrentCell.Y == rowIndex;
      var cellEdited = cellCurrent && this.DataGridView.EditingControl != null;

      // If the cell is in editing mode, there is nothing else to paint
      if (cellEdited)
        return;

      if (PartPainted(paintParts, DataGridViewPaintParts.ContentForeground)) {
        // Paint a NumericUpDown control
        // Take the borders into account
        var borderWidths = this.BorderWidths(advancedBorderStyle);
        var valBounds = cellBounds;
        valBounds.Offset(borderWidths.X, borderWidths.Y);
        valBounds.Width -= borderWidths.Right;
        valBounds.Height -= borderWidths.Bottom;
        // Also take the padding into account
        if (cellStyle.Padding != Padding.Empty) {
          valBounds.Offset(
            this.DataGridView.RightToLeft == RightToLeft.Yes ? cellStyle.Padding.Right : cellStyle.Padding.Left,
            cellStyle.Padding.Top
          );
          valBounds.Width -= cellStyle.Padding.Horizontal;
          valBounds.Height -= cellStyle.Padding.Vertical;
        }

        // Determine the NumericUpDown control location
        valBounds = GetAdjustedEditingControlBounds(valBounds, cellStyle);

        var cellSelected = (cellState & DataGridViewElementStates.Selected) != 0;

        if (renderingBitmap.Width < valBounds.Width || renderingBitmap.Height < valBounds.Height) {
          // The static bitmap is too small, a bigger one needs to be allocated.
          renderingBitmap.Dispose();
          renderingBitmap = new(valBounds.Width, valBounds.Height);
        }

        // Make sure the NumericUpDown control is parented to a visible control
        if (paintingNumericUpDown.Parent is not { Visible: true })
          paintingNumericUpDown.Parent = this.DataGridView;

        // Set all the relevant properties
        paintingNumericUpDown.TextAlign = TranslateAlignment(cellStyle.Alignment);
        paintingNumericUpDown.DecimalPlaces = this.DecimalPlaces;
        paintingNumericUpDown.ThousandsSeparator = this.ThousandsSeparator;
        paintingNumericUpDown.Font = cellStyle.Font;
        paintingNumericUpDown.Width = valBounds.Width;
        paintingNumericUpDown.Height = valBounds.Height;
        paintingNumericUpDown.RightToLeft = this.DataGridView.RightToLeft;
        paintingNumericUpDown.Location = new(0, -paintingNumericUpDown.Height - 100);
        paintingNumericUpDown.Text = formattedValue as string;

        Color backColor;
        if (PartPainted(paintParts, DataGridViewPaintParts.SelectionBackground) && cellSelected)
          backColor = cellStyle.SelectionBackColor;
        else
          backColor = cellStyle.BackColor;
        if (PartPainted(paintParts, DataGridViewPaintParts.Background)) {
          if (backColor.A < 255)
            // The NumericUpDown control does not support transparent back colors
            backColor = Color.FromArgb(255, backColor);
          paintingNumericUpDown.BackColor = backColor;
        }

        // Finally paint the NumericUpDown control
        var srcRect = valBounds with { X = 0, Y = 0 };
        if (srcRect is { Width: > 0, Height: > 0 }) {
          paintingNumericUpDown.DrawToBitmap(renderingBitmap, srcRect);
          graphics.DrawImage(renderingBitmap, valBounds, srcRect, GraphicsUnit.Pixel);
        }
      }

      if (PartPainted(paintParts, DataGridViewPaintParts.ErrorIcon))
        // Paint the potential error icon on top of the NumericUpDown control
        base.Paint(
          graphics,
          clipBounds,
          cellBounds,
          rowIndex,
          cellState,
          value,
          formattedValue,
          errorText,
          cellStyle,
          advancedBorderStyle,
          DataGridViewPaintParts.ErrorIcon
        );
    }

    /// <summary>
    ///   Little utility function called by the Paint function to see if a particular part needs to be painted.
    /// </summary>
    private static bool PartPainted(DataGridViewPaintParts paintParts, DataGridViewPaintParts paintPart)
      => (paintParts & paintPart) != 0;

    /// <summary>
    ///   Custom implementation of the PositionEditingControl method called by the DataGridView control when it
    ///   needs to relocate and/or resize the editing control.
    /// </summary>
    public override void PositionEditingControl(
      bool setLocation,
      bool setSize,
      Rectangle cellBounds,
      Rectangle cellClip,
      DataGridViewCellStyle cellStyle,
      bool singleVerticalBorderAdded,
      bool singleHorizontalBorderAdded,
      bool isFirstDisplayedColumn,
      bool isFirstDisplayedRow
    ) {
      var editingControlBounds = this.PositionEditingPanel(
        cellBounds,
        cellClip,
        cellStyle,
        singleVerticalBorderAdded,
        singleHorizontalBorderAdded,
        isFirstDisplayedColumn,
        isFirstDisplayedRow
      );
      editingControlBounds = GetAdjustedEditingControlBounds(editingControlBounds, cellStyle);
      this.DataGridView.EditingControl.Location = new(editingControlBounds.X, editingControlBounds.Y);
      this.DataGridView.EditingControl.Size = new(editingControlBounds.Width, editingControlBounds.Height);
    }

    /// <summary>
    ///   Utility function that sets a new value for the DecimalPlaces property of the cell. This function is used by
    ///   the cell and column DecimalPlaces property. The column uses this method instead of the DecimalPlaces
    ///   property for performance reasons. This way the column can invalidate the entire column at once instead of
    ///   invalidating each cell of the column individually. A row index needs to be provided as a parameter because
    ///   this cell may be shared among multiple rows.
    /// </summary>
    public void SetDecimalPlaces(int rowIndex, int value) {
      this.decimalPlaces = value;
      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.DecimalPlaces = value;
    }

    /// Utility function that sets a new value for the Increment property of the cell. This function is used by
    /// the cell and column Increment property. A row index needs to be provided as a parameter because
    /// this cell may be shared among multiple rows.
    public void SetIncrement(int rowIndex, decimal value) {
      this.increment = value;
      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.Increment = value;
    }

    /// Utility function that sets a new value for the Maximum property of the cell. This function is used by
    /// the cell and column Maximum property. The column uses this method instead of the Maximum
    /// property for performance reasons. This way the column can invalidate the entire column at once instead of 
    /// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
    /// this cell may be shared among multiple rows.
    public void SetMaximum(int rowIndex, decimal value) {
      this.maximum = value;
      if (this.minimum > this.maximum)
        this.minimum = this.maximum;
      var cellValue = this.GetValue(rowIndex);
      if (cellValue != null) {
        var currentValue = Convert.ToDecimal(cellValue);
        var constrainedValue = this.Constrain(currentValue);
        if (constrainedValue != currentValue)
          this.SetValue(rowIndex, constrainedValue);
      }

      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.Maximum = value;
    }

    /// Utility function that sets a new value for the Minimum property of the cell. This function is used by
    /// the cell and column Minimum property. The column uses this method instead of the Minimum
    /// property for performance reasons. This way the column can invalidate the entire column at once instead of 
    /// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
    /// this cell may be shared among multiple rows.
    public void SetMinimum(int rowIndex, decimal value) {
      this.minimum = value;
      if (this.minimum > this.maximum)
        this.maximum = value;
      var cellValue = this.GetValue(rowIndex);
      if (cellValue != null) {
        var currentValue = Convert.ToDecimal(cellValue);
        var constrainedValue = this.Constrain(currentValue);
        if (constrainedValue != currentValue)
          this.SetValue(rowIndex, constrainedValue);
      }

      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.Minimum = value;
    }

    /// Utility function that sets a new value for the ThousandsSeparator property of the cell. This function is used by
    /// the cell and column ThousandsSeparator property. The column uses this method instead of the ThousandsSeparator
    /// property for performance reasons. This way the column can invalidate the entire column at once instead of 
    /// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
    /// this cell may be shared among multiple rows.
    public void SetThousandsSeparator(int rowIndex, bool value) {
      this.thousandsSeparator = value;
      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.ThousandsSeparator = value;
    }

    /// <summary>
    ///   Returns a standard textual representation of the cell.
    /// </summary>
    public override string ToString()
      => $"DataGridViewNumericUpDownCell {{ ColumnIndex={this.ColumnIndex.ToString(CultureInfo.CurrentCulture)}, RowIndex={this.RowIndex.ToString(CultureInfo.CurrentCulture)} }}";

    /// <summary>
    ///   Little utility function used by both the cell and column types to translate a DataGridViewContentAlignment value into
    ///   a HorizontalAlignment value.
    /// </summary>
    public static HorizontalAlignment TranslateAlignment(DataGridViewContentAlignment align)
      => (align & anyRight) != 0
        ? HorizontalAlignment.Right
        : (align & anyCenter) != 0
          ? HorizontalAlignment.Center
          : HorizontalAlignment.Left;
  }
}
