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
//
// requires extension: System.ComponentModel.SortComparer
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace System.ComponentModel {
  /// <inheritdoc />
  /// <summary>
  /// A BindingList which is sortable in DataGridViews.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public class SortableBindingList<TValue> : BindingList<TValue> {

    #region nested types

    /// <summary>
    /// Sorts a <c>TValue</c>-item-list by a given property in a certain direction using a stable sorting-mechanism
    /// </summary>
    private struct DefinedSortComparer : IEnumerable<TValue> {

      /// <summary>
      /// A value with its original array index for stable sorting
      /// </summary>
      private struct ValueWithSourceIndex {
        public readonly TValue value;
        public readonly int index;

        public ValueWithSourceIndex(int index, TValue value) {
          this.index = index;
          this.value = value;
        }
      }

      /// <summary>
      /// The enumerator for an <c>ValueWithIndex</c> array
      /// </summary>
      private sealed class Enumerator : IEnumerator<TValue> {

        private readonly ValueWithSourceIndex[] _items;
        private int _index = -1;
        public Enumerator(ValueWithSourceIndex[] items) => this._items = items;

        #region Implementation of IDisposable

        public void Dispose() { }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext() => ++this._index < this._items.Length;
        public void Reset() => this._index = -1;
        public TValue Current => this._items[this._index].value;
        object IEnumerator.Current => this.Current;

        #endregion
      }

      /// <summary>
      /// The comparer for a specific property of <c>TValue</c> and a given sort direction
      /// </summary>
      private sealed class Comparer : IComparer<ValueWithSourceIndex> {
        private readonly int _factor;
        private readonly MethodInfo _method;
        private readonly Func<TValue, object> _call;

        public Comparer(PropertyDescriptor property, ListSortDirection direction) {
          this._factor = direction == ListSortDirection.Ascending ? 1 : -1;

          // create call delegate
          var propertyInfo = typeof(TValue).GetProperty(property.Name, BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          var method = propertyInfo?.GetGetMethod(true);

          if (method == null) {
            this._call = _ReturnDefaultValue;
            return;
          }

          if (method.ReturnType.IsClass)
#if NET45
            this._call = (Func<TValue, object>)method.CreateDelegate(typeof(Func<TValue, object>));
#else
            this._call = value => method.Invoke(value, null);
#endif
          else {
            this._method = method;
            this._call = this._CallByMethodInfo;
          }

        }

        #region Implementation of IComparer<in SortableBindingList<TValue>.DefinedSortComparer.ValueWithSourceIndex>

        /// <inheritdoc/>
        public int Compare(ValueWithSourceIndex x, ValueWithSourceIndex y) {
          var result = _CompareValues(this._Call(x.value), this._Call(y.value));
          return (result != 0 ? result : x.index - y.index) * this._factor;
        }

        #endregion

        /// <summary>
        /// Returns a property value using MethodInfo (used for value return types)
        /// </summary>
        /// <param name="instance">The object whose property to return</param>
        /// <returns>The current property value</returns>
        private object _CallByMethodInfo(TValue instance) => this._method.Invoke(instance, null);

        /// <summary>
        /// Returns the default value (used when properties can not be found)
        /// </summary>
        /// <param name="_">Enforced to pass the object instance by the delegate; but not used here</param>
        /// <returns></returns>
        private static object _ReturnDefaultValue(TValue _) => default(TValue);

        /// <summary>
        /// Returns the property value for the given object instance
        /// </summary>
        /// <param name="instance">The object whose property to return</param>
        /// <returns>The current property value</returns>
        private object _Call(TValue instance) => instance == null ? null : this._call(instance);

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

      }

      private readonly ValueWithSourceIndex[] _items;

      private DefinedSortComparer(IComparer<ValueWithSourceIndex> comparer, ValueWithSourceIndex[] items) {
        this._items = items;
        Array.Sort(this._items, 0, items.Length, comparer);
      }

      public static IEnumerable<TValue> StableSort(IList<TValue> input, PropertyDescriptor prop, ListSortDirection direction) {
        var pairs = new ValueWithSourceIndex[input.Count];
        for (var i = 0; i < input.Count; ++i)
          pairs[i] = new ValueWithSourceIndex(i, input[i]);

        var comparer = new Comparer(prop, direction);
        return new DefinedSortComparer(comparer, pairs);
      }

      #region Implementation of IEnumerable

      public IEnumerator<TValue> GetEnumerator() => new Enumerator(this._items);
      IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

      #endregion

    }

    #endregion

    private bool _isSorted;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private PropertyDescriptor _sortProperty;

    private bool _blockEvents;

    public SortableBindingList() {
      base.ListChanged += (s, e) => {
        if (this._blockEvents || this._listChanged == null)
          return;

#pragma warning disable CC0067 // Do not call overrideable methods in ctor.
        this._listChanged(s, e);
#pragma warning restore CC0067 // Do not call overrideable methods in ctor.
      };
    }

    public SortableBindingList(IEnumerable<TValue> enumerable) {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      this.AddRange(enumerable);
    }

    public SortableBindingList(IList<TValue> list) {
      // WARNING: do not use the base.ctor(IEnumerable<TValue>) as it marks the collections as read-only thus making it impossible to be sorted
      this.AddRange(list);
    }

    private ListChangedEventHandler _listChanged;

    public new event ListChangedEventHandler ListChanged {
      add => this._listChanged += value;
      remove => this._listChanged -= value;
    }

    /// <summary>
    ///   Adds multiple elements to this instance.
    /// </summary>
    /// <param name="items">The items.</param>
    public void AddRange(IEnumerable<TValue> items) {
      if (items == null)
        throw new ArgumentNullException(nameof(items));

      this._blockEvents = true;
      try {
        foreach (var item in items)
          this.Add(item);
      } finally {
        this._blockEvents = false;
      }

      this._ReApplySortIfNeeded();
      this._listChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
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

      this._blockEvents = true;
      try {
        foreach (var item in items)
          this.Remove(item);
      } finally {
        this._blockEvents = false;
      }

      this._ReApplySortIfNeeded();
      this._listChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    protected override bool SupportsSortingCore => true;
    protected override bool IsSortedCore => this._isSorted;
    protected override ListSortDirection SortDirectionCore => this._sortDirection;
    protected override PropertyDescriptor SortPropertyCore => this._sortProperty;

    protected void ApplySortCore() => this.ApplySortCore(this._sortProperty, this._sortDirection);

    private WeakReference _lastListRef;
    private Action<IEnumerable<TValue>> _lastAddRangeDelegate;

    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
      this._sortDirection = direction;
      this._sortProperty = prop;
      this._isSorted = true;
      var listRef = this.Items;
      if (this.Items.IsReadOnly)
        return;

      var items = DefinedSortComparer.StableSort(listRef, prop, direction);
      listRef.Clear();

      var addRangeDelegate = this._lastAddRangeDelegate;
      if (this._lastListRef == null || !ReferenceEquals(this._lastListRef.Target, listRef) || !this._lastListRef.IsAlive) {
        this._lastListRef = new WeakReference(listRef);
        var mi = listRef.GetType().GetMethod(nameof(List<object>.AddRange));
        this._lastAddRangeDelegate =
          addRangeDelegate =
            mi == null
              ? null
#if NET45
              : (Action<IEnumerable<TValue>>)mi.CreateDelegate(typeof(Action<IEnumerable<TValue>>), listRef)
#else
              : (Action<IEnumerable<TValue>>)(values => mi.Invoke(listRef, new object[] { values }))
#endif
          ;
      }

      if (addRangeDelegate != null)
        addRangeDelegate(items);
      else
        foreach (var item in items)
          listRef.Add(item);

      this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    /// <summary>
    /// Reapplies sort core if needed.
    /// </summary>
    private void _ReApplySortIfNeeded() {
      if (this.IsSortedCore)
        this.ApplySortCore();
    }

  }

  // ReSharper disable once PartialTypeWithSinglePart
  internal static partial class EnumerableExtensions {
    public static SortableBindingList<TItem> ToSortableBindingList<TItem>(this IEnumerable<TItem> @this) {
      if (@this == null)
        throw new NullReferenceException();

      return new SortableBindingList<TItem>(@this);
    }
  }
}
