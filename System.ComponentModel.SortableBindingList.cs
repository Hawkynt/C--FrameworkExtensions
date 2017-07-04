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
//
// requires extension: System.ComponentModel.SortComparer
//

using System.Collections.Generic;
#if NETFX_4
using System.Diagnostics.Contracts;
#endif
using System.Linq;

namespace System.ComponentModel {
  /// <summary>
  /// A BindingList which is sortable in DataGridViews.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public class SortableBindingList<TValue> : BindingList<TValue> {
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
#if NETFX_4
      Contract.Requires(enumerable != null);
#endif
      this.AddRange(enumerable);
    }

    public SortableBindingList(IList<TValue> list) {
      // WARNING: do not use the base.ctor(IEnumerable<TValue>) as it marks the collections as read-only thus making it impossible to be sorted
      this.AddRange(list);
    }

    private ListChangedEventHandler _listChanged;

    public new event ListChangedEventHandler ListChanged {
      add { this._listChanged += value; }
      remove { this._listChanged -= value; }
    }

    /// <summary>
    ///   Adds multiple elements to this instance.
    /// </summary>
    /// <param name="items">The items.</param>
    public void AddRange(IEnumerable<TValue> items) {
#if NETFX_4
      Contract.Requires(items != null);
#endif
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
    public void AddRangeIfNotExists(IEnumerable<TValue> items) => this.AddRange(items.Distinct().Where(i => !this.Contains(i)));

    /// <summary>
    ///   Removes multiple elements from this instance.
    /// </summary>
    /// <param name="items">The items.</param>
    public void RemoveRange(IEnumerable<TValue> items) {
#if NETFX_4
      Contract.Requires(items != null);
#endif
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

    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
      this._sortDirection = direction;
      this._sortProperty = prop;
      this._isSorted = true;
      var listRef = this.Items;
      if (this.Items.IsReadOnly)
        return;

      IComparer<TValue> comparer = new SortComparer<TValue>(prop, direction);

      // stable sorting
      var pairs = listRef.Select((v, i) => new { v, i }).ToList();
      pairs.Sort((x, y) => {
        var result = comparer.Compare(x.v, y.v);
        return result != 0 ? result : x.i - y.i;
      });
      listRef.Clear();

      var mi = listRef.GetType().GetMethod(nameof(List<object>.AddRange));
      if (mi != null) {
        mi.Invoke(listRef, new object[] { pairs.Select(p => p.v) });
      } else {
        foreach (var pair in pairs.Select(p => p.v))
          listRef.Add(pair);
      }

      //((dynamic)listRef).AddRange(pairs.Select(p => p.v));

      // unstable sorting
      //listRef.Sort(comparer);

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
      if (@this == null) throw new NullReferenceException();
      return new SortableBindingList<TItem>(@this);
    }
  }
}
