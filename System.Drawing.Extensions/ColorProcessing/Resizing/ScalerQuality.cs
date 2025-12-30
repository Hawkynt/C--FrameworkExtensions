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

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Specifies the quality level for color operations during scaling.
/// </summary>
/// <remarks>
/// Higher quality uses perceptually uniform color spaces (like Oklab)
/// for interpolation and comparison, at the cost of additional computation.
/// </remarks>
public enum ScalerQuality {
  /// <summary>
  /// Fast mode: uses direct sRGB operations.
  /// </summary>
  /// <remarks>
  /// Pattern matching uses exact byte comparison.
  /// Interpolation uses linear RGB blending.
  /// Best for pixel-art where preserving exact colors is preferred.
  /// </remarks>
  Fast,

  /// <summary>
  /// High quality mode: uses perceptually uniform color space.
  /// </summary>
  /// <remarks>
  /// Pattern matching uses Oklab-based distance thresholds.
  /// Interpolation uses linear RGB with gamma-correct blending.
  /// Best for photographic content or when smooth gradients are desired.
  /// </remarks>
  HighQuality
}
