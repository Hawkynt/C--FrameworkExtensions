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
///   Allows specifying a <see cref="string"/> or <see cref="Drawing.Image"/> property to be used as a button column.
/// </summary>
/// <param name="onClickMethodName">The target method name to call upon click.</param>
/// <param name="isEnabledWhenPropertyName">The boolean property which enables or disables the button.</param>
/// <example>
/// <code>
/// // Define a custom type for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public bool IsButtonEnabled { get; set; }
///
///     [DataGridViewButtonColumn(nameof(OnButtonClick), nameof(IsButtonEnabled))]
///     public string ButtonColumn { get; set; }
///
///     public void OnButtonClick()
///     {
///         // Handle button click event
///         Console.WriteLine($"Button clicked for row with Id: {Id}");
///     }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", IsButtonEnabled = true, ButtonColumn = "Click Me" },
///     new DataRow { Id = 2, Name = "Row 2", IsButtonEnabled = false, ButtonColumn = "Disabled" }
/// };
///
/// // Create a DataGridView and set its data source
/// var dataGridView = new DataGridView
/// {
///     DataSource = dataRows
/// };
///
/// // Assume the DataGridViewButtonColumnAttribute is processed and appropriate columns are added
/// </code>
/// This example demonstrates how to use the <see cref="DataGridViewButtonColumnAttribute"/> to define a button column in a <see cref="DataGridView"/>
/// using a custom `DataRow` class. The button column's click event is handled by a method specified in the attribute, and the button's enabled state is controlled by a boolean property.
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewButtonColumnAttribute(string onClickMethodName, string isEnabledWhenPropertyName = null) : Attribute {

  /// <summary>
  ///   Executes the callback with the given object instance.
  /// </summary>
  /// <param name="row">The value.</param>
  internal void OnClick(object row) {
    if (this.IsEnabled(row))
      DataGridViewExtensions.CallLateBoundMethod(row, onClickMethodName);
  }

  internal bool IsEnabled(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault(row, isEnabledWhenPropertyName, false, true, false, false)
    ;

}
