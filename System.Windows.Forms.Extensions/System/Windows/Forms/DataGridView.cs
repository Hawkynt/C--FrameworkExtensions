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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
#if SUPPORTS_CONDITIONAL_WEAK_TABLE
using System.Runtime.CompilerServices;
#endif
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Form.Extensions;
using System.Windows.Forms.VisualStyles;
using ThreadTimer = System.Threading.Timer;
using DrawingSystemColors = System.Drawing.SystemColors;
using DrawingSize = System.Drawing.Size;
using DrawingFontStyle = System.Drawing.FontStyle;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

// TODO: buttoncolumn with image support
namespace System.Windows.Forms;

#region custom datagridviewcolumns

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class BoundDataGridViewComboBoxColumn : DataGridViewColumn {
  public string DataSourcePropertyName { get; }
  public string EnabledWhenPropertyName { get; }
  public string ValueMember { get; }
  public string DisplayMember { get; }

  public BoundDataGridViewComboBoxColumn(string dataSourcePropertyName, string enabledWhenPropertyName,
    string valueMember, string displayMember) {
    var cell = new BoundDataGridViewComboBoxCell(dataSourcePropertyName, enabledWhenPropertyName, valueMember,
      displayMember);

    this.DataSourcePropertyName = dataSourcePropertyName;
    this.EnabledWhenPropertyName = enabledWhenPropertyName;
    this.ValueMember = valueMember;
    this.DisplayMember = displayMember;

    // ReSharper disable once VirtualMemberCallInConstructor
    this.CellTemplate = cell;
  }

  #region Overrides of DataGridViewColumn

  public override object Clone() {
    var result = new BoundDataGridViewComboBoxColumn(this.DataSourcePropertyName, this.EnabledWhenPropertyName,
      this.ValueMember, this.DisplayMember) {
      Name = this.Name,
      DisplayIndex = this.DisplayIndex,
      HeaderText = this.HeaderText,
      DataPropertyName = this.DataPropertyName,
      AutoSizeMode = this.AutoSizeMode,
      SortMode = this.SortMode,
      FillWeight = this.FillWeight
    };
    return result;
  }

  #endregion

  #region Overrides of DataGridViewBand

  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      // Ensure that the cell used for the template is a BoundDataGridViewComboBoxCell.
      if (value != null && !value.GetType().IsAssignableFrom(typeof(BoundDataGridViewComboBoxCell)))
        throw new InvalidCastException(nameof(BoundDataGridViewComboBoxCell));

      base.CellTemplate = value;
    }
  }

  #endregion

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

  internal class BoundDataGridViewComboBoxCell : DataGridViewTextBoxCell {
    public string DataSourcePropertyName { get; set; }
    public string EnabledWhenPropertyName { get; set; }
    public string ValueMember { get; set; }
    public string DisplayMember { get; set; }

    public BoundDataGridViewComboBoxCell() { }

    public BoundDataGridViewComboBoxCell(string dataSourcePropertyName, string enabledWhenPropertyName,
      string valueMember, string displayMember) {
      this.DataSourcePropertyName = dataSourcePropertyName;
      this.EnabledWhenPropertyName = enabledWhenPropertyName;
      this.ValueMember = valueMember;
      this.DisplayMember = displayMember;
    }

    public override Type EditType => typeof(DataGridViewComboBoxEditingControl);

    public override Type ValueType { get; set; }

    public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle,
      TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter) =>
      Convert.ChangeType(formattedValue, this.ValueType);

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue,
      DataGridViewCellStyle dataGridViewCellStyle) {
      base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
      if (this.DataGridView.EditingControl is not ComboBox comboBox)
        return;

      var owningRowDataBoundItem = this.OwningRow.DataBoundItem;
      var source = DataGridViewExtensions.GetPropertyValueOrDefault<IEnumerable>(owningRowDataBoundItem,
        this.DataSourcePropertyName, null, null, null, null);
      comboBox.DataSource = source;


      if (this.EnabledWhenPropertyName != null)
        comboBox.Enabled = DataGridViewExtensions.GetPropertyValueOrDefault(owningRowDataBoundItem,
          this.EnabledWhenPropertyName, true, true, true, true);

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

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewProgressBarColumn : DataGridViewTextBoxColumn {
  public class DataGridViewProgressBarCell : DataGridViewTextBoxCell {
    public DataGridViewProgressBarCell() {
      this.Maximum = 100;
      this.Minimum = 0;
    }

    public double Maximum { get; set; }
    public double Minimum { get; set; }
    public override object DefaultNewRowValue => 0;

    public override object Clone() {
      var cell = (DataGridViewProgressBarCell)base.Clone();
      cell.Maximum = this.Maximum;
      cell.Minimum = this.Minimum;
      return cell;
    }

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
      var intValue = 0d;
      if (value is int i)
        intValue = i;
      if (value is float f)
        intValue = f;
      if (value is double d)
        intValue = d;
      if (value is decimal dec)
        intValue = (double)dec;

      if (intValue < this.Minimum)
        intValue = this.Minimum;

      if (intValue > this.Maximum)
        intValue = this.Maximum;

      var rate = (intValue - this.Minimum) / (this.Maximum - this.Minimum);

      if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
        this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

      var borderRect = this.BorderWidths(advancedBorderStyle);
      var paintRect = new Rectangle(cellBounds.Left + borderRect.Left, cellBounds.Top + borderRect.Top,
        cellBounds.Width - borderRect.Right, cellBounds.Height - borderRect.Bottom);

      var isSelected = cellState._FOS_HasFlag(DataGridViewElementStates.Selected);
      var bkColor =
          isSelected && paintParts._FOS_HasFlag(DataGridViewPaintParts.SelectionBackground)
            ? cellStyle.SelectionBackColor
            : cellStyle.BackColor
        ;

      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.Background))
        using (var backBrush = new SolidBrush(bkColor))
          graphics.FillRectangle(backBrush, paintRect);

      paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
      paintRect.Width -= cellStyle.Padding.Horizontal;
      paintRect.Height -= cellStyle.Padding.Vertical;

      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.ContentForeground)) {
        if (ProgressBarRenderer.IsSupported) {
          ProgressBarRenderer.DrawHorizontalBar(graphics, paintRect);
          var barBounds = new Rectangle(paintRect.Left + 3, paintRect.Top + 3, paintRect.Width - 4,
            paintRect.Height - 6);
          barBounds.Width = Convert.ToInt32(Math.Round(barBounds.Width * rate));
          ProgressBarRenderer.DrawHorizontalChunks(graphics, barBounds);
        } else {
          graphics.FillRectangle(Brushes.White, paintRect);
          graphics.DrawRectangle(Pens.Black, paintRect);
          var barBounds = new Rectangle(paintRect.Left + 1, paintRect.Top + 1, paintRect.Width - 1,
            paintRect.Height - 1);
          barBounds.Width = Convert.ToInt32(Math.Round(barBounds.Width * rate));
          graphics.FillRectangle(Brushes.Blue, barBounds);
        }
      }

      if (this.DataGridView.CurrentCellAddress.X == this.ColumnIndex &&
          this.DataGridView.CurrentCellAddress.Y == this.RowIndex &&
          paintParts._FOS_HasFlag(DataGridViewPaintParts.Focus) && this.DataGridView.Focused) {
        var focusRect = paintRect;
        focusRect.Inflate(-3, -3);
        ControlPaint.DrawFocusRectangle(graphics, focusRect);
      }

      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.ContentForeground)) {
        var txt = $"{Math.Round(rate * 100)}%";
        const TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
        var fColor = cellStyle.ForeColor;
        paintRect.Inflate(-2, -2);
        TextRenderer.DrawText(graphics, txt, cellStyle.Font, paintRect, fColor, flags);
      }

      if (!paintParts._FOS_HasFlag(DataGridViewPaintParts.ErrorIcon) || !this.DataGridView.ShowCellErrors ||
          string.IsNullOrEmpty(errorText))
        return;

      var iconBounds = this.GetErrorIconBounds(graphics, cellStyle, rowIndex);
      iconBounds.Offset(cellBounds.X, cellBounds.Y);
      this.PaintErrorIcon(graphics, iconBounds, cellBounds, errorText);
    }
  }

  public DataGridViewProgressBarColumn() => this.CellTemplate = new DataGridViewProgressBarCell();

  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      if (value is DataGridViewProgressBarCell)
        base.CellTemplate = value;
      else
        throw new InvalidCastException(nameof(DataGridViewProgressBarCell));
    }
  }

  public double Maximum {
    get => ((DataGridViewProgressBarCell)this.CellTemplate).Maximum;
    set {
      if (this.Maximum == value)
        return;

      ((DataGridViewProgressBarCell)this.CellTemplate).Maximum = value;

      if (this.DataGridView == null)
        return;

      var rowCount = this.DataGridView.RowCount;
      for (var i = 0; i <= rowCount - 1; ++i) {
        var r = this.DataGridView.Rows.SharedRow(i);
        ((DataGridViewProgressBarCell)r.Cells[this.Index]).Maximum = value;
      }
    }
  }

  public double Minimum {
    get => ((DataGridViewProgressBarCell)this.CellTemplate).Minimum;
    set {
      if (this.Minimum == value)
        return;

      ((DataGridViewProgressBarCell)this.CellTemplate).Minimum = value;

      if (this.DataGridView == null)
        return;

      var rowCount = this.DataGridView.RowCount;
      for (var i = 0; i <= rowCount - 1; i++) {
        var r = this.DataGridView.Rows.SharedRow(i);
        ((DataGridViewProgressBarCell)r.Cells[this.Index]).Minimum = value;
      }
    }
  }
}

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewDisableButtonColumn : DataGridViewButtonColumn {
  /// <summary>
  ///   The cell template to use for drawing the cells' content.
  /// </summary>
  public class DataGridViewDisableButtonCell : DataGridViewButtonCell {
    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="DataGridViewDisableButtonCell" /> is enabled.
    /// </summary>
    /// <value>
    ///   <c>true</c> if enabled; otherwise, <c>false</c>.
    /// </value>
    public bool Enabled { get; set; }

    // Override the Clone method so that the Enabled property is copied.
    public override object Clone() {
      var cell = (DataGridViewDisableButtonCell)base.Clone();
      cell.Enabled = this.Enabled;
      return cell;
    }

    // By default, enable the button cell.
    public DataGridViewDisableButtonCell() => this.Enabled = true;

    protected override void Paint(
      Graphics graphics,
      Rectangle clipBounds,
      Rectangle cellBounds,
      int rowIndex,
      DataGridViewElementStates elementState,
      object value,
      object formattedValue,
      string errorText,
      DataGridViewCellStyle cellStyle,
      DataGridViewAdvancedBorderStyle advancedBorderStyle,
      DataGridViewPaintParts paintParts) {
      // If button cell is enabled, let the base class draw everything.
      var isEnabled = this.Enabled;

      // The button cell is disabled, so paint the border,
      // background, and disabled button for the cell.

      // Draw the cell background, if specified.
      var backColor = cellStyle.BackColor;
      if (backColor == Color.Empty)
        backColor = DrawingSystemColors.Control;

      if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
        using (var cellBackground = new SolidBrush(backColor))
          graphics.FillRectangle(cellBackground, cellBounds);

      // Draw the cell borders, if specified.
      if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
        this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

      // Calculate the area in which to draw the button.
      var buttonArea = cellBounds;
      var buttonAdjustment = this.BorderWidths(advancedBorderStyle);
      buttonArea.X += buttonAdjustment.X;
      buttonArea.Y += buttonAdjustment.Y;
      buttonArea.Height -= buttonAdjustment.Height;
      buttonArea.Width -= buttonAdjustment.Width;

      // Draw the button
      if (isEnabled) {
        int _Clamp(int a) => a < byte.MinValue ? byte.MinValue : a > byte.MaxValue ? byte.MaxValue : a;
        const int shadingAmount = 64;

        var lighterColor = Color.FromArgb(backColor.A, _Clamp(backColor.R + shadingAmount),
          _Clamp(backColor.G + shadingAmount), _Clamp(backColor.B + shadingAmount));
        var darkerColor = Color.FromArgb(backColor.A, _Clamp(backColor.R - shadingAmount),
          _Clamp(backColor.G - shadingAmount), _Clamp(backColor.B - shadingAmount));

        var borderWidth = 3;
        buttonArea.Inflate(-borderWidth / 2, -borderWidth / 2);

        using (var pen = new Pen(lighterColor, borderWidth)) {
          graphics.DrawLine(pen, buttonArea.Left, buttonArea.Top, buttonArea.Right, buttonArea.Top);
          graphics.DrawLine(pen, buttonArea.Left, buttonArea.Top, buttonArea.Left, buttonArea.Bottom);
        }

        using (var pen = new Pen(darkerColor, borderWidth)) {
          graphics.DrawLine(pen, buttonArea.Right, buttonArea.Bottom, buttonArea.Left, buttonArea.Bottom);
          graphics.DrawLine(pen, buttonArea.Right, buttonArea.Bottom, buttonArea.Right, buttonArea.Top);
        }
      } else
        ButtonRenderer.DrawButton(graphics, buttonArea, PushButtonState.Disabled);

      // draw button text
      var s = formattedValue?.ToString() ?? string.Empty;
      Color foreColor;
      if (isEnabled) {
        foreColor = cellStyle.ForeColor;
        if (foreColor == Color.Empty)
          foreColor = DrawingSystemColors.ControlText;
      } else
        foreColor = DrawingSystemColors.GrayText;

      TextRenderer.DrawText(graphics, s, this.DataGridView.Font, buttonArea, foreColor);
    }
  }

  public DataGridViewDisableButtonColumn() => this.CellTemplate = new DataGridViewDisableButtonCell();
}

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewMultiImageColumn : DataGridViewTextBoxColumn {
  private Action<object, int> _onClickMethod;
  private Func<object, int, string> _tooltipTextProvider;

  private readonly string _onClickMethodName;
  private readonly string _toolTipTextProviderMethodName;

  public DataGridViewMultiImageColumn(int imageSize, Padding padding, Padding margin, string onClickMethodName,
    string toolTipTextProviderMethodName) {
    this._onClickMethodName = onClickMethodName;
    this._toolTipTextProviderMethodName = toolTipTextProviderMethodName;

    var cell = new DataGridViewMultiImageCell {
      ImageSize = imageSize,
      Padding = padding,
      Margin = margin,
    };

    // ReSharper disable once VirtualMemberCallInConstructor
    this.CellTemplate = cell;
  }

  #region Overrides of DataGridViewColumn

  public override object Clone() {
    var cell = (DataGridViewMultiImageCell)this.CellTemplate;
    var result = new DataGridViewMultiImageColumn(cell.ImageSize, cell.Padding, cell.Margin, this._onClickMethodName,
      this._toolTipTextProviderMethodName) {
      Name = this.Name,
      DisplayIndex = this.DisplayIndex,
      HeaderText = this.HeaderText,
      DataPropertyName = this.DataPropertyName,
      AutoSizeMode = this.AutoSizeMode,
      SortMode = this.SortMode,
      FillWeight = this.FillWeight
    };
    return result;
  }

  #endregion

  #region Overrides of DataGridViewBand

  protected override void OnDataGridViewChanged() {
    if (this.DataGridView == null)
      return;

    var itemType = this.DataGridView.FindItemType();

    var method = GetMethodInfoOrDefault(itemType, this._onClickMethodName);
    if (method != null)
      this._onClickMethod = _GenerateObjectInstanceActionDelegate<int>(method);

    method = GetMethodInfoOrDefault(itemType, this._toolTipTextProviderMethodName);
    if (method != null)
      this._tooltipTextProvider = _GenerateObjectInstanceFunctionDelegate<int>(method);
  }

  private static MethodInfo GetMethodInfoOrDefault(Type itemType, string methodName) {
    if (itemType == null)
      return null;

    return methodName == null
      ? null
      : itemType.GetMethod(methodName,
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
  }

  private static Action<object, TParam0> _GenerateObjectInstanceActionDelegate<TParam0>(MethodInfo method) {
    var dynamicMethod = GenerateIL<TParam0>(method, typeof(void));

    return (Action<object, TParam0>)dynamicMethod.CreateDelegate(typeof(Action<object, TParam0>));
  }

  private static Func<object, TParam0, string> _GenerateObjectInstanceFunctionDelegate<TParam0>(MethodInfo method) {
    var dynamicMethod = GenerateIL<TParam0>(method, typeof(string));

    return (Func<object, TParam0, string>)dynamicMethod.CreateDelegate(typeof(Func<object, TParam0, string>));
  }

  private static DynamicMethod GenerateIL<TParam0>(MethodInfo method, Type returnType) {
    if (method == null)
      throw new ArgumentNullException(nameof(method));
    if (method.GetParameters().Length != 1)
      throw new ArgumentException("Method needs exactly one parameter", nameof(method));

    var dynamicMethod = new DynamicMethod(string.Empty, returnType, new[] { typeof(object), typeof(TParam0) }, true);
    var generator = dynamicMethod.GetILGenerator();

    if (!method.IsStatic) {
      generator.Emit(OpCodes.Ldarg_0);
      generator.Emit(OpCodes.Castclass, method.DeclaringType);
    }

    generator.Emit(OpCodes.Ldarg_1);
    generator.EmitCall(OpCodes.Call, method, null);
    generator.Emit(OpCodes.Ret);

    return dynamicMethod;
  }

  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      // Ensure that the cell used for the template is a MultiImageCell.
      if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewMultiImageCell)))
        throw new InvalidCastException(nameof(DataGridViewMultiImageCell));

      base.CellTemplate = value;
    }
  }

  #endregion

  internal class DataGridViewMultiImageCell : DataGridViewTextBoxCell {
    private readonly List<CellImage> _images = new();
    private DrawingSize? _oldCellBounds;

    private static readonly ToolTip tooltip = new() { Active = true, ShowAlways = true };
    private bool ShowCellToolTipCacheValue;

    public int ImageSize { get; set; }
    public Padding Margin { get; set; }
    public Padding Padding { get; set; }

    #region Overrides of DataGridViewCell

    protected override void OnMouseMove(DataGridViewCellMouseEventArgs e) {
      var text = string.Empty;

      for (var i = 0; i < this._images.Count; ++i) {
        var image = this._images[i];

        image.IsHovered = image.Bounds.Contains(e.Location);
        this._images[i] = image;

        if (!image.Bounds.Contains(e.Location))
          continue;

        text = ((DataGridViewMultiImageColumn)this.OwningColumn)._tooltipTextProvider?.Invoke(
          this.DataGridView.Rows[e.RowIndex].DataBoundItem, i) ?? string.Empty;
      }

      this.DataGridView.InvalidateCell(this);

      this.ShowCellToolTipCacheValue = this.DataGridView.ShowCellToolTips;
      this.DataGridView.ShowCellToolTips = false;

      if (tooltip.Tag != null && (string)tooltip.Tag == text) {
        this.DataGridView.ShowCellToolTips = this.ShowCellToolTipCacheValue;
        return;
      }

      var cellBounds = this.DataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

      tooltip.Tag = text;
      tooltip.Show(text, this.DataGridView, e.Location.X + cellBounds.X + this.ImageSize, e.Location.Y + cellBounds.Y);

      this.DataGridView.ShowCellToolTips = this.ShowCellToolTipCacheValue;
    }

    protected override void OnMouseLeave(int rowIndex) {
      for (var i = 0; i < this._images.Count; ++i) {
        var image = this._images[i];

        image.IsHovered = false;
        this._images[i] = image;
      }

      tooltip.Hide(this.DataGridView);
      this.DataGridView.InvalidateCell(this);
    }

    protected override void OnMouseClick(DataGridViewCellMouseEventArgs e) {
      tooltip.UseAnimation = false;
      tooltip.Hide(this.DataGridView);
      tooltip.UseAnimation = true;

      for (var i = 0; i < this._images.Count; ++i) {
        var image = this._images[i];

        if (!image.Bounds.Contains(e.Location))
          continue;

        ((DataGridViewMultiImageColumn)this.OwningColumn)._onClickMethod?.Invoke(
          this.DataGridView.Rows[e.RowIndex].DataBoundItem, i);
      }
    }

    public override object Clone() {
      var cell = (DataGridViewMultiImageCell)base.Clone();
      cell.ImageSize = this.ImageSize;
      return cell;
    }

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
      DataGridViewElementStates cellState, object value, object formattedValue, string errorText,
      DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
      DataGridViewPaintParts paintParts) {
      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.Border))
        this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

      var borderRect = this.BorderWidths(advancedBorderStyle);
      var paintRect = new Rectangle(cellBounds.Left + borderRect.Left, cellBounds.Top + borderRect.Top,
        cellBounds.Width - borderRect.Right, cellBounds.Height - borderRect.Bottom);

      var isSelected = cellState._FOS_HasFlag(DataGridViewElementStates.Selected);
      var bkColor = isSelected && paintParts._FOS_HasFlag(DataGridViewPaintParts.SelectionBackground)
          ? cellStyle.SelectionBackColor
          : cellStyle.BackColor
        ;

      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.Background))
        using (var backBrush = new SolidBrush(bkColor))
          graphics.FillRectangle(backBrush, paintRect);

      paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
      paintRect.Width -= cellStyle.Padding.Horizontal;
      paintRect.Height -= cellStyle.Padding.Vertical;

      var images = value == null ? new Image[] { } : (Image[])value;
      var count = images.Length;

      if (!this._oldCellBounds.HasValue || !this._oldCellBounds.Equals(paintRect.Size) || this._images.Count != count) {
        this._oldCellBounds = paintRect.Size;
        this._RecreateDrawingPanel(paintRect, count);
      }

      for (var i = 0; i < this._images.Count; ++i) {
        var imageRect = this._images[i];

        if (imageRect.IsHovered)
          using (var hoverBrush = new SolidBrush(isSelected ? cellStyle.BackColor : cellStyle.SelectionBackColor))
            graphics.FillRectangle(hoverBrush, imageRect.Bounds.X + paintRect.X,
              imageRect.Bounds.Y + paintRect.Y,
              imageRect.Bounds.Size.Width,
              imageRect.Bounds.Size.Height);

        graphics.DrawImage(images[i],
          imageRect.Bounds.X + paintRect.X + this.Padding.Left,
          imageRect.Bounds.Y + paintRect.Y + this.Padding.Top,
          imageRect.Bounds.Size.Width - (this.Padding.Left + this.Padding.Right),
          imageRect.Bounds.Size.Height - (this.Padding.Top + this.Padding.Bottom));
      }
    }

    #endregion

    private void _RecreateDrawingPanel(Rectangle cellBounds, int imageCount) {
      var size = this.ImageSize;
      var maxImages = cellBounds.Width / (size + this.Margin.Left + this.Margin.Right) *
                      (cellBounds.Height / (size + this.Margin.Top + this.Margin.Bottom));

      //resizing
      while (maxImages < imageCount) {
        size -= 8;

        maxImages = cellBounds.Width / (size + this.Margin.Left + this.Margin.Right) *
                    (cellBounds.Height / (size + this.Margin.Top + this.Margin.Bottom));
      }

      this._images.Clear();

      var x = this.Margin.Left;
      var y = this.Margin.Top;

      for (var i = 0; i < imageCount; ++i) {
        if (x + size + this.Margin.Right > cellBounds.Width) {
          x = this.Margin.Left;
          y += size + this.Margin.Bottom;
        }

        this._images.Add(new(new(x, y, size, size)));
        x += size + this.Margin.Right;
      }
    }

    private struct CellImage {
      public CellImage(Rectangle bounds) {
        this.Bounds = bounds;
        this.IsHovered = false;
      }

      public Rectangle Bounds { get; }
      public bool IsHovered { get; set; }
    }
  }

  protected virtual void OnOnImageItemSelected(object arg1, int arg2) => this._onClickMethod?.Invoke(arg1, arg2);
}

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewImageAndTextColumn : DataGridViewTextBoxColumn {
  private Image imageValue;

  public DataGridViewImageAndTextColumn() => this.CellTemplate = new DataGridViewTextAndImageCell();

  public override object Clone() {
    var c = base.Clone() as DataGridViewImageAndTextColumn;
    c.imageValue = this.imageValue;
    c.ImageSize = this.ImageSize;
    return c;
  }

  public Image Image {
    get => this.imageValue;
    set {
      if (this.Image == value)
        return;

      this.imageValue = value;
      this.ImageSize = value.Size;

      if (this.InheritedStyle == null)
        return;

      var inheritedPadding = this.InheritedStyle.Padding;
      this.DefaultCellStyle.Padding = new(this.ImageSize.Width,
        inheritedPadding.Top, inheritedPadding.Right,
        inheritedPadding.Bottom);
    }
  }

  internal DrawingSize ImageSize { get; private set; }

  public class DataGridViewTextAndImageCell : DataGridViewTextBoxCell {
    private Image _imageValue;
    private DrawingSize _imageSize;
    private bool _needsResize;

    public TextImageRelation TextImageRelation { get; set; }
    public bool KeepAspectRatio { get; set; }
    public uint FixedImageWidth { get; set; }
    public uint FixedImageHeight { get; set; }

    public override object Clone() {
      var c = base.Clone() as DataGridViewTextAndImageCell;
      c._imageValue = this._imageValue;
      c._imageSize = this._imageSize;
      return c;
    }

    public Image Image {
      get {
        if (this.OwningColumn == null || this._OwningDataGridViewImageAndTextColumn == null)
          return this._imageValue;

        return this._imageValue ?? this._OwningDataGridViewImageAndTextColumn.Image;
      }
      set {
        this._imageValue = value;
        this._needsResize = false;
        if (value == null)
          this._imageSize = DrawingSize.Empty;
        else {
          var size = value.Size;
          var fixedWidth = this.FixedImageWidth;
          var fixedHeight = this.FixedImageHeight;
          var keepAspectRatio = this.KeepAspectRatio;
          var width = size.Width;
          var height = size.Height;

          if (fixedWidth > 0) {
            if (keepAspectRatio)
              height = (int)((float)height * fixedWidth / width);
            else if (fixedHeight > 0)
              height = (int)fixedHeight;

            width = (int)fixedWidth;
          } else if (fixedHeight > 0) {
            if (keepAspectRatio)
              width = (int)((float)width * fixedHeight / height);

            height = (int)fixedHeight;
          }

          this._needsResize = width != size.Width || height != size.Height;
          this._imageSize = new(width, height);
        }

        var inheritedPadding = this.InheritedStyle.Padding;
        Padding padding;
        switch (this.TextImageRelation) {
          case TextImageRelation.ImageBeforeText:
            padding = new(0 + this._imageSize.Width, inheritedPadding.Top, inheritedPadding.Right,
              inheritedPadding.Bottom);
            break;
          case TextImageRelation.TextBeforeImage:
            padding = new(inheritedPadding.Left, inheritedPadding.Top, 0 + this._imageSize.Width,
              inheritedPadding.Bottom);
            break;
          case TextImageRelation.ImageAboveText:
            padding = new(inheritedPadding.Left, 0 + this._imageSize.Width, inheritedPadding.Right,
              inheritedPadding.Bottom);
            break;
          case TextImageRelation.TextAboveImage:
            padding = new(inheritedPadding.Left, inheritedPadding.Top, inheritedPadding.Right,
              0 + this._imageSize.Width);
            break;
          case TextImageRelation.Overlay:
          default:
            padding = inheritedPadding;
            break;
        }

        this.Style.Padding = padding;
      }
    }

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
      DataGridViewPaintParts paintParts) {
      // Paint the base content
      base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState,
        value, formattedValue, errorText, cellStyle,
        advancedBorderStyle, paintParts);

      var image = this.Image;
      if (image == null)
        return;

      // Draw the image clipped to the cell.
      var container = graphics.BeginContainer();

      graphics.SetClip(cellBounds);

      var imageSize = this._imageSize;
      var imageWidth = imageSize.Width;
      var imageHeight = imageSize.Height;

      int x, y;
      switch (this.TextImageRelation) {
        case TextImageRelation.TextBeforeImage:
          x = cellBounds.Width - imageWidth - 1;
          y = (cellBounds.Height - imageHeight) / 2;
          break;
        case TextImageRelation.ImageBeforeText:
          x = 0;
          y = (cellBounds.Height - imageHeight) / 2;
          break;
        case TextImageRelation.ImageAboveText:
          x = (cellBounds.Width - imageWidth) / 2;
          y = 0;
          break;
        case TextImageRelation.TextAboveImage:
          x = (cellBounds.Width - imageWidth) / 2;
          y = cellBounds.Height - imageHeight - 1;
          break;
        case TextImageRelation.Overlay:
        default:
          x = (cellBounds.Width - imageWidth) / 2;
          y = (cellBounds.Height - imageHeight) / 2;
          break;
      }

      x += cellBounds.Location.X;
      y += cellBounds.Location.Y;

      if (this._needsResize)
        graphics.DrawImage(image, new(x, y, imageWidth, imageHeight), 0, 0, image.Width, image.Height,
          GraphicsUnit.Pixel);
      else
        graphics.DrawImageUnscaled(image, x, y);

      graphics.EndContainer(container);
    }

    private DataGridViewImageAndTextColumn _OwningDataGridViewImageAndTextColumn =>
      this.OwningColumn as DataGridViewImageAndTextColumn;
  }
}

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewDateTimePickerColumn : DataGridViewColumn {
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

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewNumericUpDownColumn : DataGridViewColumn {
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
        throw new InvalidCastException(
          "Value provided for CellTemplate must be of type DataGridViewNumericUpDownElements.DataGridViewNumericUpDownCell or derive from it.");

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
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.DecimalPlaces;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      // Update the template cell so that subsequent cloned cells use the new value.
      this.NumericUpDownCellTemplate.DecimalPlaces = value;
      var dataGridView = this.DataGridView;
      if (dataGridView == null)
        return;

      // Update all the existing DataGridViewNumericUpDownCell cells in the column accordingly.
      var dataGridViewRows = dataGridView.Rows;
#if SUPPORTS_CONTRACTS
        Contract.Assume(dataGridViewRows != null);
#endif

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
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.Increment;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

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
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.Maximum;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

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
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.Minimum;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

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
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

      return this.NumericUpDownCellTemplate.ThousandsSeparator;
    }
    set {
      if (this.NumericUpDownCellTemplate == null)
        throw new InvalidOperationException(
          "Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");

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
            if ((this.RightToLeft == RightToLeft.No &&
                 !(textBox.SelectionLength == 0 && textBox.SelectionStart == textBox.Text.Length)) ||
                (this.RightToLeft == RightToLeft.Yes &&
                 !(textBox.SelectionLength == 0 && textBox.SelectionStart == 0)))
              return true;

          break;
        }

        case Keys.Left: {
          if (this.Controls[1] is TextBox textBox)
            // If the end of the selection is at the begining of the string
            // or if the entire text is selected and we did not start editing,
            // send this character to the dataGridView, else process the key message
            if ((this.RightToLeft == RightToLeft.No &&
                 !(textBox.SelectionLength == 0 && textBox.SelectionStart == 0)) ||
                (this.RightToLeft == RightToLeft.Yes &&
                 !(textBox.SelectionLength == 0 && textBox.SelectionStart == textBox.Text.Length)))
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
            if (textBox.SelectionLength > 0 ||
                textBox.SelectionStart < textBox.Text.Length)
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
    private static readonly DataGridViewContentAlignment anyRight = DataGridViewContentAlignment.TopRight |
                                                                    DataGridViewContentAlignment.MiddleRight |
                                                                    DataGridViewContentAlignment.BottomRight;

    private static readonly DataGridViewContentAlignment anyCenter = DataGridViewContentAlignment.TopCenter |
                                                                     DataGridViewContentAlignment.MiddleCenter |
                                                                     DataGridViewContentAlignment.BottomCenter;

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
      renderingBitmap ??= new(DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapWidth,
        DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapHeight);

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
        if (value < 0 || value > 99)
          throw new ArgumentOutOfRangeException(nameof(value),
            "The DecimalPlaces property cannot be smaller than 0 or larger than 99.");

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
#if SUPPORTS_CONTRACTS
          Contract.Assume(dataGridView != null);
#endif
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

      if (dataGridView.EditingControl is NumericUpDown numericUpDown) {
        // Editing controls get recycled. Indeed, when a DataGridViewNumericUpDownCell cell gets edited
        // after another DataGridViewNumericUpDownCell cell, the same editing control gets reused for 
        // performance reasons (to avoid an unnecessary control destruction and creation). 
        // Here the undo buffer of the TextBox inside the NumericUpDown control gets cleared to avoid
        // interferences between the editing sessions.
#if SUPPORTS_CONTRACTS
          Contract.Assume(numericUpDown.Controls.Count > 1);
#endif
        if (numericUpDown.Controls[1] is TextBox textBox)
          textBox.ClearUndo();
      }

      base.DetachEditingControl();
    }

    /// <summary>
    ///   Adjusts the location and size of the editing control given the alignment characteristics of the cell
    /// </summary>
    private Rectangle GetAdjustedEditingControlBounds(Rectangle editingControlBounds, DataGridViewCellStyle cellStyle) {
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
    protected override object GetFormattedValue(object value,
      int rowIndex,
      ref DataGridViewCellStyle cellStyle,
      TypeConverter valueTypeConverter,
      TypeConverter formattedValueTypeConverter,
      DataGridViewDataErrorContexts context) {
      // By default, the base implementation converts the Decimal 1234.5 into the string "1234.5"
      var formattedValue = base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter,
        formattedValueTypeConverter, context);
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
    protected override DrawingSize GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex,
      DrawingSize constraintSize) {
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
    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue,
      DataGridViewCellStyle dataGridViewCellStyle) {
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
    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
      DataGridViewElementStates cellState,
      object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle,
      DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
      if (this.DataGridView == null)
        return;

      // First paint the borders and background of the cell.
      base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle,
        advancedBorderStyle,
        paintParts & ~(DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.ContentForeground));

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
            cellStyle.Padding.Top);
          valBounds.Width -= cellStyle.Padding.Horizontal;
          valBounds.Height -= cellStyle.Padding.Vertical;
        }

        // Determine the NumericUpDown control location
        valBounds = this.GetAdjustedEditingControlBounds(valBounds, cellStyle);

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
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText,
          cellStyle, advancedBorderStyle, DataGridViewPaintParts.ErrorIcon);
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
    public override void PositionEditingControl(bool setLocation,
      bool setSize,
      Rectangle cellBounds,
      Rectangle cellClip,
      DataGridViewCellStyle cellStyle,
      bool singleVerticalBorderAdded,
      bool singleHorizontalBorderAdded,
      bool isFirstDisplayedColumn,
      bool isFirstDisplayedRow) {
      var editingControlBounds = this.PositionEditingPanel(cellBounds,
        cellClip,
        cellStyle,
        singleVerticalBorderAdded,
        singleHorizontalBorderAdded,
        isFirstDisplayedColumn,
        isFirstDisplayedRow);
      editingControlBounds = this.GetAdjustedEditingControlBounds(editingControlBounds, cellStyle);
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
#if SUPPORTS_CONTRACTS
        Contract.Requires(value is >= 0 and <= 99);
#endif
      this.decimalPlaces = value;
      if (this.OwnsEditingNumericUpDown(rowIndex))
        this.EditingNumericUpDown.DecimalPlaces = value;
    }

    /// Utility function that sets a new value for the Increment property of the cell. This function is used by
    /// the cell and column Increment property. A row index needs to be provided as a parameter because
    /// this cell may be shared among multiple rows.
    public void SetIncrement(int rowIndex, decimal value) {
#if SUPPORTS_CONTRACTS
        Contract.Requires(value >= (decimal)0.0);
#endif
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
      => "DataGridViewNumericUpDownCell { ColumnIndex=" + this.ColumnIndex.ToString(CultureInfo.CurrentCulture) +
         ", RowIndex=" + this.RowIndex.ToString(CultureInfo.CurrentCulture) + " }";

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

#endregion

#region attributes for messing with auto-generated columns

[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewClickableAttribute : Attribute {
  public DataGridViewClickableAttribute(string onClickMethodName = null, string onDoubleClickMethodName = null) {
    this.OnClickMethodName = onClickMethodName;
    this.OnDoubleClickMethodName = onDoubleClickMethodName;
  }

  public string OnClickMethodName { get; }
  public string OnDoubleClickMethodName { get; }

  private static readonly ConcurrentDictionary<object, ThreadTimer> _clickTimers = new();

  private void _HandleClick(object row) {
    _clickTimers.TryRemove(row, out var __);
    DataGridViewExtensions.CallLateBoundMethod(row, this.OnClickMethodName);
  }

  public void OnClick(object row) {
    if (this.OnDoubleClickMethodName == null)
      DataGridViewExtensions.CallLateBoundMethod(row, this.OnClickMethodName);

    var newTimer = new ThreadTimer(this._HandleClick, row, SystemInformation.DoubleClickTime, int.MaxValue);
    do
      if (_clickTimers.TryRemove(row, out var timer))
        timer.Dispose();
    while (!_clickTimers.TryAdd(row, newTimer));
  }

  public void OnDoubleClick(object row) {
    if (_clickTimers.TryRemove(row, out var timer))
      timer.Dispose();

    DataGridViewExtensions.CallLateBoundMethod(row, this.OnDoubleClickMethodName);
  }
}

/// <summary>
///   allows to show an image alongside to the displayed text.
/// </summary>

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewTextAndImageColumnAttribute : Attribute {
  public string ImageListPropertyName { get; }
  public string ImageKeyPropertyName { get; }
  public string ImagePropertyName { get; }
  public TextImageRelation TextImageRelation { get; }
  public uint FixedImageWidth { get; }
  public uint FixedImageHeight { get; }
  public bool KeepAspectRatio { get; }

  public DataGridViewTextAndImageColumnAttribute(string imageListPropertyName, string imageKeyPropertyName,
    TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText, uint fixedImageWidth = 0,
    uint fixedImageHeight = 0, bool keepAspectRatio = true) {
    this.ImageListPropertyName = imageListPropertyName;
    this.ImageKeyPropertyName = imageKeyPropertyName;
    this.TextImageRelation = textImageRelation;
    this.FixedImageWidth = fixedImageWidth;
    this.FixedImageHeight = fixedImageHeight;
    this.KeepAspectRatio = keepAspectRatio;
  }

  public DataGridViewTextAndImageColumnAttribute(string imagePropertyName,
    TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText, uint fixedImageWidth = 0,
    uint fixedImageHeight = 0, bool keepAspectRatio = true) {
    this.ImagePropertyName = imagePropertyName;
    this.TextImageRelation = textImageRelation;
    this.FixedImageWidth = fixedImageWidth;
    this.FixedImageHeight = fixedImageHeight;
    this.KeepAspectRatio = keepAspectRatio;
  }

  private Image _GetImage(object row, object value) {
    if (value is null)
      return null;

    Image image = null;
    var imagePropertyName = this.ImagePropertyName;
    if (imagePropertyName != null)
      image = DataGridViewExtensions.GetPropertyValueOrDefault<Image>(row, imagePropertyName, null, null, null, null);
    else {
      var imageList =
        DataGridViewExtensions.GetPropertyValueOrDefault<ImageList>(row, this.ImageListPropertyName, null, null, null,
          null);
      if (imageList == null)
        return null;

      var imageKey =
        DataGridViewExtensions.GetPropertyValueOrDefault<object>(row, this.ImageKeyPropertyName, null, null, null,
          null);
      if (imageKey != null)
        image = imageKey is int index && !index.GetType().IsEnum
            ? imageList.Images[index]
            : imageList.Images[imageKey.ToString()]
          ;
    }

    return image;
  }

  public static void OnCellFormatting(DataGridViewTextAndImageColumnAttribute @this, DataGridViewRow row,
    DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    if (row.Cells[e.ColumnIndex] is not DataGridViewImageAndTextColumn.DataGridViewTextAndImageCell textAndImageCell)
      return;

    textAndImageCell.TextImageRelation = @this.TextImageRelation;
    textAndImageCell.KeepAspectRatio = @this.KeepAspectRatio;
    textAndImageCell.FixedImageWidth = @this.FixedImageWidth;
    textAndImageCell.FixedImageHeight = @this.FixedImageHeight;
    textAndImageCell.Image = @this._GetImage(data, e.Value);
    e.FormattingApplied = true;
  }
}

/// <summary>
///   allows to show an image next to the displayed text when a condition is met.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class SupportsConditionalImageAttribute : Attribute {
  /// <summary>
  ///   Initializes a new instance of the <see cref="SupportsConditionalImageAttribute" /> class.
  /// </summary>
  /// <param name="imagePropertyName">The name of the property which returns the image to display</param>
  /// <param name="conditionalPropertyName">The name of the property which defines, if the image is shown</param>
  public SupportsConditionalImageAttribute(string imagePropertyName, string conditionalPropertyName = null) {
    this.ImagePropertyName = imagePropertyName;
    this.ConditionalPropertyName = conditionalPropertyName;
  }

  public string ImagePropertyName { get; }
  public string ConditionalPropertyName { get; }

  public Image GetImage(object row, object value) {
    if (value is null)
      return null;

    if (!DataGridViewExtensions.GetPropertyValueOrDefault(row, this.ConditionalPropertyName, false, true, true, false))
      return null;

    var image = DataGridViewExtensions.GetPropertyValueOrDefault<Image>(row, this.ImagePropertyName, null, null, null,
      null);
    return image;
  }

  public static void OnCellFormatting(IEnumerable<SupportsConditionalImageAttribute> @this, DataGridViewRow row,
    DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    if (row.Cells[e.ColumnIndex] is not DataGridViewImageAndTextColumn.DataGridViewTextAndImageCell cell)
      return;

    foreach (var attribute in @this) {
      var image = attribute.GetImage(data, e.Value);
      cell.Image = image;
      if (image == null)
        continue;

      cell.TextImageRelation = TextImageRelation.ImageBeforeText;
      cell.KeepAspectRatio = true;
      cell.FixedImageWidth = 0;
      cell.FixedImageHeight = 0;
      e.FormattingApplied = true;
      break;
    }
  }
}

[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewCheckboxColumnAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewImageColumnAttribute : DataGridViewClickableAttribute {
  public DataGridViewImageColumnAttribute(string imageListPropertyName = null, string onClickMethodName = null,
    string onDoubleClickMethodName = null, string toolTipTextPropertyName = null) : base(onClickMethodName,
    onDoubleClickMethodName) {
    this.ImageListPropertyName = imageListPropertyName;
    this.ToolTipTextPropertyName = toolTipTextPropertyName;
  }

  public string ToolTipTextPropertyName { get; }
  public string ImageListPropertyName { get; }

  private Image _GetImage(object row, object value) {
    var imageList =
      DataGridViewExtensions.GetPropertyValueOrDefault<ImageList>(row, this.ImageListPropertyName, null, null, null,
        null);
    if (imageList == null)
      return value as Image;

    var result = value is int index && !index.GetType().IsEnum
      ? imageList.Images[index]
      : imageList.Images[value.ToString()];
    return result;
  }

  private string _ToolTipText(object row) =>
    DataGridViewExtensions.GetPropertyValueOrDefault<string>(row, this.ToolTipTextPropertyName, null, null, null, null);

  public static void OnCellFormatting(DataGridViewImageColumnAttribute @this, DataGridViewRow row,
    DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    //should not be necessary but dgv throws format exception
    if (e.DesiredType != typeof(Image))
      return;

    e.Value = @this._GetImage(data, e.Value);
    e.FormattingApplied = true;
    row.Cells[column.Index].ToolTipText = @this._ToolTipText(data);
  }
}

[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewCellDisplayTextAttribute : Attribute {
  private string PropertyName { get; }

  public DataGridViewCellDisplayTextAttribute(string propertyName) => this.PropertyName = propertyName;

  private string _GetDisplayText(object row) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(row, this.PropertyName, string.Empty, string.Empty, string.Empty,
      string.Empty);

  public static void OnCellFormatting(DataGridViewCellDisplayTextAttribute @this, DataGridViewRow row,
    DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    e.Value = @this._GetDisplayText(data);
    e.FormattingApplied = true;
  }
}

/// <summary>
///   Allows specifying certain properties as read-only depending on the underlying object instance.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewConditionalReadOnlyAttribute : Attribute {
  public DataGridViewConditionalReadOnlyAttribute(string isReadOnlyWhen) => this.IsReadOnlyWhen = isReadOnlyWhen;

  public string IsReadOnlyWhen { get; }

  public bool IsReadOnly(object row) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(row, this.IsReadOnlyWhen, false, false, false, false);
}

/// <summary>
///   Allows specifying the row visibility depending on the underlying object instance.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewConditionalRowHiddenAttribute : Attribute {
  public DataGridViewConditionalRowHiddenAttribute(string isHiddenWhen) => this.IsHiddenWhen = isHiddenWhen;

  public string IsHiddenWhen { get; }

  public bool IsHidden(object row) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(row, this.IsHiddenWhen, false, false, false, false);

  public static void OnRowPrepaint(IEnumerable<DataGridViewConditionalRowHiddenAttribute> @this, DataGridViewRow row,
    object data, DataGridViewRowPrePaintEventArgs e) {
    foreach (var attribute in @this)
      if (attribute.IsHidden(data)) {
        row.Visible = false;
        return;
      }

    row.Visible = true;
  }
}

/// <summary>
///   Allows specifying a value to be used as a progressbar column.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewProgressBarColumnAttribute : Attribute {
  public DataGridViewProgressBarColumnAttribute() : this(0, 100) { }

  public DataGridViewProgressBarColumnAttribute(double minimum, double maximum) {
    this.Minimum = minimum;
    this.Maximum = maximum;
  }

  public double Minimum { get; }
  public double Maximum { get; }
}

