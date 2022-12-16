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

using System.Linq;
using System.Diagnostics;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class DictionaryExtensions {

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
      Equal = 0,
      Changed = 1,
      Added = 2,
      Removed = 3,
    }

    public interface IChangeSet<out TKey, out TValue> {
      ChangeType Type { get; }
      TKey Key { get; }
      TValue Current { get; }
      TValue Other { get; }
    }

    /// <summary>
    /// Changeset between two dicts.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    private class ChangeSet<TKey, TValue> : IChangeSet<TKey, TValue> {
      public ChangeSet(ChangeType type, TKey key, TValue currentValue, TValue otherValue) {
        this.Key = key;
        this.Current = currentValue;
        this.Other = otherValue;
        this.Type = type;
      }
      public ChangeType Type { get; }
      public TKey Key { get; }
      public TValue Current { get; }
      public TValue Other { get; }
    }

    #endregion

    /// <summary>
    /// Adds the given key/value pairs.
    /// Note: the number of parameters must be divisble by two to add all keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="keyValuePairs">The key/value pairs.</param>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> @this, params object[] keyValuePairs) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif
      if (keyValuePairs == null)
        return;

      var length = keyValuePairs.LongLength;
      if ((length & 1) == 1)
        length--;

      for (var i = 0; i < length; i += 2) {
        var key = keyValuePairs[i];
        var value = keyValuePairs[i + 1];
        @this.Add((TKey)key, (TValue)value);
      }
    }

    /// <summary>
    /// Adds a range of key/value pairs to a given dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="keyValuePairs">The key/value pairs.</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> @this, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs) {
      if (@this == null)
        throw new NullReferenceException();
      if (keyValuePairs == null)
        throw new ArgumentNullException(nameof(keyValuePairs));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      foreach (var kvp in keyValuePairs)
        @this.Add(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// Determines whether the given dictionary has a key and if so, passes the key and its value to the given function.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="action">The function to execute.</param>
    /// <returns>
    ///   <c>true</c> if the key exists in the dictionary; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static bool HasKeyDo<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Action<TKey, TValue> action) {
      if (@this == null)
        throw new NullReferenceException();
      if (action == null)
        throw new ArgumentNullException(nameof(action));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      var result = @this.TryGetValue(key, out var value);
      if (result)
        action(key, value);
      return result;
    }

    /// <summary>
    /// Determines whether the given dictionary has a key and if so, passes the value to the given function.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to lookup.</param>
    /// <param name="action">The function to execute.</param>
    /// <returns>
    ///   <c>true</c> if the key exists in the dictionary; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static bool HasKeyDo<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Action<TValue> action) {
      if (@this == null)
        throw new NullReferenceException();
      if (action == null)
        throw new ArgumentNullException(nameof(action));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      var result = @this.TryGetValue(key, out var value);
      if (result)
        action(value);
      return result;
    }

    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The value from the Dictionary or a default value</returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : default(TValue);
    }

#if NET45_OR_GREATER

    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The value from the Dictionary or a default value</returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : default(TValue);
    }

    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The value from the Dictionary or a default value</returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : default(TValue);
    }

#endif


    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The value from the Dictionary or a default value
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue defaultValue) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : defaultValue;
    }

#if NET45_OR_GREATER

    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The value from the Dictionary or a default value
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue defaultValue) {
      if (@this == null)
        throw new NullReferenceException();

#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The value from the Dictionary or a default value
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key, TValue defaultValue) {
      if (@this == null)
        throw new NullReferenceException();

#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : defaultValue;
    }

