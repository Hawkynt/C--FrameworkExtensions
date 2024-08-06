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
/// Represents a <see cref="System.Windows.Forms.DataGridViewColumn"/> with a DateTimePicker cell for selecting dates and times.
/// </summary>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public DateTime Date { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", Date = DateTime.Now },
///     new DataRow { Id = 2, Name = "Row 2", Date = DateTime.Now.AddDays(1) }
/// };
///
/// // Create a DataGridView and set its data source
/// var dataGridView = new DataGridView
/// {
///     DataSource = dataRows
/// };
///
/// // Create a DataGridViewDateTimePickerColumn and add it to the DataGridView
/// var dateTimePickerColumn = new DataGridViewDateTimePickerColumn
/// {
///     DataPropertyName = nameof(DataRow.Date),
///     IsDataBound = true,
///     HeaderText = "Select Date"
/// };
/// dataGridView.Columns.Add(dateTimePickerColumn);
/// </code>
/// </example>
public partial class DataGridViewDateTimePickerColumn() : DataGridViewColumn(new DataGridViewDateTimePickerCell()) {
  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      if (value is not DataGridViewDateTimePickerCell)
        throw new InvalidCastException("Must be a DataGridViewDateTimePickerCell");

      base.CellTemplate = value;
    }
  }
}
