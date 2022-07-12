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

using System.Linq;

// ReSharper disable PartialTypeWithSinglePart

namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class CollectionExtensions {
    /// <summary>
    /// Executes an action on each item.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">The collection.</param>
    /// <param name="action">The call to execute.</param>
    public static void ForEach<TValue>(this ICollection<TValue> @this, Action<TValue> action) {
      if (@this == null) throw new NullReferenceException();

      Array.ForEach(@this.ToArray(), action);
    }

    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIn">The type of the input collection.</typeparam>
    /// <typeparam name="TOut">The type of the output collection.</typeparam>
    /// <param name="this">The collection.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns></returns>
    public static TOut[] ConvertAll<TIn, TOut>(this ICollection<TIn> @this, Converter<TIn, TOut> converter) {
      if (@this == null) throw new NullReferenceException();

      return Array.ConvertAll(@this.ToArray(), converter);
    }

    /// <summary>
    /// Adds a range of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Collection.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<TItem>(this ICollection<TItem> @this, IEnumerable<TItem> items) {
      if (@this == null) throw new NullReferenceException();
      if (items == null) throw new ArgumentNullException(nameof(items));

      // PERF: check for special list first
      var list = @this as List<TItem>;
      if (list != null) {
        list.AddRange(items);
        return;
      }

      foreach (var item in items)
        @this.Add(item);
    }

    /// <summary>
    /// Removes the range of items.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This Collection.</param>
    /// <param name="items">The items.</param>
    public static void RemoveRange<TItem>(this ICollection<TItem> @this, IEnumerable<TItem> items) {
      if (@this == null) throw new NullReferenceException();
      if (items == null) throw new ArgumentNullException(nameof(items));

      foreach (var item in items)
        @this.Remove(item);
    }
  }
}