#endif

    /// <summary>
    /// Gets the value or generates a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>
    /// The value from the Dictionary or a default value
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> defaultValueFactory) {
      if (@this == null)
        throw new NullReferenceException();
      if (defaultValueFactory == null)
        throw new ArgumentNullException(nameof(defaultValueFactory));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : defaultValueFactory();
    }

    /// <summary>
    /// Gets the value or generates a default.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>
    /// The value from the Dictionary or a default value
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> defaultValueFactory) {
      if (@this == null)
        throw new NullReferenceException();
      if (defaultValueFactory == null)
        throw new ArgumentNullException(nameof(defaultValueFactory));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : defaultValueFactory(key);
    }

    /// <summary>
    /// Gets the value or null.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="_">Reserved, to be filled by the compiler.</param>
    /// <returns>
    /// The value of the key or <c>null</c>
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, ClassForcingTag<TValue> _ = null) where TValue : class {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : null;
    }

    /// <summary>
    /// Gets the value or null.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="_">Reserved, to be filled by the compiler.</param>
    /// <returns>
    /// The value of the key or <c>null</c>
    /// </returns>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue? GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, StructForcingTag<TValue> _ = null) where TValue : struct {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.TryGetValue(key, out var result) ? result : (TValue?)null;
    }

    /// <summary>
    /// Adds a value or updates an existing.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to add/update.</param>
    /// <param name="value">The new value.</param>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (@this.ContainsKey(key))
        @this[key] = value;
      else
        @this.Add(key, value);
    }

#if NET40_OR_GREATER

    /// <summary>
    /// Adds values or updates existings.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="values">The values.</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IEnumerable<Tuple<TKey, TValue>> values) {
      if (@this == null)
        throw new NullReferenceException();
      if (values == null)
        throw new ArgumentNullException(nameof(values));

#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      foreach (var kvp in values)
        @this.AddOrUpdate(kvp.Item1, kvp.Item2);
    }

    /// <summary>
    /// Adds values or updates existings.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="values">The values.</param>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, params Tuple<TKey, TValue>[] values) {
      if (@this == null)
        throw new NullReferenceException();

#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      foreach (var kvp in values)
        @this.AddOrUpdate(kvp.Item1, kvp.Item2);
    }

