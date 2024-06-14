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

using System.Drawing;

namespace System.Windows.Forms;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewImageColumnAttribute(
  string imageListPropertyName = null,
  string onClickMethodName = null,
  string onDoubleClickMethodName = null,
  string toolTipTextPropertyName = null
)
  : DataGridViewClickableAttribute(
    onClickMethodName,
    onDoubleClickMethodName
  ) {
  public string ToolTipTextPropertyName { get; } = toolTipTextPropertyName;
  public string ImageListPropertyName { get; } = imageListPropertyName;

  private Image _GetImage(object row, object value) {
    var imageList = DataGridViewExtensions.GetPropertyValueOrDefault<ImageList>(
      row,
      this.ImageListPropertyName,
      null,
      null,
      null,
      null
    );
    if (imageList == null)
      return value as Image;

    var result = value is int index && !index.GetType().IsEnum
      ? imageList.Images[index]
      : imageList.Images[value.ToString()];

    return result;
  }

  private string _ToolTipText(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault<string>(row, this.ToolTipTextPropertyName, null, null, null, null);

  public static void OnCellFormatting(DataGridViewImageColumnAttribute @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    //should not be necessary but dgv throws format exception
    if (e.DesiredType != typeof(Image))
      return;

    e.Value = @this._GetImage(data, e.Value);
    e.FormattingApplied = true;
    row.Cells[column.Index].ToolTipText = @this._ToolTipText(data);
  }
}
