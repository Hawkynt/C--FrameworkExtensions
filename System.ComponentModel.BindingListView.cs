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
  /// <summary>
  ///   A list view for applying filtering and stuff.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class BindingListView<TItem> : SortableBindingList<TItem> {
    /// <summary>
    ///   The underlying base list.
    /// </summary>
    private readonly BindingList<TItem> _baseList;

    private bool _isFiltering;
    private Func<TItem, bool> _filterPredicate;
    private bool _ignoreDataSourceEvents;

    public BindingListView(BindingList<TItem> baseList) {
      Contract.Requires(baseList != null);
      this._baseList = baseList;
      this._Hook(baseList);
    }

    private void _Hook(BindingList<TItem> baseList) {
      baseList.ListChanged += (s, e) => {
        if (this._ignoreDataSourceEvents)
          return;

        switch (e.ListChangedType) {
          default:
          {
            this._ApplyFiltering();
            break;
          }
        }

      };
    }

    /// <summary>
    /// Adds the range.
    /// </summary>
    /// <param name="items">The items.</param>
    public new void AddRange(IEnumerable<TItem> items) {
      this._ignoreDataSourceEvents = true;
      foreach (var item in items)
        this.DataSource.Add(item);
      this._ignoreDataSourceEvents = false;
      this._ApplyFiltering();
    }

    /// <summary>
    ///   Gets the data source.
    /// </summary>
    public BindingList<TItem> DataSource => this._baseList;

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is filtering.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is filtering; otherwise, <c>false</c>.
    /// </value>
    public bool IsFiltering {
      get { return this._isFiltering; }
      set {
        this._isFiltering = value;
        this._ApplyFiltering();
      }
    }

    /// <summary>
    ///   Gets or sets the filter predicate.
    /// </summary>
    /// <value>
    ///   The filter predicate.
    /// </value>
    public Func<TItem, bool> FilterPredicate {
      get { return this._filterPredicate; }
      set {
        this._filterPredicate = value;
        this._ApplyFiltering();
      }
    }

    /// <summary>
    ///   Re-applies filtering to all elements.
    /// </summary>
    private void _ApplyFiltering() {
      var ignoreEvents = this.RaiseListChangedEvents;
      this.RaiseListChangedEvents = false;

      this.Clear();
      var isFiltering = this.IsFiltering;
      var filter = this.FilterPredicate;
      var results = this._baseList.Where(item => !isFiltering || (filter == null) || filter(item));
      // FIXME: calls this in a GUI thread
      base.AddRange(results);

      this.RaiseListChangedEvents = ignoreEvents;
      this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, 0));
    }
  }
}
