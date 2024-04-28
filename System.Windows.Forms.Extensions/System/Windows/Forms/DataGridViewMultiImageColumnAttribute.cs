#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace System.Windows.Forms;

/// <summary>
///   Allows specifying a value to be used as column with multiple images
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewMultiImageColumnAttribute : Attribute {
  /// <summary>
  ///   Initializes a new instance of the <see cref="DataGridViewMultiImageColumnAttribute" /> class.
  /// </summary>
  /// <param name="onClickMethodName">
  ///   Name of a method within the data bound class, which should be called,
  ///   whenever a click on an image occurs (this method has to take one parameter of type int (index of the clicked image))
  /// </param>
  /// <param name="toolTipProviderMethodName">
  ///   Name of a method within the data bound class, which should be used,
  ///   to get the tooltip text for a specific image (this method has to take one parameter of type int (index of the image))
  /// </param>
  /// <param name="maximumImageSize">the maximum size of every image displayed (width and height)</param>
  /// <param name="padding">The padding within each image</param>
  /// <param name="margin">The margin around each image</param>
  public DataGridViewMultiImageColumnAttribute(string onClickMethodName = null, string toolTipProviderMethodName = null,
    int maximumImageSize = 24, int padding = 0, int margin = 0)
    : this(onClickMethodName, toolTipProviderMethodName, maximumImageSize, padding, padding, padding, padding, margin,
      margin, margin, margin) { }

  public DataGridViewMultiImageColumnAttribute(string onClickMethodName, string toolTipProviderMethodName,
    int maximumImageSize, int paddingLeft, int paddingTop, int paddingRight, int paddingBottom, int marginLeft,
    int marginTop, int marginRight, int marginBottom) {
    this.MaximumImageSize = maximumImageSize;
    this.OnClickMethodName = onClickMethodName;
    this.ToolTipProviderMethodName = toolTipProviderMethodName;
    this.Padding = new(paddingLeft, paddingTop, paddingRight, paddingBottom);
    this.Margin = new(marginLeft, marginTop, marginRight, marginBottom);
  }

  public int MaximumImageSize { get; }
  public string OnClickMethodName { get; }
  public string ToolTipProviderMethodName { get; }
  public Padding Padding { get; }
  public Padding Margin { get; }
}
