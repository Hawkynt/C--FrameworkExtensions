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

using System.Drawing;

namespace Hawkynt.ColorProcessing.Geometry;

/// <summary>
/// Selects how an inverse-mapped destination pixel that falls outside the
/// source image is filled.
/// </summary>
/// <remarks>
/// Geometric transforms (rotate, shear, affine, perspective) inverse-map
/// every destination pixel back to a source coordinate. When that coordinate
/// is outside the source bounds — typical of the four "new corners" produced
/// by an arbitrary-angle rotation — this enum says what to write instead.
/// </remarks>
public enum GeometricFillMode {
  /// <summary>
  /// Fill out-of-bounds pixels with a fixed colour
  /// (see <see cref="FillSpec.Color"/>). Default is transparent.
  /// </summary>
  Constant = 0,

  /// <summary>
  /// Clamp the source coordinate to the nearest valid edge pixel
  /// (a.k.a. "extend" / "edge-replicate"). Yields no visible seam at corners.
  /// </summary>
  Clamp = 1,

  /// <summary>
  /// Wrap the source coordinate modulo the source dimensions. Useful for
  /// tileable textures.
  /// </summary>
  Wrap = 2,

  /// <summary>
  /// Mirror the source coordinate across the boundary. Avoids hard seams
  /// without the obvious tiling that <see cref="Wrap"/> can introduce.
  /// </summary>
  Mirror = 3,
}

/// <summary>
/// How a geometric transform fills pixels whose inverse-mapped source
/// coordinate lies outside the source image.
/// </summary>
/// <param name="Mode">The boundary policy (see <see cref="GeometricFillMode"/>).</param>
/// <param name="Color">
/// The constant fill colour used when <paramref name="Mode"/> is
/// <see cref="GeometricFillMode.Constant"/>; ignored otherwise.
/// </param>
/// <remarks>
/// Convenience constructors expose the two common cases:
/// <see cref="Transparent"/> (fully transparent corners) and
/// <see cref="Of"/> (constant colour with full opacity).
/// </remarks>
public readonly struct FillSpec {

  /// <summary>The boundary policy.</summary>
  public GeometricFillMode Mode { get; }

  /// <summary>The constant fill colour for <see cref="GeometricFillMode.Constant"/>.</summary>
  public Color Color { get; }

  /// <summary>Creates a fill specification.</summary>
  /// <param name="mode">The boundary policy.</param>
  /// <param name="color">The constant fill colour (used only when <paramref name="mode"/> is <see cref="GeometricFillMode.Constant"/>).</param>
  public FillSpec(GeometricFillMode mode, Color color) {
    this.Mode = mode;
    this.Color = color;
  }

  /// <summary>Out-of-bounds pixels are written transparent.</summary>
  public static FillSpec Transparent => new(GeometricFillMode.Constant, Color.FromArgb(0, 0, 0, 0));

  /// <summary>Out-of-bounds pixels are written with the supplied opaque colour.</summary>
  public static FillSpec Of(Color color) => new(GeometricFillMode.Constant, color);

  /// <summary>Edge-replicate (clamp to nearest edge).</summary>
  public static FillSpec Clamp => new(GeometricFillMode.Clamp, Color.Empty);

  /// <summary>Mirror across the source boundary.</summary>
  public static FillSpec Mirror => new(GeometricFillMode.Mirror, Color.Empty);

  /// <summary>Tile (wrap modulo source dimensions).</summary>
  public static FillSpec Wrap => new(GeometricFillMode.Wrap, Color.Empty);
}
