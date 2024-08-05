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
    @this.Width = items
                    .Cast<object>()
                    .Select(i => TextRenderer.MeasureText(@this.GetItemText(i), font).Width)
                    .Max()
                  + vertScrollBarWidth
      ;
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

}
