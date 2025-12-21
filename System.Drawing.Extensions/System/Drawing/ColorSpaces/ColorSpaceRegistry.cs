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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Provides discovery and enumeration of all registered color spaces.
/// </summary>
public static class ColorSpaceRegistry {

  /// <summary>
  /// Information about a registered color space.
  /// </summary>
  public sealed class ColorSpaceInfo {
    /// <summary>Gets the color space attribute metadata.</summary>
    public ColorSpaceAttribute Attribute { get; }

    /// <summary>Gets the color space type.</summary>
    public Type Type { get; }

    /// <summary>Gets the number of color components (excluding alpha).</summary>
    public int ComponentCount => this.Attribute.ComponentCount;

    /// <summary>Gets the names of each component.</summary>
    public string[] ComponentNames => this.Attribute.ComponentNames;

    /// <summary>Gets the color space category type.</summary>
    public ColorSpaceType ColorSpaceType => this.Attribute.ColorSpaceType;

    /// <summary>Gets the display name for UI purposes.</summary>
    public string DisplayName => this.Attribute.DisplayName ?? this.Type.Name;

    /// <summary>Gets whether this color space is perceptually uniform.</summary>
    public bool IsPerceptuallyUniform => this.Attribute.IsPerceptuallyUniform;

    /// <summary>Gets the white point reference if applicable.</summary>
    public string? WhitePoint => this.Attribute.WhitePoint;

    /// <summary>Gets whether this is a normalized (float) variant.</summary>
    public bool IsNormalized => this.Type.Name.EndsWith("Normalized", StringComparison.Ordinal);

    /// <summary>Gets whether this implements IColorSpace.</summary>
    public bool ImplementsIColorSpace => typeof(IColorSpace).IsAssignableFrom(this.Type);

    internal ColorSpaceInfo(ColorSpaceAttribute attribute, Type type) {
      this.Attribute = attribute;
      this.Type = type;
    }

    /// <summary>
    /// Converts a Color to this color space using the factory.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    public IColorSpace FromColor(Color color) {
      var factoryType = typeof(ColorSpaceFactory<>).MakeGenericType(this.Type);
      var fromColorMethod = factoryType.GetMethod("FromColor", BindingFlags.Public | BindingFlags.Static);
      return (IColorSpace)fromColorMethod!.Invoke(null, [color])!;
    }
  }

  private static readonly Lazy<ColorSpaceInfo[]> _allColorSpaces = new(_DiscoverColorSpaces);

  /// <summary>
  /// Gets all registered color spaces.
  /// </summary>
  public static ColorSpaceInfo[] All => _allColorSpaces.Value;

  /// <summary>
  /// Gets all color space category types available.
  /// </summary>
  public static ColorSpaceType[] AvailableTypes
    => _allColorSpaces.Value.Select(c => c.ColorSpaceType).Distinct().OrderBy(t => t).ToArray();

  /// <summary>
  /// Gets color spaces filtered by category type.
  /// </summary>
  /// <param name="type">The color space category type.</param>
  public static IEnumerable<ColorSpaceInfo> GetByType(ColorSpaceType type)
    => _allColorSpaces.Value.Where(c => c.ColorSpaceType == type);

  /// <summary>
  /// Gets additive color spaces (e.g., RGB).
  /// </summary>
  public static IEnumerable<ColorSpaceInfo> AdditiveColorSpaces
    => GetByType(ColorSpaceType.Additive);

  /// <summary>
  /// Gets subtractive color spaces (e.g., CMYK).
  /// </summary>
  public static IEnumerable<ColorSpaceInfo> SubtractiveColorSpaces
    => GetByType(ColorSpaceType.Subtractive);

  /// <summary>
  /// Gets perceptual color spaces (e.g., Lab, Luv).
  /// </summary>
  public static IEnumerable<ColorSpaceInfo> PerceptualColorSpaces
    => GetByType(ColorSpaceType.Perceptual);

  /// <summary>
  /// Gets cylindrical color spaces (e.g., HSL, HSV).
  /// </summary>
  public static IEnumerable<ColorSpaceInfo> CylindricalColorSpaces
    => GetByType(ColorSpaceType.Cylindrical);

  /// <summary>
  /// Gets perceptually uniform color spaces.
  /// </summary>
  public static IEnumerable<ColorSpaceInfo> PerceptuallyUniform
    => _allColorSpaces.Value.Where(c => c.IsPerceptuallyUniform);

  /// <summary>
  /// Gets byte-based (non-normalized) color spaces only.
  /// </summary>
  public static IEnumerable<ColorSpaceInfo> ByteColorSpaces
    => _allColorSpaces.Value.Where(c => !c.IsNormalized);

  /// <summary>
  /// Gets normalized (float-based) color spaces only.
  /// </summary>
  public static IEnumerable<ColorSpaceInfo> NormalizedColorSpaces
    => _allColorSpaces.Value.Where(c => c.IsNormalized);

  /// <summary>
  /// Finds a color space by name (case-insensitive).
  /// </summary>
  /// <param name="name">The type name or display name.</param>
  public static ColorSpaceInfo? FindByName(string name)
    => _allColorSpaces.Value.FirstOrDefault(c =>
      c.Type.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
      (c.DisplayName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false)
    );

  private static ColorSpaceInfo[] _DiscoverColorSpaces() {
    var colorSpaceInterface = typeof(IColorSpace);
    var assembly = typeof(ColorSpaceRegistry).Assembly;

    return assembly
      .GetTypes()
      .Where(t => t.IsValueType && colorSpaceInterface.IsAssignableFrom(t))
      .Select(t => (type: t, attr: t.GetCustomAttribute<ColorSpaceAttribute>()))
      .Where(x => x.attr != null)
      .Select(x => new ColorSpaceInfo(x.attr!, x.type))
      .OrderBy(c => c.ColorSpaceType)
      .ThenBy(c => c.DisplayName)
      .ToArray();
  }
}
