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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Guard;
#if SUPPORTS_LINQ_PARAMETERS
using System.Linq.Expressions;
#endif

namespace System.ComponentModel;

/// <summary>
///   A BindingList which is sortable in DataGridViews.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class SortableBindingList<TValue> : BindingList<TValue>, INotifyCollectionChanged {
  #region nested types

  private static class StableSorter {
    private sealed class SortValueWithSourceIndex(int index, TValue value, object sortValue) : IComparable<SortValueWithSourceIndex> {
      private readonly int _index = index;
      public readonly TValue Value = value;
      private readonly object _sortValue = sortValue;

      #region Relational members

      public int CompareTo(SortValueWithSourceIndex other) {
        var a = this._sortValue;
        var b = other._sortValue;
        var result = _CompareValues(a, b);
        return result == 0 ? this._index.CompareTo(other._index) : result;
      }

      /// <summary>
      ///   Compares two arbitrary values of the same type with another
      /// </summary>
      /// <param name="a">One value</param>
      /// <param name="b">The other value</param>
      /// <returns>-1 if a &lt; b; +1 if a &gt; b; otherwise, 0</returns>
      private static int _CompareValues(object a, object b) {
        if (ReferenceEquals(a, b))
          return 0;

        if (a == null)
          return 1;

        if (b == null)
          return -1;

        //can ask the x value
        if (a is IComparable comparableX)
          return comparableX.CompareTo(b);

        //can ask the y value
        if (b is IComparable comparableY)
          return -comparableY.CompareTo(a);

        // try equals
        if (a.Equals(b) || b.Equals(a))
          return 0;

        //not comparable, compare string representations
        return string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal);
      }

      private sealed class _AscendingComparer : IComparer<SortValueWithSourceIndex>, IComparer {
        public int Compare(SortValueWithSourceIndex x, SortValueWithSourceIndex y) => x.CompareTo(y);
        public int Compare(object x, object y) => this.Compare((SortValueWithSourceIndex)x!, (SortValueWithSourceIndex)y!);
      }

      private sealed class _DescendingComparer : IComparer<SortValueWithSourceIndex>, IComparer {
        public int Compare(SortValueWithSourceIndex x, SortValueWithSourceIndex y) => -x.CompareTo(y);
        public int Compare(object x, object y) => this.Compare((SortValueWithSourceIndex)x!, (SortValueWithSourceIndex)y!);
      }

      public static IComparer AscendingComparer { get; } = new _AscendingComparer();
      public static IComparer DescendingComparer { get; } = new _DescendingComparer();

      #endregion
    }

    public static void InPlaceSort(IList<TValue> input, PropertyDescriptor prop, ListSortDirection direction) {
#if SUPPORTS_LINQ_PARAMETERS
      var getter = _propertyGetterCache.GetOrAdd(prop.Name, _CreatePropertyGetter);
#else
      var getter = prop.GetValue;
#endif

      var pairs = new SortValueWithSourceIndex[input.Count];
      for (var i = 0; i < pairs.Length; ++i)
        pairs[i] = new(i, input[i], getter(input[i]));

      Array.Sort(
        pairs,
        0,
        pairs.Length,
        direction == ListSortDirection.Ascending
          ? SortValueWithSourceIndex.AscendingComparer
          : SortValueWithSourceIndex.DescendingComparer
      );

      for (var i = 0; i < pairs.Length; ++i)
        input[i] = pairs[i].Value;
    }

#if SUPPORTS_LINQ_PARAMETERS
    private static readonly Dictionary<string, Func<TValue, object>> _propertyGetterCache = [];

    private static Func<TValue, object> _CreatePropertyGetter(string name) {
      var type = typeof(TValue);
      var property = type.GetProperty(name)!;
      var get = property.GetGetMethod()!;

      var parameter = Expression.Parameter(type);
      var callGetMethod = Expression.Call(parameter, get);
      var castToObject = Expression.Convert(callGetMethod, typeof(object));

      var result = (Func<TValue, object>)Expression.Lambda(castToObject, parameter).Compile();
      return result;
    }
