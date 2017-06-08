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
#if NETFX_45
using System.Runtime.CompilerServices;
#endif
using System.Linq;
// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Collections.Generic {
  internal static partial class DictionaryExtensions {
    #region nested types

    /// <summary>
    /// Used to force the compiler to chose a method-overload with a class constraint on a generic type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ClassForcingTag<T> where T : class { private ClassForcingTag() { } }
    /// <summary>
    /// Used to force the compiler to chose a method-overload with a struct constraint on a generic type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StructForcingTag<T> where T : struct { private StructForcingTag() { } }

    public enum ChangeType {
      NewKey,
      MissingKey,
      DifferentValue,
    }

    /// <summary>
    /// Changeset between two dicts.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public struct Changeset<TKey, TValue> {
      public Changeset(ChangeType type, TKey key, TValue currentValue, TValue otherValue) {
        this.Key = key;
        this.CurrentValue = currentValue;
        this.OtherValue = otherValue;
        this.Type = type;
      }
      public ChangeType Type { get; }
      public TKey Key { get; }
      public TValue CurrentValue { get; }
      public TValue OtherValue { get; }
    }
    #endregion

    /// <summary>
    /// Adds the given key/value pairs.
    /// Note: the number of parameters must be divisble by two to add all keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="keyValuePairs">The key/value pairs.</param>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> This, params object[] keyValuePairs) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      if (keyValuePairs == null)
        return;
      var length = keyValuePairs.LongLength;
      if ((length & 1) == 1)
        length--;

      for (var i = 0; i < length; i += 2) {
        var key = keyValuePairs[i];
#if NETFX_4
        Contract.Assume(i + 1 < keyValuePairs.Length);
#endif
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
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> This, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(keyValuePairs != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(action != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(action != null);
#endif
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
    /// <param name="_">Reserved, to be filled by the compiler.</param>
    /// <returns>The value for the given key or the default value for that type if the key did not exist.</returns>
    /// <remarks></remarks>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, StructForcingTag<TValue> _ = null) where TValue : struct => This.GetValueOrDefault(key, () => default(TValue));

    /// <summary>
    /// Gets the value or default.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="_">Reserved, to be filled by the compiler.</param>
    /// <returns>The value for the given key or the default value for that type if the key did not exist.</returns>
    /// <remarks></remarks>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, ClassForcingTag<TValue> _ = null) where TValue : class => This.GetValueOrDefault(key, (TValue)null);

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
#if NETFX_4
      Contract.Requires(This != null);
#endif
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      return (This.TryGetValue(key, out result) ? result : defaultValue);
      // ReSharper restore AssignNullToNotNullAttribute
    }

    /// <summary>
    /// Gets the value or null.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="_">Reserved, to be filled by the compiler.</param>
    /// <returns>The value of the key or <c>null</c></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, ClassForcingTag<TValue> _ = null) where TValue : class {
      TValue result;
      return (This.TryGetValue(key, out result) ? result : null);
    }

    /// <summary>
    /// Gets the value or null.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="_">Reserved, to be filled by the compiler.</param>
    /// <returns>The value of the key or <c>null</c></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TValue? GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> This, TKey key, StructForcingTag<TValue> _ = null) where TValue : struct {
      TValue result;
      return (This.TryGetValue(key, out result) ? result : (TValue?)null);
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(factory != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(factory != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(!ReferenceEquals(key, null));
#endif
      if (This.ContainsKey(key))
        This[key] = value;
      else
        This.Add(key, value);
    }

#if NETFX_4
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
      foreach (var kvp in values) {
        Contract.Assume(!ReferenceEquals(kvp.Item1, null));
        This.AddOrUpdate(kvp.Item1, kvp.Item2);
      }
      // ReSharper restore AssignNullToNotNullAttribute
    }
#endif

    /// <summary>
    /// Adds values or updates existings.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Dictionary.</param>
    /// <param name="values">The keys/values.</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> This, IEnumerable<KeyValuePair<TKey, TValue>> values) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(values != null);
#endif
      // ReSharper disable AssignNullToNotNullAttribute
      foreach (var kvp in values) {
#if NETFX_4
        Contract.Assume(!ReferenceEquals(kvp.Key, null));
#endif
        This.AddOrUpdate(kvp.Key, kvp.Value);
      }
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(creatorFunction != null);
#endif
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      if (This.TryGetValue(key, out result))
        return (result);

      result = creatorFunction(key);
      This.Add(key, result);
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(creatorFunction != null);
#endif
      // ReSharper disable AssignNullToNotNullAttribute
      TValue result;
      if (This.TryGetValue(key, out result))
        return (result);

      result = creatorFunction();
      This.Add(key, result);
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(generatorFunction != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(keyEnumeration != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Dictionary<TKeyTarget, TValueTarget> FullCast<TKeySource, TValueSource, TKeyTarget, TValueTarget>(this IDictionary<TKeySource, TValueSource> This) => This.ToDictionary(kvp => (TKeyTarget)(object)kvp.Key, kvp => (TValueTarget)(object)kvp.Value);

    /// <summary>
    /// Checks if the given key is missing.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> when the key is missing; otherwise, <c>false</c>.</returns>
    public static bool MissesKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) {
#if NETFX_4
      Contract.Requires(@this != null);
      Contract.Requires(!ReferenceEquals(null, key));
#endif
      return !@this.ContainsKey(key);
    }

    /// <summary>
    /// Compares to another dictionary with the same types.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="other">The other Dictionary.</param>
    /// <returns>A changeset.</returns>
    public static IEnumerable<Changeset<TKey, TValue>> CompareTo<TKey, TValue>(this Dictionary<TKey, TValue> @this, Dictionary<TKey, TValue> other) {
#if NETFX_4
      Contract.Requires(@this != null);
      Contract.Requires(other != null);
#endif

      // missing keys
      foreach (var key in @this.Keys.Where(i => !other.ContainsKey(i)))
        yield return new Changeset<TKey, TValue>(ChangeType.NewKey, key, @this[key], default(TValue));

      // new keys
      foreach (var key in other.Keys.Where(i => !@this.ContainsKey(i)))
        yield return new Changeset<TKey, TValue>(ChangeType.MissingKey, key, default(TValue), other[key]);

      // changed keys
      foreach (var key in @this.Keys.Where(other.ContainsKey))
        if (!Equals(@this[key], other[key]))
          yield return new Changeset<TKey, TValue>(ChangeType.DifferentValue, key, @this[key], other[key]);
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
      if (this._addStateIsValue) {
        var value = keyOrValue;
        var key = this._lastKeyToAdd;
        this.Add((TKey)key, (TValue)value);
        this._lastKeyToAdd = null;
      } else {
        this._lastKeyToAdd = keyOrValue;
      }
      this._addStateIsValue = !this._addStateIsValue;
    }
  }
}
