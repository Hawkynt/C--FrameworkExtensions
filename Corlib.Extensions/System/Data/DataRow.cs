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
using System.Linq;

namespace System.Data;

public static partial class DataRowExtensions {
  /// <summary>
  /// Converrs the datarow to a dictionary.
  /// </summary>
  /// <param name="this">This DataRow.</param>
  /// <returns></returns>
  public static Dictionary<string, object> ToDictionary(this DataRow @this) {
    var table = @this.Table;
    var i = 0;
    var result = table.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName, column => @this.ItemArray[i++]);
    return result;
  }
  
}