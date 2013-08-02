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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Collections {
  internal static partial class EnumerableExtensions {
    /// <summary>
    /// Counts the elements.
    /// </summary>
    /// <param name="This">The collection.</param>
    /// <returns>the number of elements</returns>
    public static int Count(this IEnumerable This) {
      Contract.Requires(This != null);
      // ReSharper disable AssignNullToNotNullAttribute
      return (Enumerable.Count(This.Cast<object>()));
      // ReSharper restore AssignNullToNotNullAttribute
    }
    /// <summary>
    /// Executes a callback for each item
    /// </summary>
    /// <typeparam name="TIN">The type of the items.</typeparam>
    /// <param name="This">The collection.</param>
    /// <param name="action">The call to execute.</param>
    public static void ForEach<TIN>(this IEnumerable This, Action<TIN> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      foreach (var item in This)
        action((TIN)item);
    }
    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIN">The type of the input.</typeparam>
    /// <typeparam name="TOUT">The type of the output.</typeparam>
    /// <param name="This">The collection to convert.</param>
    /// <param name="converter">The conversion function.</param>
    /// <returns></returns>
    public static IEnumerable ConvertAll<TIN, TOUT>(this IEnumerable This, Func<TIN, TOUT> converter) {
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
      // ReSharper disable LoopCanBeConvertedToQuery
      foreach (var item in This)
        // ReSharper restore LoopCanBeConvertedToQuery
        yield return (converter((TIN)item));
    }

    /// <summary>
    /// Converts a non-generic enumeration into an array of objects.
    /// </summary>
    /// <param name="This">This enumeration..</param>
    /// <returns>The array containing the elements.</returns>
    public static object[] ToObjectArray(this IEnumerable This) {
      return (This == null ? null : This.Cast<object>().ToArray());
    }
    #region linq
    /// <summary>
    /// Determines whether elements are present or not.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns><c>true</c> if there is at least one element; otherwise, <c>false</c>.</returns>
    public static bool Any(this IEnumerable This) {
      Contract.Requires(This != null);
      return This.GetEnumerator().MoveNext();
    }
    #endregion
  }
}