/// <summary>
///   Allows specifying a string or image property to be used as a button column.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewButtonColumnAttribute : Attribute {
  /// <summary>
  ///   Initializes a new instance of the <see cref="DataGridViewButtonColumnAttribute" /> class.
  /// </summary>
  /// <param name="onClickMethodName">The target method name to call upon click.</param>
  /// <param name="isEnabledWhen">The boolean property which enables or disables the buttons.</param>
  public DataGridViewButtonColumnAttribute(string onClickMethodName, string isEnabledWhen = null) {
    this.OnClickMethodName = onClickMethodName;
    this.IsEnabledWhen = isEnabledWhen;
  }

  public string IsEnabledWhen { get; }

  public string OnClickMethodName { get; }

  /// <summary>
  ///   Executes the callback with the given object instance.
  /// </summary>
  /// <param name="row">The value.</param>
  public void OnClick(object row) {
    if (this.IsEnabled(row))
      DataGridViewExtensions.CallLateBoundMethod(row, this.OnClickMethodName);
  }

  public bool IsEnabled(object row) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(row, this.IsEnabledWhen, false, true, false, false);
}

/// <summary>
///   Allows specifying a column to host a combobox contaning the elements specified by a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewComboboxColumnAttribute : Attribute {
  public string EnabledWhenPropertyName { get; }
  public string ValueMember { get; }
  public string DisplayMember { get; }
  public string DataSourcePropertyName { get; }

  public DataGridViewComboboxColumnAttribute(string dataSourcePropertyName, string enabledWhenPropertyName = null,
    string valueMember = null, string displayMember = null) {
    this.EnabledWhenPropertyName = enabledWhenPropertyName;
    this.ValueMember = valueMember;
    this.DisplayMember = displayMember;
    this.DataSourcePropertyName = dataSourcePropertyName;
  }
}

