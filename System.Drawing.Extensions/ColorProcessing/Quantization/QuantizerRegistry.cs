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
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Registry for runtime discovery of all available color quantization algorithms.
/// </summary>
/// <remarks>
/// <para>
/// Use this class to enumerate all quantizers at runtime, including both parameterless
/// default instances and static preset properties.
/// </para>
/// <example>
/// <code>
/// // List all available quantizers
/// foreach (var quantizer in QuantizerRegistry.All)
///   Console.WriteLine($"{quantizer.Name} ({quantizer.Type})");
///
/// // Find a specific quantizer
/// var octree = QuantizerRegistry.All.FirstOrDefault(q => q.Name.Contains("Octree"));
///
/// // Create an instance
/// var instance = QuantizerRegistry.All.First(q => q.Name == "Octree").CreateDefault();
/// </code>
/// </example>
/// </remarks>
public static class QuantizerRegistry {

  private static readonly Lazy<QuantizerDescriptor[]> _all = new(DiscoverQuantizers);

  /// <summary>
  /// Gets all registered quantizer types and presets.
  /// </summary>
  public static IEnumerable<QuantizerDescriptor> All => _all.Value;

  /// <summary>
  /// Finds a quantizer by name (case-insensitive).
  /// </summary>
  /// <param name="name">The name to search for.</param>
  /// <returns>The matching descriptor, or <c>null</c> if not found.</returns>
  public static QuantizerDescriptor? FindByName(string name)
    => All.FirstOrDefault(q => q.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Finds quantizers whose name contains the specified substring (case-insensitive).
  /// </summary>
  /// <param name="substring">The substring to search for.</param>
  /// <returns>All matching descriptors.</returns>
  public static IEnumerable<QuantizerDescriptor> FindByNameContaining(string substring)
    => All.Where(q => q.Name.Contains(substring, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Gets all quantizers of the specified type.
  /// </summary>
  /// <param name="type">The quantization type to filter by.</param>
  /// <returns>All quantizers of the specified type.</returns>
  public static IEnumerable<QuantizerDescriptor> GetByType(QuantizationType type)
    => All.Where(q => q.Type == type);

  private static QuantizerDescriptor[] DiscoverQuantizers() {
    var assembly = typeof(QuantizerRegistry).Assembly;
    var descriptors = new List<QuantizerDescriptor>();

    foreach (var type in assembly.GetTypes()) {
      if (!typeof(IQuantizer).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
        continue;

      // Check for QuantizerAttribute
      var attr = type.GetCustomAttribute<QuantizerAttribute>();

      // Discover static preset properties (don't evaluate them - they may be expensive)
      var staticProps = type.GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => typeof(IQuantizer).IsAssignableFrom(p.PropertyType) && p.CanRead)
        .ToList();

      foreach (var prop in staticProps) {
        var name = prop.Name;
        var quantizationType = attr?.QuantizationType ?? QuantizationType.Tree;
        var author = attr?.Author;
        var year = attr?.Year ?? 0;
        var qualityRating = attr?.QualityRating ?? 5;

        // Capture prop for closure - don't evaluate until CreateDefault is called
        var capturedProp = prop;
        descriptors.Add(new(type, name, author, quantizationType, year, qualityRating,
          () => (IQuantizer)capturedProp.GetValue(null)!));
      }

      // Also add default instance if parameterless constructor exists
      if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null) {
        var name = attr?.DisplayName ?? type.Name.Replace("Quantizer", "");
        var quantizationType = attr?.QuantizationType ?? QuantizationType.Tree;
        var author = attr?.Author;
        var year = attr?.Year ?? 0;
        var qualityRating = attr?.QualityRating ?? 5;

        // Only add if we haven't already added presets (to avoid duplicates)
        if (staticProps.Count == 0) {
          var capturedType = type;
          descriptors.Add(new(
            type,
            name,
            author,
            quantizationType,
            year,
            qualityRating,
            () => (IQuantizer)Activator.CreateInstance(capturedType)!));
        }
      }
    }

    return descriptors.OrderBy(q => q.Name).ToArray();
  }
}

/// <summary>
/// Describes a color quantization algorithm with its metadata.
/// </summary>
public sealed class QuantizerDescriptor {

  /// <summary>
  /// Gets the concrete type of the quantizer.
  /// </summary>
  public Type DeclaringType { get; }

  /// <summary>
  /// Gets the display name of the quantizer.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets the author of the algorithm.
  /// </summary>
  public string? Author { get; }

  /// <summary>
  /// Gets the quantization type/category.
  /// </summary>
  public QuantizationType Type { get; }

  /// <summary>
  /// Gets the year the algorithm was published.
  /// </summary>
  public int Year { get; }

  /// <summary>
  /// Gets the quality rating (1-10, higher is better).
  /// </summary>
  public int QualityRating { get; }

  private readonly Func<IQuantizer> _factory;

  internal QuantizerDescriptor(
    Type declaringType,
    string name,
    string? author,
    QuantizationType type,
    int year,
    int qualityRating,
    Func<IQuantizer> factory) {
    this.DeclaringType = declaringType;
    this.Name = name;
    this.Author = author;
    this.Type = type;
    this.Year = year;
    this.QualityRating = qualityRating;
    this._factory = factory;
  }

  /// <summary>
  /// Creates an instance of this quantizer.
  /// </summary>
  /// <returns>A quantizer instance.</returns>
  public IQuantizer CreateDefault() => this._factory();

  /// <inheritdoc />
  public override string ToString() => $"{this.Name} ({this.Type})";
}
