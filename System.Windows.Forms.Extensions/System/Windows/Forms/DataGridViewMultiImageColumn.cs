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

using System.Reflection;
using System.Reflection.Emit;

namespace System.Windows.Forms;

/// <summary>
/// Represents a <see cref="System.Windows.Forms.DataGridViewTextBoxColumn"/> that can display multiple images and handle click events with tooltips.
/// </summary>
public partial class DataGridViewMultiImageColumn : DataGridViewTextBoxColumn {
  private Action<object, int> _onClickMethod;
  private Func<object, int, string> _tooltipTextProvider;

  private readonly string _onClickMethodName;
  private readonly string _toolTipTextProviderMethodName;

  /// <summary>
  /// Initializes a new instance of the <see cref="DataGridViewMultiImageColumn"/> class.
  /// </summary>
  /// <param name="imageSizeInPixels">The size of the images displayed in the column cells.</param>
  /// <param name="padding">The padding around the images within the cells.</param>
  /// <param name="margin">The margin around the cells.</param>
  /// <param name="onClickMethodName">The name of the method to call when an image is clicked.</param>
  /// <param name="toolTipTextProviderMethodName">The name of the method that provides the tooltip text for the images.</param>
  /// <example>
  /// <code>
  /// // Define a custom class for the data grid view rows
  /// public class DataRow
  /// {
  ///     public int Id { get; set; }
  ///     public string Name { get; set; }
  ///
  ///     public void OnImageClick(object sender, int imageIndex)
  ///     {
  ///         // Handle image click event
  ///         Console.WriteLine($"Image {imageIndex} clicked in row {Id}");
  ///     }
  ///
  ///     public string GetTooltipText(object sender, int imageIndex)
  ///     {
  ///         // Provide tooltip text for the images
  ///         return $"Tooltip for image {imageIndex} in row {Id}";
  ///     }
  /// }
  ///
  /// // Create an array of DataRow instances
  /// var dataRows = new[]
  /// {
  ///     new DataRow { Id = 1, Name = "Row 1" },
  ///     new DataRow { Id = 2, Name = "Row 2" }
  /// };
  ///
  /// // Create a DataGridView and set its data source
  /// var dataGridView = new DataGridView
  /// {
  ///     DataSource = dataRows
  /// };
  ///
  /// // Create a DataGridViewMultiImageColumn and add it to the DataGridView
  /// var multiImageColumn = new DataGridViewMultiImageColumn(
  ///     imageSize: 24,
  ///     padding: new Padding(2),
  ///     margin: new Padding(2),
  ///     onClickMethodName: nameof(DataRow.OnImageClick),
  ///     toolTipTextProviderMethodName: nameof(DataRow.GetTooltipText)
  /// )
  /// {
  ///     Name = "MultiImageColumn",
  ///     HeaderText = "Images",
  ///     DataPropertyName = "Name"
  /// };
  /// dataGridView.Columns.Add(multiImageColumn);
  /// </code>
  /// </example>
  public DataGridViewMultiImageColumn(
    int imageSizeInPixels,
    Padding padding,
    Padding margin,
    string onClickMethodName,
    string toolTipTextProviderMethodName
  ) {
    this._onClickMethodName = onClickMethodName;
    this._toolTipTextProviderMethodName = toolTipTextProviderMethodName;

    var cell = new DataGridViewMultiImageCell { ImageSize = imageSizeInPixels, Padding = padding, Margin = margin, };

    // ReSharper disable once VirtualMemberCallInConstructor
    this.CellTemplate = cell;
  }

  #region Overrides of DataGridViewColumn

  /// <inheritdoc />
  public override object Clone() {
    var cell = (DataGridViewMultiImageCell)this.CellTemplate;
    var result = new DataGridViewMultiImageColumn(
      cell.ImageSize,
      cell.Padding,
      cell.Margin,
      this._onClickMethodName,
      this._toolTipTextProviderMethodName
    ) { Name = this.Name, DisplayIndex = this.DisplayIndex, HeaderText = this.HeaderText, DataPropertyName = this.DataPropertyName, AutoSizeMode = this.AutoSizeMode, SortMode = this.SortMode, FillWeight = this.FillWeight };
    return result;
  }

  #endregion

  #region Overrides of DataGridViewBand

  /// <inheritdoc />
  protected override void OnDataGridViewChanged() {
    if (this.DataGridView == null)
      return;

    var itemType = this.DataGridView.FindItemType();

    var method = GetMethodInfoOrDefault(itemType, this._onClickMethodName);
    if (method != null)
      this._onClickMethod = _GenerateObjectInstanceActionDelegate<int>(method);

    method = GetMethodInfoOrDefault(itemType, this._toolTipTextProviderMethodName);
    if (method != null)
      this._tooltipTextProvider = _GenerateObjectInstanceFunctionDelegate<int>(method);
  }

  private static MethodInfo GetMethodInfoOrDefault(Type itemType, string methodName) {
    if (itemType == null)
      return null;

    return methodName == null
      ? null
      : itemType.GetMethod(
        methodName,
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
      );
  }

  private static Action<object, TParam0> _GenerateObjectInstanceActionDelegate<TParam0>(MethodInfo method) {
    var dynamicMethod = GenerateIL<TParam0>(method, typeof(void));

    return (Action<object, TParam0>)dynamicMethod.CreateDelegate(typeof(Action<object, TParam0>));
  }

  private static Func<object, TParam0, string> _GenerateObjectInstanceFunctionDelegate<TParam0>(MethodInfo method) {
    var dynamicMethod = GenerateIL<TParam0>(method, typeof(string));

    return (Func<object, TParam0, string>)dynamicMethod.CreateDelegate(typeof(Func<object, TParam0, string>));
  }

  private static DynamicMethod GenerateIL<TParam0>(MethodInfo method, Type returnType) {
    if (method == null)
      throw new ArgumentNullException(nameof(method));
    if (method.GetParameters().Length != 1)
      throw new ArgumentException("Method needs exactly one parameter", nameof(method));

    var dynamicMethod = new DynamicMethod(string.Empty, returnType, [typeof(object), typeof(TParam0)], true);
    var generator = dynamicMethod.GetILGenerator();

    if (!method.IsStatic) {
      generator.Emit(OpCodes.Ldarg_0);
      generator.Emit(OpCodes.Castclass, method.DeclaringType!);
    }

    generator.Emit(OpCodes.Ldarg_1);
    generator.EmitCall(OpCodes.Call, method, null);
    generator.Emit(OpCodes.Ret);

    return dynamicMethod;
  }

  /// <inheritdoc />
  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set => this.SetCellTemplateOrThrow<DataGridViewMultiImageCell>(value, value => base.CellTemplate = value);
  }

  #endregion

  /// <summary>
  /// Handles the event when an image item is selected.
  /// </summary>
  /// <param name="arg1">The sender of the event.</param>
  /// <param name="arg2">The index of the selected image.</param>
  protected virtual void OnImageItemSelected(object arg1, int arg2) => this._onClickMethod?.Invoke(arg1, arg2);

}
