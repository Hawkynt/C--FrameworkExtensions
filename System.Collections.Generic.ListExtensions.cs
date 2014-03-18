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
  internal static partial class ListExtensions {

    /// <summary>
    /// Removes the given items from the list.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This enumerable.</param>
    /// <param name="items">The items.</param>
    public static void RemoveAll<TItem>(this List<TItem> This, IEnumerable<TItem> items) {
      Contract.Requires(This != null);
      Contract.Requires(items != null);

      var removeables = new List<TItem>(items);
      foreach (var item in removeables)
        This.Remove(item);

    }

    // return part of array
    public static T[] Splice<T>(this IList<T> arrData, int intStart, int intCount) {
      T[] arrRet = new T[intCount];
      for (int intI = intCount - 1; intI >= 0; intI--)
        arrRet[intI] = arrData[intI + intStart];
      return (arrRet);
    }
    // swap two array elements
    public static void Swap<T>(this IList<T> arrData, int intI, int intJ) {
      T objTmp = arrData[intI];
      arrData[intI] = arrData[intJ];
      arrData[intJ] = objTmp;
      objTmp = default(T);
    }
    // shuffle array
    public static void Shuffle<T>(this IList<T> arrData) {
      int intI = arrData.Count;
      Random objRandom = new Random();
      while (intI > 1) {
        intI--;
        arrData.Swap(objRandom.Next(intI + 1), intI);
      }
    }

    public static TOutput[] ConvertAll<TInput, TOutput>(this IList<TInput> arrThis, Converter<TInput, TOutput> ptrConverter) {
      return (Array.ConvertAll(arrThis.ToArray(),ptrConverter));
    }

    public static void ForEach<TInput>(this IList<TInput> arrThis, Action<TInput> ptrCall) {
      Array.ForEach(arrThis.ToArray(),ptrCall);
    }

    /// <summary>
    /// Removes items at the given position.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="This">This IList.</param>
    /// <param name="start">The start.</param>
    /// <param name="count">The count.</param>
    public static void RemoveRange<TInput>(this IList<TInput> This, int start, int count) {
      Contract.Requires(This != null);

      // special case I - return when nothing to remove
      if (count < 1)
        return;

      // special case II - only one item removed
      if (count == 1) {
        This.RemoveAt(start);
        return;
      }

      // special case, given a real List<T>
      var realList = This as List<TInput>;
      if (realList != null) {
        realList.RemoveRange(start, count);
        return;
      }

      // remove every single item, starting backwards to avoid broken indexes
      for (var i = Math.Min(This.Count - 1, start + count - 1); i >= start; --i)
        This.RemoveAt(i);
    }

    /// <summary>
    /// Adds the items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="This">This IList.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<TInput>(this IList<TInput> This, IEnumerable<TInput> items) {
      Contract.Requires(This != null);
      Contract.Requires(items != null);

      // special case, given a real List<T>
      var realList = This as List<TInput>;
      if (realList != null) {
        realList.AddRange(items);
        return;
      }

      foreach (var item in items)
        This.Add(item);
    }

    /// <summary>
    /// Keeps the first n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="This">This IList.</param>
    /// <param name="count">The count.</param>
    public static void KeepFirst<TInput>(this IList<TInput> This, int count) {
      Contract.Requires(This != null);

      // special case: keep nothing
      if (count < 1) {
        This.Clear();
        return;
      }

      // special case: keep the first element
      if (count == 1) {
        var item = This[0];
        This.Clear();
        This.Add(item);
        return;
      }

      // special case: keep all elements
      var len = This.Count;
      if (count >= len)
        return;

      var index = count;
      count = len - index;

      if (count > index) {

        // more to remove than to keep, copy the items and clear the list, then re-add
        var copy = new TInput[index];
        for (var i = 0; i < index; ++i)
          copy[i] = This[i];
        This.Clear();
        This.AddRange(copy);
      } else

        // only few elements to remove
        This.RemoveRange(index, count);
    }

    /// <summary>
    /// Keeps the last n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="This">This IList.</param>
    /// <param name="count">The count.</param>
    public static void KeepLast<TInput>(this IList<TInput> This, int count) {
      Contract.Requires(This != null);

      // special case: remove all items
      if (count < 1) {
        This.Clear();
        return;
      }

      // special case: keep all elements
      var len = This.Count;
      if (count > len)
        return;

      // special case: keep last item
      if (count == 1) {
        var item = This[len - 1];
        This.Clear();
        This.Add(item);
        return;
      }

      var index = len - count;
      if (count > index) {

        // more to remove than to keep
        var copy = new TInput[count];
        for (int i = count - 1, j = len - 1; i >= 0; --i, --j)
          copy[i] = This[j];
        This.Clear();
        This.AddRange(copy);
      } else

        // only few elements to remove
        This.RemoveRange(0, index);
    }

    /// <summary>
    /// Removes the first n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="This">This IList.</param>
    /// <param name="count">The count.</param>
    public static void RemoveFirst<TInput>(this IList<TInput> This, int count) {
      Contract.Requires(This != null);
      var remaining = This.Count - count;
      This.KeepLast(remaining);
    }

    /// <summary>
    /// Removes the last n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="This">This IList.</param>
    /// <param name="count">The count.</param>
    public static void RemoveLast<TInput>(this IList<TInput> This, int count) {
      Contract.Requires(This != null);
      var remaining = This.Count - count;
      This.KeepFirst(remaining);
    }

  }
}
