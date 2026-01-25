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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Guard;

namespace System.Windows.Forms;

#region C# 14 Extension Property for DataSource

/// <summary>
/// C# 14 extension property providing DataSource support for ListView.
/// </summary>
public static class ListViewExtensionProperties {
  private static readonly ConditionalWeakTable<ListView, object> _DataSources = new();
  private static readonly ConditionalWeakTable<ListView, Type> _DataSourceTypes = new();

  /// <summary>
  /// Gets the data source bound to this ListView.
  /// </summary>
  public static object GetDataSource(this ListView @this) {
    Against.ThisIsNull(@this);
    return _DataSources.TryGetValue(@this, out var data) ? data : null;
  }

  /// <summary>
  /// Sets the data source for this ListView, auto-configuring columns and populating items from attributes.
  /// </summary>
  public static void SetDataSource(this ListView @this, object value) {
    Against.ThisIsNull(@this);

    // Store or remove data source reference
    _DataSources.Remove(@this);
    _DataSourceTypes.Remove(@this);
    if (value != null) {
      _DataSources.Add(@this, value);

      // Determine element type
      var elementType = _GetElementType(value);
      if (elementType != null)
        _DataSourceTypes.Add(@this, elementType);
    }

    // Configure columns from attributes if not already set
    if (value != null && @this.Columns.Count == 0) {
      var elementType = _GetElementType(value);
      if (elementType != null)
        @this.ConfigureColumnsFromType(elementType);
    }

    // Populate items
    using (@this.PauseUpdates()) {
      @this.Items.Clear();
      if (value is IEnumerable enumerable)
        foreach (var item in enumerable)
          @this._AddItemFromData(item);
    }
  }

  private static Type _GetElementType(object data) {
    var type = data.GetType();

    // Check for generic IEnumerable<T>
    var genericEnumerable = type.GetInterfaces()
      .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    if (genericEnumerable != null)
      return genericEnumerable.GetGenericArguments()[0];

    // Check for array
    if (type.IsArray)
      return type.GetElementType();

    return null;
  }

  internal static Type GetDataSourceType(this ListView @this)
    => _DataSourceTypes.TryGetValue(@this, out var type) ? type : null;
}

#endregion

/// <summary>
/// Extension methods for <see cref="ListView"/> providing attribute-based data binding,
/// styling, and various utility methods.
/// </summary>
public static partial class ListViewExtensions {

  #region Suspended Update Token

  /// <summary>
  /// Token returned by <see cref="PauseUpdates"/> to restore updates when disposed.
  /// </summary>
  public interface ISuspendedUpdateToken : IDisposable { }

