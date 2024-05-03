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

using System.Collections.Generic;
using Guard;

namespace System.Windows.Forms;

public static partial class TreeViewExtensions {
  /// <summary>
  ///   Used to store subscriptions to the treeviews.
  /// </summary>
  private static readonly Dictionary<TreeView, DragDropInstance> _SUBSCRIBED_TREEVIEWS = new();

  /// <summary>
  ///   Enables the drag and drop functionality of a treeview.
  /// </summary>
  /// <param name="this">This TreeView.</param>
  /// <param name="folderSelector">The function to determine if a given node will be threatened as a folder.</param>
  /// <param name="allowRootNodeDragging">if set to <c>true</c> [allow root node dragging].</param>
  /// <param name="onNodeMove">The action to invoke when a node is moved.</param>
  public static void EnabledDragAndDrop(this TreeView @this, Predicate<TreeNode> folderSelector = null, bool allowRootNodeDragging = true, Action<TreeNode, TreeNode, int> onNodeMove = null) {
    Against.ThisIsNull(@this);

    lock (_SUBSCRIBED_TREEVIEWS) {
      // skip if already subscribed to
      if (_SUBSCRIBED_TREEVIEWS.ContainsKey(@this))
        return;
      
      _SUBSCRIBED_TREEVIEWS.Add(@this, new(@this, folderSelector, allowRootNodeDragging, onNodeMove));
      @this.Disposed += OnDisposing;
    }

    return;
    
    static void OnDisposing(object sender, EventArgs ea) {
      if (sender is not TreeView treeView)
        return;
      
      lock (_SUBSCRIBED_TREEVIEWS) {
        // check if we're still subscribed to the given treeview
        if (!_SUBSCRIBED_TREEVIEWS.TryGetValue(treeView, out var dragDropInstance))
          return;

        // dispose the drag drop handler
        dragDropInstance.Dispose();

        // remove from watch list
        _SUBSCRIBED_TREEVIEWS.Remove(treeView);

        // unsubscribe
        treeView.Disposed -= OnDisposing;
      }
    }
  }

}
