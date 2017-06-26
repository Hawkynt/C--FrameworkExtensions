#region (c)2010-2020 Hawkynt
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

// TODO: buttoncolumn with image support
namespace System.Windows.Forms {

  #region custom datagridviewcolumns

  public class DataGridViewProgressBarColumn : DataGridViewTextBoxColumn {

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
        if (value is int)
          intValue = (int)value;
        if (value is float)
          intValue = (float)value;
        if (value is double)
          intValue = (double)value;
        if (value is decimal)
          intValue = (double)(decimal)value;

        if (intValue < this.Minimum)
          intValue = this.Minimum;

        if (intValue > this.Maximum)
          intValue = this.Maximum;

        var rate = (intValue - this.Minimum) / (this.Maximum - this.Minimum);

        if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
          this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

        var borderRect = this.BorderWidths(advancedBorderStyle);
        var paintRect = new Rectangle(cellBounds.Left + borderRect.Left, cellBounds.Top + borderRect.Top, cellBounds.Width - borderRect.Right, cellBounds.Height - borderRect.Bottom);

        var isSelected = (cellState & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
        var bkColor =
            isSelected && (paintParts & DataGridViewPaintParts.SelectionBackground) == DataGridViewPaintParts.SelectionBackground
              ? cellStyle.SelectionBackColor
              : cellStyle.BackColor
          ;

        if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
          using (var backBrush = new SolidBrush(bkColor))
            graphics.FillRectangle(backBrush, paintRect);

        paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
        paintRect.Width -= cellStyle.Padding.Horizontal;
        paintRect.Height -= cellStyle.Padding.Vertical;

        if ((paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground) {
          if (ProgressBarRenderer.IsSupported) {
            ProgressBarRenderer.DrawHorizontalBar(graphics, paintRect);
            var barBounds = new Rectangle(paintRect.Left + 3, paintRect.Top + 3, paintRect.Width - 4, paintRect.Height - 6);
            barBounds.Width = Convert.ToInt32(Math.Round(barBounds.Width * rate));
            ProgressBarRenderer.DrawHorizontalChunks(graphics, barBounds);
          } else {
            graphics.FillRectangle(Brushes.White, paintRect);
            graphics.DrawRectangle(Pens.Black, paintRect);
            var barBounds = new Rectangle(paintRect.Left + 1, paintRect.Top + 1, paintRect.Width - 1, paintRect.Height - 1);
            barBounds.Width = Convert.ToInt32(Math.Round((barBounds.Width * rate)));
            graphics.FillRectangle(Brushes.Blue, barBounds);
          }
        }

        if (this.DataGridView.CurrentCellAddress.X == this.ColumnIndex && this.DataGridView.CurrentCellAddress.Y == this.RowIndex && (paintParts & DataGridViewPaintParts.Focus) == DataGridViewPaintParts.Focus && this.DataGridView.Focused) {
          var focusRect = paintRect;
          focusRect.Inflate(-3, -3);
          ControlPaint.DrawFocusRectangle(graphics, focusRect);
        }

        if ((paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground) {
          var txt = $"{Math.Round(rate * 100)}%";
          const TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
          var fColor = cellStyle.ForeColor;
          paintRect.Inflate(-2, -2);
          TextRenderer.DrawText(graphics, txt, cellStyle.Font, paintRect, fColor, flags);
        }

        if (
          (paintParts & DataGridViewPaintParts.ErrorIcon) != DataGridViewPaintParts.ErrorIcon
          || !this.DataGridView.ShowCellErrors || string.IsNullOrEmpty(errorText)
        )
          return;

        var iconBounds = this.GetErrorIconBounds(graphics, cellStyle, rowIndex);
        iconBounds.Offset(cellBounds.X, cellBounds.Y);
        this.PaintErrorIcon(graphics, iconBounds, cellBounds, errorText);
      }
    }

    public DataGridViewProgressBarColumn() {
      this.CellTemplate = new DataGridViewProgressBarCell();
    }

    public override DataGridViewCell CellTemplate {
      get { return base.CellTemplate; }
      set {
        if (value is DataGridViewProgressBarCell)
          base.CellTemplate = value;
        else
          throw new InvalidCastException(nameof(DataGridViewProgressBarCell));
      }
    }

    public double Maximum {
      get { return ((DataGridViewProgressBarCell)this.CellTemplate).Maximum; }
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
      get { return ((DataGridViewProgressBarCell)this.CellTemplate).Minimum; }
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

  internal class DataGridViewDisableButtonColumn : DataGridViewButtonColumn {

    /// <summary>
    /// The cell template to use for drawing the cells' content.
    /// </summary>
    public class DataGridViewDisableButtonCell : DataGridViewButtonCell {

      /// <summary>
      /// Gets or sets a value indicating whether this <see cref="DataGridViewDisableButtonCell"/> is enabled.
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
      public DataGridViewDisableButtonCell() {
        this.Enabled = true;
      }

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
        if (this.Enabled) {
          base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
          return;
        }

        // The button cell is disabled, so paint the border,
        // background, and disabled button for the cell.

        // Draw the cell background, if specified.
        if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
          using (var cellBackground = new SolidBrush(cellStyle.BackColor))
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

        // Draw the disabled button.
        ButtonRenderer.DrawButton(graphics, buttonArea, PushButtonState.Disabled);

        // Draw the disabled button text.
        if (this.FormattedValue is string)
          TextRenderer.DrawText(graphics, (string)this.FormattedValue, this.DataGridView.Font, buttonArea, SystemColors.GrayText);
      }
    }

    public DataGridViewDisableButtonColumn() {
      this.CellTemplate = new DataGridViewDisableButtonCell();
    }
  }

  #endregion

  #region attributes for messing with auto-generated columns

  /// <summary>
  /// Allows specifying certain properties as read-only depending on the underlying object instance.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  internal sealed class DataGridViewConditionalReadOnlyAttribute : Attribute {
    public DataGridViewConditionalReadOnlyAttribute(string isReadOnlyWhen) {
      this.IsReadOnlyWhen = isReadOnlyWhen;
    }

    public string IsReadOnlyWhen { get; }
    public bool IsReadOnly(object value) => DataGridViewExtensions.GetPropertyValueOrDefault(value, this.IsReadOnlyWhen, false, false, false, false);
  }

