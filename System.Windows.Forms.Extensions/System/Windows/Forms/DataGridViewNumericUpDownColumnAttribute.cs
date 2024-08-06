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
/// Specifies that a column in a <see cref="System.Windows.Forms.DataGridView"/> should display numeric up-down controls with specified settings.
/// </summary>
/// <param name="minimum">The minimum value for the numeric up-down cells.</param>
/// <param name="maximum">The maximum value for the numeric up-down cells.</param>
/// <param name="increment">(Optional: defaults to 1) The amount to increment or decrement on each button click.</param>
/// <param name="decimalPlaces">(Optional: defaults to 2) The number of decimal places to display.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///
///     [DataGridViewNumericUpDownColumn(
///         minimum: 0,
///         maximum: 100,
///         increment: 0.5,
///         decimalPlaces: 1)]
///     public decimal Value { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", Value = 10 },
///     new DataRow { Id = 2, Name = "Row 2", Value = 20 }
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
public sealed class DataGridViewNumericUpDownColumnAttribute(
  double minimum, 
  double maximum, 
  double increment = 1, 
  int decimalPlaces = 2
  )
  : Attribute {

  /// <summary>
  /// Gets the minimum value for the numeric up-down cells.
  /// </summary>
  internal decimal Minimum { get; } = (decimal)minimum;

  /// <summary>
  /// Gets the maximum value for the numeric up-down cells.
  /// </summary>
  internal decimal Maximum { get; } = (decimal)maximum;

  /// <summary>
  /// Gets the number of decimal places to display.
  /// </summary>
  internal int DecimalPlaces { get; } = decimalPlaces;

  /// <summary>
  /// Gets the amount to increment or decrement on each button click.
  /// </summary>
  internal decimal Increment { get; } = (decimal)increment;
}
