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

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
using System.Runtime.CompilerServices;
#endif
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Guard;
using DrawingSystemColors = System.Drawing.SystemColors;
using DrawingFontStyle = System.Drawing.FontStyle;
using static System.Windows.Forms.DataGridViewNumericUpDownColumn;

// TODO: buttoncolumn with image support
namespace System.Windows.Forms;

public static partial class DataGridViewExtensions {
  #region messing with auto-generated columns

  public static void EnableExtendedAttributes(this DataGridView @this) {
    Against.ThisIsNull(@this);

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
    _QueryPropertyAttribute<DataGridViewClickableAttribute>(type, column.DataPropertyName)
      ?.FirstOrDefault()
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
      _QueryPropertyAttribute<DataGridViewButtonColumnAttribute>(type, column.DataPropertyName)
        ?.FirstOrDefault()
        ?.OnClick(item);

    _QueryPropertyAttribute<DataGridViewClickableAttribute>(type, column.DataPropertyName)
      ?.FirstOrDefault()
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
      if (!column.ReadOnly && (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))) {
        var newColumn = _ConstructDateTimePickerColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewTextBoxColumns with DataGridViewButtonColumn
      var buttonColumnAttribute = (DataGridViewButtonColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewButtonColumnAttribute), true)
        .FirstOrDefault();
      if (buttonColumnAttribute != null) {
        var newColumn = _ConstructDisableButtonColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewTextBoxColumns with DataGridViewProgressBarColumn
      var progressBarColumnAttribute = (DataGridViewProgressBarColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewProgressBarColumnAttribute), true)
        .FirstOrDefault();
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
        .GetCustomAttributes(typeof(DataGridViewImageColumnAttribute), true)
        .FirstOrDefault();
      if (imageColumnAttribute != null) {
        var newColumn = _ConstructImageColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewMultiImageColumn
      var multiImageColumnAttribute = (DataGridViewMultiImageColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewMultiImageColumnAttribute), true)
        .FirstOrDefault();
      if (multiImageColumnAttribute != null) {
        var newColumn = _ConstructMultiImageColumn(column, multiImageColumnAttribute);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewConditionalImageColumn
      var conditionalImageColumnAttribute = (SupportsConditionalImageAttribute)property
        .GetCustomAttributes(typeof(SupportsConditionalImageAttribute), true)
        .FirstOrDefault();
      if (conditionalImageColumnAttribute != null) {
        var newColumn = _ConstructImageAndTextColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewConditionalImageColumn
      var imageAndTextColumnAttribute = (DataGridViewImageAndTextColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewImageAndTextColumnAttribute), true)
        .FirstOrDefault();
      if (imageAndTextColumnAttribute != null) {
        var newColumn = _ConstructImageAndTextColumn(column);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewCheckboxColumn
      var checkboxColumnAttribute = (DataGridViewCheckboxColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewCheckboxColumnAttribute), true)
        .FirstOrDefault();
      if (checkboxColumnAttribute != null) {
        var newColumn = _ConstructCheckboxColumn(column, propType == typeof(bool?));
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewNumericUpDownColumn
      var numericUpDownColumnAttribute = (DataGridViewNumericUpDownColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewNumericUpDownColumnAttribute), true)
        .FirstOrDefault();
      if (numericUpDownColumnAttribute != null) {
        var newColumn = _ConstructNumericUpDownColumn(column, numericUpDownColumnAttribute);
        columns.RemoveAt(i);
        columns.Insert(i, newColumn);
        column = newColumn;
      }

      // if needed replace DataGridViewColumns with DataGridViewDropDownColumn
      var comboBoxColumnAttribute = (DataGridViewComboboxColumnAttribute)property
        .GetCustomAttributes(typeof(DataGridViewComboboxColumnAttribute), true)
        .FirstOrDefault();
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
      _QueryPropertyAttribute<DataGridViewColumnSortModeAttribute>(type, propertyName)
        ?.FirstOrDefault()
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
        .GetCustomAttributes(typeof(DataGridViewMultiImageColumnAttribute), true)
        .FirstOrDefault();
      if (multiImageColumnAttribute != null) {
        var displayText =
          (DisplayNameAttribute)property.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault();
        newColumn = _ConstructMultiImageColumn(property.Name, displayText?.DisplayName, multiImageColumnAttribute);
        columns.Insert(index, newColumn);
      }

      if (newColumn == null)
        continue;

      // apply column width
      _QueryPropertyAttribute<DataGridViewColumnWidthAttribute>(type, property.Name)
        ?.FirstOrDefault()
        ?.ApplyTo(newColumn);
    }
  }

  private static DataGridViewBoundComboBoxColumn _ConstructComboBoxColumn(
    DataGridViewColumn column,
    DataGridViewComboboxColumnAttribute attribute
  ) =>
    new(
      attribute.DataSourcePropertyName,
      attribute.EnabledWhenPropertyName,
      attribute.ValueMember,
      attribute.DisplayMember
    ) {
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
  private static DataGridViewProgressBarColumn _ConstructProgressBarColumn(DataGridViewProgressBarColumnAttribute progressBarColumnAttribute, DataGridViewColumn column) =>
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
      ReadOnly = column.ReadOnly,
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
  private static DataGridViewCheckBoxColumn _ConstructVisibleColumn(
    DataGridViewColumn column,
    EditorBrowsableState state
  ) =>
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
  private static DataGridViewMultiImageColumn _ConstructMultiImageColumn(
    string propertyName,
    string headerText,
    DataGridViewMultiImageColumnAttribute attribute
  ) =>
    new(
      attribute.MaximumImageSize,
      attribute.Padding,
      attribute.Margin,
      attribute.OnClickMethodName,
      attribute.ToolTipProviderMethodName
    ) { Name = propertyName, DataPropertyName = propertyName, HeaderText = headerText ?? propertyName, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet, Visible = true, };

  /// <summary>
  ///   Constructs a multi image column.
  /// </summary>
  /// <param name="column">The column which was originally created by the dataGridView</param>
  /// <param name="attribute">the MultiImageColumn attribute from the data bound property</param>
  /// <returns>a new instance of <see cref="DataGridViewMultiImageColumn" /></returns>
  private static DataGridViewMultiImageColumn _ConstructMultiImageColumn(
    DataGridViewColumn column,
    DataGridViewMultiImageColumnAttribute attribute
  ) =>
    new(
      attribute.MaximumImageSize,
      attribute.Padding,
      attribute.Margin,
      attribute.OnClickMethodName,
      attribute.ToolTipProviderMethodName
    ) {
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
  private static DataGridViewColumn _ConstructNumericUpDownColumn(
    DataGridViewColumn column,
    DataGridViewNumericUpDownColumnAttribute attribute
  ) =>
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
  private static DataGridViewComboBoxColumn _ConstructEnumComboboxColumn(
    Type enumType,
    DataGridViewColumn originalColumn
  ) {
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

    TryHandle<DataGridViewImageColumnAttribute>(DataGridViewImageColumnAttribute.OnCellFormatting);
    TryHandle<DataGridViewImageAndTextColumnAttribute>(DataGridViewImageAndTextColumnAttribute.OnCellFormatting);
    TryHandle2<SupportsConditionalImageAttribute>(SupportsConditionalImageAttribute.OnCellFormatting);
    TryHandle<DataGridViewCellDisplayTextAttribute>(DataGridViewCellDisplayTextAttribute.OnCellFormatting);
    _FixDisplayTextForEnums(column, rowData, columnPropertyName, e);
    TryHandle<DataGridViewCellTooltipAttribute>(DataGridViewCellTooltipAttribute.OnCellFormatting);
    TryHandle2<DataGridViewCellStyleAttribute>(DataGridViewCellStyleAttribute.OnCellFormatting);

    return;

    void TryHandle<TAttribute>(
      Action<TAttribute, DataGridViewRow, DataGridViewColumn, object, string, DataGridViewCellFormattingEventArgs>
        handler
    ) where TAttribute : Attribute {
      var attribute = _QueryPropertyAttribute<TAttribute>(type, columnPropertyName)?.FirstOrDefault();
      if (attribute != null)
        handler(attribute, row, column, rowData, columnPropertyName, e);
    }

    void TryHandle2<TAttribute>(
      Action<IEnumerable<TAttribute>, DataGridViewRow, DataGridViewColumn, object, string,
        DataGridViewCellFormattingEventArgs> handler
    ) where TAttribute : Attribute {
      var attributes = _QueryPropertyAttribute<TAttribute>(type, columnPropertyName);
      if (attributes != null)
        handler(attributes, row, column, rowData, columnPropertyName, e);
    }
  }

  private static void _FixDisplayTextForEnums(
    DataGridViewColumn column,
    object data,
    string columnPropertyName,
    DataGridViewCellFormattingEventArgs e
  ) {
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

    bool TryHandle2<TAttribute>(Action<IEnumerable<TAttribute>, DataGridViewRow, object, DataGridViewRowPrePaintEventArgs> handler)
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

  private static void _FixCellStyleForReadOnlyAndDisabled(
    DataGridView dgv,
    DataGridViewRow row,
    Type type,
    object value,
    bool isAlreadyStyled
  ) {
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
      : e.State.HasFlag(DataGridViewElementStates.Selected)
        ? new(e.InheritedRowStyle.SelectionForeColor)
        : new SolidBrush(e.InheritedRowStyle.ForeColor);

    using var boldFont = new Font(
      e.InheritedRowStyle.Font.FontFamily,
      rowHeaderAttribute.TextSize ?? e.InheritedRowStyle.Font.Size,
      DrawingFontStyle.Bold
    );

    var drawFormat = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

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
      e.RowBounds.Height - borderWidthBottom
    );

    using (var backBrush = new SolidBrush(
        e.State.HasFlag(DataGridViewElementStates.Selected)
          ? e.InheritedRowStyle.SelectionBackColor
          : e.InheritedRowStyle.BackColor
      ))
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
  private static void _FixDisabledButtonCellStyle(
    Type type,
    DataGridViewColumn column,
    DataGridViewCell cell,
    object value
  ) {
    var dgvButtonColumnAttribute =
      _QueryPropertyAttribute<DataGridViewButtonColumnAttribute>(type, column.DataPropertyName)?.FirstOrDefault();
    if (column is DataGridViewDisableButtonColumn)
      ((DataGridViewDisableButtonColumn.DataGridViewDisableButtonCell)cell).Enabled =
        dgvButtonColumnAttribute?.IsEnabled(value) ?? value is not null;
  }

  /// <summary>
  ///   Fixes the cell style for read-only cells in (normally) non-read-only columns.
  /// </summary>
  /// <param name="type">The type of the bound item.</param>
  /// <param name="column">The column.</param>
  /// <param name="cell">The cell.</param>
  /// <param name="value">The value.</param>
  /// <param name="alreadyStyled"><c>true</c> if the cell was already styled; otherwise, <c>false</c></param>
  private static void _FixReadOnlyCellStyle(
    Type type,
    DataGridViewColumn column,
    DataGridViewCell cell,
    object value,
    bool alreadyStyled
  ) {
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

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
  private static readonly ConditionalWeakTable<DataGridView, DataGridViewState> _DGV_STATUS_BACKUPS = new();
#else
  private static readonly Dictionary<DataGridView, DataGridViewState> _DGV_STATUS_BACKUPS = [];
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
    Against.ThisIsNull(@this);

    if (SystemInformation.TerminalServerSession)
      return;

    var dgvType = @this.GetType();
    var pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
    pi?.SetValue(@this, true, null);
  }

  /// <summary>
  ///   Finds the type of the items in a bound DataGridView.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <returns>The identified item type or <c>null</c>.</returns>
  public static Type FindItemType(this DataGridView @this) {
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);
    Against.SameInstance(@this, target);

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
  public static IEnumerable<DataGridViewColumn> FindColumns(
    this DataGridView @this,
    Func<DataGridViewColumn, bool> predicate
  ) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    return @this.Columns.Cast<DataGridViewColumn>().Where(predicate);
  }

  /// <summary>
  ///   Finds the first column that matches a certain condition.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns>The first matching column or <c>null</c>.</returns>
  public static DataGridViewColumn FindFirstColumn(this DataGridView @this, Func<DataGridViewColumn, bool> predicate) {
    Against.ThisIsNull(@this);

    var matches = FindColumns(@this, predicate);
    return matches?.FirstOrDefault();
  }

  public static bool TryGetColumn(this DataGridView @this, int columnIndex, out DataGridViewColumn column) {
    Against.ThisIsNull(@this);

    if (columnIndex < 0 || columnIndex >= @this.ColumnCount) {
      column = null;
      return false;
    }

    column = @this.Columns[columnIndex];
    return column.IsDataBound;
  }

  public static bool TryGetColumn(this DataGridView @this, string columnName, out DataGridViewColumn column) {
    Against.ThisIsNull(@this);

    if (!@this.Columns.Contains(columnName)) {
      column = null;
      return false;
    }

    column = @this.Columns[columnName];
    return column?.IsDataBound ?? false;
  }

  public static DataGridViewColumn GetColumnByName(this DataGridView @this, string columnName) {
    Against.ThisIsNull(@this);

    return !@this.Columns.Contains(columnName) ? null : @this.Columns[columnName];
  }

  /// <summary>
  ///   Gets the selected items.
  /// </summary>
  /// <param name="this">This DataGridView.</param>
  /// <returns>The currently selected items</returns>
  public static IEnumerable<object> GetSelectedItems(this DataGridView @this) {
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

    return @this.SelectedCells.Count > 0;
  }

  /// <summary>
  ///   Selects the rows containing the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This DataGridView.</param>
  /// <param name="items">The items to select.</param>
  public static void SelectItems<TItem>(this DataGridView @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

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
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

    var result = @this.DataSource;
    try {
      @this.DataError += OnDataError;
      @this.DataSource = dataSource;
    } finally {
      @this.DataError -= OnDataError;
    }

    return result;
    static void OnDataError(object _, DataGridViewDataErrorEventArgs e) => e.ThrowException = false;
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
  public static void RefreshDataSource<TItem, TKey>(
    this DataGridView @this,
    IList<TItem> source,
    Func<TItem, TKey> keyGetter,
    Action preAction = null,
    Action postAction = null
  ) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(keyGetter);

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
  public static void ChangeVisibleStateOfColumn(
    this DataGridView @this,
    bool visibilityState,
    params string[] propertyNames
  ) {
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

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
    Against.ThisIsNull(@this);

    @this.SelectionChanged += SelectionChanged;
    @this.CellBeginEdit += CellBeginEdit;
    @this.CellEndEdit += CellEndEdit;
    @this.CellValidating += This_OnCellValidating;
  }

  /// <summary>
  ///   Enables Right Click Selection on DGV
  /// </summary>
  /// <param name="this">the dgv</param>
  public static void EnableRightClickSelection(this DataGridView @this) {
    Against.ThisIsNull(@this);

    @this.CellMouseDown += CellMouseDownRightClickEvent;
  }

  /// <summary>
  ///   Disables Right Click Selection on DGV
  /// </summary>
  /// <param name="this">the dgv</param>
  public static void DisableRightClickSelection(this DataGridView @this) {
    Against.ThisIsNull(@this);

    @this.CellMouseDown -= CellMouseDownRightClickEvent;
  }

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

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
  private static readonly ConditionalWeakTable<DataGridViewCell, CellEditState> _cellEditStates = new();
#else
  private static readonly Dictionary<DataGridViewCell, CellEditState> _cellEditStates = [];
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

    _cellEditStates.Add(cell, CellEditState.FromCell(cell));
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

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
  private static readonly ConditionalWeakTable<DataGridView, DataGridViewValidationState> _dataGridViewValidationStates = new();
#else
  private static readonly Dictionary<DataGridView, DataGridViewValidationState> _dataGridViewValidationStates = [];
#endif

  private static void This_OnCellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
    if (sender is not DataGridView dgv)
      return;

    if (dgv.SelectedCells.Count <= 1 || (_dataGridViewValidationStates.TryGetValue(dgv, out var state) && state.HasStartedMultipleValueChange))
      return;

    _dataGridViewValidationStates.Add(dgv, new());

    foreach (DataGridViewCell cell in dgv.SelectedCells)
      if (cell.RowIndex != e.RowIndex && cell.ColumnIndex == e.ColumnIndex)
        cell.Value = e.FormattedValue;

    _dataGridViewValidationStates.Remove(dgv);
  }

  #region various reflection caches

  private static readonly ConcurrentDictionary<Type, object[]> _TYPE_ATTRIBUTE_CACHE = [];

  /// <summary>
  ///   Queries for certain class/struct attribute in given type and all inherited interfaces.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="baseType">The base type.</param>
  /// <returns>An enumeration of all matching attributes or <c>null</c>.</returns>
  private static IEnumerable<TAttribute> _QueryPropertyAttribute<TAttribute>(Type baseType)
    where TAttribute : Attribute {
    // find all attributes, even in inherited interfaces

    var results = _TYPE_ATTRIBUTE_CACHE.GetOrAdd(
      baseType,
      type => type
        .GetCustomAttributes(true)
        .Concat(baseType.GetInterfaces().SelectMany(_GetInheritedCustomAttributes))
        .ToArray()
    );

    return results.OfType<TAttribute>();
  }

  private static object[] _GetInheritedCustomAttributes(ICustomAttributeProvider property) =>
    property.GetCustomAttributes(true);

  private static readonly ConcurrentDictionary<string, object[]> _PROPERTY_ATTRIBUTE_CACHE = [];
  private static readonly ConcurrentDictionary<string, string> _ENUM_DISPLAYNAME_CACHE = [];

  private static string _GetEnumDisplayName(object value) {
    if (value == null)
      return null;

    var type = value.GetType();
    if (!type.IsEnum)
      return null;

    var key = $"{type.FullName}\0{value}";
    if (!_ENUM_DISPLAYNAME_CACHE.TryGetValue(key, out var result))
      result = _ENUM_DISPLAYNAME_CACHE.GetOrAdd(
        key,
        _ => {
          var displayText = (DisplayNameAttribute)
              type
                .GetField(value.ToString())
                ?
                .GetCustomAttributes(typeof(DisplayNameAttribute), false)
                .FirstOrDefault()
            ;
          return displayText?.DisplayName;
        }
      );

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

  private static IEnumerable<TAttribute> _QueryPropertyAttributeAndCache<TAttribute>(
    string key,
    Type baseType,
    string propertyName
  ) where TAttribute : Attribute {
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

      var p = type.GetProperty(
        propertyName,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
      );
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

  private static readonly ConcurrentDictionary<string, Func<object, object>> _PROPERTY_GETTER_CACHE = [];

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
  internal static TValue GetPropertyValueOrDefault<TValue>(
    object value,
    string propertyName,
    TValue defaultValueNullValue,
    TValue defaultValueNoProperty,
    TValue defaultValuePropertyNotFound,
    TValue defaultValuePropertyWrongType
  ) {
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
  internal static TValue GetPropertyValue<TValue>(
    string callerName,
    object value,
    string propertyName,
    TValue defaultValueNullValue
  ) {
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
    return result switch {
      TValue i => i,
      null when !typeof(TValue).IsValueType => default,
      _ => throw new($"{callerName}: Property {type.FullName}.{propertyName} has wrong type '{(result == null ? "null" : result.GetType().FullName)}', expected '{typeof(TValue).FullName}'")
    };
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
    var createWeaklyTypedDelegateMethod = typeof(DataGridViewExtensions).GetMethod(
      nameof(_CreateWeaklyTypedDelegate),
      BindingFlags.Static | BindingFlags.NonPublic
    );
    Debug.Assert(createWeaklyTypedDelegateMethod != null);
    var constructor = createWeaklyTypedDelegateMethod.MakeGenericMethod(method.DeclaringType, method.ReturnType);
    return (Func<object, object>)constructor.Invoke(null, [method]);
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
    if (instance is null)
      return;

    if (methodName == null || methodName.Trim().Length < 1)
      return;

    var type = instance.GetType();
    var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    method?.Invoke(instance, null);
  }

  #endregion

  internal static void SetCellTemplateOrThrow<TCellType>(this DataGridViewColumn @this, DataGridViewCell value, Action<DataGridViewCell> setter) where TCellType : DataGridViewCell {
    if (value != null && value is not TCellType)
      throw new InvalidCastException($"Value provided for CellTemplate must be of type {typeof(TCellType).Name} or derive from it.");

    setter(value);
  }

  internal static TCellType GetCellTemplateOrThrow<TCellType>(this DataGridViewColumn @this) where TCellType:DataGridViewCell 
    => @this.CellTemplate as TCellType 
       ?? throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.")
    ;

  internal static void UpdateCells<TCellType>(this DataGridView @this, int columnIndex, Action<TCellType, int> updateAction) where TCellType:DataGridViewCell{
    if (@this == null)
      return;

    var dataGridViewRows = @this.Rows;
    var rowCount = dataGridViewRows.Count;
    for (var rowIndex = 0; rowIndex < rowCount; ++rowIndex) {
      var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
      if (dataGridViewRow.Cells[columnIndex] is TCellType cell)
        updateAction(cell, rowIndex);
    }

    @this.InvalidateColumn(columnIndex);
    // TODO: This column and/or grid rows may need to be autosized depending on their
    //       autosize settings. Call the autosizing methods to autosize the column, rows, 
    //       column headers / row headers as needed.
  }

}
