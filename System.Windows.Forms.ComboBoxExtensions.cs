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
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Windows.Forms {
  internal static partial class ComboBoxExtensions {
    /// <summary>
    /// Sets the datasource.
    /// </summary>
    /// <param name="this">This ComboBox.</param>
    /// <param name="source">The source.</param>
    /// <param name="displayMember">The display member, if any.</param>
    /// <param name="valueMember">The value member, if any.</param>
    public static void DataSource(this ComboBox @this, object source, string displayMember = null, string valueMember = null) {
      Contract.Requires(@this != null);
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
    /// Sets the datasource for enumerations.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="this">This ComboBox.</param>
    /// <param name="insertNull">Insert null-object (use as non-selected).</param>
    public static void DataSource<TEnum>(this ComboBox @this, bool insertNull = false) where TEnum : struct {
      Contract.Requires(typeof(TEnum).IsEnum);

      @this.DataSource(
        (insertNull ? new[] { new Tuple<object, string>(null, null) } : new Tuple<object, string>[0])
        .Concat(Enum.GetValues(typeof(TEnum)).Cast<object>()
        .Select(
          i => {
            var fieldInfo = typeof(TEnum).GetField(i.ToString());
            Contract.Assert(fieldInfo != null, "Can not find field");
            var attribute =
              (DisplayNameAttribute)fieldInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault();
            return Tuple.Create(i, attribute?.DisplayName ?? i.ToString());
          })).ToArray(), nameof(Tuple<object, string>.Item2), nameof(Tuple<object, string>.Item1)
      );
    }

    /// <summary>
    /// Sets the selected item based on an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="this">This ComboBox.</param>
    /// <param name="value">The value.</param>
    public static void SetSelectedEnumItem<TEnum>(this ComboBox @this, TEnum value) where TEnum : struct {
      Contract.Requires(typeof(TEnum).IsEnum);
      SetSelectedItem<Tuple<object, string>>(@this, i => Equals((TEnum)i.Item1, value));
    }

    /// <summary>
    /// Sets the selected item based on a predicate.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This ComboBox.</param>
    /// <param name="predicate">The predicate.</param>
    public static void SetSelectedItem<TItem>(this ComboBox @this, Func<TItem, bool> predicate) {
      var dataSource = @this.DataSource;
      var items = dataSource as IEnumerable<TItem>;
      var selectedItem = items == null ? default(TItem) : items.FirstOrDefault(predicate);
      @this.SelectedItem = selectedItem;
    }
  }
}