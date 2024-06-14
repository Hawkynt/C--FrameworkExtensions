#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

namespace System.Windows.Forms;

/// <summary>
///   Extension class for <see cref="ToolStripComboBox" /> objects.
/// </summary>
public static partial class ToolStripComboBoxExtensions {
  /// <summary>
  ///   Sets the selected item and suppress given event.
  /// </summary>
  /// <param name="toolStripComboBox">The tool strip ComboBox.</param>
  /// <param name="selectedItem">The selected item.</param>
  /// <param name="handler">The handler.</param>
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
