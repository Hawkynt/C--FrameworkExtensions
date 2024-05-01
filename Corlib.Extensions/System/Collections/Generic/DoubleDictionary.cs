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

namespace System.Collections.Generic; 

public class DoubleDictionary<TOuter, TInner, TValue> : Dictionary<TOuter, Dictionary<TInner, TValue>> {
  
  #region exceptions
  /// <summary>
  /// Throws the key not found exception.
  /// </summary>
  /// <param name="outerKey">The outer key.</param>
  /// <param name="innerKey">The inner key.</param>
  /// <param name="argumentName">Name of the argument.</param>
  private static void _ThrowKeyNotFoundException(TOuter outerKey, TInner innerKey, string argumentName) => throw new KeyNotFoundException($"key {Tuple.Create(outerKey, innerKey)} not found, argument: {argumentName}");

  /// <summary>
  /// Throws the duplicate key exception.
  /// </summary>
  /// <param name="outerKey">The outer key.</param>
  /// <param name="innerKey">The inner key.</param>
  /// <param name="argumentName">Name of the argument.</param>
  private static void _ThrowDuplicateKeyException(TOuter outerKey, TInner innerKey, string argumentName) => throw new ArgumentException($"key {Tuple.Create(outerKey, innerKey)} already exists, argument: {argumentName}");

  #endregion

  /// <summary>
  /// Gets the number of outer keys.
  /// </summary>
  public int OuterCount => base.Count;

  /// <summary>
  /// Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.
  /// </summary>
  /// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</returns>
  public new int Count => this.Sum(kvp => kvp.Value.Count);

  /// <summary>
  /// Determines whether the specified outer key/inner key combination exists.
  /// </summary>
  /// <param name="outerKey">The outer key.</param>
  /// <param name="innerKey">The inner key.</param>
  /// <returns>
  ///   <c>true</c> if the specified key exists; otherwise, <c>false</c>.
  /// </returns>
  public bool ContainsKey(TOuter outerKey, TInner innerKey) => this.TryGetValue(outerKey, out var inner) && inner != null && inner.ContainsKey(innerKey);

  /// <summary>
  /// Tries the get the value.
  /// </summary>
  /// <param name="outerKey">The outer key.</param>
  /// <param name="innerKey">The inner key.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public bool TryGetValue(TOuter outerKey, TInner innerKey, out TValue value) {
    if (this.TryGetValue(outerKey, out var inner) && inner != null)
      return inner.TryGetValue(innerKey, out value);

    value = default;
    return false;
  }

  /// <summary>
  /// Tries to add a value.
  /// </summary>
  /// <param name="outerKey">The outer key.</param>
  /// <param name="innerKey">The inner key.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public bool TryAdd(TOuter outerKey, TInner innerKey, TValue value) {
    if (!this.TryGetValue(outerKey, out var inner) || inner == null) {
      inner = new();
      this.Add(outerKey, inner);
    }

    if (inner.ContainsKey(innerKey))
      return false;

    inner.Add(innerKey, value);
    return true;
  }

  /// <summary>
  /// Adds the specified value.
  /// </summary>
  /// <param name="outer">The outer.</param>
  /// <param name="inner">The inner.</param>
  /// <param name="value">The value.</param>
  public void Add(TOuter outer, TInner inner, TValue value) {
    if (!this.TryAdd(outer, inner, value))
      _ThrowDuplicateKeyException(outer, inner, null);
  }

  /// <summary>
  /// Tries to remove the specified outer/inner key combination.
  /// </summary>
  /// <param name="outerKey">The outer key.</param>
  /// <param name="innerKey">The inner key.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public bool TryRemove(TOuter outerKey, TInner innerKey, out TValue value) {
    if (!(this.TryGetValue(outerKey, out var inner) && inner != null && inner.ContainsKey(innerKey))) {
      value = default;
      return false;
    }

    value = inner[innerKey];
    inner.Remove(innerKey);
    if (inner.Count == 0)
      this.Remove(outerKey);

    return true;
  }

  /// <summary>
  /// Removes the specified key.
  /// </summary>
  /// <param name="outerKey">The outer key.</param>
  /// <param name="innerKey">The inner key.</param>
  public void Remove(TOuter outerKey, TInner innerKey) {
    if (!this.TryRemove(outerKey, innerKey, out _))
      _ThrowKeyNotFoundException(outerKey, innerKey, this.ContainsKey(outerKey) ? "innerKey" : "outerKey");
  }

  /// <summary>
  /// Gets or sets the value associated with the specified key.
  /// </summary>
  /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException"></see>, and a set operation creates a new element with the specified key.</returns>
  ///   
  /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
  ///   
  /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
  public TValue this[TOuter outerKey, TInner innerKey] {
    get {
      if (!this.TryGetValue(outerKey, out var inner))
        _ThrowKeyNotFoundException(outerKey, innerKey, "outerKey");

      if (inner != null && inner.TryGetValue(innerKey, out var result))
        return result;

      _ThrowKeyNotFoundException(outerKey, innerKey, "innerKey");
      result = default;

      return result;
    }
    set {
      if (!this.TryGetValue(outerKey, out var inner) || inner == null) {
        _ThrowKeyNotFoundException(outerKey, innerKey, "outerKey");
        return;
      }

      try {
        inner[innerKey] = value;
      } catch (KeyNotFoundException) {
        _ThrowKeyNotFoundException(outerKey, innerKey, "innerKey");
      }
    }
  }
}