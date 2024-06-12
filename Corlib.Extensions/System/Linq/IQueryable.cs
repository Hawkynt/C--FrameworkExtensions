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
using Guard;

// ReSharper disable UnusedMember.Global
namespace System.Linq; 
// ReSharper disable once PartialTypeWithSinglePart

public static partial class IQueryableExtensions {

  /// <summary>
  /// Finds the first Element whose trimmed value determined via <paramref name="selector" /> matches the search term. 
  /// </summary>
  /// <typeparam name="TRow">The type of rows.</typeparam>
  /// <param name="this">This <see cref="IQueryable"/>.</param>
  /// <param name="selector">Which column of the record should be matched against.</param>
  /// <param name="value">The search term.</param>
  /// <param name="ignoreCase"><see langword="true"/> when case should be ignored; otherwise, <see langword="false"/></param>
  /// <returns>The matching element or <see langword="null"/>, if no such element was found</returns>
  /// <remarks>Returns <see langword="null"/> if the passed value is <see langword="null"/> or whitespace</remarks>
  public static TRow FirstOrDefaultWithSanitizedDbValue<TRow>(this IQueryable<TRow> @this, Expression<Func<TRow, string>> selector, string value, bool ignoreCase = true) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    if (value.IsNullOrWhiteSpace())
      return default;

    value = value.Trim();

    if (ignoreCase)
      value = value.ToUpper();

