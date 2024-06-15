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

namespace System.Xml;

public static partial class XmlNodeExtensions {
  /// <summary>
  ///   Appends this node to a new parent.
  /// </summary>
  /// <param name="this">The node getting the new parent.</param>
  /// <param name="newParent">The new parent for this node.</param>
  public static void ChangeParent(this XmlNode @this, XmlNode newParent) {
    var oldParent = @this.ParentNode;
    oldParent?.RemoveChild(@this);
    newParent.AppendChild(@this);
  }
}
