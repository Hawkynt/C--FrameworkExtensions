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

namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class KeyValuePairExtensions {
    /// <summary>
    /// Creates a dictionary from the given key/value pairs.
    /// Note: if more than one key/value pair exists with the same key name, only the last value gets stored in the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This enumeration of key/value pairs.</param>
    /// <param name="comparer">The equality comparer.</param>
    /// <returns>
    /// A new dictionary.
    /// </returns>
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> This, IEqualityComparer<TKey> comparer = null) {
      Debug.Assert(This != null);

      // if the enumeration is a collection, than initialize the dictionary with a known number of items.
      var collection = This as ICollection;
      var result = collection != null
        ? comparer == null ? new Dictionary<TKey, TValue>(collection.Count) : new Dictionary<TKey, TValue>(collection.Count, comparer)
        : comparer == null ? new Dictionary<TKey, TValue>() : new Dictionary<TKey, TValue>(comparer)
        ;

      foreach (var keyValuePair in This) {
        var key = keyValuePair.Key;
        var val = keyValuePair.Value;
        if (result.ContainsKey(key))
          result[key] = val;
        else
          result.Add(key, val);
      }
      return (result);
    }
  }
}
