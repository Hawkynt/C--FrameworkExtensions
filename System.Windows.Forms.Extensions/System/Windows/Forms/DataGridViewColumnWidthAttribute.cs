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
///   Allows setting an exact width in pixels for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewColumnWidthAttribute : Attribute {

  /// <summary>
  /// Specifies the width of a column in a <see cref="System.Windows.Forms.DataGridView"/>.
  /// </summary>
  /// <param name="characterCount">The width of the column in number of characters.</param>
  /// <example>
  /// <code>
  /// // Define a custom class for the data grid view rows
  /// public class DataRow
  /// {
  ///     public int Id { get; set; }
  ///     public string Name { get; set; }
  ///
  ///     [DataGridViewColumnWidth((char)20)] // width in characters
  ///     public string DisplayText { get; set; }
  /// }
  ///
  /// // Create an array of DataRow instances
  /// var dataRows = new[]
  /// {
  ///     new DataRow { Id = 1, Name = "Row 1", DisplayText = "Row 1 Display" },
  ///     new DataRow { Id = 2, Name = "Row 2", DisplayText = "Row 2 Display" }
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
  public DataGridViewColumnWidthAttribute(char characterCount) {
    this._characters = new('@', characterCount);
    this._widthInPixels = -1;
    this._mode = DataGridViewAutoSizeColumnMode.None;
  }

  /// <summary>
  /// Specifies the width of a column in a <see cref="System.Windows.Forms.DataGridView"/>.
  /// </summary>
  /// <param name="characters">The width of the column based on the specified string length.</param>
  /// <example>
  /// <code>
  /// // Define a custom class for the data grid view rows
  /// public class DataRow
  /// {
  ///     public int Id { get; set; }
  ///     public string Name { get; set; }
  ///
  ///     [DataGridViewColumnWidth("LongerString")] // width based on string length
  ///     public string Description { get; set; }
  /// }
  ///
  /// // Create an array of DataRow instances
  /// var dataRows = new[]
  /// {
  ///     new DataRow { Id = 1, Name = "Row 1", Description = "Row 1 Description" },
  ///     new DataRow { Id = 2, Name = "Row 2", Description = "Row 2 Description" }
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
  public DataGridViewColumnWidthAttribute(string characters) {
    this._characters = characters;
    this._widthInPixels = -1;
    this._mode = DataGridViewAutoSizeColumnMode.None;
  }

  /// <summary>
  /// Specifies the width of a column in a <see cref="System.Windows.Forms.DataGridView"/>.
  /// </summary>
  /// <param name="widthInPixelsInPixels">The width of the column in pixels.</param>
  /// <example>
  /// <code>
  /// // Define a custom class for the data grid view rows
  /// public class DataRow
  /// {
  ///     public int Id { get; set; }
  ///     public string Name { get; set; }
  ///
  ///     [DataGridViewColumnWidth(100)] // width in pixels
  ///     public string Details { get; set; }
  /// }
  ///
  /// // Create an array of DataRow instances
  /// var dataRows = new[]
  /// {
  ///     new DataRow { Id = 1, Name = "Row 1", Details = "Row 1 Details" },
  ///     new DataRow { Id = 2, Name = "Row 2", Details = "Row 2 Details" }
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
  public DataGridViewColumnWidthAttribute(int widthInPixelsInPixels) {
    this._characters = null;
    this._widthInPixels = widthInPixelsInPixels;
    this._mode = DataGridViewAutoSizeColumnMode.None;
  }

  /// <summary>
  /// Specifies the width of a column in a <see cref="System.Windows.Forms.DataGridView"/>.
  /// </summary>
  /// <param name="mode">The auto-sizing mode for the column.</param>
  /// <example>
  /// <code>
  /// // Define a custom class for the data grid view rows
  /// public class DataRow
  /// {
  ///     public int Id { get; set; }
  ///     public string Name { get; set; }
  ///
  ///     [DataGridViewColumnWidth(DataGridViewAutoSizeColumnMode.Fill)] // auto-sizing mode
  ///     public string Remarks { get; set; }
  /// }
  ///
  /// // Create an array of DataRow instances
  /// var dataRows = new[]
  /// {
  ///     new DataRow { Id = 1, Name = "Row 1", Remarks = "Row 1 Remarks" },
  ///     new DataRow { Id = 2, Name = "Row 2", Remarks = "Row 2 Remarks" }
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
  public DataGridViewColumnWidthAttribute(DataGridViewAutoSizeColumnMode mode) {
    this._characters = null;
    this._mode = mode;
    this._widthInPixels = -1;
  }

  private readonly DataGridViewAutoSizeColumnMode _mode;
  private readonly int _widthInPixels;
  private readonly string _characters;

  internal void ApplyTo(DataGridViewColumn column) {
    if (this._mode != DataGridViewAutoSizeColumnMode.None) {
      column.AutoSizeMode = this._mode;
      return;
    }

    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

    if (this._characters != null) {
      var font = column.DataGridView!.Font;
      var width = TextRenderer.MeasureText(this._characters, font);
      column.MinimumWidth = width.Width;
      column.Width = width.Width;
    } else if (this._widthInPixels >= 0) {
      column.MinimumWidth = this._widthInPixels;
      column.Width = this._widthInPixels;
    }
  }

}
