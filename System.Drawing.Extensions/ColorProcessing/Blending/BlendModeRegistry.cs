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

namespace Hawkynt.ColorProcessing.Blending;

/// <summary>
/// Registry for runtime discovery of all available blend modes.
/// </summary>
/// <remarks>
/// <para>
/// Use this class to enumerate all blend modes at runtime, find modes by name,
/// or filter by category.
/// </para>
/// <example>
/// <code>
/// foreach (var mode in BlendModeRegistry.All)
///   Console.WriteLine($"{mode.Name} ({mode.Category})");
///
/// var multiply = BlendModeRegistry.FindByName("Multiply");
/// </code>
/// </example>
/// </remarks>
public static class BlendModeRegistry {

  private static readonly Lazy<IReadOnlyList<BlendModeDescriptor>> _all = new(
    () => new ReadOnlyList<BlendModeDescriptor>(DiscoverBlendModes()));

  /// <summary>
  /// Gets all registered blend modes.
  /// </summary>
  public static IReadOnlyList<BlendModeDescriptor> All => _all.Value;

  /// <summary>
  /// Finds a blend mode by name (case-insensitive).
  /// </summary>
  /// <param name="name">The name to search for.</param>
  /// <returns>The matching descriptor, or <c>null</c> if not found.</returns>
  public static BlendModeDescriptor? FindByName(string name)
    => All.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Gets all blend modes in the specified category.
  /// </summary>
  /// <param name="category">The category to filter by.</param>
  /// <returns>All blend modes in the category.</returns>
  public static IEnumerable<BlendModeDescriptor> GetByCategory(BlendModeCategory category)
    => All.Where(d => d.Category == category);

  /// <summary>
  /// Gets the descriptor for a specific blend mode type.
  /// </summary>
  /// <typeparam name="TMode">The blend mode type.</typeparam>
  /// <returns>The descriptor, or <c>null</c> if the type is not registered.</returns>
  public static BlendModeDescriptor? GetDescriptor<TMode>() where TMode : struct, IBlendMode
    => All.FirstOrDefault(d => d.Type == typeof(TMode));

  private static List<BlendModeDescriptor> DiscoverBlendModes() {
    var assembly = typeof(BlendModeRegistry).Assembly;
    var descriptors = new List<BlendModeDescriptor>();

    foreach (var type in assembly.GetTypes()) {
      if (!type.IsValueType || type.IsAbstract)
        continue;

      if (!typeof(IBlendMode).IsAssignableFrom(type))
        continue;

      var descriptor = BlendModeDescriptor.FromType(type);
      if (descriptor != null)
        descriptors.Add(descriptor);
    }

    return descriptors.OrderBy(d => d.Name).ToList();
  }
}
