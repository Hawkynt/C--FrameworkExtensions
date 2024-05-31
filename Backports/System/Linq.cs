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

using System.Collections;
using System.Collections.Generic;

namespace System.Linq;

using Diagnostics.CodeAnalysis;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class EnumerablePolyfills {

#if !SUPPORTS_FIRSTLASTSINGLE_PREDICATE

  public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    foreach (var item in @this)
      return item;

    return defaultValue;
  }

 public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate)
  => FirstOrDefault(@this, predicate, default)
  ;

 public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    foreach (var item in @this)
      if (predicate(item))
        return item;

    return defaultValue;
  }

  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    var result = defaultValue;
    var found = false;
    foreach (var item in @this) {
      if (found)
        throw new InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      return defaultValue;

    return result;
  }

  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    var result = defaultValue;
    var found = false;
    foreach (var item in @this) {
      if(!predicate(item))
        continue;

      if (found)
        throw new InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      return defaultValue;

    return result;
  }

  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate)
    => SingleOrDefault(@this, predicate, default)
    ;

  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    var result = defaultValue;
    foreach (var item in @this)
      result = item;
    
    return result;
  }

  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate)
   => LastOrDefault(@this, predicate, default)
   ;

  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    var result = defaultValue;
    foreach (var item in @this)
      if (predicate(item))
        result = item;
    
    return result;
  }

#endif

