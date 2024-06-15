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
using System.Linq;

namespace System.Data;

public static partial class DataRowExtensions {
  /// <summary>
  ///   Converrs the datarow to a dictionary.
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
