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

using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Forms;

/// <summary>
/// Specifies that a row in a <see cref="System.Windows.Forms.DataGridView"/> is conditionally selectable based on a property value.
/// </summary>
/// <param name="conditionProperty">
/// The name of a boolean property that determines if the row is selectable. When this property is <see langword="true"/>, the row is selectable. Defaults to <see langword="null"/> (always selectable).
/// </param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// [DataGridViewRowSelectable(conditionProperty: nameof(CanSelect))]
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public bool CanSelect { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", CanSelect = true },
///     new DataRow { Id = 2, Name = "Row 2", CanSelect = false }
/// };
///
/// // Create a DataGridView and set its data source
/// var dataGridView = new DataGridView
/// {
///     DataSource = dataRows
/// };
///
/// // Enable extended attributes to recognize the custom attributes
/// dataGridView.EnableExtendedAttributes();
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class DataGridViewRowSelectableAttribute(string conditionProperty = null) : Attribute {
  
  private bool _IsSelectable(object value)
    => DataGridViewExtensions.GetPropertyValueOrDefault(value, conditionProperty, true, true, false, false);

  internal static void OnSelectionChanged(IEnumerable<DataGridViewRowSelectableAttribute> @this, DataGridViewRow row, object data, EventArgs e) {
    if (@this.Any(attribute => !attribute._IsSelectable(data)))
      row.Selected = false;
  }

}
