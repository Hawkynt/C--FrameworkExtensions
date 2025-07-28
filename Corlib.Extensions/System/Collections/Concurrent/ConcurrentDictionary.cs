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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Guard;

namespace System.Collections.Concurrent;

public static partial class ConcurrentDictionaryExtensions {
  /// <summary>
  ///   Adds the key to the dictionary or updates its value.
  /// </summary>
  /// <typeparam name="TKey">type of keys</typeparam>
  /// <typeparam name="TValue">type of values</typeparam>
  /// <param name="this">This <see cref="ConcurrentDictionary" />.</param>
  /// <param name="key">The key.</param>
  /// <param name="value">The value.</param>
  public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, [DisallowNull] TKey key, TValue value) {
    Against.ThisIsNull(@this);

    @this.AddOrUpdate(key, value, (_, __) => value);
  }

  /// <summary>
  ///   Add a value and return a the generated key created by a func, !can loop endlessly
  /// </summary>
  /// <typeparam name="Tkey">type of keys</typeparam>
  /// <typeparam name="TValue">type of values</typeparam>
  /// <param name="this">This <see cref="ConcurrentDictionary" />.</param>
  /// <param name="value">The value to add.</param>
  /// <param name="keyFunction">The key generator.</param>
  /// <returns>The added key.</returns>
  public static Tkey Add<Tkey, TValue>(this ConcurrentDictionary<Tkey, TValue> @this, TValue value, Func<Tkey> keyFunction) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(keyFunction);

    Tkey result;
    do
      result = keyFunction();
    while (!@this.TryAdd(result, value));
    return result;
  }

  /// <summary>
  ///   Adds a value and return a the generated key created by an enumerator, !can loop endlessly
  /// </summary>
  /// <typeparam name="TKey">type of keys</typeparam>
  /// <typeparam name="TValue">type of values</typeparam>
  /// <param name="this">This <see cref="ConcurrentDictionary" />.</param>
  /// <param name="value">The value.</param>
  /// <param name="keyEnumerator">The enum containing possible keys.</param>
  /// <returns>The added key.</returns>
  public static TKey Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TValue value, IEnumerator<TKey> keyEnumerator) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(keyEnumerator);

    TKey result;
    do {
      keyEnumerator.MoveNext();
      result = keyEnumerator.Current;
    } while (!@this.TryAdd(result, value));

    return result;
  }

  /// <summary>
  ///   Adds the specified value using an available key from the given keys.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="this">This <see cref="ConcurrentDictionary" />.</param>
  /// <param name="value">The value.</param>
  /// <param name="keys">The keys.</param>
  /// <returns>The added key.</returns>
  public static TKey Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TValue value, IEnumerable<TKey> keys) {
    Against.ThisIsNull(@this);

    return @this.Add(value, keys.GetEnumerator());
  }

  /// <summary>
  ///   Gets the a matching key for a given value
  /// </summary>
  /// <typeparam name="TKey">type of keys</typeparam>
  /// <typeparam name="TValue">type of values</typeparam>
  /// <param name="this">This <see cref="ConcurrentDictionary" />.</param>
  /// <param name="value">The value to look for.</param>
  /// <param name="key">A key that has that value.</param>
  /// <returns><c>true</c> when the value was found; otherwise, <c>false</c>.</returns>
  public static bool TryGetKey<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TValue value, out TKey key) {
    Against.ThisIsNull(@this);

    var result = false;
    key = default;
    var keyValuePairs = @this.ToArray();
    for (var index = keyValuePairs.Length - 1; index >= 0 && !result; --index)
      if (value.Equals(keyValuePairs[index].Value)) {
        result = true;
        key = keyValuePairs[index].Key;
      }

    return result;
  }

  /// <summary>
  ///   Removes the specified key.
  /// </summary>
  /// <typeparam name="TKey">type of keys</typeparam>
  /// <typeparam name="TValue">type of values</typeparam>
  /// <param name="this">This <see cref="ConcurrentDictionary" />.</param>
  /// <param name="key">The key to remove.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TKey key) {
    Against.ThisIsNull(@this);

    return @this.TryRemove(key, out _);
  }

  /// <summary>
  ///   Gets the or add the given key and value.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <param name="this">This ConcurrentDictionary.</param>
  /// <param name="key">The key/value.</param>
  public static TKey GetOrAdd<TKey>(this ConcurrentDictionary<TKey, TKey> @this, TKey key) {
    Against.ThisIsNull(@this);

    return @this.GetOrAdd(key, key);
  }
}
