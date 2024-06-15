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

using System.Diagnostics;
using System.Runtime.Serialization;
using Guard;

namespace System.Collections.Generic;

[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
public class BiDictionary<TFirst, TSecond>
  : IDictionary<TFirst, TSecond>,
#if SUPPORTS_READ_ONLY_COLLECTIONS
    IReadOnlyDictionary<TFirst, TSecond>,
#endif
    IDictionary {
  private readonly IDictionary<TFirst, TSecond> _firstToSecond = new Dictionary<TFirst, TSecond>();
  [NonSerialized] private readonly IDictionary<TSecond, TFirst> _secondToFirst = new Dictionary<TSecond, TFirst>();
  [NonSerialized] private readonly ReverseDictionary _reverseDictionary;

  public BiDictionary() => this._reverseDictionary = new(this);
  public IDictionary<TSecond, TFirst> Reverse => this._reverseDictionary;
  public int Count => this._firstToSecond.Count;

  object ICollection.SyncRoot => ((ICollection)this._firstToSecond).SyncRoot;
  bool ICollection.IsSynchronized => ((ICollection)this._firstToSecond).IsSynchronized;
  bool IDictionary.IsFixedSize => ((IDictionary)this._firstToSecond).IsFixedSize;
  public bool IsReadOnly => this._firstToSecond.IsReadOnly || this._secondToFirst.IsReadOnly;

  public TSecond this[TFirst key] {
    get => this._firstToSecond[key];
    set {
      this._firstToSecond[key] = value;
      this._secondToFirst[value] = key;
    }
  }

  object IDictionary.this[object key] {
    get => ((IDictionary)this._firstToSecond)[key];
    set {
      ((IDictionary)this._firstToSecond)[key] = value;
      ((IDictionary)this._secondToFirst)[value] = key;
    }
  }

  public ICollection<TFirst> Keys => this._firstToSecond.Keys;
  ICollection IDictionary.Keys => ((IDictionary)this._firstToSecond).Keys;

#if SUPPORTS_READ_ONLY_COLLECTIONS

  IEnumerable<TFirst> IReadOnlyDictionary<TFirst, TSecond>.Keys => ((IReadOnlyDictionary<TFirst, TSecond>)this._firstToSecond).Keys;
  IEnumerable<TSecond> IReadOnlyDictionary<TFirst, TSecond>.Values => ((IReadOnlyDictionary<TFirst, TSecond>)this._firstToSecond).Values;

#endif

  public ICollection<TSecond> Values => this._firstToSecond.Values;
  ICollection IDictionary.Values => ((IDictionary)this._firstToSecond).Values;
  public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator() => this._firstToSecond.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)this._firstToSecond).GetEnumerator();

  public void Add(TFirst key, TSecond value) {
    this._firstToSecond.Add(key, value);
    this._secondToFirst.Add(value, key);
  }

  void IDictionary.Add(object key, object value) {
    ((IDictionary)this._firstToSecond).Add(key, value);
    ((IDictionary)this._secondToFirst).Add(value, key);
  }

  public void Add(KeyValuePair<TFirst, TSecond> item) {
    this._firstToSecond.Add(item);
    this._secondToFirst.Add(item.Reverse());
  }

  public bool ContainsKey(TFirst key) => this._firstToSecond.ContainsKey(key);
  public bool ContainsValue(TSecond value) => this._secondToFirst.ContainsKey(value);
  public bool Contains(KeyValuePair<TFirst, TSecond> item) => this._firstToSecond.Contains(item);
  public bool TryGetValue(TFirst key, out TSecond value) => this._firstToSecond.TryGetValue(key, out value);
  public bool TryGetKey(TSecond key, out TFirst value) => this._secondToFirst.TryGetValue(key, out value);

  public bool Remove(TFirst key) {
    if (!this._firstToSecond.TryGetValue(key, out var value))
      return false;

    this._firstToSecond.Remove(key);
    this._secondToFirst.Remove(value);
    return true;
  }

  void IDictionary.Remove(object key) {
    var firstToSecond = (IDictionary)this._firstToSecond;
    if (!firstToSecond.Contains(key))
      return;

    var value = firstToSecond[key];
    firstToSecond.Remove(key);
    ((IDictionary)this._secondToFirst).Remove(value);
  }

  public bool Remove(KeyValuePair<TFirst, TSecond> item) => this._firstToSecond.Remove(item);
  public bool Contains(object key) => ((IDictionary)this._firstToSecond).Contains(key);

  public void Clear() {
    this._firstToSecond.Clear();
    this._secondToFirst.Clear();
  }

  public void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex) => this._firstToSecond.CopyTo(array, arrayIndex);
  void ICollection.CopyTo(Array array, int index) => ((IDictionary)this._firstToSecond).CopyTo(array, index);

  [OnDeserialized]
  internal void OnDeserialized(StreamingContext context) {
    this._secondToFirst.Clear();
    foreach (var item in this._firstToSecond)
      this._secondToFirst.Add(item.Value, item.Key);
  }

  private class ReverseDictionary(BiDictionary<TFirst, TSecond> owner) : IDictionary<TSecond, TFirst>,
#if SUPPORTS_READ_ONLY_COLLECTIONS
    IReadOnlyDictionary<TSecond, TFirst>,