/// <summary>
///   Allows specifying a value to be used as column with multiple images
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewMultiImageColumnAttribute : Attribute {
  /// <summary>
  ///   Initializes a new instance of the <see cref="DataGridViewMultiImageColumnAttribute" /> class.
  /// </summary>
  /// <param name="onClickMethodName">
  ///   Name of a method within the data bound class, which should be called,
  ///   whenever a click on an image occurs (this method has to take one parameter of type int (index of the clicked image))
  /// </param>
  /// <param name="toolTipProviderMethodName">
  ///   Name of a method within the data bound class, which should be used,
  ///   to get the tooltip text for a specific image (this method has to take one parameter of type int (index of the image))
  /// </param>
  /// <param name="maximumImageSize">the maximum size of every image displayed (width and height)</param>
  /// <param name="padding">The padding within each image</param>
  /// <param name="margin">The margin around each image</param>
  public DataGridViewMultiImageColumnAttribute(string onClickMethodName = null, string toolTipProviderMethodName = null,
    int maximumImageSize = 24, int padding = 0, int margin = 0)
    : this(onClickMethodName, toolTipProviderMethodName, maximumImageSize, padding, padding, padding, padding, margin,
      margin, margin, margin) { }

  public DataGridViewMultiImageColumnAttribute(string onClickMethodName, string toolTipProviderMethodName,
    int maximumImageSize, int paddingLeft, int paddingTop, int paddingRight, int paddingBottom, int marginLeft,
    int marginTop, int marginRight, int marginBottom) {
    this.MaximumImageSize = maximumImageSize;
    this.OnClickMethodName = onClickMethodName;
    this.ToolTipProviderMethodName = toolTipProviderMethodName;
    this.Padding = new(paddingLeft, paddingTop, paddingRight, paddingBottom);
    this.Margin = new(marginLeft, marginTop, marginRight, marginBottom);
  }

  public int MaximumImageSize { get; }
  public string OnClickMethodName { get; }
  public string ToolTipProviderMethodName { get; }
  public Padding Padding { get; }
  public Padding Margin { get; }
}

