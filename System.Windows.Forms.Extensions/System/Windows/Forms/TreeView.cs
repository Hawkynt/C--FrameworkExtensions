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
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Windows.Forms;

public static partial class TreeViewExtensions {
  /// <summary>
  ///   Used to store subscriptions to the treeviews.
  /// </summary>
  private static readonly Dictionary<TreeView, DragDropInstance> _SUBSCRIBED_TREEVIEWS = new();

  /// <summary>
  ///   Enables the drag and drop functionality of a treeview.
  /// </summary>
  /// <param name="This">This TreeView.</param>
  /// <param name="folderSelector">The function to determine if a given node will be threatened as a folder.</param>
  /// <param name="allowRootNodeDragging">if set to <c>true</c> [allow root node dragging].</param>
  /// <param name="onNodeMove">The action to invoke when a node is moved.</param>
  public static void EnabledDragAndDrop(this TreeView This, Predicate<TreeNode> folderSelector = null,
    bool allowRootNodeDragging = true, Action<TreeNode, TreeNode, int> onNodeMove = null) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    lock (_SUBSCRIBED_TREEVIEWS) {
      // skip if already subscribed to
      if (_SUBSCRIBED_TREEVIEWS.ContainsKey(This))
        return;
      _SUBSCRIBED_TREEVIEWS.Add(This, new(This, folderSelector, allowRootNodeDragging, onNodeMove));
      This.Disposed += _OnDisposing;
    }
  }

  /// <summary>
  ///   Called whenever a treeview disposes.
  /// </summary>
  /// <param name="sender">The TreeNode that is going to be disposed.</param>
  /// <param name="ea">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private static void _OnDisposing(object sender, EventArgs ea) {
    var treeView = sender as TreeView;
#if SUPPORTS_CONTRACTS
      Contract.Assert(treeView != null, "Sender must be a TreeView");
#endif
    lock (_SUBSCRIBED_TREEVIEWS) {
      // check if we're still subscribed to the given treeview
      if (!_SUBSCRIBED_TREEVIEWS.TryGetValue(treeView, out var dragDropInstance))
        return;

      // dispose the drag drop handler
      dragDropInstance.Dispose();

      // remove from watch list
      _SUBSCRIBED_TREEVIEWS.Remove(treeView);

      // unsubscribe
      treeView.Disposed -= _OnDisposing;
    }
  }
}
