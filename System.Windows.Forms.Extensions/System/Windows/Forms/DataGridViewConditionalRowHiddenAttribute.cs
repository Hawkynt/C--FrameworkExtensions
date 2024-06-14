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

using System.Collections.Generic;

namespace System.Windows.Forms;

/// <summary>
///   Allows specifying the row visibility depending on the underlying object instance.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class DataGridViewConditionalRowHiddenAttribute(string isHiddenWhen) : Attribute {
  public string IsHiddenWhen { get; } = isHiddenWhen;

  public bool IsHidden(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault(row, this.IsHiddenWhen, false, false, false, false);

  public static void OnRowPrepaint(IEnumerable<DataGridViewConditionalRowHiddenAttribute> @this, DataGridViewRow row, object data, DataGridViewRowPrePaintEventArgs e) {
    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var attribute in @this)
      if (attribute.IsHidden(data)) {
        row.Visible = false;
        return;
      }

    row.Visible = true;
  }
}