/// <summary>
///   Allows specifying a value to be used as column with numeric up down control
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewNumericUpDownColumnAttribute : Attribute {
  public decimal Minimum { get; }
  public decimal Maximum { get; }
  public int DecimalPlaces { get; }
  public decimal Increment { get; }

  public DataGridViewNumericUpDownColumnAttribute(double minimum, double maximum, double increment = 1,
    int decimalPlaces = 2) {
    this.Minimum = (decimal)minimum;
    this.Maximum = (decimal)maximum;
    this.Increment = (decimal)increment;
    this.DecimalPlaces = decimalPlaces;
  }
}

/// <summary>
///   Allows setting an exact width in pixels for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewColumnWidthAttribute : Attribute {
  public DataGridViewColumnWidthAttribute(char characters) {
    this.Characters = new('@', characters);
    this.Width = -1;
    this.Mode = DataGridViewAutoSizeColumnMode.None;
  }

  public DataGridViewColumnWidthAttribute(string characters) {
    this.Characters = characters;
    this.Width = -1;
    this.Mode = DataGridViewAutoSizeColumnMode.None;
  }

  public DataGridViewColumnWidthAttribute(int width) {
    this.Characters = null;
    this.Width = width;
    this.Mode = DataGridViewAutoSizeColumnMode.None;
  }

  public DataGridViewColumnWidthAttribute(DataGridViewAutoSizeColumnMode mode) {
    this.Characters = null;
    this.Mode = mode;
    this.Width = -1;
  }

  public DataGridViewAutoSizeColumnMode Mode { get; }
  public int Width { get; }
  public string Characters { get; }

  public void ApplyTo(DataGridViewColumn column) {
    if (this.Mode != DataGridViewAutoSizeColumnMode.None) {
      column.AutoSizeMode = this.Mode;
      return;
    }

    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

    if (this.Characters != null) {
      var font = column.DataGridView.Font;
      var width = TextRenderer.MeasureText(this.Characters, font);
      column.MinimumWidth = width.Width;
      column.Width = width.Width;
    } else if (this.Width >= 0) {
      column.MinimumWidth = this.Width;
      column.Width = this.Width;
    }
  }
}

/// <summary>
///   Allows setting the sort mode for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewColumnSortModeAttribute : Attribute {
  public DataGridViewColumnSortMode SortMode { get; }

  public DataGridViewColumnSortModeAttribute(DataGridViewColumnSortMode sortMode) => this.SortMode = sortMode;

  public void ApplyTo(DataGridViewColumn column) {
    column.SortMode = this.SortMode;
  }
}

/// <summary>
///   Allows setting an exact height in pixels for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewRowHeightAttribute : Attribute {
  public string RowHeightProperty { get; }
  public string CustomRowHeightEnabledProperty { get; }
  public string CustomRowHeightProperty { get; }
  public int HeightInPixel { get; }

  private readonly Action<DataGridViewRow, object> _applyRowHeightAction;

  public DataGridViewRowHeightAttribute(int heightInPixel) {
    this.HeightInPixel = heightInPixel;

    this._applyRowHeightAction = this._ApplyPixelRowHeightUnconditional;
  }

  public DataGridViewRowHeightAttribute(int heightInPixel, string customRowHeightEnabledProperty) {
    this.HeightInPixel = heightInPixel;
    this.CustomRowHeightEnabledProperty = customRowHeightEnabledProperty;

    this._applyRowHeightAction = this._ApplyPixelRowHeightConditional;
  }

  public DataGridViewRowHeightAttribute(string customRowHeightProperty) {
    this.CustomRowHeightProperty = customRowHeightProperty;

    this._applyRowHeightAction = this._ApplyPropertyConrolledRowHeightUnconditional;
  }

  public DataGridViewRowHeightAttribute(string customRowHeightProperty, string customRowHeightEnabledProperty) {
    this.CustomRowHeightProperty = customRowHeightProperty;
    this.CustomRowHeightEnabledProperty = customRowHeightEnabledProperty;

    this._applyRowHeightAction = this._ApplyPropertyConrolledRowHeightConditional;
  }

  private void _ApplyPixelRowHeightUnconditional(DataGridViewRow row, object rowData) {
    row.MinimumHeight = this.HeightInPixel;
    row.Height = this.HeightInPixel;
  }

  private void _ApplyPixelRowHeightConditional(DataGridViewRow row, object rowData) {
    if (!DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightEnabledProperty, false, false,
          false, false))
      return;

    row.MinimumHeight = this.HeightInPixel;
    row.Height = this.HeightInPixel;
  }

  private void _ApplyPropertyConrolledRowHeightUnconditional(DataGridViewRow row, object rowData) {
    var originalHeight = row.Height;
    var rowHeight = DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightProperty,
      originalHeight, originalHeight, originalHeight, originalHeight);

    row.MinimumHeight = rowHeight;
    row.Height = rowHeight;
  }

  private void _ApplyPropertyConrolledRowHeightConditional(DataGridViewRow row, object rowData) {
    if (!DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightEnabledProperty, false, false,
          false, false))
      return;

    var originalHeight = row.Height;
    var rowHeight = DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightProperty,
      originalHeight, originalHeight, originalHeight, originalHeight);

    row.MinimumHeight = rowHeight;
    row.Height = rowHeight;
  }

  public void ApplyTo(object rowData, DataGridViewRow row) => this._applyRowHeightAction?.Invoke(row, rowData);
}

/// <summary>
///   Allows an specific object to be represented as a full row header.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewFullMergedRowAttribute : Attribute {
  public DataGridViewFullMergedRowAttribute(string headingTextPropertyName, string foreColor = null,
    float textSize = -1) {
    this.HeadingTextPropertyName = headingTextPropertyName;
    this.ForeColor = foreColor?.ParseColor();
    this.TextSize = textSize < 0 ? null : textSize;
  }

  public Color? ForeColor { get; }
  public float? TextSize { get; }
  public string HeadingTextPropertyName { get; }

  public string GetHeadingText(object rowData) => DataGridViewExtensions.GetPropertyValueOrDefault(rowData,
    this.HeadingTextPropertyName, string.Empty, string.Empty, string.Empty, string.Empty);
}

/// <summary>
///   Allows adjusting the cell style in a DataGridView for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewCellStyleAttribute : Attribute {
  /// <summary>
  ///   Creates a new <see cref="DataGridViewCellStyleAttribute" />.
  /// </summary>
  /// <param name="foreColor">
  ///   The color-name used for the property <see cref="DataGridViewCellStyle.ForeColor" />.
  ///   <remarks>Supports many types of color-names, such as hex values, (a)rgb values, known-colors, system-colors, etc.</remarks>
  /// </param>
  /// <param name="backColor">
  ///   The color-name used for the property <see cref="DataGridViewCellStyle.BackColor" />.
  ///   <remarks>Supports many types of color-names, such as hex values, (a)rgb values, known-colors, system-colors, etc.</remarks>
  /// </param>
  /// <param name="format">The value for the property <see cref="DataGridViewCellStyle.Format" />.</param>
  /// <param name="alignment">The value for the property <see cref="DataGridViewCellStyle.Alignment" />.</param>
  /// <param name="wrapMode">The value for the property <see cref="DataGridViewCellStyle.WrapMode" />.</param>
  /// <param name="conditionalPropertyName">
  ///   The name of the <see cref="bool" />-property, deciding if this attribute should
  ///   be enabled.
  /// </param>
  /// <param name="foreColorPropertyName">
  ///   The name of the <see cref="Color" />-property, retrieving the value for
  ///   <see cref="DataGridViewCellStyle.ForeColor" />.
  /// </param>
  /// <param name="backColorPropertyName">
  ///   The name of the <see cref="Color" />-property, retrieving the value for
  ///   <see cref="DataGridViewCellStyle.BackColor" />.
  /// </param>
  /// <param name="wrapModePropertyName">
  ///   The name of the <see cref="DataGridViewTriState" />-property, retrieving the value
  ///   for <see cref="DataGridViewCellStyle.WrapMode" />.
  /// </param>
  public DataGridViewCellStyleAttribute(string foreColor = null, string backColor = null, string format = null,
    DataGridViewContentAlignment alignment = DataGridViewContentAlignment.NotSet,
    DataGridViewTriState wrapMode = DataGridViewTriState.NotSet, string conditionalPropertyName = null,
    string foreColorPropertyName = null, string backColorPropertyName = null,
    string wrapModePropertyName = null) {
    this.ForeColor = foreColor?.ParseColor();
    this.BackColor = backColor?.ParseColor();
    this.ConditionalPropertyName = conditionalPropertyName;
    this.Format = format;
    this.WrapMode = wrapMode;
    this.ForeColorPropertyName = foreColorPropertyName;
    this.BackColorPropertyName = backColorPropertyName;
    this.WrapModePropertyName = wrapModePropertyName;
    this.Alignment = alignment;
  }

  public string ConditionalPropertyName { get; }
  public Color? ForeColor { get; }
  public Color? BackColor { get; }
  public string Format { get; }
  public DataGridViewContentAlignment Alignment { get; }
  public DataGridViewTriState WrapMode { get; }
  public string ForeColorPropertyName { get; }
  public string BackColorPropertyName { get; }
  public string WrapModePropertyName { get; }

  private void _ApplyTo(DataGridViewCellStyle style, object data) {
    var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(data, this.ForeColorPropertyName, null, null,
      null, null) ?? this.ForeColor;
    if (color != null)
      style.ForeColor = color.Value;

    color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(data, this.BackColorPropertyName, null, null, null,
      null) ?? this.BackColor;
    if (color != null)
      style.BackColor = color.Value;

    var wrapMode = DataGridViewExtensions.GetPropertyValueOrDefault(data, this.WrapModePropertyName,
      DataGridViewTriState.NotSet, DataGridViewTriState.NotSet, DataGridViewTriState.NotSet,
      DataGridViewTriState.NotSet);
    style.WrapMode = this.WrapMode != DataGridViewTriState.NotSet ? this.WrapMode : wrapMode;

    if (this.Format != null)
      style.Format = this.Format;

    style.Alignment = this.Alignment;
  }

  private bool _IsEnabled(object data) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(data, this.ConditionalPropertyName, true, true, false, false);

  public static void OnCellFormatting(IEnumerable<DataGridViewCellStyleAttribute> @this, DataGridViewRow row,
    DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    foreach (var attribute in @this)
      if (attribute._IsEnabled(data))
        attribute._ApplyTo(e.CellStyle, data);
  }
}

/// <summary>
///   Allows defining the cell tooltip in a DataGridView for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewCellTooltipAttribute : Attribute {
  public string ToolTipText { get; }
  public string TooltipTextPropertyName { get; }
  public string ConditionalPropertyName { get; }
  public string Format { get; }

  public DataGridViewCellTooltipAttribute(string tooltipText = null, string tooltipTextPropertyName = null,
    string conditionalPropertyName = null, string format = null) {
    this.ToolTipText = tooltipText;
    this.TooltipTextPropertyName = tooltipTextPropertyName;
    this.ConditionalPropertyName = conditionalPropertyName;
    this.Format = format;
  }

  private void _ApplyTo(DataGridViewCell cell, object data) {
    var conditional =
      DataGridViewExtensions.GetPropertyValueOrDefault(data, this.ConditionalPropertyName, false, true, false, false);
    if (!conditional) {
      cell.ToolTipText = string.Empty;
      return;
    }

    var text = DataGridViewExtensions.GetPropertyValueOrDefault<object>(data, this.TooltipTextPropertyName, null, null,
      null, null) ?? this.ToolTipText;
    cell.ToolTipText =
      (this.Format != null && text is IFormattable f ? f.ToString(this.Format, null) : text?.ToString()) ??
      string.Empty;
  }

  public static void OnCellFormatting(DataGridViewCellTooltipAttribute @this, DataGridViewRow row,
    DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    if (row.Cells[e.ColumnIndex] is { } dgvCell)
      @this._ApplyTo(dgvCell, data);
  }
}

