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
///   Extension class for <see cref="ToolStripComboBox" /> objects.
/// </summary>
public static partial class ToolStripComboBoxExtensions {

  /// <summary>
  /// Sets the selected item of the <see cref="ToolStripComboBox"/> and suppresses the specified event during the operation.
  /// </summary>
  /// <param name="toolStripComboBox">This <see cref="ToolStripComboBox"/> instance.</param>
  /// <param name="selectedItem">The item to select.</param>
  /// <param name="handler">The event handler to suppress during the operation.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="toolStripComboBox"/> or <paramref name="handler"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ToolStripComboBox comboBox = new ToolStripComboBox();
  /// comboBox.Items.AddRange(new object[] { "Item 1", "Item 2", "Item 3" });
  /// comboBox.SelectedIndexChanged += (sender, e) => { Console.WriteLine("Event Triggered"); };
  /// comboBox.SetSelectedItemAndSuppressEvent("Item 2", comboBox.SelectedIndexChanged);
  /// // The ToolStripComboBox now has "Item 2" selected without triggering the SelectedIndexChanged event.
  /// </code>
  /// </example>
  public static void SetSelectedItemAndSuppressEvent(
    this ToolStripComboBox toolStripComboBox,
    object selectedItem,
    EventHandler handler
  ) {
    // no handler given? just set the given item as selected
    if (handler == null) {
      toolStripComboBox.SelectedItem = selectedItem;
      return;
    }

    // prevent multiple event handler adding
    var hasHandlerBeenDetached = false;
    try {
      toolStripComboBox.SelectedIndexChanged -= handler;
      hasHandlerBeenDetached = true;

      toolStripComboBox.SelectedItem = selectedItem;
    } finally {
      if (hasHandlerBeenDetached)
        toolStripComboBox.SelectedIndexChanged += handler;
    }
  }
}
