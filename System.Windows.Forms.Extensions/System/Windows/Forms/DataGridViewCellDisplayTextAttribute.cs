﻿#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

namespace System.Windows.Forms;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewCellDisplayTextAttribute(string propertyName) : Attribute {
  private string PropertyName { get; } = propertyName;

  private string _GetDisplayText(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault(row, this.PropertyName, string.Empty, string.Empty, string.Empty, string.Empty);

  public static void OnCellFormatting(
    DataGridViewCellDisplayTextAttribute @this,
    DataGridViewRow row,
    DataGridViewColumn column,
    object data,
    string columnName,
    DataGridViewCellFormattingEventArgs e
  ) {
    e.Value = @this._GetDisplayText(data);
    e.FormattingApplied = true;
  }
}