  private sealed class SuspendedUpdateToken(ListView listView) : ISuspendedUpdateToken {
    private bool _isDisposed;

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      listView.EndUpdate();
    }
  }

  /// <summary>
  /// Pauses updates to the ListView until the returned token is disposed.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <returns>A token that restores updates when disposed.</returns>
  /// <example>
  /// <code>
  /// using (listView.PauseUpdates()) {
  ///   // Add many items without flickering
  ///   foreach (var item in items)
  ///     listView.Items.Add(item);
  /// }
  /// </code>
  /// </example>
  public static ISuspendedUpdateToken PauseUpdates(this ListView @this) {
    Against.ThisIsNull(@this);
    @this.BeginUpdate();
    return new SuspendedUpdateToken(@this);
  }

  #endregion

  #region EnableExtendedAttributes

  private static readonly ConditionalWeakTable<ListView, object> _ExtendedAttributesEnabled = new();

  /// <summary>
  /// Enables attribute-based rendering for this ListView.
  /// This hooks owner-draw events to apply <see cref="ListItemStyleAttribute"/>,
  /// <see cref="ListViewColumnColorAttribute"/>, and <see cref="ListViewRepeatedImageAttribute"/>.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <example>
  /// <code>
  /// listView.EnableExtendedAttributes();
  /// listView.SetDataSource(myData);
  /// </code>
  /// </example>
  public static void EnableExtendedAttributes(this ListView @this) {
    Against.ThisIsNull(@this);

    // Check if already enabled
    if (_ExtendedAttributesEnabled.TryGetValue(@this, out _))
      return;

    _ExtendedAttributesEnabled.Add(@this, new object());

    // Unsubscribe first to avoid duplicates
    @this.DrawItem -= _ListView_DrawItem;
    @this.DrawSubItem -= _ListView_DrawSubItem;
    @this.DrawColumnHeader -= _ListView_DrawColumnHeader;

    // Subscribe
    @this.DrawItem += _ListView_DrawItem;
    @this.DrawSubItem += _ListView_DrawSubItem;
    @this.DrawColumnHeader += _ListView_DrawColumnHeader;

    // Enable owner draw for custom rendering
    @this.OwnerDraw = true;
    @this.View = View.Details;
  }

  private static void _ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
    e.DrawDefault = true;
  }

  private static void _ListView_DrawItem(object sender, DrawListViewItemEventArgs e) {
    var data = e.Item.Tag;
    if (data == null) {
      e.DrawDefault = true;
      return;
    }

    var type = data.GetType();

    // Apply item-level styles
    var styleAttrs = ListControlExtensions.GetStyleAttributes(type);
    foreach (var attr in styleAttrs)
      if (attr.IsEnabled(data)) {
        if (attr.GetForeColor(data) is { } fg)
          e.Item.ForeColor = fg;
        if (attr.GetBackColor(data) is { } bg)
          e.Item.BackColor = bg;
      }

    // Don't use DrawDefault - we need DrawSubItem to be called for each sub-item
    // Draw selection/focus background manually
    if ((e.State & ListViewItemStates.Selected) != 0) {
      using var brush = new SolidBrush(SystemColors.Highlight);
      e.Graphics.FillRectangle(brush, e.Bounds);
    }
  }

  private static void _ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) {
    var listView = (ListView)sender;
    var data = e.Item.Tag;

    if (data == null) {
      e.DrawDefault = true;
      return;
    }

    var type = data.GetType();

    // Get property name from column Tag (set by ConfigureColumnsFromType)
    string propertyName = null;
    if (e.ColumnIndex >= 0 && e.ColumnIndex < listView.Columns.Count)
      propertyName = listView.Columns[e.ColumnIndex].Tag as string;

    // If no Tag, try to match by column index to property order
    if (propertyName == null) {
      var columnProperties = ListControlExtensions.GetColumnProperties(type);
      if (columnProperties != null && e.ColumnIndex < columnProperties.Length)
        propertyName = columnProperties[e.ColumnIndex].Name;
    }

    if (propertyName == null) {
      e.DrawDefault = true;
      return;
    }

    // Get the property by name
    var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

    // Check for repeated image attribute
    if (property != null) {
      var repeatedImageAttr = ListControlExtensions.GetRepeatedImageAttribute(type, propertyName);
      if (repeatedImageAttr != null) {
        _DrawRepeatedImages(listView, e, data, property, repeatedImageAttr);
        return;
      }

      // Check for column color attributes
      var columnColorAttrs = ListControlExtensions.GetColumnColorAttributes(type, propertyName);
      foreach (var attr in columnColorAttrs)
        if (attr.IsEnabled(data)) {
          if (attr.GetForeColor(data) is { } fg)
            e.SubItem.ForeColor = fg;
          if (attr.GetBackColor(data) is { } bg)
            e.SubItem.BackColor = bg;
        }
    }

    e.DrawDefault = true;
  }

  private static void _DrawRepeatedImages(ListView listView, DrawListViewSubItemEventArgs e, object data, PropertyInfo property, ListViewRepeatedImageAttribute attr) {
    var image = attr.GetImage(data);
    if (image == null) {
      e.DrawDefault = true;
      return;
    }

    // Get the numeric value from the property as double to preserve fractional part
    var rawValue = property.GetValue(data);
    var numericValue = rawValue switch {
      int i => i,
      long l => l,
      short s => s,
      byte b => b,
      sbyte sb => sb,
      double d => d,
      float f => f,
      decimal m => (double)m,
      _ => 0.0
    };

    // Check for negative values - use grayscale
    var isNegative = numericValue < 0;
    numericValue = Math.Abs(numericValue);

    // Clamp to max count
    numericValue = Math.Min(numericValue, attr.MaxCount);

    var fullCount = (int)numericValue;
    var fractionalPart = numericValue - fullCount;

    // Determine background color (handle selection)
    var backColor = e.Item.Selected ? SystemColors.Highlight : listView.BackColor;

    // Draw background
    using (var brush = new SolidBrush(backColor))
      e.Graphics.FillRectangle(brush, e.Bounds);

    // Convert to grayscale if negative
    var imageToDraw = isNegative ? _ConvertToGrayscale(image) : image;

    try {
      // Draw the full images
      var x = e.Bounds.X + 2;
      var y = e.Bounds.Y + (e.Bounds.Height - imageToDraw.Height) / 2;

      for (var i = 0; i < fullCount; ++i) {
        e.Graphics.DrawImage(imageToDraw, x, y);
        x += imageToDraw.Width + 2;
      }

      // Draw partial image for fractional part
      if (fractionalPart > 0.01) {
        var partialWidth = (int)(imageToDraw.Width * fractionalPart);
        if (partialWidth > 0) {
          var srcRect = new Rectangle(0, 0, partialWidth, imageToDraw.Height);
          var destRect = new Rectangle(x, y, partialWidth, imageToDraw.Height);
          e.Graphics.DrawImage(imageToDraw, destRect, srcRect, GraphicsUnit.Pixel);
        }
      }
    } finally {
      // Dispose grayscale image if we created one
      if (isNegative && imageToDraw != image)
        imageToDraw.Dispose();
    }
  }

  private static Image _ConvertToGrayscale(Image original) {
    var grayscale = new Bitmap(original.Width, original.Height);
    using (var g = Graphics.FromImage(grayscale)) {
      // Grayscale color matrix
      var colorMatrix = new System.Drawing.Imaging.ColorMatrix(new[] {
        new[] { 0.3f, 0.3f, 0.3f, 0, 0 },
        new[] { 0.59f, 0.59f, 0.59f, 0, 0 },
        new[] { 0.11f, 0.11f, 0.11f, 0, 0 },
        new[] { 0f, 0f, 0f, 1f, 0 },
        new[] { 0f, 0f, 0f, 0f, 1f }
      });

      using var attributes = new System.Drawing.Imaging.ImageAttributes();
      attributes.SetColorMatrix(colorMatrix);

      g.DrawImage(
        original,
        new Rectangle(0, 0, original.Width, original.Height),
        0, 0, original.Width, original.Height,
        GraphicsUnit.Pixel,
        attributes
      );
    }
    return grayscale;
  }

  #endregion

  #region Column Configuration

  /// <summary>
  /// Configures columns from the properties of a type that have <see cref="ListViewColumnAttribute"/>.
  /// </summary>
  /// <typeparam name="T">The type to configure columns from.</typeparam>
  /// <param name="this">This ListView.</param>
  public static void ConfigureColumnsFromType<T>(this ListView @this)
    => @this.ConfigureColumnsFromType(typeof(T));

  /// <summary>
  /// Configures columns from the properties of a type that have <see cref="ListViewColumnAttribute"/>.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="type">The type to configure columns from.</param>
  public static void ConfigureColumnsFromType(this ListView @this, Type type) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(type);

    var properties = ListControlExtensions.GetColumnProperties(type);
    @this.Columns.Clear();

    foreach (var property in properties) {
      var attr = property.GetCustomAttribute<ListViewColumnAttribute>();
      if (attr == null || !attr.Visible)
        continue;

      var header = attr.HeaderText ?? property.Name;
      var width = attr.Width;
      var alignment = attr.Alignment;

      var column = @this.Columns.Add(header, width, alignment);
      column.Tag = property.Name;
    }
  }

  /// <summary>
  /// Adds a column to the ListView.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="text">The column header text.</param>
  /// <param name="width">The column width (-1 for auto-size).</param>
  /// <param name="alignment">The column alignment.</param>
  /// <returns>The created column header.</returns>
  public static ColumnHeader AddColumn(this ListView @this, string text, int width = -1, HorizontalAlignment alignment = HorizontalAlignment.Left) {
    Against.ThisIsNull(@this);
    return @this.Columns.Add(text, width, alignment);
  }

  /// <summary>
  /// Adds multiple columns to the ListView.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="headers">The column header texts.</param>
  public static void AddColumns(this ListView @this, params string[] headers) {
    Against.ThisIsNull(@this);
    foreach (var header in headers)
      @this.Columns.Add(header);
  }

  /// <summary>
  /// Auto-resizes all columns to fit their content.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void AutoResizeAllColumns(this ListView @this) {
    Against.ThisIsNull(@this);
    @this.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
  }

  #endregion

  #region Item Management

  internal static void _AddItemFromData(this ListView @this, object data) {
    if (data == null)
      return;

    var type = data.GetType();
    var properties = ListControlExtensions.GetColumnProperties(type);

    ListViewItem item;
    if (properties.Length > 0) {
      var firstProp = properties[0];
      var columnAttr = ListControlExtensions.GetColumnAttribute(type, firstProp.Name);
      var text = ListControlExtensions.GetDisplayValue(data, firstProp, columnAttr);
      item = new ListViewItem(text) { Tag = data };

      // Add sub-items for remaining columns
      for (var i = 1; i < properties.Length; ++i) {
        var prop = properties[i];
        var attr = ListControlExtensions.GetColumnAttribute(type, prop.Name);
        if (attr != null && !attr.Visible)
          continue;

        var subText = ListControlExtensions.GetDisplayValue(data, prop, attr);
        item.SubItems.Add(subText);
      }
    } else {
      // No column attributes, use ToString()
      item = new ListViewItem(data.ToString()) { Tag = data };
    }

    // Apply image from ListItemImageAttribute
    var imageAttr = ListControlExtensions.GetImageAttribute(type);
    if (imageAttr != null) {
      var imageKey = imageAttr.GetImageKey(data);
      if (imageKey != null)
        item.ImageKey = imageKey;
      else {
        var imageIndex = imageAttr.GetImageIndex(data);
        if (imageIndex >= 0)
          item.ImageIndex = imageIndex;
      }
    }

    @this.Items.Add(item);
  }

  /// <summary>
  /// Adds an item to the ListView.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="text">The item text.</param>
  /// <returns>The created ListViewItem.</returns>
  public static ListViewItem AddItem(this ListView @this, string text) {
    Against.ThisIsNull(@this);
    return @this.Items.Add(text);
  }

  /// <summary>
  /// Adds an item with sub-items to the ListView.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="text">The item text.</param>
  /// <param name="subItems">The sub-item texts.</param>
  /// <returns>The created ListViewItem.</returns>
  public static ListViewItem AddItem(this ListView @this, string text, params string[] subItems) {
    Against.ThisIsNull(@this);
    var item = new ListViewItem(text);
    foreach (var subItem in subItems)
      item.SubItems.Add(subItem);
    return @this.Items.Add(item);
  }

  /// <summary>
  /// Adds an item from a data object using attributes.
  /// </summary>
  /// <typeparam name="T">The type of the data object.</typeparam>
  /// <param name="this">This ListView.</param>
  /// <param name="data">The data object.</param>
  /// <returns>The created ListViewItem.</returns>
  public static ListViewItem AddItem<T>(this ListView @this, T data) where T : class {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(data);

    var countBefore = @this.Items.Count;
    @this._AddItemFromData(data);
    return @this.Items.Count > countBefore ? @this.Items[countBefore] : null;
  }

  /// <summary>
  /// Adds multiple items from data objects.
  /// </summary>
  /// <typeparam name="T">The type of the data objects.</typeparam>
  /// <param name="this">This ListView.</param>
  /// <param name="dataItems">The data objects.</param>
  public static void AddItems<T>(this ListView @this, IEnumerable<T> dataItems) where T : class {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(dataItems);

    using (@this.PauseUpdates())
      foreach (var data in dataItems)
        @this._AddItemFromData(data);
  }

  /// <summary>
  /// Removes items matching a predicate.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="predicate">The predicate to match.</param>
  /// <returns>The number of items removed.</returns>
  public static int RemoveWhere(this ListView @this, Predicate<ListViewItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    var toRemove = @this.Items.Cast<ListViewItem>().Where(i => predicate(i)).ToList();
    using (@this.PauseUpdates())
      foreach (var item in toRemove)
        @this.Items.Remove(item);

    return toRemove.Count;
  }

  /// <summary>
  /// Finds items matching a predicate.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="predicate">The predicate to match.</param>
  /// <returns>The matching items.</returns>
  public static IEnumerable<ListViewItem> FindItems(this ListView @this, Predicate<ListViewItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);
    return @this.Items.Cast<ListViewItem>().Where(i => predicate(i));
  }

  /// <summary>
  /// Finds items by their Tag value.
  /// </summary>
  /// <typeparam name="T">The type of the tag.</typeparam>
  /// <param name="this">This ListView.</param>
  /// <param name="tag">The tag value to find.</param>
  /// <returns>The matching items.</returns>
  public static IEnumerable<ListViewItem> FindItemsByTag<T>(this ListView @this, T tag) {
    Against.ThisIsNull(@this);
    return @this.Items.Cast<ListViewItem>().Where(i => Equals(i.Tag, tag));
  }

  /// <summary>
  /// Clears all items and disposes any IDisposable tags.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void ClearAndDispose(this ListView @this) {
    Against.ThisIsNull(@this);

    foreach (ListViewItem item in @this.Items)
      if (item.Tag is IDisposable disposable)
        disposable.Dispose();

    @this.Items.Clear();
  }

  /// <summary>
  /// Gets all items in the ListView.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <returns>All items.</returns>
  public static IEnumerable<ListViewItem> GetAllItems(this ListView @this) {
    Against.ThisIsNull(@this);
    return @this.Items.Cast<ListViewItem>();
  }

  /// <summary>
  /// Gets the bound data objects from all items.
  /// </summary>
  /// <typeparam name="T">The type of the data objects.</typeparam>
  /// <param name="this">This ListView.</param>
  /// <returns>The bound data objects.</returns>
  public static IEnumerable<T> GetBoundData<T>(this ListView @this) where T : class {
    Against.ThisIsNull(@this);
    return @this.Items.Cast<ListViewItem>().Select(i => i.Tag as T).Where(t => t != null);
  }

  #endregion

  #region Selection

  /// <summary>
  /// Gets the selected items.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <returns>The selected items.</returns>
  public static IEnumerable<ListViewItem> GetSelectedItems(this ListView @this) {
    Against.ThisIsNull(@this);
    return @this.SelectedItems.Cast<ListViewItem>();
  }

  /// <summary>
  /// Gets the bound data from selected items.
  /// </summary>
  /// <typeparam name="T">The type of the data objects.</typeparam>
  /// <param name="this">This ListView.</param>
  /// <returns>The bound data objects from selected items.</returns>
  public static IEnumerable<T> GetSelectedItems<T>(this ListView @this) where T : class {
    Against.ThisIsNull(@this);
    return @this.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag as T).Where(t => t != null);
  }

  /// <summary>
  /// Gets the checked items.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <returns>The checked items.</returns>
  public static IEnumerable<ListViewItem> GetCheckedItems(this ListView @this) {
    Against.ThisIsNull(@this);
    return @this.CheckedItems.Cast<ListViewItem>();
  }

  /// <summary>
  /// Gets the bound data from checked items.
  /// </summary>
  /// <typeparam name="T">The type of the data objects.</typeparam>
  /// <param name="this">This ListView.</param>
  /// <returns>The bound data objects from checked items.</returns>
  public static IEnumerable<T> GetCheckedItems<T>(this ListView @this) where T : class {
    Against.ThisIsNull(@this);
    return @this.CheckedItems.Cast<ListViewItem>().Select(i => i.Tag as T).Where(t => t != null);
  }

  /// <summary>
  /// Selects all items.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void SelectAll(this ListView @this) {
    Against.ThisIsNull(@this);
    using (@this.PauseUpdates())
      foreach (ListViewItem item in @this.Items)
        item.Selected = true;
  }

  /// <summary>
  /// Deselects all items.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void SelectNone(this ListView @this) {
    Against.ThisIsNull(@this);
    using (@this.PauseUpdates())
      foreach (ListViewItem item in @this.Items)
        item.Selected = false;
  }

  /// <summary>
  /// Inverts the selection.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void InvertSelection(this ListView @this) {
    Against.ThisIsNull(@this);
    using (@this.PauseUpdates())
      foreach (ListViewItem item in @this.Items)
        item.Selected = !item.Selected;
  }

  /// <summary>
  /// Checks all items.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void CheckAll(this ListView @this) {
    Against.ThisIsNull(@this);
    using (@this.PauseUpdates())
      foreach (ListViewItem item in @this.Items)
        item.Checked = true;
  }

  /// <summary>
  /// Unchecks all items.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void UncheckAll(this ListView @this) {
    Against.ThisIsNull(@this);
    using (@this.PauseUpdates())
      foreach (ListViewItem item in @this.Items)
        item.Checked = false;
  }

  /// <summary>
  /// Selects items matching a predicate.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="predicate">The predicate to match.</param>
  public static void SelectWhere(this ListView @this, Predicate<ListViewItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    using (@this.PauseUpdates())
      foreach (ListViewItem item in @this.Items)
        item.Selected = predicate(item);
  }

  #endregion

  #region Filtering

  private static readonly ConditionalWeakTable<ListView, List<ListViewItem>> _FilteredItems = new();

  /// <summary>
  /// Filters items, hiding those that don't match the predicate.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="predicate">The predicate to match (items returning true are shown).</param>
  public static void Filter(this ListView @this, Predicate<ListViewItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    // First restore any previously hidden items
    @this.ClearFilter();

    // Find items to hide
    var toHide = @this.Items.Cast<ListViewItem>().Where(i => !predicate(i)).ToList();
    if (toHide.Count == 0)
      return;

    // Store hidden items
    _FilteredItems.Add(@this, toHide);

    // Remove from view
    using (@this.PauseUpdates())
      foreach (var item in toHide)
        @this.Items.Remove(item);
  }

  /// <summary>
  /// Filters items by text search in any column.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="searchText">The text to search for (case-insensitive).</param>
  public static void FilterByText(this ListView @this, string searchText) {
    Against.ThisIsNull(@this);

    if (string.IsNullOrEmpty(searchText)) {
      @this.ClearFilter();
      return;
    }

    var search = searchText.ToLowerInvariant();
    @this.Filter(item => {
      if (item.Text.ToLowerInvariant().Contains(search))
        return true;

      return item.SubItems.Cast<ListViewItem.ListViewSubItem>()
        .Any(sub => sub.Text.ToLowerInvariant().Contains(search));
    });
  }

  /// <summary>
  /// Clears the filter and restores all hidden items.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void ClearFilter(this ListView @this) {
    Against.ThisIsNull(@this);

    if (!_FilteredItems.TryGetValue(@this, out var hiddenItems))
      return;

    _FilteredItems.Remove(@this);

    using (@this.PauseUpdates())
      foreach (var item in hiddenItems)
        @this.Items.Add(item);
  }

  #endregion

  #region Sorting

  /// <summary>
  /// Sorts items by a column.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="columnIndex">The column index to sort by.</param>
  /// <param name="ascending">True for ascending, false for descending.</param>
  public static void SortByColumn(this ListView @this, int columnIndex, bool ascending = true) {
    Against.ThisIsNull(@this);

    @this.ListViewItemSorter = new ListViewColumnComparer(columnIndex, ascending);
    @this.Sort();
  }

  /// <summary>
  /// Sorts items by a key selector.
  /// </summary>
  /// <typeparam name="TKey">The type of the sort key.</typeparam>
  /// <param name="this">This ListView.</param>
  /// <param name="keySelector">The function to extract the sort key from an item.</param>
  /// <param name="ascending">True for ascending, false for descending.</param>
  public static void SortBy<TKey>(this ListView @this, Func<ListViewItem, TKey> keySelector, bool ascending = true) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(keySelector);

    @this.ListViewItemSorter = new ListViewKeyComparer<TKey>(keySelector, ascending);
    @this.Sort();
  }

  private sealed class ListViewColumnComparer(int columnIndex, bool ascending) : IComparer {
    public int Compare(object x, object y) {
      var itemX = (ListViewItem)x;
      var itemY = (ListViewItem)y;

      var textX = columnIndex < itemX.SubItems.Count ? itemX.SubItems[columnIndex].Text : string.Empty;
      var textY = columnIndex < itemY.SubItems.Count ? itemY.SubItems[columnIndex].Text : string.Empty;

      var result = string.Compare(textX, textY, StringComparison.CurrentCulture);
      return ascending ? result : -result;
    }
  }

  private sealed class ListViewKeyComparer<TKey>(Func<ListViewItem, TKey> keySelector, bool ascending) : IComparer {
    public int Compare(object x, object y) {
      var itemX = (ListViewItem)x;
      var itemY = (ListViewItem)y;

      var keyX = keySelector(itemX);
      var keyY = keySelector(itemY);

      var result = Comparer<TKey>.Default.Compare(keyX, keyY);
      return ascending ? result : -result;
    }
  }

  #endregion

  #region Utility

  /// <summary>
  /// Scrolls to make an item visible.
  /// </summary>
  /// <param name="this">This ListView.</param>
  /// <param name="item">The item to scroll to.</param>
  public static void ScrollToItem(this ListView @this, ListViewItem item) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(item);
    item.EnsureVisible();
  }

  /// <summary>
  /// Scrolls to the end of the list.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void ScrollToEnd(this ListView @this) {
    Against.ThisIsNull(@this);
    if (@this.Items.Count > 0)
      @this.Items[@this.Items.Count - 1].EnsureVisible();
  }

  /// <summary>
  /// Enables double buffering to reduce flickering.
  /// </summary>
  /// <param name="this">This ListView.</param>
  public static void EnableDoubleBuffering(this ListView @this) {
    Against.ThisIsNull(@this);
    typeof(ListView)
      .GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance | Reflection.BindingFlags.NonPublic)
      ?.SetValue(@this, true);
  }

  #endregion

}
