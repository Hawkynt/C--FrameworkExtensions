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

/// <summary>
/// Provides static methods for querying objects that implement IEnumerable.
/// This class is required for reflection-based LINQ method access (e.g., typeof(Enumerable).GetMethod("Cast")).
/// </summary>
public static partial class Enumerable {

  /// <summary>
  /// Casts the elements of an IEnumerable to the specified type.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> Cast<TResult>(IEnumerable source) {
    ArgumentNullException.ThrowIfNull(source);

    return Invoke(source);

    static IEnumerable<TResult> Invoke(IEnumerable items) {
      foreach (var item in items)
        yield return (TResult)item;
    }
  }

  /// <summary>
  /// Filters the elements of an IEnumerable based on a specified type.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> OfType<TResult>(IEnumerable source) {
    ArgumentNullException.ThrowIfNull(source);

    return Invoke(source);

    static IEnumerable<TResult> Invoke(IEnumerable items) {
      foreach (var item in items)
        if (item is TResult result)
          yield return result;
    }
  }

  /// <summary>
  /// Generates a sequence of integral numbers within a specified range.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<int> Range(int start, int count) {
    if (count < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(count));
    if ((long)start + count - 1 > int.MaxValue)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(count));

    return Invoke(start, count);

    static IEnumerable<int> Invoke(int start, int count) {
      for (var i = 0; i < count; ++i)
        yield return start + i;
    }
  }

  /// <summary>
  /// Generates a sequence that contains one repeated value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) {
    if (count < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(count));

    return Invoke(element, count);

    static IEnumerable<TResult> Invoke(TResult element, int count) {
      for (var i = 0; i < count; ++i)
        yield return element;
    }
  }

  /// <summary>
  /// Returns an empty IEnumerable that has the specified type argument.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> Empty<TResult>() => [];

}

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

  extension(IEnumerable<int> @this) {
    public int Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      var result = 0;
      foreach (var item in @this)
        result += item;
      return result;
    }
  }

  extension(IEnumerable<int?> @this) {
    public int? Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      int? result = 0;
      foreach (var item in @this)
        result += item.GetValueOrDefault();
      return result;
    }
  }

  extension(IEnumerable<long> @this) {
    public long Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      var result = 0L;
      foreach (var item in @this)
        result += item;
      return result;
    }
  }

  extension(IEnumerable<long?> @this) {
    public long? Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      long? result = 0L;
      foreach (var item in @this)
        result += item.GetValueOrDefault();
      return result;
    }
  }

  extension(IEnumerable<float> @this) {
    public float Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      var result = 0f;
      foreach (var item in @this)
        result += item;
      return result;
    }
  }

  extension(IEnumerable<float?> @this) {
    public float? Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      float? result = 0f;
      foreach (var item in @this)
        result += item.GetValueOrDefault();
      return result;
    }
  }

  extension(IEnumerable<double> @this) {
    public double Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      var result = 0d;
      foreach (var item in @this)
        result += item;
      return result;
    }
  }

  extension(IEnumerable<double?> @this) {
    public double? Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      double? result = 0d;
      foreach (var item in @this)
        result += item.GetValueOrDefault();
      return result;
    }
  }

  extension(IEnumerable<decimal> @this) {
    public decimal Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      var result = 0m;
      foreach (var item in @this)
        result += item;
      return result;
    }
  }

  extension(IEnumerable<decimal?> @this) {
    public decimal? Sum() {
      ArgumentNullException.ThrowIfNull(@this);
      decimal? result = 0m;
      foreach (var item in @this)
        result += item.GetValueOrDefault();
      return result;
    }
  }

  extension<TSource>(IEnumerable<TSource> @this) {
    public int Sum(Func<TSource, int> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      var result = 0;
      foreach (var item in @this)
        result += selector(item);
      return result;
    }

    public int? Sum(Func<TSource, int?> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      int? result = 0;
      foreach (var item in @this)
        result += selector(item).GetValueOrDefault();
      return result;
    }

    public long Sum(Func<TSource, long> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      var result = 0L;
      foreach (var item in @this)
        result += selector(item);
      return result;
    }

    public long? Sum(Func<TSource, long?> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      long? result = 0L;
      foreach (var item in @this)
        result += selector(item).GetValueOrDefault();
      return result;
    }

    public float Sum(Func<TSource, float> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      var result = 0f;
      foreach (var item in @this)
        result += selector(item);
      return result;
    }

    public float? Sum(Func<TSource, float?> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      float? result = 0f;
      foreach (var item in @this)
        result += selector(item).GetValueOrDefault();
      return result;
    }

    public double Sum(Func<TSource, double> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      var result = 0d;
      foreach (var item in @this)
        result += selector(item);
      return result;
    }

    public double? Sum(Func<TSource, double?> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      double? result = 0d;
      foreach (var item in @this)
        result += selector(item).GetValueOrDefault();
      return result;
    }

    public decimal Sum(Func<TSource, decimal> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      var result = 0m;
      foreach (var item in @this)
        result += selector(item);
      return result;
    }

    public decimal? Sum(Func<TSource, decimal?> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);
      decimal? result = 0m;
      foreach (var item in @this)
        result += selector(item).GetValueOrDefault();
      return result;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TResult> SelectMany<TResult>(Func<TSource, IEnumerable<TResult>> selector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(selector);

      return Invoke(@this, selector);

      static IEnumerable<TResult> Invoke(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
        foreach (var item in source)
          foreach (var subItem in selector(item))
            yield return subItem;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TResult> SelectMany<TCollection, TResult>(Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(collectionSelector);
      ArgumentNullException.ThrowIfNull(resultSelector);

      return Invoke(@this, collectionSelector, resultSelector);

      static IEnumerable<TResult> Invoke(IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
        foreach (var item in source)
          foreach (var subItem in collectionSelector(item))
            yield return resultSelector(item, subItem);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Reverse() {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source) {
        var list = source.ToList();
        for (var i = list.Count - 1; i >= 0; --i)
          yield return list[i];
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Distinct()
      => @this.Distinct(EqualityComparer<TSource>.Default);

    public IEnumerable<TSource> Distinct(IEqualityComparer<TSource> comparer) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, comparer ?? EqualityComparer<TSource>.Default);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
        var seen = new Dictionary<Wrapper<TSource>, bool>(new WrapperComparer<TSource>());
        foreach (var item in source) {
          var wrapper = new Wrapper<TSource>(item, comparer);
          if (seen.ContainsKey(wrapper))
            continue;

          seen[wrapper] = true;
          yield return item;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Except(IEnumerable<TSource> second)
      => @this.Except(second, EqualityComparer<TSource>.Default);

    public IEnumerable<TSource> Except(IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);

      return Invoke(@this, second, comparer ?? EqualityComparer<TSource>.Default);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
        var set = new Dictionary<Wrapper<TSource>, bool>(new WrapperComparer<TSource>());
        foreach (var item in second)
          set[new Wrapper<TSource>(item, comparer)] = true;

        foreach (var item in first) {
          var wrapper = new Wrapper<TSource>(item, comparer);
          if (!set.ContainsKey(wrapper))
            yield return item;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Intersect(IEnumerable<TSource> second)
      => @this.Intersect(second, EqualityComparer<TSource>.Default);

    public IEnumerable<TSource> Intersect(IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);

      return Invoke(@this, second, comparer ?? EqualityComparer<TSource>.Default);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
        var set = new Dictionary<Wrapper<TSource>, bool>(new WrapperComparer<TSource>());
        foreach (var item in second)
          set[new Wrapper<TSource>(item, comparer)] = true;

        var returned = new Dictionary<Wrapper<TSource>, bool>(new WrapperComparer<TSource>());
        foreach (var item in first) {
          var wrapper = new Wrapper<TSource>(item, comparer);
          if (set.ContainsKey(wrapper) && !returned.ContainsKey(wrapper)) {
            returned[wrapper] = true;
            yield return item;
          }
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Union(IEnumerable<TSource> second)
      => @this.Union(second, EqualityComparer<TSource>.Default);

    public IEnumerable<TSource> Union(IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);

      return Invoke(@this, second, comparer ?? EqualityComparer<TSource>.Default);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
        var set = new Dictionary<Wrapper<TSource>, bool>(new WrapperComparer<TSource>());
        foreach (var item in first) {
          var wrapper = new Wrapper<TSource>(item, comparer);
          if (!set.ContainsKey(wrapper)) {
            set[wrapper] = true;
            yield return item;
          }
        }
        foreach (var item in second) {
          var wrapper = new Wrapper<TSource>(item, comparer);
          if (!set.ContainsKey(wrapper)) {
            set[wrapper] = true;
            yield return item;
          }
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(TSource value)
      => @this.Contains(value, EqualityComparer<TSource>.Default);

    public bool Contains(TSource value, IEqualityComparer<TSource> comparer) {
      ArgumentNullException.ThrowIfNull(@this);

      comparer ??= EqualityComparer<TSource>.Default;
      foreach (var item in @this)
        if (comparer.Equals(item, value))
          return true;

      return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Skip(int count) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, count);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, int count) {
        var skipped = 0;
        foreach (var item in source) {
          if (skipped < count) {
            ++skipped;
            continue;
          }
          yield return item;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Take(int count) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, count);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, int count) {
        var taken = 0;
        foreach (var item in source) {
          if (taken >= count)
            yield break;
          ++taken;
          yield return item;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Concat(IEnumerable<TSource> second) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);

      return Invoke(@this, second);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> first, IEnumerable<TSource> second) {
        foreach (var item in first)
          yield return item;
        foreach (var item in second)
          yield return item;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> DefaultIfEmpty()
      => @this.DefaultIfEmpty(default!);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> DefaultIfEmpty(TSource defaultValue) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, defaultValue);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, TSource defaultValue) {
        var hasElements = false;
        foreach (var item in source) {
          hasElements = true;
          yield return item;
        }
        if (!hasElements)
          yield return defaultValue;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource ElementAt(int index) {
      ArgumentNullException.ThrowIfNull(@this);

      var currentIndex = 0;
      foreach (var item in @this) {
        if (currentIndex == index)
          return item;
        ++currentIndex;
      }

      AlwaysThrow.ArgumentOutOfRangeException(nameof(index));
      return default!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource? ElementAtOrDefault(int index) {
      ArgumentNullException.ThrowIfNull(@this);

      var currentIndex = 0;
      foreach (var item in @this) {
        if (currentIndex == index)
          return item;
        ++currentIndex;
      }

      return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource[]> Chunk(int size) {
      ArgumentNullException.ThrowIfNull(@this);
      if (size < 1)
        AlwaysThrow.ArgumentOutOfRangeException(nameof(size));

      return Invoke(@this, size);

      static IEnumerable<TSource[]> Invoke(IEnumerable<TSource> source, int size) {
        var chunk = new List<TSource>(size);
        foreach (var item in source) {
          chunk.Add(item);
          if (chunk.Count == size) {
            yield return chunk.ToArray();
            chunk.Clear();
          }
        }
        if (chunk.Count > 0)
          yield return chunk.ToArray();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector)
      => @this.DistinctBy(keySelector, EqualityComparer<TKey>.Default);

    public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);

      return Invoke(@this, keySelector, comparer ?? EqualityComparer<TKey>.Default);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
        var seen = new Dictionary<Wrapper<TKey>, bool>(new WrapperComparer<TKey>());
        foreach (var item in source) {
          var key = keySelector(item);
          var wrapper = new Wrapper<TKey>(key, comparer);
          if (seen.ContainsKey(wrapper))
            continue;

          seen[wrapper] = true;
          yield return item;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Order()
      => @this.Order(Comparer<TSource>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> Order(IComparer<TSource>? comparer) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, comparer ?? Comparer<TSource>.Default);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, IComparer<TSource> comparer) {
        var list = source.ToList();
        list.Sort(comparer);
        return list;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> OrderDescending()
      => @this.OrderDescending(Comparer<TSource>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> OrderDescending(IComparer<TSource>? comparer) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, comparer ?? Comparer<TSource>.Default);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, IComparer<TSource> comparer) {
        var list = source.ToList();
        list.Sort((x, y) => comparer.Compare(y, x));
        return list;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector)
      => _GroupByCore(@this, keySelector, EqualityComparer<TKey>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
      => _GroupByCore(@this, keySelector, comparer ?? EqualityComparer<TKey>.Default);
  }

  private static IEnumerable<IGrouping<TKey, TSource>> _GroupByCore<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(keySelector);

    var groups = new Dictionary<Wrapper<TKey>, Grouping<TKey, TSource>>(new WrapperComparer<TKey>());
    var orderedGroups = new List<Grouping<TKey, TSource>>();
    foreach (var element in source) {
      var key = keySelector(element);
      var wrapper = new Wrapper<TKey>(key, comparer);
      if (!groups.TryGetValue(wrapper, out var group)) {
        group = new Grouping<TKey, TSource>(key);
        groups[wrapper] = group;
        orderedGroups.Add(group);
      }
      group.Add(element);
    }

    foreach (var group in orderedGroups)
      yield return group;
  }

  private sealed class WrapperComparer<T> : IEqualityComparer<Wrapper<T>> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Wrapper<T> x, Wrapper<T> y) => x.Equals(y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(Wrapper<T> obj) => obj.GetHashCode();
  }

  extension<TSource>(IEnumerable<TSource> @this) {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TSource> ToDictionary<TKey>(Func<TSource, TKey> keySelector) where TKey : notnull
      => @this.ToDictionary(keySelector, EqualityComparer<TKey>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TSource> ToDictionary<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer) where TKey : notnull
      => @this.ToDictionary(keySelector, x => x, comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
      => @this.ToDictionary(keySelector, elementSelector, EqualityComparer<TKey>.Default);

    public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);
      ArgumentNullException.ThrowIfNull(elementSelector);

      var result = new Dictionary<TKey, TElement>(comparer ?? EqualityComparer<TKey>.Default);
      foreach (var item in @this)
        result.Add(keySelector(item), elementSelector(item));

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> SkipWhile(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      return Invoke(@this, predicate);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        var skipping = true;
        foreach (var item in source) {
          if (skipping && predicate(item))
            continue;
          skipping = false;
          yield return item;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> TakeWhile(Func<TSource, bool> predicate) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      return Invoke(@this, predicate);

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        foreach (var item in source) {
          if (!predicate(item))
            yield break;
          yield return item;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> OrderByDescending<TKey>(Func<TSource, TKey> keySelector)
      => @this.OrderByDescending(keySelector, Comparer<TKey>.Default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TSource> OrderByDescending<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(keySelector);
      ArgumentNullException.ThrowIfNull(comparer);

      return Invoke(@this, keySelector, Compare);

      int Compare((int, TKey, TSource) x, (int, TKey, TSource) y) {
        var result = comparer.Compare(y.Item2, x.Item2);
        return result != 0 ? result : x.Item1.CompareTo(y.Item1);
      }

      static IEnumerable<TSource> Invoke(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Comparison<(int, TKey, TSource)> compare) {
        var sortedList = new List<(int, TKey, TSource)>(source.Select((v, i) => (i, keySelector(v), v)));
        sortedList.Sort(compare);
        return sortedList.Select(i => i.Item3);
      }
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

  [DoesNotReturn]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ThrowNoElements() => AlwaysThrow.InvalidOperationException("Sequence contains no elements");

}

#endif
