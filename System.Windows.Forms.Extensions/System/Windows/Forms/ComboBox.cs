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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Guard;

namespace System.Windows.Forms;

/// <summary>
/// Specifies how images and text are displayed in a ComboBox with extended attributes.
/// </summary>
public enum ComboBoxDisplayMode {
  /// <summary>Text only, no image.</summary>
  TextOnly,
  /// <summary>Image only, no text.</summary>
  ImageOnly,
  /// <summary>Image before text (default).</summary>
  ImageBeforeText,
  /// <summary>Text before image.</summary>
  TextBeforeImage
}

public static partial class ComboBoxExtensions {

  /// <summary>
  /// Automatically adjusts the width of the <see cref="System.Windows.Forms.ComboBox"/> to fit the width of its longest item.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Short", "Longer item", "The longest item in the list" });
  /// comboBox.AutoAdjustWidth();
  /// // The ComboBox width is now adjusted to fit the longest item.
  /// </code>
  /// </example>
  public static void AutoAdjustWidth(this ComboBox @this) {
    Against.ThisIsNull(@this);

    var items = @this.Items;
    var vertScrollBarWidth =
        items.Count > @this.MaxDropDownItems // visible scrollbar?
          ? SystemInformation.VerticalScrollBarWidth
          : 0
      ;

    var font = @this.Font;
    var displayMode = @this.GetDisplayMode();

    // Calculate image width if images are used
    var imageWidth = 0;
    if (displayMode != ComboBoxDisplayMode.TextOnly && items.Count > 0) {
      var firstItem = items[0];
      if (firstItem != null) {
        var type = firstItem.GetType();
        var imageAttr = ListControlExtensions.GetImageAttribute(type);
        if (imageAttr != null) {
          // Check if any item has an image
          foreach (var item in items.Cast<object>()) {
            var image = imageAttr.GetImage(item);
            if (image != null) {
              imageWidth = image.Width + 6; // image width + padding
              break;
            }
          }
        }
      }
    }

    var textWidth = displayMode == ComboBoxDisplayMode.ImageOnly
      ? 0
      : items
          .Cast<object>()
          .Select(i => TextRenderer.MeasureText(@this.GetItemText(i), font).Width)
          .DefaultIfEmpty(0)
          .Max();

    @this.Width = textWidth + imageWidth + vertScrollBarWidth + 4; // +4 for margins
  }

  /// <summary>
  /// Sets the data source, display member, and value member properties of the <see cref="System.Windows.Forms.ComboBox"/>.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="source">The data source for the ComboBox.</param>
  /// <param name="displayMember">(Optional: defaults to <see langword="null"/>) The property to display for the items in the ComboBox.</param>
  /// <param name="valueMember">(Optional: defaults to <see langword="null"/>) The property to use as the actual value for the items in the ComboBox.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// var dataSource = new List&lt;Person&gt;
  /// {
  ///     new Person { Id = 1, Name = "Alice" },
  ///     new Person { Id = 2, Name = "Bob" }
  /// };
  /// comboBox.DataSource(dataSource, "Name", "Id");
  /// // The ComboBox is now populated with the names of the persons and uses their Ids as values.
  /// </code>
  /// </example>
  public static void DataSource(this ComboBox @this, object source, string displayMember = null, string valueMember = null) {
    Against.ThisIsNull(@this);

    var oldDis = @this.DisplayMember;
    var oldVal = @this.ValueMember;
    @this.DataSource = null;
    @this.DisplayMember = displayMember ?? oldDis;
    @this.ValueMember = valueMember ?? oldVal;
    @this.DataSource = source;
    @this.DisplayMember = displayMember ?? oldDis;
    @this.ValueMember = valueMember ?? oldVal;
    @this.SelectedIndex = @this.Items.Count > 0 ? 0 : -1;
  }

