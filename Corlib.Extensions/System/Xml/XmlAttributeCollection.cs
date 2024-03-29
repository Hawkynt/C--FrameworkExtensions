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

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System.Xml {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class XmlAttributeCollectionExtensions {
    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <param name="This">The attribute collection.</param>
    /// <param name="key">The attribute name.</param>
    /// <param name="defaultValue">The default value; optional, defaults to <c>null</c>.</param>
    /// <returns>The value of that attribute or the given default value.</returns>
    /// <remarks></remarks>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string GetValueOrDefault(this XmlAttributeCollection This, string key, string defaultValue = null) => This[key] == null ? defaultValue : This[key].Value;
  }
}