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

using System.Collections.Concurrent;
using System.ComponentModel;
#if NETFX_4
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.Reflection;
using System.Text;
using System.Transactions;

namespace System.Data.Linq {
  internal static partial class DataContextExtensions {

    /// <summary>
    /// Gets the SQL statements for committing the current changeset.
    /// </summary>
    /// <param name="this">This DataContext.</param>
    /// <returns>String containing the sql statements.</returns>
    public static string GetChangeSqlStatement(this DataContext @this) {
#if NETFX_4
      Contract.Requires(@this != null);
#endif
      using (var memStream = new MemoryStream())
      using (var textWriter = new StreamWriter(memStream) { AutoFlush = true }) {
        using (new TransactionScope()) {
          @this.Log = textWriter;
          try {
            @this.SubmitChanges();
          } catch {
            return null;
          }
          Transaction.Current.Rollback();
        }
        return Encoding.UTF8.GetString(memStream.ToArray());
      }
    }

    private static readonly ConcurrentDictionary<Type, MethodInfo> _initializeMethods = new ConcurrentDictionary<Type, MethodInfo>();

    /// <summary>
    /// Detaches an entity from its existing connection.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="this">Can be ANY connection.</param>
    /// <param name="entity">The entity to detach from its connection.</param>
    public static void Detach<TEntity>(this DataContext @this, TEntity entity) where TEntity : INotifyPropertyChanged, INotifyPropertyChanging
          => _initializeMethods.GetOrAdd(typeof(TEntity), t => t.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic)).Invoke(entity, null)
          ;

  }
}