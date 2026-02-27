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

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Registry for runtime discovery of all available scaling algorithms.
/// </summary>
/// <remarks>
/// <para>
/// Use this class to enumerate all scalers at runtime, find scalers by name,
/// or filter scalers by category.
/// </para>
/// <example>
/// <code>
/// // List all available scalers
/// foreach (var scaler in ScalerRegistry.All)
///   Console.WriteLine($"{scaler.Name} by {scaler.Author} ({scaler.Category})");
///
/// // Find a specific scaler
/// var xbr = ScalerRegistry.PixelScalers.FirstOrDefault(s => s.Name.Contains("XBR"));
///
/// // Create an instance
/// var instance = ScalerRegistry.PixelScalers
///   .First(s => s.Name == "HQ")
///   .CreateDefault();
/// </code>
/// </example>
/// </remarks>
public static class ScalerRegistry {

  private static readonly Lazy<IReadOnlyList<ScalerDescriptor>> _all = new(
    () => new ReadOnlyList<ScalerDescriptor>(DiscoverScalers()));
  private static readonly Lazy<IReadOnlyList<ScalerDescriptor>> _pixelScalers = new(
    () => new ReadOnlyList<ScalerDescriptor>(_all.Value.Where(d => d.IsPixelScaler).ToList()));
  private static readonly Lazy<IReadOnlyList<ScalerDescriptor>> _resamplers = new(
    () => new ReadOnlyList<ScalerDescriptor>(_all.Value.Where(d => d.IsResampler).ToList()));

  /// <summary>
  /// Gets all registered scalers (both pixel-art scalers and resamplers).
  /// </summary>
  public static IReadOnlyList<ScalerDescriptor> All => _all.Value;

  /// <summary>
  /// Gets all registered pixel-art scalers.
  /// </summary>
  public static IReadOnlyList<ScalerDescriptor> PixelScalers => _pixelScalers.Value;

  /// <summary>
  /// Gets all registered resamplers.
  /// </summary>
  public static IReadOnlyList<ScalerDescriptor> Resamplers => _resamplers.Value;

  /// <summary>
  /// Finds a scaler by name (case-insensitive).
  /// </summary>
  /// <param name="name">The name to search for.</param>
  /// <returns>The matching descriptor, or <c>null</c> if not found.</returns>
  public static ScalerDescriptor? FindByName(string name)
    => All.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Finds scalers whose name contains the specified substring (case-insensitive).
  /// </summary>
  /// <param name="substring">The substring to search for.</param>
  /// <returns>All matching descriptors.</returns>
  public static IEnumerable<ScalerDescriptor> FindByNameContaining(string substring)
    => All.Where(d => d.Name.Contains(substring, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Gets all scalers in the specified category.
  /// </summary>
  /// <param name="category">The category to filter by.</param>
  /// <returns>All scalers in the category.</returns>
  public static IEnumerable<ScalerDescriptor> GetByCategory(ScalerCategory category)
    => All.Where(d => d.Category == category);

  /// <summary>
  /// Gets all scalers by the specified author (case-insensitive).
  /// </summary>
  /// <param name="author">The author name to search for.</param>
  /// <returns>All scalers by the author.</returns>
  public static IEnumerable<ScalerDescriptor> GetByAuthor(string author)
    => All.Where(d => d.Author != null && d.Author.Equals(author, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Gets all pixel-art scalers that support the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns>All pixel-art scalers supporting the scale.</returns>
  public static IEnumerable<ScalerDescriptor> GetPixelScalersByScale(ScaleFactor scale)
    => PixelScalers.Where(d => d.SupportedScales.Contains(scale));

  /// <summary>
  /// Gets all pixel-art scalers that support the specified scale factor.
  /// </summary>
  /// <param name="scaleX">The horizontal scale factor.</param>
  /// <param name="scaleY">The vertical scale factor.</param>
  /// <returns>All pixel-art scalers supporting the scale.</returns>
  public static IEnumerable<ScalerDescriptor> GetPixelScalersByScale(int scaleX, int scaleY)
    => GetPixelScalersByScale(new ScaleFactor(scaleX, scaleY));

  /// <summary>
  /// Gets the descriptor for a specific scaler type.
  /// </summary>
  /// <typeparam name="TScaler">The scaler type.</typeparam>
  /// <returns>The descriptor, or <c>null</c> if the type is not registered.</returns>
  public static ScalerDescriptor? GetDescriptor<TScaler>() where TScaler : struct, IScalerInfo
    => All.FirstOrDefault(d => d.Type == typeof(TScaler));

  private static List<ScalerDescriptor> DiscoverScalers() {
    var assembly = typeof(ScalerRegistry).Assembly;
    var descriptors = new List<ScalerDescriptor>();

    foreach (var type in assembly.GetTypes()) {
      if (!type.IsValueType || type.IsAbstract)
        continue;

      if (!typeof(IPixelScaler).IsAssignableFrom(type) && !typeof(IResampler).IsAssignableFrom(type))
        continue;

      var descriptor = ScalerDescriptor.FromType(type);
      if (descriptor != null)
        descriptors.Add(descriptor);
    }

    return descriptors.OrderBy(d => d.Name).ToList();
  }
}
