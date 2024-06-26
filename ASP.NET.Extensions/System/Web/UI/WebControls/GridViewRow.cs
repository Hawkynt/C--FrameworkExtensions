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

#if NETFRAMEWORK

using System.Data;

namespace System.Web.UI.WebControls;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class GridViewRowExtensions {
  /// <summary>
  /// Gets the data from a column.
  /// </summary>
  /// <param name="This">The this.</param>
  /// <param name="columnName">Name of the column.</param>
  /// <returns></returns>
  public static object GetDataFromColumn(this GridViewRow This, string columnName) {
    if (This.RowType != DataControlRowType.DataRow)
      throw new ArgumentException("The GridViewRow must be a DataRow.");

    var row = ((DataRowView)This.DataItem).Row;
    return row[columnName];
  }
}

#endif