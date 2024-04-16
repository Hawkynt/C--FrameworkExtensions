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

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
namespace System;

// ReSharper disable UnusedMember.Global
// ReSharper disable once PartialTypeWithSinglePart
public static partial class StringPolyfills {

#if !SUPPORTS_STRING_CONTAINS_COMPARISON_TYPE

  /// <summary>
  /// Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.
  /// </summary>
  /// <param name="this">This <see cref="string"/></param>
  /// <param name="value">The string to seek.</param>
  /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
  /// <returns><see langword="true" /> if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string (""); otherwise, <see langword="false" />.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool Contains(this string @this, string value, StringComparison comparisonType) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (value == null)
      throw new ArgumentNullException(nameof(value));
    
    if (value.Length <= 0)
      return true;

    if (value.Length > @this.Length)
      return false;

    return @this.IndexOf(value, comparisonType) >= 0;
  }

#endif

}
