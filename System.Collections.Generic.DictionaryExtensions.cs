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
  internal static partial class DictionaryExtensions {
    /// <summary>
    /// Adds the given key/value pairs.
    /// Note: the number of parameters must be divisble by two to add all keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="keyValuePairs">The key/value pairs.</param>
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> This, params object[] keyValuePairs) {
      Contract.Requires(This != null);
      if (keyValuePairs == null)
        return;
      var length = keyValuePairs.LongLength;
      if ((length & 1) == 1)
        length--;
      for (var i = 0; i < length; i += 2) {
        var key = keyValuePairs[i];
        var value = keyValuePairs[i + 1];
        This.Add((TKey)key, (TValue)value);
      }
    }

    /// <summary>
    /// Adds a range of key/value pairs to a given dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="keyValuePairs">The key/value pairs.</param>
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> This, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs) {
      Contract.Requires(This != null);
      Contract.Requires(keyValuePairs != null);
      foreach (var kvp in keyValuePairs)
        This.Add(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// Determines whether the given dictionary has a key and if so, passes the key and its value to the given function.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="action">The function to execute.</param>
    /// <returns><c>true</c> if the key exists in the dictionary; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public static bool HasKeyDo<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, Action<TKey, TValue> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue value;
      var result = This.TryGetValue(key, out value);
      if (result)
        action(key, value);
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Determines whether the given dictionary has a key and if so, passes the value to the given function.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="action">The function to execute.</param>
    /// <returns><c>true</c> if the key exists in the dictionary; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public static bool HasKeyDo<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, Action<TValue> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue value;
      var result = This.TryGetValue(key, out value);
      if (result)
        action(value);
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Gets the value or default.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <returns>The value for the given key or the default value for that type if the key did not exist.</returns>
    /// <remarks></remarks>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key) {
      Contract.Requires(This != null);
      return (This.GetValueOrDefault(key, _ => default(TValue)));
    }

    /// <summary>
    /// Gets the value or a given default value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="defaultValue">The default value to return.</param>
    /// <returns>The value for the given key or the default value if the key did not exist.</returns>
    /// <remarks></remarks>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, TValue defaultValue) {
      Contract.Requires(This != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      return (This.TryGetValue(key, out result) ? result : defaultValue);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Gets the value or calculates a default value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="factory">The function that calculates the default value.</param>
    /// <returns>The value for the given key or the default value if the key did not exist.</returns>
    /// <remarks>The function gets only called when the key did not exist.</remarks>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, Func<TKey, TValue> factory) {
      Contract.Requires(This != null);
      Contract.Requires(factory != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      return (This.TryGetValue(key, out result) ? result : factory(key));
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Gets the value or calculates a default value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="factory">The function that calculates the default value.</param>
    /// <returns>The value for the given key or the default value if the key did not exist.</returns>
    /// <remarks>The function gets only called when the key did not exist.</remarks>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, Func<TValue> factory) {
      Contract.Requires(This != null);
      Contract.Requires(factory != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      return (This.TryGetValue(key, out result) ? result : factory());
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Adds a value or updates an existing.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to add/update.</param>
    /// <param name="value">The new value.</param>
    /// <remarks></remarks>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, TValue value) {
      Contract.Requires(This != null);
      Contract.Requires(!ReferenceEquals(key, null));
      if (This.ContainsKey(key))
        This[key] = value;
      else
        This.Add(key, value);
    }

    /// <summary>
    /// Adds values or updates existings.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="values">The keys/values.</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> This, IEnumerable<Tuple<TKey, TValue>> values) {
      Contract.Requires(This != null);
      Contract.Requires(values != null);
      // ReSharper disable AssignNullToNotNullAttribute
      foreach (var kvp in values)
        This.AddOrUpdate(kvp.Item1, kvp.Item2);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Adds values or updates existings.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="values">The keys/values.</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> This, IEnumerable<KeyValuePair<TKey, TValue>> values) {
      Contract.Requires(This != null);
      Contract.Requires(values != null);
      // ReSharper disable AssignNullToNotNullAttribute
      foreach (var kvp in values)
        This.AddOrUpdate(kvp.Key, kvp.Value);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Gets the key's value from a dictionary or adds the key with the value from the function.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup/add.</param>
    /// <param name="creatorFunction">The function that generates a value for that key if it doesn't exist.</param>
    /// <returns>The key's value or the value from the function.</returns>
    /// <remarks></remarks>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, Func<TKey, TValue> creatorFunction) {
      Contract.Requires(This != null);
      Contract.Requires(creatorFunction != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      if (!This.TryGetValue(key, out result)) {
        result = creatorFunction(key);
        This.Add(key, result);
      }
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Gets the key's value from a dictionary or adds the key with the value from the function.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup/add.</param>
    /// <param name="creatorFunction">The function that generates a value for that key if it doesn't exist.</param>
    /// <returns>The key's value or the value from the function.</returns>
    /// <remarks></remarks>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, Func<TValue> creatorFunction) {
      Contract.Requires(This != null);
      Contract.Requires(creatorFunction != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      if (!This.TryGetValue(key, out result)) {
        result = creatorFunction();
        This.Add(key, result);
      }
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Adds the specified value to a dictionary by using the output of a function as the key name.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="generatorFunction">The generator function.</param>
    /// <returns>The key added.</returns>
    /// <remarks>Can loop infinitely, depending on the function !!!</remarks>
    public static TKey Add<TKey, TValue>(this IDictionary<TKey, TValue> This, TValue value, Func<TKey> generatorFunction) {
      Contract.Requires(This != null);
      Contract.Requires(generatorFunction != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TKey result;
      do {
        result = generatorFunction();
      } while (This.ContainsKey(result));
      This.Add(result, value);
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Adds the specified value to a dictionary by using an enumerator.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="keyEnumeration">The enumeration that returns possible key names.</param>
    /// <returns>The key added.</returns>
    /// <remarks>Can loop infinitely, depending on the enumeration !!!</remarks>
    public static TKey Add<TKey, TValue>(this IDictionary<TKey, TValue> This, TValue value, IEnumerator<TKey> keyEnumeration) {
      Contract.Requires(This != null);
      Contract.Requires(keyEnumeration != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TKey result;
      do {
        keyEnumeration.MoveNext();
        result = keyEnumeration.Current;
      } while (This.ContainsKey(result));
      This.Add(result, value);
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Tries to add a key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, TValue value) {
      Contract.Requires(This != null);
      // ReSharper disable AssignNullToNotNullAttribute
      if (This.ContainsKey(key))
        return (false);
      This.Add(key, value);
      return (true);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Tries to remove a key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">The value if the keys was present.</param>
    /// <returns>A value indicating whether the key was found and removed, or not.</returns>
    /// <remarks></remarks>
    public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, out TValue value) {
      Contract.Requires(This != null);
      // ReSharper disable AssignNullToNotNullAttribute
      var result = This.TryGetValue(key, out value);
      if (result)
        This.Remove(key);
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Tries to update a given keys' value when it matches a comparison value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="comparisonValue">The comparison value.</param>
    /// <returns><c>true</c> if update was successful; otherwise, <c>false</c>.</returns>
    /// <remarks></remarks>
    public static bool TryUpdate<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, TValue newValue, TValue comparisonValue) {
      Contract.Requires(This != null);
      // ReSharper disable AssignNullToNotNullAttribute
      TValue oldValue;
      var result = This.TryGetValue(key, out oldValue) && (EqualityComparer<TValue>.Default.Equals(oldValue, comparisonValue));
      if (result)
        This[key] = newValue;
      return (result);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Casts a dictionary fully.
    /// </summary>
    /// <typeparam name="TKeySource">The type of the keys in the source dictionary.</typeparam>
    /// <typeparam name="TValueSource">The type of the values in the source dictionary.</typeparam>
    /// <typeparam name="TKeyTarget">The type of the keys in the target dictionary.</typeparam>
    /// <typeparam name="TValueTarget">The type of the values in the target dictionary.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <returns>A new dictionary with the casted values.</returns>
    public static Dictionary<TKeyTarget, TValueTarget> FullCast<TKeySource, TValueSource, TKeyTarget, TValueTarget>(this IDictionary<TKeySource, TValueSource> This) {
      Contract.Requires(This != null);
      return (This.ToDictionary(kvp => (TKeyTarget)(object)kvp.Key, kvp => (TValueTarget)(object)kvp.Value));
    }

    /// <summary>
    /// Adds the a bunch of key/value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="values">The values.</param>
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> This, IEnumerable<KeyValuePair<TKey, TValue>> values) {
      Contract.Requires(This != null);
      Contract.Requires(values != null);
      foreach (var kvp in values)
        This.Add(kvp.Key, kvp.Value);
    }
  }

  /// <summary>
  /// This is like a normal dictionary, except that an easier collection initializer can be used.
  /// </summary>
  /// <typeparam name="TKey">The type of the keys.</typeparam>
  /// <typeparam name="TValue">The type of the values.</typeparam>
  [Serializable]
  public class Dictionary2<TKey, TValue> : Dictionary<TKey, TValue> {

    /// <summary>
    /// The value of the last given key.
    /// </summary>
    private object _lastKeyToAdd;

    /// <summary>
    /// When this is set to <c>true</c>, the next call to Add will add the given key/value pair; 
    /// otherwise, the next call will only save the given value as a key.
    /// </summary>
    private bool _addStateIsValue;

    /// <summary>
    /// Adds the specified key/value pair on every second call (first call is key, second is value).
    /// </summary>
    /// <param name="keyOrValue">The key or value.</param>
    public void Add(object keyOrValue) {
      if (_addStateIsValue) {
        var value = keyOrValue;
        var key = this._lastKeyToAdd;
        this.Add((TKey)key, (TValue)value);
        this._lastKeyToAdd = null;
      } else {
        this._lastKeyToAdd = keyOrValue;
      }
      _addStateIsValue = !_addStateIsValue;
    }
  }
}
