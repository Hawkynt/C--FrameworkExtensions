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

#if !NETSTANDARD && !NET5_0_OR_GREATER && NET40_OR_GREATER
using System.ComponentModel;
using System.Data.Common;

namespace System.Data;
public static partial class DataTableExtensions {
  
  /// <summary>
  /// Fills a datatable from a dataadapter.
  /// </summary>
  /// <typeparam name="ATableAdapter">The type of the table adapter.</typeparam>
  /// <param name="this">This DataTable.</param>
  /// <param name="connection">Optional: A different connection string.</param>
  public static void FillWith<ATableAdapter>(this DataTable @this, string connection = null) where ATableAdapter : Component, new() {
    dynamic adapter = new ATableAdapter();
    if (connection != null)
      ((DbConnection)adapter.Connection).ConnectionString = connection;

    adapter.Fill((dynamic)@this);
  }
}

#endif