#if !SUPPORTS_LINQ

  public static TResult[] ToArray<TResult>(this IEnumerable<TResult> @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    switch (@this) {
      case ICollection<TResult> collection: {
        var result = new TResult[collection.Count];
        collection.CopyTo(result, 0);
        return result;
      }
      default: {
        var result = new TResult[64];
        var length = 0;

        foreach (var item in @this) {
          if (result.Length == length) {
            var next = new TResult[length + 128];
            Array.Copy(result, 0, next, 0, length);
            result = next;
          }

          result[length++] = item;
        }

        if (length == result.Length)
          return result;

        {
          var next = new TResult[length];
          Array.Copy(result, 0, next, 0, length);
          result = next;
        }

        return result;
      }
    }
  }

  public static IEnumerable<TResult> Cast<TResult>(this IEnumerable @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this);
    
    static IEnumerable<TResult> Invoke(IEnumerable @this) {
      foreach (var item in @this)
        yield return (TResult)item;
    }
  }

  public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    return Invoke(@this, predicate);

    static IEnumerable<TSource> Invoke(IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
      foreach (var item in @this)
        if (predicate(item))
          yield return item;
    }
  }
  public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> @this, Func<TSource, int, bool> predicate) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    return Invoke(@this, predicate);

    static IEnumerable<TSource> Invoke(IEnumerable<TSource> @this, Func<TSource, int, bool> predicate) {
      var i = 0;
      foreach (var item in @this)
        if (predicate(item, i++))
          yield return item;
    }
  }

  public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> @this, Func<TSource, TResult> selector) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (selector == null)
      throw new ArgumentNullException(nameof(selector));

    return Invoke(@this, selector);
    
    static IEnumerable<TResult> Invoke(IEnumerable<TSource> @this, Func<TSource, TResult> selector) {
      foreach (var item in @this)
        yield return selector(item);
    }
  }

  public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> @this, Func<TSource, int, TResult> selector) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (selector == null)
      throw new ArgumentNullException(nameof(selector));

    return Invoke(@this, selector);
    
    static IEnumerable<TResult> Invoke(IEnumerable<TSource> @this, Func<TSource, int, TResult> selector) {
      var index = 0;
      foreach (var item in @this)
        yield return selector(item, index++);
    }
  }

  public static IEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector)
    => OrderBy(@this, keySelector, Comparer<TKey>.Default)
    ;

  public static IEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (keySelector == null)
      throw new ArgumentNullException(nameof(keySelector));
    if (comparer == null)
      throw new ArgumentNullException(nameof(comparer));
    
    return Invoke(@this, keySelector, Compare);

    int Compare((int, TKey, TSource) x, (int, TKey, TSource) y) {
      var result = comparer.Compare(x.Item2, y.Item2);
      return result != 0 ? result : x.Item1.CompareTo(y.Item1);
    }

    static IEnumerable<TSource> Invoke(IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, Comparison<(int, TKey, TSource)> compare) {
      var sortedList = new List<(int, TKey, TSource)>(@this.Select((v, i) => (i, keySelector(v), v)));
      sortedList.Sort(compare);
      return sortedList.Select(i => i.Item3);
    }
  }
  
  public static IEnumerable<TSource> ThenBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector)
    // Assuming source is already sorted by a previous OrderBy or ThenBy
    => OrderBy(@this, keySelector, Comparer<TKey>.Default)
    ;

  public static IEnumerable<TSource> ThenBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    // Assuming source is already sorted by a previous OrderBy or ThenBy
    => OrderBy(@this, keySelector, comparer)
    ;

  public static TSource First<TSource>(this IEnumerable<TSource> @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    foreach (var item in @this)
      return item;

    _ThrowNoElements();
    return default;
  }

  public static TSource First<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    foreach (var item in @this)
      if(predicate(item))
        return item;

    _ThrowNoElements();
    return default;
  }

  public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this) => FirstOrDefault(@this, default(TSource));

  public static TSource Single<TSource>(this IEnumerable<TSource> @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    var result = default(TSource);
    var found = false;
    foreach (var item in @this) {
      if (found)
        throw new InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      _ThrowNoElements();

    return result;
  }

  public static TSource Single<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    var result = default(TSource);
    var found = false;
    foreach (var item in @this) {
      if (!predicate(item))
        continue;
        
      if (found)
        throw new InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      _ThrowNoElements();

    return result;
  }

  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this) => SingleOrDefault(@this, default(TSource));

  public static TSource Last<TSource>(this IEnumerable<TSource> @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    var result = default(TSource);
    var found = false;
    foreach (var item in @this) {
      result = item;
      found = true;
    }

    if (!found)
      _ThrowNoElements();

    return result;
  }

  public static TSource Last<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    var result = default(TSource);
    var found = false;
    foreach (var item in @this) {
      if(!predicate(item))
        continue;

      result = item;
      found = true;
    }

    if (!found)
      _ThrowNoElements();

    return result;
  }

  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this) => LastOrDefault(@this, default(TSource));

  public static TSource Min<TSource>(this IEnumerable<TSource> source) {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    var comparer = Comparer<TSource>.Default;
    using var enumerator = source.GetEnumerator();
    if (!enumerator.MoveNext()) {
      _ThrowNoElements();
      return default;
    }

    var min = enumerator.Current;
    while (enumerator.MoveNext()) {
      var current = enumerator.Current;
      if (comparer.Compare(current, min) < 0)
        min = current;
    }

    return min;
  }

  public static TSource Max<TSource>(this IEnumerable<TSource> source) {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    var comparer = Comparer<TSource>.Default;

    using var enumerator = source.GetEnumerator();
    if (!enumerator.MoveNext()) {
      _ThrowNoElements();
      return default;
    }

    var max = enumerator.Current;
    while (enumerator.MoveNext()) {
      var current = enumerator.Current;
      if (comparer.Compare(current, max) > 0)
        max = current;
    }

    return max;
  }

  private readonly struct Wrapper<T> {
    private readonly T value;
    private readonly IEqualityComparer<T> _comparer;

    public Wrapper(T value, IEqualityComparer<T> comparer) {
      this.value = value;
      this._comparer = comparer;
    }

    public override int GetHashCode() => this.value == null ? 0 : this._comparer.GetHashCode(this.value);

    #region Overrides of ValueType

    public override bool Equals(object obj) => obj is Wrapper<T> w && this._comparer.Equals(this.value, w.value);

    #endregion
  }

  private sealed class Grouping<TKey, TSource> : IGrouping<TKey, TSource> {
    private readonly List<TSource> _items = new();
    public Grouping(TKey key) => this.Key = key;

    public TKey Key { get; }

    internal void Add(TSource item) => this._items.Add(item);

    #region Implementation of IEnumerable

    public IEnumerator<TSource> GetEnumerator() => this._items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion
  }

  public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
    if (source == null)
      throw new ArgumentNullException(nameof(source));
    if (keySelector == null)
      throw new ArgumentNullException(nameof(keySelector));

    var comparer = EqualityComparer<TKey>.Default;
    return Invoke(source, keySelector, comparer);

    static IEnumerable<IGrouping<TKey, TSource>> Invoke(IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, EqualityComparer<TKey> comparer) {
      var groups = new Dictionary<Wrapper<TKey>, Grouping<TKey, TSource>>();
      foreach (var element in @this) {
        var key = keySelector(element);
        var wrapper = new Wrapper<TKey>(key, comparer);
        if (!groups.ContainsKey(wrapper)) {
          groups[wrapper] = new(key);
        }
        groups[wrapper].Add(element);
      }

      return groups.Values.Cast<IGrouping<TKey, TSource>>();
    }
  }

#endif

  [DoesNotReturn]
  private static void _ThrowNoElements() => throw new InvalidOperationException("Sequence contains no elements");

}

#if !SUPPORTS_LINQ

public interface IGrouping<out TKey, TElement> : IEnumerable<TElement> {
  TKey Key { get; }
}

#endif
