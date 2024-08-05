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

using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms;

/// <summary>
/// Represents a <see cref="System.Windows.Forms.DataGridViewColumn"/> that hosts <see cref="DataGridViewNumericUpDownCell"/> cells.
/// </summary>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
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
/// // Create a DataGridViewNumericUpDownColumn and add it to the DataGridView
/// var numericUpDownColumn = new DataGridViewNumericUpDownColumn
/// {
///     Name = "NumericUpDownColumn",
///     HeaderText = "Value",
///     DataPropertyName = nameof(DataRow.Value),
///     DecimalPlaces = 2,
///     Increment = 1,
///     Minimum = 0,
///     Maximum = 100,
///     UseThousandsSeparator = true
/// };
/// dataGridView.Columns.Add(numericUpDownColumn);
/// </code>
/// </example>
public partial class DataGridViewNumericUpDownColumn()
  : DataGridViewColumn(new DataGridViewNumericUpDownCell()) {

  /// <summary>
  /// Represents the implicit cell that gets cloned when adding rows to the grid.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set => this.SetCellTemplateOrThrow<DataGridViewNumericUpDownCell>(value, value => base.CellTemplate = value);
  }

  /// <summary>
  /// Replicates the DecimalPlaces property of the <see cref="DataGridViewNumericUpDownCell"/> cell type.
  /// </summary>
  [Category("Appearance")]
  [DefaultValue(DataGridViewNumericUpDownCell.DEFAULT_DECIMAL_PLACES)]
  [Description("Indicates the number of decimal places to display.")]
  public int DecimalPlaces {
    get => this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().DecimalPlaces;
    set {
      this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().DecimalPlaces = value;
      this.DataGridView.UpdateCells<DataGridViewNumericUpDownCell>(this.Index, (cell, row) => cell.SetDecimalPlaces(row, value));
    }
  }

  /// <summary>
  /// Replicates the Increment property of the <see cref="DataGridViewNumericUpDownCell"/> cell type.
  /// </summary>
  [Category("Data")]
  [Description("Indicates the amount to increment or decrement on each button click.")]
  public decimal Increment {
    get => this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().Increment;
    set {
      this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().Increment = value;
      this.DataGridView.UpdateCells<DataGridViewNumericUpDownCell>(this.Index, (cell, row) => cell.SetIncrement(row, value));
    }
  }

  /// <summary>
  /// Replicates the Maximum property of the <see cref="DataGridViewNumericUpDownCell"/> cell type.
  /// </summary>
  [Category("Data")]
  [Description("Indicates the maximum value for the numeric up-down cells.")]
  [RefreshProperties(RefreshProperties.All)]
  public decimal Maximum {
    get => this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().Maximum;
    set {
      this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().Maximum = value;
      this.DataGridView.UpdateCells<DataGridViewNumericUpDownCell>(this.Index, (cell, row) => cell.SetMaximum(row, value));
    }
  }

  /// <summary>
  /// Replicates the Minimum property of the <see cref="DataGridViewNumericUpDownCell"/> cell type.
  /// </summary>
  [Category("Data")]
  [Description("Indicates the minimum value for the numeric up-down cells.")]
  [RefreshProperties(RefreshProperties.All)]
  public decimal Minimum {
    get => this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().Minimum;
    set {
      this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().Minimum = value;
      this.DataGridView.UpdateCells<DataGridViewNumericUpDownCell>(this.Index, (cell, row) => cell.SetMinimum(row, value));
    }
  }

  /// <summary>
  /// Replicates the ThousandsSeparator property of the <see cref="DataGridViewNumericUpDownCell"/> cell type.
  /// </summary>
  [Category("Data")]
  [DefaultValue(DataGridViewNumericUpDownCell.DEFAULT_THOUSANDS_SEPARATOR)]
  [Description("Indicates whether the thousands separator will be inserted between every three decimal digits.")]
  public bool UseThousandsSeparator {
    get => this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().UseThousandsSeparator;
    set {
      this.GetCellTemplateOrThrow<DataGridViewNumericUpDownCell>().UseThousandsSeparator = value;
      this.DataGridView.UpdateCells<DataGridViewNumericUpDownCell>(this.Index, (cell, row) => cell.SetThousandsSeparator(row, value));
    }
  }
 
  /// <summary>
  /// Indicates whether the DecimalPlaces property should be persisted.
  /// </summary>
  private bool ShouldSerializeDecimalPlaces()
    => !this.DecimalPlaces.Equals(DataGridViewNumericUpDownCell.DEFAULT_DECIMAL_PLACES);
  
  /// <summary>
  /// Indicates whether the Increment property should be persisted.
  /// </summary>
  private bool ShouldSerializeIncrement()
    => !this.Increment.Equals(DataGridViewNumericUpDownCell.DEFAULT_INCREMENT);

  /// <summary>
  /// Indicates whether the Maximum property should be persisted.
  /// </summary>
  private bool ShouldSerializeMaximum()
    => !this.Maximum.Equals(DataGridViewNumericUpDownCell.DEFAULT_MAXIMUM);

  /// <summary>
  /// Indicates whether the Minimum property should be persisted.
  /// </summary>
  private bool ShouldSerializeMinimum()
    => !this.Minimum.Equals(DataGridViewNumericUpDownCell.DEFAULT_MINIMUM);

  /// <summary>
  /// Indicates whether the UseThousands property should be persisted.
  /// </summary>
  private bool ShouldSerializeThousandsSeparator()
    => !this.UseThousandsSeparator.Equals(DataGridViewNumericUpDownCell.DEFAULT_THOUSANDS_SEPARATOR);
  
  /// <summary>
  ///   Returns a standard compact string representation of the column.
  /// </summary>
  public override string ToString() => $"DataGridViewNumericUpDownColumn {{ Name = {this.Name}, Index = {this.Index.ToString(CultureInfo.CurrentCulture)} }}";

}