#endif

    /// <summary>
    /// Adds values or updates existings.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="values">The values.</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IEnumerable<KeyValuePair<TKey, TValue>> values) {
      if (@this == null)
        throw new NullReferenceException();
      if (values == null)
        throw new ArgumentNullException(nameof(values));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      foreach (var kvp in values)
        @this.AddOrUpdate(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// Adds values or updates existings.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="values">The values.</param>
    /// <exception cref="NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, params KeyValuePair<TKey, TValue>[] values) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      foreach (var kvp in values)
        @this.AddOrUpdate(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// Gets the key's value from a dictionary or adds the key with the default value.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to lookup/add.</param>
    /// <returns>
    /// The key's value or the default value.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (!@this.TryGetValue(key, out var result))
        @this.Add(key, result = default(TValue));

      return result;
    }

    /// <summary>
    /// Gets the key's value from a dictionary or adds the key with the default value.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to lookup/add.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The key's value or the default value.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue defaultValue) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (!@this.TryGetValue(key, out var result))
        @this.Add(key, result = defaultValue);

      return result;
    }


    /// <summary>
    /// Gets the key's value from a dictionary or adds the key with the default value.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to lookup/add.</param>
    /// <returns>
    /// The key's value or the default value.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static TKey GetOrAdd<TKey>(this IDictionary<TKey, TKey> @this, TKey key) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (!@this.TryGetValue(key, out var result))
        @this.Add(key, result = key);

      return result;
    }

    /// <summary>
    /// Gets the key's value from a dictionary or adds the key with the default value.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to lookup/add.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>
    /// The key's value or the default value.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> defaultValueFactory) {
      if (@this == null)
        throw new NullReferenceException();
      if (defaultValueFactory == null)
        throw new ArgumentNullException(nameof(defaultValueFactory));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (!@this.TryGetValue(key, out var result))
        @this.Add(key, result = defaultValueFactory());

      return result;
    }

    /// <summary>
    /// Gets the key's value from a dictionary or adds the key with the default value.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to lookup/add.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>
    /// The key's value or the default value.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> defaultValueFactory) {
      if (@this == null)
        throw new NullReferenceException();
      if (defaultValueFactory == null)
        throw new ArgumentNullException(nameof(defaultValueFactory));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (!@this.TryGetValue(key, out var result))
        @this.Add(key, result = defaultValueFactory(key));

      return result;
    }

    /// <summary>
    /// Adds the specified value to a dictionary by using the output of a function as the key name.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="generatorFunction">The generator function.</param>
    /// <returns>
    /// The key added.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <remarks>
    /// Can loop infinitely, depending on the function !!!
    /// </remarks>
    [DebuggerStepThrough]
    public static TKey Add<TKey, TValue>(this IDictionary<TKey, TValue> @this, TValue value, Func<TKey> generatorFunction) {
      if (@this == null)
        throw new NullReferenceException();
      if (generatorFunction == null)
        throw new ArgumentNullException(nameof(generatorFunction));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      TKey result;
      do {
        result = generatorFunction();
      } while (@this.ContainsKey(result));
      @this.Add(result, value);
      return result;
    }

    /// <summary>
    /// Adds the specified value to a dictionary by using an enumerator.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="keyEnumerator">The enumeration that returns possible key names.</param>
    /// <returns>
    /// The key added.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <remarks>
    /// Can loop infinitely, depending on the enumeration !!!
    /// </remarks>
    [DebuggerStepThrough]
    public static TKey Add<TKey, TValue>(this IDictionary<TKey, TValue> @this, TValue value, IEnumerator<TKey> keyEnumerator) {
      if (@this == null)
        throw new NullReferenceException();
      if (keyEnumerator == null)
        throw new ArgumentNullException(nameof(keyEnumerator));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      TKey result;
      do {
        keyEnumerator.MoveNext();
        result = keyEnumerator.Current;
      } while (@this.ContainsKey(result));
      @this.Add(result, value);
      return result;
    }

    /// <summary>
    /// Tries to add a key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> on success; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (@this.ContainsKey(key))
        return false;

      @this.Add(key, value);
      return true;
    }

    /// <summary>
    /// Tries to remove a key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">The value if the keys was present.</param>
    /// <returns>
    /// A value indicating whether the key was found and removed, or not.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, out TValue value) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      var result = @this.TryGetValue(key, out value);
      if (result)
        @this.Remove(key);

      return result;
    }

    /// <summary>
    /// Tries to update a given keys' value when it matches a comparison value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="comparisonValue">The comparison value.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>
    ///   <c>true</c> if update was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    [DebuggerStepThrough]
    public static bool TryUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue newValue, TValue comparisonValue, IEqualityComparer<TValue> comparer = null) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      var result = @this.TryGetValue(key, out var oldValue) && (comparer ?? EqualityComparer<TValue>.Default).Equals(oldValue, comparisonValue);
      if (result)
        @this[key] = newValue;

      return result;
    }

    /// <summary>
    /// Casts a dictionary fully.
    /// </summary>
    /// <typeparam name="TKeySource">The type of the keys in the source dictionary.</typeparam>
    /// <typeparam name="TValueSource">The type of the values in the source dictionary.</typeparam>
    /// <typeparam name="TKeyTarget">The type of the keys in the target dictionary.</typeparam>
    /// <typeparam name="TValueTarget">The type of the values in the target dictionary.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <returns>A new dictionary with the casted values.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static Dictionary<TKeyTarget, TValueTarget> FullCast<TKeySource, TValueSource, TKeyTarget, TValueTarget>(this IDictionary<TKeySource, TValueSource> @this) => @this.ToDictionary(kvp => (TKeyTarget)(object)kvp.Key, kvp => (TValueTarget)(object)kvp.Value);

    /// <summary>
    /// Checks if the given key is missing.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> when the key is missing; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static bool MissesKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) => !@this.ContainsKey(key);

    /// <summary>
    /// Compares two dictionaries against each other.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="other">The other Dictionary.</param>
    /// <param name="valueComparer">The value comparer; optional: uses default.</param>
    /// <param name="keyComparer">The key comparer; optional uses source comparer or TKey default.</param>
    /// <returns>
    /// A changeset.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static IEnumerable<IChangeSet<TKey, TValue>> CompareTo<TKey, TValue>(this Dictionary<TKey, TValue> @this, Dictionary<TKey, TValue> other, IEqualityComparer<TValue> valueComparer = null, IEqualityComparer<TKey> keyComparer = null)
      => CompareTo((IDictionary<TKey, TValue>)@this, other, valueComparer, keyComparer)
      ;

    /// <summary>
    /// Compares two dictionaries against each other.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="other">The other Dictionary.</param>
    /// <param name="valueComparer">The value comparer; optional: uses default.</param>
    /// <param name="keyComparer">The key comparer; optional uses source comparer or TKey default.</param>
    /// <returns>
    /// A changeset.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static IEnumerable<IChangeSet<TKey, TValue>> CompareTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> other, IEqualityComparer<TValue> valueComparer = null, IEqualityComparer<TKey> keyComparer = null) {
      if (@this == null)
        throw new NullReferenceException();
      if (other == null)
        throw new ArgumentNullException(nameof(other));

