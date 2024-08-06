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

namespace System.Windows.Forms;

/// <summary>
/// Specifies a condition under which a row in a <see cref="System.Windows.Forms.DataGridView"/> should be hidden.
/// </summary>
/// <param name="isHiddenWhen">The name of the property that determines when the row should be hidden.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public bool IsHidden { get; set; }
///
///     [DataGridViewConditionalRowHidden(nameof(IsHidden))]
///     public string DisplayText { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", IsHidden = true, DisplayText = "Hidden Row" },
///     new DataRow { Id = 2, Name = "Row 2", IsHidden = false, DisplayText = "Visible Row" }
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
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class DataGridViewConditionalRowHiddenAttribute(string isHiddenWhen) : Attribute {
  
  internal bool IsHidden(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault(row, isHiddenWhen, false, false, false, false);

  internal static void OnRowPrepaint(IEnumerable<DataGridViewConditionalRowHiddenAttribute> @this, DataGridViewRow row, object data, DataGridViewRowPrePaintEventArgs e) {
    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var attribute in @this)
      if (attribute.IsHidden(data)) {
        row.Visible = false;
        return;
      }

    row.Visible = true;
  }

}
