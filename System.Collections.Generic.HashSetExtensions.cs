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



namespace System.Collections.Generic {
  internal static partial class HashSetExtensions {

    /// <summary>
    /// Determines whether the specified HashSet does not contain the given item.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This HashSet.</param>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c> if the item is not in the set; otherwise, <c>false</c>.</returns>
    public static bool ContainsNot<TItem>(this HashSet<TItem> @this, TItem item) {
      return (!@this.Contains(item));
    }

  }
}