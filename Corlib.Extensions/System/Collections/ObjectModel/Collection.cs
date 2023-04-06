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

namespace System.Collections.ObjectModel; 

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  static partial class CollectionExtensions {
  /// <summary>
  /// Adds items to a collection.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Collection.</param>
  /// <param name="items">The items.</param>
  public static void AddRange<TItem>(this Collection<TItem> @this, IEnumerable<TItem> items) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(items);
      
    foreach (var item in items)
      @this.Add(item);
  }
}