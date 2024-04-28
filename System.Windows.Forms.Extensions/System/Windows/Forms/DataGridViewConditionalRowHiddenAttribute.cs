#region (c)2010-2042 Hawkynt

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

namespace System.Windows.Forms;

/// <summary>
///   Allows specifying the row visibility depending on the underlying object instance.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class DataGridViewConditionalRowHiddenAttribute : Attribute {
  public DataGridViewConditionalRowHiddenAttribute(string isHiddenWhen) => this.IsHiddenWhen = isHiddenWhen;

  public string IsHiddenWhen { get; }

  public bool IsHidden(object row) =>
    DataGridViewExtensions.GetPropertyValueOrDefault(row, this.IsHiddenWhen, false, false, false, false);

  public static void OnRowPrepaint(IEnumerable<DataGridViewConditionalRowHiddenAttribute> @this, DataGridViewRow row,
    object data, DataGridViewRowPrePaintEventArgs e) {
    foreach (var attribute in @this)
      if (attribute.IsHidden(data)) {
        row.Visible = false;
        return;
      }

    row.Visible = true;
  }
}
