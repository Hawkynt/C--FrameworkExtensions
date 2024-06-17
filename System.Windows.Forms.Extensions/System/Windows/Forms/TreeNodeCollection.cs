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

namespace System.Windows.Forms;

public static partial class TreeNodeCollectionExtensions {
  /// <summary>
  ///   Flatteneds the hierarchy.
  /// </summary>
  /// <param name="this">This TreeNodeCollection.</param>
  /// <returns>An enumeration of nodes in the order of flat appearance.</returns>
  public static IEnumerable<TreeNode> AllNodes(this TreeNodeCollection @this) {
    var stack = new Stack<TreeNode>();
    for (var i = @this.Count - 1; i >= 0; --i)
      stack.Push(@this[i]);

    while (stack.Count > 0) {
      var node = stack.Pop();
      yield return node;
      if (node.Nodes.Count < 1)
        continue;

      for (var i = node.Nodes.Count - 1; i >= 0; --i)
        stack.Push(node.Nodes[i]);
    }
  }
}
