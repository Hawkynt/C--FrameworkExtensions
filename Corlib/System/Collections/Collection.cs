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

using System.Diagnostics;
using System.Linq;

namespace System.Collections {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class CollectionExtensions {
    /// <summary>
    /// Executes an action on each item.
    /// </summary>
    /// <param name="This">The collection.</param>
    /// <param name="call">The call to execute.</param>
    public static void ForEach(this ICollection This, Action<object> call) {
      Debug.Assert(This != null);
      foreach (var value in This)
        call(value);
    }
    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TOUT">The type of the output collection.</typeparam>
    /// <param name="This">The collection to convert.</param>
    /// <param name="converter">The converter.</param>
    /// <returns></returns>
    public static TOUT[] ConvertAll<TOUT>(this ICollection This, Converter<object, TOUT> converter) {
      Debug.Assert(This != null);
      Debug.Assert(converter != null);
      return (
        from object data in This
        select converter(data)
      ).ToArray();
    }

    /// <summary>
    /// Copies the collection into an array.
    /// </summary>
    /// <param name="This">This ICollection.</param>
    /// <returns>An array containing all elements.</returns>
    public static object[] ToArray(this ICollection This) {
      Debug.Assert(This != null);
      var len = This.Count;
      var result = new object[len];
      This.CopyTo(result, 0);
      return (result);
    }

  }
}