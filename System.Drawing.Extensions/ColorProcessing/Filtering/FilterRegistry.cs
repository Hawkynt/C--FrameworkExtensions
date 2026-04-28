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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Guard;

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// Registry for runtime discovery of all available pixel filters.
/// </summary>
/// <remarks>
/// <para>
/// Use this class to enumerate all filters at runtime, find filters by name,
/// or filter by category.
/// </para>
/// <example>
/// <code>
/// foreach (var filter in FilterRegistry.All)
///   Console.WriteLine($"{filter.Name} ({filter.Category})");
///
/// var vonKries = FilterRegistry.FindByName("VonKries");
/// </code>
/// </example>
/// </remarks>
public static partial class FilterRegistry {

  /// <summary>
  /// Implemented by the source generator at compile time. See
  /// <c>ScalerRegistry._CollectFromSourceGenerator</c> for the contract.
  /// </summary>
  static partial void _CollectFromSourceGenerator(List<FilterDescriptor> into);


  private static readonly Lazy<IReadOnlyList<FilterDescriptor>> _all = new(
    () => new ReadOnlyList<FilterDescriptor>(DiscoverFilters()));

  /// <summary>
  /// Gets all registered pixel filters.
  /// </summary>
  public static IReadOnlyList<FilterDescriptor> All => _all.Value;

  /// <summary>
  /// Finds a filter by name (case-insensitive).
  /// </summary>
  /// <param name="name">The name to search for.</param>
  /// <returns>The matching descriptor, or <c>null</c> if not found.</returns>
  public static FilterDescriptor? FindByName(string name)
    => All.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Finds filters whose name contains the specified substring (case-insensitive).
  /// </summary>
  /// <param name="substring">The substring to search for.</param>
  /// <returns>All matching descriptors.</returns>
  public static IEnumerable<FilterDescriptor> FindByNameContaining(string substring)
    => All.Where(d => d.Name.Contains(substring, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Gets all filters in the specified category.
  /// </summary>
  /// <param name="category">The category to filter by.</param>
  /// <returns>All filters in the category.</returns>
  public static IEnumerable<FilterDescriptor> GetByCategory(FilterCategory category)
    => All.Where(d => d.Category == category);

  /// <summary>
  /// Gets all filters by the specified author (case-insensitive).
  /// </summary>
  /// <param name="author">The author name to search for.</param>
  /// <returns>All filters by the author.</returns>
  public static IEnumerable<FilterDescriptor> GetByAuthor(string author)
    => All.Where(d => d.Author != null && d.Author.Equals(author, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Gets the descriptor for a specific filter type.
  /// </summary>
  /// <typeparam name="TFilter">The filter type.</typeparam>
  /// <returns>The descriptor, or <c>null</c> if the type is not registered.</returns>
  public static FilterDescriptor? GetDescriptor<TFilter>() where TFilter : struct, IPixelFilter
    => All.FirstOrDefault(d => d.Type == typeof(TFilter));

  private static readonly List<FilterDescriptor> _ParametricVariants = [];

  /// <summary>
  /// Registers an additional parametric variant of an existing filter type. The
  /// variant appears in <see cref="All"/> alongside the fixed-default entry. Its
  /// parameter surface is looked up from <see cref="ParameterMetadata"/> via the
  /// supplied key.
  /// </summary>
  public static void RegisterParametric(
    Type type,
    string name,
    string parameterKey,
    string? author = null,
    string? description = null,
    string? url = null,
    int year = 0,
    FilterCategory category = FilterCategory.Enhancement) {
    Against.ArgumentIsNull(type);
    Against.ArgumentIsNullOrEmpty(name);
    Against.ArgumentIsNullOrEmpty(parameterKey);
    lock (_ParametricVariants)
      _ParametricVariants.Add(FilterDescriptor.__CreateParametric(type, name, author, description, url, year, category, parameterKey));
  }

  private static List<FilterDescriptor> DiscoverFilters() {
    var descriptors = new List<FilterDescriptor>();

    _CollectFromSourceGenerator(descriptors);

    if (descriptors.Count == 0) {
      var assembly = typeof(FilterRegistry).Assembly;
      foreach (var type in assembly.GetTypes()) {
        if (!type.IsValueType || type.IsAbstract)
          continue;

        if (!typeof(IPixelFilter).IsAssignableFrom(type))
          continue;

        var descriptor = FilterDescriptor.FromType(type);
        if (descriptor != null)
          descriptors.Add(descriptor);
      }
    }

    // Touch each parametric filter type so its static constructor runs and registers itself.
    ParametricFilters.EnsureRegistered();

    lock (_ParametricVariants)
      descriptors.AddRange(_ParametricVariants);

    return descriptors
      .GroupBy(d => (d.Type, d.Name))
      .Select(g => g.First())
      .OrderBy(d => d.Name)
      .ToList();
  }
}