  /// <summary>
  /// Sets the data source of the <see cref="System.Windows.Forms.ComboBox"/> to the values of the specified enum type, with options to insert a null entry and ignore specified values.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to use as the data source.</typeparam>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="insertNull">(Optional: defaults to <see langword="false"/>) If set to <c>true</c>, inserts a null entry at the beginning of the ComboBox.</param>
  /// <param name="ignoreValues">(Optional: defaults to <see langword="null"/>) An array of enum values to ignore.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// enum Colors { Red, Green, Blue, Yellow }
  ///
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.DataSource&lt;Colors&gt;(insertNull: true, ignoreValues: new[] { Colors.Yellow });
  /// // The ComboBox is now populated with the enum values, excluding "Yellow", and includes a null entry.
  /// </code>
  /// </example>
  public static void DataSource<TEnum>(this ComboBox @this, bool insertNull = false, TEnum[] ignoreValues = null) where TEnum : struct
    => DataSource(@this, (insertNull ? [new(null, null)] : new Tuple<object, string>[0])
      .Concat(
        Enum
          .GetValues(typeof(TEnum))
          .Cast<object>()
          .Where(i => ignoreValues == null || ignoreValues.Length == 0 || !ignoreValues.Contains((TEnum)i))
          .Select(
            i => {
              var fieldInfo = typeof(TEnum).GetField(i.ToString());
              var attribute =
                (DisplayNameAttribute)fieldInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault();
              return Tuple.Create(i, attribute?.DisplayName ?? i.ToString());
            }
          )
      )
      .ToArray(),
      nameof(Tuple<object, string>.Item2),
      nameof(Tuple<object, string>.Item1)
    );

  /// <summary>
  /// Sets the selected item of the <see cref="System.Windows.Forms.ComboBox"/> to the specified enum value.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="value">The enum value to set as the selected item.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// enum Colors { Red, Green, Blue }
  ///
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(Enum.GetValues(typeof(Colors)).Cast&lt;object&gt;().ToArray());
  /// comboBox.SetSelectedEnumItem(Colors.Green);
  /// // The ComboBox now has "Green" selected.
  /// </code>
  /// </example>
  public static void SetSelectedEnumItem<TEnum>(this ComboBox @this, TEnum value) where TEnum : struct
    => SetSelectedItem<Tuple<object, string>>(@this, i => Equals((TEnum)i.Item1, value));

  /// <summary>
  /// Sets the selected item of the <see cref="System.Windows.Forms.ComboBox"/> to the specified value.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="value">The value to set as the selected item.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Alice", "Bob", "Charlie" });
  /// comboBox.SetSelectedItem("Bob");
  /// // The ComboBox now has "Bob" selected.
  /// </code>
  /// </example>
  public static void SetSelectedItem(this ComboBox @this, object value)
    => SetSelectedItem<Tuple<object, string>>(@this, i => Equals(i.Item1, value));

  /// <summary>
  /// Sets the selected item of the <see cref="System.Windows.Forms.ComboBox"/> based on a specified predicate.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the ComboBox.</typeparam>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="predicate">The function to determine which item should be selected.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Alice", "Bob", "Charlie" });
  /// comboBox.SetSelectedItem&lt;string&gt;(item => item == "Bob");
  /// // The ComboBox now has "Bob" selected.
  /// </code>
  /// </example>
  public static void SetSelectedItem<TItem>(this ComboBox @this, Func<TItem, bool> predicate) {
    Against.ThisIsNull(@this);
    
    var dataSource = @this.DataSource;
    var selectedItem = dataSource is not IEnumerable<TItem> items ? default : items.FirstOrDefault(predicate);
    @this.SelectedItem = selectedItem;
  }

