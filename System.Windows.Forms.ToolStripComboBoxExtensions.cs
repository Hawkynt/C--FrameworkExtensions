#region (c)2010-2030 Hawkynt
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

namespace System.Windows.Forms {

    /// <summary>
    /// Extension class for <see cref="ToolStripComboBox"/> objects.
    /// </summary>
    internal static partial class ToolStripComboBoxExtensions
    {

        /// <summary>
        /// Sets the selected item and suppress given event.
        /// </summary>
        /// <param name="toolStripComboBox">The tool strip ComboBox.</param>
        /// <param name="selectedItem">The selected item.</param>
        /// <param name="handler">The handler.</param>
        public static void SetSelectedItemAndSuppressEvent(this ToolStripComboBox toolStripComboBox, object selectedItem, EventHandler handler) {
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
                if (hasHandlerBeenDetached) {
                    toolStripComboBox.SelectedIndexChanged += handler;
                }
            }
        }

    }
}
