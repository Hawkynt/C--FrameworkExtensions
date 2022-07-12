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
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
#if NET40_OR_GREATER
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace System.Data.Linq {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class DataContextExtensions {


    /// <summary>
    /// Gets the SQL statements for committing the current changeset.
    /// </summary>
    /// <param name="this">This DataContext.</param>
    /// <returns>String containing the sql statements.</returns>
    public static string GetChangeSqlStatement(this DataContext @this) {
#if NET40_OR_GREATER
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

    /// <summary>
    /// Runs specific update actions in a database transaction
    /// </summary>
    /// <typeparam name="TContext">The type of this DataContext</typeparam>
    /// <param name="this">The DataContext where the statements should be executed</param>
    /// <param name="updateAction">update actions which should be executed on this DataContext</param>
    /// <param name="exception">out parameter for any occurred exception within the transaction</param>
    /// <returns>True if the transaction was committed successfully, false otherwise</returns>
    public static bool RunInTransaction<TContext>(this TContext @this, Func<TContext, bool> updateAction, out Exception exception) where TContext : DataContext {
      try {
        @this.Connection.Open();
        @this.Transaction = @this.Connection.BeginTransaction();

        exception = null;

        var success = updateAction.Invoke(@this);

        if (!success) {
          @this.Transaction.Rollback();
          return false;
        }

        @this.Transaction.Commit();
        return true;

      } catch (Exception e) {
        Trace.WriteLine(e);

        exception = e;
        @this.Transaction.Rollback();

        return false;
      }
    }

    /// <summary>
    /// Try to load data from the specified database
    /// </summary>
    /// <typeparam name="TContext">The type of this DataContext</typeparam>
    /// <typeparam name="TResult">The type of the data which should be returned</typeparam>
    /// <param name="this">The DataContext where to run the loading-statement</param>
    /// <param name="loadRequest">A function which returns the requested data from the database</param>
    /// <param name="results">out parameter for the results returned by the loadRequest</param>
    /// <param name="exception">out parameter for any occurred exception within the loadRequest</param>
    /// <returns>True if the data could be loaded successfully, false otherwise</returns>
    public static bool TryGetResults<TContext, TResult>(this TContext @this, Func<TContext, TResult[]> loadRequest, out TResult[] results, out Exception exception) where TContext : DataContext {
      try {
        results = loadRequest.Invoke(@this);
        exception = null;

        return true;
      } catch (Exception e) {
        Trace.WriteLine(e);

        exception = e;
        results = new TResult[] { };
        return false;
      }
    }

#if NET45_OR_GREATER
    /// <summary>
    /// Try to load asynchronously data from the specified database
    /// </summary>
    /// <typeparam name="TContext">The type of this DataContext</typeparam>
    /// <typeparam name="TResult">The type of the data which should be returned</typeparam>
    /// <param name="this">The DataContext where to run the loading-statement</param>
    /// <param name="loadRequest">A function which returns the requested data from the database</param>
    /// <returns>An awaitable Task which returns a Tuple which holds the success of the loadRequest,
    /// a reference to a possibly occurred exception and the resulting data</returns>
    public static async Task<Tuple<bool, Exception, TResult[]>> TryGetResultsAsync<TContext, TResult>(this TContext @this, Func<TContext, TResult[]> loadRequest) where TContext : DataContext {
      return await Task.Run(() => {
        try {
          var results = loadRequest.Invoke(@this);

          return Tuple.Create(true, (Exception)null, results);
        } catch (Exception e) {
          Trace.WriteLine(e);

          return Tuple.Create(false, e, new TResult[] { });
        }
      });
    }
#endif

    /// <summary>
    /// Try to load a single row from the specified database
    /// </summary>
    /// <typeparam name="TContext">The type of this DataContext</typeparam>
    /// <typeparam name="TResult">The type of the data which should be returned</typeparam>
    /// <param name="this">The DataContext where to run the loading-statement</param>
    /// <param name="loadRequest">A function which returns the requested data from the database</param>
    /// <param name="result">out parameter for the result returned by the loadRequest</param>
    /// <param name="exception">out parameter for any occurred exception within the loadRequest</param>
    /// <returns>True if the data could be loaded successfully, false otherwise</returns>
    public static bool TryGetSingleResult<TContext, TResult>(this TContext @this, out TResult result, Func<TContext, TResult> loadRequest, out Exception exception) where TContext : DataContext  {
      try {
        result = loadRequest.Invoke(@this);
        exception = null;

        return true;
      } catch (Exception e) {
        Trace.WriteLine(e);

        exception = e;
        result = default(TResult);
        return false;
      }
    }
  }
}

#endif