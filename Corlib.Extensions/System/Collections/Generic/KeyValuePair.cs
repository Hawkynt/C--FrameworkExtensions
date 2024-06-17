#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using Guard;

namespace System.Collections.Generic;

public static partial class KeyValuePairExtensions {
  /// <summary>
  ///   Creates a dictionary from the given key/value pairs.
  ///   Note: if more than one key/value pair exists with the same key name, only the last value gets stored in the
  ///   dictionary.
  /// </summary>
  /// <typeparam name="TKey">The type of the keys.</typeparam>
  /// <typeparam name="TValue">The type of the values.</typeparam>
  /// <param name="this">This enumeration of key/value pairs.</param>
  /// <param name="comparer">The equality comparer.</param>
  /// <returns>
  ///   A new dictionary.
  /// </returns>
  public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> @this, IEqualityComparer<TKey> comparer = null) {
    Against.ThisIsNull(@this);

    // if the enumeration is a collection, than initialize the dictionary with a known number of items.
    var result = @this is ICollection collection
        ? comparer == null ? new(collection.Count) : new Dictionary<TKey, TValue>(collection.Count, comparer)
        : comparer == null
          ? []
          : new Dictionary<TKey, TValue>(comparer)
      ;

    foreach (var keyValuePair in @this) {
      var key = keyValuePair.Key;
      var val = keyValuePair.Value;
      result[key] = val;
    }

    return result;
  }
}
