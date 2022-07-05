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
using System.Linq;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Collections.Specialized {
  internal static partial class OrderedDictionaryExtensions {
  }

  internal class OrderedDictionary<TKey, TValue> : Dictionary<TKey, TValue> {
    private readonly List<TKey> _keys = new List<TKey>();

    public OrderedDictionary() {
    }

    public OrderedDictionary(IEqualityComparer<TKey> comparer)
      : base(comparer) {
    }

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
}