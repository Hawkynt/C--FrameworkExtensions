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

#if NETFX_4
using System.Diagnostics.Contracts;
#endif
using System.Linq;

// ReSharper disable PartialTypeWithSinglePart

namespace System.Collections.Generic {
  internal static partial class CollectionExtensions {
    /// <summary>
    /// Executes an action on each item.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="arrThis">The collection.</param>
    /// <param name="ptrCall">The call to execute.</param>
    public static void ForEach<TValue>(this ICollection<TValue> arrThis, Action<TValue> ptrCall) => Array.ForEach(arrThis.ToArray(), ptrCall);

    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIn">The type of the input collection.</typeparam>
    /// <typeparam name="TOut">The type of the output collection.</typeparam>
    /// <param name="arrThis">The collection.</param>
    /// <param name="ptrConverter">The converter function.</param>
    /// <returns></returns>
    public static TOut[] ConvertAll<TIn, TOut>(this ICollection<TIn> arrThis, Converter<TIn, TOut> ptrConverter) => Array.ConvertAll(arrThis.ToArray(), ptrConverter);

    /// <summary>
    /// Adds a range of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Collection.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<TItem>(this ICollection<TItem> This, IEnumerable<TItem> items) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif

      // PERF: check for special list first
      var list = This as List<TItem>;
      if (list != null) {
        list.AddRange(items);
        return;
      }

      foreach (var item in items)
        This.Add(item);
    }

    /// <summary>
    /// Removes the range of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="This">This Collection.</param>
    /// <param name="items">The items.</param>
    public static void RemoveRange<TItem>(this ICollection<TItem> This, IEnumerable<TItem> items) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif
      foreach (var item in items)
        This.Remove(item);
    }
  }
}