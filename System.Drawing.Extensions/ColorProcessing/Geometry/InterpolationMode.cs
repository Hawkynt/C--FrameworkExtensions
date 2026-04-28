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

namespace Hawkynt.ColorProcessing.Geometry;

/// <summary>
/// Selects which separable resampling kernel a geometric transform uses
/// when an inverse-mapped destination pixel falls between source samples.
/// </summary>
/// <remarks>
/// <para>
/// Each value names a concrete <see cref="Resizing.IKernelResampler"/> shipped
/// with this library. Geometric transforms call
/// <see cref="Resizing.IKernelResampler.EvaluateWeight"/> directly on the
/// chosen kernel instead of going through the full resize pipeline, which
/// keeps the transform code path single-file and trivially deterministic.
/// </para>
/// <para>
/// <see cref="NearestNeighbor"/> is exact and reversible for 90°-multiple
/// rotations, <see cref="Bilinear"/> is the smoothest 2×2 default,
/// <see cref="Bicubic"/> matches GDI+ <c>HighQualityBicubic</c> in feel, and
/// <see cref="Lanczos3"/> is the highest-quality choice (6×6 footprint).
/// </para>
/// </remarks>
public enum GeometricInterpolation {
  /// <summary>
  /// Nearest-neighbour. Fastest. Pixel-exact for 90° rotations and integer
  /// translations.
  /// </summary>
  NearestNeighbor = 0,

  /// <summary>2×2 bilinear interpolation. Default for arbitrary-angle warps.</summary>
  Bilinear = 1,

  /// <summary>4×4 Mitchell-style bicubic. Sharper than bilinear with mild ringing.</summary>
  Bicubic = 2,

  /// <summary>6×6 Lanczos-3 windowed sinc. Highest quality, slowest.</summary>
  Lanczos3 = 3,
}
