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
using System.Reflection;

namespace Hawkynt.ColorProcessing.Blending;

/// <summary>
/// Describes a blend mode algorithm with its metadata.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="BlendModeRegistry"/> to enumerate all available blend modes at runtime.
/// Each descriptor provides metadata from the <see cref="BlendModeInfoAttribute"/>
/// and can create default instances.
/// </para>
/// </remarks>
public sealed class BlendModeDescriptor {

  /// <summary>
  /// Gets the concrete type of the blend mode.
  /// </summary>
  public Type Type { get; }

  /// <summary>
  /// Gets the display name of the blend mode.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets a description of the blend mode.
  /// </summary>
  public string? Description { get; }

  /// <summary>
  /// Gets the category of the blend mode.
  /// </summary>
  public BlendModeCategory Category { get; }

  /// <summary>
  /// Gets whether this blend mode operates on full pixels (HSL-based).
  /// </summary>
  public bool IsFullPixelMode { get; }

  private BlendModeDescriptor(Type type, string name, string? description, BlendModeCategory category, bool isFullPixelMode) {
    this.Type = type;
    this.Name = name;
    this.Description = description;
    this.Category = category;
    this.IsFullPixelMode = isFullPixelMode;
  }

  /// <summary>
  /// Creates a descriptor from a blend mode type.
  /// </summary>
  /// <param name="type">The blend mode type (must implement <see cref="IBlendMode"/>).</param>
  /// <returns>A descriptor, or <c>null</c> if the type doesn't have the <see cref="BlendModeInfoAttribute"/>.</returns>
  internal static BlendModeDescriptor? FromType(Type type) {
    var attr = type.GetCustomAttribute<BlendModeInfoAttribute>();
    if (attr == null)
      return null;

    if (!typeof(IBlendMode).IsAssignableFrom(type))
      return null;

    return new(
      type,
      attr.Name,
      attr.Description,
      attr.Category,
      typeof(IFullPixelBlendMode).IsAssignableFrom(type)
    );
  }

  /// <summary>
  /// Creates a default instance of this blend mode.
  /// </summary>
  /// <returns>A new instance of the blend mode.</returns>
  public IBlendMode CreateDefault() => (IBlendMode)Activator.CreateInstance(this.Type)!;

  /// <inheritdoc />
  public override string ToString() => $"{this.Name} ({this.Category})";
}
