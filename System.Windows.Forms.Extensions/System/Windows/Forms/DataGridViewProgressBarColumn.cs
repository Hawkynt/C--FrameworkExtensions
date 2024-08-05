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

namespace System.Windows.Forms;

/// <summary>
/// Represents a <see cref="System.Windows.Forms.DataGridViewTextBoxColumn"/> that hosts <see cref="DataGridViewProgressBarCell"/> cells.
/// </summary>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public double Progress { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", Progress = 0.5 },
///     new DataRow { Id = 2, Name = "Row 2", Progress = 0.8 }
/// };
///
/// // Create a DataGridView and set its data source
/// var dataGridView = new DataGridView
/// {
///     DataSource = dataRows
/// };
///
/// // Create a DataGridViewProgressBarColumn and add it to the DataGridView
/// var progressBarColumn = new DataGridViewProgressBarColumn
/// {
///     Name = "ProgressBarColumn",
///     HeaderText = "Progress",
///     DataPropertyName = nameof(DataRow.Progress),
///     Minimum = 0,
///     Maximum = 1
/// };
/// dataGridView.Columns.Add(progressBarColumn);
/// </code>
/// </example>
public partial class DataGridViewProgressBarColumn() 
  : DataGridViewColumn(new DataGridViewProgressBarCell()) {

  /// <summary>
  /// Represents the implicit cell that gets cloned when adding rows to the grid.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set => this.SetCellTemplateOrThrow<DataGridViewProgressBarCell>(value, value => base.CellTemplate = value);
  }

  /// <summary>
  /// Replicates the Maximum property of the <see cref="DataGridViewProgressBarCell"/> cell type.
  /// </summary>
  [Category("Appearance")]
  [DefaultValue(DataGridViewProgressBarCell.DEFAULT_MAXIMUM)]
  [Description("Indicates the maximum to display.")]
  public double Maximum {
    get => this.GetCellTemplateOrThrow<DataGridViewProgressBarCell>().Maximum;
    set {
      if (this.Maximum == value)
        return;

      this.GetCellTemplateOrThrow<DataGridViewProgressBarCell>().Maximum = value;
      this.DataGridView.UpdateCells<DataGridViewProgressBarCell>(this.Index, (cell, row) => cell.Maximum = value);
    }
  }

  /// <summary>
  /// Replicates the Minimum property of the <see cref="DataGridViewProgressBarCell"/> cell type.
  /// </summary>
  [Category("Appearance")]
  [DefaultValue(DataGridViewProgressBarCell.DEFAULT_MAXIMUM)]
  [Description("Indicates the minimum to display.")]
  public double Minimum {
    get => ((DataGridViewProgressBarCell)this.CellTemplate).Minimum;
    set {
      if (this.Minimum == value)
        return;

      this.GetCellTemplateOrThrow<DataGridViewProgressBarCell>().Minimum = value;
      this.DataGridView.UpdateCells<DataGridViewProgressBarCell>(this.Index, (cell, row) => cell.Minimum = value);
    }
  }
}
