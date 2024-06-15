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
using System.Linq;

namespace System.Collections.Specialized;

public class OrderedDictionary<TKey, TValue> : Dictionary<TKey, TValue> {
  private readonly List<TKey> _keys = [];

  public OrderedDictionary() { }

  public OrderedDictionary(IEqualityComparer<TKey> comparer)
    : base(comparer) { }

  public new void Add(TKey key, TValue value) {
    base.Add(key, value);
    this._keys.Add(key);
  }

  public new void Remove(TKey key) {
    base.Remove(key);
    this._keys.Remove(key);
  }

  public new void Clear() {
    base.Clear();
    this._keys.Clear();
  }

  public new IEnumerable<TKey> Keys => this._keys.Select(k => k);
  public new IEnumerable<TValue> Values => this._keys.Select(k => this[k]);
  public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._keys.Select(k => new KeyValuePair<TKey, TValue>(k, this[k])).GetEnumerator();
}
