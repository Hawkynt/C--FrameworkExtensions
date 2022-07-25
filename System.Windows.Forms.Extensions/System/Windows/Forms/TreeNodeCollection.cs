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

namespace System.Windows.Forms {
  // ReSharper disable once PartialTypeWithSinglePart

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class TreeNodeCollectionExtensions {

    /// <summary>
    /// Flatteneds the hierarchy.
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
}