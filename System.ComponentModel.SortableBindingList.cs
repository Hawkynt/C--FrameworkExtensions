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
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.ComponentModel {
  public class SortableBindingList<TValue> : BindingList<TValue> {
    private bool _isSorted = false;
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private PropertyDescriptor _sortProperty;

    private bool _blockEvents;

    public SortableBindingList() {
      base.ListChanged += (s, e) => {
        if (this._blockEvents || this._listChanged == null)
          return;

        this._listChanged(s, e);
      };
    }

    public SortableBindingList(IEnumerable<TValue> enumerable) {
      this.AddRange(enumerable);
    }

    public SortableBindingList(List<TValue> list)
      : base(list) { }

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
      Contract.Requires(items != null);
      this._blockEvents = true;
      foreach (var item in items)
        this.Add(item);
      this._blockEvents = false;

      this._ReApplySortIfNeeded();

      if (this._listChanged != null)
        this._listChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    private void _ReApplySortIfNeeded() {
      if (this.IsSortedCore)
        this.ApplySortCore();
    }

    /// <summary>
    ///   Removes multiple elements from this instance.
    /// </summary>
    /// <param name="items">The items.</param>
    public void RemoveRange(IEnumerable<TValue> items) {
      Contract.Requires(items != null);
      this._blockEvents = true;
      foreach (var item in items)
        this.Remove(item);
      this._blockEvents = false;

      this._ReApplySortIfNeeded();
      if (this._listChanged != null)
        this._listChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    protected override bool SupportsSortingCore {
      get { return true; }
    }

    protected override bool IsSortedCore {
      get { return this._isSorted; }
    }

    protected override ListSortDirection SortDirectionCore {
      get { return this._sortDirection; }
    }

    protected override PropertyDescriptor SortPropertyCore {
      get { return this._sortProperty; }
    }

    protected void ApplySortCore() {
      this.ApplySortCore(this._sortProperty, this._sortDirection);
    }

    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
      this._sortDirection = direction;
      this._sortProperty = prop;
      this._isSorted = true;
      var listRef = this.Items as List<TValue>;
      if (listRef == null)
        return;

      IComparer<TValue> comparer = new SortComparer<TValue>(prop, direction);

      // stable sorting
      var pairs = listRef.Select((v, i) => Tuple.Create(v, i)).ToList();
      pairs.Sort((x, y) => {
        var result = comparer.Compare(x.Item1, y.Item1);
        return (result != 0 ? result : x.Item2 - y.Item2);
      });
      listRef.Clear();
      listRef.AddRange(pairs.Select(p => p.Item1));

      // unstable sorting
      //listRef.Sort(comparer);

      this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }
  }

  internal static class libSortableBindingListSatelliteExtensions {
    public static SortableBindingList<TItem> ToSortableBindingList<TItem>(this IEnumerable<TItem> This) {
      Contract.Requires(This != null);
      return (new SortableBindingList<TItem>(This));
    }
  }
}
