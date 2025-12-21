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

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Interface for color space types that can convert to <see cref="Color"/>.
/// </summary>
public interface IColorSpace: IEquatable<Color> {
  /// <summary>
  /// Converts this color to a <see cref="Color"/>.
  /// </summary>
  /// <returns>The equivalent RGB color.</returns>
  Color ToColor();

  T ToColor<T>() where T : struct, IColorSpace;

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS

  /// <summary>
  /// Converts a <see cref="Color"/> to this color space.
  /// </summary>
  /// <param name="color">The color to convert.</param>
  /// <returns>The color in this color space.</returns>
  static abstract IColorSpace FromColor(Color color);

#endif

}
