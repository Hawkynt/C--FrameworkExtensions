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

using System.Diagnostics.Contracts;
using System.Linq;
namespace System.Collections.Generic {
  internal static partial class CollectionExtensions {
    /// <summary>
    /// Executes an action on each item.
    /// </summary>
    /// <typeparam name="TVALUE">The type of the values.</typeparam>
    /// <param name="arrThis">The collection.</param>
    /// <param name="ptrCall">The call to execute.</param>
    public static void ForEach<TVALUE>(this ICollection<TVALUE> arrThis, Action<TVALUE> ptrCall) {
      Array.ForEach(arrThis.ToArray(), ptrCall);
    }
    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIN">The type of the input collection.</typeparam>
    /// <typeparam name="TOUT">The type of the output collection.</typeparam>
    /// <param name="arrThis">The collection.</param>
    /// <param name="ptrConverter">The converter function.</param>
    /// <returns></returns>
    public static TOUT[] ConvertAll<TIN, TOUT>(this ICollection<TIN> arrThis, Converter<TIN, TOUT> ptrConverter) {
      return (Array.ConvertAll(arrThis.ToArray(), ptrConverter));
    }

    /// <summary>
    /// Adds a range of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Collection.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<TItem>(this ICollection<TItem> This, IEnumerable<TItem> items) {
      Contract.Requires(This != null);
      Contract.Requires(items != null);

      // PERF: check for special list first
      var list = This as List<TItem>;
      if (list != null) {
        list.AddRange(items);
        return;
      }

      foreach (var item in items)
        This.Add(item);
    }
  }
}