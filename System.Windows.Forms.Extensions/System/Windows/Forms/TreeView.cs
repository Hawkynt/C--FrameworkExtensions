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

using System.Collections.Generic;
using Guard;

namespace System.Windows.Forms;

public static partial class TreeViewExtensions {
  /// <summary>
  ///   Used to store subscriptions to the treeviews.
  /// </summary>
  private static readonly Dictionary<TreeView, DragDropInstance> _SUBSCRIBED_TREEVIEWS = new();

  /// <summary>
  /// Enables the drag-and-drop functionality of a <see cref="System.Windows.Forms.TreeView"/>.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.TreeView"/> instance.</param>
  /// <param name="folderSelector">(Optional: defaults to <see langword="null"/>) The function to determine if a given node will be treated as a folder.</param>
  /// <param name="allowRootNodeDragging">(Optional: defaults to <c>true</c>) If set to <c>true</c>, allows root node dragging.</param>
  /// <param name="onNodeMove">(Optional: defaults to <see langword="null"/>) The action to invoke when a node is moved.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TreeView treeView = new TreeView();
  ///
  /// treeView.EnabledDragAndDrop(
  ///     folderSelector: node => node.Text.StartsWith("Folder"),
  ///     allowRootNodeDragging: false,
  ///     onNodeMove: (movedNode, newParent, index) =>
  ///     {
  ///         Console.WriteLine($"Node '{movedNode.Text}' moved to '{newParent?.Text ?? "root"}' at index {index}.");
  ///     });
  /// 
  /// // Assuming you have a TreeView with nodes and want to enable drag-and-drop functionality
  /// </code>
  /// </example>
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

    static void OnDisposing(object sender, EventArgs _) {
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
