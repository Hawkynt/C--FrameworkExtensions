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

using System.Linq;

namespace System.Xml.Linq;

public static partial class XDocumentExtensions {
  /// <summary>
  ///   Gets the given attribute's value or a default value.
  /// </summary>
  /// <param name="this">This <see cref="XElement">XElement</see></param>
  /// <param name="attributeName">The local name of the attribute to get</param>
  /// <param name="comparison">The type of comparison, defaults to OrdinalIgnoreCase.</param>
  /// <returns>The attribute's value or <c>null</c></returns>
  public static string GetAttributeOrDefault(this XElement @this, string attributeName, StringComparison comparison = StringComparison.OrdinalIgnoreCase) => @this.Attributes().Where(a => string.Equals(a.Name.LocalName, attributeName, comparison)).Select(a => a.Value).FirstOrDefault();
}