    var equalsMethod = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string) });

    var constant = Expression.Constant(value, typeof(string));
    var instance = Expression.Parameter(typeof(TRow), "row");

    Expression resolveCall = Expression.Invoke(selector, instance);
    resolveCall = Expression.Call(resolveCall, nameof(string.Trim), null, null);

    if(ignoreCase)
      resolveCall = Expression.Call(resolveCall, nameof(string.ToUpper), null, null);

    var expression = Expression.Call(resolveCall, equalsMethod!, constant);
    var lambda = Expression.Lambda<Func<TRow, bool>>(expression, instance);

    return @this.FirstOrDefault(lambda);
  }

  /// <summary>
  /// Modifies the result-set to include filtering based on the given query string.
  /// Multiple filters may be present, split by whitespaces, automatically combined with AND.
  /// </summary>
  /// <typeparam name="TRow">The type of rows.</typeparam>
  /// <param name="this">The <see cref="IQueryable{T}"/> to be filtered.</param>
  /// <param name="selector">An expression that selects the column to filter on (e.g., r => r.Name).</param>
  /// <param name="query">The query string containing terms to filter by, separated by spaces (e.g., "green white" means only entries with both "green" AND "white").</param>
  /// <param name="ignoreCase">If set to <see langword="true"/>, ignores case when comparing; defaults to <see langword="false"/>.</param>
  /// <returns>A filtered <see cref="IQueryable{T}"/> based on the specified query.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// var records = dbContext.Records.AsQueryable();
  /// var filteredRecords = records.FilterIfNeeded(r => r.Name, "green white", true);
  /// </code>
  /// This example demonstrates how to filter a queryable list of records where the "Name" column contains both "green" and "white", ignoring case.
  /// </example>
  public static IQueryable<TRow> FilterIfNeeded<TRow>(this IQueryable<TRow> @this, Expression<Func<TRow, string>> selector, string query, bool ignoreCase = false) 
    => FilterIfNeeded(@this, query, ignoreCase, selector)
    ;

  /// <summary>
  /// Modifies the result-set to include filtering based on the given query string across multiple columns.
  /// Multiple filters may be present, split by whitespaces, combined with AND.
  /// Separate expressions for columns are combined using OR within each filter term.
  /// </summary>
  /// <typeparam name="TRow">The type of rows.</typeparam>
  /// <param name="this">The <see cref="IQueryable{T}"/> to be filtered.</param>
  /// <param name="query">The query string containing terms to filter by, separated by spaces (e.g., "green white" means only entries with both "green" AND "white").</param>
  /// <param name="selectors">An array of expressions that select the columns to filter on (e.g., r => r.Name, r => r.Description).</param>
  /// <returns>A filtered <see cref="IQueryable{T}"/> based on the specified query.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="selectors"/> is <see langword="null"/> or contains null elements.</exception>
  /// <example>
  /// <code>
  /// var records = dbContext.Records.AsQueryable();
  /// var filteredRecords = records.FilterIfNeeded("green white", r => r.Name, r => r.Description);
  /// </code>
  /// This example demonstrates how to filter a queryable list of records where either the "Name" or "Description" column contains both "green" and "white" (but they don't need to be in the same column), with exact case.
  /// </example>
  /// <remarks>
  /// This method allows for filtering across multiple columns. Each filter term is matched against each column, combining column matches with OR and combining filter terms with AND.
  /// </remarks>
  public static IQueryable<TRow> FilterIfNeeded<TRow>(this IQueryable<TRow> @this, string query, params Expression<Func<TRow, string>>[] selectors)
    => FilterIfNeeded(@this, query, false, selectors)
    ;

  /// <summary>
  /// Modifies the result-set to include filtering based on the given query string across multiple columns.
  /// Multiple filters may be present, split by whitespaces, combined with AND.
  /// Separate expressions for columns are combined using OR within each filter term.
  /// </summary>
  /// <typeparam name="TRow">The type of rows.</typeparam>
  /// <param name="this">The <see cref="IQueryable{T}"/> to be filtered.</param>
  /// <param name="query">The query string containing terms to filter by, separated by spaces (e.g., "green white" means only entries with both "green" AND "white").</param>
  /// <param name="ignoreCase">If set to <see langword="true"/>, ignores case when comparing; defaults to <see langword="false"/>.</param>
  /// <param name="selectors">An array of expressions that select the columns to filter on (e.g., r => r.Name, r => r.Description).</param>
  /// <returns>A filtered <see cref="IQueryable{T}"/> based on the specified query.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="selectors"/> is <see langword="null"/> or contains null elements.</exception>
  /// <example>
  /// <code>
  /// var records = dbContext.Records.AsQueryable();
  /// var filteredRecords = records.FilterIfNeeded("green white", true, r => r.Name, r => r.Description);
  /// </code>
  /// This example demonstrates how to filter a queryable list of records where either the "Name" or "Description" column contains both "green" and "white" (but they don't need to be in the same column), ignoring case.
  /// </example>
  /// <remarks>
  /// This method allows for filtering across multiple columns. Each filter term is matched against each column, combining column matches with OR and combining filter terms with AND.
  /// </remarks>
  public static IQueryable<TRow> FilterIfNeeded<TRow>(this IQueryable<TRow> @this, string query, bool ignoreCase, params Expression<Func<TRow, string>>[] selectors) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(selectors);

    if (query.IsNullOrWhiteSpace())
      return @this;

    var results = @this;
    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var filter in query.Trim().Split(" ")) {
      if (filter.IsNullOrWhiteSpace())
        continue;

      var constant = Expression.Constant(ignoreCase ? filter.ToLower() : filter);
      var instance = Expression.Parameter(typeof(TRow), "row");

      Expression combinedExpression = null;
      
      foreach (var selector in selectors) {
        var resolveCall = Expression.Invoke(selector, instance);
        Expression selectorExpression = ignoreCase ? Expression.Call(resolveCall, nameof(string.ToLower), Type.EmptyTypes) : resolveCall;
        var containsCall = Expression.Call(selectorExpression, nameof(string.Contains), null, constant);

        if (combinedExpression == null)
          combinedExpression = containsCall;
        else
          combinedExpression = Expression.OrElse(combinedExpression, containsCall);

      }

      var lambda = Expression.Lambda<Func<TRow, bool>>(combinedExpression!, instance);
      results = results.Where(lambda);
    }

    return results;
  }

  /// <summary>Sorts the elements of a sequence in ascending order according to a property.</summary>
  /// <typeparam name="TElement">The type of the elements of <paramref name="this" />.</typeparam>
  /// <param name="this">A sequence of values to order.</param>
  /// <param name="propertyPath">Path to the property to order by e.g. <c>nameof(<typeparamref name="TElement"/>.PropertyName)</c>.</param>
  /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a property.</returns>
  public static IOrderedQueryable<TElement> OrderByPropertyName<TElement>(this IQueryable<TElement> @this, string propertyPath)
    => _OrderByPropertyNameUsing(@this, propertyPath, nameof(Queryable.OrderBy));

  /// <inheritdoc cref="OrderByPropertyName{T}"/>
  /// <summary>Sorts the elements of a sequence in descending order according to a property.</summary>
  public static IOrderedQueryable<T> OrderByPropertyNameDescending<T>(this IQueryable<T> @this, string propertyPath)
    => _OrderByPropertyNameUsing(@this, propertyPath, nameof(Queryable.OrderByDescending));

  /// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order according to a property.</summary>
  /// <inheritdoc cref="OrderByPropertyName{T}"/>
  public static IOrderedQueryable<T> ThenByPropertyName<T>(this IOrderedQueryable<T> @this, string propertyPath)
    => _OrderByPropertyNameUsing(@this, propertyPath, nameof(Queryable.ThenBy));

  /// <summary>Performs a subsequent ordering of the elements in a sequence in descending order according to a property.</summary>
  /// <inheritdoc cref="OrderByPropertyName{T}"/>
  public static IOrderedQueryable<T> ThenByPropertyNameDescending<T>(this IOrderedQueryable<T> @this, string propertyPath)
    => _OrderByPropertyNameUsing(@this, propertyPath, nameof(Queryable.ThenByDescending));

  private static IOrderedQueryable<T> _OrderByPropertyNameUsing<T>(this IQueryable<T> @this, string propertyPath, string method) {
    var parameter = Expression.Parameter(typeof(T), "item");
    var member = propertyPath.Split('.')
      .Aggregate((Expression)parameter, Expression.PropertyOrField);

    var keySelector = Expression.Lambda(member, parameter);
    var methodCall = Expression.Call(typeof(Queryable), method, new[]
        { parameter.Type, member.Type },
      @this.Expression, Expression.Quote(keySelector));

    return (IOrderedQueryable<T>)@this.Provider.CreateQuery(methodCall);
  }
}
