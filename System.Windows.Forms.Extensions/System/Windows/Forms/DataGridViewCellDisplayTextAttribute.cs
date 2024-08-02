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

namespace System.Windows.Forms;

/// <summary>
/// Specifies a property to be used for the display text of a <see cref="System.Windows.Forms.DataGridViewCell"/>.
/// </summary>
/// <param name="propertyName">The name of the property to use as the display text.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///
///     [DataGridViewCellDisplayText(nameof(DisplayName))]
///     public string DisplayText { get; set; }
///
///     public string DisplayName => $"{Name} (ID: {Id})";
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", DisplayText = "Display 1" },
///     new DataRow { Id = 2, Name = "Row 2", DisplayText = "Display 2" }
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
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewCellDisplayTextAttribute(string propertyName) : Attribute {

  private string _GetDisplayText(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault(row, propertyName, string.Empty, string.Empty, string.Empty, string.Empty);

  internal static void OnCellFormatting(
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