/// <summary>
///   Allows adjusting the cell style in a DataGridView for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DataGridViewRowStyleAttribute : Attribute {
  public DataGridViewRowStyleAttribute(string foreColor = null, string backColor = null, string format = null,
    string conditionalPropertyName = null, string foreColorPropertyName = null, string backColorPropertyName = null,
    bool isBold = false, bool isItalic = false, bool isStrikeout = false, bool isUnderline = false) {
    this.ForeColor = foreColor?.ParseColor();
    this.BackColor = backColor?.ParseColor();
    this.ConditionalPropertyName = conditionalPropertyName;
    this.Format = format;
    this.ForeColorPropertyName = foreColorPropertyName;
    this.BackColorPropertyName = backColorPropertyName;
    var fontStyle = DrawingFontStyle.Regular;
    if (isBold)
      fontStyle |= DrawingFontStyle.Bold;
    if (isItalic)
      fontStyle |= DrawingFontStyle.Italic;
    if (isStrikeout)
      fontStyle |= DrawingFontStyle.Strikeout;
    if (isUnderline)
      fontStyle |= DrawingFontStyle.Underline;
    this.FontStyle = fontStyle;
  }

  public string ConditionalPropertyName { get; }
  public Color? ForeColor { get; }
  public Color? BackColor { get; }
  public string Format { get; }
  public DrawingFontStyle FontStyle { get; }
  public string ForeColorPropertyName { get; }
  public string BackColorPropertyName { get; }

  private void _ApplyTo(DataGridViewRow row, object rowData) {
    var style = row.DefaultCellStyle;

    var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(rowData, this.ForeColorPropertyName, null,
      null, null, null) ?? this.ForeColor;
    if (color != null)
      style.ForeColor = color.Value;

    color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(rowData, this.BackColorPropertyName, null, null,
      null, null) ?? this.BackColor;
    if (color != null)
      style.BackColor = color.Value;

    if (this.Format != null)
      style.Format = this.Format;

    if (this.FontStyle != DrawingFontStyle.Regular)
      style.Font = new(style.Font ?? row.InheritedStyle.Font, this.FontStyle);
  }

  private bool _IsEnabled(object value) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(value, this.ConditionalPropertyName, true, true, false, false);

  public static void OnRowPrepaint(IEnumerable<DataGridViewRowStyleAttribute> @this, DataGridViewRow row, object data,
    DataGridViewRowPrePaintEventArgs e) {
    foreach (var attribute in @this)
      if (attribute._IsEnabled(data))
        attribute._ApplyTo(row, data);
  }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  class DataGridViewRowSelectableAttribute : Attribute {
  public DataGridViewRowSelectableAttribute(string conditionProperty = null) =>
    this.ConditionPropertyName = conditionProperty;

  public string ConditionPropertyName { get; }

  public bool IsSelectable(object value) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(value, this.ConditionPropertyName, true, true, false, false);

  public static void OnSelectionChanged(IEnumerable<DataGridViewRowSelectableAttribute> @this, DataGridViewRow row,
    object data, EventArgs e) {
    if (@this.Any(attribute => !attribute.IsSelectable(data)))
      row.Selected = false;
  }
}

