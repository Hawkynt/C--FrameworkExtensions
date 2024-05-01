﻿#region (c)2010-2042 Hawkynt
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

using System.Diagnostics;

namespace System.Globalization;

public static partial class CultureInfoExtensions {
  /// <summary>
  /// Gets the region info.
  /// </summary>
  /// <param name="this">This CultureInfo.</param>
  /// <returns>The RegionInfo for the given culture.</returns>
  public static RegionInfo GetRegionInfo(this CultureInfo @this) {
    Debug.Assert(@this != null);
    return new(@this.LCID);
  }

  /// <summary>
  /// Gets the ISO currency symbol.
  /// </summary>
  /// <param name="this">This CultureInfo.</param>
  /// <returns>The symbol name for the currency of the given culture, eg. EUR, GBP, USD.</returns>
  public static string GetISOCurrencySymbol(this CultureInfo @this) {
    Debug.Assert(@this != null);
    return @this.GetRegionInfo().ISOCurrencySymbol;
  }
}