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
/// Specifies column configuration for properties displayed in a <see cref="ListView"/> in Details view.
/// </summary>
/// <param name="headerText">
/// The text to display in the column header. If <see langword="null"/>, the property name is used.
/// </param>
/// <example>
/// <code>
/// public class Product {
///   [ListViewColumn("Product Name", Width = 200)]
///   public string Name { get; set; }
///
///   [ListViewColumn("Price", Width = 80, Alignment = HorizontalAlignment.Right)]
///   public decimal Price { get; set; }
///
///   [ListViewColumn("Stock", Width = 60, Alignment = HorizontalAlignment.Center)]
///   public int Quantity { get; set; }
///
///   [ListViewColumn(Visible = false)]  // Hidden column
///   public int InternalId { get; set; }
/// }
///
/// // Usage
/// listView.EnableExtendedAttributes();
/// listView.DataSource = products;  // Columns are auto-generated from attributes
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ListViewColumnAttribute(string headerText = null) : Attribute {

  /// <summary>
  /// Gets the text to display in the column header.
  /// </summary>
  public string HeaderText { get; } = headerText;

  /// <summary>
  /// Gets or sets the width of the column in pixels.
  /// Use -1 for auto-size to content, -2 for auto-size to header.
  /// </summary>
  public int Width { get; set; } = -1;

  /// <summary>
  /// Gets or sets the display index of the column.
  /// Use -1 to use the natural order based on property declaration.
  /// </summary>
  public int DisplayIndex { get; set; } = -1;

  /// <summary>
  /// Gets or sets the horizontal alignment of the column content.
  /// </summary>
  public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;

  /// <summary>
  /// Gets or sets whether the column is visible.
  /// </summary>
  public bool Visible { get; set; } = true;

  /// <summary>
  /// Gets or sets the format string for displaying the value.
  /// </summary>
  public string Format { get; set; }
}
