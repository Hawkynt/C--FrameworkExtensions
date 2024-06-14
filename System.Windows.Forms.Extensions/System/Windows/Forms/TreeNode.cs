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

using System.Drawing;
using Guard;

namespace System.Windows.Forms;

public static partial class TreeNodeExtensions {
  /// <summary>
  ///   Find out if a given node is a child of another one.
  /// </summary>
  /// <param name="this">This TreeNode</param>
  /// <param name="parent">The parent node to check.</param>
  /// <returns>
  ///   <c>true</c> when the node is a child of the given parent, no matter how deep the level is; otherwise,
  ///   <c>false</c>.
  /// </returns>
  public static bool IsChildOf(this TreeNode @this, TreeNode parent) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(parent);

    var start = @this;
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
  /// <param name="this">This TreeNode.</param>
  /// <returns>An id.</returns>
  public static string GetId(this TreeNode @this) {
    Against.ThisIsNull(@this);

    var parent = @this.Parent;
    if (parent == null)
      return string.Empty;

    var baseId = parent.GetId();
    var neighbours = parent.Nodes;

    var myId = 0;
    foreach (var neighbour in neighbours)
      if (neighbour == @this)
        return $"{baseId}/{myId:000000000}";
      else
        ++myId;

    return null;
  }

  /// <summary>
  ///   Gets the image for a given tree node.
  /// </summary>
  /// <param name="this">This TreeNode.</param>
  /// <returns>The image for that node or <c>null</c>.</returns>
  public static Image GetImage(this TreeNode @this) {
    Against.ThisIsNull(@this);

    var treeView = @this.TreeView;

    var imageList = treeView?.ImageList;
    if (imageList == null)
      return null;

    var images = imageList.Images;
    if (images.Count < 1)
      return null;

    if (@this.ImageIndex >= 0)
      return images[@this.ImageIndex];

    return images.ContainsKey(@this.ImageKey) ? images[@this.ImageKey] : null; //images[0]);
  }
}
