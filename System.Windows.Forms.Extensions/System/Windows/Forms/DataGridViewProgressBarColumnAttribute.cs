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
/// Specifies that a column in a <see cref="System.Windows.Forms.DataGridView"/> should display progress bars with specified minimum and maximum values.
/// </summary>
/// <param name="minimum">The minimum value for the progress bar cells.</param>
/// <param name="maximum">The maximum value for the progress bar cells.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///
///     [DataGridViewProgressBarColumn(minimum: 0, maximum: 100)]
///     public double Progress { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", Progress = 50 },
///     new DataRow { Id = 2, Name = "Row 2", Progress = 80 }
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
public sealed class DataGridViewProgressBarColumnAttribute(double minimum, double maximum) : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="DataGridViewProgressBarColumnAttribute"/> class with default minimum and maximum values.
  /// </summary>
  public DataGridViewProgressBarColumnAttribute() : this(
    DataGridViewProgressBarColumn.DataGridViewProgressBarCell.DEFAULT_MINIMUM, 
    DataGridViewProgressBarColumn.DataGridViewProgressBarCell.DEFAULT_MAXIMUM
  ) { }

  /// <summary>
  /// Gets the minimum value for the progress bar cells.
  /// </summary>
  internal double Minimum { get; } = minimum;

  /// <summary>
  /// Gets the maximum value for the progress bar cells.
  /// </summary>
  internal double Maximum { get; } = maximum;

}
