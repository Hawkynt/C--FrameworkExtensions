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

using System.Linq.Expressions;

// ReSharper disable UnusedMember.Global
namespace System.Linq {
  // ReSharper disable once PartialTypeWithSinglePart

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class IQueryableExtensions {
    
    /// <summary>
    /// Modifies the resultset to include filtering based on the given query string.
    /// Multiple filters may be present, split by whitespaces, combined with AND.
    /// </summary>
    /// <typeparam name="TRow">The type of rows</typeparam>
    /// <param name="this">This IQueryable</param>
    /// <param name="query">The query, eg. "green white" (means only entries with "green" AND "white")</param>
    /// <param name="selector">Which column of the record to filter</param>
    public static IQueryable<TRow> FilterIfNeeded<TRow>(this IQueryable<TRow> @this, Expression<Func<TRow, string>> selector, string query) {
      if (@this == null)
        throw new ArgumentNullException(nameof(@this));
      if (selector == null)
        throw new ArgumentNullException(nameof(selector));

      if (query.IsNullOrWhiteSpace())
        return @this;

      var results = @this;
      // ReSharper disable once LoopCanBeConvertedToQuery
      foreach (var filter in query.Trim().Split(" ")) {
        if (filter.IsNullOrWhiteSpace())
          continue;

        var constant = Expression.Constant(filter);
        var instance = Expression.Parameter(typeof(TRow), "row");
        var resolveCall = Expression.Invoke(selector, instance);
        var expression = Expression.Call(resolveCall, nameof(string.Contains), null, constant);
        var lambda = Expression.Lambda<Func<TRow, bool>>(expression, instance);
        results = results.Where(lambda);
      }

      return results;
    }
  }
}
