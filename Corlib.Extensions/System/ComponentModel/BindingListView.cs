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

#if !NET20_OR_GREATER || NET40_OR_GREATER

using System.Collections.Generic;
using System.Linq;

namespace System.ComponentModel;

/// <summary>
///   A list view for applying filtering and stuff.
/// </summary>
/// <typeparam name="TItem">The type of the item.</typeparam>
public class BindingListView<TItem> : SortableBindingList<TItem> {
  private bool _isFiltering;
  private Predicate<TItem> _filterPredicate;
  private bool _ignoreDataSourceEvents;

  public BindingListView(BindingList<TItem> baseList) {
    this.DataSource = baseList ?? throw new ArgumentNullException(nameof(baseList));
    this._Hook(baseList);
  }

  private void _Hook(IBindingList baseList) 
    => baseList.ListChanged += (_, _) 
      => {
    if (this._ignoreDataSourceEvents)
      return;

    this._ApplyFiltering();
  };

  public new void Add(TItem item) => this.DataSource.Add(item);

  /// <summary>
  ///   Adds the range.
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
  public BindingList<TItem> DataSource { get; }

  /// <summary>
  ///   Gets or sets a value indicating whether this instance is filtering.
  /// </summary>
  /// <value>
  ///   <c>true</c> if this instance is filtering; otherwise, <c>false</c>.
  /// </value>
  public bool IsFiltering {
    get => this._isFiltering;
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
  public Predicate<TItem> FilterPredicate {
    get => this._filterPredicate;
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
    var results = !isFiltering || filter == null ? this.DataSource : this.DataSource.Where(item => filter(item));

    // FIXME: calls this in a GUI thread
    base.AddRange(results);

    this.RaiseListChangedEvents = ignoreEvents;
    this.OnListChanged(new(ListChangedType.Reset, 0));
  }
}

#endif
