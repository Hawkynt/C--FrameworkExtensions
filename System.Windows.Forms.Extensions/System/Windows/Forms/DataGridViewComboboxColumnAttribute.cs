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
/// Specifies the properties for a combo box column in a <see cref="System.Windows.Forms.DataGridView"/>.
/// </summary>
/// <param name="dataSourcePropertyName">The name of the property to use as the data source for the combo box items.</param>
/// <param name="enabledWhenPropertyName">(Optional: defaults to <see langword="null"/>) The name of the property that determines when the combo box is enabled.</param>
/// <param name="valueMember">(Optional: defaults to <see langword="null"/>) The name of the property to use as the value member for the combo box items.</param>
/// <param name="displayMember">(Optional: defaults to <see langword="null"/>) The name of the property to use as the display member for the combo box items.</param>
/// <example>
/// <code>
/// // Define a custom class for the combo box items
/// public class ComboBoxItem
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
/// }
///
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public bool IsEnabled { get; set; }
///     public ComboBoxItem[] ComboBoxItems { get; set; }
///     public int SelectedValue { get; set; }
///
///     [DataGridViewComboboxColumn(nameof(ComboBoxItems), nameof(IsEnabled), nameof(ComboBoxItem.Id), nameof(ComboBoxItem.Name))]
///     public int ComboBoxColumn { get; set; }
/// }
///
/// // Create an array of ComboBoxItem instances
/// var comboBoxItems = new[]
/// {
///     new ComboBoxItem { Id = 1, Name = "Option 1" },
///     new ComboBoxItem { Id = 2, Name = "Option 2" }
/// };
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", IsEnabled = true, ComboBoxItems = comboBoxItems, SelectedValue = 1 },
///     new DataRow { Id = 2, Name = "Row 2", IsEnabled = false, ComboBoxItems = comboBoxItems, SelectedValue = 2 }
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
public sealed class DataGridViewComboboxColumnAttribute(string dataSourcePropertyName, string enabledWhenPropertyName = null, string valueMember = null, string displayMember = null)
  : Attribute {
  internal string EnabledWhenPropertyName { get; } = enabledWhenPropertyName;
  internal string ValueMember { get; } = valueMember;
  internal string DisplayMember { get; } = displayMember;
  internal string DataSourcePropertyName { get; } = dataSourcePropertyName;
}
