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

using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace System.Windows.Controls {
  // ReSharper disable once PartialTypeWithSinglePart
  // ReSharper disable once UnusedMember.Global
  internal static partial class ItemCollectionExtensions {
    /// <summary>
    /// Gets the element in this collection by it's name tag.
    /// </summary>
    /// <param name="This">This ItemCollection.</param>
    /// <param name="name">The name.</param>
    /// <returns>The FrameworkElement that matches the given name or <c>null</c>.</returns>
    public static FrameworkElement GetElementByName(this ItemCollection This, string name) {
      Contract.Requires(This != null);
      return This.OfType<FrameworkElement>().FirstOrDefault(e => e != null && e.Name == name);
    }

    /// <summary>
    /// Adds a bunch of items.
    /// </summary>
    /// <param name="This">This ItemCollection.</param>
    /// <param name="items">The items.</param>
    public static void AddRange(this ItemCollection This, IEnumerable items) {
      Contract.Requires(This != null);
      Contract.Requires(items != null);
      foreach (var item in items)
        This.Add(item);
    }
  }
}
