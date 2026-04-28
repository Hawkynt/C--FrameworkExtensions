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
using Guard;

namespace Hawkynt.Drawing.Exif;

/// <summary>
/// EXIF orientation tag values per Exif 2.32 §4.6.4 / TIFF tag 0x0112.
/// </summary>
/// <remarks>
/// <para>
/// Cameras and phones write the sensor's native orientation; the tag tells a
/// renderer how to rotate/flip the pixels for an upright view. Most modern
/// browsers honour the tag automatically; GDI+ does not, which is why we
/// expose a normalising helper here.
/// </para>
/// </remarks>
public enum ExifOrientationValue {
  /// <summary>0x01: top-left (no transform required — already upright).</summary>
  Normal = 1,

  /// <summary>0x02: top-right (mirror horizontally).</summary>
  MirrorHorizontal = 2,

  /// <summary>0x03: bottom-right (rotate 180°).</summary>
  Rotate180 = 3,

  /// <summary>0x04: bottom-left (mirror vertically).</summary>
  MirrorVertical = 4,

  /// <summary>0x05: left-top (mirror horizontally + rotate 270° clockwise).</summary>
  MirrorHorizontalRotate270 = 5,

  /// <summary>0x06: right-top (rotate 90° clockwise).</summary>
  Rotate90 = 6,

  /// <summary>0x07: right-bottom (mirror horizontally + rotate 90° clockwise).</summary>
  MirrorHorizontalRotate90 = 7,

  /// <summary>0x08: left-bottom (rotate 270° clockwise).</summary>
  Rotate270 = 8,
}

/// <summary>
/// Reads JPEG/HEIF/TIFF EXIF orientation (TIFF tag 0x0112) from a
/// <see cref="Bitmap"/> and applies the appropriate lossless rotation/flip
/// to bring the image to the visually-correct orientation.
/// </summary>
/// <remarks>
/// <para>
/// The orientation tag is read via <see cref="Image.PropertyItems"/>
/// (System.Drawing exposes EXIF via property ID <c>0x0112</c>). All eight
/// values defined by Exif 2.32 §4.6.4 are handled. Bitmaps without the tag
/// are returned unchanged (cloned).
/// </para>
/// <para>
/// The transform is applied via <see cref="Bitmap.RotateFlip"/>, which is
/// lossless for 90°-multiple rotations and flips — the pixel buffer is just
/// re-indexed, no resampling occurs.
/// </para>
/// </remarks>
public static class ExifOrientation {

  /// <summary>EXIF orientation TIFF tag ID (per Exif 2.32 §4.6.4).</summary>
  public const int OrientationTagId = 0x0112;

  /// <summary>
  /// Reads the EXIF orientation tag (0x0112) from <paramref name="source"/>.
  /// </summary>
  /// <param name="source">The bitmap to inspect.</param>
  /// <returns>
  /// The decoded <see cref="ExifOrientationValue"/>, or <c>null</c> if the
  /// tag is absent or malformed.
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  public static ExifOrientationValue? Read(Bitmap source) {
    Against.ArgumentIsNull(source);

    try {
      var item = source.GetPropertyItem(OrientationTagId);
      if (item == null || item.Value == null || item.Value.Length < 2)
        return null;
      // EXIF orientation is a 16-bit unsigned short, little-endian on disk
      // by the time GDI+ exposes it.
      var raw = BitConverter.ToUInt16(item.Value, 0);
      return raw is >= 1 and <= 8 ? (ExifOrientationValue)raw : (ExifOrientationValue?)null;
    } catch {
      // GetPropertyItem throws if the tag is absent.
      return null;
    }
  }

  /// <summary>
  /// Reads the EXIF orientation of <paramref name="source"/> and returns a
  /// new bitmap re-oriented to the upright "Normal" view.
  /// </summary>
  /// <param name="source">The source bitmap (not mutated).</param>
  /// <returns>
  /// A new bitmap with the orientation applied, or a clone of
  /// <paramref name="source"/> if no orientation tag is present.
  /// </returns>
  /// <remarks>
  /// <para>
  /// All eight EXIF values are mapped to the matching
  /// <see cref="System.Drawing.RotateFlipType"/> on a clone of the source.
  /// The operation is lossless: pixel data is never resampled, only
  /// re-indexed by the framework's rotate/flip primitive.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  public static Bitmap Apply(Bitmap source) {
    Against.ArgumentIsNull(source);

    var orientation = Read(source);
    var clone = (Bitmap)source.Clone();
    if (!orientation.HasValue || orientation.Value == ExifOrientationValue.Normal)
      return clone;

    clone.RotateFlip(_ToRotateFlip(orientation.Value));
    // GDI+ leaves the property item attached after RotateFlip, which would
    // misinform downstream consumers; remove it so the bitmap is canonically upright.
    try { clone.RemovePropertyItem(OrientationTagId); } catch { /* tag absent */ }
    return clone;
  }

  /// <summary>Mutates <paramref name="source"/> in place by applying its EXIF orientation.</summary>
  /// <param name="source">The bitmap to re-orient.</param>
  /// <returns>
  /// <c>true</c> if a non-trivial transform was applied, <c>false</c> if the
  /// orientation tag was absent or already <see cref="ExifOrientationValue.Normal"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
  public static bool ApplyInPlace(Bitmap source) {
    Against.ArgumentIsNull(source);
    var orientation = Read(source);
    if (!orientation.HasValue || orientation.Value == ExifOrientationValue.Normal)
      return false;
    source.RotateFlip(_ToRotateFlip(orientation.Value));
    try { source.RemovePropertyItem(OrientationTagId); } catch { /* tag absent */ }
    return true;
  }

  private static RotateFlipType _ToRotateFlip(ExifOrientationValue v) => v switch {
    ExifOrientationValue.Normal => RotateFlipType.RotateNoneFlipNone,
    ExifOrientationValue.MirrorHorizontal => RotateFlipType.RotateNoneFlipX,
    ExifOrientationValue.Rotate180 => RotateFlipType.Rotate180FlipNone,
    ExifOrientationValue.MirrorVertical => RotateFlipType.RotateNoneFlipY,
    ExifOrientationValue.MirrorHorizontalRotate270 => RotateFlipType.Rotate90FlipX,
    ExifOrientationValue.Rotate90 => RotateFlipType.Rotate90FlipNone,
    ExifOrientationValue.MirrorHorizontalRotate90 => RotateFlipType.Rotate270FlipX,
    ExifOrientationValue.Rotate270 => RotateFlipType.Rotate270FlipNone,
    _ => RotateFlipType.RotateNoneFlipNone,
  };
}