#endif
    IDictionary {
    public int Count => owner._secondToFirst.Count;
    object ICollection.SyncRoot => ((ICollection)owner._secondToFirst).SyncRoot;
    bool ICollection.IsSynchronized => ((ICollection)owner._secondToFirst).IsSynchronized;
    bool IDictionary.IsFixedSize => ((IDictionary)owner._secondToFirst).IsFixedSize;
    public bool IsReadOnly => owner._secondToFirst.IsReadOnly || owner._firstToSecond.IsReadOnly;

    public TFirst this[TSecond key] {
      get => owner._secondToFirst[key];
      set {
        owner._secondToFirst[key] = value;
        owner._firstToSecond[value] = key;
      }
    }

    object IDictionary.this[object key] {
      get => ((IDictionary)owner._secondToFirst)[key];
      set {
        ((IDictionary)owner._secondToFirst)[key] = value;
        ((IDictionary)owner._firstToSecond)[value] = key;
      }
    }

    public ICollection<TSecond> Keys => owner._secondToFirst.Keys;
    ICollection IDictionary.Keys => ((IDictionary)owner._secondToFirst).Keys;

#if SUPPORTS_READ_ONLY_COLLECTIONS

    IEnumerable<TSecond> IReadOnlyDictionary<TSecond, TFirst>.Keys => ((IReadOnlyDictionary<TSecond, TFirst>)owner._secondToFirst).Keys;
    IEnumerable<TFirst> IReadOnlyDictionary<TSecond, TFirst>.Values => ((IReadOnlyDictionary<TSecond, TFirst>)owner._secondToFirst).Values;

#endif

    public ICollection<TFirst> Values => owner._secondToFirst.Values;
    ICollection IDictionary.Values => ((IDictionary)owner._secondToFirst).Values;
    public IEnumerator<KeyValuePair<TSecond, TFirst>> GetEnumerator() => owner._secondToFirst.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)owner._secondToFirst).GetEnumerator();

    public void Add(TSecond key, TFirst value) {
      owner._secondToFirst.Add(key, value);
      owner._firstToSecond.Add(value, key);
    }

    void IDictionary.Add(object key, object value) {
      ((IDictionary)owner._secondToFirst).Add(key, value);
      ((IDictionary)owner._firstToSecond).Add(value, key);
    }

    public void Add(KeyValuePair<TSecond, TFirst> item) {
      owner._secondToFirst.Add(item);
      owner._firstToSecond.Add(item.Reverse());
    }

    public bool ContainsKey(TSecond key) => owner._secondToFirst.ContainsKey(key);
    public bool Contains(KeyValuePair<TSecond, TFirst> item) => owner._secondToFirst.Contains(item);
    public bool TryGetValue(TSecond key, out TFirst value) => owner._secondToFirst.TryGetValue(key, out value);

    public bool Remove(TSecond key) {
      if (!owner._secondToFirst.TryGetValue(key, out var value))
        return false;

      owner._secondToFirst.Remove(key);
      owner._firstToSecond.Remove(value);
      return true;
    }

    void IDictionary.Remove(object key) {
      var firstToSecond = (IDictionary)owner._secondToFirst;
      if (!firstToSecond.Contains(key))
        return;

      var value = firstToSecond[key];
      firstToSecond.Remove(key);
      ((IDictionary)owner._firstToSecond).Remove(value);
    }

    public bool Remove(KeyValuePair<TSecond, TFirst> item) => owner._secondToFirst.Remove(item);
    public bool Contains(object key) => ((IDictionary)owner._secondToFirst).Contains(key);

    public void Clear() {
      owner._secondToFirst.Clear();
      owner._firstToSecond.Clear();
    }

    public void CopyTo(KeyValuePair<TSecond, TFirst>[] array, int arrayIndex) => owner._secondToFirst.CopyTo(array, arrayIndex);
    void ICollection.CopyTo(Array array, int index) => ((IDictionary)owner._secondToFirst).CopyTo(array, index);
  }
}

internal class DictionaryDebugView<TKey, TValue>(IDictionary<TKey, TValue> dictionary) {
  private readonly IDictionary<TKey, TValue> _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

  [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
  public KeyValuePair<TKey, TValue>[] Items {
    get {
      var array = new KeyValuePair<TKey, TValue>[this._dictionary.Count];
      this._dictionary.CopyTo(array, 0);
      return array;
    }
  }
}

public static partial class KeyValuePairExtensions {
  public static KeyValuePair<TValue, TKey> Reverse<TKey, TValue>(this KeyValuePair<TKey, TValue> @this) {
    Against.ThisIsNull(@this);

    return new(@this.Value, @this.Key);
  }
}

public static partial class EnumerableExtensions {
  public static BiDictionary<TKey, TValue> ToBiDictionary<TItem, TKey, TValue>(this IEnumerable<TItem> @this, Func<TItem, TKey> keySelector, Func<TItem, TValue> valueSelector) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(keySelector);
    Against.ArgumentIsNull(valueSelector);

    var result = new BiDictionary<TKey, TValue>();
    foreach (var item in @this)
      result.Add(keySelector(item), valueSelector(item));

    return result;
  }
}
