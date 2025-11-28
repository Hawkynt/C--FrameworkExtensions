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

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms;

public partial class DataGridViewNumericUpDownColumn {
  internal class DataGridViewNumericUpDownCell : DataGridViewTextBoxCell {

    // Used in KeyEntersEditMode function
    [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
    private static extern short VkKeyScan(char key);

    // Used in TranslateAlignment function
    private static readonly DataGridViewContentAlignment _ANY_RIGHT = DataGridViewContentAlignment.TopRight | DataGridViewContentAlignment.MiddleRight | DataGridViewContentAlignment.BottomRight;
    private static readonly DataGridViewContentAlignment _ANY_CENTER = DataGridViewContentAlignment.TopCenter | DataGridViewContentAlignment.MiddleCenter | DataGridViewContentAlignment.BottomCenter;

    // Default dimensions of the static rendering bitmap used for the painting of the non-edited cells
    private const int _DEFAULT_RENDERING_BITMAP_WIDTH = 100;
    private const int _DEFAULT_RENDERING_BITMAP_HEIGHT = 22;

    // Default value of the DecimalPlaces property
    internal const int DEFAULT_DECIMAL_PLACES = 0;

    // Default value of the Increment property
    internal const decimal DEFAULT_INCREMENT = decimal.One;

    // Default value of the Maximum property
    internal const decimal DEFAULT_MAXIMUM = (decimal)100.0;

    // Default value of the Minimum property
    internal const decimal DEFAULT_MINIMUM = decimal.Zero;

    // Default value of the ThousandsSeparator property
    internal const bool DEFAULT_THOUSANDS_SEPARATOR = false;

    // Type of this cell's editing control
    private static readonly Type _DEFAULT_EDIT_TYPE = typeof(DataGridViewNumericUpDownEditingControl);

    // Type of this cell's value. The formatted value type is string, the same as the base class DataGridViewTextBoxCell
    private static readonly Type _DEFAULT_VALUE_TYPE = typeof(decimal);

    // The bitmap used to paint the non-edited cells via a call to NumericUpDown.DrawToBitmap
    [ThreadStatic] private static Bitmap _renderingBitmap;

    // The NumericUpDown control used to paint the non-edited cells via a call to NumericUpDown.DrawToBitmap
    [ThreadStatic] private static NumericUpDown _paintingNumericUpDown;

    private int _decimalPlaces; // Caches the value of the DecimalPlaces property
    private decimal _increment; // Caches the value of the Increment property
    private decimal _minimum; // Caches the value of the Minimum property
    private decimal _maximum; // Caches the value of the Maximum property
    private bool _useThousandsSeparator; // Caches the value of the ThousandsSeparator property

    private string _decimalPlacesPropertyName; // Property name for binding DecimalPlaces
    private string _incrementPropertyName; // Property name for binding Increment
    private string _minimumPropertyName; // Property name for binding Minimum
    private string _maximumPropertyName; // Property name for binding Maximum
    private string _useThousandsSeparatorPropertyName; // Property name for binding UseThousandsSeparator

    /// <summary>
    ///   Constructor for the DataGridViewNumericUpDownCell cell type
    /// </summary>
    public DataGridViewNumericUpDownCell() : this(null, null, null, null, null) {
    }

    /// <summary>
    ///   Constructor for the DataGridViewNumericUpDownCell cell type with property bindings
    /// </summary>
    public DataGridViewNumericUpDownCell(
      string decimalPlacesPropertyName,
      string incrementPropertyName,
      string minimumPropertyName,
      string maximumPropertyName,
      string useThousandsSeparatorPropertyName
    ) {
      this._decimalPlacesPropertyName = decimalPlacesPropertyName;
      this._incrementPropertyName = incrementPropertyName;
      this._minimumPropertyName = minimumPropertyName;
      this._maximumPropertyName = maximumPropertyName;
      this._useThousandsSeparatorPropertyName = useThousandsSeparatorPropertyName;
      
      // a thread specific bitmap used for the painting of the non-edited cells
      _renderingBitmap ??= new(
        _DEFAULT_RENDERING_BITMAP_WIDTH,
        _DEFAULT_RENDERING_BITMAP_HEIGHT
      );

      // a thread specific NumericUpDown control used for the painting of the non-edited cells
      _paintingNumericUpDown ??= new() {
        BorderStyle = BorderStyle.None, 
        Maximum = decimal.MaxValue / 10, 
        Minimum = decimal.MinValue / 10
      };

      // Set the default values of the properties:
      this._decimalPlaces = DEFAULT_DECIMAL_PLACES;
      this._increment = DEFAULT_INCREMENT;
      this._minimum = DEFAULT_MINIMUM;
      this._maximum = DEFAULT_MAXIMUM;
      this._useThousandsSeparator = DEFAULT_THOUSANDS_SEPARATOR;
    }

    /// <summary>
    ///   The DecimalPlaces property replicates the one from the NumericUpDown control
    /// </summary>
    [DefaultValue(DEFAULT_DECIMAL_PLACES)]
    public int DecimalPlaces {
      get => this._decimalPlaces;
      set {
        if (value is < 0 or > 99)
          throw new ArgumentOutOfRangeException(
            nameof(value),
            "The DecimalPlaces property cannot be smaller than 0 or larger than 99."
          );

        if (this._decimalPlaces == value)
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
    public override Type EditType => _DEFAULT_EDIT_TYPE;

    /// <summary>
    ///   The Increment property replicates the one from the NumericUpDown control
    /// </summary>
    public decimal Increment {
      get => this._increment;
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
      get => this._maximum;
      set {
        if (this._maximum == value)
          return;

        this.SetMaximum(this.RowIndex, value);
        this.OnCommonChange();
      }
    }

    /// <summary>
    ///   The Minimum property replicates the one from the NumericUpDown control
    /// </summary>
    public decimal Minimum {
      get => this._minimum;
      set {
        if (this._minimum == value)
          return;

        this.SetMinimum(this.RowIndex, value);
        this.OnCommonChange();
      }
    }

    /// <summary>
    ///   The ThousandsSeparator property replicates the one from the NumericUpDown control
    /// </summary>
    [DefaultValue(DEFAULT_THOUSANDS_SEPARATOR)]
    public bool UseThousandsSeparator {
      get => this._useThousandsSeparator;
      set {
        if (this._useThousandsSeparator == value)
          return;

        this.SetThousandsSeparator(this.RowIndex, value);
        this.OnCommonChange();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Returns the type of the cell's Value property
    /// </summary>
    public override Type ValueType => base.ValueType ?? _DEFAULT_VALUE_TYPE;

    /// <summary>
    ///   Clones a DataGridViewNumericUpDownCell cell, copies all the custom properties.
    /// </summary>
    public override object Clone() {
      var result = (DataGridViewNumericUpDownCell)base.Clone();
      result.DecimalPlaces = this.DecimalPlaces;
      result.Increment = this.Increment;
      result.Maximum = this.Maximum;
      result.Minimum = this.Minimum;
      result.UseThousandsSeparator = this.UseThousandsSeparator;
      result._decimalPlacesPropertyName = this._decimalPlacesPropertyName;
      result._incrementPropertyName = this._incrementPropertyName;
      result._maximumPropertyName = this._maximumPropertyName;
      result._minimumPropertyName = this._minimumPropertyName;
      result._useThousandsSeparatorPropertyName = this._useThousandsSeparatorPropertyName;
      return result;
    }

    /// <summary>
    ///   Returns the provided value constrained to be within the min and max.
    /// </summary>
    private decimal Constrain(decimal value) {
      if (this._minimum > this._maximum)
        Debug.Fail("min must be <= max");

      if (value < this._minimum)
        value = this._minimum;

      if (value > this._maximum)
        value = this._maximum;

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
      return unformattedDecimal.ToString((this.UseThousandsSeparator ? "N" : "F") + this.DecimalPlaces);
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

      // Apply property bindings if available, otherwise use cell properties
      var owningRowDataBoundItem = this.OwningRow?.DataBoundItem;
      if (owningRowDataBoundItem != null) {
        numericUpDown.DecimalPlaces = DataGridViewExtensions.GetPropertyValueOrDefault(
          owningRowDataBoundItem,
          this._decimalPlacesPropertyName,
          this.DecimalPlaces,
          this.DecimalPlaces,
          this.DecimalPlaces,
          this.DecimalPlaces
        );

        numericUpDown.Increment = DataGridViewExtensions.GetPropertyValueOrDefault(
          owningRowDataBoundItem,
          this._incrementPropertyName,
          this.Increment,
          this.Increment,
          this.Increment,
          this.Increment
        );

        numericUpDown.Maximum = DataGridViewExtensions.GetPropertyValueOrDefault(
          owningRowDataBoundItem,
          this._maximumPropertyName,
          this.Maximum,
          this.Maximum,
          this.Maximum,
          this.Maximum
        );

        numericUpDown.Minimum = DataGridViewExtensions.GetPropertyValueOrDefault(
          owningRowDataBoundItem,
          this._minimumPropertyName,
          this.Minimum,
          this.Minimum,
          this.Minimum,
          this.Minimum
        );

        numericUpDown.ThousandsSeparator = DataGridViewExtensions.GetPropertyValueOrDefault(
          owningRowDataBoundItem,
          this._useThousandsSeparatorPropertyName,
          this.UseThousandsSeparator,
          this.UseThousandsSeparator,
          this.UseThousandsSeparator,
          this.UseThousandsSeparator
        );
      } else {
        numericUpDown.DecimalPlaces = this.DecimalPlaces;
        numericUpDown.Increment = this.Increment;
        numericUpDown.Maximum = this.Maximum;
        numericUpDown.Minimum = this.Minimum;
        numericUpDown.ThousandsSeparator = this.UseThousandsSeparator;
      }

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

        var renderingBitmap = _renderingBitmap;
        if (renderingBitmap.Width < valBounds.Width || renderingBitmap.Height < valBounds.Height) {
          // The static bitmap is too small, a bigger one needs to be allocated.
          renderingBitmap.Dispose();
          _renderingBitmap = renderingBitmap = new(valBounds.Width, valBounds.Height);
        }

        // Make sure the NumericUpDown control is parented to a visible control
        var numericUpDown = _paintingNumericUpDown;
        if (numericUpDown.Parent is not { Visible: true })
          numericUpDown.Parent = this.DataGridView;

        // Set all the relevant properties
        numericUpDown.TextAlign = TranslateAlignment(cellStyle.Alignment);
        numericUpDown.DecimalPlaces = this.DecimalPlaces;
        numericUpDown.ThousandsSeparator = this.UseThousandsSeparator;
        numericUpDown.Font = cellStyle.Font;
        numericUpDown.Width = valBounds.Width;
        numericUpDown.Height = valBounds.Height;
        numericUpDown.RightToLeft = this.DataGridView.RightToLeft;
        numericUpDown.Location = new(0, -numericUpDown.Height - 100);
        numericUpDown.Text = formattedValue as string;

        Color backColor;
        if (PartPainted(paintParts, DataGridViewPaintParts.SelectionBackground) && cellSelected)
          backColor = cellStyle.SelectionBackColor;
        else
          backColor = cellStyle.BackColor;

        if (PartPainted(paintParts, DataGridViewPaintParts.Background)) {
          
          // The NumericUpDown control does not support transparent back colors
          if (backColor.A < 255)
            backColor = Color.FromArgb(255, backColor);

          numericUpDown.BackColor = backColor;
        }

        // Finally paint the NumericUpDown control
        var srcRect = valBounds with { X = 0, Y = 0 };
        if (srcRect is { Width: > 0, Height: > 0 }) {
          numericUpDown.DrawToBitmap(renderingBitmap, srcRect);
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
      
      var editingControl = this.DataGridView.EditingControl;
      editingControl.Location = new(editingControlBounds.X, editingControlBounds.Y);
      editingControl.Size = new(editingControlBounds.Width, editingControlBounds.Height);
    }

    /// <summary>
    ///   Utility function that sets a new value for the DecimalPlaces property of the cell. This function is used by
    ///   the cell and column DecimalPlaces property. The column uses this method instead of the DecimalPlaces
    ///   property for performance reasons. This way the column can invalidate the entire column at once instead of
    ///   invalidating each cell of the column individually. A row index needs to be provided as a parameter because
    ///   this cell may be shared among multiple rows.
    /// </summary>
    public void SetDecimalPlaces(int rowIndex, int value) {
      this._decimalPlaces = value;
      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.DecimalPlaces = value;
    }

    /// Utility function that sets a new value for the Increment property of the cell. This function is used by
    /// the cell and column Increment property. A row index needs to be provided as a parameter because
    /// this cell may be shared among multiple rows.
    public void SetIncrement(int rowIndex, decimal value) {
      this._increment = value;
      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.Increment = value;
    }

    /// Utility function that sets a new value for the Maximum property of the cell. This function is used by
    /// the cell and column Maximum property. The column uses this method instead of the Maximum
    /// property for performance reasons. This way the column can invalidate the entire column at once instead of 
    /// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
    /// this cell may be shared among multiple rows.
    public void SetMaximum(int rowIndex, decimal value) {
      this._maximum = value;
      if (this._minimum > this._maximum)
        this._minimum = this._maximum;
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
      this._minimum = value;
      if (this._minimum > this._maximum)
        this._maximum = value;
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
      this._useThousandsSeparator = value;
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
      => (align & _ANY_RIGHT) != 0
        ? HorizontalAlignment.Right
        : (align & _ANY_CENTER) != 0
          ? HorizontalAlignment.Center
          : HorizontalAlignment.Left;
  }
}
