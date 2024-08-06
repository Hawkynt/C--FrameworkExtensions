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

using System.Collections.Concurrent;

namespace System.Windows.Forms;

/// <summary>
/// Allows defining click and double-click events for a cell in a <see cref="System.Windows.Forms.DataGridView"/> for automatically generated columns.
/// </summary>
/// <param name="onClickMethodName">The name of the method to call when the cell is clicked.</param>
/// <param name="onDoubleClickMethodName">The name of the method to call when the cell is double-clicked.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///
///     [DataGridViewClickable(onClickMethodName: nameof(OnCellClick), onDoubleClickMethodName: nameof(OnCellDoubleClick))]
///     public string DisplayText { get; set; }
///
///     public void OnCellClick()
///     {
///         // Handle cell click event
///         Console.WriteLine("Cell clicked");
///     }
///
///     public void OnCellDoubleClick()
///     {
///         // Handle cell double-click event
///         Console.WriteLine("Cell double-clicked");
///     }
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
[AttributeUsage(AttributeTargets.Property)]
public class DataGridViewClickableAttribute(string onClickMethodName = null, string onDoubleClickMethodName = null)
  : Attribute {
  
  private static readonly ConcurrentDictionary<object, System.Threading.Timer> _clickTimers = [];

  private void _HandleClick(object row) {
    _clickTimers.TryRemove(row, out _);
    DataGridViewExtensions.CallLateBoundMethod(row, onClickMethodName);
  }

  internal void OnClick(object row) {
    if (onDoubleClickMethodName == null)
      DataGridViewExtensions.CallLateBoundMethod(row, onClickMethodName);

    var newTimer = new System.Threading.Timer(this._HandleClick, row, SystemInformation.DoubleClickTime, int.MaxValue);
    do
      if (_clickTimers.TryRemove(row, out var timer))
        timer.Dispose();
    while (!_clickTimers.TryAdd(row, newTimer));
  }

  internal void OnDoubleClick(object row) {
    if (_clickTimers.TryRemove(row, out var timer))
      timer.Dispose();

    DataGridViewExtensions.CallLateBoundMethod(row, onDoubleClickMethodName);
  }

}
