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
///   Allows specifying a value to be used as column with multiple images
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewMultiImageColumnAttribute(
  string onClickMethodName,
  string toolTipProviderMethodName,
  int maximumImageSize,
  int paddingLeft,
  int paddingTop,
  int paddingRight,
  int paddingBottom,
  int marginLeft,
  int marginTop,
  int marginRight,
  int marginBottom
)
  : Attribute {
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
  public DataGridViewMultiImageColumnAttribute(
    string onClickMethodName = null,
    string toolTipProviderMethodName = null,
    int maximumImageSize = 24,
    int padding = 0,
    int margin = 0
  )
    : this(
      onClickMethodName,
      toolTipProviderMethodName,
      maximumImageSize,
      padding,
      padding,
      padding,
      padding,
      margin,
      margin,
      margin,
      margin
    ) { }

  public int MaximumImageSize { get; } = maximumImageSize;
  public string OnClickMethodName { get; } = onClickMethodName;
  public string ToolTipProviderMethodName { get; } = toolTipProviderMethodName;
  public Padding Padding { get; } = new(paddingLeft, paddingTop, paddingRight, paddingBottom);
  public Padding Margin { get; } = new(marginLeft, marginTop, marginRight, marginBottom);
}