  /// <summary>
  /// Allows specifying a value to be used as a progressbar column.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  internal sealed class DataGridViewProgressBarColumnAttribute : Attribute {
    public DataGridViewProgressBarColumnAttribute(double minimum = 0, double maximum = 100) {
      this.Minimum = minimum;
      this.Maximum = maximum;
    }

    public double Minimum { get; }
    public double Maximum { get; }
  }

  /// <summary>
  /// Allows specifying a string or image property to be used as a button column.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  internal sealed class DataGridViewButtonColumnAttribute : Attribute {
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridViewButtonColumnAttribute"/> class.
    /// </summary>
    /// <param name="targetMethod">The target method name to call upon click.</param>
    /// <param name="isEnabledWhen">The boolean property which enables or disables the buttons.</param>
    public DataGridViewButtonColumnAttribute(string targetMethod, string isEnabledWhen = null) {
      this.TargetMethod = targetMethod;
      this.IsEnabledWhen = isEnabledWhen;
    }
    public string IsEnabledWhen { get; }

    public string TargetMethod { get; }

    /// <summary>
    /// Executes the callback with the given object instance.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Execute(object value) {
      if (ReferenceEquals(null, value))
        return;

      var property = value.GetType().GetMethod(this.TargetMethod);
      property?.Invoke(value, null);
    }

    public bool IsEnabled(object value) => DataGridViewExtensions.GetPropertyValueOrDefault(value, this.IsEnabledWhen, false, true, false, false);

  }

  /// <summary>
  /// Allows setting an exact width in pixels for automatically generated columns.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  internal sealed class DataGridViewColumnWidthAttribute : Attribute {
    public DataGridViewColumnWidthAttribute(int width) {
      this.Width = width;
      this.Mode = DataGridViewAutoSizeColumnMode.None;
    }

    public DataGridViewColumnWidthAttribute(DataGridViewAutoSizeColumnMode mode) {
      this.Mode = mode;
      this.Width = -1;
    }

    public DataGridViewAutoSizeColumnMode Mode { get; }
    public int Width { get; }

    public void ApplyTo(DataGridViewColumn column) {
      if (this.Mode != DataGridViewAutoSizeColumnMode.None) {
        column.AutoSizeMode = this.Mode;
        return;
      }

      column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      column.MinimumWidth = this.Width;
      column.Width = this.Width;
    }
  }

  /// <summary>
  /// Allows adjusting the cell style in a DataGridView for automatically generated columns.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
  internal sealed class DataGridViewCellStyleAttribute : Attribute {

    public DataGridViewCellStyleAttribute(string foreColor = null, string backColor = null, string format = null, string conditionalPropertyName = null, string foreColorPropertyName = null, string backColorPropertyName = null) {
      this.ForeColor = foreColor == null ? (Color?)null : DataGridViewExtensions._ParseColor(foreColor);
      this.BackColor = backColor == null ? (Color?)null : DataGridViewExtensions._ParseColor(backColor);
      this.ConditionalPropertyName = conditionalPropertyName;
      this.Format = format;
      this.ForeColorPropertyName = foreColorPropertyName;
      this.BackColorPropertyName = backColorPropertyName;
    }

    public string ConditionalPropertyName { get; }
    public Color? ForeColor { get; }
    public Color? BackColor { get; }
    public string Format { get; }
    public string ForeColorPropertyName { get; }
    public string BackColorPropertyName { get; }

    public void ApplyTo(DataGridViewCellStyle style, object value) {
      var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(value, this.ForeColorPropertyName, null, null, null, null) ?? this.ForeColor;
      if (color != null)
        style.ForeColor = color.Value;

      color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(value, this.BackColorPropertyName, null, null, null, null) ?? this.BackColor;
      if (color != null)
        style.BackColor = color.Value;

      if (this.Format != null)
        style.Format = this.Format;
    }

    public bool IsEnabled(object value) => DataGridViewExtensions.GetPropertyValueOrDefault(value, this.ConditionalPropertyName, true, true, false, false);

  }

  /// <summary>
  /// Allows adjusting the cell style in a DataGridView for automatically generated columns.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
  internal sealed class DataGridViewRowStyleAttribute : Attribute {

    public DataGridViewRowStyleAttribute(string foreColor = null, string backColor = null, string format = null, string conditionalPropertyName = null, string foreColorPropertyName = null, string backColorPropertyName = null) {
      this.ForeColor = foreColor == null ? (Color?)null : DataGridViewExtensions._ParseColor(foreColor);
      this.BackColor = backColor == null ? (Color?)null : DataGridViewExtensions._ParseColor(backColor);
      this.ConditionalPropertyName = conditionalPropertyName;
      this.Format = format;
      this.ForeColorPropertyName = foreColorPropertyName;
      this.BackColorPropertyName = backColorPropertyName;
    }

    public string ConditionalPropertyName { get; }
    public Color? ForeColor { get; }
    public Color? BackColor { get; }
    public string Format { get; }
    public string ForeColorPropertyName { get; }
    public string BackColorPropertyName { get; }

    public void ApplyTo(DataGridViewCellStyle style, object value) {

      var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(value, this.ForeColorPropertyName, null, null, null, null) ?? this.ForeColor;
      if (color != null)
        style.ForeColor = color.Value;

      color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(value, this.BackColorPropertyName, null, null, null, null) ?? this.BackColor;
      if (color != null)
        style.BackColor = color.Value;

      if (this.Format != null)
        style.Format = this.Format;
    }

    public bool IsEnabled(object value) => DataGridViewExtensions.GetPropertyValueOrDefault(value, this.ConditionalPropertyName, true, true, false, false);

  }


  #endregion

  internal static partial class DataGridViewExtensions {

    #region messing with auto-generated columns

    public static void EnableExtendedAttributes(this DataGridView @this) {
      if (@this == null)
        throw new NullReferenceException();

      // unsubscribe first to avoid duplicate subscriptions
      @this.DataSourceChanged -= _DataSourceChanged;
      @this.CellPainting -= _CellPainting;
      @this.RowPostPaint -= _RowPostPaint;
      @this.RowPrePaint -= _RowPrePaint;
      @this.CellClick -= _CellClick;
      @this.EnabledChanged -= _EnabledChanged;
      @this.Disposed -= _RemoveDisabledState;

      // subscribe to events
      @this.CellPainting += _CellPainting;
      @this.DataSourceChanged += _DataSourceChanged;
      @this.RowPostPaint += _RowPostPaint;
      @this.RowPrePaint += _RowPrePaint;
      @this.CellClick += _CellClick;
      @this.EnabledChanged += _EnabledChanged;
      @this.Disposed += _RemoveDisabledState;
    }

    /// <summary>
    /// Executes button column click events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs" /> instance containing the event data.</param>
    private static void _CellClick(object sender, DataGridViewCellEventArgs e) {
      var dgv = sender as DataGridView;

      if (dgv == null)
        return;

      if (e.RowIndex < 0 || e.ColumnIndex < 0)
        return;

      var type = FindItemType(dgv);

      if (type == null)
        return;

      var column = dgv.Columns[e.ColumnIndex] as DataGridViewButtonColumn;
      if (column == null)
        return;

      if (!column.IsDataBound)
        return;

      var item = dgv.Rows[e.RowIndex].DataBoundItem;

      var buttonAttribute = _QueryPropertyAttribute<DataGridViewButtonColumnAttribute>(type, column.DataPropertyName)?.FirstOrDefault();
      if (buttonAttribute == null)
        return;

      if (buttonAttribute.IsEnabled(item))
        buttonAttribute.Execute(item);
    }

    /// <summary>
    /// Fixes column widths according to property attributes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="_">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private static void _DataSourceChanged(object sender, EventArgs _) {
      var dgv = sender as DataGridView;

      if (dgv == null)
        return;

      var type = FindItemType(dgv);

      if (type == null)
        return;

      var columns = dgv.Columns;

      for (var i = 0; i < columns.Count; i++) {
        var column = columns[i];

        // ignore unbound columns
        if (!column.IsDataBound)
          continue;

        var propertyName = column.DataPropertyName;
        var property = type.GetProperty(propertyName);

        // ignore unknown properties
        if (property == null)
          continue;

        // if needed replace DataGridViewTextBoxColumns with DataGridViewButtonColumn
        var buttonColumnAttribute = (DataGridViewButtonColumnAttribute)property.GetCustomAttributes(typeof(DataGridViewButtonColumnAttribute), true).FirstOrDefault();
        if (buttonColumnAttribute != null) {
          var newColumn = new DataGridViewDisableButtonColumn {
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
          columns.RemoveAt(i);
          columns.Insert(i, newColumn);
          column = newColumn;
        }

        // if needed replace DataGridViewTextBoxColumns with DataGridViewProgressBarColumn
        var progressBarColumnAttribute = (DataGridViewProgressBarColumnAttribute)property.GetCustomAttributes(typeof(DataGridViewProgressBarColumnAttribute), true).FirstOrDefault();
        if (progressBarColumnAttribute != null) {
          var newColumn = new DataGridViewProgressBarColumn {
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
          columns.RemoveAt(i);
          columns.Insert(i, newColumn);
          column = newColumn;
        }

        // if needed replace DataGridViewTextBoxColumns for Enums with DataGridViewComboboxColumn
        var propType = property.PropertyType;

        if (propType.IsEnum && !column.ReadOnly) {
          var newColumn = _ConstructEnumComboboxColumn(propType, column);
          columns.RemoveAt(i);
          columns.Insert(i, newColumn);
          column = newColumn;
        }

        // apply column width
        _QueryPropertyAttribute<DataGridViewColumnWidthAttribute>(type, propertyName)?.FirstOrDefault()?.ApplyTo(column);
      }
    }

    /// <summary>
    /// Construct a DataGridViewComboboxColumn for enum types.
    /// </summary>
    /// <param name="enumType">Type of the enum.</param>
    /// <param name="originalColumn">The original column.</param>
    /// <returns></returns>
    private static DataGridViewComboBoxColumn _ConstructEnumComboboxColumn(Type enumType, DataGridViewColumn originalColumn) {
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

    /// <summary>
    /// Adjusts styles according to property attributes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellPaintingEventArgs" /> instance containing the event data.</param>
    private static void _CellPainting(object sender, DataGridViewCellPaintingEventArgs e) {
      var dgv = sender as DataGridView;

      if (dgv == null)
        return;

      var columnIndex = e.ColumnIndex;

      if (columnIndex < 0 || columnIndex >= dgv.ColumnCount)
        return;

      var column = dgv.Columns[e.ColumnIndex];

      if (!column.IsDataBound)
        return;

      var columnPropertyName = column.DataPropertyName;

      var rowIndex = e.RowIndex;

      if (rowIndex < 0 || rowIndex >= dgv.RowCount)
        return;

      var value = dgv.Rows[rowIndex].DataBoundItem;
      var type = rowIndex < 0 ? FindItemType(dgv) : value?.GetType() ?? FindItemType(dgv);

      if (type == null)
        return;

      // apply cell style
      var attributes = _QueryPropertyAttribute<DataGridViewCellStyleAttribute>(type, columnPropertyName);

      if (attributes != null)
        foreach (var attribute in attributes)
          if (attribute.IsEnabled(value))
            attribute.ApplyTo(e.CellStyle, value);

    }

    /// <summary>
    /// Fixes row styles.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewRowPrePaintEventArgs" /> instance containing the event data.</param>
    private static void _RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e) {
      var dgv = sender as DataGridView;

      if (dgv == null)
        return;

      var rowIndex = e.RowIndex;

      if (rowIndex < 0 || rowIndex >= dgv.RowCount)
        return;

      var value = dgv.Rows[rowIndex].DataBoundItem;
      var type = value?.GetType() ?? FindItemType(dgv);

      var attributes = _QueryPropertyAttribute<DataGridViewRowStyleAttribute>(type);
      if (attributes != null)
        foreach (var attribute in attributes)
          if (attribute.IsEnabled(value))
            attribute.ApplyTo(dgv.Rows[rowIndex].DefaultCellStyle, value);
    }

    /// <summary>
    /// Fixes column where some cells could be read-only depending on bound object type.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewRowPostPaintEventArgs" /> instance containing the event data.</param>
    private static void _RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e) {
      var dgv = sender as DataGridView;

      if (dgv == null)
        return;

      var rowIndex = e.RowIndex;

      if (rowIndex < 0 || rowIndex >= dgv.RowCount)
        return;

      var value = dgv.Rows[rowIndex].DataBoundItem;
      var type = value?.GetType() ?? FindItemType(dgv);

      if (type == null)
        return;

      var cells = dgv.Rows[rowIndex].Cells;

      // repair cell styles (eg readonly cells, disabled button cells)
      foreach (DataGridViewColumn column in dgv.Columns) {

        if (column.DataPropertyName == null)
          continue;

        var cell = cells[column.Index];
        if (!dgv.ReadOnly)
          _FixReadOnlyCellStyle(type, column, cell, value);

        _FixDisabledButtonCellStyle(type, column, cell, value);

      }
    }

    /// <summary>
    /// Fixes the cell style for DataGridViewDisableButtonColumns depending on actual value.
    /// </summary>
    /// <param name="type">The type of the bound item.</param>
    /// <param name="column">The column.</param>
    /// <param name="cell">The cell.</param>
    /// <param name="value">The value.</param>
    private static void _FixDisabledButtonCellStyle(Type type, DataGridViewColumn column, DataGridViewCell cell, object value) {
      var dgvButtonColumnAttribute = _QueryPropertyAttribute<DataGridViewButtonColumnAttribute>(type, column.DataPropertyName)?.FirstOrDefault();
      if (column is DataGridViewDisableButtonColumn)
        ((DataGridViewDisableButtonColumn.DataGridViewDisableButtonCell)cell).Enabled = dgvButtonColumnAttribute?.IsEnabled(value) ?? !ReferenceEquals(null, value);
    }

    /// <summary>
    /// Fxies the cell style for read-only cells in (normally) non-read-only columns.
    /// </summary>
    /// <param name="type">The type of the bound item.</param>
    /// <param name="column">The column.</param>
    /// <param name="cell">The cell.</param>
    /// <param name="value">The value.</param>
    private static void _FixReadOnlyCellStyle(Type type, DataGridViewColumn column, DataGridViewCell cell, object value) {
      var readOnlyAttribute = _QueryPropertyAttribute<ReadOnlyAttribute>(type, column.DataPropertyName)?.FirstOrDefault();
      if (readOnlyAttribute != null)
        cell.ReadOnly = readOnlyAttribute.IsReadOnly;

      var dgvReadOnlyAttribute = _QueryPropertyAttribute<DataGridViewConditionalReadOnlyAttribute>(type, column.DataPropertyName)?.FirstOrDefault();
      if (dgvReadOnlyAttribute != null)
        cell.ReadOnly = dgvReadOnlyAttribute.IsReadOnly(value);

      if (!cell.ReadOnly)
        return;

      cell.Style.BackColor = SystemColors.Control;
      cell.Style.ForeColor = SystemColors.GrayText;
    }

    #endregion

    #region fixing stuff

    /// <summary>
    /// Saves the state of a DataGridView during Enable/Disable state transitions.
    /// </summary>
    private class DataGridViewState {

      private readonly bool _readonly;
      private readonly Color _defaultCellStyleBackColor;
      private readonly Color _defaultCellStyleForeColor;
      private readonly Color _columnHeadersDefaultCellStyleBackColor;
      private readonly Color _columnHeadersDefaultCellStyleForeColor;
      private readonly bool _enableHeadersVisualStyles;
      private readonly Color _backgroundColor;

      private DataGridViewState(bool @readonly, Color defaultCellStyleBackColor, Color defaultCellStyleForeColor, Color columnHeadersDefaultCellStyleBackColor, Color columnHeadersDefaultCellStyleForeColor, bool enableHeadersVisualStyles, Color backgroundColor) {
        this._readonly = @readonly;
        this._defaultCellStyleBackColor = defaultCellStyleBackColor;
        this._defaultCellStyleForeColor = defaultCellStyleForeColor;
        this._columnHeadersDefaultCellStyleBackColor = columnHeadersDefaultCellStyleBackColor;
        this._columnHeadersDefaultCellStyleForeColor = columnHeadersDefaultCellStyleForeColor;
        this._enableHeadersVisualStyles = enableHeadersVisualStyles;
        this._backgroundColor = backgroundColor;
      }

      /// <summary>
      /// Restores the saved state to the given DataGridView.
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
      /// Saves the state of the given DataGridView.
      /// </summary>
      /// <param name="dataGridView">The DataGridView to save state from.</param>
      /// <returns></returns>
      public static DataGridViewState FromDataGridView(DataGridView dataGridView) {
        return new DataGridViewState(
          dataGridView.ReadOnly,
          dataGridView.DefaultCellStyle.BackColor,
          dataGridView.DefaultCellStyle.ForeColor,
          dataGridView.ColumnHeadersDefaultCellStyle.BackColor,
          dataGridView.ColumnHeadersDefaultCellStyle.ForeColor,
          dataGridView.EnableHeadersVisualStyles,
          dataGridView.BackgroundColor
        );
      }

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

#if NETFX_4
    private static readonly ConditionalWeakTable<DataGridView, DataGridViewState> _DGV_STATUS_BACKUPS = new ConditionalWeakTable<DataGridView, DataGridViewState>();
#else
    private static readonly Dictionary<DataGridView, DataGridViewState> _DGV_STATUS_BACKUPS = new Dictionary<DataGridView, DataGridViewState>();
#endif

    /// <summary>
    /// Handles the Disposed event of the control; removes any state from the state list for the given DataGridView.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="_">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private static void _RemoveDisabledState(object sender, EventArgs _) {
      var dgv = sender as DataGridView;
      if (dgv == null)
        return;

      _DGV_STATUS_BACKUPS.Remove(dgv);
    }

    /// <summary>
    /// Handles the EnabledChanged event of the control; saves the state in the state list and changes colors and borders to appear grayed-out.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="_">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private static void _EnabledChanged(object sender, EventArgs _) {
      var dgv = sender as DataGridView;
      if (dgv == null)
        return;

      if (dgv.Enabled) {

        // if state was saved, restore it
        DataGridViewState lastState;
        if (!_DGV_STATUS_BACKUPS.TryGetValue(dgv, out lastState))
          return;

        _DGV_STATUS_BACKUPS.Remove(dgv);
        lastState.RestoreTo(dgv);
      } else {

        // if state already saved, ignore
        DataGridViewState lastState;
        if (_DGV_STATUS_BACKUPS.TryGetValue(dgv, out lastState))
          return;

        var state = DataGridViewState.FromDataGridView(dgv);
        _DGV_STATUS_BACKUPS.Add(dgv, state);
        DataGridViewState.ChangeToDisabled(dgv);
      }
    }

    #endregion

    /// <summary>
    /// Finds the type of the items in a bound DataGridView.
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
        return type.GenericTypeArguments[0]; /* and IEnumerable<T>, etc. */

      return null;
    }

    /// <summary>
    /// Scrolls to the end.
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
        ;
      }
    }

    /// <summary>
    /// Clones the columns to another datagridview.
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

      target.Columns.AddRange((from i in @this.Columns.Cast<DataGridViewColumn>() select (DataGridViewColumn)i.Clone()).ToArray());
    }

    /// <summary>
    /// Finds the columns that match a certain condition.
    /// </summary>
    /// <param name="this">This DataGridView.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>An enumeration of columns.</returns>
    public static IEnumerable<DataGridViewColumn> FindColumns(this DataGridView @this, Predicate<DataGridViewColumn> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));

      return (from i in @this.Columns.Cast<DataGridViewColumn>() where predicate(i) select i);
    }

    /// <summary>
    /// Finds the first column that matches a certain condition.
    /// </summary>
    /// <param name="this">This DataGridView.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The first matching column or <c>null</c>.</returns>
    public static DataGridViewColumn FindFirstColumn(this DataGridView @this, Predicate<DataGridViewColumn> predicate) {
      if (@this == null)
        throw new NullReferenceException();

      var matches = FindColumns(@this, predicate);
      return matches?.FirstOrDefault();
    }

    /// <summary>
    /// Gets the selected items.
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
    /// Gets the selectedd items.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This DataGridView.</param>
    /// <returns>The currently selected items</returns>
    public static IEnumerable<TItem> GetSelectedItems<TItem>(this DataGridView @this) {
      if (@this == null)
        throw new NullReferenceException();

      return (@this.GetSelectedItems().Cast<TItem>());
    }

    /// <summary>
    /// Determines whether if any cell is currently selected.
    /// </summary>
    /// <param name="this">This DataGridView.</param>
    /// <returns><c>true</c> if any cell is currently selected; otherwise <c>false</c>.</returns>
    public static bool IsAnyCellSelected(this DataGridView @this) {
      if (@this == null)
        throw new NullReferenceException();

      return @this.SelectedCells.Count > 0;
    }

    /// <summary>
    /// Selects the rows containing the given items.
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
        if (row.DataBoundItem is TItem && bucket.Contains((TItem)row.DataBoundItem))
          row.Selected = true;
    }

    /// <summary>
    /// Resets the selection.
    /// </summary>
    /// <param name="this">This DataGridView.</param>
    public static void ResetSelection(this DataGridView @this) {
      if (@this == null)
        throw new NullReferenceException();

      foreach (DataGridViewRow row in @this.SelectedRows)
        row.Selected = false;
    }

    /// <summary>
    /// Refreshes the data source and restores selections and scroll position.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <param name="this">This DataGridView.</param>
    /// <param name="source">The source.</param>
    /// <param name="keyGetter">The key getter.</param>
    /// <param name="preAction">The pre action.</param>
    /// <param name="postAction">The post action.</param>
    public static void RefreshDataSource<TItem, TKey>(this DataGridView @this, IList<TItem> source, Func<TItem, TKey> keyGetter, Action preAction = null, Action postAction = null) {
      if (@this == null)
        throw new NullReferenceException();
      if (keyGetter == null)
        throw new ArgumentNullException(nameof(keyGetter));

      try {
        @this.SuspendLayout();

        // save scroll position
        var hscroll = @this.HorizontalScrollingOffset;
        var vscroll = @this.FirstDisplayedScrollingRowIndex;

        // save selected items
        var selected = new HashSet<TKey>(GetSelectedItems<TItem>(@this).Select(keyGetter));

        // reset data source
        preAction?.Invoke();
        @this.DataSource = source;
        postAction?.Invoke();

        // reselect
        foreach (var row in @this.Rows.Cast<DataGridViewRow>())
          row.Selected = selected.Contains(keyGetter((TItem)row.DataBoundItem));

        //re-apply scrolling
        if (vscroll >= 0 && vscroll < source.Count)
          @this.FirstDisplayedScrollingRowIndex = vscroll;

        @this.HorizontalScrollingOffset = hscroll;
      } finally {
        @this.ResumeLayout(true);
      }
    }

    /// <summary>
    /// Automatically adjusts the height of the control.
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

      var rowHeight = rows.Sum(row => row.Height + 1 /* 1px border between rows */ );
      @this.Height = headerHeight + rowHeight;
    }

    #region various reflection caches

    private static readonly ConcurrentDictionary<Type, object[]> _TYPE_ATTRIBUTE_CACHE = new ConcurrentDictionary<Type, object[]>();

    /// <summary>
    /// Queries for certain class/struct attribute in given type and all inherited interfaces.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="baseType">The base type.</param>
    /// <returns>An enumeration of all matching attributes or <c>null</c>.</returns>
    private static IEnumerable<TAttribute> _QueryPropertyAttribute<TAttribute>(Type baseType) where TAttribute : Attribute {
      // find all attributes, even in inherited interfaces

      var results = _TYPE_ATTRIBUTE_CACHE.GetOrAdd(baseType, type => {
        return type
          .GetCustomAttributes(true)
          .Concat(baseType.GetInterfaces().SelectMany(p => p.GetCustomAttributes(true)))
          .ToArray();
      });

      return results.OfType<TAttribute>();
    }

    private static readonly ConcurrentDictionary<string, object[]> _PROPERTY_ATTRIBUTE_CACHE = new ConcurrentDictionary<string, object[]>();

    /// <summary>
    /// Queries for certain property attribute in given type and all inherited interfaces.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="baseType">The base type.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>An enumeration of all matching attributes or <c>null</c>.</returns>
    private static IEnumerable<TAttribute> _QueryPropertyAttribute<TAttribute>(Type baseType, string propertyName) where TAttribute : Attribute {

      var results = _PROPERTY_ATTRIBUTE_CACHE.GetOrAdd(baseType.FullName + "\0" + propertyName, _ => {
        var property = baseType.GetProperty(propertyName);

        // ignore missing properties
        var declaringType = property?.DeclaringType;

        if (declaringType == null)
          return null;

        // find all attributes, even in inherited interfaces
        return property
            .GetCustomAttributes(true)
            .Concat(
              declaringType.GetInterfaces()
                .Select(intf => intf.GetProperty(propertyName))
                .Where(p => p != null)
                .SelectMany(p => p.GetCustomAttributes(true))
            )
            .ToArray()
          ;
      });

      return results?.OfType<TAttribute>();
    }


    private static readonly ConcurrentDictionary<string, PropertyInfo> _PROPERTY_CACHE = new ConcurrentDictionary<string, PropertyInfo>();
    /// <summary>
    /// Gets the property value or default.
    /// </summary>
    /// <typeparam name="TValue">The type of the property.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="defaultValueNullValue">The default value to return when value is <c>null</c>.</param>
    /// <param name="defaultValueNoProperty">The default value to return when propertyname is <c>null</c>.</param>
    /// <param name="defaultValuePropertyNotFound">The default value to return when property not found.</param>
    /// <param name="defaultValuePropertyWrongType">The default value to return when property type does not match.</param>
    /// <returns></returns>
    internal static TValue GetPropertyValueOrDefault<TValue>(object value, string propertyName, TValue defaultValueNullValue, TValue defaultValueNoProperty, TValue defaultValuePropertyNotFound, TValue defaultValuePropertyWrongType) {
      // null value, return default
      if (ReferenceEquals(null, value))
        return defaultValueNullValue;

      // if no property given, return default
      if (propertyName == null)
        return defaultValueNoProperty;

      // find property and ask for bool values
      var type = value.GetType();
      var property = _PROPERTY_CACHE.GetOrAdd(type + "\0" + propertyName, _ => type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

      // property not found, return default
      if (property == null)
        return defaultValuePropertyNotFound;

      var result = property.GetValue(value);
      if (result is TValue)
        return (TValue)result;

      // not right type, return default
      return defaultValuePropertyWrongType;
    }

    #endregion

    #region parsing colors

    private static readonly Regex _COLOR_MATCH = new Regex(@"^(?:#(?<eightdigit>[0-9a-z]{8}))|(?:#(?<sixdigit>[0-9a-z]{6}))|(?:#(?<fourdigit>[0-9a-z]{4}))|(?:#(?<threedigit>[0-9a-z]{3}))|(?<knowncolor>[a-z]+)|(?:'(?<systemcolor>[a-z]+)')$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Tuple<string, Func<string, Color>>[] _COLOR_PARSERS = {
      Tuple.Create<string, Func<string, Color>>("eightdigit",v=>Color.FromArgb(
        Convert.ToByte(string.Empty+v[0]+v[1],16),
        Convert.ToByte(string.Empty+v[2]+v[3],16),
        Convert.ToByte(string.Empty+v[4]+v[5],16),
        Convert.ToByte(string.Empty+v[6]+v[7],16)
      )),
      Tuple.Create<string, Func<string, Color>>("sixdigit",v=>Color.FromArgb(
        Convert.ToByte(string.Empty+v[0]+v[1],16),
        Convert.ToByte(string.Empty+v[2]+v[3],16),
        Convert.ToByte(string.Empty+v[4]+v[5],16)
      )),
      Tuple.Create<string, Func<string, Color>>("fourdigit",v=>Color.FromArgb(
        Convert.ToByte(string.Empty+v[0]+v[0],16),
        Convert.ToByte(string.Empty+v[1]+v[1],16),
        Convert.ToByte(string.Empty+v[2]+v[2],16),
        Convert.ToByte(string.Empty+v[3]+v[3],16)
      )),
      Tuple.Create<string, Func<string, Color>>("threedigit",v=>Color.FromArgb(
        Convert.ToByte(string.Empty+v[0]+v[0],16),
        Convert.ToByte(string.Empty+v[1]+v[1],16),
        Convert.ToByte(string.Empty+v[2]+v[2],16)
      )),
      Tuple.Create<string, Func<string, Color>>("knowncolor",Color.FromName),
      Tuple.Create<string, Func<string, Color>>("systemcolor",Color.FromName),
    };

    internal static Color _ParseColor(string @this) {
      var match = _COLOR_MATCH.Match(@this);
      if (!match.Success)
        throw new ArgumentException("Unknown color", nameof(@this));

      foreach (var parser in _COLOR_PARSERS) {
        var group = match.Groups[parser.Item1];
        if (group.Success)
          return parser.Item2(group.Value);

      }

      throw new ArgumentException("Unknown color", nameof(@this));
    }

    #endregion

  }
}
