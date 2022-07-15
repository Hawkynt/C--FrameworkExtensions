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

namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class DoubleDictionary<TOuter, TInner, TValue> : Dictionary<TOuter, Dictionary<TInner, TValue>> {
    #region exceptions
    /// <summary>
    /// Throws the key not found exception.
    /// </summary>
    /// <param name="outerKey">The outer key.</param>
    /// <param name="innerKey">The inner key.</param>
    /// <param name="argumentName">Name of the argument.</param>
    private void _ThrowKeyNotFoundException(TOuter outerKey, TInner innerKey, string argumentName) {
      throw new KeyNotFoundException(string.Format("key {0} not found, argument: {1}", Tuple.Create(outerKey, innerKey).ToString(), argumentName));
    }

    /// <summary>
    /// Throws the duplicate key exception.
    /// </summary>
    /// <param name="outerKey">The outer key.</param>
    /// <param name="innerKey">The inner key.</param>
    /// <param name="argumentName">Name of the argument.</param>
    private void _ThrowDuplicateKeyException(TOuter outerKey, TInner innerKey, string argumentName) {
      throw new ArgumentException(string.Format("key {0} already exists, argument: {1}", Tuple.Create(outerKey, innerKey).ToString(), argumentName));
    }
    #endregion

    /// <summary>
    /// Gets the number of outer keys.
    /// </summary>
    public int OuterCount {
      get { return (base.Count); }
    }

    /// <summary>
    /// Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.
    /// </summary>
    /// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2"></see>.</returns>
    public new int Count {
      get { return (this.Sum(kvp => kvp.Value.Count)); }
    }

    /// <summary>
    /// Determines whether the specified outer key/inner key combination exists.
    /// </summary>
    /// <param name="outerKey">The outer key.</param>
    /// <param name="innerKey">The inner key.</param>
    /// <returns>
    ///   <c>true</c> if the specified key exists; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsKey(TOuter outerKey, TInner innerKey) {
      Dictionary<TInner, TValue> inner;
      return (this.TryGetValue(outerKey, out inner) && inner != null && inner.ContainsKey(innerKey));
    }

    /// <summary>
    /// Tries the get the value.
    /// </summary>
    /// <param name="outerKey">The outer key.</param>
    /// <param name="innerKey">The inner key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(TOuter outerKey, TInner innerKey, out TValue value) {
      Dictionary<TInner, TValue> inner;
      if (!this.TryGetValue(outerKey, out inner) || inner == null) {
        value = default(TValue);
        return (false);
      }
      return (inner.TryGetValue(innerKey, out value));
    }

    /// <summary>
    /// Tries to add a value.
    /// </summary>
    /// <param name="outerKey">The outer key.</param>
    /// <param name="innerKey">The inner key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public bool TryAdd(TOuter outerKey, TInner innerKey, TValue value) {
      Dictionary<TInner, TValue> inner;
      if (!this.TryGetValue(outerKey, out inner) || inner == null) {
        inner = new Dictionary<TInner, TValue>();
        this.Add(outerKey, inner);
      }
      if (inner.ContainsKey(innerKey))
        return (false);
      inner.Add(innerKey, value);
      return (true);
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="outer">The outer.</param>
    /// <param name="inner">The inner.</param>
    /// <param name="value">The value.</param>
    public void Add(TOuter outer, TInner inner, TValue value) {
      if (!this.TryAdd(outer, inner, value))
        this._ThrowDuplicateKeyException(outer, inner, null);
    }

    /// <summary>
    /// Tries to remove the specified outer/inner key combination.
    /// </summary>
    /// <param name="outerKey">The outer key.</param>
    /// <param name="innerKey">The inner key.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public bool TryRemove(TOuter outerKey, TInner innerKey, out TValue value) {
      Dictionary<TInner, TValue> inner;
      if (!(this.TryGetValue(outerKey, out inner) && inner != null && inner.ContainsKey(innerKey))) {
        value = default(TValue);
        return (false);
      }
      value = inner[innerKey];
      inner.Remove(innerKey);
      if (inner.Count == 0)
        this.Remove(outerKey);
      return (true);
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="outerKey">The outer key.</param>
    /// <param name="innerKey">The inner key.</param>
    public void Remove(TOuter outerKey, TInner innerKey) {
      TValue dummy;
      if (!this.TryRemove(outerKey, innerKey, out dummy))
        this._ThrowKeyNotFoundException(outerKey, innerKey, this.ContainsKey(outerKey) ? "innerKey" : "outerKey");
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
        Dictionary<TInner, TValue> inner;
        if (!this.TryGetValue(outerKey, out inner))
          this._ThrowKeyNotFoundException(outerKey, innerKey, "outerKey");
        TValue result;
        if (inner == null || !inner.TryGetValue(innerKey, out result)) {
          this._ThrowKeyNotFoundException(outerKey, innerKey, "innerKey");
          result = default(TValue);
        }
        return (result);
      }
      set {
        Dictionary<TInner, TValue> inner;
        if (!this.TryGetValue(outerKey, out inner) || inner == null) {
          this._ThrowKeyNotFoundException(outerKey, innerKey, "outerKey");
          return;
        }
        try {
          inner[innerKey] = value;
        } catch (KeyNotFoundException) {
          this._ThrowKeyNotFoundException(outerKey, innerKey, "innerKey");
        }
      }
    }
  }
}