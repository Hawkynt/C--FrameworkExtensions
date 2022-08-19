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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETSTANDARD || NETCOREAPP
#define SUPPORTS_LINQ_PARAMETERS
#endif

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

#if SUPPORTS_LINQ_PARAMETERS
using System.Linq.Expressions;
#endif

// ReSharper disable UnusedMember.Global

namespace System.ComponentModel {
  /// <inheritdoc />
  /// <summary>
  /// A BindingList which is sortable in DataGridViews.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class SortableBindingList<TValue> : BindingList<TValue>,INotifyCollectionChanged {

#region nested types

    private static class StableSorter {

      private sealed class SortValueWithSourceIndex : IComparable<SortValueWithSourceIndex> {
        private readonly int _index;
        public readonly TValue Value;
        private readonly object _sortValue;

        public SortValueWithSourceIndex(int index, TValue value, object sortValue) {
          this._index = index;
          this.Value = value;
          this._sortValue = sortValue;
        }

#region Relational members

        public int CompareTo(SortValueWithSourceIndex other) {
          var a = this._sortValue;
          var b = other._sortValue;
          var result = _CompareValues(a, b);
          return result == 0 ? this._index.CompareTo(other._index) : result;
        }

        /// <summary>
        /// Compares two arbitrary values of the same type with another
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

        private class _AscendingComparer : IComparer<SortValueWithSourceIndex>, IComparer {
          public int Compare(SortValueWithSourceIndex x, SortValueWithSourceIndex y) => x.CompareTo(y);
          public int Compare(object x, object y) => this.Compare((SortValueWithSourceIndex)x!, (SortValueWithSourceIndex)y!);
        }

        private class _DescendingComparer : IComparer<SortValueWithSourceIndex>, IComparer {
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
          pairs[i] = new SortValueWithSourceIndex(i, input[i], getter(input[i]));

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
      private static readonly Dictionary<string, Func<TValue, object>> _propertyGetterCache = new();

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

    private bool _isSorted;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private PropertyDescriptor _sortProperty;
    
    public SortableBindingList() { }

    public SortableBindingList(IEnumerable<TValue> enumerable) {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      this.AddRange(enumerable);
    }

    public SortableBindingList(IList<TValue> list) {
      // WARNING: do not use the base.ctor(IEnumerable<TValue>) as it marks the collections as read-only thus making it impossible to be sorted
      this.AddRange(list);
    }

    public void Sort(string propertyName, ListSortDirection direction)
      => this.ApplySortCore(TypeDescriptor.GetProperties(typeof(TValue))[propertyName], direction)
    ;

    /// <summary>
    ///   Adds multiple elements to this instance.
    /// </summary>
    /// <param name="items">The items.</param>
    public void AddRange(IEnumerable<TValue> items) {
      if (items == null)
        throw new ArgumentNullException(nameof(items));

      this.Overhaul(l => {
        foreach (var item in items)
          l.Add(item);
      });
    }

    /// <summary>
    /// Adds items in the range if they not exist.
    /// </summary>
    /// <param name="items">The items.</param>
    public void AddRangeIfNotExists(IEnumerable<TValue> items) {
      if (items == null)
        throw new ArgumentNullException(nameof(items));

      this.AddRange(items.Distinct().Where(i => !this.Contains(i)));
    }

    /// <summary>
    ///   Removes multiple elements from this instance.
    /// </summary>
    /// <param name="items">The items.</param>
    public void RemoveRange(IEnumerable<TValue> items) {
      if (items == null)
        throw new ArgumentNullException(nameof(items));

      this.Overhaul(l => {
        foreach (var item in items)
          l.Remove(item);
      });
    }

    #region Overrides of BindingList<TValue>

    protected override void OnListChanged(ListChangedEventArgs e) {
      if (this._ReApplySortIfNeeded()) {
        base.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      } else {
        base.OnListChanged(e);

        // TODO: only fire whats needed
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
      if (list.IsReadOnly || list.Count<1)
        return;

      this.Overhaul(li => StableSorter.InPlaceSort(li, prop, direction));
    }

    private bool _ReApplySortIfNeeded() {
      if (!this.IsSortedCore)
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

  // ReSharper disable once PartialTypeWithSinglePart
#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class EnumerableExtensions {
    public static SortableBindingList<TItem> ToSortableBindingList<TItem>(this IEnumerable<TItem> @this) {
      if (@this == null)
        throw new NullReferenceException();

      return new SortableBindingList<TItem>(@this);
    }
  }
}
