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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Guard;

namespace System.Windows.Forms;

/// <summary>
/// Extension methods for <see cref="ListBox"/> providing attribute-based rendering,
/// styling, and various utility methods.
/// </summary>
public static partial class ListBoxExtensions {

  #region Suspended Update Token

  /// <summary>
  /// Token returned by <see cref="PauseUpdates"/> to restore updates when disposed.
  /// </summary>
  public interface ISuspendedUpdateToken : IDisposable { }

  private sealed class SuspendedUpdateToken(ListBox listBox) : ISuspendedUpdateToken {
    private bool _isDisposed;

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      listBox.EndUpdate();
    }
  }

  /// <summary>
  /// Pauses updates to the ListBox until the returned token is disposed.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  /// <returns>A token that restores updates when disposed.</returns>
  /// <example>
  /// <code>
  /// using (listBox.PauseUpdates()) {
  ///   // Add many items without flickering
  ///   foreach (var item in items)
  ///     listBox.Items.Add(item);
  /// }
  /// </code>
  /// </example>
  public static ISuspendedUpdateToken PauseUpdates(this ListBox @this) {
    Against.ThisIsNull(@this);
    @this.BeginUpdate();
    return new SuspendedUpdateToken(@this);
  }

  #endregion

  #region EnableExtendedAttributes

  private static readonly ConditionalWeakTable<ListBox, object> _ExtendedAttributesEnabled = new();
  private static readonly ConditionalWeakTable<ListBox, Form> _ListBoxParentForms = new();

  /// <summary>
  /// Enables attribute-based rendering for this ListBox.
  /// This hooks owner-draw events to apply <see cref="ListItemStyleAttribute"/> and <see cref="ListItemImageAttribute"/>.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  /// <example>
  /// <code>
  /// listBox.EnableExtendedAttributes();
  /// listBox.DataSource = myData;
  /// </code>
  /// </example>
  public static void EnableExtendedAttributes(this ListBox @this) {
    Against.ThisIsNull(@this);

    // Check if already enabled
    if (_ExtendedAttributesEnabled.TryGetValue(@this, out _))
      return;

    _ExtendedAttributesEnabled.Add(@this, new object());

    // Unsubscribe first to avoid duplicates
    @this.DrawItem -= _ListBox_DrawItem;
    @this.HandleCreated -= _ListBox_HandleCreated;

    // Subscribe
    @this.DrawItem += _ListBox_DrawItem;
    @this.HandleCreated += _ListBox_HandleCreated;

    // Enable owner draw for custom rendering
    @this.DrawMode = DrawMode.OwnerDrawFixed;

    // Hook parent form if already has handle
    if (@this.IsHandleCreated)
      _HookListBoxParentForm(@this);
  }

  private static void _ListBox_HandleCreated(object sender, EventArgs e) => _HookListBoxParentForm((ListBox)sender);

  private static void _HookListBoxParentForm(ListBox listBox) {
    // Unhook old parent form if any
    if (_ListBoxParentForms.TryGetValue(listBox, out var oldForm)) {
      oldForm.Resize -= _ListBox_ParentForm_Resize;
      oldForm.Activated -= _ListBox_ParentForm_Activated;
      _ListBoxParentForms.Remove(listBox);
    }

    // Hook new parent form
    var newForm = listBox.FindForm();
    if (newForm == null)
      return;

    _ListBoxParentForms.Add(listBox, newForm);
    newForm.Resize += _ListBox_ParentForm_Resize;
    newForm.Activated += _ListBox_ParentForm_Activated;
  }

  private static void _ListBox_ParentForm_Resize(object sender, EventArgs e) {
    var form = (Form)sender;
    _RefreshListBoxesInContainer(form);
  }

  private static void _ListBox_ParentForm_Activated(object sender, EventArgs e) {
    var form = (Form)sender;
    _RefreshListBoxesInContainer(form);
  }

  private static void _RefreshListBoxesInContainer(Control container) {
    foreach (Control control in container.Controls) {
      if (control is ListBox listBox && _ExtendedAttributesEnabled.TryGetValue(listBox, out _))
        listBox.Refresh();
      else if (control.HasChildren)
        _RefreshListBoxesInContainer(control);
    }
  }

  private static void _ListBox_DrawItem(object sender, DrawItemEventArgs e) {
    var listBox = (ListBox)sender;

    if (e.Index < 0 || e.Index >= listBox.Items.Count) {
      e.DrawBackground();
      return;
    }

    var data = listBox.Items[e.Index];
    if (data == null) {
      e.DrawBackground();
      return;
    }

    var type = data.GetType();

    // Determine colors
    var foreColor = e.ForeColor;
    var backColor = e.BackColor;

    // Apply selection colors if selected
    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
      backColor = SystemColors.Highlight;
      foreColor = SystemColors.HighlightText;
    } else {
      // Apply style attributes
      var styleAttrs = ListControlExtensions.GetStyleAttributes(type);
      foreach (var attr in styleAttrs)
        if (attr.IsEnabled(data)) {
          if (attr.GetForeColor(data) is { } fg)
            foreColor = fg;
          if (attr.GetBackColor(data) is { } bg)
            backColor = bg;
        }
    }

    // Draw background
    using (var brush = new SolidBrush(backColor))
      e.Graphics.FillRectangle(brush, e.Bounds);

    // Get display text
    var displayText = listBox.GetItemText(data);

    // Check for image attribute
    var imageAttr = ListControlExtensions.GetImageAttribute(type);
    var image = imageAttr?.GetImage(data);
    var textX = e.Bounds.X + 2;

    if (image != null) {
      // Draw image
      var imageY = e.Bounds.Y + (e.Bounds.Height - image.Height) / 2;
      e.Graphics.DrawImage(image, textX, imageY);
      textX += image.Width + 4;
    }

    // Draw text
    using (var brush = new SolidBrush(foreColor))
      e.Graphics.DrawString(displayText, e.Font, brush, textX, e.Bounds.Y + 2);

    // Draw focus rectangle if focused
    e.DrawFocusRectangle();
  }

  #endregion

  #region Selection

  /// <summary>
  /// Gets the bound data from selected items (for multi-select ListBox).
  /// </summary>
  /// <typeparam name="T">The type of the data objects.</typeparam>
  /// <param name="this">This ListBox.</param>
  /// <returns>The bound data objects from selected items.</returns>
  public static IEnumerable<T> GetSelectedItems<T>(this ListBox @this) where T : class {
    Against.ThisIsNull(@this);
    return @this.SelectedItems.Cast<object>().OfType<T>();
  }

  /// <summary>
  /// Gets the currently selected item as the specified type.
  /// </summary>
  /// <typeparam name="T">The type of the data object.</typeparam>
  /// <param name="this">This ListBox.</param>
  /// <returns>The selected item, or null if nothing is selected or type doesn't match.</returns>
  public static T GetSelectedItem<T>(this ListBox @this) where T : class {
    Against.ThisIsNull(@this);
    return @this.SelectedItem as T;
  }

  /// <summary>
  /// Selects all items (for multi-select ListBox).
  /// </summary>
  /// <param name="this">This ListBox.</param>
  public static void SelectAll(this ListBox @this) {
    Against.ThisIsNull(@this);

    if (@this.SelectionMode == SelectionMode.One || @this.SelectionMode == SelectionMode.None)
      return;

    using (@this.PauseUpdates())
      for (var i = 0; i < @this.Items.Count; ++i)
        @this.SetSelected(i, true);
  }

  /// <summary>
  /// Deselects all items.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  public static void SelectNone(this ListBox @this) {
    Against.ThisIsNull(@this);
    @this.ClearSelected();
  }

  /// <summary>
  /// Selects items matching a predicate.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  /// <param name="predicate">The predicate to match.</param>
  public static void SelectWhere(this ListBox @this, Predicate<object> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    using (@this.PauseUpdates())
      for (var i = 0; i < @this.Items.Count; ++i)
        @this.SetSelected(i, predicate(@this.Items[i]));
  }

  #endregion

  #region Filtering

  private static readonly ConditionalWeakTable<ListBox, FilterState> _FilterStates = new();

  private sealed class FilterState {
    public object OriginalDataSource { get; set; }
    public List<object> OriginalItems { get; set; }
    public Predicate<object> CurrentFilter { get; set; }
  }

  /// <summary>
  /// Filters items, showing only those that match the predicate.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  /// <param name="predicate">The predicate to match (items returning true are shown).</param>
  public static void Filter(this ListBox @this, Predicate<object> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    if (!_FilterStates.TryGetValue(@this, out var state)) {
      // Store original items
      state = new FilterState {
        OriginalDataSource = @this.DataSource,
        OriginalItems = @this.Items.Cast<object>().ToList()
      };
      _FilterStates.Add(@this, state);
    }

    state.CurrentFilter = predicate;

    // Apply filter
    var filteredItems = state.OriginalItems.Where(i => predicate(i)).ToList();

    using (@this.PauseUpdates()) {
      @this.DataSource = null;
      @this.Items.Clear();
      foreach (var item in filteredItems)
        @this.Items.Add(item);
    }
  }

  /// <summary>
  /// Clears the filter and restores all items.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  public static void ClearFilter(this ListBox @this) {
    Against.ThisIsNull(@this);

    if (!_FilterStates.TryGetValue(@this, out var state))
      return;

    _FilterStates.Remove(@this);

    using (@this.PauseUpdates()) {
      @this.Items.Clear();
      if (state.OriginalDataSource != null)
        @this.DataSource = state.OriginalDataSource;
      else
        foreach (var item in state.OriginalItems)
          @this.Items.Add(item);
    }
  }

  #endregion

  #region Utility

  /// <summary>
  /// Scrolls to make an item visible.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  /// <param name="item">The item to scroll to.</param>
  public static void ScrollToItem(this ListBox @this, object item) {
    Against.ThisIsNull(@this);

    var index = @this.Items.IndexOf(item);
    if (index >= 0)
      @this.TopIndex = index;
  }

  /// <summary>
  /// Gets the bound data from all items.
  /// </summary>
  /// <typeparam name="T">The type of the data objects.</typeparam>
  /// <param name="this">This ListBox.</param>
  /// <returns>The bound data objects.</returns>
  public static IEnumerable<T> GetBoundData<T>(this ListBox @this) where T : class {
    Against.ThisIsNull(@this);
    return @this.Items.Cast<object>().OfType<T>();
  }

  /// <summary>
  /// Enables double buffering to reduce flickering.
  /// </summary>
  /// <param name="this">This ListBox.</param>
  public static void EnableDoubleBuffering(this ListBox @this) {
    Against.ThisIsNull(@this);
    typeof(ListBox)
      .GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance | Reflection.BindingFlags.NonPublic)
      ?.SetValue(@this, true);
  }

  #endregion

}
