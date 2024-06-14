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
using System.Linq;

namespace System.Windows.Forms;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataGridViewRowSelectableAttribute(string conditionProperty = null) : Attribute {
  public string ConditionPropertyName { get; } = conditionProperty;

  public bool IsSelectable(object value)
    => DataGridViewExtensions.GetPropertyValueOrDefault(value, this.ConditionPropertyName, true, true, false, false);

  public static void OnSelectionChanged(IEnumerable<DataGridViewRowSelectableAttribute> @this, DataGridViewRow row, object data, EventArgs e) {
    if (@this.Any(attribute => !attribute.IsSelectable(data)))
      row.Selected = false;
  }
}
