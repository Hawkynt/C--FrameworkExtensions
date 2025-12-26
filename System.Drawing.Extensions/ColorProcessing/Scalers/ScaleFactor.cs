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

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Represents a scaling factor with separate horizontal and vertical components.
/// </summary>
/// <param name="X">The horizontal scaling factor.</param>
/// <param name="Y">The vertical scaling factor.</param>
public readonly record struct ScaleFactor(int X, int Y) {

  /// <summary>
  /// Creates a uniform scaling factor (same scale in both dimensions).
  /// </summary>
  /// <param name="scale">The uniform scale value.</param>
  /// <returns>A new <see cref="ScaleFactor"/> with equal X and Y components.</returns>
  public static ScaleFactor Uniform(int scale) => new(scale, scale);

  /// <summary>
  /// Implicitly converts an integer to a uniform <see cref="ScaleFactor"/>.
  /// </summary>
  /// <param name="scale">The uniform scale value.</param>
  public static implicit operator ScaleFactor(int scale) => Uniform(scale);

  /// <summary>
  /// Calculates the target dimensions for a given source size.
  /// </summary>
  /// <param name="sourceWidth">The source width.</param>
  /// <param name="sourceHeight">The source height.</param>
  /// <returns>The scaled dimensions as a tuple.</returns>
  public (int Width, int Height) Apply(int sourceWidth, int sourceHeight)
    => (sourceWidth * this.X, sourceHeight * this.Y);

  /// <inheritdoc />
  public override string ToString() => this.X == this.Y ? $"{this.X}x" : $"{this.X}x{this.Y}";
}
