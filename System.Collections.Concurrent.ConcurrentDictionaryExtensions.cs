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

namespace System.Collections.Concurrent {
  internal static partial class ConcurrentDictionaryExtensions {
    /// <summary>
    /// Adds the key to the dictionary or updates its value.
    /// </summary>
    /// <typeparam name="TKey">type of keys</typeparam>
    /// <typeparam name="TValue">type of values</typeparam>
    /// <param name="This">This <see cref="ConcurrentDictionary"/>.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> This, TKey key, TValue value) {
      Contract.Requires(This != null);
      Contract.Requires(!ReferenceEquals(key, null));
      This.AddOrUpdate(key, value, (_, __) => value);
    }
    /// <summary>
    /// Add a value and return a the generated key created by a func, !can loop endlessly
    /// </summary>
    /// <typeparam name="Tkey">type of keys</typeparam>
    /// <typeparam name="TValue">type of values</typeparam>
    /// <param name="This">This <see cref="ConcurrentDictionary"/>.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="keyFunction">The key generator.</param>
    /// <returns>The added key.</returns>
    public static Tkey Add<Tkey, TValue>(this ConcurrentDictionary<Tkey, TValue> This, TValue value, Func<Tkey> keyFunction) {
      Contract.Requires(This != null);
      Tkey result;
      do {
        result = keyFunction();
      } while (!This.TryAdd(result, value));
      return (result);
    }

    /// <summary>
    /// Adds a value and return a the generated key created by an enumerator, !can loop endlessly
    /// </summary>
    /// <typeparam name="TKey">type of keys</typeparam>
    /// <typeparam name="TValue">type of values</typeparam>
    /// <param name="This">This <see cref="ConcurrentDictionary"/>.</param>
    /// <param name="value">The value.</param>
    /// <param name="keyEnumerator">The enum containing possible keys.</param>
    /// <returns>The added key.</returns>
    public static TKey Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> This, TValue value, IEnumerator<TKey> keyEnumerator) {
      Contract.Requires(This != null);
      TKey result;
      do {
        keyEnumerator.MoveNext();
        result = keyEnumerator.Current;
      } while (!This.TryAdd(result, value));
      return (result);
    }
    /// <summary>
    /// Adds the specified value using an available key from the given keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="This">This <see cref="ConcurrentDictionary"/>.</param>
    /// <param name="value">The value.</param>
    /// <param name="keys">The keys.</param>
    /// <returns>The added key.</returns>
    public static TKey Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> This, TValue value, IEnumerable<TKey> keys) {
      Contract.Requires(This != null);
      return (This.Add(value, keys.GetEnumerator()));
    }

    /// <summary>
    /// Gets the a matching key for a given value
    /// </summary>
    /// <typeparam name="TKey">type of keys</typeparam>
    /// <typeparam name="TValue">type of values</typeparam>
    /// <param name="This">This <see cref="ConcurrentDictionary"/>.</param>
    /// <param name="value">The value to look for.</param>
    /// <param name="key">A key that has that value.</param>
    /// <returns><c>true</c> when the value was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetKey<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> This, TValue value, out TKey key) {
      Contract.Requires(This != null);
      var result = false;
      key = default(TKey);
      var keyValuePairs = This.ToArray();
      for (var index = keyValuePairs.Length - 1; index >= 0 && !result; index--)
        if (value.Equals(keyValuePairs[index].Value)) {
          result = true;
          key = keyValuePairs[index].Key;
        }
      return (result);
    }
    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <typeparam name="TKey">type of keys</typeparam>
    /// <typeparam name="TValue">type of values</typeparam>
    /// <param name="This">This <see cref="ConcurrentDictionary"/>.</param>
    /// <param name="key">The key to remove.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> This, TKey key) {
      Contract.Requires(This != null);
      TValue value;
      return (This.TryRemove(key, out value));
    }
  }
}