#endregion

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  static partial class DataGridViewExtensions {
  #region messing with auto-generated columns

  public static void EnableExtendedAttributes(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    // unsubscribe first to avoid duplicate subscriptions
    @this.DataSourceChanged -= _DataSourceChanged;
    @this.RowPrePaint -= _RowPrePaint;
    @this.CellContentClick -= _CellClick;
    @this.CellContentDoubleClick -= _CellDoubleClick;
    @this.EnabledChanged -= _EnabledChanged;
    @this.Disposed -= _RemoveDisabledState;
    @this.CellFormatting -= _CellFormatting;
    @this.CellMouseUp -= _CellMouseUp;
    @this.RowPostPaint -= _RowPostPaint;
    @this.SelectionChanged -= _SelectionChanged;

    // subscribe to events
    @this.DataSourceChanged += _DataSourceChanged;
    @this.RowPrePaint += _RowPrePaint;
    @this.CellContentClick += _CellClick;
    @this.CellContentDoubleClick += _CellDoubleClick;
    @this.EnabledChanged += _EnabledChanged;
    @this.Disposed += _RemoveDisabledState;
    @this.CellFormatting += _CellFormatting;
    @this.CellMouseUp += _CellMouseUp;
    @this.RowPostPaint += _RowPostPaint;
    @this.SelectionChanged += _SelectionChanged;
  }

  /// <summary>
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private static void _SelectionChanged(object sender, EventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    var selectedRows = dgv.Rows.Cast<DataGridViewRow>().Where(r => r.Selected);
    foreach (var row in selectedRows) {
      if (!row._TryGetRowType(out var type))
        continue;

      var attributes = _QueryPropertyAttribute<DataGridViewRowSelectableAttribute>(type);
      DataGridViewRowSelectableAttribute.OnSelectionChanged(attributes, row, row.DataBoundItem, e);
    }
  }

  /// <summary>
  ///   Allows single clicking checkbox columns.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">
  ///   The <see cref="System.Windows.Forms.DataGridViewCellMouseEventArgs" /> instance containing the event
  ///   data.
  /// </param>
  private static void _CellMouseUp(object sender, DataGridViewCellMouseEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (!dgv.TryGetColumn(e.ColumnIndex, out var column))
      return;

    if (column is DataGridViewCheckBoxColumn && e.RowIndex >= 0)
      dgv.EndEdit();
  }

  /// <summary>
  ///   Executes image column double click events.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs" /> instance containing the event data.</param>
  private static void _CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (e.RowIndex < 0 || e.ColumnIndex < 0)
      return;

    var type = FindItemType(dgv);

    if (type == null)
      return;

    var column = dgv.Columns[e.ColumnIndex];
    if (column == null)
      return;

    var item = dgv.Rows[e.RowIndex].DataBoundItem;
    _QueryPropertyAttribute<DataGridViewClickableAttribute>(type, column.DataPropertyName)?.FirstOrDefault()
      ?.OnDoubleClick(item);
  }

  /// <summary>
  ///   Executes button/image column click events.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs" /> instance containing the event data.</param>
  private static void _CellClick(object sender, DataGridViewCellEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (!dgv._TryGetRow(e.RowIndex, out var row))
      return;

    if (!dgv.TryGetColumn(e.ColumnIndex, out var column))
      return;

    if (!row._TryGetRowType(out var type))
      return;

    var item = row.DataBoundItem;

    if (column is DataGridViewButtonColumn)
      _QueryPropertyAttribute<DataGridViewButtonColumnAttribute>(type, column.DataPropertyName)?.FirstOrDefault()
        ?.OnClick(item);

    _QueryPropertyAttribute<DataGridViewClickableAttribute>(type, column.DataPropertyName)?.FirstOrDefault()
      ?.OnClick(item);
  }

  /// <summary>
  ///   Fixes column widths according to property attributes.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="_">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private static void _DataSourceChanged(object sender, EventArgs _) {
    _ResetRowCache();
    if (sender is not DataGridView dgv)
      return;

    var type = FindItemType(dgv);

    if (type == null)
      return;

    var columns = dgv.Columns;

    for (var i = 0; i < columns.Count; ++i) {
      var column = columns[i];

      // ignore unbound columns
      if (!column.IsDataBound)
        continue;

      var propertyName = column.DataPropertyName;
      var property = type.GetProperty(propertyName);

      // ignore unknown properties
      if (property == null)
        continue;

      //if needed replace DataGridViewTextBoxColumns with DataGridViewDateTimePickerColumns
      if (!column.ReadOnly &&
          (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))) {
        var newColumn = _ConstructDateTimePickerColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewTextBoxColumns with DataGridViewButtonColumn
      var buttonColumnAttribute = (DataGridViewButtonColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewButtonColumnAttribute), true).FirstOrDefault();
      if (buttonColumnAttribute != null) {
        var newColumn = _ConstructDisableButtonColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewTextBoxColumns with DataGridViewProgressBarColumn
      var progressBarColumnAttribute = (DataGridViewProgressBarColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewProgressBarColumnAttribute), true).FirstOrDefault();
      if (progressBarColumnAttribute != null) {
        var newColumn = _ConstructProgressBarColumn(progressBarColumnAttribute, column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewTextBoxColumns for Enums with DataGridViewComboboxColumn
      var propType = property.PropertyType;

      if (propType.IsEnum) {
        if (column.ReadOnly) {
          // TODO: show display text for enums
        } else {
          var newColumn = _ConstructEnumComboboxColumn(propType, column);
          columns.RemoveAt(i);
          columns.Insert(i, newColumn);
          column = newColumn;
        }
      }

      // if needed replace DataGridViewColumns with DataGridViewImageColumn
      var imageColumnAttribute = (DataGridViewImageColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewImageColumnAttribute), true).FirstOrDefault();
      if (imageColumnAttribute != null) {
        var newColumn = _ConstructImageColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewMultiImageColumn
      var multiImageColumnAttribute = (DataGridViewMultiImageColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewMultiImageColumnAttribute), true).FirstOrDefault();
      if (multiImageColumnAttribute != null) {
        var newColumn = _ConstructMultiImageColumn(column, multiImageColumnAttribute);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewConditionalImageColumn
      var conditionalImageColumnAttribute = (SupportsConditionalImageAttribute)property
        .GetCustomAttributes(typeof(SupportsConditionalImageAttribute), true).FirstOrDefault();
      if (conditionalImageColumnAttribute != null) {
        var newColumn = _ConstructImageAndTextColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewConditionalImageColumn
      var imageAndTextColumnAttribute = (DataGridViewTextAndImageColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewTextAndImageColumnAttribute), true).FirstOrDefault();
      if (imageAndTextColumnAttribute != null) {
        var newColumn = _ConstructImageAndTextColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewCheckboxColumn
      var checkboxColumnAttribute = (DataGridViewCheckboxColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewCheckboxColumnAttribute), true).FirstOrDefault();
      if (checkboxColumnAttribute != null) {
        var newColumn = _ConstructCheckboxColumn(column, propType == typeof(bool?));
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewNumericUpDownColumn
      var numericUpDownColumnAttribute = (DataGridViewNumericUpDownColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewNumericUpDownColumnAttribute), true).FirstOrDefault();
      if (numericUpDownColumnAttribute != null) {
        var newColumn = _ConstructNumericUpDownColumn(column, numericUpDownColumnAttribute);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewDropDownColumn
      var comboBoxColumnAttribute = (DataGridViewComboboxColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewComboboxColumnAttribute), true).FirstOrDefault();
      if (comboBoxColumnAttribute != null) {
        var newColumn = _ConstructComboBoxColumn(column, comboBoxColumnAttribute);
        newColumn.DataPropertyName = property.Name;
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // apply visibility for column
      var columnVisibilityAttribute =
        _QueryPropertyAttribute<EditorBrowsableAttribute>(type, propertyName).FirstOrDefault();
      if (columnVisibilityAttribute != null) {
        var newColumn = _ConstructVisibleColumn(column, columnVisibilityAttribute.State);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // apply column width
      _QueryPropertyAttribute<DataGridViewColumnWidthAttribute>(type, propertyName)?.FirstOrDefault()?.ApplyTo(column);

      //apply column sort mode
      _QueryPropertyAttribute<DataGridViewColumnSortModeAttribute>(type, propertyName)?.FirstOrDefault()
        ?.ApplyTo(column);
    }

    //Query all properties which are assignable from IList and thus not auto generated
    var listProperties = type.GetProperties();

    // if needed add DataGridViewColumns with DataGridViewMultiImageColumnAttribute
    for (var index = 0; index < listProperties.Length; ++index) {
      var property = listProperties[index];

      var browsableAttribute =
        (BrowsableAttribute)property.GetCustomAttributes(typeof(BrowsableAttribute), true).FirstOrDefault();
      if (browsableAttribute is { Browsable: false })
        continue;

      if (dgv.Columns.Contains(property.Name))
        continue;

      DataGridViewColumn newColumn = null;

      var multiImageColumnAttribute = (DataGridViewMultiImageColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewMultiImageColumnAttribute), true).FirstOrDefault();
      if (multiImageColumnAttribute != null) {
        var displayText =
          (DisplayNameAttribute)property.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault();
        newColumn = _ConstructMultiImageColumn(property.Name, displayText?.DisplayName, multiImageColumnAttribute);
        columns.Insert(index, newColumn);
      }

      if (newColumn == null)
        continue;

      // apply column width
      _QueryPropertyAttribute<DataGridViewColumnWidthAttribute>(type, property.Name)?.FirstOrDefault()
        ?.ApplyTo(newColumn);
    }
  }

  private static BoundDataGridViewComboBoxColumn _ConstructComboBoxColumn(DataGridViewColumn column,
    DataGridViewComboboxColumnAttribute attribute) =>
    new(attribute.DataSourcePropertyName, attribute.EnabledWhenPropertyName, attribute.ValueMember,
      attribute.DisplayMember) {
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = column.ReadOnly,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = column.Visible,
    };

  /// <summary>
  ///   Constructs a button column where buttons can be disabled.
  /// </summary>
  /// <param name="column">The column.</param>
  /// <returns></returns>
  private static DataGridViewDisableButtonColumn _ConstructDisableButtonColumn(DataGridViewColumn column) =>
    new() {
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = true,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = column.Visible,
    };

  /// <summary>
  ///   Constructs a progressbar column.
  /// </summary>
  /// <param name="progressBarColumnAttribute">The progress bar column attribute.</param>
  /// <param name="column">The column.</param>
  /// <returns></returns>
  private static DataGridViewProgressBarColumn _ConstructProgressBarColumn(
    DataGridViewProgressBarColumnAttribute progressBarColumnAttribute, DataGridViewColumn column) =>
    new() {
      Minimum = progressBarColumnAttribute.Minimum,
      Maximum = progressBarColumnAttribute.Maximum,
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = true,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = column.Visible,
    };

  /// <summary>
  ///   Constructs an image column.
  /// </summary>
  /// <param name="column">The column.</param>
  /// <returns></returns>
  private static DataGridViewImageColumn _ConstructImageColumn(DataGridViewColumn column) =>
    new() {
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = true,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = column.Visible,
      DefaultCellStyle = { NullValue = null }
    };

  /// <summary>
  ///   Constructs a imageAndText column.
  /// </summary>
  /// <param name="column">the column, which will be the base for constructing this column</param>
  /// <returns>a new instance of <see cref="DataGridViewImageAndTextColumn" /></returns>
  private static DataGridViewImageAndTextColumn _ConstructImageAndTextColumn(DataGridViewColumn column) =>
    new() {
      HeaderText = column.HeaderText,
      AutoSizeMode = column.AutoSizeMode,
      DataPropertyName = column.DataPropertyName,
      DefaultCellStyle = column.DefaultCellStyle,
      Width = column.Width,
      Visible = column.Visible,
      ToolTipText = column.ToolTipText,
      Selected = column.Selected,
      ReadOnly = column.ReadOnly,
      Name = column.Name,
      HeaderCell = column.HeaderCell,
      DefaultHeaderCellType = column.DefaultHeaderCellType,
    };

  /// <summary>
  ///   Constructs a checkbox column.
  /// </summary>
  /// <param name="column">The column.</param>
  /// <param name="supportThreeState">if set to <c>true</c> supports three state.</param>
  /// <returns></returns>
  private static DataGridViewCheckBoxColumn
    _ConstructCheckboxColumn(DataGridViewColumn column, bool supportThreeState) =>
    new() {
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = true,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = column.Visible,
      DefaultCellStyle = { NullValue = null },
      ThreeState = supportThreeState,
      TrueValue = true,
      FalseValue = false,
      IndeterminateValue = null
    };

  /// <summary>
  ///   Constructs a checkbox column.
  /// </summary>
  /// <param name="column">The column.</param>
  /// <param name="state">The Visibility State</param>
  /// <returns></returns>
  private static DataGridViewCheckBoxColumn _ConstructVisibleColumn(DataGridViewColumn column,
    EditorBrowsableState state) =>
    new() {
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = true,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = state == EditorBrowsableState.Always,
      DefaultCellStyle = { NullValue = null },
      TrueValue = true,
      FalseValue = false,
      IndeterminateValue = null
    };

  /// <summary>
  ///   Constructs a multi image column.
  /// </summary>
  /// <param name="propertyName">The name of the data bound property.</param>
  /// <param name="headerText">The Text which should be displayed as header</param>
  /// <param name="attribute">the MultiImageColumn attribute from the data bound property</param>
  /// <returns>a new instance of <see cref="DataGridViewMultiImageColumn" /></returns>
  private static DataGridViewMultiImageColumn _ConstructMultiImageColumn(string propertyName, string headerText,
    DataGridViewMultiImageColumnAttribute attribute) =>
    new(attribute.MaximumImageSize, attribute.Padding, attribute.Margin, attribute.OnClickMethodName,
      attribute.ToolTipProviderMethodName) {
      Name = propertyName,
      DataPropertyName = propertyName,
      HeaderText = headerText ?? propertyName,
      ReadOnly = true,
      AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet,
      Visible = true,
    };

  /// <summary>
  ///   Constructs a multi image column.
  /// </summary>
  /// <param name="column">The column which was originally created by the dataGridView</param>
  /// <param name="attribute">the MultiImageColumn attribute from the data bound property</param>
  /// <returns>a new instance of <see cref="DataGridViewMultiImageColumn" /></returns>
  private static DataGridViewMultiImageColumn _ConstructMultiImageColumn(DataGridViewColumn column,
    DataGridViewMultiImageColumnAttribute attribute) =>
    new(attribute.MaximumImageSize, attribute.Padding, attribute.Margin, attribute.OnClickMethodName,
      attribute.ToolTipProviderMethodName) {
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = true,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = column.Visible
    };

  /// <summary>
  /// </summary>
  /// <param name="column"></param>
  /// <param name="attribute"></param>
  /// <returns></returns>
  private static DataGridViewColumn _ConstructNumericUpDownColumn(DataGridViewColumn column,
    DataGridViewNumericUpDownColumnAttribute attribute) =>
    new DataGridViewNumericUpDownColumn {
      Name = column.Name,
      DataPropertyName = column.DataPropertyName,
      HeaderText = column.HeaderText,
      ReadOnly = column.ReadOnly,
      DisplayIndex = column.DisplayIndex,
      Width = column.Width,
      AutoSizeMode = column.AutoSizeMode,
      ContextMenuStrip = column.ContextMenuStrip,
      Visible = column.Visible,
      Minimum = attribute.Minimum,
      Maximum = attribute.Maximum,
      DecimalPlaces = attribute.DecimalPlaces,
      Increment = attribute.Increment
    };

  /// <summary>
  ///   Construct a DataGridViewComboboxColumn for enum types.
  /// </summary>
  /// <param name="enumType">Type of the enum.</param>
  /// <param name="originalColumn">The original column.</param>
  /// <returns></returns>
  private static DataGridViewComboBoxColumn _ConstructEnumComboboxColumn(Type enumType,
    DataGridViewColumn originalColumn) {
    var fields = enumType.GetFields();
    var values = (
      from field in fields
      where !field.IsSpecialName
      let displayAttribute = field.GetCustomAttributes(true).OfType<DisplayNameAttribute>().FirstOrDefault()
      select Tuple.Create(displayAttribute?.DisplayName ?? field.Name, field.GetValue(null))
    ).ToArray();

    var newColumn = new DataGridViewComboBoxColumn {
      Name = originalColumn.Name,
      DataPropertyName = originalColumn.DataPropertyName,
      HeaderText = originalColumn.HeaderText,
      ReadOnly = originalColumn.ReadOnly,
      DisplayIndex = originalColumn.DisplayIndex,
      Width = originalColumn.Width,
      AutoSizeMode = originalColumn.AutoSizeMode,
      ContextMenuStrip = originalColumn.ContextMenuStrip,
      Visible = originalColumn.Visible,
      ValueType = enumType,
      DataSource = values,
      ValueMember = nameof(Tuple<string, object>.Item2),
      DisplayMember = nameof(Tuple<string, object>.Item1),
    };
    return newColumn;
  }

  private static DataGridViewDateTimePickerColumn _ConstructDateTimePickerColumn(DataGridViewColumn originalColumn) =>
    new() {
      Name = originalColumn.Name,
      DataPropertyName = originalColumn.DataPropertyName,
      HeaderText = originalColumn.HeaderText,
      ReadOnly = originalColumn.ReadOnly,
      DisplayIndex = originalColumn.DisplayIndex,
      Width = originalColumn.Width,
      AutoSizeMode = originalColumn.AutoSizeMode,
      ContextMenuStrip = originalColumn.ContextMenuStrip,
      Visible = originalColumn.Visible,
    };

  /// <summary>
  ///   Adjusts formatted values according to property attributes.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">
  ///   The <see cref="System.Windows.Forms.DataGridViewCellFormattingEventArgs" /> instance containing the
  ///   event data.
  /// </param>
  private static void _CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (!dgv._TryGetRow(e.RowIndex, out var row))
      return;

    if (!dgv.TryGetColumn(e.ColumnIndex, out var column))
      return;

    if (!row._TryGetRowType(out var type))
      return;

    var rowData = row.DataBoundItem;
    var columnPropertyName = column.DataPropertyName;

    void TryHandle<TAttribute>(
      Action<TAttribute, DataGridViewRow, DataGridViewColumn, object, string, DataGridViewCellFormattingEventArgs>
        handler) where TAttribute : Attribute {
      var attribute = _QueryPropertyAttribute<TAttribute>(type, columnPropertyName)?.FirstOrDefault();
      if (attribute != null)
        handler(attribute, row, column, rowData, columnPropertyName, e);
    }

    void TryHandle2<TAttribute>(
      Action<IEnumerable<TAttribute>, DataGridViewRow, DataGridViewColumn, object, string,
        DataGridViewCellFormattingEventArgs> handler) where TAttribute : Attribute {
      var attributes = _QueryPropertyAttribute<TAttribute>(type, columnPropertyName);
      if (attributes != null)
        handler(attributes, row, column, rowData, columnPropertyName, e);
    }

    TryHandle<DataGridViewImageColumnAttribute>(DataGridViewImageColumnAttribute.OnCellFormatting);
    TryHandle<DataGridViewTextAndImageColumnAttribute>(DataGridViewTextAndImageColumnAttribute.OnCellFormatting);
    TryHandle2<SupportsConditionalImageAttribute>(SupportsConditionalImageAttribute.OnCellFormatting);
    TryHandle<DataGridViewCellDisplayTextAttribute>(DataGridViewCellDisplayTextAttribute.OnCellFormatting);
    _FixDisplayTextForEnums(column, rowData, columnPropertyName, e);
    TryHandle<DataGridViewCellTooltipAttribute>(DataGridViewCellTooltipAttribute.OnCellFormatting);
    TryHandle2<DataGridViewCellStyleAttribute>(DataGridViewCellStyleAttribute.OnCellFormatting);
  }

  private static void _FixDisplayTextForEnums(DataGridViewColumn column, object data, string columnPropertyName,
    DataGridViewCellFormattingEventArgs e) {
    if (column is not DataGridViewTextBoxColumn)
      return;

    var property = data?.GetType().GetProperty(columnPropertyName);
    if (property == null)
      return;

    var columnDataType = property.PropertyType;
    var columnData = property.GetValue(data, null);
    if (!columnDataType.IsEnum)
      return;

    var displayText = _GetEnumDisplayName(columnData);
    if (displayText == null)
      return;

    e.Value = displayText;
    e.FormattingApplied = true;
  }

  /// <summary>
  ///   Fixes row styles.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">
  ///   The <see cref="System.Windows.Forms.DataGridViewRowPrePaintEventArgs" /> instance containing the event
  ///   data.
  /// </param>
  private static void _RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (!dgv._TryGetRow(e.RowIndex, out var row))
      return;

    if (!row._TryGetRowType(out var type))
      return;

    var value = row.DataBoundItem;

    bool TryHandle2<TAttribute>(
      Action<IEnumerable<TAttribute>, DataGridViewRow, object, DataGridViewRowPrePaintEventArgs> handler)
      where TAttribute : Attribute {
      var attribute = _QueryPropertyAttribute<TAttribute>(type);
      if (attribute == null)
        return false;

      handler(attribute, row, value, e);
      return true;
    }

    TryHandle2<DataGridViewConditionalRowHiddenAttribute>(DataGridViewConditionalRowHiddenAttribute.OnRowPrepaint);
    var isAlreadyStyled = TryHandle2<DataGridViewRowStyleAttribute>(DataGridViewRowStyleAttribute.OnRowPrepaint);
    _FixCellStyleForReadOnlyAndDisabled(dgv, row, type, value, isAlreadyStyled);
    _QueryPropertyAttribute<DataGridViewRowHeightAttribute>(type).FirstOrDefault()?.ApplyTo(value, row);
  }

  private static void _FixCellStyleForReadOnlyAndDisabled(DataGridView dgv, DataGridViewRow row, Type type,
    object value,
    bool isAlreadyStyled) {
    var cells = row.Cells;
    foreach (DataGridViewColumn column in dgv.Columns) {
      if (column.DataPropertyName == null)
        continue;

      var cell = cells[column.Index];
      if (!dgv.ReadOnly)
        _FixReadOnlyCellStyle(type, column, cell, value, isAlreadyStyled);

      _FixDisabledButtonCellStyle(type, column, cell, value);
    }
  }

  private static void _RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e) {
    _ResetRowCache();
    if (sender is not DataGridView dgv)
      return;

    if (!dgv._TryGetRow(e.RowIndex, out var row))
      return;

    if (!row._TryGetRowType(out var type))
      return;

    var value = row.DataBoundItem;

    var rowHeaderAttribute = _QueryPropertyAttribute<DataGridViewFullMergedRowAttribute>(type).FirstOrDefault();
    if (rowHeaderAttribute == null)
      return;

    using var brush = rowHeaderAttribute.ForeColor != null
      ? new(rowHeaderAttribute.ForeColor.Value)
      : e.State._FOS_HasFlag(DataGridViewElementStates.Selected)
        ? new(e.InheritedRowStyle.SelectionForeColor)
        : new SolidBrush(e.InheritedRowStyle.ForeColor);

    using var boldFont = new Font(e.InheritedRowStyle.Font.FontFamily,
      rowHeaderAttribute.TextSize ?? e.InheritedRowStyle.Font.Size, DrawingFontStyle.Bold);

    var drawFormat = new StringFormat {
      LineAlignment = StringAlignment.Center,
      Alignment = StringAlignment.Center
    };

    var borderWidthLeft = dgv.AdvancedCellBorderStyle.Left is DataGridViewAdvancedCellBorderStyle.InsetDouble
      or DataGridViewAdvancedCellBorderStyle.OutsetDouble
      ? 2
      : 1;
    var borderWidthRight = dgv.AdvancedCellBorderStyle.Right is DataGridViewAdvancedCellBorderStyle.InsetDouble
      or DataGridViewAdvancedCellBorderStyle.OutsetDouble
      ? 2
      : 1;
    var borderWidthBottom = dgv.AdvancedCellBorderStyle.Bottom is DataGridViewAdvancedCellBorderStyle.InsetDouble
      or DataGridViewAdvancedCellBorderStyle.OutsetDouble
      ? 2
      : 1;

    var rowBoundsWithoutBorder = new Rectangle(
      e.RowBounds.X + borderWidthLeft,
      e.RowBounds.Y,
      e.RowBounds.Width - borderWidthLeft - borderWidthRight,
      e.RowBounds.Height - borderWidthBottom);

    using (var backBrush = new SolidBrush(e.State._FOS_HasFlag(DataGridViewElementStates.Selected)
             ? e.InheritedRowStyle.SelectionBackColor
             : e.InheritedRowStyle.BackColor))
      e.Graphics.FillRectangle(backBrush, rowBoundsWithoutBorder);

    e.Graphics.DrawString(rowHeaderAttribute.GetHeadingText(value), boldFont, brush, e.RowBounds, drawFormat);
  }

  /// <summary>
  ///   Fixes the cell style for DataGridViewDisableButtonColumns depending on actual value.
  /// </summary>
  /// <param name="type">The type of the bound item.</param>
  /// <param name="column">The column.</param>
  /// <param name="cell">The cell.</param>
  /// <param name="value">The value.</param>
  private static void _FixDisabledButtonCellStyle(Type type, DataGridViewColumn column, DataGridViewCell cell,
    object value) {
    var dgvButtonColumnAttribute =
      _QueryPropertyAttribute<DataGridViewButtonColumnAttribute>(type, column.DataPropertyName)?.FirstOrDefault();
    if (column is DataGridViewDisableButtonColumn)
      ((DataGridViewDisableButtonColumn.DataGridViewDisableButtonCell)cell).Enabled =
        dgvButtonColumnAttribute?.IsEnabled(value) ?? !ReferenceEquals(null, value);
  }

  /// <summary>
  ///   Fixes the cell style for read-only cells in (normally) non-read-only columns.
  /// </summary>
  /// <param name="type">The type of the bound item.</param>
  /// <param name="column">The column.</param>
  /// <param name="cell">The cell.</param>
  /// <param name="value">The value.</param>
  /// <param name="alreadyStyled"><c>true</c> if the cell was already styled; otherwise, <c>false</c></param>
  private static void _FixReadOnlyCellStyle(Type type, DataGridViewColumn column, DataGridViewCell cell, object value,
    bool alreadyStyled) {
    var readOnlyAttribute = _QueryPropertyAttribute<ReadOnlyAttribute>(type, column.DataPropertyName)?.FirstOrDefault();
    if (readOnlyAttribute != null)
      cell.ReadOnly = readOnlyAttribute.IsReadOnly;

    var dgvReadOnlyAttribute =
      _QueryPropertyAttribute<DataGridViewConditionalReadOnlyAttribute>(type, column.DataPropertyName)
        ?.FirstOrDefault();
    if (dgvReadOnlyAttribute != null)
      cell.ReadOnly = dgvReadOnlyAttribute.IsReadOnly(value);

    if (!cell.ReadOnly)
      return;

    // do not fix style if whole dgv is read-only
    if (column.DataGridView.ReadOnly)
      return;

    if (alreadyStyled)
      return;

    cell.Style.BackColor = DrawingSystemColors.Control;
    cell.Style.ForeColor = DrawingSystemColors.GrayText;
  }


  private static DataGridViewRow _tryGetRowCache;
  private static void _ResetRowCache() => _tryGetRowCache = null;

  private static bool _TryGetRow(this DataGridView @this, int rowIndex, out DataGridViewRow row) {
    var cache = _tryGetRowCache;
    if (cache != null && cache.DataGridView == @this && cache.Index == rowIndex) {
      row = cache;
      return true;
    }

    if (rowIndex < 0 || rowIndex >= @this.RowCount) {
      _tryGetRowCache = row = null;
      return false;
    }

    row = @this.Rows[rowIndex];
    _tryGetRowCache = row;
    return true;
  }

  private static bool _TryGetRowType(this DataGridViewRow @this, out Type rowDataType) {
    rowDataType = @this.DataBoundItem?.GetType() ?? FindItemType(@this.DataGridView);
    return rowDataType != null;
  }

  #endregion

  #region fixing stuff

  /// <summary>
  ///   Saves the state of a DataGridView during Enable/Disable state transitions.
  /// </summary>
  private class DataGridViewState {
    private readonly bool _readonly;
    private readonly Color _defaultCellStyleBackColor;
    private readonly Color _defaultCellStyleForeColor;
    private readonly Color _columnHeadersDefaultCellStyleBackColor;
    private readonly Color _columnHeadersDefaultCellStyleForeColor;
    private readonly bool _enableHeadersVisualStyles;
    private readonly Color _backgroundColor;

    private DataGridViewState(bool @readonly, Color defaultCellStyleBackColor, Color defaultCellStyleForeColor,
      Color columnHeadersDefaultCellStyleBackColor, Color columnHeadersDefaultCellStyleForeColor,
      bool enableHeadersVisualStyles, Color backgroundColor) {
      this._readonly = @readonly;
      this._defaultCellStyleBackColor = defaultCellStyleBackColor;
      this._defaultCellStyleForeColor = defaultCellStyleForeColor;
      this._columnHeadersDefaultCellStyleBackColor = columnHeadersDefaultCellStyleBackColor;
      this._columnHeadersDefaultCellStyleForeColor = columnHeadersDefaultCellStyleForeColor;
      this._enableHeadersVisualStyles = enableHeadersVisualStyles;
      this._backgroundColor = backgroundColor;
    }

    /// <summary>
    ///   Restores the saved state to the given DataGridView.
    /// </summary>
    /// <param name="dataGridView">The DataGridView to restore state to.</param>
    public void RestoreTo(DataGridView dataGridView) {
      dataGridView.SuspendLayout();
      {
        dataGridView.ReadOnly = this._readonly;
        dataGridView.DefaultCellStyle.BackColor = this._defaultCellStyleBackColor;
        dataGridView.DefaultCellStyle.ForeColor = this._defaultCellStyleForeColor;
        dataGridView.ColumnHeadersDefaultCellStyle.BackColor = this._columnHeadersDefaultCellStyleBackColor;
        dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = this._columnHeadersDefaultCellStyleForeColor;
        dataGridView.EnableHeadersVisualStyles = this._enableHeadersVisualStyles;
        dataGridView.BackgroundColor = this._backgroundColor;
      }
      dataGridView.ResumeLayout(true);
    }

    /// <summary>
    ///   Saves the state of the given DataGridView.
    /// </summary>
    /// <param name="dataGridView">The DataGridView to save state from.</param>
    /// <returns></returns>
    public static DataGridViewState FromDataGridView(DataGridView dataGridView) =>
      new(
        dataGridView.ReadOnly,
        dataGridView.DefaultCellStyle.BackColor,
        dataGridView.DefaultCellStyle.ForeColor,
        dataGridView.ColumnHeadersDefaultCellStyle.BackColor,
        dataGridView.ColumnHeadersDefaultCellStyle.ForeColor,
        dataGridView.EnableHeadersVisualStyles,
        dataGridView.BackgroundColor
      );

    public static void ChangeToDisabled(DataGridView dataGridView) {
      dataGridView.SuspendLayout();
      {
        dataGridView.ReadOnly = true;
        dataGridView.EnableHeadersVisualStyles = false;
        dataGridView.DefaultCellStyle.ForeColor = SystemColors.GrayText;
        dataGridView.DefaultCellStyle.BackColor = SystemColors.Control;
        dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.GrayText;
        dataGridView.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
        dataGridView.BackgroundColor = SystemColors.Control;
      }
      dataGridView.ResumeLayout(true);
    }
  }

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
    private static readonly ConditionalWeakTable<DataGridView, DataGridViewState> _DGV_STATUS_BACKUPS = new();