#endif
  }

  #endregion

  public bool IsAutomaticallySorted { get; set; }

  private bool _isSorted;
  private ListSortDirection _sortDirection = ListSortDirection.Ascending;
  private PropertyDescriptor _sortProperty;

  public SortableBindingList() { }

  public SortableBindingList(IEnumerable<TValue> enumerable) {
    Against.ArgumentIsNull(enumerable);

    this.AddRange(enumerable);
  }

  public SortableBindingList(IList<TValue> list) =>
    // WARNING: do not use the base.ctor(IEnumerable<TValue>) as it marks the collections as read-only thus making it impossible to be sorted
    this.AddRange(list);

  public void Sort(string propertyName, ListSortDirection direction)
    => this.ApplySortCore(TypeDescriptor.GetProperties(typeof(TValue))[propertyName], direction);

  /// <summary>
  ///   Adds multiple elements to this instance.
  /// </summary>
  /// <param name="items">The items.</param>
  public void AddRange(IEnumerable<TValue> items) {
    Against.ArgumentIsNull(items);

    this.Overhaul(
      l => {
        foreach (var item in items)
          l.Add(item);
      }
    );
  }

  /// <summary>
  ///   Adds items in the range if they not exist.
  /// </summary>
  /// <param name="items">The items.</param>
  public void AddRangeIfNotExists(IEnumerable<TValue> items) {
    Against.ArgumentIsNull(items);

    this.AddRange(items.Distinct().Where(i => !this.Contains(i)));
  }

  /// <summary>
  ///   Removes multiple elements from this instance.
  /// </summary>
  /// <param name="items">The items.</param>
  public void RemoveRange(IEnumerable<TValue> items) {
    Against.ArgumentIsNull(items);

    this.Overhaul(
      l => {
        foreach (var item in items)
          l.Remove(item);
      }
    );
  }

  #region Overrides of BindingList<TValue>

  protected override void OnListChanged(ListChangedEventArgs e) {
    if (this._ReApplySortIfNeeded()) {
      base.OnListChanged(new(ListChangedType.Reset, -1));
      this.OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
    } else {
      base.OnListChanged(e);

      // TODO: only fire whats needed
      this.OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
    }
  }

  #endregion

  protected override bool SupportsSortingCore => true;
  protected override bool IsSortedCore => this._isSorted;
  protected override ListSortDirection SortDirectionCore => this._sortDirection;
  protected override PropertyDescriptor SortPropertyCore => this._sortProperty;

  protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
    this._sortDirection = direction;
    this._sortProperty = prop;
    this._isSorted = true;
    var list = this.Items;
    if (list.IsReadOnly || list.Count < 1)
      return;

    this.Overhaul(li => StableSorter.InPlaceSort(li, prop, direction));
  }

  protected override void RemoveSortCore() {
    this._sortDirection = ListSortDirection.Ascending;
    this._sortProperty = null;
    this._isSorted = false;
  }

  private bool _ReApplySortIfNeeded() {
    if (!(this.IsAutomaticallySorted && this.IsSortedCore))
      return false;

    var isRaisingListChangedEvents = this.RaiseListChangedEvents;
    try {
      this.RaiseListChangedEvents = false;
      this.ApplySortCore(this._sortProperty, this._sortDirection);
    } finally {
      this.RaiseListChangedEvents = isRaisingListChangedEvents;
    }

    return true;
  }

  #region Implementation of INotifyCollectionChanged

  public event NotifyCollectionChangedEventHandler CollectionChanged;

  #endregion

  protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => this.CollectionChanged?.Invoke(this, e);
}

public static partial class EnumerableExtensions {
  public static SortableBindingList<TItem> ToSortableBindingList<TItem>(this IEnumerable<TItem> @this) {
    Against.ThisIsNull(@this);

    return new(@this);
  }
}
