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

  extension(IEnumerable @this) {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TResult> Cast<TResult>() {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this);

      static IEnumerable<TResult> Invoke(IEnumerable source) {
        foreach (var item in source)
          yield return (TResult)item;
      }
    }
  }

  extension<TSource>(IEnumerable<TSource> @this) {
    public TSource[] ToArray() {
      ArgumentNullException.ThrowIfNull(@this);

      switch (@this) {
        case ICollection<TSource> collection: {
          var result = new TSource[collection.Count];
          collection.CopyTo(result, 0);
          return result;
        }
        default: {
          var result = new TSource[64];
          var length = 0;

          foreach (var item in @this) {
            if (result.Length == length) {
              var next = new TSource[length + 128];
              Array.Copy(result, 0, next, 0, length);
              result = next;
            }

            result[length++] = item;
          }

          if (length == result.Length)
            return result;

          {
            var next = new TSource[length];
            Array.Copy(result, 0, next, 0, length);
            result = next;
          }

          return result;
        }
      }
    }

    public List<TSource> ToList() {
      ArgumentNullException.ThrowIfNull(@this);

      return @this switch {
        ICollection<TSource> collection => new(collection),
        _ => new(@this)
      };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector)
      => @this.ToLookup(keySelector, EqualityComparer<TKey>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
      => @this.ToLookup(keySelector, i => i, comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
      => @this.ToLookup(keySelector, elementSelector, EqualityComparer<TKey>.Default);

    public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);
      ArgumentNullException.ThrowIfNull(elementSelector);

      comparer ??= EqualityComparer<TKey>.Default;
      var lookup = new Lookup<TKey, TElement>(comparer);
      foreach (var item in @this)
        lookup.Add(keySelector(item), elementSelector(item));

      return lookup;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Where(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      return Invoke(@this, predicate);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        foreach (var item in source)
          if (predicate(item))
            yield return item;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Where(Func<TSource, int, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      return Invoke(@this, predicate);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
        var i = 0;
        foreach (var item in source)
          if (predicate(item, i++))
            yield return item;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);

      return Invoke(@this, selector);

      static IEnumerable<TResult> Invoke(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
        foreach (var item in source)
          yield return selector(item);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TResult> Select<TResult>(Func<TSource, int, TResult> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);

      return Invoke(@this, selector);

      static IEnumerable<TResult> Invoke(IEnumerable<TSource> source, Func<TSource, int, TResult> selector) {
        var index = 0;
        foreach (var item in source)
          yield return selector(item, index++);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> OrderBy<TKey>(Func<TSource, TKey> keySelector)
      => @this.OrderBy(keySelector, Comparer<TKey>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> OrderBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);
      ArgumentNullException.ThrowIfNull(comparer);

      return Invoke(@this, keySelector, Compare);

      int Compare((int, TKey, TSource) x, (int, TKey, TSource) y) {
        var result = comparer.Compare(x.Item2, y.Item2);
        return result != 0 ? result : x.Item1.CompareTo(y.Item1);
      }

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Comparison<(int, TKey, TSource)> compare) {
        var sortedList = new List<(int, TKey, TSource)>(source.Select((v, i) => (i, keySelector(v), v)));
        sortedList.Sort(compare);
        return sortedList.Select(i => i.Item3);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> ThenBy<TKey>(Func<TSource, TKey> keySelector)
      // Assuming source is already sorted by a previous OrderBy or ThenBy
      => @this.OrderBy(keySelector, Comparer<TKey>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> ThenBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
      // Assuming source is already sorted by a previous OrderBy or ThenBy
      => @this.OrderBy(keySelector, comparer);

    public TSource First() {
      ArgumentNullException.ThrowIfNull(@this);

      foreach (var item in @this)
        return item;

      _ThrowNoElements();
      return default;
    }

    public TSource First(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      foreach (var item in @this)
        if (predicate(item))
          return item;

      _ThrowNoElements();
      return default;
    }

    public TSource? FirstOrDefault() {
      ArgumentNullException.ThrowIfNull(@this);

      foreach (var item in @this)
        return item;

      return default;
    }

    public TSource Single() {
      ArgumentNullException.ThrowIfNull(@this);

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

      return result!;
    }

    public TSource Single(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

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

      return result!;
    }

    public TSource? SingleOrDefault() {
      ArgumentNullException.ThrowIfNull(@this);

      var result = default(TSource);
      var found = false;
      foreach (var item in @this) {
        if (found)
          AlwaysThrow.InvalidOperationException("Sequence contains more than one element");

        result = item;
        found = true;
      }

      return result;
    }

    public TSource Last() {
      ArgumentNullException.ThrowIfNull(@this);

      var result = default(TSource);
      var found = false;
      foreach (var item in @this) {
        result = item;
        found = true;
      }

      if (!found)
        _ThrowNoElements();

      return result!;
    }

    public TSource Last(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

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

      return result!;
    }

    public TSource? LastOrDefault() {
      ArgumentNullException.ThrowIfNull(@this);

      var result = default(TSource);
      foreach (var item in @this)
        result = item;

      return result;
    }

    public TSource Min() {
      ArgumentNullException.ThrowIfNull(@this);

      var comparer = Comparer<TSource>.Default;
      using var enumerator = @this.GetEnumerator();
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

    public TSource Max() {
      ArgumentNullException.ThrowIfNull(@this);

      var comparer = Comparer<TSource>.Default;

      using var enumerator = @this.GetEnumerator();
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

    public bool Any() {
      ArgumentNullException.ThrowIfNull(@this);

      using var enumerator = @this.GetEnumerator();
      return enumerator.MoveNext();
    }

    public bool Any(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      foreach (var item in @this)
        if (predicate(item))
          return true;

      return false;
    }

    public bool All(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      foreach (var item in @this)
        if (!predicate(item))
          return false;

      return true;
    }

    public int Count() {
      ArgumentNullException.ThrowIfNull(@this);

      if (@this is ICollection<TSource> collection)
        return collection.Count;

      var count = 0;
      foreach (var _ in @this)
        ++count;

      return count;
    }

    public int Count(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      var count = 0;
      foreach (var item in @this)
        if (predicate(item))
          ++count;

      return count;
    }

    public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(func);

      var result = seed;
      foreach (var item in @this)
        result = func(result, item);

      return result;
    }

    public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(func);
      ArgumentNullException.ThrowIfNull(resultSelector);

      var result = seed;
      foreach (var item in @this)
        result = func(result, item);

      return resultSelector(result);
    }

    public TSource Aggregate(Func<TSource, TSource, TSource> func) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(func);

      using var enumerator = @this.GetEnumerator();
      if (!enumerator.MoveNext())
        _ThrowNoElements();

      var result = enumerator.Current;
      while (enumerator.MoveNext())
        result = func(result, enumerator.Current);

      return result;
    }
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

  private sealed class Lookup<TKey, TElement> : ILookup<TKey, TElement> {
    private readonly Dictionary<Wrapper<TKey>, Grouping<TKey, TElement>> _groups;
    private readonly List<Grouping<TKey, TElement>> _orderedGroups = [];
    private readonly IEqualityComparer<TKey> _comparer;

    internal Lookup(IEqualityComparer<TKey> comparer) {
      this._comparer = comparer ?? EqualityComparer<TKey>.Default;
      this._groups = [];
    }

    public int Count => this._orderedGroups.Count;

    public IEnumerable<TElement> this[TKey key] {
      get {
        var wrapper = new Wrapper<TKey>(key, this._comparer);
        return this._groups.TryGetValue(wrapper, out var grouping) ? grouping : [];
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(TKey key) => this._groups.ContainsKey(new Wrapper<TKey>(key, this._comparer));

    internal void Add(TKey key, TElement element) {
      var wrapper = new Wrapper<TKey>(key, this._comparer);
      if (!this._groups.TryGetValue(wrapper, out var grouping)) {
        grouping = new(key);
        this._groups[wrapper] = grouping;
        this._orderedGroups.Add(grouping);
      }

      grouping.Add(element);
    }

    #region Implementation of IEnumerable

    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() {
      foreach (var group in this._orderedGroups)
        yield return group;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(keySelector);

    return Invoke(source, keySelector, EqualityComparer<TKey>.Default);

    static IEnumerable<IGrouping<TKey, TSource>> Invoke(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, EqualityComparer<TKey> comparer) {
      var groups = new Dictionary<Wrapper<TKey>, Grouping<TKey, TSource>>();
      foreach (var element in source) {
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
