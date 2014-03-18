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

namespace System.Windows.Forms {
  internal static partial class DataGridViewExtensions {

    /// <summary>
    /// Scrolls to the end.
    /// </summary>
    /// <param name="This">This DataGridView.</param>
    public static void ScrollToEnd(this DataGridView This) {
      Contract.Requires(This != null);
      var rowCount = This.RowCount;
      if (rowCount <= 0)
        return;

      try {
        This.FirstDisplayedScrollingRowIndex = rowCount - 1;
      } catch (Exception) {
        ;
      }
    }

    /// <summary>
    /// Clones the columns to another datagridview.
    /// </summary>
    /// <param name="This">This DataGridView.</param>
    /// <param name="target">The target DataGridView.</param>
    public static void CloneColumns(this DataGridView This, DataGridView target) {
      Contract.Requires(This != null);
      Contract.Requires(target != null);
      Contract.Requires(This != target);
      target.Columns.AddRange((from i in This.Columns.Cast<DataGridViewColumn>() select (DataGridViewColumn)i.Clone()).ToArray());
    }

    /// <summary>
    /// Finds the columns that match a certain condition.
    /// </summary>
    /// <param name="This">This DataGridView.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>An enumeration of columns.</returns>
    public static IEnumerable<DataGridViewColumn> FindColumns(this DataGridView This, Predicate<DataGridViewColumn> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      return (from i in This.Columns.Cast<DataGridViewColumn>() where predicate(i) select i);
    }

    /// <summary>
    /// Finds the first column that matches a certain condition.
    /// </summary>
    /// <param name="This">This DataGridView.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The first matching column or <c>null</c>.</returns>
    public static DataGridViewColumn FindFirstColumn(this DataGridView This, Predicate<DataGridViewColumn> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var matches = This.FindColumns(predicate);
      return (matches == null ? null : matches.FirstOrDefault());
    }
  }
}