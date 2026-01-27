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
using System.Linq;
using System.Reflection;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Registry for runtime discovery of all available dithering algorithms and presets.
/// </summary>
/// <remarks>
/// <para>
/// Use this class to enumerate all ditherers at runtime, including both parameterless
/// default instances and static preset properties (e.g., <c>ErrorDiffusion.FloydSteinberg</c>).
/// </para>
/// <example>
/// <code>
/// // List all available ditherers
/// foreach (var ditherer in DithererRegistry.All)
///   Console.WriteLine($"{ditherer.Name} ({ditherer.Type})");
///
/// // Find a specific ditherer
/// var floydSteinberg = DithererRegistry.All.FirstOrDefault(d => d.Name.Contains("Floyd"));
///
/// // Create an instance
/// var instance = DithererRegistry.All.First(d => d.Name == "Bayer4x4").CreateDefault();
/// </code>
/// </example>
/// </remarks>
public static class DithererRegistry {

  private static readonly Lazy<DithererDescriptor[]> _all = new(DiscoverDitherers);

  /// <summary>
  /// Gets all registered ditherer presets and types.
  /// </summary>
  public static IEnumerable<DithererDescriptor> All => _all.Value;

  /// <summary>
  /// Finds a ditherer by name (case-insensitive).
  /// </summary>
  /// <param name="name">The name to search for.</param>
  /// <returns>The matching descriptor, or <c>null</c> if not found.</returns>
  public static DithererDescriptor? FindByName(string name)
    => All.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Finds ditherers whose name contains the specified substring (case-insensitive).
  /// </summary>
  /// <param name="substring">The substring to search for.</param>
  /// <returns>All matching descriptors.</returns>
  public static IEnumerable<DithererDescriptor> FindByNameContaining(string substring)
    => All.Where(d => d.Name.Contains(substring, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Gets all ditherers of the specified type.
  /// </summary>
  /// <param name="type">The dithering type to filter by.</param>
  /// <returns>All ditherers of the specified type.</returns>
  public static IEnumerable<DithererDescriptor> GetByType(DitheringType type)
    => All.Where(d => d.Type == type);

  private static DithererDescriptor[] DiscoverDitherers() {
    var assembly = typeof(DithererRegistry).Assembly;
    var descriptors = new List<DithererDescriptor>();

    foreach (var type in assembly.GetTypes()) {
      if (!typeof(IDitherer).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
        continue;

      // Check for DithererAttribute
      var attr = type.GetCustomAttribute<DithererAttribute>();

      // Discover static preset properties (don't evaluate them - they may be expensive)
      var staticProps = type.GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => typeof(IDitherer).IsAssignableFrom(p.PropertyType) && p.CanRead)
        .ToList();

      foreach (var prop in staticProps) {
        var name = prop.Name;
        var ditheringType = attr?.Type ?? DitheringType.Custom;
        var author = attr?.Author;
        var year = attr?.Year ?? 0;
        var description = attr?.Description;

        // Capture prop for closure - don't evaluate until CreateDefault is called
        var capturedProp = prop;
        descriptors.Add(new(type, name, author, description, ditheringType, year,
          () => (IDitherer)capturedProp.GetValue(null!)!));
      }

      // Also add default instance if parameterless constructor exists
      if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null) {
        var name = attr?.Name ?? type.Name.Replace("Ditherer", "");
        var ditheringType = attr?.Type ?? DitheringType.Custom;
        var author = attr?.Author;
        var year = attr?.Year ?? 0;
        var description = attr?.Description;

        // Only add if we haven't already added presets (to avoid duplicates for simple ditherers)
        if (staticProps.Count == 0) {
          var capturedType = type;
          descriptors.Add(new(
            type,
            name,
            author,
            description,
            ditheringType,
            year,
            () => (IDitherer)Activator.CreateInstance(capturedType)!));
        }
      }
    }

    return descriptors.OrderBy(d => d.Name).ToArray();
  }
}

/// <summary>
/// Describes a dithering algorithm with its metadata.
/// </summary>
public sealed class DithererDescriptor {

  /// <summary>
  /// Gets the concrete type of the ditherer.
  /// </summary>
  public Type DeclaringType { get; }

  /// <summary>
  /// Gets the display name of the ditherer preset.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets the author of the algorithm.
  /// </summary>
  public string? Author { get; }

  /// <summary>
  /// Gets a description of the algorithm.
  /// </summary>
  public string? Description { get; }

  /// <summary>
  /// Gets the dithering type/category.
  /// </summary>
  public DitheringType Type { get; }

  /// <summary>
  /// Gets the year the algorithm was published.
  /// </summary>
  public int Year { get; }

  private readonly Func<IDitherer> _factory;

  internal DithererDescriptor(
    Type declaringType,
    string name,
    string? author,
    string? description,
    DitheringType type,
    int year,
    Func<IDitherer> factory) {
    this.DeclaringType = declaringType;
    this.Name = name;
    this.Author = author;
    this.Description = description;
    this.Type = type;
    this.Year = year;
    this._factory = factory;
  }

  /// <summary>
  /// Creates an instance of this ditherer preset.
  /// </summary>
  /// <returns>A ditherer instance.</returns>
  public IDitherer CreateDefault() => this._factory();

  /// <inheritdoc />
  public override string ToString() => $"{this.Name} ({this.Type})";
}
