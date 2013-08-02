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
  }
}