  /// <summary>
  /// Sets the selected item of the <see cref="System.Windows.Forms.ComboBox"/> and temporarily suppresses the <see cref="System.Windows.Forms.ComboBox.SelectedIndexChanged"/> event.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="selectedItem">The item to set as the selected item.</param>
  /// <param name="handler">The event handler to temporarily remove and reattach to suppress the event.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Alice", "Bob", "Charlie" });
  /// comboBox.SelectedIndexChanged += (sender, e) => Console.WriteLine("Index changed");
  /// comboBox.SetSelectedItemAndSuppressIndexChangedEvent("Bob", comboBox.SelectedIndexChanged);
  /// // The ComboBox now has "Bob" selected without triggering the "Index changed" event.
  /// </code>
  /// </example>
  public static void SetSelectedItemAndSuppressIndexChangedEvent(this ComboBox @this, object selectedItem, EventHandler handler) {
    Against.ThisIsNull(@this);

    // no handler given? just set the given item as selected
    if (handler == null) {
      @this.SelectedItem = selectedItem;
      return;
    }

    // prevent multiple event handler adding
    var hasHandlerBeenDetached = false;
    try {
      @this.SelectedIndexChanged -= handler;
      hasHandlerBeenDetached = true;

      @this.SelectedItem = selectedItem;
    } finally {
      if (hasHandlerBeenDetached)
        @this.SelectedIndexChanged += handler;
    }
  }

  /// <summary>
  /// Sets the selected value of the <see cref="System.Windows.Forms.ComboBox"/> and temporarily suppresses the <see cref="System.Windows.Forms.ComboBox.SelectedIndexChanged"/> event.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="selectedValue">The value to set as the selected value.</param>
  /// <param name="handler">The event handler to temporarily remove and reattach to suppress the event.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Alice", "Bob", "Charlie" });
  /// comboBox.SelectedIndexChanged += (sender, e) => Console.WriteLine("Index changed");
  /// comboBox.SetSelectedValueAndSuppressIndexChangedEvent("Bob", comboBox.SelectedIndexChanged);
  /// // The ComboBox now has "Bob" selected without triggering the "Index changed" event.
  /// </code>
  /// </example>
  public static void SetSelectedValueAndSuppressIndexChangedEvent(this ComboBox @this, object selectedValue, EventHandler handler) {
    Against.ThisIsNull(@this);

    // no handler given? just set the given value as selected
    if (handler == null) {
      @this.SelectedValue = selectedValue;
      return;
    }

    // prevent multiple event handler adding
    var hasHandlerBeenDetached = false;
    try {
      @this.SelectedIndexChanged -= handler;
      hasHandlerBeenDetached = true;

      @this.SelectedValue = selectedValue;
    } finally {
      if (hasHandlerBeenDetached)
        @this.SelectedIndexChanged += handler;
    }
  }

  /// <summary>
  /// Sets the selected item of the <see cref="System.Windows.Forms.ComboBox"/> and temporarily suppresses the <see cref="System.Windows.Forms.ComboBox.SelectedValueChanged"/> event.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="selectedItem">The item to set as the selected item.</param>
  /// <param name="handler">The event handler to temporarily remove and reattach to suppress the event.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Alice", "Bob", "Charlie" });
  /// comboBox.SelectedValueChanged += (sender, e) => Console.WriteLine("Value changed");
  /// comboBox.SetSelectedItemAndSuppressValueChangedEvent("Bob", comboBox.SelectedValueChanged);
  /// // The ComboBox now has "Bob" selected without triggering the "Value changed" event.
  /// </code>
  /// </example>
  public static void SetSelectedItemAndSuppressValueChangedEvent(this ComboBox @this, object selectedItem, EventHandler handler) {
    Against.ThisIsNull(@this);

    // no handler given? just set the given value as selected
    if (handler == null) {
      @this.SelectedItem = selectedItem;
      return;
    }

    // prevent multiple event handler adding
    var hasHandlerBeenDetached = false;
    try {
      @this.SelectedValueChanged -= handler;
      hasHandlerBeenDetached = true;

      @this.SelectedItem = selectedItem;
    } finally {
      if (hasHandlerBeenDetached)
        @this.SelectedValueChanged += handler;
    }
  }