#else
  private static readonly Dictionary<DataGridView, DataGridViewState> _DGV_STATUS_BACKUPS = new();
#endif

  /// <summary>
  ///   Handles the Disposed event of the control; removes any state from the state list for the given DataGridView.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="_">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private static void _RemoveDisabledState(object sender, EventArgs _) {
    if (sender is not DataGridView dgv)
      return;

    _DGV_STATUS_BACKUPS.Remove(dgv);
  }

  /// <summary>
  ///   Handles the EnabledChanged event of the control; saves the state in the state list and changes colors and borders to
  ///   appear grayed-out.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="_">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private static void _EnabledChanged(object sender, EventArgs _) {
    if (sender is not DataGridView dgv)
      return;

    if (dgv.Enabled) {
      // if state was saved, restore it
      if (!_DGV_STATUS_BACKUPS.TryGetValue(dgv, out var lastState))
        return;

      _DGV_STATUS_BACKUPS.Remove(dgv);
      lastState.RestoreTo(dgv);
    } else {
      // if state already saved, ignore
      if (_DGV_STATUS_BACKUPS.TryGetValue(dgv, out var _))
        return;

      var state = DataGridViewState.FromDataGridView(dgv);
      _DGV_STATUS_BACKUPS.Add(dgv, state);
      DataGridViewState.ChangeToDisabled(dgv);
    }
  }

  #endregion

  public static void EnableDoubleBuffering(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    if (SystemInformation.TerminalServerSession)
      return;

    var dgvType = @this.GetType();
    var pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
    if (pi != null)
      pi.SetValue(@this, true, null);
  }

  /// <summary>
  ///   Finds the type of the items in a bound DataGridView.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <returns>The identified item type or <c>null</c>.</returns>
  public static Type FindItemType(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    var source = @this.DataSource;
    if (source == null)
      return null;

    var type = source.GetType();
    if (type.HasElementType)
      return type.GetElementType(); /* only handle arrays ... */

    if (type.IsGenericType)
      return type.GetGenericArguments()[0]; /* and IEnumerable<T>, etc. */

    return null;
  }

  /// <summary>
  ///   Scrolls to the end.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  public static void ScrollToEnd(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    var rowCount = @this.RowCount;
    if (rowCount <= 0)
      return;

    try {
      @this.FirstDisplayedScrollingRowIndex = rowCount - 1;
    } catch (Exception) {
      // ReSharper disable once EmptyStatement
      ; // I can be a breakpoint if you want to know the exception thrown
    }
  }

  /// <summary>
  ///   Clones the columns to another datagridview.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <param name="target">The target DataGridView.</param>
  public static void CloneColumns(this DataGridView @this, DataGridView target) {
    if (@this == null)
      throw new NullReferenceException();

    if (target == null)
      throw new ArgumentNullException(nameof(target));

    if (ReferenceEquals(@this, target))
      throw new ArgumentException("Source and target are equal.", nameof(target));

    target.Columns.AddRange(@this.Columns.Cast<DataGridViewColumn>().Select(_CloneColumn).ToArray());
  }

  /// <summary>
  ///   Clones the given column.
  /// </summary>
  /// <param name="column">The column.</param>
  /// <returns></returns>
  private static DataGridViewColumn _CloneColumn(DataGridViewColumn column) => (DataGridViewColumn)column.Clone();

  /// <summary>
  ///   Finds the columns that match a certain condition.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns>An enumeration of columns.</returns>
  public static IEnumerable<DataGridViewColumn> FindColumns(this DataGridView @this,
    Func<DataGridViewColumn, bool> predicate) {
    if (@this == null)
      throw new NullReferenceException();

    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    return @this.Columns.Cast<DataGridViewColumn>().Where(predicate);
  }

  /// <summary>
  ///   Finds the first column that matches a certain condition.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns>The first matching column or <c>null</c>.</returns>
  public static DataGridViewColumn FindFirstColumn(this DataGridView @this, Func<DataGridViewColumn, bool> predicate) {
    if (@this == null)
      throw new NullReferenceException();

    var matches = FindColumns(@this, predicate);
    return matches?.FirstOrDefault();
  }

  public static bool TryGetColumn(this DataGridView @this, int columnIndex, out DataGridViewColumn column) {
    if (columnIndex < 0 || columnIndex >= @this.ColumnCount) {
      column = null;
      return false;
    }

    column = @this.Columns[columnIndex];
    return column.IsDataBound;
  }

  public static bool TryGetColumn(this DataGridView @this, string columnName, out DataGridViewColumn column) {
    if (!@this.Columns.Contains(columnName)) {
      column = null;
      return false;
    }

    column = @this.Columns[columnName];
    return column?.IsDataBound ?? false;
  }

  public static DataGridViewColumn GetColumnByName(this DataGridView @this, string columnName) =>
    !@this.Columns.Contains(columnName) ? null : @this.Columns[columnName];

  /// <summary>
  ///   Gets the selected items.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <returns>The currently selected items</returns>
  public static IEnumerable<object> GetSelectedItems(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    return
      @this
        .SelectedCells
        .Cast<DataGridViewCell>()
        .Select(c => c.OwningRow)
        .Concat(@this.SelectedRows.Cast<DataGridViewRow>())
        .Distinct()
        .OrderBy(i => i.Index)
        .Select(i => i.DataBoundItem)
      ;
  }

  /// <summary>
  ///   Gets the selected items.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This DataGridView.</param>
  /// <returns>The currently selected items</returns>
  public static IEnumerable<TItem> GetSelectedItems<TItem>(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    return @this.GetSelectedItems().Cast<TItem>();
  }

  /// <summary>
  ///   Gets the first selected item in display order.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This DataGridView.</param>
  /// <param name="item">out variable to store the first selected item in display order</param>
  /// <returns>true, if an item is currently selected, false otherwise</returns>
  public static bool TryGetFirstSelectedItem<TItem>(this DataGridView @this, out TItem item) {
    if (@this == null)
      throw new NullReferenceException();

    using var enumerator = @this.GetSelectedItems<TItem>().GetEnumerator();

    var result = enumerator.MoveNext();
    item = enumerator.Current;

    return result;
  }

  /// <summary>
  ///   Gets the index of the first selected item in display order
  /// </summary>
  /// <param name="this">This DataGridView</param>
  /// <param name="index">the index of the first selected item in display order</param>
  /// <returns>true if at least one item  was selected, false otherwise</returns>
  private static bool _TryGetFirstSelectedItemIndex(this DataGridView @this, out int index) {
    if (@this == null)
      throw new NullReferenceException();

    var result = @this
      .SelectedCells
      .Cast<DataGridViewCell>()
      .Select(c => c.OwningRow)
      .Concat(@this.SelectedRows.Cast<DataGridViewRow>())
      .Distinct()
      .OrderBy(i => i.Index)
      .Select(i => i.Index)
      .ToArray();

    index = result.FirstOrDefault();

    return result.Any();
  }

  /// <summary>
  ///   Determines whether if any cell is currently selected.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <returns><c>true</c> if any cell is currently selected; otherwise <c>false</c>.</returns>
  public static bool IsAnyCellSelected(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    return @this.SelectedCells.Count > 0;
  }

  /// <summary>
  ///   Selects the rows containing the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This DataGridView.</param>
  /// <param name="items">The items to select.</param>
  public static void SelectItems<TItem>(this DataGridView @this, IEnumerable<TItem> items) {
    if (@this == null)
      throw new NullReferenceException();

    if (items == null)
      throw new ArgumentNullException(nameof(items));

    var bucket = new HashSet<TItem>(items);
    foreach (var row in @this.Rows.Cast<DataGridViewRow>())
      if (row.DataBoundItem is TItem item && bucket.Contains(item))
        row.Selected = true;
  }

  /// <summary>
  ///   Resets the selection.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  public static void ResetSelection(this DataGridView @this) {
    if (@this == null)
      throw new NullReferenceException();

    foreach (DataGridViewRow row in @this.SelectedRows)
      row.Selected = false;
  }

  /// <summary>
  ///   Changes the datasource without throwing format errors.
  /// </summary>
  /// <param name="this">This DataGridView</param>
  /// <param name="dataSource">The new data source.</param>
  /// <returns>The old data source.</returns>
  public static object ExchangeDataSource(this DataGridView @this, object dataSource) {
    var result = @this.DataSource;
    void OnThisOnDataError(object _, DataGridViewDataErrorEventArgs e) => e.ThrowException = false;
    try {
      @this.DataError += OnThisOnDataError;
      @this.DataSource = dataSource;
    } finally {
      @this.DataError -= OnThisOnDataError;
    }

    return result;
  }

  /// <summary>
  ///   Refreshes the data source and restores selections and scroll position.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <typeparam name="TKey">The type of the keys.</typeparam>
  /// <param name="this">This DataGridView.</param>
  /// <param name="source">The source.</param>
  /// <param name="keyGetter">The key getter.</param>
  /// <param name="preAction">The pre action.</param>
  /// <param name="postAction">The post action.</param>
  public static void RefreshDataSource<TItem, TKey>(this DataGridView @this, IList<TItem> source,
    Func<TItem, TKey> keyGetter, Action preAction = null, Action postAction = null) {
    if (@this == null)
      throw new NullReferenceException();

    if (keyGetter == null)
      throw new ArgumentNullException(nameof(keyGetter));

    try {
      @this.SuspendLayout();

      // save scroll position
      var hScroll = @this.HorizontalScrollingOffset;
      var vScroll = @this.FirstDisplayedScrollingRowIndex;

      // save selected items
      var selected = new HashSet<TKey>(GetSelectedItems<TItem>(@this).Select(keyGetter));

      var scrollOffset = 0;
      if (@this._TryGetFirstSelectedItemIndex(out var firstSelectedIndex))
        scrollOffset = firstSelectedIndex - vScroll;

      // reset data source
      preAction?.Invoke();
      ExchangeDataSource(@this, source);
      postAction?.Invoke();

      if (source == null)
        return;

      // reselect
      if (@this.MultiSelect) {
        if (selected.Count < 1)
          return;
        foreach (var row in @this.Rows.Cast<DataGridViewRow>())
          row.Selected = selected.Contains(keyGetter((TItem)row.DataBoundItem));
      } else
        foreach (var row in @this.Rows.Cast<DataGridViewRow>()) {
          if (row.DataBoundItem == null || !selected.Contains(keyGetter((TItem)row.DataBoundItem)))
            continue;

          row.Selected = true;
        }

      if (@this._TryGetFirstSelectedItemIndex(out firstSelectedIndex))
        vScroll = firstSelectedIndex - scrollOffset;

      //re-apply scrolling
      if (vScroll >= 0 && vScroll < source.Count)
        @this.FirstDisplayedScrollingRowIndex = vScroll;

      @this.HorizontalScrollingOffset = hScroll;
    } finally {
      @this.ResumeLayout(true);
    }
  }

  /// <summary>
  ///   Changes the visibility of one or more columns.
  /// </summary>
  /// <param name="this">The DataGridView</param>
  /// <param name="visibilityState">the new visibility state</param>
  /// <param name="propertyNames">collection of property names, which visibility should be changed</param>
  public static void ChangeVisibleStateOfColumn(this DataGridView @this, bool visibilityState,
    params string[] propertyNames) {
    foreach (var propertyName in propertyNames) {
      DataGridViewColumn column;
      if ((column = @this.Columns[propertyName]) != null)
        column.Visible = visibilityState;
    }
  }

  /// <summary>
  ///   Automatically adjusts the height of the control.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <param name="maxRowCount">The maximum row count, if any.</param>
  public static void AutoAdjustHeight(this DataGridView @this, int maxRowCount = -1) {
    if (@this == null)
      throw new NullReferenceException();

    var headerHeight = @this.ColumnHeadersVisible ? @this.ColumnHeadersHeight : 0;
    var rows = @this.Rows.Cast<DataGridViewRow>();

    if (maxRowCount > 0)
      rows = rows.Take(maxRowCount).ToArray();

    var rowHeight = rows.Sum(row => row.Height + 1 /* 1px border between rows */);
    @this.Height = headerHeight + rowHeight;
  }


  /// <summary>
  ///   Enables multi cell editing on the given DataGridView
  /// </summary>
  /// <param name="this">The DataGridView</param>
  public static void EnableMultiCellEditing(this DataGridView @this) {
    @this.SelectionChanged += SelectionChanged;
    @this.CellBeginEdit += CellBeginEdit;
    @this.CellEndEdit += CellEndEdit;
    @this.CellValidating += This_OnCellValidating;
  }

  /// <summary>
  ///   Enables Right Click Selection on DGV
  /// </summary>
  /// <param name="this">the dgv</param>
  public static void EnableRightClickSelection(this DataGridView @this) =>
    @this.CellMouseDown += CellMouseDownRightClickEvent;

  /// <summary>
  ///   Disables Right Click Selection on DGV
  /// </summary>
  /// <param name="this">the dgv</param>
  public static void DisableRightClickSelection(this DataGridView @this) =>
    @this.CellMouseDown -= CellMouseDownRightClickEvent;

  /// <summary>
  ///   The cell mouse event that enables the right click selection
  /// </summary>
  /// <param name="sender">the object (this dgv)</param>
  /// <param name="e">the EventArgs given</param>
  private static void CellMouseDownRightClickEvent(object sender, DataGridViewCellMouseEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (e.ColumnIndex == -1 || e.RowIndex == -1 || e.Button != MouseButtons.Right)
      return;

    var c = dgv[e.ColumnIndex, e.RowIndex];
    if (c.Selected)
      return;

    c.DataGridView.ClearSelection();
    c.DataGridView.CurrentCell = c;
    c.Selected = true;
  }

  private sealed class _CellEditState {
    private readonly Color _foreColor;
    private readonly Color _backColor;

    private _CellEditState(Color foreColor, Color backColor) {
      this._foreColor = foreColor;
      this._backColor = backColor;
    }

    public static _CellEditState FromCell(DataGridViewCell cell) {
      var style = cell.Style;
      return new(style.ForeColor, style.BackColor);
    }

    public void ToCell(DataGridViewCell cell) {
      var style = cell.Style;
      style.ForeColor = this._foreColor;
      style.BackColor = this._backColor;
    }
  }

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
    private static readonly ConditionalWeakTable<DataGridViewCell, _CellEditState> _cellEditStates = new();
