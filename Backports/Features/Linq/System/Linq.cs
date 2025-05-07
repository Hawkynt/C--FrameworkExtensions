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

#if !SUPPORTS_LINQ
using Guard;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class EnumerablePolyfills {
  public static TResult[] ToArray<TResult>(this IEnumerable<TResult> @this) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> Cast<TResult>(this IEnumerable @this) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

    return Invoke(@this);

    static IEnumerable<TResult> Invoke(IEnumerable @this) {
      foreach (var item in @this)
        yield return (TResult)item;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (predicate == null)
      AlwaysThrow.ArgumentNullException(nameof(predicate));

    return Invoke(@this, predicate);

    static IEnumerable<TSource> Invoke(IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
      foreach (var item in @this)
        if (predicate(item))
          yield return item;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> @this, Func<TSource, int, bool> predicate) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (predicate == null)
      AlwaysThrow.ArgumentNullException(nameof(predicate));

    return Invoke(@this, predicate);

    static IEnumerable<TSource> Invoke(IEnumerable<TSource> @this, Func<TSource, int, bool> predicate) {
      var i = 0;
      foreach (var item in @this)
        if (predicate(item, i++))
          yield return item;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> @this, Func<TSource, TResult> selector) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (selector == null)
      AlwaysThrow.ArgumentNullException(nameof(selector));

    return Invoke(@this, selector);

    static IEnumerable<TResult> Invoke(IEnumerable<TSource> @this, Func<TSource, TResult> selector) {
      foreach (var item in @this)
        yield return selector(item);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> @this, Func<TSource, int, TResult> selector) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (selector == null)
      AlwaysThrow.ArgumentNullException(nameof(selector));

    return Invoke(@this, selector);

    static IEnumerable<TResult> Invoke(IEnumerable<TSource> @this, Func<TSource, int, TResult> selector) {
      var index = 0;
      foreach (var item in @this)
        yield return selector(item, index++);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector)
    => OrderBy(@this, keySelector, Comparer<TKey>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));
    if (comparer == null)
      AlwaysThrow.ArgumentNullException(nameof(comparer));

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> ThenBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector)
    // Assuming source is already sorted by a previous OrderBy or ThenBy
    => OrderBy(@this, keySelector, Comparer<TKey>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TSource> ThenBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    // Assuming source is already sorted by a previous OrderBy or ThenBy
    => OrderBy(@this, keySelector, comparer);

  public static TSource First<TSource>(this IEnumerable<TSource> @this) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

    foreach (var item in @this)
      return item;

    _ThrowNoElements();
    return default;
  }

  public static TSource First<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (predicate == null)
      AlwaysThrow.ArgumentNullException(nameof(predicate));

    foreach (var item in @this)
      if (predicate(item))
        return item;

    _ThrowNoElements();
    return default;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this) => FirstOrDefault(@this, default(TSource));

  public static TSource Single<TSource>(this IEnumerable<TSource> @this) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

    var result = default(TSource);
    var found = false;
    foreach (var item in @this) {
      if (found)
        AlwaysThrow.InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      _ThrowNoElements();

    return result;
  }

  public static TSource Single<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (predicate == null)
      AlwaysThrow.ArgumentNullException(nameof(predicate));

    var result = default(TSource);
    var found = false;
    foreach (var item in @this) {
      if (!predicate(item))
        continue;

      if (found)
        AlwaysThrow.InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      _ThrowNoElements();

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this) => SingleOrDefault(@this, default(TSource));

  public static TSource Last<TSource>(this IEnumerable<TSource> @this) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));

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
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (predicate == null)
      AlwaysThrow.ArgumentNullException(nameof(predicate));

    var result = default(TSource);
    var found = false;
    foreach (var item in @this) {
      if (!predicate(item))
        continue;

      result = item;
      found = true;
    }

    if (!found)
      _ThrowNoElements();

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this) => LastOrDefault(@this, default(TSource));

  public static TSource Min<TSource>(this IEnumerable<TSource> source) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));

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
      AlwaysThrow.ArgumentNullException(nameof(source));

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

  private readonly struct Wrapper<T>(T value, IEqualityComparer<T> comparer) {
    private readonly T value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => this.value == null ? 0 : comparer.GetHashCode(this.value);

    #region Overrides of ValueType

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is Wrapper<T> w && comparer.Equals(this.value, w.value);

    #endregion
  }

  private sealed class Grouping<TKey, TSource>(TKey key) : IGrouping<TKey, TSource> {
    private readonly List<TSource> _items = [];

    public TKey Key { get; } = key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(TSource item) => this._items.Add(item);

    #region Implementation of IEnumerable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<TSource> GetEnumerator() => this._items.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
    if (source == null)
      AlwaysThrow.ArgumentNullException(nameof(source));
    if (keySelector == null)
      AlwaysThrow.ArgumentNullException(nameof(keySelector));

    return Invoke(source, keySelector, EqualityComparer<TKey>.Default);

    static IEnumerable<IGrouping<TKey, TSource>> Invoke(IEnumerable<TSource> @this, Func<TSource, TKey> keySelector, EqualityComparer<TKey> comparer) {
      var groups = new Dictionary<Wrapper<TKey>, Grouping<TKey, TSource>>();
      foreach (var element in @this) {
        var key = keySelector(element);
        var wrapper = new Wrapper<TKey>(key, comparer);
        if (!groups.ContainsKey(wrapper))
          groups[wrapper] = new(key);
        groups[wrapper].Add(element);
      }

      return groups.Values.Cast<IGrouping<TKey, TSource>>();
    }
  }

  [DoesNotReturn]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ThrowNoElements() => AlwaysThrow.InvalidOperationException("Sequence contains no elements");

}

#endif
