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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Xml;

public static partial class XmlAttributeCollectionExtensions {
  /// <summary>
  ///   Gets the value or a default.
  /// </summary>
  /// <param name="this">The attribute collection.</param>
  /// <param name="key">The attribute name.</param>
  /// <param name="defaultValue">The default value; optional, defaults to <c>null</c>.</param>
  /// <returns>The value of that attribute or the given default value.</returns>
  /// <remarks></remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string GetValueOrDefault(this XmlAttributeCollection @this, string key, string defaultValue = null) => @this[key] == null ? defaultValue : @this[key].Value;
}
