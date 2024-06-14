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
using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class ComboBoxExtensions {
  /// <summary>
  ///   Automatically adjusts the width according to items.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  public static void AutoAdjustWidth(this ComboBox @this) {
    var items = @this.Items;
    var vertScrollBarWidth =
        items.Count > @this.MaxDropDownItems // visible scrollbar?
          ? SystemInformation.VerticalScrollBarWidth
          : 0
      ;

    var font = @this.Font;
    @this.Width = items
                    .Cast<object>()
                    .Select(i => TextRenderer.MeasureText(@this.GetItemText(i), font).Width)
                    .Max()
                  + vertScrollBarWidth
      ;
  }

  /// <summary>
  ///   Sets the datasource.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  /// <param name="source">The source.</param>
  /// <param name="displayMember">The display member, if any.</param>
  /// <param name="valueMember">The value member, if any.</param>
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
  ///   Sets the datasource for enumerations.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="this">This ComboBox.</param>
  /// <param name="insertNull">Insert null-object (use as non-selected).</param>
  /// <param name="ignoreValues">Values not to be used.</param>
  public static void DataSource<TEnum>(this ComboBox @this, bool insertNull = false, TEnum[] ignoreValues = null) where TEnum : struct
    => @this.DataSource(
      (insertNull ? [new(null, null)] : new Tuple<object, string>[0])
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
  ///   Sets the selected item based on an enum value.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="this">This ComboBox.</param>
  /// <param name="value">The value.</param>
  public static void SetSelectedEnumItem<TEnum>(this ComboBox @this, TEnum value) where TEnum : struct
    => SetSelectedItem<Tuple<object, string>>(@this, i => Equals((TEnum)i.Item1, value));

  /// <summary>
  ///   Sets the selected item.
  /// </summary>
  /// <param name="this">This ComboBox.</param>
  /// <param name="value">The value.</param>
  public static void SetSelectedItem(this ComboBox @this, object value)
    => SetSelectedItem<Tuple<object, string>>(@this, i => Equals(i.Item1, value));

  /// <summary>
  ///   Sets the selected item based on a predicate.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This ComboBox.</param>
  /// <param name="predicate">The predicate.</param>
  public static void SetSelectedItem<TItem>(this ComboBox @this, Func<TItem, bool> predicate) {
    var dataSource = @this.DataSource;
    var selectedItem = dataSource is not IEnumerable<TItem> items ? default : items.FirstOrDefault(predicate);
    @this.SelectedItem = selectedItem;
  }

  /// <summary>
  ///   Sets the selected item and suppress given event.
  /// </summary>
  /// <param name="this">The tool strip ComboBox.</param>
  /// <param name="selectedItem">The selected item.</param>
  /// <param name="handler">The handler.</param>
  public static void SetSelectedItemAndSuppressIndexChangedEvent(this ComboBox @this, object selectedItem, EventHandler handler) {
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
  ///   Sets the selected item and suppress given event.
  /// </summary>
  /// <param name="this">The tool strip ComboBox.</param>
  /// <param name="selectedValue">The selected value.</param>
  /// <param name="handler">The handler.</param>
  public static void SetSelectedValueAndSuppressIndexChangedEvent(this ComboBox @this, object selectedValue, EventHandler handler) {
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
  ///   Sets the selected item and suppress given event.
  /// </summary>
  /// <param name="this">The ComboBox.</param>
  /// <param name="selectedItem">The selected item.</param>
  /// <param name="handler">The handler.</param>
  public static void SetSelectedItemAndSuppressValueChangedEvent(this ComboBox @this, object selectedItem, EventHandler handler) {
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
  ///   Tries to get the selected item in a combobox
  /// </summary>
  /// <typeparam name="TItem">The Type of the items of the combobox</typeparam>
  /// <param name="this">The combobox</param>
  /// <param name="result">ot parameter for the selected item</param>
  /// <returns>true if there was a selection, false otherwise</returns>
  public static bool TryGetSelectedItem<TItem>(this ComboBox @this, out TItem result) {
    var selected = @this.SelectedItem;

    if (selected is TItem item) {
      result = item;
      return true;
    }

    result = default;
    return false;
  }

  /// <summary>
  ///   Tries to get the selected item in a combobox
  /// </summary>
  /// <typeparam name="TEnum">The Type of the enumeration which holds the items of the combobox</typeparam>
  /// <param name="this">The combobox</param>
  /// <param name="item">ot parameter for the selected item</param>
  /// <returns>true if there was a selection, false otherwise</returns>
  public static bool TryGetSelectedEnumItem<TEnum>(this ComboBox @this, out TEnum item) where TEnum : struct, IConvertible {
    var success = @this.TryGetSelectedItem(out Tuple<object, string> enumValue);

    item = success ? (TEnum)enumValue.Item1 : default;
    return success;
  }

  /// <summary>
  ///   Gets the selected item in a combobox
  /// </summary>
  /// <typeparam name="TItem">The Type of the items of the combobox</typeparam>
  /// <param name="this">The combobox</param>
  /// <returns>The selected item in the combobox if there is a selection, and default(TItem) otherwise</returns>
  public static TItem GetSelectedItem<TItem>(this ComboBox @this)
    => !TryGetSelectedItem(@this, out TItem item) ? default : item;
}
