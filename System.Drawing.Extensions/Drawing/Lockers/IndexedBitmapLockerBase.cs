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
using System.Drawing;
using System.Drawing.Imaging;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Abstract base class for indexed (palette-based) bitmap lockers.
/// </summary>
internal abstract class IndexedBitmapLockerBase : BitmapLockerBase {
  /// <summary>The color palette.</summary>
  protected readonly Color[] _palette;

  /// <summary>Gets the color palette.</summary>
  public Color[] Palette => this._palette;

  /// <summary>The stride in bytes.</summary>
  public new int Stride { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="IndexedBitmapLockerBase"/> class.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="validFormats">The valid pixel formats.</param>
  protected IndexedBitmapLockerBase(Bitmap bitmap, ImageLockMode lockMode, params PixelFormat[] validFormats)
    : base(bitmap, lockMode, validFormats) {
    this._palette = bitmap.Palette.Entries;
    this.Stride = this._data.Stride;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="IndexedBitmapLockerBase"/> class with region support.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="rect">The region of the bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="validFormats">The valid pixel formats.</param>
  protected IndexedBitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, params PixelFormat[] validFormats)
    : base(bitmap, rect, lockMode, 0, validFormats) {
    this._palette = bitmap.Palette.Entries;
    this.Stride = this._data.Stride;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="IndexedBitmapLockerBase"/> class with region and target format support.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="rect">The region of the bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="targetFormat">The target pixel format.</param>
  /// <param name="validFormats">The valid pixel formats.</param>
  protected IndexedBitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, PixelFormat targetFormat, params PixelFormat[] validFormats)
    : base(bitmap, rect, lockMode, 0, targetFormat, validFormats) {
    this._palette = bitmap.Palette.Entries;
    this.Stride = this._data.Stride;
  }

  /// <summary>
  /// Finds the color index in the palette.
  /// </summary>
  /// <param name="color">The color to find.</param>
  /// <returns>The index of the color in the palette.</returns>
  /// <exception cref="ArgumentException">Thrown when the color is not in the palette.</exception>
  protected int FindColorIndex(Color color) {
    var index = Array.IndexOf(this._palette, color);
    return index < 0 ? throw new ArgumentException($"Color {color} is not in the palette.", nameof(color)) : index;
  }
}