#else
  private static readonly Dictionary<DataGridViewCell, _CellEditState> _cellEditStates = new();
#endif

  /// <summary>
  ///   Highlights Cells on Beginning of Edit
  /// </summary>
  /// <param name="sender">the object (this dgv)</param>
  /// <param name="e">the EventArgs given</param>
  private static void CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    var cell = dgv[e.ColumnIndex, e.RowIndex];
    var cellStyle = cell.Style;
    if (dgv.SelectedCells.Count <= 1)
      return;

    _cellEditStates.Add(cell, _CellEditState.FromCell(cell));
    cellStyle.ForeColor = DrawingSystemColors.HighlightText;
    cellStyle.BackColor = DrawingSystemColors.Highlight;
  }

  /// <summary>
  ///   Highlights Cells on Ending of Edit
  /// </summary>
  /// <param name="sender">the object (this dgv)</param>
  /// <param name="e">the EventArgs given</param>
  private static void CellEndEdit(object sender, DataGridViewCellEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    var cell = dgv[e.ColumnIndex, e.RowIndex];
    if (!_cellEditStates.TryGetValue(cell, out var state))
      return;

    state.ToCell(cell);
    _cellEditStates.Remove(cell);
  }

  /// <summary>
  ///   Changes the last selected cell to new selected cells
  /// </summary>
  /// <param name="sender">the object (this dgv)</param>
  /// <param name="e">the EventArgs given</param>
  private static void SelectionChanged(object sender, EventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    var selectedCells = dgv.SelectedCells.Cast<DataGridViewCell>().ToArray();

    if (!selectedCells.Any())
      return;

    var lastSelected = selectedCells.Last();

    for (var i = 0; i < selectedCells.Length - 1; ++i) {
      var cell = selectedCells[i];
      if (cell == lastSelected)
        continue;

      if (cell.ColumnIndex != lastSelected.ColumnIndex)
        cell.Selected = false;
    }
  }

  private sealed class DataGridViewValidationState {
    public bool HasStartedMultipleValueChange => true;
  }

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
    private static readonly ConditionalWeakTable<DataGridView, DataGridViewValidationState> _dataGridViewValidationStates
 = new();
#else
  private static readonly Dictionary<DataGridView, DataGridViewValidationState> _dataGridViewValidationStates = new();
#endif

  private static void This_OnCellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (dgv.SelectedCells.Count <= 1 || (_dataGridViewValidationStates.TryGetValue(dgv, out var state) &&
                                         state.HasStartedMultipleValueChange))
      return;

    _dataGridViewValidationStates.Add(dgv, new());

    foreach (DataGridViewCell cell in dgv.SelectedCells)
      if (cell.RowIndex != e.RowIndex && cell.ColumnIndex == e.ColumnIndex)
        cell.Value = e.FormattedValue;

    _dataGridViewValidationStates.Remove(dgv);
  }

  #region various reflection caches

  private static readonly ConcurrentDictionary<Type, object[]> _TYPE_ATTRIBUTE_CACHE = new();

  /// <summary>
  ///   Queries for certain class/struct attribute in given type and all inherited interfaces.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="baseType">The base type.</param>
  /// <returns>An enumeration of all matching attributes or <c>null</c>.</returns>
  private static IEnumerable<TAttribute> _QueryPropertyAttribute<TAttribute>(Type baseType)
    where TAttribute : Attribute {
    // find all attributes, even in inherited interfaces

    var results = _TYPE_ATTRIBUTE_CACHE.GetOrAdd(baseType, type => type
      .GetCustomAttributes(true)
      .Concat(baseType.GetInterfaces().SelectMany(_GetInheritedCustomAttributes))
      .ToArray()
    );

    return results.OfType<TAttribute>();
  }

  private static object[] _GetInheritedCustomAttributes(ICustomAttributeProvider property) =>
    property.GetCustomAttributes(true);

  private static readonly ConcurrentDictionary<string, object[]> _PROPERTY_ATTRIBUTE_CACHE = new();
  private static readonly ConcurrentDictionary<string, string> _ENUM_DISPLAYNAME_CACHE = new();

  private static string _GetEnumDisplayName(object value) {
    if (value == null)
      return null;

    var type = value.GetType();
    if (!type.IsEnum)
      return null;

    var key = type.FullName + "\0" + value;
    if (!_ENUM_DISPLAYNAME_CACHE.TryGetValue(key, out var result))
      result = _ENUM_DISPLAYNAME_CACHE.GetOrAdd(key, _ => {
        var displayText = (DisplayNameAttribute)
            type
              .GetField(value.ToString())?
              .GetCustomAttributes(typeof(DisplayNameAttribute), false)
              .FirstOrDefault()
          ;
        return displayText?.DisplayName;
      });

    return result;
  }

  /// <summary>
  ///   Queries for certain property attribute in given type and all inherited interfaces.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="baseType">The base type.</param>
  /// <param name="propertyName">Name of the property.</param>
  /// <returns>An enumeration of all matching attributes or <c>null</c>.</returns>
  private static IEnumerable<TAttribute> _QueryPropertyAttribute<TAttribute>(Type baseType, string propertyName)
    where TAttribute : Attribute {
    var key = baseType.FullName + "\0" + propertyName + "\0" + typeof(TAttribute).Name;
    return _PROPERTY_ATTRIBUTE_CACHE.TryGetValue(key, out var results)
        ? results?.OfType<TAttribute>()
        : _QueryPropertyAttributeAndCache<TAttribute>(key, baseType, propertyName)
      ;
  }

  private static IEnumerable<TAttribute> _QueryPropertyAttributeAndCache<TAttribute>(string key, Type baseType,
    string propertyName) where TAttribute : Attribute {
    TAttribute[] ValueFactory(string _) => _GetAttributesForProperty<TAttribute>(baseType, propertyName).ToArray();
    return _PROPERTY_ATTRIBUTE_CACHE.GetOrAdd(key, ValueFactory)?.OfType<TAttribute>();
  }

  private static IEnumerable<TAttribute> _GetAttributesForProperty<TAttribute>(Type type, string propertyName)
    => _GetProperties(type, propertyName)
      .SelectMany(p => p.GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>());

  private static IEnumerable<PropertyInfo> _GetProperties(Type type, string propertyName) {
    var list = new LinkedList<Type>();
    list.AddFirst(type);
    for (;;) {
      var first = list.First;
      if (first == null)
        break;

      list.RemoveFirst();
      type = first.Value;

      var p = type.GetProperty(propertyName,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
      if (p != null)
        yield return p;

      // add base type for testing next
      var baseType = type.BaseType;
      if (baseType != null) {
        first.Value = baseType; // re-using LinkedListNode to avoid GC
        list.AddFirst(first);
      }

      // add interfaces to be tested after all base-types are done
      foreach (var i in type.GetInterfaces())
        list.AddLast(i);
    }
  }

  private static readonly ConcurrentDictionary<string, Func<object, object>> _PROPERTY_GETTER_CACHE = new();

  /// <summary>
  ///   Gets the property value or default.
  /// </summary>
  /// <typeparam name="TValue">The type of the property.</typeparam>
  /// <param name="value">The value.</param>
  /// <param name="propertyName">Name of the property.</param>
  /// <param name="defaultValueNullValue">The default value to return when value is <c>null</c>.</param>
  /// <param name="defaultValueNoProperty">The default value to return when propertyname is <c>null</c>.</param>
  /// <param name="defaultValuePropertyNotFound">The default value to return when property not found.</param>
  /// <param name="defaultValuePropertyWrongType">The default value to return when property type does not match.</param>
  /// <returns></returns>
  internal static TValue GetPropertyValueOrDefault<TValue>(object value, string propertyName,
    TValue defaultValueNullValue, TValue defaultValueNoProperty, TValue defaultValuePropertyNotFound,
    TValue defaultValuePropertyWrongType) {
    // null value, return default
    if (value is null)
      return defaultValueNullValue;

    // if no property given, return default
    if (propertyName == null)
      return defaultValueNoProperty;

    // find property and ask for bool values
    var type = value.GetType();
    var key = type + "\0" + propertyName;

    if (!_PROPERTY_GETTER_CACHE.TryGetValue(key, out var property)) {
      // only allocate lambda class if key not existing to keep GC pressure small
      Func<object, object> ValueFactory(string _) {
        var prop = _GetProperties(type, propertyName).FirstOrDefault();
        return prop == null ? null : _GetWeaklyTypedGetterDelegate(prop);
      }

      property = _PROPERTY_GETTER_CACHE.GetOrAdd(key, ValueFactory);
    }

    // property not found, return default
    if (property == null)
      return defaultValuePropertyNotFound;

    var result = property(value);
    if (result is TValue i)
      return i;

    // not right type, return default
    return defaultValuePropertyWrongType;
  }

  /// <summary>
  ///   Gets the property value or default.
  /// </summary>
  /// <typeparam name="TValue">The type of the property.</typeparam>
  /// <param name="callerName">The caller attribute name</param>
  /// <param name="value">The value.</param>
  /// <param name="propertyName">Name of the property.</param>
  /// <param name="defaultValueNullValue">The default value to return when value is <c>null</c>.</param>
  /// <returns></returns>
  internal static TValue GetPropertyValue<TValue>(string callerName, object value, string propertyName,
    TValue defaultValueNullValue) {
    // null value, return default
    if (value is null)
      return defaultValueNullValue;

    // if no property given, return default
    if (propertyName == null)
      throw new($"{callerName}: No property name set");

    // find property and ask for bool values
    var type = value.GetType();
    var key = type + "\0" + propertyName;

    if (!_PROPERTY_GETTER_CACHE.TryGetValue(key, out var property)) {
      // only allocate lambda class if key not existing to keep GC pressure small
      Func<object, object> ValueFactory(string _) {
        var prop = _GetProperties(type, propertyName).FirstOrDefault();
        return prop == null ? null : _GetWeaklyTypedGetterDelegate(prop);
      }

      property = _PROPERTY_GETTER_CACHE.GetOrAdd(key, ValueFactory);
    }

    // property not found, return default
    if (property == null)
      throw new($"{callerName}: Could not find {type.FullName}.{propertyName}");

    var result = property(value);
    switch (result) {
      case TValue i:
        return i;
      case null when !typeof(TValue).IsValueType:
        return default;
      default:
        throw new(
          $"{callerName}: Property {type.FullName}.{propertyName} has wrong type '{(result == null ? "null" : result.GetType().FullName)}', expected '{typeof(TValue).FullName}'");
    }
  }

  /// <summary>
  ///   Creates a weakly typed delegate to call the get method very fast.
  /// </summary>
  /// <param name="property">The property.</param>
  /// <returns></returns>
  private static Func<object, object> _GetWeaklyTypedGetterDelegate(PropertyInfo property) {
    // find getter
    var method = property.GetGetMethod(true);
    if (method == null)
      return null;

    if (method.IsStatic) {
      var d = Delegate.CreateDelegate(typeof(Func<object>), method);
      return _ => d.DynamicInvoke(null);
    }

    // use helper method to get weakly typed version
    var createWeaklyTypedDelegateMethod = typeof(DataGridViewExtensions).GetMethod(nameof(_CreateWeaklyTypedDelegate),
      BindingFlags.Static | BindingFlags.NonPublic);
    Debug.Assert(createWeaklyTypedDelegateMethod != null);
    var constructor = createWeaklyTypedDelegateMethod.MakeGenericMethod(method.DeclaringType, method.ReturnType);
    return (Func<object, object>)constructor.Invoke(null, new object[] { method });
  }

  // ReSharper disable once UnusedMethodReturnValue.Local
  /// <summary>
  ///   Creates a weakly-typed delegate for the given method info.
  /// </summary>
  /// <typeparam name="TTarget">The type of the method's first parameter, usually the methods declaring type.</typeparam>
  /// <typeparam name="TReturn">The type of the return value.</typeparam>
  /// <param name="method">The method.</param>
  /// <returns></returns>
  private static Func<object, object> _CreateWeaklyTypedDelegate<TTarget, TReturn>(MethodInfo method)
    where TTarget : class {
    // get a type-safe delegate
    var func = (Func<TTarget, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);

    // wrap it into a weakly typed delegate
    return target => func((TTarget)target);
  }

  /// <summary>
  ///   Calls the late bound method.
  /// </summary>
  /// <param name="instance">The instance to call the method from.</param>
  /// <param name="methodName">Name of the method.</param>
  /// <returns>An object representing the return value of the called method, or null if the methods return type is void</returns>
  [DebuggerStepThrough]
  internal static void CallLateBoundMethod(object instance, string methodName) {
    if (ReferenceEquals(null, instance))
      return;

    if (methodName == null || methodName.Trim().Length < 1)
      return;

    var type = instance.GetType();
    var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (method == null)
      return;

    method.Invoke(instance, null);
  }

  #endregion
}