  /// <summary>
  /// Tries to get the selected item of the <see cref="System.Windows.Forms.ComboBox"/> as a specified type.
  /// </summary>
  /// <typeparam name="TItem">The type to which the selected item should be cast.</typeparam>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="result">When this method returns, contains the selected item as the specified type, if the cast is successful; otherwise, the default value of the specified type.</param>
  /// <returns><see langword="true"/> if the selected item is successfully cast to the specified type; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Alice", "Bob", "Charlie" });
  /// comboBox.SelectedItem = "Bob";
  ///
  /// if (comboBox.TryGetSelectedItem&lt;string&gt;(out var selectedItem))
  /// {
  ///     Console.WriteLine($"Selected item: {selectedItem}");
  /// }
  /// else
  /// {
  ///     Console.WriteLine("No valid item selected.");
  /// }
  /// // Output: Selected item: Bob
  /// </code>
  /// </example>
  public static bool TryGetSelectedItem<TItem>(this ComboBox @this, out TItem result) {
    Against.ThisIsNull(@this);

    var selected = @this.SelectedItem;

    if (selected is TItem item) {
      result = item;
      return true;
    }

    result = default;
    return false;
  }

  /// <summary>
  /// Tries to get the selected item of the <see cref="System.Windows.Forms.ComboBox"/> as a specified enum type.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to which the selected item should be cast.</typeparam>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <param name="item">When this method returns, contains the selected item as the specified enum type, if the cast is successful; otherwise, the default value of the specified enum type.</param>
  /// <returns><see langword="true"/> if the selected item is successfully cast to the specified enum type; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// enum Colors { Red, Green, Blue }
  ///
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(Enum.GetValues(typeof(Colors)).Cast&lt;object&gt;().ToArray());
  /// comboBox.SelectedItem = Colors.Green;
  ///
  /// if (comboBox.TryGetSelectedEnumItem&lt;Colors&gt;(out var selectedColor))
  /// {
  ///     Console.WriteLine($"Selected color: {selectedColor}");
  /// }
  /// else
  /// {
  ///     Console.WriteLine("No valid color selected.");
  /// }
  /// // Output: Selected color: Green
  /// </code>
  /// </example>
  public static bool TryGetSelectedEnumItem<TEnum>(this ComboBox @this, out TEnum item) where TEnum : struct, IConvertible {
    var success = TryGetSelectedItem(@this, out Tuple<object, string> enumValue);

    item = success ? (TEnum)enumValue.Item1 : default;
    return success;
  }

  /// <summary>
  /// Gets the selected item of the <see cref="System.Windows.Forms.ComboBox"/> or a default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the selected item.</typeparam>
  /// <param name="this">The <see cref="System.Windows.Forms.ComboBox"/> instance.</param>
  /// <returns>The selected item or the default value of the specified type if unsuccessful.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ComboBox comboBox = new ComboBox();
  /// comboBox.Items.AddRange(new object[] { "Alice", "Bob", "Charlie" });
  /// comboBox.SelectedItem = "Bob";
  ///
  /// string selectedItem = comboBox.GetSelectedItem&lt;string&gt;();
  /// Console.WriteLine($"Selected item: {selectedItem}");
  /// // Output: Selected item: Bob
  /// </code>
  /// </example>
  public static TItem GetSelectedItem<TItem>(this ComboBox @this)
    => !TryGetSelectedItem(@this, out TItem item) ? default : item;

  #region Suspended Update Token

  /// <summary>
  /// Token returned by <see cref="PauseUpdates"/> to restore updates when disposed.
  /// </summary>
  public interface ISuspendedUpdateToken : IDisposable { }

