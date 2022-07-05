#region (c)2010-2020 Hawkynt
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

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Linq;

namespace System.Xml.Linq {
  internal static partial class XDocumentExtensions {

    /// <summary>
    /// Gets the given attribute's value or a default value.
    /// </summary>
    /// <param name="this">This <see cref="XElement">XElement</see></param>
    /// <param name="attributeName">The local name of the attribute to get</param>
    /// <param name="comparison">The type of comparison, defaults to OrdinalIgnoreCase.</param>
    /// <returns>The attribute's value or <c>null</c></returns>
    public static string GetAttributeOrDefault(this XElement @this, string attributeName, StringComparison comparison = StringComparison.OrdinalIgnoreCase) => @this.Attributes().Where(a => string.Equals(a.Name.LocalName, attributeName, comparison)).Select(a => a.Value).FirstOrDefault();

  }
}