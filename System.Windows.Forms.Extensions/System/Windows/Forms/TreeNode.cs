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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Drawing;

namespace System.Windows.Forms;

public static partial class TreeNodeExtensions {
  /// <summary>
  ///   Find out if a given node is a child of another one.
  /// </summary>
  /// <param name="This">This TreeNode</param>
  /// <param name="parent">The parent node to check.</param>
  /// <returns>
  ///   <c>true</c> when the node is a child of the given parent, no matter how deep the level is; otherwise,
  ///   <c>false</c>.
  /// </returns>
  public static bool IsChildOf(this TreeNode This, TreeNode parent) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(parent != null);
#endif
    var start = This;
    while (start != null) {
      if (start == parent)
        return true;
      start = start.Parent;
    }

    return false;
  }

  /// <summary>
  ///   Gets the id for that node.
  /// </summary>
  /// <param name="This">This TreeNode.</param>
  /// <returns>An id.</returns>
  public static string GetId(this TreeNode This) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    var parent = This.Parent;
    if (parent == null)
      return string.Empty;
    var baseId = parent.GetId();
    var neighbours = parent.Nodes;
#if SUPPORTS_CONTRACTS
      Contract.Assert(neighbours != null);
#endif
    var myId = 0;
    foreach (var neighbour in neighbours)
      if (neighbour == This)
        return $"{baseId}/{myId:000000000}";
      else
        myId++;
    return null;
  }

  /// <summary>
  ///   Gets the image for a given tree node.
  /// </summary>
  /// <param name="This">This TreeNode.</param>
  /// <returns>The image for that node or <c>null</c>.</returns>
  public static Image GetImage(this TreeNode This) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif

    var treeView = This.TreeView;

    var imageList = treeView?.ImageList;
    if (imageList == null)
      return null;

    var images = imageList.Images;
#if SUPPORTS_CONTRACTS
      Contract.Assert(images != null);
#endif
    if (images.Count < 1)
      return null;

    if (This.ImageIndex >= 0)
      return images[This.ImageIndex];

    if (images.ContainsKey(This.ImageKey))
      return images[This.ImageKey];

    return null; //images[0]);
  }
}