#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (valueComparer == null)
        valueComparer = EqualityComparer<TValue>.Default;

      var keys = @this.Keys.Concat(other.Keys).Distinct(keyComparer ?? (@this as Dictionary<TKey, TValue>)?.Comparer ?? EqualityComparer<TKey>.Default);
      foreach (var key in keys) {
        if (!other.TryGetValue(key, out var otherValue)) {
          yield return new ChangeSet<TKey, TValue>(ChangeType.Added, key, @this[key], default(TValue));
          continue;
        }

        if (!@this.TryGetValue(key, out var thisValue)) {
          yield return new ChangeSet<TKey, TValue>(ChangeType.Removed, key, default(TValue), other[key]);
          continue;
        }

        if (ReferenceEquals(thisValue, otherValue) || valueComparer.Equals(thisValue, otherValue)) {
          yield return new ChangeSet<TKey, TValue>(ChangeType.Equal, key, thisValue, otherValue);
          continue;
        }

        yield return new ChangeSet<TKey, TValue>(ChangeType.Changed, key, thisValue, otherValue);
      }
    }

#if NET45_OR_GREATER
    /// <summary>
    /// Compares two dictionaries against each other.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="this">This Dictionary.</param>
    /// <param name="other">The other Dictionary.</param>
    /// <param name="valueComparer">The value comparer; optional: uses default.</param>
    /// <param name="keyComparer">The key comparer; optional uses source comparer or TKey default.</param>
    /// <returns>
    /// A changeset.
    /// </returns>
    /// <exception cref="System.NullReferenceException"></exception>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static IEnumerable<IChangeSet<TKey, TValue>> CompareTo<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, IReadOnlyDictionary<TKey, TValue> other, IEqualityComparer<TValue> valueComparer = null, IEqualityComparer<TKey> keyComparer = null) {
      if (@this == null)
        throw new NullReferenceException();
      if (other == null)
        throw new ArgumentNullException(nameof(other));

#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (valueComparer == null)
        valueComparer = EqualityComparer<TValue>.Default;

      var keys = @this.Keys.Concat(other.Keys).Distinct(keyComparer ?? (@this as Dictionary<TKey, TValue>)?.Comparer ?? EqualityComparer<TKey>.Default);
      foreach (var key in keys) {
        TValue otherValue;
        if (!other.TryGetValue(key, out otherValue)) {
          yield return new ChangeSet<TKey, TValue>(ChangeType.Added, key, @this[key], default(TValue));
          continue;
        }

        TValue thisValue;
        if (!@this.TryGetValue(key, out thisValue)) {
          yield return new ChangeSet<TKey, TValue>(ChangeType.Removed, key, default(TValue), other[key]);
          continue;
        }

        if (ReferenceEquals(thisValue, otherValue) || valueComparer.Equals(thisValue, otherValue)) {
          yield return new ChangeSet<TKey, TValue>(ChangeType.Equal, key, thisValue, otherValue);
          continue;
        }

        yield return new ChangeSet<TKey, TValue>(ChangeType.Changed, key, thisValue, otherValue);
      }
    }
#endif

  }
}