  private sealed class SuspendedUpdateToken(ComboBox comboBox) : ISuspendedUpdateToken {
    private bool _isDisposed;

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      comboBox.EndUpdate();
    }
  }

  /// <summary>
  /// Pauses updates to the ComboBox until the returned token is disposed.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  /// <returns>A token that restores updates when disposed.</returns>
  /// <example>
  /// <code>
  /// using (comboBox.PauseUpdates()) {
  ///   // Add many items without flickering
  ///   foreach (var item in items)
  ///     comboBox.Items.Add(item);
  /// }
  /// </code>
  /// </example>
  public static ISuspendedUpdateToken PauseUpdates(this ComboBox @this) {
    Against.ThisIsNull(@this);
    @this.BeginUpdate();
    return new SuspendedUpdateToken(@this);
  }

  #endregion

  #region EnableExtendedAttributes

  private static readonly ConditionalWeakTable<ComboBox, object> _ExtendedAttributesEnabled = new();
  private static readonly ConditionalWeakTable<ComboBox, DisplayModeHolder> _DisplayModes = new();
  private static readonly ConditionalWeakTable<ComboBox, Form> _ComboBoxParentForms = new();

  private sealed class DisplayModeHolder {
    public ComboBoxDisplayMode Mode { get; set; } = ComboBoxDisplayMode.ImageBeforeText;
  }

  /// <summary>
  /// Gets the display mode for this ComboBox.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  /// <returns>The current display mode.</returns>
  public static ComboBoxDisplayMode GetDisplayMode(this ComboBox @this) {
    Against.ThisIsNull(@this);
    return _DisplayModes.TryGetValue(@this, out var holder) ? holder.Mode : ComboBoxDisplayMode.ImageBeforeText;
  }

  /// <summary>
  /// Sets the display mode for this ComboBox.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  /// <param name="mode">The display mode to use.</param>
  public static void SetDisplayMode(this ComboBox @this, ComboBoxDisplayMode mode) {
    Against.ThisIsNull(@this);

    if (!_DisplayModes.TryGetValue(@this, out var holder)) {
      holder = new DisplayModeHolder();
      _DisplayModes.Add(@this, holder);
    }

    holder.Mode = mode;
    @this.Invalidate();
  }

  /// <summary>
  /// Enables attribute-based rendering for this ComboBox.
  /// This hooks owner-draw events to apply <see cref="ListItemStyleAttribute"/> and <see cref="ListItemImageAttribute"/>.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  /// <example>
  /// <code>
  /// comboBox.EnableExtendedAttributes();
  /// comboBox.DataSource = myData;
  /// </code>
  /// </example>
  public static void EnableExtendedAttributes(this ComboBox @this) {
    Against.ThisIsNull(@this);

    // Check if already enabled
    if (_ExtendedAttributesEnabled.TryGetValue(@this, out _))
      return;

    _ExtendedAttributesEnabled.Add(@this, new object());

    // Initialize display mode holder
    if (!_DisplayModes.TryGetValue(@this, out _))
      _DisplayModes.Add(@this, new DisplayModeHolder());

    // Unsubscribe first to avoid duplicates
    @this.DrawItem -= _ComboBox_DrawItem;
    @this.HandleCreated -= _ComboBox_HandleCreated;

    // Subscribe
    @this.DrawItem += _ComboBox_DrawItem;
    @this.HandleCreated += _ComboBox_HandleCreated;

    // Enable owner draw for custom rendering
    @this.DrawMode = DrawMode.OwnerDrawFixed;

    // Hook parent form if already has handle
    if (@this.IsHandleCreated)
      _HookComboBoxParentForm(@this);
  }

  private static void _ComboBox_HandleCreated(object sender, EventArgs e) => _HookComboBoxParentForm((ComboBox)sender);

  private static void _HookComboBoxParentForm(ComboBox comboBox) {
    // Unhook old parent form if any
    if (_ComboBoxParentForms.TryGetValue(comboBox, out var oldForm)) {
      oldForm.Resize -= _ComboBox_ParentForm_Resize;
      oldForm.Activated -= _ComboBox_ParentForm_Activated;
      _ComboBoxParentForms.Remove(comboBox);
    }

    // Hook new parent form
    var newForm = comboBox.FindForm();
    if (newForm == null)
      return;

    _ComboBoxParentForms.Add(comboBox, newForm);
    newForm.Resize += _ComboBox_ParentForm_Resize;
    newForm.Activated += _ComboBox_ParentForm_Activated;
  }

  private static void _ComboBox_ParentForm_Resize(object sender, EventArgs e) {
    var form = (Form)sender;
    _RefreshComboBoxesInContainer(form);
  }

  private static void _ComboBox_ParentForm_Activated(object sender, EventArgs e) {
    var form = (Form)sender;
    _RefreshComboBoxesInContainer(form);
  }

  private static void _RefreshComboBoxesInContainer(Control container) {
    foreach (Control control in container.Controls) {
      if (control is ComboBox comboBox && _ExtendedAttributesEnabled.TryGetValue(comboBox, out _))
        comboBox.Refresh();
      else if (control.HasChildren)
        _RefreshComboBoxesInContainer(control);
    }
  }

  private static void _ComboBox_DrawItem(object sender, DrawItemEventArgs e) {
    var comboBox = (ComboBox)sender;

    if (e.Index < 0 || e.Index >= comboBox.Items.Count) {
      e.DrawBackground();
      return;
    }

    var data = comboBox.Items[e.Index];
    if (data == null) {
      e.DrawBackground();
      return;
    }

    var type = data.GetType();
    var displayMode = comboBox.GetDisplayMode();

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

    // Get display text and image
    var displayText = comboBox.GetItemText(data);
    var imageAttr = ListControlExtensions.GetImageAttribute(type);
    var image = imageAttr?.GetImage(data);

    var textX = e.Bounds.X + 2;
    var textY = e.Bounds.Y + (e.Bounds.Height - e.Font.Height) / 2;

    switch (displayMode) {
      case ComboBoxDisplayMode.TextOnly:
        // Draw text only
        using (var brush = new SolidBrush(foreColor))
          e.Graphics.DrawString(displayText, e.Font, brush, textX, textY);
        break;

      case ComboBoxDisplayMode.ImageOnly:
        // Draw image only (centered)
        if (image != null) {
          var imageX = e.Bounds.X + (e.Bounds.Width - image.Width) / 2;
          var imageY = e.Bounds.Y + (e.Bounds.Height - image.Height) / 2;
          e.Graphics.DrawImage(image, imageX, imageY);
        }
        break;

      case ComboBoxDisplayMode.ImageBeforeText:
        // Draw image then text
        if (image != null) {
          var imageY = e.Bounds.Y + (e.Bounds.Height - image.Height) / 2;
          e.Graphics.DrawImage(image, textX, imageY);
          textX += image.Width + 4;
        }
        using (var brush = new SolidBrush(foreColor))
          e.Graphics.DrawString(displayText, e.Font, brush, textX, textY);
        break;

      case ComboBoxDisplayMode.TextBeforeImage:
        // Draw text then image
        using (var brush = new SolidBrush(foreColor))
          e.Graphics.DrawString(displayText, e.Font, brush, textX, textY);
        if (image != null) {
          var textSize = e.Graphics.MeasureString(displayText, e.Font);
          var imageX = textX + (int)textSize.Width + 4;
          var imageY = e.Bounds.Y + (e.Bounds.Height - image.Height) / 2;
          e.Graphics.DrawImage(image, imageX, imageY);
        }
        break;
    }

    // Draw focus rectangle if focused
    e.DrawFocusRectangle();
  }

  #endregion

  #region Selection

  /// <summary>
  /// Deselects the current item.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  public static void SelectNone(this ComboBox @this) {
    Against.ThisIsNull(@this);
    @this.SelectedIndex = -1;
  }

  /// <summary>
  /// Selects the first item matching a predicate.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  /// <param name="predicate">The predicate to match.</param>
  public static void SelectWhere(this ComboBox @this, Predicate<object> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.Items.Count; ++i)
      if (predicate(@this.Items[i])) {
        @this.SelectedIndex = i;
        return;
      }

    @this.SelectedIndex = -1;
  }

  #endregion

  #region Utility

  /// <summary>
  /// Gets the bound data from all items.
  /// </summary>
  /// <typeparam name="T">The type of the data objects.</typeparam>
  /// <param name="this">This ComboBox.</param>
  /// <returns>The bound data objects.</returns>
  public static IEnumerable<T> GetBoundData<T>(this ComboBox @this) where T : class {
    Against.ThisIsNull(@this);
    return @this.Items.Cast<object>().OfType<T>();
  }

  /// <summary>
  /// Enables double buffering to reduce flickering.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  public static void EnableDoubleBuffering(this ComboBox @this) {
    Against.ThisIsNull(@this);
    typeof(ComboBox)
      .GetProperty("DoubleBuffered", Reflection.BindingFlags.Instance | Reflection.BindingFlags.NonPublic)
      ?.SetValue(@this, true);
  }

  #endregion

}
